// See https://aka.ms/new-console-template for more information
using LegacyServices;

Console.WriteLine("Hello, World!");

List<BaseService> services = [
    new LegacyServices.TcpMultiplex.Service()
];
var configRoot = Path.Combine(AppContext.BaseDirectory, "Config");

foreach (var s in services)
{
    Console.WriteLine("Configuring {0}...", s.Name);
    s.Config(Path.Combine(configRoot, s.Name + ".json"));
    try
    {
        s.Start();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Unable to start service. {0}", ex.Message);
    }
}
Console.WriteLine("Press CTRL+C to exit");
Thread.CurrentThread.Join();
