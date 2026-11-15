using WebDevMasterClass.Services.Products.Entities;

namespace WebDevMasterClass.Services.Products.Data;

public interface IProducts
{
    Task<Product?> WithId(int id);
    Task<Product[]> ThatAreFeatured();
}