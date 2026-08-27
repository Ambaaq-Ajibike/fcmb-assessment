using BankingApi.Api.Contracts;
using BankingApi.Api.Exceptions;
using BankingApi.Api.Models.Entities;
using BankingApi.Api.Repositories;

namespace BankingApi.Api.Services;

public sealed class BankingService(IBankingRepository repository) : IBankingService
{
    public async Task<AccountResponse> GetAccountAsync(
        string userId,
        CancellationToken cancellationToken)
    {
        var account = await repository.GetAccountAsync(userId, cancellationToken)
            ?? throw new NotFoundException("account_not_found", "Account was not found.");

        return Map(account);
    }

    public async Task<AccountResponse> UpdateProfileAsync(
        string userId,
        UpdateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await repository.UpdateProfileAsync(
            userId,
            request.FullName.Trim(),
            request.PhoneNumber?.Trim(),
            cancellationToken);

        if (!updated)
            throw new NotFoundException("account_not_found", "Account was not found.");

        return await GetAccountAsync(userId, cancellationToken);
    }

    public async Task<TransferResponse> TransferAsync(
        string userId,
        TransferRequest request,
        CancellationToken cancellationToken)
    {
        var description = string.IsNullOrWhiteSpace(request.Description)
            ? "Fund transfer"
            : request.Description.Trim();
        var result = await repository.TransferAsync(
            userId,
            request.RecipientAccountNumber,
            request.Amount,
            description,
            cancellationToken);

        return new TransferResponse(
            result.TransactionId,
            result.SenderAccountNumber,
            request.RecipientAccountNumber,
            request.Amount,
            result.Balance,
            result.CreatedAt);
    }

    public async Task<IReadOnlyList<TransactionResponse>> GetTransactionsAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        if (page < 1 || pageSize is < 1 or > 100)
            throw new ValidationException(
                "invalid_pagination",
                "Page must be at least 1 and pageSize must be between 1 and 100.");

        var transactions = await repository.GetTransactionsAsync(
            userId,
            page,
            pageSize,
            cancellationToken);

        return transactions
            .Select(transaction => new TransactionResponse(
                transaction.Id,
                transaction.SenderAccountNumber,
                transaction.RecipientAccountNumber,
                transaction.Amount,
                transaction.CreatedAt,
                transaction.Description,
                transaction.Direction))
            .ToList();
    }

    private static AccountResponse Map(AccountDetails account) => new(
        account.UserId,
        account.FullName,
        account.Email,
        account.PhoneNumber,
        account.AccountNumber,
        account.Balance);
}
