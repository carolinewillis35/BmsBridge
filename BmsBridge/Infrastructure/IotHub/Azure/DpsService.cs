using Microsoft.Extensions.Options;
using Microsoft.Azure.Devices.Client;
using Microsoft.Azure.Devices.Provisioning.Client;
using Microsoft.Azure.Devices.Provisioning.Client.Transport;
using Microsoft.Azure.Devices.Shared;
using System.Security.Cryptography;
using System.Text;

public class DpsService
{
    private readonly AzureSettings _options;
    private readonly KeyvaultService _keyvaultService;
    private readonly ILogger<DpsService> _logger;
    private DeviceClient? _deviceClient;
    private string? _groupKey;
    private string? _hostName;
    private string? _deviceId;
    private ProvisioningDeviceClient? _dpsClient;

    public DpsService(IOptions<AzureSettings> optionsAccessor, KeyvaultService keyvaultService, ILogger<DpsService> logger)
    {
        _options = optionsAccessor.Value;
        _keyvaultService = keyvaultService;
        _logger = logger;
    }

    public async Task<DeviceClient> ProvisionDeviceAsync()
    {
        if (_deviceClient != null)
        {
            return _deviceClient;
        }

        _logger.LogInformation("Provisioning Azure IoT device.");
        if (string.IsNullOrEmpty(_groupKey))
        {
            _groupKey = await _keyvaultService.GetDpsEnrollmentGroupKeyAsync();
        }

        var keyBytes = Convert.FromBase64String(_groupKey);
        using var hmac = new HMACSHA256(keyBytes);
        var deviceKeyBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(_options.device_id));
        var deviceKey = Convert.ToBase64String(deviceKeyBytes);

        var security = new SecurityProviderSymmetricKey(
            registrationId: _options.device_id,
            primaryKey: deviceKey,
            secondaryKey: null
        );

        var transport = new ProvisioningTransportHandlerMqtt();

        _dpsClient = ProvisioningDeviceClient.Create(
            globalDeviceEndpoint: "global.azure-devices-provisioning.net",
            idScope: _options.scope_id,
            securityProvider: security,
            transport: transport
        );

        var registrationResult = await _dpsClient.RegisterAsync();

        _logger.LogInformation("Azure DPS registration result: {result}.", registrationResult.Status);

        if (registrationResult.Status != ProvisioningRegistrationStatusType.Assigned)
        {
            throw new Exception($"DPS registration failed: {registrationResult.Status}");
        }

        _hostName = registrationResult.AssignedHub;
        _deviceId = registrationResult.DeviceId;

        var auth = new DeviceAuthenticationWithRegistrySymmetricKey(
            _deviceId,
            deviceKey
        );

        _deviceClient = DeviceClient.Create(_hostName, auth, TransportType.Mqtt);

        _logger.LogInformation("Azure IoT device has been successfully provisioned in IoTHub.");
        return _deviceClient;
    }
}
