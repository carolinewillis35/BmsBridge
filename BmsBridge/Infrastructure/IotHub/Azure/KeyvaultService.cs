using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;

public class KeyvaultService
{
    private readonly AzureSettings _options;
    private readonly string _keyvaultUrl;
    private readonly ILogger<KeyvaultService> _logger;
    private readonly ClientCertificateCredential _clientCertificateCredential;
    private readonly SecretClient _secretClient;
    private string? _dpsEnrollmentGroupKey;

    public KeyvaultService(IOptions<AzureSettings> optionsAccessor, CertificateProvider certificateProvider, ILogger<KeyvaultService> logger)
    {
        _options = optionsAccessor.Value;
        _keyvaultUrl = $"https://{_options.vault_name}.vault.azure.net/";
        _logger = logger;
        _logger.LogDebug("Creating SecretClient instance for Azure KeyVault.");
        _clientCertificateCredential = new ClientCertificateCredential(
            _options.tenant_id,
            _options.client_id,
            certificateProvider.GetCertificate()
        );
        _secretClient = new SecretClient(
            new Uri(_keyvaultUrl),
            _clientCertificateCredential
        );
    }

    public async Task<string> GetDpsEnrollmentGroupKeyAsync()
    {
        if (string.IsNullOrEmpty(_dpsEnrollmentGroupKey))
        {
            _logger.LogInformation("Retrieving Azure KeyVault secret for the DPS enrollment group key.");
            KeyVaultSecret secret = await _secretClient.GetSecretAsync(_options.secret_name);
            _dpsEnrollmentGroupKey = secret.Value;
            _logger.LogInformation("Retrieved Azure Keyvault secret for the DPS enrollment group key successfully.");
        }
        return _dpsEnrollmentGroupKey;
    }
}
