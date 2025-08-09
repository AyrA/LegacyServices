using LegacyServices.Services.Daytime;
using System.Globalization;
using System.Net;
using System.Net.Sockets;

namespace ServiceTests;

public class DaytimeTests
{
    private Service service;

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

    [Test]
    public void SetConfig()
    {
        service.Config(service.GetDefaultConfig());
        Assert.Pass();
    }

    [Test]
    public void LoadConfigFromFile()
    {
        var opt = service.GetDefaultConfig();
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
    public async Task GetDaytime()
    {
        CancellationTokenSource cts = new();
        cts.CancelAfter(5000);
        service.Config(service.GetDefaultConfig());
        service.Start();

        using var cli = new TcpClient();
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, service.Port), cts.Token);
        using var ns = new NetworkStream(cli.Client);
        ns.ReadTimeout = ns.WriteTimeout = cli.SendTimeout = cli.ReceiveTimeout = 2000;
        using var sr = new StreamReader(ns);
        var dt = sr.ReadToEnd().Trim();
        Assert.DoesNotThrow(() => { DateTime.Parse(dt); });
    }

    [Test]
    public async Task GetDaytimeCustomFormat()
    {
        string[] formats = ["u", "f", "yyyy-MM-dd HH:mm:ss"];
        foreach (var f in formats)
        {
            TestContext.WriteLine("Trying {0} (ex: {1})", f, DateTime.Now.ToString(f, CultureInfo.InvariantCulture));

            using CancellationTokenSource cts = new();
            cts.CancelAfter(5000);
            var config = service.GetDefaultConfig();
            config.Format = f;
            service.Stop();
            service.Config(config);
            service.Start();

            using var cli = new TcpClient();
            await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, service.Port), cts.Token);
            using var ns = new NetworkStream(cli.Client);
            ns.ReadTimeout = ns.WriteTimeout = cli.SendTimeout = cli.ReceiveTimeout = 2000;
            using var sr = new StreamReader(ns);
            var dt = sr.ReadToEnd().Trim();
            Assert.That(DateTime.TryParseExact(dt, f, CultureInfo.InvariantCulture, DateTimeStyles.None, out _), Is.True);
        }
    }
}