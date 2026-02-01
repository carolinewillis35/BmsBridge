public interface IHttpPipelineExecutor
{
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct, string? Name = null);
}
