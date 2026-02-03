using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;

public sealed class StoreCertificateSource : ICertificateSource
{
    private readonly string _subjectName;

    public StoreCertificateSource(IOptions<AzureSettings> azureSettings)
    {
        _subjectName = azureSettings.Value.certificate_subject;
    }

    public X509Certificate2? Load()
    {
        using var store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
        store.Open(OpenFlags.ReadOnly);

        var certs = store.Certificates
            .Find(X509FindType.FindBySubjectName, _subjectName, validOnly: false);

        return certs
            .Cast<X509Certificate2>()
            .FirstOrDefault(CertificateValidator.IsValid);
    }
}
