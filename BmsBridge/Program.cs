using Serilog;

var builder = Host.CreateApplicationBuilder(args);

// Configuration
var configPath = Path.Combine(builder.Environment.ContentRootPath, "appsettings.json");
SettingsConfigurator.EnsureConfig(configPath);

// Logging
builder.Logging.ClearProviders();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Logging.AddSerilog();
Log.Information("Starting version v0.1.0");

builder.Services.Configure<AzureSettings>(builder.Configuration.GetSection("AzureSettings"));
builder.Services.Configure<GeneralSettings>(builder.Configuration.GetSection("GeneralSettings"));
builder.Services.Configure<NetworkSettings>(builder.Configuration.GetSection("NetworkSettings"));

// builder.Services.AddHostedService<Worker>();

var app = builder.Build();
app.Run();
