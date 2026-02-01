using MihaZupan;
using System.Runtime.InteropServices;

public static class Socks5HandlerFactory
{
    public static HttpMessageHandler Create(string proxyAddress)
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            throw new PlatformNotSupportedException(
                "SOCKS5 proxy is only supported on Linux development machines.");
        }

        var uri = new Uri(proxyAddress);

        var proxy = new HttpToSocks5Proxy(uri.Host, uri.Port);

        return new HttpClientHandler
        {
            Proxy = proxy,
            UseProxy = true
        };
    }
}
