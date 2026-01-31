public class ThrottleHandler : IRequestHandler
{
    private readonly IRequestHandler _next;
    private readonly TimeSpan _delay;
    private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
    private DateTime _lastRequest = DateTime.MinValue;

    public ThrottleHandler(IRequestHandler next, TimeSpan delay)
    {
        _next = next;
        _delay = delay;
    }

    public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        await _lock.WaitAsync();
        try
        {
            var now = DateTime.UtcNow;
            var elapsed = now - _lastRequest;

            if (elapsed < _delay)
                await Task.Delay(_delay - elapsed);

            var response = await _next.SendAsync(request, ct);
            _lastRequest = DateTime.UtcNow;
            return response;
        }
        finally
        {
            _lock.Release();
        }
    }
}
