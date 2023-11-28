using System.Windows;
using Hurst.BaseLibWpf;


namespace VsDevTool.TestProject
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
            get { return "Test-Project"; }
        }
        #endregion

        #region VendorName
        /// <summary>
        /// Get the (short, one-word or acronym version of) name of the maker or owner of this software,
        /// as would be used for the first part of the CommonDataPath, and the window title-bar prefix.
        /// This is also used for the vendor folder-name for the settings-file, if used, under AppData\Local.
        /// This returns "DesignForge".
        /// </summary>
        public override string VendorName
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return "DesignForge";
            }
        }
        #endregion

    }
}
