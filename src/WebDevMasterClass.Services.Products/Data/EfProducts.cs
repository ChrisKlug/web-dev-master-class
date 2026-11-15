using Microsoft.EntityFrameworkCore;
using WebDevMasterClass.Services.Products.Entities;

namespace WebDevMasterClass.Services.Products.Data;

public class EfProducts(ProductsContext ctx) : IProducts
{
    public Task<Product?> WithId(int id)
        => ctx.Set<Product>()
            .FirstOrDefaultAsync(p => p.Id == id);

    public Task<Product[]> ThatAreFeatured()
        => ctx.Set<Product>()
            .Where(x => x.IsFeatured)
            .ToArrayAsync();
}