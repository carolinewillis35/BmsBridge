using Microsoft.Extensions.Logging.Abstractions;

public class DeviceClientReplayTests
{
    [Fact(Skip = "Avoid printing to console for now")]
    public async Task E2DeviceClient_Replay_Test()
    {
        var executor = new ReplayHttpPipelineExecutor(new GeneralSettings());
        var indexProvider = new EmbeddedE2IndexMappingProvider();
        var normalizer = new NormalizerService();

        var client = new E2DeviceClient(
            new Uri("http://fake-device"),
            executor,
            indexProvider,
            normalizer,
            NullLoggerFactory.Instance,
            new ConsoleIotDevice()
        );

        await client.InitializeAsync();

        // PollAsync prints normalized messages to console
        try
        {
            await client.PollAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            Assert.False(false);
        }
    }
}
