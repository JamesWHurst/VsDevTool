using System;
using System.Configuration;
using System.Reflection;
#if NETFX_CORE
using System.Linq;
using System.Net;
using Windows.ApplicationModel;
#else
using System.Diagnostics;
#endif
using System.IO;
using Microsoft.Win32;


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// This is really just a "miscellaneous" grab-bag of methods. But "SystemLib" sounds cooler.
    /// </summary>
    public static class SystemLib
    {
        #region ComputerName
        /// <summary>
        /// Get the name of this computer (the local host) upon which this code is executing.
        /// </summary>
        public static string ComputerName
        {
            get
            {
#if !SILVERLIGHT
                if (_computerName == null)
                {
                    //TODO: With Silverlight, how to get the source-host name?

#if NETFX_CORE
                    _computerName = Environment.GetEnvironmentVariable( "COMPUTERNAME" );
#else
                    _computerName = Environment.MachineName;
#endif
                    // Alternatively, could use System.Net.Dns.GetHostName();
                }
#endif
                return _computerName;
            }
            set { _computerName = value; }
        }

        /// <summary>
        /// This is the computer-name.
        /// </summary>
        private static string _computerName;

        #endregion

        #region CurrentThreadId
        /// <summary>
        /// Get the (integer) ID of the current managed thread (that thread upon which *this* code is executing).
        /// </summary>
        /// <remarks>
        /// The purpose of this property is to provide a platform-agnostic way to acquire the thread id.
        /// </remarks>
        public static int CurrentThreadId
        {
            get
            {
#if NETFX_CORE
                return Environment.CurrentManagedThreadId;
#else
                return System.Threading.Thread.CurrentThread.ManagedThreadId;
#endif
            }
        }
        #endregion

        #region GetProgramName
#if NETFX_CORE
        /// <summary>
        /// Return the name of this executing program -- getting it from the given Type's assembly's title.
        /// </summary>
        /// <returns>The title (of the calling assembly) as a string</returns>
        /// <remarks>
        /// Note: This caches the retrieved name and uses that value on subsequent calls, regardless of the passed argument
        ///       because this uses reflection and is a comparatively heavy chore.
        ///       However, if you subsequently call it using a Type argument from a different assembly, you won't get the result
        ///       that you expect.
        /// 
        /// If the code is currently running under Visual Studio 2013's XAML-design process,
        /// the Entry-Assembly name will be "XDesProc". The program-name that is returned
        /// in this case, is "CiderDesigner".
        /// </remarks>
        public static string GetProgramName(Type typeFromApplication)
        {
            //CBL  I need to test this when in designer-mode.

            if (_programName == null)
            {
                // Get the assembly with Reflection:
                Assembly callingAssembly = typeFromApplication.GetTypeInfo().Assembly;

                // This would yield the assembly name, which is not what I want. 
                //string assemblyName = callingAssembly.GetName().Name;

                // Get the custom attribute informations:
                var titleAttribute = callingAssembly.CustomAttributes.Where(ca => ca.AttributeType == typeof( AssemblyTitleAttribute )).FirstOrDefault();
                // Now get the string value of the title attribute contained in the constructor:
                string assemblyTitle = titleAttribute.ConstructorArguments[0].Value.ToString();

                _programName = assemblyTitle;
            }
            return _programName;
        }
#else
        /// <summary>
        /// Return the name of this executing program, getting it from the entry assembly.
        /// </summary>
        /// <param name="anything">This parameter is unused.</param>
        /// <returns>The name (of the program or assembly) as a string</returns>
        /// <remarks>
        /// Note: This caches the retrieved name and uses that value on subsequent calls, regardless of the passed argument
        ///       because this uses reflection and is a comparatively heavy chore.
        ///       However, if you subsequently call it using a Type argument from a different assembly, you won't get the result
        ///       that you expect.
        /// 
        /// If the code is currently running under Visual Studio 2013's XAML-design process,
        /// the Entry-Assembly name will be "XDesProc". The program-name that is returned
        /// in this case, is "CiderDesigner".
        /// 
        /// If the code is currently running under Visual Studio 2015 in an MSTest unit-test,
        /// the program-name that is returned will be "UnitTestAdapter".
        /// 
        /// The <paramref name="anything"/> parameter is not used. It is provided simply to maintain compatibility
        /// with the Universal Windows Platform code.
        /// </remarks>
        public static string GetProgramName( object anything )
        {
            if (_programName == null)
            {
                string programName;
                var entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null)
                {
                    var assblyName = entryAssembly.GetName();
                    programName = assblyName.Name;
                    // With Visual Studio 2013, when executing within the XAML Designer the process is XDesProc.
                    // In that case - rename it to something more descriptive.
                    if (programName.Equals( "XDesProc" ))
                    {
                        programName = "CiderDesigner";
                    }
                }
                else
                {
                    // Attempt to get it for web applications.
                    var httpContext = System.Web.HttpContext.Current;
                    if (httpContext != null)
                    {
                        var assembly = httpContext.ApplicationInstance.GetType().BaseType.Assembly;
                        programName = assembly.GetName().Name;
                    }
                    else
                    {
                        // Try getting the calling assembly, which would work if this is a unit-test.
                        var callingAssembly = Assembly.GetCallingAssembly();
                        programName = callingAssembly.GetName().Name;
                        if (programName.Contains("LogNut"))
                        {
                            // Check for this specific case, of running within an MSTest unit test
                            // which under VS 2015 yielded UnitTestAdapter: Running test".
                            string currentDomainName = AppDomain.CurrentDomain.FriendlyName;
                            if (currentDomainName.StartsWith("UnitTestAdapter"))
                            {
                                programName = "UnitTestAdapter";
                            }
                        }
                    }
                }
                // Remove the prefix "Hurst." if it has it.
                if (programName.StartsWith( "Hurst." ))
                {
                    return programName.PartAfter( "Hurst." );
                }
                _programName = programName;
            }
            return _programName;
        }
