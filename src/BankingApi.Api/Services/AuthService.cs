using BankingApi.Api.Contracts;
using BankingApi.Api.Exceptions;
using BankingApi.Api.Models.Entities;
using BankingApi.Api.Repositories;

namespace BankingApi.Api.Services;

public sealed class AuthService(
    IBankingRepository repository,
    IPasswordService passwords,
    ITokenService tokens) : IAuthService
{
    public async Task<AuthResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        if (await repository.GetUserByEmailAsync(email, cancellationToken) is not null)
            throw new ConflictException("email_exists", "An account with this email already exists.");

        var registration = await repository.CreateUserAsync(
            request.FullName.Trim(),
            email,
            passwords.Hash(request.Password),
            cancellationToken);

        var user = new AuthenticatedUser
        {
            Id = registration.UserId,
            FullName = request.FullName.Trim(),
            Email = email,
            PasswordHash = string.Empty
        };
        var accessToken = tokens.Create(user);

        return new AuthResponse(
            registration.UserId,
            user.FullName,
            email,
            registration.AccountNumber,
            accessToken.Token,
            accessToken.ExpiresAt);
    }

    public async Task<AuthResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await repository.GetUserByEmailAsync(request.Email.Trim().ToLowerInvariant(),cancellationToken);

        if (user is null || !passwords.Verify(user.PasswordHash, request.Password))
            throw new UnauthorizedException("Email or password is incorrect.");

        var account = await repository.GetAccountAsync(user.Id, cancellationToken)
            ?? throw new NotFoundException("account_not_found", "Account was not found.");
        var accessToken = tokens.Create(user);

        return new AuthResponse(
            user.Id,
            user.FullName,
            user.Email,
            account.AccountNumber,
            accessToken.Token,
            accessToken.ExpiresAt);
    }
}
