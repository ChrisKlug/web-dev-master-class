namespace WebDevMasterClass.Services.Orders.Entities;

public abstract class Address
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    protected Address() {} // For EF Core
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    protected Address(string name, string street1, string? street2, 
        string postalCode, string city, string country)
    {
        Name = name;
        Street1 = street1;
        Street2 = street2;
        PostalCode = postalCode;
        City = city;
        Country = country;
    }

    public string Name { get; private set; }
    public string Street1 { get; private set; }
    public string? Street2 { get; private set; }
    public string PostalCode { get; private set; }
    public string City { get; private set; }
    public string Country { get; private set; }
}