#endif

        /// <summary>
        /// The computed name of the calling assembly is saved in this, so that it may be
        /// readily retrieved upon subsequent calls to GetProgramName.
        /// </summary>
        private static string _programName;

        #endregion GetProgramName

        #region GetProgramVersionTextFromAppConfig
        /// <summary>
        /// Return this application's "version", as a string formatted as: {JH.}YYYY.MMDD.HHMM
        /// where YYYY is year, MM is month, DD is day of the month, HHMM is the time in 24-hour format,
        /// and JH are initials assigned to whichever developer is compiling this application.
        /// </summary>
        /// <remarks>
        /// This gets the specially-formatted text that denotes this program's "Program-Version",
        /// as a string formatted as: {JH.}YYYY.MMDD.HHMM
        /// where YYYY = year, MM = month, DD = day of the month, HHMM = the time in 24-hour format,
        /// and JH are initials assigned to whichever developer is compiling this application.
        /// This is read from the app configuration file (as opposed to the Assembly and File versions
        /// that are available from the AssemblyInfo.cs file)
        /// which is App.Config in the source-code, and gets output as {program}.exe.config when compiled.
        /// </remarks>
        public static string GetProgramVersionTextFromAppConfig()
        {
            if (_programVersionText == null)
            {
                _programVersionText = System.Configuration.ConfigurationManager.AppSettings["ProgramVersion"];
                if (_programVersionText == null)
                {
                    _programVersionText = "(version not set)";
                }
            }
            return _programVersionText;
        }
        private static string _programVersionText;

        /// <summary>
        /// Return this application's "version", as a string formatted as: {BL.}YYYY.MMDD.HHMM
        /// where YYYY is year, MM is month, DD is day of the month, HHMM is the time in 24-hour format, and BL are initials assigned to whichever
        /// developer is compiling this application.
        /// </summary>
        /// <param name="isToIncludeBuilder">this indicates whether to include a prefix that indicates the user who built this particular executable.</param>
        /// <remarks>
        /// This gets the specially-formatted text that denotes this program's "Program-Version",
        /// as a string formatted as: {JH.}YYYY.MM.DD.HHMM
        /// where YYYY = year, MM = month, DD = day of the month, HHMM = the time in 24-hour format,
        /// and JH are initials assigned to whichever developer is compiling this application.
        /// This is read from the app configuration file (as opposed to the Assembly and File versions
        /// that are available from the AssemblyInfo.cs file)
        /// which is App.Config in the source-code, and gets output as {program}.exe.config when compiled.
        /// </remarks>
        public static string GetProgramVersionTextFromAppConfig( bool isToIncludeBuilder )
        {
            if (_programVersionText == null)
            {
                _programVersionText = ConfigurationManager.AppSettings["ProgramVersion"];
                if (_programVersionText == null)
                {
                    _programVersionText = "(version not set)";
                }
                else
                {
                    if (!isToIncludeBuilder)
                    {
                        // Strip off the builder-prefix.
                        if (StringLib.IsEnglishAlphabetLetter( _programVersionText[0] )
                            && StringLib.IsEnglishAlphabetLetter( _programVersionText[1] ))
                        {
                            _programVersionText = _programVersionText.PartAfter( "." );
                        }
                    }
                }
            }
            return _programVersionText;
        }
        #endregion GetProgramVersionTextFromAppConfig

        #region GetVersion
        /// <summary>
        /// Return the version of this executing program, getting the version from the executing assembly.
        /// </summary>
        /// <returns>The version (of the program or assembly) as a string</returns>
        /// <remarks>
        /// This applies to both .Net and Silverlight.
        /// </remarks>
        public static string GetVersion()
        {
            string programVersion;
#if NETFX_CORE
            programVersion = String.Format( "{0}.{1}.{2}.{3}",
                                            Package.Current.Id.Version.Major,
                                            Package.Current.Id.Version.Minor,
                                            Package.Current.Id.Version.Build,
                                            Package.Current.Id.Version.Revision );
#else
            //CBL This does not consistently work - it gets the version of the wrong assembly.
            //var executingAssembly1 = Assembly.GetExecutingAssembly();
            //var entryAssembly = Assembly.GetEntryAssembly();
            //var callingAssembly = Assembly.GetCallingAssembly();

            var assembly = Assembly.GetEntryAssembly();
#if SILVERLIGHT
            object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyFileVersionAttribute), false);
            if (attributes != null && attributes.Length > 0)
            {
                AssemblyFileVersionAttribute fileVersionAttribute = (AssemblyFileVersionAttribute)attributes[0];
                programVersion = fileVersionAttribute.Version;
            }
            else
            {
                programVersion = String.Empty;
            }
