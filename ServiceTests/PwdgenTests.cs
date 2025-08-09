using LegacyServices.Services.Pwdgen;
using LegacyServices.Validation;
using System.Net;
using System.Net.Sockets;

namespace ServiceTests;

public class PwdgenTests
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

        //Negative length
        config.Length = -1;
        config.Count = 0;
        Assert.Throws<ValidationException>(config.Validate);

        //Negative count
        config.Count = -1;
        config.Length = 0;
        Assert.Throws<ValidationException>(config.Validate);

        //Both negative
        config.Count = -1;
        config.Length = -1;
        Assert.Throws<ValidationException>(config.Validate);

        //Excessive length
        config.Count = 0;
        config.Length = 100;
        Assert.Throws<ValidationException>(config.Validate);

        //Both zero
        config.Count = 0;
        config.Length = 0;
        Assert.DoesNotThrow(config.Validate);

        //Both sensible
        config.Count = 10;
        config.Length = 20;
        Assert.DoesNotThrow(config.Validate);
    }

    [Test]
    public async Task GetPasswordsWithDefaults()
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

        //Default is 6 passwords à 8 chars
        for (var i = 0; i < 6; i++)
        {
            var pw = await sr.ReadLineAsync(cts.Token);
            Assert.That(pw, Is.Not.Null);
            Assert.That(pw, Has.Length.EqualTo(8));
        }
        //End of stream check
        var final = await sr.ReadLineAsync(cts.Token);
        Assert.That(final, Is.Null);
    }

    [Test]
    public async Task GetPasswordsWithCustomConfig()
    {
        CancellationTokenSource cts = new();
        cts.CancelAfter(5000);
        var config = service.GetDefaultConfig();
        config.Length = 20;
        config.Count = 10;
        service.Config(config);
        service.Start();

        using var cli = new TcpClient();
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, service.Port), cts.Token);
        using var ns = new NetworkStream(cli.Client);
        ns.ReadTimeout = ns.WriteTimeout = cli.SendTimeout = cli.ReceiveTimeout = 2000;
        using var sr = new StreamReader(ns);

        //10 passwords à 20 chars
        for (var i = 0; i < config.Count; i++)
        {
            var pw = await sr.ReadLineAsync(cts.Token);
            Assert.That(pw, Is.Not.Null);
            Assert.That(pw, Has.Length.EqualTo(config.Length));
        }
        //End of stream check
        var final = await sr.ReadLineAsync(cts.Token);
        Assert.That(final, Is.Null);
    }
}