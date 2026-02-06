public interface IDeviceRunner
{
    public string DeviceIp { get; }

    Task RunLoopAsync(CancellationToken ct);

    void Pause();
    void Resume();
    void AllowProbe();
}
