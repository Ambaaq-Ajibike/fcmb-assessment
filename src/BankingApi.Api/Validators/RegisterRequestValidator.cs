using BankingApi.Api.Contracts;
using FluentValidation;

namespace BankingApi.Api.Validators;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().Length(2, 150);
        RuleFor(x => x.Email).NotEmpty().MaximumLength(254).EmailAddress();
        RuleFor(x => x.Password)
            .NotEmpty()
            .Length(8, 128)
            .Must(password => password.Any(char.IsUpper)).WithMessage("Password must contain an uppercase letter.")
            .Must(password => password.Any(char.IsLower)).WithMessage("Password must contain a lowercase letter.")
            .Must(password => password.Any(char.IsDigit)).WithMessage("Password must contain a number.");
    }
}
