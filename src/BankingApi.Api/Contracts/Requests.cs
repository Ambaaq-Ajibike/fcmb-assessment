namespace BankingApi.Api.Contracts;

public sealed record RegisterRequest(string FullName, string Email, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record UpdateProfileRequest(string FullName, string? PhoneNumber);
public sealed record TransferRequest(string RecipientAccountNumber, decimal Amount, string? Description);