#else
            //CBL First see whether the application is IVersionable !
            // console programs
            // windows forms
            // WPF
            //
            if (assembly == null)
            {
                // Attempt to get it for web applications.
                var httpContext = System.Web.HttpContext.Current;
                if (httpContext != null)
                {
                    assembly = httpContext.ApplicationInstance.GetType().BaseType.Assembly;
                }
                else
                {
                    // Try getting the calling assembly, which would work if this is a unit-test.
                    assembly = Assembly.GetCallingAssembly();
                }
            }
            programVersion = assembly.GetName().Version.ToString();
#endif
#endif
            return programVersion;
        }
        #endregion GetVersion

        #region HasMainWindow
#if !NETFX_CORE
        /// <summary>
        /// Return true if the current process has a main-window.
        /// This can be used to distinguish, for example, when a program is being invoked
        /// from a command-line, as opposed to run within a window.
        /// </summary>
        /// <returns></returns>
        public static bool HasMainWindow
        {
            get
            {
                return (Process.GetCurrentProcess().MainWindowHandle != IntPtr.Zero);
            }
        }
#endif
        #endregion

        #region Is64BitProcess
#if !NETFX_CORE
        /// <summary>
        /// Return true if this is running as a 64-bit process.
        /// </summary>
        public static bool Is64BitProcess
        {
            get
            {
                // Here's a more involved solution: https://code.msdn.microsoft.com/windowsapps/CSCheckOSBitness-e579f3ef
                // See also https://code.msdn.microsoft.com/windowsapps/Sample-to-demonstrate-how-495e69db
#if (PRE_4 || PRE_5)
                return IntPtr.Size == 8;
#else
                return Environment.Is64BitProcess;
#endif
            }
        }
