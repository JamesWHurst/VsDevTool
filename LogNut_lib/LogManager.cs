#if PRE_4
#define PRE_5
#endif
#if !PRE_4
using System.Collections.Concurrent;
#endif
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
//#if !PRE_4 && OUTPUT_SVC
//using System.IO.MemoryMappedFiles;
//using ProtoBuf;
//#endif
using System.Linq;
using System.Security.Permissions;
#if !PRE_5
using System.Runtime.CompilerServices;
#endif
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
#if !PRE_4
using System.Threading.Tasks;
#endif
using Hurst.LogNut.Util;
using System.Windows;
#if NETFX_CORE
using Windows.UI.Xaml;
#else
using System.Windows.Threading;
#endif
using Hurst.LogNut.Util.Annotations;
using Hurst.LogNut.OutputPipes;

#if INCLUDE_JSON
using Newtonsoft.Json;
#endif


// define TRACE_LOGSVC to include writing out logs of the communication with the log-servicing process.
// define INCLUDE_JSON for this project if you want to produce JSON output.
// define OUTPUT_SVC to include the code for outputing to a logging service.


namespace Hurst.LogNut
{
    /// <summary>
    /// The LogManager contains the factory methods for creating and accessing the Logger objects,
    /// and system-wide logging settings.
    /// </summary>
    public static partial class LogManager
    {
        #region events

        /// <summary>
        /// This event is raised when LogNut logs something (anything).
        /// </summary>
        /// <remarks>
        /// This event is intended to enable following the normal flow of logging, such as keeping a counter - and not to
        /// indicate that any sort of error occurred.
        /// </remarks>
        public static event EventHandler<LogEventArgs> SomethingWasLogged;

        /// <summary>
        /// This event is raised whenever any of the LogError methods are called.
        /// This is intended to enable a program to keep a running total of the number of exceptions that have been raised within it's code.
        /// Yes - if you hook both events, LogError will throw both events.
        /// </summary>
        public static event EventHandler<LogEventArgs> ExceptionWasLogged;

        /// <summary>
        /// This event is raised when LogNut has some internal or operational fault, such as being unable to write to the log file.
        /// </summary>
        public static event EventHandler<LoggingFaultEventArgs> LoggingFaultOccurred;

        #endregion events

        //CBL
        // New thought for Categories:
        // Provide a method to set which enum-type to use for categories,
        // and then the user simply calls 
        // LogInfo( CatNamed("CatNameA"), "stuff");
        // or, is it possible to provide for..
        // LogInfo( LoggingCategories.ReportPerf, "stuff"); ?

        private static System.Enum enumCategoryType;

        public static void SetCategoryTypeToBe( System.Enum yourEnumTypeForCategories )
        {
            enumCategoryType = yourEnumTypeForCategories;
        }

        private static Type enumCategoryType2;

        public static void SetCategoryTypeToBe2( Type yourEnumTypeForCategories )
        {
            if (!yourEnumTypeForCategories.IsEnum)
            {
                throw new ArgumentException( message: "This parameter requires an enum type.", paramName: nameof( yourEnumTypeForCategories ) );
            }
            enumCategoryType2 = yourEnumTypeForCategories;
        }

        //public static void LogSomeShit( System.Enum category, string whatToLog )
        //{
        //    string categoryName = category.ToString();
        //    LogCategory cat = GetCategory( categoryName );
        //    LogInfo( cat, whatToLog );
        //    //  category.GetType()
        //    //category.GetType().GetEnumNames()
        //}

        /// <summary>
        /// Get or set whether we are presently using ETW for logging.
        /// </summary>
        public static bool IsUsingETW { get; set; }

        #region GetLogger factory method
        /// <summary>
        /// Get the default logger object.
        /// </summary>
        /// <returns>a Logger object</returns>
        public static Logger GetLogger()
        {
            return GetLogger( null );
        }

        /// <summary>
        /// The primary factory-method for getting Logger objects,
        /// whether it's creating a new one or accessing one already created (via it's name).
        /// </summary>
        /// <param name="loggerName">the name to give the logger, or that of an already-created logger to retrieve (optional - if null then it is given a default name)</param>
        /// <returns>a Logger object</returns>
        /// <remarks>
        /// Calling this also results in HasBeenConfiguredProgrammatically being set to true.
        /// </remarks>
        public static Logger GetLogger( string loggerName )
        {
            TryConfiguringLogging();

            // If no name was provided, give it the default name.
            if (StringLib.HasNothing( loggerName ))
            {
                loggerName = NameOfDefaultLogger;
            }
            Logger logger;
            lock (_loggerListLockObject)
            {
                logger = GetLoggerWithName( loggerName );
                if (logger == null)
                {
                    logger = new Logger( loggerName );
                    if (_theLoggers == null)
                    {
                        _theLoggers = new List<Logger>();
                    }
                    _theLoggers.Add( logger );
                }
            }
            return logger;
        }
        #endregion

        #region GetCurrentClassLogger factory method
#if !NETFX_CORE
        /// <summary>
        /// Get the logger instance that has as it's name the name of the C# class that this method was called from. Alert: this uses reflection.
        /// </summary>
        /// <returns>a Logger object</returns>
        /// <remarks>
        /// This method is obsolete going forward with the .NET Core versions.
        /// 
        /// This returns a logger whose name reflects that of the declaring type of the code that calls this.
        /// For example, if your class Animal has method Bite which contains this code:
        /// <c>
        ///   Logger myLogger = LogManager.GetCurrentClassLogger();
        ///   Console.WriteLine("The name of this logger is: " + myLogger.Name);
        /// </c>
        /// and you run it, it would output "The name of this logger is: Animal"
        /// 
        /// If no C# declaring-type (class) is found, then the default logger is returned.
        /// 
        /// Note: Consider using GetLoggerForClass instead - it is faster.
        /// 
        /// Alert: This method uses reflection, which makes it slower than calling <see cref="GetLogger()"/> and limits it's portability.
        ///        Consider not using this if that is a concern; in fact it may be removed in a future release.
        /// </remarks>
        public static Logger GetCurrentClassLogger()
        {
            // Notes
            //   This had been returning a ISimpleLogger, but it really does need much of the full facilities of a Logger.

            var containingClass = new StackFrame(1).GetMethod().DeclaringType;
            if (containingClass == null)
            {
                return GetLogger();
            }
            else
            {
                return GetLogger( containingClass.Name );
            }
        }
#endif
        #endregion

        #region GetLoggerForClass factory method
        /// <summary>
        /// Get a logger that has as it's name the name of the C# class that is specified as the generic parameter argument.
        /// </summary>
        /// <typeparam name="T">this is the class for which the logger shall be named</typeparam>
        /// <returns>a logger with the name of the specified class</returns>
        /// <remarks>
        /// This method is faster than GetCurrentClassLogger as it does not use reflection.
        /// Benchmarking on .NET Framework 4.7 yielded roughly half the execution time as did GeetCurrentClassLogger.
        /// </remarks>
        public static Logger GetLoggerForClass<T>() where T : class
        {
            return GetLogger( typeof( T ).Name );
        }
        #endregion

        #region categories

        #region Categories
        /// <summary>
        /// Get the list of LogCategories that are defined at this point (since the last call to ResetCategories).
        /// </summary>
        public static IList<LogCategory> Categories
        {
            get
            {
                if (_categories == null)
                {
                    _categories = new List<LogCategory>();
                }
                return _categories;
            }
        }
        private static IList<LogCategory> _categories;
        #endregion

        /// <summary>
        /// Create a new LogCategory with the given name and add it to the list.
        /// </summary>
        /// <param name="name">the name to give the new category</param>
        /// <returns>the newly-created category</returns>
        public static LogCategory AddCategory( string name )
        {
            if (_catCounter < MaxCats)
            {
                _catCounter++;
                LogCategory newCat = new LogCategory(name);
                // I limit this to 58, and not 64 - because 6 bit-positions are already taken for the log-level.
                //if (_maskCounter < 58)
                //{
                ulong newMask = MaskFirstCatBit << _catCounter;
                newCat.Mask = newMask;
                //    _maskCounter++;
                //}
                //else
                //{
                //    newCat = LogCategory.Empty;
                //}
                Categories.Add( newCat );
                IsCleared = false;
                return newCat;
            }
            else
            {
                throw new InvalidOperationException( message: "You can only create up to " + MaxCats + " categories." );
            }
        }
        public const ulong MaskFirstCatBit = 0x0000000000000040UL;
        //public const ulong MaskTrace = 0x0000000000000001UL;
        //public const ulong MaskDebug = 0x0000000000000002UL;
        //public const ulong MaskInfo  = 0x0000000000000004UL;
        //public const ulong MaskWarn  = 0x0000000000000008UL;
        //public const ulong MaskError = 0x0000000000000010UL;
        //public const ulong MaskFatal = 0x0000000000000020UL;
        private static int _catCounter;
        private const int MaxCats = 10;

        public static LogCategory AddCategory<T>( T enumValue ) where T : struct
        {
            if (!typeof( T ).IsEnum)
            {
                throw new ArgumentException( String.Format( "Argumnent {0} is not an Enum", typeof( T ).FullName ) );
            }
            //string name =  typeof(T).Name;
            //Debug.WriteLine("name = " + name);
            var newCat = AddCategory(name: enumValue.ToString());
            return newCat;
        }

        /// <summary>
        /// Get the LogCategory that has the given name (not case-sensitive, or return null if it is not found.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>either the LogCategory that has the name or else null</returns>
        public static LogCategory GetCategory( string name )
        {
            if (name == null)
            {
                throw new ArgumentNullException( paramName: nameof( name ) );
            }
            LogCategory result = null;
            bool wasFound = false;
            lock (_categoryListLock)
            {
                foreach (var cat in Categories)
                {
                    if (name.Equals( cat.Name, StringComparison.OrdinalIgnoreCase ))
                    {
                        wasFound = true;
                        result = cat;
                        break;
                    }
                }
                if (!wasFound)
                {
                    result = AddCategory( name );
                }
            }
            return result;
        }
        /// <summary>
        /// Without using this lock, GetCategory was generating exceptions intermittently
        /// during the SubsystemTests_MultithreadedAccess.
        /// </summary>
        private static object _categoryListLock = new object();

        /// <summary>
        /// Return the LogCategory which matches the given enum-value, or create it if it doesn't exist.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static LogCategory GetCategory<T>( T enumValue ) where T : struct
        {
            if (!typeof( T ).IsEnum)
            {
                throw new ArgumentException( String.Format( "Argumnent {0} is not an Enum", typeof( T ).FullName ) );
            }
            LogCategory result = null;
            string catName = enumValue.ToString();
            bool wasFound = false;
            lock (_categoryListLock)
            {
                foreach (var eachCategory in Categories)
                {
                    if (eachCategory.Name.Equals( catName, StringComparison.OrdinalIgnoreCase ))
                    {
                        // This cat already exists. Cool.
                        result = eachCategory;
                        wasFound = true;
                        break;
                    }
                }

                if (!wasFound)
                {
                    result = AddCategory<T>( enumValue );
                }

            }
            return result;
        }

        #region ClearCategories
        /// <summary>
        /// Reset (clear) the list of created LogCategories.
        /// </summary>
        public static void ClearCategories()
        {
            lock (_categoryListLock)
            {
                if (_categories != null)
                {
                    _categories.Clear();
                    _categories = null;
                }
            }
        }
        #endregion

        #region DisableCategory
        /// <summary>
        /// Turn off all log-output from the named category.
        /// </summary>
        /// <param name="categoryName">the name of the category to turn off</param>
        /// <returns>the number of categories found that match the given name (which should be exactly one) - just in case you care</returns>
        /// <remarks>
        /// Categories defined after a call to this method would NOT be disabled even if their name would match.
        /// </remarks>
        public static int DisableCategory( string categoryName )
        {
            int n = 0;
            if (categoryName == null)
            {
                throw new ArgumentNullException( paramName: nameof( categoryName ) );
            }
            foreach (var cat in Categories)
            {
                if (cat.Name.Equals( categoryName, StringComparison.OrdinalIgnoreCase ))
                {
                    n++;
                    cat.Disable();
                    IsCleared = false;
                }
            }
            return n;
        }

        /// <summary>
        /// Turn off all log-output from the category denoted by the given enum-value.
        /// </summary>
        /// <typeparam name="T">the specific enum-type</typeparam>
        /// <param name="enumValue">the value whose string-representation is the name of the category in question</param>
        /// <returns>the number of categories found that match the given name (which should be exactly one) - just in case you care</returns>
        public static int DisableCategory<T>( T enumValue ) where T : struct
        {
            if (!typeof( T ).IsEnum)
            {
                throw new ArgumentException( String.Format( "Argumnent {0} is not an Enum", typeof( T ).FullName ) );
            }
            int n = 0;
            string catName = enumValue.ToString();
            // If no categories have been defined yet - do nothing.
            if (_categories != null)
            {
                foreach (var eachCategory in _categories)
                {
                    if (eachCategory.Name.Equals( catName, StringComparison.OrdinalIgnoreCase ))
                    {
                        // This cat does exist. Cool.
                        n++;
                        eachCategory.Disable();
                        IsCleared = false;
                    }
                }
            }
            return n;
        }

        /// <summary>
        /// Turn off all log-output from the named category.
        /// </summary>
        /// <param name="cat">the category to turn off</param>
        /// <returns>the number of categories found that match the given name (which should be exactly one) - just in case you care</returns>
        /// <remarks>
        /// Categories defined after a call to this method would NOT be disabled even if their name would match.
        /// </remarks>
        public static void DisableCategory( LogCategory cat )
        {
            cat.Disable();
            IsCleared = false;
        }
        #endregion

        public static void EnableCategory( string name, bool isToEnableThis )
        {
            LogCategory cat = GetCategory(name);
            if (cat != null)
            {
                cat.IsEnabled = isToEnableThis;
            }
        }

        public static void EnableCategory<T>( T enumValue ) where T : struct
        {
            if (!typeof( T ).IsEnum)
            {
                throw new ArgumentException( String.Format( "Argumnent {0} is not an Enum", typeof( T ).FullName ) );
            }
            string catName = enumValue.ToString();
            // If no categories have been defined yet - do nothing.
            if (_categories != null)
            {
                foreach (var eachCategory in _categories)
                {
                    if (eachCategory.Name.Equals( catName, StringComparison.OrdinalIgnoreCase ))
                    {
                        // This cat does exist. Cool.
                        eachCategory.Enable();
                        break;
                    }
                }
            }
        }

        public static void EnableAllCategories()
        {
            if (_categories != null)
            {
                foreach (var eachCategory in _categories)
                {
                    eachCategory.Enable();
                }
            }
        }

        public static void DisableAllCategories()
        {
            if (_categories != null)
            {
                bool clearedAtLeastOne = false;
                foreach (var eachCategory in _categories)
                {
                    eachCategory.Disable();
                    clearedAtLeastOne = true;
                }
                if (clearedAtLeastOne)
                {
                    IsCleared = false;
                }
            }
        }

        #region GetCategoryNames
        /// <summary>
        /// Return a list of the names of all the LogCategories that have been defined at this point.
        /// </summary>
        /// <returns>a list of strings denoting the names of all categories</returns>
        public static List<string> GetCategoryNames()
        {
            List<string> categoryNames = new List<string>();
            if (_categories != null)
            {
                foreach (var eachCategory in _categories)
                {
                    if (StringLib.HasSomething( eachCategory.Name ))
                    {
                        categoryNames.Add( eachCategory.Name );
                    }
                }
            }
            return categoryNames;
        }
        #endregion

