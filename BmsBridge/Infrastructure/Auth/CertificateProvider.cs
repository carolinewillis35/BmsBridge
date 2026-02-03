using System.Security.Cryptography.X509Certificates;

public sealed class CertificateProvider : ICertificateProvider
{
    private readonly IReadOnlyList<ICertificateSource> _sources;

    public CertificateProvider(IEnumerable<ICertificateSource> sources)
    {
        _sources = sources.ToList();
    }

    public X509Certificate2 GetCertificate()
    {
        foreach (var source in _sources)
        {
            try
            {
                var cert = source.Load();
                if (cert != null)
                    return cert;
            }
            finally
            {

            }
        }

        throw new InvalidOperationException("No valid certificate found.");
    }
}
