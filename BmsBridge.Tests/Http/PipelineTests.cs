using System.Diagnostics;

public class PipelineTests

{
    [Fact(Skip = "Handled by minimum delay test.")]
    public async Task Pipeline_CanSendGetRequest()
    {
        // Arrange: build a pipeline with no proxy and no retries
        var pipeline = HttpPipelineFactory.Create(
            throttleDelay: TimeSpan.FromMilliseconds(100),
            enableRetries: false,
            socks5Proxy: null
        );

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "https://httpbin.org/get"
        );

        // Act
        var response = await pipeline.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        var content = await response.Content.ReadAsStringAsync();

        // Optional: write to test output or console
        Console.WriteLine("Response content:");
        Console.WriteLine(content);

        Assert.Contains("\"url\": \"https://httpbin.org/get\"", content);
    }

    [Fact]
    public async Task ThrottleHandler_EnforcesMinimumDelayBetweenRequests()
    {
        // Arrange
        var throttleDelay = TimeSpan.FromMilliseconds(1000);

        var pipeline = HttpPipelineFactory.Create(
            throttleDelay: throttleDelay,
            enableRetries: false,
            socks5Proxy: null
        );

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "https://httpbin.org/get"
        );

        // Warm-up request (sets lastRequest timestamp)
        await pipeline.SendAsync(request);

        var stopwatch = Stopwatch.StartNew();


        var secondRequest = new HttpRequestMessage(
            HttpMethod.Get,
            "https://httpbin.org/get"
        );

        // Act: send a second request immediately
        await pipeline.SendAsync(secondRequest);

        stopwatch.Stop();

        // Assert: elapsed time should be >= throttleDelay
        Assert.True(
            stopwatch.Elapsed >= throttleDelay,
            $"Expected at least {throttleDelay.TotalMilliseconds}ms delay, but got {stopwatch.Elapsed.TotalMilliseconds}ms"
        );

        Console.WriteLine($"Elapsed: {stopwatch.Elapsed.TotalMilliseconds}ms");
    }

    [Fact]
    public async Task Pipeline_CanUseProxy()
    {
        if (!System.IO.Directory.Exists("/proc")) // crude Linux check
            return;

        // Arrange: build a pipeline with no proxy and no retries
        var pipeline = HttpPipelineFactory.Create(
            throttleDelay: TimeSpan.FromMilliseconds(100),
            enableRetries: false,
            socks5Proxy: "socks5://localhost:1080"
        );

        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "https://httpbin.org/get"
        );

        // Act
        var response = await pipeline.SendAsync(request);

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"url\": \"https://httpbin.org/get\"", content);
    }

    [Fact]
    public async Task RetryHandler_RetriesOnFailure()
    {
        var retryDelay = TimeSpan.FromMilliseconds(300);
        var retryCount = 3;

        var pipeline = HttpPipelineFactory.Create(
            throttleDelay: TimeSpan.FromMilliseconds(10),
            enableRetries: true,
            retryCount: retryCount,
            socks5Proxy: null
        );

        // This domain will never resolve, forcing retries
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "http://nonexistent.bmsbridge.test"
        );

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await pipeline.SendAsync(request);
        }
        catch
        {
            // Expected — we only care about timing
        }

        stopwatch.Stop();

        var expectedMinimum = retryDelay * retryCount;

        Assert.True(
            stopwatch.Elapsed >= expectedMinimum,
            $"Expected at least {expectedMinimum.TotalMilliseconds}ms due to retries, but got {stopwatch.Elapsed.TotalMilliseconds}ms"
        );

        Console.WriteLine($"Elapsed: {stopwatch.Elapsed.TotalMilliseconds}ms");
    }
}
