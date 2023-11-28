using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Hurst.BaseLibWpf.Display;
using Hurst.BaseLibWpf;
using Hurst.LogNut.Util;


namespace VsDevTool
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : DesktopApplication
    {
        #region The
        /// <summary>
        /// Get the App object that is our application
        /// </summary>
        public new static App The
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return Application.Current as App; }
        }
        #endregion

        #region ProductName
        /// <summary>
        /// Get the name of this application for display within titlebars and the like,
        /// which in this case is "VS-Dev-Tool".
        /// </summary>
        public override string ProductName
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return "VS-Dev-Tool"; }
        }
        #endregion

        #region VendorName
        /// <summary>
        /// Get the (short, one-word version of) name of the maker or owner of this software,
        /// as would be used for the first part of the CommonDataPath, and the window title-bar prefix.
        /// This is "DesignForge" for this application.
        /// </summary>
        public override string VendorName
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return "GP";
            }
        }
        #endregion

        #region ConfigureDisplayBox
        /// <summary>
        /// Set the options to use by default for subsequent invocations of a display-box.
        /// </summary>
        protected override void ConfigureDisplayBox()
        {
            DisplayBox.SetDefaultConfiguration(
                new DisplayBoxDefaultConfiguration()
                                 //.SetToUseNewIcons( false )
                                 .SetDisplayBoxWidth( 500 )
                                 //.SetBackgroundTexture( DisplayBoxBackgroundTexturePreset.BrushedMetal )
                                 .SetDefaultTimeoutFor( DisplayBoxType.UserMistake, 15 )
                                 .SetDefaultTimeoutFor( DisplayBoxType.Information, 4 )
                                 .SetToUseHurstButtonStyles( true )
                                 .SetToBeTopmostWindowByDefault( true )
                                 .SetToUseNewSounds( true ) );
        }
        #endregion

        #region OnStartup
        /// <summary>
        /// Override the OnStartup method to setting some shit.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnStartup( StartupEventArgs e )
        {
            // Ensure there is only one instance of this application running at any one time.
            _singleInstanceEvent = new EventWaitHandle( false, EventResetMode.AutoReset, _eventName, out bool wasCreated );
            if (!wasCreated)
            {
                // If the event-handle was not newly created, then that means another instance of this program is already running.
                _singleInstanceEvent.Set();
                Shutdown();
            }
            else // this is the first instance of this program.
            {
                // Create a new Task that waits for that event to be signalled (meaning a second instance of this program tried to launch)
                // and, when that happens, make *this* instance become active and visible to the user.
                SynchronizationContext ctx = SynchronizationContext.Current;
                Task.Factory.StartNew( () =>
                {
                    while (true)
                    {
                        _singleInstanceEvent.WaitOne();
                        ctx.Post( _ => MakeActiveApplication(), null );
                    }
                } );
            }

            // Configure the logging.
            //CBL           LogManager.Config.SubjectProgramVersion = Util.GetVersion();
#if !DEBUG
            //LogManager.Config.IsSuppressingExceptions = true;
#endif
            base.OnStartup( e );
        }
        #endregion

        private void MakeActiveApplication()
        {
            MainWindow.Activate();
            // These next 2 statements are to bring this program-window to the foreground,
            // without making it always insist upon staying in the foreground (which would be annoying).
            MainWindow.Topmost = true;
            MainWindow.Topmost = false;
            MainWindow.Focus();
        }

        private const string _eventName = "VsDevTool";
        private EventWaitHandle _singleInstanceEvent;
    }
}
