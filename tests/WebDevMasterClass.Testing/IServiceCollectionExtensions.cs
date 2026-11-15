using System.Security.Claims;
using idunno.Authentication.Basic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace WebDevMasterClass.Testing;

internal static class IServiceCollectionExtensions
{
    public static IServiceCollection AddTestAuthentication(
        this IServiceCollection services
    )
    {
        services.AddAuthentication(options =>
            {
                options.DefaultScheme = BasicAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = BasicAuthenticationDefaults.AuthenticationScheme;
            })
            .AddBasic(options =>
            {
                options.AllowInsecureProtocol =
                    true; // TestServer is plain http                                                                               
                options.Events = new BasicAuthenticationEvents
                {
                    OnValidateCredentials = ctx =>
                    {
                        if (ctx.Username.Equals("test", StringComparison.InvariantCultureIgnoreCase)
                            && ctx.Password == "test")
                        {
                            ctx.Principal = new ClaimsPrincipal(new ClaimsIdentity(
                                [new Claim("name", ctx.Username)], ctx.Scheme.Name, "name", "role"));
                            ctx.Success();
                        }

                        return Task.CompletedTask;
                    }
                };
            });

        return services;
    }
}