using Users.Application.DependencyInjection;
using Users.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

var usersDbConnectionString = builder.Configuration.GetConnectionString("UsersDb")
                              ?? throw new InvalidOperationException("Connection string 'UsersDb' is not configured.");

builder.Services
    .AddUsersApplication()
    .AddUsersInfrastructure(usersDbConnectionString);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();