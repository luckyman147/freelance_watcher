using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using AzureWatcher.Function.Application.Services;
using AzureWatcher.Function.Domain.Interfaces;
using AzureWatcher.Function.Domain.Interfaces.Email;
using AzureWatcher.Function.Infrastructure.Email;
using AzureWatcher.Function.Infrastructure.Scraping;
using AzureWatcher.Function.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;
using AzureWatcher.Function;

var builder = Host.CreateApplicationBuilder(args);

// 1. Storage Configuration (PostgreSQL)
var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"]
                       ?? throw new System.InvalidOperationException("DefaultConnection config is missing.");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddTransient<IStorageService, DatabaseStorageService>();

// 2. HttpClient & Scraper Configuration
builder.Services.AddHttpClient<IJobScraperService, TunisieFreelanceApiScraper>();

// 3. Email Configuration
builder.Services.AddSingleton<IEmailService, SmtpEmailService>();

// 4. Application Services
builder.Services.AddScoped<JobMonitorService>();

// 5. Worker Service
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

// Wait for database to be ready and apply migrations
using (var scope = host.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    // Automatically apply any pending migrations or create the db
    db.Database.Migrate();
}

host.Run();
