using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;

using Moyo.Oms.Api.Authorization;
using Moyo.Oms.Api.Configuration;
using Moyo.Oms.Api.Identity;
using Moyo.Oms.Api.Middleware;
using Moyo.Oms.Application;
using Moyo.Oms.Application.Abstractions.Identity;
using Moyo.Oms.Application.Abstractions.Products;
using Moyo.Oms.Application.Products;
using Moyo.Oms.Infrastructure;
using Moyo.Oms.Infrastructure.Products;

var builder = WebApplication.CreateBuilder(args);

const string vendorPortalCorsPolicy = "AllowVendorPortal";

string connectionString =
    builder.Configuration.GetConnectionString("OmsDatabase")
    ?? throw new InvalidOperationException("Connection string 'OmsDatabase' is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("EntraId"));

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.VendorAdministrator, policy =>
        policy.RequireRole(AuthorizationPolicies.VendorAdministrator));

builder.Services.AddCors(options =>
{
    options.AddPolicy(vendorPortalCorsPolicy, policy =>
        policy
            .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [])
            .AllowAnyHeader()
            .AllowAnyMethod());
});

// Requests carry enums as their names ("Increase", "Shipped"). Without this converter
// System.Text.Json only binds enums from their numeric values and rejects the body with a 400.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

builder.Services.AddOptions<PmsOptions>()
    .Bind(builder.Configuration.GetSection(PmsOptions.SectionName));

builder.Services.AddHttpClient<IProductCatalogClient, HttpProductCatalogClient>((serviceProvider, client) =>
{
    PmsOptions options = serviceProvider.GetRequiredService<IOptions<PmsOptions>>().Value;
    client.BaseAddress = new Uri(options.BaseUrl);
});

builder.Services.AddScoped<IProductSyncService, ProductSyncService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, CurrentUser>();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOmsSwagger(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment() || app.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.OAuthClientId(app.Configuration["EntraId:ClientId"]!);
        options.OAuthUsePkce();
        options.OAuthScopeSeparator(" ");
    });
}

app.UseExceptionHandler();

app.UseCors(vendorPortalCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
