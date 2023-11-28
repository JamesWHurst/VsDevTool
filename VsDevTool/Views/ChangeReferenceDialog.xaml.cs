using System;
using System.Windows;
using Hurst.BaseLibWpf;
using UiBaseLib;
using VsDevTool.ViewModels;


namespace VsDevTool.Views
{
    /// <summary>
    /// Interaction logic for ChangeReferenceDialog.xaml
    /// </summary>
    public partial class ChangeReferenceDialog : Window
    {
        public ChangeReferenceDialog()
        {
            InitializeComponent();
            _viewModel = ApplicationViewModel.The;
            _viewModel.SelectAssemblyToReferenceRequested += OnSelectAssemblyToReferenceRequested;
            LayoutUpdated += OnLayoutUpdated;
            ContentRendered += OnContentRendered;
        }

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

        private void OnContentRendered( object sender, EventArgs e )
        {
            _viewModel.LoadReferencesAccordingToSelectedProjects();
        }

        private void OnSelectAssemblyToReferenceRequested( object sender, EventArgs e )
        {
            var fileSelector = new Microsoft.Win32.OpenFileDialog();
            fileSelector.Multiselect = false;
            fileSelector.DefaultExt = "sln";
            fileSelector.Title = "Select the assembly to reference..";
            var r = fileSelector.ShowDialog();

            if (r == true)
            {
                string selectedPath = fileSelector.FileName;
                _viewModel.ReferenceToChangeTo = selectedPath;
            }
        }

        private void OnClick_CloseButton( object sender, RoutedEventArgs e )
        {
            this.Close();
        }

        #region fields

        private bool _isAlreadyPositioned;
        private readonly ApplicationViewModel _viewModel;

        #endregion fields
    }
}
