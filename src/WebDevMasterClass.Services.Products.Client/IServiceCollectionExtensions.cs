using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace WebDevMasterClass.Services.Products.Client;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddProductsClient(
        this IServiceCollection services,
        Action<ProductsClientOptions> config
    )
    {
        services.AddOptions<ProductsClientOptions>()
            .Configure(config)
            .ValidateOnStart();
        
        services.AddHttpClient<IProductsClient, HttpProductsClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<ProductsClientOptions>>();
            client.BaseAddress = new Uri(options.Value.BaseAddress!);
        });
        return services;
    }
}

public class ProductsClientOptions
{
    [Required]
    public string? BaseAddress { get; set; }
}