#endif
        #endregion

        #region IsRunningFromIde
        /// <summary>
        /// Get whether this program is currently running out of the bin\Debug or bin\Release directories that would commonly be the case when using Microsoft Visual Studio.
        /// </summary>
        public static bool IsRunningFromIde
        {
            get
            {
                if (!_isRunningFromIde.HasValue)
                {
                    string directoryOfThisExecutable = FilesystemLib.GetProgramExecutionDirectory();
                    _isRunningFromIde = (directoryOfThisExecutable.Contains( @"bin\Debug" ) || directoryOfThisExecutable.Contains( @"bin\Release" ));
                }
                return _isRunningFromIde.Value;
            }
        }

        private static bool? _isRunningFromIde;

        #endregion

        #region IsThisAWebApplication
        /// <summary>
        /// Get whether this code is currently running as a Web Application
        /// (by testing whether System.Web.HttpContext.Current is not null).
        /// </summary>
        /// <remarks>
        /// This property only exists in order to provide a platform-agnostic way
        /// to check for whether this code is running as a web-server, as opposed to 
        /// a Desktop or Universal Windows Platform (UWP) application.
        /// </remarks>
        public static bool IsThisAWebApplication
        {
            get
            {
#if NETFX_CORE
//CBL Need to implement this!
                return false;
#else
                var httpContext = System.Web.HttpContext.Current;
                return (httpContext != null);
#endif
            }
        }
        #endregion

        #region IsWindows10
        /// <summary>
        /// Get whether this code is currently executing on the Windows 10 operating-system.
        /// </summary>
        /// <remarks>
        /// This simply checks <code>Environment.OSVersion</code> and returns <code>true</code>
        /// if <code>Version.Major</code> is 6 and <code>Version.Minor</code> is 2.
        /// </remarks>
        public static bool IsWindows10
        {
            get
            {
                // On Win10 Platform is "Win32NT", Version is "6.2.9200.0", Major = 6, Minor = 2
                // On Win7  Platform is "Win32NT", Version is "6.1.7601.65536", Major = 6, Minor = 1
                // Logger.LogDebug( "OSVersion.Platform = {0}, OSVersion.Version = {1}, Version.Major = {2}, MajorRevision = {3}, Minor = {4}, MinorRevision = {5}", os.Platform, os.Version, os.Version.Major, os.Version.MajorRevision, os.Version.Minor, os.Version.MinorRevision );
                // Logger.LogDebug( "SystemLib.IsWindows10 = {0}", SystemLib.IsWindows10 );
                var os = Environment.OSVersion;
                return (os.Version.Major == 6 && os.Version.Minor == 2);
            }
        }
        #endregion

        #region LaunchProgram
        /// <summary>
        /// Invoke the program that is located at the given pathname.
        /// </summary>
        /// <param name="pathnameOfApplication">the full pathname of the executable to run</param>
        /// <param name="argument">any arguments to provide to the program (may be null)</param>
        /// <param name="currentDir">the directory to set as it's 'current directory' (may be null)</param>
        /// <param name="isToCheckForOtherInstance">if true - checks for an existing instance of the program and does not launch a new one if found</param>
        public static void LaunchProgram( string pathnameOfApplication, string argument = null, string currentDir = null, bool isToCheckForOtherInstance = true )
        {
            bool isToLaunchIt = true;

            if (isToCheckForOtherInstance)
            {
                string procName = System.IO.Path.GetFileNameWithoutExtension( pathnameOfApplication );
                Process[] procList = Process.GetProcessesByName( procName );

                if (procList.Length > 0)
                {
                    isToLaunchIt = false;
                    Debug.WriteLine( string.Format( "Found an instance of {0}.", pathnameOfApplication ) );
                    Debug.WriteLine( string.Format( "Number of Processes: {0} = {1}.", procName, procList.Length ) );
                }
            }

            if (isToLaunchIt)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo( pathnameOfApplication );
                if (!string.IsNullOrEmpty( argument ))
                {
                    startInfo.Arguments = argument;
                }
                if (!string.IsNullOrEmpty( currentDir ))
                {
                    startInfo.WorkingDirectory = currentDir;
                }
                startInfo.Verb = "runas";
                Process.Start( startInfo );
            }
        }
        #endregion

        #region Username
        /// <summary>
        /// Get the string that denotes the username of the user who is currently logged in to the operating system
        /// that is running this program.
        /// </summary>
        public static string Username
        {
            get
            {
#if NETFX_CORE
                //CBL  None of this shit works yet!

                //return Windows.System.UserProfile.UserInformation;
                //var username = await Windows.System.UserProfile.UserInformation.GetDomainNameAsync();

                //var creds = CredentialCache.DefaultNetworkCredentials;
                var username = CredentialCache.DefaultNetworkCredentials.UserName;

                //CBL THIS is the code that I had within Configuration.Username

                //var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
                //if (identity != null)
                //{
                //    string identityName = identity.Name;
                //    // Remove any domain-part that is within this string, which uses a back-slash separator.
                //    int positionOfSlash = identityName.IndexOf( @"\" );
                //    if (positionOfSlash > -1)
                //    {
                //        _userName = identityName.Substring( positionOfSlash + 1 );
                //    }
                //    else
                //    {
                //        _userName = identityName;
                //    }
                //}
                //else // identity is null
                //{
                //    _userName = Environment.UserName;
                //}
                //string username = Windows.System.User.
                return username;
#else
                return Environment.UserName;
#endif
            }
        }
        #endregion
    }
}