        #region IsCategoryEnabled
        /// <summary>
        /// Check the given category and if found, return whether it is currently enabled - or null if not found.
        /// </summary>
        /// <typeparam name="T">the specific enum-type</typeparam>
        /// <param name="enumValue">the value whose string-representation is the name of the category in question</param>
        /// <returns>the enabled state of the given category, or null if it is not found</returns>
        public static bool? IsCategoryEnabled<T>( T enumValue )
        {
            if (!typeof( T ).IsEnum)
            {
                throw new ArgumentException( String.Format( "Argumnent {0} is not an Enum", typeof( T ).FullName ) );
            }
            bool? result = null;
            if (_categories != null)
            {
                string catName = enumValue.ToString();
                foreach (var eachCategory in _categories)
                {
                    if (eachCategory.Name.Equals( catName, StringComparison.OrdinalIgnoreCase ))
                    {
                        // This cat does exist. Cool.
                        result = eachCategory.IsEnabled;
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Check the given category and if found, return whether it is currently enabled - or null if not found.
        /// </summary>
        /// <param name="catName">the name of the category to check</param>
        /// <returns>the enabled state of the given category, or null if it is not found</returns>
        public static bool? IsCategoryEnabled( string catName )
        {
            if (catName == null)
            {
                throw new ArgumentNullException( paramName: nameof( catName ) );
            }
            bool? result = null;
            if (_categories != null)
            {
                foreach (var eachCategory in _categories)
                {
                    if (eachCategory.Name.Equals( catName, StringComparison.OrdinalIgnoreCase ))
                    {
                        // This cat does exist. Cool.
                        result = eachCategory.IsEnabled;
                        break;
                    }
                }
            }
            return result;
        }
        #endregion

        #endregion categories

        #region NameOfDefaultLogger
        /// <summary>
        /// Get the name that identifies a logger if none has been explicitly set for it, of which there can be only one
        /// and is by definition the "default" logger. This is needed, for example, when saving this logger in the Registry, under it's name.
        /// This value is "def".
        /// </summary>
        public static string NameOfDefaultLogger
        {
            get { return "def"; }
        }
        #endregion

        #region configuration

        #region Config
        /// <summary>
        /// Get or set the object that holds the configuration settings that are
        /// not contained within any of the other Config-properties.
        /// </summary>
        /// <remarks>
        /// Accessing this property (whether getting or setting) results in HasBeenConfiguredProgrammatically being set to true.
        /// </remarks>
        public static LogConfig Config
        {
            get
            {
                if (_config == null)
                {
                    _config = new LogConfig();
                    HasBeenConfiguredProgrammatically = true;
                }
                return _config;
            }
            set
            {
                _config = value;
                HasBeenConfiguredProgrammatically = true;
            }
        }
        #endregion

        #region HasBeenConfiguredProgrammatically
        /// <summary>
        /// Get or set whether, within this process,
        /// the running application has already invoked whatever method is has provided
        /// for configuring it's logging - or at least the existence of said method has been already checked for.
        /// </summary>
        public static bool HasBeenConfiguredProgrammatically { get; set; }

        #endregion

        #region TryConfiguringLogging
        /// <summary>
        /// If the running program is a WPF Application and implements ILoggingConfigurable,
        /// then call it's ConfigureLogging method.
        /// This is called at the first call to <c>GetLogger</c>, and it is called exactly once.
        /// </summary>
        /// <remarks>
        /// In this way, logging-code within libraries - some of which may be in 
        /// XAML view-models that are executing within the designer, can still operate.
        /// But when that same code is running within a running application, it then *should*
        /// give that application the opportunity to configure it's logging first.
        /// </remarks>
        public static void TryConfiguringLogging()
        {
            if (!HasBeenConfiguredProgrammatically)
            {
                // Try to call ConfigureLogging.

                //CBL Test this on Windows Store, ASP.NET, Mono. Can I still access this Application class?
                var somethingThatCanConfigureLogging = Application.Current as ILoggingConfigurable;
                if (somethingThatCanConfigureLogging != null)
                {
                    somethingThatCanConfigureLogging.ConfigureLogging();
                }

                HasBeenConfiguredProgrammatically = true;
            }
        }
        #endregion

        #endregion configuration

        #region logging methods

        #region LogTrace
        /// <summary>
        /// Log a message at level <c>Trace</c>, calling the <c>ToString</c> method of the given object to produce the message,
        /// using the default logger.
        /// This is just a shortcut for <c>LogManager.GetLogger().LogTrace</c> .
        /// </summary>
        /// <param name="objectMessage">the object that represents the message to log</param>
        [Conditional( "TRACE" )]
        public static void LogTrace( string textToLog )
        {
            var logger = GetLogger();
            logger.LogTrace( textToLog );
        }

        /// <summary>
        /// Log a message at level <c>Trace</c>, the message being the given object-array and expressed using the given string format,
        /// using the default logger.
        /// This is just a shortcut for <c>LogManager.GetLogger().LogTrace</c> .
        /// </summary>
        /// <param name="format">the format to use to express the message (as in <c>String.Format</c>)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [Conditional( "TRACE" )]
        [StringFormatMethod( "format" )]
        public static void LogTrace( string format, params object[] args )
        {
            var logger = GetLogger();
            logger.LogTrace( format, args );
        }
        #endregion

        #region LogDebug
        /// <summary>
        /// Log a message at level <c>Debug</c>, calling the <c>ToString</c> method of the given object to produce the message,
        /// using the default logger.
        /// This is just a shortcut for <c>LogManager.GetLogger().LogDebug</c> .
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        public static void LogDebug( string textToLog )
        {
            var logger = GetLogger();
            logger.LogDebug( textToLog );
        }

        /// <summary>
        /// Log a message at level <c>Debug</c>, the message being the given object-array and expressed using the given string format,
        /// using the default logger.
        /// This is just a shortcut for <c>LogManager.GetLogger().LogDebug</c> .
        /// </summary>
        /// <param name="format">the format to use to express the message (as in <c>String.Format</c>)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [StringFormatMethod( "format" )]
        public static void LogDebug( string format, params object[] args )
        {
            var logger = GetLogger();
            logger.LogDebug( format, args );
        }
        #endregion

        #region LogInfo
        /// <summary>
        /// Log a message at level <c>Infomation</c>, calling the <c>ToString</c> method of the given object to produce the message,
        /// using the default logger.
        /// This is just a shortcut for <c>LogManager.GetLogger().LogInfo</c> .
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        /// <returns>a reference to the logger object so that you can chain method-calls</returns>
        public static Logger LogInfo( string textToLog )
        {
            var logger = GetLogger();
            logger.LogInfo( textToLog );
            return logger;
        }

        /// <summary>
        /// Log a message at level <c>Infomation</c>, the message being the given object-array and expressed using the given string format,
        /// using the default logger.
        /// This is just a shortcut for <c>LogManager.GetLogger().LogInfo</c> .
        /// </summary>
        /// <param name="format">the format to use to express the message (as in <c>String.Format</c>)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        /// <returns>a reference to the logger object so that you can chain method-calls</returns>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [StringFormatMethod( "format" )]
        public static Logger LogInfo( string format, params object[] args )
        {
            var logger = GetLogger();
            logger.LogInfo( format, args );
            return logger;
        }
        #endregion

        #region LogWarning, or Warn
        /// <summary>
        /// Log a message at level <c>Warning</c>, calling the <c>ToString</c> method of the given object to produce the message,
        /// using the default logger.
        /// This is just a shortcut for <c>LogManager.GetLogger().LogWarning</c>.
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        public static void LogWarning( string textToLog )
        {
            var logger = GetLogger();
            logger.LogWarning( textToLog );
        }

        /// <summary>
        /// Log a message at level <c>Warning</c>, the message being the given object-array and expressed using the given string format,
        /// using the default logger.
        /// This is just a shortcut for <c>LogManager.GetLogger().LogWarning</c> .
        /// </summary>
        /// <param name="format">the format to use to express the message (as in <c>String.Format</c>)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        /// <returns>a reference to the logger object so that you can chain method-calls</returns>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [StringFormatMethod( "format" )]
        public static void LogWarning( string format, params object[] args )
        {
            var logger = GetLogger();
            logger.LogWarning( format, args );
        }

        /// <summary>
        /// Log a message at level <c>Warning</c>, the message being the given object-array and expressed using the given string format,
        /// using the default logger.
        /// This is just a shortcut for <c>LogManager.GetLogger().LogWarning</c>.
        /// And yes -- this is just a synonym for <c>LogWarning</c>.
        /// </summary>
        /// <param name="format">the format to use to express the message (as in <c>String.Format</c>)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        /// <returns>a reference to the logger object so that you can chain method-calls</returns>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [StringFormatMethod( "format" )]
        public static void Warn( string format, params object[] args )
        {
            var logger = GetLogger();
            logger.LogWarning( format, args );
        }
        #endregion

        #region LogError
        /// <summary>
        /// Log a message at level <c>Error</c>, calling the <c>ToString</c> method of the given object to produce the message,
        /// using the default logger.
        /// This is just a shortcut for <c>LogManager.GetLogger().LogError</c> .
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        public static void LogError( string textToLog )
        {
            var logger = GetLogger();
            logger.LogError( textToLog );
        }

        /// <summary>
        /// Log a message at level <c>Error</c>, the message being the given object-array and expressed using the given string format,
        /// using the default logger.
        /// This is just a shortcut for <c>LogManager.GetLogger().LogError</c>.
        /// </summary>
        /// <param name="format">the format to use to express the message (as in <c>String.Format</c>)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        /// <remarks>
        /// For the highest-possible runtime performance you should consider avoiding this one due to the use of the params parameter,
        /// to minimize memory-allocations.
        /// </remarks>
        [StringFormatMethod( "format" )]
        public static void LogError( string format, params object[] args )
        {
            var logger = GetLogger();
            logger.LogError( format, args );
        }
        #endregion

        #region LogCritical
        /// <summary>
        /// Log a message at level <c>Critical</c>, calling the <c>ToString</c> method of the given object to produce the message,
        /// using the default logger.
        /// This is just a shortcut for <c>LogManager.GetLogger().LogCritical</c> .
        /// </summary>
        /// <param name="textToLog">the string that represents the message to log</param>
        public static void LogCritical( string textToLog )
        {
            var logger = GetLogger();
            logger.LogCritical( textToLog );
        }

        /// <summary>
        /// Log a message at level <c>Critical</c>, the message being the given object-array and expressed using the given string format,
        /// using the default logger.
        /// This is just a shortcut for <c>LogManager.GetLogger().LogCritical</c>.
        /// </summary>
        /// <param name="format">the format to use to express the message (as in <c>String.Format</c>)</param>
        /// <param name="args">an array of objects that represents the values to go into the message we want to log</param>
        [StringFormatMethod( "format" )]
        public static void LogCritical( string format, params object[] args )
        {
            var logger = GetLogger();
            logger.LogCritical( format, args );
        }
        #endregion

        #region LogException
        /// <summary>
        /// Log a message at level "Error" which consists of information on the given Exception, on the default logger.
        /// This is a shortcut for LogManager.GetLogger().LogError.
        /// </summary>
        /// <param name="exception">the Exception to describe</param>
        public static void LogException( Exception exception )
        {
            var logger = GetLogger();
            logger.LogError( exception );
        }

        /// <summary>
        /// Log a message at level "Error" which consists of information concerning the given Exception, on the default logger.
        /// This is a shortcut for LogManager.GetLogger().LogError.
        /// </summary>
        /// <param name="exception">the Exception to describe</param>
        /// <param name="format">to convey additional information - this is the format-string (as in String.Fomrrat)</param>
        /// <param name="args">to convey additional information - this is an array of objects that represent values to insert into the format-string</param>
        [StringFormatMethod( "format" )]
        public static void LogException( Exception exception,
                                         string format, params object[] args )
        {
            var logger = GetLogger();
            logger.LogError( exception, String.Format( format, args ) );
        }
        #endregion

        #endregion logging methods

        #region file output

        #region FileOutputFolder_DefaultValue
        /// <summary>
        /// Get the default value that would be used for the property <see cref="LogConfig.FileOutputFolder"/>
        /// (that is, its' value if it is not been explicitly set).
        /// For UWP this is LocalFolder,
        /// For .NET for Windows desktop applications this is Documents\Logs (see remarks for details)
        /// For ASP.NET applications bin\Logs .
        /// </summary>
        /// <remarks>
        /// For UWP (Universal Windows Platform) this returns LocalFolder\Logs.  (CBL true?)
        /// 
        /// For the .NET Framework for Windows desktop applications this returns on
        ///   Windows 10, 8, Vista:      C:\Users\{username}\Documents\Logs,
        ///   Windows 7:                 C:\Users\{username}\My Documents\Logs,
        ///   Windows XP:                C:\Documents and Settings\{username}\My Documents\Logs .
        ///   Windows Server 2008, 2012: C:\Users\{username}\My Documents\Logs,
        ///   Windows Server 2003:       C:\Documents and Settings\{username}\My Documents\Logs .
        /// 
        /// For ASP.NET applications where My Documents\Log is not accessible,
        /// it is a Logs folder under the subject-program's executable folder (the "bin" folder)  CBL Is that correct?
        /// 
        /// The actual default value may be arrived at within your C# code via this .NET call:
        /// <c>Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Logs");</c> .
        /// </remarks>
        public static string FileOutputFolder_DefaultValue
        {
            get
            {
                //CBL Is this the only place wherein this infor originates?
                return NutFileLib.GetDefaultFileOutputDirectory();
            }
        }
        #endregion

        #region FileRecordSeparator
        /// <summary>
        /// Get the character that is to be used as the record terminator for log records written to file.
        /// It is the paragraph symbol.
        /// </summary>
        public static char FileRecordSeparator
        {
            get { return _fileRecordSeparator; }
        }
        #endregion

        #region MoveDirectoryContent
        /// <summary>
        /// Move the content of the given source-directory to under the given destination-parent-directory. Not including sub-directories.
        /// </summary>
        /// <param name="sourceDirectory">This is the source-directory to move the files out of.</param>
        /// <param name="destinationParentDirectory">This is the directory to move the files into. This must already exist.</param>
        /// <param name="fileMatchExpression">a Windows files-spec pattern expression that the files must match. Null indicates all files. Null = everything</param>
        /// <param name="isToRolloverExistingDestinFile">if true - then rollover any destination filenames that already exists in the destination, so that it does not get overwritten</param>
        /// <exception cref="DirectoryNotFoundException">Both the source-directory and the destination-parent-directory must already exist.</exception>
        /// <remarks>
        /// The destination-parent-directory must already exist, otherwise an exception is thrown.
        /// Both the source and destination-parent directories must be there before this method is called.
        ///
        /// This checks first to see whether the destination file, if it already exists, has the exact same content as the source file
        /// and only makes the move if it does not. Otherwise it simply deletes the source file.
        /// </remarks>
        public static void MoveDirectoryContent( string sourceDirectory,
                                                 string destinationParentDirectory,
                                                 string fileMatchExpression,
                                                 bool isToRolloverExistingDestinFile )
        {
            // I put this here, just to use the file-lock so that it does not interfere with the direct-to-disk logging:
            lock (_logFileWriteLockObject)
            {
                FilesystemLib.MoveDirectoryContent( sourceDirectory: sourceDirectory,
                                                    destinationParentDirectory: destinationParentDirectory,
                                                    fileMatchExpression: fileMatchExpression,
                                                    isToRolloverExistingDestinFile: isToRolloverExistingDestinFile );
            }
        }
        #endregion

        #region MoveLogsToLogReceiverIfPresent
        /// <summary>
        /// Shove the existing log-files over to an attached removable-drive that qualifies as a 'log-reciever', if present.
        /// </summary>
        /// <param name="subdirectoryForPlacingLogOutput"></param>
        /// <returns>true if this does find that a 'log-reciever' is present</returns>
        public static bool MoveLogsToLogReceiverIfPresent( string subdirectoryForPlacingLogOutput )
        {
            bool isLogRcvrPresent = false;
            const string keyFile = "gLogOutput.gtk";
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Removable)
                {
                    string rootDir = drive.RootDirectory.FullName;
                    string logOutputKeyFile = Path.Combine(rootDir, keyFile);
                    if (File.Exists( logOutputKeyFile ))
                    {
                        // Found the log-receiver drive.
                        isLogRcvrPresent = true;

                        //LogManager.RemovableDrive = rootDir;
                        string destinationDir = rootDir;
                        if (subdirectoryForPlacingLogOutput != null)
                        {
                            destinationDir = Path.Combine( rootDir, subdirectoryForPlacingLogOutput );
                        }
                        else
                        {
                            destinationDir = rootDir;
                        }

                        string sourceDir = Config.FileOutputFolder;

                        NutUtil.CloseTheOutputFile();

                        if (FilesystemLib.HasContent( sourceDir ))
                        {
                            if (!Directory.Exists( destinationDir ))
                            {
                                Directory.CreateDirectory( destinationDir );
                            }

                            MoveDirectoryContent( sourceDirectory: sourceDir,
                                                  destinationParentDirectory: destinationDir,
                                                  fileMatchExpression: null,
                                                  isToRolloverExistingDestinFile: true );
                        }

                        // Also get anything that's in here..
                        string userLogDir = @"C:\Users\LuVivaSystem\Documents\Logs";

                        // Skip if this is the same directory as we just copied.
                        if (!userLogDir.Equals( sourceDir, StringComparison.OrdinalIgnoreCase ))
                        {
                            if (FilesystemLib.HasContent( userLogDir ))
                            {
                                string destinationUserLogsDir = Path.Combine(destinationDir, "UsersLogs");
                                if (!Directory.Exists( destinationUserLogsDir ))
                                {
                                    Directory.CreateDirectory( destinationUserLogsDir );
                                }

                                MoveDirectoryContent( sourceDirectory: userLogDir,
                                                      destinationParentDirectory: destinationUserLogsDir,
                                                      fileMatchExpression: null,
                                                      isToRolloverExistingDestinFile: true );
                            }
                        }
                        break;
                    }
                }
            }
            return isLogRcvrPresent;
        }
        #endregion

        #region SetLogOutputToLogRcvrFlashDriveIfPresent
        /// <summary>
        /// Look for any removable-drive that contains a file named gLogOutput.gtk and, if found,
        /// set that as the secondary log-output destination. Return false if no such drive is found.
        /// </summary>
        /// <param name="subdirectoryForPlacingLogOutput">This denotes which sub-directory on that flash-drive to place the log-output into. Leaving this null means to put it into the root directory.</param>
        /// <returns>true if the log-receiver flash-drive was detected and used</returns>
        public static bool SetLogOutputToLogRcvrFlashDriveIfPresent( string subdirectoryForPlacingLogOutput )
        {
            bool isLogRcvrPresent = false;
            const string keyFile = "gLogOutput.gtk";

            string rootDir = FileInterface.GetRemovableDriveContainingFile(identifyingFilename: keyFile);
            if (rootDir != null)
            {
                // Found the log-receiver drive.
                isLogRcvrPresent = true;
                string logDirectory = rootDir;
                if (subdirectoryForPlacingLogOutput != null)
                {
                    logDirectory = Path.Combine( rootDir, subdirectoryForPlacingLogOutput );
                }
                Config.RemovableDrivePreferredFileOutputFolder = logDirectory;
            }
            return isLogRcvrPresent;
        }
        #endregion

        public static IFilesystemInterface FileInterface
        {
            get
            {
                if (_filesystemInterface == null)
                {
                    _filesystemInterface = new FilesystemInterface();
                }

                return _filesystemInterface;
            }
            set { _filesystemInterface = value; }
        }
        private static IFilesystemInterface _filesystemInterface;

        #region TruncateLogFile
        /// <summary>
        /// Cause the output file to be emptied, except for a pithy notice with a timestamp.
        /// </summary>
        /// <returns>true if the log-file was found, false if it failed to find it</returns>
        /// <remarks>
        /// Normally you would just omit the pathname argument and let it truncate
        /// the file that is specified by the value of the LogManager.FileOutputPath property.
        /// 
        /// After the file is cleared, this single line of text is written to it
        /// (if, for example, this is October 28th, 2012 at 9:48 AM and 59 seconds):
        /// "====[ Content Truncated on 2012-10-28 9:48:59 ]====" + new-line
        /// </remarks>
        //public static bool TruncateLogFile()
        //{
        //    return TruncateLogFile( null );
        //}

        /// <summary>
        /// Cause the indicated file to be emptied, except for a pithy notice with a timestamp.
        /// </summary>
        /// <param name="pathname">the file-system path of the file to truncate. If you provide a null value for this, it selects the FileOutputPath value.</param>
        /// <returns>true if the log-file was found, false if it failed to find it</returns>
        /// <remarks>
        /// Normally you would just omit the pathname argument and let it truncate
        /// the file that is specified by the value of the LogManager.FileOutputPath property.
        /// 
        /// After the file is cleared, this single line of text is written to it
        /// (if, for example, this is October 28th, 2012 at 9:48 AM and 59 seconds):
        /// "====[ Content Truncated on 2012-10-28 9:48:59 ]====" + new-line
        /// </remarks>
        //public static bool TruncateLogFile( string pathname )
        //{
        //    bool wasFound = false;
        //    if (pathname == null)
        //    {
        //        pathname = Config.FileOutputPath;
        //    }

        //    try
        //    {
        //        lock (_logFileWriteLockObject)
        //        {
        //            if (FilesystemLib.FileExists( pathname ))
        //            {
        //                wasFound = true;
        //                string truncationMessage = TruncationPrefix + LogRecordFormatter.GetTimeStamp( DateTime.Now, Config, false ) + TruncationSuffix + Environment.NewLine;
        //                FilesystemLib.WriteTextToFile( pathname, truncationMessage, FileMode.Truncate, false );
        //            }
        //        }
        //    }
        //    catch (Exception x)
        //    {
        //        throw new Exception( "attempting to access log file " + pathname, x );
        //    }
        //    return wasFound;
        //}
        #endregion

        #endregion file output

        #region Visual Studio IDE facilities

        #region IsInDesignMode property
        /// <summary>
        /// Get whether this code is being executed in the context of Visual Studio "Cider" (the visual designer) or Blend,
        /// as opposed to running normally as an application.
        /// Note: This applies only to WPF applications, AND only if Config.SubjectProgram has been assigned. Otherwise returns false.
        /// </summary>
        /// <remarks>
        /// CBL  Check whether this is still true.
        /// Once this is initially set, it is not expected that there will be a subsequent need to set it.
        /// </remarks>
        public static bool IsInDesignMode
        {
            get
            {
                if (_isInDesignMode == null)
                {
                    _isInDesignMode = IdeLib.GetWhetherIsInDesignMode();
                }
                return _isInDesignMode.Value;
            }
        }
        private static bool? _isInDesignMode;
        #endregion

        #region IsUsingTraceForConsoleOutput
        /// <summary>
        /// Get or set whether, when running under Microsoft's Visual Studio IDE, to use Trace output
        /// as opposed to Console output,
        /// whenever writing to the Output window. The default is false (output using Console).
        /// </summary>
        /// <remarks>
        /// This is placed here, within <c>LogManager</c> as opposed to the <see cref="LogConfig"/> class,
        /// so that code may access it even when no <see cref="LogConfig"/> instance is available.
        /// </remarks>
        public static bool IsUsingTraceForConsoleOutput
        {
            get { return _isUsingTraceForConsoleOutput; }
            set { _isUsingTraceForConsoleOutput = value; }
        }

        /// <summary>
        /// Set LogNut to use Debug output (with Visual Studio) as opposed to Console output,
        /// whenever writing to the console-output window. The default is false (output using Console).
        /// </summary>
        /// <remarks>
        /// This is placed here, within <c>LogManager</c> as opposed to the <see cref="LogConfig"/> class,
        /// so that code may access it even when no <see cref="LogConfig"/> instance is available.
        /// </remarks>
        public static void SetToUseTraceForConsoleOutput()
        {
            _isUsingTraceForConsoleOutput = true;
        }
        #endregion

        #endregion Visual Studio IDE facilities

        #region testing facilities

        #region BeginTest
        /// <summary>
        /// Turn on test-mode, and get a new instance of TestFacility, which is an <c>IDisposable</c> so that
        /// if you use this with a C# using clause, TestFacility.Dispose will set LogManager.IsTesting to false upon exiting the block.
        /// </summary>
        /// <param name="isToDoActualOutput">set this to false to cause all log output to be remembered, but not actually happen</param>
        /// <returns>a reference to the new <c>TestFacility</c> object</returns>
        public static TestFacility BeginTest( bool isToDoActualOutput )
        {
            _testFacility = new TestFacility();
            _testFacility.IsSimulatingOutput = !isToDoActualOutput;
            return TestFacility;
        }
        #endregion

        #region TestFacility
        /// <summary>
        /// Get the TestFacility that is associated with LogManager.
        /// Alert: this can be null.
        /// </summary>
        public static TestFacility TestFacility
        {
            get { return _testFacility; }
            set { _testFacility = value; }
        }
        #endregion

        #region ClearLoggers
        /// <summary>
        /// Remove the collection of individual loggers which have been created thus far, leaving that collection empty.
        /// This is so that we may start with a fresh list of loggers when running a unit-test.
        /// </summary>
        public static void ClearLoggers()
        {
            _theLoggers = null;
        }
        #endregion

        #region IsTesting
        /// <summary>
        /// Get a flag that indicates whether LogManager is in unit-test mode.
        /// </summary>
        public static bool IsTesting
        {
            get { return _testFacility != null && _testFacility.IsTesting; }
        }
        #endregion

        #endregion testing facilities

        /// <summary>
        /// Shuts off ALL logging.
        /// This is the master OFF-switch, which resets any overrides.
        /// </summary>
        /// <remarks>
        /// Since the property Config.IsLoggingEnabled may be overridden by
        /// individual loggers and categories, this clears all of those overrides
        /// by setting their override-properties to null.
        /// </remarks>
        public static void ResetAllLoggingEnablements()
        {
            Config.IsLoggingEnabledByDefault = true;
            Config.IsLoggingEnabled = true;
            ClearLoggerOverrides();
            if (_categories != null)
            {
                foreach (var cat in Categories)
                {
                    cat.ClearOverrides();
                }
            }
        }

        public static void ClearLoggerOverrides()
        {
            if (_theLoggers != null)
            {
                foreach (var logger in _theLoggers)
                {
                    logger.ClearOverrides();
                }
            }
        }

        #region LogRecordFormatter
        /// <summary>
        /// Get or set the ILogRecordFormatter implementation that renders a log-record into text
        /// for output to a file or whatever.
        /// By default it is a DefaultLogRecordFormatter,
        /// but it may be set to a different ILogRecordFormatter implementation.
        /// </summary>
        /// <remarks>
        /// You can change the way in which LogNut renders log-records by replacing the object that this property holds.
        /// </remarks>
        public static ILogRecordFormatter LogRecordFormatter
        {
            get
            {
                if (_logRecordFormatter == null)
                {
                    _logRecordFormatter = DefaultLogRecordFormatter.The;
                }
                return _logRecordFormatter;
            }
            set { _logRecordFormatter = value; }
        }
        #endregion

        #region SetPointOfReferenceForElapsedTime
        /// <summary>
        /// Set the reference-time, for use when showing the elapsed-time within a log-record prefix.
        /// </summary>
        /// <param name="toWhen">a nullable <c>DateTime</c> to set it to</param>
        /// <returns>a <c>DateTime</c> that denotes the new reference</returns>
        /// <remarks>
        /// Setting this, means that subsequent records that are logged, when have an 'elapsed-time' value
        /// that is the time that has passed since this <see cref="ReferenceTime"/>.
        /// 
        /// If you provide an argument value of <c>null</c> then the <see cref="ReferenceTime"/>
        /// is set to now.
        /// </remarks>
        public static DateTime SetPointOfReferenceForElapsedTime( DateTime? toWhen )
        {
            if (toWhen.HasValue)
            {
                ReferenceTime = toWhen.Value;
            }
            else
            {
                ReferenceTime = DateTime.Now;
            }
            return ReferenceTime;
        }

        /// <summary>
        /// Set the reference-time to <c>Now</c>, for use when showing the elapsed-time within a log-record prefix.
        /// </summary>
        /// <returns>a <c>DateTime</c> that denotes the new reference</returns>
        /// <remarks>
        /// Setting this, means that subsequent records that are logged, when have an 'elapsed-time' value
        /// that is the time that has passed since this <see cref="ReferenceTime"/>.
        /// </remarks>
        public static DateTime SetPointOfReferenceForElapsedTime()
        {
            ReferenceTime = DateTime.Now;
            return ReferenceTime;
        }
        #endregion

        #region SetClear
        /// <summary>
        /// Assign a method to be called when the current subject-program terminates.
        /// This is not reliable for every kind of executable - see notes.
        /// </summary>
        /// <remarks>
        /// Crucial Note: In some cases such as unit-tests, the event-handler that this sets is NOT called when your process exits.
        /// For programs other than WPF Desktop Apps, you should call <c>SetClear</c> explicitly.
        /// </remarks>
        public static void SetClear()
        {
            // Console.WriteLine( "begin SetClear" );
            if (System.Windows.Application.Current != null)
            {
                var app = System.Windows.Application.Current;
                // Ensure we are on a UI thread..
                //CBL But actually, we should put *this* operation onto the UI thread if necessary, right here.
                Dispatcher dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
                if (dispatcher != null)
                {
                    if (dispatcher.CheckAccess())
                    {

                        app.Exit += OnAppExit;
                        IsClearSet = true;

                    }
                }
            }
            else // there is no current System.Windows.Application.
            {
                // This is for Windows Forms programs - although I do not know how to determine whether this *is* running with a Windows Forms program.
                System.Windows.Forms.Application.ApplicationExit += OnApplicationExit;
                // And this is for everything else (CBL I've not tested this with ASP.NET as yet).
                AppDomain.CurrentDomain.ProcessExit += OnCurrentDomainProcessExit;
                IsClearSet = true;
            }
            // Debug.WriteLine( "end SetClear, IsClearSet = " + IsClearSet );
        }

        /// <summary>
        /// 
        /// This does get called for Windows.Forms applications.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void OnApplicationExit( object sender, EventArgs e )
        {
            //Debug.WriteLine( "LogManager.OnApplicationExit" );
            if (!IsCleared)
            {
                Clear();
            }
        }

        private static void OnCurrentDomainProcessExit( object sender, EventArgs e )
        {
            //Debug.WriteLine( "LogManager.OnCurrentDomainProcessExit" );
            if (!IsCleared)
            {
                Clear();
            }
        }

        private static void OnAppExit( object sender, ExitEventArgs e )
        {
            //  Debug.WriteLine( "LogManager.OnAppExit" );
            if (!IsCleared)
            {
                Clear();
            }
        }
        #endregion

        #region IsCleared
        /// <summary>
        /// Get or set whether <see cref="LogManager.Clear"/> has been called yet, during this program-run.
        /// </summary>
        public static bool IsCleared { get; set; }
        #endregion

        #region IsClearSet
        /// <summary>
        /// Get whether LogManager was able to set an event-handler to call LogManager.Clear when your program exits.
        /// If this does *not* get set once your program launches, you should arrange to call Clear explicitly.
        /// </summary>
        public static bool IsClearSet { get; private set; }
        #endregion

        //CBL Need some documentation here!
        public static void CloseOutputFiles()
        {
            NutUtil.CloseTheOutputFile();
        }

        #region StopElapsedTimer
        /// <summary>
        /// Stop and set to null the Stopwatch object that is used for Logger.LogElapsedTime2
        /// </summary>
        public static void StopElapsedTimer()
        {
            if (_repeatingTimeReference != null)
            {
                _repeatingTimeReference.Stop();
                _repeatingTimeReference = null;
            }
        }
        #endregion

        #region Clear
        /// <summary>
        /// Clear out all references to any existing Logger objects, and set the property values of LogManager to their default values.
        /// This is the same as SetToDefaults, except that it also clears out the loggers, and also any OutputPipes.
        /// </summary>
        public static void Clear()
        {
            //Debug.WriteLine( "begin LogManager.Clear" );

            StopElapsedTimer();

            //CBL  I need to add clearing of the log-servicing process objects and stopping it's task.
            if (!IsCleared)
            {
                // Turn off all logging, so that additional attempts at logging don't happen while this is executing.
                Config.IsLoggingEnabled = false;

                if (_queueOfLogRecordsToWrite != null)
                {
                    // Inform the collection of log-records to send, that there will be no more logs during this session.
                    //CBL Don't I need to undo this, afterward? I may want to send more logs!
#if !PRE_4
                    QueueOfLogRecordsToWrite.CompleteAdding();
#endif
                    // Wait up to 60 seconds for the ManualResetEvent to signal that the log-writing task is finished.
                    // Since we have called CompleteAdding on the Queue, we know that no more log-output is being added.
                    //Stopwatch stopwatch = Stopwatch.StartNew();
                    //Console.WriteLine( "Waiting on QueueOfLogsToWriteEmptiedEvent..." );
                    bool r = QueueOfLogsToWriteEmptiedEvent.WaitOne(TimeSpan.FromSeconds(60));
                    //Console.WriteLine( "After WaitOne that returned {0} : {1}", r, stopwatch.Elapsed.TotalSeconds );
                }

#if !PRE_4 && OUTPUT_SVC
                if (_queueOfLogMessagesToSendToSvc != null)
                {
                    _queueOfLogMessagesToSendToSvc.CompleteAdding();

                    // Wait up to 30 seconds for the ManualResetEvent to signal that the send-to-service task is finished.
                    // Since we have called CompleteAdding on the Queue, we know that no more log-output is being added.
                    Stopwatch stopwatch = Stopwatch.StartNew();
                    Debug.WriteLine( "Waiting on QueueOfMsgsToSvcEmptiedEvent..." );
                    bool r = QueueOfMsgsToSvcEmptiedEvent.WaitOne(TimeSpan.FromSeconds(30));
                    Debug.WriteLine( "After WaitOne that returned {0} : {1}", r, stopwatch.Elapsed.TotalSeconds );
                }
#endif

                // After all possible log-output has been attended to, but before we begin clearing the configuration
                // properties - seems like the right moment to finalize the output-pipe. 
                foreach (var pipe in OutputPipes)
                {
                    pipe.FinalizeWriteToFile( null );
                }

#if !NETFX_CORE
                //NutUtil.GetterOfExecutionFolder = null;
#endif
                Config.Clear();
                lock (_loggerListLockObject)
                {
                    NutUtil.CloseTheOutputFile();
#if !PRE_4
                    //CBL
                    // Clear out the transmission task if that exists..
                    if (_logTransmissionTask != null)
                    {
                        // If the log-transmission task is still running,
                        if (_logTransmissionTask.Status == TaskStatus.Running)
                        {
                            if (_cancellationTokenSource != null)
                            {
                                // then cancel it.
                                OurCancellationTokenSource.Cancel();
                                try
                                {
                                    // Wait for it to complete, up to a maximum of sixty seconds.
                                    // Stopwatch stopwatch = Stopwatch.StartNew();
                                    //Console.WriteLine( "Waiting for _logTransmissionTask to respond to cancel.." );
                                    bool r = _logTransmissionTask.Wait(60000);
                                    //Console.WriteLine( "After _logTransmissionTask.Wait returned {0} : {1}", r, stopwatch.Elapsed.TotalSeconds );
                                }
                                catch (AggregateException x)
                                {
                                    foreach (var innerException in x.InnerExceptions)
                                    {
                                        //CBL Unfinished
                                        LogManager.RaiseLoggingFaultOccurred( new object(), null, "", innerException, null );
                                        NutUtil.WriteToConsoleAndInternalLog( "in LogManager.Clear, innerException ", innerException );
                                    }
                                }
                            }
                        }
                        //Console.WriteLine( "logTask Status: " + _logTransmissionTask.Status );

                        // We can't Dispose the Task unless it has RanTocompletion, Faulted, or been Canceled.
                        if (_logTransmissionTask.IsCompleted || _logTransmissionTask.IsCanceled)
                        {
#if !NETFX_CORE
                            // Test this!  CBL  
                            _logTransmissionTask.Dispose();
#endif
                        }
                        _logTransmissionTask = null;
                        if (_cancellationTokenSource != null)
                        {
                            _cancellationTokenSource.Dispose();
                            _cancellationTokenSource = null;
                        }
                    }
#endif
                    _isFirstFileOutput = true;
                }

                if (_queueOfLogRecordsToWrite != null)
                {
                    //CBL Should I do something to dispose of the queue here?
#if !PRE_4
                    try
                    {
                        _queueOfLogRecordsToWrite.Dispose();
                    }
                    catch (Exception)
                    {
                        // This advises to dispose of the BlockingCollection within a try-catch block,
                        // which is one of the rare occasions that I consent to eat exceptions.
                        // https://msdn.microsoft.com/en-us/library/dd267312(v=vs.110).aspx
                    }
#endif
                    _queueOfLogRecordsToWrite = null;
                    //NutUtil.CloseTheOutputFile();
                }
#if !PRE_4 && OUTPUT_SVC
                if (_queueOfLogMessagesToSendToSvc != null)
                {
                    try
                    {
                        _queueOfLogMessagesToSendToSvc.Dispose();
                    }
                    catch (Exception)
                    {
                        // This advises to dispose of the BlockingCollection within a try-catch block,
                        // which is one of the rare occasions that I consent to eat exceptions.
                        // https://msdn.microsoft.com/en-us/library/dd267312(v=vs.110).aspx
                    }
                    _queueOfLogMessagesToSendToSvc = null;
                }
#endif
                NutFileLib.Clear();
                _isUsingTraceForConsoleOutput = false;
                _logRecordFormatter = null;
                ReferenceTime = default( DateTime );
                if (_testFacility != null)
                {
                    _testFacility.Dispose();
                    _testFacility = null;
                }
                if (_theLoggers != null)
                {
                    _theLoggers.Clear();
                    _theLoggers = null;
                }
                _config = null;
                // Now do all of the plugins...
                if (_outputPipes != null)
                {
                    foreach (IOutputPipe outputPipe in OutputPipes)
                    {
                        outputPipe.Clear();
                    }
                    OutputPipes.Clear();
                }
            }
            // Reset the list of categories..
            ClearCategories();
            Config.IsLoggingEnabled = true;
            IsCleared = true;
            //Debug.WriteLine( "end LogManager.Clear" );
        }
        #endregion Clear

        #region internal implementation

        #region private properties

        #region JsonSerializerSettings
#if INCLUDE_JSON
        /// <summary>
        /// Get the JsonSerializerSettings to use when serializing a log-record into JSON format, whenever that format is used.
        /// </summary>
        private static JsonSerializerSettings JsonSerializerSettings
        {
            get
            {
                if (_jsonSerializerSettings == null)
                {
                    _jsonSerializerSettings = new JsonSerializerSettings();
                    _jsonSerializerSettings.DefaultValueHandling = DefaultValueHandling.Ignore;
                    _jsonSerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    _jsonSerializerSettings.ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor;
                }
                return _jsonSerializerSettings;
            }
        }
#endif
        #endregion

        #region QueueOfLogRecordsToWrite
        /// <summary>
        /// Get the queue of log records that are waiting to be written to disk by the LogManager thread that does this. This is used when sending asynchronously
        /// but not using a separate log-servicing process such as the Windows Service.
        /// </summary>
#if PRE_4
        private static LockFreeQueue<LogSendRequest> QueueOfLogRecordsToWrite
        {
            get { return _queueOfLogRecordsToWrite; }
        }
#else
        private static BlockingCollection<LogSendRequest> QueueOfLogRecordsToWrite
        {
            get
            {
                if (_queueOfLogRecordsToWrite == null)
                {
                    _queueOfLogRecordsToWrite = new BlockingCollection<LogSendRequest>();
                }
                return _queueOfLogRecordsToWrite;
            }
        }
#endif
        #endregion


#if !PRE_4 && OUTPUT_SVC
        #region QueueOfLogMessagesToSendToSvc
#if !PRE_4
        /// <summary>
        /// Get the queue of messages to send to the log-servicing process.
        /// </summary>
        /// <remarks>
        /// 'log-servicing process' means the separate program or Windows Service that is receiving our logs
        /// and writing or transmitting them for us, when that feature is used.
        /// </remarks>
        internal static BlockingCollection<IpcMessage> QueueOfLogMessagesToSendToSvc
        {
            get
            {
                if (_queueOfLogMessagesToSendToSvc == null)
                {
                    _queueOfLogMessagesToSendToSvc = new BlockingCollection<IpcMessage>();
                }
                return _queueOfLogMessagesToSendToSvc;
            }
        }
        internal static BlockingCollection<IpcMessage> _queueOfLogMessagesToSendToSvc;
#endif
        #endregion
#endif

        #endregion private properties

        #region non-public methods

        #region FilenameForTimeRollBackup
        /// <summary>
        /// Given a 'base' filename, produce the new filename that would be the rolling-date-backup
        /// that corresponds to the given backupTime.
        /// </summary>
        /// <param name="baseFileName">The filename to add the backup-date prefix to</param>
        /// <param name="backupTime">The DateTime that the backup-file corresponds to</param>
        /// <returns>A new filename of the form YYYY_MMDDHH_baseFileName, or without the DD or HH depending upon the FileOutputRollPoint</returns>
        private static string FilenameForTimeRollBackup( string baseFileName, DateTime backupTime )
        {
            string result = baseFileName;
            if (Config.FileOutputRollPoint != RollPoint.NoneSpecified)
            {
                var dirInfo = new DirectoryInfo(baseFileName);
                string filenamePart = dirInfo.Name;
                string directoryPart = StringLib.WithoutAtEnd(dirInfo.FullName, filenamePart);
                var sb = new StringBuilder();
                // For both TopOfDay and TopOfWeek, the form is:  YYYY_MMDD_baseFileName
                sb.Append( directoryPart ).Append( backupTime.Year.ToString() ).Append( "_" );
                if (backupTime.Month < 10)
                {
                    sb.Append( "0" + backupTime.Month.ToString() );
                }
                else
                {
                    sb.Append( backupTime.Month.ToString() );
                }
                // For TopOfMonth, the form is:  YYYY_MM_baseFileName
                if (Config.FileOutputRollPoint != RollPoint.TopOfMonth)
                {
                    if (backupTime.Day < 10)
                    {
                        sb.Append( "0" + backupTime.Day.ToString() );
                    }
                    else
                    {
                        sb.Append( backupTime.Day.ToString() );
                    }
                    if (Config.FileOutputRollPoint == RollPoint.TopOfHour)
                    {
                        // For TopOfHour, the form is:  YYYY_MMDDHH_baseFileName
                        if (backupTime.Hour < 10)
                        {
                            sb.Append( "0" + backupTime.Hour.ToString() );
                        }
                        else
                        {
                            sb.Append( backupTime.Hour.ToString() );
                        }
                    }
                }
                sb.Append( "_" ).Append( filenamePart );
                result = sb.ToString();
            }
            return result;
        }
        #endregion

        #region GetLoggerWithName
        /// <summary>
        /// See whether we have a logger by this name already within our list of Loggers that have already been instantiated,
        /// and return that if we do -- otherwise return null.
        /// </summary>
        /// <param name="name">The name we are inquiring about (case-insensitive)</param>
        /// <returns>the logger with the given name, if it exists -- otherwise null</returns>
        private static Logger GetLoggerWithName( string name )
        {
            Logger thatLogger = null;
            if (_theLoggers != null)
            {
                if (StringLib.HasNothing( name ))
                {
                    name = NameOfDefaultLogger;
                }
                foreach (var logger in _theLoggers)
                {
                    if (logger.Name.Equals( name, StringComparison.OrdinalIgnoreCase ))
                    {
                        thatLogger = logger;
                        break;
                    }
                }
            }
            return thatLogger;
        }
        #endregion

        #region HandleInternalFault
        /// <summary>
        /// Manage the fault-condition (like an exception). This writes it to the console and to the internal log,
        /// raises the LoggingFaultOccurred event, and (unless exceptions are being suppressed) throws the given exception.
        /// </summary>
        /// <param name="exception">the exception that gives rise to this fault</param>
        /// <param name="logRecord">the log-record that was being written when this fault occurred</param>
        /// <param name="format">a format-string to convey a simple user-message of this fault</param>
        /// <param name="args">an array of arguments for the format-string</param>
        [StringFormatMethod( "format" )]
        internal static void HandleInternalFault( Exception exception, LogRecord logRecord, string format, params object[] args )
        {
            string msg = String.Format(format, args);
            NutUtil.WriteToConsoleAndInternalLog( msg, exception );
            RaiseLoggingFaultOccurred( new object(), null, msg, exception, logRecord );

            if (exception != null && !Config.IsSuppressingExceptions)
            {
                throw new LoggingException( msg, exception );
            }
        }

        /// <summary>
        /// Raise the LoggingFaultOccurred event to signal a fault within the logging facility.
        /// </summary>
        /// <param name="sourceObject"></param>
        /// <param name="userSummaryMessage"></param>
        /// <param name="reason"></param>
        /// <param name="exception"></param>
        /// <param name="logRecord"></param>
        internal static void RaiseLoggingFaultOccurred( object sourceObject, string userSummaryMessage, string reason, Exception exception, LogRecord logRecord )
        {
            NutUtil.WriteToConsole( "LoggingFaultOccurred" );
#if !PRE_4
            LoggingFaultOccurred?.Invoke( sourceObject, new LoggingFaultEventArgs( userSummaryMessage, reason, exception, logRecord ) );
#else
            var eventCopy = LoggingFaultOccurred;
            if (eventCopy != null)
            {
                eventCopy(sourceObject, new LoggingFaultEventArgs(userSummaryMessage, reason, exception, logRecord));
            }
#endif
        }

        /// <summary>
        /// Raise the LoggingFaultOccurred event to signal a fault within the logging facility.
        /// </summary>
        /// <param name="sourceObject"></param>
        /// <param name="userSummaryMessage"></param>
        /// <param name="reason"></param>
        /// <param name="exception"></param>
        /// <param name="logRecord"></param>
        /// <param name="pathRedirectedTo"></param>
        internal static void RaiseLoggingFaultOccurred( object sourceObject, string userSummaryMessage, string reason, Exception exception, LogRecord logRecord, string pathRedirectedTo )
        {
            NutUtil.WriteToConsole( "LoggingFaultOccurred" );
#if !PRE_4
            LoggingFaultOccurred?.Invoke( sourceObject, new LoggingFaultEventArgs( userSummaryMessage, reason, exception, logRecord, isRedirected: true, pathRedirectedTo: pathRedirectedTo ) );
#else
            var eventCopy = LoggingFaultOccurred;
            if (eventCopy != null)
            {
                eventCopy(sourceObject, new LoggingFaultEventArgs(userSummaryMessage, reason, exception, logRecord, true, pathRedirectedTo));
            }
#endif
        }
        #endregion

        #region output pipes

        #region GetOutputPipe
        /// <summary>
        /// Return the element of the OutputPipes collection
        /// that is of the given generic-paramter class T, or null if not found.
        /// </summary>
        /// <typeparam name="T">any class that implements IOutputPipe</typeparam>
        /// <returns>either the first instance of the given subclass of IOutputPipe, or null</returns>
        public static T GetOutputPipe<T>() where T : IOutputPipe
        {
            T result = default(T);
            if (_outputPipes != null)
            {
                foreach (IOutputPipe outputPipe in LogManager.OutputPipes)
                {
                    if (outputPipe is T)
                    {
                        result = (T)outputPipe;
                        break;
                    }
                }
            }
            return result;
        }
        #endregion

        internal static List<IOutputPipe> _outputPipes;

        /// <summary>
        /// Get the list of IOutputPipes through which logging output will be transmitted.
        /// </summary>
        internal static IList<IOutputPipe> OutputPipes
        {
            get
            {
                if (_outputPipes == null)
                {
                    _outputPipes = new List<IOutputPipe>();
                }
                return _outputPipes;
            }
        }

        /// <summary>
        /// Connect the given output-pipe to our chain of output-pipes. This also Enables the output-pipe.
        /// </summary>
        /// <param name="outputPipe">the IOutputPipe to add to our output chain</param>
        public static void AttachOutputPipe( IOutputPipe outputPipe )
        {
#if DEBUG
            // First, ensure there are no other such pipes already attached.
            //CBL  ??
            Type typeOfNewPipe = outputPipe.GetType();
            foreach (var pipe in OutputPipes)
            {
                Type typeOfThisPipe = pipe.GetType();
                if (typeOfNewPipe == typeOfThisPipe)
                {
                    throw new InvalidOperationException( "An " + typeOfNewPipe + " has already been added!" );
                }
            }
#endif
            OutputPipes.Add( outputPipe );
            outputPipe.IsEnabled = true;
            outputPipe.InitializeUponAttachment( Config );
        }

        public static void EnableOnlyOutputPipe<T>() where T : IOutputPipe
        {
            if (_outputPipes != null)
            {
                foreach (IOutputPipe outputPipe in OutputPipes)
                {
                    if (outputPipe.GetType() == typeof( T ))
                    {
                        outputPipe.IsEnabled = true;
                    }
                    else
                    {
                        outputPipe.IsEnabled = false;
                    }
                }
            }
        }

        #endregion output pipes

        #region ThrowExceptionWasLogged
        /// <summary>
        /// Throw the ExceptionWasLogged event. This method is necessary because we need to cause the event to be thrown from
        /// outside of the LogManager class.
        /// </summary>
        /// <param name="logger">the Logger that logged the exception</param>
        /// <param name="exception">the Exception that was logged</param>
        /// <param name="additionalInformation">If not-null, this is the value that was supplied to the additionalInformation parameter.</param>
        internal static void ThrowExceptionWasLogged( Logger logger, Exception exception, string additionalInformation )
        {
            // I want to construct the LogEventArgs only if there is a listener to this event.
#if !PRE_4
            ExceptionWasLogged?.Invoke( logger, new LogEventArgs( loggerName: logger.Name, exception: exception, additionalInformation: additionalInformation ) );
#else
            var copyOfEvent = ExceptionWasLogged;
            if (copyOfEvent != null)
            {
                copyOfEvent(logger, new LogEventArgs(loggerName: logger.Name, exception: exception, additionalInformation: additionalInformation));
            }
#endif
        }
        #endregion

        #region Rollover
        /// <summary>
        /// Depending upon the current <see cref="RolloverMode"/>, cause a "rollover" by renaming any existing files
        /// if necessary.
        /// </summary>
        /// <param name="outputFileFolder">the filesystem-folder that the file output normally goes to</param>
        /// <param name="outputFilename">the filename of the log file</param>
        public static void Rollover( string outputFileFolder, string outputFilename )
        {
            //CBL This had been internal - but EtwOutput needs it. Do I really need to expose this in our public API ?

            if (outputFileFolder == null)
            {
                throw new ArgumentNullException( paramName: nameof( outputFileFolder ) );
            }
            if (outputFilename == null)
            {
                throw new ArgumentNullException( paramName: nameof( outputFilename ) );
            }

            try
            {
                string pathnameOfOutputFile = Path.Combine(outputFileFolder, outputFilename);
                // Derive the folder to use for the rolled-over files.
                string folderForArchivedFiles;
                if (Config.FileOutputArchiveFolder == null)
                {
                    folderForArchivedFiles = outputFileFolder;
                }
                else
                {
                    folderForArchivedFiles = Config.FileOutputArchiveFolder_FullPath;
                    if (!Directory.Exists( folderForArchivedFiles ))
                    {
                        Directory.CreateDirectory( folderForArchivedFiles );
                    }
                }
                // We need to change the existing file into a roll-backup and start creating a new one.
                int maxBackups = Config.MaxNumberOfFileRollovers;
                // First, see how many roll-backups we already have..
                bool emptySlotFound = false;
                for (int i = 1; i <= maxBackups; i++)
                {
                    string tentativeFilenameForRollover = FileStringLib.FilenameForRollover(outputFilename, i);
                    // Do the filename checks without regarding the extension, as that can vary
                    // (it may be .txt, or .zip).
                    //          string filenameWithoutExtension = filenameToCheck.PathnameWithoutExtension();
                    //          string outputFolder = LogManager.FileOutputFolder;
                    //          string pattern = filenameWithoutExtension + ".*";
                    //          if (!ZIOHelper.FilesExistThatMatchPattern(outputFolder, pattern))
                    bool isConflict = false;
                    string pathnameOfRolloverFile = Path.Combine(folderForArchivedFiles, tentativeFilenameForRollover);
                    if (Config.IsFileOutputToCompressFiles)
                    {
                        if (FilesystemLib.FileExists( pathnameOfRolloverFile ))
                        {
                            isConflict = true;
                        }
                        else
                        {
                            string filenameCompressed = tentativeFilenameForRollover.PathnameWithoutExtension() + ".zip";
                            string pathnameOfRolloverFileCompressed = Path.Combine(folderForArchivedFiles, filenameCompressed);
                            if (FilesystemLib.FileExists( pathnameOfRolloverFileCompressed ))
                            {
                                isConflict = true;
                            }
                        }
                    }
                    else
                    {
                        isConflict = FilesystemLib.FileExists( pathnameOfRolloverFile );
                    }

                    if (!isConflict)
                    {
                        emptySlotFound = true;
                        // Rename the existing pathName to filenameToCheck.
                        // But if it happens to be locked and this fails - then redirect it to a different file.
                        //CBL But I want to fix this next line so that it doesn't keep making an entry for every retry-attempt.

                        FilesystemLib.MoveFile( pathnameOfOutputFile, pathnameOfRolloverFile, s => NutUtil.WriteToConsoleAndInternalLog( s ), FilesystemLib.DefaultRetryTimeLimit );

                        // Do any post-processing that needs to be done on that file now that we have rolled it over.
                        NutUtil.PostProcessLogFile( pathnameOfRolloverFile );
                        break;
                    }
                }

                //CBL
                // This is unfinished. Trying to account for ZIPped files!
                if (!emptySlotFound)
                {
                    // All N backups have been created already.
                    // Delete backup 1 (the oldest), and renumber the rest to be 1 less.
                    // eg
                    //   Log.txt     -> rename to Log(3).txt
                    //   Log(1).txt  -> delete
                    //   Log(2).txt  -> rename to Log(1).txt
                    //   Log(3).txt  -> rename to Log(2).txt
                    //
                    string outputFilePath = Path.Combine(folderForArchivedFiles, outputFilename);
                    string pathnameToDelete = FileStringLib.FilenameForRollover(outputFilePath, 1);
                    FilesystemLib.DeleteFile( pathnameToDelete );
                    for (int i = 2; i <= maxBackups; i++)
                    {
                        string pathnameToRename = FileStringLib.FilenameForRollover(outputFilePath, i);
                        string pathnameToRenameTo = FileStringLib.FilenameForRollover(outputFilePath, i - 1);
                        FilesystemLib.MoveFile( pathnameToRename, pathnameToRenameTo );
                    }

                    string filenameToRenameCurrentFileTo = FileStringLib.FilenameForRollover(outputFilePath, maxBackups);
                    FilesystemLib.MoveFile( pathnameOfOutputFile, filenameToRenameCurrentFileTo );
                    // Do any post-processing that needs to be done on that file now that we have rolled it over.
                    NutUtil.PostProcessLogFile( filenameToRenameCurrentFileTo );
                }
            }
            catch (Exception x)
            {
                x.Data.Add( key: nameof( outputFileFolder ) + ":", value: StringLib.AsQuotedString( outputFileFolder ) );
                x.Data.Add( key: nameof( outputFilename ) + ":", value: StringLib.AsQuotedString( outputFilename ) );
                throw;
            }
        }
        #endregion Rollover

        #region QueueOrSendToLog
        /// <summary>
        /// The internal write-to-log method that handles ALL log output.
        /// </summary>
        /// <param name="logger">the Logger that is sending this log</param>
        /// <param name="logRecord">what to log</param>
        /// <remarks>
        /// When a write actually happens (ie, is not disabled for some reason) this logger gets stored in the Windows Registry.
        /// </remarks>
        internal static void QueueOrSendToLog( Logger logger, LogRecord logRecord, bool isToSuppressTraceOutput )
        {
            if (!IsClearSet)
            {
                SetClear();
            }
            SomethingWasLogged?.Invoke( null, new LogEventArgs( logRecord ) );
            //CBL Do we want this try/catch block here?
            try
            {
                // Note: The enablement of logging, and of this particular level, is already gated
                //       before this method is called.
                // Compose the log record..

                // If this TestFacility feature is enabled (as when running unit-tests),
                // save these log-records in a special collection -
                // instead of sending them out as we would ordinarily do.

                // If we're not simulating logging (as in unit-testing)..
                bool isTestSimulating = false;
                if (TestFacility != null)
                {
                    if (TestFacility.IsSimulatingOutput)
                    {
                        isTestSimulating = true;
                    }
                }
                if (!IsTesting && !isTestSimulating)
                {
                    // Real output (not just testing).
                    var logSendRequest = new LogSendRequest(logger, logRecord, isToSuppressTraceOutput);

#if !PRE_4 && OUTPUT_SVC
                    if (Config.IsWindowsServiceOutputEnabled && !_hasServiceStopped)
                    {
                        OutputToService( logSendRequest );
                    }
                    else if (Config.IsAsynchronous)
#else
                    if (Config.IsAsynchronous)
#endif
                    {
                        SendViaTransmissionTask( logSendRequest );
                    }
                    else
                    {
                        Send( logSendRequest );
                    }
                    // Whenever a log record gets sent, note this logger in the Windows Registry so this fact may be known
                    // by other applications.
                    //#if !SILVERLIGHT && !NETFX_CORE
                    //                    if (LogManager.Config.IsUsingRegistry)
                    //                    {
                    //                        AddMeToRegistry();
                    //                    }
                    //#endif
                }
                else // this is a simulation of logging.
                {
                    _testFacility = TestFacility;

                    var configForConsoleOut = LogConfig.GetForConsoleOutput(Config);
                    string recordAsText = DefaultLogRecordFormatter.The.GetLogRecordAsText(logRecord, configForConsoleOut);
                    NutUtil.WriteToConsole( recordAsText );

                    if (_testFacility.IsCounting)
                    {
                        if (_testFacility.RecordsSinceLastReset.Count < _testFacility.MaxTestCount)
                        {
                            _testFacility.RecordsSinceLastReset.Add( logRecord );
                        }
                    }
                }
            }
            catch (Exception x)
            {
                string msg = "(Level = " + logRecord.Level + ", Message = \"" + logRecord.Message + "\")";
                throw new LoggingException( msg, logRecord.Level, logRecord.Message, x );
            }
        }
        #endregion QueueOrSendToLog

        #region SendViaTransmissionTask
        /// <summary>
        /// For loggers that are set to transmit asynchronously, this adds the given log record to the queue to be transmitted by a separate thread.
        /// </summary>
        /// <param name="logSendRequest">the LogSendRequest object that contains the logger and the log-record it wants to send</param>
        internal static void SendViaTransmissionTask( LogSendRequest logSendRequest )
        {
            // The purpose of this lock is to ensure the Queue is serviced and made empty before the QueueOfLogsToWriteEmptiedEvent goes positive.
            // It would be possible for someone to send a log-output, and then after this call to TryAdd of it to the Queue, but before the Reset --
            //  that the subject-program decides to exit and calls WaitOne on this event. With unlucky timing it could return from WaitOne before the event is rest,
            // and thus that log-output might not go out.
            lock (_queueLockObject)
            {
                // Block no more than 100ms while attempting to add this to the queue.
                if (QueueOfLogRecordsToWrite.TryAdd( logSendRequest, TimeSpan.FromMilliseconds( 100 ) ))
                {
                    // Clear the ManualResetEvent's contained flag to false, so that any other threads who want to wait until the output has been serviced
                    // (via a called to WaitOne) will block now until it has been.
                    QueueOfLogsToWriteEmptiedEvent.Reset();
                }
                else
                {
                    NutUtil.WriteToConsoleAndInternalLog( "Queue is full, dumping log-record." );
#if !PRE_4
                    RaiseLoggingFaultOccurred( sourceObject: null,
                                               userSummaryMessage: "LogNut log-queue is full, now dumping log-records.",
                                               reason: "In LogManager.SendViaTransmissionTask.",
                                               exception: null,
                                               logRecord: logSendRequest.Record );
#else
                    RaiseLoggingFaultOccurred(null,
                                               "LogNut log-queue is full, now dumping log-records.",
                                               "In LogManager.SendViaTransmissionTask.",
                                               null,
                                               logSendRequest.Record);
#endif
                }
            } // end lock.

#if !PRE_4
            try
            {
                _cancellationToken = OurCancellationTokenSource.Token;
                //else
                //{
                //    _cancellationToken.ThrowIfCancellationRequested();
                //}

                // If the task which will send out the log records has not been created yet,
                // create and start a new thread..
                if (_logTransmissionTask == null)
                {
                    if (_logTransmissionTask != null)
                    {
                        Debug.WriteLine( "before launching new xmtTask, it's Status is " + _logTransmissionTask.Status );
                    }

                    _logTransmissionTask = Task.Factory.StartNew( () =>
                    {
                        SendLogs();

                    }, _cancellationToken )

                    .ContinueWith( t =>
                    {
                        var aggregateException = t.Exception.Flatten();
                        foreach (var exception in aggregateException.InnerExceptions)
                        {
                            NutUtil.WriteToConsoleAndInternalLog( "in LogManager.SendViaTransmissionTask: ", exception );
                        }
                        // This is so that it may resume recreated upon the next log-output.
                        _logTransmissionTask = null;

                    }, TaskContinuationOptions.OnlyOnFaulted );

                }
            }
            catch (AggregateException x)
            {
                //CBL  Hey - this fails to catch exceptions!!
                // This only catches exceptions that occur from the creation or starting of that task.
                // This may not need to be AggregateException.
                foreach (var innerException in x.InnerExceptions)
                {
                    if (!(innerException is System.Threading.Tasks.TaskCanceledException))
                    {
                        //CBL
                        RaiseLoggingFaultOccurred( new object(), null, "in LogManager.SendViaTransmissionTask", innerException, logSendRequest.Record );
                        NutUtil.WriteToConsoleAndInternalLog( "in LogManager.SendViaTransmissionTask, innerException is ", innerException );
                    }
                }
            }
#else
            throw new NotImplementedException("Sorry - this has not been implemented yet for .NET Framework 3.5");
#endif
        }
        #endregion SendViaTransmissionTask

        #region SendLogs
        /// <summary>
        /// This method is executed by the background thread to work through the queue of log requests and send those out.
        /// </summary>
        private static void SendLogs()
        {
            // This is a long-running task, to execute until the Clear method tells it no more is coming (if logging is asynchronous).
            //
            bool isToKeepRunning = true;

#if !DEBUG
            //var timerSaveFiles = new System.Threading.Timer( callback: _ => OnTimer(), state: null, dueTime: 1000, period: 1000 );
#endif

            // While no log-output is coming in, this loops once per second: checking the Queue for one second, and then checking for termination before looping around to checking the Queue again.
            // When there are log-requests in the queue, there is no wait - this loops at full speed until the Queue is emptied.
            while (isToKeepRunning)
            {
                // Take one out of the queue
                LogSendRequest logSendRequest;

                // This will block up to the given time-interval waiting for a log-message.
                // That interval needs to be short-enough to handle a cancellation in a timely manner,
                // and to output log requests quickly enough.
#if !PRE_4
                if (QueueOfLogRecordsToWrite.TryTake( out logSendRequest, 1000 ))
#else
                if (QueueOfLogRecordsToWrite.TryTake(out logSendRequest, TimeSpan.FromMilliseconds(1000)))
#endif
                {
                    // Send it
                    Send( logSendRequest );
                    // _hasNewLogs = true;
                }
                else // the queue is empty.
                {
                    // Check for either the Queue being set as final (no more additions) or for cancellation here,
                    // since we have (for this moment) serviced all of the log-output..
#if !PRE_4
                    if (QueueOfLogRecordsToWrite.IsCompleted)
                    {
                        Debug.WriteLine( "Breaking out of async-write loop because IsCompleted." );
                        isToKeepRunning = false;
                    }
                    else if (_cancellationToken.IsCancellationRequested)
                    {
                        // Break out of the loop when cancellation is requested.
                        Debug.WriteLine( "Breaking out of async-write loop because cancelled." );
                        isToKeepRunning = false;
                    }
                    //CBL Need to implement cancellation for .NET 3.5
#endif
                }

            } // end loop.

            // Unblock any other threads that are waiting upon the Queue to become empty,
            // in case anyone is wanting to know when the queue is finally finished being processed.
            QueueOfLogsToWriteEmptiedEvent.Set();
        }
        #endregion

        // Timer, to save/re-open the log files.
        //public static void OnTimer()
        //{
        //    Debug.WriteLine( "OnTimer!!" );
        //    if (_hasNewLogs)
        //    {
        //        NutUtil.CloseTheOutputFile();
        //        if (Config.IsFileOutputFilenameToIncludeDateTime)
        //        {
        //            FileLib.RecreateFileOutputFilename();
        //        }
        //    }
        //    _hasNewLogs = false;
        //}
        //private static bool _hasNewLogs;

        #region Send
        /// <summary>
        /// The internal write-to-log method that handles ALL log output.
        /// When a write actually happens (ie, is not disabled for some reason) this logger gets stored in the Windows Registry.
        /// </summary>
        /// <param name="logSendRequest">the LogSendRequest object that contains the log-record to send</param>
        /// <returns>true if the log-record did go out, false otherwise</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)" )]
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "LogNut.LogManager.RaiseFileRedirectionFaultOccurred(System.Object,System.String,System.Exception,LogNut.LogRecord,System.String)" )]
        internal static bool Send( LogSendRequest logSendRequest )
        {
            // The general format, if not in XML, is  CBL
            // where the 'v' is present to indicate when we're in 'design-mode' (that stands for "Visual designer").

            // Output it to the Console, if we're doing that..
            bool didThisRecordGoOut = SendOutputToConsole(logSendRequest);

            #region file output
            // Attend to the file output, if we're doing that..
            // Both LogManager and the Logger have the ability to enable or disable this; the Logger's property overrides that of the LogManager.
            // Thus, it does get written to file if Logger's property is true, or if it is null and LogManager says it may.
            if (Config.IsFileOutputEnabled)
            {
                string sRecord = NutUtil.RenderFileOutput(logRecord: logSendRequest.Record, config: Config, isFirstFileOutput: _isFirstFileOutput);
                // Now sRecord has the log record rendered as a string.

                // not using a service - we are writing it out our own dam self.
                lock (_logFileWriteLockObject)
                {
                    // Write the log-record to the file:
                    //
                    didThisRecordGoOut = OutputToFile( logSendRequest: logSendRequest, sRecord: sRecord, isThisFirstFileOutput: _isFirstFileOutput );

                } // end lock-region.

                // Remember this so that upon subsequent file-outputs we know it is not the first file-output.
                _isFirstFileOutput = false;
            } // end if file-output is requested.
            #endregion file output

            // Attend to any IOutputPipes that are attached..
            SendOutputToPipes( logSendRequest, ref didThisRecordGoOut );

            return didThisRecordGoOut;
        }
        #endregion Send

