using System.Text;

public sealed class ReplayHttpPipelineExecutor : IHttpPipelineExecutor
{
    public ReplayHttpPipelineExecutor(GeneralSettings settings)
    {
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken ct,
        string? Name = null)
    {
        var filePath = $"/home/henry/Projects/BmsBridge/BmsBridge/ReplayData/{Name}.txt";
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Replay file not found: {filePath}");

        var raw = await File.ReadAllTextAsync(filePath, ct);

        return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent(raw, Encoding.UTF8, "text/plain")
        };
    }
}
