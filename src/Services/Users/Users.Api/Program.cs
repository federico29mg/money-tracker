using Scalar.AspNetCore;

using Users.Api.ExceptionHandling;
using Users.Application.DependencyInjection;
using Users.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var usersDbConnectionString = builder.Configuration.GetConnectionString("UsersDb")
                              ?? throw new InvalidOperationException("Connection string 'UsersDb' is not configured.");

builder.Services
    .AddUsersApplication()
    .AddUsersInfrastructure(usersDbConnectionString);

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthorization();

app.MapControllers();

app.Run();