        /// <summary>
        /// Get or set the integer that identifies every log-output.
        /// This gets incremented for every log that is sent out, and serves to help identify when one is missing.
        /// </summary>
        public static int LogNumber
        {
            get { return _logNumber; }
            set { _logNumber = value; }
        }

        private static int _logNumber;

#if !PRE_4 && OUTPUT_SVC
        public static void OutputToService( LogSendRequest logSendRequest )
        {
            //Say( "begin LogManager.OutputToService" );

            // Output it to the Console, if we're doing that..
            //bool didThisRecordGoOut = SendOutputToConsole( logSendRequest );
            IpcMessage ipcMessage = new IpcMessage();
            string recordAsText = NutUtil.RenderFileOutput(logSendRequest: logSendRequest, config: Config, isFirstFileOutput: _isFirstOutputToService);
            _isFirstOutputToService = false;
            ipcMessage.Content = recordAsText;
            ipcMessage.Operation = LogOperationId.LogThis;
            ipcMessage.LogNumber = _logNumber++;

            // The purpose of this lock is to ensure the Queue is serviced and made empty before the QueueOfMsgsToSvcEmptiedEvent goes positive.
            // It would be possible for someone to send a log-output, and then after this call to TryAdd of it to the Queue, but before the Reset --
            //  that the subject-program decides to exit and calls WaitOne on this event. With unlucky timing it could return from WaitOne before the event is rest,
            // and thus that log-output might not go out.
            lock (_queueLockObject)
            {
                // Block no more than 100ms while attempting to add this to the queue.
                if (QueueOfLogMessagesToSendToSvc.TryAdd( ipcMessage, TimeSpan.FromMilliseconds( 100 ) ))
                {
                    // Clear the ManualResetEvent's contained flag to false, so that any other threads who want to wait until the output has been serviced
                    // (via a called to WaitOne) will block now until it has been.
                    QueueOfMsgsToSvcEmptiedEvent.Reset();
                }
                else
                {
                    NutUtil.WriteToConsoleAndInternalLog( "Queue is full, dumping log-record." );
                }
            } // end lock.

            // We guard entry into this block because we do not want a second thread to barge in and create this task before the current thread does.
            lock (_serviceOutputTaskCreationLock)
            {
                if (!_hasOutputTaskBeenLaunched)
                {
                    var cancellationToken = OurCancellationTokenSource.Token;
                    var uiTaskScheduler = TaskScheduler.FromCurrentSynchronizationContext();

                    Task t1 = Task.Factory.StartNew(() =>
                    {
                        DoOutputToServiceViaPipes(cancellationToken: cancellationToken);

                    }, cancellationToken, TaskCreationOptions.AttachedToParent, TaskScheduler.Default);

                    t1.ContinueWith( _ => { ContinueNotFaulted(); }, cancellationToken, TaskContinuationOptions.NotOnFaulted, uiTaskScheduler );
                    t1.ContinueWith( _ => { ContinueOnError( t1.Exception ); }, cancellationToken, TaskContinuationOptions.OnlyOnFaulted, uiTaskScheduler );
                    _hasOutputTaskBeenLaunched = true;
                }
            } // end lock.
            //Say( "end LogManager.OutputToService" );
        }

