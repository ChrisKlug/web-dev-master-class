using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Extensions.FileProviders;
using Orleans.Dashboard;
using WebDevMasterClass.Services.Orders.gRPC;
using WebDevMasterClass.Services.Products.Client;
using WebDevMasterClass.Web.Models;
using WebDevMasterClass.Web.ShoppingCart;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var auth = builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie();

if (!builder.Environment.IsEnvironment("IntegrationTesting"))
{
    auth.AddOpenIdConnect(options =>
    {
        options.Authority = builder.Configuration["IdentityServer:Url"];

        options.ClientId = "interactive.mvc.sample";
        options.ClientSecret = "secret";

        options.ResponseType = "code";
        options.UsePkce = true;

        options.GetClaimsFromUserInfoEndpoint = true;
        options.SaveTokens = true;
        options.MapInboundClaims = false;
        options.DisableTelemetry = true;

        options.TokenValidationParameters = new()
        {
            NameClaimType = "name",
            RoleClaimType = "role"
        };

        options.Events.OnRedirectToIdentityProvider = ctx =>
        {
            if (ctx.HttpContext.Request.Path.StartsWithSegments("/api"))
            {
                ctx.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                ctx.HandleResponse();
            }
            return Task.CompletedTask;
        };
    });
}

builder.Services.AddHttpForwarderWithServiceDiscovery();
builder.Services.AddControllers();
builder.Services.AddOrleans(silo =>
{
    silo.UseLocalhostClustering();
    silo.AddMemoryGrainStorageAsDefault();
    silo.AddDashboard();
});

builder.Services.AddProductsClient(options =>
{
    options.BaseAddress = "https://products";
});
builder.Services.AddGrpcClient<OrdersService.OrdersServiceClient>(options =>
{
    options.Address = new Uri("https://orders");
});

var app = builder.Build();

app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/images/products",
    FileProvider = new PhysicalFileProvider(
        Path.Combine(
            app.Environment.ContentRootPath,
            "../../_resources/productimages")
    )
});

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapOrleansDashboard(routePrefix: "/dashboard");

app.MapGet("/api/me", (ClaimsPrincipal user) 
    => Results.Ok(user.Identity!.Name)
).RequireAuthorization();

app.MapPost("/api/shopping-cart", async (
    AddShoppingCartItemModel model, 
    HttpContext ctx,
    IProductsClient productsClient,
    IGrainFactory grainFactory
) =>
{
    var product = await productsClient.GetProduct(model.ProductId);
    if (product is null)
    {
        return Results.BadRequest();
    }
    string cartId;
    if (ctx.Request.Cookies.ContainsKey("ShoppingCartId")) 
    {
        cartId = ctx.Request.Cookies["ShoppingCartId"]!;
    }
    else 
    { 
        cartId = Guid.CreateVersion7().ToString();
        ctx.Response.Cookies.Append("ShoppingCartId", cartId);
    }
    var cart = grainFactory.GetGrain<IShoppingCart>(cartId);
    await cart.AddItem(new ShoppingCartItem
    {
        ProductId = product.Id,
        ProductName = product.Name,
        Price = product.Price,
        Quantity = model.Quantity
    });
    return Results.Ok(await cart.GetItems());
});
app.MapGet("/api/shopping-cart", async (
    HttpContext ctx,
    IGrainFactory grainFactory
) =>
{
    if (ctx.Request.Cookies.ContainsKey("ShoppingCartId"))
    {
        var cart = grainFactory.GetGrain<IShoppingCart>(
            ctx.Request.Cookies["ShoppingCartId"]!
        );
        return Results.Ok(await cart.GetItems());
    }
    return Results.Ok(Array.Empty<ShoppingCartItem>());
});

app.MapDefaultEndpoints();

app.Map("/api/{**catch-all}", (HttpContext ctx) =>
{
    ctx.Response.StatusCode = 404;
});
app.MapForwarder("/{**catch-all}", "https+http://ui");

app.Run();