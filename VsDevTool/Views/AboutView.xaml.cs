using System;
using System.Windows;
using Hurst.BaseLibWpf;
using UiBaseLib;
using VsDevTool.ViewModels;


namespace VsDevTool.Views
{
    /// <summary>
    /// Interaction logic for AboutView.xaml
    /// </summary>
    public partial class AboutView : Window
    {
        public AboutView()
        {
            InitializeComponent();
            _viewModel = ApplicationViewModel.The;
            LayoutUpdated += OnLayoutUpdated;
            Loaded += OnLoaded;
        }

        #region OnLayoutUpdated
        /// <summary>
        /// Handle the LayoutUpdated event by setting the position of this window on the user's desktop to be just to the right (where possible) of the main-window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLayoutUpdated( object sender, EventArgs e )
        {
            if (!_isAlreadyPositioned)
            {
                // Try to align it along the right side of the parent window when possible.
                this.AlignToParent( AlignmentType.ToRightOfParent );
                _isAlreadyPositioned = true;
            }
        }
        #endregion

        private void OnLoaded( object sender, RoutedEventArgs e )
        {
            //Title = App.The.ProductIdentificationPrefix + ":  About this";
            txtVersion.Text = "Version " + App.The.ProgramVersionText;
        }

        private void OnClick_OkButton( object sender, RoutedEventArgs e )
        {
            this.Close();
        }

        #region fields

        private bool _isAlreadyPositioned;
        private readonly ApplicationViewModel _viewModel;

        #endregion fields
    }
}
