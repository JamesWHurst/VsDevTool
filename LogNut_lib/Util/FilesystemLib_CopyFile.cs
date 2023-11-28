using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
#if !NETFX_CORE
using Hurst.LogNut.Util.Native;
#endif


namespace Hurst.LogNut.Util
{
    public static partial class FilesystemLib
    {
        // These methods all use one implementation of CopyFile.
        // Those which attempt to retry the operation include the parameters retryNotifier and timeoutInMilliseconds.


        #region CopyFile( sourceFilePath, destinationFilePath )
        /// <summary>
        /// Copy the source file to the destination file-pathname.
        /// This is a simple Copy - with no rollover and no retrying.
        /// </summary>
        /// <param name="sourceFilePath">the pathname of the source-file to copy from</param>
        /// <param name="destinationFilePath">the pathname to copy to -- this becomes the name of the new file</param>
        /// <returns>true if no errors are indicated</returns>
        /// <exception cref="ArgumentNullException">the value provided for either path must not be null</exception>
        public static bool CopyFile( string sourceFilePath,
                                     string destinationFilePath )
        {
            return CopyFile( sourceFilePath, destinationFilePath, false, null, 0 );
        }
        #endregion

        #region CopyFile( sourceFilePath, destinationFilePath, isToRolloverExistingDestinFile )
        /// <summary>
        /// Copy the source file to the destination file-pathname, optionally creating a rolled-over copy of any pre-existing file at the destination path.
        /// This is a simple Copy - with no retrying.
        /// </summary>
        /// <param name="sourceFilePath">the pathname of the source-file to copy from</param>
        /// <param name="destinationFilePath">the pathname to copy to -- this becomes the name of the new file</param>
        /// <param name="isToRolloverExistingDestinFile">if true - then rollover the destination-filename if it already exists, so that it does not get overwritten</param>
        /// <returns>true if no errors are indicated</returns>
        /// <exception cref="ArgumentNullException">the value provided for either path must not be null</exception>
        /// <exception cref="DriveNotFoundException">the destination-path must have a valid drive</exception>
        /// <remarks>
        /// Rollover:
        /// If isToRolloverExistingDestinFile is true, and there already exists a file of the same name at the destination,
        /// and that file does not have the same content - then a backup copy (ie a "rollover") of it is made by calling FilesystemLib.Rollover.
        /// 
        /// If the original outputFilename is "File.txt" and it exists, and "File(1).txt" does not exist, then
        /// File.txt gets renamed to "File(1).txt"
        ///   File.txt -> File(1).txt
        ///
        /// If File.txt exists and File(1).txt also exists - then File.txt gets renamed to File(2).txt
        ///   File.txt -> File(2).txt
        ///
        /// The highest-numbered file is the newest, and it attempts to not disturb any other backup-files until maxBackups is reached.
        ///
        /// If maxBackups is (for example) 3, and there are already all three backup versions present, then the oldest file (which is "File(1).txt") is deleted
        /// and all the others are renamed downward, such that File(2).txt is renamed to File(1).txt and File(3).txt is renamed to File(2).txt
        /// and then File.txt is rolled-over (renamed) to File(3).txt .
        ///   File(1).txt deleted
        ///   File(2).txt -> File(1).txt
        ///   File(3).txt -> File(2).txt
        ///   File.txt    -> File(3).txt  and this is now the newest version.
        /// </remarks>
        public static bool CopyFile( string sourceFilePath,
                                     string destinationFilePath,
                                     bool isToRolloverExistingDestinFile )
        {
            return CopyFile( sourceFilePath, destinationFilePath, isToRolloverExistingDestinFile, null, 0 );
        }
        #endregion

