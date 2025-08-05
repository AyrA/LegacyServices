using LegacyServices.Users;
using System.Net;
using System.Net.Sockets;

namespace ServiceTests;

public class UserTests
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
    public async Task GetUsers()
    {
        CancellationTokenSource cts = new();
        cts.CancelAfter(5000);
        service.Config(service.GetDefaultConfig());
        service.Start();

        using var cli = new TcpClient();
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 11), cts.Token);
        using var ns = new NetworkStream(cli.Client);
        ns.ReadTimeout = ns.WriteTimeout = cli.SendTimeout = cli.ReceiveTimeout = 2000;
        using var sr = new StreamReader(ns);
        while (true)
        {
            var line = sr.ReadLine();
            if (line == null)
            {
                break;
            }
            TestContext.WriteLine("IN: {0}", line);
            Assert.That(line, Is.Not.Empty);
        }
    }
}