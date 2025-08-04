using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace LegacyServices.TcpMultiplex;

public class Options : IValidateable
{
    public bool Enabled { get; set; }
    public bool StartTls { get; set; }
    public bool Help { get; set; }
    public CertificateOption? Certificate { get; set; }
    public ServiceOption[]? Services { get; set; }

    public ServiceOption? GetService(string name)
    {
        return Services?.FirstOrDefault(m => name.EqualsCI(m.Name));
    }

    [MemberNotNull(nameof(Services))]
    public void Validate()
    {
        Certificate?.Validate();
        if (Services == null || Services.Length == 0)
        {
            throw new ValidationException("At least one service must be defined");
        }
        if (Services.Any(m => "HELP".EqualsCI(m.Name)))
        {
            throw new ValidationException("'HELP' is a reserved name and cannot be used as a service");
        }
        if (StartTls && Services.Any(m => "STARTTLS".EqualsCI(m.Name)))
        {
            throw new ValidationException("'STARTTLS' is a reserved name and cannot be used as a service when TLS is enabled");
        }
    }
}

public class CertificateOption : IValidateable
{
    public string? Public { get; set; }
    public string? Private { get; set; }

    public void Validate()
    {
        int nulls = Public == null ? 1 : 0;
        nulls += Private == null ? 1 : 0;

        if (nulls == 1)
        {
            throw new ValidationException("Cannot specify only public or private part of a certificate alone");
        }
        if (nulls == 2)
        {
            if (!File.Exists(Public))
            {
                throw new ValidationException($"Certificate '{Public}' cannot be found");
            }
            if (!File.Exists(Private))
            {
                throw new ValidationException($"Key '{Private}' cannot be found");
            }
            byte[] pub, priv;

            try
            {
                pub = File.ReadAllBytes(Public);
            }
            catch (Exception ex)
            {
                throw new ValidationException($"Certificate '{Public}' cannot be read", ex);
            }
            try
            {
                priv = File.ReadAllBytes(Private);
            }
            catch (Exception ex)
            {
                throw new ValidationException($"Key '{Private}' cannot be read", ex);
            }
            try
            {
                Tools.CreateCertificate(pub, priv).Dispose();
            }
            catch (Exception ex)
            {
                throw new ValidationException($"Failed to create certificate from the provided public and private parts. {ex.Message}", ex);
            }
        }
    }
}

public class ServiceOption : IValidateable
{
    public string? Name { get; set; }
    public IPEndPoint? Endpoint { get; set; }
    public bool Public { get; set; }

    [MemberNotNull(nameof(Name), nameof(Endpoint))]
    public void Validate()
    {
        ValidationException.ThrowIfNull(Name);
        ValidationException.ThrowIfNull(Endpoint);
    }
}
