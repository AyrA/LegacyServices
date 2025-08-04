namespace LegacyServices;

internal abstract class BaseService
{
    /// <summary>
    /// Gets the service name.
    /// </summary>
    /// <remarks>Also used to retrieve the configuration file</remarks>
    public abstract string Name { get; }
    /// <summary>
    /// Starts the service
    /// </summary>
    public abstract void Start();

    /// <summary>
    /// Stops the service
    /// </summary>
    public abstract void Stop();

    /// <summary>
    /// (Re-)loads service configuration from the given file
    /// </summary>
    /// <param name="configFile">Service configuration file</param>
    public abstract void Config(string configFile);
}
