using System.Net;
using System.Net.Sockets;

namespace LegacyServices.Services.Message;

internal class Service : BaseService<Options>
{
    private Options? opt;
    private TcpListener? listener;

    public Service() : base(18)
    {
        Name = "Message";
    }

    public override void Config(string configFile)
    {
        var options = Tools.LoadConfig<Options>(configFile);
        Config(options);
    }

    public override void Config(Options options)
    {
        options.Validate();
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
            Discard = false,
            VersionA = true,
            VersionB = true
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
        listener = new(IPAddress.IPv6Any, Port);
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
        Socket socket = null!;
        IPEndPoint clientEP;
        try
        {
            socket = await listener.AcceptSocketAsync();
            socket.NoDelay = true;
            if (socket.RemoteEndPoint is IPEndPoint ep)
            {
                clientEP = ep;
            }
            else
            {
                throw new IOException("Unable to determine remote endpoint");
            }
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
            while (true)
            {
                using var cts = new CancellationTokenSource();
                cts.CancelAfter(30000);
                var versionByte = ns.ReadByte();
                if (versionByte < 0)
                {
                    throw new IOException("Unexpected end of stream");
                }
                if (versionByte != 'A' && versionByte != 'B')
                {
                    await Reply(false, "Unsupported message version", ns, cts.Token);
                    return;
                }
                if (versionByte == 'A' && !options.VersionA)
                {
                    await Reply(false, "Version 'A' is disabled on this system", ns, cts.Token);
                    return;
                }
                if (versionByte == 'B' && !options.VersionB)
                {
                    await Reply(false, "Version 'B' is disabled on this system", ns, cts.Token);
                    return;
                }

                var rcpt = await Tools.ReadNullTerminatedStringAsync(ns, 512, cts.Token);
                var rcptTerminal = await Tools.ReadNullTerminatedStringAsync(ns, 512, cts.Token);
                var msg = await Tools.ReadNullTerminatedStringAsync(ns, 512, cts.Token);

                //The values below were defined in version "B" only
                var sender = string.Empty;
                var senderTerminal = string.Empty;
                var cookie = string.Empty;
                var signature = string.Empty;
                if (versionByte == 'B')
                {
                    sender = await Tools.ReadNullTerminatedStringAsync(ns, 512, cts.Token);
                    senderTerminal = await Tools.ReadNullTerminatedStringAsync(ns, 512, cts.Token);
                    cookie = await Tools.ReadNullTerminatedStringAsync(ns, 32, cts.Token);
                    signature = await Tools.ReadNullTerminatedStringAsync(ns, 512, cts.Token);
                }
                //Discard messages not for the current user or the console session
                if (string.IsNullOrEmpty(rcpt) || rcpt.EqualsCI(Environment.UserName))
                {
                    if (string.IsNullOrEmpty(rcptTerminal) || rcptTerminal == "*" || rcptTerminal.EqualsCI("CONSOLE"))
                    {
                        if (IsSafe(msg))
                        {
                            if (string.IsNullOrEmpty(sender))
                            {
                                Console.WriteLine("Message from {0}: {1}", clientEP.Address, msg);
                                await Reply(true, "Message sent", ns, cts.Token);
                            }
                            else if (IsSafe(sender) && IsSafe(senderTerminal))
                            {
                                if (!options.Discard)
                                {
                                    Console.WriteLine("Message from {0}@{1} at {2}: {3}", sender, senderTerminal, clientEP.Address, msg);
                                }
                                await Reply(true, "Message sent", ns, cts.Token);
                            }
                            else
                            {
                                await Reply(false, "Invalid sender or terminal name", ns, cts.Token);
                            }
                        }
                        else
                        {
                            await Reply(false, "Message contains unsupported characters", ns, cts.Token);
                        }
                    }
                    else
                    {
                        await Reply(false, "Unknown terminal", ns, cts.Token);
                    }
                }
                else
                {
                    await Reply(false, "Unknown user", ns, cts.Token);
                }
                await ns.FlushAsync(cts.Token);
            }
        }
        catch
        {
            //Network errors happen all the time.
            //Do nothing and simply close the connection
        }
    }

    private static bool IsSafe(IEnumerable<char> data)
    {
        return data.All(m => !char.IsControl(m) || m == '\r' || m == '\n' || m == '\t');
    }

    private static async Task Reply(bool success, string message, Stream s, CancellationToken ct = default)
    {
        await s.WriteAsync($"{(success ? '+' : '-')}{message}\0".Latin1(), ct);
    }
}
