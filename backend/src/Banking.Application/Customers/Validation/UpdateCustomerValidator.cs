using FluentValidation;
using System.Text.RegularExpressions;
using Banking.Application.Customers.Commands;

namespace Banking.Application.Customers.Validation;

public sealed class UpdateCustomerValidator : AbstractValidator<UpdateCustomerCommand>
{
    public UpdateCustomerValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.FirstName).NotEmpty().Must(NotWhiteSpace);
        RuleFor(x => x.LastName).NotEmpty().Must(NotWhiteSpace);

        RuleFor(x => x.SsnTin).NotEmpty().Must(BeValidSsnOrTin)
            .WithMessage("SSN/TIN must be a US SSN (9 digits) or a foreign TIN (8-20 alphanumeric).");

        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        RuleFor(x => x.Street).NotEmpty().Must(NotWhiteSpace);
        RuleFor(x => x.City).NotEmpty().Must(NotWhiteSpace);
        RuleFor(x => x.State).NotEmpty().Must(NotWhiteSpace);
        RuleFor(x => x.PostalCode).NotEmpty().Must(NotWhiteSpace);
        RuleFor(x => x.Country).NotEmpty().Must(NotWhiteSpace);
    }

    private static bool NotWhiteSpace(string s) => !string.IsNullOrWhiteSpace(s);

    private static bool BeValidSsnOrTin(string s)
    {
        var trimmed = s.Trim();
        var digitsOnly = Regex.Replace(trimmed, @"[\s-]", "");
        if (Regex.IsMatch(digitsOnly, @"^\d{9}$")) return true;

        var norm = new string(trimmed.Where(char.IsLetterOrDigit).ToArray());
        return Regex.IsMatch(norm, @"^[A-Za-z0-9]{8,20}$");
    }
}
