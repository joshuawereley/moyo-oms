using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

using Moyo.Oms.Api.Authorization;
using Moyo.Oms.Api.Configuration;
using Moyo.Oms.Api.Middleware;
using Moyo.Oms.Application;
using Moyo.Oms.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

string connectionString =
    builder.Configuration.GetConnectionString("OmsDatabase")
    ?? throw new InvalidOperationException("Connection string 'OmsDatabase' is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("EntraId"));

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.VendorAdministrator, policy =>
        policy.RequireClaim(
            AuthorizationPolicies.RoleClaimType,
            AuthorizationPolicies.VendorAdministrator));

builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOmsSwagger(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
