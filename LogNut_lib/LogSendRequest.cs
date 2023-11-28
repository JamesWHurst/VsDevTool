#if PRE_4
#define PRE_5
#endif


namespace Hurst.LogNut
{
    /// <summary>
    /// This class represents a request by a Logger to transmit a LogRecord.
    /// </summary>
    public class LogSendRequest
    {
        /// <summary>
        /// Create a new LogSendRequest for the given Logger.
        /// </summary>
        /// <param name="requestingLogger">the Logger that wants to send a log-record</param>
        /// <param name="logRecord">the log-record that the logger wants to send</param>
        public LogSendRequest( Logger requestingLogger, LogRecord logRecord, bool isToSuppressTraceOutput )
        {
            //CBL Do we have a redundancy here between IsConsoleOutputRequested and IsToSuppressTraceOutput ?

            //CBL
            this.IsToSuppressTraceOutput = isToSuppressTraceOutput;
            this.Record = logRecord;
            this.LoggerName = requestingLogger.Name;

            // If this particular logger is catching Visual Studio Trace output,
            // then don't echo it to the Visual Studio output-window as, assuming that this log operation
            // is a result of Trace output, that would result in redundant writes to the output-window.
            //CBL Note however, that this also means that non-Trace logging output won't get echoed by this logger.

            //CBL This is causing my Output to the IDE not to happen. Need to fix!
            //if (requestingLogger.IsCatchingTraceOutput)
            //{
            //    this.IsConsoleOutputRequested = false;
            //}
            //else
            {
                this.IsConsoleOutputRequested = requestingLogger.IsToOutputToConsole_EffectiveValue;
            }
        }

        /// <summary>
        /// Get or set the actual log-record that this request is requesting to send out.
        /// </summary>
        public LogRecord Record { get; set; }

        /// <summary>
        /// This reflects the state of the logger's IsToOutputToConsole_Override flag when it sent this log-record.
        /// </summary>
        public bool IsConsoleOutputRequested { get; set; }

        #region IsToSuppressTraceOutput
        /// <summary>
        /// Get or set whether to prevent output to the (Console or Trace) output in Visual Studio.
        /// Default is false - to not prevent it.
        /// </summary>
        /// <remarks>
        /// This is used when the logging-output is from, for example, a WPF binding error -- which produces
        /// it's own output to the Visual Studio output-window.
        /// 
        /// This property is then used to prevent duplicate outputs to that output-window.
        /// </remarks>
        public bool IsToSuppressTraceOutput
        {
            get { return _isToSuppressTraceOutput; }
            set { _isToSuppressTraceOutput = value; }
        }

        /// <summary>
        /// This indicates whether to prevent output to the (Console or Trace) output in Visual Studio.
        /// Default is false - to not prevent it.
        /// </summary>
        private bool _isToSuppressTraceOutput;

        #endregion

        /// <summary>
        /// Get the name of the Logger that issued this log-record send request.
        /// </summary>
        public string LoggerName { get; private set; }
    }
}
