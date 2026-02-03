public abstract class BaseDeviceClient : IDeviceClient
{
    protected readonly Uri _endpoint;
    protected readonly IHttpPipelineExecutor _executor;
    protected readonly ILogger _logger;
    protected readonly INormalizerService _normalizer;
    protected readonly ILoggerFactory _loggerFactory;

    public abstract string DeviceType { get; }

    public string DeviceIp => _endpoint.Host;

    protected BaseDeviceClient(
        Uri endpoint,
        ILoggerFactory loggerFactory,
        IHttpPipelineExecutor executor,
        INormalizerService normalizer)
    {
        _endpoint = endpoint;
        _logger = loggerFactory.CreateLogger(GetType());
        _executor = executor;
        _normalizer = normalizer;
        _loggerFactory = loggerFactory;
    }

    public abstract Task InitializeAsync(CancellationToken ct = default);
    public abstract Task PollAsync(CancellationToken ct = default);
}
