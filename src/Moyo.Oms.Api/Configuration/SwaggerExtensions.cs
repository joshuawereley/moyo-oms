using Microsoft.OpenApi.Models;

namespace Moyo.Oms.Api.Configuration;

/// <summary>
/// Configures Swagger/OpenAPI with Entra ID Auth2 for interactive testing.
/// </summary>

public static class SwaggerExtensions
{
    public static IServiceCollection AddOmsSwagger(
            this IServiceCollection services,
            IConfiguration configuration)
    {
        string tenantId = configuration["EntraId:TenantId"]!;
        string clientId = configuration["EntraId:ClientId"]!;
        string scope = $"api://{clientId}/access_as_user";

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new
                        Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize"),
                        TokenUrl = new Uri($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            [scope] = "Access the MOYO OMS API",
                        },
                    },
                },
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2",
                        },
                    },
                    new[] { scope }
                },
            });
        });

        return services;
    }
}
