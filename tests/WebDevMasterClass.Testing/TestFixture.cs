using System.Data.Common;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.MsSql;
using Xunit;

namespace WebDevMasterClass.Testing;

public class TestFixture
{
    public async Task ExecuteTest<TProgram>(
        Func<HttpClient, Task> test,
        Action<IServiceCollection>? serviceConfig = null,
        bool isAuthenticated = true
    ) where TProgram : class
    {
        await using var app = new WebApplicationFactory<TProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("IntegrationTesting");
                builder.ConfigureTestServices(services =>
                {
                    serviceConfig?.Invoke(services);
                    services.AddTestAuthentication();
                });
            });
        
        using var client = app.CreateClient();
        
        if (isAuthenticated)
        {
            var base64EncodedAuthenticationString = Convert.ToBase64String("test:test"u8.ToArray());
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64EncodedAuthenticationString);
        }
        
        await test(client);
    }
}

public class TestFixture<TDbContext> : IAsyncLifetime where TDbContext: DbContext
{
    private MsSqlContainer sqlContainer = 
        new MsSqlBuilder("mcr.microsoft.com/mssql/server:2022-latest")
            .WithDatabase(typeof(TDbContext).Name)
            .WithReuse(true)
            .Build();
    
    public async Task ExecuteTest<TProgram>(
        Func<DbCommand, Task> dbSetup, 
        Func<HttpClient, Task> test
    ) where TProgram : class
    {
        await using var app = new WebApplicationFactory<TProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("IntegrationTesting");
                builder.ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddInMemoryCollection([
                        new KeyValuePair<string, string?>(
                            "ConnectionStrings:WebDevMasterClass-Products",
                            SqlConnectionString
                        )
                    ]);
                });

                builder.ConfigureTestServices(services =>
                {
                    var dbDescriptor = services.First(x => x.ServiceType == typeof(TDbContext));
                    var optionsDescriptor = services.First(x => x.ServiceType == typeof(IDbContextOptionsConfiguration<TDbContext>));
                    services.Remove(dbDescriptor);
                    services.Remove(optionsDescriptor);
                    services.AddDbContext<TDbContext>((svc, options) => {
                        var config = svc.GetRequiredService<IConfiguration>();
                        options.UseSqlServer(config.GetConnectionString("WebDevMasterClass-Products"), opts =>
                        {
                            opts.ExecutionStrategy(x => new NonRetryingExecutionStrategy(x));
                        });
                    }, ServiceLifetime.Singleton);
                    services.AddTestAuthentication();
                });
            });
        
        var ctx = app.Services.GetRequiredService<TDbContext>();
        await using var transaction = await ctx.Database.BeginTransactionAsync();
        await using var conn = ctx.Database.GetDbConnection();
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = transaction.GetDbTransaction();
        await dbSetup(cmd);
        
        using var client = app.CreateClient();
        await test(client);
    }
    
    public async Task ExecuteTest<TProgram, TGrpcService>(
        Func<TGrpcService, Task> test,
        Func<DbCommand, Task>? dbSetup = null,
        Func<DbCommand, Task>? validateDb = null,
        IInterceptor[]? interceptors = null
    )
        where TProgram : class
        where TGrpcService : ClientBase
    {
        var app = new WebApplicationFactory<TProgram>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("IntegrationTesting");
                builder.ConfigureAppConfiguration((ctx, config) =>
                {
                    config.AddInMemoryCollection([
                        new KeyValuePair<string, string?>(
                            "ConnectionStrings:Sql",
                            SqlConnectionString
                        )
                    ]);
                });

                builder.ConfigureTestServices(services =>
                {
                    var dbDescriptor = services.First(x => x.ServiceType == typeof(TDbContext));
                    var optionsDescriptor = services.First(x => x.ServiceType == typeof(IDbContextOptionsConfiguration<TDbContext>));
                    services.Remove(dbDescriptor);
                    services.Remove(optionsDescriptor);
                    services.AddDbContext<TDbContext>((svc, options) =>
                    {
                        var config = svc.GetRequiredService<IConfiguration>();
                        options.UseSqlServer(config.GetConnectionString("Sql"), opts =>
                        {
                            opts.ExecutionStrategy(x => new NonRetryingExecutionStrategy(x));
                        });
                        options.AddInterceptors(interceptors ?? []);
                    }, ServiceLifetime.Singleton);
                });
            });
        
        using var services = app.Services.CreateScope();

        var ctx = app.Services.GetRequiredService<TDbContext>();
        await using var transaction = await ctx.Database.BeginTransactionAsync();
        await using var conn = ctx.Database.GetDbConnection();
        await using var cmd = conn.CreateCommand();
        cmd.Transaction = transaction.GetDbTransaction();
        
        if (dbSetup != null)
            await dbSetup(cmd);

        var options = new GrpcChannelOptions {
            HttpHandler = app.Server.CreateHandler()
        };
        var channel = GrpcChannel.ForAddress(app.Server.BaseAddress, options);
        var client = (TGrpcService)Activator.CreateInstance(typeof(TGrpcService), channel)!;
        await test(client);

        if (validateDb != null)
            await validateDb(cmd);
    }
    
    public ValueTask DisposeAsync()
        => sqlContainer.DisposeAsync();

    public async ValueTask InitializeAsync()
    {
        await sqlContainer.StartAsync();
        
        var options = new DbContextOptionsBuilder<TDbContext>()
            .UseSqlServer(SqlConnectionString);

        var dbContext = (TDbContext)Activator.CreateInstance(typeof(TDbContext), options.Options)!;
        await dbContext.Database.MigrateAsync();
    }
    
    public string SqlConnectionString => sqlContainer.GetConnectionString();
}