        private static object _serviceOutputTaskCreationLock = new object();
        private static bool _hasOutputTaskBeenLaunched;
        internal static bool _hasServiceStopped;

        public static void CancelServiceTask()
        {
            OurCancellationTokenSource.Cancel();
        }
#endif

        //TODO
        // This region begs for proper documentation.
#if FALSE
        #region DoOutputToServiceViaMmf
#if !PRE_4
        public static void DoOutputToServiceViaMmf( CancellationToken cancellationToken )
        {
            Say( "begin LogManager.DoOutputToServiceViaMmf" );

            try
            {
                // Create the memory-mapped file which allows 'Reading' and 'Writing'
                using (MemoryMappedFile mmf = MemoryMappedFile.CreateOrOpen( MmfProtocol.MmfName, MmfProtocol.MMF_MAX_SIZE, MemoryMappedFileAccess.ReadWrite ))
                {
                    Say( "in DoOutputToService, have MMF" );
                    // Create a view-stream for this process, which allows us to write data from offset 0 to 1024 (whole memory)
                    using (MemoryMappedViewStream mmvStream = mmf.CreateViewStream( 0, MmfProtocol.MMF_VIEW_SIZE ))
                    {
                        Say( "got mmvStream" );

                        // Loop waiting for the Mutex to appear...
                        bool isToKeepRunning = true;
                        while (!cancellationToken.IsCancellationRequested && isToKeepRunning)
                        {
                            Mutex mutexLockMmf = null;
                            bool doesMutexExist = false;
                            //bool isUnauthorized = false;
                            // Attempt to open the Mutex..
                            try
                            {
                                //Say( "OpenExisting Mutex.." );
                                mutexLockMmf = Mutex.OpenExisting( name: MmfProtocol.MutexNameLockMmf );
                                doesMutexExist = true;
                                //Say( "I see the mutexLockMmf now!" );
                            }
                            catch (WaitHandleCannotBeOpenedException x1)
                            {
                                Say( "x1.Message is " + x1.Message + " - this is normal while waiting for it to be created by the LognutService." );
                                doesMutexExist = false;
                            }
                            catch (UnauthorizedAccessException x2)
                            {
                                Say( "mutexLockMmf - Unauthorized access: " + x2.Message );
                                //isUnauthorized = true;
                                doesMutexExist = false;
                            }

                            if (doesMutexExist)
                            {
                                //using (EventWaitHandle eventwhLogAvail = new EventWaitHandle( initialState: false, mode: EventResetMode.AutoReset, name: MmfProtocol.EventNameLogAvail, createdNew: out bool isCreatedNewEventwhLogAvail ))
                                using (EventWaitHandle eventwhLogAvail = EventWaitHandle.OpenExisting( name: MmfProtocol.EventNameLogAvail ))
                                {
                                    Say( "opened eventwhLogAvail" );

                                    //using (EventWaitHandle eventwhResponding = new EventWaitHandle( initialState: false, mode: EventResetMode.AutoReset, name: MmfProtocol.EventNameResponding, createdNew: out bool isCreatedNewEventwhResponding ))
                                    using (EventWaitHandle eventwhResponding = EventWaitHandle.OpenExisting( name: MmfProtocol.EventNameResponding ))
                                    {
                                        Say( "opened eventwhResponding" );

                                        // Since this Mutex already exists, that indicates the LognutWindowsService is running.
                                        try
                                        {
                                            while (!cancellationToken.IsCancellationRequested && isToKeepRunning)
                                            {
                                                if (QueueOfLogMessagesToSendToSvc.TryTake( out IpcMessage ipcMessage, 2000 ))
                                                {

                                                    bool doWeHaveMutex = false;
                                                    // Wait for the LogSvc to become ready..
                                                    // If the LognutWindowsService is not up, then this will return immediately.
                                                    try
                                                    {
                                                        // I need to wait upon mutexLockMmf to know that the previous message has been processed.
                                                        doWeHaveMutex = mutexLockMmf.WaitOne( timeout: TimeSpan.FromSeconds( 10 ) );
                                                        _isMutexReleased = false;
                                                    }
                                                    catch (AbandonedMutexException x3)
                                                    {
                                                        Say( "Abandoned Mutex: " + x3.Message );
                                                        isToKeepRunning = false;
                                                        break;
                                                    }

                                                    if (doWeHaveMutex)
                                                    {
                                                        if (!_hasAnnouncedAcquiredMutex)
                                                        {
                                                            Say( "Acquired mutex. Sending log..." );
                                                            _hasAnnouncedAcquiredMutex = true;
                                                        }

                                                        //string recordAsText = NutUtil.RenderFileOutput( logSendRequest: logSendRequest, config: Config, isFirstFileOutput: _isFirstOutputToService );

                                                        // Write log to the MMF..
                                                        // Use protobuf-net to serialize the variable 'message1' and write it to the memory mapped file.

                                                        Serializer.SerializeWithLengthPrefix( destination: mmvStream, instance: ipcMessage, style: PrefixStyle.Base128 );


                                                        // sets the current position back to the beginning of the stream
                                                        mmvStream.Seek( 0, SeekOrigin.Begin );

                                                        //CBL I may need to test these next 2 Waits, in case of error - so I can revert to other logging .

                                                        // Release the eventwhLogAvail that allows Rcv to proceed to process it's log
                                                        // Since this is an AutoReset event, it Resets after the Rcv thread has proceeded through.
                                                        Say( "setting eventwhLogAvail." );
                                                        eventwhLogAvail.Set();

                                                        // Wait for eventwhResponding signal from Rcv that tells me it is ready for me to block
                                                        // Because I have not released mutexLockMmf yet, I know that Xmt cannot speed around the re-process the MMF too soon.
                                                        Say( "eventwhResponding WaitOne." );
                                                        bool isGot = eventwhResponding.WaitOne(TimeSpan.FromSeconds(10));
                                                        if (isGot)
                                                        {
                                                            Say( "eventwhResponding aquired." );
                                                        }
                                                        else
                                                        {
                                                            Say( "eventwhResponding timed-out!! Aborting this shit." );
                                                            _hasServiceStopped = true;
                                                            isToKeepRunning = false;
                                                        }

                                                        // Set the eventwhLogAvail to block such that I am back to state 1.
                                                        // Now I can release mutexLockMmf because Rcv will stop when it loops back to wait on it.

                                                        // Release mutexLockMmf (the mutex that protects the MMF) so that now Xmt may do it's work.
                                                        mutexLockMmf.ReleaseMutex();
                                                        _isMutexReleased = true;
                                                        //Say( "mutexLockMmf released." );

                                                    }
                                                    else
                                                    {
                                                        Say( "mutex timeout." );
                                                    }
                                                }
                                                else // The Queue is empty at this moment.
                                                {
                                                    Say( "Queue is empty at this moment." );
                                                    // Check for either the Queue being set as final (no more additions) or for cancellation here,
                                                    // since we have (for this moment) serviced all of the log-output..
                                                    if (QueueOfLogMessagesToSendToSvc.IsCompleted)
                                                    {
                                                        Say( "Breaking out of send-to-svc loop because IsCompleted." );
                                                        isToKeepRunning = false;
                                                    }
                                                    else if (_cancellationToken.IsCancellationRequested)
                                                    {
                                                        // Break out of the loop when cancellation is requested.
                                                        Say( "Breaking out of send-to-svc loop because cancelled." );
                                                        isToKeepRunning = false;
                                                    }
                                                }
                                            } // end loop.
                                        }
                                        finally
                                        {
                                            Say( "DoOutputToService finally block" );
                                            // Unblock any other threads that are waiting upon the Queue to become empty,
                                            // in case anyone is wanting to know when the queue is finally finished being processed.
                                            QueueOfMsgsToSvcEmptiedEvent.Set();
                                            // Ensure we do not leave while holding on to the mutexSvcReady.
                                            if (!_isMutexReleased)
                                            {
                                                Say( "in DoOutputToService finally block, releasing Mutex." );
                                                mutexLockMmf.ReleaseMutex();
                                                _isMutexReleased = true;
                                            }
                                        }
                                        break;
                                    } // end using eventwhResponding.
                                } // end using eventwhLogAvail.
                            }
                            else // Mutex does not exist yet.
                            {
                                Say( "Waiting 2 more seconds for Mutex to appear." );
                                Thread.Sleep( 2000 );
                            }
                        } // end outer look-for-mutex-exist loop.

                        _hasServiceStopped = true;
                        if (!isToKeepRunning)
                        {
                            Say( "in DoOutputToService, exited loop because !isToKeepRunning." );
                        }
                        else
                        {
                            Say( "in DoOutputToService, exited loop because of cancellation." );
                        }
                    }
                }

            }
            catch (Exception x)
            {
                Say( "LogManager.DoOutputToService: OOps! " + x.Message );
            }
            Say( "end LogManager.DoOutputToServiceViaMmf" );
        }
#endif
        #endregion DoOutputToServiceViaMmf
#endif

