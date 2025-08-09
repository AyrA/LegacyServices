namespace LegacyServices.Services;

internal abstract class BaseService<T>(int port) : BaseService(port)
{
    /// <summary>
    /// (Re-)loads service configuration from the given configuration
    /// </summary>
    /// <param name="config">Configuration</param>
    public abstract void Config(T config);

    /// <summary>
    /// Gets a minimal functional configuration
    /// </summary>
    /// <returns>Configuration</returns>
    public abstract T GetDefaultConfig();
}

internal abstract class BaseService(int port)
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
    /// Gets the port in use by the service
    /// </summary>
    /// <remarks>
    /// This value indicates the TCP port of the listener,
    /// not whether the listener is actually listening or suspended.
    /// </remarks>
    public int Port => port;

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
