using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;
using Hurst.BaseLibWpf;
using Hurst.BaseLibWpf.DialogWindows;
using Hurst.LogNut;
using Hurst.LogNut.Util;
using VsDevTool.DomainModels;
using VsDevTool.ViewModels;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;


namespace VsDevTool.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            _viewModel = ApplicationViewModel.The;

            _viewModel.AddResourceFileToSetsRequested += OnAddResourceFileToSetsRequested;
            _viewModel.AddSolutionToApplicationRequested += OnAddSolutionToApplicationRequested;
            _viewModel.ChangeReferenceRequested += OnChangeReferenceRequested;
            _viewModel.EditOptionsRequested += OnEditOptionsRequested;
            _viewModel.OpenLastReportRequested += OnOpenLastReportRequested;
            _viewModel.SaveApplicationStructureRequested += OnSaveApplicationStructureRequested;
            _viewModel.SelectMetricsComparsionSourceFolder1Requested += OnSelectMetricsComparsionSourceFolder1Requested;
            _viewModel.SelectMetricsComparsionSourceFolder2Requested += OnSelectMetricsComparsionSourceFolder2Requested;
            _viewModel.SelectMetricsComparisonReportDestinationPathRequested += OnSelectMetricsComparisonReportDestinationPathRequested;
            _viewModel.SelectProjectRequested += OnSelectProjectRequested;
            _viewModel.SelectSolutionRequested += OnSelectSolutionRequested;
            _viewModel.SelectOutputPathRequested += OnSelectOutputPathRequested;
            _viewModel.SelectCopySourceDestinationPathRequested += OnSelectCopySourceDestinationPathRequested;
            ApplicationViewModel.SetUxToDefaultsRequested += OnSetUxToDefaultsRequested;
            _viewModel.UserNotificationRequested += OnUserNotificationRequested;
            _viewModel.SelectSpreadsheetFileRequested += OnSelectSpreadsheetFileRequested;


            LayoutUpdated += OnLayoutUpdated;
            ContentRendered += OnContentRendered;
            Closing += OnClosing;
        }

        #region Logger
        /// <summary>
        /// Get the logger for this class to use.
        /// </summary>
        public static Logger Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = LogManager.GetCurrentClassLogger();
                }
                return _logger;
            }
            set { _logger = value; }
        }
        private static Logger _logger;
        #endregion

        #region UserSettings
        /// <summary>
        /// Get the singleton-instance of the user-set configuration-settings for this program.
        /// </summary>
        public VsDevToolUserSettings UserSettings
        {
            get { return VsDevToolUserSettings.The; }
        }
        #endregion

        private void OnSelectSpreadsheetFileRequested( object sender, EventArgs e )
        {
            var fileSelector = new Microsoft.Win32.OpenFileDialog();
            fileSelector.Multiselect = false;
            fileSelector.DefaultExt = "xlsx";
            fileSelector.Title = "Select the Excel spreadsheet file..";
            fileSelector.Filter = "XLSX files (*.xlsx)|*.xlsx";
            var r = fileSelector.ShowDialog();

            if (r == true)
            {
                string newPathname = fileSelector.FileName;
                _viewModel.SpreadsheetFilePathname = newPathname;
                _viewModel.Save();
            }
        }

        private void OnAddResourceFileToSetsRequested( object sender, EventArgs e )
        {
            var fileSelector = new Microsoft.Win32.OpenFileDialog();
            fileSelector.Multiselect = false;
            fileSelector.DefaultExt = "resx";
            fileSelector.Title = "Select the resources file..";
            fileSelector.Filter = "RESX files (*.resx)|*.resx";
            var r = fileSelector.ShowDialog();

            if (r == true)
            {
                string selectedPath = fileSelector.FileName;
                var newResourceFile = new VsResourceFile(visualStudioProject: null, pathname: selectedPath);
                _viewModel.ResourceFiles.Add(newResourceFile);
                _viewModel.Save();
            }
        }

        private void OnAddSolutionToApplicationRequested( object sender, EventArgs e )
        {
            var fileSelector = new Microsoft.Win32.OpenFileDialog();
            fileSelector.Multiselect = false;
            fileSelector.DefaultExt = "sln";
            fileSelector.Title = "Select the solution-file to add to this application..";
            var r = fileSelector.ShowDialog();

            if (r == true)
            {
                string selectedPath = fileSelector.FileName;
                Debug.WriteLine($"selectedPath={StringLib.AsQuotedString(selectedPath)}");
                _viewModel.ApplicationSolutions.Add(new VsSolution(selectedPath));
            }
            else
            {
                Debug.WriteLine("User cancelled operation.");
            }
        }

        private void OnChangeReferenceRequested( object sender, EventArgs e )
        {
            var changeReferenceDialog = new ChangeReferenceDialog();
            changeReferenceDialog.Owner = this;
            var r = changeReferenceDialog.ShowDialog();
        }

        #region OnContentRendered
        /// <summary>
        /// Handle the ContentRendered event by initializing the drive-detector timer.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnContentRendered( object sender, EventArgs e )
        {
            _dispatcherTimerDriveDetector = new DispatcherTimer();
            _dispatcherTimerDriveDetector.Interval = TimeSpan.FromSeconds(1);
            _dispatcherTimerDriveDetector.Tick += OnTick_TimerDriveDetector;
            _dispatcherTimerDriveDetector.Start();
        }
        #endregion

        #region OnEditOptionsRequested
        /// <summary>
        /// Handle the EditOptionsRequested event by bringing up the Options dialog-window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnEditOptionsRequested( object sender, EventArgs e )
        {
            var editOptionsWindow = new OptionsDialog();
            editOptionsWindow.Owner = this;
            var r = editOptionsWindow.ShowDialog();
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
            // Remember the original window extent, in case the user later wants to reset it back to the original designed values.
            if (!_hasOriginalExtentBeenSaved)
            {
                _windowHeightOriginal = this.Height;
                _windowWidthOriginal = this.Width;
                _hasOriginalExtentBeenSaved = true;
            }
            UserSettings.SetWindowToSavedPosition(this);
        }
        #endregion

        #region OnSetUxToDefaultsRequested
        /// <summary>
        /// Handle the SetUxToDefaultsRequested event by setting the window to it's original designed extent.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSetUxToDefaultsRequested( object sender, EventArgs e )
        {
            this.Height = _windowHeightOriginal;
            this.Width = _windowWidthOriginal;
        }
        #endregion

        private void OnOpenLastReportRequested( object sender, EventArgs e )
        {
            FilesystemLib.OpenNotepadWithFile(_viewModel.ModuleDependencyReportPath);
        }

        private void OnSaveApplicationStructureRequested( object sender, EventArgs e )
        {
            // First validate what the user has thus far entered.
            switch (_viewModel.Scope)
            {
                case ApplicationAnalysisScope.ApplicationScope:
                    string name = _viewModel.ApplicationName;
                    if (String.IsNullOrWhiteSpace(name))
                    {
                        App.The.Interlocution.WarnUser("You need to set an Application name first.");
                        return;
                    }
                    if (_viewModel.ApplicationSolutions.Count == 0)
                    {
                        App.The.Interlocution.WarnUser("You need to set add at least one Visual Studio solution to the Application first.");
                        return;
                    }
                    break;
                case ApplicationAnalysisScope.SolutionScope:
                    if (String.IsNullOrWhiteSpace(_viewModel.VsSolutionFilePathname))
                    {
                        App.The.Interlocution.WarnUser("You need to enter the pathname to the Visual Studio solution first.");
                        return;
                    }
                    break;
                case ApplicationAnalysisScope.ProjectScope:
                    if (String.IsNullOrWhiteSpace(_viewModel.VsProjectFilePathname))
                    {
                        App.The.Interlocution.WarnUser("You need to enter the pathname to the Visual Studio project first.");
                        return;
                    }
                    break;
            }

            //CBL Get this value from our setting
            string documentsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string pathname = Path.Combine(documentsDirectory, "TestProject1.xml");
            _viewModel.SaveCurrentApplicationGraphAs(pathname);

            //var fileSelector = new Microsoft.Win32.SaveFileDialog();
            //fileSelector.DefaultExt = "xml";
            //fileSelector.OverwritePrompt = false;
            //fileSelector.Filter = "XML files (*.xml)|*.xml";
            //fileSelector.Title = "Here you indicate what file to save this Application-Graph to..";
            //var r = fileSelector.ShowDialog();

            //if (r == true)
            //{
            //    string selectedPath = fileSelector.FileName;
            //    _viewModel.SaveCurrentApplicationGraphAs( selectedPath );
            //}
        }

        private void OnSelectMetricsComparsionSourceFolder1Requested( object sender, EventArgs e )
        {
            var folderSelector = new FolderSelectionDialog();
            folderSelector.Description = "Select the root-folder for source-version 1 to compare the metrics of.";
            folderSelector.IsToShowNewFolderButton = false;
            folderSelector.InitialDirectory = _viewModel.MetricsComparisonSourceFolder1;
            var r = folderSelector.ShowDialog();
            if (r == DisplayUxResult.Ok)
            {
                _viewModel.MetricsComparisonSourceFolder1 = folderSelector.SelectedPath;
            }
        }

        private void OnSelectMetricsComparsionSourceFolder2Requested( object sender, EventArgs e )
        {
            var folderSelector = new FolderSelectionDialog();
            folderSelector.Description = "Select the root-folder for source-version 2 to compare the metrics of.";
            folderSelector.IsToShowNewFolderButton = false;
            folderSelector.InitialDirectory = _viewModel.MetricsComparisonSourceFolder2;
            var r = folderSelector.ShowDialog();
            if (r == DisplayUxResult.Ok)
            {
                _viewModel.MetricsComparisonSourceFolder2 = folderSelector.SelectedPath;
            }
        }

        private void OnSelectMetricsComparisonReportDestinationPathRequested( object sender, EventArgs e )
        {
            var pathnameSelector = new FileSaveDialog();
            pathnameSelector.Title = "Specify the pathname to write the metrics-comparison report to.";
            pathnameSelector.InitialDirectory = _viewModel.MetricsComparisonReportDestinationPath;
            if (StringLib.HasNothing(_viewModel.MetricsComparisonReportDestinationPath))
            {
                pathnameSelector.SelectedFilename = "CodeMetricsComparisonReport.txt";
            }
            var r = pathnameSelector.ShowDialog();
            if (r == DisplayUxResult.Ok)
            {
                _viewModel.MetricsComparisonReportDestinationPath = pathnameSelector.SelectedFilename;
            }
        }

        private void OnSelectCopySourceDestinationPathRequested( object sender, EventArgs e )
        {
            //var dirSelector = new Microsoft.Win32.SaveFileDialog();
            using (var dirSelector = new FolderBrowserDialog())
            {

                if (StringLib.HasSomething(_viewModel.CopySourceDestinationPath))
                {
                    dirSelector.SelectedPath = _viewModel.CopySourceDestinationPath;
                }
                dirSelector.Description = "Select the folder to copy to.";
                DialogResult response = dirSelector.ShowDialog();
                if (response == System.Windows.Forms.DialogResult.OK)
                {
                    string dir = dirSelector.SelectedPath;
                    _viewModel.CopySourceDestinationPath = dir;
                }
            }

            //var folderSelector = new FolderSelectionDialog();
            //folderSelector.Description = "Select the folder to copy your source-files to.";
            //folderSelector.IsToShowNewFolderButton = true;
            //var r = folderSelector.ShowDialog();
            //if (r == DisplayUxResult.Ok)
            //{
            //    _viewModel.CopySourceDestinationPath = folderSelector.SelectedPath;
            //}
        }

        private void OnSelectProjectRequested( object sender, EventArgs e )
        {
            var fileSelector = new Microsoft.Win32.OpenFileDialog();
            fileSelector.Multiselect = false;
            fileSelector.DefaultExt = "sln";
            fileSelector.Title = "Select the project-file..";
            var r = fileSelector.ShowDialog();

            if (r == true)
            {
                string selectedPath = fileSelector.FileName;
                _viewModel.VsProjectFilePathname = selectedPath;
                _viewModel.Save();
            }
        }

        /// <summary>
        /// In Panel 2, the user has selected the "Select" button.
        /// Respond by loading Panel 3 with the projects within the selected VS-solution.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectSolutionRequested( object sender, EventArgs e )
        {
            Debug.WriteLine($"OnSelectSolutionRequested, SelectedSolution={this._viewModel.SelectedSolution}");
            //_viewModel.SelectedSolution
            _viewModel.LoadProjects();

            //var fileSelector = new OpenFileDialog();
            //fileSelector.Multiselect = false;
            //fileSelector.DefaultExt = "sln";
            //fileSelector.Title = "Select the solution-file..";
            //var r = fileSelector.ShowDialog();

            //if (r == true)
            //{
            //    string selectedPath = fileSelector.FileName;
            //    _viewModel.VsSolutionFilePathname = selectedPath;
            //}

            //var dialog = new FileSelector();
            //dialog.ParentElement = this;
            //dialog.Description = "Select the Visual Studio solution.";
            //var r = dialog.ShowDialog();
            //if (r == DisplayUxResult.Ok)
            //{
            //    string selectedPath = dialog.SelectedPath;
            //    _viewModel.VsSolutionFilePathname = selectedPath;
            //}
        }

        private void OnSelectOutputPathRequested( object sender, EventArgs e )
        {
            var fileSelector = new OpenFileDialog();
            fileSelector.Multiselect = false;
            fileSelector.DefaultExt = "sln";
            fileSelector.Title = "Select the pathname to write your report to.";
            var r = fileSelector.ShowDialog();

            if (r == true)
            {
                string selectedPath = fileSelector.FileName;
                _viewModel.ModuleDependencyReportPath = selectedPath;
            }

            //var dialog = new FileSelector();
            //dialog.ParentElement = this;
            //dialog.Description = "Select the pathname to write your report to.";
            //var r = dialog.ShowDialog();
            //if (r == DisplayUxResult.Ok)
            //{
            //    string selectedPath = dialog.SelectedPath;
            //    _viewModel.ModuleDependencyReportPath = selectedPath;
            //}
        }

        private void OnTick_TimerDriveDetector( object sender, EventArgs e )
        {
            //Console.WriteLine( "OnTick_TimerDriveDetector" );
            if (_isFirstTick)
            {
                _dispatcherTimerDriveDetector.Interval = TimeSpan.FromSeconds(10);
                _isFirstTick = false;
            }
            bool wasInstallerDriveFound = false;
            DriveInfo[] thumbDrives = ApplicationViewModel.GetRemovableDrives();
            if (thumbDrives.Length > 0)
            {
                for (int i = 0; i < thumbDrives.Length; i++)
                {
                    DriveInfo drive = thumbDrives[i];
                    string volumeLabel = drive.VolumeLabel;
                    string rootDir = thumbDrives[0].RootDirectory.FullName;
                    if (StringLib.HasSomething(volumeLabel))
                    {
                        if (volumeLabel.Contains("Installer"))
                        {
                            //App.TheLogger.LogTrace( "Removable drive at {0} labelled {1} found.", rootDir, volumeLabel );
                            _viewModel.InstallerDrivePath = rootDir;
                            wasInstallerDriveFound = true;
                            break;
                        }
                        else
                        {
                            if (!_hasComplainedAboutThumbdrive)
                            {
                                Logger.LogTrace("Removable drive {0} deteced but the volume-lable does not contain the text Installer.", drive.VolumeLabel);
                                _hasComplainedAboutThumbdrive = true;
                            }
                        }
                    }
                    else
                    {
                        if (!_hasComplainedAboutThumbdrive)
                        {
                            Logger.Warn("Removable drive {0} detected that has no volume-label.", rootDir);
                            _hasComplainedAboutThumbdrive = true;
                        }
                    }
                }
            }
            if (!wasInstallerDriveFound)
            {
                _viewModel.InstallerDrivePath = "";
            }
            _viewModel.IsInstallerDriveDetected = wasInstallerDriveFound;
        }

        private void OnUserNotificationRequested( object sender, UserNotificationEventArgs e )
        {
            if (e.IsUserMistake)
            {
                Logger.LogWarning(e.MessageToUser);
                App.The.Interlocution.NotifyUserOfMistake(e.MessageToUser);
            }
            else if (e.IsError)
            {
                Logger.LogError(e.MessageToUser);
                App.The.Interlocution.NotifyUserOfError(e.MessageToUser);
            }
            else if (e.IsWarning)
            {
                App.The.Interlocution.WarnUser(e.MessageToUser);
            }
            else
            {
                App.The.Interlocution.NotifyUser(e.MessageToUser);
            }
        }

        #region OnClosing
        /// <summary>
        /// Handle the Closing event of the main-window
        /// by saving any information that the operator has entered.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClosing( object sender, System.ComponentModel.CancelEventArgs e )
        {
            _viewModel?.Save();
            UserSettings.SavePositionOf(this);
            //UserSettings.SaveWindowPositionIfChanged( this );
            UserSettings.SaveIfChanged();
        }
        #endregion

        #region fields

        /// <summary>
        /// This DispatcherTimer is for polling to see whether an Installer-drive has been plugged-in.
        /// </summary>
        private DispatcherTimer _dispatcherTimerDriveDetector;

        private bool _hasComplainedAboutThumbdrive;
        /// <summary>
        /// This flag serves to cause the LayoutUpdated event - which gets raised every time the window is moved - to set the saved extent only once.
        /// </summary>
        private bool _hasOriginalExtentBeenSaved;

        private bool _isFirstTick = true;
        private string _previousInstallerDrive;
        private readonly ApplicationViewModel _viewModel;

        /// <summary>
        /// This serves to hold the original design height of the main-window, in case the user wants to reset it back to this.
        /// </summary>
        private double _windowHeightOriginal;

        /// <summary>
        /// This serves to hold the original design width of the main-window, in case the user wants to reset it back to this.
        /// </summary>
        private double _windowWidthOriginal;

        #endregion fields
    }
}
