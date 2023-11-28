#if PRE_4
#define PRE_5
#endif
using System;
using System.Collections;
#if !PRE_4
using System.Collections.Concurrent;
#endif
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
#if !PRE_4
using System.Threading.Tasks;
#endif
using System.Windows;
#if !PRE_5
using System.Runtime.CompilerServices;
#else
using System.Reflection;
#endif
using Hurst.LogNut.Util;
using Hurst.LogNut.Util.Annotations;


// The actual writing-to-file happens within the method LogManager.Send


namespace Hurst.LogNut
{
    /// <summary>
    /// This is the LogNut logger class, instances of which are the objects that do the logging.
    /// </summary>
    public class Logger : ILognutLogger
    {
        #region Name
        /// <summary>
        /// Get the name of this Logger object. By default it is an empty string.
        /// </summary>
        public string Name
        {
            // I made this not settable, because LogManager saves a list of all the loggers and accesses them by name.
            get
            {
                if (_name == null)
                {
                    _name = String.Empty;
                }
                return _name;
            }
        }
        #endregion

        #region logging-output methods

        //CBL  Okay - so I don't need all of these: Log, and LogString.  merge this shit!

        #region Log
        /// <summary>
        /// Output a log-record containing the given log-message (textToLog), at the given log-level.
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
        public void Log(LogLevel level, LogCategory cat, string textToLog, bool isToSuppressTraceOutput)
        {
            //CBL I had also created LogString because I needed a way to force LogError to come to that method.
            // When format contained open/close brackets, it apparently thought
            // that was intended as a format-string, and called into the method with an args parameter, resulting in an additional FormatException.

            if (this.IsEffectivelyEnabledForLevelAndCategory(level, cat))
            {
                string message = textToLog ?? "null";
                try
                {
                    LogManager.QueueOrSendToLog(this, CreateLogRecord(level, cat, message), isToSuppressTraceOutput);
                }
#if !NETFX_CORE
                catch (ThreadAbortException)
                {
                    // These can happen if the app is attempting to log something as it shuts down.
                }
#endif
                catch (Exception x)
                {
                    var sb = new StringBuilder(String.Format("Handled a " + StringLib.ExceptionNameShortened(x) + " within Logger.Log(" + level + ", " + cat + ", "));
                    if (textToLog == null)
                    {
                        sb.Append("textToLog: null)");
                    }
                    else if (StringLib.HasNothing(textToLog))
                    {
                        sb.Append("textToLog: empty)");
                    }
                    else
                    {
                        string logMessage = StringLib.Shortened(message, 40);
                        sb.Append("textToLog: ").Append(StringLib.AsQuotedString(logMessage)).Append(")");
                    }
                    sb.AppendLine();
                    //CBL This makes a *very* verbose output! Improve this.
                    sb.Append(StringLib.ExceptionDetails(x, true));

                    //CBL  We definitely do not need both sb and x. Too verbose!
                    NutUtil.WriteToConsoleAndInternalLog(sb.ToString(), x);
                    //NutUtil.WriteToConsole( sb.ToString() );
                    LogManager.RaiseLoggingFaultOccurred(this, null, sb.ToString(), x, null);

                    // Throw a LoggingException to report this, unless we are suppressing exceptions.
                    if (!LogManager.Config.IsSuppressingExceptions)
                    {
                        throw new LoggingException(sb.ToString(), level, textToLog, x);
                    }
                }
            }
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
        [StringFormatMethod("format")]
        public void LogStringFormat(LogLevel level, LogCategory cat, string format, params object[] args)
        {
            if (this.IsEffectivelyEnabledForLevelAndCategory(level, cat))
            {
                try
                {
                    if (format == null)
                    {
                        LogManager.QueueOrSendToLog(this, CreateLogRecord(level, cat, "null"), isToSuppressTraceOutput: false);
                    }
                    else
                    {
                        // Convert any null objects to the string-literal "null".
                        LogRecord logRecord;
                        if (args.Length > 0)
                        {
                            var argsNonNull = (from a in args select (a ?? "null")).ToArray();
                            logRecord = CreateLogRecord(level, cat, String.Format(format, argsNonNull));
                        }
                        else
                        {
                            // Since args has nothing in it, submit this with only the format string.
                            logRecord = CreateLogRecord(level, cat, format);
                        }
                        LogManager.QueueOrSendToLog(this, logRecord, isToSuppressTraceOutput: false);
                    }
                }
#if !NETFX_CORE
                catch (ThreadAbortException)
                {
                    // These can happen if the app is attempting to log something as it shuts down.
                }
#endif
                catch (Exception x)
                {
                    string formatString = StringLib.AsString(format);
                    var sb = new StringBuilder("Handled a " + StringLib.ExceptionNameShortened(x) + " within Logger.Log(" + level + ", format, args) ");
                    if (x is FormatException)
                    {
                        // These can be generated when the args has the wrong number of items (does not match the number of format fields),
                        // so provide details to the developer here.
                        sb.Append("where format is ").Append(StringLib.Shortened(formatString, 80));
                        sb.Append(" and args is ");
                        if (args == null)
                        {
                            sb.Append("null");
                        }
                        else
                        {
                            sb.Append(" of length ").Append(args.Length);
                        }
                    }
                    else
                    {
                        sb.Append("where format,args evaluates to ").Append(StringLib.Shortened(formatString, 80));
                    }
                    sb.AppendLine();
                    //CBL This makes a *very* verbose output! Improve this.
                    sb.Append(StringLib.ExceptionDetails(x, true));

                    //CBL  We definitely do not need both sb and x. Too verbose!
                    NutUtil.WriteToConsoleAndInternalLog(sb.ToString(), x);
                    LogManager.RaiseLoggingFaultOccurred(this, null, sb.ToString(), x, null);

                    // Throw a LoggingException to report this, unless we are suppressing exceptions.
                    if (!LogManager.Config.IsSuppressingExceptions)
                    {
                        throw new LoggingException(sb.ToString(), level, formatString, x);
                    }
                }
            }
        }

        /// <summary>
        /// Output a log-record containing the given log-message (textToLog), at log-level Infomation.
        /// This is redundant -- it's simply an ultra-simplification for when you don't care about the level and want to log a simple string.
        /// </summary>
        /// <param name="textToLog">the string comprising the message to log</param>
        /// <exception cref="LoggingException">all Exceptions are rethrown as a <see cref="LoggingException"/></exception>
        public void Log(string textToLog)
        {
            //CBL Needs unit-tests.
            LogLevel level = LogLevel.Infomation;
            LogCategory cat = LogCategory.Empty;
            Log(level: level, cat: cat, textToLog: textToLog, isToSuppressTraceOutput: false);
        }
        #endregion Log

        #region LogTrace, LogTraceWithContext
        /// <summary>
        /// Log a message at level <c>Trace</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogTrace(string textToLog)
        {
#if TRACE
            Log(LogLevel.Trace, LogCategory.Empty, textToLog, false);
#endif
        }

        /// <summary>
        /// Log a message at level <c>Trace</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogTrace(LogCategory cat, string textToLog)
        {
#if TRACE
            Log(LogLevel.Trace, cat, textToLog, false);
#endif
        }

        /// <summary>
        /// Log a message at level <c>Trace</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="catEnum">an enumeration-type value that specifies which category</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        [Conditional("TRACE")]
        public void LogTrace(System.Enum catEnum, string textToLog)
        {
            //CBL Issue: Here, PRE_6 really should denote < C# 6.0, not the version of .NET
            // That needs to be reconsidered.
            // Also, do we really want to be calling GetCategory every time here?
#if PRE_6
            string categoryName = catEnum.ToString();
#else
            string categoryName = nameof(catEnum);
#endif
            LogCategory cat = LogManager.GetCategory(categoryName);
            Log(LogLevel.Trace, cat, textToLog, false);
        }

        /// <summary>
        /// Log a message at level <c>Trace</c>, the message being the given object-array and expressed using the given string format.
        /// </summary>
        /// <param name="format">the format to use to express the message (as in String.Format)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [StringFormatMethod("format")]
        public void LogTrace(string format, params object[] args)
        {
#if TRACE
            //CBL
            var cat = LogCategory.Empty;

            LogStringFormat(LogLevel.Trace, cat, format, args);
            // Log( LogLevel.Trace, LogCategory.Empty, format, args );
#endif
        }

        /// <summary>
        /// Log a message at level <c>Trace</c>, the message being the given object-array and expressed using the given string format.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="format">the format to use to express the message (as in String.Format)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [Conditional("TRACE")]
        [StringFormatMethod("format")]
        public void LogTrace(LogCategory cat, string format, params object[] args)
        {
            LogStringFormat(LogLevel.Trace, cat, format, args);
        }

#if !PRE_5
        /// <summary>
        /// Log a message at level <c>Trace</c>, and add source-code trace information (.NET 4.5 and up).
        /// You provide only a value for the objectMessage parameter --
        /// let the compiler provide the arguments to the memberName, sourceFilePath, and sourceLineNumber parameters.
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
        /// You only provide the value for objectMessage; the compiler inserts all of the other values.
        /// In this way, you automatically get information written to the log that identifies exactly where this was called.
        /// </example>
        [Conditional("TRACE")]
        public void LogTraceWithContext(string textToLog,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string sourceFilePath = "",
                                         [CallerLineNumber] int sourceLineNumber = 0)
        {
            LogWithContext(LogLevel.Trace, LogCategory.Empty, textToLog, memberName, sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Log a message at level <c>Trace</c>, and add source-code trace information (.NET 4.5 and up).
        /// You provide only a value for the objectMessage parameter --
        /// let the compiler provide the arguments to the memberName, sourceFilePath, and sourceLineNumber parameters.
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
        /// You only provide the value for objectMessage; the compiler inserts all of the other values.
        /// In this way, you automatically get information written to the log that identifies exactly where this was called.
        /// </example>
        [Conditional("TRACE")]
        public void LogTraceWithContext(LogCategory cat,
                                         string textToLog,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string sourceFilePath = "",
                                         [CallerLineNumber] int sourceLineNumber = 0)
        {
            LogWithContext(LogLevel.Trace, cat, textToLog, memberName, sourceFilePath, sourceLineNumber);
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
            var stackTrace = new StackTrace(fNeedFileInfo: true);
            var frame1 = stackTrace.GetFrame(1);
            string methodName = frame1.GetMethod().Name;

            LogWithContext( LogLevel.Trace, LogCategory.Empty, textToLog, methodName, frame1.GetFileName(), frame1.GetFileLineNumber() );
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
            var stackTrace = new StackTrace(fNeedFileInfo: true);
            var frame1 = stackTrace.GetFrame(1);
            string methodName = frame1.GetMethod().Name;

            LogWithContext( LogLevel.Trace, cat, textToLog, methodName, frame1.GetFileName(), frame1.GetFileLineNumber() );
        }
#endif
        #endregion LogTrace, LogTraceWithContext

        #region LogDebug, LogDebugWithContext
        /// <summary>
        /// Log a message at level <c>Debug</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogDebug(string textToLog)
        {
            //CBL Do I want to get it here also?
            if (LogManager.Config.IsLoggingEnabled)
            {
                Log(LogLevel.Debug, LogCategory.Empty, textToLog, false);
            }
        }

        /// <summary>
        /// Log a message at level <c>Debug</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogDebug(LogCategory cat, string textToLog)
        {
            if (LogManager.Config.IsLoggingEnabled)
            {
                Log(LogLevel.Debug, cat: cat, textToLog: textToLog, isToSuppressTraceOutput: false);
            }
        }

        /// <summary>
        /// Log a message at level <c>Debug</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="catEnum">an enumeration-type value that specifies which category</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogDebug(System.Enum catEnum, string textToLog)
        {
            if (LogManager.Config.IsLoggingEnabled)
            {
                string categoryName = nameof(catEnum);
                LogCategory cat = LogManager.GetCategory(categoryName);
                Log(LogLevel.Debug, cat: cat, textToLog: textToLog, isToSuppressTraceOutput: false);
            }
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
        [StringFormatMethod("format")]
        public void LogDebug(string format, params object[] args)
        {
            LogStringFormat(LogLevel.Debug, LogCategory.Empty, format, args);
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
        [StringFormatMethod("format")]
        public void LogDebug(LogCategory cat, string format, params object[] args)
        {
            LogStringFormat(LogLevel.Debug, cat, format, args);
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
        public void LogDebugWithContext(string textToLog,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string sourceFilePath = "",
                                         [CallerLineNumber] int sourceLineNumber = 0)
        {
            LogWithContext(LogLevel.Debug, LogCategory.Empty, textToLog, memberName, sourceFilePath, sourceLineNumber);
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
        public void LogDebugWithContext(LogCategory cat,
                                         string textToLog,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string sourceFilePath = "",
                                         [CallerLineNumber] int sourceLineNumber = 0)
        {
            LogWithContext(LogLevel.Debug, cat, textToLog, memberName, sourceFilePath, sourceLineNumber);
        }
#else
        /// <summary>
        /// Log a message at level "Debug", and add source-code trace information including the calling method, source-file path and line-number.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogDebugWithContext( string textToLog )
        {
            var stackTrace = new StackTrace(fNeedFileInfo: true);
            var frame1 = stackTrace.GetFrame(1);
            string methodName = frame1.GetMethod().Name;

            LogWithContext( LogLevel.Debug, LogCategory.Empty, textToLog, methodName, frame1.GetFileName(), frame1.GetFileLineNumber() );
        }

        /// <summary>
        /// Log a message at level "Debug", and add source-code trace information including the calling method, source-file path and line-number.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogDebugWithContext( LogCategory cat, string textToLog )
        {
            var stackTrace = new StackTrace(fNeedFileInfo: true);
            var frame1 = stackTrace.GetFrame(1);
            string methodName = frame1.GetMethod().Name;

            LogWithContext( LogLevel.Debug, cat, textToLog, methodName, frame1.GetFileName(), frame1.GetFileLineNumber() );
        }
#endif

        #endregion

        #region LogInfo, LogInfoWithContext

        /// <summary>
        /// Log a message at level <c>Infomation</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="textToLog">the string that represents the Message to log</param>
        public void LogInfo(string textToLog)
        {
            Log(LogLevel.Infomation, LogCategory.Empty, textToLog, false);
        }

        /// <summary>
        /// Log a message at level <c>Infomation</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the Message to log</param>
        public void LogInfo(LogCategory cat, string textToLog)
        {
            Log(level: LogLevel.Infomation, cat: cat, textToLog: textToLog, isToSuppressTraceOutput: false);
        }

        /// <summary>
        /// Log a message at level <c>Infomation</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="catEnum">an enumeration-type value that specifies which category</param>
        /// <param name="textToLog">the string that represents the Message to log</param>
        public void LogInfo(System.Enum catEnum, string textToLog)
        {
            string categoryName = nameof(catEnum);
            LogCategory cat = LogManager.GetCategory(categoryName);
            Log(level: LogLevel.Infomation, cat: cat, textToLog: textToLog, isToSuppressTraceOutput: false);
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
        [StringFormatMethod("format")]
        public void LogInfo(string format, params object[] args)
        {
            LogStringFormat(LogLevel.Infomation, LogCategory.Empty, format, args);
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
        [StringFormatMethod("format")]
        public void WriteLine(string format, params object[] args)
        {
            LogStringFormat(LogLevel.Infomation, LogCategory.Empty, format, args);
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
        [StringFormatMethod("format")]
        public void LogInfo(LogCategory cat, string format, params object[] args)
        {
            LogStringFormat(LogLevel.Infomation, cat, format, args);
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
        public void LogInfoWithContext(string textToLog,
                                        [CallerMemberName] string memberName = "",
                                        [CallerFilePath] string sourceFilePath = "",
                                        [CallerLineNumber] int sourceLineNumber = 0)
        {
            LogWithContext(LogLevel.Infomation, LogCategory.Empty, textToLog, memberName, sourceFilePath, sourceLineNumber);
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
        public void LogInfoWithContext(LogCategory cat,
                                        string textToLog,
                                        [CallerMemberName] string memberName = "",
                                        [CallerFilePath] string sourceFilePath = "",
                                        [CallerLineNumber] int sourceLineNumber = 0)
        {
            LogWithContext(LogLevel.Infomation, cat, textToLog, memberName, sourceFilePath, sourceLineNumber);
        }
#else
        /// <summary>
        /// Log a message at level <c>Info</c>, and add source-code trace information including the calling method, source-file path and line-number.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogInfoWithContext( string textToLog )
        {
            var stackTrace = new StackTrace(fNeedFileInfo: true);
            var frame1 = stackTrace.GetFrame(1);
            string methodName = frame1.GetMethod().Name;

            LogWithContext( LogLevel.Info, LogCategory.Empty, textToLog, methodName, frame1.GetFileName(), frame1.GetFileLineNumber() );
        }

        /// <summary>
        /// Log a message at level <c>Info</c>, and add source-code trace information including the calling method, source-file path and line-number.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogInfoWithContext( LogCategory cat, string textToLog )
        {
            var stackTrace = new StackTrace(fNeedFileInfo: true);
            var frame1 = stackTrace.GetFrame(1);
            string methodName = frame1.GetMethod().Name;

            LogWithContext( LogLevel.Info, cat, textToLog, methodName, frame1.GetFileName(), frame1.GetFileLineNumber() );
        }
#endif
        #endregion LogInfo, LogInfoWithContext

        #region LogWarning, LogWarningWithContext
        /// <summary>
        /// Log a message at level <c>Warning</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log, by using it's ToString method</param>
        public void LogWarning(string textToLog)
        {
            Log(LogLevel.Warning, LogCategory.Empty, textToLog, false);
        }

        /// <summary>
        /// Log a message at level <c>Warning</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log, by using it's ToString method</param>
        public void LogWarning(LogCategory cat, string textToLog)
        {
            Log(LogLevel.Warning, cat, textToLog, false);
        }

        /// <summary>
        /// Log a message at level <c>Warning</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="catEnum">an enumeration-type value that specifies which category</param>
        /// <param name="textToLog">the string that represents the message to log, by using it's ToString method</param>
        public void LogWarning(System.Enum catEnum, string textToLog)
        {
            string categoryName = nameof(catEnum);
            LogCategory cat = LogManager.GetCategory(categoryName);
            Log(LogLevel.Warning, cat, textToLog, false);
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
        [StringFormatMethod("format")]
        public void LogWarning(string format, params object[] args)
        {
            LogStringFormat(LogLevel.Warning, LogCategory.Empty, format, args);
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
        [StringFormatMethod("format")]
        public void LogWarning(LogCategory cat, string format, params object[] args)
        {
            LogStringFormat(LogLevel.Warning, cat, format, args);
        }

        /// <summary>
        /// Log a message at level <c>Warning</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log, by using it's ToString method</param>
        public void Warn(string textToLog)
        {
            Log(LogLevel.Warning, LogCategory.Empty, textToLog, false);
        }

        /// <summary>
        /// Log a message at level <c>Warning</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log, by using it's ToString method</param>
        public void Warn(LogCategory cat, string textToLog)
        {
            Log(LogLevel.Warning, cat, textToLog, false);
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
        [StringFormatMethod("format")]
        public void Warn(string format, params object[] args)
        {
            LogStringFormat(LogLevel.Warning, LogCategory.Empty, format, args);
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
        [StringFormatMethod("format")]
        public void Warn(LogCategory cat, string format, params object[] args)
        {
            LogStringFormat(LogLevel.Warning, cat, format, args);
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
        public void LogWarningWithContext(string textToLog,
                                           [CallerMemberName] string memberName = "",
                                           [CallerFilePath] string sourceFilePath = "",
                                           [CallerLineNumber] int sourceLineNumber = 0)
        {
            LogWithContext(LogLevel.Warning, LogCategory.Empty, textToLog, memberName, sourceFilePath, sourceLineNumber);
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
        public void LogWarningWithContext(LogCategory cat,
                                           string textToLog,
                                           [CallerMemberName] string memberName = "",
                                           [CallerFilePath] string sourceFilePath = "",
                                           [CallerLineNumber] int sourceLineNumber = 0)
        {
            LogWithContext(LogLevel.Warning, cat, textToLog, memberName, sourceFilePath, sourceLineNumber);
        }
#else
        /// <summary>
        /// Log a message at level <c>Warn</c>, and add source-code trace information including the calling method, source-file path and line-number.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogWarningWithContext( string textToLog )
        {
            var stackTrace = new StackTrace(fNeedFileInfo: true);
            var frame1 = stackTrace.GetFrame(1);
            string methodName = frame1.GetMethod().Name;

            LogWithContext( LogLevel.Warn, LogCategory.Empty, textToLog, methodName, frame1.GetFileName(), frame1.GetFileLineNumber() );
        }

        /// <summary>
        /// Log a message at level <c>Warn</c>, and add source-code trace information including the calling method, source-file path and line-number.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogWarningWithContext( LogCategory cat, string textToLog )
        {
            var stackTrace = new StackTrace(fNeedFileInfo: true);
            var frame1 = stackTrace.GetFrame(1);
            string methodName = frame1.GetMethod().Name;

            LogWithContext( LogLevel.Warn, cat, textToLog, methodName, frame1.GetFileName(), frame1.GetFileLineNumber() );
        }
#endif
        #endregion LogWarning, LogWarningWithContext

        #region LogError, LogErrorWithContext
        /// <summary>
        /// Log a message at level <c>Error</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogError(string textToLog)
        {
            Log(LogLevel.Error, LogCategory.Empty, textToLog, false);
        }

        /// <summary>
        /// Log a message at level <c>Error</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogError(LogCategory cat, string textToLog)
        {
            Log(LogLevel.Error, cat, textToLog, false);
        }

        public void LogError(System.Enum category, string whatToLog)
        {
            //CBL For C# > 6.0, using nameof
            string categoryName = category.ToString();
            LogCategory cat = LogManager.GetCategory(categoryName);
            LogError(cat, whatToLog);
            //  category.GetType()
            //category.GetType().GetEnumNames()
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
        [StringFormatMethod("format")]
        public void LogError(string format, params object[] args)
        {
            LogStringFormat(LogLevel.Error, LogCategory.Empty, format, args);
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
        [StringFormatMethod("format")]
        public void LogError(LogCategory cat, string format, params object[] args)
        {
            LogStringFormat(LogLevel.Error, cat, format, args);
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
        public void LogErrorWithContext(string textToLog,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string sourceFilePath = "",
                                         [CallerLineNumber] int sourceLineNumber = 0)
        {
            LogWithContext(LogLevel.Error, LogCategory.Empty, textToLog, memberName, sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Log a message at level <c>Error</c>, and add source-code trace information (.Net 4.5 and up).
        /// You would only provide a value for the textToLog parameter --
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
        public void LogErrorWithContext(LogCategory cat,
                                         string textToLog,
                                         [CallerMemberName] string memberName = "",
                                         [CallerFilePath] string sourceFilePath = "",
                                         [CallerLineNumber] int sourceLineNumber = 0)
        {
            LogWithContext(LogLevel.Error, cat, textToLog, memberName, sourceFilePath, sourceLineNumber);
        }
#else
        /// <summary>
        /// Log a message at level <c>Error</c>, and add source-code trace information including the calling method, source-file path and line-number.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogErrorWithContext( string textToLog )
        {
            var stackTrace = new StackTrace(fNeedFileInfo: true);
            var frame1 = stackTrace.GetFrame(1);
            string methodName = frame1.GetMethod().Name;

            LogWithContext( LogLevel.Error, LogCategory.Empty, textToLog, methodName, frame1.GetFileName(), frame1.GetFileLineNumber() );
        }

        /// <summary>
        /// Log a message at level <c>Error</c>, and add source-code trace information including the calling method, source-file path and line-number.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogErrorWithContext( LogCategory cat, string textToLog )
        {
            var stackTrace = new StackTrace(fNeedFileInfo: true);
            var frame1 = stackTrace.GetFrame(1);
            string methodName = frame1.GetMethod().Name;

            LogWithContext( LogLevel.Error, cat, textToLog, methodName, frame1.GetFileName(), frame1.GetFileLineNumber() );
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
            bool includeNewlines = !LogManager.Config.IsFileOutputSpreadsheetCompatible;
            string message = StringLib.ExceptionDetails(x: ex,
                                                               includeNewlines: includeNewlines,
                                                               additionalInformation: additionalInformation,
                                                               memberName: memberName,
                                                               sourceFilePath: sourceFilePath,
                                                               sourceLineNumber: sourceLineNumber,
                                                               showStackInformation: LogManager.Config.IsToShowStackTraceForExceptions);
            Log(LogLevel.Error, LogCategory.CatExceptions, message, false);
            LogManager.ThrowExceptionWasLogged(logger: this, exception: ex, additionalInformation: additionalInformation);
        }
#else
        // This is for < .NET 4.5

        /// <summary>
        /// Log a message at level "Error" which consists of information concerning the given Exception.
        /// </summary>
        /// <param name="exception">the Exception to describe</param>
        public void LogError( Exception exception )
        {
            // If I just reused this other overload of LogException, the stack-trace information would be off by one level of method-call.
            //LogException( exception, null );
            // For .NET 4.0, which does not have the CallerMemberName and other stuff..
            var stackTrace = new StackTrace(true);
            var frame1 = stackTrace.GetFrame(1);
            var sourceFilePath = frame1.GetFileName();
            int sourceLineNumber = frame1.GetFileLineNumber();
            string methodName = frame1.GetMethod().Name;

            bool includeNewlines = !LogManager.Config.IsFileOutputSpreadsheetCompatible;
            string message = StringLib.ExceptionDetails( exception,
                                                         includeNewlines,
                                                         null,
                                                         methodName,
                                                         sourceFilePath,
                                                         sourceLineNumber,
                                                         LogManager.Config.IsToShowStackTraceForExceptions);
            Log( level: LogLevel.Error, cat: LogCategory.CatExceptions, textToLog: message, isToSuppressTraceOutput: false );
            LogManager.ThrowExceptionWasLogged( logger: this, exception: exception, additionalInformation: null );
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
            // For .NET 4.0, which does not have the CallerMemberName and other stuff..
            var stackTrace = new StackTrace(true);
            var frame1 = stackTrace.GetFrame(1);
            var sourceFilePath = frame1.GetFileName();
            int sourceLineNumber = frame1.GetFileLineNumber();
            string methodName = frame1.GetMethod().Name;

            bool includeNewlines = !LogManager.Config.IsFileOutputSpreadsheetCompatible;
            string message = StringLib.ExceptionDetails(exception,
                                                        includeNewlines,
                                                        additionalInformation,
                                                        methodName,
                                                        sourceFilePath,
                                                        sourceLineNumber,
                                                        LogManager.Config.IsToShowStackTraceForExceptions);
            Log( level: LogLevel.Error, cat: LogCategory.CatExceptions, textToLog: message, isToSuppressTraceOutput: false );
            LogManager.ThrowExceptionWasLogged( logger: this, exception: exception, additionalInformation: additionalInformation );
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
            string additionalInfor;
            if (format == null)
            {
                additionalInfor = null;
            }
            else
            {
                additionalInfor = String.Format(format, args);
            }
            bool includeNewlines = !LogManager.Config.IsFileOutputSpreadsheetCompatible;
            LogStringFormat(LogLevel.Error,
                             LogCategory.CatExceptions,
                             StringLib.ExceptionDetails(ex,
                                                         includeNewlines,
                                                         additionalInfor,
                                                         null,
                                                         null,
                                                         0,
                                                         LogManager.Config.IsToShowStackTraceForExceptions));
            LogManager.ThrowExceptionWasLogged(logger: this, exception: ex, additionalInformation: additionalInfor);
        }

        #endregion LogError, LogErrorWithContext

        #region LogCritical, LogCriticalWithContext
        /// <summary>
        /// Log a message at level <c>Critical</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogCritical(string textToLog)
        {
            Log(LogLevel.Critical, LogCategory.Empty, textToLog, false);
        }

        /// <summary>
        /// Log a message at level <c>Critical</c>, calling the <c>ToString</c> method of the given object to produce the message.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogCritical(LogCategory cat, string textToLog)
        {
            Log(LogLevel.Critical, cat, textToLog, false);
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
        [StringFormatMethod("format")]
        public void LogCritical(string format, params object[] args)
        {
            LogStringFormat(LogLevel.Critical, LogCategory.Empty, format, args);
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
        [StringFormatMethod("format")]
        public void LogCritical(LogCategory cat, string format, params object[] args)
        {
            LogStringFormat(LogLevel.Critical, cat, format, args);
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
        public void LogCriticalWithContext(LogCategory cat,
                                            string textToLog,
                                            [CallerMemberName] string memberName = "",
                                            [CallerFilePath] string sourceFilePath = "",
                                            [CallerLineNumber] int sourceLineNumber = 0)
        {
            LogWithContext(LogLevel.Critical, cat, textToLog, memberName, sourceFilePath, sourceLineNumber);
        }
#else
        /// <summary>
        /// Log a message at level <c>Critical</c>, and add source-code trace information including the calling method, source-file path and line-number.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogCriticalWithContext( string textToLog )
        {
            var stackTrace = new StackTrace(fNeedFileInfo: true);
            var frame1 = stackTrace.GetFrame(1);
            string methodName = frame1.GetMethod().Name;

            LogWithContext( LogLevel.Critical, LogCategory.Empty, textToLog, methodName, frame1.GetFileName(), frame1.GetFileLineNumber() );
        }

        /// <summary>
        /// Log a message at level <c>Critical</c>, and add source-code trace information including the calling method, source-file path and line-number.
        /// </summary>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="textToLog">the string that represents the message to log</param>
        public void LogCriticalWithContext( LogCategory cat, string textToLog )
        {
            var stackTrace = new StackTrace(fNeedFileInfo: true);
            var frame1 = stackTrace.GetFrame(1);
            string methodName = frame1.GetMethod().Name;

            LogWithContext( LogLevel.Critical, cat, textToLog, methodName, frame1.GetFileName(), frame1.GetFileLineNumber() );
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
        [Conditional("TRACE")]
        public void LogMethodBegin(object objectMessage = null,
                                    [CallerMemberName] string memberName = "",
                                    [CallerLineNumber] int sourceLineNumber = 0)
        {
            //CBL Also test for both Debug and Release.
            string methodName = memberName;
            //int sourceFileLine = sourceLineNumber;
#if NETFX_CORE
            var stackTrace = new StackTrace( exception: null, needFileInfo: true );
#else
            var stackTrace = new StackTrace(fNeedFileInfo: true);
#endif
            //var frame1 = stackTrace.GetFrame( 1 );
            // int lineNumber = frame1.GetFileLineNumber();
            int lineNumber = sourceLineNumber;
            //methodName = frame1.GetMethod().Name;

            string logContent;
            if (LogManager.Config.IsToLogMethodBeginWithClassName)
            {
#if NETFX_CORE
                var callerType = stackTrace.GetFrames()[1].GetMethod().DeclaringType;
#else
                var callerType = stackTrace.GetFrame(1).GetMethod().DeclaringType;
#endif
                string className = callerType?.Name;

                logContent = "begin " + className + "." + methodName + " at line " + lineNumber;
            }
            else
            {
                logContent = "begin " + methodName + " at line " + lineNumber;
            }

            LogCategory cat = LogCategory.MethodTrace;

            if (objectMessage != null)
            {
                LogTrace(cat, logContent + ", " + objectMessage);
            }
            else
            {
                LogTrace(cat, logContent);
            }
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
            var stackTrace = new StackTrace(fNeedFileInfo: true);
            var frame1 = stackTrace.GetFrame(1);
            int lineNumber = frame1.GetFileLineNumber();
            string methodName = frame1.GetMethod().Name;

            string lineNumberText;
            if (lineNumber == 0)
            {
                lineNumberText = String.Empty;
            }
            else
            {
                lineNumberText = " at line " + lineNumber;
            }

            string logContent;
            if (LogManager.Config.IsToLogMethodBeginWithClassName)
            {
                var callerType = stackTrace.GetFrame(1).GetMethod().DeclaringType;
                string className = callerType?.Name;

                logContent = "begin " + className + "." + methodName + lineNumberText;
            }
            else
            {
                logContent = "begin " + methodName + lineNumberText;
            }

            if (objectMessage != null)
            {
                LogTrace( LogCategory.MethodTrace, logContent + ", " + objectMessage );
            }
            else
            {
                LogTrace( LogCategory.MethodTrace, logContent );
            }
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
        [Conditional("TRACE")]
        public void LogMethodEnd(object objectMessage = null, [CallerMemberName] string memberName = "")
        {
            string methodName = memberName;
            string logMessage;
            if (objectMessage != null)
            {
                logMessage = "end " + methodName + "  " + objectMessage + ".";
            }
            else
            {
                logMessage = "end " + methodName + ".";
            }
            LogTrace(LogCategory.MethodTrace, logMessage);
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
            var stackTrace = new StackTrace();
            var frame1 = stackTrace.GetFrame(1);
            string methodName = frame1.GetMethod().Name;
            string logMessage;
            if (objectMessage != null)
            {
                logMessage = "end " + methodName + "  " + objectMessage + ".";
            }
            else
            {
                logMessage = "end " + methodName + ".";
            }
            LogTrace( LogCategory.MethodTrace, logMessage );
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
        [Conditional("TRACE")]
        public void LogMethodEndErrorIfFalse(bool isOkay,
                                              [CallerMemberName] string memberName = "",
                                              [CallerFilePath] string sourceFilePath = "",
                                              [CallerLineNumber] int sourceLineNumber = 0)
        {
            string methodName = memberName;
            if (isOkay)
            {
                LogTrace(LogCategory.MethodTrace, "end " + methodName + " returning true.");
            }
            else
            {
                LogError(LogCategory.MethodTrace, "end " + methodName + " returning false.");
            }
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
            var stackTrace = new StackTrace();
            var frame1 = stackTrace.GetFrame(1);
            string methodName = frame1.GetMethod().Name;
            if (isOkay)
            {
                LogTrace( LogCategory.MethodTrace, "end " + methodName + " returning true." );
            }
            else
            {
                LogError( LogCategory.MethodTrace, "end " + methodName + " returning false." );
            }
        }
#endif
        #endregion

        #endregion LogMethodBegin and End

        #region AddToPresentationTraceListeners
        /// <summary>
        /// Start logging Visual Studio PresentationTraceSources trace-output such as WPF binding errors.
        /// </summary>
        /// <param name="levelToLogAt">the log-level to use when logging trace output</param>
        public void AddToPresentationTraceListeners(LogLevel levelToLogAt)
        {
            //CBL Do we have duplicate such functionality?
            // Capture any WPF binding-error trace output..
            //TODO   Must implement this!
            //  VisualStudioLib.AddLoggerToPresentationTraceListeners( destinationLogger: this, levelToLogAt: levelToLogAt );
        }
        #endregion

        #region time-interval logging methods

        #region SetTimeReference
        /// <summary>
        /// Call this method to start measuring a time interval, to establish your zero-reference.
        /// A subsequent call to LogElapsedTime will log the amount of time that has passed since you called this method.
        /// </summary>
        /// <returns>a reference to this logger so that methods may be chained together</returns>
        public Logger SetTimeReference()
        {
            _timeStamp = DateTime.UtcNow;
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
        /// <remarks>
        /// If you don't call SetTimeReference before calling this method, then it does the work of SetTimeReference
        /// and indicates the elapsed time as zero.
        /// </remarks>
        public Logger LogElapsedTime(LogCategory cat, string comment)
        {
            try
            {
                var sb = new StringBuilder();
                if (StringLib.HasSomething(comment))
                {
                    sb.Append(comment);
                }

                if (_timeStamp == DateTime.MinValue)
                {
                    //NutUtil.WriteToConsole("You need to call method Logger.SetTimeReference before making a call to LogElapsedTime.");
                    sb.Append(" (time-reference set)");
                }
                else
                {
                    DateTime timeNow = DateTime.UtcNow;
                    TimeSpan timeInterval = timeNow - _timeStamp;
                    sb.Append(" :  Elapsed Time is ");
                    if ((int)timeInterval.TotalSeconds == 0)
                    {
                        sb.Append(((int)timeInterval.TotalMilliseconds).ToString()).Append(" milliseconds");
                    }
                    else if ((int)timeInterval.TotalMinutes == 0)
                    {
                        sb.Append(((int)timeInterval.Seconds).ToString()).Append(".").Append(((int)timeInterval.Milliseconds).ToString()).Append(" seconds");
                    }
                    else
                    {
                        sb.Append(new DateTime(timeInterval.Ticks).ToString("HH:mm:ss.fff"));
                    }
                }
                LogInfo(cat, sb.ToString());
            }
            catch (Exception x)
            {
                LogManager.HandleInternalFault(x, null, "Unable to log elapsed-time.");
            }
            return this;
        }

        /// <summary>
        /// Send an (Infomation-level) log output that simply indicates how much time has elapsed
        /// since the last call to SetTimeReference, and ALSO since the last call this this method.
        /// </summary>
        /// <param name="comment">a note to include in the log output</param>
        /// <returns>a reference to this logger so that methods may be chained together</returns>
        /// <remarks>
        /// If you don't call SetTimeReference before calling this method, then it does the work of SetTimeReference
        /// and indicates the elapsed time as zero.
        /// </remarks>
        public Logger LogElapsedTime2(string comment)
        {
            try
            {
                var sb = new StringBuilder();
                if (StringLib.HasSomething(comment))
                {
                    sb.Append(comment);
                }

                if (_timeStamp == DateTime.MinValue)
                {
                    sb.Append(" (time-reference set)");
                }
                else
                {
                    DateTime timeNow = DateTime.UtcNow;
                    TimeSpan timeInterval = timeNow - _timeStamp;
                    sb.Append(" :  Elapsed Time is ");
                    if ((int)timeInterval.TotalSeconds == 0)
                    {
                        sb.Append(((int)timeInterval.TotalMilliseconds).ToString()).Append(" milliseconds");
                    }
                    else if ((int)timeInterval.TotalMinutes == 0)
                    {
                        sb.Append(((int)timeInterval.Seconds).ToString()).Append(".").Append(((int)timeInterval.Milliseconds).ToString()).Append(" seconds");
                    }
                    else
                    {
                        sb.Append(new DateTime(timeInterval.Ticks).ToString("HH:mm:ss.fff"));
                    }
                    // additional part, to calculat the time-delta from the last call of this method..
                    if (LogManager._repeatingTimeReference == null)
                    {
                        LogManager._repeatingTimeReference = new Stopwatch();
                        sb.Append(", delta 0");
                    }
                    else
                    {
                        sb.Append(", delta = ");
                        long ms = LogManager._repeatingTimeReference.ElapsedMilliseconds;
                        if (ms < 1000)
                        {
                            sb.Append(((int)ms).ToString()).Append("ms");
                        }
                        else
                        {
                            TimeSpan timeInterval2 = LogManager._repeatingTimeReference.Elapsed;
                            sb.Append(String.Format("{0:0.000}", timeInterval2.TotalSeconds));
                            sb.Append("s");
                        }
                        LogManager._repeatingTimeReference.Reset();
                        LogManager._repeatingTimeReference.Start();
                    }
                }
                LogInfo(sb.ToString());
            }
            catch (Exception x)
            {
                LogManager.HandleInternalFault(x, null, "Unable to log elapsed-time.");
            }
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
            _isEnabled_override = null;
            LowestLevelThatIsEnabled_Override = null;
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
        //public bool? IsEnabled
        //{
        // CBL  Carefully reassess this, and delete if not used.
        //    get { return _isEnabled_override; }
        //    set { _isEnabled_override = value; }
        //}

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
                if (_isEnabled_override.HasValue)
                {
                    return _isEnabled_override.Value;
                }
                else
                {
                    return LogManager.Config.IsLoggingEnabled;
                }
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
        public Logger Enable()
        {
            _isEnabled_override = true;
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
        public Logger Disable()
        {
            _isEnabled_override = false;
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
        public Logger EnableAllLevels()
        {
            _lowestLevelThatIsEnabled_override = default(LogLevel);
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
        public Logger EnableLevelsDownTo(LogLevel level)
        {
            _lowestLevelThatIsEnabled_override = level;
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
            get { return _lowestLevelThatIsEnabled_override; }
            set { _lowestLevelThatIsEnabled_override = value; }
        }
        #endregion

        #region IsEnabled
        /// <summary>
        /// Return true if output is enabled for this logger AND according to LogManager, and for this given LogLevel
        /// </summary>
        /// <param name="level">The LogLevel to check for</param>
        /// <returns>true only if this logger is enabled and also enabled for the given LogLevel</returns>
        public bool IsEnabled(LogLevel level)
        {
            if (IsEnabled_EffectiveValue)
            {
                if (_lowestLevelThatIsEnabled_override.HasValue)
                {
                    return level >= _lowestLevelThatIsEnabled_override.Value;
                }
                else
                {
                    return level >= LogManager.Config.LowestLevelThatIsEnabled;
                }
            }
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
        public bool IsEffectivelyEnabledForLevelAndCategory(LogLevel level, LogCategory cat)
        {
            if (cat == null)
            {
                //throw new ArgumentNullException( paramName: nameof(cat) );
                //CBL Temporary
                cat = LogCategory.Empty;
            }
            bool answer;

            if (!LogManager.Config.IsLoggingEnabled)
            {
                answer = false;
            }
            else
            {
                // Cat overrides Logger and Config.
                if (cat != LogCategory.Empty && cat.IsEnabled.HasValue)
                {
                    if (cat.IsEnabled == true)
                    {
                        // The cat is enabled, which overrides the IsEnabled on the logger and Config,
                        // so that leaves the level to consider.
                        if (cat.LowestLevelThatIsEnabled.HasValue)
                        {
                            answer = level >= cat.LowestLevelThatIsEnabled.Value;
                        }
                        else if (this.LowestLevelThatIsEnabled_Override.HasValue)
                        {
                            answer = level >= LowestLevelThatIsEnabled_Override.Value;
                        }
                        else // no overrides for level.
                        {
                            // If a category is enabled, that overrides the top-level LowestLevelThatIsEnabled.
                            //answer = level >= LogManager.Config.LowestLevelThatIsEnabled;
                            answer = true;
                        }
                    }
                    else
                    {
                        answer = false;
                    }
                }
                else // cat does not override.
                {
                    // Check this logger for overrides..
                    if (this._isEnabled_override.HasValue)
                    {
                        if (this._isEnabled_override.Value == true)
                        {
                            if (_lowestLevelThatIsEnabled_override.HasValue)
                            {
                                answer = level >= _lowestLevelThatIsEnabled_override.Value;
                            }
                            else
                            {
                                // If a logger is enabled, that overrides the top-level LowestLevelThatIsEnabled.
                                //answer = level >= LogManager.Config.LowestLevelThatIsEnabled;
                                answer = true;
                            }
                        }
                        else
                        {
                            answer = false;
                        }
                    }
                    else // neither cat nor logger override Config.
                    {
                        if (LogManager.Config.IsLoggingEnabledByDefault)
                        {
                            answer = level >= LogManager.Config.LowestLevelThatIsEnabled;
                        }
                        else
                        {
                            answer = false;
                        }
                    }
                }
            }
            return answer;
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
            get { return _isToOutputToConsole_override; }
            set { _isToOutputToConsole_override = value; }
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
                if (IsToOutputToConsole_Override.HasValue)
                {
                    return IsToOutputToConsole_Override.Value;
                }
                else
                {
                    return LogManager.Config.IsToOutputToConsole;
                }
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
        public Logger EnableConsoleOutput(bool? nullOrOverride)
        {
            _isToOutputToConsole_override = nullOrOverride;
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
        public Logger DisableConsoleOutput()
        {
            _isToOutputToConsole_override = false;
            return this;
        }

        /// <summary>
        /// This is an override of the LogManager.Config.IsToOutputToConsole property,
        /// which dictates whether output to the console is going to happen.
        /// The default is null, meaning there is no override.
        /// </summary>
        private bool? _isToOutputToConsole_override;

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
        public void LogStatistics(string logMessageToSelect)
        {
            var sb = new StringBuilder();
            if (_logAggregate == null || _logAggregate.Count == 0)
            {
                sb.Append("There are no items that have been counted to log ");
                sb.Append(logMessageToSelect);
            }
            else
            {
                sb.Append("Items for ").Append(logMessageToSelect).Append(": ");
                int nTransitions = 0;
                TimeSpan maxOnTime = TimeSpan.Zero;
                TimeSpan minOnTime = TimeSpan.MaxValue;
                TimeSpan totalOnTime = TimeSpan.Zero;
                TimeSpan maxTimeBetweenOn = TimeSpan.Zero;
                TimeSpan minTimeBetweenOn = TimeSpan.MaxValue;
                bool wasLastOn = false;
                DateTime whenOn = default(DateTime);

                for (int i = 0; i < LogAggregate.Count; i++)
                {
                    StatisticPoint p = LogAggregate[i];
                    if (p.IsBegin)
                    {
                        // If this is not the first computation of on-time,
                        // compare it to the previous statistics..
                        if (whenOn != default(DateTime))
                        {
                            TimeSpan onToOn = p.Record.When - whenOn;
                            if (onToOn > maxTimeBetweenOn)
                            {
                                maxTimeBetweenOn = onToOn;
                            }
                            if (onToOn < minTimeBetweenOn)
                            {
                                minTimeBetweenOn = onToOn;
                            }
                        }
                        whenOn = p.Record.When;
                    }
                    else
                    {
                        if (wasLastOn)
                        {
                            nTransitions++;
                            TimeSpan howLong = p.Record.When - whenOn;

                            if (howLong > maxOnTime)
                            {
                                maxOnTime = howLong;
                            }
                            if (howLong < minOnTime)
                            {
                                minOnTime = howLong;
                            }
                            totalOnTime += howLong;
                        }
                    }
                    wasLastOn = p.IsBegin;
                }
                sb.Append(nTransitions).Append(" transitions 0->1->0, ");
                sb.Append("Min-1Time = ").Append(TimeLib.AsString(howLong: minOnTime, excludeSecondsUnlessLessThanMinute: false, addSmallSpace: false));
                sb.Append(", Max-1Time = ").Append(TimeLib.AsString(howLong: maxOnTime, excludeSecondsUnlessLessThanMinute: false, addSmallSpace: false));
                double averageOnTime = totalOnTime.TotalMilliseconds / nTransitions;
                sb.Append(", Avg-1Time = ").Append(averageOnTime);

                sb.Append(";  Min-Time1To1 = ").Append(TimeLib.AsString(howLong: minTimeBetweenOn, excludeSecondsUnlessLessThanMinute: false, addSmallSpace: false));
                sb.Append(", Max-Time1To1 = ").Append(TimeLib.AsString(howLong: maxTimeBetweenOn, excludeSecondsUnlessLessThanMinute: false, addSmallSpace: false));
            }
            LogInfo(sb.ToString());
        }
        #endregion

        #region AddToStatistics
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logMessage"></param>
        /// <param name="isBegin"></param>
        public void AddToStatistics(string logMessage, bool isBegin)
        {
            var record = CreateLogRecord(LogLevel.Infomation, LogCategory.Empty, logMessage);
            StatisticPoint point = new StatisticPoint(record, isBegin);
            LogAggregate.Add(point);
        }
        #endregion

        #region ClearStatistics
        /// <summary>
        /// Empty this logger of all of the information that has accumulated by calls to <see cref="AddToStatistics"/>.
        /// </summary>
        public void ClearStatistics()
        {
            _logAggregate?.Clear();
        }
        #endregion

        private List<StatisticPoint> _logAggregate;

        private List<StatisticPoint> LogAggregate
        {
            get
            {
                if (_logAggregate == null)
                {
                    _logAggregate = new List<StatisticPoint>();
                }
                return _logAggregate;
            }
        }

        private class StatisticPoint
        {
            public StatisticPoint(LogRecord logRecord, bool isBegin)
            {
                this.Record = logRecord;
                this.IsBegin = isBegin;
            }

            public LogRecord Record { get; }

            public bool IsBegin { get; }

            #region ToString
            /// <summary>
            /// Return a string that represents the current object.
            /// </summary>
            /// <returns>A string that represents the current object.</returns>
            public override string ToString()
            {
                var sb = new StringBuilder("StatisticPoint(");
                if (this.IsBegin)
                {
                    sb.Append("1, ");
                }
                else
                {
                    sb.Append("0, ");
                }
                sb.Append(this.Record);
                sb.Append(")");
                return sb.ToString();
            }
            #endregion
        }

        #endregion statistical facilities

        #region ToString
        /// <summary>
        /// Override the ToString method to produce a more informative description of this object.
        /// </summary>
        /// <returns>A string describing this object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder("Logger(");
            if (StringLib.HasSomething(this.Name) && !this.Name.Equals(LogManager.NameOfDefaultLogger))
            {
                sb.Append("\"").Append(this.Name).Append("\",");
            }
            if (_isEnabled_override.HasValue)
            {
                if (_isEnabled_override.Value == true)
                {
                    sb.Append("enabled,");
                }
                else
                {
                    sb.Append("disabled,");
                }
            }
            if (IsToOutputToConsole_Override.HasValue)
            {
                if (IsToOutputToConsole_Override.Value)
                {
                    sb.Append("console output is ON,");
                }
                else
                {
                    sb.Append("console output is disabled,");
                }
            }
            sb.Append(")");
            return sb.ToString();
        }
        #endregion

        #region internal implementation

        #region constructors

        internal Logger()
        {
            _name = LogManager.NameOfDefaultLogger;
        }

        internal Logger(string name)
        {
            _name = name;
        }
        #endregion constructors

        #region CreateLogRecord
        /// <summary>
        /// The internal LogRecord-creation method.
        /// </summary>
        /// <param name="level">The LogLevel associated with this log-record</param>
        /// <param name="cat">the LogCategory associated with this log-record</param>
        /// <param name="theMessageToLog">This will be the Message component of this log-record</param>
        internal LogRecord CreateLogRecord(LogLevel level, LogCategory cat, string theMessageToLog)
        {
            // Compose the log record..
            string id;
            bool isInDesignMode;
            DateTime timestamp;
            string username;
            string version;
            // If testing, force certain fields to a specified value.
            if (LogManager.IsTesting)
            {
                id = LogManager.TestFacility.IdToApply;
                if (LogManager.TestFacility.UsernameToApply != null)
                {
                    username = LogManager.TestFacility.UsernameToApply;
                }
                else
                {
                    username = null;
                }
                if (LogManager.TestFacility.DesignModeToApply.HasValue)
                {
                    isInDesignMode = LogManager.TestFacility.DesignModeToApply.Value;
                }
                else
                {
                    isInDesignMode = LogManager.IsInDesignMode;
                }
                if (LogManager.TestFacility.TimestampToApply != default(DateTime))
                {
                    timestamp = LogManager.TestFacility.TimestampToApply;
                }
                else
                {
                    timestamp = DateTime.Now;
                }
                if (LogManager.TestFacility.VersionToApply != null)
                {
                    version = LogManager.TestFacility.VersionToApply;
                }
                else
                {
                    version = null;
                }
            }
            else
            {
                id = null;
                isInDesignMode = LogManager.IsInDesignMode;
                timestamp = DateTime.Now;
                username = LogManager.Config.Username;
                version = LogManager.Config.SubjectProgramVersion;
            }
            string programName = LogManager.Config.GetSubjectProgramName(null); //CBL

#if !PRE_4
            LogRecord logRecord = new LogRecord(id: id,
                                                 message: theMessageToLog,
                                                 level: level,
                                                 cat: cat,
                                                 when: timestamp,
                                                 sourceHost: LogManager.Config.SourceHostName,
                                                 sourceLogger: Name,
                                                 subjectProgramName: programName,
                                                 subjectProgramVersion: version,
                                                 threadId: SystemLib.CurrentThreadId,
                                                 user: username,
                                                 isInDesignMode: isInDesignMode);
#else
            LogRecord logRecord = new LogRecord( id,
                                                 theMessageToLog,
                                                 level,
                                                 cat,
                                                 timestamp,
                                                 LogManager.Config.SourceHostName,
                                                 Name,
                                                 programName,
                                                 version,
                                                 SystemLib.CurrentThreadId,
                                                 username,
                                                 isInDesignMode );
#endif
            return logRecord;
        }
        #endregion CreateLogRecord

        #region LogWithContext
        /// <summary>
        /// An internal helper-method for adding the Caller Information that is available with .NET 4.5 and higher,
        /// and which is otherwise arrived at under earlier versions of the .NET Framework.
        /// </summary>
        /// <param name="level">the LogLevel to log to</param>
        /// <param name="cat">You can organize your logging-output into categories by specifying this LogCategory.</param>
        /// <param name="messageObject">the object that represents the message to log</param>
        /// <param name="memberName">the class-method from which this method's parent was called</param>
        /// <param name="sourceFilePath">the file-system path of the source-code file from which this method's parent was called</param>
        /// <param name="sourceLineNumber">the line-number within the source-code file from which this method's parent was called</param>
        /// <returns>a reference to this object so that you can chain method-calls</returns>
        /// <remarks>
        /// This is not available for versions of .NET previous to version 4.5
        /// </remarks>
        private Logger LogWithContext(LogLevel level, LogCategory cat, object messageObject, string memberName, string sourceFilePath, int sourceLineNumber)
        {
            if (this.IsEffectivelyEnabledForLevelAndCategory(level, cat))
            {
                var sb = new StringBuilder(messageObject == null ? "null" : messageObject.ToString());
                sb.Append(".  method: ").Append(memberName);

                if (LogManager.Config.IsToShowSourceFile)
                {
                    sb.Append(", source-file: ").Append(sourceFilePath);
                }
                sb.Append(", line-number: ").Append(sourceLineNumber);

                try
                {
                    LogManager.QueueOrSendToLog(this, CreateLogRecord(level, cat, sb.ToString()), isToSuppressTraceOutput: false);
                }
                catch (Exception x)
                {
                    string msg = String.Format("Got a " + x.GetType() + " within Logger.LogWithContext(" + level + ", " + sb);
                    throw new LoggingException(message: msg, level: level, originalLogMessage: (messageObject == null ? "null" : messageObject.ToString()), innerException: x);
                }
            }
            return this;
        }
        #endregion

        #region fields

        /// <summary>
        /// This is the name (if any) associated with this logger in order to distinguish it from other loggers.
        /// </summary>
        private string _name;

        /// <summary>
        /// This denotes lowest-level LogLevel that is currently enabled JUST FOR THIS SPECIFIC LOGGER.
        /// The default is null, which means this logger voices no preference and the LogManager.Config
        /// is what controls.
        /// </summary>
        private LogLevel? _lowestLevelThatIsEnabled_override;

        /// <summary>
        /// This flag controls whether output from this logger is on (enabled), or shut off (false).
        /// The initial default value is null, meaning no override.
        /// </summary>
        private bool? _isEnabled_override;

        /// <summary>
        /// This is used for logging (crude) estimates of time intervals, as for performance monitoring.
        /// </summary>
        private DateTime _timeStamp = DateTime.MinValue;

        #endregion fields

        #endregion internal implementation
    }
}
