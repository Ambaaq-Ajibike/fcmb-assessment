namespace BankingApi.Api.Contracts;

public sealed record AuthResponse(
    string UserId,
    string FullName,
    string Email,
    string AccountNumber,
    string AccessToken,
    DateTimeOffset ExpiresAt);

public sealed record AccountResponse(
    string UserId,
    string FullName,
    string Email,
    string? PhoneNumber,
    string AccountNumber,
    decimal Balance);

public sealed record TransferResponse(
    string TransactionId,
    string SenderAccountNumber,
    string RecipientAccountNumber,
    decimal Amount,
    decimal Balance,
    DateTimeOffset CreatedAt);

public sealed record TransactionResponse(
    string Id,
    string SenderAccountNumber,
    string RecipientAccountNumber,
    decimal Amount,
    DateTimeOffset CreatedAt,
    string Description,
    string Direction);
