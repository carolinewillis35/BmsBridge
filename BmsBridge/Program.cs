using Serilog;
using Microsoft.Extensions.Options;
using System.Runtime.InteropServices;

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

// Singletons
// builder.Services.AddSingleton<IIotDevice, AzureIotDevice>(); // prod // TODO: Make dynamic
builder.Services.AddSingleton<IIotDevice, ConsoleIotDevice>(); // test

if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
{
    builder.Services.AddSingleton<ICertificateSource>(sp =>
    {
        var options = sp.GetRequiredService<IOptions<AzureSettings>>();
        return new StoreCertificateSource(options);
    });
}
else
{
    builder.Services.AddSingleton<ICertificateSource>(sp =>
        new PfxCertificateSource("/home/henry/Projects/BmsBridge/BmsBridge/DevelopmentKeys/CertificateTest.pfx")); // TODO: make dynamic/from user file
}
builder.Services.AddSingleton<CertificateProvider>();
builder.Services.AddSingleton<KeyvaultService>();
builder.Services.AddSingleton<DpsService>();
builder.Services.AddSingleton<IE2IndexMappingProvider, EmbeddedE2IndexMappingProvider>();
builder.Services.AddSingleton<INormalizerService, NormalizerService>();

// builder.Services.AddSingleton<IDeviceRunnerFactory, DeviceRunnerFactory>(); // prod // TODO: Make dynamic
builder.Services.AddSingleton<IDeviceRunnerFactory, ReplayDeviceRunnerFactory>(); // test

builder.Services.AddSingleton<IDeviceHealthRegistry, InMemoryDeviceHealthRegistry>();
builder.Services.AddSingleton<ICircuitBreakerService, CircuitBreakerService>();
builder.Services.AddSingleton<IHealthTelemetryService, HealthTelemetryService>();
builder.Services.AddSingleton<IRunnerControlService, RunnerControlService>();
builder.Services.AddSingleton<IDeviceRunnerRegistry, DeviceRunnerRegistry>();

// Workers
builder.Services.AddHostedService<DeviceWorker>();
builder.Services.AddHostedService<HealthMonitorWorker>();


var app = builder.Build();
app.Run();
