using System;
using System.IO;
using Hurst.LogNut.Util;


namespace VsDevTool.DomainModels
{
    /// <summary>
    /// This represents a .NET assembly that is referenced by a given Visual Studio project,
    /// but whose generating-project is not included. In other words - this is just the .DLL or .EXE .
    /// </summary>
    public class VsAssembly
    {
        #region constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public VsAssembly()
        {
        }

        public VsAssembly( string pathname )
        {
            if (pathname == null)
            {
                throw new ArgumentNullException( paramName: nameof( pathname ) );
            }
            if (String.IsNullOrWhiteSpace( pathname ))
            {
                throw new ArgumentException( message: "The pathname must not be empty.", paramName: nameof( pathname ) );
            }
            this.Pathname = pathname;
        }
        #endregion

        public string Name
        {
            get { return FileStringLib.GetFileNameFromFilePath( Pathname ); }
        }

        public string FileVersion { get; set; }

        public string Folder
        {
            get { return FileStringLib.GetDirectoryOfPath( Pathname ); }
        }

        public string Pathname { get; set; }

        public string ProductVersion { get; set; }

        /// <summary>
        /// Get or set when this .NET assembly was last modified
        /// (the assembly itself, not it's files).
        /// </summary>
        public DateTime WhenLastWritten
        {
            get
            {
                if (!_hasBeenChecked)
                {
                    CheckTheFile();
                }
                return _whenLastWritten;
            }
            set { _whenLastWritten = value; }
        }

        #region internal implementation

        private void CheckTheFile()
        {
            _hasBeenChecked = true;
            if (File.Exists( Pathname ))
            {
                WhenLastWritten = FilesystemLib.GetFileLastWriteTime( Pathname );
            }
            else
            {
                throw new FileNotFoundException( message: "Unable to find file " + Pathname );
            }
        }

        private bool _hasBeenChecked;
        private DateTime _whenLastWritten;

        #endregion internal implementation
    }
}
