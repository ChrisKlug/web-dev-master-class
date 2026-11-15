namespace WebDevMasterClass.Services.Orders.Entities;

public class DeliveryAddress : Address
{
    private DeliveryAddress() { } // For EF Core
    private DeliveryAddress(string name, string street1, string? street2,
        string postalCode, string city, string country)
        : base(name, street1, street2, postalCode, city, country)
    {
    }

    public static DeliveryAddress Create(string name, string street1, string? street2,
        string postalCode, string city, string country)
        => new DeliveryAddress(name, street1, street2, postalCode, city, country);
}