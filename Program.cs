using Auth.Config;

var builder = WebApplication.CreateBuilder(args);

IConfigurationSection apiSection = builder.Configuration.GetSection(ApiConfig.SectionName);
builder.Services.Configure<ApiConfig>(apiSection);

var apiConfig = apiSection.Get<ApiConfig>();

if (apiConfig is null) throw new Exception("ApiConfig not provided.");

builder.WebHost.UseUrls($"http://*:{apiConfig.Port}");

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
  app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("health", () => new
{
  service = apiConfig.Service,
  status = apiConfig.Status,
  port = apiConfig.Port,
  time = apiConfig.Time
});

await app.RunAsync();
