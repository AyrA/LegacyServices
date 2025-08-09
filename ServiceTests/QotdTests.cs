using LegacyServices.Services.Qotd;
using LegacyServices.Validation;
using System.Net;
using System.Net.Sockets;

namespace ServiceTests;

public class QotdTests
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

        //Excessive length
        config.AllowExcessiveLength = false;
        config.Quotes = [string.Empty.PadRight(1000, '#')];
        Assert.Throws<ValidationException>(config.Validate);

        //Excessive length with allow flag
        config.AllowExcessiveLength = true;
        Assert.DoesNotThrow(config.Validate);

        //Null list
        config.Quotes = null;
        Assert.DoesNotThrow(config.Validate);

        //Empty
        config.Quotes = [];
        Assert.Throws<ValidationException>(config.Validate);

        //Null item
        config.Quotes = ["1", null!, "2"];
        Assert.Throws<ValidationException>(config.Validate);

        //Empty item
        config.Quotes = ["1", "", "2"];
        Assert.Throws<ValidationException>(config.Validate);

        //WS item
        config.Quotes = ["1", "\t\t\t", "2"];
        Assert.Throws<ValidationException>(config.Validate);
    }

    [Test]
    public async Task GetQuote()
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
        var result = sr.ReadToEnd().Trim();
        Assert.That(string.IsNullOrWhiteSpace(result), Is.False);
    }

    [Test]
    public async Task GetCustomQuote()
    {
        CancellationTokenSource cts = new();
        cts.CancelAfter(5000);
        var config = service.GetDefaultConfig();
        config.Quotes = ["Quote A", "Quote B", "Quote C", "Quote D"];
        service.Config(config);
        service.Start();

        using var cli = new TcpClient();
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, service.Port), cts.Token);
        using var ns = new NetworkStream(cli.Client);
        ns.ReadTimeout = ns.WriteTimeout = cli.SendTimeout = cli.ReceiveTimeout = 2000;
        using var sr = new StreamReader(ns);
        var result = sr.ReadToEnd().Trim();
        Assert.That(config.Quotes, Does.Contain(result));
    }
}