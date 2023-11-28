using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using Hurst.LogNut;
using Hurst.LogNut.Util;
using Hurst.BaseLibWpf.Display;


namespace Hurst.BaseLibWpf
{
    /// <summary>
    /// class DesktopApplication provides a few convenient facilities that are commonly needed for desktop WPF applications.
    /// It implements IDesktopApplication, and IVersionable.
    /// </summary>
    public abstract class DesktopApplication : Application, IDesktopApplication, IVersionable
    {
        #region The
        /// <summary>
        /// Get a reference to the <see cref="DesktopApplication"/> object - the parent class of our application. Same as the 'Current' method.
        /// </summary>
        public static DesktopApplication The
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return Application.Current as DesktopApplication; }
        }
        #endregion

        #region public properties

        #region Current
        /// <summary>
        /// Get a reference to the DesktopApplication object - the parent class of our application. Same as the 'The' method.
        /// </summary>
        public static new DesktopApplication Current
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return (DesktopApplication)Application.Current; }
        }
        #endregion

        #region Interlocution
        /// <summary>
        /// Get the object that will be used as the application-wide basis for user-notification.
        /// </summary>
        public virtual IInterlocution Interlocution
        {
            get
            {
                // Lock the object so we won't be competing with other threads at this moment..
                lock (_syncRoot)
                {
                    if (_interlocution == null)
                    {
                        _interlocution = DisplayBox.GetNewInstance(this);
                        ConfigureDisplayBox();
                    }
                    return _interlocution;
                }
            }
        }
        private DisplayBox _interlocution;

        /// <summary>
        /// syncronization root object, for locking
        /// </summary>
        private static readonly object _syncRoot = new object();

        #endregion

        #region IsInDesignMode
        /// <summary>
        /// This indicates whether this code is being executed in the context of Visual Studio "Cider" (the visual designer) or Blend,
        /// as opposed to running normally as an application.
        /// </summary>
        public bool IsInDesignMode
        {
            get
            {
                //return Hurst.BaseLibWpf.DesignerProperties.IsInDesignMode;
                return ViewModel.IsInDesignModeStatic;
            }
        }
        #endregion

        #region Logger
        /// <summary>
        /// Get or set the <see cref="ISimpleLogger"/> that will be used by this application, by default.
        /// </summary>
        public ISimpleLogger Logger
        {
            //CBL I may want to consider making this not-static,
            // and using dependency-injection to introduce the logger implementation.
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                //if (_logger == null)
                //{
                //    _logger = LogManager.GetLogger();
                //}
                return _logger;
            }
            set { _logger = value; }
        }

        private ISimpleLogger _logger;

        #endregion

        #region ProductName
        /// <summary>
        /// Provide the name of this application as it would be displayed to the user in window titlebars, etc.
        /// Unless overridden, this simply yields the assembly name.
        /// Note: You should override this if you don't want reflection to be used to get this value.
        /// </summary>
        public virtual string ProductName
        {
            get
            {
                // Since the name hasn't been explicitly set anywhere (or this method overridden), get it from the assembly.
                if (_applicationName == null)
                {
#if SILVERLIGHT
                    Application thisApp = Application.Current;
                    string className = thisApp.ToString();
                    _applicationName = StringLib.WithoutAtEnd(className, ".App");
#else
                    // This is the implementation I found in a demo-app for TX Text Control.
                    //CBL But, for MusicTrainer, it's returning BaseLibWPF !
                    //_applicationName = ((AssemblyProductAttribute)Attribute.GetCustomAttribute( Assembly.GetExecutingAssembly(), typeof( AssemblyProductAttribute ) )).Product;

                    // This was my own, earlier implementation.
                    Assembly assembly = Application.Current.GetType().Assembly;
                    _applicationName = assembly.GetName().Name;

                    // If it contains a period, assume the first part is the namespace and cut that off.
                    if (_applicationName.Contains("."))
                    {
                        _applicationName = _applicationName.PartAfter(".");
                    }
#endif
                }
                return _applicationName;
            }
        }
        private string _applicationName;

        #endregion

        #region ProductNameShort
        /// <summary>
        /// Provide the abbreviated name of this application for display in titlebars, etc.
        /// Unless overridden, this yields the same as the "ProductName" property.
        /// </summary>
        public virtual string ProductNameShort
        {
            get
            {
                return this.ProductName;
            }
        }
        #endregion

        #region ProductIdentificationPrefix
        /// <summary>
        /// Get the string to use to identify this software product to the user,
        /// for use - for example - as a prefix in the titlebar for all windows and dialogs of this application,
        /// or for Windows Event Log entries.
        /// This implements the mandated standard which is the vendor followed by the application's name.
        /// To set the titlebar you would append a separator (such as a colon and a space) before your specific title information.
        /// </summary>
        public virtual string ProductIdentificationPrefix
        {
            get
            {
                return this.VendorName + " " + this.ProductNameShort;
            }
        }
        #endregion

        #region ProgramName
        /// <summary>
        /// Return the name of the currently-executing program. If this is an Application that implements IDesktopApplication,
        /// then use it's ProductNameShort function, otherwise get it from the assembly. This applies to .Net or Silverlight.
        /// </summary>
        /// <returns>The program (assembly)-name as a string, or String.Empty if unable to determine it</returns>
        public string ProgramName
        {
            get
            {
                // We test for null, not for an empty string - so that this test is only done once.
                if (_programName == null)
                {
                    // If the applic implements IDesktopApplication, then use that to get a (short) name for the application.
                    IDesktopApplication iApp = Application.Current as IDesktopApplication;
                    if (iApp != null)
                    {
                        _programName = iApp.ProductNameShort;
                    }
                    else
                    {
                        // This is the next-best method that I know of, to get the name of the running application.
#if SILVERLIGHT
                    Application thisApp = Application.Current;
                    string className = thisApp.ToString();
                    _programName = WithoutAtEnd(className, ".App");
#else
                        Assembly thisAssembly = Assembly.GetEntryAssembly();
                        if (thisAssembly != null)
                        {
                            AssemblyName thisAssemblyName = thisAssembly.GetName();
                            _programName = thisAssemblyName.Name;
                        }
                        else // perhaps this is a unit-test that is running?
                        {
                            string friendlyName = AppDomain.CurrentDomain.FriendlyName;
                            int iColon = friendlyName.IndexOf(':');
                            if (iColon >= 0)
                            {
                                string result = friendlyName.Substring(iColon + 1);
                                if (StringLib.HasSomething(result))
                                {
                                    _programName = result.Trim();
                                }
                            }
                        }
#endif
                    }
                }
                if (_programName == null)
                {
                    _programName = String.Empty;
                }
                return _programName;
            }
            set
            {
                _programName = value;
            }
        }

        /// <summary>
        /// The name (reasonably shortened) of the currently executing program.
        /// </summary>
        private string _programName;

        #endregion ProgramName

        #region ThisApplicationExecutablePath
        /// <summary>
        /// Get the pathname of this application's executable.
        /// </summary>
        public string ThisApplicationExecutablePath
        {
            get
            {
                if (String.IsNullOrEmpty(_sExecutablePath))
                {
                    Assembly assembly = Application.Current.GetType().Assembly;
                    _sExecutablePath = assembly.Location;
                }
                return _sExecutablePath;
            }
        }

        private string _sExecutablePath;

        #endregion

        #region ThisApplicationCurrentUserDataFolder
        /// <summary>
        /// Get the filesystem-path for storing this application's data - that is specific to the current, non-roaming user.
        /// This path value contains the vendor-name and this product-name.
        /// With Windows 8.1, this is C:\Users\{username}\AppData\Local\{vendor-name}\{product-name}\
        /// </summary>
        public virtual string ThisApplicationCurrentUserDataFolder
        {
            get
            {
                if (String.IsNullOrEmpty(_thisApplicationCurrentUserDataFolder))
                {
                    string partUnderSpecialFolder = Path.Combine(this.VendorName, this.ProductName);
                    _thisApplicationCurrentUserDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), partUnderSpecialFolder);
                }
                return _thisApplicationCurrentUserDataFolder;
            }
        }

        private string _thisApplicationCurrentUserDataFolder;

        #endregion

        #region ThisApplicationLocalMachineDataFolder
        /// <summary>
        /// Get the filesystem-path for storing this application's data, such as would be common across local users of this desktop computer.
        /// This path value contains the vendor-name and this product-name.
        /// </summary>
        public virtual string ThisApplicationLocalMachineDataFolder
        {
            get
            {
                if (String.IsNullOrEmpty(_thisApplicationLocalMachineDataFolder))
                {
                    string partUnderSpecialFolder = Path.Combine(this.VendorName, this.ProductName);
                    _thisApplicationLocalMachineDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), partUnderSpecialFolder);
                }
                return _thisApplicationLocalMachineDataFolder;
            }
        }

        private string _thisApplicationLocalMachineDataFolder;

        #endregion

        #region ThisVendorCurrentUserDataFolder
        /// <summary>
        /// Get the filesystem-path for storing data for all applications of this vendor - that is specific to the current, non-roaming user.
        /// This path value contains the vendor-name, but not this product-name.
        /// </summary>
        public virtual string ThisVendorCurrentUserDataFolder
        {
            get
            {
                if (String.IsNullOrEmpty(_thisVendorCurrentUserDataFolder))
                {
                    string partUnderSpecialFolder = this.VendorName;
                    _thisVendorCurrentUserDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), partUnderSpecialFolder);
                }
                return _thisVendorCurrentUserDataFolder;
            }
        }

        private string _thisVendorCurrentUserDataFolder;

        #endregion

        #region Username
        /// <summary>
        /// Get the name of the user who is currently running this program.
        /// For Silverlight, you need to override this to provide an actual name - otherwise it simply returns a question-mark.
        /// </summary>
        public virtual string Username
        {
            get
            {
#if SILVERLIGHT
                return "?";
#else
                return Environment.UserName;
#endif
            }
            set
            {
                Console.WriteLine("Username.Set called. Why?");
            }
        }
        #endregion

        #region VendorName
        /// <summary>
        /// Get the (short, one-word or acronym version of) name of the maker or owner of this software,
        /// as would be used for the first part of the CommonDataPath, and the window title-bar prefix.
        /// This is also used for the vendor folder-name for the settings-file, if used, under AppData\Local.
        /// This returns "VendorName" if you fail to override it.
        /// </summary>
        public virtual string VendorName
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                Debug.WriteLine("Alert: You need to override the VendorName method.");
                return "VendorName";
            }
        }
        #endregion

        #region ProgramVersion
        /// <summary>
        /// Return the version of this executing program, by getting it from the executing assembly.
        /// This applies to both .Net and Silverlight.
        /// </summary>
        /// <returns>The version (of the program or assembly) as a string</returns>
        //public virtual string ProgramVersion
        //{
        //    get
        //    {
        //        if (_programVersion == null)
        //        {
        //            //_programVersion = FilesystemLib.GetVersion();

        //            //CBL  This duplicates code from SystemLib.GetProgramVersionTextFromAppConfig
        //            _programVersion = ConfigurationManager.AppSettings["ProgramVersion"];
        //            if (_programVersion == null)
        //            {
        //                _programVersion = "(not set)";
        //            }
        //            return _programVersion;
        //        }
        //        return _programVersion;
        //    }
        //}

        #endregion ProgramVersion

        #region ProgramVersionText
        /// <summary>
        /// Get the program-version from the App.Config file, as a string.
        /// Alert: this will not return the correct value until the program's main-window exists.
        /// </summary>
        public virtual string ProgramVersionText
        {
            get
            {
                return ProgramVersionTextFromAppConfig;
            }
        }
        #endregion

        #region ProgramVersionTextFromAppConfig
        /// <summary>
        /// Get the specially-formatted text that denotes this program's "Program-Version",
        /// as a string formatted as: {BL.}YYYY.MM.DD.HHMM
        /// where YYYY = year, MM = month, DD = day of the month, HHMM = the time in 24-hour format,
        /// and "BL" are initials intended to indicate whichever developer or build-box is compiling this application.
        /// </summary>
        /// <remarks>
        /// This is read from the app configuration file (as opposed to the Assembly and File versions
        /// that are available from the AssemblyInfo.cs file)
        /// which is App.Config in the source-code, and gets output as {program}.exe.config when compiled.
        /// </remarks>
        public string ProgramVersionTextFromAppConfig
        {
            get
            {
                if (_versionText == null)
                {
                    _versionText = SystemLib.GetProgramVersionTextFromAppConfig();
                }
                return _versionText;
            }
        }
        #endregion

        #endregion public properties

        #region CommandLineArguments
        /// <summary>
        /// Get the string-array containing the command-line arguments that were passed to this program upon invocation,
        /// if any. This gets assigned during the <see cref="OnStartup"/> method-call.
        /// </summary>
        public string[] CommandLineArguments { get; private set; }

        #endregion

        #region OnStartup
        /// <summary>
        /// Override the Application.OnStartup method to set handlers for unhandled exceptions.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup(StartupEventArgs e)
        {
            this.CommandLineArguments = e.Args;
            Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException_CurrentDomain);
            base.OnStartup(e);
        }
        #endregion

        #region ConfigureDisplayBox
        /// <summary>
        /// Overload this method in your subclass if you want to use DisplayBox as your IInterlocation,
        /// and you want to set some options on DisplayBox.
        /// </summary>
        protected virtual void ConfigureDisplayBox()
        {
            DisplayBox.SetDefaultConfiguration(
                new DisplayBoxDefaultConfiguration()
                    .SetBackgroundTexture(DisplayBoxBackgroundTexturePreset.BrushedMetal)
                    .SetDefaultTimeoutFor(DisplayBoxType.UserMistake, 15)
                    .SetDefaultTimeoutFor(DisplayBoxType.Information, 4)
                    .SetToUseHurstButtonStyles(true)
                    .SetToBeTopmostWindowByDefault(true)
                    .SetToUseNewSounds(true));
        }
        #endregion

        #region GetProgramVersionTextFromAppConfig
        /// <summary>
        /// Return the specially-formatted text that denotes this program's "Program-Version",
        /// as a string formatted as: {BL.}YYYY.MM.DD.HHMM
        /// where YYYY = year, MM = month, DD = day of the month, HHMM = the time in 24-hour format,
        /// and "BL" are initials intended to indicate whichever developer or build-box is compiling this application.
        /// </summary>
        /// <param name="isInDesignMode">this indicates whether this is being called from a program while in design mode (optional: default is false).</param>
        /// <remarks>
        /// This gets the specially-formatted text that denotes this program's "Program-Version",
        /// as a string formatted as: {JH.}YYYY.MM.DD.HHMM
        /// where YYYY = year, MM = month, DD = day of the month, HHMM = the time in 24-hour format,
        /// and JH are initials assigned to whichever developer is compiling this application.
        /// This is read from the app configuration file (as opposed to the Assembly and File versions
        /// that are available from the AssemblyInfo.cs file)
        /// which is App.Config in the source-code, and gets output as {program}.exe.config when compiled.
        /// </remarks>
        public string GetProgramVersionTextFromAppConfig()
        {
            //CBL Do I really want these duplicates of this functionality?
            if (_versionText == null)
            {
                _versionText = SystemLib.GetProgramVersionTextFromAppConfig();
            }
            return _versionText;
        }
        #endregion

        #region NotifyOfError
        /// <summary>
        /// Inform the end-user of the situation, and also log it.
        /// </summary>
        /// <param name="message">the text to show to the user</param>
        public virtual void NotifyOfError(string message)
        {
            // Log this..
            ILoggingProvider applicationThatCanLog = Application.Current as ILoggingProvider;
            if (applicationThatCanLog != null)
            {
                var logger = applicationThatCanLog.Logger;
                logger.LogError(message);
            }
            Interlocution.NotifyUserOfError(message);
        }
        #endregion

        #region NotifyOfException
        /// <summary>
        /// Handle the given Exception. This combines logging it, and showing it to the user in a display-box.
        /// </summary>
        /// <param name="exception">an Exception to log and to notify the user of</param>
        /// <param name="informationForDeveloper">(optional) any additional information to describe the context of this error</param>
        /// <param name="messageToUser">(optional) the message to show to the user if you don't want "It seems there is an issue"</param>
        /// <remarks>
        /// This method will also log the exception, if the implementing class also implements ILoggableApplication.
        /// </remarks>
        public static void NotifyOfException(Exception exception, string informationForDeveloper = null, string messageToUser = null)
        {
            DesktopApplication applicationInstance = Application.Current as DesktopApplication;
            if (applicationInstance != null)
            {
                if (exception == null)
                {
                    throw new ArgumentNullException(nameof(exception));
                }

                // Log this..
                ILoggingProvider applicationThatCanLog = Application.Current as ILoggingProvider;
                if (applicationThatCanLog != null)
                {
                    var logger = applicationThatCanLog.Logger;
                    string msgToLog = String.Format("Message To User: {0};  To Developer: {1}", informationForDeveloper, messageToUser);
                    logger.LogError(exception, msgToLog);
                }

                // Show it to the user..
                string msg;
                if (String.IsNullOrWhiteSpace(informationForDeveloper))
                {
                    msg = String.Format("An exception has been raised: \"{0}\"\r\n{1}", exception.Message, exception.ToString());
                }
                else
                {
                    msg = String.Format("An exception has been raised: \"{0}\"\r\n{1}\r\n{2}", exception.Message, informationForDeveloper,
                        exception.ToString());
                }
                string toUser;
                if (messageToUser == null)
                {
                    toUser = "It seems there is an issue.";
                }
                else
                {
                    toUser = messageToUser;
                }
                applicationInstance.Interlocution.NotifyUserOfError(toUser: toUser, toDeveloper: msg, exception: exception);
            }
        }
        #endregion

        #region OnDispatcherUnhandledException
        /// <summary>
        /// Handle the DispatcherUnhandledException event by both logging it and notifying the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            // This next line is to (hopefully!) prevent repeated message-boxes when this is called repeatedly.
            //CBL However, we should probably log this.
            if (_numberOfTimesHasHandledDispatcherUnhandledException < 3)
            {
                _numberOfTimesHasHandledDispatcherUnhandledException++;
                // Set this so that the application does not crash at this point.
                e.Handled = true;
                string note = "This was handled within OnDispatcherUnhandledException.";
                ILoggingProvider applicationThatCanLog = Application.Current as ILoggingProvider;
                if (applicationThatCanLog != null)
                {
                    var logger = applicationThatCanLog.Logger;
                    logger.LogError(e.Exception, note);
                }
                string userMessage = "This application experienced an issue with an unhandled exception: " + e.Exception.Message;
                string caption = this.ProductNameShort + ": Uh Oh";
                //CBL  Unfortunately, in the BeMs application the DisplayBox library is crashing.  2015-10-5
                //Interlocution.NotifyUserOfError(exception: e.Exception, toUser: userMessage, toDeveloper: note);
                MessageBox.Show(messageBoxText: userMessage, caption: caption, icon: MessageBoxImage.Error, button: MessageBoxButton.OK);
            }
        }

        #endregion

        #region OnUnhandledException_CurrentDomain
        /// <summary>
        /// Handle the UnhandledException event for the current AppDomain, by both logging the error and notifying the user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <remarks>
        /// Usage within your WPF Application:
        /// 
        /// Within OnStartup..
        /// 
        /// AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(OnUnhandledException_CurrentDomain);
        /// 
        /// </remarks>
        protected virtual void OnUnhandledException_CurrentDomain(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;
            string developerMessage = String.Format("Handled exception {0} within OnUnhandledException_CurrentDomain.", StringLib.AsString(exception));

            // Log it..
            ILoggingProvider applicationThatCanLog = Application.Current as ILoggingProvider;
            ILognutLogger logger = null;
            bool hasBeenLogged = false;
            if (applicationThatCanLog != null)
            {
                logger = applicationThatCanLog.Logger;
                if (exception == null)
                {
                    logger.LogError(developerMessage);
                }
                else
                {
                    logger.LogError(exception, developerMessage);
                }
                hasBeenLogged = true;
            }

            // Notify the user..
            string userMessage;
            if (exception == null)
            {
                if (hasBeenLogged)
                {
                    userMessage = "Handled and logged exception";
                }
                else
                {
                    userMessage = "Handled exception";
                }
            }
            else
            {
                userMessage = String.Format("Handled exception: {0}", exception.Message);
            }
            Interlocution.NotifyUserOfError(exception: exception, toUser: userMessage, toDeveloper: developerMessage, captionAfterPrefix: "Handling exception");

            // If termination is indicated, force a clean shutdown of this program..
            if (e.IsTerminating)
            {
                // Note: this can still be hooked by the developer to provide last-moment saving of information.
                logger?.LogError(exception, developerMessage);
                //CBL Do we really want to terminate here? And why did I have a Windows Forms call?
                //System.Windows.Forms.Application.Exit();
                Application.Current.MainWindow.Close();
            }
        }
        #endregion

        /// <summary>
        /// This tracks the number of times that an unhandled exception has been handled for the WPF Dispatcher.
        /// </summary>
        private int _numberOfTimesHasHandledDispatcherUnhandledException;

        /// <summary>
        /// This stores the version-number (as a string) of the subject-program.
        /// </summary>
        private string _versionText;
    }
}
