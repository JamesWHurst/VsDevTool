

namespace Hurst.LogNut.OutputPipes
{
    /// <summary>
    /// OutputPipes (things that implement IOutputPipe)
    /// represent forms of output for LogNut to log to.
    /// </summary>
    /// <remarks>
    /// Instances of IOutputPipe currently include:
    ///  * EmailOutputPipe (named "Email")
    ///  * IpcOutputPipe   (named "IPC")
    ///  * WindowsEventLog (named "WindowsEventLog")
    /// </remarks>
    public interface IOutputPipe
    {
        #region properties

        /// <summary>
        /// Get or set whether output to this OutputPipe is enabled.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Get or set whether output to this OutputPipe is has been failing.
        /// This is used to avoid repeatedly trying to send a log over a non-working channel.
        /// </summary>
        bool IsFailing { get; set; }

        /// <summary>
        /// Get the formatter used to generate the log-records.
        /// Leave this null unless you wish to override it.
        /// </summary>
        ILogRecordFormatter LogRecordFormatter { get; }

        /// <summary>
        /// Get the Name - the string-literal that uniquely identifies this class of IOutputPipe.
        /// </summary>
        string Name { get; }

        #endregion properties

        #region methods

        /// <summary>
        /// Reset any resources used by this output-pipe back to their initial state.
        /// LogManager calls this method on every active IOutputPipe in it's own Clear method.
        /// </summary>
        void Clear();

        /// <summary>
        /// Write whatever logging needs to be written to the log-output text-file and close it.
        /// </summary>
        /// <param name="filenameExtension">Extension to change the filename to. Null = accept the appropriate default.</param>
        /// <remarks>
        /// You may not need to call this for most logging-output pipes.
        /// The original motivation for this was to provide a way for ETW output to be rendered
        /// from the .ETL file to a text file at the end of logging.
        /// </remarks>
        void FinalizeWriteToFile( string filenameExtension );

        /// <summary>
        /// This gets called whenever an IOutputPipe gets attached to the logging system
        /// to perform any needed initialization.
        /// </summary>
        /// <param name="logConfig">This is provided in case this output-pipe needs to affect it.</param>
        /// <remarks>
        /// This must be implemented, but if unneeded then it can just do nothing.
        /// </remarks>
        void InitializeUponAttachment( LogConfig logConfig );

        /// <summary>
        /// Given strings that are intended to represent a property-name and a value,
        /// if these apply to this output-pipe, set the appropriate configuration-value.
        /// </summary>
        /// <param name="propertyName">the name of the configuration-property to set</param>
        /// <param name="propertyValue">a string representing the value to set it to</param>
        /// <returns>true only if the given property-name matched a configuration-setting for this particular IOutputPipe implementation</returns>
        bool SetConfigurationFromText( string propertyName, string propertyValue );

        /// <summary>
        /// Set any and all configuration-settings for this output-pipe to their default values.
        /// </summary>
        void SetToDefaults();

        /// <summary>
        /// Send output to this OutputPipe.
        /// </summary>
        /// <param name="request">what to write</param>
        /// <returns>true if the request actually went out</returns>
        bool Write( LogSendRequest request );

        /// <summary>
        /// Save the configuration values to the persistent store.
        /// </summary>
        void SaveConfiguration();

        #endregion methods
    }
}
