public sealed class RunnerControlService : IRunnerControlService
{
    private readonly IDeviceRunnerRegistry _runnerRegistry;
    private readonly ILogger<RunnerControlService> _logger;

    public RunnerControlService(
        IDeviceRunnerRegistry runnerRegistry,
        ILogger<RunnerControlService> logger)
    {
        _runnerRegistry = runnerRegistry;
        _logger = logger;
    }

    public void ApplyControl(DeviceHealthSnapshot snapshot)
    {
        var runner = _runnerRegistry.GetDeviceRunner(snapshot.DeviceIp);

        if (runner is null)
            return;

        switch (snapshot.CircuitState)
        {
            case DeviceCircuitState.Closed:
                runner.Resume();
                break;

            case DeviceCircuitState.Open:
                runner.Pause();
                break;

            case DeviceCircuitState.HalfOpen:
                runner.AllowProbe();
                break;
        }
    }
}