        #region CopyFile( sourceFilePath, destinationFilePath, timeoutInMilliseconds )
        /// <summary>
        /// Copy the source file to the destination file-pathname, retrying the operation if necessary.
        /// </summary>
        /// <param name="sourceFilePath">the pathname of the source-file to copy from</param>
        /// <param name="destinationFilePath">the pathname to copy to -- this becomes the full pathname of the new file</param>
        /// <param name="timeoutInMilliseconds">a limit on how long to keep trying before giving up, in milliseconds</param>
        /// <exception cref="ArgumentNullException">the value provided for either path must not be null</exception>
        /// <exception cref="ArgumentException">the value provided for either path must be a syntactically-valid path</exception>
        /// <remarks>
        /// This is an overload of the method
        ///   CopyFile( string sourceFilePath,
        ///             string destinationFilePath,
        ///             bool isToRolloverExistingDestinFile,
        ///             Action actionForRetryNotification,
        ///             int timeoutInMilliseconds ),
        ///  with isToRolloverExistingDestinFile set to false, actionForRetryNotification set to null.
        /// 
        /// On the full .NET Framework, this encapsulates the Win32 CopyFile function -- and adds the additional functionality.
        /// </remarks>
        public static void CopyFile( string sourceFilePath,
                                     string destinationFilePath,
                                     int timeoutInMilliseconds )
        {
            CopyFile( sourceFilePath, destinationFilePath, false, retryNotifier: null, timeoutInMilliseconds: timeoutInMilliseconds );
        }
        #endregion

