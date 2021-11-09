using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Host
{
    public static class AuthorizationExtentions
    {
        public static IServiceCollection AddAuthorizationAndPolicies(this IServiceCollection services, IConfiguration configuration)
        {
            return services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireClaim(ClaimTypes.Role, configuration.GetValue<string>("oidc:roles:user"))
                    .Build();

                options.AddPolicy("admin", policy => policy
                    .RequireAuthenticatedUser()
                    .RequireClaim(ClaimTypes.Role, configuration.GetValue<string>("oidc:roles:admin")));
            });
        }
    }
}