        //private static bool _hasAnnouncedAcquiredMutex;
        //private static bool _isFirstOutputToService = true;
        //private static bool _isMutexReleased;

        [Conditional( "TRACE" )]
        public static void Say( string what )
        {
            DateTime now = DateTime.Now;
            string text = String.Format("{0:yyyy-MM-dd HH:mm:ss}", now);
            // Relace the space with an underscore, and remove any colons.
            string whenPart = text.Replace(" ", "_").Replace(":", "");
            string msg = whenPart + "[" + Thread.CurrentThread.ManagedThreadId + "] " + what;

            Debug.WriteLine( what );
#if TRACE_LOGSVC
            WriteToSvcLogFile( msg );
#endif
        }

        #region WriteToFile
        /// <summary>
        /// Write the given text to a file that is reserved for this log-service related business.
        /// </summary>
        /// <param name="message"></param>
        public static void WriteToSvcLogFile( string message )
        {
            Debug.WriteLine( "[" + Thread.CurrentThread.ManagedThreadId + "] begin WriteToSvcLogFile" );
            lock (FileLockObject)
            {
                if (!Directory.Exists( LogDir ))
                {
                    Directory.CreateDirectory( LogDir );
                }
                if (_logManagerSvcLogPathname == null)
                {
                    string filename = NutFileLib.CreateLogOutputFilenameWithDate(originalFilenameWithoutExtension: LogManagerSvcLogBaseFilename, extension: "txt");
                    Debug.WriteLine( "[" + Thread.CurrentThread.ManagedThreadId + "] in LogManager.WriteToSvcLogFile, created new file " + filename );
                    _logManagerSvcLogPathname = Path.Combine( LogDir, filename );
                }
                if (!File.Exists( _logManagerSvcLogPathname ))
                {
                    // Create a file to write to.   
                    using (StreamWriter sw = File.CreateText( _logManagerSvcLogPathname ))
                    {
                        sw.WriteLine( message );
                    }
                }
                else
                {
                    using (StreamWriter sw = File.AppendText( _logManagerSvcLogPathname ))
                    {
                        sw.WriteLine( message );
                    }
                }
            }
        }
        #endregion

