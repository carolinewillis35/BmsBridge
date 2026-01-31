public static class HttpPipelineFactory
{
    public static IRequestHandler Create(
        TimeSpan throttleDelay,
        bool enableRetries = false,
        int retryCount = 3,
        string? socks5Proxy = null)
    {
        IRequestHandler handler = new HttpClientHandlerWrapper(socks5Proxy);

        handler = new ThrottleHandler(handler, throttleDelay);

        if (enableRetries)
            handler = new RetryHandler(handler, retryCount);

        return handler;
    }
}
