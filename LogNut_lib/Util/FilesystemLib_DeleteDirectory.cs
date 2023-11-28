#if PRE_4
#define PRE_5
#endif
#if (NETFW_462_OR_ABOVE || NETFX_CORE)
#define NO_LONGPATH
#endif
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Hurst.LogNut.Util.Native;


// define PRE_4 if this is to be compiled for versions of .NET earlier than 4.0 (namely, .NET Framework 3.5).
// Compile with the pragma NO_LONGPATH if you want to use only the normal .NET filesystem facilities.

// Note: .NET Framework at 4.62 and above, no longer has the path-length limitation.
// Define the compiler-pragma NETFW_462_OR_ABOVE when targetting this version of .NET.

// Todo:
// I need to make specific provisions for handling the asynchronous nature of flash drives.
// see  https://stackoverflow.com/questions/10834466/force-flush-file-cash-for-a-usb-device-c-sharp


namespace Hurst.LogNut.Util
{
    public static partial class FilesystemLib
    {
        #region DeleteDirectory( directoryPathToDelete )
        /// <summary>
        /// Remove the given filesystem-folder, if it exists, and everything within it.
        /// This calls DeleteDirectoryContent recursively, and then Directory.Delete.
        /// </summary>
        /// <param name="directoryPathToDelete">the directory to remove</param>
        /// <returns>true if the given directory was found, false if it already did not exist</returns>
        /// <exception cref="ArgumentNullException">The directory-path must not be null</exception>
        /// <exception cref="ArgumentException">The directory-path must be syntactically valid</exception>
        /// <remarks>
        /// Unlike <c>Directory.Delete</c>, this tests for the existence of the given folder first.
        /// It is not an error to call this method on a non-existent folder.
        /// 
        /// This also deletes recursively, thus you can delete a folder that contains other folders and files.
        /// </remarks>
        public static bool DeleteDirectory( string directoryPathToDelete )
        {
            // Check the argument value...
            if (directoryPathToDelete == null)
            {
                throw new ArgumentNullException( nameof(directoryPathToDelete) );
            }

            // If the given directory does not exist then we have nothing to do.
            try
            {
                bool didExist = false;
                if (DirectoryExists( directoryPathToDelete ))
                {
                    didExist = true;
                    // First remove anything that may be within this directory.
                    DeleteDirectoryContent( directoryPathToDelete );
                    // Remove any attributes that would render this directory un-removable.
                    SetDirectoryAttributes( directoryPathToDelete, FileAttributes.Normal );
                    // Now finally go ahead and remove this directory.
#if NO_LONGPATH
                    Directory.Delete( correctedPath );
#else
                    bool isDeletedOk = Win32.RemoveDirectory( directoryPathToDelete );
#endif
                    // When testing by deleting 100 directories on a flash-drive, this method would fail if done at full execution speed.
                    // Sleep(1) did not cure it. This figure is purely a guess.
                    Thread.Sleep( 50 );
                }
                return didExist;
            }
            catch (Exception x)
            {
                x.Data.Add( key: "directoryPathToDelete: ", value: StringLib.AsQuotedString( directoryPathToDelete ) );
                throw;
            }
        }
        #endregion

        #region DeleteDirectory( directoryPath, timeoutInMilliseconds )
        /// <summary>
        /// Remove the filesystem-folder denoted by the given directory-path, and everything within it - retrying the operation if needed.
        /// </summary>
        /// <param name="directoryPath">the directory to remove</param>
        /// <param name="timeoutInMilliseconds">how long to keep trying before we give up (in milliseconds)</param>
        /// <returns>true if the given directory was successfully removed, false if it was unable to do so</returns>
        /// <exception cref="ArgumentNullException">The directory-path must not be null</exception>
        /// <remarks>
        /// Unlike <c>Directory.Delete</c>, this tests for the existence of the given folder first and also retries the deletion if necessary.
        /// It is not an error to call this method on a non-existent folder.
        /// 
        /// This also deletes recursively, thus you can delete a folder that contains other folders and files.
        /// </remarks>
        public static bool DeleteDirectory( string directoryPath, int timeoutInMilliseconds )
        {
            //CBL I need to add a retryNotifier
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }
            bool isSuccessful = true;

