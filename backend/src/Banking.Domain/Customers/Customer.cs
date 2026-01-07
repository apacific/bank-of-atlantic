using Banking.Domain.Accounts;

namespace Banking.Domain.Customers;

/// <summary>
/// Represents a bank customer with personal information and associated accounts.
/// Aggregates customer details including name, contact info, and financial accounts.
/// </summary>
public sealed class Customer
{
    /// <summary>
    /// Unique identifier for the customer.
    /// </summary>
    public Guid Id { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// The customer's first name.
    /// </summary>
    public string FirstName { get; private set; } = "";

    /// <summary>
    /// The customer's last name.
    /// </summary>
    public string LastName  { get; private set; } = "";

    /// <summary>
    /// Optional suffix (Jr., Sr., III, etc.).
    /// </summary>
    public string? Suffix   { get; private set; }

    /// <summary>
    /// Optional title (Mr., Ms., Dr., etc.).
    /// </summary>
    public string? Title    { get; private set; }

    /// <summary>
    /// Social Security Number or Tax ID.
    /// </summary>
    public string SsnTin { get; private set; } = "";

    /// <summary>
    /// Normalized SSN/TIN (uppercase, digits and letters only).
    /// </summary>
    public string SsnTinNormalized { get; private set; } = "";

    /// <summary>
    /// The customer's email address.
    /// </summary>
    public string Email { get; private set; } = "";

    /// <summary>
    /// Normalized email address (lowercase, trimmed).
    /// </summary>
    public string EmailNormalized { get; private set; } = "";

    /// <summary>
    /// The customer's mailing address.
    /// </summary>
    public Address MailingAddress { get; private set; } = default!;

    /// <summary>
    /// The date the customer relationship was established.
    /// </summary>
    public DateOnly CustomerSince { get; private set; }

    /// <summary>
    /// Collection of accounts owned by this customer.
    /// </summary>
    public List<Account> Accounts { get; private set; } = new();

    private Customer() { }

    /// <summary>
    /// Creates a new customer with initial profile information.
    /// </summary>
    /// <param name="firstName">First name</param>
    /// <param name="lastName">Last name</param>
    /// <param name="suffix">Optional name suffix</param>
    /// <param name="title">Optional title</param>
    /// <param name="ssnTin">Social Security or Tax ID</param>
    /// <param name="email">Email address</param>
    /// <param name="address">Mailing address</param>
    /// <param name="customerSince">Date customer relationship started</param>
    public Customer(
        string firstName, string lastName, string? suffix, string? title,
        string ssnTin, string email, Address address, DateOnly customerSince)
    {
        UpdateProfile(firstName, lastName, suffix, title, ssnTin, email, address);
        CustomerSince = customerSince;
    }

    /// <summary>
    /// Updates the customer's profile information.
    /// Normalizes and trims all string inputs.
    /// </summary>
    /// <param name="firstName">First name</param>
    /// <param name="lastName">Last name</param>
    /// <param name="suffix">Optional name suffix</param>
    /// <param name="title">Optional title</param>
    /// <param name="ssnTin">Social Security or Tax ID</param>
    /// <param name="email">Email address</param>
    /// <param name="address">Mailing address</param>
    public void UpdateProfile(
        string firstName, string lastName, string? suffix, string? title,
        string ssnTin, string email, Address address)
    {
        FirstName = firstName.Trim();
        LastName  = lastName.Trim();
        Suffix    = string.IsNullOrWhiteSpace(suffix) ? null : suffix.Trim();
        Title     = string.IsNullOrWhiteSpace(title) ? null : title.Trim();

        SsnTin = ssnTin.Trim();
        SsnTinNormalized = NormalizeSsnTin(ssnTin);

        Email = email.Trim();
        EmailNormalized = NormalizeEmail(email);

        MailingAddress = address;
    }

    /// <summary>
    /// Normalizes an email address to lowercase.
    /// </summary>
    /// <param name="email">The email address to normalize</param>
    /// <returns>Normalized email (trimmed, lowercase)</returns>
    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    /// <summary>
    /// Normalizes SSN/TIN by extracting only alphanumeric characters and converting to uppercase.
    /// </summary>
    /// <param name="v">The SSN/TIN to normalize</param>
    /// <returns>Normalized SSN/TIN (uppercase alphanumeric only)</returns>
    private static string NormalizeSsnTin(string v)
        => new string(v.Trim().ToUpperInvariant().Where(char.IsLetterOrDigit).ToArray());
}
