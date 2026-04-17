using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Scalar.AspNetCore;
using ReddIF.Context;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

string? mySqlConnection = builder.Configuration.GetConnectionString("DefaulConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseMySql(mySqlConnection,
        ServerVersion.AutoDetect(new MySqlConnection())));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
} 

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();