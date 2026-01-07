using Banking.Application.Accounts.Commands;
using FluentValidation;

namespace Banking.Application.Accounts.Validation;

public sealed class CreateAccountValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();

        RuleFor(x => x.AccountType)
            .NotNull()
            .WithMessage("Account type is required.");
    }
}
