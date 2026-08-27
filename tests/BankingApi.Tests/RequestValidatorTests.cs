using BankingApi.Api.Contracts;
using BankingApi.Api.Validators;

namespace BankingApi.Tests;

public sealed class RequestValidatorTests
{
    [Fact]
    public void Register_RejectsInvalidFields()
    {
        var result = new RegisterRequestValidator().Validate(new RegisterRequest("A", "not-an-email", "password"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterRequest.FullName));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterRequest.Email));
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(RegisterRequest.Password));
    }

    [Theory]
    [InlineData("123456789")]
    [InlineData("12345678901")]
    [InlineData("12345A7890")]
    [InlineData("")]
    public void Transfer_RejectsInvalidAccountNumbers(string accountNumber)
    {
        var result = new TransferRequestValidator().Validate(new TransferRequest(accountNumber, 10m, null));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(TransferRequest.RecipientAccountNumber));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    [InlineData(1000000000000)]
    public void Transfer_RejectsInvalidAmounts(decimal amount)
    {
        var result = new TransferRequestValidator().Validate(new TransferRequest("1234567890", amount, null));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(TransferRequest.Amount));
    }

    [Fact]
    public void Profile_RejectsUnsupportedPhoneCharacters()
    {
        var result = new UpdateProfileRequestValidator().Validate(new UpdateProfileRequest("Valid Name", "+234-80-CALL"));

        Assert.Contains(result.Errors, error => error.PropertyName == nameof(UpdateProfileRequest.PhoneNumber));
    }
}
