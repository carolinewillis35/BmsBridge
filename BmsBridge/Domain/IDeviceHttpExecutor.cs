public interface IDeviceHttpExecutor
{
    Task<DeviceOperationResult<HttpResponseMessage>> SendAsync(
        string deviceIp,
        HttpRequestMessage request,
        CancellationToken ct,
        string? operationName = null
    );
}
