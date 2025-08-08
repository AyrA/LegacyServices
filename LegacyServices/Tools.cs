using LegacyServices.Json;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

namespace LegacyServices;

internal static class Tools
{
    public const string CRLF = "\r\n";
    private static readonly UTF8Encoding u8 = new(false, true);
    private static readonly JsonSerializerOptions opt;
    private static readonly string privateKey;
    private static readonly string publicCert;

    static Tools()
    {
        opt = new(JsonSerializerDefaults.General)
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
        opt.Converters.Add(new IPEndPointConverter());
        opt.Converters.Add(new ScientificNotationConverter());
        opt.MakeReadOnly(true);

        var segments = new string[]
        {
            "CN=Self signed service certificate",
            "OU=LegacyServices",
            "O=https://github.com/AyrA/LegacyServices"
        };
        using var key = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        var dName = new X500DistinguishedName(string.Join("\r\n", segments),
            //Using line breaks here means we don't have to escape other special characters
            X500DistinguishedNameFlags.UseNewLines);
        var req = new CertificateRequest(dName, key, HashAlgorithmName.SHA256);
        using var cert = req.CreateSelfSigned(DateTimeOffset.UtcNow.AddMinutes(-1), DateTime.UtcNow.AddYears(1));
        publicCert = cert.ExportCertificatePem();
        privateKey = key.ExportECPrivateKeyPem();
    }

    public static T LoadConfig<T>(string configFile)
    {
        if (File.Exists(configFile))
        {
            return JsonSerializer.Deserialize<T>(File.ReadAllText(configFile), opt)
                ?? throw new InvalidDataException("Unable to deserialize config file");
        }
        throw new ArgumentException("Config file not found");
    }

    public static X509Certificate2 GetSelfSignedCertificate()
    {
        //Windows needs certificates from PFX.
        //You would think that it doesn't matters whether the certificate comes from PEM or PFX,
        //but it does
        if (OperatingSystem.IsWindows())
        {
            using var pemCert = X509Certificate2.CreateFromPem(publicCert, privateKey);
            return new X509Certificate2(pemCert.Export(X509ContentType.Pfx));
        }
        return X509Certificate2.CreateFromPem(publicCert, privateKey);
    }

    public static X509Certificate2 CreateCertificate(byte[] certData, byte[] keyData)
    {
        var cert = X509Certificate2.CreateFromPem(ToPem(certData, "CERTIFICATE"), ToPem(keyData, "PRIVATE KEY"));
        //Windows needs certificates from PFX.
        //You would think that it doesn't matters whether the certificate comes from PEM or PFX,
        //but it does
        if (OperatingSystem.IsWindows())
        {
            using (cert)
            {
                return new X509Certificate2(cert.Export(X509ContentType.Pfx));
            }
        }
        return cert;
    }

    public static string ReadLine(Stream s)
    {
        List<byte> bytes = [];
        byte[] crlf = [0x0D, 0x0A];
        while (true)
        {
            var b = s.ReadByte();
            if (b < 0)
            {
                break;
            }
            bytes.Add((byte)b);
            if (bytes.TakeLast(2).SequenceEqual(crlf))
            {
                bytes.RemoveAt(bytes.Count - 1);
                bytes.RemoveAt(bytes.Count - 1);
                break;
            }
        }
        return bytes.Utf();
    }

    public static async Task<string> ReadLineAsync(Stream s)
    {
        List<byte> bytes = [];
        byte[] crlf = [0x0D, 0x0A];
        byte[] buffer = [0];
        while (true)
        {
            if (0 == await s.ReadAsync(buffer))
            {
                break;
            }
            bytes.Add(buffer[0]);
            if (bytes.TakeLast(2).SequenceEqual(crlf))
            {
                bytes.RemoveAt(bytes.Count - 1);
                bytes.RemoveAt(bytes.Count - 1);
                break;
            }
        }
        return bytes.Utf();
    }

    public static string Exec(string command)
    {
        var psi = new ProcessStartInfo(command)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true
        };
        using var p = Process.Start(psi)
            ?? throw new ArgumentException("Supplied command could not be started");
        p.StandardInput.Close();
        _ = p.StandardError.ReadToEndAsync();
        return p.StandardOutput.ReadToEnd();
    }

    private static string ToPem(byte[] data, string type)
    {
        try
        {
            return u8.GetString(data);
        }
        catch (DecoderFallbackException)
        {
            var lines = Convert.ToBase64String(data)
                .Chunk(72)
                .Select(m => new string(m));
            return
                $"-----BEGIN {type}-----" + CRLF +
                string.Join(CRLF, lines) + CRLF +
                $"-----END {type}-----" + CRLF;
        }
    }
}
