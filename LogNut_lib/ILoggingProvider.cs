
namespace Hurst.LogNut
{
    /// <summary>
    /// If you make your Application implement this interface, the base-level libraries can log themselves.
    /// </summary>
    public interface ILoggingProvider
    {
        /// <summary>
        /// Get the default Logger of this <c>ILoggingProvider</c>.
        /// </summary>
        ILognutLogger Logger { get; }
    }

    /// <summary>
    /// Interface <c>ILoggingConfigurable</c> mandates a <see cref="ConfigureLogging"/> method.
    /// </summary>
    public interface ILoggingConfigurable
    {
        /// <summary>
        /// Set up whatever logging this program is going to use.
        /// </summary>
        void ConfigureLogging();
    }

    /// <summary>
    /// This is a platform-neutral log-manager
    /// </summary>
    public interface ISimpleLogManager
    {
        /// <summary>
        /// Get an instance of a <see cref="BaseLib.ISimpleLogger"/> with the given logger-name, to do some logging with.
        /// </summary>
        /// <param name="loggerName">the name (any arbitrary bit of text actually) to associate with this logger. This may be null.</param>
        /// <returns>a new or pre-existing ILognutLogger object of the given name</returns>
        ILognutLogger GetLogger(string loggerName);

        /// <summary>
        /// Get or set whether logging is enabled on any loggers from this log-manager.
        /// </summary>
        bool IsLoggingEnabled { get; set; }

        /// <summary>
        /// Get or set whether Infomation-level output is enabled.
        /// </summary>
        bool IsInfoEnabled { get; set; }

        /// <summary>
        /// Get or set whether Warning-level output is enabled.
        /// </summary>
        bool IsWarnEnabled { get; set; }

        /// <summary>
        /// Get or set whether Debug-level output is enabled.
        /// </summary>
        bool IsErrorEnabled { get; set; }

        /// <summary>
        /// Make the logging output go to the file denoted by the given filesystem-path.
        /// </summary>
        /// <param name="pathToLogFile">the filename to log to</param>
        void SetLogFilename(string pathToLogFile);
    }
}
