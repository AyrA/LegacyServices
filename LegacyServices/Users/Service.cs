using System.Net;
using System.Net.Sockets;

namespace LegacyServices.Users;

internal class Service : BaseService<Options>
{
    private Options? opt;
    private TcpListener? server;

    public Service()
    {
        Name = "Users";
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
        server = new(IPAddress.IPv6Any, 11);
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
            socket.NoDelay = true;
            Accept();
        }
        catch
        {
            socket?.Dispose();
            return;
        }
        var data = (string.Join(Tools.CRLF, GetUsers()) + Tools.CRLF).Utf();
        var cts = new CancellationTokenSource();
        cts.CancelAfter(1000);
        using (socket)
        {
            try
            {
                await socket.SendAsync(data, cts.Token);
            }
            catch
            {
                //NOOP
            }
        }
    }

    private static string[] GetUsers()
    {
        //TODO: Get list of all active users.
        //This usually requires some sort of elevated permission,
        //so we don't do it here
        return [Environment.UserName];
    }
}
