using BankingApi.Api.Contracts;

namespace BankingApi.Api.Services;

public interface IBankingService
{
    Task<AccountResponse> GetAccountAsync(string userId, CancellationToken cancellationToken);
    Task<AccountResponse> UpdateProfileAsync(string userId, UpdateProfileRequest request, CancellationToken cancellationToken);
    Task<TransferResponse> TransferAsync(string userId, TransferRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyList<TransactionResponse>> GetTransactionsAsync(
        string userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
