using BankingApi.Api.Contracts;
using BankingApi.Api.Exceptions;
using BankingApi.Api.Models;
using BankingApi.Api.Models.Entities;
using BankingApi.Api.Repositories;
using BankingApi.Api.Services;
namespace BankingApi.Tests;

public sealed class BankingServiceTests
{
    [Fact]
    public async Task Transfer_MapsSuccessfulResult()
    {
        var repo = new FakeRepository
        {
            TransferResult = new FundTransferResult(
                Guid.NewGuid().ToString(),
                "1234567890",
                750m,
                DateTimeOffset.UtcNow)
        };
        var result = await new BankingService(repo).TransferAsync(
            Guid.NewGuid().ToString(),
            new TransferRequest("0987654321", 250m, null),
            default);
        Assert.Equal(750m, result.Balance);
        Assert.Equal("Fund transfer", repo.Description);
    }

    [Theory, InlineData(0, 20), InlineData(1, 0), InlineData(1, 101)]
    public async Task History_RejectsInvalidPagination(int page, int pageSize)
    {
        await Assert.ThrowsAsync<ValidationException>(() =>
            new BankingService(new FakeRepository()).GetTransactionsAsync(
                Guid.NewGuid().ToString(),
                page,
                pageSize,
                default));
    }

    private sealed class FakeRepository : IBankingRepository
    {
        public FundTransferResult TransferResult { get; init; } = null!;
        public string? Description { get; private set; }
        public Task<UserRegistrationResult> CreateUserAsync(
            string a,
            string b,
            string c,
            CancellationToken d) => throw new NotImplementedException();
        public Task<User?> GetUserByEmailAsync(string a, CancellationToken b) => throw new NotImplementedException();
        public Task<AccountDetails?> GetAccountAsync(string a, CancellationToken b) => throw new NotImplementedException();
        public Task<bool> UpdateProfileAsync(string a, string b, string? c, CancellationToken d) => throw new NotImplementedException();
        public Task<FundTransferResult> TransferAsync(
            string a,
            string b,
            decimal c,
            string description,
            CancellationToken d)
        {
            Description = description;

            return Task.FromResult(TransferResult);
        }
        public Task<IReadOnlyList<TransactionDetails>> GetTransactionsAsync(
            string a,
            int b,
            int c,
            CancellationToken d) => Task.FromResult<IReadOnlyList<TransactionDetails>>([]);
    }
}
