using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using Scalar.AspNetCore;

using Transactions.Api.Authentication;
using Transactions.Api.ExceptionHandling;
using Transactions.Application.DependencyInjection;
using Transactions.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddTransactionsApplication()
    .AddTransactionsInfrastructure(builder.Configuration);

builder.Services
    .AddOptions<JwtValidationOptions>()
    .Bind(builder.Configuration.GetSection(JwtValidationOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer();
builder.Services.ConfigureOptions<ConfigureJwtBearerOptions>();
builder.Services.AddAuthorization();

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Enums travel as names ("CreditCard", "Expense") instead of numbers. Controllers read the MVC JSON options,
// the OpenAPI document reads the minimal-API ones, so both need the converter or Scalar would show numbers.
builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddOpenApi(options => options.AddDocumentTransformer<BearerSecuritySchemeTransformer>());

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();