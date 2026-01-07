using Banking.Application.Accounts.Commands;
using FluentValidation;

namespace Banking.Application.Accounts.Validation;

public sealed class UpdateAccountValidator : AbstractValidator<UpdateAccountCommand>
{
    public UpdateAccountValidator()
    {
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.AccountId).NotEmpty();
        RuleFor(x => x.AvailableBalance).Must(v => v >= -1_000_000_000m)
            .WithMessage("Available balance is out of range.");
    }
}
