using System;
using System.Diagnostics;
using System.Windows.Forms;
using Hurst.LogNut.Util;
using Hurst.BaseLibWpf.Display;


namespace Hurst.BaseLibWpf.DialogWindows
{
    /// <summary>
    /// This class comprises a dialog-window designed expressly for
    /// providing a UX to the user for specifying where to save a file to.
    /// </summary>
    public sealed class FileSaveDialog : IDisposable
    {
        #region default constructor
        /// <summary>
        /// Default constructor
        /// </summary>
        public FileSaveDialog()
        {
            m_FileDialog = new System.Windows.Forms.SaveFileDialog();
            m_FileDialog.CheckFileExists = false;
            m_FileDialog.AddExtension = true;
            m_FileDialog.AutoUpgradeEnabled = true;
        }
        #endregion

        #region InitialDirectory
        /// <summary>
        /// Get or set the initial folder displayed by the dialog.
        /// </summary>
        public string InitialDirectory
        {
            get { return m_FileDialog.InitialDirectory; }
            set { m_FileDialog.InitialDirectory = value; }
        }
        #endregion

        #region Title
        /// <summary>
        /// Get or set the open-file dialogbox title
        /// </summary>
        public string Title
        {
            get { return m_FileDialog.Title; }
            set { m_FileDialog.Title = value; }
        }
        #endregion

        #region SelectedFilename
        /// <summary>
        /// Gets/sets the filename selected by the user.
        /// </summary>
        public string SelectedFilename
        {
            get { return m_FileDialog.FileName; }
            set { m_FileDialog.FileName = value; }
        }
        #endregion

        #region AddExtensionIfOmitted
        /// <summary>
        /// Get or set the value that determines whether the dialog automatically adds an extension to the filename if the user omits it.
        /// </summary>
        public bool AddExtensionIfOmitted
        {
            get { return m_FileDialog.AddExtension; }
            set { m_FileDialog.AddExtension = value; }
        }
        #endregion

        #region Filter
        /// <summary>
        /// Gets/sets the current filename filter string,
        /// which determines the choices that appear in the "Save as file type" box in the dialog.
        /// </summary>
        public string Filter
        {
            get { return m_FileDialog.Filter; }
            set { m_FileDialog.Filter = value; }
        }
        #endregion

        #region ShowOverwritePrompt
        /// <summary>
        /// Gets/sets whether to display a warning if the user specifies a filename that already exists.
        /// </summary>
        public bool ShowOverwritePrompt
        {
            get { return m_FileDialog.OverwritePrompt; }
            set { m_FileDialog.OverwritePrompt = value; }
        }
        #endregion

        #region UseMainWindowAsOwner

        private bool m_bUseMainWindowAsOwner;

        public bool UseMainWindowAsOwner
        {
            get { return m_bUseMainWindowAsOwner; }
            set { m_bUseMainWindowAsOwner = value; }
        }
        #endregion

        #region ShowDialog
        /// <summary>
        /// Invokes a 'common dialog box' with a default owner-window.
        /// </summary>
        /// <returns>a TaskDialogResult that maps exactly what a Forms.FileDialog would return</returns>
        public DisplayUxResult ShowDialog()
        {
            System.Windows.Forms.DialogResult dr;
            //cbl  I don't see that this distinction makes any difference at all.
            if (UseMainWindowAsOwner)
            {
                IWin32Window win32dow = GetMainWindowAsIWin32Window();
                dr = m_FileDialog.ShowDialog(win32dow);
            }
            else
            {
                dr = m_FileDialog.ShowDialog();
            }
            return DisplayBox.ResultFrom(dr);
        }
        #endregion

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #region Internal Implementation

        #region GetMainWindowAsIWin32Window
        /// <summary>
        /// Retrieves the main window for the current process and returns it as a IWin32Window
        /// </summary>
        /// <returns></returns>
        public IWin32Window GetMainWindowAsIWin32Window()
        {
            string sFriendlyName = AppDomain.CurrentDomain.FriendlyName;
            // Get process collection by the application name without extension (.exe)
            Process[] pro = Process.GetProcessesByName(sFriendlyName.Substring(0, sFriendlyName.LastIndexOf('.')));
            // Get main window handle pointer and wrap into IWin32Window
            return new WindowWrapper(pro[0].MainWindowHandle);
        }
        #endregion

        private void Dispose(bool isDisposingManagedResources)
        {
            if (isDisposingManagedResources)
            {
                if (m_FileDialog != null)
                {
                    m_FileDialog.Dispose();
                }
            }
        }

        /// <summary>
        /// This embedded SaveFileDialog is used to perform all of the actual functionality.
        /// </summary>
        private System.Windows.Forms.SaveFileDialog m_FileDialog;
        //private System.Windows.Forms.FileDialog m_FileDialog;

        #endregion // Internal Implementation
    }
}
