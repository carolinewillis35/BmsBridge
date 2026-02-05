using FakeItEasy;
using Microsoft.Extensions.Logging;

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

    [Fact]
    public async Task Wrapper_RecordsFailure_OnTimeout()
    {
        var fakeInner = A.Fake<IHttpPipelineExecutor>();
        A.CallTo(() => fakeInner.SendAsync(A<HttpRequestMessage>._, A<CancellationToken>._, null))
            .Throws(new TaskCanceledException());

        var registry = new InMemoryDeviceHealthRegistry();
        registry.RegisterDevice("10.0.0.5", BmsType.EmersonE2);

        var logger = A.Fake<ILogger<DeviceHttpExecutor>>();
        var wrapper = new DeviceHttpExecutor(fakeInner, registry, logger);

        var result = await wrapper.SendAsync("10.0.0.5", new HttpRequestMessage(), CancellationToken.None);

        Assert.False(result.Success);
        Assert.Equal(DeviceErrorType.Timeout, result.ErrorType);

        var snapshot = registry.GetSnapshot("10.0.0.5")!;
        Assert.Equal(1, snapshot.ConsecutiveFailures);
    }
}
