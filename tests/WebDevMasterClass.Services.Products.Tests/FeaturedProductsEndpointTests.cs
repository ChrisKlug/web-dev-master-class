using System.Net;
using Newtonsoft.Json.Linq;
using WebDevMasterClass.Services.Products.Data;
using WebDevMasterClass.Services.Products.Tests.Data;
using WebDevMasterClass.Testing;

namespace WebDevMasterClass.Services.Products.Tests;

public class FeaturedProductsEndpointTests(TestFixture<ProductsContext> fixture)
{
    [Fact]
    public Task GET_Returns_HTTP_200_and_all_products_marked_as_featured()
        => fixture.ExecuteTest<ProductsContext>(
            dbSetup: async cmd => 
            {
                await cmd.AddProduct("Product 1", "Description 1", 100m, true, "product1");
                await cmd.AddProduct("Product 2", "Description 2", 200m, true, "product2");
                await cmd.AddProduct("Product 3", "Description 3", 300m, true, "product3");
                await cmd.AddProduct("Product 4", "Description 4", 50m, false, "product4");
            },
            test: async client =>
            {
                var response = await client.GetAsync("/api/products/featured");
                
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);
                
                var products = JArray.Parse(await response.Content.ReadAsStringAsync());
                Assert.Equal(3, products.Count);
                
                Assert.Contains(products, x => x.Value<string>("name") == "Product 1");
                Assert.Contains(products, x => x.Value<string>("name") == "Product 2");
                Assert.Contains(products, x => x.Value<string>("name") == "Product 3");
            }
        );
    // { 
    //     await using var app = new WebApplicationFactory<Program>()
    //         .WithWebHostBuilder(builder =>
    //         {
    //             builder.UseEnvironment("IntegrationTesting");
    //             builder.ConfigureAppConfiguration((ctx, config) =>
    //             {
    //                 config.AddInMemoryCollection([
    //                     new KeyValuePair<string, string?>(
    //                         "ConnectionStrings:WebDevMasterClass-Products",
    //                         fixture.SqlConnectionString
    //                     )
    //                 ]);
    //             });
    //
    //             builder.ConfigureTestServices(services =>
    //             {
    //                 var dbDescriptor = services.First(x => x.ServiceType == typeof(ProductsContext));
    //                 var optionsDescriptor = services.First(x => x.ServiceType == typeof(DbContextOptions<ProductsContext>));
    //                 services.Remove(dbDescriptor);
    //                 services.Remove(optionsDescriptor);
    //                 services.AddDbContext<ProductsContext>((svc, options) => {
    //                     var config = svc.GetRequiredService<IConfiguration>();
    //                     options.UseSqlServer(config.GetConnectionString("WebDevMasterClass-Products"), opts =>
    //                     {
    //                         opts.ExecutionStrategy(x => new NonRetryingExecutionStrategy(x));
    //                     });
    //                 }, ServiceLifetime.Singleton);
    //             });
    //         });
    //     
    //     var ctx = app.Services.GetRequiredService<ProductsContext>();
    //     await using var transaction = await ctx.Database.BeginTransactionAsync();
    //     await using var conn = ctx.Database.GetDbConnection();
    //     await using var cmd = conn.CreateCommand();
    //     cmd.Transaction = transaction.GetDbTransaction();
    //     await cmd.AddProduct("Product 1", "Description 1", 100m, true, "product1");
    //     await cmd.AddProduct("Product 2", "Description 2", 200m, true, "product2");
    //     await cmd.AddProduct("Product 3", "Description 3", 300m, true, "product3");
    //     await cmd.AddProduct("Product 4", "Description 4", 50m, false, "product4");
    //     
    //     using var client = app.CreateClient();
    //     
    //     var response = await client.GetAsync("/api/products/featured");
    //     
    //     Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    //
    //     var products = JArray.Parse(await response.Content.ReadAsStringAsync());
    //     Assert.Equal(3, products.Count);
    //
    //     Assert.Contains(products, x => x.Value<string>("name") == "Product 1");
    //     Assert.Contains(products, x => x.Value<string>("name") == "Product 2");
    //     Assert.Contains(products, x => x.Value<string>("name") == "Product 3");
    // }
}