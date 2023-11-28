using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using Hurst.BaseLibWpf;
using Hurst.LogNut.Util;


namespace VsDevTool.DomainModels
{
    /// <summary>
    /// This class holds the options that are set by the user for this program.
    /// </summary>
    public class VsDevToolUserSettings : WpfUserSettings
    {
        #region public properties

        #region The
        /// <summary>
        /// Get the one instance of the VsDevToolUserSettings class.
        /// </summary>
        public static VsDevToolUserSettings The
        {
            get
            {
                if (_userSettings == null)
                {
                    _userSettings = GetInstance<VsDevToolUserSettings>( App.The, isToSaveLocation: true, isToSaveSize: true );
                }
                // This extra-step ensures this property is set, even if the settings-object was last deserialized without this value.
                _userSettings.PositionOfMainWindow.IsSavingSize = true;
                return _userSettings;
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
                    IsChanged = true;
                }
            }
        }
        #endregion

        #region ApplicationSolutions
        /// <summary>
        /// Get or set the collection of VS-solution paths that are contained within this 'application'.
        /// Applies only when in Application-Scope.
        /// </summary>
        [XmlArrayItem( "Path", typeof( string ) )]
        [XmlArray( "ApplicationSolutionPaths" )]
        public List<string> ApplicationSolutionPaths
        {
            get
            {
                if (_applicationSolutionPaths == null)
                {
                    _applicationSolutionPaths = new List<string>();
                }
                return _applicationSolutionPaths;
            }
            set
            {
                if (value != _applicationSolutionPaths)
                {
                    _applicationSolutionPaths = value;
                    IsChanged = true;
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
            get { return _copySourceDestinationPath; }
            set
            {
                if (value != _copySourceDestinationPath)
                {
                    _copySourceDestinationPath = value;
                    IsChanged = true;
                }
            }
        }
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
                    IsChanged = true;
                }
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
                    IsChanged = true;
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
                    IsChanged = true;
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
                    IsChanged = true;
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
                    IsChanged = true;
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
                    IsChanged = true;
                }
            }
        }
        #endregion

        #region IsToIncludeTestProjects
        /// <summary>
        /// This flag dictates whether the reports on a VS-Solution is to include VS-Projects that are Test projects,
        /// as identified by having ".Test" within the name.  Default is false.
        /// </summary>
        public bool IsToIncludeTestProjects
        {
            get { return _isToIncludeTestProjects; }
            set
            {
                if (value != _isToIncludeTestProjects)
                {
                    _isToIncludeTestProjects = value;
                    IsChanged = true;
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
                    IsChanged = true;
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
                    IsChanged = true;
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
                    IsChanged = true;
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
                    IsChanged = true;
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
                    IsChanged = true;
                }
            }
        }
        #endregion

        #region ModuleDependencyReportPath
        /// <summary>
        /// Get or set the output-pathname to put the module-dependency report into.
        /// The default value is My Documenets\VsDevTool\VsDevReport.txt
        /// </summary>
        public string ModuleDependencyReportPath
        {
            get { return _moduleDependencyReportPath; }
            set
            {
                if (value != _moduleDependencyReportPath)
                {
                    _moduleDependencyReportPath = value;
                    IsChanged = true;
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
                    IsChanged = true;
                }
            }
        }
        #endregion

        #region ProjectCopyrightNotice
        /// <summary>
        /// Get or set what to set the Copyright-Notice to, wihin any given project's AssemblyInfo.cs file.
        /// </summary>
        public string CopyrightNotice
        {
            get { return _copyrightNotice; }
            set
            {
                if (value != _copyrightNotice)
                {
                    _copyrightNotice = value;
                    IsChanged = true;
                }
            }
        }
        #endregion

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
                    IsChanged = true;
                }
            }
        }
        #endregion

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
                    IsChanged = true;
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
                    IsChanged = true;
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
                    IsChanged = true;
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
               // if (!value.Equals( _spreadsheetFilePathname, StringComparison.OrdinalIgnoreCase ))
                {
                    _spreadsheetFilePathname = value;
                    IsChanged = true;
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
                    IsChanged = true;
                }
            }
        }
        #endregion

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
                    IsChanged = true;
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
            get { return _vsSolutionFilePathname; }
            set
            {
                if (value != _vsSolutionFilePathname)
                {
                    _vsSolutionFilePathname = value;
                    IsChanged = true;
                }
            }
        }
        #endregion

        #region XamlPrefixForLocalizableValues
        /// <summary>
        /// This is the text-pattern to scan the XAML source-code files for, when compiling a list of all localizable values.
        /// </summary>
        public string XamlPrefixForLocalizableValues
        {
            get { return _xamlPrefixForLocalizableValues; }
            set
            {
                if (value != _xamlPrefixForLocalizableValues)
                {
                    _xamlPrefixForLocalizableValues = value;
                    IsChanged = true;
                }
            }
        }
        #endregion

        #endregion public properties

        #region fields

        /// <summary>
        /// This denotes the name of the Application that is currently being looked at.
        /// </summary>
        private string _applicationName;

        /// <summary>
        /// This is the collection of VS-solution paths that are contained within this 'application'.
        /// Applies only when in Application-Scope.
        /// </summary>
        private List<string> _applicationSolutionPaths;

        /// <summary>
        /// This denotes the text of what to set the Copyright-Notice to, wihin any given project's AssemblyInfo.cs file.
        /// </summary>
        private string _copyrightNotice;

        /// <summary>
        /// For when the user wants to copy the collection of source-code files to some other location,
        /// this directory-path denotes the destination of that copy-operation.
        /// </summary>
        private string _copySourceDestinationPath;

        /// <summary>
        /// This is the directory-path to use for storing the application version-histories by default.
        /// The default value is My Documenets\VsDevTool\VersionHistory
        /// </summary>
        private string _defaultRootFolderForVersionStateSnapshots;

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
        /// This denotes the text to set for the company-name property within a given project's AssemblyINfo.cs files.
        /// </summary>
        private string _projectCompanyName;

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
        /// This integer reflects the index of the tab-item that is currently selected within the tab-control within the UX.
        /// The purpose of this is to restore that selection when this program is next launched.
        /// </summary>
        private int _selectedTabItem;

        /// <summary>
        /// This is the pathname of the Excel spreadsheet file for importing resource-strings from.
        /// </summary>
        private string _spreadsheetFilePathname;

        /// <summary>
        /// This denotes the version of the .NET Framework that is to be targeted.
        /// </summary>
        private NetFrameworkVersion _targetNetFrameworkVersion;

        /// <summary>
        /// This is the single instance of this class.
        /// </summary>
        private static VsDevToolUserSettings _userSettings;

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
        /// This is the text-pattern to scan the XAML source-code files for, when compiling a list of all localizable values.
        /// </summary>
        private string _xamlPrefixForLocalizableValues = "lex:Loc";

        #endregion fields
    }
}
