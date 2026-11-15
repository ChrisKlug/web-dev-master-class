using WebDevMasterClass.AppHost.Extensions;

var builder = DistributedApplication.CreateBuilder(args);

var sql = builder.AddSqlServer("sqlserver")
    .WithDataVolume("webdevdata")
    .WithLifetime(ContainerLifetime.Persistent);

var db = sql.AddDatabase("WebDevMasterClass-Products")
    .WithSeedDataCommand();

var ordersDb = sql.AddDatabase("WebDevMasterClass-Orders");

var orders = builder.AddProject<Projects.Orders>("orders", "https")
    .WithEnvironment("ConnectionStrings__Sql", ordersDb.Resource.ConnectionStringExpression)
    .WaitFor(ordersDb);

#pragma warning disable ASPIRECERTIFICATES001
var idsrv = builder.AddContainer(
        "identityserver", 
        "zerokoll/webdevworkshop-identity-server"
    )
    .WithHttpsEndpoint(targetPort: 8081)
    .WithEnvironment("ASPNETCORE_URLS", "https://*:8081")
    .WithHttpsDeveloperCertificate()
    .WithHttpsCertificateConfiguration(ctx =>   {
        ctx.EnvironmentVariables["Kestrel__Certificates__Default__Path"] = ctx.CertificatePath;
        ctx.EnvironmentVariables["Kestrel__Certificates__Default__KeyPath"] = ctx.KeyPath;
        return Task.CompletedTask;
    })
    .WithExternalHttpEndpoints();
#pragma warning restore ASPIRECERTIFICATES001

var products = builder.AddProject<Projects.Products>("products")
    .WithReference(db)
    .WaitFor(db);

var ui = builder.AddDockerfile("ui", "../../_resources/ui")
    .WithHttpEndpoint(targetPort: 80)
    .WithUrlForEndpoint("http", url =>
    {
        url.DisplayLocation = UrlDisplayLocation.DetailsOnly;
    })
    .WithHttpHealthCheck("/");

builder.AddProject<Projects.Web>("web", "aspire")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithUrlForEndpoint("https", url =>
    {
        url.DisplayText = "Web";
    })
    .WithUrlForEndpoint("https", ep => new ResourceUrlAnnotation
    {
        Url = $"{ep.Url}/dashboard",
        DisplayText = "Orleans Dashboard"
    })
    .WithReference(ui.GetEndpoint("http"))
    .WaitFor(ui)
    .WithReference(products)
    .WaitFor(products)
    .WithReference(orders)
    .WaitFor(orders)
    .WithEnvironment("IdentityServer__Url", idsrv.GetEndpoint("https"));

builder.Build().Run();