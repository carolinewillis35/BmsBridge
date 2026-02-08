public sealed class E3DeviceRunner : BaseDeviceRunner
{
    public E3DeviceRunner(
        Uri endpoint,
        IDeviceHttpExecutor pipelineExecutor,
        IE2IndexMappingProvider indexProvider,
        INormalizerService normalizerService,
        ILoggerFactory loggerFactory,
        IIotDevice iotDevice
    ) : base(endpoint,
        pipelineExecutor,
        indexProvider,
        normalizerService,
        loggerFactory,
        iotDevice
    )
    { }

    protected override IDeviceClient CreateClient()
    {
        return new E3DeviceClient(
            _endpoint,
            _pipelineExecutor,
            _normalizer,
            _loggerFactory,
            _iotDevice
        );
    }
}
