namespace WebDevMasterClass.Web.ShoppingCart;

[GenerateSerializer]
public class ShoppingCartItem
{
    [Id(0)]
    public int ProductId { get; set; }
    [Id(1)]
    public string ProductName { get; set; } = string.Empty;
    [Id(2)]
    public decimal Price { get; set; }
    [Id(3)]
    public int Quantity { get; set; }
}