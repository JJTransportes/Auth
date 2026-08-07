using Auth.Config;
using Auth.Data;
using Auth.Endpoints;
using Auth.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

IConfigurationSection appConfigSection = builder.Configuration.GetSection(AppConfig.SectionName);
builder.Services.Configure<AppConfig>(appConfigSection);

var appConfig = appConfigSection.Get<AppConfig>();

if (appConfig is null) throw new Exception("AppConfig not provided.");

builder.WebHost.UseUrls($"http://*:{appConfig.Port}");

builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(appConfig.ConnectionString));

builder.Services.AddScoped<IAccountRepository, AccountRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapAccountEndpoints();

app.MapGet("health", () => new
{
  service = appConfig.Service,
  status = appConfig.Status,
  port = appConfig.Port,
  time = appConfig.Time
});

await app.RunAsync();
