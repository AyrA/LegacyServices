namespace LegacyServices;

internal abstract class BaseService<T> : BaseService
{
    /// <summary>
    /// (Re-)loads service configuration from the given configuration
    /// </summary>
    /// <param name="config">Configuration</param>
    public abstract void Config(T config);
}


internal abstract class BaseService
{
    private string name = null!;

    /// <summary>
    /// Gets whether the service is in a state where is could be started
    /// </summary>
    public bool IsReady { get; protected set; }

    /// <summary>
    /// Gets the service name
    /// </summary>
    public string Name
    {
        get
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Name has not been set");
            }
            return name;
        }

        protected set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            name = value;
        }
    }

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
