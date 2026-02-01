public interface IDeviceOperation
{
    string Name { get; }

    Task ExecuteAsync(IHttpPipelineExecutor executor, CancellationToken ct);
}
