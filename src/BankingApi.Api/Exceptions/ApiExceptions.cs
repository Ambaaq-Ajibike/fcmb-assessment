namespace BankingApi.Api.Exceptions;

public abstract class ApiException(int statusCode, string code, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
    public string Code { get; } = code;
}
public sealed class ConflictException(string code, string message) : ApiException(409, code, message);
public sealed class NotFoundException(string code, string message) : ApiException(404, code, message);
public sealed class ValidationException(string code, string message) : ApiException(400, code, message);
public sealed class UnauthorizedException(string message) : ApiException(401, "invalid_credentials", message);
