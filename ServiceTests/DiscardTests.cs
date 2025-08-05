using LegacyServices.Discard;
using System.Net;
using System.Net.Sockets;

namespace ServiceTests;

public class DiscardTests
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
    public async Task DiscardDoesNotEcho()
    {
        CancellationTokenSource cts = new();
        //cts.CancelAfter(5000);
        service.Config(service.GetDefaultConfig());
        service.Start();

        byte[] data = [.. Enumerable.Range(0, 0x100).Select(m => (byte)m)];
        using var cli = new TcpClient();
        cli.NoDelay = true;
        cli.SendTimeout = cli.ReceiveTimeout = 2000;
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 9), cts.Token);
        cli.Client.Send(data);
        Assert.Throws<SocketException>(() => cli.Client.Receive(data));
    }


    [Test]
    public async Task DiscardTimeout()
    {
        CancellationTokenSource cts = new();
        cts.CancelAfter(15000);
        var config = service.GetDefaultConfig();
        config.Timeout = 3;
        service.Config(config);
        service.Start();

        using var cli = new TcpClient();
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 9), cts.Token);
        using var ns = new NetworkStream(cli.Client, true);
        ns.ReadTimeout = ns.WriteTimeout = 15000;
        var discard = ns.CopyToAsync(Stream.Null); //Discard received data
        byte[] data = [.. Enumerable.Range(0, 0x100).Select(m => (byte)m)];
        await ns.WriteAsync(data, cts.Token);
        await ns.FlushAsync(cts.Token);
        //Wait 5 seconds
        await Task.Delay(5000);
        //Trying to send now should throw an exception when we fill the buffer
        try
        {
            for (var i = 0; i < 100; i++)
            {
                await ns.WriteAsync(data, cts.Token);
                await ns.FlushAsync(cts.Token);
            }
            Assert.Fail("Expected socket exception but was successful instead");
        }
        catch (SocketException)
        {
            Assert.Pass();
        }
        catch (IOException)
        {
            Assert.Pass();
        }
    }

    [Test]
    public async Task DiscardMaxData()
    {
        CancellationTokenSource cts = new();
        cts.CancelAfter(15000);
        var config = service.GetDefaultConfig();
        config.MaxData = 5000;
        service.Config(config);
        service.Start();

        using var cli = new TcpClient();
        cli.NoDelay = true;
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 9), cts.Token);
        using var ns = new NetworkStream(cli.Client, true);
        var discard = ns.CopyToAsync(Stream.Null); //Discard received data
        byte[] data = [.. Enumerable.Range(0, 0x100).Select(m => (byte)m)];
        try
        {
            for (var i = 0; i < 1000; i++)
            {
                await ns.WriteAsync(data, cts.Token);
                await ns.FlushAsync(cts.Token);
            }
            Assert.Fail("Expected socket exception but was successful instead");
        }
        catch (SocketException)
        {
            Assert.Pass();
        }
        catch (IOException)
        {
            Assert.Pass();
        }
    }
}