        private static object FileLockObject = new Object();
        private const string LogDir = @"C:\LuvivaLog";
        private const string LogManagerSvcLogBaseFilename = "LogMgrSvcLog";
        private static string _logManagerSvcLogPathname;

#if !PRE_4
        #region OurCancellationTokenSource
        /// <summary>
        /// Get our CancellationTokenSource
        /// </summary>
        private static CancellationTokenSource OurCancellationTokenSource
        {
            get
            {
                if (_cancellationTokenSource == null)
                {
                    _cancellationTokenSource = new CancellationTokenSource();
                }
                return _cancellationTokenSource;
            }
        }
        private static CancellationTokenSource _cancellationTokenSource;
        #endregion

        public static void ContinueNotFaulted()
        {
#if !PRE_4 && OUTPUT_SVC
            _hasServiceStopped = true;
            Say( "LogManager.ContinueNotFaulted: Done." );
#endif
        }

        public static void ContinueOnError( Exception x )
        {
#if !PRE_4 && OUTPUT_SVC
            _hasServiceStopped = true;
            //Logger.LogException( x, "Within ContinueWorkFirstRunOnError" );
            string msg = "LogManager.ContinueOnError: " + StringLib.ExceptionShortSummary(x);
            Say( msg );
#endif
        }
#endif

