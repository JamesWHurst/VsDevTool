using System;
using System.Diagnostics;
#if !PRE_5
using System.Runtime.CompilerServices;
#endif
using Hurst.LogNut.Util.Annotations;


// The actual writing-to-file happens within the method LogManager.Send


namespace Hurst.LogNut
{
    /// <summary>
    /// This is a ILognutLogger implementation that does nothing. Useful for unit-tests.
    /// </summary>
    public class LoggerDoNothing : ILognutLogger
    {
        public LoggerDoNothing()
        { }

        #region Name
        /// <summary>
        /// Get the name of this Logger object. By default it is an empty string.
        /// </summary>
        public string Name
        {
            // I made this not settable, because LogManager saves a list of all the loggers and accesses them by name.
            get
            {
                return String.Empty;
            }
        }
        #endregion

        #region logging-output methods

        //CBL  Okay - so I don't need all of these: Log, and LogString.  merge this shit!

        #region Log
        /// <summary>
        /// Does nothing.
        /// This is one of two methods that all logging-output methods call.
        /// </summary>
        /// <param name="level">the LogLevel at which to log this</param>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string comprising the message to log</param>
        /// <exception cref="LoggingException">all Exceptions are rethrown as a <see cref="LoggingException"/>.</exception>
        /// <remarks>
        /// Parameter isToSuppressTraceOutput is included for the case wherein VisualStudioLib.Write, or Flush, is writing Visual Studio
        /// trace-output, and we don't want to write it to the Output window twice.
        /// </remarks>
        public void Log( LogLevel level, LogCategory cat, string textToLog, bool isToSuppressTraceOutput )
        {
        }

        /// <summary>
        /// Output a log-record containing the given log-message (whatToLog), at the given log-level.
        /// This is the 2nd of two methods that all logging-output methods call.
        /// </summary>
        /// <param name="level">the <see cref="LogLevel"/> at which to log this</param>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="format">the format to use to express the message (as in String.Format)</param>
        /// <param name="args">an array of objects that represents values to go into the message we want to log</param>
        /// <exception cref="LoggingException">all Exceptions are rethrown as a <see cref="LoggingException"/></exception>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [StringFormatMethod( "format" )]
        public void LogStringFormat( LogLevel level, LogCategory cat, string format, params object[] args )
        {
        }

        /// <summary>
        /// Output a log-record containing the given log-message (textToLog), at log-level Infomation.
        /// This is redundant -- it's simply an ultra-simplification for when you don't care about the level and want to log a simple string.
        /// </summary>
        /// <param name="textToLog">the string comprising the message to log</param>
        /// <exception cref="LoggingException">all Exceptions are rethrown as a <see cref="LoggingException"/></exception>
        public void Log( string textToLog )
        {
        }
        #endregion Log

