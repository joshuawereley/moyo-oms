using Moyo.Oms.Api.Middleware;
using Moyo.Oms.Application;
using Moyo.Oms.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

string connectionString =
    builder.Configuration.GetConnectionString("OmsDatabase")
    ?? throw new InvalidOperationException("Connection string 'OmsDatabase' is not configured.");

builder.Services.AddControllers();
builder.Services.AddApplication();
builder.Services.AddInfrastructure(connectionString);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.MapControllers();

app.Run();
