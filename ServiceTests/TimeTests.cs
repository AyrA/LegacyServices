using LegacyServices.Services.Time;
using System.Net;
using System.Net.Sockets;

namespace ServiceTests;

public class TimeTests
{
    private static readonly DateTime Epoch = new(1900, 1, 1, 0, 0, 0, 0, 0, DateTimeKind.Utc);
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
    public async Task GetTime32()
    {
        using CancellationTokenSource cts = new();
        cts.CancelAfter(5000);
        service.Config(service.GetDefaultConfig());
        service.Start();

        using var cli = new TcpClient();
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 37), cts.Token);
        using var ns = new NetworkStream(cli.Client);
        ns.ReadTimeout = ns.WriteTimeout = cli.SendTimeout = cli.ReceiveTimeout = 2000;
        byte[] buffer = new byte[20];
        int count = await ns.ReadAsync(buffer, cts.Token);
        Assert.That(count, Is.EqualTo(4));
        var secs = (uint)IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, 0));
        var dt = Epoch.AddSeconds(secs);
        var diff = Math.Abs(dt.Subtract(DateTime.UtcNow).TotalSeconds);
        Assert.That(Math.Floor(diff), Is.LessThan(2));
        TestContext.WriteLine("Diff is {0} seconds", diff);
    }

    [Test]
    public async Task GetTime64()
    {
        using CancellationTokenSource cts = new();
        cts.CancelAfter(5000);
        var config = service.GetDefaultConfig();
        config.UseInt64 = true;
        service.Config(config);
        service.Start();

        using var cli = new TcpClient();
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 37), cts.Token);
        using var ns = new NetworkStream(cli.Client);
        ns.ReadTimeout = ns.WriteTimeout = cli.SendTimeout = cli.ReceiveTimeout = 2000;
        byte[] buffer = new byte[20];
        int count = await ns.ReadAsync(buffer, cts.Token);
        Assert.That(count, Is.EqualTo(8));
        var secs = (ulong)IPAddress.NetworkToHostOrder(BitConverter.ToInt64(buffer, 0));
        var dt = Epoch.AddSeconds(secs);
        var diff = Math.Abs(dt.Subtract(DateTime.UtcNow).TotalSeconds);
        Assert.That(Math.Floor(diff), Is.LessThan(2));
        TestContext.WriteLine("Diff is {0} seconds", diff);
    }
}