        #region CopyFile( sourceFilePath, destinationFilePath, isToRolloverExistingDestinFile, retryNotifier, timeoutInMilliseconds )
        /// <summary>
        /// Copy the source file to the destination file-pathname, retrying the operation if necessary.
        /// </summary>
        /// <param name="sourceFilePath">the pathname of the source-file to copy from</param>
        /// <param name="destinationFilePath">the pathname to copy to -- this becomes the full pathname of the new file</param>
        /// <param name="isToRolloverExistingDestinFile">if true - then rollover the destination-filename if it already exists, so that it does not get overwritten</param>
        /// <param name="retryNotifier">an delegate to call when a retry occurs (may be null)</param>
        /// <param name="timeoutInMilliseconds">a limit on how long to keep trying before giving up, in milliseconds. Zero indicates no retrying.</param>
        /// <returns>true if no errors are indicated</returns>
        /// <exception cref="ArgumentNullException">the value provided for either path must not be null</exception>
        /// <exception cref="ArgumentException">the value provided for either path must be a syntactically-valid path</exception>
        /// <remarks>
        /// On the full .NET Framework, this encapsulates the Win32 CopyFile function -- and adds the additional functionality.
        ///
        /// If the destinationFilePath already exists = this overwrites it.
        ///
        /// Rollover:
        /// If isToRolloverExistingDestinFile is set to true, and there already exists a file of the same name at the destination,
        /// and that file does not have the same content - then a backup copy of it is made by calling FilesystemLib.Rollover.
        /// 
        /// The delay between retries is initially 50ms, and then doubles thereafter until it reaches 1 second, then retries each second until it times-out.
        /// Thus the retry-delays are 50, 100, 200, 400, 800, 1000, 1000, ...
        /// </remarks>
        public static bool CopyFile( string sourceFilePath,
                                     string destinationFilePath,
                                     bool isToRolloverExistingDestinFile,
                                     FileProgressNotifier retryNotifier,
                                     int timeoutInMilliseconds )
        {
            // Validate the arguments..
            if (sourceFilePath == null)
            {
                throw new ArgumentNullException( "sourceFilePath" );
            }
            if (destinationFilePath == null)
            {
                throw new ArgumentNullException( "destinationFilePath" );
            }
            // Confirm that the source and destination are not the same.
            if (sourceFilePath.Equals( destinationFilePath, StringComparison.OrdinalIgnoreCase ))
            {
                throw new ArgumentException( "Source and Destination paths must not be the same.", "destinationFilePath" );
            }

            bool isSuccessful = true;

            try
            {

                // Confirm that the source-file does exist.
                bool sourceDrivePresent = true;
                if (File.Exists( sourceFilePath ))
                {
                    // If this is allowing for retries (timeout > 0) then check to see whether the source-file is locked...
                    if (timeoutInMilliseconds > 0)
                    {
                        int i = 0;
                        bool isTimedOut = false;
                        var stopwatch = Stopwatch.StartNew();
                        while (IsFileLocked( sourceFilePath ))
                        {
                            //Console.WriteLine("CopyFile waiting " + RetryIntervals[i] + " ms for source to unlock.");
                            if (retryNotifier != null)
                            {
                                retryNotifier( isRetry: true, isDirectory: false, message: "Waiting " + RetryIntervals[i] + "ms for source to unlock." );
                            }

                            Thread.Sleep( RetryIntervals[i] );
                            if (stopwatch.ElapsedMilliseconds > timeoutInMilliseconds)
                            {
                                isTimedOut = true;
                                break;
                            }

                            if (i < RetryIntervals.Length - 1)
                            {
                                i++;
                            }
                        }

                        stopwatch.Stop();

                        if (isTimedOut)
                        {
                            throw new TimeoutException( message: "CopyFile timed-out waiting for " + sourceFilePath + " to become unlocked." );
                        }

                        //else
                        //{
                        //    Console.WriteLine("CopyFile - source lock has released.");
                        //}
                    }
                }
                else // source does NOT exist.
                {
                    if (DriveExists( sourceFilePath ))
                    {
                        throw new FileNotFoundException( @"Could not find sourceFilePath """ + sourceFilePath + @"""" );
                    }
                    else // bad drive
                    {
                        // Assuming this may be a USB-connected drive that is only intermittently connected, keep trying to connect to it
                        // for as long as the specified timeout.  CBL This needs testing.
                        sourceDrivePresent = false;
                        string drive = FileStringLib.GetDrive(sourceFilePath);

                        if (drive != null)
                        {
                            if (timeoutInMilliseconds > 0)
                            {
                                int i = 0;
                                int n = 1;
                                var stopwatch = Stopwatch.StartNew();

                                while (stopwatch.ElapsedMilliseconds <= timeoutInMilliseconds)
                                {
                                    if (retryNotifier != null)
                                    {
                                        retryNotifier( isRetry: true, isDirectory: false, message: n.ToString() + ": waiting " + RetryIntervals[i] + "ms for drive to return." );
                                    }

                                    Thread.Sleep( RetryIntervals[i] );

                                    if (DriveExists( drive ))
                                    {
                                        sourceDrivePresent = true;
                                        break;
                                    }

                                    if (i < RetryIntervals.Length - 1)
                                    {
                                        i++;
                                    }

                                    n++;
                                } // end loop.

                                stopwatch.Stop();

                                if (sourceDrivePresent)
                                {
                                    if (retryNotifier != null)
                                    {
                                        retryNotifier( isRetry: true, isDirectory: false, message: n.ToString() + ": Source drive is back up!" );
                                    }
                                }
                                else
                                {
                                    throw new DriveNotFoundException( message: "Timed out: Source Drive " + drive + " not found." );
                                }
                            }
                            else
                            {
                                if (!DriveExists( drive ))
                                {
                                    throw new DriveNotFoundException( message: "Source Drive " + drive + " not found." );
                                }
                            }
                        }

                        //CBL What if drive is null ?
                    }
                }

                // Make a backup copy of the destination file if that file already exists and a rollover is called for.
                if (isToRolloverExistingDestinFile)
                {
                    if (FileExists( destinationFilePath ))
                    {
                        // There is no need to backup the destination file if the newer one has the exact same content.
                        if (!FilesHaveSameContent( sourceFilePath, destinationFilePath ))
                        {
#if PRE_4
                        Rollover( destinationFilePath, null, 100 );
#else
                            Rollover( outputFilePathname: destinationFilePath, archiveFileFolder: null, maxBackups: 100 );
#endif
                        }
                    }
                }

#if NETFX_CORE
            isSuccessful = true;
            File.Copy( sourceFileName: sourceFilePath, destFileName: destinationFilePath );
#else
                // Ensure the destination's directory exists..
                string destinationDirectory = FileStringLib.GetDirectoryOfPath(destinationFilePath);
                if (DirectoryExists( destinationDirectory ))
                {
                    if (FileExists( destinationFilePath ))
                    {
                        if (timeoutInMilliseconds == 0)
                        {
                            // Is the destination-file Hidden or Read-Only ?
                            var destinationFileAttributes = GetFileAttributes(destinationFilePath);
                            if ((destinationFileAttributes & (FileAttributes.Hidden | FileAttributes.ReadOnly)) != 0)
                            {
                                // When the destination-file existed and was Hidden or Read-Only, that was causing the Win32.CopyFile to fail with a lastWin32Error of 5.
                                DeleteFile( destinationFilePath );
                            }
                        }
                        else // the method-call calls for retrying, so we will let DeleteFile do the retry-loop for us.
                        {
                            //CBL Should we provide the retry-notification here?
                            DeleteFile( destinationFilePath, retryNotifier, timeoutInMilliseconds );
                        }
                    }
                }
                else // the destination-directory does not yet exist.
                {
                    // As long as the drive-part is valid, create the destination-directory.
                    if (DriveExists( destinationDirectory ))
                    {
                        CreateDirectory( destinationDirectory );
                    }
                    else // bad drive
                    {
                        string drive = FileStringLib.GetDrive(destinationDirectory);
                        throw new DriveNotFoundException( message: "Destination Drive " + drive + " not found." );
                    }
                }


                // Do the file-copy operation.
                isSuccessful = !Native.Win32.CopyFile( sourceFilePath, destinationFilePath, false );


                var lastWin32Error = Marshal.GetLastWin32Error();
                // Sometimes it returns false, yet lastWin32Error indicates success. Check for that..
                if (lastWin32Error == Win32.ERROR_SUCCESS)
                {
                    isSuccessful = true;
                }

                // When destination-file is Hidden, isSuccessful is true but lastWin32Error is 5, "Access is denied".
                if (lastWin32Error == Win32.ERROR_ACCESS_DENIED || lastWin32Error == Win32.ERROR_SHARING_VIOLATION || lastWin32Error == Win32.ERROR_PATH_NOT_FOUND)
                {
                    isSuccessful = false;
                    // If the destination is specified to be just an existing folder, that also results in ERROR_ACCESS_DENIED. Let's give a more specific error-message.
                    if (lastWin32Error == Win32.ERROR_ACCESS_DENIED)
                    {
                        if (DirectoryExists( destinationFilePath ))
                        {
                            throw new ArgumentException( message: "The destination-pathname is actually a directory.", paramName: nameof( destinationFilePath ) );
                        }
                    }
                }
#endif
                if (isSuccessful)
                {
                    if (FileExists( destinationFilePath ))
                    {
                        // This seems to be redundant. The existing Copy method already copies the file-attributes to the new file.
                        // if ((sourceAttributes & FileAttributes.Hidden) != 0)
                        // {
                        // SetFileAttributes( destinationFilePath, sourceAttributes );
                        // }
                    }
                    else
                    {
                        isSuccessful = false;
                    }
                }
            }
            catch (Exception x)
            {
                x.Data.Add( key: "sourceFilePath", value: sourceFilePath );
                x.Data.Add( key: "destinationFilePath", value: destinationFilePath );
                throw;
            }

            return isSuccessful;
        }
        #endregion CopyFile( sourceFileName, destFileName, isToRolloverExistingDestinFile, actionForRetryNotification, timeoutInMilliseconds )


        #region CopyFileToDirectory( sourceFilePath, destinationDirectoryPath )
        /// <summary>
        /// Copy the source file to the destination directory, the destinatiion-filename being unchanged.
        /// This is a simple Copy - with no retrying.
        /// </summary>
        /// <param name="sourceFilePath">the pathname of the source-file to copy from</param>
        /// <param name="destinationDirectoryPath">the directory to copy the file to - this does not include the name of the new file</param>
        /// <returns>true if no errors are indicated</returns>
        /// <exception cref="ArgumentNullException">the value provided for either path must not be null</exception>
        public static bool CopyFileToDirectory( string sourceFilePath,
                                                string destinationDirectoryPath )
        {
            string filename = FileStringLib.GetFileNameFromFilePath( sourceFilePath );
            string destinationFilePath = Path.Combine( destinationDirectoryPath, filename );

            return CopyFile( sourceFilePath, destinationFilePath, false, null, 0 );
        }
        #endregion

        #region CopyFileToDirectory( sourceFilePath, destinationDirectoryPath, isToRolloverExistingDestinFile )
        /// <summary>
        /// Copy the source file to the destination directory, the destination-filename being unchanged.
        /// This is a simple Copy - with no retrying.
        /// </summary>
        /// <param name="sourceFilePath">the pathname of the source-file to copy from</param>
        /// <param name="destinationDirectoryPath">the directory to copy the file to - this does not include the name of the new file</param>
        /// <param name="isToRolloverExistingDestinFile">if true - then rollover the destination-filename if it already exists, so that it does not get overwritten</param>
        /// <returns>true if no errors are indicated</returns>
        /// <exception cref="ArgumentNullException">the value provided for either path must not be null</exception>
        /// <remarks>
        /// Rollover:
        /// If isToRolloverExistingDestinFile is set to true, and there already exists a file of the same name at the destination,
        /// and that file does not have the same content - then a backup copy of it is made by calling FilesystemLib.Rollover.
        /// </remarks>
        public static bool CopyFileToDirectory( string sourceFilePath,
                                                string destinationDirectoryPath,
                                                bool isToRolloverExistingDestinFile )
        {
            string filename = FileStringLib.GetFileNameFromFilePath( sourceFilePath );
            string destinationFilePath = Path.Combine( destinationDirectoryPath, filename );

            return CopyFile( sourceFilePath, destinationFilePath, isToRolloverExistingDestinFile, null, 0 );
        }
        #endregion

        #region CopyFileToDirectory( sourceFilePath, destinationDirectoryPath, isToRolloverExistingDestinFile, retryNotifier, timeoutInMilliseconds )
        /// <summary>
        /// Copy the source file to the destination directory, the destination-filename being unchanged.
        /// This is a simple Copy - with no retrying.
        /// </summary>
        /// <param name="sourceFilePath">the pathname of the source-file to copy from</param>
        /// <param name="destinationDirectoryPath">the directory to copy the file to - this does not include the name of the new file</param>
        /// <param name="isToRolloverExistingDestinFile">if true - then rollover the destination-filename if it already exists, so that it does not get overwritten</param>
        /// <param name="retryNotifier">an delegate to call when a retry occurs (may be null)</param>
        /// <param name="timeoutInMilliseconds">a limit on how long to keep trying before giving up, in milliseconds</param>
        /// <returns>true if no errors are indicated</returns>
        /// <exception cref="ArgumentNullException">the value provided for either path must not be null</exception>
        public static bool CopyFileToDirectory( string sourceFilePath,
                                                string destinationDirectoryPath,
                                                bool isToRolloverExistingDestinFile,
                                                FileProgressNotifier retryNotifier,
                                                int timeoutInMilliseconds )
        {
            string filename = FileStringLib.GetFileNameFromFilePath( sourceFilePath );
            string destinationFilePath = Path.Combine( destinationDirectoryPath, filename );

            return CopyFile( sourceFilePath, destinationFilePath, isToRolloverExistingDestinFile, retryNotifier, timeoutInMilliseconds );
        }
        #endregion
    }
}
