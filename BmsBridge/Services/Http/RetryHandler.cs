public class RetryHandler : IRequestHandler
{
    private readonly IRequestHandler _next;
    private readonly int _maxRetries;
    private readonly TimeSpan _delay;

    public RetryHandler(IRequestHandler next, int maxRetries = 3, TimeSpan? retryDelay = null)
    {
        _next = next;
        _maxRetries = maxRetries;
        _delay = retryDelay ?? TimeSpan.FromSeconds(1);
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        int attempts = 0;

        while (true)
        {
            try
            {
                return await _next.SendAsync(request);
            }
            catch when (attempts < _maxRetries)
            {
                attempts++;
                await Task.Delay(_delay);
            }
        }
    }
}
