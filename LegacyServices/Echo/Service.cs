using System.Net;
using System.Net.Sockets;

namespace LegacyServices.Echo;

internal class Service : BaseService<Options>
{
    private Options? opt;
    private TcpListener? server;

    public Service()
    {
        Name = "Echo";
    }

    public override void Config(Options config)
    {
        opt = config;
        if (!config.Enabled)
        {
            Stop();
        }
        IsReady = config.Enabled;
    }

    public override void Config(string configFile)
    {
        var options = Tools.LoadConfig<Options>(configFile);
        Config(options);
    }

    public override Options GetDefaultConfig()
    {
        return new() { Enabled = true };
    }

    public override void Start()
    {
        if (opt == null || !opt.Enabled)
        {
            throw new InvalidOperationException("Service is not enabled");
        }
        if (server != null)
        {
            throw new InvalidOperationException("Service already started");
        }
        server = new(IPAddress.IPv6Any, 7);
        server.Server.DualMode = true;
        try
        {
            server.Start();
        }
        catch
        {
            server.Dispose();
            server = null;
            throw;
        }
        Accept();
    }

    public override void Stop()
    {
        server?.Dispose();
        server = null;
    }

    private async void Accept()
    {
        var options = opt;
        if (server == null || options == null)
        {
            return;
        }
        Socket socket = null!;
        try
        {
            socket = await server.AcceptSocketAsync();
            Accept();
        }
        catch
        {
            socket?.Dispose();
            return;
        }
        using var ns = new NetworkStream(socket, true);
        //Terminate if the remote end becomes unresponsive
        ns.WriteTimeout = ns.ReadTimeout = 30000;
        try
        {
            await ns.CopyToAsync(ns);
        }
        catch
        {
            //NOOP
        }
    }
}
