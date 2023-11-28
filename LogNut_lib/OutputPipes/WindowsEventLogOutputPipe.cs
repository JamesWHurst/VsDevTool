using System;
using System.Diagnostics;
using Hurst.LogNut.Util;


namespace Hurst.LogNut.OutputPipes
{
    /// <summary>
    /// This is an OutputPipe that outputs log-records to
    /// the Windows Event Log.
    /// </summary>
    public class WindowsEventLogOutputPipe : IOutputPipe
    {
        #region public properties

        #region IsEnabled
        /// <summary>
        /// Get or set whether output to this OutputPipe is enabled. This defaults to false.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Enable logging output to this output.
        /// Calling this is the same as setting the IsEnabled property to true.
        /// </summary>
        /// <remarks>
        /// This duplicates the function of the IsEnabled property setter, in order to provide a fluent API.
        /// </remarks>
        /// <returns>a reference to this WindowsEventLogOutputPipe object, such that further method calls may be chained</returns>
        public WindowsEventLogOutputPipe Enable()
        {
            this.IsEnabled = true;
            return this;
        }

        /// <summary>
        /// Turn off logging output to this output.
        /// Calling this is the same as setting the IsEnabled property to false.
        /// </summary>
        /// <remarks>
        /// This duplicates the function of the IsEnabled property setter, in order to provide a fluent API.
        /// </remarks>
        /// <returns>a reference to this WindowsEventLogOutputPipe object, such that further method calls may be chained</returns>
        public WindowsEventLogOutputPipe Disable()
        {
            this.IsEnabled = false;
            return this;
        }
        #endregion

        #region IsFailing
        /// <summary>
        /// Get or set whether output to this OutputPipe is has been failing.
        /// This is used to avoid repeatedly trying to send a log over a non-working channel.
        /// </summary>
        public bool IsFailing { get; set; }
        #endregion

        #region LogRecordFormatter
        /// <summary>
        /// Get the formatter used to generate the log-records.
        /// Leave this null unless you wish to override it.
        /// </summary>
        public ILogRecordFormatter LogRecordFormatter
        {
            get
            {
                return LogManager.LogRecordFormatter;
            }
        }
        #endregion

        #region Name
        /// <summary>
        /// Get the Name - the string-literal that uniquely identifies this class of IOutputPipe.
        /// </summary>
        public string Name
        {
            get { return "WindowsEventLog"; }
        }
        #endregion

        #region SystemEventLogEventId
        /// <summary>
        /// Get or set the Event-ID to use when writing to the Windows Event Log.
        /// This would be what appears under the "Event ID" column when viewing the Windows Event Log. This is zero by default.
        /// </summary>
        public int SystemEventLogEventId { get; set; }

        /// <summary>
        /// Set the Event-ID to use when writing to the Windows Event Log.
        /// Calling this is the same as setting the SystemEventLogEventId property.
        /// </summary>
        /// <param name="eventId">the integer value to use for the Event-ID</param>
        /// <remarks>
        /// If you don't set this, 0 (zero) is used by default.
        /// This duplicates the function of the SystemEventLogEventID property setter, in order to provide a fluent API.
        /// </remarks>
        /// <returns>a reference to this WindowsEventLogOutputPipe object, such that further method calls may be chained</returns>
        public WindowsEventLogOutputPipe SetSystemEventLogEventId( int eventId )
        {
            SystemEventLogEventId = eventId;
            return this;
        }
        #endregion

