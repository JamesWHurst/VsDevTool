using System;
using System.Windows.Forms;
using Hurst.LogNut.Util;
using Hurst.BaseLibWpf.Display;


namespace Hurst.BaseLibWpf.DialogWindows
{
    /// <summary>
    /// This simply encapsulates the Windows.Forms.FolderBrowserDialog,
    /// and (in this incarnation) actually contains one.
    /// Later this will be replaced with a WPF version so that the Forms dll won't need to be loaded.
    /// </summary>
    public sealed class FolderSelectionDialog : IDisposable
    {
        #region constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public FolderSelectionDialog()
        {
            this.IsMainWindowTheOwner = true;
        }
        #endregion

        #region public properties

        #region Description
        /// <summary>
        /// Gets or sets the descriptive text displayed above the tree view control in the window.
        /// </summary>
        public string Description { get; set; }

        #endregion

        #region InitialDirectory
        /// <summary>
        /// Get or set the initial folder displayed by the dialog box.
        /// </summary>
        public string InitialDirectory { get; set; }

        #endregion

        #region IsMainWindowTheOwner
        /// <summary>
        /// Get or set the flag that indicates whether we use the application's main window
        /// for the 'parent' window for this dialog.
        /// </summary>
        private bool IsMainWindowTheOwner { get; set; }

        #endregion

        #region IsToShowNewFolderButton
        /// <summary>
        /// Gets or sets a flag that dictates whether the New Folder button is displayed.
        /// </summary>
        public bool IsToShowNewFolderButton { get; set; }

        #endregion

        #region ParentWindow
        /// <summary>
        /// Get or set the Window that is to serve as the 'parent', in the desktop-stacking sense.
        /// </summary>
        public System.Windows.Window ParentWindow { get; set; }

        #endregion

        #region SelectedPath
        /// <summary>
        /// Get the filesystem path that the user selected within the dialog.
        /// </summary>
        public string SelectedPath
        {
            get
            {
                if (_folderBrowserDialog != null)
                {
                    return _folderBrowserDialog.SelectedPath;
                }
                else
                {
                    return "";
                }
            }
        }
        #endregion

        #endregion public properties

        #region ShowDialog
        /// <summary>
        /// Invokes a 'common dialog box' with a default owner-window.
        /// </summary>
        /// <returns>a TaskDialogResult that maps exactly what a Forms.FolderBrowserDialog would return</returns>
        public DisplayUxResult ShowDialog()
        {
            System.Windows.Forms.DialogResult dialogResult;
            if (_folderBrowserDialog == null)
            {
                _folderBrowserDialog = new FolderBrowserDialog();
                if (!String.IsNullOrEmpty( this.Description ))
                {
                    _folderBrowserDialog.Description = this.Description;
                }
                if (!String.IsNullOrEmpty( this.InitialDirectory ))
                {
                    _folderBrowserDialog.SelectedPath = this.InitialDirectory;
                }
                _folderBrowserDialog.RootFolder = Environment.SpecialFolder.MyComputer;
                _folderBrowserDialog.ShowNewFolderButton = this.IsToShowNewFolderButton;
            }
            dialogResult = _folderBrowserDialog.ShowDialog();
            return DisplayBox.ResultFrom( dialogResult );
        }
        #endregion

        public void Dispose()
        {
            Dispose( true );
            GC.SuppressFinalize( this );
        }

        #region internal implementation

        private void Dispose( bool isDisposingManagedResources )
        {
            if (isDisposingManagedResources)
            {
                if (_folderBrowserDialog != null)
                {
                    _folderBrowserDialog.Dispose();
                }
            }
        }

        /// <summary>
        /// This embedded FolderBrowserDialog is used to perform all of the actual functionality.
        /// </summary>
        private System.Windows.Forms.FolderBrowserDialog _folderBrowserDialog;

        #endregion internal implementation
    }
}