            // There is nothing to do if the directory is not there.
            if (DirectoryExists( directoryPath ))
            {
                isSuccessful = false;
                bool isDeletedOk = false;
                //CBL Need to check: should I call the conversion for long-paths for every method-call here?

                // First remove any files that are within this directory...

                bool isFilesDeleted = false;
                Stopwatch stopwatch = Stopwatch.StartNew();
                while (!isFilesDeleted)
                {
                    try
                    {
                        foreach (string filePathname in Directory.GetFiles( path: directoryPath ))
                        {
                            isDeletedOk = DeleteFile( filePathname, null, timeoutInMilliseconds );

                            //CBL Possible improvement: should we check after each file-deletion, whether it still exists?
                            if (!isDeletedOk)
                            {
                                // We could not delete that file - so return false to indicate failure.
                                Debug.WriteLine( "Failed to DeleteDirectory( " + directoryPath + " ) because could not delete file " + filePathname + ". " );
                                return false;
                            }
                        } // end loop through files.

                        isFilesDeleted = true;
                    }
                    catch (UnauthorizedAccessException x)
                    {
                        // This is expected when the folder itself being locked causes Directory.GetFiles to throw this exception.
                        if (stopwatch.ElapsedMilliseconds > timeoutInMilliseconds)
                        {
                            x.Data.Add( key: "directoryPath", value: directoryPath );
                            x.Data.Add( key: "Problem", value: "Timed-out trying to GetFiles" );
                            //CBL Or should I throw instead a TimeoutException ?
                            throw x;
                        }
                    }

                } // end retry-loop for file-deletion.


                // Remove any sub-directories..


                //CBL  Should I try this without using recursion? Possible stack-overflow, plus the path-checking gets repeated everytime.
                foreach (var subdirectoryPath in Directory.GetDirectories( path: directoryPath ))
                {
                    isDeletedOk = DeleteDirectory( subdirectoryPath, timeoutInMilliseconds );

                    if (!isDeletedOk)
                    {
                        // We could not delete that file - so return false to indicate failure.
                        Debug.WriteLine( "Failed to DeleteDirectory(" + directoryPath + ") because could not delete sub-directory " + subdirectoryPath );
                        return false;
                    }
                } // end loop through subdirectories.


                // Now finally go ahead and remove this directory...


                // Enter a retry-loop for the deletion of the directory that was actually specified..
                int nAttempt = 1;
                int retryDelay = 50;  // milliseconds
                Exception exceptionFirstThrown = null;

                while (!isSuccessful)
                {
                    try
                    {
                        // Test whether it exists (again) before deleting it, because it might have been deleted already on a previous iteration
                        // of this retry-loop and simply failed to disappear immediately.
                        if (Directory.Exists( path: directoryPath ))
                        {
                            // Try to delete it.
                            if (nAttempt > 1)
                            {
                                try
                                {
                                    SetDirectoryAttributes( directoryPath: directoryPath, attributes: FileAttributes.Normal );
                                }
                                catch (Win32Exception x)
                                {
                                    //CBL ?
                                    Console.WriteLine( "In DeleteDirectory(" + directoryPath + "), SetDirectoryAttributes threw a Win32Exception: " + x.Message );
                                }

                                // Allow just a bit of time for it to happen.
                                Thread.Sleep( 1 );
                            }

                            // DeleteDirectory( directoryPath );  //CBL ?
                            Directory.Delete( path: directoryPath );

                            // Allow just a bit of time for it to happen.
                            Thread.Sleep( 1 );
                            // Verify that it did happen before we claim success.
                            if (!Directory.Exists( path: directoryPath ))
                            {
                                isSuccessful = true;
                            }
                        }
                        else // didn't have to do anything.
                        {
                            isSuccessful = true;
                        }
                    }
                    catch (Exception x)
                    {
                        isSuccessful = false;
                        if (exceptionFirstThrown == null)
                        {
                            exceptionFirstThrown = x;
                        }
                    }

                    if (!isSuccessful)
                    {
                        string msg = String.Format( "Upon attempt {0} Directory.Delete({1}) failed", nAttempt, directoryPath );
                        if (stopwatch.ElapsedMilliseconds > timeoutInMilliseconds)
                        {
                            Debug.WriteLine( msg + " - giving up." );
                            break;
                        }
                        else
                        {
                            Debug.WriteLine( msg + "," + "  will wait " + retryDelay + " ms and try again." );
                            Thread.Sleep( retryDelay );
                            // The delay will proceed through the values 50, 100, 200, 400, 800, 1000 (milliseconds).
                            if (retryDelay < 500)
                            {
                                retryDelay *= 2;
                            }
                            else
                            {
                                retryDelay = 1000;
                            }
                            nAttempt++;
                        }
                    }
                }
                stopwatch.Stop();

                // If we timed out while unsuccessfully trying to delete it..
                if (!isSuccessful)
                {
                    // If an exception happened and we're not suppressing those, add some info to it and rethrow it.
                    if (exceptionFirstThrown != null)
                    {
                        // Add the argument information to the exception.
                        exceptionFirstThrown.Data.Add( key: "directoryPath", value: directoryPath );
                        exceptionFirstThrown.Data.Add( key: "nAttempt", value: nAttempt );
                        exceptionFirstThrown.Data.Add( key: "timeoutInMilliseconds", value: timeoutInMilliseconds );
                        // re-throw the original exception that was initially detected.
                        throw exceptionFirstThrown;
                    }
                    Debug.WriteLine( "Failed to RemoveDirectory( " + directoryPath + ", timeoutInMilliseconds: " + timeoutInMilliseconds + " ). " );
                }
            }
            return isSuccessful;
        }
        #endregion