        #region LogTrace, LogTraceWithContext
        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogTrace( string textToLog )
        {
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogTrace( LogCategory cat, string textToLog )
        {
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="catEnum">an enumeration-type value that specifies which category</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        [Conditional( "TRACE" )]
        public void LogTrace( System.Enum catEnum, string textToLog )
        {
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="format">the format to use to express the message (as in String.Format)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [StringFormatMethod( "format" )]
        public void LogTrace( string format, params object[] args )
        {
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="format">the format to use to express the message (as in String.Format)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [Conditional( "TRACE" )]
        [StringFormatMethod( "format" )]
        public void LogTrace( LogCategory cat, string format, params object[] args )
        {
        }

#if !PRE_5
        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        /// <param name="memberName">(in .NET 4.5+ automatically inserted by the compiler) the class-method from which this was called.</param>
        /// <param name="sourceFilePath">(in .NET 4.5+ automatically inserted by the compiler) the file-system path of the source-code file from which this was called</param>
        /// <param name="sourceLineNumber">(in .NET 4.5+ automatically inserted by the compiler) the line-number within the source-code file from which this was called</param>
        /// <returns>a reference to this object so that you can chain method-calls</returns>
        /// <example>
        /// This logs the given text-message and also the file, class-member, and line-number at which this call is made.
        /// <code>
        /// LogTraceWithContext("An unexpected visitor came unto my door.");
        /// </code>
        /// You only provide the value for textToLog; the compiler inserts all of the other values.
        /// In this way, you automatically get information written to the log that identifies exactly where this was called.
        /// </example>
        [Conditional( "TRACE" )]
        public void LogTraceWithContext( string textToLog,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string sourceFilePath = "",
                                         [CallerLineNumber] int sourceLineNumber = 0 )
        {
        }

        /// <summary>
        /// Does nothing.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        /// <param name="memberName">(in .NET 4.5+ automatically inserted by the compiler) the class-method from which this was called.</param>
        /// <param name="sourceFilePath">(in .NET 4.5+ automatically inserted by the compiler) the file-system path of the source-code file from which this was called</param>
        /// <param name="sourceLineNumber">(in .NET 4.5+ automatically inserted by the compiler) the line-number within the source-code file from which this was called</param>
        /// <returns>a reference to this object so that you can chain method-calls</returns>
        /// <example>
        /// This logs the given text-message and also the file, class-member, and line-number at which this call is made.
        /// <code>
        /// LogTraceWithContext("An unexpected visitor came unto my door.");
        /// </code>
        /// You only provide the value for textToLog; the compiler inserts all of the other values.
        /// In this way, you automatically get information written to the log that identifies exactly where this was called.
        /// </example>
        [Conditional( "TRACE" )]
        public void LogTraceWithContext( LogCategory cat,
                                         string textToLog,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string sourceFilePath = "",
                                         [CallerLineNumber] int sourceLineNumber = 0 )
        {
        }
#else
        /// <summary>
        /// Log a message at level <c>Trace</c>, and add information that denotes it's location within the source-code including the calling method, source-file path and line-number.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        /// <remarks>
        /// This adds to the normal LogTrace output information of the form:
        /// "method {method-name}, source-file: {pathname of source-code file}, line-number: {the line-number within that source-code file}".
        /// </remarks>
        [Conditional( "TRACE" )]
        public void LogTraceWithContext( string textToLog )
        {
        }

        /// <summary>
        /// Log a message at level <c>Trace</c>, and add information that denotes it's location within the source-code including the calling method, source-file path and line-number.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        /// <remarks>
        /// This adds to the normal LogTrace output information of the form:
        /// "method {method-name}, source-file: {pathname of source-code file}, line-number: {the line-number within that source-code file}".
        /// </remarks>
        [Conditional( "TRACE" )]
        public void LogTraceWithContext( LogCategory cat, string textToLog )
        {
        }
#endif
        #endregion LogTrace, LogTraceWithContext

        #region LogDebug, LogDebugWithContext
        /// <summary>
        /// Log a message at level <c>Debug</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogDebug( string textToLog )
        {
        }

        /// <summary>
        /// Log a message at level <c>Debug</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogDebug( LogCategory cat, string textToLog )
        {
        }

        /// <summary>
        /// Log a message at level <c>Debug</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="catEnum">an enumeration-type value that specifies which category</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogDebug( System.Enum catEnum, string textToLog )
        {
        }

        /// <summary>
        /// Log a message at level <c>Debug</c>, the message being the given object-array and expressed using the given string format.
        /// </summary>
        /// <param name="format">the format to use to express the message (as in <c>String.Format</c>)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [StringFormatMethod( "format" )]
        public void LogDebug( string format, params object[] args )
        {
        }

        /// <summary>
        /// Log a message at level <c>Debug</c>, the message being the given object-array and expressed using the given string format.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="format">the format to use to express the message (as in <c>String.Format</c>)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [StringFormatMethod( "format" )]
        public void LogDebug( LogCategory cat, string format, params object[] args )
        {
        }

#if !PRE_5
        /// <summary>
        /// Log a message at level <c>Debug</c>, and add source-code trace information (.NET 4.5 and up).
        /// You would only provide a value for the objectMessage parameter --
        /// let the compiler provide the arguments to the memberName, sourceFilePath, and sourceLineNumber parameters.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        /// <param name="memberName">(in .NET 4.5+ automatically inserted by the compiler) the class-method from which this was called.</param>
        /// <param name="sourceFilePath">(in .NET 4.5+ automatically inserted by the compiler) the file-system path of the source-code file from which this was called</param>
        /// <param name="sourceLineNumber">(in .NET 4.5+ automatically inserted by the compiler) the line-number within the source-code file from which this was called</param>
        /// <example>
        /// This logs the given text-message and also the file, class-member, and line-number at which this call is made.
        /// <code>
        /// LogDebugWithContext("An unexpected visitor came unto my door.");
        /// </code>
        /// You only provide the value for objectMessage; the compiler inserts all of the other values.
        /// In this way, you automatically get information written to the log that identifies exactly where this was called.
        /// </example>
        public void LogDebugWithContext( string textToLog,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string sourceFilePath = "",
                                         [CallerLineNumber] int sourceLineNumber = 0 )
        {
        }

        /// <summary>
        /// Log a message at level <c>Debug</c>, and add source-code trace information (.NET 4.5 and up).
        /// You would only provide a value for the objectMessage parameter --
        /// let the compiler provide the arguments to the memberName, sourceFilePath, and sourceLineNumber parameters.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        /// <param name="memberName">(in .NET 4.5+ automatically inserted by the compiler) the class-method from which this was called.</param>
        /// <param name="sourceFilePath">(in .NET 4.5+ automatically inserted by the compiler) the file-system path of the source-code file from which this was called</param>
        /// <param name="sourceLineNumber">(in .NET 4.5+ automatically inserted by the compiler) the line-number within the source-code file from which this was called</param>
        /// <example>
        /// This logs the given text-message and also the file, class-member, and line-number at which this call is made.
        /// <code>
        /// LogDebugWithContext("An unexpected visitor came unto my door.");
        /// </code>
        /// You only provide the value for objectMessage; the compiler inserts all of the other values.
        /// In this way, you automatically get information written to the log that identifies exactly where this was called.
        /// </example>
        public void LogDebugWithContext( LogCategory cat,
                                         string textToLog,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string sourceFilePath = "",
                                         [CallerLineNumber] int sourceLineNumber = 0 )
        {
        }
#else
        /// <summary>
        /// Log a message at level "Debug", and add source-code trace information including the calling method, source-file path and line-number.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogDebugWithContext( string textToLog )
        {
        }

        /// <summary>
        /// Log a message at level "Debug", and add source-code trace information including the calling method, source-file path and line-number.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogDebugWithContext( LogCategory cat, string textToLog )
        {
        }
#endif

        #endregion

        #region LogInfo, LogInfoWithContext

        /// <summary>
        /// Log a message at level <c>Infomation</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="textToLog">the string that represents the Message to log</param>
        public void LogInfo( string textToLog )
        {
        }

        /// <summary>
        /// Log a message at level <c>Infomation</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the Message to log</param>
        public void LogInfo( LogCategory cat, string textToLog )
        {
        }

        /// <summary>
        /// Log a message at level <c>Infomation</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="catEnum">an enumeration-type value that specifies which category</param>
        /// <param name="textToLog">the string that represents the Message to log</param>
        public void LogInfo( System.Enum catEnum, string textToLog )
        {
        }

        /// <summary>
        /// Log a message at level <c>Infomation</c>, the message being the given object-array and expressed using the given string format.
        /// </summary>
        /// <param name="format">the format to use to express the message (as in String.Format)</param>
        /// <param name="args">an array of objects that represents the values to go into the Message we want to log</param>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [StringFormatMethod( "format" )]
        public void LogInfo( string format, params object[] args )
        {
        }

        /// <summary>
        /// Log a message at level <c>Infomation</c>, the message being the given object-array and expressed using the given string format.
        /// </summary>
        /// <param name="format">the format to use to express the message (as in String.Format)</param>
        /// <param name="args">an array of objects that represents the values to go into the Message we want to log</param>
        /// <remarks>
        /// This is just a synonym for LogInfo, intended for convenience in changing from <code>Console.WriteLine</code> code statements.
        /// 
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [StringFormatMethod( "format" )]
        public void WriteLine( string format, params object[] args )
        {
        }

        /// <summary>
        /// Log a message at level <c>Infomation</c>, the message being the given object-array and expressed using the given string format.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="format">the format to use to express the message (as in String.Format)</param>
        /// <param name="args">an array of objects that represents the values to go into the Message we want to log</param>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [StringFormatMethod( "format" )]
        public void LogInfo( LogCategory cat, string format, params object[] args )
        {
        }

#if !PRE_5
        /// <summary>
        /// Log a message at level <c>Infomation</c>, and add source-code trace information (.Net 4.5+ only).
        /// You would only provide a value for the objectMessage parameter --
        /// let the compiler provide the arguments to the memberName, sourceFilePath, and sourceLineNumber parameters.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        /// <param name="memberName">(in .NET 4.5+ automatically inserted by the compiler) the class-method from which this was called</param>
        /// <param name="sourceFilePath">(in .NET 4.5+ automatically inserted by the compiler) the file-system path of the source-code file from which this was called</param>
        /// <param name="sourceLineNumber">(in .NET 4.5+ automatically inserted by the compiler) the line-number within the source-code file from which this was called</param>
        /// <example>
        /// This logs the given text-message and also the file, class-member, and line-number at which this call is made.
        /// <code>
        /// LogInfoTrace("An unexpected visitor came unto my door.");
        /// </code>
        /// You only provide the value for objectMessage; the compiler inserts all of the other values.
        /// In this way, you automatically get information written to the log that identifies exactly where this was called.
        /// </example>
        public void LogInfoWithContext( string textToLog,
                                        [CallerMemberName] string memberName = "",
                                        [CallerFilePath] string sourceFilePath = "",
                                        [CallerLineNumber] int sourceLineNumber = 0 )
        {
        }

        /// <summary>
        /// Log a message at level <c>Infomation</c>, and add source-code trace information (.Net 4.5+ only).
        /// You would only provide a value for the objectMessage parameter --
        /// let the compiler provide the arguments to the memberName, sourceFilePath, and sourceLineNumber parameters.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        /// <param name="memberName">(in .NET 4.5+ automatically inserted by the compiler) the class-method from which this was called</param>
        /// <param name="sourceFilePath">(in .NET 4.5+ automatically inserted by the compiler) the file-system path of the source-code file from which this was called</param>
        /// <param name="sourceLineNumber">(in .NET 4.5+ automatically inserted by the compiler) the line-number within the source-code file from which this was called</param>
        /// <example>
        /// This logs the given text-message and also the file, class-member, and line-number at which this call is made.
        /// <code>
        /// LogInfoTrace("An unexpected visitor came unto my door.");
        /// </code>
        /// You only provide the value for objectMessage; the compiler inserts all of the other values.
        /// In this way, you automatically get information written to the log that identifies exactly where this was called.
        /// </example>
        public void LogInfoWithContext( LogCategory cat,
                                        string textToLog,
                                        [CallerMemberName] string memberName = "",
                                        [CallerFilePath] string sourceFilePath = "",
                                        [CallerLineNumber] int sourceLineNumber = 0 )
        {
        }
#else
        /// <summary>
        /// Log a message at level <c>Info</c>, and add source-code trace information including the calling method, source-file path and line-number.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogInfoWithContext( string textToLog )
        {
        }

        /// <summary>
        /// Log a message at level <c>Info</c>, and add source-code trace information including the calling method, source-file path and line-number.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogInfoWithContext( LogCategory cat, string textToLog )
        {
        }
#endif
        #endregion LogInfo, LogInfoWithContext

        #region LogWarning, LogWarningWithContext
        /// <summary>
        /// Log a message at level <c>Warning</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log, by using it's ToString method</param>
        public void LogWarning( string textToLog )
        {
        }

        /// <summary>
        /// Log a message at level <c>Warning</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log, by using it's ToString method</param>
        public void LogWarning( LogCategory cat, string textToLog )
        {
        }

        /// <summary>
        /// Log a message at level <c>Warning</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="catEnum">an enumeration-type value that specifies which category</param>
        /// <param name="textToLog">the string that represents the message to log, by using it's ToString method</param>
        public void LogWarning( System.Enum catEnum, string textToLog )
        {
        }

        /// <summary>
        /// Log a message at level <c>Warning</c>, the message being the given object-array and expressed using the given string format.
        /// </summary>
        /// <param name="format">the format to use to express the message (as in String.Format)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [StringFormatMethod( "format" )]
        public void LogWarning( string format, params object[] args )
        {
        }

        /// <summary>
        /// Log a message at level <c>Warning</c>, the message being the given object-array and expressed using the given string format.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="format">the format to use to express the message (as in String.Format)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [StringFormatMethod( "format" )]
        public void LogWarning( LogCategory cat, string format, params object[] args )
        {
        }

        /// <summary>
        /// Log a message at level <c>Warning</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log, by using it's ToString method</param>
        public void Warn( string textToLog )
        {
        }

        /// <summary>
        /// Log a message at level <c>Warning</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log, by using it's ToString method</param>
        public void Warn( LogCategory cat, string textToLog )
        {
        }

        /// <summary>
        /// Log a message at level <c>Warning</c>, the message being the given object-array and expressed using the given string format.
        /// This is a synonym for LogWarning.
        /// </summary>
        /// <param name="format">the format to use to express the message (as in String.Format)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [StringFormatMethod( "format" )]
        public void Warn( string format, params object[] args )
        {
        }

        /// <summary>
        /// Log a message at level <c>Warning</c>, the message being the given object-array and expressed using the given string format.
        /// This is a synonym for LogWarning.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="format">the format to use to express the message (as in String.Format)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [StringFormatMethod( "format" )]
        public void Warn( LogCategory cat, string format, params object[] args )
        {
        }

#if !PRE_5
        /// <summary>
        /// Log a message at level <c>Warning</c>, and add source-code trace information (.Net 4.5 and up only).
        /// You would only provide a value for the objectMessage parameter --
        /// let the compiler provide the arguments to the memberName, sourceFilePath, and sourceLineNumber parameters.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        /// <param name="memberName">(in .NET 4.5+ automatically inserted by the compiler) the class-method from which this was called</param>
        /// <param name="sourceFilePath">(in .NET 4.5+ automatically inserted by the compiler) the file-system path of the source-code file from which this was called</param>
        /// <param name="sourceLineNumber">(in .NET 4.5+ automatically inserted by the compiler) the line-number within the source-code file from which this was called</param>
        /// <example>
        /// This logs the given text-message and also the file, class-member, and line-number at which this call is made.
        /// <code>
        /// LogWarningWithContext("Be thou thus warned - I seem to be running low on memory.");
        /// </code>
        /// You only provide the value for objectMessage; the compiler inserts all of the other values.
        /// In this way, you automatically get information written to the log that identifies exactly where this was called.
        /// </example>
        public void LogWarningWithContext( string textToLog,
                                           [CallerMemberName] string memberName = "",
                                           [CallerFilePath] string sourceFilePath = "",
                                           [CallerLineNumber] int sourceLineNumber = 0 )
        {
        }

        /// <summary>
        /// Log a message at level <c>Warning</c>, and add source-code trace information (.Net 4.5 and up only).
        /// You would only provide a value for the objectMessage parameter --
        /// let the compiler provide the arguments to the memberName, sourceFilePath, and sourceLineNumber parameters.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        /// <param name="memberName">(in .NET 4.5+ automatically inserted by the compiler) the class-method from which this was called</param>
        /// <param name="sourceFilePath">(in .NET 4.5+ automatically inserted by the compiler) the file-system path of the source-code file from which this was called</param>
        /// <param name="sourceLineNumber">(in .NET 4.5+ automatically inserted by the compiler) the line-number within the source-code file from which this was called</param>
        /// <example>
        /// This logs the given text-message and also the file, class-member, and line-number at which this call is made.
        /// <code>
        /// LogWarningWithContext("Be thou thus warned - I seem to be running low on memory.");
        /// </code>
        /// You only provide the value for objectMessage; the compiler inserts all of the other values.
        /// In this way, you automatically get information written to the log that identifies exactly where this was called.
        /// </example>
        public void LogWarningWithContext( LogCategory cat,
                                           string textToLog,
                                           [CallerMemberName] string memberName = "",
                                           [CallerFilePath] string sourceFilePath = "",
                                           [CallerLineNumber] int sourceLineNumber = 0 )
        {
        }
#else
        /// <summary>
        /// Log a message at level <c>Warn</c>, and add source-code trace information including the calling method, source-file path and line-number.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogWarningWithContext( string textToLog )
        {
        }

        /// <summary>
        /// Log a message at level <c>Warn</c>, and add source-code trace information including the calling method, source-file path and line-number.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogWarningWithContext( LogCategory cat, string textToLog )
        {
        }
#endif
        #endregion LogWarning, LogWarningWithContext

        #region LogError, LogErrorWithContext
        /// <summary>
        /// Log a message at level <c>Error</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogError( string textToLog )
        {
        }

        /// <summary>
        /// Log a message at level <c>Error</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogError( LogCategory cat, string textToLog )
        {
        }

        public void LogError( System.Enum category, string whatToLog )
        {
        }

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
        public void LogError( string format, params object[] args )
        {
        }

        /// <summary>
        /// Log a message at level <c>Error</c>, the message being the given object-array and expressed using the given string format.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="format">the format to use to express the message (as in String.Format)</param>
        /// <param name="args">an array of objects that represents values to go into the message we want to log</param>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [StringFormatMethod( "format" )]
        public void LogError( LogCategory cat, string format, params object[] args )
        {
        }

#if !PRE_5
        /// <summary>
        /// Log a message at level <c>Error</c>, and add source-code trace information (.Net 4.5 and up).
        /// You would only provide a value for the objectMessage parameter --
        /// let the compiler provide the arguments to the memberName, sourceFilePath, and sourceLineNumber parameters.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        /// <param name="memberName">(in .NET 4.5+ automatically inserted by the compiler) the class-method from which this was called</param>
        /// <param name="sourceFilePath">(in .NET 4.5+ automatically inserted by the compiler) the file-system path of the source-code file from which this was called</param>
        /// <param name="sourceLineNumber">(in .NET 4.5+ automatically inserted by the compiler) the line-number within the source-code file from which this was called</param>
        /// <example>
        /// This logs the given text-message and also the file, class-member, and line-number at which this call is made.
        /// <code>
        /// LogErrorWithContext("Oh my - this result seems to be in error.");
        /// </code>
        /// You only provide the value for objectMessage; the compiler inserts all of the other values.
        /// In this way, you automatically get information written to the log that identifies exactly where this was called.
        /// </example>
        public void LogErrorWithContext( string textToLog,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string sourceFilePath = "",
                                         [CallerLineNumber] int sourceLineNumber = 0 )
        {
        }

        /// <summary>
        /// Log a message at level <c>Error</c>, and add source-code trace information (.Net 4.5 and up).
        /// You would only provide a value for the objectMessage parameter --
        /// let the compiler provide the arguments to the memberName, sourceFilePath, and sourceLineNumber parameters.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        /// <param name="memberName">(in .NET 4.5+ automatically inserted by the compiler) the class-method from which this was called</param>
        /// <param name="sourceFilePath">(in .NET 4.5+ automatically inserted by the compiler) the file-system path of the source-code file from which this was called</param>
        /// <param name="sourceLineNumber">(in .NET 4.5+ automatically inserted by the compiler) the line-number within the source-code file from which this was called</param>
        /// <example>
        /// This logs the given text-message and also the file, class-member, and line-number at which this call is made.
        /// <code>
        /// LogErrorWithContext("Oh my - this result seems to be in error.");
        /// </code>
        /// You only provide the value for objectMessage; the compiler inserts all of the other values.
        /// In this way, you automatically get information written to the log that identifies exactly where this was called.
        /// </example>
        public void LogErrorWithContext( LogCategory cat,
                                         string textToLog,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string sourceFilePath = "",
                                         [CallerLineNumber] int sourceLineNumber = 0 )
        {
        }
#else
        /// <summary>
        /// Log a message at level <c>Error</c>, and add source-code trace information including the calling method, source-file path and line-number.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogErrorWithContext( string textToLog )
        {
        }

        /// <summary>
        /// Log a message at level <c>Error</c>, and add source-code trace information including the calling method, source-file path and line-number.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogErrorWithContext( LogCategory cat, string textToLog )
        {
        }
#endif

#if (!PRE_5)  // This is for .NET 4.5 / C# 5.0 and higher
        /// <summary>
        /// Log a message at level <c>Error</c> which consists of information concerning the given Exception.
        /// </summary>
        /// <param name="ex">the Exception to describe</param>
        /// <param name="additionalInformation">(optional) any text that you want to add to help describe where and why this happened</param>
        /// <param name="memberName">(in .NET 4.5+ automatically inserted by the compiler) the class-method from which this was called</param>
        /// <param name="sourceFilePath">(in .NET 4.5+ automatically inserted by the compiler) the file-system path of the source-code file from which this was called</param>
        /// <param name="sourceLineNumber">(in .NET 4.5+ automatically inserted by the compiler) the line-number within the source-code file from which this was called</param>
        /// <remarks>
        /// This adds to the error-output information of the form:
        /// "member-name: {method-name}, source-file: {pathname of source-code file}, line-number: {the line-number within that source-code file}".
        /// </remarks>
        public void LogError( Exception ex,
                              string additionalInformation = null,
                              [CallerMemberName] string memberName = "",
                              [CallerFilePath] string sourceFilePath = "",
                              [CallerLineNumber] int sourceLineNumber = 0 )
        {
        }
#else
        // This is for < .NET 4.5

        /// <summary>
        /// Log a message at level "Error" which consists of information concerning the given Exception.
        /// </summary>
        /// <param name="exception">the Exception to describe</param>
        public void LogError( Exception exception )
        {
        }

        /// <summary>
        /// Log a message at level "Error" which consists of information concerning the given Exception.
        /// </summary>
        /// <param name="exception">the Exception to describe</param>
        /// <param name="additionalInformation">any text that you want to add to help describe where and why this happened (may be null)</param>
        /// <remarks>
        /// This adds to the normal LogTrace output information of the form:
        /// "member-name: {method-name}, source-file: {pathname of source-code file}, line-number: {the line-number within that source-code file}".
        /// </remarks>
        public void LogError( Exception exception, string additionalInformation )
        {
        }
#endif

        /// <summary>
        /// Log a message at level "Error" which consists of information concerning the given Exception.
        /// </summary>
        /// <param name="ex">the Exception to describe</param>
        /// <param name="format">to convey additional information - this is the format-string (as in String.Fomrrat)</param>
        /// <param name="args">to convey additional information - this is an array of objects that represent values to insert into the format-string</param>
        [StringFormatMethod("format")]
        public void LogError(Exception ex, string format, params object[] args)
        {
        }

        #endregion LogError, LogErrorWithContext

        #region LogCritical, LogCriticalWithContext
        /// <summary>
        /// Log a message at level <c>Critical</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogCritical( string textToLog )
        {
        }

        /// <summary>
        /// Log a message at level <c>Critical</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogCritical( LogCategory cat, string textToLog )
        {
        }

        /// <summary>
        /// Log a message at level <c>Critical</c>, the message being the given object-array and expressed using the given string format.
        /// </summary>
        /// <param name="format">the format to use to express the message (as in String.Format)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [StringFormatMethod( "format" )]
        public void LogCritical( string format, params object[] args )
        {
        }

        /// <summary>
        /// Log a message at level <c>Critical</c>, the message being the given object-array and expressed using the given string format.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="format">the format to use to express the message (as in String.Format)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [StringFormatMethod( "format" )]
        public void LogCritical( LogCategory cat, string format, params object[] args )
        {
        }

#if !PRE_5
        /// <summary>
        /// Log a message at level <c>Critical</c>, and add source-code trace information (.Net 4.5 and higher).
        /// You would only provide a value for the objectMessage parameter --
        /// let the compiler provide the arguments to the memberName, sourceFilePath, and sourceLineNumber parameters.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        /// <param name="memberName">(in .NET 4.5+ automatically inserted by the compiler) the class-method from which this was called</param>
        /// <param name="sourceFilePath">(in .NET 4.5+ automatically inserted by the compiler) the file-system path of the source-code file from which this was called</param>
        /// <param name="sourceLineNumber">(in .NET 4.5+ automatically inserted by the compiler) the line-number within the source-code file from which this was called</param>
        /// <example>
        /// This logs the given text-message and also the file, class-member, and line-number at which this call is made.
        /// <code>
        /// LogCriticalWithContext("A quite fatal thing has just happened!");
        /// </code>
        /// You only provide the value for objectMessage; the compiler inserts all of the other values.
        /// In this way, you automatically get information written to the log that identifies exactly where this was called.
        /// </example>
        public void LogCriticalWithContext( LogCategory cat,
                                            string textToLog,
                                            [CallerMemberName] string memberName = "",
                                            [CallerFilePath] string sourceFilePath = "",
                                            [CallerLineNumber] int sourceLineNumber = 0 )
        {
        }
#else
        /// <summary>
        /// Log a message at level <c>Critical</c>, and add source-code trace information including the calling method, source-file path and line-number.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogCriticalWithContext( string textToLog )
        {
        }

        /// <summary>
        /// Log a message at level <c>Critical</c>, and add source-code trace information including the calling method, source-file path and line-number.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogCriticalWithContext( LogCategory cat, string textToLog )
        {
        }
#endif
        #endregion

        #region LogMethodBegin and End

        #region LogMethodBegin
#if !PRE_5
        /// <summary>
        /// Log a message that signals that execution has entered a class-method, with Category MethodTrace.
        /// This logs the name of the method and an optional message, as well as the line-number, at level Trace.
        /// It is of the form: 'begin class-name.method-name Message,  at line 69'
        /// </summary>
        /// <param name="objectMessage">any additional information to note</param>
        /// <param name="memberName">(in .NET 4.5+ automatically inserted by the compiler) the class-method from which this was called</param>
        /// <param name="sourceLineNumber">(in .NET 4.5+ automatically inserted by the compiler) the line-number within the source-code file from which this was called</param>
        /// <remarks>
        /// This logs "begin {method-name} at line {line-number}" at level Trace.
        /// </remarks>
        [Conditional( "TRACE" )]
        public void LogMethodBegin( object objectMessage = null,
                                    [CallerMemberName] string memberName = "",
                                    [CallerLineNumber] int sourceLineNumber = 0 )
        {
        }
#else
        /// <summary>
        /// Log a message that signals that execution has entered a class-method.
        /// This logs the name of the method and an optional message, as well as the line-number, at level Trace
        /// and with LogCategory <see cref="LogCategory.MethodTrace"/>
        /// It is of the form: 'begin class-name.method-name Message,  at line 69'
        /// </summary>
        /// <param name="objectMessage">any additional information to note</param>
        /// <remarks>
        /// This logs "begin {method-name} at line {line-number}" at level Trace.
        /// </remarks>
        [Conditional( "TRACE" )]
        public void LogMethodBegin( object objectMessage = null )
        {
        }
#endif
        #endregion

        #region LogMethodEnd
#if !PRE_5
        /// <summary>
        /// Log a message denoting the end of the method and an (optional) message, at level Trace.
        /// </summary>
        /// <param name="objectMessage">the text to add to the log-message (optional). May be null.</param>
        /// <param name="memberName">(in .NET 4.5+ automatically inserted by the compiler) the class-method from which this was called</param>
        /// <remarks>
        /// This logs "end {method-name}." at level Trace.
        /// 
        /// This is just a rather trivial shortcut, which substitutes for the following code:
        /// <example>
        /// Instead of..
        /// <code>
        ///     MyLogger.LogTrace( "end MyMethod." );
        /// </code>
        /// You can..
        /// <code>
        ///     MyLogger.LogMethodEnd();
        /// </code>
        /// </example>
        /// 
        /// This method is conditional upon TRACE being defined.
        /// </remarks>
        [Conditional( "TRACE" )]
        public void LogMethodEnd( object objectMessage = null, [CallerMemberName] string memberName = "" )
        {
        }

#else

        /// <summary>
        /// Log a message denoting the end of the method and an (optional) message, at level Trace.
        /// </summary>
        /// <param name="objectMessage">the text to add to the log-message (optional). May be null.</param>
        /// <remarks>
        /// This logs "end {method-name}." at level Trace.
        /// 
        /// This is just a rather trivial shortcut, which substitutes for the following code:
        /// <example>
        /// Instead of..
        /// <code>
        ///     MyLogger.LogTrace( "end MyMethod." );
        /// </code>
        /// You can..
        /// <code>
        ///     MyLogger.LogMethodEnd();
        /// </code>
        /// </example>
        /// 
        /// This method is conditional upon TRACE being defined.
        /// </remarks>
        [Conditional( "TRACE" )]
        public void LogMethodEnd( object objectMessage = null )
        {
        }
#endif
        #endregion

        #region LogMethodEndErrorIfFalse
#if !PRE_5
        /// <summary>
        /// Log a message denoting the end of the method and whether the method is returning okay (true) or an error (false).
        /// This logs it at level Trace if okay, otherwise at level Error.
        /// </summary>
        /// <param name="isOkay">the return-value, which if false signals that this is returning non-success</param>
        /// <param name="memberName">(in .NET 4.5+ automatically inserted by the compiler) the class-method from which this was called</param>
        /// <param name="sourceFilePath">(in .NET 4.5+ automatically inserted by the compiler) the file-system path of the source-code file from which this was called</param>
        /// <param name="sourceLineNumber">(in .NET 4.5+ automatically inserted by the compiler) the line-number within the source-code file from which this was called</param>
        /// <remarks>
        /// This logs at level Trace "end {method-name} returning true." into the log when isOkay is true,
        /// and logs at level Error "end {method-name} returning false." when isOkay is false.
        /// 
        /// This is just a rather trivial shortcut-method, which substitutes for the following code:
        /// <example>
        /// Instead of..
        /// <code>
        ///   if (ok)
        ///   {
        ///     MyLogger.LogTrace( "end GetCervicalGuideRfid returning true" );
        ///   }
        ///   else
        ///   {
        ///     MyLogger.LogError( "end GetCervicalGuideRfid returning false" );
        ///   }
        /// </code>
        /// You can..
        /// <code>
        ///   MyLogger.LogMethodEndErrorIfFalse( ok );
        /// </code>
        /// </example>
        /// 
        /// This method is conditional upon TRACE being defined.
        /// </remarks>
        [Conditional( "TRACE" )]
        public void LogMethodEndErrorIfFalse( bool isOkay,
                                              [CallerMemberName] string memberName = "",
                                              [CallerFilePath] string sourceFilePath = "",
                                              [CallerLineNumber] int sourceLineNumber = 0 )
        {
        }

#else

        /// <summary>
        /// Log a message denoting the end of the method and whether the method is returning okay (true) or an error (false).
        /// This logs it at level Trace if okay, otherwise at level Error.
        /// </summary>
        /// <param name="isOkay">the return-value, which if false signals that this is returning non-success</param>
        /// <remarks>
        /// This logs at level Trace "end {method-name} returning true." into the log when isOkay is true,
        /// and logs at level Error "end {method-name} returning false." when isOkay is false.
        /// 
        /// This is just a rather trivial shortcut-method, which substitutes for the following code:
        /// <example>
        /// Instead of..
        /// <code>
        ///   if (ok)
        ///   {
        ///     MyLogger.LogTrace( "end GetCervicalGuideRfid returning true" );
        ///   }
        ///   else
        ///   {
        ///     MyLogger.LogError( "end GetCervicalGuideRfid returning false" );
        ///   }
        /// </code>
        /// You can..
        /// <code>
        ///   MyLogger.LogMethodEndErrorIfFalse( ok );
        /// </code>
        /// </example>
        /// 
        /// This method is conditional upon TRACE being defined.
        /// </remarks>
        [Conditional( "TRACE" )]
        public void LogMethodEndErrorIfFalse( bool isOkay )
        {
        }
#endif
        #endregion

        #endregion LogMethodBegin and End

        #region AddToPresentationTraceListeners
        /// <summary>
        /// Start logging Visual Studio PresentationTraceSources trace-output such as WPF binding errors.
        /// </summary>
        /// <param name="levelToLogAt">the log-level to use when logging trace output</param>
        public void AddToPresentationTraceListeners( LogLevel levelToLogAt )
        {
        }
        #endregion

        #region time-interval logging methods

        #region SetTimeReference
        /// <summary>
        /// Call this method to start measuring a time interval, to establish your zero-reference.
        /// A subsequent call to LogElapsedTime will log the amount of time that has passed since you called this method.
        /// </summary>
        /// <returns>a reference to this logger so that methods may be chained together</returns>
        public ILognutLogger SetTimeReference()
        {
            return this;
        }
        #endregion

        #region LogElapsedTime
        /// <summary>
        /// Send an (Infomation-level) log output that simply indicates how much time has elapsed since the last call to SetTimeReference.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="comment">a note to include in the log output</param>
        /// <returns>a reference to this logger so that methods may be chained together</returns>
        public ILognutLogger LogElapsedTime( LogCategory cat, string comment )
        {
            return this;
        }
        #endregion

        #endregion time-interval logging methods

        #endregion logging-output methods

        #region enablement - applicable to this particular logger

        #region ClearOverrides
        /// <summary>
        /// Set any enablement-overrides back to the default state of null (meaning no preference is voiced).
        /// </summary>
        public void ClearOverrides()
        {
        }
        #endregion

        #region IsEnabled
        /// <summary>
        /// Get or set whether this logger override's the LogManager's setting to be enabled.
        /// Default is null, which means no override is set. and you would never set this property unless you want to control THIS logger separately from others.
        /// </summary>
        /// <remarks>
        /// This flag is specific to this logger; LogManager.Config has a separate IsLoggingEnabled property
        /// that is independent of this one.
        /// 
        /// If this property is left at it's default value <c>null</c>,
        /// then it is the property <c>LogManager.Config.IsLoggingEnabled</c> that controls whether logging-output happens.
        /// 
        /// If this property is set to either <c>true</c> or <c>false</c>, then this controls whether logging happens from this logger.
        /// Also, if this is set - it overrides the property Config.LowestLevelThatIsEnabled
        /// (note that the Logger class also has it's own LowestLevelThatIsEnabled property which by default is null).
        /// </remarks>
        public bool? IsEnabled
        {
            get { return null; }
            set {  }
        }

        /// <summary>
        /// Get whether this logger is actually enabled -- taking into account both the <c>Logger.IsEnabled</c> property
        /// and the <c>LogManager.Config.IsLoggingEnabled</c> property.
        /// </summary>
        /// <remarks>
        /// The property <c>Logger.IsEnabled</c> is specific to this logger; LogManager.Config has a separate IsLoggingEnabled property
        /// that is independent of this one and serves as the global setting, which this specific logger may override just for itself.
        /// 
        /// If the property <c>Logger.IsEnabled</c> is left at it's default value <c>null</c>,
        /// then it is the property <c>LogManager.Config.IsLoggingEnabled</c> that controls whether logging-output happens,
        /// and *this* property, is the effective value.
        /// 
        /// If the property <c>Logger.IsEnabled</c> is set to either <c>true</c> or <c>false</c>,
        /// then that is what controls whether logging happens from this logger, and this property <c>IsEnabled_EffectiveValue</c> reflects that effective value.
        /// </remarks>
        public bool IsEnabled_EffectiveValue
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Enable output from this logger. This is the same as setting <c>IsEnabled</c> to true.
        /// </summary>
        /// <returns>a reference to this logger so that methods may be chained together</returns>
        /// <remarks>
        /// You don't necessarily have to set <c>Logger.IsEnabled</c> to true in order to get logging to happen.
        /// This is only for when you want to override the global setting that is <c>LogManager.Config.IsLoggingEnabled</c> .
        /// </remarks>
        public ILognutLogger Enable()
        {
            return this;
        }

        /// <summary>
        /// Disable all output from this logger. This is the same as setting <c>IsEnabled</c> to false.
        /// </summary>
        /// <returns>a reference to this logger so that methods may be chained together</returns>
        /// <remarks>
        /// You don't necessarily have to set <c>Logger.IsEnabled</c> to true in order to get logging to happen,
        /// nor set it to <c>false</c> to turn it off.
        /// This is only for when you want to override the global setting that is <c>LogManager.Config.IsLoggingEnabled</c> .
        /// </remarks>
        public ILognutLogger Disable()
        {
            return this;
        }
        #endregion

        #region level enablement

        #region EnableAllLevels
        /// <summary>
        /// Allow output from this logger down to the lowest level,
        /// i.e. all log-levels.
        /// </summary>
        /// <returns>a reference to this Logger object so that methods may be chained together</returns>
        public LoggerDoNothing EnableAllLevels()
        {
            return this;
        }
        #endregion

        #region EnableLevelsDownTo
        /// <summary>
        /// Given a LogLevel, enable the levels for this logger from <c>Critical</c> down to the given level (inclusive),
        /// and disable the remaining, lower levels.
        /// This is an override of the property on <c>LogManager.Config</c>.
        /// </summary>
        /// <param name="level">The log-level to enable down to, from the highest priority down to this one</param>
        /// <returns>a reference to this logger object so that methods may be chained together</returns>
        /// <remarks>
        /// Given a LogLevel, enable the levels for this logger from <c>Critical</c> down to the given level (inclusive),
        /// descending by order of 'severity', and disable the remaining, lower levels.
        /// Ie <c>Infomation</c> -> enable all levels;
        /// <c>Debug</c> -> enable <c>Debug</c>, <c>Warning</c>, <c>Error</c>, <c>Critical</c>;
        /// <c>Warning</c> -> <c>Warning</c>, <c>Error</c>, <c>Critical</c>;
        /// <c>Error</c> -> <c>Error</c>, <c>Critical</c>,
        /// and <c>Critical</c> -> enable only <c>Critical</c>.
        /// </remarks>
        public LoggerDoNothing EnableLevelsDownTo( LogLevel level )
        {
            return this;
        }
        #endregion

        #region LowestLevelThatIsEnabled_Override
        /// <summary>
        /// Get or set the override value of the minimum log-level that is enabled for this Logger.
        /// Setting this to <c>Trace</c>, enables all levels - which is the default.
        /// This overrides the setting on <c>LogManager.Config</c>.
        /// </summary>
        /// <remarks>
        /// This property is normally null, meaning it is the <c>LogManager.Config</c> property that controls.
        /// </remarks>
        public LogLevel? LowestLevelThatIsEnabled_Override
        {
            get { return default(LogLevel); }
            set {  }
        }
        #endregion

        #region IsEffectivelyEnabledForLevel
        /// <summary>
        /// Return true if output is enabled for this logger AND according to LogManager, and for this given LogLevel
        /// </summary>
        /// <param name="level">The LogLevel to check for</param>
        /// <returns>true only if this logger is enabled and also enabled for the given LogLevel</returns>
        public bool IsEffectivelyEnabledForLevel( LogLevel level )
        {
            return false;
        }
        #endregion

        #endregion level enablement

        /// <summary>
        /// Return true if output is enabled for this logger, for this LogLevel, and this LogCategory.
        /// Category overrides level, such that if not enabled by level but is by category - it gets logged.
        /// </summary>
        /// <param name="level"></param>
        /// <param name="cat"></param>
        /// <returns></returns>
        public bool IsEffectivelyEnabledForLevelAndCategory( LogLevel level, LogCategory cat )
        {
            return false;
        }

        #endregion enablement - applicable to this particular logger

        #region console output

        #region IsCatchingTraceOutput
        /// <summary>
        /// Get or set whether this logger has Visual Studio Trace-output redirected to it.
        /// </summary>
        public bool IsCatchingTraceOutput { get; set; }

        #endregion

        /// <summary>
        /// Get or set the override of the <see cref="LogConfig.IsToOutputToConsole"/> property,
        /// which dictates whether output to the console is going to happen, for this specific logger.
        /// The default is null, meaning there is no override.
        /// </summary>
        /// <remarks>
        /// The default value of the <see cref="LogConfig.IsToOutputToConsole"/> property is true,
        /// so when this property is either <c>true</c>, or <c>null</c>, output to the console will happen.
        /// </remarks>
        public bool? IsToOutputToConsole_Override
        {
            get { return null; }
            set {  }
        }

        /// <summary>
        /// Get the actual, effective value of whether to output to the console.
        /// This takes the value from <c>LogManager.Config.IsToOutputToConsole</c>, unless this Logger has overridden it 
        /// - in which case the logger's value of <c>IsToOutputToConsole_Override</c> is returned.
        /// </summary>
        public bool IsToOutputToConsole_EffectiveValue
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Set the override of the <see cref="LogConfig.IsToOutputToConsole"/> property,
        /// which dictates whether output to the console is going to happen.
        /// The default value of that property is null, meaning there is no override.
        /// </summary>
        /// <param name="nullOrOverride">set this to true or false to override, or null to not override</param>
        /// <returns>a reference to this logger so that methods may be chained together</returns>
        /// <remarks>
        /// This is the same thing as setting the <see cref="IsToOutputToConsole_Override"/> property.
        /// It is provided so that you can chain together method-calls on your logger
        /// using a fluent-API pattern.
        /// 
        /// The default value of the <see cref="LogConfig.IsToOutputToConsole"/> property is true,
        /// so when the override-property that this sets is either <c>true</c>, or <c>null</c>,
        /// output to the console will happen.
        /// </remarks>
        public ILognutLogger EnableConsoleOutput( bool? nullOrOverride )
        {
            return this;
        }

        /// <summary>
        /// Disable output to the console.
        /// </summary>
        /// <returns>a reference to this logger so that methods may be chained together</returns>
        /// <remarks>
        /// This is the same thing as setting the <see cref="IsToOutputToConsole_Override"/> property to false.
        /// It is provided so that you can chain together method-calls on your logger
        /// using a fluent-API pattern.
        /// 
        /// The default value of the <see cref="LogConfig.IsToOutputToConsole"/> property is true,
        /// so when the override-property that this sets is either <c>true</c>, or <c>null</c>,
        /// output to the console will happen.
        /// </remarks>
        public ILognutLogger DisableConsoleOutput()
        {
            return this;
        }

        #endregion console output

        #region statistical facilities

        //CBL  New functionality:
        // I want to be able to log that something happened, with no disk-writing every time
        // but to just save it, and then write it to disk later,
        // and just create a chart of the frequency of those logs, or perhaps some statistics of it.
        // Avg, min, max interval.

        #region LogStatistics
        /// <summary>
        /// Write a log at the Infomation level that summarizes some statistics regarding the preceding
        /// calls to <c>AddToStatistics</c>.
        /// </summary>
        /// <param name="logMessageToSelect"></param>
        public void LogStatistics( string logMessageToSelect )
        {
        }
        #endregion

        #region AddToStatistics
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logMessage"></param>
        /// <param name="isBegin"></param>
        public void AddToStatistics( string logMessage, bool isBegin )
        {
        }
        #endregion

        #region ClearStatistics
        /// <summary>
        /// Empty this logger of all of the information that has accumulated by calls to <see cref="AddToStatistics"/>.
        /// </summary>
        public void ClearStatistics()
        {
        }
        #endregion

        #endregion statistical facilities

        #region ToString
        /// <summary>
        /// Override the ToString method to produce a more informative description of this object.
        /// </summary>
        /// <returns>A string describing this object</returns>
        public override string ToString()
        {
            return "LoggerDoNothing";
        }
        #endregion

    }
}
