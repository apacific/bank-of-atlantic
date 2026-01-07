namespace Banking.Domain.Customers;

public sealed class Address
{
    public string Street { get; private set; } = "";
    public string City { get; private set; } = "";
    public string State { get; private set; } = "";
    public string PostalCode { get; private set; } = "";
    public string Country { get; private set; } = "";

    private Address() { }

    public Address(string street, string city, string state, string postalCode, string country)
    {
        Street = street;
        City = city;
        State = state;
        PostalCode = postalCode;
        Country = country;
    }
}
