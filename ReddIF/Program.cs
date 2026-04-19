using Scalar.AspNetCore;
using Supabase;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var supabaseUrl = "https://nxegzzyxpwrnsptcfcjg.supabase.co";
var supabaseKey = "sb_secret_ye4tqYkUH7XdmJc7PztV7g_55Uaw_Qr";  
    
    
builder.Services.AddSingleton(provider =>
{
    var client = new Client(supabaseUrl, supabaseKey);
    client.InitializeAsync().Wait();
    return client;
});

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