using System.Data;
using BankingApi.Api.Data;
using BankingApi.Api.Exceptions;
using BankingApi.Api.Models;
using BankingApi.Api.Models.Entities;
using Dapper;
namespace BankingApi.Api.Repositories;

public sealed class BankingRepository(IDbConnectionFactory connectionFactory) : IBankingRepository
{
    public async Task<UserRegistrationResult> CreateUserAsync(
        string fullName,
        string email,
        string passwordHash,
        decimal openingBalance,
        CancellationToken ct)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(IsolationLevel.ReadCommitted, ct);
        const string userSql = """
            INSERT INTO dbo.Users (Id, FullName, Email, PasswordHash)
            VALUES (@Id, @FullName, @Email, @PasswordHash);
            """;
        var userId = Guid.NewGuid().ToString();
        var userParameters = new
        {
            Id = userId,
            FullName = fullName,
            Email = email,
            PasswordHash = passwordHash
        };
        await connection.ExecuteAsync(new CommandDefinition(
            userSql,
            userParameters,
            transaction,
            cancellationToken: ct));

        var accountNumber = await GenerateUniqueAccountNumberAsync(connection, transaction, ct);
        const string accountSql = """
            INSERT INTO dbo.Accounts (Id, UserId, AccountNumber, Balance)
            VALUES (@Id, @UserId, @AccountNumber, @OpeningBalance);
            """;
        var accountParameters = new
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            AccountNumber = accountNumber,
            OpeningBalance = openingBalance
        };
        await connection.ExecuteAsync(new CommandDefinition(
            accountSql,
            accountParameters,
            transaction,
            cancellationToken: ct));
        await transaction.CommitAsync(ct);

        return new UserRegistrationResult(userId, accountNumber);
    }

    public async Task<AuthenticatedUser?> GetUserByEmailAsync(string email, CancellationToken ct)
    {
        const string sql = "SELECT Id, FullName, Email, PasswordHash FROM dbo.Users WHERE Email = @Email;";
        await using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            sql,
            new { Email = email },
            cancellationToken: ct);

        return await connection.QuerySingleOrDefaultAsync<AuthenticatedUser>(command);
    }

    public async Task<AccountDetails?> GetAccountAsync(string userId, CancellationToken ct)
    {
        const string sql = """
            SELECT u.Id UserId, u.FullName, u.Email, u.PhoneNumber, a.AccountNumber, a.Balance
            FROM dbo.Users u INNER JOIN dbo.Accounts a ON a.UserId = u.Id WHERE u.Id = @UserId;
            """;
        await using var connection = connectionFactory.CreateConnection();
        var command = new CommandDefinition(
            sql,
            new { UserId = userId },
            cancellationToken: ct);

        return await connection.QuerySingleOrDefaultAsync<AccountDetails>(command);
    }

    public async Task<bool> UpdateProfileAsync(string userId, string fullName, string? phoneNumber, CancellationToken ct)
    {
        const string sql = """
            UPDATE dbo.Users
            SET FullName = @FullName,
                PhoneNumber = @PhoneNumber,
                UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @UserId;
            """;
        await using var connection = connectionFactory.CreateConnection();
        var parameters = new
        {
            UserId = userId,
            FullName = fullName,
            PhoneNumber = phoneNumber
        };
        var command = new CommandDefinition(sql, parameters, cancellationToken: ct);

        return await connection.ExecuteAsync(command) == 1;
    }

    public async Task<FundTransferResult> TransferAsync(
        string userId,
        string recipientNumber,
        decimal amount,
        string description,
        CancellationToken ct)
    {
        await using var connection = connectionFactory.CreateConnection();
        await connection.OpenAsync(ct);
        await using var tx = await connection.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        const string idsSql = """
            SELECT Id, UserId, AccountNumber
            FROM dbo.Accounts WITH (UPDLOCK, HOLDLOCK)
            WHERE UserId = @UserId OR AccountNumber = @Recipient
            ORDER BY Id;
            """;

        var accountsCommand = new CommandDefinition(
            idsSql,
            new { UserId = userId, Recipient = recipientNumber },
            tx,
            cancellationToken: ct);
        var accounts = (await connection.QueryAsync<LockedAccount>(accountsCommand)).ToList();
        var sender = accounts.SingleOrDefault(account => account.UserId == userId)
            ?? throw new NotFoundException("account_not_found", "Sender account was not found.");
        var recipient = accounts.SingleOrDefault(account => account.AccountNumber == recipientNumber)
            ?? throw new NotFoundException("recipient_not_found", "Recipient account was not found.");
        if (sender.Id == recipient.Id)
        {
            throw new ValidationException("self_transfer", "You cannot transfer to your own account.");
        }

        const string debitSql = """
            UPDATE dbo.Accounts
            SET Balance = Balance - @Amount,
                UpdatedAt = SYSUTCDATETIME()
            OUTPUT INSERTED.Balance
            WHERE Id = @Id AND Balance >= @Amount;
            """;
        var debitCommand = new CommandDefinition(
            debitSql,
            new { Amount = amount, sender.Id },
            tx,
            cancellationToken: ct);
        var balance = await connection.QuerySingleOrDefaultAsync<decimal?>(debitCommand);
        if (balance is null)
        {
            throw new ValidationException("insufficient_funds", "The account has insufficient funds.");
        }
        const string creditSql = """
            UPDATE dbo.Accounts
            SET Balance = Balance + @Amount,
                UpdatedAt = SYSUTCDATETIME()
            WHERE Id = @Id;
            """;
        var creditCommand = new CommandDefinition(
            creditSql,
            new { Amount = amount, recipient.Id },
            tx,
            cancellationToken: ct);
        await connection.ExecuteAsync(creditCommand);

        var transactionId = Guid.NewGuid().ToString();
        var createdAt = DateTimeOffset.UtcNow;
        const string insertSql = """
            INSERT INTO dbo.Transactions
                (Id, SenderAccountId, RecipientAccountId, Amount, Description, CreatedAt)
            VALUES
                (@Id, @SenderId, @RecipientId, @Amount, @Description, @CreatedAt);
            """;
        var transactionParameters = new
        {
            Id = transactionId,
            SenderId = sender.Id,
            RecipientId = recipient.Id,
            Amount = amount,
            Description = description,
            CreatedAt = createdAt
        };
        var insertCommand = new CommandDefinition(
            insertSql,
            transactionParameters,
            tx,
            cancellationToken: ct);
        await connection.ExecuteAsync(insertCommand);
        await tx.CommitAsync(ct);

        return new FundTransferResult(transactionId, sender.AccountNumber, balance.Value, createdAt);
    }

    public async Task<IReadOnlyList<TransactionDetails>> GetTransactionsAsync(string userId, int page, int pageSize, CancellationToken ct)
    {
        const string sql = """
            SELECT t.Id, sa.AccountNumber SenderAccountNumber, ra.AccountNumber RecipientAccountNumber,
                   t.Amount, t.CreatedAt, t.Description,
                   CASE WHEN sa.UserId = @UserId THEN 'Debit' ELSE 'Credit' END Direction
            FROM dbo.Transactions t
            INNER JOIN dbo.Accounts sa ON sa.Id = t.SenderAccountId
            INNER JOIN dbo.Accounts ra ON ra.Id = t.RecipientAccountId
            WHERE sa.UserId = @UserId OR ra.UserId = @UserId
            ORDER BY t.CreatedAt DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;
        await using var connection = connectionFactory.CreateConnection();
        var parameters = new
        {
            UserId = userId,
            Offset = (page - 1) * pageSize,
            PageSize = pageSize
        };
        var command = new CommandDefinition(sql, parameters, cancellationToken: ct);
        var rows = await connection.QueryAsync<TransactionDetails>(command);
        return rows.AsList();
    }

    private static async Task<string> GenerateUniqueAccountNumberAsync(
        System.Data.Common.DbConnection connection,
        System.Data.Common.DbTransaction tx,
        CancellationToken ct)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var number = Random.Shared.NextInt64(1_000_000_000, 10_000_000_000).ToString();
            const string sql = """
                SELECT CAST(
                    CASE WHEN EXISTS (
                        SELECT 1 FROM dbo.Accounts WHERE AccountNumber = @Number
                    ) THEN 1 ELSE 0 END
                AS bit);
                """;
            var command = new CommandDefinition(
                sql,
                new { Number = number },
                tx,
                cancellationToken: ct);
            var exists = await connection.ExecuteScalarAsync<bool>(command);
            if (!exists)
            {
                return number;
            }
        }
        throw new InvalidOperationException("Could not allocate an account number.");
    }
    private sealed class LockedAccount
    {
        public string Id { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string AccountNumber { get; set; } = string.Empty;
    }
}
