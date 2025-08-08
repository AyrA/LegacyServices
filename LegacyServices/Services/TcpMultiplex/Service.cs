using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace LegacyServices.Services.TcpMultiplex;

internal class Service : BaseService<Options>
{
    private Options? opt;
    private X509Certificate2? certificate;
    private TcpListener? listener;

    public Service()
    {
        Name = "TcpMultiplex";
    }

    public override void Config(string configFile)
    {
        var options = Tools.LoadConfig<Options>(configFile);
        Config(options);
    }

    public override void Config(Options options)
    {
        options.Validate();
        certificate?.Dispose();
        if (options.StartTls)
        {
            //Use self signed certificate if certificate files are not specified
            if (!string.IsNullOrEmpty(options.Certificate?.Private) && !string.IsNullOrEmpty(options.Certificate?.Public))
            {
                certificate = X509Certificate2.CreateFromPemFile(options.Certificate.Public, options.Certificate.Private);
            }
            else
            {
                certificate = Tools.GetSelfSignedCertificate();
            }
        }
        else
        {
            certificate = null;
        }
        opt = options;
        //Stop service if it's no longer enabled
        if (!opt.Enabled)
        {
            Stop();
            IsReady = false;
            return;
        }
        IsReady = true;
    }

    public override Options GetDefaultConfig()
    {
        return new()
        {
            Enabled = true,
            Help = true,
            StartTls = true,
            Services = [new() { Name = "TCPMUX", Endpoint = IPEndPoint.Parse("127.1:1"), Public = true }]
        };
    }

    public override void Start()
    {
        if (opt == null)
        {
            throw new InvalidOperationException("Configuration has not been loaded");
        }
        if (!opt.Enabled)
        {
            throw new InvalidOperationException("Service is disabled");
        }
        if (listener != null)
        {
            throw new InvalidOperationException("Service already started");
        }
        listener = new(IPAddress.IPv6Any, 1);
        listener.Server.DualMode = true;
        listener.Start();
        Accept();
    }

    public override void Stop()
    {
        listener?.Stop();
        listener?.Dispose();
        listener = null;
    }

    private async void Accept()
    {
        var options = opt;
        if (listener == null || options == null)
        {
            return;
        }
        options.Validate();
        Stream currentStream;
        SslStream? tls = null;
        Socket socket = null!;
        try
        {
            socket = await listener!.AcceptSocketAsync();
            Accept();
        }
        catch
        {
            socket?.Dispose();
            return;
        }
        using var ns = new NetworkStream(socket, true);

        //Terminate if the remote end becomes unresponsive
        CancellationTokenSource cts = new();
        cts.CancelAfter(30000);
        ns.WriteTimeout = ns.ReadTimeout = 30000;

        currentStream = ns;
        try
        {
            var line = await Tools.ReadLineAsync(currentStream, 80, cts.Token);

            //Do TLS if enabled
            if (certificate != null && options.StartTls && line.EqualsCI("STARTTLS"))
            {
                var authOpt = new SslServerAuthenticationOptions()
                {
                    ServerCertificate = certificate
                };
                tls = new SslStream(currentStream);
                currentStream = tls;
                await tls.AuthenticateAsServerAsync(authOpt, cts.Token);
                //Read again but now from the TLS stream to get the command
                line = await Tools.ReadLineAsync(currentStream, 80, cts.Token);
            }

            //Do HELP if requested
            if (line.EqualsCI("HELP"))
            {
                if (options.Help)
                {
                    var list = options.Services
                        .Where(m => m.Public)
                        .Select(m => m.Name);
                    var bytes = (string.Join(Tools.CRLF, list) + Tools.CRLF).Latin1();
                    await currentStream.WriteAsync(bytes, cts.Token);
                    await currentStream.FlushAsync(cts.Token);
                }
                else
                {
                    //Pretend the service list is empty if help is disabled
                    await currentStream.WriteAsync("\r\n".Latin1(), cts.Token);
                }
                return;
            }

            var service = options.GetService(line);
            if (service != null)
            {
                service.Validate();
                //Disable timeouts again
                ns.WriteTimeout = ns.ReadTimeout = Timeout.Infinite;
                if (!await Forward(currentStream, service.Endpoint))
                {
                    await currentStream.WriteAsync("-Service unavailable\r\n".Latin1(), cts.Token);
                }
            }
            else
            {
                await currentStream.WriteAsync("-Unknown service\r\n".Latin1(), cts.Token);
            }
        }
        catch
        {
            //Network errors happen all the time.
            //Do nothing and simply close the connection
        }
        finally
        {
            tls?.Dispose();
            currentStream?.Dispose();
        }
    }

    private static async Task<bool> Forward(Stream local, IPEndPoint destination)
    {
        using var cts = new CancellationTokenSource();
        cts.CancelAfter(5000);
        using var client = new TcpClient();
        try
        {
            await client.ConnectAsync(destination, cts.Token);
        }
        catch
        {
            return false;
        }
        //Connected
        await local.WriteAsync("+Connected\r\n".Latin1(), cts.Token);
        using var ns = new NetworkStream(client.Client, true);
        var t1 = local.CopyToAsync(ns);
        var t2 = ns.CopyToAsync(local);
        try
        {
            await Task.WhenAny(t1, t2);
        }
        catch
        {
            //Don't care at this point
        }
        return true;
    }
}
