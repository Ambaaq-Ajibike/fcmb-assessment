using BankingApi.Api.Contracts;
using FluentValidation;

namespace BankingApi.Api.Validators;

public sealed class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    private static readonly HashSet<char> AllowedPhoneSymbols = ['+', '-', ' ', '(', ')'];

    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().Length(2, 150);
        When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber), () =>
        {
            RuleFor(x => x.PhoneNumber!)
                .MaximumLength(30)
                .Must(phone => phone.All(character => char.IsDigit(character) || AllowedPhoneSymbols.Contains(character)))
                .WithMessage("Phone number contains unsupported characters.")
                .Must(phone => phone.Any(char.IsDigit))
                .WithMessage("Phone number must contain at least one digit.");
        });
    }
}
