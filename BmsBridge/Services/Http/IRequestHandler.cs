public interface IRequestHandler
{
    Task<HttpResponseMessage> SendAsync(HttpRequestMessage request);
}

