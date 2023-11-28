using System;
using System.ComponentModel;
using System.Windows;
using Hurst.BaseLibWpf;
using Hurst.BaseLibWpf.DialogWindows;
using Hurst.LogNut.Util;
using UiBaseLib;
using VsDevTool.ViewModels;


namespace VsDevTool.Views
{
    /// <summary>
    /// Interaction logic for OptionsDialog.xaml
    /// </summary>
    public partial class OptionsDialog : Window
    {
        #region constructor
        /// <summary>
        /// Create a new OptionsDialog window.
        /// </summary>
        public OptionsDialog()
        {
            InitializeComponent();

            _viewModel = ApplicationViewModel.The;
            _viewModel.SelectRootFolderForHistoryRequested += OnSelectRootFolderForHistoryRequested;
            LayoutUpdated += OnLayoutUpdated;
        }
        #endregion

        #region OnLayoutUpdated
        /// <summary>
        /// Handle the LayoutUpdated event by setting the position of this application on the user's desktop.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLayoutUpdated( object sender, EventArgs e )
        {
            if (!_isAlreadyPositioned)
            {
                // Try to align it along the left side of the parent window.
                this.AlignToParent( AlignmentType.UnderParent );
                _isAlreadyPositioned = true;
            }
        }
        #endregion

        private void OnSelectRootFolderForHistoryRequested( object sender, EventArgs e )
        {
            var folderSelector = new FolderSelectionDialog();
            folderSelector.InitialDirectory = _viewModel.DefaultRootFolderForVersionStateSnapshots;
            folderSelector.IsToShowNewFolderButton = true;
            folderSelector.Description = "Select the folder to save the application version-state history in.";
            folderSelector.ParentWindow = this;
            var r = folderSelector.ShowDialog();

            if (r == DisplayUxResult.Ok)
            {
                _viewModel.DefaultRootFolderForVersionStateSnapshots = folderSelector.SelectedPath;
            }

            //fileSelector.Multiselect = false;
            //fileSelector.DefaultExt = "sln";
            //fileSelector.Title = "Select the solution-file..";
            //var r = fileSelector.ShowDialog();

            //if (r == true)
            //{
            //    string selectedPath = fileSelector.FileName;
            //    _viewModel.VsSolutionFilePathname = selectedPath;
            //}
        }

        #region OnClick_CloseButton
        /// <summary>
        /// Handle the Click-event of the close-button by closing this options-dialog-window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClick_CloseButton( object sender, RoutedEventArgs e )
        {
            this.Close();
        }
        #endregion

        #region OnClosing
        /// <summary>
        /// Override the OnClosing method, to save the view-model before raising the <see cref="E:System.Windows.Window.Closing"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs"/> that contains the event data.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            _viewModel.Save();
            base.OnClosing(e);
        }
        #endregion

        #region fields

        private bool _isAlreadyPositioned;
        private readonly ApplicationViewModel _viewModel;

        #endregion fields
    }
}