        #region GetOutputDirToUse
        /// <summary>
        /// Return the file-folder to put the logging output into.
        /// This will be FileOutputFolder unless it's preferred to place it onto a removable-drive.
        /// Create the dir if necessary.
        /// </summary>
        /// <returns>the directory to put the logging-output into at this current moment</returns>
        public static string GetOutputDirToUse( bool isToCreateIfNeccesary, out bool wasPreferringRemovableDriveButIsDown )
        {
            string destinationDir = Config.FileOutputFolder;
            wasPreferringRemovableDriveButIsDown = false;
            //CBL Need to merge some of this with the code in LogManager.OutputToFile.

            // Have we been configured to write to a removable drive?
            if (Config.RemovableDrivePreferredFileOutputFolder != null)
            {
                // If the removable-drive is DOWN, just go with the FileOutputFolder.
                if (Config.RemovableDrive != null && Directory.Exists( Config.RemovableDrive ))
                {
                    // It's up.
                    destinationDir = Config.RemovableDrivePreferredFileOutputFolder;
                }
                else
                {
                    wasPreferringRemovableDriveButIsDown = true;
                }
            } // end if removable-drive is indicated.

            if (isToCreateIfNeccesary)
            {
                // Create the output directory if it does not exist yet.
                if (!Directory.Exists( destinationDir ))
                {
                    FilesystemLib.CreateDirectory( destinationDir );
                }
            }

            return destinationDir;
        }
        #endregion GetOutputDirToUse

        #region OutputToFile
        /// <summary>
        /// Write the log to the appropriate file.
        /// </summary>
        /// <param name="logSendRequest"></param>
        /// <param name="sRecord"></param>
        /// <param name="isThisFirstFileOutput"></param>
        /// <returns>true if the log did actually get written to a file</returns>
        private static bool OutputToFile( LogSendRequest logSendRequest, string sRecord, bool? isThisFirstFileOutput )
        {
            //CBL  Surely I can find a way to simplify this huge expanse of code!
            //CBL  Need to add: regardless of the rollover-mode, it should respect the max-number-of-files limit.
            LogRecord logRecord = logSendRequest.Record;
            bool didLogGetWritten = false;
            bool wasPreferringRemovableDriveButIsDown;
            string destinationDir = GetOutputDirToUse(false, out wasPreferringRemovableDriveButIsDown);
            string outputFilename = Config.GetFileOutputFilename_ExplicitlySetOrDefault();
            string outputFilenameToUse = outputFilename;

            // A default.
            bool isToAppend = true;
            bool isToRolloverNow = false;
            RolloverMode rolloverModeToUse = Config.FileOutputRolloverMode;

            //CBL Here, is where I make the change to allow for a (preferred) USB-drive destination
            //    but a fallback otherwise to a more solid destination, and to test this every time.
            //
            // This is tentative code, and must be moved below..

            // The output directory:
            //
            if (wasPreferringRemovableDriveButIsDown)
            {
                //CBL Test this for output-files with and without extensions.
                string extension = FileStringLib.GetExtension(outputFilename);
                if (StringLib.HasSomething( extension ))
                {
                    outputFilenameToUse = FileStringLib.GetFileNameWithoutExtension( outputFilename ) + SUFFIX_FOR_FILES_DIVERTED_FROM_REMOVABLE + "." + extension;
                }
                else
                {
                    outputFilenameToUse = FileStringLib.GetFileNameWithoutExtension( outputFilename ) + SUFFIX_FOR_FILES_DIVERTED_FROM_REMOVABLE;
                }
            }


            // Ensure the directory exists, where the log-file is intended to go.

            // If the output-folder is not there,
            if (!FilesystemLib.DirectoryExists( destinationDir ))
            {
                isToRolloverNow = false;

                // then create it.
                try
                {
                    Directory.CreateDirectory( destinationDir );
                }
                catch (Exception x)
                {
                    // Problem trying to create the destination directory.
                    // Look around for some other place to put it...
                    //

                    bool isResolved = false;
                    //CBL  What the hell is all this shit? !!!

                    //CBL Must add tests to verify the correct operation
                    // when unable to create the output folder.
                    // Also, we should get some indication that it had to change output folder.

                    AccessibilityEvaluator writeEvaluator = new AccessibilityEvaluator();
                    string descriptionOfPathChosen, reason;
#if !PRE_4
                    string newOutputDir = NutUtil.FindWorkingFileOutputFolder(writeEvaluator, firstPathToTry: destinationDir, secondPathToTry: null, descriptionOfPathChosen: out descriptionOfPathChosen, reason: out reason);
#else
                    string newOutputDir = NutUtil.FindWorkingFileOutputFolder(writeEvaluator, destinationDir, null, out descriptionOfPathChosen, out reason);
#endif
                    // If we found a different destination..
                    if (!destinationDir.Equals( newOutputDir, StringComparison.OrdinalIgnoreCase ))
                    {
                        try
                        {
                            string proposedPath = Path.Combine(newOutputDir, outputFilenameToUse);
                            // If there is no log-file in this folder yet,
                            // see if we can actual create one and write to it, before you accept this as our new output-folder.
                            if (!File.Exists( proposedPath ))
                            {
                                // See whether we can create and delete a file in this folder (that is, do we have write permissions to it?).
                                FilesystemLib.WriteText( proposedPath, "Just testing to see whether I can create a file here." );
                                FilesystemLib.DeleteFile( proposedPath );
                            }
                            NutUtil.WriteToConsoleAndInternalLog( @"Unable to write to existing output-folder ""{0}"" ({1}), directing the output to {2} instead.", destinationDir, x.Message, proposedPath );
                            Config.FileOutputFolder = newOutputDir;
                            isResolved = true;
                        }
                        catch (Exception x2)
                        {
                            NutUtil.WriteToConsoleAndInternalLog( "Exception {0}: {1}, when attempting to create a file within the execution directory.", x2.GetType(), x2.Message );
                        }
                    }

                    if (!isResolved)
                        throw;
                }
            }
            else // normal flow - yes the output directory is there.
            {
                if (isThisFirstFileOutput.HasValue && isThisFirstFileOutput.Value == true)
                {
                    isToRolloverNow = Config.IsToCreateNewOutputFileUponStartup;
                    isToAppend = !isToRolloverNow;
                }
            } // end if else output-directory is present.


            string destinationPathname = Path.Combine(destinationDir, outputFilenameToUse);


            // Rollover:
            //

            // Rolling over only applies if the file is already there.
            bool doesFileExistAlready = FilesystemLib.FileExists(destinationPathname);
            if (doesFileExistAlready)
            {
                //CBL I should combine the logic below, for the case of Composite.

                if (rolloverModeToUse == RolloverMode.Size)
                {
                    // Ensure the log file does not exceed the size limit.
                    if (isToRolloverNow || (FilesystemLib.GetFileLength( destinationPathname ) + sRecord.Length > Config.MaxFileSize))
                    {
                        // We are going to exceed MaxFileSize.

                        // If we cannot do a file-rollover, then just truncate the existing file.
                        if (Config.MaxNumberOfFileRollovers == 0)
                        {
                            isToAppend = false;
                        }
                        else
                        {
                            try
                            {
                                Rollover( destinationDir, outputFilenameToUse );
                                // Doing the rollover means the file no longer has the same name.
                                doesFileExistAlready = false;
                            }
                            catch (Exception x)
                            {
                                isToAppend = false;
                                HandleInternalFault( x, logRecord, "Unable to Rollover file {0} .", destinationPathname );
                            }
                        }
                    }
                }
                else if (rolloverModeToUse == RolloverMode.Date && Config.FileOutputRollPoint != RollPoint.NoneSpecified)
                {
                    // eg
                    //   Log.log    -> rename to 2011_0626_Log.log
                    bool isToRollOverByWhen = false;
                    DateTime whenLastWritten;
                    if (isToRolloverNow)
                    {
                        isToRollOverByWhen = true;
                        whenLastWritten = FilesystemLib.GetFileLastWriteTime( destinationPathname );
                    }
                    else
                    {
                        whenLastWritten = FilesystemLib.GetFileLastWriteTime( destinationPathname );
                        var now = DateTime.Now;
                        switch (Config.FileOutputRollPoint)
                        {
                            case RollPoint.TopOfHour:
                                // Was it last written before the current hour?
                                if (whenLastWritten.Hour != now.Hour)
                                {
                                    isToRollOverByWhen = true;
                                }
                                break;
                            case RollPoint.TopOfDay:
                                // Was it last written before today?
                                if (whenLastWritten.DayOfYear != now.DayOfYear)
                                {
                                    isToRollOverByWhen = true;
                                }
                                break;
                            case RollPoint.TopOfWeek:
                                // Was it last written before this week?
                                var cultureInfo = CultureInfo.CurrentUICulture;
                                int weekWhenLastWritten = cultureInfo.Calendar.GetWeekOfYear(whenLastWritten, CalendarWeekRule.FirstFullWeek, DayOfWeek.Sunday);
                                int weekNow = cultureInfo.Calendar.GetWeekOfYear(now, CalendarWeekRule.FirstFullWeek, DayOfWeek.Sunday);
                                if (weekNow != weekWhenLastWritten)
                                {
                                    isToRollOverByWhen = true;
                                }
                                break;
                            case RollPoint.TopOfMonth:
                                // Was it last written before this month?
                                if (now.Month != whenLastWritten.Month)
                                {
                                    isToRollOverByWhen = true;
                                }
                                break;
                        }
                    }

                    if (isToRollOverByWhen)
                    {
                        // The existing file was last written yesterday or earlier, so roll it over into that day's log file
                        // and create a fresh one.
                        string filenameToRenameCurrentFileTo = FilenameForTimeRollBackup(destinationPathname, whenLastWritten);
                        try
                        {
                            FilesystemLib.MoveFile( destinationPathname, filenameToRenameCurrentFileTo );
                            // Doing the rollover means the file no longer has the same name.
                            doesFileExistAlready = false;
                        }
                        catch (Exception x)
                        {
                            HandleInternalFault( x, logRecord, "Unable to move file {0} to {1}.", destinationPathname, filenameToRenameCurrentFileTo );
                        }
                    }
                }
                else if (rolloverModeToUse == RolloverMode.Composite)
                {
                    //CBL THIS was for RolloverMode.Once, but I changed it to get it to compile. Need to implement this!
                    //CBL  Fixed, but needs testing. Also, what about MaxFileSize?

                    // Ensure the log file does not exceed the size limit.
                    if (isThisFirstFileOutput == true)
                    {
                        // If we cannot do a file-rollover, then just truncate the existing file.
                        if (Config.MaxNumberOfFileRollovers == 0)
                        {
                            isToAppend = false;
                        }
                        else
                        {
                            try
                            {
                                Rollover( destinationDir, outputFilenameToUse );
                                // Doing the rollover means the file no longer has the same name.
                                doesFileExistAlready = false;
                            }
                            catch (Exception x)
                            {
                                NutUtil.WriteToConsole( "Rollover raised an exception." );
                                // Treat the case where the file is locked by another process in a special way -- attempt to write instead to 
                                // another file in the same location.
                                if (x is Win32Exception && x.Message.Contains( "is being used by another process" ))
                                {
                                    string newFilename = outputFilenameToUse.PathnameWithoutExtension() + "_REDIRECTED." + outputFilenameToUse.FilenameExtensionWithoutPeriod();
                                    string newPath = Path.Combine(destinationDir, newFilename);
                                    if (FilesystemLib.FileExists( newPath ))
                                    {
                                        isToAppend = true;
                                    }
                                    else
                                    {
                                        isToAppend = false;
                                    }
                                    Config.FileOutputFilename = newFilename;
                                    RaiseLoggingFaultOccurred( new object(), null, "Attempted to Rollover - output file was locked so redirected output to alternate file.", x, logRecord, newFilename );
                                }
                                else
                                {
                                    isToAppend = false;
                                    HandleInternalFault( x, logRecord, "Unable to Rollover file {0} .", outputFilenameToUse );
                                }
                            }
                        }
                    }
                }
            }
            else // file does not already exist
            {
                isToAppend = false;
            }
            //CBL Hey! I don't see a branch to handle a RolloverMode of Composite!!

            bool isToWriteThrough = Config.IsFileOutputToWriteThrough;

            try
            {
                //CBL This was working fine, until I gave it an empty string for the file folder.
                //CBL 2018/7/4 If I set output to a flash-drive, then remove the flash-drive and do a log
                //    - this results in a Win32Exception here. Need to fix that.

                if (Config.IsToOpenCloseOutputFileEveryTime)
                {
                    FilesystemLib.WriteTextToFile( destinationPathname, sRecord, isToAppend: isToAppend, isToWriteThrough: isToWriteThrough );
                }
                else
                {
                    // For async, I'm trying out this method that does not involve opening/closing a file every time. It seems to be faster by an order of magnitude.
                    // But is it safe for unreliable USB-connected drives?  cbl
                    NutUtil.WriteTextToFileAndKeepOpen( pathname: destinationPathname, text: sRecord, doesFileExist: doesFileExistAlready, isToAppend: isToAppend );
                }
            }
            catch (Exception x1)
            {
                //CBL Temp.
                Debug.WriteLine( "Exception thrown in WriteTextToFile: " + x1.Message );

                // Treat the case where the file is locked by another process in a special way -- attempt to write instead to 
                // another file in the same location.
                if (x1 is Win32Exception && x1.Message.Contains( "is being used by another process" ))
                {
                    // Try it once more, after a slight delay.
                    bool wasSuccessful;
                    Thread.Sleep( 25 );
                    try
                    {
                        FilesystemLib.WriteTextToFile( destinationPathname, sRecord, isToAppend, isToWriteThrough );
                        wasSuccessful = true;
                        didLogGetWritten = true;
                    }
                    catch (Exception)
                    {
                        wasSuccessful = false;
                        didLogGetWritten = false;
                    }

                    if (wasSuccessful)
                    {
                        Debug.WriteLine( "But 2nd attempt to write to it succeeded." );
                    }
                    else
                    {
                        // Create a new pathname from the original, of the form:
                        //   If the original is LogNuts.log, then the result would be LogNuts_REDIRECTED.log
                        string newPath1 = destinationPathname.PathnameWithoutExtension() + "_REDIRECTED." + destinationPathname.FilenameExtensionWithoutPeriod();
                        try
                        {
                            // Just append in this case, for simplicity.
                            FilesystemLib.WriteTextToFile( pathname: newPath1, contents: sRecord, isToAppend: true, isToWriteThrough: false );
                            RaiseLoggingFaultOccurred( new object(), "Output file was locked so redirected to alternate file.", null, x1, logRecord, newPath1 );
                        }
                        catch (Exception x2)
                        {
                            // Create a new pathname from the original, of the form:
                            //   If the original is LogNuts.log, then this one would be LogNuts_REDIRECTED(1).log
                            string newPath2 = destinationPathname.PathnameWithoutExtension() + "_REDIRECTED(1)." + destinationPathname.FilenameExtensionWithoutPeriod();
                            try
                            {
                                // Just append in this case, for simplicity.
                                FilesystemLib.WriteTextToFile( pathname: newPath2, contents: sRecord, isToAppend: true, isToWriteThrough: false );
                                RaiseLoggingFaultOccurred( new object(), "Output file was locked so redirected to 2nd alternate file.", null, x2, logRecord, newPath2 );
                            }
                            catch (Exception x3)
                            {
                                HandleInternalFault( x3, logRecord, "Unable to neither write to log file {0} nor redirect to file {1} .", outputFilenameToUse, newPath2 );
                                didLogGetWritten = false;
                            }
                        }
                    }
                }
                else
                {
                    HandleInternalFault( x1, logRecord, "Unable to write to log file {0} .", destinationPathname );
                    didLogGetWritten = false;
                }
            }

            return didLogGetWritten;
        }
        #endregion OutputToFile

