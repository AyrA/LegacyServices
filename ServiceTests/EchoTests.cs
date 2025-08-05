using LegacyServices.Echo;
using System.Net;
using System.Net.Sockets;

namespace ServiceTests;

public class EchoTests
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
    public async Task Echo()
    {
        CancellationTokenSource cts = new();
        cts.CancelAfter(5000);
        service.Config(service.GetDefaultConfig());
        service.Start();

        using var cli = new TcpClient();
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 7), cts.Token);
        using var ns = new NetworkStream(cli.Client, true);
        ns.ReadTimeout = ns.WriteTimeout = 5000;
        using var bw = new BinaryWriter(ns);
        using var br = new BinaryReader(ns);
        byte[] data = [.. Enumerable.Range(0, 0x100).Select(m => (byte)m)];
        bw.Write(data);
        bw.Flush();
        var compare = br.ReadBytes(data.Length);
        Assert.That(data, Is.EquivalentTo(compare));
    }
}