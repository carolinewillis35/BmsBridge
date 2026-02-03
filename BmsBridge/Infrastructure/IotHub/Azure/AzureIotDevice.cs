using System.Text;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Client.Exceptions;
using System.Text.Json.Nodes;
using System.Text.Json;

public sealed class AzureIotDevice : IIotDevice, IAsyncDisposable
{
    private readonly ILogger<AzureIotDevice> _logger;
    private readonly DpsService _dpsService;
    private readonly SemaphoreSlim _connectionLock = new(1, 1);

    private DeviceClient? _deviceClient;

    public bool IsConnected { get; private set; }

    public AzureIotDevice(DpsService dpsService, ILogger<AzureIotDevice> logger)
    {
        _dpsService = dpsService;
        _logger = logger;
    }

    public async Task ConnectAsync(CancellationToken ct = default)
    {
        if (IsConnected)
        {
            return;
        }

        await _connectionLock.WaitAsync(ct);

        try
        {
            if (IsConnected)
            {
                return;
            }

            _deviceClient ??= await _dpsService.ProvisionDeviceAsync();

            _deviceClient.SetConnectionStatusChangesHandler(OnConnectionStatusChanged);

            await _deviceClient.OpenAsync(ct);
            _logger.LogInformation("Azure IoT device has been connected successfully.");
        }
        finally
        {
            _connectionLock.Release();
        }
    }

    public async Task SendMessageAsync(JsonObject payload, CancellationToken ct = default)
    {
        await ConnectAsync(ct);

        var payloadString = JsonSerializer.Serialize(payload);

        var message = new Message(Encoding.UTF8.GetBytes(payloadString));

        try
        {
            await _deviceClient!.SendEventAsync(message, ct);
            _logger.LogInformation("Message sent to Azure IotHub successfully");
        }
        catch (IotHubException ex)
        {
            _logger.LogError(ex, "Failed to send {payload} to Azure IotHub.", payload);
            IsConnected = false;
            throw;
        }
    }

    public async Task DisconnectAsync(CancellationToken ct = default)
    {
        await _connectionLock.WaitAsync(ct);
        try
        {
            if (_deviceClient is not null)
            {
                await _deviceClient.CloseAsync(ct);
                IsConnected = false;
            }
        }
        finally
        {
            _logger.LogInformation("Azure Iot Device has been disconnected successfully.");
            _connectionLock.Release();
        }
    }

    private void OnConnectionStatusChanged(ConnectionStatus status, ConnectionStatusChangeReason reason)
    {
        IsConnected = status == ConnectionStatus.Connected;
    }

    public async ValueTask DisposeAsync()
    {
        if (_deviceClient is not null)
        {
            _logger.LogDebug("Disposing of Azure IoT device.");
            await _deviceClient.DisposeAsync();
        }
    }
}
