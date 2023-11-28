using System;
#if !NETFX_CORE
using System.Runtime.Serialization;
#endif
using Hurst.LogNut.Util;


namespace Hurst.LogNut
{
    /// <summary>
    /// This type of Exception indicates an error occurred within the LogNut logging facility.
    /// </summary>
#if !NETFX_CORE
    [Serializable]
#endif
    public class LoggingException : Exception
    {
        /// <summary>
        /// Create a new LoggingException.
        /// </summary>
        public LoggingException() : base() { }

        /// <summary>
        /// Create a new LoggingException with the given (arbitrary) descriptive text.
        /// </summary>
        /// <param name="message">a helpful bit of text to include with the exception</param>
        public LoggingException( string message )
            : base( message )
        { }

        /// <summary>
        /// Create a new LoggingException with some descriptive text that is composed from
        /// the given format string together with the arguments array, which are combined using String.Format.
        /// </summary>
        /// <param name="format">the format-string as used within String.Format</param>
        /// <param name="args">the arguments to the format-string</param>
        public LoggingException( string format, params object[] args )
            : base( string.Format( format, args ) )
        { }

        /// <summary>
        /// Create a new LoggingException with the given text as message,
        /// and the given exception to include as the inner-exception.
        /// </summary>
        /// <param name="message">a description of what happened</param>
        /// <param name="innerException">the exception that is to be included within this LoggingException</param>
        public LoggingException( string message, Exception innerException )
            : base( message, innerException )
        { }

        /// <summary>
        /// Create a new LoggingException with the given text as message,
        /// and the given original-log-message, to describe the operation that was being attempted,
        /// and the given exception to include as the inner-exception.
        /// </summary>
        /// <param name="message">a description of what happened</param>
        /// <param name="level">the LogLevel that was specified to be logged</param>
        /// <param name="originalLogMessage">the message that was specified to be logged</param>
        /// <param name="innerException">the exception that is to be included within this LoggingException</param>
        public LoggingException( string message, LogLevel level, string originalLogMessage, Exception innerException )
            : base( message, innerException )
        {
            base.Data.Add( "level", level );
            base.Data.Add( "logMessage", StringLib.AsString( originalLogMessage ) );
        }

#if !NETFX_CORE
        /// <summary>
        /// Create a new LoggingException based upon the given SerializationInfo and StreamingContext.
        /// </summary>
        /// <param name="info">this contains all the info needed to serialize this object</param>
        /// <param name="context">this defines the source and destination of the serialized stream</param>
        protected LoggingException( SerializationInfo info, StreamingContext context )
            : base( info, context )
        { }
#endif
    }
}
