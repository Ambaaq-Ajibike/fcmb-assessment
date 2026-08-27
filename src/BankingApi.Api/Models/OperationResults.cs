namespace BankingApi.Api.Models;

public sealed record UserRegistrationResult(string UserId, string AccountNumber);

public sealed record FundTransferResult(
    string TransactionId,
    string SenderAccountNumber,
    decimal Balance,
    DateTimeOffset CreatedAt);

public sealed record AccessTokenResult(string Token, DateTimeOffset ExpiresAt);
