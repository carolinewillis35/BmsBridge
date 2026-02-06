using System.Collections.Concurrent;

public sealed class DeviceRunnerRegistry : IDeviceRunnerRegistry
{
    private readonly ConcurrentDictionary<string, IDeviceRunner> _devices = new();
    private readonly ILogger<DeviceRunnerRegistry> _logger;

    public DeviceRunnerRegistry(ILogger<DeviceRunnerRegistry> logger)
    {
        _logger = logger;
    }

    public void RegisterDevice(IDeviceRunner deviceRunner)
    {
        _logger.LogInformation($"Registering device {deviceRunner.DeviceIp} to health registry.");

        _devices.AddOrUpdate(
            deviceRunner.DeviceIp,
            deviceRunner,
            (_, __) => deviceRunner
        );
    }

    public IDeviceRunner? GetDeviceRunner(string deviceIp)
    {
        if (!_devices.TryGetValue(deviceIp, out var state))
            return null;

        lock (state)
        {
            return state;
        }
    }

    public IReadOnlyCollection<IDeviceRunner> GetAllDeviceRunners()
    {
        var list = new List<IDeviceRunner>();

        foreach (var kvp in _devices)
        {
            var state = kvp.Value;
            lock (state)
            {
                list.Add(state);
            }
        }
        return list;
    }
}
