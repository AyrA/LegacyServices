using LegacyServices.Services.Chargen;
using LegacyServices.Validation;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace ServiceTests;

public class ChargenTests
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
    public void Validation()
    {
        var config = service.GetDefaultConfig();

        //Line delay
        config.LineDelay = -1;
        config.LineLimit = 0;
        Assert.Throws<ValidationException>(config.Validate);

        //Global delay
        config = service.GetDefaultConfig();
        config.LineDelay = 0;
        config.GlobalDelay = true;
        Assert.Throws<ValidationException>(config.Validate);

        //Line count
        config = service.GetDefaultConfig();
        config.LineLimit = -1;
        config.LineDelay = 0;
        Assert.Throws<ValidationException>(config.Validate);
    }

    [Test]
    public async Task GetLines()
    {
        CancellationTokenSource cts = new();
        cts.CancelAfter(5000);
        service.Config(service.GetDefaultConfig());
        service.Start();

        using var cli = new TcpClient();
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 19), cts.Token);
        using var ns = new NetworkStream(cli.Client);
        ns.ReadTimeout = ns.WriteTimeout = cli.SendTimeout = cli.ReceiveTimeout = 2000;
        using var sr = new StreamReader(ns);
        for (var i = 0; i < 100; i++)
        {
            Assert.That(await sr.ReadLineAsync(cts.Token), Is.Not.Null);
        }
    }

    [Test]
    public async Task GetLimitedLines()
    {
        const int limit = 10;
        CancellationTokenSource cts = new();
        cts.CancelAfter(5000);
        var config = service.GetDefaultConfig();
        config.LineLimit = limit;
        service.Config(config);
        service.Start();

        using var cli = new TcpClient();
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 19), cts.Token);
        using var ns = new NetworkStream(cli.Client);
        ns.ReadTimeout = ns.WriteTimeout = cli.SendTimeout = cli.ReceiveTimeout = 2000;
        using var sr = new StreamReader(ns);
        for (var i = 0; i < limit; i++)
        {
            Assert.That(await sr.ReadLineAsync(cts.Token), Is.Not.Null);
        }
        Assert.That(await sr.ReadLineAsync(cts.Token), Is.Null);
    }

    [Test]
    public async Task GetDelayedLines()
    {
        TestTools.IgnoreTimeoutTest();
        const int delay = 100;
        const int count = 10;
        CancellationTokenSource cts = new();
        cts.CancelAfter(5000);
        var config = service.GetDefaultConfig();
        config.LineDelay = delay;
        service.Config(config);
        service.Start();

        using var cli = new TcpClient();
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 19), cts.Token);
        using var ns = new NetworkStream(cli.Client);
        ns.ReadTimeout = ns.WriteTimeout = cli.SendTimeout = cli.ReceiveTimeout = 2000;
        using var sr = new StreamReader(ns);
        var sw = Stopwatch.StartNew();
        for (var i = 0; i < count; i++)
        {
            await sr.ReadLineAsync(cts.Token);
        }
        sw.Stop();
        Assert.That(sw.ElapsedMilliseconds, Is.GreaterThan((delay * count) - 1));
    }

}