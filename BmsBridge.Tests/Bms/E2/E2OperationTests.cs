public class E2OperationTests
{
    private IHttpPipelineExecutor executor { get; init; } = new ReplayHttpPipelineExecutor(new GeneralSettings());
    private Uri endpoint { get; init; } = new Uri("http://fake.uri");
    private string replayDir { get; init; } = "/home/henry/Projects/BmsBridge/BmsBridge/ReplayData/";

    private async Task<T> RunOperationAsync<T>(string name)
        where T : BaseDeviceOperation
    {
        var replayFile = Path.Combine(replayDir, $"{name}.txt");

        Assert.True(File.Exists(replayFile),
            $"Replay file missing: {replayFile}. " +
            "Run the real controller capture first.");

        // Create the operation using reflection
        var op = (T)Activator.CreateInstance(typeof(T), endpoint)!;

        await op.ExecuteAsync(executor, CancellationToken.None);

        Assert.NotNull(op.ToJson());

        return op;
    }

    [Fact]
    public async Task E2GetControllerList_Works()
    {
        await RunOperationAsync<E2GetControllerListOperation>("E2.GetControllerList");
    }
}
