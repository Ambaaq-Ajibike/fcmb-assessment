namespace BankingApi.Api.Services;

public interface IPasswordService
{
    string Hash(string password);
    bool Verify(string hash, string password);
}
