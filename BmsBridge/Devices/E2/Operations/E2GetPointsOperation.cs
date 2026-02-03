using System.Text.Json;
using System.Text.Json.Nodes;

public sealed class E2GetPointsOperation : E2BaseDeviceOperation
{
    public override string Name => "E2.GetMultiExpandedStatus";

    public string ControllerName { get; }
    public string CellName { get; }

    public IReadOnlyList<(int Index, string PointName)> PointsToQuery { get; }

    public Dictionary<string, JsonObject>? Points { get; private set; }

    public E2GetPointsOperation(
        Uri endpoint,
        string controllerName,
        string cellName,
        IReadOnlyList<(int Index, string PointName)> pointsToQuery,
        ILoggerFactory loggerFactory)
        : base(endpoint, loggerFactory)
    {
        ControllerName = controllerName;
        CellName = cellName;
        PointsToQuery = pointsToQuery;
    }

    public override async Task ExecuteAsync(IHttpPipelineExecutor executor, CancellationToken ct)
    {
        var paramArray = new JsonArray();

        foreach (var (index, _) in PointsToQuery)
        {
            var key = $"{ControllerName}:{CellName}:{index}";
            paramArray.Add(key);
        }

        var parameters = new JsonArray { paramArray };

        var request = BuildRequest(Name, parameters);
        _logger.LogInformation($"Sending {Name} to {Endpoint}");
        var response = await executor.SendAsync(request, ct, Name);
        await ParseAsync(response, ct);
    }

    protected override async Task ParseAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var json = JsonNode.Parse(await response.Content.ReadAsStringAsync(ct));
        var dataArray = json?["result"]?["data"] as JsonArray;

        var dict = new Dictionary<string, JsonObject>();

        if (dataArray is not null)
        {
            for (int i = 0; i < dataArray.Count && i < PointsToQuery.Count; i++)
            {
                if (dataArray[i] is JsonObject obj)
                {
                    var (_, pointName) = PointsToQuery[i];
                    dict[pointName] = obj;
                }
            }
        }

        Points = dict;

        ExportObject = new JsonObject
        {
            ["CellName"] = CellName,
            ["Points"] = JsonNode.Parse(JsonSerializer.Serialize(dict))
        };
    }
}
