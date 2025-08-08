using LegacyServices.Services.Message;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ServiceTests;

public class MessageTests
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
    public async Task SendMessageA()
    {
        CancellationTokenSource cts = new();
        cts.CancelAfter(5000);
        service.Config(service.GetDefaultConfig());
        service.Start();

        using var cli = new TcpClient();
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 18), cts.Token);
        using var ns = new NetworkStream(cli.Client);
        ns.ReadTimeout = ns.WriteTimeout = cli.SendTimeout = cli.ReceiveTimeout = 5000;
        byte[] msg = [(byte)'A', .. ToBin(Environment.UserName), .. ToBin(""), .. ToBin("test")];
        byte[] response = new byte[512];
        await ns.WriteAsync(msg, cts.Token);
        Assert.That(await ns.ReadAsync(response, cts.Token), Is.GreaterThan(0));

        var i = Array.IndexOf(response, (byte)0);
        if (i < 0)
        {
            i = response.Length;
        }
        var decoded = Encoding.Latin1.GetString([.. response.Take(i)]);
        TestContext.WriteLine("Response: {0}", decoded);

        Assert.That(decoded, Is.Not.Empty);
        Assert.That(decoded[0], Is.EqualTo('+'));
    }

    [Test]
    public async Task SendMessageB()
    {
        CancellationTokenSource cts = new();
        cts.CancelAfter(5000);
        service.Config(service.GetDefaultConfig());
        service.Start();

        using var cli = new TcpClient();
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 18), cts.Token);
        using var ns = new NetworkStream(cli.Client);
        ns.ReadTimeout = ns.WriteTimeout = cli.SendTimeout = cli.ReceiveTimeout = 5000;
        byte[] msg = [(byte)'B', .. ToBin(Environment.UserName), .. ToBin(""), .. ToBin("test"),
        .. ToBin("Sender"), .. ToBin("SenderTerminal"), .. ToBin("Cookie"), .. ToBin("Sig")];
        byte[] response = new byte[512];
        await ns.WriteAsync(msg, cts.Token);
        Assert.That(await ns.ReadAsync(response, cts.Token), Is.GreaterThan(0));

        var i = Array.IndexOf(response, (byte)0);
        if (i < 0)
        {
            i = response.Length;
        }
        var decoded = Encoding.Latin1.GetString([.. response.Take(i)]);
        TestContext.WriteLine("Response: {0}", decoded);

        Assert.That(decoded, Is.Not.Empty);
        Assert.That(decoded[0], Is.EqualTo('+'));
    }

    [Test]
    public async Task SendMultiple()
    {
        CancellationTokenSource cts = new();
        cts.CancelAfter(5000);
        service.Config(service.GetDefaultConfig());
        service.Start();

        using var cli = new TcpClient();
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 18), cts.Token);
        using var ns = new NetworkStream(cli.Client);
        ns.ReadTimeout = ns.WriteTimeout = cli.SendTimeout = cli.ReceiveTimeout = 5000;
        byte[] msg = [(byte)'A', .. ToBin(Environment.UserName), .. ToBin(""), .. ToBin("test")];
        byte[] response = new byte[512];
        for (var i = 0; i < 5; i++)
        {
            await ns.WriteAsync(msg, cts.Token);

            var nullByte = await ns.ReadAsync(response, cts.Token);
            Assert.That(nullByte, Is.GreaterThan(0));

            var decoded = Encoding.Latin1.GetString([.. response.Take(nullByte - 1)]);
            TestContext.WriteLine("Response: {0}", decoded);

            Assert.That(decoded, Is.Not.Empty);
            Assert.That(decoded[0], Is.EqualTo('+'));
        }
    }

    [Test]
    public async Task SendMessageADisabled()
    {
        CancellationTokenSource cts = new();
        cts.CancelAfter(5000);
        var config = service.GetDefaultConfig();
        config.VersionA = false;
        service.Config(config);
        service.Start();

        using var cli = new TcpClient();
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 18), cts.Token);
        using var ns = new NetworkStream(cli.Client);
        ns.ReadTimeout = ns.WriteTimeout = cli.SendTimeout = cli.ReceiveTimeout = 5000;
        byte[] msg = [(byte)'A', .. ToBin(Environment.UserName), .. ToBin(""), .. ToBin("test")];
        byte[] response = new byte[512];
        await ns.WriteAsync(msg, cts.Token);
        Assert.That(await ns.ReadAsync(response, cts.Token), Is.GreaterThan(0));

        var i = Array.IndexOf(response, (byte)0);
        if (i < 0)
        {
            i = response.Length;
        }
        var decoded = Encoding.Latin1.GetString([.. response.Take(i)]);
        TestContext.WriteLine("Response: {0}", decoded);

        Assert.That(decoded, Is.Not.Empty);
        Assert.That(decoded[0], Is.EqualTo('-'));
    }

    [Test]
    public async Task SendMessageBDisabled()
    {
        CancellationTokenSource cts = new();
        cts.CancelAfter(5000);
        var config = service.GetDefaultConfig();
        config.VersionB = false;
        service.Config(config);
        service.Start();

        using var cli = new TcpClient();
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 18), cts.Token);
        using var ns = new NetworkStream(cli.Client);
        ns.ReadTimeout = ns.WriteTimeout = cli.SendTimeout = cli.ReceiveTimeout = 5000;
        byte[] msg = [(byte)'B', .. ToBin(Environment.UserName), .. ToBin(""), .. ToBin("test"),
        .. ToBin("Sender"), .. ToBin("SenderTerminal"), .. ToBin("Cookie"), .. ToBin("Sig")];
        byte[] response = new byte[512];
        await ns.WriteAsync(msg, cts.Token);
        Assert.That(await ns.ReadAsync(response, cts.Token), Is.GreaterThan(0));

        var i = Array.IndexOf(response, (byte)0);
        if (i < 0)
        {
            i = response.Length;
        }
        var decoded = Encoding.Latin1.GetString([.. response.Take(i)]);
        TestContext.WriteLine("Response: {0}", decoded);

        Assert.That(decoded, Is.Not.Empty);
        Assert.That(decoded[0], Is.EqualTo('-'));
    }

    [Test]
    public async Task CheckDataSanitize()
    {
        CancellationTokenSource cts = new();
        cts.CancelAfter(5000);
        service.Config(service.GetDefaultConfig());
        service.Start();

        using var cli = new TcpClient();
        await cli.ConnectAsync(new IPEndPoint(IPAddress.Loopback, 18), cts.Token);
        using var ns = new NetworkStream(cli.Client);
        ns.ReadTimeout = ns.WriteTimeout = cli.SendTimeout = cli.ReceiveTimeout = 5000;

        byte[][] messages = [
            //Invalid message
            [(byte)'A', .. ToBin(Environment.UserName), .. ToBin(""), .. ToBin("te\bst")],
            [(byte)'B', .. ToBin(Environment.UserName), .. ToBin(""), .. ToBin("te\bst"),
            .. ToBin("Sender"), .. ToBin("SenderTerminal"), .. ToBin("Cookie"), .. ToBin("Sig")],
            //Invalid sender
            [(byte)'B', .. ToBin(Environment.UserName), .. ToBin(""), .. ToBin("test"),
            .. ToBin("Sen\bder"), .. ToBin("SenderTerminal"), .. ToBin("Cookie"), .. ToBin("Sig")],
            //Invalid sender terminal
            [(byte)'B', .. ToBin(Environment.UserName), .. ToBin(""), .. ToBin("test"),
            .. ToBin("Sender"), .. ToBin("Sender\bTerminal"), .. ToBin("Cookie"), .. ToBin("Sig")]
        ];

        foreach (var msg in messages)
        {
            byte[] response = new byte[512];
            await ns.WriteAsync(msg, cts.Token);
            Assert.That(await ns.ReadAsync(response, cts.Token), Is.GreaterThan(0));

            var i = Array.IndexOf(response, (byte)0);
            if (i < 0)
            {
                i = response.Length;
            }
            var decoded = Encoding.Latin1.GetString([.. response.Take(i)]);
            TestContext.WriteLine("Response: {0}", decoded);

            Assert.That(decoded, Is.Not.Empty);
            Assert.That(decoded[0], Is.EqualTo('-'));
        }
    }

    private static byte[] ToBin(string text)
    {
        return [.. Encoding.Latin1.GetBytes(text), 0x00];
    }
}