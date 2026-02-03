using System.Text.Json.Nodes;

public sealed class DpsTestWorker : BackgroundService
{
    private readonly IIotDevice _iotDevice;
    private readonly ILogger<DpsTestWorker> _logger;

    public DpsTestWorker(IIotDevice iotDevice, ILogger<DpsTestWorker> logger)
    {
        _iotDevice = iotDevice;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("IoT Test Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Build a simple test payload
                var payload = new JsonObject
                {
                    ["message"] = "Hello from test worker",
                    ["timestamp"] = DateTime.UtcNow.ToString("O")
                };

                await _iotDevice.SendMessageAsync(payload, stoppingToken);

                _logger.LogInformation("Test message sent successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send test message to IoT Hub.");
            }

            // Wait 30 seconds before sending again
            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}
