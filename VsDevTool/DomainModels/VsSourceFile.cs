using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Hurst.LogNut;
using Hurst.LogNut.Util;


namespace VsDevTool.DomainModels
{
    public class VsSourceFile : IComparable
    {
        #region ctors
        /// <summary>
        /// Create a new <see cref="VsSourceFile"/> object with no pathname specified.
        /// </summary>
        public VsSourceFile()
        {

        }

        /// <summary>
        /// Create a new <see cref="VsSourceFile"/> object from the given pathname.
        /// </summary>
        /// <param name="pathname">the filesystem-pathname of the physical file that will underly this object</param>
        /// <param name="isToIgnoreFilesystem">this boolean flag dictates whether to check this against the underlying filesystem</param>
        public VsSourceFile( string pathname, bool isToIgnoreFilesystem = false )
        {
            if (pathname == null)
            {
                throw new ArgumentNullException( paramName: nameof( pathname ) );
            }
            if (String.IsNullOrWhiteSpace( pathname ))
            {
                throw new ArgumentException( message: "The pathname must not be empty.", paramName: nameof( pathname ) );
            }
            if (!isToIgnoreFilesystem)
            {
                if (!File.Exists( pathname ))
                {
                    //throw new FileNotFoundException( message: "Source file " + pathname + " not found." );
                    IsNotFound = true;
                }
            }
            _pathname = pathname;
            _isToIgnoreFilesystem = isToIgnoreFilesystem;
        }
        #endregion

        #region FileVersion
        /// <summary>
        /// Get or set the "File Version" property from the disk iamge of this file, if it is a .NET assembly.
        /// </summary>
        public string FileVersion
        {
            get
            {
                if (!_isToIgnoreFilesystem)
                {
                    if (!_hasBeenChecked)
                    {
                        CheckTheFile();
                    }
                }
                else
                {
                    return String.Empty;
                }
                return _fileVersion;
            }
            set { _fileVersion = value; }
        }
        #endregion

        #region IsNotFound
        /// <summary>
        /// Get or set whether this particular source-file was found to be physically present on the given pathname,
        /// if that is checked. Default is false (meaning either it has not been checked, or it was found to be present).
        /// </summary>
        public bool IsNotFound { get; set; }

        #endregion

        #region IsXamlFile
        /// <summary>
        /// Return true if this is a XAML file (that is, has the extension .xaml).
        /// </summary>
        public bool IsXamlFile
        {
            get
            {
                string extension = FileStringLib.GetExtension( _pathname );
                return extension.Equals( "XAML", StringComparison.InvariantCultureIgnoreCase );
            }
        }
        #endregion

        #region Name
        /// <summary>
        /// Get the filesystem-filename, including any extension, of this file.
        /// This excludes any drive or directory information.
        /// </summary>
        public string Name
        {
            get
            {
                return FileStringLib.GetFileNameFromFilePath( Pathname );
            }
        }
        #endregion

        #region Pathname
        /// <summary>
        /// Get or set the filesystem-pathname of this file.
        /// </summary>
        public string Pathname
        {
            [System.Diagnostics.DebuggerStepThrough]
            get { return _pathname; }
            set { _pathname = value; }
        }
        #endregion

        #region VcIdentifier
        /// <summary>
        /// Get or set an arbitrary identifier that a version-control system uses to identify this file,
        /// if applicable.
        /// </summary>
        public string VcIdentifier { get; set; }

        #endregion

        #region WhenLastWritten
        /// <summary>
        /// Get or set the last-modified date-time of this file.
        /// </summary>
        public DateTime WhenLastWritten
        {
            get
            {
                if (!_isToIgnoreFilesystem)
                {
                    if (!_hasBeenChecked)
                    {
                        CheckTheFile();
                    }
                }
                return _whenLastWritten;
            }
            set { _whenLastWritten = value; }
        }
        #endregion

        #region CompareTo
        /// <summary>
        /// Given another VsSourceFile object, return 0 if they are equal, 1 if greater and 0 if lessor (in terms of the pathname properties).
        /// </summary>
        /// <param name="otherObject">the other Object to compare this to</param>
        /// <returns>the result of calling CompareTo on their pathnames</returns>
        public int CompareTo( object otherObject )
        {
            if ((object)otherObject == null)
            {
                return 1;
            }
            VsSourceFile otherSourceFile = otherObject as VsSourceFile;
            // Here, I cast it to Object first, in order to NOT result in a call back to the == operator defined in this class.
            if ((object)otherSourceFile != null)
            {
                return this.Pathname.CompareTo( otherSourceFile.Pathname );
            }
            else
            {
                throw new ArgumentException( "otherObject is not a VsSourceFile." );
            }
        }
        #endregion

