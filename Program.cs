using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.EntityFrameworkCore;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using Microsoft.Extensions.Configuration;
using PAS.Data;
using PAS.DBContext;  // For AppDbContext
using PAS.Services;   // For all Services


var builder = WebApplication.CreateBuilder(args);

// Add logging configuration
builder.Logging.ClearProviders(); // Clear default logging providers
builder.Logging.AddConsole(); // Add console logger (you can also log to a file, debug, etc.)
builder.Logging.AddDebug();   // Optional: for debugging in Visual Studio Output

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddScoped<VendorService>();
builder.Services.AddScoped<InventoryService>();
builder.Services.AddScoped<PurchaseOrderService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<FieldService>();
builder.Services.AddScoped<QuoteService>();

// Connect to PostgreSQL database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
