using LegacyServices.Validation;
using System.Net;
using System.Net.Sockets;

namespace LegacyServices.Services;

/// <summary>
/// Base service for any TCP service that unconditionally sends a simple response, and then disconnects
/// </summary>
/// <typeparam name="T">Options</typeparam>
/// <param name="port">TCP server port</param>
internal abstract class BaseResponseService<T>(int port) : BaseService<T> where T : class, IEnable, new()
{
    /// <summary>
    /// Current set of options. Null if none have been set yet
    /// </summary>
    protected T? opt;
    /// <summary>
    /// Current TCP listener. Null if not listening
    /// </summary>
    private TcpListener? server;
    /// <summary>
    /// Gets whether to repeatedly call <see cref="GetResponse(T, int)"/>
    /// or to disconnect after a single call
    /// </summary>
    protected bool repeat;
    /// <summary>
    /// Gets whether to set the NoDelay flag on sockets or not.
    /// Default is to set the flag
    /// </summary>
    protected bool useNodelay = true;

    public override void Config(T config)
    {
        if (config is IValidateable v)
        {
            v.Validate();
        }
        opt = config;
        IsReady = config.Enabled;
        if (!config.Enabled)
        {
            Stop();
        }
    }

    public override void Config(string configFile)
    {
        Config(Tools.LoadConfig<T>(configFile));
    }

    public override T GetDefaultConfig() => new() { Enabled = true };

    public override void Start()
    {
        if (opt == null || !IsReady)
        {
            throw new InvalidOperationException("Service is not enabled");
        }
        if (server != null)
        {
            throw new InvalidOperationException("Service already started");
        }
        server = new(IPAddress.IPv6Any, port);
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

    protected abstract Task<byte[]?> GetResponse(T options, int iteration);

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
            socket.NoDelay = useNodelay;
            Accept();
        }
        catch
        {
            socket?.Dispose();
            return;
        }
        using (socket)
        {
            try
            {
                int iteration = 0;
                do
                {
                    using var cts = new CancellationTokenSource();
                    cts.CancelAfter(10000);
                    var data = await GetResponse(options, ++iteration);
                    if (data == null || data.Length == 0)
                    {
                        return;
                    }
                    await socket.SendAsync(data, cts.Token);
                }
                while (repeat);
            }
            catch
            {
                //NOOP
            }
        }
    }

}
