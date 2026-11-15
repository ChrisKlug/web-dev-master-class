using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using WebDevMasterClass.Services.Products.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddFastEndpoints();

builder.AddSqlServerDbContext<ProductsContext>("WebDevMasterClass-Products");
builder.Services.AddScoped<IProducts, EfProducts>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await using var ctx = scope.ServiceProvider.GetRequiredService<ProductsContext>();
    await ctx.Database.MigrateAsync();
}

app.MapDefaultEndpoints();

app.MapFastEndpoints();

app.Run();