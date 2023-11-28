#if PRE_4
#define PRE_5
#endif
using System;
using System.Diagnostics;
using Hurst.LogNut.Util.Annotations;
#if !PRE_5
using System.Runtime.CompilerServices;
#endif


namespace Hurst.LogNut
{
    /// <summary>
    /// This represents a generic, platform-neutral logger
    /// </summary>
    public interface ISimpleLogger
    {
        /// <summary>
        /// Log a message at level <c>Trace</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        void LogTrace( string textToLog );

        /// <summary>
        /// Log a message at level <c>Trace</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        void LogTrace( LogCategory cat, string textToLog );

        /// <summary>
        /// Log a message at level <c>Trace</c>, the message being the given object-array and expressed using the given string format.
        /// </summary>
        /// <param name="format">the format to use to express the message (as in String.Format)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [StringFormatMethod( "format" )]
        void LogTrace( string format, params object[] args );

        /// <summary>
        /// Log a message at level <c>Debug</c>, calling the ToString method of the given object to produce the message.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        /// <returns>a reference to this object so that you can chain method-calls</returns>
        void LogDebug( string textToLog );

        /// <summary>
        /// Log a message at level <c>Debug</c>, the message being the given object-array and expressed using the given string format.
        /// </summary>
        /// <param name="format">the format to use to express the message (as in <c>String.Format</c>)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        /// <returns>a reference to this object so that you can chain method-calls</returns>
        [StringFormatMethod( "format" )]
        void LogDebug( string format, params object[] args );

        /// <summary>
        /// Log a message at level <c>Info</c>, calling the ToString method of the given object to produce the message.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        /// <returns>a reference to this object so that you can chain method-calls</returns>
        void LogInfo( string textToLog );

        /// <summary>
        /// Log a message at level <c>Info</c>, the message being the given object-array and expressed using the given string format.
        /// </summary>
        /// <param name="format">the format to use to express the message (as in String.Format)</param>
        /// <param name="args">an array of objects that represents the values to go into the Message we want to log</param>
        /// <returns>a reference to this object so that you can chain method-calls</returns>
        [StringFormatMethod( "format" )]
        void LogInfo( string format, params object[] args );

        /// <summary>
        /// Log a message at level <c>Warn</c>, calling the ToString method of the given object to produce the message.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        /// <returns>a reference to this object so that you can chain method-calls</returns>
        void LogWarning( string textToLog );

        /// <summary>
        /// Log a message at level "Warn".
        /// </summary>
        /// <param name="format">the format to use to express the message (as in String.Format)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        /// <returns>a reference to this object so that you can chain method-calls</returns>
        [StringFormatMethod( "format" )]
        void LogWarning( string format, params object[] args );

        /// <summary>
        /// Log a message at level <c>Warn</c>, the message being the given object-array and expressed using the given string format.
        /// This is a synonym for LogWarning.
        /// </summary>
        /// <param name="format">the format to use to express the message (as in String.Format)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        /// <returns>a reference to this object so that you can chain method-calls</returns>
        [StringFormatMethod( "format" )]
        void Warn( string format, params object[] args );

        /// <summary>
        /// Log a message at level <c>Error</c>, calling the ToString method of the given object to produce the message.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        /// <returns>a reference to this object so that you can chain method-calls</returns>
        void LogError( string textToLog );

        /// <summary>
        /// Log a message at level <c>Error</c>, the message being the given object-array and expressed using the given string format.
        /// </summary>
        /// <param name="format">the format to use to express the message (as in String.Format)</param>
        /// <param name="args">an array of objects that represents values to go into the message we want to log</param>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [StringFormatMethod( "format" )]
        void LogError( string format, params object[] args );

        /// <summary>
        /// Log a message at level <c>Fatal</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        void LogFatal( string textToLog );

        #region LogException

#if (!PRE_5)  // This is for .NET 4.5 / C# 5.0 and higher
        /// <summary>
        /// Log a message at level "Error" which consists of information concerning the given Exception.
        /// </summary>
        /// <param name="exception">the Exception to describe</param>
        /// <param name="additionalInformation">(optional) any text that you want to add to help describe where and why this happened</param>
        /// <param name="memberName">(in .NET 4.5+ automatically inserted by the compiler) the class-method from which this was called</param>
        /// <param name="sourceFilePath">(in .NET 4.5+ automatically inserted by the compiler) the file-system path of the source-code file from which this was called</param>
        /// <param name="sourceLineNumber">(in .NET 4.5+ automatically inserted by the compiler) the line-number within the source-code file from which this was called</param>
        void LogException( Exception exception,
                           string additionalInformation = null,
                           [CallerMemberName] string memberName = "",
                           [CallerFilePath] string sourceFilePath = "",
                           [CallerLineNumber] int sourceLineNumber = 0 );
#else
        // This is for < .NET 4.5

        /// <summary>
        /// Log a message at level "Error" which consists of information concerning the given Exception.
        /// </summary>
        /// <param name="exception">the Exception to describe</param>
        void LogException( Exception exception );

        /// <summary>
        /// Log a message at level "Error" which consists of information concerning the given Exception.
        /// </summary>
        /// <param name="exception">the Exception to describe</param>
        /// <param name="additionalInformation">any text that you want to add to help describe where and why this happened (may be null)</param>
        void LogException( Exception exception, string additionalInformation );
#endif

        /// <summary>
        /// Log a message at level "Error" which consists of information concerning the given Exception.
        /// </summary>
        /// <param name="exception">the Exception to describe</param>
        /// <param name="format">to convey additional information - this is the format-string (as in String.Format)</param>
        /// <param name="args">to convey additional information - this is an array of objects that represent values to insert into the format-string</param>
        [StringFormatMethod( "format" )]
        void LogException( Exception exception, string format, params object[] args );

        #endregion LogException
    }
}
