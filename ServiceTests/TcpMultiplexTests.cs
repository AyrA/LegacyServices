using LegacyServices;
using LegacyServices.TcpMultiplex;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;

namespace ServiceTests;

public class TcpMultiplexTests
{
    Service service;

    [SetUp]
    public void Setup()
    {
        service?.Stop();
        service = new();
    }

    [TearDown]
    public void TearDown()
    {
        service?.Stop();
    }

    private static Options GetBaseOptions()
    {
        return new Options()
        {
            Enabled = true,
            Help = true,
            Services = [
                new() {
                    Endpoint = IPEndPoint.Parse("127.0.0.1:80"),
                    Name = "Private",
                    Public = false
                },
                new() {
                    Endpoint = IPEndPoint.Parse("127.0.0.1:80"),
                    Name = "Public",
                    Public = true
                }],
            StartTls = true
        };
    }

    [Test]
    public void LoadConfigFromFile()
    {
        var opt = GetBaseOptions();
        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, opt.ToJson());
        try
        {
            service.Config(tempFile);
        }
        finally
        {
            File.Delete(tempFile);
        }
        Assert.Pass("Configuration loaded successfully");
    }

    [Test]
    public void UseBuiltinConfig()
    {
        var opt = service.GetDefaultConfig();
        service.Config(opt);
        Assert.That(service.IsReady, Is.True);
    }

    [Test]
    public void StartService()
    {
        service.Config(GetBaseOptions());
        service.Start();
        Assert.Pass("Service started successfully");
    }

    [Test]
    public async Task GetHelp()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        service.Config(GetBaseOptions());
        service.Start();

        using var cli = new TcpClient();
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 1), cts.Token);
        using var ns = new NetworkStream(cli.Client, true);
        ns.ReadTimeout = ns.WriteTimeout = 5000;
        using var sw = new StreamWriter(ns);
        using var sr = new StreamReader(ns);
        sw.NewLine = "\r\n";

        await WriteLine(sw, "HELP", cts.Token);

        var lines = new List<string>();
        while (!sr.EndOfStream)
        {
            var line = await ReadLine(sr, cts.Token);
            if (line != null)
            {
                lines.Add(line);
            }
        }

        Assert.That(lines, Does.Contain("Public"));
        Assert.That(lines, Does.Not.Contain("Private"));
    }

    [Test]
    public async Task GetHelpWithinTls()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(5));

        service.Config(GetBaseOptions());
        service.Start();

        using var cli = new TcpClient();
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 1), cts.Token);
        using var ns = new NetworkStream(cli.Client, true);
        ns.ReadTimeout = ns.WriteTimeout = 5000;
        using var tls = new SslStream(ns, false);

        await WriteLine(ns, "STARTTLS", cts.Token);
        await tls.AuthenticateAsClientAsync(new SslClientAuthenticationOptions()
        {
            RemoteCertificateValidationCallback = (_, _, _, _) => true
        }, cts.Token);

        TestContext.WriteLine("Remote certificate: {0}", tls.RemoteCertificate?.Subject);

        using var sw = new StreamWriter(tls);
        using var sr = new StreamReader(tls);
        sw.NewLine = "\r\n";

        await WriteLine(sw, "HELP", cts.Token);

        var lines = new List<string>();
        while (!sr.EndOfStream)
        {
            var line = await ReadLine(sr, cts.Token);
            if (line != null)
            {
                lines.Add(line);
            }
        }

        Assert.That(lines, Does.Contain("Public"));
        Assert.That(lines, Does.Not.Contain("Private"));
    }

    [Test]
    public async Task UseHttp()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(10));

        service.Config(GetBaseOptions());
        service.Start();

        using var cli = new TcpClient();
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 1), cts.Token);
        using var ns = new NetworkStream(cli.Client, true);
        ns.ReadTimeout = ns.WriteTimeout = 10000;
        using var sw = new StreamWriter(ns);
        using var sr = new StreamReader(ns);
        sw.NewLine = "\r\n";

        await WriteLine(sw, "Public", cts.Token);
        Assert.That(await ReadLine(sr, cts.Token), Does.StartWith("+"));
        await WriteLine(sw, "GET /404 HTTP/1.1", cts.Token);
        await WriteLine(sw, "Host: localhost", cts.Token);
        await WriteLine(sw, "Connection: close", cts.Token);
        await WriteLine(sw, "", cts.Token);


        var lines = new List<string>();
        while (!sr.EndOfStream)
        {
            var line = await ReadLine(sr, cts.Token);
            if (line != null)
            {
                lines.Add(line);
            }
        }

        Assert.That(lines, Is.Not.Empty);
    }

    [Test]
    public async Task UseHttpWithTls()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(TimeSpan.FromSeconds(10));

        service.Config(GetBaseOptions());
        service.Start();

        using var cli = new TcpClient();
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 1), cts.Token);
        using var ns = new NetworkStream(cli.Client, true);
        ns.ReadTimeout = ns.WriteTimeout = 10000;

        using var tls = new SslStream(ns, false);

        await WriteLine(ns, "STARTTLS", cts.Token);
        await tls.AuthenticateAsClientAsync(new SslClientAuthenticationOptions()
        {
            RemoteCertificateValidationCallback = (_, _, _, _) => true
        }, cts.Token);

        TestContext.WriteLine("Remote certificate: {0}", tls.RemoteCertificate?.Subject);

        using var sw = new StreamWriter(tls);
        using var sr = new StreamReader(tls);
        sw.NewLine = "\r\n";

        await WriteLine(sw, "Public", cts.Token);
        Assert.That(await ReadLine(sr, cts.Token), Does.StartWith("+"));
        await WriteLine(sw, "GET /404 HTTP/1.1", cts.Token);
        await WriteLine(sw, "Host: localhost", cts.Token);
        await WriteLine(sw, "Connection: close", cts.Token);
        await WriteLine(sw, "", cts.Token);


        var lines = new List<string>();
        while (!sr.EndOfStream)
        {
            var line = await ReadLine(sr, cts.Token);
            if (line != null)
            {
                lines.Add(line);
            }
        }

        Assert.That(lines, Is.Not.Empty);
    }

    private static async Task WriteLine(Stream s, string line, CancellationToken ct)
    {
        TestContext.WriteLine("OUT line: {0}", line);
        await s.WriteAsync($"{line}\r\n".Utf(), ct);
    }

    private static async Task WriteLine(StreamWriter sw, string line, CancellationToken ct)
    {
        TestContext.WriteLine("OUT line: {0}", line);
        await sw.WriteLineAsync(line.ToCharArray(), ct);
        await sw.FlushAsync(ct);
    }

    private static async Task<string?> ReadLine(StreamReader sr, CancellationToken ct)
    {
        var line = await sr.ReadLineAsync(ct);
        if (line != null)
        {
            TestContext.WriteLine("IN line: {0}", line);
        }
        else
        {
            TestContext.WriteLine("IN line was null");
        }

        return line;
    }
}
