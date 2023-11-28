
namespace Hurst.LogNut
{
    #region enum types used within LogNut

    /// <summary>
    /// This is a (coarse) way to prioritize the logging output, reflecting it's level of urgency or purpose.
    /// The default value is Trace.
    /// </summary>
    public enum LogLevel
    {
        // This had been named LogLevel, however there is a Microsoft.Extensions.Logging.LogLevel and that caused misleading compiler errors whenever BaseLib was not present.
        // 2023/7/16  I see that Microsoft's LogLevel has a None value, for no logging.
        //   I am intending this to (tentatively, if it doesn't bastardize this library excessively) be compatible with
        //   Microsoft.Extensions.Logging, and Microsoft's ILogger.
        //   see  https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel?view=dotnet-plat-ext-7.0


        /// <summary>
        /// This level is associated with verbose logging that is specifically for temporary, development and debugging purposes
        /// (lowest, default level - integer value 0).
        /// </summary>
        Trace = 0,
        /// <summary>
        /// This level is associated with logging that is specifically for debugging purposes, but not so verbose as Trace.
        /// (2nd urgency level - integer value 1).
        /// </summary>
        Debug = 1,
        /// <summary>
        /// This level represents logging just for informational purposes.
        /// (3rd urgency level - integer value 3).
        /// </summary>
        Infomation = 2,
        /// <summary>
        /// This level represents a level of urgency that is not necessarily an error, but does merit a warning
        /// (4th urgency level - integer value 3).
        /// </summary>
        Warning = 3,
        /// <summary>
        /// This level represents a definite program or operation error, but with a level of urgency that is less than that of Critical
        /// (5th urgency level - integer value 4).
        /// </summary>
        Error = 4,
        /// <summary>
        /// This level represents a fatal program or system condition from which the software cannot automatically recover
        /// (6th and highest urgency level - integer value 5).
        /// </summary>
        Critical = 5,
        /// <summary>
        /// This is used to specify that a logging category shouldn't write messages.
        /// </summary>
        None = 6
    }

    public enum ServiceInstalledState
    {
        /// <summary>
        /// The state of the LogNut Windows Service is not known or even whether it has been installed.
        /// </summary>
        Unknown,
        /// <summary>
        /// The LogNut Windows Service is not installed.
        /// </summary>
        NotInstalled,
        /// <summary>
        /// The LogNut Windows Service is installed on this host.
        /// </summary>
        Installed
    }

    /// <summary>
    /// This denotes how a log-record is sent or received. The TraceReceived event has this as a property to indicate how the log came in.
    /// </summary>
    public enum LogTransmissionMethod
    {
        /// <summary>
        /// The log-record is written to a database
        /// </summary>
        Database,
        /// <summary>
        /// The log-record is generated directly, as when testing a log-viewer
        /// </summary>
        Internal,
        /// <summary>
        /// Message-Queuing is used, on the local host
        /// </summary>
        MessageQueuingLocal,
        /// <summary>
        /// Message-Queuing is used, in communication with another host on the Local-Area-Network (LAN)
        /// </summary>
        MessageQueuingLan,
        /// <summary>
        /// Message-Queuing is used, in communication with a central LogNut server
        /// </summary>
        MessageQueuingServer,
        /// <summary>
        /// The SignalR transmission-method is used
        /// </summary>
        SignalR,
        /// <summary>
        /// The log-record is written to the Windows Event Log
        /// </summary>
        WindowsEventLog,
        /// <summary>
        /// The log-record is written to a file
        /// </summary>
        WritingToFile
    }

    /// <summary>
    /// This denotes the scope, or the extensive-ness, of communication between systems that is used. Default is LocalBox.
    /// </summary>
    public enum IpcScope
    {
        /// <summary>
        /// Log output is only to the local host, and any IPC is with other processes running on this same host.
        /// </summary>
        LocalBox,
        /// <summary>
        /// Log output may be transmitted to any TCP/IP-reachable host on the LAN.
        /// </summary>
        LocalAreaNetwork,
        /// <summary>
        /// Log output is sent to the LogNut server, from whence it may be retrieved by any connected viewer.
        /// </summary>
        Server
    }

    /// <summary>
    /// This denotes the type of communication when IPC is used. Default is None.
    /// </summary>
    public enum IpcType
    {
        /// <summary>
        /// (the default) No IPC is used - log output is simply written to the file-system or one of the other destinations
        /// where there is no need to consider other host systems.
        /// </summary>
        None,
        /// <summary>
        /// Log output is sent using a Message Queue to transport it to its destination.
        /// </summary>
        MessageQueuing,
        /// <summary>
        /// Log output is transmitted using SignalR as the communication protocol.
        /// </summary>
        SignalR
    }

    /// <summary>
    /// This denotes the basic format choice for persisting log records to a file - SimpleText, Xml, or Json.
    /// The default is SimpleText.
    /// </summary>
    public enum LogFileFormatType
    {
        /// <summary>
        /// Log-records are written in the basic textual format with a simple header for meta-information (the default).
        /// </summary>
        SimpleText,
        /// <summary>
        /// Log-records are in the standard XML format.
        /// </summary>
        Xml,
        /// <summary>
        /// Log-records are written in JSON format.
        /// </summary>
        Json
    }

    /// <summary>
    /// Values of this type denote the basis on which logging output file is to be rolled over to a new file.
    /// Can be Size (the default), Date, or Composite (meaning both).
    /// This only applies to file output.
    /// </summary>
    public enum RolloverMode
    {
        /// <summary>
        /// Roll files based only on the size of the file (the default).
        /// </summary>
        Size = 0,
        /// <summary>
        /// Roll files based only on the date of the file.
        /// </summary>
        Date,
        /// <summary>
        /// Roll files based on both the size and the date of the file.
        /// </summary>
        Composite
    }

    /// <summary>
    /// Values of this type denote the frequency with which the logging output is rolled over to a new file,
    /// if the RolloverMode is either Date or Composite.
    /// Can be NoneSpecified, TopOfHour, TopOfDay (the default), TopOfWeek, or TopOfMonth.
    /// </summary>
    public enum RollPoint
    {
        /// <summary>
        /// Roll the log NOT based on the date/time (as, for example, when based on size).
        /// </summary>
        NoneSpecified = -2,
        /// <summary>
        /// Roll the log at the start of each hour.
        /// </summary>
        TopOfHour = -1,
        /// <summary>
        /// Roll the log at midnight of each day (this is the default value).
        /// </summary>
        TopOfDay = 0,
        /// <summary>
        /// Roll the log at each week.
        /// </summary>
        TopOfWeek = 1,
        /// <summary>
        /// Roll the log each month.
        /// </summary>
        TopOfMonth
    }
    #endregion
}
