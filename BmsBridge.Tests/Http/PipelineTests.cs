using System.Diagnostics;

public class PipelineTests

{
    [Fact(Skip = "Handled by minimum delay test.")]
    public async Task Pipeline_CanSendGetRequest()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

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
        var response = await pipeline.SendAsync(request, cts.Token);

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
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
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
        await pipeline.SendAsync(request, cts.Token);

        var stopwatch = Stopwatch.StartNew();


        var secondRequest = new HttpRequestMessage(
            HttpMethod.Get,
            "https://httpbin.org/get"
        );

        // Act: send a second request immediately
        await pipeline.SendAsync(secondRequest, cts.Token);

        stopwatch.Stop();

        // Assert: elapsed time should be >= throttleDelay
        Assert.True(
            stopwatch.Elapsed >= throttleDelay,
            $"Expected at least {throttleDelay.TotalMilliseconds}ms delay, but got {stopwatch.Elapsed.TotalMilliseconds}ms"
        );

        Console.WriteLine($"Elapsed: {stopwatch.Elapsed.TotalMilliseconds}ms");
    }

    [Fact(Skip = "Verified to work, skipping for now.")]
    public async Task Pipeline_CanUseProxy()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
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
        var response = await pipeline.SendAsync(request, cts.Token);

        // Assert
        Assert.True(response.IsSuccessStatusCode);

        var content = await response.Content.ReadAsStringAsync();

        Assert.Contains("\"url\": \"https://httpbin.org/get\"", content);
    }

    [Fact]
    public async Task RetryHandler_RetriesOnFailure()
    {
        var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
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
            await pipeline.SendAsync(request, cts.Token);
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

    [Fact]
    public async Task TimeoutHandler_CancelsRequest_WhenTimeoutExceeded()
    {
        // Arrange: very short timeout so the test is fast
        var timeout = TimeSpan.FromMilliseconds(300);

        var pipeline = HttpPipelineFactory.Create(
            throttleDelay: TimeSpan.FromMilliseconds(10),
            enableRetries: false,
            retryCount: 0,
            timeout: timeout,
            socks5Proxy: null
        );

        // httpbin.org/delay/5 waits 5 seconds before responding
        var request = new HttpRequestMessage(
            HttpMethod.Get,
            "https://httpbin.org/delay/5"
        );

        // Act + Assert
        var sw = Stopwatch.StartNew();

        await Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await pipeline.SendAsync(request, CancellationToken.None);
        });

        sw.Stop();

        // Ensure timeout happened *before* the server responded
        Assert.True(
            sw.Elapsed < TimeSpan.FromSeconds(2),
            $"Timeout should have occurred quickly, but elapsed was {sw.Elapsed.TotalMilliseconds}ms"
        );

        Console.WriteLine($"Timeout triggered after {sw.Elapsed.TotalMilliseconds}ms");
    }

    [Fact]
    public void Pipeline_CanBeDisposed()
    {
        var pipeline = HttpPipelineFactory.Create(
            throttleDelay: TimeSpan.FromMilliseconds(10),
            enableRetries: false,
            retryCount: 0,
            socks5Proxy: null
        );

        try
        {
            pipeline.Dispose();
            Assert.True(true);
        }
        catch
        {
            Assert.True(false);
        }
    }
}
