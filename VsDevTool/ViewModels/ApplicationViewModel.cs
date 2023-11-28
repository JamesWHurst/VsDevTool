using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Hurst.BaseLibWpf;
using Hurst.LogNut;
using Hurst.LogNut.Util;
using Hurst.XamlDevLib;
using VsDevTool.DomainModels;
using VsDevTool.Views;



namespace VsDevTool.ViewModels
{
    public class ApplicationViewModel : ViewModel
    {
        /// <summary>
        /// This event signals that the user has requested to
        /// select a resources file and add that to the list of resource-sets contained within the ListView
        /// on the Globalization tab.
        /// </summary>
        public event EventHandler<EventArgs> AddResourceFileToSetsRequested;

        public event EventHandler<EventArgs> AddSolutionToApplicationRequested;
        public event EventHandler<EventArgs> ChangeReferenceRequested;
        public event EventHandler<EventArgs> EditOptionsRequested;
        public event EventHandler<EventArgs> OpenLastReportRequested;
        public event EventHandler<EventArgs> SaveApplicationStructureRequested;
        public event EventHandler<EventArgs> SelectCopySourceDestinationPathRequested;
        public event EventHandler<EventArgs> SelectOutputPathRequested;
        public event EventHandler<EventArgs> SelectProjectRequested;
        public event EventHandler<EventArgs> SelectAssemblyToReferenceRequested;
        public event EventHandler<EventArgs> SelectRootFolderForHistoryRequested;

        public event EventHandler<EventArgs> SelectMetricsComparsionSourceFolder1Requested;
        public event EventHandler<EventArgs> SelectMetricsComparsionSourceFolder2Requested;
        public event EventHandler<EventArgs> SelectMetricsComparisonReportDestinationPathRequested;

        public event EventHandler<EventArgs> SelectSolutionRequested;

        public event EventHandler<EventArgs> SelectSpreadsheetFileRequested;

        /// <summary>
        /// This event serves to ask the UX to set itself to it's original design-extent.
        /// </summary>
        public static event EventHandler<EventArgs> SetUxToDefaultsRequested;
        public event EventHandler<UserNotificationEventArgs> UserNotificationRequested;

        #region constructor
        /// <summary>
        /// Create a new view-model instance.
        /// </summary>
        private ApplicationViewModel()
        {
            try
            {
                string myDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (!IsInDesignMode)
                {
                    ApplicationName = UserSettings.ApplicationName;
                    CopySourceDestinationPath = UserSettings.CopySourceDestinationPath;
                    if (UserSettings.DefaultRootFolderForVersionStateSnapshots == null)
                    {
                        DefaultRootFolderForVersionStateSnapshots = Path.Combine(myDocumentsFolder, "VsDevTool", "VersionHistory");
                    }
                    else
                    {
                        DefaultRootFolderForVersionStateSnapshots = UserSettings.DefaultRootFolderForVersionStateSnapshots;
                    }
                    IsToCleanVsDirectoriesAlso = UserSettings.IsToCleanVsDirectoriesAlso;
                    IsToExcludeMyOwnVsDirectory = UserSettings.IsToExcludeMyOwnVsDirectory;
                    IsToIncludeFiles = UserSettings.IsToIncludeFiles;
                    IsToIncludeFullPaths = UserSettings.IsToIncludeFullPaths;
                    IsToIncludeReferencedProjects = UserSettings.IsToIncludeReferencedProjects;
                    IsToIncludeTestProjects = UserSettings.IsToIncludeTestProjects;
                    IsToIncludeVersions = UserSettings.IsToIncludeVersions;
                    IsToIncludeWhenLastWritten = UserSettings.IsToIncludeWhenLastWritten;

                    MetricsComparisonSourceFolder1 = UserSettings.MetricsComparisonSourceFolder1;
                    MetricsComparisonSourceFolder2 = UserSettings.MetricsComparisonSourceFolder2;
                    MetricsComparisonReportDestinationPath = UserSettings.MetricsComparisonReportDestinationPath;

                    ModuleDependencyReportPath = UserSettings.ModuleDependencyReportPath;
                    // Set that to a sensible default location if none has been entered.
                    if (UserSettings.ModuleDependencyReportPath == null)
                    {
                        ModuleDependencyReportPath = Path.Combine(myDocumentsFolder, "VsDevTool", "VsDevReport.txt");
                    }
                    else
                    {
                        ModuleDependencyReportPath = UserSettings.ModuleDependencyReportPath;
                    }
                    OperationStatus = "-";
                    ProjectCopyrightNotice = UserSettings.CopyrightNotice;

                    ProgramVersion = App.The.GetProgramVersionTextFromAppConfig();

                    ProjectCompanyName = UserSettings.ProjectCompanyName;
                    ReferenceToChangeTo = UserSettings.ReferenceToChangeTo;
                    ResourceManagerKeyPattern = UserSettings.ResourceManagerKeyPattern;
                    Scope = UserSettings.Scope;
                    SelectedTabItem = UserSettings.SelectedTabItem;
                    SpreadsheetFilePathname = UserSettings.SpreadsheetFilePathname;
                    TargetNetFrameworkVersion = UserSettings.TargetNetFrameworkVersion;
                    VsProjectFilePathname = UserSettings.VsProjectFilePathname;
                    VsSolutionFilePathname = UserSettings.VsSolutionFilePathname;
                    WhenLastAnalyzed = DateTime.Now; //CBL
                    XamlPrefixForLocalizableValues = UserSettings.XamlPrefixForLocalizableValues;

                    foreach (var solutionPath in UserSettings.ApplicationSolutionPaths)
                    {
                        ApplicationSolutions.Add(new VsSolution(solutionPath));
                    }
                }
                else // is in design-time mode
                {
                    NugetReference = "Nuget-Package 10.11.12.13";
                    ApplicationName = "ApplicationName";
                    CopySourceDestinationPath = @"C:\Where To Copy That To";
                    DefaultRootFolderForVersionStateSnapshots = Path.Combine(myDocumentsFolder, "VsDevTool", "VersionHistory");

                    InstallerDrivePath = "F:";
                    IsInstallerDriveDetected = true;

                    ModuleDependencyReportPath = @"C:\OutputPath";
                    OperationStatus = "Analysing solution...";
                    ProgramVersion = "JH.1957.10.28.1234";
                    ProjectCompanyName = "Guided Therapeutics, Inc.";
                    ProjectCopyrightNotice = "Copyright \u00A9 2016 Guided Therapeutics Inc.";
                    ReferenceToChangeTo = @"C:\dev\archive\Desktop Applications\VsTools\ToolLibrary_40.dll";
                    ProjectReferencesUsed.Add(new VsProject("ReferenceA", isToCheckForExistence: false));
                    ProjectReferencesUsed.Add(new VsProject(@"C:\Dev\AppsDesktop\VsReportTool\VsDevTool.TestProject\VsDevTool.TestProject.csproj"));
                    ProjectReferencesUsed.Add(new VsProject("ReferenceC", isToCheckForExistence: false));
                    ProjectReferencesUsed.Add(new VsProject("ReferenceD", isToCheckForExistence: false));
                    ProjectReferencesUsed.Add(new VsProject("ReferenceE", isToCheckForExistence: false));
                    ProjectReferencesUsed.Add(new VsProject("ReferenceF", isToCheckForExistence: false));
                    ProjectReferencesUsed.Add(new VsProject("ReferenceG", isToCheckForExistence: false));
                    ProjectReferencesUsed.Add(new VsProject("ReferenceH", isToCheckForExistence: false));
                    ProjectReferencesUsed.Add(new VsProject("ReferenceI", isToCheckForExistence: false));
                    ProjectReferencesUsed.Add(new VsProject("ReferenceJ", isToCheckForExistence: false));
                    ProjectReferencesUsed.Add(new VsProject("ReferenceK", isToCheckForExistence: false));
                    ResourceManagerKeyPattern = "Look for me!";
                    Scope = ApplicationAnalysisScope.ApplicationScope;
                    VsSolutionFilePathname = @"C:\SampleFolder1";
                    WhenLastAnalyzed = DateTime.Now;
                    WhenWasLastStructureSnapshot = DateTime.Today;
                    string solutionPathname = @"C:\dev\GT\Development\BeMs\BeMs.sln";
                    VsSolutionFilePathname = solutionPathname;

                    ApplicationSolutions.Add(new VsSolution("RedSolution.sln"));
                    ApplicationSolutions.Add(new VsSolution("GeenSolution.sln"));
                    ApplicationSolutions.Add(new VsSolution("BlueSolution.sln"));

                    SolutionProjects.Add(new VsProject(@"C:\dev\AppsDesktop\VsReportTool\VsDevTool.TestProject\VsDevTool.TestProject.csproj", false));
                    SolutionProjects.Add(new VsProject(@"C:\dev\AppsDesktop\VsReportTool\VsDevTool.TestProject2\VsDevTool.TestProject2.csproj", false));
                    SelectedProject = SolutionProjects[0];

                    SelectedTabItem = 2;
                    WindowTitlePrefix = "GP  Vs-Report-Tool";

                    MetricsComparisonSourceFolder1 = @"C:\FolderPath1";
                    MetricsComparisonSourceFolder2 = @"C:\FolderPath2";
                    MetricsComparisonReportDestinationPath = @"C:\MetricsComparisonReportPathname";

                    //string projectPath = @"C:\dev\AppsDesktop\VsReportTool\VsDevTool.TestProject\VsDevTool.TestProject";
                    //string resource1Path = Path.Combine( projectPath, @"Resources\Strings\Resources1.resx" );
                    //var r1= new VsResourceFile( null, resource1Path ) ;
                    //ResourceFiles.Add( r1 );
                    //SelectedResourceFile = r1;
                }
            }
            catch (Exception x)
            {
                Logger.LogError(x);
            }
        }
        #endregion

        #region public properties

