using LegacyServices.Services;

List<BaseService> services = [
    new LegacyServices.Services.TcpMultiplex.Service(),
    new LegacyServices.Services.Echo.Service(),
    new LegacyServices.Services.Discard.Service(),
    new LegacyServices.Services.Users.Service(),
    new LegacyServices.Services.Daytime.Service(),
    new LegacyServices.Services.Netstat.Service(),
    new LegacyServices.Services.Qotd.Service(),
    new LegacyServices.Services.Message.Service(),
    new LegacyServices.Services.Chargen.Service(),
    new LegacyServices.Services.Time.Service(),
    new LegacyServices.Services.Pwdgen.Service(),
];
var configRoot = Path.Combine(AppContext.BaseDirectory, "Config");

foreach (var s in services)
{
    Console.WriteLine("== {0} ==", s.Name);
    Console.Write("Configuring ... ");
    try
    {
        var config = Path.Combine(configRoot, s.Name + ".json");
        if (!File.Exists(config))
        {
            Fail("Configuration '{0}' does not exist. Will skip service", config);
        }
        else
        {
            s.Config(config);
            Ok();
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Fail("Configuration failed. Service will be skipped. Reason: {0}", ex.Message);
        continue;
    }
    if (s.IsReady)
    {
        Console.Write("Starting service ... ");
        try
        {
            s.Start();
            Ok();
        }
        catch (Exception ex)
        {
            Fail("Unable to start service. {0}", ex.Message);
        }
    }
    else
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Service is disabled by configuration. Did not start");
        Console.ResetColor();
    }
}
Console.WriteLine("Press CTRL+C to exit");
Thread.CurrentThread.Join();

static void Ok()
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("[OK]");
    Console.ResetColor();
}

static void Fail(string format, params object?[] args)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("[FAIL]");
    Console.WriteLine(format, args);
    Console.ResetColor();
}