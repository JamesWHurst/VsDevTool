using System;
using System.Text;
using Hurst.LogNut.Util;


namespace Hurst.LogNut
{
    /// <summary>
    /// This subclass of EventArgs carries information to describe
    /// the act of logging something.
    /// It is intended for use as when simply counting the logs that are going out.
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        /// <summary>
        /// Create a new LogEventArgs instance
        /// that references the log-record that was sent out to the logging facility.
        /// </summary>
        /// <param name="record">the log-record that is the subject of this event</param>
        /// <remarks>
        /// This constructor leaves the property <see cref="ExceptionThatWasLogged"/> as null. This class is not
        /// intended to be only for exceptions or errors: it announces simply that something is being logged
        /// and no exception need be involved.
        /// </remarks>
        public LogEventArgs( LogRecord record )
        {
            this.Record = record;
            if (record != null)
            {
                string loggerName = record.SourceLogger;
                if (StringLib.HasNothing( loggerName ) || loggerName.Equals( LogManager.NameOfDefaultLogger ))
                {
                    this.LoggerName = null;
                }
                else
                {
                    this.LoggerName = loggerName;
                }
            }
        }

        /// <summary>
        /// Create a new LogEventArgs instance that denotes a call to LogError was made.
        /// If an additional-information argument was provided then use a different overload of this ctor.
        /// </summary>
        /// <param name="loggerName">the name of the Logger object that produced the logging output</param>
        /// <param name="exception">the exception gave rise to this thing being logged (may be null)</param>
        public LogEventArgs( string loggerName, Exception exception )
        {
            if (StringLib.HasSomething( loggerName ))
            {
                this.LoggerName = loggerName;
            }
            this.ExceptionThatWasLogged = exception;
        }

        /// <summary>
        /// Create a new LogEventArgs instance that denotes a call to LogError was made.
        /// </summary>
        /// <param name="loggerName">the name of the Logger object that produced the logging output</param>
        /// <param name="exception">the exception gave rise to this thing being logged (may be null)</param>
        /// <param name="additionalInformation">the value that was provided for the additionalInformation parameter (may be null)</param>
        public LogEventArgs( string loggerName, Exception exception, string additionalInformation )
        {
            if (StringLib.HasSomething( loggerName ))
            {
                this.LoggerName = loggerName;
            }
            this.ExceptionThatWasLogged = exception;
            this.ExceptionAdditionalInformation = additionalInformation;
        }

        /// <summary>
        /// Get or set the exception that was indicated by the logging output.
        /// </summary>
        public Exception ExceptionThatWasLogged { get; set; }

        /// <summary>
        /// Get or set the value that was provided for the additionalInformation argument when an exception was logged.
        /// This is null if this event did not involve an exception or nothing was provided for this.
        /// </summary>
        public string ExceptionAdditionalInformation { get; set; }

        /// <summary>
        /// Get the name of the Logger that produced this logging output.
        /// If it is unknown or was the default logger than this is null.
        /// </summary>
        public string LoggerName { get; private set; }

        /// <summary>
        /// Get a snippet of the text that was logged, or null if not applicable.
        /// </summary>
        public string MessageShortened
        {
            get
            {
                if (this.Record == null || Record.Message == null)
                {
                    return null;
                }
                return StringLib.Shortened( Record.Message, 32 );
            }
        }

        /// <summary>
        /// Get or set the log-record that was to be written, that is the subject of this event.
        /// </summary>
        public LogRecord Record { get; set; }

        /// <summary>
        /// Get whether the event that was logged consisted of a C# exception.
        /// This returns true if ExceptionThatWsRaised is non-null.
        /// </summary>
        public bool WasAnException
        {
            get { return ExceptionThatWasLogged != null; }
        }

        #region ToString
        /// <summary>
        /// Override the <c>ToString</c> method to say something informative about this object.
        /// </summary>
        /// <returns>a string that concisely denotes the properties</returns>
        public override string ToString()
        {
            var sb = new StringBuilder("LogEventArgs(");
            bool hasOutputYet = false;
            if (StringLib.HasSomething( this.LoggerName ))
            {
                sb.Append( "LoggerName = " ).Append( this.LoggerName );
                hasOutputYet = true;
            }
            if (ExceptionThatWasLogged != null)
            {
                if (hasOutputYet)
                {
                    sb.Append( ", " );
                }
                sb.Append( "ExceptionThatWasLogged: " ).Append( ExceptionThatWasLogged ).Append( ", " );
            }
            sb.Append( "Record: " ).Append( this.Record );
            if (StringLib.HasSomething( this.ExceptionAdditionalInformation ))
            {
                if (hasOutputYet)
                {
                    sb.Append( ", " );
                }
                sb.Append( ", AdditionalInformation: " ).Append( this.ExceptionAdditionalInformation );
            }
            if (StringLib.HasSomething( this.MessageShortened ))
            {
                if (hasOutputYet)
                {
                    sb.Append( ", " );
                }
                sb.Append( ", MessageShortened: " ).Append( this.MessageShortened );
            }
            return sb.ToString();
        }
        #endregion ToString
    }
}
