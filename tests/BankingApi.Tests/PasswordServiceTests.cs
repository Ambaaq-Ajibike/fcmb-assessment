using BankingApi.Api.Services;
namespace BankingApi.Tests;

public sealed class PasswordServiceTests
{
    private readonly PasswordService _service = new();
    [Fact]
    public void Hash_And_Verify_RoundTrip()
    {
        var hash = _service.Hash("StrongPass1!");

        Assert.NotEqual("StrongPass1!", hash);
        Assert.True(_service.Verify(hash, "StrongPass1!"));
    }

    [Fact]
    public void Verify_Rejects_WrongPassword()
    {
        var hash = _service.Hash("StrongPass1!");

        Assert.False(_service.Verify(hash, "WrongPass1!"));
    }
}
