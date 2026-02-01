using System.Text.Json.Nodes;
using System.Text.Json;

public class E2OperationTests
{
    private IHttpPipelineExecutor executor { get; init; } = new ReplayHttpPipelineExecutor(new GeneralSettings());
    private Uri endpoint { get; init; } = new Uri("http://fake.uri");
    private string replayDir { get; init; } = "/home/henry/Projects/BmsBridge/BmsBridge/ReplayData/";

    private async Task<T> RunOperationAsync<T>(string name, Func<T> factory)
        where T : BaseDeviceOperation
    {
        var replayFile = Path.Combine(replayDir, $"{name}.txt");

        Assert.True(File.Exists(replayFile),
            $"Replay file missing: {replayFile}. " +
            "Run the real controller capture first.");

        var op = factory();

        await op.ExecuteAsync(executor, CancellationToken.None);

        var json = op.ToJson();
        Assert.NotNull(json);

        // Pretty-print the JSON for debugging
        // try
        // {
        //     // var parsed = JsonNode.Parse(json);
        //     var pretty = json?.ToJsonString(new JsonSerializerOptions
        //     {
        //         WriteIndented = true
        //     });
        //
        //     Console.WriteLine($"=== {name} Replay Output ===");
        //     Console.WriteLine(pretty);
        //     Console.WriteLine("============================");
        // }
        // catch
        // {
        //     Console.WriteLine($"Raw JSON for {name}: {json}");
        // }

        return op;
    }

    [Fact]
    public async Task E2GetControllerList_Works()
    {
        await RunOperationAsync(
            "E2.GetControllerList",
            () => new E2GetControllerListOperation(endpoint)
        );
    }

    [Fact]
    public async Task E2GetCellList_Works()
    {
        await RunOperationAsync(
            "E2.GetCellList",
            () => new E2GetCellListOperation(endpoint, "fake")
        );
    }

    [Fact]
    public async Task E2GetAlarmList_Works()
    {
        await RunOperationAsync(
            "E2.GetAlarmList",
            () => new E2GetAlarmListOperation(endpoint, "fake")
        );
    }
}
