using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;

public sealed class StoreCertificateSource : ICertificateSource
{
    private readonly string _subjectName;
    private readonly ILogger<StoreCertificateSource> _logger;

    public StoreCertificateSource(
        IOptions<AzureSettings> azureSettings,
        ILogger<StoreCertificateSource> logger)
    {
        _subjectName = azureSettings.Value.certificate_subject;
        _logger = logger;

        _logger.LogInformation("Certificate subject configured as: {Subject}", _subjectName);
    }

    public X509Certificate2? Load()
    {
        _logger.LogInformation("Opening CurrentUser My certificate store.");

        using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
        store.Open(OpenFlags.ReadOnly);

        _logger.LogInformation("Store opened. Total certificates found: {Count}", store.Certificates.Count);

        foreach (var cert in store.Certificates)
        {
            _logger.LogInformation("Found certificate: {Subject}", cert.Subject);
        }

        var certs = store.Certificates
            .Find(X509FindType.FindBySubjectName, _subjectName, validOnly: false);

        _logger.LogInformation("Matching certificates found: {Count}", certs.Count);

        var selected = certs
            .Cast<X509Certificate2>()
            .FirstOrDefault();

        if (selected == null)
        {
            _logger.LogWarning("No matching certificate found.");
        }
        else
        {
            _logger.LogInformation("Selected certificate: {Subject}", selected.Subject);
            _logger.LogInformation("Has private key: {HasPrivateKey}", selected.HasPrivateKey);
        }

        return selected;
    }
}
