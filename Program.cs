using System.Reflection;
using System.Text.Json.Serialization;
using Auth.Config;
using Auth.Data;
using Auth.Endpoints;
using Auth.Interfaces;
using Auth.Repositories;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

IConfigurationSection appConfigSection = builder.Configuration.GetSection(AppConfig.SectionName);
builder.Services.Configure<AppConfig>(appConfigSection);

var appConfig = appConfigSection.Get<AppConfig>();

if (appConfig is null) throw new Exception("AppConfig not provided.");

builder.WebHost.UseUrls($"http://*:{appConfig.Port}");

builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(options =>
    {
        options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(appConfig.ConnectionString));

builder.Services.AddScoped<IEmailVerificationRepository, EmailVerificationRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), includeInternalTypes: true);

builder.Services.AddMassTransit(bus =>
    {
        var currentAssembly = Assembly.GetExecutingAssembly();

        bus.SetKebabCaseEndpointNameFormatter();

        bus.AddConsumers(currentAssembly);

        bus.UsingRabbitMq((context, configurator) =>
        {
            configurator.Host(new Uri(appConfig.MessageHost), x =>
            {
                x.Username(appConfig.MessageUser);
                x.Password(appConfig.MessagePassword);
            });

            configurator.ConfigureEndpoints(context);
        });
    });

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