        #region SystemEventLogSourceName
        /// <summary>
        /// Get or set what to use for the "Source" when writing to the Windows Event Log, if that is enabled.
        /// </summary>
        /// <remarks>
        /// If you do not set this explicitly, then by default, this will be of the form "LogNut" plus:
        /// if the subject-program is an IDesktopApplication, a dash and the ProductIdentifierPrefix,
        /// if not an IDesktopApplication but there is a value for the LogManager.SubjectProgramName property, then "LogNut-" + SubjectProgramName.
        /// otherwise, just "LogNut".
        /// </remarks>
        public string SystemEventLogSourceName
        {
            get
            {
                if (String.IsNullOrEmpty( _systemEventLogSourceName ))
                {
                    string subjectProgramName = LogManager.Config.GetSubjectProgramName( typeof( LogManager ) );
                    if (StringLib.HasSomething( subjectProgramName ))
                    {
                        _systemEventLogSourceName = "LogNut-" + subjectProgramName;
                    }
                    else
                    {
                        _systemEventLogSourceName = "LogNut";
                    }
                }
                return _systemEventLogSourceName;
            }
            set
            {
                _systemEventLogSourceName = value;
                // If assigning this property back to null, then set the flag that indicates that reading this property will yield the default value.
                // If setting to anything else, clear this flag so that other code can know that a non-default value has been set.
                IsSystemEventLogSourceNameTheDefaultValue = (value == null);
            }
        }

        /// <summary>
        /// Set what to use for the "Source" when writing to the Windows Event Log, if that is enabled.
        /// Calling this is the same as setting the SystemEventLogSourceName property.
        /// </summary>
        /// <param name="sourceName">the string value to use for the "Source" column of the Windows Event Log</param>
        /// <remarks>
        /// This duplicates the function of the SystemEventLogSourceName property setter, in order to provide a fluent API.
        /// </remarks>
        /// <returns>a reference to this LogConfig object, such that further method calls may be chained</returns>
        public WindowsEventLogOutputPipe SetSystemEventLogSourceName( string sourceName )
        {
            SystemEventLogSourceName = sourceName;
            return this;
        }
        #endregion

        #endregion public properties

        #region public methods

        #region Clear
        /// <summary>
        /// Reset any resources used by this output-pipe back to their initial state.
        /// </summary>
        public void Clear()
        {

        }
        #endregion

        #region FinalizeWriteToFile
        /// <summary>
        /// Write whatever logging needs to be written to the log-output text-file and close it.
        /// In the case of this WindowsEventLogOutputPipe, this does nothing.
        /// </summary>
        /// <param name="filenameExtension">Extension to change the filename to. Null = accept the appropriate default.</param>
        /// <remarks>
        /// You may not need to call this for most logging-output pipes.
        /// The original motivation for this was to provide a way for ETW output to be rendered
        /// from the .ETL file to a text file at the end of logging.
        /// </remarks>
        public void FinalizeWriteToFile( string filenameExtension )
        {
            // NOP
        }
        #endregion

        #region InitializeUponAttachment
        /// <summary>
        /// This gets called whenever an instance of this WindowsEventLogOutputPipe gets attached to the logging system
        /// to perform any needed initialization.
        /// </summary>
        /// <param name="logConfig">This is provided in case this output-pipe needs to affect it.</param>
        public void InitializeUponAttachment( LogConfig logConfig )
        {
        }
        #endregion

        #region SaveConfiguration
        /// <summary>
        /// Save the configuration values to the persistent store.
        /// </summary>
        public void SaveConfiguration()
        {
            // CBL Implement
        }
        #endregion

        #region SetConfigurationFromText
        /// <summary>
        /// Given strings that are intended to represent a property-name and a value,
        /// if these apply to this output-pipe, set the appropriate configuration-value.
        /// </summary>
        /// <param name="propertyName">the name of the configuration-property to set</param>
        /// <param name="propertyValue">a string representing the value to set it to</param>
        /// <returns>true only if the given property-name matched a configuration-setting for this particular IOutputPipe implementation</returns>
        public bool SetConfigurationFromText( string propertyName, string propertyValue )
        {
            bool ok = false;

            return ok;
        }
        #endregion

        #region SetToDefaults
        /// <summary>
        /// Set any and all configuration-settings for this output-pipe to their default values.
        /// </summary>
        public void SetToDefaults()
        {
            IsFailing = false;
        }
        #endregion

