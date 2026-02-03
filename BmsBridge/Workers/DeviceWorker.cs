using Microsoft.Extensions.Options;

public sealed class DeviceWorker : BackgroundService
{
    private readonly ILogger<DeviceWorker> _logger;
    private readonly IDeviceRunnerFactory _deviceRunnerFactory;
    private readonly NetworkSettings _networkSettings;

    public DeviceWorker(IDeviceRunnerFactory deviceRunnerFactory, IOptions<NetworkSettings> networkSettings, ILogger<DeviceWorker> logger)
    {
        _deviceRunnerFactory = deviceRunnerFactory;
        _networkSettings = networkSettings.Value;
        _logger = logger;
    }

    private IEnumerable<IDeviceRunner> GetDeviceRunners()
    {
        List<IDeviceRunner> deviceRunners = new();

        _logger.LogInformation("Loading {Count} devices", _networkSettings.bms_devices.Count);
        foreach (var deviceConfig in _networkSettings.bms_devices)
        {
            deviceRunners.Add(_deviceRunnerFactory.Create(deviceConfig));
            _logger.LogInformation("Device: IP={IP}, Type={Type}", deviceConfig.IP, deviceConfig.DeviceType);
        }

        return deviceRunners;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var cycleCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            var cycleToken = cycleCts.Token;

            var restartAfter = TimeSpan.FromHours(4);

            var timerTask = Task.Delay(restartAfter, stoppingToken)
                .ContinueWith(_ => cycleCts.Cancel());

            var runners = GetDeviceRunners();

            var tasks = runners
                .Select(runner => runner.RunLoopAsync(cycleToken))
                .ToList();

            _logger.LogInformation("DeviceWorker started {Count} device runners.", tasks.Count);

            var completed = await Task.WhenAny(Task.WhenAll(tasks), timerTask);

            if (completed == timerTask)
            {
                cycleCts.Cancel();
                await Task.WhenAll(tasks);
                continue;
            }
        }
    }
}
