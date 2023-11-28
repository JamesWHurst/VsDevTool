using System;
using System.IO;


// Compile with the pragma NO_LONGPATH if you want to use only the normal .NET filesystem facilities.


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// Intended to mirror the System.IO.Directory class to provide some static methods (CreateDirectory, Delete, Exists)
    /// but capable of handling longer pathnames.
    /// </summary>
    public static class ZDirectory
    {
        #region CreateDirectory
        /// <summary>
        /// Create a new folder on the filesystem.
        /// </summary>
        /// <param name="folderPath">the pathname of the folder to create</param>
        /// <exception cref="ArgumentNullException">folderPath must not be null</exception>
        public static void CreateDirectory( string folderPath )
        {
            if (folderPath == null)
            {
                throw new ArgumentNullException( "folderPath" );
            }

#if (NETFX_CORE || NO_LONGPATH)

            if (!Directory.Exists( folderPath ))
            {
                Directory.CreateDirectory( folderPath );
            }

#else
            if (!FilesystemLib.DirectoryExists( folderPath ))
            {
                FilesystemLib.CreateDirectory( folderPath );
            }
#endif
        }

        /// <summary>
        /// Create a new filesystem-folder within the given parent-folder.
        /// </summary>
        /// <param name="pathOfParentFolder">the full path of the folder within which to create the new one</param>
        /// <param name="nameOfFolderToCreate">the name (not the full path) of the new folder to create</param>
        /// <exception cref="ArgumentNullException">The given paths must not be null</exception>
        /// <remarks>
        /// This particular method simply combines the two paths and calls <c>CreateDirectory</c>.
        /// </remarks>
        public static void CreateDirectory( string pathOfParentFolder, string nameOfFolderToCreate )
        {
            if (pathOfParentFolder == null)
            {
                throw new ArgumentNullException( "pathOfParentFolder" );
            }
            if (nameOfFolderToCreate == null)
            {
                throw new ArgumentNullException( "nameOfFolderToCreate" );
            }

            string folderPath = Path.Combine( pathOfParentFolder, nameOfFolderToCreate );
            CreateDirectory( folderPath );
        }
        #endregion

        #region Delete
        /// <summary>
        /// Delete the specified folder.
        /// </summary>
        /// <param name="folderPath">the pathname of the folder to remove</param>
        /// <remarks>
        /// Unlike <c>Directory.Delete</c>, this tests for the existence of the given folder first.
        /// It is not an error to call this method on a non-existant folder.
        /// 
        /// This also deletes recursively, thus you can delete a folder that contains other folders and files.
        /// </remarks>
        public static void Delete( string folderPath )
        {
            FilesystemLib.DeleteDirectory( folderPath );
        }
        #endregion

        #region Exists
        /// <summary>
        /// Get whether the folder represented by the given path does in fact exist at this moment.
        /// </summary>
        public static bool Exists( string folderPath )
        {
#if (NETFX_CORE || NO_LONGPATH)

            return Directory.Exists( folderPath );

#else

            return FilesystemLib.DirectoryExists( folderPath );

#endif
        }
        #endregion

        #region GetFiles
        /// <summary>
        /// Return the files within the given folder as an array of <see cref="ZFileInfo"/> objects.
        /// </summary>
        /// <param name="directoryPath">the path of the folder to get the files from</param>
        /// <returns>an array of <see cref="ZFileInfo"/> objects representing the files that were found</returns>
        /// <exception cref="ArgumentNullException">The value provided for <paramref name="directoryPath"/> must not be null.</exception>
        public static ZFileInfo[] GetFiles( string directoryPath )
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }
            ZDirectoryInfo dirInfo = new ZDirectoryInfo( directoryPath );
            return dirInfo.GetFiles();
        }

        /// <summary>
        /// Return the files that match the given file-spec within the given folder as an array of <see cref="ZFileInfo"/> objects.
        /// </summary>
        /// <param name="directoryPath">the path of the folder to get the files from</param>
        /// <param name="fileSpec">the wildcard-expression to match the file-names against</param>
        /// <returns>an array of <see cref="ZFileInfo"/> objects representing the matching files that were found</returns>
        /// <exception cref="ArgumentNullException">The values provided for <paramref name="directoryPath"/> and <paramref name="fileSpec"/> must not be null.</exception>
        public static ZFileInfo[] GetFiles( string directoryPath, string fileSpec )
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }
            ZDirectoryInfo dirInfo = new ZDirectoryInfo( directoryPath );
            return dirInfo.GetFiles( fileSpec );
        }
        #endregion GetFiles

        #region GetParent
        /// <summary>
        /// Return the parent directory of that of the specified ZDirectoryInfo.
        /// </summary>
        /// <param name="ofWhatDirectory">the filesystem-path to get the parent of</param>
        /// <returns>a new ZDirectoryInfo representing the parent directory</returns>
        /// <exception cref="ArgumentNullException">The value provided for <paramref name="ofWhatDirectory"/> must not be null.</exception>
        public static ZDirectoryInfo GetParent( this ZDirectoryInfo ofWhatDirectory )
        {
            if (ofWhatDirectory == null)
            {
                throw new ArgumentNullException( "ofWhatDirectory" );
            }
            var parentDirectoryInfo = Directory.GetParent( ofWhatDirectory.FullName );
            return new ZDirectoryInfo( parentDirectoryInfo.FullName );
        }

        /// <summary>
        /// Return the parent directory of the specified path.
        /// </summary>
        /// <param name="ofWhatDirectory">the filesystem-path to get the parent of</param>
        /// <returns>the parent directory of the given path</returns>
        /// <exception cref="ArgumentNullException">The value provided for <paramref name="ofWhatDirectory"/> must not be null.</exception>
        public static ZDirectoryInfo GetParent( this string ofWhatDirectory )
        {
            if (ofWhatDirectory == null)
            {
                throw new ArgumentNullException( "ofWhatDirectory" );
            }
            var parentDirectoryInfo = Directory.GetParent( ofWhatDirectory );
            return new ZDirectoryInfo( parentDirectoryInfo.FullName );
        }
        #endregion GetParent

        #region IsParentOf
        /// <summary>
        /// Return true if the given directory has possibleParentDirectory within it's parent-directory path.
        /// Eg, if directory is \Red\Green\Blue, and possibleParentDirectory is \Red, then the answer is true.
        /// If they are the same - return false.
        /// directory may actually a file - same result. 
        /// </summary>
        /// <param name="possibleParentDirectory">the ZDirectoryInfo for which we want to know whether is a PARENT of ofWhatDirectory</param>
        /// <param name="ofWhatDirectory">the ZDirectoryInfo for which we want to know whether this possibleParentDirectory is a parent</param>
        /// <returns>true if possibleParentDirectory is within the parent-path of directory</returns>
        /// <exception cref="ArgumentNullException">The values provided for possibleParentDirectory and ofWhatDirectory must not be null.</exception>
        public static bool IsParentOf( this ZDirectoryInfo possibleParentDirectory, ZDirectoryInfo ofWhatDirectory )
        {
            if (possibleParentDirectory == null)
            {
                throw new ArgumentNullException( "possibleParentDirectory" );
            }
            if (ofWhatDirectory == null)
            {
                throw new ArgumentNullException( "ofWhatDirectory" );
            }
#if !PRE_4
            return FileStringLib.IsParentOf( possibleParentDirectory: possibleParentDirectory.FullName, directory: ofWhatDirectory.FullName );
#else
            return FileStringLib.IsParentOf( possibleParentDirectory.FullName, ofWhatDirectory.FullName );
#endif
        }
        #endregion IsParentOf

        #region IsRootDirectory
        /// <summary>
        /// Given a ZDirectoryInfo, return true if it consists of only the root folder (whether or not it includes the drive-letter).
        /// Eg, returns true for "\", and also for "C:\". Spaces are trimmed first.
        /// </summary>
        /// <param name="zDirectoryInfo">a ZDirectoryInfo denoting the filesystem-path to test</param>
        /// <returns>true if the given path is the root folder</returns>
        /// <exception cref="ArgumentNullException">The value provided for zDirectoryInfo must not be null.</exception>
        public static bool IsRootDirectory( this ZDirectoryInfo zDirectoryInfo )
        {
            if (zDirectoryInfo == null)
            {
                throw new ArgumentNullException( "zDirectoryInfo" );
            }
            bool result = false;
            string path = zDirectoryInfo.FullName.Trim();
            if (path.Length == 3 && StringLib.IsEnglishAlphabetLetter( path[0] ) && path[1] == ':' && (path[2] == '\\' || path[2] == '/'))
            {
                // Path begins with "X:\" or "X:/".
                result = true;
            }
            else if (path.Length == 1 && (path[0] == '\\' || path[0] == '/'))
            {
                // No drive. Path begins with "\" or "/".
                result = true;
            }
            return result;
        }
        #endregion
    }
}
