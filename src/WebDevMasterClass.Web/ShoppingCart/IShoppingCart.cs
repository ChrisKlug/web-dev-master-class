namespace WebDevMasterClass.Web.ShoppingCart;

public interface IShoppingCart : IGrainWithStringKey
{
    Task AddItem(ShoppingCartItem item);
    Task<ShoppingCartItem[]> GetItems();
    Task Clear();
}