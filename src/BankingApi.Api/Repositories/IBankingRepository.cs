using BankingApi.Api.Models;
using BankingApi.Api.Models.Entities;
namespace BankingApi.Api.Repositories;

public interface IBankingRepository
{
    Task<UserRegistrationResult> CreateUserAsync(
        string fullName,
        string email,
        string passwordHash,
        CancellationToken cancellationToken);
    Task<AuthenticatedUser?> GetUserByEmailAsync(string email, CancellationToken cancellationToken);
    Task<AccountDetails?> GetAccountAsync(string userId, CancellationToken cancellationToken);
    Task<bool> UpdateProfileAsync(string userId, string fullName, string? phoneNumber, CancellationToken cancellationToken);
    Task<FundTransferResult> TransferAsync(
        string userId,
        string recipientAccountNumber,
        decimal amount,
        string description,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<TransactionDetails>> GetTransactionsAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
