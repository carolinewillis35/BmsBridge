public class HealthRegistryTests
{
    [Fact]
    public void HealthRegistry_TracksSuccessAndFailure()
    {
        var registry = new InMemoryDeviceHealthRegistry();

        var deviceIp = "10.0.0.5";
        registry.RegisterDevice("10.0.0.5", BmsType.EmersonE2);

        registry.RecordFailure(deviceIp, DeviceErrorType.Timeout);
        registry.RecordFailure(deviceIp, DeviceErrorType.Timeout);
        registry.RecordSuccess(deviceIp, TimeSpan.FromMilliseconds(120));

        var snapshot = registry.GetSnapshot(deviceIp)!;

        Assert.Equal("10.0.0.5", snapshot.DeviceIp);
        Assert.Equal(BmsType.EmersonE2, snapshot.DeviceType);
        Assert.Equal(0, snapshot.ConsecutiveFailures);
        Assert.Equal(DeviceErrorType.None, snapshot.LastErrorType);
        Assert.NotNull(snapshot.LastSuccessUtc);
        Assert.NotNull(snapshot.LastFailureUtc);
    }
}
