public class TimeoutHandler : IRequestHandler
{
    private readonly IRequestHandler _next;
    private readonly TimeSpan _timeout;

    public TimeoutHandler(IRequestHandler next, TimeSpan timeout)
    {
        _next = next;
        _timeout = timeout;
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken = default)
    {
        using var timeoutCts = new CancellationTokenSource(_timeout);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            timeoutCts.Token
        );

        return await _next.SendAsync(request, linkedCts.Token);
    }

    public void Dispose()
    {
        _next.Dispose();
    }
}
