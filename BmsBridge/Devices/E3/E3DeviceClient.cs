using System.Text.Json.Nodes;

public sealed class E3DeviceClient : BaseDeviceClient
{
    private bool _initialized;

    public override BmsType DeviceType => BmsType.EmersonE3;

    private JsonArray _polledData = new();

    // Oneshot objects
    private List<JsonObject> _networkDevices = new();
    private List<JsonObject> _appTypes = new();
    private List<JsonObject> _systemInventory = new();
    private List<string> _logGroups = new();
    private List<JsonObject> _loggedApps = new();
    private JsonObject? _systemInfo = new();

    // Polling objects
    private List<JsonObject> _alarms = new();

    public E3DeviceClient(
        Uri endpoint,
        IDeviceHttpExecutor pipelineExecutor,
        INormalizerService normalizerService,
        ILoggerFactory loggerFactory,
        IIotDevice iotDevice
    ) : base(
            endpoint: endpoint,
            pipelineExecutor: pipelineExecutor,
            normalizer: normalizerService,
            loggerFactory: loggerFactory,
            iotDevice: iotDevice
    )
    { }

    public override async Task InitializeAsync(CancellationToken ct = default)
    {
        _logger.LogInformation($"Initializing E3 device client at {_endpoint}");

        // _networkDevices = await GetNetworkSummaryAsync(ct);
        // _appTypes = await GetAppTypesAsync(ct);
        // _systemInventory = await GetSystemInventoryAsync(ct);
        // _systemInfo = await GetSystemInformationAsync(ct);
        _logGroups = await GetLogGroupsAsync(ct);
        _loggedApps = await GetLoggedAppsAsync(ct);

        // _networkDevices.ForEach(_polledData.Add);
        // _appTypes.ForEach(_polledData.Add);
        // _systemInventory.ForEach(_polledData.Add);
        // _polledData.Add(_systemInfo);

        _initialized = true;
    }

    public override async Task PollAsync(CancellationToken ct = default)
    {
        await EnsureInitialized();
        // await Task.Delay(60_000, ct);

        // _alarms = await GetAlarmsAsync(ct);

        // _alarms.ForEach(_polledData.Add);

        var diff = _dataWarehouse.ProcessIncoming(_polledData);
        await _iotDevice.SendMessageAsync(diff, ct);

        ClearPollingData();
    }

    // ------------------------------------------------------------
    // Initialization helpers
    // ------------------------------------------------------------

    private async Task<List<JsonObject>> GetNetworkSummaryAsync(CancellationToken ct = default)
    {
        var op = new E3GetNetworkSummaryOperation(_endpoint, _loggerFactory);
        return await NormalizeJsonArrayOp(op, "Controllers", ct);
    }

    private async Task<List<JsonObject>> GetAppTypesAsync(CancellationToken ct = default)
    {
        var op = new E3GetAppTypesOperation(_endpoint, _loggerFactory);
        return await NormalizeJsonArrayOp(op, "AppTypes", ct);
    }

    private async Task<List<JsonObject>> GetSystemInventoryAsync(CancellationToken ct = default)
    {
        var op = new E3GetSystemInventoryOperation(_endpoint, _loggerFactory);
        return await NormalizeJsonArrayOp(op, "SystemInventory", ct);
    }

    private async Task<List<string>> GetLogGroupsAsync(CancellationToken ct = default)
    {
        var op = new E3GetDefaultLogGroupOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        var outList = new List<string>();

        if (!result.Success || result.Data is null)
            return outList;

        var lgriids = result.Data![0]?.AsObject()?["lgriid"];

        if (lgriids is null)
            return outList;

        foreach (var iid in lgriids.AsArray())
        {
            var entry = iid?.ToString();
            if (entry is null)
                continue;
            outList.Add(entry);
        }

        return outList;
    }

    private async Task<List<JsonObject>> GetLoggedAppsAsync(CancellationToken ct = default)
    {
        var outList = new List<JsonObject>();

        foreach (var lgiid in _logGroups)
        {
            var op = new E3GetAppsForLogGroupOperation(_endpoint, lgiid, _loggerFactory);
            var result = op.ExecuteAsync(_pipelineExecutor, ct);
        }
        return outList;
    }

    private async Task<JsonObject?> GetSystemInformationAsync(CancellationToken ct = default)
    {
        var op = new E3GetSystemInformationOperation(_endpoint, _loggerFactory);
        var result = await NormalizeJsonArrayOp(op, "SystemInformation", ct);

        if (result.Count > 0)
            return result[0];

        return null;
    }

    // ------------------------------------------------------------
    // Polling helpers
    // ------------------------------------------------------------

    private async Task<List<JsonObject>> GetAlarmsAsync(CancellationToken ct = default)
    {
        var op = new E3GetAlarmsOperation(_endpoint, _loggerFactory);
        return await NormalizeJsonArrayOp(op, "Alarms", ct);
    }

    // ------------------------------------------------------------
    // Utility
    // ------------------------------------------------------------

    private async Task<List<JsonObject>> NormalizeJsonArrayOp(E3BaseDeviceOperation op, string addressString, CancellationToken ct = default)
    {
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        if (!result.Success || result.Data is null)
            return new List<JsonObject>();

        var outList = new List<JsonObject>();

        foreach (var entry in result.Data.AsArray())
        {
            if (entry is null)
                continue;

            var normData = _normalizer.Normalize(
                DeviceIp,
                DeviceType.ToString(),
                addressString,
                entry.AsObject()
            );

            outList.Add(normData);
        }

        return outList;
    }

    private async Task EnsureInitialized()
    {
        if (!_initialized)
            await InitializeAsync();
    }

    private void ClearPollingData()
    {
        _alarms = new();
        _polledData = new();
    }
}
