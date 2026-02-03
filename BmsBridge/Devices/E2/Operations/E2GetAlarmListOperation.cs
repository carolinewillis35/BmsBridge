using System.Text.Json.Nodes;

public sealed class E2GetAlarmListOperation : E2BaseDeviceOperation
{
    public override string Name => "E2.GetAlarmList";

    public string ControllerName;

    public IReadOnlyList<JsonObject>? Alarms { get; set; }

    public E2GetAlarmListOperation(Uri endpoint, string controllerName, ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
        ControllerName = controllerName;
    }

    protected override async Task ParseAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var json = JsonNode.Parse(await response.Content.ReadAsStringAsync(ct));

        var resultElement = json?["result"];
        var dataArray = resultElement?["data"] as JsonArray;

        if (dataArray is null)
        {
            Alarms = Array.Empty<JsonObject>();
            return;
        }

        var list = new List<JsonObject>();

        foreach (var item in dataArray)
        {
            if (item is JsonObject obj)
                list.Add(obj);
        }

        Alarms = list;
        ExportObject = Alarms;
    }

    public override async Task ExecuteAsync(IHttpPipelineExecutor executor, CancellationToken ct)
    {
        var parameters = new JsonArray { ControllerName };
        var request = BuildRequest(Name, parameters);
        _logger.LogInformation($"Sending {Name} to {Endpoint}");
        var response = await executor.SendAsync(request, ct, Name);
        await ParseAsync(response, ct);
    }
}
