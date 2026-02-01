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


// TEMPORARY MANUAL TEST HARNESS
if (args.Contains("--test-operator"))
{
    var endpoint = new Uri("http://10.128.223.180:14106/JSON-RPC");

    var settings = new GeneralSettings
    {
        keep_alive = false,
        http_request_delay_seconds = 1,
        http_retry_count = 0,
        http_timeout_delay_seconds = 5
    };

    var executor = new HttpPipelineExecutor(settings);

    // var op = new E2GetControllerListOperation(endpoint);
    // var op = new E2GetAlarmListOperation(endpoint, "HVAC/LTS");

    var loader = new EmbeddedE2IndexMappingProvider();

    var op = new E2GetPointsOperation(endpoint, "HVAC/LTS", "AC1 FAN", loader.GetPointsForCellType(33));

    await op.ExecuteAsync(executor, CancellationToken.None);

    return;
}

// var app = builder.Build();
// app.Run();
