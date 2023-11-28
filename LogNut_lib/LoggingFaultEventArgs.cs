using System;
using System.Text;
using Hurst.LogNut.Util;


namespace Hurst.LogNut
{
    /// <summary>
    /// This subclass of EventArgs carries information to describe a file-output logging error.
    /// </summary>
    public class LoggingFaultEventArgs : EventArgs
    {
        /// <summary>
        /// Create a new LoggingFaultEventArgs instance
        /// that contains the given textual reason for it, the exception and the log-record
        /// that the system was attempting to write.
        /// </summary>
        /// <param name="userSummaryMessage">end-user oriented announcement of this error</param>
        /// <param name="reason">more detailed, developer-oriented information to display regarding this error (may be null)</param>
        /// <param name="ex">the exception that was thrown when the logging error occurred</param>
        /// <param name="record">the log-record that is the subject of this event</param>
        public LoggingFaultEventArgs( string userSummaryMessage, string reason, Exception ex, LogRecord record )
        {
            this.UserSummaryMessage = userSummaryMessage;
            this.Reason = reason;
            this.ExceptionThatWasRaised = ex;
            this.Record = record;
        }

        /// <summary>
        /// Create a new LoggingFaultEventArgs instance
        /// that contains the given textual reason for it, the exception and the log-record
        /// that the system was attempting to write.
        /// </summary>
        /// <param name="userSummaryMessage">end-user oriented announcement of this error</param>
        /// <param name="reason">more detailed, developer-oriented information to display regarding this error (may be null)</param>
        /// <param name="exception">the exception that was thrown when the logging error occurred</param>
        /// <param name="record">the log-record that is the subject of this event</param>
        /// <param name="isRedirected">whether this involved redirecting the file output (successfully) to an alternative file</param>
        /// <param name="pathRedirectedTo">the filesystem-pathname that the log-record was written to instead of the configured path</param>
        public LoggingFaultEventArgs( string userSummaryMessage, string reason, Exception exception, LogRecord record, bool isRedirected, string pathRedirectedTo )
        {
            this.UserSummaryMessage = userSummaryMessage;
            this.Reason = reason;
            this.ExceptionThatWasRaised = exception;
            this.Record = record;
            this.IsRedirected = isRedirected;
            this.PathRedirectedTo = pathRedirectedTo;
        }

        /// <summary>
        /// Get or set an end-user-oriented plain-English-language announcement of a logging fault.
        /// </summary>
        public string UserSummaryMessage { get; set; }

        /// <summary>
        /// Get or set a developer-oriented plain-English-language description of what happened.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Get or set the exception that was thrown when this logging error occurred.
        /// </summary>
        public Exception ExceptionThatWasRaised { get; set; }

        /// <summary>
        /// Get or set the log-record that was to be written, that is the subject of this event.
        /// </summary>
        public LogRecord Record { get; set; }

        /// <summary>
        /// Get or set whether this fault-condition involved redirecting the file output (successfully) to an alternative file
        /// within the same folder.
        /// </summary>
        public bool IsRedirected { get; set; }

        /// <summary>
        /// Get or set the filesystem-pathname that the log-record was written to instead of the configured path,
        /// if that happened. Only valid if IsRedirected is true.
        /// </summary>
        public string PathRedirectedTo { get; set; }

        /// <summary>
        /// Return a simple summary text message composed from the state of this object
        /// such as would be suitable to announce to the end-user that something happened.
        /// </summary>
        /// <returns>a string with an English-language composed message</returns>
        /// <remarks>
        /// If no <see cref="UserSummaryMessage"/> is supplied, then the text "A logging-output fault has occurred"
        /// is provided in it's place.
        /// </remarks>
        public string GetEndUserSummaryMessage()
        {
            var sb = new StringBuilder();

            // Add the announcement.
            if (UserSummaryMessage != null)
            {
                sb.Append( UserSummaryMessage );
            }
            else
            {
                sb.Append( "A logging-output fault has occurred." );
            }

            // Identify the exception, if applicable.
            if (ExceptionThatWasRaised != null)
            {
                // Only list the exception explicitly, if it is not already mentioned within Reason.
                string exceptionName = StringLib.ExceptionNameShortened( ExceptionThatWasRaised );
                if (Reason == null || !Reason.Contains( exceptionName ))
                {
                    sb.AppendLine();
                    sb.AppendFormat( "{0} was raised.", StringLib.ExceptionNameShortened( ExceptionThatWasRaised ) );
                }
            }

            // If we put it somewhere else - let a brothah know.
            if (IsRedirected && PathRedirectedTo != null)
            {
                sb.AppendLine();
                sb.AppendFormat( "The log output was redirected to {0}", PathRedirectedTo );
            }
            return sb.ToString();
        }

        /// <summary>
        /// Return a text message composed from the state of this object
        /// that conveys detailed developer-oriented information.
        /// This includes the text returned by <see cref="GetEndUserSummaryMessage"/>.
        /// </summary>
        /// <returns>a string with an English-language composed message</returns>
        public string GetDetailedDeveloperMessage()
        {
            var sb = new StringBuilder();
            if (Reason != null)
            {
                sb.Append( Reason );
            }
            if (Record != null && Record.Level >= LogLevel.Infomation && StringLib.HasSomething( Record.Message ))
            {
                if (Reason != null)
                {
                    sb.AppendLine();
                }
                sb.Append( "Attempted to log: " );
                sb.Append( StringLib.Shortened( Record.Message, 40 ) );
            }
            return sb.ToString();
        }

        /// <summary>
        /// Return a text message that denotes the state of this object
        /// that contains both the text returned by <see cref="GetEndUserSummaryMessage"/>,
        /// and that returned by <see cref="GetDetailedDeveloperMessage"/> separated by a line containing four dashes.
        /// </summary>
        /// <returns>a string with an English-language composed message</returns>
        /// <remarks>
        /// If no <see cref="UserSummaryMessage"/> is supplied, then the text "A logging-output fault has occurred"
        /// is provided in it's place.
        /// </remarks>
        public string GetUserDetailedMessage()
        {
            var sb = new StringBuilder( this.GetEndUserSummaryMessage() );
            string detailedMessage = GetDetailedDeveloperMessage();
            if (StringLib.HasSomething( detailedMessage ))
            {
                sb.AppendLine();
                sb.Append( "- - - -" );
                sb.AppendLine();
                sb.Append( detailedMessage );
            }
            return sb.ToString();
        }

        /// <summary>
        /// Override the <c>ToString</c> method to provide a more informative denotation of the state of this object.
        /// </summary>
        /// <returns>a string that concisely denotes the properties</returns>
        public override string ToString()
        {
            var sb = new StringBuilder( "LoggingFaultEventArgs(" );
            if (IsRedirected)
            {
                sb.Append( "IsRedirected," );
                if (StringLib.HasSomething( PathRedirectedTo ))
                {
                    sb.AppendFormat( " to {0}, ", PathRedirectedTo );
                }
            }
            if (StringLib.HasSomething( this.UserSummaryMessage ))
            {
                sb.Append( "UserSummaryMessage = " ).Append( this.UserSummaryMessage ).Append( ", " );
            }
            if (StringLib.HasSomething( this.Reason ))
            {
                sb.Append( "Reason = " ).Append( this.Reason ).Append( ", " );
            }
            if (ExceptionThatWasRaised != null)
            {
                sb.Append( "ExceptionThatWasLogged = " ).Append( ExceptionThatWasRaised ).Append( ", " );
            }
            sb.Append( "Record = " ).Append( this.Record ).Append( " )" );
            return sb.ToString();
        }
    }
}
