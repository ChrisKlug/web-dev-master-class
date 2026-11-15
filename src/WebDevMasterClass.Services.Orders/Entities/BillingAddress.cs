namespace WebDevMasterClass.Services.Orders.Entities;

public class BillingAddress : Address
{
    private BillingAddress() { } // For EF Core
    private BillingAddress(string name, string street1, string? street2,
        string postalCode, string city, string country)
        : base(name, street1, street2, postalCode, city, country)
    {
    }

    public static BillingAddress Create(string name, string street1, string? street2,
        string postalCode, string city, string country)
        => new BillingAddress(name, street1, street2, postalCode, city, country);
}