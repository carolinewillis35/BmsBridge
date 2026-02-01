public class RetryHandler : IRequestHandler
{
    private readonly IRequestHandler _next;
    private readonly int _maxRetries;
    private readonly TimeSpan _delay;

    public RetryHandler(IRequestHandler next, int maxRetries, TimeSpan delay)
    {
        _next = next;
        _maxRetries = maxRetries;
        _delay = delay;
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        int attempts = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                return await _next.SendAsync(request, cancellationToken);
            }
            catch when (attempts < _maxRetries)
            {
                attempts++;
                await Task.Delay(_delay, cancellationToken);
            }
        }
    }

    public void Dispose()
    {
        _next.Dispose();
    }
}