        #region Equals
        /// <summary>
        /// Determines whether the specified object is equal to the current object,
        /// in terms of the pathnames that they contain
        /// </summary>
        /// <returns>
        /// true if the specified object is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals( object obj )
        {
            if (obj == null)
            {
                return false;
            }
            VsSourceFile otherSourceFile = obj as VsSourceFile;
            // Here, I cast it to Object first, in order to NOT result in a call back to the == operator defined in this class.
            if ((object)otherSourceFile == null)
            {
                return false;
            }
            return this.Pathname.Equals( otherSourceFile.Pathname, StringComparison.InvariantCultureIgnoreCase );
        }

        /// <summary>
        /// Determines whether the specified VsSourceFile is equal to the current one, in terms of the pathnames they contain.
        /// </summary>
        /// <returns>
        /// true if the specified VsSourceFile  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="otherSourceFile">The VsSourceFile to compare with the current one. </param>
        public bool Equals( VsSourceFile otherSourceFile )
        {
            // Here, I cast it to Object first, in order to NOT result in a call back to the == operator defined in this class.
            if ((object)otherSourceFile == null)
            {
                return false;
            }
            return this.Pathname.Equals( otherSourceFile.Pathname, StringComparison.InvariantCultureIgnoreCase );
        }
        #endregion

        #region GetHashCode
        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        public override int GetHashCode()
        {
            return this.Pathname.GetHashCode();
        }
        #endregion

        #region operator ==

        public static bool operator ==( VsSourceFile a, VsSourceFile b )
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals( a, b ))
            {
                return true;
            }

            // If one is null, but not both, return false.
            // Here, I cast a and b to Object first, in order to NOT result in a call back to this same operator and thus cause an infinite loop.
            if (((object)a == null) || ((object)b == null))
            {
                return false;
            }

            // Return true if the pathnames match.
            return a.Pathname.Equals( b.Pathname, StringComparison.InvariantCultureIgnoreCase );
        }

        public static bool operator !=( VsSourceFile a, VsSourceFile b )
        {
            return !(a == b);
        }
        #endregion

        #region ToString
        /// <summary>
        /// Override the ToString method to yield a useful indication of this object's state.
        /// </summary>
        /// <returns>a string denoting some of this object's properties</returns>
        public override string ToString()
        {
            var sb = new StringBuilder( "VsSourceFile(" );
            sb.Append( "Name = " ).Append( Name ).Append( ", Pathname = " ).Append( Pathname );
            if (!_isToIgnoreFilesystem)
            {
                if (IsNotFound)
                {
                    sb.Append( ", NOT-FOUND!" );
                }
                else
                {
                    // Only include the last-written time, if that has already been gotten.
                    if (_whenLastWritten != default( DateTime ))
                    {
                        sb.Append( ", WhenLastWritten = " ).Append( TimeLib.AsStandardDateTimeString( _whenLastWritten ) );
                    }
                    if (_fileVersion != null)
                    {
                        sb.Append( ", FileVersion = " ).Append( _fileVersion );
                    }
                }
            }
            sb.Append( ")" );
            return sb.ToString();
        }
        #endregion

        #region internal implementation

        [DllImport( "coredll.dll", SetLastError = true )]
        private static extern int GetModuleFileName( IntPtr hModule, StringBuilder lpFilename, int nSize );

        private void CheckTheFile()
        {
            _hasBeenChecked = true;
            if (File.Exists( _pathname ))
            {
                WhenLastWritten = FilesystemLib.GetFileLastWriteTime( _pathname );
                // If it is a .NET assembly, try to get it's file-version.
                string ext = FileStringLib.GetExtension( _pathname );
                if (ext.Equals( "DLL", StringComparison.InvariantCultureIgnoreCase ))
                {
                    try
                    {
                        //var name = new StringBuilder(1024);
                        //GetModuleFileName(IntPtr.Zero, name, 1024);
                        //var version = Assembly.LoadFrom(name.ToString()).GetName().Version;
                        var version = Assembly.LoadFrom( _pathname ).GetName().Version;
                        _fileVersion = version.ToString();
                    }
                    catch (Exception x)
                    {
                        LogManager.LogException( x );
                    }
                }
            }
            else
            {
                throw new FileNotFoundException( message: "Unable to find file " + _pathname );
            }
        }

        protected string _pathname;
        private bool _hasBeenChecked;
        private bool _isToIgnoreFilesystem;
        private DateTime _whenLastWritten;
        // This applies only to assemblies that are referenced.
        private string _fileVersion;

        #endregion internal implementation
    }
}