        #region SendOutputToConsole
        /// <summary>
        /// Send the log-output to the console (e.g. the Visual Studio Output-panel). Used only within method Send.
        /// </summary>
        /// <param name="logSendRequest">the log to send</param>
        /// <returns>true if this output is enabled and it did goet sent</returns>
        private static bool SendOutputToConsole( LogSendRequest logSendRequest )
        {
            // Output it to the Console, if we're doing that..
            //CBL Double-check this. Looks like redundant or unnecessary code.
            bool wasSent = false;
            if (!logSendRequest.IsToSuppressTraceOutput)
            {
                if (logSendRequest.IsConsoleOutputRequested)
                {
                    // I'm turning most of these meta-data options off,
                    // so that the console-window is not inundated with extraneous detail..

                    //CBL  Test to ensure the design-view visual indicator gets included.

                    string recordAsText = logSendRequest.Record.AsText(LogConfig.GetForConsoleOutput(Config));
                    NutUtil.WriteToConsole( recordAsText );

                    wasSent = true;
                }
            }
            return wasSent;
        }
        #endregion

        #region SendOutputToPipes
        /// <summary>
        /// Send the log-output to any pipes that are attached and enabled. Used only within method Send.
        /// </summary>
        /// <param name="logSendRequest">the log to send</param>
        /// <param name="didThisRecordGoOut">a running-flag that denotes whether this log-record has been sent out (to anything)</param>
        private static void SendOutputToPipes( LogSendRequest logSendRequest, ref bool didThisRecordGoOut )
        {
            // Attend to any IOutputPipes that are attached..
            if (_outputPipes != null)
            {
                foreach (IOutputPipe outputPipe in OutputPipes)
                {
                    if (outputPipe.IsEnabled)
                    {
                        try
                        {
                            bool wasSent = outputPipe.Write(logSendRequest);

                            didThisRecordGoOut |= wasSent;
                        }
                        catch (Exception x)
                        {
                            // Note: We probably do not want this error to prevent any other output-pipes fromw orking.
                            outputPipe.IsFailing = true;
                            HandleInternalFault( x, logSendRequest.Record, "Unable to log via pipe " + outputPipe.Name );
                        }
                    }
                }
            }
        }
        #endregion

        #endregion non-public methods

        #region fields

        private static LogConfig _config;

        /// <summary>
        /// This is the initial part of the text line that is written when the file is explicitly truncated.
        /// </summary>
        internal const string TruncationPrefix = "====[ Content Truncated on ";

        /// <summary>
        /// This is the last part of the text line that is written when the file is explicitly truncated.
        /// </summary>
        internal const string TruncationSuffix = " ]====";
#if !PRE_4
        private static CancellationToken _cancellationToken;
#endif
        #region the delimiters used for the std text-output

        /// <summary>
        /// Within the standard log-record prefix, this is the character that separates the host-name from the subject-program name.
        /// </summary>
        internal const Char DelimiterAfterHost = '/';

        /// <summary>
        /// Within the standard log-record prefix, this is the character that separates the subject-program-name from the subject-program version.
        /// </summary>
        internal const Char DelimiterAfterProgram = ':';

        /// <summary>
        /// Within the standard log-record prefix, this is the character that separates the version from the user-name.
        /// </summary>
        internal const Char DelimiterAfterVersion = ',';

        /// <summary>
        /// Within the standard log-record prefix, this is the character that separates the user-name from the thread-id.
        /// </summary>
        internal const Char DelimiterAfterUser = '|';

        /// <summary>
        /// Within the standard log-record prefix, this is the character that separates the thread-id from the logger-name.
        /// </summary>
        internal const Char DelimiterAfterThread = ';';

        /// <summary>
        /// Within the standard log-record prefix, this is the character that separates the logger-name from the level.
        /// </summary>
        internal const Char DelimiterAfterLoggerName = '\\';

        /// <summary>
        /// Within the standard log-record prefix, this indicates that a category-name follows.
        /// </summary>
        internal const Char DelimiterBeforeCategory = '{';

        /// <summary>
        /// Within the standard log-record prefix, this indicates that a category-name has ended (this comes after the end of the category-name).
        /// </summary>
        internal const Char DelimiterAfterCategory = '}';

        /// <summary>
        /// Within the standard log-record prefix, this is the character that separates the LogLevel from the design-view indicator.
        /// </summary>
        internal const Char DelimiterBeforeDesignViewIndicator = '*';

        /// <summary>
        /// This is the character that is inserted within the log-prefix to indicate the program-under-test is being executed
        /// in "design-mode", meaning in Visual Studio Cider or in Blend.
        /// </summary>
        internal const string VisualDesignerIndicator = "v";

        /// <summary>
        /// This is the character we use as the record separator for log record written to file. It is the paragraph symbol.
        /// I selected this because it needs to render within Windows Notepad, which fails to show most symbols.
        /// </summary>
        private const char _fileRecordSeparator = '\u00B6';

        #endregion the delimiters used for the std text-output

        /// <summary>
        /// This flag is cleared after the first log output to a file has occurred, on this invocation of the subject-program.
        /// </summary>
        private static bool _isFirstFileOutput = true;

#if INCLUDE_JSON
        /// <summary>
        /// This is the JsonSerializerSettings to use when serializing a log-record into JSON format, whenever that format is used.
        /// It is only instantiated when it is first needed.
        /// </summary>
        private static JsonSerializerSettings _jsonSerializerSettings;
#endif

        /// <summary>
        /// This dictates whether, when running under Microsoft's Visual Studio IDE, to use Debug output
        /// as opposed to Console output,
        /// whenever writing to the console-output window. The default is false (output using Console).
        /// </summary>
        private static bool _isUsingTraceForConsoleOutput;

        /// <summary>
        /// This object is defined simply for locking, for thread-safe access.
        /// </summary>
        private static readonly object _logFileWriteLockObject = new Object();

        /// <summary>
        /// For thread-safe access to the list of Logger objects
        /// </summary>
        private static readonly object _loggerListLockObject = new Object();

        /// <summary>
        /// This is the ILogRecordFormatter implementation that renders a log-record into text for output to a file or whatever.
        /// By default it is a DefaultLogRecordFormatter, but it may be set to a different ILogRecordFormatter implementation.
        /// </summary>
        private static ILogRecordFormatter _logRecordFormatter;

        /// <summary>
        /// The queue of log transmission requests that are waiting to be written to disk by the LogManager thread that does the actual work
        /// (when the log-servicing process is not being used).
        /// </summary>
#if !PRE_4
        internal static BlockingCollection<LogSendRequest> _queueOfLogRecordsToWrite;
#else
        internal static LockFreeQueue<LogSendRequest> _queueOfLogRecordsToWrite = new LockFreeQueue<LogSendRequest>();
#endif

#if !PRE_4
        /// <summary>
        /// The C# Task that is employed to transmit the log-record asynchronously.
        /// </summary>
        private static Task _logTransmissionTask;
#endif
        /// <summary>
        /// A member class-variable whose only purpose is to coordinate locking the queue for inter-task access.
        /// </summary>
        private static readonly object _queueLockObject = new Object();

        /// <summary>
        /// This ManualResetEvent serves to provide something to wait upon after sending a log-record,
        /// for tests and in case something cares to know when the log-queue has become empty.
        /// </summary>
        public static ManualResetEvent QueueOfLogsToWriteEmptiedEvent = new ManualResetEvent(true);

        /// <summary>
        /// This ManualResetEvent serves to provide something to wait upon after transmitting an IpcMessage to the log-servicing process.
        /// </summary>
        public static ManualResetEvent QueueOfMsgsToSvcEmptiedEvent = new ManualResetEvent(true);
        //CBL Would it be more efficient to make this ManualResetEvent a lazy-instantiated property?

        /// <summary>
        /// This is the point-of-reference that is used when displaying the elapsed-time within the prefix of a log-record output.
        /// </summary>
        public static DateTime ReferenceTime;

        internal static TestFacility _testFacility;

        /// <summary>
        /// The collection of logger instances that have been created of this LogManager.
        /// </summary>
        private static List<Logger> _theLoggers;

        /// <summary>
        /// This applies only when the RemovableDrivePreferredFileOutputFolder is specified (is non-null).
        /// When the drive is down and we divert file-output back to the fallback location, this text gets appended to the filename
        /// in order to identify it as such. This is "_DIVERTED".
        /// </summary>
        private const string SUFFIX_FOR_FILES_DIVERTED_FROM_REMOVABLE = "_DIVERTED";

        /// <summary>
        /// This is used for Logger.LogElapsedTime2.
        /// </summary>
        internal static Stopwatch _repeatingTimeReference;

        #endregion fields

        #endregion internal implementation
    }
}
