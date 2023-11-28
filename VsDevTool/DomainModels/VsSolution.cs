using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Hurst.LogNut.Util;
using VsDevTool.ViewModels;


namespace VsDevTool.DomainModels
{
    public class VsSolution
    {
        #region constructors

        public VsSolution()
        {

        }

        /// <summary>
        /// Construct a new VsSolution instance given the pathname to the Visual Studio solution file.
        /// </summary>
        /// <param name="pathnameOfSolutionFile">the pathname to the Visual Studio solution file</param>
        public VsSolution( string pathnameOfSolutionFile )
        {
            if (String.IsNullOrWhiteSpace( pathnameOfSolutionFile ))
            {
                throw new ArgumentNullException( paramName: nameof( pathnameOfSolutionFile ) );
            }
            string ext = FileStringLib.GetExtension( pathnameOfSolutionFile );
            if (!ext.IsEqualIgnoringCase( "sln" ))
            {
                throw new ArgumentException( message: "The pathname of the VS-solution file should have the extension .sln .", paramName: nameof( pathnameOfSolutionFile ) );
            }
            this.SolutionPathname = pathnameOfSolutionFile;
        }
        #endregion

        public string Folder
        {
            get { return FileStringLib.GetDirectoryOfPath( SolutionPathname ); }
        }

        #region Name
        /// <summary>
        /// Get the name of the VS-solution file, which is simply the filename without the extension.
        /// </summary>
        public string Name
        {
            get
            {
                if (_name == null)
                {
                    _name = FileStringLib.GetFileNameWithoutExtension( SolutionPathname );
                }
                return _name;
            }
        }
        #endregion

        public string SolutionPathname { get; set; }

        public List<VsProject> Projects
        {
            get
            {
                if (_projects == null)
                {
                    _projects = new List<VsProject>();
                    ParseForProjects();
                }
                return _projects;
            }
            set { _projects = value; }
        }

        public override string ToString()
        {
            var sb = new StringBuilder( "VsSolution(" );
            if (!String.IsNullOrWhiteSpace( this.Name ))
            {
                sb.Append( this.Name );
            }
            else
            {
                sb.Append( "no name, path=" ).Append( SolutionPathname );
            }
            sb.Append( ")" );
            return sb.ToString();
        }

        #region internal implementation

        private void ParseForProjects()
        {
            if (String.IsNullOrWhiteSpace( SolutionPathname ))
            {
                throw new InvalidOperationException( "Cannot parse for projects until pathname has been set." );
            }
            if (!File.Exists( SolutionPathname ))
            {
                throw new FileNotFoundException( message: "You cannot parse a solution file that does not exist", fileName: SolutionPathname );
            }

            // Change the 'CurrentDirectory' to the folder of this solution-file,
            // so that relative paths may be correctly resolved.
            string solutionFolder = FileStringLib.GetDirectoryOfPath( SolutionPathname );
            string originalValueOfCurrentDirectory = Directory.GetCurrentDirectory();

            try
            {
                Directory.SetCurrentDirectory( solutionFolder );

                Console.WriteLine( "Parsing into solution file " + SolutionPathname );
                foreach (string line in FilesystemLib.ReadLines( SolutionPathname ))
                {
                    if (line.Contains( ".csproj" ))
                    {
                        // Get the project name.

                        // Look for the first ") = "
                        int indexOfRightParen = line.IndexOf( ") =" );
                        if (indexOfRightParen < 0)
                        {
                            Console.WriteLine( "Error: Failed to find end of GUID on line that contains project: " + line );
                            continue;
                        }
                        int indexOfQuoteAfterName = line.IndexOf( @"""", indexOfRightParen + 5 );
                        if (indexOfQuoteAfterName < 0)
                        {
                            Console.WriteLine( "Error: Failed to find double-quote on line that contains project: " + line );
                            continue;
                        }
                        int len = indexOfQuoteAfterName - indexOfRightParen - 5;
                        string projectName = line.Substring( indexOfRightParen + 5, len );
                        if (!ApplicationViewModel.The.IsToIncludeTestProjects && projectName.Contains( ".Test" ))
                        {
                            Console.WriteLine( "Excluding project " + projectName + " because that is an automated-test project." );
                        }
                        else
                        {
                            Console.WriteLine( "Found project " + projectName );

                            // Get the project's pathname.

                            // Within this solution file, the path is given relative to the solution's own folder.

                            int indexOfQuoteBeforePath = line.IndexOf( @"""", indexOfQuoteAfterName + 1 );
                            int indexOfQuoteAfterPath = line.IndexOf( @"""", indexOfQuoteBeforePath + 1 );
                            len = indexOfQuoteAfterPath - indexOfQuoteBeforePath - 1;
                            string relativePath = line.Substring( indexOfQuoteBeforePath + 1, len );

                            string actualPath = Path.GetFullPath( relativePath );

                            var newProject = new VsProject( actualPath );
                            newProject.AssemblyName = projectName;

                            // Get the project's GUID.
                            _projects.Add( newProject );

                            // Get the last-modified time.
                        }
                    }
                }
            }
            finally
            {
                // Restore the original setting of the 'Current Directory'.
                Directory.SetCurrentDirectory( originalValueOfCurrentDirectory );
            }
        }

        /// <summary>
        /// This is the name of the VS-solution file, which is simply the filename without the extension.
        /// </summary>
        private string _name;

        private List<VsProject> _projects;

        #endregion internal implementation
    }
}
