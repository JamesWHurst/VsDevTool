#if PRE_4
#define PRE_5
#endif
using System;
using System.IO;
using System.Linq;
using System.Text;


// Compile with the pragma NO_LONGPATH if you want to use only the normal .NET filesystem facilities.


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// This is the Zeta-Long-Paths library object that extends the concept of a DirectoryInfo object
    /// and supports paths that are longer than MAX_FILE characters.
    /// </summary>
    public class ZDirectoryInfo
    {
        #region constructors
        /// <summary>
        /// Create a new ZDirectoryInfo object that represents the folder at the given filesystem-path.
        /// </summary>
        /// <param name="directoryPath">the filesystem-path that this object will represent</param>
        /// <exception cref="ArgumentNullException">The given directory-path must not be null.</exception>
        /// <exception cref="ArgumentException">The value provided for the directory-path must be a syntactically-valid path.</exception>
        public ZDirectoryInfo( string directoryPath )
        {
            if (directoryPath == null)
            {
#if PRE_4
                throw new ArgumentNullException( "directoryPath" );
#else
                throw new ArgumentNullException( nameof( directoryPath ) );
#endif
            }
            _path = directoryPath;
#if (NO_LONGPATH)
            _directoryInfo = new DirectoryInfo(directoryPath);
#endif
        }
        #endregion

        #region public properties

        #region Attributes
        /// <summary>
        /// Get or set the FileAttributes of this file.
        /// </summary>
        public FileAttributes Attributes
        {
            get
            {
#if NETFX_CORE
                return Directory.GetAttributes( _path );
#else
                return FilesystemLib.GetDirectoryAttributes( _path );
#endif
            }
            set
            {
#if NETFX_CORE
                Directory.SetAttributes( path: _path, fileAttributes: value );
#else
                FilesystemLib.SetDirectoryAttributes( _path, value );
#endif
            }
        }
        #endregion

        #region Exists
        /// <summary>
        /// Get whether the folder represented by the path that this object contains, does in fact exist at this moment.
        /// </summary>
        public bool Exists
        {
            get
            {
#if (NETFX_CORE || NO_LONGPATH)
                return Directory.Exists( _path );
#else
                return FilesystemLib.DirectoryExists( _path );
#endif
            }
        }
        #endregion

        #region FullName
        /// <summary>
        /// Get the full path that this ZDirectoryInfo object represents.
        /// </summary>
        public string FullName
        {
            [System.Diagnostics.DebuggerStepThrough]
            get
            {
                return _path;
            }
        }
        #endregion

        #region Name
        /// <summary>
        /// Get the name, without any other path information.
        /// </summary>
        public string Name
        {
            get { return FileStringLib.GetDirectoryNameOnlyFromPath( _path ); }
        }
        #endregion

        #region Parent
        /// <summary>
        /// Get the parent-directory.
        /// </summary>
        public ZDirectoryInfo Parent
        {
            get
            {
                string pathWithoutFinalSlash = _path.WithoutAtEnd( '\\' );
                return new ZDirectoryInfo( FileStringLib.GetDirectoryPathNameFromFilePath( pathWithoutFinalSlash ) );
            }
        }
        #endregion

        #region times

        #region CreationTime
        /// <summary>
        /// Get or set the DateTime representing the creation-time of this folder.
        /// </summary>
        public DateTime CreationTime
        {
            get
            {
#if NETFX_CORE
                return Directory.GetCreationTime( _path );
#else
                return FilesystemLib.GetDirectoryCreationTime( _path );
#endif
            }
            set
            {
                //CBL  I don't like for this property to provide a setter here
#if NETFX_CORE
                Directory.SetCreationTime( path: _path, creationTime: value );
#else
                FilesystemLib.SetDirectoryCreationTime( _path, value );
#endif
            }
        }
        #endregion

        #region LastAccessTime
        /// <summary>
        /// Get or set the DateTime representing the last-access-time of this folder.
        /// </summary>
        public DateTime LastAccessTime
        {
            get
            {
#if NETFX_CORE
                return File.GetLastAccessTime( _path );
#else
                return FilesystemLib.GetFileLastAccessTime( _path );
#endif
            }
            set
            {
#if NETFX_CORE
                File.SetLastAccessTime( path: _path, lastAccessTime: value );
#else
                FilesystemLib.SetFileLastAccessTime( _path, value );
#endif
            }
        }
        #endregion

        #region LastWriteTime
        /// <summary>
        /// Get or set the DateTime representing the last-write-time of this folder.
        /// </summary>
        public DateTime LastWriteTime
        {
            get
            {
#if NETFX_CORE
                return File.GetLastWriteTime( _path );
#else
                return FilesystemLib.GetFileLastWriteTime( _path );
#endif
            }
            set
            {
#if NETFX_CORE
                File.SetLastWriteTime( path: _path, lastWriteTime: value );
#else
                FilesystemLib.SetFileLastWriteTime( _path, value );
#endif
            }
        }
        #endregion

        #endregion times

        #endregion public properties

        #region public methods

        #region operator ==
        /// <summary>
        /// Return true if the two given ZDirectoryInfo objects are equal (in other words -- their FullName properties are the same).
        /// </summary>
        /// <param name="a">the first ZDirectoryInfo, to compare against the other</param>
        /// <param name="b">the second ZDirectoryInfo, to compare a against</param>
        /// <returns>true if the FullName properties are equal</returns>
        public static bool operator ==( ZDirectoryInfo a, ZDirectoryInfo b )
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

            // Return true if the FullNames match.
            return a.FullName.Equals( b.FullName, StringComparison.OrdinalIgnoreCase );
        }

        /// <summary>
        /// Return true if the two given ZDirectoryInfo objects are unequal (in other words -- their FullName property is not the same).
        /// </summary>
        /// <param name="a">the first ZDirectoryInfo, to compare against the other</param>
        /// <param name="b">the second ZDirectoryInfo, to compare a against</param>
        /// <returns>true if the FullName properties are different</returns>
        public static bool operator !=( ZDirectoryInfo a, ZDirectoryInfo b )
        {
            return !(a == b);
        }
        #endregion

        #region Create
        /// <summary>
        /// Create the folder on the actual filesystem-path that this ZDirectoryInfo represents.
        /// </summary>
        /// <exception cref="ArgumentNullException">folderPath must not be null</exception>
        public void Create()
        {
            Directory.CreateDirectory( _path );
        }
        #endregion

        #region Delete
        /// <summary>
        /// Remove this directory; if recursive is true then do it even if it has contents.
        /// </summary>
        public void Delete()
        {
            FilesystemLib.DeleteDirectory( _path );
        }
        #endregion

        #region DeleteContent
        /// <summary>
        /// Remove everything within the folder that this <c>ZDirectoryInfo</c> represents,
        /// if it exists. If the folder does not exist - this does nothing and returns false.
        /// </summary>
        /// <returns>true if the folder had anything within it</returns>
        public bool DeleteContent()
        {
            return FilesystemLib.DeleteDirectoryContent( _path );
        }
        #endregion

        #region DeleteContentFiles
        /// <summary>
        /// If the filesystem-folder that this <c>ZDirectoryInfo</c> represents exists, remove any files within it that match the given file-specification.
        /// The directory itself is not deleted.
        /// This simply invokes <c>FilesystemLib.DeleteDirectoryContentFiles</c>
        /// </summary>
        /// <param name="fileSpec">a file-spec (which may contain wildcard characters) that denotes what to delete</param>
        /// <exception cref="ArgumentNullException">the value provided for string must not be null</exception>
        public void DeleteContentFiles( string fileSpec )
        {
            FilesystemLib.DeleteDirectoryContentFiles( _path, fileSpec );
        }
        #endregion

        #region DeleteContentSubdirectories
        /// <summary>
        /// If the filesystem-folder that this <c>ZDirectoryInfo</c> represents exists, remove any folders within it that match the given file-spec
        /// (which may include wildcard characters).
        /// The directory that this <c>ZDirectoryInfo</c> object itself represents is not deleted.
        /// This simply invokes <c>FilesystemLib.DeleteDirectoryContentSubdirectories</c>
        /// </summary>
        /// <param name="folderSpec">a file-spec (which may contain wildcard characters) that denotes which folders to delete</param>
        /// <exception cref="ArgumentNullException">the value provided for fileSpec must not be null</exception>
        public void DeleteContentSubdirectories( string folderSpec )
        {
            FilesystemLib.DeleteDirectoryContentSubdirectories( _path, folderSpec );
        }
        #endregion

        #region DeleteFile
        /// <summary>
        /// Remove the given file, if it exists within the folder represented by this <c>ZDirectoryInfo</c> object.
        /// This retries the deletion if it fails, and keeps retrying for up to five seconds - throwing an exception if that fails.
        /// </summary>
        /// <param name="filename">the name of the file to delete (no folder-path parts)</param>
        /// <exception cref="ArgumentNullException">the value provided for filename must not be null</exception>
        /// <exception cref="ArgumentException">the value provided for filename must be a syntactically-valid filename with no slashes</exception>
        /// <remarks>
        /// This can safely be called with the name of a file that does not exist.
        /// 
        /// If the Read-Only attribute is set on the file, that is ignored - the file is still deleted.
        /// </remarks>
        public void DeleteFile( string filename )
        {
            // Check the argument...
            if (filename == null)
            {
                throw new ArgumentNullException( "filename" );
            }
            // Ensure it is a simple filename and not a path.
            if (filename.Contains( "/" ) || filename.Contains( @"\" ))
            {
                throw new ArgumentException( @"This (""" + filename + @""") should be just a filename, with no slashes." );
            }
            string pathname = Path.Combine( _path, filename );

            FilesystemLib.DeleteFile( pathname );
        }
        #endregion

        #region Equals
        /// <summary>
        /// Determines whether the specified object is equal to the current object,
        /// in terms of the FullName property that they contain
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
            ZDirectoryInfo otherFileSourceLocation = obj as ZDirectoryInfo;
            // Here, I cast it to Object first, in order to NOT result in a call back to the == operator defined in this class.
            if ((object)otherFileSourceLocation == null)
            {
                return false;
            }
            return this.FullName.Equals( otherFileSourceLocation.FullName, StringComparison.OrdinalIgnoreCase );
        }

        /// <summary>
        /// Determines whether the specified ZDirectoryInfo is equal to the current one, in terms of the FullName they contain.
        /// </summary>
        /// <returns>
        /// true if the specified ZDirectoryInfo  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="otherFileSourceLocation">The ZDirectoryInfo to compare with the current one. </param>
        public bool Equals( ZDirectoryInfo otherFileSourceLocation )
        {
            // Here, I cast it to Object first, in order to NOT result in a call back to the == operator defined in this class.
            if ((object)otherFileSourceLocation == null)
            {
                return false;
            }
            return this.FullName.Equals( otherFileSourceLocation.FullName, StringComparison.OrdinalIgnoreCase );
        }
        #endregion

        #region GetDirectories
        /// <summary>
        /// Get the folders within the folder represented by this ZDirectoryInfo object,
        /// as an array of ZDirectoryInfo objects.
        /// </summary>
        /// <returns>an array of ZDirectoryInfo objects representing the folders that were found</returns>
        public ZDirectoryInfo[] GetDirectories()
        {
            return FilesystemLib.GetDirectories( _path );
        }

        /// <summary>
        /// Get the folders within the folder represented by this ZDirectoryInfo object that match the given pattern,
        /// as an array of ZDirectoryInfo objects.
        /// </summary>
        /// <param name="folderSpec">the wildcard-expression to match the folder-names against</param>
        /// <returns>an array of ZDirectoryInfo objects representing the folders that were found</returns>
        public ZDirectoryInfo[] GetDirectories( string folderSpec )
        {
            return FilesystemLib.GetDirectories( _path, folderSpec );
        }
        #endregion

        #region GetFiles
        /// <summary>
        /// Get the files within the folder represented by this ZDirectoryInfo object,
        /// as an array of <see cref="ZFileInfo"/> objects.
        /// </summary>
        /// <returns>an array of ZFileInfo objects representing the files that were found</returns>
        public ZFileInfo[] GetFiles()
        {
            return FilesystemLib.GetFiles( _path );
        }

        /// <summary>
        /// Get the files within the folder represented by this ZDirectoryInfo object that match the given file-spec
        /// as an array of <see cref="ZFileInfo"/> objects.
        /// </summary>
        /// <param name="fileSpec">the wildcard-expression to match the file-names against</param>
        /// <returns>an array of ZFileInfo objects representing the files that were found</returns>
        public ZFileInfo[] GetFiles( string fileSpec )
        {
            return FilesystemLib.GetFiles( _path, fileSpec );
        }

        /// <summary>
        /// Get the files within the folder represented by this ZDirectoryInfo object that match the given wildcard-pattern,
        /// based upon the given SearchOption, as an array of <see cref="ZFileInfo"/> objects.
        /// </summary>
        /// <param name="fileSpec">the wildcard-expression to match the file-names against</param>
        /// <param name="searchOption">this indicates how deeply to earch - TopDirectoryOnly or AllDirectories</param>
        /// <returns>an array of ZFileInfo objects representing the files that were found</returns>
        public ZFileInfo[] GetFiles( string fileSpec, System.IO.SearchOption searchOption )
        {
            return FilesystemLib.GetFiles( _path, fileSpec, searchOption );
        }

        /// <summary>
        /// Get the files within the folder represented by this ZDirectoryInfo object,
        /// based upon the given SearchOption, as an array of <see cref="ZFileInfo"/> objects.
        /// </summary>
        /// <param name="searchOption">this indicates how deeply to earch - TopDirectoryOnly or AllDirectories</param>
        /// <returns>an array of ZFileInfo objects representing the files that were found</returns>
        public ZFileInfo[] GetFiles( System.IO.SearchOption searchOption )
        {
            return FilesystemLib.GetFiles( _path, searchOption );
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
            return this.FullName.GetHashCode();
        }
        #endregion

        #region IsHidden
        /// <summary>
        /// Get whether the directory that is denoted by this <c>ZDirectoryInfo</c> object has the Hidden attribute set.
        /// </summary>
        public bool IsHidden
        {
            get
            {
                return FilesystemLib.IsDirectoryHidden(_path);
            }
        }
        #endregion

        #region IsReadonly
        /// <summary>
        /// Get whether the directory that is denoted by this <c>ZDirectoryInfo</c> object has the ReadOnly attribute set.
        /// </summary>
        public bool IsReadonly
        {
            get
            {
                return FilesystemLib.IsDirectoryReadonly(_path);
            }
        }
        #endregion

        #region MakeToHaveExactlyTheseDirectories
        /// <summary>
        /// Given an array of sub-directory names (NOT full paths), create the corresponding sub-directories within the directory that
        /// this <see cref="ZDirectoryInfo"/> object represents, that don't already exist,
        /// and delete any sub-directories within it that are not present within this array of names.
        /// </summary>
        /// <param name="subdirectoriesExpected">the array of sub-directory names denoted those that are to be allowed to be present (or created)</param>
        public void MakeToHaveExactlyTheseDirectories( string[] subdirectoriesExpected )
        {
            if (subdirectoriesExpected == null)
            {
                throw new ArgumentNullException( "subdirectoriesExpected" );
            }
            HelpToMakeToHaveExactlyTheseDirectories( _path, subdirectoriesExpected );
            VerifyHasExactlyTheseSubDirectories( subdirectoriesExpected );
        }
        #endregion

        #region MakeToHaveNoDirectories
        /// <summary>
        /// Delete any folders that are present within the folder that this <see cref="ZDirectoryInfo"/> object represents,
        /// </summary>
        public void MakeToHaveNoDirectories()
        {
            HelpToMakeToHaveNoSubdirectories( _path );
        }

        /// <summary>
        /// Delete any folders that are present within the given sub-directory of that directory that this <see cref="ZDirectoryInfo"/>
        /// object represents. If that sub-directory does not exist yet - create it.
        /// </summary>
        /// <param name="subFolderName">the name (NOT full path) of a sub-directory of this one</param>
        public void MakeToHaveNoDirectories( string subFolderName )
        {
            if (subFolderName == null)
            {
                throw new ArgumentNullException( "subFolderName" );
            }
            string subpath = Path.Combine( _path, subFolderName );
            HelpToMakeToHaveNoSubdirectories( subpath );
        }
        #endregion

        #region MakeToHaveExactlyTheseFiles
        /// <summary>
        /// Given an array of filenames (NOT full paths), create those files within the directory that
        /// this <see cref="ZDirectoryInfo"/> object represents (creating this directory if it does not already exist),
        /// and delete any files within it that are not present within this array.
        /// </summary>
        /// <param name="filenamesExpected">the array of filenames that are to be allowed to be present (or created)</param>
        public void MakeToHaveExactlyTheseFiles( string[] filenamesExpected )
        {
            if (filenamesExpected == null)
            {
                throw new ArgumentNullException( "filenamesExpected" );
            }
            HelpToMakeToHaveExactlyTheseFiles( _path, filenamesExpected );
        }

        /// <summary>
        /// Given a sub-directory name and an array of filenames (NOT full paths), create those files within the named sub-directory of that directory which
        /// this <see cref="ZDirectoryInfo"/> object represents (creating this directory and sub-directory if they do not already exist),
        /// and delete any files within that sub-directory that are not present within this array.
        /// </summary>
        /// <param name="subFolderName">the name (NOT full path) of a sub-directory of this one</param>
        /// <param name="filenamesExpected">the array of filenames that are to be allowed to be present (or created)</param>
        public void MakeToHaveExactlyTheseFiles( string subFolderName, string[] filenamesExpected )
        {
            if (subFolderName == null)
            {
                throw new ArgumentNullException( "subFolderName" );
            }
            if (filenamesExpected == null)
            {
                throw new ArgumentNullException( "filenamesExpected" );
            }
            string subpath = Path.Combine( _path, subFolderName );
            HelpToMakeToHaveExactlyTheseFiles( subpath, filenamesExpected );
        }
        #endregion

        #region MakeHaveNoFiles
        /// <summary>
        /// Delete any files that are present within the directory that this <see cref="ZDirectoryInfo"/> object represents,
        /// or, if the directory does not exist yet - create it.
        /// </summary>
        public void MakeHaveNoFiles()
        {
            HelpToMakeHaveNoFiles( _path );
        }

        /// <summary>
        /// Delete any files that are present within the given sub-directory of that directory that this <see cref="ZDirectoryInfo"/>
        /// object represents. If that sub-directory does not exist yet - create it.
        /// </summary>
        /// <param name="subFolderName">the name (NOT full path) of a sub-directory of this one</param>
        public void MakeHaveNoFiles( string subFolderName )
        {
            if (subFolderName == null)
            {
                throw new ArgumentNullException( "subFolderName" );
            }
            string subpath = Path.Combine( _path, subFolderName );
            HelpToMakeHaveNoFiles( subpath );
        }
        #endregion

        #region SetFileHidden
        /// <summary>
        /// Set the "Hidden"-attribute of the given file that is within the directory denoted by this <c>ZDirectoryInfo</c> object.
        /// </summary>
        /// <param name="filename">the filename (within this folder) of the file to set the attribute of</param>
        /// <param name="isToBeHidden">true to turn the Hidden attribute on, false to turn it off</param>
        /// <exception cref="ArgumentNullException">The argument supplied for <paramref name="filename"/> must not be null.</exception>
        /// <exception cref="ArgumentException">the value provided for filename must be a syntactically-valid filename with no slashes</exception>
        public void SetFileHidden( string filename, bool isToBeHidden )
        {
            if (filename == null)
            {
                throw new ArgumentNullException( "filename" );
            }
            if (filename.Contains( "/" ) || filename.Contains( @"\" ))
            {
                throw new ArgumentException( @"This (""" + filename + @""") should be just a filename, with no slashes." );
            }
            string filePath = Path.Combine( _path, filename );
            FilesystemLib.SetFileHidden( filePath, isToBeHidden );
        }
        #endregion

        #region SetFileReadOnly
        /// <summary>
        /// Set the "ReadOnly"-attribute of the given file that is within the directory denoted by this <c>ZDirectoryInfo</c> object.
        /// </summary>
        /// <param name="filename">the filename (within this folder) of the file to set the attribute of</param>
        /// <param name="isToBeReadOnly">true to turn the ReadOnly attribute on, false to turn it off</param>
        /// <exception cref="ArgumentNullException">The argument supplied for <paramref name="filename"/> must not be null.</exception>
        /// <exception cref="ArgumentException">the value provided for filename must be a syntactically-valid filename with no slashes</exception>
        public void SetFileReadOnly( string filename, bool isToBeReadOnly )
        {
            if (filename == null)
            {
                throw new ArgumentNullException( "filename" );
            }
            if (filename.Contains( "/" ) || filename.Contains( @"\" ))
            {
                throw new ArgumentException( @"This (""" + filename + @""") should be just a filename, with no slashes." );
            }
            string filePath = Path.Combine( _path, filename );
            FilesystemLib.SetFileReadonly( filePath, isToBeReadOnly );
        }
        #endregion

        #region SetHidden
        /// <summary>
        /// Set the "Hidden"-attribute of the directory denoted by this <c>ZDirectoryInfo</c> object.
        /// </summary>
        /// <param name="isToBeHidden">true to turn the Hidden attribute on, false to turn it off</param>
        /// <returns>true if the directory was not already Hidden and had to be changed</returns>
        /// <remarks>
        /// The reason for the return-value is to provide a way for unit-tests to tell whether this had to actually change the directory-attributes,
        /// since if it's already the desired state of Hidden then we want to ensure no attempt is made to change it.
        ///
        /// Test-status: Needed!
        /// </remarks>
        public bool SetHidden( bool isToBeHidden )
        {
            return FilesystemLib.SetDirectoryHidden(_path, isToBeHidden);
        }
        #endregion

        #region SetReadOnly
        /// <summary>
        /// Set the "ReadOnly"-attribute of the directory denoted by this <c>ZDirectoryInfo</c> object.
        /// </summary>
        /// <param name="isToBeReadOnly">true to turn the ReadOnly attribute on, false to turn it off</param>
        /// <returns>true if the directory was not already ReadOnly and had to be changed</returns>
        /// <remarks>
        /// The reason for the return-value is to provide a way for unit-tests to tell whether this had to actually change the directory-attributes,
        /// since if it's already the desired state of ReadOnly then we want to ensure no attempt is made to change it.
        ///
        /// Test-status: Needed!
        /// </remarks>
        public bool SetReadOnly( bool isToBeReadOnly )
        {
            return FilesystemLib.SetDirectoryReadOnly( _path, isToBeReadOnly );
        }
        #endregion

        #region SubDirectoryExists
        /// <summary>
        /// Return true if the given sub-directory exists under that directory that is denoted by this ZDirectoryInfo-object.
        /// </summary>
        /// <param name="subDirectoryName">the name (not full path) of a sub-folder under that of this ZDirectoryInfo-object</param>
        /// <returns>true if the filesystem-folder does exist on the drive</returns>
        /// <exception cref="ArgumentNullException">the value provided for <paramref name="subDirectoryName"/> must not be null</exception>
        /// <exception cref="ArgumentException">the value provided for <paramref name="subDirectoryName"/> must not contain path-separators.</exception>
        public bool SubDirectoryExists( string subDirectoryName )
        {
            if (subDirectoryName == null)
            {
                throw new ArgumentNullException( "subDirectoryName" );
            }
            int indexOfSlash = subDirectoryName.IndexOf( @"\" );
            if (indexOfSlash != -1)
            {
#if PRE_4
                throw new ArgumentException( "the subdirectory-name must not contain a path-separator", "subDirectoryName" );
#else
                throw new ArgumentException( message: String.Format( "the subdirectory-name must not contain a path-separator" ), paramName: nameof( subDirectoryName ) );
#endif
            }
            string subFolderPath = Path.Combine( _path, subDirectoryName );
            return Directory.Exists( subFolderPath );
        }

        /// <summary>
        /// Return true if the given sub-directory exists under the other given sub-directory that is under that which is denoted by this ZDirectoryInfo-object.
        /// </summary>
        /// <param name="subDirectoryName">the name (not full path) of a sub-folder under that of this ZDirectoryInfo-object</param>
        /// <param name="subDirectoryUnderThat">the name (not full path) of a yet lower-level sub-folder under that of <paramref name="subDirectoryName"/></param>
        /// <returns>true if the filesystem-folder does exist on the drive</returns>
        /// <exception cref="ArgumentNullException">the value provided for either argument must not be null</exception>
        /// <exception cref="ArgumentException">the value provided for either sub-folder name must not contain path-separators.</exception>
        public bool SubDirectoryExists( string subDirectoryName, string subDirectoryUnderThat )
        {
            if (subDirectoryName == null)
            {
                throw new ArgumentNullException( "subDirectoryName" );
            }
            if (subDirectoryUnderThat == null)
            {
                throw new ArgumentNullException( "subDirectoryUnderThat" );
            }
            int indexOfSlash = subDirectoryName.IndexOf( @"\" );
            if (indexOfSlash != -1)
            {
                throw new ArgumentException( "The subfolder-name must not contain a path-separator", "subDirectoryName" );
            }
            indexOfSlash = subDirectoryUnderThat.IndexOf( @"\" );
            if (indexOfSlash != -1)
            {
                throw new ArgumentException( "The subfolder-name must not contain a path-separator", "subDirectoryUnderThat" );
            }
#if !PRE_4
            string subFolderPath = Path.Combine( _path, subDirectoryName, subDirectoryUnderThat );
#else
            string subFolderPath1 = Path.Combine( _path, subDirectoryName );
            string subFolderPath = Path.Combine( subFolderPath1, subDirectoryUnderThat );
#endif
            return Directory.Exists( subFolderPath );
        }
        #endregion

        #region ToString
        /// <summary>
        /// Override the ToString method to show the filesystem-path that this object represents.
        /// </summary>
        /// <returns>the filesystem-path of this directory, or "no path" if none is specified</returns>
        public override string ToString()
        {
            var sb = new StringBuilder( "ZDirectoryInfo(" );
#if PRE_4
            if (String.IsNullOrEmpty(this._path))
#else
            if (String.IsNullOrWhiteSpace( this._path ))
#endif
            {
                sb.Append( "no path)" );
            }
            else
            {
                sb.Append( this._path );
                sb.Append( ")" );
            }
            return sb.ToString();
        }
        #endregion

        #region VerifyHasExactlyTheseFiles
        /// <summary>
        /// Check that the filesystem-folder denoted by this object has the indicated files within it (without recursing into sub-folders), and no others, throwing an exception if that is not the case.
        /// </summary>
        /// <param name="filenamesExpected">the names of the files that are expected to be within the folder denoted by this object</param>
        public void VerifyHasExactlyTheseFiles( string[] filenamesExpected )
        {
            HelpToVerifyHasExactlyTheseFiles( _path, filenamesExpected );
        }

        /// <summary>
        /// Check that the given subfolder within the folder denoted by this object, has the files indicated by the given array of names, and no others, throwing an exception if that is not the case.
        /// </summary>
        /// <param name="subfolderName">the name of the subfolder within that folder denoted by this object, in which the indicated files are expected to be</param>
        /// <param name="filenamesExpected">the names of the files that are expected to be in the sub-folder denoted by <paramref name="subfolderName"/></param>
        public void VerifyHasExactlyTheseFiles( string subfolderName, string[] filenamesExpected )
        {
            if (subfolderName == null)
            {
                throw new ArgumentNullException( "subfolderName" );
            }
            if (filenamesExpected == null)
            {
                throw new ArgumentNullException( "filenamesExpected" );
            }
            string subpath = Path.Combine( _path, subfolderName );
            HelpToVerifyHasExactlyTheseFiles( subpath, filenamesExpected );
        }
        #endregion

        #region VerifyHasExactlyTheseSubDirectories
        /// <summary>
        /// Check that the filesystem-folder denoted by this object has the indicated sub-folders, and no others, throwing an exception if that is not the case.
        /// </summary>
        /// <param name="subdirectoriesExpected">the sub-directories that are expected to be in the parent-directory denoted by this object</param>
        public void VerifyHasExactlyTheseSubDirectories( string[] subdirectoriesExpected )
        {
            if (subdirectoriesExpected == null)
            {
                throw new ArgumentNullException( "subdirectoriesExpected" );
            }
            HelpToVerifyHasExactlyTheseSubDirectories( _path, subdirectoriesExpected );
        }

        /// <summary>
        /// Check that the given subfolder within the folder denoted by this object, has the indicated sub-folders, and no others, throwing an exception if that is not the case.
        /// </summary>
        /// <param name="subfolderName">the name of the subfolder within that folder denoted by this object, in which the given sub-folders are expected to be</param>
        /// <param name="subdirectoriesExpected">the sub-directories that are expected to be in the sub-folder denoted by <paramref name="subfolderName"/></param>
        public void VerifyHasExactlyTheseSubDirectories( string subfolderName, string[] subdirectoriesExpected )
        {
            if (subfolderName == null)
            {
                throw new ArgumentNullException( "subfolderName" );
            }
            if (subdirectoriesExpected == null)
            {
                throw new ArgumentNullException( "subdirectoriesExpected" );
            }
            string subfolderPath = Path.Combine( _path, subfolderName );
            HelpToVerifyHasExactlyTheseSubDirectories( subfolderPath, subdirectoriesExpected );
        }
        #endregion

        #region VerifyHasNoFiles
        /// <summary>
        /// Throw an <see cref="InvalidOperationException"/> if the directory denoted by this <see cref="ZDirectoryInfo"/> has any files within it.
        /// </summary>
        public void VerifyHasNoFiles()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo( _path );
            var filesFound = directoryInfo.GetFiles();
            int numberFound = filesFound.Length;
            if (numberFound != 0)
            {
                throw new InvalidOperationException( String.Format( "This calls for folder {0} to have no files but instead it has {1}.", _path, numberFound ) );
            }
        }
        #endregion

        #endregion public methods

        #region internal implementation

        #region private methods

        private void HelpToMakeToHaveExactlyTheseDirectories( string folderPath, string[] subdirectoriesExpected )
        {
            FilesystemLib.CreateDirectory( folderPath );
            DirectoryInfo directoryInfo = new DirectoryInfo( folderPath );
            var directoryInfosFound = directoryInfo.GetDirectories();
            var directoryNamesFound = (from f in directoryInfosFound select f.Name);
            int numberExpected = subdirectoriesExpected.Length;
            if (numberExpected == 0)
            {
                foreach (var name in directoryNamesFound)
                {
                    string path = Path.Combine( folderPath, name );

                    //CBL
#if NETFX_CORE
                    Directory.Delete( path );
#else
                    FilesystemLib.DeleteDirectory( path );
#endif
                }
            }
            else // numberExpected > 0
            {
                // Remove any unexpected subdirectories..
                foreach (var dirInfo in directoryInfosFound)
                {
                    string name = dirInfo.Name;
                    if (!subdirectoriesExpected.Contains( name ))
                    {
                        //CBL
#if NETFX_CORE
                        Directory.Delete( dirInfo.FullName );
#else
                        FilesystemLib.DeleteDirectory( dirInfo.FullName );
#endif
                    }
                }
                // Create any missing files..
                foreach (var name in subdirectoriesExpected)
                {
                    if (!directoryNamesFound.Contains( name ))
                    {
                        FilesystemLib.CreateDirectory( folderPath, name );
                    }
                }
            }
            VerifyHasExactlyTheseSubDirectories( subdirectoriesExpected );
        }

        private void HelpToMakeToHaveExactlyTheseFiles( string folderPath, string[] filenamesExpected )
        {
            if (folderPath == null)
            {
                throw new ArgumentNullException( "folderPath" );
            }
            if (filenamesExpected == null)
            {
                throw new ArgumentNullException( "filenamesExpected" );
            }
            FilesystemLib.CreateDirectory( folderPath );
            DirectoryInfo directoryInfo = new DirectoryInfo( folderPath );
            var filesFound = directoryInfo.GetFiles();
            var filenamesFound = (from f in filesFound select f.Name);
            int numberExpected = filenamesExpected.Length;
            if (numberExpected == 0)
            {
                foreach (var filename in filenamesFound)
                {
                    string pathname = Path.Combine( folderPath, filename );
                    FilesystemLib.DeleteFile( pathname );
                }
            }
            else // numberExpected > 0
            {
                // Remove any unexpected files..
                foreach (var fileInfo in filesFound)
                {
                    string filename = fileInfo.Name;
                    if (!filenamesExpected.Contains( filename ))
                    {
                        FilesystemLib.DeleteFile( fileInfo.FullName );
                    }
                }
                // Create any missing files..
                foreach (var filename in filenamesExpected)
                {
                    if (!filenamesFound.Contains( filename ))
                    {
                        string pathname = Path.Combine( folderPath, filename );
                        FilesystemLib.WriteText( pathname, FilesystemLib.SampleFileContent );
                    }
                }
            }
            VerifyHasExactlyTheseFiles( folderPath, filenamesExpected );
        }

        private void HelpToMakeHaveNoFiles( string folderPath )
        {
            if (folderPath == null)
            {
                throw new ArgumentNullException( "folderPath" );
            }
            FilesystemLib.CreateDirectory( folderPath );
            DirectoryInfo directoryInfo = new DirectoryInfo( folderPath );
            var filesFound = directoryInfo.GetFiles();
            var filenamesFound = (from f in filesFound select f.Name);
            foreach (var filename in filenamesFound)
            {
                string pathname = Path.Combine( folderPath, filename );
                FilesystemLib.DeleteFile( pathname );
            }
        }

        private void HelpToVerifyHasExactlyTheseFiles( string folderPath, string[] filenamesExpected )
        {
            if (folderPath == null)
            {
                throw new ArgumentNullException( "folderPath" );
            }
            if (filenamesExpected == null)
            {
#if PRE_4
                throw new ArgumentNullException( "filenamesExpected" );
#else
                throw new ArgumentNullException( paramName: nameof( filenamesExpected ) );
#endif
            }
            DirectoryInfo directoryInfo = new DirectoryInfo( folderPath );
            var filesFound = directoryInfo.GetFiles();
            var filenamesFound = (from f in filesFound select f.Name);
            int numberFound = filesFound.Length;
            int numberExpected = filenamesExpected.Length;
            if (numberExpected > 0)
            {
                // Check that every file found there, is on the list of the expected files..
                foreach (var file in filesFound)
                {
                    if (!filenamesExpected.Contains( file.Name ))
                    {
                        throw new InvalidOperationException( String.Format( "Unexpected file {0} found within folderPath {1}", file.Name, folderPath ) );
                    }
                }
                // Check that every file that is expected, is actually there..
                foreach (var filename in filenamesExpected)
                {
                    if (!filenamesFound.Contains( filename ))
                    {
                        throw new FileNotFoundException( String.Format( "File {0} not found within folderPath {1}", filename, folderPath ) );
                    }
                }
            }
            // This is a redundant double-check that the numbers of expected, and found, are the same.
            if (numberFound != numberExpected)
            {
                if (numberExpected == 1 && numberFound == 0)
                {
                    throw new InvalidOperationException( String.Format( "This calls for folder {0} to have 1 file ({1}) but instead it has none.", folderPath, filenamesExpected[0] ) );
                }
                else
                {
                    throw new InvalidOperationException( String.Format( "This calls for folder {0} to have {1} files but instead it has {2}.", folderPath, numberExpected, numberFound ) );
                }
            }
        }

        private void HelpToMakeToHaveNoSubdirectories( string folderPath )
        {
            if (folderPath == null)
            {
                throw new ArgumentNullException( "folderPath" );
            }
            FilesystemLib.CreateDirectory( folderPath );
            DirectoryInfo directoryInfo = new DirectoryInfo( folderPath );
            var directoriesFound = directoryInfo.GetDirectories();
            var directoryNamesFound = (from f in directoriesFound select f.Name);
            foreach (string name in directoryNamesFound)
            {
                string path = Path.Combine( folderPath, name );
                FilesystemLib.DeleteDirectory( path );
            }
        }

        private void HelpToVerifyHasExactlyTheseSubDirectories( string folderPath, string[] subdirectoriesExpected )
        {
            if (folderPath == null)
            {
                throw new ArgumentNullException( "folderPath" );
            }
            if (subdirectoriesExpected == null)
            {
#if PRE_4
              throw new ArgumentNullException( "subdirectoriesExpected" );
#else
                throw new ArgumentNullException( paramName: nameof( subdirectoriesExpected ) );
#endif
            }
            DirectoryInfo directoryInfo = new DirectoryInfo( folderPath );
            var directoriesFound = directoryInfo.GetDirectories();
            var directoryNamesFound = (from d in directoriesFound select d.Name);
            int numberFound = directoriesFound.Length;
            int numberExpected = subdirectoriesExpected.Length;
            if (numberExpected > 0)
            {
                // Check that every folder found there, is on the list of the expected folders..
                foreach (var dirInfoFound in directoriesFound)
                {
                    if (!subdirectoriesExpected.Contains( dirInfoFound.Name ))
                    {
                        throw new InvalidOperationException( String.Format( "Unexpected subfolder {0} found within folderPath {1}", dirInfoFound.Name, folderPath ) );
                    }
                }
                // Check that every subfolder that is expected, is actually there..
                foreach (var directoryName in subdirectoriesExpected)
                {
                    if (!directoryNamesFound.Contains( directoryName ))
                    {
                        throw new FileNotFoundException( String.Format( "Subfolder {0} not found within folderPath {1}", directoryName, folderPath ) );
                    }
                }
            }
            // This is a redundant double-check that the numbers of expected, and found, are the same.
            if (numberFound != numberExpected)
            {
                if (numberExpected == 1 && numberFound == 0)
                {
                    throw new InvalidOperationException( String.Format( "This calls for folder {0} to have 1 subfolder ({1}) but instead it has none.", folderPath, subdirectoriesExpected[0] ) );
                }
                else
                {
                    throw new InvalidOperationException( String.Format( "This calls for folder {0} to have {1} subfolders but instead it has {2}.", folderPath, numberExpected, numberFound ) );
                }
            }
        }

        #endregion private methods

        #region fields

        /// <summary>
        /// This is the filesystem-path of the folder that this ZDirectoryInfo object represents.
        /// </summary>
        private readonly string _path;

#if (NO_LONGPATH)
       private readonly DirectoryInfo _directoryInfo;
#endif

        #endregion fields

        #endregion internal implementation
    }
}