        #region DeleteDirectoryContent( directoryInfo )
        /// <summary>
        /// Remove any files or sub-directories from the given filesystem-folder denoted by directoryInfo, if it exists.
        /// This is different from DeleteDirectory, in that the directory itself is not deleted.
        /// </summary>
        /// <param name="directoryInfo">the DirectoryInfo to remove the content of</param>
        /// <returns>true if there was content found within the directory that was deleted</returns>
        /// <exception cref="ArgumentNullException">the value provided for directoryPath must not be null</exception>
        /// <exception cref="ArgumentException">the value provided for directoryPath must not be an empty string</exception>
        /// <remarks>
        /// This does not attempt to delete the System Volume Information directory.
        /// </remarks>
        public static bool DeleteDirectoryContent( DirectoryInfo directoryInfo )
        {
            if (directoryInfo == null)
            {
                throw new ArgumentNullException( "directoryInfo" );
            }
            return DeleteDirectoryContent( directoryInfo.FullName );
        }
        #endregion

        #region DeleteDirectoryContent( directoryPath )
        /// <summary>
        /// Remove any files or sub-directories from the given directory-path, if it exists.
        /// This is different from DeleteDirectory, in that the directory itself is not deleted.
        /// </summary>
        /// <param name="directoryPath">the folder to make empty</param>
        /// <returns>true if there was content found within the directory that was deleted</returns>
        /// <exception cref="ArgumentNullException">the value provided for directoryPath must not be null</exception>
        /// <remarks>
        /// This does not attempt to delete the System Volume Information directory.
        /// </remarks>
        public static bool DeleteDirectoryContent( string directoryPath )
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }
            return DeleteDirectoryContent( directoryPath, null );
        }
        #endregion

        #region DeleteDirectoryContent( directoryPath, pathnameOfFileToExclude )
        /// <summary>
        /// Remove any files or sub-directories from the given directory-path, if it exists - except for the paths specified to exclude.
        /// This is slightly different from DeleteDirectory, in that the directory itself is not deleted.
        /// </summary>
        /// <param name="directoryPath">the folder to make empty</param>
        /// <param name="pathnameOfFileToExclude">the name of a file to not delete, if it is found anywhere within the directory-tree (may be null)</param>
        /// <returns>true if there was content found within the directory that was deleted</returns>
        /// <exception cref="ArgumentNullException">the value provided for directoryPath must not be null</exception>
        /// <exception cref="ArgumentException">the value provided for directoryPath must not be an empty string</exception>
        /// <remarks>
        /// This does not attempt to delete the System Volume Information directory.
        /// </remarks>
        public static bool DeleteDirectoryContent( string directoryPath, string pathnameOfFileToExclude )
        {
            //CBL Test this against deleting from the root dir, but be cautious!!
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }

            try
            {
                // If there are no subfolders nor files, then leave the result at false.
                bool didDelete = false;

                if (Directory.Exists( directoryPath ))
                {
                    // Delete any files...
#if !PRE_4
                    foreach (string filePathname in Directory.EnumerateFiles( path: directoryPath ))
#else
                    foreach (string filePathname in Directory.GetFiles( directoryPath ))
#endif
                    {
                        string filename = FileStringLib.GetFileNameFromFilePath( filePathname );
                        //CBL Need to consider that pathname to exclude could be just the filename, or a complete pathname.
                        if (pathnameOfFileToExclude != null && filename.Equals( pathnameOfFileToExclude ))
                        {
                            continue;
                        }
                        // Remove any attributes that would render this file un-removable.
                        SetFileReadonly( filePathname, false );
                        //CBL Is ReadOnly the only attribute we need to worry about here?
                        //SetFileAttributes( filePathname, FileAttributes.Normal );

                        File.Delete( filePathname );

                        didDelete = true;
                    }
                    // If any files were deleted, yield a slight bit of time to other threads before proceeding.
                    if (didDelete)
                    {
                        Thread.Sleep( 1 );
                    }

                    // Delete any directories...
                    bool didDeleteDir = false;
#if !PRE_4
                    foreach (string subdirectoryPath in Directory.EnumerateDirectories( path: directoryPath ))
#else
                    foreach (string subdirectoryPath in Directory.GetDirectories( directoryPath ))
#endif
                    {
                        // Skip past the System Volume Information folder.
                        if (!subdirectoryPath.Contains( FileStringLib.NameOfSystemVolInforDirectory ))
                        {
                            // Remove any attributes that would render this directory un-removable.
                            SetDirectoryAttributes( subdirectoryPath, FileAttributes.Normal );
                            // Remove anything within the directory.
                            DeleteDirectoryContent( directoryPath: subdirectoryPath, pathnameOfFileToExclude: pathnameOfFileToExclude );
                            // Allow a bit of time for that to take effect.
                            Thread.Sleep( 100 );

                            // Now finally go ahead and remove this directory.
                            Directory.Delete( subdirectoryPath );

                            didDelete = true;
                            didDeleteDir = true;
                        }
                    }

                    // If any directories were deleted, yield a slight bit of time to other threads before proceeding.
                    if (didDeleteDir)
                    {
                        Thread.Sleep( 1 );
                    }
                }
                return didDelete;
            }
            catch (Exception x)
            {
                const string Key1 = "directoryPath:";
                if (!x.Data.Contains( Key1 ))
                {
                    x.Data.Add( key: Key1, value: StringLib.AsQuotedString( directoryPath ) );
                }

                const string Key2 = "pathnameOfFileToExclude:";
                if (!x.Data.Contains( Key2 ))
                {
                    x.Data.Add( key: Key2, value: StringLib.AsQuotedString( pathnameOfFileToExclude ) );
                }
                throw;
            }
        }
        #endregion

        #region DeleteDirectoryContentFiles( directoryPath, fileSpec )
        /// <summary>
        /// Given a filesystem-folder, if it exists - remove any files within it that match the given file-specification, and retry if necessary.
        /// The directory itself is not deleted. The retry-timelimit is set to the default value (5 seconds).
        /// </summary>
        /// <param name="directoryPath">the folder to delete content from</param>
        /// <param name="fileSpec">a file-spec (which may contain wildcard characters) that denotes what to delete</param>
        /// <exception cref="ArgumentNullException">The value provided for the directory-path must not be null.</exception>
        /// <exception cref="ArgumentException">The value provided for the directory-path must be a syntactically-valid path.</exception>
        /// <remarks>
        /// The retry-delay will proceed through 25, 50, 100, 200, 400, 800, 1000, 2000, 3000, 4000, 5000 milliseconds (or whatever the timeout is set to).
        /// </remarks>
        public static void DeleteDirectoryContentFiles( string directoryPath, string fileSpec )
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }
            if (fileSpec == null)
            {
                throw new ArgumentNullException( "fileSpec" );
            }
            if (DirectoryExists( directoryPath ))
            {
                foreach (string filePathname in Directory.GetFiles( path: directoryPath, searchPattern: fileSpec, searchOption: SearchOption.TopDirectoryOnly ))
                {
                    DeleteFile( filePathname );
                }
            }
        }
        #endregion

        #region DeleteDirectoryContentFiles( directoryPath, fileSpec, timeoutInMilliseconds )
        /// <summary>
        /// Given a filesystem-folder, if it exists - remove any files within it that match the given file-specification, and retry if necessary.
        /// The directory itself is not deleted.
        /// </summary>
        /// <param name="directoryPath">the folder to delete content from</param>
        /// <param name="fileSpec">a file-spec (which may contain wildcard characters) that denotes what to delete</param>
        /// <param name="timeoutInMilliseconds">a limit on how long to keep trying before giving up, in milliseconds (optional - default is 5 seconds)</param>
        /// <returns>true if was able to successfully remove all files that were found (or if none were found), false if it was unable to do so</returns>
        /// <exception cref="ArgumentNullException">The value provided for the directory-path must not be null.</exception>
        /// <exception cref="ArgumentException">The value provided for the directory-path must be a syntactically-valid path.</exception>
        /// <remarks>
        /// The retry-delay will proceed through 50, 100, 200, 400, 800, 1000, 2000, 3000, 4000, 5000 milliseconds (or whatever the timeout is set to).
        /// </remarks>
        public static bool DeleteDirectoryContentFiles( string directoryPath, string fileSpec, int timeoutInMilliseconds )
        {
            //CBL And how is this different from simply equiping, for example, DeleteFile or DeleteDirectory, with wildcard capabilities?
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }
            if (fileSpec == null)
            {
                throw new ArgumentNullException( "fileSpec" );
            }

            bool wasDeletedOk = true;
            if (DirectoryExists( directoryPath ))
            {
                foreach (string filePathname in Directory.GetFiles( path: directoryPath, searchPattern: fileSpec, searchOption: SearchOption.TopDirectoryOnly ))
                {
                    // DeleteFile does itself have a retry-loop, thus there is no need to incorporate one into *this* method.
                    bool isThisDeletedOk = DeleteFile( filePathname, null, timeoutInMilliseconds: timeoutInMilliseconds );

                    if (!isThisDeletedOk)
                    {
                        wasDeletedOk = false;
                        Debug.WriteLine( "DeleteDirectoryContentFiles(" + directoryPath + ", timeoutInMilliseconds: " + timeoutInMilliseconds + ") failed on file " + filePathname );
                    }
                }
            }

            return wasDeletedOk;
        }
        #endregion

        #region DeleteDirectoryContentSubdirectories( directoryPath )
        /// <summary>
        /// Given a filesystem-folder, if it exists - remove any subfolders within it.
        /// The directory denoted by <paramref name="directoryPath"/> is not itself deleted.
        /// </summary>
        /// <param name="directoryPath">the folder to delete content from</param>
        /// <returns>true if all subdirectories found were successfully deleted, or the directory did not exist to begin with</returns>
        /// <exception cref="ArgumentNullException">the value provided for directoryPath must not be null</exception>
        public static bool DeleteDirectoryContentSubdirectories( string directoryPath )
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }
            // If folderSpec can be null, then we could just call the other overload of this.  CBL

            bool wasDeletedOk = true;
            if (Directory.Exists( directoryPath ))
            {
#if !PRE_4
                foreach (string subdirectoryPath in Directory.EnumerateDirectories( path: directoryPath ))
#else
                foreach (string subdirectoryPath in Directory.GetDirectories( directoryPath ))
#endif
                {
                    bool  isThisDeletedOk = DeleteDirectory( subdirectoryPath );

                    if (!isThisDeletedOk)
                    {
                        wasDeletedOk = false;
                        Debug.WriteLine( "DeleteDirectoryContentSubdirectories(" + directoryPath + ") failed on subdirectory " + subdirectoryPath );
                    }
                }
            }
            return wasDeletedOk;
        }
        #endregion

        #region DeleteDirectoryContentSubdirectories( directoryPath, folderSpec )
        /// <summary>
        /// Given a filesystem-folder, if it exists - remove any subfolders within it that match the given file-specification
        /// (which may include wildcard characters).
        /// The directory denoted by <paramref name="directoryPath"/> is not itself deleted.
        /// </summary>
        /// <param name="directoryPath">the folder to delete content from</param>
        /// <param name="folderSpec">a file-spec (which may contain wildcard characters) that denotes which folders to delete</param>
        /// <returns>true if all subdirectories found were successfully deleted, or the directory did not exist to begin with</returns>
        /// <exception cref="ArgumentNullException">the value provided for directoryPath must not be null</exception>
        public static bool DeleteDirectoryContentSubdirectories( string directoryPath, string folderSpec )
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }

            bool wasDeletedOk = true;
            if (Directory.Exists( directoryPath ))
            {
#if !PRE_4
                foreach (string subdirectoryPath in Directory.EnumerateDirectories( path: directoryPath, searchPattern: folderSpec, searchOption: SearchOption.TopDirectoryOnly ))
#else
                foreach (string subdirectoryPath in Directory.GetDirectories( directoryPath, searchPattern: folderSpec, searchOption: SearchOption.TopDirectoryOnly ))
#endif
                {
                    bool  isThisDeletedOk = DeleteDirectory( subdirectoryPath );

                    if (!isThisDeletedOk)
                    {
                        wasDeletedOk = false;
                        Debug.WriteLine( "DeleteDirectoryContentSubdirectories(" + directoryPath + ", folderSpec: " + folderSpec + ") failed on subdirectory " + subdirectoryPath );
                    }
                }
            }
            return wasDeletedOk;
        }
        #endregion

        #region DeleteDirectoryContentSubdirectories( directoryPath, folderSpec, timeoutInMilliseconds )
        /// <summary>
        /// Given a filesystem-folder, if it exists - remove any subfolders that are within it that match the given file-specification
        /// (which may include wildcard characters). If any of the deletions fail - retry them.
        /// The directory denoted by <paramref name="directoryPath"/> is not itself deleted.
        /// </summary>
        /// <param name="directoryPath">the folder to delete content from</param>
        /// <param name="folderSpec">a file-spec (which may contain wildcard characters) that denotes which folders to delete. Set this to null to indicate all.</param>
        /// <param name="timeoutInMilliseconds">a limit on how long to keep trying before giving up, in milliseconds (optional - default is 5 seconds)</param>
        /// <returns>true if all subdirectories found were successfully deleted, or the directory did not exist to begin with</returns>
        /// <exception cref="ArgumentNullException">the value provided for directoryPath must not be null</exception>
        /// <remarks>
        /// The retry-delay will proceed through 50, 100, 200, 400, 800, 1000, 2000, 3000.. 10000 milliseconds (or whatever the timeout is set to).
        /// </remarks>
        public static bool DeleteDirectoryContentSubdirectories( string directoryPath, string folderSpec, int timeoutInMilliseconds )
        {
            //CBL And how is this different from simply equiping, for example, DeleteFile or DeleteDirectory, with wildcard capabilities?
            //CBL Test with null for folderSpec, to see whether all these overloads are really needed.
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }
            bool wasDeletedOk = true;

            // There is nothing to do if the directory is not there.
            if (DirectoryExists( directoryPath ))
            {
#if !PRE_4
                foreach (string subdirectoryPath in Directory.EnumerateDirectories( path: directoryPath, searchPattern: folderSpec, searchOption: SearchOption.TopDirectoryOnly ))
#else
                foreach (string subdirectoryPath in Directory.GetDirectories( directoryPath, searchPattern: folderSpec, searchOption: SearchOption.TopDirectoryOnly ))
#endif
                {
                    if (!subdirectoryPath.Contains( FileStringLib.NameOfSystemVolInforDirectory ))
                    {
                        // Do the deletion.
                        bool isThisDeletionSuccessful = DeleteDirectory( subdirectoryPath, timeoutInMilliseconds );

                        if (!isThisDeletionSuccessful)
                        {
                            wasDeletedOk = false;
                            string msg =  "DeleteDirectoryContentSubdirectories(" + directoryPath + ", folderSpec: " + StringLib.AsString( folderSpec ) + "," + timeoutInMilliseconds + ") failed on subdirectory " + subdirectoryPath ;
                            Debug.WriteLine( msg );
                        }
                    }
                } // end loop through sub-directories.
            }
            return wasDeletedOk;
        }
        #endregion
    }
}
