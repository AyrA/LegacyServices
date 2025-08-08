using LegacyServices;

List<BaseService> services = [
    new LegacyServices.TcpMultiplex.Service(),
    new LegacyServices.Echo.Service(),
    new LegacyServices.Discard.Service(),
    new LegacyServices.Users.Service(),
];
var configRoot = Path.Combine(AppContext.BaseDirectory, "Config");

foreach (var s in services)
{
    Console.WriteLine("== {0} ==", s.Name);
    Console.Write("Configuring ... ");
    try
    {
        s.Config(Path.Combine(configRoot, s.Name + ".json"));
        Ok();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Fail("Configuration failed. Reason: {0}", ex.Message);
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