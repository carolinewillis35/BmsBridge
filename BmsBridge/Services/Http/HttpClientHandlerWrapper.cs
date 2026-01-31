public class HttpClientHandlerWrapper : IRequestHandler, IDisposable
{
    private readonly HttpClient _client;
    private readonly HttpMessageHandler _handler;

    public HttpClientHandlerWrapper(string? socks5Proxy = null)
    {
        _handler = string.IsNullOrWhiteSpace(socks5Proxy)
            ? new HttpClientHandler()
            : Socks5HandlerFactory.Create(socks5Proxy);

        _client = new HttpClient(_handler);
    }

    public Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        return _client.SendAsync(request, cancellationToken);
    }

    public void Dispose()
    {
        _client.Dispose();
        _handler.Dispose();
    }
}
