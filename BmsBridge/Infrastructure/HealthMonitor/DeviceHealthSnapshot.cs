public sealed class DeviceHealthSnapshot
{
    public string DeviceIp { get; init; } = default!;
    public BmsType DeviceType { get; init; } = default!;

    public DateTimeOffset? LastSuccessUtc { get; init; }
    public DateTimeOffset? LastFailureUtc { get; init; }
    public int ConsecutiveFailures { get; init; }
    public TimeSpan? LastLatency { get; init; }
    public DeviceErrorType? LastErrorType { get; init; }
    public DeviceCircuitState CircuitState { get; init; }
}