        #region Write
        /// <summary>
        /// Send output to the Windows Event Log.
        /// </summary>
        /// <param name="request">what to write</param>
        /// <returns>true if the request actually went out</returns>
        public bool Write( LogSendRequest request )
        {
            bool wentOut = false;
            //WindowsEventLogConfiguration config = (WindowsEventLogConfiguration)this.Config;
            //TODO
#if !SILVERLIGHT
            // Attend to the output to the Windows Event Log, if we're doing that...
            if (this.IsEnabled)
            {
                // Map the LogRecordType to an EventLogEntryType, as best we can.
                System.Diagnostics.EventLogEntryType eventLogEntryType;
                switch (request.Record.Level)
                {
                    case LogLevel.Error:
                    case LogLevel.Critical:
                        eventLogEntryType = EventLogEntryType.Error;
                        break;
                    case LogLevel.Warning:
                        eventLogEntryType = EventLogEntryType.Warning;
                        break;
                    default:
                        eventLogEntryType = EventLogEntryType.Information;
                        break;
                }
                string sourceName = this.SystemEventLogSourceName;
                if (!System.Diagnostics.EventLog.SourceExists( sourceName ))
                {
                    System.Diagnostics.EventLog.CreateEventSource( sourceName, "Application" );
                }
                using (var myEventLog = new System.Diagnostics.EventLog())
                {
                    //CBL  What about these other properties?
                    //myEventLog.Log = "";
                    //myEventLog.LogDisplayName
                    //myEventLog.MachineName
                    //myEventLog.MaximumKilobytes
                    //myEventLog.MinimumRetentionDays
                    //myEventLog.ModifyOverflowPolicy(OverflowAction.OverwriteOlder, 30);
                    //myEventLog.WriteEntry(message: "", type: EventLogEntryType.Error, eventID: 0, category: 0);
                    myEventLog.Source = sourceName;
                    //CBL  Adjust the LogConfig to be appropriate for this.
                    string logText = request.Record.AsText( null );
                    //string logText = request.Record.AsText(ContentConfiguration.ForEventLog);

                    myEventLog.WriteEntry( logText, eventLogEntryType, this.SystemEventLogEventId );
                    wentOut = true;
                }
            }
#endif
            return wentOut;
        }
        #endregion Write

        #endregion public methods

        #region internal implementation

        #region ForEventLog
        /// <summary>
        /// Get an instance of a LogConfig that has its properties set
        /// appropriately for output to the Windows Event Log.
        /// </summary>
        private static LogConfig ForEventLog
        {
            get
            {
                //CBL Not even used?
                if (_instanceForEventLog == null)
                {
                    _instanceForEventLog = new LogConfig();
                    _instanceForEventLog.IsToShowCategory = true;
                    _instanceForEventLog.IsToShowFractionsOfASecond = false;
                    _instanceForEventLog.IsToShowLevel = true;
                    _instanceForEventLog.IsToShowLoggerName = false;
                    _instanceForEventLog.IsToShowPrefix = true;
                    _instanceForEventLog.IsToShowSourceHost = false;
                    _instanceForEventLog.IsToShowStackTraceForExceptions = false;
                    _instanceForEventLog.IsToShowSubjectProgram = true;
                    _instanceForEventLog.IsToShowSubjectProgramVersion = false;
                    _instanceForEventLog.IsToShowThread = false;
                    _instanceForEventLog.IsToShowTimestamp = false;
                    _instanceForEventLog.IsToShowUser = false;
                }
                return _instanceForEventLog;
            }
        }
        #endregion

        #region fields

        /// <summary>
        /// This is a LogConfig that has properties set appropriately
        /// for writing to the Windows Event-Log.
        /// </summary>
        private static LogConfig _instanceForEventLog;

        /// <summary>
        /// This flag is provided so that other classes may know that the SystemEventLogSourceName
        /// property has been explicitly set to some value other than null, by calling it's setter.
        /// </summary>
        internal bool IsSystemEventLogSourceNameTheDefaultValue = true;

        /// <summary>
        /// This caches the text to use for the "Source" when writing to the Windows Event Log, if that is enabled.
        /// If this is left null then it derives a source name based upon the application name.
        /// </summary>
        private string _systemEventLogSourceName;

        #endregion fields

        #endregion internal implementation
    }
}
