namespace WebDevMasterClass.Web.ShoppingCart;

public class ShoppingCartGrain(
    [PersistentState("ShoppingCartState")]
    IPersistentState<ShoppingCartGrain.ShoppingCartState> state
) : Grain, IShoppingCart
{   
    public Task AddItem(ShoppingCartItem item)
    {
        var existingItem = state.State.Items.FirstOrDefault(x => x.ProductId == item.ProductId);
        if (existingItem is null)
        {
            state.State.Items.Add(item);
        }
        else
        {
            existingItem.Quantity += item.Quantity;
        }
        return state.WriteStateAsync();
    }

    public Task<ShoppingCartItem[]> GetItems()
        => Task.FromResult(state.State.Items.ToArray());

    public Task Clear()
    {
        DeactivateOnIdle();
        return state.ClearStateAsync();
    }

    public class ShoppingCartState
    {
        public List<ShoppingCartItem> Items { get; set; } = [];
    }
}