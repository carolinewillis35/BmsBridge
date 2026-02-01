using System.Text.Json.Nodes;

public sealed class E2GetCellListOperation : E2BaseDeviceOperation
{
    public override string Name => "E2.GetCellList";

    public string ControllerName;

    public IReadOnlyList<E2CellListInfo>? Cells { get; private set; }

    public E2GetCellListOperation(Uri endpoint, string controllerName)
        : base(endpoint)
    {
        ControllerName = controllerName;
    }

    public sealed class E2CellListInfo
    {
        public string Controller { get; init; } = "";
        public string CellName { get; init; } = "";
        public string CellTypeName { get; init; } = "";
        public string CellLongName { get; init; } = "";
        public int CellType { get; init; }
    }

    protected override async Task ParseAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var json = JsonNode.Parse(await response.Content.ReadAsStringAsync(ct));

        var resultElement = json?["result"];
        var dataArray = resultElement?["data"] as JsonArray;

        if (dataArray is null)
        {
            Cells = Array.Empty<E2CellListInfo>();
            return;
        }

        var list = new List<E2CellListInfo>();

        foreach (var item in dataArray)
        {
            if (item is not JsonObject obj)
                continue;

            var info = new E2CellListInfo
            {
                Controller = obj["controller"]?.ToString() ?? "",
                CellName = obj["cellname"]?.ToString() ?? "",
                CellTypeName = obj["celltypename"]?.ToString() ?? "",
                CellLongName = obj["celllongname"]?.ToString() ?? "",
                CellType = obj["celltype"]?.GetValue<int>() ?? 0,
            };

            list.Add(info);
        }

        Cells = list;
        ExportObject = Cells;
    }

    public override async Task ExecuteAsync(IHttpPipelineExecutor executor, CancellationToken ct)
    {
        var parameters = new JsonArray { ControllerName };
        var request = BuildRequest(Name, parameters);
        var response = await executor.SendAsync(request, ct, Name);
        await ParseAsync(response, ct);
    }
}
