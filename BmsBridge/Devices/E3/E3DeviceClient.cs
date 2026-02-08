using System.Text.Json.Nodes;

public sealed class E3DeviceClient : BaseDeviceClient
{
    private bool _initialized;

    public override BmsType DeviceType => BmsType.EmersonE3;

    private JsonArray _polledData = new();

    // Oneshot objects
    private List<JsonObject> _systemInventory = new();
    private List<JsonObject> _appDesc = new();
    private string? _sessionID;

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

        _sessionID = await GetSessionIDAsync(ct);
        _systemInventory = await GetSystemInventoryAsync(ct);
        _systemInventory.ForEach(_polledData.Add);

    }

    public override async Task PollAsync(CancellationToken ct = default)
    {
        await ClearPollingData(ct);

        _appDesc = await GetAppDescriptionsAsync(ct);
        _alarms = await GetAlarmsAsync(ct);

        _alarms.ForEach(_polledData.Add);
        _appDesc.ForEach(_polledData.Add);

        var diff = _dataWarehouse.ProcessIncoming(_polledData);
        await _iotDevice.SendMessageAsync(diff, ct);

        _initialized = true;
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

    private async Task<List<JsonObject>> GetAppDescriptionsAsync(CancellationToken ct = default)
    {
        var outList = new List<JsonObject>();

        foreach (var entry in _systemInventory)
        {
            var iid = entry?["data"]?["iid"]?.GetValue<string>();
            var appname = entry?["data"]?["appname"]?.GetValue<string>();

            if (string.IsNullOrEmpty(iid) || string.IsNullOrEmpty(appname))
                continue;

            var op = new E3GetAppDescriptionOperation(_endpoint, _sessionID!, iid, _loggerFactory);
            var dataResult = await NormalizeJsonArrayOp(op, appname, ct);

            foreach (var result in dataResult)
            {
                var trimmedData = new JsonObject
                {
                    ["device_key"] = result["device_key"]?.DeepClone(),
                    ["ip"] = result["ip"]?.DeepClone()
                };

                if (result["data"] is JsonObject dataObj)
                {
                    var newData = new JsonObject();

                    if (dataObj.TryGetPropertyValue("name", out var name))
                        newData["name"] = name!.DeepClone();

                    if (dataObj.TryGetPropertyValue("val", out var val))
                        newData["val"] = val!.DeepClone();

                    trimmedData["data"] = newData;
                }

                outList.Add(trimmedData);
            }
        }
        return outList;
    }

    private async Task<List<JsonObject>> GetSystemInventoryAsync(CancellationToken ct = default)
    {
        var op = new E3GetSystemInventoryOperation(_endpoint, _sessionID!, _loggerFactory);
        return await NormalizeJsonArrayOp(op, "SystemInventory", ct);
    }

    private async Task<List<JsonObject>> GetGroupsAsync(CancellationToken ct = default)
    {
        var op = new E3GetGroupsOperation(_endpoint, _sessionID!, _loggerFactory);
        return await NormalizeJsonArrayOp(op, "Groups", ct);
    }

    private async Task<List<string>> GetLogGroupsAsync(CancellationToken ct = default)
    {
        var op = new E3GetDefaultLogGroupOperation(_endpoint, _sessionID!, _loggerFactory);
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

    private async Task<string> GetSessionIDAsync(CancellationToken ct = default)
    {
        var op = new E3GetSessionIDOperation(_endpoint, _loggerFactory);
        var result = await op.ExecuteAsync(_pipelineExecutor, ct);

        if (!result.Success || result.Data is null)
            return string.Empty;

        var sessionID = result.Data[0]?.AsObject()?["sid"]?.GetValue<string>();

        var sid = sessionID ?? string.Empty;

        if (!string.IsNullOrEmpty(sid))
        {
            var logOp = new E3LoginOperation(_endpoint, sid, _loggerFactory);
            await logOp.ExecuteAsync(_pipelineExecutor, ct);
        }

        return sid;
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

    private async Task ClearPollingData(CancellationToken ct = default)
    {
        if (!_initialized)
            return;

        _alarms = new();
        _polledData = new();
        await GetSessionIDAsync(ct);
    }
}
