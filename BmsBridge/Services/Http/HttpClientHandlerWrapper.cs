public class HttpClientHandlerWrapper : IRequestHandler
{
    private readonly HttpClient _client;

    public HttpClientHandlerWrapper(string? socks5Proxy = null)
    {
        HttpMessageHandler handler;

        if (!string.IsNullOrWhiteSpace(socks5Proxy))
        {
            handler = Socks5HandlerFactory.Create(socks5Proxy);
        }
        else
        {
            handler = new HttpClientHandler();
        }

        _client = new HttpClient(handler);
    }

    public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        => _client.SendAsync(request);
}
