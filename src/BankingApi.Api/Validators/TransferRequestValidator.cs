using BankingApi.Api.Contracts;
using FluentValidation;

namespace BankingApi.Api.Validators;

public sealed class TransferRequestValidator : AbstractValidator<TransferRequest>
{
    public TransferRequestValidator()
    {
        RuleFor(x => x.RecipientAccountNumber)
            .NotEmpty()
            .Must(number => number.Length == 10 && number.All(char.IsDigit))
            .WithMessage("Recipient account number must contain exactly 10 digits.");
        RuleFor(x => x.Amount)
            .InclusiveBetween(0.01m, 999_999_999_999.99m);
        RuleFor(x => x.Description).MaximumLength(200);
    }
}