        #region The
        /// <summary>
        /// Get the (singleton) instance of this view-model.
        /// </summary>
        public static ApplicationViewModel The
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ApplicationViewModel();
                }
                return _instance;
            }
        }
        #endregion

        #region ApplicationName
        /// <summary>
        /// Get or set the name of the Application that is currently being looked at.
        /// </summary>
        public string ApplicationName
        {
            get { return _applicationName; }
            set
            {
                if (value != _applicationName)
                {
                    _applicationName = value;
                }
            }
        }
        #endregion

        #region ApplicationSolutions

        /// <summary>
        /// Get or set the collection of VS-solutions that are contained within this 'application'.
        /// Applies only when in Application-Scope.
        /// </summary>
        public ObservableCollection<VsSolution> ApplicationSolutions
        {
            get
            {
                if (_applicationSolutions == null)
                {
                    _applicationSolutions = new ObservableCollection<VsSolution>();
                }
                return _applicationSolutions;
            }
            set
            {
                if (value != _applicationSolutions)
                {
                    _applicationSolutions = value;
                    Notify("ApplicationSolutions");
                }
            }
        }

        #endregion

        #region ColorOfInstallerStatusField

        /// <summary>
        /// Get the color (as a Brush) to show the installer-drive-status-field in,
        /// which is green when a drive is detected, gray otherwise.
        /// </summary>
        public Brush ColorOfInstallerStatusField
        {
            get
            {
                if (IsInstallerDriveDetected)
                {
                    if (_brushGreen == null)
                    {
                        _brushGreen = new SolidColorBrush(Colors.LightGreen);
                    }
                    _colorOfInstallerStatusField = _brushGreen;
                }
                else
                {
                    if (_brushGray == null)
                    {
                        _brushGray = new SolidColorBrush(Colors.LightSlateGray);
                    }
                    _colorOfInstallerStatusField = _brushGray;
                }
                return _colorOfInstallerStatusField;
            }
        }

        private Brush _colorOfInstallerStatusField;
        private Brush _brushGreen;
        private Brush _brushGray;

        #endregion

        #region DefaultRootFolderForVersionStateSnapshots

        /// <summary>
        /// Get or set the directory-path to use for storing the application version-histories by default.
        /// The default value is My Documenets\VsDevTool\VersionHistory
        /// </summary>
        public string DefaultRootFolderForVersionStateSnapshots
        {
            get { return _defaultRootFolderForVersionStateSnapshots; }
            set
            {
                if (value != _defaultRootFolderForVersionStateSnapshots)
                {
                    _defaultRootFolderForVersionStateSnapshots = value;
                    Notify("DefaultRootFolderForVersionStateSnapshots");
                }
            }
        }

        #endregion

        #region CopySourceDestinationPath
        /// <summary>
        /// For when the user wants to copy the collection of source-code files to some other location,
        /// this directory-path denotes the destination of that copy-operation.
        /// </summary>
        public string CopySourceDestinationPath
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return _copySourceDestinationPath; }
            set
            {
                if (value != _copySourceDestinationPath)
                {
                    _copySourceDestinationPath = value;
                    Notify("CopySourceDestinationPath");
                }
            }
        }
        #endregion

        #region InstallerDriveDetectionStatus

        /// <summary>
        /// Get the text to show within that display-field that tells whether an installer-drive is detected.
        /// </summary>
        public string InstallerDriveDetectionStatus
        {
            get
            {
                if (IsInstallerDriveDetected)
                {
                    if (!String.IsNullOrWhiteSpace(InstallerDrivePath))
                    {
                        return "Installer-Drive detected - " + InstallerDrivePath;
                    }
                    return "Installer-Drive detected - but path is unknown";
                }
                else
                {
                    return "No Installer-Drive detected.";
                }
            }
        }

        #endregion

        #region InstallerDrivePath

        /// <summary>
        /// Get or set the filesystem-path upon which the installer-drive has been detected.
        /// </summary>
        public string InstallerDrivePath
        {
            get { return _installerDrivePath; }
            set
            {
                if (value != _installerDrivePath)
                {
                    _installerDrivePath = value;
                    Notify();
                    Notify("InstallerDriveDetectionStatus");
                }
            }
        }

        #endregion

        #region IsASolutionOrProjectToOperateUpon

        /// <summary>
        /// Get whether there is sufficient information entered thus far, such that 
        /// operations can be performed upon the given Visual Studio solution or project.
        /// </summary>
        public bool IsASolutionOrProjectToOperateUpon
        {
            get
            {
                bool result;
                switch (Scope)
                {
                    case ApplicationAnalysisScope.ApplicationScope:
                        result = ApplicationSolutions.Count > 0;
                        break;
                    case ApplicationAnalysisScope.SolutionScope:
                        result = !String.IsNullOrWhiteSpace(VsSolutionFilePathname);
                        break;
                    default:
                        result = !String.IsNullOrWhiteSpace(VsProjectFilePathname);
                        break;
                }
                return result;
            }
        }

        #endregion

        #region IsASolutionToOperateUpon

        /// <summary>
        /// Get whether there is a VS-solution selected.
        /// </summary>
        public bool IsASolutionToOperateUpon
        {
            get
            {
                bool result;
                switch (Scope)
                {
                    case ApplicationAnalysisScope.ApplicationScope:
                        result = SelectedSolution != null;
                        break;
                    case ApplicationAnalysisScope.SolutionScope:
                        result = !String.IsNullOrWhiteSpace(VsSolutionFilePathname);
                        break;
                    default:
                        result = false;
                        break;
                }
                return result;
            }
        }

        #endregion

        #region IsToCleanVsDirectoriesAlso
        /// <summary>
        /// Get or set whether, when CleanArtifactsOfProject is called,
        /// to also clean-out any .vs folders.
        /// Default is false.
        /// </summary>
        public bool IsToCleanVsDirectoriesAlso
        {
            get { return _isToCleanVsDirectoriesAlso; }
            set
            {
                if (value != _isToCleanVsDirectoriesAlso)
                {
                    _isToCleanVsDirectoriesAlso = value;
                    Notify();
                }
            }
        }
        #endregion

        #region  IsToExcludeMyOwnVsDirectory
        /// <summary>
        /// Get or set whether, when CleanArtifactsOfProject is called,
        /// to skip that Visual Studio project which represents this program itself (VsDevTool).
        /// Default is true.
        /// </summary>
        public bool IsToExcludeMyOwnVsDirectory
        {
            get { return _isToExcludeMyOwnVsDirectory; }
            set
            {
                if (value != _isToExcludeMyOwnVsDirectory)
                {
                    _isToExcludeMyOwnVsDirectory = value;
                    Notify();
                }
            }
        }
        #endregion

        #region IsInstallerDriveDetected
        /// <summary>
        /// Get or set whether an 'installer' removable-drive has been detected as being attached.
        /// </summary>
        public bool IsInstallerDriveDetected
        {
            get { return _isInstallerDriveDetected; }
            set
            {
                if (value != _isInstallerDriveDetected)
                {
                    _isInstallerDriveDetected = value;
                    Notify();
                    Notify("InstallerDriveDetectionStatus");
                    Notify("ColorOfInstallerStatusField");
                }
            }
        }
        #endregion

        #region IsToIncludeFiles

        /// <summary>
        /// This flag dictates whether to include all the files of each project, within the report,
        /// as opposed to including only the projects.
        /// </summary>
        public bool IsToIncludeFiles
        {
            get { return _isToIncludeFiles; }
            set
            {
                if (value != _isToIncludeFiles)
                {
                    _isToIncludeFiles = value;
                    Notify("IsToIncludeVersions");
                }
            }
        }

        #endregion

        #region IsToIncludeFullPaths

        /// <summary>
        /// This flag dictates whether to include the full pathnames of all files within the report.
        /// </summary>
        public bool IsToIncludeFullPaths
        {
            get { return _isToIncludeFullPaths; }
            set
            {
                if (value != _isToIncludeFullPaths)
                {
                    _isToIncludeFullPaths = value;
                    Notify("IsToIncludeVersions");
                }
            }
        }

        #endregion

        #region IsToIncludeReferencedProjects

        /// <summary>
        /// This flag dictates whether the report upon a given VS-Project is to include the other projects that it references.
        /// Default is true.
        /// </summary>
        public bool IsToIncludeReferencedProjects
        {
            get { return _isToIncludeReferencedProjects; }
            set
            {
                if (value != _isToIncludeReferencedProjects)
                {
                    _isToIncludeReferencedProjects = value;
                    Notify("IsToIncludeReferencedProjects");
                }
            }
        }

        #endregion

        #region IsToIncludeTestProjects

        /// <summary>
        /// This flag dictates whether the reports on a VS-Solution is to include VS-Projects that are Test projects,
        /// as identified by having ".Test*" within the name.  Default is false.
        /// </summary>
        public bool IsToIncludeTestProjects
        {
            get { return _isToIncludeTestProjects; }
            set
            {
                if (value != _isToIncludeTestProjects)
                {
                    _isToIncludeTestProjects = value;
                    Notify("IsToIncludeTestProjects");
                }
            }
        }

        #endregion

        #region IsToIncludeVersions

        /// <summary>
        /// This flag dictates whether to include the file and assbly versions within the report.
        /// </summary>
        public bool IsToIncludeVersions
        {
            get { return _isToIncludeVersions; }
            set
            {
                if (value != _isToIncludeVersions)
                {
                    _isToIncludeVersions = value;
                    Notify("IsToIncludeVersions");
                }
            }
        }

        #endregion

        #region IsToIncludeWhenLastWritten

        /// <summary>
        /// Get or set whether to include the time at which each file was last written within the report.
        /// </summary>
        public bool IsToIncludeWhenLastWritten
        {
            get { return _isToIncludeWhenLastWritten; }
            set
            {
                if (value != _isToIncludeWhenLastWritten)
                {
                    _isToIncludeWhenLastWritten = value;
                    Notify("IsToIncludeWhenLastWritten");
                }
            }
        }

        #endregion

        #region MetricsComparisonSourceFolder1

        /// <summary>
        /// Get or set the input folder-path 1 for the code-metrics comparison report.
        /// </summary>
        public string MetricsComparisonSourceFolder1
        {
            get { return _metricsComparisonSourceFolder1; }
            set
            {
                if (value != _metricsComparisonSourceFolder1)
                {
                    _metricsComparisonSourceFolder1 = value;
                    Notify();
                }
            }
        }

        #endregion

        #region MetricsComparisonSourceFolder2

        /// <summary>
        /// Get or set the input folder-path 2 for the code-metrics comparison report.
        /// </summary>
        public string MetricsComparisonSourceFolder2
        {
            get { return _metricsComparisonSourceFolder2; }
            set
            {
                if (value != _metricsComparisonSourceFolder2)
                {
                    _metricsComparisonSourceFolder2 = value;
                    Notify();
                }
            }
        }

        #endregion

        #region MetricsComparisonReportDestinationPath

        /// <summary>
        /// Get or set the output file-pathname for the code-metrics comparison report.
        /// </summary>
        public string MetricsComparisonReportDestinationPath
        {
            get { return _metricsComparisonReportDestinationPath; }
            set
            {
                if (value != _metricsComparisonReportDestinationPath)
                {
                    _metricsComparisonReportDestinationPath = value;
                    Notify();
                }
            }
        }

        #endregion

        #region ModuleDependencyReportPath
        /// <summary>
        /// Get or set the output-pathname to put the module-dependency report into.
        /// </summary>
        public string ModuleDependencyReportPath
        {
            get { return _moduleDependencyReportPath; }
            set
            {
                if (value != _moduleDependencyReportPath)
                {
                    _moduleDependencyReportPath = value;
                    Notify();
                }
            }
        }
        #endregion

        public string NugetReference
        {
            get { return _nugetReference; }
            set
            {
                if (value != _nugetReference)
                {
                    _nugetReference = value;
                    Notify();
                }
            }
        }
        private string _nugetReference;

        #region OperationStatus

        /// <summary>
        /// Get or set the string that denotes the state of the currently-running operation,
        /// as it would be presented to the end-user.
        /// </summary>
        public string OperationStatus
        {
            get { return _operationStatus; }
            set
            {
                if (value != _operationStatus)
                {
                    _operationStatus = value;
                    Notify("OperationStatus");
                }
            }
        }

        #endregion

        #region ProgramVersion

        /// <summary>
        /// Get or set the string that denotes the 'program-version' that this program wants to label itself as,
        /// as it would be presented to the end-user.
        /// </summary>
        public string ProgramVersion
        {
            get { return _programVersion; }
            set
            {
                if (value != _programVersion)
                {
                    _programVersion = value;
                    Notify("ProgramVersion");
                }
            }
        }

        #endregion

        #region ProjectCompanyName

        /// <summary>
        /// Get or set the text to set for the company-name property within a given project's AssemblyINfo.cs files.
        /// </summary>
        public string ProjectCompanyName
        {
            get { return _projectCompanyName; }
            set
            {
                if (value != _projectCompanyName)
                {
                    _projectCompanyName = value;
                    Notify("CopySourceDestinationPath");
                }
            }
        }

        #endregion

        #region ProjectCopyrightNotice

        /// <summary>
        /// Get or set what to set the Copyright-Notice to, wihin any given project's AssemblyInfo.cs file.
        /// </summary>
        public string ProjectCopyrightNotice
        {
            get { return _projectCopyrightNotice; }
            set
            {
                if (value != _projectCopyrightNotice)
                {
                    _projectCopyrightNotice = value;
                    Notify("ProjectCopyrightNotice");
                }
            }
        }

        #endregion

        public List<VsProject> ProjectsProcessed
        {
            get
            {
                if (_projectsProcessed == null)
                {
                    _projectsProcessed = new List<VsProject>();
                }
                return _projectsProcessed;
            }
        }

        public List<VsProject> ProjectReferencesUsed
        {
            get
            {
                if (_projectReferencesUsed == null)
                {
                    _projectReferencesUsed = new List<VsProject>();
                }
                return _projectReferencesUsed;
            }
        }

        private List<VsProject> _projectReferencesUsed;

        public VsProject SelectedProjectReference
        {
            get { return _selectedProjectReference; }
            set
            {
                if (value != _selectedProjectReference)
                {
                    _selectedProjectReference = value;
                    Debug.WriteLine("SelectedProjectReference set to " + value);
                    Notify();
                }
            }
        }

        private VsProject _selectedProjectReference;

        #region ReferenceToChangeTo
        /// <summary>
        /// Get or set the last value used to specify what to change the project-reference to,
        /// within the Change Reference dialog.
        /// </summary>
        public string ReferenceToChangeTo
        {
            get { return _referenceToChangeTo; }
            set
            {
                if (value != _referenceToChangeTo)
                {
                    _referenceToChangeTo = value;
                    Notify("ReferenceToChangeTo");
                }
            }
        }
        #endregion

        public ObservableCollection<VsResourceFile> ResourceFiles
        {
            get
            {
                if (_resourceFiles == null)
                {
                    _resourceFiles = new ObservableCollection<VsResourceFile>();
                }
                return _resourceFiles;
            }
            set { _resourceFiles = value; }
        }

        private ObservableCollection<VsResourceFile> _resourceFiles;

        public VsResourceFile SelectedResourceFile
        {
            get { return _selectedResourceFile; }
            set
            {
                _selectedResourceFile = value;
                Notify();
            }
        }

        private VsResourceFile _selectedResourceFile;

        #region ResourceManagerKeyPattern
        /// <summary>
        /// The text-pattern to scan for when looking for dynamically-composed strings to be globalized.
        /// </summary>
        public string ResourceManagerKeyPattern
        {
            get { return _resourceManagerKeyPattern; }
            set
            {
                if (value != _resourceManagerKeyPattern)
                {
                    _resourceManagerKeyPattern = value;
                    Notify("ResourceManagerKeyPattern");
                }
            }
        }
        #endregion

        #region Scope
        /// <summary>
        /// Get or set the enum-variable dictates which scale we wish to do our analysis at - Application, Solution, or Project.
        /// </summary>
        public ApplicationAnalysisScope Scope
        {
            get { return _scope; }
            set
            {
                if (value != _scope)
                {
                    _scope = value;
                    Notify(nameof(Scope));
                    Notify(nameof(IsApplicationScope));
                    Notify(nameof(IsSolutionScope));
                    Notify(nameof(IsProjectScope));
                    Notify(nameof(IsApplicationOrSolutionScope));
                }
            }
        }

        public bool IsApplicationScope
        {
            get
            {
#if DEBUG
                if (IsInDesignMode)
                {
                    return true;
                }
#endif
                return _scope == ApplicationAnalysisScope.ApplicationScope;
            }
        }

        public bool ShowApplicationScope
        {
            get
            {
#if DEBUG
                if (IsInDesignMode)
                {
                    return true;
                }
#endif
                return true;  //CBL
                //return _scope == ApplicationAnalysisScope.ApplicationScope;
            }
        }

        public bool IsSolutionScope
        {
            get
            {
#if DEBUG
                if (IsInDesignMode)
                {
                    return true;
                }
#endif
                return _scope == ApplicationAnalysisScope.SolutionScope;
            }
        }

        public bool ShowSolutionScope
        {
            get
            {
#if DEBUG
                if (IsInDesignMode)
                {
                    return true;
                }
#endif
                return true;  //CBL
                //return _scope == ApplicationAnalysisScope.SolutionScope;
            }
        }

        public bool IsApplicationOrSolutionScope
        {
            get
            {
#if DEBUG
                if (IsInDesignMode)
                {
                    return true;
                }
#endif
                return _scope == ApplicationAnalysisScope.ApplicationScope || _scope == ApplicationAnalysisScope.SolutionScope;
            }
        }

        public bool IsProjectScope
        {
            get
            {
#if DEBUG
                if (IsInDesignMode)
                {
                    return true;
                }
#endif
                return _scope == ApplicationAnalysisScope.ProjectScope;
            }
        }

        public bool ShowProjectScope
        {
            get
            {
#if DEBUG
                if (IsInDesignMode)
                {
                    return true;
                }
#endif
                return true;  //CBL
                //return _scope == ApplicationAnalysisScope.ProjectScope;
            }
        }

        #endregion

        #region SelectedProject
        /// <summary>
        /// Get or set the item within the Projects listbox that is to be regarded as currently-selected.
        /// </summary>
        public VsProject SelectedProject
        {
            get { return _selectedProject; }
            set
            {
                if (value != _selectedProject)
                {
                    _selectedProject = value;
                    Notify("SelectedProject");
                }
            }
        }
        #endregion

        #region SelectedSolution
        /// <summary>
        /// Get or set the VS-solution within the listbox that is to be regarded as currently-selected.
        /// Applies only when in Application-Scope.
        /// </summary>
        public VsSolution SelectedSolution
        {
            get { return _selectedSolution; }
            set
            {
                Debug.WriteLine($"SelectedSolution set to {value}");
                if (value != _selectedSolution)
                {
                    _selectedSolution = value;
                    Notify("SelectedSolution");
                    // Avoid a null-reference exception here if it was de-selected..
                    if (_selectedSolution == null)
                    {
                        VsSolutionFilePathname = "";
                    }
                    else
                    {
                        VsSolutionFilePathname = _selectedSolution.SolutionPathname;
                    }

                    Notify(nameof(VsSolutionFilePathname));
                }
            }
        }
        #endregion

        #region SelectedTabItem
        /// <summary>
        /// Get or set the index of the tab-item that is currently selected within the tab-control within the UX.
        /// The purpose of this is to restore that selection when this program is next launched.
        /// </summary>
        public int SelectedTabItem
        {
            get { return _selectedTabItem; }
            set
            {
                if (value != _selectedTabItem)
                {
                    _selectedTabItem = value;
                    Notify();
                }
            }
        }
        #endregion

        #region UserSettings
        /// <summary>
        /// Return the user-settings object.
        /// </summary>
        public VsDevToolUserSettings UserSettings
        {
            get { return VsDevToolUserSettings.The; }
        }
        #endregion

        #region SolutionProjects
        /// <summary>
        /// Get or set the collection of VS-projects that comprise the selected VS-solution,
        /// to serve as the ItemsSource for the "Projects" listbox.
        /// Applies when in application-scope or solution-scope.
        /// </summary>
        public ObservableCollection<VsProject> SolutionProjects
        {
            get
            {
                if (_solutionProjects == null)
                {
                    _solutionProjects = new ObservableCollection<VsProject>();
                }
                return _solutionProjects;
            }
            set
            {
                if (value != _solutionProjects)
                {
                    _solutionProjects = value;
                    Notify("SolutionProjects");
                }
            }
        }
        #endregion

        #region SpreadsheetFilePathname
        /// <summary>
        /// Get or set the pathname of the Excel spreadsheet file for importing resource-strings from.
        /// </summary>
        public string SpreadsheetFilePathname
        {
            get { return _spreadsheetFilePathname; }
            set
            {
                if (value != _spreadsheetFilePathname)
                {
                    _spreadsheetFilePathname = value;
                    Notify();
                }
            }
        }
        #endregion

        #region TargetNetFrameworkVersion

        /// <summary>
        /// Get or set the version of the .NET Framework that is to be targeted.
        /// The default is 4.51
        /// </summary>
        public NetFrameworkVersion TargetNetFrameworkVersion
        {
            get { return _targetNetFrameworkVersion; }
            set
            {
                if (value != _targetNetFrameworkVersion)
                {
                    _targetNetFrameworkVersion = value;
                    Notify("TargetNetFrameworkVersion");
                }
            }
        }

        #endregion

        public string TooltipForCopyToStageButton
        {
            get
            {
                string s = @"Copy Release files from C:\GTSource\LuViva\LuViva\bin\Release to " + this.CopySourceDestinationPath + " and remove any existing installer-files there.";
                return s;
            }
        }

        #region VsProjectFilePathname

        /// <summary>
        /// This string denotes the pathname of the Visual Studio project that the user wishes to analyze,
        /// which is a distinct operation from analyzing a solution or application.
        /// </summary>
        public string VsProjectFilePathname
        {
            get { return _vsProjectFilePathname; }
            set
            {
                if (value != _vsProjectFilePathname)
                {
                    _vsProjectFilePathname = value;
                    Notify("VsProjectFilePathname");
                }
            }
        }

        #endregion

        #region VsSolutionFilePathname
        /// <summary>
        /// This string denotes the pathname of the Visual Studio solution file that is currently targeted for processing.
        /// </summary>
        public string VsSolutionFilePathname
        {
            get
            {
                Debug.WriteLine($"VsSolutionFilePathname.get returning {StringLib.AsQuotedString(_vsSolutionFilePathname)}");
                return _vsSolutionFilePathname;
            }
            set
            {
                if (value != _vsSolutionFilePathname)
                {
                    _vsSolutionFilePathname = value;
                    Notify("VsSolutionFilePathname");
                }
            }
        }
        #endregion

        #region WhenLastAnalyzed

        /// <summary>
        /// Get or set when the build-stores were last used to assemble the destination artifacts.
        /// </summary>
        public DateTime WhenLastAnalyzed
        {
            get { return _whenLastAnalyzed; }
            set
            {
                if (value != _whenLastAnalyzed)
                {
                    _whenLastAnalyzed = value;
                    Notify("WhenLastAnalyzed");
                    Notify("WhenLastAnalyzedText");
                }
            }
        }

        /// <summary>
        /// Get the text that says when the build-stores were last used to assemble the destination artifacts.
        /// </summary>
        public string WhenLastAnalyzedText
        {
            get
            {
                if (_whenLastAnalyzed == default(DateTime))
                {
                    return String.Empty;
                }
                return "Last analyzed " + _whenLastAnalyzed.AsDateTimeString();
            }
        }

        #endregion

        #region WhenWasLastStructureSnapshot

        /// <summary>
        /// This DateTime denotes when the application structure was last analyzed and saved to storage as a snapshot.
        /// </summary>
        public DateTime WhenWasLastStructureSnapshot
        {
            get { return _whenWasLastStructureSnapshot; }
            set
            {
                if (value != _whenWasLastStructureSnapshot)
                {
                    _whenWasLastStructureSnapshot = value;
                    Notify("WhenWasLastStructureSnapshot");
                    Notify("WhenWasLastStructureSnapshotText");
                }
            }
        }

        /// <summary>
        /// Get the text expression of when the application structure was last analyzed and saved to storage as a snapshot.
        /// </summary>
        public string WhenWasLastStructureSnapshotText
        {
            get
            {
                if (_whenWasLastStructureSnapshot == default(DateTime))
                {
                    return String.Empty;
                }
                return "Last Snapshot was at " + _whenWasLastStructureSnapshot.AsDateTimeString();
            }
        }

        public bool IsLastSnapshot
        {
            get { return _whenWasLastStructureSnapshot != default(DateTime); }
        }

        #endregion

        public string WindowTitlePrefix
        {
            get
            {
                if (_windowTitlePrefix == null)
                {
                    string adminText = " (not admin)";
                    if (IsRunAsAdmin1())
                    {
                        adminText = " (Administrator1)";
                    }
                    else
                    {
                        if (IsRunAsAdmin2())
                        {
                            adminText = " (Administrator2)";
                        }
                    }
                    _windowTitlePrefix = App.The.VendorName + "  " + App.The.ProductName + adminText;
                }
                return _windowTitlePrefix;
            }
            set
            {
                _windowTitlePrefix = value;
                Notify();
            }
        }

        private string _windowTitlePrefix;

        public bool IsRunAsAdmin1()
        {
            return Thread.CurrentPrincipal.IsInRole(WindowsBuiltInRole.Administrator.ToString());
        }

        public bool IsRunAsAdmin2()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(id);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        #region XamlPrefixForLocalizableValues
        /// <summary>
        /// This is the text-pattern to scan the XAML source-code files for, when compiling a list of all localizable values.
        /// The default value is "lex:Loc" (only the 2nd 'L' is in uppercase).
        /// </summary>
        public string XamlPrefixForLocalizableValues
        {
            get { return _xamlPrefixForLocalizableValues; }
            set
            {
                if (value != _xamlPrefixForLocalizableValues)
                {
                    _xamlPrefixForLocalizableValues = value;
                    Notify();
                }
            }
        }

        #endregion

        #endregion public properties

        #region commands

        public ICommand FindAllNugetReferencesCommand
        {
            get
            {
                if (_findAllNugetReferencesCommand == null)
                {
                    _findAllNugetReferencesCommand = new UiCommand(_ =>
                    {
                        FindAllNugetReferences();
                    });
                }
                return _findAllNugetReferencesCommand;
            }
        }
        private ICommand _findAllNugetReferencesCommand;

        
        public ICommand FindThisNugetReferenceCommand
        {
            get
            {
                if (_findThisNugetReferenceCommand == null)
                {
                    _findThisNugetReferenceCommand = new UiCommand(_ =>
                    {
                        FindThisNugetReference();
                    });
                }
                return _findThisNugetReferenceCommand;
            }
        }
        private ICommand _findThisNugetReferenceCommand;

        public ICommand SetNugetTextReferencesCommand
        {
            get
            {
                if (_setNugetTextReferencesCommand == null)
                {
                    _setNugetTextReferencesCommand = new UiCommand(_ =>
                    {
                        SetNugetTextReferences();
                    });
                }
                return _setNugetTextReferencesCommand;
            }
        }
        private ICommand _setNugetTextReferencesCommand;


        public ICommand ImportStringsFromExcelSpreadsheetCommand
        {
            get
            {
                if (_importStringsFromExcelSpreadsheetCommand == null)
                {
                    _importStringsFromExcelSpreadsheetCommand = new UiCommand(_ =>
                    {
                        ImportStringsFromExcelSpreadsheet();
                    });
                }
                return _importStringsFromExcelSpreadsheetCommand;
            }
        }
        private ICommand _importStringsFromExcelSpreadsheetCommand;

        public ICommand CreateResourceFileCommand
        {
            get
            {
                if (_createResourceFileCommand == null)
                {
                    _createResourceFileCommand = new UiCommand(_ =>
                    {
                        CreateResourceXmlFile();
                    });
                }
                return _createResourceFileCommand;
            }
        }

        private ICommand _createResourceFileCommand;

        public ICommand SelectSpeadsheetForImportCommand
        {
            get
            {
                if (_selectSpeadsheetForImportCommand == null)
                {
                    _selectSpeadsheetForImportCommand = new UiCommand(_ =>
                    {
                        SelectSpreadsheetFileRequested?.Invoke(this, EventArgs.Empty);
                    });
                }
                return _selectSpeadsheetForImportCommand;
            }
        }

        private ICommand _selectSpeadsheetForImportCommand;

        /// <summary>
        /// Get the ICommand that signals the user's intention to
        /// select a resources file and add that to the list of resource-sets contained within the ListView
        /// on the Globalization tab.
        /// </summary>
        public ICommand AddResourceFileToSetsCommand
        {
            get
            {
                if (_addResourceFileToSetsCommand == null)
                {
                    _addResourceFileToSetsCommand = new UiCommand(_ =>
                    {
                        AddResourceFileToSetsRequested?.Invoke(this, EventArgs.Empty);
                    });
                }
                return _addResourceFileToSetsCommand;
            }
        }

        /// <summary>
        /// Get the ICommand that signals the user's intention to add another VS-solution to the current application.
        /// </summary>
        public ICommand AddSolutionToApplicationCommand
        {
            get
            {
                if (_addSolutionToApplicationCommand == null)
                {
                    _addSolutionToApplicationCommand = new UiCommand(_ => AddSolutionToApplication());
                }
                return _addSolutionToApplicationCommand;
            }
        }

        /// <summary>
        /// Get the ICommand that signals the user's intention to switch out a project's reference to some other assembly.
        /// </summary>
        public ICommand ChangeReferenceCommand
        {
            get
            {
                if (_changeReferenceCommand == null)
                {
                    _changeReferenceCommand = new UiCommand(_ => RequestToChangeReference(), _ => IsASolutionOrProjectToOperateUpon);
                }
                return _changeReferenceCommand;
            }
        }

        /// <summary>
        /// Get the ICommand that signals the user's intention to do a "Clean" of all Visual Studio output files (software artifacts).
        /// </summary>
        public ICommand CleanAllArtifactsCommand
        {
            get
            {
                if (_cleanAllArtifactsCommand == null)
                {
                    _cleanAllArtifactsCommand = new UiCommand(_ => CleanAllArtifacts(), _ => IsASolutionOrProjectToOperateUpon);
                }
                return _cleanAllArtifactsCommand;
            }
        }

        /// <summary>
        /// Get the ICommand that signals the user's intention to copy all of the source-files of the chosen application, solution, or project,
        /// to some other location.
        /// </summary>
        public ICommand CopySourceCommand
        {
            get
            {
                if (_copySourceCommand == null)
                {
                    _copySourceCommand = new UiCommand(_ => CopySource());
                }
                return _copySourceCommand;
            }
        }

        /// <summary>
        /// Get the ICommand that signals the user's wish to compare the code-metrics of two versions of the source-code
        /// and generate a textual report of their differences.
        /// </summary>
        public ICommand CompareCodeMetricsAndGenerateReportCommand
        {
            get
            {
                if (_compareCodeBasesAndGenerateReportCommand == null)
                {
                    _compareCodeBasesAndGenerateReportCommand = new UiCommand(_ => CompareCodeMetricsAndGenerateReport());
                }
                return _compareCodeBasesAndGenerateReportCommand;
            }
        }

        /// <summary>
        /// Get the ICommand that signals the user's wish to compare the artifact-structure of two versions of the source-code
        /// and display the differences,
        /// </summary>
        public ICommand CompareStructureSnapshotsCommand
        {
            get
            {
                if (_compareStructureSnapshotsCommand == null)
                {
                    _compareStructureSnapshotsCommand = new UiCommand(_ => CompareStructureSnapshots());
                }
                return _compareStructureSnapshotsCommand;
            }
        }

        public void CompareStructureSnapshots()
        {

        }

        /// <summary>
        /// Get the ICommand that signals the user's intention to empty out the ShadowCache folder.
        /// </summary>
        public ICommand ClearShadowCacheCommand
        {
            get
            {
                if (_clearShadowCacheCommand == null)
                {
                    _clearShadowCacheCommand = new UiCommand(_ => ClearShadowCache());
                }
                return _clearShadowCacheCommand;
            }
        }

        /// <summary>
        /// Get the ICommand that signals the user's intention to copy the build Release directory and it's contents to the staging location,
        /// which simply means where Ben's Excel code will produce the installer-drive contents.
        /// </summary>
        public ICommand CopyReleaseToStageCommand
        {
            get
            {
                if (_copyReleaseToStageCommand == null)
                {
                    _copyReleaseToStageCommand = new UiCommand(_ => CopyReleaseToStage());
                }
                return _copyReleaseToStageCommand;
            }
        }

        /// <summary>
        /// Get the RelayCommand that signals the user's intention to remove the lex DesignCulture attribute from all of the XAML files.
        /// </summary>
        public ICommand DeleteDesignCultureAttributesCommand
        {
            get
            {
                if (_deleteDesignCultureAttributesCommand == null)
                {
                    _deleteDesignCultureAttributesCommand = new UiCommand(_ => DeleteDesignCultureAttributesFromAllXamlFiles());
                }
                return _deleteDesignCultureAttributesCommand;
            }
        }

        /// <summary>
        /// Get the ICommand that signals the user's intention to deploy the current release-graph.
        /// </summary>
        public ICommand DeployReleaseGraphCommand
        {
            get
            {
                if (_deployReleaseGraphCommand == null)
                {
                    _deployReleaseGraphCommand = new UiCommand(_ => DeployReleaseGraph());
                }
                return _deployReleaseGraphCommand;
            }
        }

        /// <summary>
        /// Get the ICommand that signals the user's intention to edit the program options.
        /// </summary>
        public ICommand EditOptionsCommand
        {
            get
            {
                if (_editOptionsCommand == null)
                {
                    _editOptionsCommand = new UiCommand(_ => EditOptions());
                }
                return _editOptionsCommand;
            }
        }

        /// <summary>
        /// The RelayCommand that signals the generation of the dependency-graph.
        /// </summary>
        public ICommand GenerateDepGraphCommand
        {
            get
            {
                if (_generateDepGraphCommand == null)
                {
                    _generateDepGraphCommand = new UiCommand(_ => GenerateDepGraph());
                }
                return _generateDepGraphCommand;
            }
        }

        /// <summary>
        /// The RelayCommand that signals the generation of the dependency-graph.
        /// </summary>
        public ICommand GenerateFileListCommand
        {
            get
            {
                if (_generateFileListCommand == null)
                {
                    _generateFileListCommand = new UiCommand(_ => GenerateFileList(), _ => IsASolutionOrProjectToOperateUpon);
                }
                return _generateFileListCommand;
            }
        }

        public ICommand GenerateGlobReportCommand
        {
            get
            {
                if (_generateGlobReportCommand == null)
                {
                    _generateGlobReportCommand = new UiCommand(_ => GenerateGlobReport(), _ => IsASolutionOrProjectToOperateUpon);
                }
                return _generateGlobReportCommand;
            }
        }

        public ICommand LoadProjectsCommand
        {
            get
            {
                if (_loadProjectsCommand == null)
                {
                    _loadProjectsCommand = new UiCommand(_ => LoadProjects(), _ => IsASolutionToOperateUpon);
                }
                return _loadProjectsCommand;
            }
        }

        /// <summary>
        /// The RelayCommand that signals the user's request to open the text-file
        /// containing the most recently-created report.
        /// </summary>
        public ICommand OpenLastReportCommand
        {
            get
            {
                if (_openLastReportCommand == null)
                {
                    _openLastReportCommand = new UiCommand(_ => OpenLastReport());
                }
                return _openLastReportCommand;
            }
        }

        /// <summary>
        /// Get the command that signals the user's intention to refactor the lex-preficed attributes within all of the target-application's XAML files.
        /// </summary>
        public ICommand RefactorLexCommand
        {
            get
            {
                if (_refactorLexCommand == null)
                {
                    _refactorLexCommand = new UiCommand(_ => RefactorLex());
                }
                return _refactorLexCommand;
            }
        }

        /// <summary>
        /// Get the ICommand that signals the user's intention to remove the currently-selected VS-solution from the list of those
        /// associated with the current application.
        /// </summary>
        public ICommand RemoveSolutionFromApplicationCommand
        {
            get
            {
                if (_removeSolutionFromApplicationCommand == null)
                {
                    _removeSolutionFromApplicationCommand = new UiCommand(_ => DeleteSolutionFromApplication(), _ => SelectedSolution != null);
                }
                return _removeSolutionFromApplicationCommand;
            }
        }

        /// <summary>
        /// Get the command that signals to save the current ApplicationGraph to a disk file.
        /// </summary>
        public ICommand SaveApplicationStructureCommand
        {
            get
            {
                if (_saveApplicationStructureCommand == null)
                {
                    _saveApplicationStructureCommand = new UiCommand(_ =>
                    {
                        SaveApplicationStructureRequested?.Invoke(this, EventArgs.Empty);
                    });
                }
                return _saveApplicationStructureCommand;
            }
        }

        /// <summary>
        /// The RelayCommand that serves the "Browse.." button for selecting the destination directory for source-code copy operations.
        /// </summary>
        public ICommand SelectCopySourceDestinPathCommand
        {
            get
            {
                if (_selectCopySourceDestinPathCommand == null)
                {
                    _selectCopySourceDestinPathCommand = new UiCommand(_ =>
                    {
                        SelectCopySourceDestinationPathRequested?.Invoke(this, EventArgs.Empty);
                    });
                }
                return _selectCopySourceDestinPathCommand;
            }
        }

        /// <summary>
        /// The RelayCommand that indicates the user has requested to select the reference to change to.
        /// </summary>
        public ICommand SelectAssemblyToReferenceCommand
        {
            get
            {
                if (_selectAssemblyToReferenceCommand == null)
                {
                    _selectAssemblyToReferenceCommand = new UiCommand(_ =>
                    {
                        SelectAssemblyToReferenceRequested?.Invoke(this, EventArgs.Empty);
                    });
                }
                return _selectAssemblyToReferenceCommand;
            }
        }

        /// <summary>
        /// Get the RelayCommand that serves the browse-button for selecting the folder-path to location-1 for a code-metrics comparison.
        /// </summary>
        public ICommand SelectMetricsComparsionSourceFolder1Command
        {
            get
            {
                if (_selectMetricsComparisonSourceFolder1Command == null)
                {
                    _selectMetricsComparisonSourceFolder1Command = new UiCommand(_ =>
                    {
                        SelectMetricsComparsionSourceFolder1Requested?.Invoke(this, EventArgs.Empty);
                    });
                }
                return _selectMetricsComparisonSourceFolder1Command;
            }
        }

        /// <summary>
        /// Get the RelayCommand that serves the browse-button for selecting the folder-path to location-2 for a code-metrics comparison.
        /// </summary>
        public ICommand SelectMetricsComparsionSourceFolder2Command
        {
            get
            {
                if (_selectMetricsComparisonSourceFolder2Command == null)
                {
                    _selectMetricsComparisonSourceFolder2Command = new UiCommand(_ =>
                    {
                        SelectMetricsComparsionSourceFolder2Requested?.Invoke(this, EventArgs.Empty);
                    });
                }
                return _selectMetricsComparisonSourceFolder2Command;
            }
        }

        /// <summary>
        /// Get the RelayCommand that serves the browse-button for selecting the pathname to write the metrics-comparision report to.
        /// </summary>
        public ICommand SelectMetricsComparisonReportDestinationPathCommand
        {
            get
            {
                if (_selectMetricsComparisonReportDestinationPathCommand == null)
                {
                    _selectMetricsComparisonReportDestinationPathCommand = new UiCommand(_ =>
                    {
                        SelectMetricsComparisonReportDestinationPathRequested?.Invoke(this, EventArgs.Empty);
                    });
                }
                return _selectMetricsComparisonReportDestinationPathCommand;
            }
        }

        /// <summary>
        /// The RelayCommand that serves the "Browse.." button for selecting the pathname to write the report to.
        /// </summary>
        public ICommand SelectOutputPathCommand
        {
            get
            {
                if (_selectOutputPathCommand == null)
                {
                    _selectOutputPathCommand = new UiCommand(_ => SelectOutputPath());
                }
                return _selectOutputPathCommand;
            }
        }

        public ICommand SelectRootFolderForHistoryCommand
        {
            get
            {
                if (_selectRootFolderForHistoryCommand == null)
                {
                    _selectRootFolderForHistoryCommand = new UiCommand(_ => SelectRootFolderForHistory());
                }
                return _selectRootFolderForHistoryCommand;
            }
        }

        /// <summary>
        /// The RelayCommand that serves the "Browse.." button for selecting a Visual Studio solution to operate upon.
        /// </summary>
        public ICommand SelectVsProjectCommand
        {
            get
            {
                if (_selectVsProjectCommand == null)
                {
                    _selectVsProjectCommand = new UiCommand(_ => SelectVsProject());
                }
                return _selectVsProjectCommand;
            }
        }

        /// <summary>
        /// The RelayCommand that serves the "Browse.." button for selecting a Visual Studio solution to operate upon.
        /// </summary>
        public ICommand SelectVsSolutionCommand
        {
            get
            {
                if (_selectVsSolutionCommand == null)
                {
                    _selectVsSolutionCommand = new UiCommand(_ => SelectVsSolution());
                }
                return _selectVsSolutionCommand;
            }
        }

        public ICommand SetCompanyNameCommand
        {
            get
            {
                if (_setCompanyNameCommand == null)
                {
                    _setCompanyNameCommand = new UiCommand(_ => SetCompany(), _ => IsASolutionOrProjectToOperateUpon);
                }
                return _setCompanyNameCommand;
            }
        }

        public ICommand SetCopyrightCommand
        {
            get
            {
                if (_setCopyrightCommand == null)
                {
                    _setCopyrightCommand = new UiCommand(_ => SetCopyright(), _ => IsASolutionOrProjectToOperateUpon);
                }
                return _setCopyrightCommand;
            }
        }

        public ICommand SetGlobParametersToDefaultsCommand
        {
            get
            {
                if (_setGlobParametersToDefaultsCommand == null)
                {
                    _setGlobParametersToDefaultsCommand = new UiCommand(_ => SetGlobalizationParametersToDefaults());
                }
                return _setGlobParametersToDefaultsCommand;
            }
        }

        public ICommand SetNetFrameworkVersionCommand
        {
            get
            {
                if (_setNetFrameworkVersionCommand == null)
                {
                    _setNetFrameworkVersionCommand = new UiCommand(_ => SetNetFrameworkVersion(), _ => IsASolutionOrProjectToOperateUpon);
                }
                return _setNetFrameworkVersionCommand;
            }
        }

        public ICommand SetOptionsToDefaultsCommand
        {
            get
            {
                if (_setOptionsToDefaultsCommand == null)
                {
                    _setOptionsToDefaultsCommand = new UiCommand(_ => SetOptionsToDefaults());
                }
                return _setOptionsToDefaultsCommand;
            }
        }

        public ICommand SetProjectVersionsCommand
        {
            get
            {
                if (_setProjectVersionsCommand == null)
                {
                    _setProjectVersionsCommand = new UiCommand(_ => SetProjectVersions());
                }
                return _setProjectVersionsCommand;
            }
        }

        public ICommand SetToNewReferenceCommand
        {
            get
            {
                if (_setToNewReferenceCommand == null)
                {
                    _setToNewReferenceCommand = new UiCommand(_ => SetToNewReference(), _ =>
                    {
                        return SelectedProjectReference != null;
                    });
                }
                return _setToNewReferenceCommand;
            }
        }

        public ICommand ShowHelpAboutMeCommand
        {
            get
            {
                if (_showHelpAboutMeCommand == null)
                {
                    _showHelpAboutMeCommand = new UiCommand(_ =>
                    {
                        var window = new AboutView();
                        window.Owner = Application.Current.MainWindow;
                        window.Show();
                    });
                }
                return _showHelpAboutMeCommand;
            }
        }

        /// <summary>
        /// The RelayCommand that causes this program to quit.
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get
            {
                if (_exitCommand == null)
                {
                    _exitCommand = new UiCommand(_ => Exit());
                }
                return _exitCommand;
            }
        }

        #endregion commands

        #region public methods

        #region ImportStringsFromExcelSpreadsheet
        public void ImportStringsFromExcelSpreadsheet()
        {
            Debug.WriteLine("begin ImportStringsFromExcelSpreadsheet");
            // Excel.Application xlApp = new Excel.Application();
            //string pathname = this.SpreadsheetFilePathname;
            string pathname = this.SpreadsheetFilePathname;
            if (!String.IsNullOrWhiteSpace(pathname))
            {
                if (File.Exists(pathname))
                {
                    // This does iterate down column A.
                    // Using EPPlus
                    var r = ExcelImporter.ImportStringsFromExcelSpreadsheet(pathname, worksheetIndex: 2);

                    int n = r.Count;
                    Console.WriteLine("n = " + n);
                    if (n > 0)
                    {
                        for (int i = 0; i < n; i++)
                        {
                            var item = r[i];
                            Console.WriteLine("i = " + i + ": item is " + item);
                        }
                    }

                    // Use the English resources-file to acquire the key values for these Chinese translations..

                    Debug.WriteLine("start reading English reference-file..");
                    //const string pathnameOfEnglishResourceFile = @"C:\GTSource\Luviva\LuvivaUI.Resources\Resources.resx";
                    const string pathnameOfEnglishResourceFile = @"C:\GTSource\Luviva\LuvivaUI.Resources\ReportResources.resx";
                    if (!File.Exists(pathnameOfEnglishResourceFile))
                    {
                        throw new FileNotFoundException(fileName: pathnameOfEnglishResourceFile, message: "I do not see this file.");
                    }
                    var resourceFile = new VsResourceFile(pathnameOfEnglishResourceFile);
                    var resourcesEnglish = resourceFile.GetStrings();

                    Debug.WriteLine("Read " + resourcesEnglish.Count + " items from " + pathnameOfEnglishResourceFile);
                    // See whether, for each English-term from the Chinese resources, there is a corresponding value in the English file.
                    if (n > 0)
                    {
                        int numberMatched = 0;
                        int numberNotMatched = 0;
                        for (int i = 0; i < n; i++)
                        {
                            LanguageResource item = r[i];
                            string englishValue = item.EnglishValue;
                            Debug.WriteLine("considering item " + item);

                            bool wasFound = false;
                            foreach (var dItem in resourcesEnglish)
                            {
                                string valueFromEnglish = dItem.Value;
                                if (englishValue.Equals(valueFromEnglish, StringComparison.OrdinalIgnoreCase))
                                {
                                    wasFound = true;
                                    item.Key = dItem.Key;
                                    break;
                                }
                            }

                            if (wasFound)
                            {
                                numberMatched++;
                            }
                            else
                            {
                                numberNotMatched++;
                            }
                            Debug.WriteLine("Done searching for this item, wasFound = " + wasFound);
                        }
                        Debug.WriteLine("Finished checking all entries. numberMatched = " + numberMatched + ", numberNotMatched = " + numberNotMatched);
                    }

                    Debug.WriteLine("Start writing the Chinese resx file..");
                    const string pathnameOfChineseResourceFile = @"C:\GTSource\Luviva\LuvivaUI.Resources\ReportResources.zh-CN.resx";
                    if (!File.Exists(pathnameOfEnglishResourceFile))
                    {
                        File.Delete(pathnameOfChineseResourceFile);
                    }

                    var resourceFileChinese = new VsResourceFile(pathnameOfChineseResourceFile);
                    int countWritten = resourceFileChinese.WriteResxFile(r);

                    Debug.WriteLine("Done - " + countWritten + " string-resources written.");

                    //FileInfo file = new FileInfo( pathname );
                    //using (ExcelPackage package = new ExcelPackage( file ))
                    //{
                    //    // get the first worksheet in the workbook
                    //    int numberOfSheets = package.Workbook.Worksheets.Count;
                    //    Console.WriteLine( "numberOfSheets is " + numberOfSheets );
                    //    int iStart = 0;
                    //    int iEnd = numberOfSheets - 1;
                    //    if (package.Compatibility.IsWorksheets1Based)
                    //    {
                    //        iStart = 1;
                    //        iEnd = numberOfSheets;
                    //    }
                    //    for (int i = iStart; i <= iEnd; i++)
                    //    {
                    //        ExcelWorksheet worksheet = package.Workbook.Worksheets[i];
                    //        Console.WriteLine( "For " + i + ": worksheet.Name = " + worksheet.Name );
                    //    }

                    //    int worksheetResourcesIndex = 3;
                    //    ExcelWorksheet worksheetResources = package.Workbook.Worksheets[worksheetResourcesIndex];
                    //    //ExcelColumn firstColumn = worksheetResources.Column( 1 );

                    //    string englishVersion = "?";
                    //    string chineseVersion = "?";
                    //    string descriptionInEnglish = "?";
                    //    string descriptionInChinese;
                    //    bool lastWasEnglish = true;

                    //    int n = 1;
                    //    do
                    //    {
                    //        var x = worksheetResources.Cells[n, 1].Value;
                    //        string s = x as String;
                    //        //Console.Write( "for n = " + n + ", x = " + x );
                    //        if (x != null)
                    //        {
                    //            // If this is English
                    //            if (!IsChinese( s ))
                    //            {
                    //               // Console.WriteLine( "   is English" );
                    //                englishVersion = s;
                    //                descriptionInEnglish = worksheetResources.Cells[n, 2].Value as String;
                    //                lastWasEnglish = true;
                    //            }
                    //            else // Chinese
                    //            {
                    //                //Console.WriteLine( "    is Chinese" );
                    //                chineseVersion = s;
                    //                lastWasEnglish = false;
                    //                Console.WriteLine("for n " + n + ": English = " + englishVersion + ", Chinese = " + chineseVersion + ", description = " + descriptionInEnglish);
                    //            }
                    //        }
                    //        else // blank
                    //        {
                    //            if (lastWasEnglish)
                    //            {
                    //                chineseVersion = "None";
                    //                Console.WriteLine( "for n " + n + ": English = " + englishVersion + ", Chinese = " + chineseVersion + ", description = " + descriptionInEnglish );
                    //            }
                    //            else
                    //            {
                    //                Console.WriteLine("for n = " + n + " - blank line.");
                    //            }
                    //            lastWasEnglish = false;
                    //        }

                    //        n++;

                    //    } while (n < 50);

                    // Strategy:

                    // Get the next row value:
                    //   If this is English,
                    //     accept this as the English version (I do not see keys here?)
                    //     Get the enxt row value
                    //     If this is Chinese
                    //       accept this as a translation.
                    //     else if this is blank
                    //       add that English term to the list as having no translation (it appears in the UX as-is)
                    //   If this is blank
                    //     next


                    //int col = 2; //The item description
                    //// output the data in column 2
                    //for (int row = 2; row < 5; row++)
                    //    Console.WriteLine( "\tCell({0},{1}).Value={2}", row, col, worksheet.Cells[row, col].Value );

                    //// output the formula in row 5
                    //Console.WriteLine( "\tCell({0},{1}).Formula={2}", 3, 5, worksheet.Cells[3, 5].Formula );
                    //Console.WriteLine( "\tCell({0},{1}).FormulaR1C1={2}", 3, 5, worksheet.Cells[3, 5].FormulaR1C1 );

                    //// output the formula in row 5
                    //Console.WriteLine( "\tCell({0},{1}).Formula={2}", 5, 3, worksheet.Cells[5, 3].Formula );
                    //Console.WriteLine( "\tCell({0},{1}).FormulaR1C1={2}", 5, 3, worksheet.Cells[5, 3].FormulaR1C1 );
                    //}



                }
                else // wrong file?
                {
                    NotifyUserOfMistake("I don't see that file.");
                }
            }
            else // no file!
            {
                NotifyUserOfMistake("Please specify what Excel spreadsheet file to import from.");
            }
        }
        #endregion ImportStringsFromExcelSpreadsheet

        public void CreateResourceXmlFile()
        {
            var resourceFile = new VsResourceFile(@".\TestResources.resx");
            List<LanguageResource> resources = new List<LanguageResource>();
            resources.Add(new LanguageResource(valueEnglish: "two", valueOtherLanguage: "does"));
            resourceFile.WriteResxFile(resources);
        }

        public void AddSolutionToApplication()
        {
            // Raise the appropriatte event, so that the view may respond to it.
            AddSolutionToApplicationRequested?.Invoke(this, EventArgs.Empty);
        }

        #region CleanAllArtifacts
        public void CleanAllArtifacts()
        {
            switch (this.Scope)
            {
                case ApplicationAnalysisScope.ApplicationScope:
                    NotifyUserOfMistake("Sorry - not implemented yet for application-scope.");
                    break;
                case ApplicationAnalysisScope.SolutionScope:
                    if (File.Exists(this.VsSolutionFilePathname))
                    {
                        VsSolution solution = new VsSolution(this.VsSolutionFilePathname);
                        Logger.LogInfo("Cleaning artifact-output for VS solution " + solution.Name);
                        foreach (var project in solution.Projects)
                        {
                            CleanArtifactsOfProject(project);
                        }
                    }
                    else
                    {
                        NotifyUserOfError("That VS solution file does not seem to be present.");
                    }
                    break;
                case ApplicationAnalysisScope.ProjectScope:
                    if (File.Exists(VsProjectFilePathname))
                    {
                        VsProject project = new VsProject(VsProjectFilePathname);
                        int numberOfFilesDeleted = CleanArtifactsOfProject(project);
                        if (numberOfFilesDeleted > 0)
                        {
                            NotifyUser("Deleted " + numberOfFilesDeleted + " software-artifact files in project " + project + " .");
                        }
                    }
                    else
                    {
                        NotifyUserOfError("That VS project file does not seem to be present.");
                    }
                    break;
            }
        }
        #endregion CleanAllArtifacts

        #region CleanArtifactsOfProject
        private int CleanArtifactsOfProject( VsProject project )
        {
            // ALERT: This only applies to C# Projects, as organized under Visual Studio.

            // Skip my own project-directory if called for.
            if (IsToExcludeMyOwnVsDirectory)
            {
                string projectPath = project.Pathname;
                if (projectPath.Contains("VsDevTool"))
                {
                    Logger.LogInfo("Skipping my own project " + project);
                    return 0;
                }
            }

            int numberOfFilesDeleted = 0;
            string projectFolder = FileStringLib.GetDirectoryOfPath(project.Pathname);
            string objFolder = Path.Combine(projectFolder, "obj");

            if (FilesystemLib.DirectoryExists(objFolder))
            {
                try
                {
                    int countOfFiles = FilesystemLib.GetFiles(objFolder, SearchOption.AllDirectories).Length;
                    FilesystemLib.DeleteDirectory(objFolder);
                    numberOfFilesDeleted += countOfFiles;
                }
                catch (Exception x)
                {
                    Logger.LogError(x, "Attempting to delete the obj folder within project " + project);
                }
            }

            string binFolder = Path.Combine(projectFolder, "bin");

            if (FilesystemLib.DirectoryExists(binFolder))
            {
                try
                {
                    int countOfFiles = FilesystemLib.GetFiles(binFolder, SearchOption.AllDirectories).Length;
                    FilesystemLib.DeleteDirectory(binFolder);
                    numberOfFilesDeleted += countOfFiles;
                }
                catch (Exception x)
                {
                    Logger.LogError(x, "Attempting to delete the bin folder within project " + project);
                }
            }

            if (IsToCleanVsDirectoriesAlso)
            {
                string vsFolder = Path.Combine(projectFolder, ".vs");

                if (FilesystemLib.DirectoryExists(vsFolder))
                {
                    try
                    {
                        int countOfFiles = FilesystemLib.GetFiles(vsFolder, SearchOption.AllDirectories).Length;
                        FilesystemLib.DeleteDirectory(vsFolder);
                        numberOfFilesDeleted += countOfFiles;
                    }
                    catch (Exception x)
                    {
                        Logger.LogError(x, "Attempting to delete the .vs folder within project " + project);
                    }
                }
            }

            Logger.LogInfo("Cleaning artifact-output for VS project " + project + ", " + numberOfFilesDeleted + " files deleted.");
            return numberOfFilesDeleted;
        }
        #endregion CleanArtifactsOfProject

        #region ClearShadowCache
        public void ClearShadowCache()
        {
            // And now clear-out the designer-cache..
            try
            {
                string applicationDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                string vsDataFolder = Path.Combine(applicationDataFolder, "Microsoft", "VisualStudio");
                // here we msut choose which version of Visual Studio to operate upon. VS 2015 uses "14.0".
                string vs2015DataFolder = Path.Combine(vsDataFolder, "14.0");
                if (FilesystemLib.DirectoryExists(vs2015DataFolder))
                {
                    string vsShadowCache = Path.Combine(vs2015DataFolder, "Designer", "ShadowCache");
                    FilesystemLib.DeleteDirectoryContent(vsShadowCache);
                    NotifyUser("The Visual Studio shadow-cache under version 14.0 has been cleared.");
                }
                else
                {
                    NotifyUserOfError("There is no version-14 data folder for Visual Studio.");
                }
            }
            catch (Exception x)
            {
                //App.TheLogger.LogException( x, "Whilst clearing out the shadow-cache for version 14.0" );
                App.NotifyOfException(exception: x, informationForDeveloper: "Whilst clearing out the shadow-cache for version 14.0", messageToUser: "Hmm..");
            }
        }
        #endregion ClearShadowCache

        #region CompareCodeMetricsAndGenerateReport
        public void CompareCodeMetricsAndGenerateReport()
        {
            // Validate the filesystem-paths that were specified..
            if (!Directory.Exists(MetricsComparisonSourceFolder1))
            {
                NotifyUserOfMistake("I do not see that Folder 1 is present.");
                return;
            }
            if (!Directory.Exists(MetricsComparisonSourceFolder2))
            {
                NotifyUserOfMistake("I do not see that Folder 2 is present.");
                return;
            }
            string outputDirectory = FileStringLib.GetDirectoryOfPath(MetricsComparisonReportDestinationPath);
            if (!Directory.Exists(outputDirectory))
            {
                NotifyUserOfMistake("I do not see that the destination-directory is present.");
                return;
            }
        }
        #endregion CompareCodeMetricsAndGenerateReport

        #region CopyReleaseToStage
        public void CopyReleaseToStage()
        {
            // Warn if we are not running with Administrator privileges.
            if (!IsRunAsAdmin2())
            {
                //Console.WriteLine( "Warning: InteMirror is not being run with Administrator privileges." );
                Logger.Warn("InteMirror is not being run with Administrator privileges");
                return;
            }

            if (!String.IsNullOrWhiteSpace(this.CopySourceDestinationPath))
            {
                string stageDir = this.CopySourceDestinationPath;
                string stageReleaseDir = Path.Combine(stageDir, "Release");

                // Remove the existing Deploy\Release directory..
                if (Directory.Exists(stageReleaseDir))
                {
                    Logger.LogTrace("CopyReleaseToStage: deleting Release directory at " + stageReleaseDir);
                    // this fails on a DropBox folder.
                    //FilesystemLib.DeleteDirectory( stageReleaseDir );
                    //FilesystemLib.DeleteDirectoryA( stageReleaseDir );
                    Directory.Delete(stageReleaseDir, true);
                }
                if (Directory.Exists(stageReleaseDir))
                {
                    NotifyUserOfError("Apparently the staged-release dir failed to delete!");
                    return;
                }

                // Copy the Release directory that was just built over to Deploy..
                string releaseDir = @"C:\GTSource\LuViva\LuViva\bin\Release";
                if (Directory.Exists(releaseDir))
                {
                    Logger.LogTrace("CopyReleaseToStage: copying release files to " + stageReleaseDir);
                    //FilesystemLib.CopyDirectory( releaseDir, stageReleaseDir );
                    Logger.LogTrace("CopyReleaseToStage: done copying release files.");

                    // Now remove any existing installer-files directory..
                    string installerDirName = "D6_0 EN_8e";
                    string installerDir = Path.Combine(stageDir, installerDirName);
                    if (Directory.Exists(installerDir))
                    {
                        Logger.LogTrace("CopyReleaseToStage: deleting previous installer-dir content from " + installerDir);
                        //FilesystemLib.DeleteDirectoryA( installerDir );
                        string pathOfSpecialFile = Path.Combine(installerDir, @"_Installer (Thumb Drive) Deploy-000_2 ^0C32_1", "gs.gtk");
                        Logger.LogTrace("Clearing the Hidden attribute of " + pathOfSpecialFile);
                        FilesystemLib.SetFileHidden(pathOfSpecialFile, false);
                        if (FilesystemLib.IsFileHidden(pathOfSpecialFile))
                        {
                            //NotifyUserOfError( "Unable to clear the Hidden attribute of the gs.gtk file." );
                            Logger.LogError("Unable to clear the Hidden attribute of the gs.gtk file.");
                            //return;
                        }

                        Logger.LogTrace("Clearing the ReadOnly attribute of " + pathOfSpecialFile);
                        FilesystemLib.SetFileReadonly(pathOfSpecialFile, false);
                        if (FilesystemLib.IsFileReadonly(pathOfSpecialFile))
                        {
                            NotifyUserOfError("Unable to clear the ReadOnly attribute of the gs.gtk file.");
                            return;
                        }

                        Logger.LogTrace("Deleting " + pathOfSpecialFile);
                        File.Delete(pathOfSpecialFile);
                        Logger.LogTrace("Deleting the rest of " + installerDir);
                        Directory.Delete(installerDir, true);
                    }
                }
                else // the source Release directory does not seem to be there.
                {
                    NotifyUserOfMistake("The source-Release directory is not found where it is expected to be, at " + releaseDir);
                }
            }
            else // no path has been entered yet to specify where to copy it to.
            {
                NotifyUserOfMistake("You need to enter the staging-directory to copy the Release files to.");
            }
        }
        #endregion CopyReleaseToStage

        #region CopySource
        /// <summary>
        /// Copy all of the source-files from the selected solutions/projects, to the destination-folder as specified
        /// by the field "Root of Destination-Path for Source Files:".
        /// </summary>
        public void CopySource()
        {
            if (!String.IsNullOrWhiteSpace(this.CopySourceDestinationPath))
            {
                if (!Directory.Exists(this.CopySourceDestinationPath))
                {
                    FilesystemLib.CreateDirectory(this.CopySourceDestinationPath);
                }
                //CBL
                ProjectsProcessed.Clear();
                var sources = new SortedSet<FileSourceLocation>();
                int numberOfProjects = 0;

                if (Scope == ApplicationAnalysisScope.ApplicationScope)
                {
                    //int numberOfFiles = 0;
                    if (ApplicationSolutions.Count > 0)
                    {
                        foreach (var solution in ApplicationSolutions)
                        {
                            sources.Add(new FileSourceLocation(solution.SolutionPathname, solution.Folder));
                            foreach (var project in solution.Projects)
                            {
                                ProjectsProcessed.Add(project);
                            }
                        }
                    }
                    else
                    {
                        NotifyUserOfMistake("You need to add at least one Visual Studio solution to this application first.");
                    }
                }
                else if (Scope == ApplicationAnalysisScope.SolutionScope)
                {
                    string solutionPathname = this.VsSolutionFilePathname;
                    if (!String.IsNullOrWhiteSpace(solutionPathname))
                    {
                        if (File.Exists(solutionPathname))
                        {
                            VsSolution solution = new VsSolution(solutionPathname);
                            sources.Add(new FileSourceLocation(solution.SolutionPathname, solution.Folder));

                            // Note: I am not bothering to add intermediate files such as .suo, folder .vs, etc.

                            foreach (var project in solution.Projects)
                            {
                                ProjectsProcessed.Add(project);
                            }
                        }
                        else
                        {
                            NotifyUserOfError("That VS solution file does not seem to be present.");
                        }
                    }
                    else
                    {
                        NotifyUserOfMistake("You need to enter a pathname to the Visual Studio solution you want to analyze.");
                    }
                }
                else if (Scope == ApplicationAnalysisScope.ProjectScope)
                {
                    if (!String.IsNullOrWhiteSpace(VsProjectFilePathname))
                    {
                        if (File.Exists(VsProjectFilePathname))
                        {
                            VsProject project = new VsProject(VsProjectFilePathname);
                            ProjectsProcessed.Add(project);
                        }
                        else
                        {
                            NotifyUserOfError("That VS project file does not seem to be present.");
                        }
                    }
                    else
                    {
                        NotifyUserOfMistake("You need to enter a pathname to the VS project you want to analyze.");
                    }
                }

                // Now that we have accumulated a list of the projects, get a list of all of their source-files.
                int nProjects = ProjectsProcessed.Count;
                if (nProjects > 0)
                {
                    Logger.LogTrace(nProjects + " projects found within the given source. Enumerating their files...");

                    var projectsToEnumeration = ProjectsProcessed.ToArray();
                    foreach (var project in projectsToEnumeration)
                    {
                        sources.Add(new FileSourceLocation(project.Pathname, project.Folder));
                        var files = AddSourceFilesFromProject(project, numberOfProjects: ref numberOfProjects);
                        foreach (var f in files)
                        {
                            sources.Add(new FileSourceLocation(f.Pathname, project.Folder));
                        }
                    }

                    // Now that we have accumulated a list of the source-files, check to see whether any are rooted above the root-folder.
                    //
                    // First, go through the complete list of files to see what the lowest common root-folder is.
                    // Eg, if the files are..
                    //   C:\Dir1\Dir2\FileA,  commonRoot = C:\Dir1\Dir2
                    //   C:\Dir1\FileB,       -> C:\Dir1
                    //   C:\Dir3\FileC,       -> \ (error - cannot be root-directory!)
                    //   D:\Dir4\FileD,       ->   (error - cannot be different drive!)
                    string commonRoot = null;
                    foreach (var f in sources)
                    {
                        string path = f.SourcePathname;
                        string dir = FileStringLib.GetDirectoryOfPath(path);
                        if (commonRoot == null)
                        {
                            commonRoot = dir;
                        }
                        else
                        {
                            if (dir.Length < commonRoot.Length)
                            {
                                if (commonRoot.StartsWith(dir))
                                {
                                    commonRoot = dir;
                                }
                            }
                        }
                    }
                    bool isDifferentDrives;
                    bool isRoot;
                    string reason;
                    var sourcePaths = (from f in sources select f.SourcePathname);
                    commonRoot = FileStringLib.GetLowestCommonRoot(sourcePaths, out isDifferentDrives, out isRoot, out reason);

                    // Now that we have checked the source-files, begin the copy operation.
                    if (sources.Count > 0)
                    {
                        Logger.LogTrace(sources.Count + " source-files found. Beginning the copy operation...");

                        int numberOfFailures = 0;
                        int numberActuallyCopied = 0;
                        foreach (FileSourceLocation fileSourceLocation in sources)
                        {
                            // We need to compose a destination-path for each file.
                            // eg, to copy files from C:\Dev\ProjectA, to C:\Backups\Blue\DestinFolder
                            // We can have the project's folder which is C:\Dev\ProjectA,
                            // and a source-file path of C:\Dev\ProjectA\Images\Pic.gif
                            //
                            // The desired destin would be: C:\Backups\Blue\DestinFolder\ProjectA\Images\Pic.gif
                            //
                            // That is the purpose for the FileSourceLocation class: to contain both a pathname, and the source-folder,
                            // such that the appropriate destination-folder may be created from that.
                            try
                            {
                                string destinationPathname = fileSourceLocation.GetDestinationPathname(commonRootOfSource: commonRoot, destinationRootFolder: CopySourceDestinationPath);

                                Debug.WriteLine("Copying file " + fileSourceLocation.SourcePathname + " to " + destinationPathname);

                                FilesystemLib.CopyFile(sourceFilePath: fileSourceLocation.SourcePathname, destinationFilePath: destinationPathname);
                                numberActuallyCopied++;
                            }
                            catch (Exception x)
                            {
                                numberOfFailures++;
                                Logger.LogError(x);

                                if (numberOfFailures > 3)
                                {
                                    NotifyUserOfError("Reached 3 failures in attempting to copy files. Aborting.");
                                    break;
                                }
                            }
                        }

                        NotifyUser(numberActuallyCopied + " source-files copied.");
                    }
                    else
                    {
                        NotifyUser("No source-files to copy.");
                    }
                }

            }
            else // no path has been entered yet to specify where to copy it to.
            {
                NotifyUserOfMistake("You need to enter the destination-path to copy the source files to.");
            }
        }
        #endregion CopySource

        public void DeleteSolutionFromApplication()
        {
            if (SelectedSolution != null)
            {
                this.ApplicationSolutions.Remove(SelectedSolution);
            }
        }

        #region DeployReleaseGraph
        public void DeployReleaseGraph()
        {
            Console.WriteLine("DeployReleaseGraph, InstallerDrivePath is " + this.InstallerDrivePath);

            // Warn if we are not running with Administrator privileges.
            if (!IsRunAsAdmin2())
            {
                //Console.WriteLine( "Warning: InteMirror is not being run with Administrator privileges." );
                NotifyUserOfMistake("You need to run this program as Administrator.");
                return;
            }

            if (IsInstallerDriveDetected)
            {
                string installerDir = InstallerDrivePath;
                FilesystemLib.DeleteDirectoryContentFiles(installerDir, "*.*");
            }
            else
            {
                NotifyUserOfMistake("No installer-drive is present. You need to use what that has the text Installer within it's volume-label.");
            }
        }
        #endregion DeployReleaseGraph

        #region EditOptions
        public void EditOptions()
        {
            EditOptionsRequested?.Invoke(this, EventArgs.Empty);
        }
        #endregion EditOptions

        #region GenerateDepGraph
        /// <summary>
        /// Create a dependency-graph for the Visual Studio project that is currently targeted.
        /// </summary>
        public void GenerateDepGraph()
        {
            Save();
            ProjectsProcessed.Clear();
            var sb = new StringBuilder();
            if (Scope == ApplicationAnalysisScope.ApplicationScope)
            {
                sb.Append("Application ").Append(this.ApplicationName).AppendLine().AppendLine();

            }
            else if (Scope == ApplicationAnalysisScope.ProjectScope)
            {
                GenerateProjectReport();
            }
            else // ApplicationAnalysisScope.SolutionScope
            {
                string solutionPathname = this.VsSolutionFilePathname;
                if (!String.IsNullOrWhiteSpace(solutionPathname))
                {
                    if (File.Exists(solutionPathname))
                    {
                        VsSolution solution = new VsSolution(solutionPathname);
                        var projects = solution.Projects;
                        int numberOfProjects = 0;
                        int numberOfFiles = 0;
                        sb.Append("Solution ").Append(solution.SolutionPathname).AppendLine().AppendLine();

                        sb.Append(@"Projects within Solution """).Append(solution.Name).Append(@""": ").AppendLine().AppendLine();
                        foreach (var project in projects)
                        {
                            if (!ProjectsProcessed.Contains(project))
                            {
                                ProjectsProcessed.Add(project);
                                numberOfProjects++;
                                sb.Append("Project: ").AppendLine(project.ToString());

                                AddProjectInfor(project, sb, numberOfProjects: ref numberOfProjects, numberOfSourceFiles: ref numberOfFiles);

                                sb.AppendLine();
                            }
                        }

                        sb.Append("Total Projects: ").Append(numberOfProjects);
                        if (IsToIncludeFiles)
                        {
                            sb.Append(",  Total Source-Files: ").Append(numberOfFiles);
                        }
                        sb.AppendLine();

                        FilesystemLib.WriteText(ModuleDependencyReportPath, sb.ToString());

                        OpenLastReport();
                    }
                    else
                    {
                        NotifyUserOfError("That VS solution file does not seem to be present.");
                    }
                }
                else
                {
                    NotifyUserOfMistake("You need to enter a pathname to the Visual Studio solution you want to analyze.");
                }
            }
        }
        #endregion

        #region GenerateFileList
        public void GenerateFileList()
        {
            Save();
            ProjectsProcessed.Clear();
            var sourceFiles = new SortedSet<VsSourceFile>();
            int numberOfProjects = 0;

            if (Scope == ApplicationAnalysisScope.ApplicationScope)
            {
                //int numberOfFiles = 0;
                if (ApplicationSolutions.Count > 0)
                {
                    foreach (var solution in ApplicationSolutions)
                    {
                        sourceFiles.Add(new VsSourceFile(solution.SolutionPathname));

                        var projects = solution.Projects;

                        // Note: I am not bothering to add intermediate files such as .suo, folder .vs, etc.

                        foreach (var project in projects)
                        {
                            var additionalFiles = AddSourceFilesFromProject(project, numberOfProjects: ref numberOfProjects);
                            foreach (var f in additionalFiles)
                            {
                                sourceFiles.Add(f);
                            }
                        }
                    }

                    // We now have the list. Create the report from it.

                    var sb = new StringBuilder();
                    sb.Append("Source-Files in Application ").Append(ApplicationName).AppendLine();
                    GenerateFileReportFromList(sourceFiles, sb);
                    sb.AppendLine();
                    sb.Append("Number of Solutions: ").Append(ApplicationSolutions.Count);
                    sb.Append(", Total Projects: ").Append(numberOfProjects);
                    sb.Append(",  Total Source-Files: ").Append(sourceFiles.Count);
                    sb.AppendLine();

                    FilesystemLib.WriteText(ModuleDependencyReportPath, sb.ToString());

                    OpenLastReport();
                }
                else
                {
                    NotifyUserOfMistake("You need to add at least one Visual Studio solution to this application first.");
                }
            }
            else if (Scope == ApplicationAnalysisScope.SolutionScope)
            {
                string solutionPathname = this.VsSolutionFilePathname;
                if (!String.IsNullOrWhiteSpace(solutionPathname))
                {
                    if (File.Exists(solutionPathname))
                    {
                        VsSolution solution = new VsSolution(solutionPathname);
                        var projects = solution.Projects;

                        sourceFiles.Add(new VsSourceFile(solutionPathname));

                        // Note: I am not bothering to add intermediate files such as .suo, folder .vs, etc.

                        foreach (var project in projects)
                        {
                            var additionalFiles = AddSourceFilesFromProject(project, numberOfProjects: ref numberOfProjects);
                            foreach (var f in additionalFiles)
                            {
                                sourceFiles.Add(f);
                            }
                        }

                        // We now have the list. Create the report from it.

                        var sb = new StringBuilder();
                        sb.Append("Source-Files in Visual Studio Solution ").Append(solution.Name).AppendLine().AppendLine();
                        GenerateFileReportFromList(sourceFiles, sb);
                        sb.AppendLine();
                        sb.Append("Total Projects: ").Append(numberOfProjects);
                        sb.Append(",  Total Source-Files: ").Append(sourceFiles.Count);
                        sb.AppendLine();

                        FilesystemLib.WriteText(ModuleDependencyReportPath, sb.ToString());

                        OpenLastReport();
                    }
                    else
                    {
                        NotifyUserOfError("That VS solution file does not seem to be present.");
                    }
                }
                else
                {
                    NotifyUserOfMistake("You need to enter a pathname to the Visual Studio solution you want to analyze.");
                }
            }
            else if (Scope == ApplicationAnalysisScope.ProjectScope)
            {
                if (!String.IsNullOrWhiteSpace(VsProjectFilePathname))
                {
                    if (File.Exists(VsProjectFilePathname))
                    {
                        VsProject project = new VsProject(VsProjectFilePathname);

                        sourceFiles = AddSourceFilesFromProject(project, numberOfProjects: ref numberOfProjects);


                        // We now have the list. Create the report from it.

                        var sb = new StringBuilder();
                        sb.Append("Source-Files in Visual Studio Project ").Append(project.AssemblyName).AppendLine().AppendLine();

                        GenerateFileReportFromList(sourceFiles, sb);
                        sb.AppendLine();
                        sb.Append("Total Source-Files for this Project: ").Append(sourceFiles.Count);
                        sb.AppendLine();

                        FilesystemLib.WriteText(ModuleDependencyReportPath, sb.ToString());

                        OpenLastReport();
                    }
                    else
                    {
                        NotifyUserOfError("That VS project file does not seem to be present.");
                    }
                }
                else
                {
                    NotifyUserOfMistake("You need to enter a pathname to the VS project you want to analyze.");
                }
            }
        }
        #endregion GenerateFileList

        #region AddSourceFilesFromProject
        private SortedSet<VsSourceFile> AddSourceFilesFromProject( VsProject project, ref int numberOfProjects )
        {
            SortedSet<VsSourceFile> files = new SortedSet<VsSourceFile>();
            ProjectsProcessed.Add(project);
            files.Add(new VsSourceFile(project.Pathname));
            numberOfProjects++;

            foreach (var file in project.SourceFiles)
            {
                files.Add(new VsSourceFile(file.Pathname));
            }
            if (IsToIncludeReferencedProjects)
            {
                foreach (var referencedProject in project.ReferencedProjects)
                {
                    if (!ProjectsProcessed.Contains(referencedProject))
                    {
                        ProjectsProcessed.Add(referencedProject);
                        numberOfProjects++;

                        var additionalFiles = AddSourceFilesFromProject(referencedProject, numberOfProjects: ref numberOfProjects);
                        foreach (var f in additionalFiles)
                        {
                            files.Add(f);
                        }
                    }
                }
            }
            return files;
        }
        #endregion AddSourceFilesFromProject

        #region GenerateFileReportFromList
        private void GenerateFileReportFromList( SortedSet<VsSourceFile> files, StringBuilder sb )
        {
            foreach (var file in files)
            {
                // filename
                if (IsToIncludeFullPaths)
                {
                    sb.Append(file.Pathname);
                }
                else
                {
                    sb.Append(file.Name);
                }

                // last-written
                if (IsToIncludeWhenLastWritten)
                {
                    sb.Append("\t").Append(file.WhenLastWritten);
                }
                sb.AppendLine();
            }
        }
        #endregion GenerateFileReportFromList

        #region GenerateProjectReport
        public void GenerateProjectReport()
        {
            Save();
            if (!String.IsNullOrWhiteSpace(VsProjectFilePathname))
            {
                if (File.Exists(VsProjectFilePathname))
                {
                    VsProject project = new VsProject(VsProjectFilePathname);
                    int numberOfFiles = 0;
                    int numberOfProjects = 1;
                    var sb = new StringBuilder();
                    sb.Append("Project ").Append(project.AssemblyName).AppendLine().AppendLine();

                    // This particular list of projects, is used to track which ones have already been processed - to avoid duplicates.
                    ProjectsProcessed.Add(project);

                    AddProjectInfor(project, sb, numberOfProjects: ref numberOfProjects, numberOfSourceFiles: ref numberOfFiles);

                    ProjectsProcessed.Clear();
                    sb.AppendLine().Append("Number of Projects: " + numberOfProjects);
                    if (IsToIncludeFiles)
                    {
                        sb.Append(",  Total Source-Files: ").Append(numberOfFiles);
                    }
                    sb.AppendLine();

                    FilesystemLib.WriteText(ModuleDependencyReportPath, sb.ToString());

                    OpenLastReport();
                }
                else
                {
                    NotifyUserOfError("That VS project file does not seem to be present.");
                }
            }
            else
            {
                NotifyUserOfMistake("You need to enter a pathname to the VS project you want to analyze.");
            }
        }
        #endregion GenerateProjectReport

        #region GenerateGlobReport
        public void GenerateGlobReport()
        {
            if (String.IsNullOrWhiteSpace(this.XamlPrefixForLocalizableValues) && String.IsNullOrWhiteSpace(this.ResourceManagerKeyPattern))
            {
                VsProject project = new VsProject(VsProjectFilePathname);
                string patterns;
                if (String.IsNullOrWhiteSpace(XamlPrefixForLocalizableValues))
                {
                    patterns = this.ResourceManagerKeyPattern;
                }
                else if (String.IsNullOrWhiteSpace(ResourceManagerKeyPattern))
                {
                    patterns = this.XamlPrefixForLocalizableValues;
                }
                else
                {
                    patterns = this.XamlPrefixForLocalizableValues + "; " + this.ResourceManagerKeyPattern;
                }
                SortedSet<string> keys = XamlScanner.GetAllKeys(rootDirectoryPath: project.Folder, patterns: patterns);
                var sb = new StringBuilder();
                sb.Append("Localizable Keys within Visual Studio Project ").Append(project.Pathname).AppendLine().AppendLine();

                foreach (var key in keys)
                {
                    sb.Append("  key: ");
                    sb.Append(key);
                    sb.AppendLine();
                }

                sb.AppendLine();
                sb.Append("Total Keys for this VS-Project: ").Append(keys.Count);
                sb.AppendLine();

                FilesystemLib.WriteText(ModuleDependencyReportPath, sb.ToString());

                OpenLastReport();
            }
            else
            {
                NotifyUserOfMistake("You need to specify what to scan for.");
            }
        }
        #endregion GenerateGlobReport

        #region GetRemovableDrives
        public static DriveInfo[] GetRemovableDrives()
        {
            List<DriveInfo> driveList = new List<DriveInfo>();

            try
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    if (drive.IsReady == true && drive.DriveType == DriveType.Removable)
                        driveList.Add(drive);
                }
            }
            catch (Exception x)
            {
                LogManager.LogException(x);
            }

            return driveList.ToArray();
        }
        #endregion GetRemovableDrives

        #region LoadProjects
        /// <summary>
        /// When in application-scope and a solution is selected, or in solution-scope,
        /// this command causes the projects that are part of the selected solution to be loaded
        /// and thus visible within the "Projects" listbox.
        /// </summary>
        public void LoadProjects()
        {
            VsSolution solution;
            if (_scope == ApplicationAnalysisScope.ApplicationScope)
            {
                solution = SelectedSolution;
            }
            else
            {
                solution = new VsSolution(VsSolutionFilePathname);
            }
            SolutionProjects.Clear();
            foreach (var project in solution.Projects)
            {
                project.GetInformationFromTheProjectFile();
                SolutionProjects.Add(project);
            }
        }
        #endregion LoadProjects

        public void RequestToChangeReference()
        {
            ChangeReferenceRequested?.Invoke(this, EventArgs.Empty);
        }

        #region SaveCurrentApplicationGraphAs
        public void SaveCurrentApplicationGraphAs( string pathname )
        {
            Logger.LogTrace("SaveCurrentApplicationGraphAs( " + pathname + " ).");

            switch (Scope)
            {
                case ApplicationAnalysisScope.ApplicationScope:
                    break;
                case ApplicationAnalysisScope.SolutionScope:
                    break;
                case ApplicationAnalysisScope.ProjectScope:
                    var project = new VsProject(VsProjectFilePathname);
                    project.SerializeToXML(pathname);
                    break;
            }
        }
        #endregion SaveCurrentApplicationGraphAs

        #region SetCompany
        public void SetCompany()
        {
            OperationResult result = DoToProjects(SetCompanyOf);
            string messageToUser;
            if (result.WasAllSuccessful)
            {
                if (result.NumberThatWereSuccessful == 0)
                {
                    messageToUser = "This did not set the company-name of any project." + Environment.NewLine + result.Reason;
                    NotifyUserOfError(messageToUser);
                }
                else
                {
                    if (result.NumberThatWereSuccessful == 1)
                    {
                        messageToUser = "The company-name has been set for this project.";
                    }
                    else
                    {
                        messageToUser = "The company-name has been set for " + result.NumberThatWereSuccessful + " projects.";
                    }
                    NotifyUser(messageToUser);
                }
            }
            else
            {
                messageToUser = "This failed for " + result.NumberOfFailures + " projects." + Environment.NewLine + result.Reason;
                NotifyUserOfError(messageToUser);
            }
        }
        #endregion SetCompany

        #region SetCopyright
        public void SetCopyright()
        {
            OperationResult result = DoToProjects(SetCopyrightOf);
            string messageToUser;
            if (result.WasAllSuccessful)
            {
                if (result.NumberThatWereSuccessful == 0)
                {
                    messageToUser = "This did not set the copyright of any project." + Environment.NewLine + result.Reason;
                    NotifyUserOfError(messageToUser);
                }
                else
                {
                    if (result.NumberThatWereSuccessful == 1)
                    {
                        messageToUser = "The copyright has been set for this project.";
                    }
                    else
                    {
                        messageToUser = "The copyright has been set for " + result.NumberThatWereSuccessful + " projects.";
                    }
                    NotifyUser(messageToUser);
                }
            }
            else
            {
                messageToUser = "This failed for " + result.NumberOfFailures + " projects." + Environment.NewLine + result.Reason;
                NotifyUserOfError(messageToUser);
            }
        }
        #endregion SetCopyright

        #region SetGlobalizationParametersToDefaults
        public void SetGlobalizationParametersToDefaults()
        {
            this.XamlPrefixForLocalizableValues = "lex:Loc";
            this.ResourceManagerKeyPattern = "?";  //CBL
        }
        #endregion SetGlobalizationParametersToDefaults

        #region SetNetFrameworkVersion
        public void SetNetFrameworkVersion()
        {
            OperationResult result = DoToProjects(SetNetFrameworkVersionOf);
            string messageToUser;
            if (result.WasAllSuccessful)
            {
                if (result.NumberThatWereSuccessful == 0)
                {
                    messageToUser = "This did not set the company-name of any project." + Environment.NewLine + result.Reason;
                    NotifyUserOfError(messageToUser);
                }
                else
                {
                    if (result.NumberThatWereSuccessful == 1)
                    {
                        messageToUser = "The company-name has been set for this project.";
                    }
                    else
                    {
                        messageToUser = "The company-name has been set for " + result.NumberThatWereSuccessful + " projects.";
                    }
                    NotifyUser(messageToUser);
                }
            }
            else
            {
                messageToUser = "This failed for " + result.NumberOfFailures + " projects." + Environment.NewLine + result.Reason;
                NotifyUserOfError(messageToUser);
            }
        }
        #endregion SetNetFrameworkVersion

        public void SetProjectVersions()
        {

        }

        #region FindAllNugetReferences
        /// <summary>
        /// Handle the "Find All" button-selection which ignores the content within 'Nuget Reference Text'
        /// and simply lists all Nuget References found within the VS-Project.
        /// </summary>
        public void FindAllNugetReferences()
        {
            Debug.WriteLine("FindAllNugetReferences");

            OperationResult result = DoToProjects(FindPackageSpecifications);
            string messageToUser;
            if (result.WasAllSuccessful)
            {
                if (result.NumberThatWereSuccessful == 0)
                {
                    messageToUser = "This did not find that package-specification in any of those projects." + Environment.NewLine + result.Reason;
                    NotifyUserOfError(messageToUser);
                }
                else
                {
                    if (result.NumberThatWereSuccessful == 1)
                    {
                        messageToUser = "The package-specification was found for this project.";
                    }
                    else
                    {
                        messageToUser = "The package-specification was found for " + result.NumberThatWereSuccessful + " projects.";
                    }
                    NotifyUser(messageToUser);
                }
            }
            else
            {
                messageToUser = "This failed for " + result.NumberOfFailures + " projects." + Environment.NewLine + result.Reason;
                NotifyUserOfError(messageToUser);
            }
        }
        #endregion FindAllNugetReferences

        #region FindThisNugetReference
        /// <summary>
        /// Handle the "Find" button-selection which considers the content within 'Nuget Reference Text'
        /// and lists all Nuget References found within the VS-Project that match it.
        /// </summary>
        public void FindThisNugetReference()
        {
            Debug.WriteLine("FindThisNugetReference");

            string referenceText = NugetReference;
            if (string.IsNullOrWhiteSpace(referenceText))
            {
                NotifyUserOfError("You have to enter a Nuget Package Reference to look for.");
                return;
            }

            OperationResult result = DoToProjects(FindPackageSpecifications);
            string messageToUser;
            if (result.WasAllSuccessful)
            {
                if (result.NumberThatWereSuccessful == 0)
                {
                    messageToUser = "This did not find that package-specification in any of those projects." + Environment.NewLine + result.Reason;
                    NotifyUserOfError(messageToUser);
                }
                else
                {
                    if (result.NumberThatWereSuccessful == 1)
                    {
                        messageToUser = "The package-specification was found for this project.";
                    }
                    else
                    {
                        messageToUser = "The package-specification was found for " + result.NumberThatWereSuccessful + " projects.";
                    }
                    NotifyUser(messageToUser);
                }
            }
            else
            {
                messageToUser = "This failed for " + result.NumberOfFailures + " projects." + Environment.NewLine + result.Reason;
                NotifyUserOfError(messageToUser);
            }
        }
        #endregion FindThisNugetReference

        private OperationResult FindPackageSpecifications( VsProject project )
        {
            var result = new OperationResult();
            result.WasAllSuccessful = true;
            var NugetReferences = project.GetPackageReferencesFromTheProjectFile();

            Debug.WriteLine($"Found {NugetReferences.Count} Nuget references within {project.AssemblyName}.");
            foreach (var nugetReference in NugetReferences)
            {
                Debug.WriteLine($"{nugetReference}");
            }
            return result;
        }

        public void SetNugetTextReferences()
        {
            Debug.WriteLine("SetNugetTextReferences");

        }

        #region LoadReferencesAccordingToSelectedProjects
        public void LoadReferencesAccordingToSelectedProjects()
        {
            Debug.WriteLine("LoadReferencesAccordingToSelectedProjects");
            this.ProjectReferencesUsed.Clear();
            //VsProject project = new VsProject( VsProjectFilePathname );
            //project.GetInformationFromTheProjectFile();
            //foreach (var referencedProject in project.ReferencedProjects)
            //{
            //    ReferencesUsed.Add( referencedProject.Pathname );
            //}

            OperationResult result = DoToProjects(project =>
            {
                var r = new OperationResult();
                project.GetInformationFromTheProjectFile();
                foreach (var referencedProject in project.ReferencedProjects)
                {
                    ProjectReferencesUsed.Add(referencedProject);
                }
                r.WasAllSuccessful = true;
                return r;
            });
        }
        #endregion LoadReferencesAccordingToSelectedProjects


        //<ProjectReference Include = "..\..\..\Libs\BaseLib\BaseLib_451.csproj" >
        // <Project>{bc955177-e070-4c54-9ad1-e7cac1c8fd3f}</Project>
        //   <Name>BaseLib_451</Name>
        //</ProjectReference>

        // Issues:
        // 1. That GUID exists as the "ProjectGuid" within the actual referenced project.
        // 2. Those are relative paths.

        #region SetToNewReference
        public void SetToNewReference()
        {
            //CBL  Still implementing
            VsProject newProjectToReference = new VsProject(ReferenceToChangeTo);
            SelectedProjectReference.GetInformationFromTheProjectFile();
            Logger.LogDebug("SetToNewReference, SelectedProjectReference = " + this.SelectedProjectReference.AssemblyName + " with GUID=" + SelectedProjectReference.Guid + " and new ref = " + ReferenceToChangeTo + " with GUID of " + newProjectToReference.Guid);
            // Make a backup copy of any projects we are going to change.
            string originalPathname = VsProjectFilePathname;
            string pathWithoutExt = FileStringLib.GetPathnameWithoutExtension(originalPathname);
            string newPathname = pathWithoutExt + "_NEW.csproj";

            //CBL Temporarily, just do it to the one selected project.

            //VsProject oldReferencedProject = new VsProject( originalPathname );
            //oldReferencedProject.GetInformationFromTheProjectFile();

            // Read lines from the original project-file, and write them to the new file {path}_NEW.csproj...
            VsProject selectedProject = new VsProject(VsProjectFilePathname);
            string directoryOfProject = selectedProject.Folder;
            string pathOfProjectToReplace = SelectedProjectReference.Pathname;
            bool wasFound = false;
            using (var reader = new StreamReader(originalPathname))
            using (var writer = new StreamWriter(newPathname))
            {
                String lineOfText, text;
                while ((lineOfText = reader.ReadLine()) != null)
                {
                    if (lineOfText.Contains("<ProjectReference Include="))
                    {
                        string relativePath = lineOfText.PartBetween("Include=", ">").WithoutDoubleQuotes();

                        // For GetFullPath to work, we have to set the current-directory to that of this project.
                        // CBL  Clarify?
                        string actualPath = FileStringLib.GetAbsolutePath(pathToMakeAbsolute: relativePath, basePathToWhichToMakeAbsoluteTo: directoryOfProject);

                        if (actualPath.Equals(pathOfProjectToReplace))
                        {
                            Logger.LogTrace("Found the project-reference to replace.");
                            // Let's check it against the following GUID also.
                            lineOfText = reader.ReadLine();
                            if (lineOfText.Contains("<Project>{"))
                            {
                                string guidOnThisLine = lineOfText.PartBetween(pattern1: "<Project>{", pattern2: "}");
                                if (guidOnThisLine.Equals(SelectedProjectReference.Guid, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    Logger.LogTrace("Matched the GUID");

                                    // Convert the resplacement referenced-proj path from absolute to relative.
                                    string newRelativePath = FileStringLib.GetRelativePath(pathToWhichToMakeRelativeTo: directoryOfProject, pathToMakeRelative: newProjectToReference.Pathname);

                                    string line1 = "    <ProjectReference Include=\"" + newRelativePath + "\">";
                                    writer.WriteLine(line1);
                                    string line2 = "      <Project>{" + newProjectToReference.Guid + "}</Project>";
                                    writer.WriteLine(line2);
                                    lineOfText = reader.ReadLine();
                                    string newProjectName = FileStringLib.GetFileNameWithoutExtension(newProjectToReference.Pathname);
                                    string line3 = "      <Name>" + newProjectName + "</Name>";
                                    writer.WriteLine(line3);
                                    lineOfText = reader.ReadLine();
                                    string line4 = "    </ProjectReference>";
                                    writer.WriteLine(line4);
                                    wasFound = true;
                                    continue;
                                }
                                else
                                {
                                    Logger.LogTrace("But failed to match the GUID");
                                }
                            }
                        }
                        //var project = new VsProject( actualPath );
                    }
                    //if (lineOfText.Contains( "ProgramVersion" ))
                    //{
                    //    text = @"    <add key=""ProgramVersion"" value=""" + programVersion + @"""/>";
                    //    wasFound = true;
                    //}
                    //else
                    {
                        text = lineOfText;
                    }
                    writer.WriteLine(text);
                }
            }
            if (!wasFound)
            {
                Logger.LogError("Failed to find project-reference {0} within project {1}.", SelectedProjectReference, VsProjectFilePathname);
            }
        }
        #endregion SetToNewReference

        #region Save
        /// <summary>
        /// Store the current view-model state to the user-settings object.
        /// </summary>
        public void Save()
        {
            UserSettings.ApplicationName = this.ApplicationName;
            UserSettings.CopyrightNotice = ProjectCopyrightNotice;
            UserSettings.CopySourceDestinationPath = this.CopySourceDestinationPath;
            UserSettings.DefaultRootFolderForVersionStateSnapshots = this.DefaultRootFolderForVersionStateSnapshots;
            UserSettings.IsToCleanVsDirectoriesAlso = this.IsToCleanVsDirectoriesAlso;
            UserSettings.IsToExcludeMyOwnVsDirectory = this.IsToExcludeMyOwnVsDirectory;
            UserSettings.IsToIncludeFiles = this.IsToIncludeFiles;
            UserSettings.IsToIncludeFullPaths = this.IsToIncludeFullPaths;
            UserSettings.IsToIncludeReferencedProjects = this.IsToIncludeReferencedProjects;
            UserSettings.IsToIncludeTestProjects = this.IsToIncludeTestProjects;
            UserSettings.IsToIncludeVersions = this.IsToIncludeVersions;
            UserSettings.IsToIncludeWhenLastWritten = this.IsToIncludeWhenLastWritten;
            UserSettings.MetricsComparisonSourceFolder1 = this.MetricsComparisonSourceFolder1;
            UserSettings.MetricsComparisonSourceFolder2 = this.MetricsComparisonSourceFolder2;
            UserSettings.MetricsComparisonReportDestinationPath = this.MetricsComparisonReportDestinationPath;
            UserSettings.ModuleDependencyReportPath = this.ModuleDependencyReportPath;
            UserSettings.ProjectCompanyName = this.ProjectCompanyName;
            UserSettings.ReferenceToChangeTo = this.ReferenceToChangeTo;
            UserSettings.ResourceManagerKeyPattern = this.ResourceManagerKeyPattern;
            UserSettings.Scope = this.Scope;
            UserSettings.SelectedTabItem = this.SelectedTabItem;
            UserSettings.SpreadsheetFilePathname = this.SpreadsheetFilePathname;
            UserSettings.TargetNetFrameworkVersion = this.TargetNetFrameworkVersion;
            UserSettings.VsProjectFilePathname = this.VsProjectFilePathname;
            UserSettings.VsSolutionFilePathname = this.VsSolutionFilePathname;
            UserSettings.XamlPrefixForLocalizableValues = this.XamlPrefixForLocalizableValues;

            UserSettings.ApplicationSolutionPaths.Clear();
            foreach (var solution in ApplicationSolutions)
            {
                UserSettings.ApplicationSolutionPaths.Add(solution.SolutionPathname);
            }
        }
        #endregion

        public void Exit()
        {
            // In Metro, this would be CoreApplication.Exit
            Application.Current.MainWindow.Close();
        }

        #endregion public methods

        #region internal implementation

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

        #region private methods

        #region AddProjectInfor
        private void AddProjectInfor( VsProject project, StringBuilder sb, ref int numberOfProjects, ref int numberOfSourceFiles, string indentation = "  " )
        {
            // Only display the Title if it is non-empty and not the same as the assembly-name.
            if (!String.IsNullOrWhiteSpace(project.Title) && !project.Title.Equals(project.AssemblyName))
            {
                sb.Append(indentation).Append("Title: ").Append(project.Title).AppendLine();
            }
            sb.Append(indentation).Append("Guid: ").Append(project.Guid).AppendLine();
            sb.Append(indentation).Append("Description: ").Append(project.Description).AppendLine();
            if (IsToIncludeFullPaths)
            {
                sb.Append(indentation).Append("Project-file Pathname: ").Append(project.Pathname).AppendLine();
            }
            if (IsToIncludeVersions)
            {
                sb.Append(indentation).Append("Assembly Version: ").Append(project.AssemblyVersion).AppendLine();
                sb.Append(indentation).Append("File Version: ").Append(project.FileVersion).AppendLine();
            }
            if (IsToIncludeWhenLastWritten)
            {
                sb.Append(indentation).Append("LastWritten: ").AppendFormat("{0:yyyy/M/dd hh:mmtt}", project.WhenLastWritten).AppendLine();
            }
            if (IsToIncludeFiles)
            {
                foreach (var file in project.SourceFiles)
                {
                    numberOfSourceFiles++;
                    sb.Append("      ").Append("file: ");
                    if (IsToIncludeFullPaths)
                    {
                        sb.Append(file.Pathname);
                    }
                    else
                    {
                        sb.Append(file.Name);
                    }
                    if (IsToIncludeVersions)
                    {
                        if (file.FileVersion != null)
                        {
                            sb.Append(", FileVersion = ").Append(file.FileVersion);
                        }
                    }
                    if (IsToIncludeWhenLastWritten)
                    {
                        sb.Append(", LastWritten = ").AppendFormat("{0:yyyy/M/dd hh:mmtt}", file.WhenLastWritten);
                    }
                    sb.AppendLine();
                }
                sb.AppendLine("    Source-Files for this project: " + project.SourceFiles.Count);
            }
            if (IsToIncludeReferencedProjects)
            {
                foreach (var referencedProject in project.ReferencedProjects)
                {
                    if (!ProjectsProcessed.Contains(referencedProject))
                    {
                        ProjectsProcessed.Add(referencedProject);

                        numberOfProjects++;
                        sb.AppendLine();
                        sb.Append("Referenced-Project ").Append(referencedProject.AssemblyName).AppendLine();
                        AddProjectInfor(referencedProject, sb, numberOfProjects: ref numberOfProjects, numberOfSourceFiles: ref numberOfSourceFiles);
                    }
                }
            }
        }
        #endregion AddProjectInfor

        #region RefactorLex
        public void RefactorLex()
        {
            string lexNamespace = "lex:";
            int numberOfFilesChanged = 0;
            Logger.LogInfo("begin RefactorLex - " + this.Scope);

            OperationResult result = DoToProjects(project =>
            {
                var r = new OperationResult();
                project.GetInformationFromTheProjectFile();

                // Remove any lines from the .XAML files that contain lex:LocalizeDictionary.DesignCulture="en"
                var listOfFilesChanged = project.RefactorLexForAllXamlFiles(newResourceDictionaryValue: "LuvivaUI.Resources", isToKeepOriginal: false);
                numberOfFilesChanged += listOfFilesChanged.Length;

                string msg;
                if (numberOfFilesChanged == 0)
                {
                    msg = "For project " + project.Pathname + " no XAML files changed.";
                }
                else
                {
                    msg = "For project " + project.Pathname + " " + numberOfFilesChanged + " XAML files changed";
                }
                Logger.LogInfo(msg);

                r.WasAllSuccessful = true;
                return r;
            });


            if (numberOfFilesChanged > 0)
            {
                NotifyUser("Found the lex attribute in " + numberOfFilesChanged + " .XAML files.");
            }
            else
            {
                NotifyUser("Did not find the lex attribute in any of the .XAML files.");
            }


            //VsProject project = new VsProject( VsProjectFilePathname );
            //string patterns;
            //if (String.IsNullOrWhiteSpace( XamlPrefixForLocalizableValues ))
            //{
            //    patterns = this.ResourceManagerKeyPattern;
            //}
            //else if (String.IsNullOrWhiteSpace( ResourceManagerKeyPattern ))
            //{
            //    patterns = this.XamlPrefixForLocalizableValues;
            //}
            //else
            //{
            //    patterns = this.XamlPrefixForLocalizableValues + "; " + this.ResourceManagerKeyPattern;
            //}
            //SortedSet<string> keys = XamlScanner.GetAllKeys( rootDirectoryPath: project.Folder, patterns: patterns );
            //var sb = new StringBuilder();
            //sb.Append( "Localizable Keys within Visual Studio Project " ).Append( project.Pathname ).AppendLine().AppendLine();

            //foreach (var key in keys)
            //{
            //    sb.Append( "  key: " );
            //    sb.Append( key );
            //    sb.AppendLine();
            //}

            //sb.AppendLine();
            //sb.Append( "Total Keys for this VS-Project: " ).Append( keys.Count );
            //sb.AppendLine();

            //FilesystemLib.WriteText( ModuleDependencyReportPath, sb.ToString() );

            //OpenLastReport();
        }
        #endregion RefactorLex

        #region DeleteDesignCultureAttributesFromAllXamlFiles
        public int DeleteDesignCultureAttributesFromAllXamlFiles()
        {
            int numberOfFilesChanged = 0;
            Logger.LogInfo("begin DeleteDesignCultureAttributesFromAllXamlFiles - " + this.Scope);

            OperationResult result = DoToProjects(project =>
            {
                var r = new OperationResult();
                project.GetInformationFromTheProjectFile();

                // Remove any lines from the .XAML files that contain lex:LocalizeDictionary.DesignCulture="en"
                var listOfFilesChanged = project.DeleteLineFromAllXamlFilesThatContainDesignCulture();
                numberOfFilesChanged += listOfFilesChanged.Length;

                string msg;
                if (numberOfFilesChanged == 0)
                {
                    msg = "For project " + project.Pathname + " no XAML files changed.";
                }
                else
                {
                    msg = "For project " + project.Pathname + " " + numberOfFilesChanged + " XAML files changed";
                }
                Logger.LogInfo(msg);

                r.WasAllSuccessful = true;
                return r;
            });


            if (numberOfFilesChanged > 0)
            {
                NotifyUser("Found the DesignCulture attribute in " + numberOfFilesChanged + " .XAML files.");
            }
            else
            {
                NotifyUser("Did not find the DesignCulture attribute in any of the .XAML files.");
            }

            return numberOfFilesChanged;
        }
        #endregion DeleteDesignCultureAttributesFromAllXamlFiles

        #region DoToProjects
        /// <summary>
        /// A helper-method that executes the given Func against all applicable VS projects, depending upon the application-scope that is currently selected.
        /// </summary>
        /// <param name="whatToDo">the function to perform against the chosen project(s)</param>
        /// <returns>a ProjectOperationResult that contains the results of the operation</returns>
        /// <remarks>
        /// 
        /// </remarks>
        private OperationResult DoToProjects( Func<VsProject, OperationResult> whatToDo )
        {
            var sb = new StringBuilder();
            var result = new OperationResult();

            switch (this.Scope)
            {
                case ApplicationAnalysisScope.ApplicationScope:
                    foreach (var vsSolution in ApplicationSolutions)
                    {
                        if (File.Exists(vsSolution.SolutionPathname))
                        {
                            VsSolution solution = new VsSolution(this.VsSolutionFilePathname);
                            foreach (var project in solution.Projects)
                            {
                                result.NumberThatWereProcessed++;

                                OperationResult r = whatToDo(project);

                                if (r.WasAllSuccessful)
                                {
                                    result.NumberThatWereSuccessful++;
                                }
                                else
                                {
                                    result.WasAllSuccessful = false;
                                    sb.AppendLine(r.Reason);
                                }
                            }
                        }
                        else
                        {
                            sb.AppendLine("The VS solution " + vsSolution.Name + " does not seem to be present.");
                            result.WasAllSuccessful = false;
                            break;
                        }
                    }
                    result.Reason = sb.ToString();
                    break;

                case ApplicationAnalysisScope.SolutionScope:
                    if (File.Exists(this.VsSolutionFilePathname))
                    {
                        VsSolution solution = new VsSolution(this.VsSolutionFilePathname);
                        foreach (var project in solution.Projects)
                        {
                            result.NumberThatWereProcessed++;

                            OperationResult r = whatToDo(project);

                            if (r.WasAllSuccessful)
                            {
                                result.NumberThatWereSuccessful++;
                            }
                            else
                            {
                                result.WasAllSuccessful = false;
                                sb.AppendLine(r.Reason);
                            }
                        }
                        result.Reason = sb.ToString();
                    }
                    else
                    {
                        result.NumberThatWereProcessed = 0;
                        result.NumberThatWereSuccessful = 0;
                        result.Reason = "That VS solution file " + VsProjectFilePathname + " does not seem to be present.";
                        result.WasAllSuccessful = false;
                    }
                    break;

                case ApplicationAnalysisScope.ProjectScope:
                    result.NumberThatWereProcessed = 1;
                    if (File.Exists(VsProjectFilePathname))
                    {
                        VsProject project = new VsProject(VsProjectFilePathname);

                        OperationResult r = whatToDo(project);

                        result.Reason = r.Reason;
                        result.WasAllSuccessful = r.WasAllSuccessful;
                        if (r.WasAllSuccessful)
                        {
                            result.NumberThatWereSuccessful = 1;
                        }
                    }
                    else
                    {
                        result.NumberThatWereSuccessful = 0;
                        result.Reason = "That VS project file " + VsProjectFilePathname + " does not seem to be present.";
                        result.WasAllSuccessful = false;
                    }
                    break;
            }
            return result;
        }
        #endregion DoToProjects

        private void NotifyUser( string message )
        {
            // Raise the appropriate event, so that the view may respond to it.
            UserNotificationRequested?.Invoke(this, new UserNotificationEventArgs(message: message, isWarning: false, isError: false));
        }

        private void NotifyUserOfError( string message )
        {
            // Raise the appropriate event, so that the view may respond to it.
            UserNotificationRequested?.Invoke(this, new UserNotificationEventArgs(message: message, isWarning: false, isError: true));
        }

        private void NotifyUserOfMistake( string message )
        {
            // Raise the appropriate event, so that the view may respond to it.
            UserNotificationRequested?.Invoke(this, new UserNotificationEventArgs(message: message, isWarning: true, isError: false, isUserMistake: true));
        }

        private void OpenLastReport()
        {
            OpenLastReportRequested?.Invoke(this, EventArgs.Empty);
        }

        private void SelectOutputPath()
        {
            SelectOutputPathRequested?.Invoke(this, EventArgs.Empty);
        }

        private void SelectRootFolderForHistory()
        {
            SelectRootFolderForHistoryRequested?.Invoke(this, EventArgs.Empty);
        }

        private void SelectVsProject()
        {
            SelectProjectRequested?.Invoke(this, EventArgs.Empty);
        }

        private void SelectVsSolution()
        {
            SelectSolutionRequested?.Invoke(this, EventArgs.Empty);
        }

        private OperationResult SetCompanyOf( VsProject project )
        {
            var result = new OperationResult();
            try
            {
                string reason;
                bool isChanged = project.SetCompany(ProjectCompanyName, reason: out reason);

                result.NumberThatWereProcessed = 1;
                if (isChanged)
                {
                    result.WasAllSuccessful = true;
                    result.NumberThatWereSuccessful = 1;
                }
                else
                {
                    result.Reason = reason;
                }
            }
            catch (Exception x)
            {
                result.Reason = x.Message;
            }
            return result;
        }

        private OperationResult SetCopyrightOf( VsProject project )
        {
            var result = new OperationResult();
            try
            {
                string reason;
                bool isChanged = project.SetCompany(ProjectCopyrightNotice, reason: out reason);

                result.NumberThatWereProcessed = 1;
                if (isChanged)
                {
                    result.WasAllSuccessful = true;
                    result.NumberThatWereSuccessful = 1;
                }
                else
                {
                    result.Reason = reason;
                }
            }
            catch (Exception x)
            {
                result.Reason = x.Message;
            }
            return result;
        }

        private OperationResult SetNetFrameworkVersionOf( VsProject project )
        {
            var result = new OperationResult();
            //CBL

            return result;
        }

        private void SetOptionsToDefaults()
        {
            string myDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            DefaultRootFolderForVersionStateSnapshots = Path.Combine(myDocumentsFolder, "VsDevTool", "VersionHistory");
            IsToCleanVsDirectoriesAlso = false;
            IsToExcludeMyOwnVsDirectory = true;
            SetUxToDefaultsRequested?.Invoke(this, EventArgs.Empty);
        }

        #endregion private methods

        #region fields

        private UiCommand _addResourceFileToSetsCommand;
        private UiCommand _addSolutionToApplicationCommand;
        private UiCommand _changeReferenceCommand;
        private UiCommand _cleanAllArtifactsCommand;
        private UiCommand _clearShadowCacheCommand;
        private UiCommand _compareCodeBasesAndGenerateReportCommand;
        private UiCommand _compareStructureSnapshotsCommand;
        private UiCommand _copyReleaseToStageCommand;
        private UiCommand _copySourceCommand;
        private UiCommand _deleteDesignCultureAttributesCommand;
        private UiCommand _deployReleaseGraphCommand;
        private UiCommand _editOptionsCommand;
        private UiCommand _generateDepGraphCommand;
        private UiCommand _generateFileListCommand;
        private UiCommand _generateGlobReportCommand;
        private UiCommand _loadProjectsCommand;
        private UiCommand _openLastReportCommand;
        private UiCommand _refactorLexCommand;
        private UiCommand _removeSolutionFromApplicationCommand;
        private UiCommand _saveApplicationStructureCommand;
        private UiCommand _selectAssemblyToReferenceCommand;
        private UiCommand _selectCopySourceDestinPathCommand;
        private UiCommand _selectMetricsComparisonSourceFolder1Command;
        private UiCommand _selectMetricsComparisonSourceFolder2Command;
        private UiCommand _selectMetricsComparisonReportDestinationPathCommand;
        private UiCommand _selectRootFolderForHistoryCommand;
        private UiCommand _selectVsProjectCommand;
        private UiCommand _selectVsSolutionCommand;
        private UiCommand _selectOutputPathCommand;
        private UiCommand _setCompanyNameCommand;
        private UiCommand _setCopyrightCommand;
        private UiCommand _setGlobParametersToDefaultsCommand;
        private UiCommand _setNetFrameworkVersionCommand;
        private UiCommand _setOptionsToDefaultsCommand;
        private UiCommand _setProjectVersionsCommand;
        private UiCommand _setToNewReferenceCommand;
        private UiCommand _showHelpAboutMeCommand;
        private UiCommand _exitCommand;

        /// <summary>
        /// This denotes the name of the Application that is currently being looked at.
        /// </summary>
        private string _applicationName;

        /// <summary>
        /// This is the collection of VS-solutions that are contained within this 'application'.
        /// Applies only when in Application-Scope.
        /// </summary>
        private ObservableCollection<VsSolution> _applicationSolutions;

        /// <summary>
        /// For when the user wants to copy the collection of source-code files to some other location,
        /// this directory-path denotes the destination of that copy-operation.
        /// </summary>
        private string _copySourceDestinationPath;

        /// <summary>
        /// This is the directory-path to use for storing the application version-histories by default.
        /// The default value is under My Documenets\VsDevTool.
        /// </summary>
        private string _defaultRootFolderForVersionStateSnapshots;

        /// <summary>
        /// This string denotes the filesystem-path upon which the installer-drive has been detected, such as "F:".
        /// </summary>
        private string _installerDrivePath;

        /// <summary>
        /// This is the one, singleton instance of this class.
        /// </summary>
        private static ApplicationViewModel _instance;

        /// <summary>
        /// This flag indicates whether an 'installer' removable-drive has been detected as being attached.
        /// </summary>
        private bool _isInstallerDriveDetected;

        /// <summary>
        /// This dictates whether, when CleanArtifactsOfProject is called,
        /// to also clean-out any .vs folders.
        /// Default is false.
        /// </summary>
        private bool _isToCleanVsDirectoriesAlso;

        /// <summary>
        /// This dictates whether, when CleanArtifactsOfProject is called,
        /// to skip that Visual Studio project which represents this program itself (VsDevTool).
        /// Default is true.
        /// </summary>
        private bool _isToExcludeMyOwnVsDirectory = true;

        /// <summary>
        /// This flag dictates whether to include all the files of each project, within the report,
        /// as opposed to including only the projects.
        /// </summary>
        private bool _isToIncludeFiles;

        /// <summary>
        /// This flag dictates whether to include the full pathnames of all files within the report.
        /// </summary>
        private bool _isToIncludeFullPaths;

        /// <summary>
        /// This flag dictates whether the report upon a given VS-Project is to include the other projects that it references.
        /// Default is true.
        /// </summary>
        private bool _isToIncludeReferencedProjects = true;

        /// <summary>
        /// This flag dictates whether the reports on a VS-Solution is to include VS-Projects that are Test projects,
        /// as identified by having ".Test*" within the name.  Default is false.
        /// </summary>
        private bool _isToIncludeTestProjects;

        /// <summary>
        /// This flag dictates whether to include the file and assbly versions within the report.
        /// </summary>
        private bool _isToIncludeVersions;

        /// <summary>
        /// This flag dictates whether to include the time at which each file was last written within the report.
        /// </summary>
        private bool _isToIncludeWhenLastWritten;

        /// <summary>
        /// This string denotes the input folder-path 1 for the code-metrics comparison report.
        /// </summary>
        private string _metricsComparisonSourceFolder1;

        /// <summary>
        /// This string denotes the input folder-path 2 for the code-metrics comparison report.
        /// </summary>
        private string _metricsComparisonSourceFolder2;

        /// <summary>
        /// This string denotes the output file-pathname for the code-metrics comparison report.
        /// </summary>
        private string _metricsComparisonReportDestinationPath;

        /// <summary>
        /// This string denotes the output-pathname to put the module-dependency report into.
        /// </summary>
        private string _moduleDependencyReportPath;

        /// <summary>
        /// This string denotes the state of the currently-running operation,
        /// as it would be presented to the end-user.
        /// </summary>
        private string _operationStatus;

        /// <summary>
        /// This string denotes the 'program-version' that this program wants to label itself as,
        /// as it would be presented to the end-user.
        /// </summary>
        private string _programVersion;

        /// <summary>
        /// This denotes the text to set for the company-name property within a given project's AssemblyINfo.cs files.
        /// </summary>
        private string _projectCompanyName;

        /// <summary>
        /// This denotes the text of what to set the Copyright-Notice to, wihin any given project's AssemblyInfo.cs file.
        /// </summary>
        private string _projectCopyrightNotice;

        private List<VsProject> _projectsProcessed;

        /// <summary>
        /// This denotes the last value used to specify what to change the project-reference to,
        /// within the Change Reference dialog.
        /// </summary>
        private string _referenceToChangeTo;

        /// <summary>
        /// The text-pattern to scan for when looking for dynamically-composed strings to be globalized.
        /// </summary>
        private string _resourceManagerKeyPattern;

        /// <summary>
        /// This enum-variable dictates which scale we wish to do our analysis at - Application, Solution, or Project.
        /// </summary>
        private ApplicationAnalysisScope _scope;

        /// <summary>
        /// This is the VSProject within the Projects listbox that is to be regarded as currently-selected.
        /// </summary>
        private VsProject _selectedProject;

        /// <summary>
        /// This, when non-null, indicates the VS-solution within the listbox that is to be regarded as currently-selected.
        /// Applies only when in Application-Scope.
        /// </summary>
        private VsSolution _selectedSolution;

        /// <summary>
        /// This integer reflects the index of the tab-item that is currently selected within the tab-control within the UX.
        /// The purpose of this is to restore that selection when this program is next launched.
        /// </summary>
        private int _selectedTabItem = 2;

        /// <summary>
        /// This is the collection of VS-projects that comprise the selected VS-solution,
        /// to serve as the ItemsSource for the "Projects" listbox.
        /// Applies when in application-scope or solution-scope.
        /// </summary>
        private ObservableCollection<VsProject> _solutionProjects;

        /// <summary>
        /// This is the pathname of the Excel spreadsheet file for importing resource-strings from.
        /// </summary>
        private string _spreadsheetFilePathname;

        /// <summary>
        /// This denotes the version of the .NET Framework that is to be targeted.
        /// </summary>
        private NetFrameworkVersion _targetNetFrameworkVersion;

        /// <summary>
        /// This string denotes the pathname of the Visual Studio project that the user wishes to analyze,
        /// which is a distinct operation from analyzing a solution or application.
        /// </summary>
        private string _vsProjectFilePathname;

        /// <summary>
        /// This string denotes the pathname of the Visual Studio solution file that is currently targeted for processing.
        /// </summary>
        private string _vsSolutionFilePathname;

        /// <summary>
        /// This DateTime denotes when the Visual Studio solution was most-recently analyzed by this program.
        /// </summary>
        private DateTime _whenLastAnalyzed;

        /// <summary>
        /// This DateTime denotes when the application structure was last analyzed and saved to storage as a snapshot.
        /// </summary>
        private DateTime _whenWasLastStructureSnapshot;

        /// <summary>
        /// This is the text-pattern to scan the XAML source-code files for, when compiling a list of all localizable values.
        /// </summary>
        private string _xamlPrefixForLocalizableValues = "lex:Loc";

        #endregion fields

        #endregion internal implementation
    }
}
