using System.Text.Json.Nodes;

public class VoidIotDevice : IIotDevice
{
    public bool IsConnected { get; init; } = true;

    public Task ConnectAsync(CancellationToken ct = default)
    {
        // No-op for dummy device
        return Task.CompletedTask;
    }

    public Task SendMessageAsync(JsonNode payload, CancellationToken ct = default)
    {
        return Task.CompletedTask;
    }

    public Task DisconnectAsync(CancellationToken ct = default)
    {
        // No-op for dummy device
        return Task.CompletedTask;
    }
}
