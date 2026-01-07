using Banking.Application.Customers.Commands;
using Banking.Application.Customers.Validation;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace Banking.Application.Tests.Customers.Validation;

/// <summary>
/// Unit tests for CreateCustomerValidator.
/// Validates command-level input validation (email format, SSN/TIN format, required fields).
/// </summary>
public class CreateCustomerValidatorTests
{
    private readonly CreateCustomerValidator _validator = new();

    private static CreateCustomerCommand CreateValidCommand(
        string? firstName = "John",
        string? lastName = "Doe",
        string? email = "john.doe@example.com",
        string? ssnTin = "123-45-6789",
        string? street = "123 Main St",
        string? city = "Springfield",
        string? state = "IL",
        string? postalCode = "62701",
        string? country = "USA")
    {
        return new CreateCustomerCommand(
            FirstName: firstName ?? "John",
            LastName: lastName ?? "Doe",
            Suffix: null,
            Title: null,
            SsnTin: ssnTin ?? "123-45-6789",
            Email: email ?? "john.doe@example.com",
            Street: street ?? "123 Main St",
            City: city ?? "Springfield",
            State: state ?? "IL",
            PostalCode: postalCode ?? "62701",
            Country: country ?? "USA"
        );
    }

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("  ")]
    public void Validate_WithInvalidFirstName_ShouldFail(string firstName)
    {
        // Arrange
        var command = new CreateCustomerCommand(
            FirstName: firstName,
            LastName: "Doe",
            Suffix: null,
            Title: null,
            SsnTin: "123-45-6789",
            Email: "john.doe@example.com",
            Street: "123 Main St",
            City: "Springfield",
            State: "IL",
            PostalCode: "62701",
            Country: "USA"
        );

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FirstName");
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public void Validate_WithInvalidEmail_ShouldFail(string email)
    {
        // Arrange
        var command = CreateValidCommand(email: email);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_WithValidUsaSsn_ShouldPass()
    {
        // Arrange - SSN with dashes
        var command = CreateValidCommand(ssnTin: "123-45-6789");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidUsaSsnNoFormatting_ShouldPass()
    {
        // Arrange - SSN without dashes
        var command = CreateValidCommand(ssnTin: "123456789");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidForeignTin_ShouldPass()
    {
        // Arrange - Foreign TIN (8-20 alphanumeric)
        var command = CreateValidCommand(ssnTin: "ABC12345678");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("123")]           // Too short SSN
    [InlineData("ABC1234")]       // Foreign TIN too short (7 alphanumeric)
    public void Validate_WithInvalidSsnTin_ShouldFail(string ssnTin)
    {
        // Arrange
        var command = CreateValidCommand(ssnTin: ssnTin);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "SsnTin");
    }

    [Fact]
    public void Validate_WithMissingAddressField_ShouldFail()
    {
        // Arrange - Missing city
        var command = CreateValidCommand(city: "");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "City");
    }

    [Fact]
    public void Validate_WithWhitespaceOnlyAddressField_ShouldFail()
    {
        // Arrange - City is whitespace only
        var command = CreateValidCommand(city: "   ");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "City");
    }
}
