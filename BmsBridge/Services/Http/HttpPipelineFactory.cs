public static class HttpPipelineFactory
{
    public static IRequestHandler Create(
        TimeSpan throttleDelay,
        bool enableRetries = false,
        int retryCount = 3,
        TimeSpan? timeout = null,
        string? socks5Proxy = null)
    {
        IRequestHandler handler = new HttpClientHandlerWrapper(socks5Proxy);

        handler = new ThrottleHandler(handler, throttleDelay);

        if (enableRetries)
            handler = new RetryHandler(handler, retryCount, TimeSpan.FromMilliseconds(300));

        if (timeout != null)
            handler = new TimeoutHandler(handler, timeout.Value);

        return handler;
    }
}
