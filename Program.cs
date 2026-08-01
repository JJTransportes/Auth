using Auth.Config;

var builder = WebApplication.CreateBuilder(args);

IConfigurationSection appConfigSection = builder.Configuration.GetSection(AppConfig.SectionName);
builder.Services.Configure<AppConfig>(appConfigSection);

var appConfig = appConfigSection.Get<AppConfig>();

if (appConfig is null) throw new Exception("AppConfig not provided.");

builder.WebHost.UseUrls($"http://*:{appConfig.Port}");

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("health", () => new
{
  service = appConfig.Service,
  status = appConfig.Status,
  port = appConfig.Port,
  time = appConfig.Time
});

await app.RunAsync();
