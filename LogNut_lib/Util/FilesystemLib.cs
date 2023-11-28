#if PRE_4
#define PRE_5
#endif
#if (NETFW_462_OR_ABOVE)
#define NO_LONGPATH
#endif
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading;
#if !PRE_4
using System.Threading.Tasks;
#endif
#if NETFX_CORE
using Windows.ApplicationModel;
using Windows.Storage;
#else
using Hurst.LogNut.Util.Native;
using System.Management;
using System.Security.AccessControl;
#endif
using Microsoft.Win32.SafeHandles;
//using SharpCompress.Archives;
//using SharpCompress.Archives.Zip;
//using SharpCompress.Common;
//using SharpCompress.Writers;
using FileAccess = System.IO.FileAccess;
using FileAttributes = System.IO.FileAttributes;


// define PRE_4 if this is to be compiled for versions of .NET earlier than 4.0
// Compile with the pragma NO_LONGPATH if you want to use only the normal .NET filesystem facilities.

// Note: .NET Framework at 4.62 and above, no longer has the path-length limitation.
// Define the compiler-pragma NETFW_462_OR_ABOVE when targetting this version of .NET.


// Todo:
//   Copy folder-tree
//     copy only those needed in order to sync-up
//     do a verify after copy
//   Delete contents of a directory, but not the dir itself
//   
//   Verify a directory against a data-structure that denotes
//   the exact tree-structure and a hash-value of each file.
// I need to make specific provisions for handling the asynchronous nature of flash drives.
// see  https://stackoverflow.com/questions/10834466/force-flush-file-cash-for-a-usb-device-c-sharp


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// class FilesystemLib exists to contain some application-neutral filesystem-related methods.
    /// </summary>
    public static partial class FilesystemLib
    {
        /// <summary>
        /// This represents the maximum valid length for a single folder-name ( 255 ).
        /// </summary>
        public const int MAX_PATH = 255;  //CBL But Win32 has this same const defined as 260 !

        /// <summary>
        /// When unspecified by an explicit argument, this is the default amount of time allowed for retrying a file operation,
        /// in milliseconds.  This is currently set to 5000 ms.
        /// </summary>
        public const int DefaultRetryTimeLimit = 5000;

        /// <summary>
        /// This is just some random text for inserting into text-files upon creation, for testing. It is "Bitch better have my money." .
        /// </summary>
        public const string SampleFileContent = "Bitch better have my money.";

        public static string GetTimeStamp(DateTime forWhen)
        {
            //CBL  This seems unfinished, and also unused. ?   2022/5/3

            // Previous version of this method had: DateTime, bool, bool, bool
            // which was when, showDate, showFractionsOfASecond, isFixedWidth

            // I am putting this in ISO 8601 format, which is the nearest thing we have to an international standard.
            // See http://www.iso.org/iso/date_and_time_format

            bool isToShowFractionsOfASecond = true;
            var sb = new StringBuilder();
            sb.AppendFormat("{0:yyyy-MM-dd HH:mm:ss}", forWhen);

            if (isToShowFractionsOfASecond)
            {
                if (forWhen.Millisecond != 0)
                {
                    sb.Append(".").AppendFormat("{0:D3}", forWhen.Millisecond);
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// This delegate represents a method that, e.g. a copy operation can call to advise the caller of it's progress.
        /// </summary>
        /// <param name="isRetry">true to indicate that this is a notification of an error or of having to retry an operation</param>
        /// <param name="isDirectory">Set true to indicate this concerns a filesystem directory, false when this concerns a file</param>
        /// <param name="message">This conveys the main message of this notification, such as the pathname of the file involved</param>
        public delegate void FileProgressNotifier( bool isRetry, bool isDirectory, string message );

        /// <summary>
        /// These 9 numbers denote the amount of time (in milliseconds) to wait between retries for those operations that involve retrying an operation.
        /// </summary>
        /// <remarks>
        /// Example of a retry-loop this is intended for:
        /// <example>
        /// // If this is allowing for retries (timeout > 0) then check to see whether the source-file is locked.
        /// if (timeoutInMilliseconds > 0)
        /// {
        ///     int i = 0;
        ///     bool isTimedOut = false;
        ///     var stopwatch = Stopwatch.StartNew();
        ///     while (IsFileLocked(sourceFilePath ))
        ///     {
        ///         Thread.Sleep(RetryIntervals[i] );
        ///         if (stopwatch.ElapsedMilliseconds > timeoutInMilliseconds)
        ///         {
        ///             isTimedOut = true;
        ///             break;
        ///         }
        ///         if (i &lt; RetryIntervals.Length - 1)
        ///         {
        ///             i++;
        ///         }
        ///     }
        ///     stopwatch.Stop();
        /// 
        ///     if (isTimedOut)
        ///     {
        ///         throw new TimeoutException( message: "CopyFile timed-out waiting for " + sourceFilePath + " to become unlocked." );
        ///     }
        /// }
        /// </example>
        /// </remarks>
        public static int[] RetryIntervals = { 50, 100, 200, 300, 400, 500, 600, 800, 1000  };


        //CBL This next 2 methods work fine, but I am not presently using them.

        //public static void CompressDirectory( string directoryToCompress, string pathnameOfCompressedFile )
        //{
        //    using (var archive = ZipArchive.Create())
        //    {
        //        archive.AddAllFromDirectory( directoryToCompress );
        //        var w = new WriterOptions(CompressionType.Deflate);
        //        archive.SaveTo( filePath: pathnameOfCompressedFile, options: w );
        //    }
        //}

        //public static void Uncompress( string pathnameOfCompressedFile, string directoryPathToPlaceResult )
        //{
        //    if (Directory.Exists( directoryPathToPlaceResult ))
        //    {
        //        FilesystemLib.DeleteDirectoryContent( directoryPathToPlaceResult );
        //    }
        //    else
        //    {
        //        Directory.CreateDirectory( directoryPathToPlaceResult );
        //    }

        //    var archive = ArchiveFactory.Open(pathnameOfCompressedFile);
        //    foreach (var entry in archive.Entries)
        //    {
        //        if (!entry.IsDirectory)
        //        {
        //            Console.WriteLine( entry.Key );
        //            entry.WriteToDirectory( directoryPathToPlaceResult, new ExtractionOptions() { ExtractFullPath = true, Overwrite = true } );
        //        }
        //    }
        //}

        //public static void CompressDirectory_DotNetZip( string directoryToCompress, string pathnameOfCompressedFile )
        //{
        //    using (ZipFile zip = new ZipFile())
        //    {
        //        zip.AddDirectory( directoryToCompress );
        //        zip.CompressionLevel = CompressionLevel.BestCompression;
        //        zip.CompressionMethod = (Ionic.Zip.CompressionMethod)CompressionMethod.Deflate;
        //        zip.Comment = "This file archive was created at " + System.DateTime.Now.ToString( "G" );
        //        zip.Save( pathnameOfCompressedFile );
        //    }
        //}

        //public static void Uncompress_DotNetZip( string pathnameOfCompressedFile, string directoryPathToPlaceResult )
        //{
        //    // See  https://archive.codeplex.com/?p=dotnetzip
        //    using (var zip1 = ZipFile.Read( pathnameOfCompressedFile ))
        //    {
        //        foreach (var e in zip1)
        //        {
        //            e.Extract( directoryPathToPlaceResult, ExtractExistingFileAction.OverwriteSilently );
        //        }
        //    }
        //}

        #region CreateDirectory
        /// <summary>
        /// Create a new folder on the filesystem.
        /// </summary>
        /// <param name="directoryPath">the filesystem-path denoting the folder to create</param>
        /// <exception cref="ArgumentNullException">The directoryPath must not be null</exception>
        /// <exception cref="ArgumentException">the value provided for directoryPath must be a syntactically-valid path</exception>
        /// <exception cref="PathTooLongException">directoryPath must not have folder or file names that are too long</exception>
        public static void CreateDirectory( string directoryPath )
        {
            //CBL I must create a retrying version of this!
            // Check directoryPath..
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }

            try
            {
                if (!DirectoryExists( directoryPath ))
                {
                    // Create each folder down the tree, where necessary..
                    string basePart;
                    string[] childParts = FileStringLib.SplitFolderPath( directoryPath, out basePart );

                    // If basePart is of the form "C:" then change it to "C:\"..
                    if (basePart.Length == 2 && basePart[1] == ':')
                    {
                        basePart = basePart + @"\";
                    }

                    var path = basePart;

                    foreach (var childPart in childParts)
                    {
                        path = Path.Combine( path, childPart );

                        if (!DirectoryExists( path ))
                        {
                            //CBL Is this all needed?
                            doCreateDirectory( path );
                            Thread.Sleep( 50 );
                        }
                    }

                    //CBL
                    // That doesn't work for, eg, \\SERVERNAME\Folder1
                    //
                }
            }
            catch (Exception x)
            {
                const string Key1 = "directoryPath:";
                if (!x.Data.Contains( Key1 ))
                {
                    x.Data.Add( key: Key1, value: directoryPath );
                }
                throw;
            }
        }

        #region doCreateDirectory
        private static void doCreateDirectory( string directoryPath )
        {
            //int timeoutInMilliseconds = 10000;
            int lastWin32Error = 0;
            int nAttempt = 1;
            bool wasSuccessful = false;
            while (!wasSuccessful && nAttempt < 7)
            {
                wasSuccessful = Native.Win32.CreateDirectory( directoryPath, IntPtr.Zero );
                if (wasSuccessful)
                {
                    return;
                }
                else // not successful
                {
                    // If the error was 5, meaning "Access Denied", try it a few more times - backing off the wait-time
                    // each time, to see if that helps.
                    lastWin32Error = Marshal.GetLastWin32Error();
                    if (lastWin32Error == 5)
                    {
                        switch (nAttempt)
                        {
                            case 1:
                                Thread.Sleep( 50 );
                                break;
                            case 2:
                                Thread.Sleep( 100 );
                                break;
                            case 3:
                                Thread.Sleep( 200 );
                                break;
                            case 4:
                                Thread.Sleep( 400 );
                                break;
                            case 5:
                                Thread.Sleep( 800 );
                                break;
                            case 6:
                                Thread.Sleep( 1600 );
                                break;
                            case 7:
                                Thread.Sleep( 3200 );
                                break;
                        }
                        nAttempt++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            // Retrying the operation did not help. So, just whine about it.
            throw new Win32Exception( lastWin32Error,
                                      "in doCreateDirectory: Error " + lastWin32Error + " creating directory '" + directoryPath + "': " + FileStringLib.CheckAddDotEnd( new Win32Exception( lastWin32Error ).Message ) + ", nAttempt = " + nAttempt );
        }
        #endregion



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
        #endregion CreateDirectory

        #region CreateTextFile
        /// <summary>
        /// Create the given file with the given text-content.
        /// </summary>
        /// <param name="fileInfo">a FileInfo denoting the pathname of the file to create</param>
        /// <param name="content">what to write into the file; this may be null</param>
        public static void CreateTextFile( this FileInfo fileInfo, string content )
        {
            string pathname = fileInfo.FullName;
#if NETFX_CORE
            File.WriteAllText( path: pathname, contents: content );
#else
            using (System.IO.StreamWriter streamWriter = new System.IO.StreamWriter( pathname, true ))
            {
                if (!String.IsNullOrEmpty( content ))
                {
                    streamWriter.WriteLine( content );
                }
            }
#endif
        }

        /// <summary>
        /// Create the given file with the given text-content.
        /// </summary>
        /// <param name="pathname">the full pathname of the file to create</param>
        /// <param name="content">what to write into the file; this may be null</param>
        public static void CreateTextFile( string pathname, string content )
        {
#if NETFX_CORE
            File.WriteAllText( path: pathname, contents: content );
#else
            using (System.IO.StreamWriter streamWriter = new System.IO.StreamWriter( pathname, true ))
            {
                if (!String.IsNullOrEmpty( content ))
                {
                    streamWriter.WriteLine( content );
                }
            }
#endif
        }
        #endregion

        #region DirectoryExists
        /// <summary>
        /// Check whether the given filesystem-folder actually exists on the filesystem,
        /// and return true if it does.
        /// </summary>
        /// <param name="directoryPath">the filesystem-path denoting the folder to check</param>
        /// <returns>true if directoryPath exists, false otherwsie</returns>
        /// <exception cref="ArgumentNullException">The directory-path must not be null</exception>
        public static bool DirectoryExists( string directoryPath )
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }
#if (NETFX_CORE || NO_LONGPATH)
            return Directory.Exists( directoryPath );
#else
            if (directoryPath.Length < MAX_PATH)
            {
                return Directory.Exists( directoryPath );
            }
            else
            {
                string correctedDirectoryPath = FileStringLib.CheckAddLongPathPrefix( directoryPath ).TrimEnd( '\\' );


                //CBL  This is what I had been using. However, I ran into a case wherein it returns true
                //     for a non-existent directory.
                var win32FileAttributeData = default( Native.Win32.WIN32_FILE_ATTRIBUTE_DATA );

                var b = Native.Win32.GetFileAttributesEx( correctedDirectoryPath, 0, ref win32FileAttributeData );
                return b &&
                       win32FileAttributeData.dwFileAttributes != -1 &&
                       (win32FileAttributeData.dwFileAttributes & 16) != 0;
            }

            //CBL ???
            // --

            //var a = PInvokeHelper.GetFileAttributes(directoryPath);
            //if ((a & PInvokeHelper.INVALID_FILE_ATTRIBUTES) == PInvokeHelper.INVALID_FILE_ATTRIBUTES)
            //{
            //    return false;
            //}
            //else
            //{
            //    return (a & PInvokeHelper.FILE_ATTRIBUTE_DIRECTORY) == PInvokeHelper.FILE_ATTRIBUTE_DIRECTORY;
            //}

            // --

            //            Win32.WIN32_FIND_DATA findData;
            //            var result = Win32.FindFirstFile( correctedDirectoryPath, out findData );

            //if (findData == )


            //PInvokeHelper.WIN32_FIND_DATA fd;
            //var result = PInvokeHelper.FindFirstFile( directoryPath.TrimEnd( '\\' ) /*+ @"\*"*/, out fd );

            //if (result.ToInt32() == PInvokeHelper.ERROR_FILE_NOT_FOUND || result == PInvokeHelper.INVALID_HANDLE_VALUE)
            //{
            //    return false;
            //}
            //else
            //{
            //    return ((int)fd.dwFileAttributes & PInvokeHelper.FILE_ATTRIBUTE_DIRECTORY) != 0;
            //}

            //CBL  Added this just to get it to complete.
            //return true;
#endif
        }
        #endregion

        #region DriveExists
#if !NETFX_CORE
        /// <summary>
        /// Return true if the given drive-letter is reported to be a valid accessible drive.
        /// </summary>
        /// <param name="driveLetter">a character denoting the disk-drive letter to check for</param>
        /// <returns>true if <c>DriveInfo</c> reports this drive as being ready</returns>
        public static bool DriveExists( char driveLetter )
        {
            DriveInfo driveInfo = new DriveInfo( driveLetter.ToString() );
            return driveInfo.IsReady;
        }

        /// <summary>
        /// Return true if the given disk-drive is reported to be a valid accessible drive.
        /// The disk-drive is of the form "C:" or "D", and it may even include a directory (which need not exist).
        /// </summary>
        /// <param name="drive">a string denoting the disk-drive letter to check for</param>
        /// <returns>true if <c>DriveInfo</c> reports this drive as being ready</returns>
        /// <exception cref="ArgumentNullException">The argument provided for <paramref name="drive"/> must not be null.</exception>
        public static bool DriveExists( string drive )
        {
            if (drive == null)
            {
                throw new ArgumentNullException( "drive" );
            }
            DriveInfo driveInfo = new DriveInfo( drive );
            return driveInfo.IsReady;
        }
#endif
        #endregion

        #region ExistsOnPath
        /// <summary>
        /// Return true if the given filename is either a path that exists,
        /// or can be found in one of the folders specified within the PATH environment-variable.
        /// </summary>
        /// <param name="fileName">a filename or pathname to check for</param>
        /// <returns>true if found, false otherwise</returns>
        public static bool ExistsOnPath( string fileName )
        {
            if (GetFullPath( fileName ) != null)
                return true;
            return false;
        }
        #endregion

        #region FileExists
        /// <summary>
        /// Get whether an filesystem-file exists at the given path.
        /// </summary>
        /// <param name="filePath">the pathname to check</param>
        /// <returns>true if a file actually exists at the given path</returns>
        /// <exception cref="ArgumentNullException">The filePath must not be null</exception>
        public static bool FileExists( string filePath )
        {

            if (filePath == null)
            {
                throw new ArgumentNullException( "filePath" );
            }
#if (NO_LONGPATH || NETFX_CORE)
            return File.Exists( filePath );
#else
            //CBL  This is from ZLongPathLib

            string correctedFilePath = FileStringLib.CheckAddLongPathPrefix( filePath );

            var wIn32FileAttributeData = default( Native.Win32.WIN32_FILE_ATTRIBUTE_DATA );

            var b = Native.Win32.GetFileAttributesEx( correctedFilePath, 0, ref wIn32FileAttributeData );
            return b &&
                   wIn32FileAttributeData.dwFileAttributes != -1 &&
                   (wIn32FileAttributeData.dwFileAttributes & 16) == 0;

            // --

            //var a = PInvokeHelper.GetFileAttributes(correctedFilePath);
            //if ((a & PInvokeHelper.INVALID_FILE_ATTRIBUTES) == PInvokeHelper.INVALID_FILE_ATTRIBUTES)
            //{
            //    return false;
            //}
            //else
            //{
            //    return (a & PInvokeHelper.FILE_ATTRIBUTE_DIRECTORY) == 0;
            //}

            // --

            //filePath = CheckAddLongPathPrefix(filePath);

            //PInvokeHelper.WIN32_FIND_DATA fd;
            //var result = PInvokeHelper.FindFirstFile(filePath.TrimEnd('\\'), out fd);

            //if (result.ToInt32() == PInvokeHelper.ERROR_FILE_NOT_FOUND || result == PInvokeHelper.INVALID_HANDLE_VALUE)
            //{
            //    return false;
            //}
            //else
            //{
            //    return ((int)fd.dwFileAttributes & PInvokeHelper.FILE_ATTRIBUTE_DIRECTORY) == 0;
            //}
#endif
        }
        #endregion

        #region FilesHaveSameContent
        /// <summary>
        /// Return true if the two given files have the same content.
        /// </summary>
        /// <param name="pathnameFile1">one file to compare against the other</param>
        /// <param name="pathnameFile2">the file to compare file1 against</param>
        /// <returns>true if the two files have equal bytes</returns>
        public static bool FilesHaveSameContent( string pathnameFile1, string pathnameFile2 )
        {
            // See also:  https://stackoverflow.com/questions/1358510/how-to-compare-2-files-fast-using-net
            if (pathnameFile1 == null)
            {
                throw new ArgumentNullException( "pathnameFile1" );
            }
            if (pathnameFile2 == null)
            {
                throw new ArgumentNullException( "pathnameFile2" );
            }
            // Determine if the same file was referenced two times.
            if (pathnameFile1 == pathnameFile2)
            {
                // Return true to indicate that the files are the same.
                return true;
            }
            int file1byte;
            int file2byte;
            FileStream fs1 = null;
            FileStream fs2 = null;

            try
            {
                // Open the two files.
                fs1 = new FileStream( pathnameFile1, FileMode.Open );
                fs2 = new FileStream( pathnameFile2, FileMode.Open );

                // Check the file sizes. If they are not the same, the files are not the same.
                if (fs1.Length != fs2.Length)
                {
                    // Return false to indicate files are different
                    return false;
                }

                // Read and compare a byte from each file until either a non-matching set of bytes is found
                // or until the end of file1 is reached.
                do
                {
                    // Read one byte from each file.
                    file1byte = fs1.ReadByte();
                    file2byte = fs2.ReadByte();
                } while ((file1byte == file2byte) && (file1byte != -1));
            }
            catch (Exception x)
            {
                x.Data.Add( "pathnameFile1", pathnameFile1 );
                x.Data.Add( "pathnameFile2", pathnameFile2 );
                throw x;
            }
            finally
            {
                // Close the files.
                if (fs1 != null)
                {
                    fs1.Close();
                }
                if (fs2 != null)
                {
                    fs2.Close();
                }
            }

            // Return the success of the comparison. "file1byte" is equal to "file2byte" at this point only if the files are the same.
            return ((file1byte - file2byte) == 0);
        }
        #endregion FilesHaveSameContent

        #region FilesExistThatMatchPattern
        /// <summary>
        /// Return true if the given directoryPath contains at least one file that matches the given pattern.
        /// </summary>
        /// <param name="directoryPath">the filesystem-folder to look within</param>
        /// <param name="pattern">the filespec-pattern that determines what files to look for</param>
        /// <returns>true if a match is found, false otherwise</returns>
        public static bool FilesExistThatMatchPattern( string directoryPath, string pattern )
        {
            // This is copied and modified slight from the GetFiles method of this same class.
            directoryPath = FileStringLib.CheckAddLongPathPrefix( directoryPath );

            Native.Win32.WIN32_FIND_DATA findData;
            var findHandle = Native.Win32.FindFirstFile( directoryPath.TrimEnd( '\\' ) + "\\" + pattern, out findData );

            if (findHandle != Native.Win32.INVALID_HANDLE_VALUE)
            {
                try
                {
                    bool found;
                    do
                    {
                        var currentFileName = findData.cFileName;

                        // if this is a file, find its contents
                        if (((int)findData.dwFileAttributes & Native.Win32.FILE_ATTRIBUTE_DIRECTORY) == 0)
                        {
                            return true;
                        }

                        // find next
                        found = Native.Win32.FindNextFile( findHandle, out findData );
                    }
                    while (found);
                }
                finally
                {
                    // close the find handle
                    Native.Win32.FindClose( findHandle );
                }
            }
            return false;
        }
        #endregion

        #region FindProgram
#if !NETFX_CORE
        /// <summary>
        /// Try to locate the given program and return it's full pathname, if possible.
        /// </summary>
        /// <param name="programName">The program to look for, which may be a generic name, a file, or path</param>
        /// <returns>The full pathname if found, null otherwise</returns>
        public static string FindProgram( string programName )
        {
            string sPath = "";
            // For the Windows root path: Environment.GetEnvironmentVariable("SystemRoot"), or "WinDir"
            // We'll later want to search this too.
            //string s3 = Environment.GetFolderPath(Environment.SpecialFolder.Programs);
            string sFilename = programName.ToUpper();

            // Internet Explorer
            //if (sFilename == "IE" || sFilename == "IEXPLORE" || sFilename == "IEXPLORE.EXE" || sFilename == "INTERNET EXPLORER")
            //{
            //    sFilename = "iexplore.exe";
            //    string sPathTail = Path.Combine( @"Internet Explorer", sFilename );
            //    if (!ExistsOnProgramFilesDir( sPathTail, out sPath ))
            //    {
            //        sPath = "";
            //    }
            //    return sPath;
            //}

            // Windows Explorer
            // Target: %SystemRoot%\explorer.exe
            string sSystemRoot = Environment.GetFolderPath( Environment.SpecialFolder.System );
            string sWindowsDir = Environment.GetEnvironmentVariable( "WinDir" );
            if (sFilename == "EXPLORER.EXE" || sFilename == "WINDOWS EXPLORER")
            {
                sFilename = "explorer.exe";
                // This isn't working!!
                sPath = Path.Combine( sSystemRoot, sFilename );
                if (!File.Exists( sPath ))
                {
                    sPath = Path.Combine( sWindowsDir, sFilename );
                    if (!File.Exists( sPath ))
                    {
                        sPath = "";
                    }
                }
                return sPath;
            }

            // Notepad
            // Target: %SystemRoot%\system32\notepad.exe
            if (sFilename == "NOTEPAD" || sFilename == "NOTEPAD.EXE")
            {
                sPath = Path.Combine( sSystemRoot, "notepad.exe" );
                if (!File.Exists( sPath ))
                {
                    sPath = "";
                }
                return sPath;
            }

            // Try a generic look-see..

            return sPath;
        }
#endif
        #endregion

        #region GetDirectories
        /// <summary>
        /// Get the directories found within the given filesystem-path, as an array of ZDirectoryInfo objects.
        /// </summary>
        /// <param name="directoryPath">the filesystem-path to get them from</param>
        /// <returns>an array of the ZDirectoryInfo objects found there</returns>
        public static ZDirectoryInfo[] GetDirectories( string directoryPath )
        {
#if NETFX_CORE
            return (from eachPathname in Directory.GetDirectories( path: directoryPath ) select new ZDirectoryInfo( eachPathname )).ToArray();
#else
            return GetDirectories( directoryPath, @"*" );
#endif
        }

        /// <summary>
        /// Get the directories found within the given filesystem-path that match the given pattern,
        /// as an array of ZDirectoryInfo objects.
        /// </summary>
        /// <param name="directoryPath">the filesystem-path to get them from</param>
        /// <param name="folderSpec">the filespec to filter the returned results by. For example: *.txt (must not be null)</param>
        /// <returns>an array of the matching ZDirectoryInfo objects found there</returns>
        public static ZDirectoryInfo[] GetDirectories( string directoryPath, string folderSpec )
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }
            if (folderSpec == null)
            {
                throw new ArgumentNullException( "folderSpec" );
            }
#if NETFX_CORE
//CBL Hey, what about the validation on the .NET side?
            return (from eachPathname in Directory.GetDirectories( path: directoryPath, searchPattern: folderSpec ) select new ZDirectoryInfo( eachPathname )).ToArray();
#else
            directoryPath = FileStringLib.CheckAddLongPathPrefix( directoryPath );

            if (!DirectoryExists( directoryPath ))
            {
                throw new DirectoryNotFoundException( "Argument " + directoryPath + " to GetDirectories is not found." );
            }

            var results = new List<ZDirectoryInfo>();
            Native.Win32.WIN32_FIND_DATA findData;
            var findHandle = Native.Win32.FindFirstFile( directoryPath.TrimEnd( '\\' ) + @"\" + folderSpec, out findData );

            if (findHandle != Native.Win32.INVALID_HANDLE_VALUE)
            {
                try
                {
                    bool found;
                    do
                    {
                        var currentFileName = findData.cFileName;

                        // if this is a directory, find its contents
                        if (((int)findData.dwFileAttributes & Native.Win32.FILE_ATTRIBUTE_DIRECTORY) != 0)
                        {
                            if (currentFileName != @"." && currentFileName != @"..")
                            {
                                results.Add( new ZDirectoryInfo( Path.Combine( directoryPath, currentFileName ) ) );
                            }
                        }

                        // find next
                        found = Native.Win32.FindNextFile( findHandle, out findData );
                    } while (found);
                }
                finally
                {
                    // close the find handle
                    Native.Win32.FindClose( findHandle );
                }
            }

            return results.ToArray();
#endif
        }
        #endregion GetDirectories

        #region GetDirectoryAttributes
        /// <summary>
        /// Return the file-attributes of the filesystem-directory denoted by the given path.
        /// </summary>
        /// <param name="directoryPath">the directory to get the attributes of</param>
        /// <returns>the FileAttributes of the given directory</returns>
        public static FileAttributes GetDirectoryAttributes( string directoryPath )
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }
#if NETFX_CORE
            return Directory.GetAttributes( directoryPath );
#else
            if (!DirectoryExists( directoryPath ))
            {
                throw new DirectoryNotFoundException( "directoryPath " + directoryPath + " not found." );
            }

            return (FileAttributes)Native.Win32.GetFileAttributes( directoryPath );
#endif
        }
        #endregion

        #region GetDirectoryCreationTime
        /// <summary>
        /// Given a directory path, return the DateTime representing the creation-time of that directory.
        /// </summary>
        /// <param name="directoryPath">the filesystem-path of the directory to get the creation-time of</param>
        /// <returns>a DateTime denoting the creation-time</returns>
        /// <exception cref="DirectoryNotFoundException">The given directory must exist</exception>
        public static DateTime GetDirectoryCreationTime(string directoryPath)
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException("directoryPath");
            }
            // Throw an exception if the folder is not there.
            if (!DirectoryExists(directoryPath))
            {
                throw new DirectoryNotFoundException("directoryPath '" + directoryPath + "' not found.");
            }
#if NETFX_CORE
            return Directory.GetCreationTime( directoryPath );
#else
#if (NO_LONGPATH)
            return Directory.GetCreationTime( directoryPath );
#else
            if (directoryPath.Length < MAX_PATH)
            {
                return Directory.GetCreationTime(directoryPath);
            }
            string correctedFolderPath = FileStringLib.CheckAddLongPathPrefix(directoryPath);

            // CBL Test this for with and without the end slash.
            Native.Win32.WIN32_FIND_DATA fd;
            var result = Native.Win32.FindFirstFile(correctedFolderPath.TrimEnd('\\'), out fd);

            //CBL  If the file or folder is not there, then should we not want to throw an exception?
            // and let's test it for access permission situations too.

            if (result == Native.Win32.INVALID_HANDLE_VALUE)
            {
                return DateTime.MinValue;
            }
            else
            {
                //bool notFound = false;
                //Int64 r = result.ToInt64();
                //if (r == Native.Win32.ERROR_FILE_NOT_FOUND)
                //{
                //    notFound = true;
                //}
                //else if (r == Native.Win32.ERROR_PATH_NOT_FOUND)
                //{
                //    notFound = true;
                //}

                //if (notFound)
                //{
                //    Native.Win32.FindClose(result);
                //    return DateTime.MinValue;
                //}

                //int r2 = result.ToInt32();

                try
                {
                    Int64 r = result.ToInt64();
                    if (r > Int32.MinValue && r < Int32.MaxValue && (int)r == Native.Win32.ERROR_FILE_NOT_FOUND)
                    //CBL  This next line was producing an arithmetic overflow.
                    //if (result.ToInt32() == Native.Win32.ERROR_FILE_NOT_FOUND)
                    {
                        return default(DateTime);
                    }
                    else
                    {
                        var ft = fd.ftCreationTime;

                        var hft2 = (((long)ft.dwHighDateTime) << 32) + ft.dwLowDateTime;
                        return DateTime.FromFileTimeUtc(hft2);
                    }
                }
                finally
                {
                    Native.Win32.FindClose(result);
                }
            }
#endif
#endif
        }
        #endregion

        #region GetDirectoryLastWriteTime
        /// <summary>
        /// Given a directory path, return the DateTime representing when it was last written to.
        /// </summary>
        /// <param name="directoryPath">the filesystem-path of the directory to get the last-written-time of</param>
        /// <returns>a DateTime denoting the last-write-time</returns>
        /// <exception cref="DirectoryNotFoundException">The given directory must exist</exception>
        public static DateTime GetDirectoryLastWriteTime(string directoryPath)
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException("directoryPath");
            }
            // Throw an exception if the folder is not there.
            if (!DirectoryExists(directoryPath))
            {
                throw new DirectoryNotFoundException("directoryPath '" + directoryPath + "' not found.");
            }
            // Note: I didn't include most of the error-handling code from GetDirectoryCreationTime.
            //       Do we need that?  CBL
            return Directory.GetCreationTime( directoryPath );
        }
        #endregion

        #region GetDirectorySize
        /// <summary>
        /// Return the size, in bytes, that the given folder consumes on the disk-drive.
        /// </summary>
        /// <param name="directoryPath">folder for which the size is requested</param>
        /// <returns>size of the given folder, as an unsigned-long</returns>
        /// <exception cref="ArgumentNullException">The argument supplied for <paramref name="directoryPath"/> must not be null.</exception>
        public static ulong GetDirectorySize( string directoryPath )
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }
            // Throw an exception if the folder is not there.
            if (!DirectoryExists( directoryPath ))
            {
                throw new DirectoryNotFoundException( "directoryPath '" + directoryPath + "' not found." );
            }
#if NETFX_CORE
            ulong totalSize = 0;
            StorageFolder folder = KnownFolders.DocumentsLibrary;
            IReadOnlyList<IStorageItem> items = await folder.GetItemsAsync();
            foreach (var item in items)
            {
                if (item is StorageFile)
                {
                    var properties = await item.GetBasicPropertiesAsync();
                    //totalSize += item.
                }
            }
            return totalSize;
#else
#if PRE_5
            int numberOfFiles, numberOfSubfolders;
            long size = RecurseDirectoryCounting( directoryPath, -1, out numberOfFiles, out numberOfSubfolders );
            return (ulong)size;
#else
            // Note: This is far simpler, and is vastly faster than a method that uses fully managed code --
            //       but it is faster yet to use the Win32 method.
            Scripting.FileSystemObject fso = new Scripting.FileSystemObject();
            Scripting.Folder folder = fso.GetFolder( directoryPath );
            return (ulong)folder.Size;
#endif
#endif
        }
        #endregion GetDirectorySize

        #region GetFileAttributes
        /// <summary>
        /// Return the file-attributes of the filesystem-file denoted by the given path.
        /// </summary>
        /// <param name="filePathname">the file to get the attributes of</param>
        /// <returns>the FileAttributes of the given file</returns>
        public static FileAttributes GetFileAttributes( string filePathname )
        {
            if (filePathname == null)
            {
                throw new ArgumentNullException( "filePathname" );
            }
#if NETFX_CORE
            return File.GetAttributes( filePathname );
#else
            if (!FileExists( filePathname ))
            {
                throw new FileNotFoundException( "filePathname " + filePathname + " not found." );
            }

            return (FileAttributes)Native.Win32.GetFileAttributes( filePathname );
#endif
        }
        #endregion

        #region GetFileCreationTime
        /// <summary>
        /// Given a pathname, return the DateTime representing the creation-time of that file or folder.
        /// </summary>
        /// <param name="filePathname">the filesystem-path of the file or folder</param>
        /// <returns>a DateTime dennoting the creation-time</returns>
        public static DateTime GetFileCreationTime( string filePathname )
        {
            if (filePathname == null)
            {
                throw new ArgumentNullException( "filePathname" );
            }
            //CBL I think this ought to throw an exception if the file is not there.
#if NETFX_CORE
            return File.GetCreationTime( filePathname );
#else
#if (NO_LONGPATH)
            FileInfo fileInfo = new FileInfo(filePathname);
            return fileInfo.CreationTime;
#else
            string filePath = FileStringLib.CheckAddLongPathPrefix( filePathname );

            Native.Win32.WIN32_FIND_DATA fd;
            var result = Native.Win32.FindFirstFile( filePath.TrimEnd( '\\' ), out fd );

            //CBL  If the file or folder is not there, then should we not want to throw an exception?
            // and let's test it for access permission situations too.

            if (result == Native.Win32.INVALID_HANDLE_VALUE)
            {
                return DateTime.MinValue;
            }
            else
            {
                //bool notFound = false;
                //Int64 r = result.ToInt64();
                //if (r == Native.Win32.ERROR_FILE_NOT_FOUND)
                //{
                //    notFound = true;
                //}
                //else if (r == Native.Win32.ERROR_PATH_NOT_FOUND)
                //{
                //    notFound = true;
                //}

                //if (notFound)
                //{
                //    Native.Win32.FindClose(result);
                //    return DateTime.MinValue;
                //}

                //int r2 = result.ToInt32();

                try
                {
                    //CBL Re-think this..
                    Int64 r = result.ToInt64();
                    if (r > Int32.MinValue && r < Int32.MaxValue && (int)r == Native.Win32.ERROR_FILE_NOT_FOUND)
                    //CBL  This next line was producing an arithmetic overflow.
                    //if (result.ToInt32() == Native.Win32.ERROR_FILE_NOT_FOUND)
                    {
                        return DateTime.MinValue;
                    }
                    else
                    {
                        var ft = fd.ftCreationTime;

                        var hft2 = (((long)ft.dwHighDateTime) << 32) + ft.dwLowDateTime;
                        return DateTime.FromFileTimeUtc( hft2 );
                    }
                }
                finally
                {
                    Native.Win32.FindClose( result );
                }
            }
#endif
#endif
        }
        #endregion

        #region GetFileLastAccessTime
        /// <summary>
        /// Given a pathname, return the DateTime representing the last-access-time of that file or folder.
        /// </summary>
        /// <param name="filePathname">the filesystem-path of the file or folder</param>
        /// <returns>a DateTime dennoting the last-access-time</returns>
        public static DateTime GetFileLastAccessTime( string filePathname )
        {
            if (filePathname == null)
            {
                throw new ArgumentNullException( "filePathname" );
            }
#if NETFX_CORE
            return File.GetLastAccessTime( filePathname );
#else
#if (NO_LONGPATH)
            FileInfo fileInfo = new FileInfo(filePathname);
            return fileInfo.LastAccessTime;
#else
            string filePath = FileStringLib.CheckAddLongPathPrefix( filePathname );

            Native.Win32.WIN32_FIND_DATA fd;
            var result = Native.Win32.FindFirstFile( filePath.TrimEnd( '\\' ), out fd );

            if (result == Native.Win32.INVALID_HANDLE_VALUE)
            {
                return DateTime.MinValue;
            }
            else
            {
                try
                {
                    if (result.ToInt32() == Native.Win32.ERROR_FILE_NOT_FOUND)
                    {
                        return DateTime.MinValue;
                    }
                    else
                    {
                        var ft = fd.ftLastAccessTime;

                        var hft2 = (((long)ft.dwHighDateTime) << 32) + ft.dwLowDateTime;
                        return DateTime.FromFileTimeUtc( hft2 );
                    }
                }
                finally
                {
                    Native.Win32.FindClose( result );
                }
            }
#endif
#endif
        }
        #endregion

        #region GetFileLastWriteTime
        /// <summary>
        /// Given a pathname, return the DateTime representing the last-write-time of that file or folder.
        /// </summary>
        /// <param name="filePathname">the filesystem-path of the file or folder</param>
        /// <returns>a DateTime dennoting the last-write-time, or DateTime.MinValue if the file is not present</returns>
        /// <exception cref="ArgumentNullException">The argument supplied for <paramref name="filePathname"/> must not be null.</exception>
        public static DateTime GetFileLastWriteTime( string filePathname )
        {
            if (filePathname == null)
            {
                throw new ArgumentNullException( "filePathname" );
            }
#if NETFX_CORE
            return File.GetLastWriteTime( filePathname );
#else
            //CBL I also must test to see whether this copies over the date-time attributes
            //#if (NO_LONGPATH)
            FileInfo fileInfo = new FileInfo(filePathname);
            return fileInfo.LastWriteTime;
            //#else
            //filePathname = FileStringLib.CheckAddLongPathPrefix( filePathname );

            //Native.Win32.WIN32_FIND_DATA fd;
            //var result = Native.Win32.FindFirstFile( filePathname.TrimEnd( '\\' ), out fd );

            //if (result == Native.Win32.INVALID_HANDLE_VALUE)
            //{
            //    return DateTime.MinValue;
            //}
            //else
            //{
            //    try
            //    {
            //        if (result.ToInt32() == Native.Win32.ERROR_FILE_NOT_FOUND)
            //        {
            //            return DateTime.MinValue;
            //        }
            //        else
            //        {
            //            var ft = fd.ftLastWriteTime;

            //            var hft2 = (((long)ft.dwHighDateTime) << 32) + ft.dwLowDateTime;
            //            return DateTime.FromFileTimeUtc( hft2 );
            //        }
            //    }
            //    finally
            //    {
            //        Native.Win32.FindClose( result );
            //    }
            //}
            //#endif
#endif
        }
        #endregion

        #region GetFileLength
        /// <summary>
        /// Return the size in bytes of the file or folder dennoted by the given filesystem-path.
        /// </summary>
        /// <param name="filePath">the filesystem-path of the file or folder</param>
        /// <returns>the size of the file in bytes, as a 64-bit long value</returns>
        /// <exception cref="ArgumentNullException">The pathname must not be null</exception>
        /// <exception cref="FileNotFoundException">The pathname must already exist</exception>
        public static long GetFileLength( string filePath )
        {
            //CBL  This comes from ZLongPathLib.

            if (filePath == null)
            {
                throw new ArgumentNullException( "filePath" );
            }
            if (!FileExists( filePath ))
            {
                throw new FileNotFoundException( "The given file pathname was not found.", filePath );
            }
#if NETFX_CORE
            long length = 0;

            using (FileStream file = File.Open( filePath, FileMode.Open ))
            {
                length = file.Length;
            }

            return length;
#else
            filePath = FileStringLib.CheckAddLongPathPrefix( filePath );

            Native.Win32.WIN32_FIND_DATA fd;
            IntPtr result = Native.Win32.FindFirstFile( filePath.TrimEnd( '\\' ), out fd );

            if (result == Native.Win32.INVALID_HANDLE_VALUE)
            {
                return 0;
            }
            else
            {
                try
                {
                    //CBL  I was getting an overflow exception here, when using only the result.ToInt32 on a 64-bit process.
                    // Test this for both x86 and x64.
                    bool wasNotFound = false;

                    if (SystemLib.Is64BitProcess)
                    {
                        Int64 resultValue64 = result.ToInt64();
                        if (resultValue64 == Native.Win32.ERROR_FILE_NOT_FOUND)
                        {
                            wasNotFound = true;
                        }
                    }
                    else
                    {
                        int resultValue32 = result.ToInt32();
                        if (resultValue32 == Native.Win32.ERROR_FILE_NOT_FOUND)
                        {
                            wasNotFound = true;
                        }
                    }
                    if (wasNotFound)
                    //if (result.ToInt32() == PInvokeHelper.ERROR_FILE_NOT_FOUND)
                    {
                        return 0;
                    }
                    else
                    {
                        var low = (uint)fd.nFileSizeLow;
                        var high = (uint)fd.nFileSizeHigh;

                        return (((long)high) << 32) + low;
                    }
                }
                finally
                {
                    Native.Win32.FindClose( result );
                }
            }
#endif
        }
        #endregion

        #region GetFileOwner
        /// <summary>
        /// Return a string representing the 'Owner' of the given file.
        /// If the owner is of the administrator's group, then "Administrators" is returned.
        /// </summary>
        /// <param name="path">the full pathname of the file to get the owner of</param>
        /// <returns>a string denoting the owner of the given file</returns>
        public static string GetFileOwner( string path )
        {
            //CBL This is hanging. And it's ridiculous - need to fix it!

            //ManagementObject mgmt = new ManagementObject( "Win32_LogicalFileSecuritySetting.path='" + path + "'" );
            //ManagementBaseObject secDesc = mgmt.InvokeMethod( "GetSecurityDescriptor", null, null );
            //ManagementBaseObject descriptor = secDesc.Properties["Descriptor"].Value as ManagementBaseObject;
            //ManagementBaseObject owner = descriptor.Properties["Owner"].Value as ManagementBaseObject;
            //return owner.Properties["Domain"].Value.ToString() + "\\" + owner.Properties["Name"].Value.ToString();

            //var tmp = System.IO.File.GetAccessControl(fullPathname).GetOwner(typeof(System.Security.Principal.SecurityIdentifier));
            //var owner = tmp.Translate(typeof(System.Security.Principal.NTAccount));
            //Console.WriteLine("Owner: {0}", tmp.Translate(typeof(System.Security.Principal.NTAccount)));

            return "JH";
        }

        /// <summary>
        /// Return the owner of what is at the given filesystem-path, as a string.
        /// </summary>
        /// <param name="filePath">the filesystem-path to get the owner of</param>
        /// <returns>a string representing the owner</returns>
        public static string GetFileOwner2( string filePath )
        {
            //CBL This comes from ZLongPathLib

            // http://www.dotnet247.com/247reference/msgs/21/108780.aspx

            filePath = FileStringLib.CheckAddLongPathPrefix( filePath );

            IntPtr pZero;
            IntPtr pSid;
            IntPtr psd; // Not used here

            var errorReturn =
                Native.Win32.GetNamedSecurityInfo(
                    filePath,
                    Native.Win32.SeFileObject,
                    Native.Win32.OwnerSecurityInformation,
                    out pSid,
                    out pZero,
                    out pZero,
                    out pZero,
                    out psd );

            if (errorReturn == 0)
            {
                const int bufferSize = 64;
                var buffer = new StringBuilder();
                var accounLength = bufferSize;
                var domainLength = bufferSize;
                int sidNameUse;
                var account = new StringBuilder( bufferSize );
                var domain = new StringBuilder( bufferSize );

                errorReturn =
                    Native.Win32.LookupAccountSid(
                        null,
                        pSid,
                        account,
                        ref accounLength,
                        domain,
                        ref domainLength,
                        out sidNameUse );

                if (errorReturn == 0)
                {
                    // http://msdn.microsoft.com/en-us/library/ms681382(VS.85).aspx.

                    var lastWin32Error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(
                        lastWin32Error,
                        string.Format(
                            "Error {0} looking up account SID while getting file owner for file '{1}': {2}",
                            lastWin32Error,
                            filePath,
                            FileStringLib.CheckAddDotEnd( new Win32Exception( lastWin32Error ).Message ) ) );
                }
                else
                {
                    buffer.Append( domain );
                    buffer.Append( @"\" );
                    buffer.Append( account );
                    return buffer.ToString();
                }
            }
            else
            {
                // http://msdn.microsoft.com/en-us/library/ms681382(VS.85).aspx.

                var lastWin32Error = Marshal.GetLastWin32Error();
                throw new Win32Exception(
                    lastWin32Error,
                    string.Format(
                        "Error {0} getting names security info while getting file owner for file '{1}': {2}",
                        lastWin32Error,
                        filePath,
                        FileStringLib.CheckAddDotEnd( new Win32Exception( lastWin32Error ).Message ) ) );
            }
        }
        #endregion

        #region GetFiles
        /// <summary>
        /// Return an array of ZFileInfo objects representing the files in the given filesystem-folder
        /// </summary>
        /// <param name="directoryPath">the filesystem-folder to get the array of files from</param>
        /// <returns>an array of ZFileInfo objects denoting the files found</returns>
        /// <exception cref="ArgumentNullException">The values provided for <paramref name="directoryPath"/> must not be null.</exception>
        public static ZFileInfo[] GetFiles( string directoryPath )
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }
#if NETFX_CORE
            return (from eachPathname in Directory.GetFiles( path: directoryPath ) select new ZFileInfo( eachPathname )).ToArray();
#else
            return GetFiles( directoryPath, @"*.*" );
#endif
        }

        /// <summary>
        /// Return an array of ZFileInfo objects representing the files in the given filesystem-folder
        /// that match the given pattern.
        /// </summary>
        /// <param name="directoryPath">the filesystem-folder to get the array of files from</param>
        /// <param name="fileSpec">the filename-pattern to search for, such as *.txt</param>
        /// <returns>an array of ZFileInfo objects denoting the files that match the pattern</returns>
        /// <exception cref="ArgumentNullException">The values provided for <paramref name="directoryPath"/> must not be null.</exception>
        public static ZFileInfo[] GetFiles( string directoryPath, string fileSpec )
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }
#if NETFX_CORE
            return (from eachPathname in Directory.GetFiles( path: directoryPath, searchPattern: fileSpec ) select new ZFileInfo( eachPathname )).ToArray();
#else
            return GetFiles( directoryPath, fileSpec, SearchOption.TopDirectoryOnly );
#endif
        }

        /// <summary>
        /// Return an array of ZFileInfo objects representing the files in the given filesystem-folder,
        /// depending upon the given SearchOption
        /// </summary>
        /// <param name="directoryPath">the filesystem-folder to get the array of files from</param>
        /// <param name="searchOption">this indicates whether to search only the given folder, or all of its sub-folders</param>
        /// <returns>an array of ZFileInfo objects denoting the matching files that were found</returns>
        /// <exception cref="ArgumentNullException">The values provided for <paramref name="directoryPath"/> must not be null.</exception>
        public static ZFileInfo[] GetFiles( string directoryPath, SearchOption searchOption )
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }
#if NETFX_CORE
            return (from eachPathname in Directory.GetFiles( path: directoryPath, searchPattern: "*.*", searchOption: searchOption ) select new ZFileInfo( eachPathname )).ToArray();
#else
            return GetFiles( directoryPath, @"*.*", searchOption );
#endif
        }

        /// <summary>
        /// Return an array of ZFileInfo objects representing the files in the given filesystem-folder
        /// </summary>
        /// <param name="directoryPath">the filesystem-folder to get the array of files from</param>
        /// <param name="fileSpec">the filename-pattern to search for, such as *.txt</param>
        /// <param name="searchOption">this indicates whether to search only the given folder, or all of its sub-folders</param>
        /// <returns>an array of ZFileInfo objects denoting the matching files that were found</returns>
        /// <exception cref="ArgumentNullException">The values provided for <paramref name="directoryPath"/> must not be null.</exception>
        public static ZFileInfo[] GetFiles( string directoryPath, string fileSpec, SearchOption searchOption )
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }
#if NETFX_CORE
            return (from eachPathname in Directory.GetFiles( path: directoryPath, searchPattern: fileSpec, searchOption: searchOption ) select new ZFileInfo( eachPathname )).ToArray();
#else
            directoryPath = FileStringLib.CheckAddLongPathPrefix( directoryPath );

            var results = new List<ZFileInfo>();
            Native.Win32.WIN32_FIND_DATA findData;
            var findHandle = Native.Win32.FindFirstFile( directoryPath.TrimEnd( '\\' ) + "\\" + fileSpec, out findData );

            if (findHandle != Native.Win32.INVALID_HANDLE_VALUE)
            {
                try
                {
                    bool found;
                    do
                    {
                        var currentFileName = findData.cFileName;

                        // if this is a file, find its contents
                        if (((int)findData.dwFileAttributes & Native.Win32.FILE_ATTRIBUTE_DIRECTORY) == 0)
                        {
                            results.Add( new ZFileInfo( Path.Combine( directoryPath, currentFileName ) ) );
                        }

                        // find next
                        found = Native.Win32.FindNextFile( findHandle, out findData );
                    }
                    while (found);
                }
                finally
                {
                    // close the find handle
                    Native.Win32.FindClose( findHandle );
                }
            }

            if (searchOption == SearchOption.AllDirectories)
            {
                foreach (var dir in GetDirectories( directoryPath ))
                {
                    results.AddRange( GetFiles( dir.FullName, fileSpec, searchOption ) );
                }
            }

            return results.ToArray();
#endif
        }
        #endregion GetFiles

        #region GetFullPath
        /// <summary>
        /// Given a filename, return the full path to it - either as found as is,
        /// or else as found within one of the folders specified by the PATH environment-variable.
        /// </summary>
        /// <param name="fileName">the filename to check for</param>
        /// <returns>the full path to it, or else null to indicate not found</returns>
        public static string GetFullPath( string fileName )
        {
            // If the file exists as given, just return it's full path.
            if (File.Exists( fileName ))
                return Path.GetFullPath( fileName );
            // Otherwise, search the folders specified within the PATH environment-variable..
            string pathEnvValue = Environment.GetEnvironmentVariable( "PATH" );
            foreach (var folder in pathEnvValue.Split( ';' ))
            {
                // Check for the file within this folder.
                var fullPath = Path.Combine( folder, fileName );
                if (File.Exists( fullPath ))
                    return fullPath;
            }
            // If not found, return null to indicate this.
            return null;
        }
        #endregion

        #region GetLocalApplicationDataFolderPath
        /// <summary>
        /// Return a string that denotes the filesystem-path of the local application data folder.
        /// On UWP this is (what?) and on .NET Framework it is the ?
        /// </summary>
        /// <returns>a string denoting the path of the local data folder</returns>
        public static string GetLocalApplicationDataFolderPath()
        {
            //CBL Unsure of whether this is appropriate. I added this, for the benefit of the UserSettings class.
#if NETFX_CORE
            return Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#else
            return Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData );
#endif
        }
        #endregion

        #region GetMyLocalFolderPath
        /// <summary>
        /// Return a string that denotes the filesystem-path of the user's private, local data folder.
        /// On UWP this is LocalFolder, and on .NET Framework it is the My Documents Folder.
        /// </summary>
        /// <returns>a string denoting the path of the local data folder</returns>
        /// <remarks>
        /// This method exists in order to provide a platform-neutral way of acquiring the value of
        /// the Local-Folder (from the UWP's perspective), or equivalently, the My Documents folder
        /// (from the .NET Framework perspective) in a platform-neutral way.
        /// 
        /// On the Universal Windows Platform (or Windows Store Apps), this is the Path property
        /// of the Local Folder.
        /// 
        /// This should give you something like (as on Win10, .NET Framework platform) C:\Users\{user}\Documents.
        /// </remarks>
        public static string GetMyLocalFolderPath()
        {
#if NETFX_CORE
            return Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#else
            return Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
#endif
        }
        #endregion

        #region GetRemovableDrives
        /// <summary>
        /// Return the removable drives that are present
        /// as an array of strings, each being the drive-letter-colon "E:" of the removable drive.
        /// </summary>
        /// <returns>an array of strings each denoting the drive found</returns>
        public static string[] GetRemovableDriveTexts()
        {
            List<string> result = new List<string>();
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Removable)
                {
                    //string rootDir = drive.Name;
                    string driveLetter = FileStringLib.GetDrive( drive.Name );
                    result.Add( driveLetter );
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Return the removable drives that are present
        /// as an array of DriveInfo objects.
        /// </summary>
        /// <returns>an array of DriveInfo objects each denoting a drive that is ready and removable</returns>
        public static DriveInfo[] GetRemovableDrives()
        {
            List<DriveInfo> driveList = new List<DriveInfo>();

            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Removable)
                {
                    driveList.Add( drive );
                }
            }
            return driveList.ToArray();
        }

        /// <summary>
        /// Return a string containing a space-separated list of the removable drives that are currently present and ready,
        /// this list denoting each drive with it's drive-letter and colon.
        /// </summary>
        /// <returns>a string of the form "D: E: F:" to denote the removable drives found</returns>
        public static string GetRemovableDrivesText()
        {
            var sb = new StringBuilder();
            int n = 0;
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Removable)
                {
                    n++;
                    string driveLetter = FileStringLib.GetDrive( drive.Name );
                    if (n > 1)
                    {
                        sb.Append( " " );
                    }
                    sb.Append( driveLetter );
                }
            }
            if (n == 0)
            {
                sb.Append( "None!" );
            }
            return sb.ToString();
        }
        #endregion GetRemovableDrives

        #region GetTheSharedLocalFolderPath
        /// <summary>
        /// Return a string that denotes the filesystem-path of the shared, or 'public', local data folder.
        /// On UWP this is SharedLocalFolder,
        /// on .NET Framework it is the Public Documents Folder,
        /// and on .NET 3.5 this returns the "My Documents" folder.
        /// </summary>
        /// <returns>a string denoting the path to the appropriate documents folder</returns>
        /// <remarks>
        /// This method exists in order to provide a platform-neutral way of acquiring the value of
        /// the Shared-Local-Folder (from the UWP's perspective), or equivalently, the Public Documents folder
        /// (from the .NET Framework perspective) in a platform-neutral way.
        /// 
        /// On the Universal Windows Platform (or Windows Store Apps), this is the Path property
        /// of the Shared Local Folder.
        /// 
        /// This should give you something like (as on Win10, .NET Framework platform) C:\Users\Public\Documents.
        /// 
        /// On the .NET Framework 3.5, there is no equivalent folder provided by the API
        /// so the path to the "My Documents" folder is returned.
        /// </remarks>
        public static string GetTheSharedLocalFolderPath()
        {
#if NETFX_CORE
            string publicFolder = Windows.Storage.ApplicationData.Current.SharedLocalFolder.Path;
#else
#if !PRE_4
            string publicFolder = Environment.GetFolderPath( Environment.SpecialFolder.CommonDocuments );
#else
            string publicFolder = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments );
#endif
#endif
            return publicFolder;
        }
        #endregion

        #region GetProgramExecutionDirectory
#if !NETFX_CORE
        /// <summary>
        /// Return the directory of this executing program, getting it from the assembly.
        /// </summary>
        /// <returns>the directory-name (of the program or assembly) as a string</returns>
        public static string GetProgramExecutionDirectory()
        {
            //CBL I don't think I need this fake-value facility, since we do have now the interface.
            if (_fakeValueToReturnForProgramExecutionDirectory == null)
            {
                //CBL There seems to another, probably better way. Try using
                //    var dir = $"{AppDomain.CurrentDomain.BaseDirectory}"

                string executionDirectory;
                // Attempt to get it for web applications.
                var httpContext = System.Web.HttpContext.Current;
                if (httpContext != null)
                {
                    var assembly = httpContext.ApplicationInstance.GetType().BaseType.Assembly;
                    var codebase = assembly.CodeBase;
                    executionDirectory = FileStringLib.GetDirectoryOfPath( codebase );
                }
                else // not a web-app?
                {
                    var assembly = Assembly.GetEntryAssembly();
                    string location;
                    if (assembly != null)
                    {
                        location = assembly.Location;
                    }
                    else // this happens when running as a unit-test.
                    {
                        var callingAssembly = Assembly.GetCallingAssembly();
                        location = callingAssembly.Location;
                    }
                    executionDirectory = FileStringLib.GetDirectoryOfPath( location );
                }
                return executionDirectory;
            }
            else
            {
                return _fakeValueToReturnForProgramExecutionDirectory;
            }
        }

        /// <summary>
        /// Return the directory (drive + folder) of this executing program, getting it from the assembly,
        /// and also output the separate drive and folder strings.
        /// </summary>
        /// <param name="drive">the disk-drive of the executing program gets written to this</param>
        /// <param name="folder">the folder (wihtout disk-drive) of the executing program</param>
        /// <returns>the directory-name (of the program or assembly) as a string</returns>
        public static string GetProgramExecutionDirectory( out string drive, out string folder )
        {
            string executionDirectory = GetProgramExecutionDirectory();
            FileStringLib.GetDriveAndDirectory( executionDirectory, out drive, out folder );
            return executionDirectory;
        }

        /// <summary>
        /// Specify a value that we want to force the method <see cref="GetProgramExecutionDirectory()"/> to return
        /// - as for unit-testing. Set this to null to remove the forced-fake value.
        /// </summary>
        public static void SetValueToReturnForProgramExecutionDirectory( string fakeValueToUse )
        {
            //CBL  Or, use a mocking-object instead.
            _fakeValueToReturnForProgramExecutionDirectory = fakeValueToUse;
        }

        /// <summary>
        /// If this is non-null, then it represents a value that we want to force the method <see cref="GetProgramExecutionDirectory()"/> to return
        /// - as for unit-testing.
        /// </summary>
        private static string _fakeValueToReturnForProgramExecutionDirectory;
#endif
        #endregion

        #region HasContent
        /// <summary>
        /// Return true if the given directory exists and has at least one file within it or within any of it's sub-directories.
        /// </summary>
        /// <param name="directoryPath">the filesystem-directory to look in</param>
        /// <returns>true if the given directory (or any sub-directory within it) contains any files</returns>
        public static bool HasContent( string directoryPath )
        {
            // Validate the argument values (only a minimal, cursory set of checks)..
            if (directoryPath == null)
            {
                throw new ArgumentNullException( nameof( directoryPath ) );
            }

            if (DirectoryExists( directoryPath ))
            {
#if !PRE_4
                foreach (var whatever in Directory.EnumerateFiles( path: directoryPath ))
                {
                    return true;
                }
                foreach (string subdirectory in Directory.EnumerateDirectories( path: directoryPath ))
                {
                    return HasContent( subdirectory );
                }
#else
                //CBL I may want to see whether this is better to use in place of the above.
                return Directory.GetFileSystemEntries( directoryPath ).Length > 0;
#endif
            }
            return false;
        }
        #endregion

        #region IsDirectoryHidden
        /// <summary>
        /// Return true if the given directory has it's "Hidden" attribute set.
        /// </summary>
        /// <param name="directoryPath">the filesystem-path denoting directory to get the Hidden-attribute of</param>
        /// <returns>true if the Hidden-attribute is on for the given directory</returns>
        /// <exception cref="ArgumentNullException">The argument supplied for <paramref name="directoryPath"/> must not be null.</exception>
        public static bool IsDirectoryHidden( string directoryPath )
        {
            //UT: done
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }
            FileAttributes attributes = GetDirectoryAttributes( directoryPath );
            return (attributes & FileAttributes.Hidden) != 0;
        }
        #endregion

        #region IsDirectoryReadonly
        /// <summary>
        /// Return true if the given directory has it's "Read-Only" attribute set.
        /// </summary>
        /// <param name="directoryPath">the filesystem-path denoting directory to get the readonly-attribute of</param>
        /// <returns>true if the readonly-attribute is on for the given directory</returns>
        /// <exception cref="ArgumentNullException">The argument supplied for <paramref name="directoryPath"/> must not be null.</exception>
        public static bool IsDirectoryReadonly( string directoryPath )
        {
            //UT: Needs tests!
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }
            FileAttributes attributes = GetDirectoryAttributes( directoryPath );
            return (attributes & FileAttributes.ReadOnly) != 0;
        }
        #endregion

        #region IsFileHidden
        /// <summary>
        /// Return true if the given file has it's "Hidden" attribute set.
        /// </summary>
        /// <param name="filePathname">the filesystem-pathname of the file to get the Hidden attribute of</param>
        /// <returns>true if the Hidden attribute is on for the given file</returns>
        /// <exception cref="ArgumentNullException">The argument supplied for <paramref name="filePathname"/> must not be null.</exception>
        public static bool IsFileHidden( string filePathname )
        {
            if (filePathname == null)
            {
                throw new ArgumentNullException( "filePathname" );
            }
            FileAttributes attributes = GetFileAttributes( filePathname );
            return (attributes & FileAttributes.Hidden) != 0;
        }
        #endregion

        #region IsFileReadonly
        /// <summary>
        /// Return true if the given file has it's "Read-Only" attribute set.
        /// </summary>
        /// <param name="filePathname">the filesystem-pathname of the file to get the readonly-attribute of</param>
        /// <returns>true if the readonly-attribute is on for the given file</returns>
        /// <exception cref="ArgumentNullException">The argument supplied for <paramref name="filePathname"/> must not be null.</exception>
        public static bool IsFileReadonly( string filePathname )
        {
            if (filePathname == null)
            {
                throw new ArgumentNullException( "filePathname" );
            }
            FileAttributes attributes = GetFileAttributes( filePathname );
            return (attributes & FileAttributes.ReadOnly) != 0;
        }
        #endregion

        #region IsFileLocked
        /// <summary>
        /// Return true if the given file is currently locked against being opened.
        /// </summary>
        /// <param name="pathname">the pathname of the file to check</param>
        /// <returns>true if the file is locked against being opened</returns>
        public static bool IsFileLocked( string pathname )
        {
            // See https://stackoverflow.com/questions/1304/how-to-check-for-file-lock
            try
            {
                using (File.Open( pathname, FileMode.Open ))
                {
                }
            }
            catch (IOException x)
            {
                var errorCode = Marshal.GetHRForException( x ) & ( ( 1 << 16 ) - 1 );
                return errorCode == 32 || errorCode == 33;
            }
            return false;
        }
        #endregion

        #region MoveDirectory
        /// <summary>
        /// Move the folder from the given source path to the destination path.
        /// </summary>
        /// <param name="sourceDirectoryPath">where to move the folder from</param>
        /// <param name="destinationDirectoryPath">where to move it to</param>
        public static void MoveDirectory( string sourceDirectoryPath,
                                          string destinationDirectoryPath )
        {
            if (sourceDirectoryPath == null)
            {
                throw new ArgumentNullException( nameof( sourceDirectoryPath ) );
            }
            if (destinationDirectoryPath == null)
            {
                throw new ArgumentNullException( nameof( destinationDirectoryPath ) );
            }
            if (!DirectoryExists( sourceDirectoryPath ))
            {
                throw new DirectoryNotFoundException( message: "sourceDirectoryPath " + sourceDirectoryPath + " not found." );
            }

            if (!Native.Win32.MoveFile( sourceDirectoryPath, destinationDirectoryPath ))
            {
                // http://msdn.microsoft.com/en-us/library/ms681382(VS.85).aspx.

                var lastWin32Error = Marshal.GetLastWin32Error();
                throw new Win32Exception(
                    lastWin32Error,
                    string.Format(
                        "Error {0} moving directory '{1}' to '{2}': {3}",
                        lastWin32Error,
                        sourceDirectoryPath,
                        destinationDirectoryPath,
                        FileStringLib.CheckAddDotEnd( new Win32Exception( lastWin32Error ).Message ) ) );
            }
        }
        #endregion

        #region MoveDirectoryContent
        /// <summary>
        /// Move the content of the given source-directory to under the given destination-parent-directory. Not including sub-directories.
        /// </summary>
        /// <param name="sourceDirectory">This is the source-directory to move the files out of.</param>
        /// <param name="destinationParentDirectory">This is the directory to move the files into. This must already exist.</param>
        /// <param name="fileMatchExpression">a Windows files-spec pattern expression that the files must match. Null indicates all files. Null = everything</param>
        /// <param name="isToRolloverExistingDestinFile">if true - then rollover any destination filenames that already exists in the destination, so that it does not get overwritten</param>
        /// <exception cref="DirectoryNotFoundException">Both the source-directory and the destination-parent-directory must already exist.</exception>
        /// <remarks>
        /// The destination-parent-directory must already exist, otherwise an exception is thrown.
        /// Both the source and destination-parent directories must be there before this method is called.
        ///
        /// This checks first to see whether the destination file, if it already exists, has the exact same content as the source file
        /// and only makes the move if it does not. Otherwise it simply deletes the source file.
        /// </remarks>
        public static void MoveDirectoryContent( string sourceDirectory,
                                                 string destinationParentDirectory,
                                                 string fileMatchExpression,
                                                 bool isToRolloverExistingDestinFile )
        {
            //CBL Must test!
            // Validate the argument values (only a minimal, cursory set of checks)..
            if (sourceDirectory == null)
            {
                throw new ArgumentNullException( nameof( sourceDirectory ) );
            }
            if (destinationParentDirectory == null)
            {
                throw new ArgumentNullException( nameof( destinationParentDirectory ) );
            }
            if (!DirectoryExists( sourceDirectory ))
            {
                throw new DirectoryNotFoundException( message: "sourceDirectory " + sourceDirectory + " not found." );
            }
            if (!DirectoryExists( destinationParentDirectory ))
            {
                throw new DirectoryNotFoundException( message: "destinationParentDirectory " + destinationParentDirectory + " not found." );
            }

            string fileSpec;
            if (fileMatchExpression == null)
            {
                fileSpec = "*";
            }
            else
            {
                fileSpec = fileMatchExpression;
            }
#if !PRE_4
            foreach (string sourceFilePathname in Directory.EnumerateFiles( path: sourceDirectory, searchOption: SearchOption.TopDirectoryOnly, searchPattern: fileSpec ))
            {
#else
            foreach (string sourceFilePathname in Directory.GetFiles( path: sourceDirectory, searchPattern: fileSpec, searchOption: SearchOption.TopDirectoryOnly))
            {
#endif
                bool hasSameContent;
                string filename = FileStringLib.GetFileNameFromFilePath(sourceFilePathname);
                string destinationPathname = Path.Combine(destinationParentDirectory, filename);

                if (FileExists( destinationPathname ))
                {
                    // Make a backup copy of the destination file if that file already exists and a rollover is called for.
                    if (isToRolloverExistingDestinFile)
                    {
                        // There is no need to backup the destination file if the newer one has the exact same content.
                        if (!FilesHaveSameContent( sourceFilePathname, destinationPathname ))
                        {
                            hasSameContent = false;
                            Rollover( outputFilePathname: destinationPathname, archiveFileFolder: null, maxBackups: 100 );
                        }
                        else
                        {
                            hasSameContent = true;
                        }
                    }
                    else
                    {
                        hasSameContent = FilesHaveSameContent( sourceFilePathname, destinationPathname );
                    }
                }
                else
                {
                    hasSameContent = false;
                }

                if (!hasSameContent)
                {
                    MoveFile( sourceFilePath: sourceFilePathname, destinationFilePath: destinationPathname );
                }
                else
                {
                    DeleteFile( sourceFilePathname );
                }
            }
        }
        #endregion MoveDirectoryContent

        #region MoveFile( sourceFilePath, destinationFilePath )
        /// <summary>
        /// Given two filesystem-paths, move the file indicated by the first path to the second path. This can also be a renaming operation.
        /// If the destinationFilePath already exists, this method deletes it first.
        /// </summary>
        /// <param name="sourceFilePath">the source path that the file will be moved or renamed from</param>
        /// <param name="destinationFilePath">the destination path that it will be moved or renamed to</param>
        public static void MoveFile( string sourceFilePath,
                                     string destinationFilePath )
        {
            // Check the argument values..
            if (sourceFilePath == null)
            {
                throw new ArgumentNullException( "sourceFilePath" );
            }
            if (destinationFilePath == null)
            {
                throw new ArgumentNullException( "destinationFilePath" );
            }
            if (!File.Exists( sourceFilePath ))
            {
                throw new FileNotFoundException( message: "That sourceFilePath does not seem to be present.", fileName: sourceFilePath );
            }
            try
            {
                //CBL
                string destinationFolder = FileStringLib.GetDirectoryPathNameFromFilePath( destinationFilePath );
                if (!DirectoryExists( destinationFolder ))
                {
                    CreateDirectory( destinationFolder );
                }

#if (NO_LONGPATH)

                File.Move(sourceFilePath, destinationFilePath);
#else
                if (FileExists( destinationFilePath ))
                {
                    DeleteFile( destinationFilePath );
                }

                if (!Native.Win32.MoveFile( sourceFilePath, destinationFilePath ))
                {
                    // http://msdn.microsoft.com/en-us/library/ms681382(VS.85).aspx.

                    var lastWin32Error = Marshal.GetLastWin32Error();
                    throw new Win32Exception( lastWin32Error,
                                              "Error " + lastWin32Error + " moving file \"" + sourceFilePath + "\" to \"" + destinationFilePath + "\": " +
                                              FileStringLib.CheckAddDotEnd( new Win32Exception( lastWin32Error ).Message ) );
                }
#endif
            }
            //CBL  How do we want to handle this?
            //catch (IOException x)
            //{
            //    // Try doing it differently..
            //    File.Copy(sourceFilePath, destinationFilePath);
            //    File.Delete(sourceFilePath);
            //}
            catch (Exception x)
            {
                // Wrap up some additional detail into the exception and re-throw it.
                var sb = new StringBuilder();
                sb.Append( x.GetType() );
                sb.Append( @" in FilesystemLib.MoveFile(""" );
                sb.Append( sourceFilePath );
                sb.Append( @""", """ );
                sb.Append( destinationFilePath );
                sb.Append( @"""): " );
                sb.Append( x.Message );
                x.Data.Add( "Context", sb.ToString() );
                throw;
            }
        }
        #endregion

        #region MoveFile( sourceFilePath, destinationFilePath, timeoutInMilliseconds )
        /// <summary>
        /// Given two filesystem-paths, source and destination, move the source to the second destination, and retry if necessary.
        /// This can also be a renaming operation.
        /// Similar to <see cref="File.Move"/> except that if an IOException is thrown, this retries the operation for up to timeoutInMilliseconds.
        /// If the destinationFilePath already exists, deletes it first.
        /// </summary>
        /// <param name="sourceFilePath">the source path that the file will be moved or renamed from</param>
        /// <param name="destinationFilePath">the destination path that it will be moved or renamed to</param>
        /// <param name="timeoutInMilliseconds">a limit on how long to keep trying before giving up, in milliseconds (0 means no retrying)</param>
        /// <remarks>
        /// This method wraps the MoveFile method with a retry-block. If an <c>IOException</c>
        /// occurs it waits for a period of time and then tries it again.
        /// The time that this waits between attempts starts at 50 milliseconds, and doubles with each time until the timeout is exceeded
        /// after which it re-throws the exception.
        /// </remarks>
        public static void MoveFile( string sourceFilePath,
                                     string destinationFilePath,
                                     int timeoutInMilliseconds )
        {
            MoveFile( sourceFilePath, destinationFilePath, null, timeoutInMilliseconds );
        }
        #endregion

        #region MoveFile( sourceFilePath, destinationFilePath, actionForRetryNotification, timeoutInMilliseconds )
        /// <summary>
        /// Given two filesystem-paths, source and destination, move the source to the second destination, and retry if necessary.
        /// This can also be a renaming operation.
        /// Similar to <see cref="File.Move"/> except that if an IOException is thrown, this retries the operation for up to timeoutInMilliseconds.
        /// If the destinationFilePath already exists, deletes it first.
        /// </summary>
        /// <param name="sourceFilePath">the source path that the file will be moved or renamed from</param>
        /// <param name="destinationFilePath">the destination path that it will be moved or renamed to</param>
        /// <param name="actionForRetryNotification">an action to call when a retry occurs (may be null)</param>
        /// <param name="timeoutInMilliseconds">a limit on how long to keep trying before giving up, in milliseconds (0 means no retrying)</param>
        /// <remarks>
        /// This method wraps the MoveFile method with a retry-block. If an <c>IOException</c>
        /// occurs it waits for a period of time and then tries it again.
        /// The time that this waits between attempts starts at 50 milliseconds, and doubles with each time until the timeout is exceeded
        /// after which it re-throws the exception.
        /// </remarks>
        public static void MoveFile( string sourceFilePath,
                                     string destinationFilePath,
                                     Action<string> actionForRetryNotification,
                                     int timeoutInMilliseconds )
        {
            //CBL I copied this from my code within ZIOHelper. Should really consolidate this stuff.
            if (sourceFilePath == null)
            {
                throw new ArgumentNullException( "sourceFilePath" );
            }
            if (destinationFilePath == null)
            {
                throw new ArgumentNullException( "destinationFilePath" );
            }
            if (!FileExists( sourceFilePath ))
            {
                throw new FileNotFoundException( "Could not find sourceFilePath '" + sourceFilePath + "'" );
            }
            int nAttempt = 1;
            bool wasSuccessful = false;
            Stopwatch stopwWatch = Stopwatch.StartNew();
            int retryDelay = 50;

            while (!wasSuccessful)
            {
                try
                {
                    // If the destination-file is already there,
                    if (FileExists( destinationFilePath ))
                    {
                        // Ensure it is not readonly.
                        if (IsFileReadonly( destinationFilePath ))
                        {
                            SetFileReadonly( destinationFilePath, false );
                        }
                        // And remove it.
                        DeleteFile( destinationFilePath );
                    }
                    else
                    {
                        // If the destination folder does not yet exist, create it.
                        string destinationFolder = FileStringLib.GetDirectoryPathNameFromFilePath( destinationFilePath );
                        if (!ZDirectory.Exists( destinationFolder ))
                        {
                            ZDirectory.CreateDirectory( destinationFolder );
                        }
                    }

                    //CBL  Do I not want to use my ZLongPathLib version of Move ?
                    FilesystemLib.MoveFile( sourceFilePath, destinationFilePath );

                    wasSuccessful = true;
#if DEBUG
                    if (nAttempt > 1)
                    {
                        Debug.WriteLine( "MoveFile succeeded after " + nAttempt + " attempts." );
                    }
#endif
                }
                catch (DirectoryNotFoundException x)
                {
                    const string Key1 = "sourceFilePath:";
                    if (!x.Data.Contains( Key1 ))
                    {
                        x.Data.Add( Key1, sourceFilePath );
                    }
                    x.Data.Add( "destinationFilePath", destinationFilePath );
                    throw;
                }
                catch (Exception x)
                {
                    string msg = String.Format( "Upon attempt {0} FilesystemLib.MoveFile({1}, {2}) failed with error: {3}", nAttempt, sourceFilePath, destinationFilePath, x.Message );
                    if (actionForRetryNotification != null)
                    {
                        actionForRetryNotification( msg );
                    }
                    if (stopwWatch.ElapsedMilliseconds > timeoutInMilliseconds)
                    {
                        stopwWatch.Stop();
                        Debug.WriteLine( msg + Environment.NewLine + "- giving up." );
                        throw;
                    }
                    else
                    {
                        Debug.WriteLine( msg + "," + Environment.NewLine + "  will wait " + retryDelay + " ms and try again." );
                        Thread.Sleep( retryDelay );
                        // The delay will proceed through the values 50, 100, 200, 400, 800, 1000, 2000, 3000.. 10000.
                        if (retryDelay < 1000)
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
        }
        #endregion MoveFile( sourceFilePath, destinationFilePath, actionForRetryNotification, timeoutInMilliseconds )

        #region OpenNotepadWithFile
#if !NETFX_CORE
        /// <summary>
        /// Open Windows NotePad to display the given text-file.
        /// </summary>
        /// <param name="pathnameOfTextFile">the pathname of the text-file to open</param>
        /// <returns>0 if successful, -1 if otherwise</returns>
        /// <remarks>
        /// Yes, this is a rather trivial method. Opening Notepad was a particularly common usage of RunProgram in our code.
        /// </remarks>
        public static int OpenNotepadWithFile( string pathnameOfTextFile )
        {
            return RunProgram( "NotePad.exe", pathnameOfTextFile );
        }
#endif
        #endregion

        #region ReadAllBytes
        /// <summary>
        /// Read all of the bytes from the file at the given path and return it as an array.
        /// </summary>
        /// <param name="pathname">the filesystem-path of the file to read</param>
        /// <returns>an array of bytes containing all of the contents of the file</returns>
        public static byte[] ReadAllBytes( string pathname )
        {
#if NETFX_CORE
            return File.ReadAllBytes( path: pathname );
#else
            using (var fs =
                new FileStream(
                    CreateFileHandle( pathname,
                                      Native.CreationDisposition.OpenAlways,
                                      Native.FileAccess.GenericRead,
                                      Native.FileShare.Read,
                                      0 ),
                    System.IO.FileAccess.Read ))
            {
                var buf = new byte[fs.Length];
                fs.Read( buf, 0, buf.Length );

                return buf;
            }
#endif
        }
        #endregion

        #region ReadAllText
        /// <summary>
        /// Read all of the text from the indicated file, and then closes it.
        /// </summary>
        /// <param name="pathname">the filesystem-path of the file to get the text from</param>
        /// <returns>a string containing all of the text read from the file</returns>
        public static string ReadAllText( string pathname )
        {
#if NETFX_CORE
            return File.ReadAllText( path: pathname );
#else
            //var encoding = new UTF8Encoding( false, true );
            var encoding = Encoding.Default;
            return ReadAllText( pathname, encoding );
#endif
        }

        /// <summary>
        /// Read all of the text from the file at the given path.
        /// </summary>
        /// <param name="pathname">the filesystem-path of the file to get the text from</param>
        /// <param name="encoding">the Encoding to use to interpret the bytes as they are read from the file</param>
        /// <returns>a string containing all of the text read from the file</returns>
        public static string ReadAllText( string pathname,
                                          Encoding encoding )
        {
#if NETFX_CORE
            return File.ReadAllText( path: pathname, encoding: encoding );
#else
            SafeFileHandle fileHandle = CreateFileHandle( pathname,
                                                          Native.CreationDisposition.OpenAlways,
                                                          Native.FileAccess.GenericRead,
                                                          Native.FileShare.Read,
                                                          0 );
            //CBL  Just trying to see whether Seek would help.
            Seek( fileHandle, 0, SeekOrigin.Begin );

            using (var fs = new FileStream( fileHandle, FileAccess.Read ))
            {
                using (var sr = new StreamReader( fs, encoding ))
                {
                    return sr.ReadToEnd();
                }
            }

            //using (var fs =
            //    new FileStream(
            //        CreateFileHandle( path,
            //                          Native.CreationDisposition.OpenAlways,
            //                          Native.FileAccess.GenericRead,
            //                          Native.FileShare.Read,
            //                          0 ),
            //        System.IO.FileAccess.Read ))
            //using (var sr = new StreamReader( fs, encoding ))
            //{
            //    return sr.ReadToEnd();
            //}
#endif
        }
        #endregion

        #region ReadLines
        /// <summary>
        /// Read the lines of the given file.
        /// </summary>
        /// <param name="path">the pathname of the file to read</param>
        /// <returns>a collection of strings representing the lines of text from the file</returns>
        /// <exception cref="ArgumentNullException">the value specified for the path must not be null</exception>
        /// <exception cref="FileNotFoundException">the file specified must actually exist</exception>
        public static IEnumerable<string> ReadLines( string path )
        {
#if !PRE_4
            return File.ReadLines( path );
#else
            List<string> lines = new List<string>();
            StreamReader file = new StreamReader( path );
            string line;
            while ((line = file.ReadLine()) != null)
            {
                lines.Add( line );
            }
            return lines;
#endif
        }
        #endregion

        //CBL  How to do this?  If I am to process each record individually, without reading the entire file first
        // then it seems I'd need to keep the file open, and maintain a position within it.
        // CBL  This probably does not need to be distinct from ReadAllText

        #region ReadText
        /// <summary>
        /// Read text from the given file, up to the given number of characters,
        /// and return that text as a string.
        /// </summary>
        /// <param name="path">the filesystem-file to read the text from</param>
        /// <param name="numberOfCharacters">the number of text characters to read</param>
        /// <returns>a string containing the characters read from the file</returns>
        /// <exception cref="ArgumentNullException">if path is null</exception>
        /// <exception cref="FileNotFoundException">if path represents a non-existing file</exception>
        public static string ReadText( string path,
                                       int numberOfCharacters )
        {
            if (path == null)
            {
                throw new ArgumentNullException( "path" );
            }
            if (!FileExists( path ))
            {
                throw new FileNotFoundException( "It makes no sense to try to read text of a non-existent file.", path );
            }
            var sb = new StringBuilder();
            var encoding = new UTF8Encoding( false, true );
            int n = 0;
            using (var fileStream = new FileStream( CreateFileHandle( path,
                                                              Native.CreationDisposition.OpenAlways,
                                                              Native.FileAccess.GenericRead,
                                                              Native.FileShare.Read,
                                                              0 ),
                                            System.IO.FileAccess.Read ))
            using (var sr = new StreamReader( fileStream, encoding ))
            {
                // Loop through the file, one character at a time..
                while (!sr.EndOfStream && n < numberOfCharacters)
                {
                    int iChar = sr.Read();
                    // If Read returns -1 that indicates end-of-file.
                    // Regard a NUL as a termination character also.
                    if (iChar <= 0)
                    {
                        break;
                    }
                    else
                    {
                        sb.Append( (char)iChar );
                        n++;
                    }
                }
            }
            //CBL The above seems quite inefficient.
            // int n = fileStream.Read( buffer: buffer, offset: 0, count: 0 );
            return sb.ToString();
        }
        #endregion

        #region ReadTextAsListOfRecords
        /// <summary>
        /// Read all of the text from the given file into a List of strings,
        /// using the given record-terminator character as a delimiter to break up the text.
        /// </summary>
        /// <param name="pathname">the filesystem-pathname of the file to read the text from</param>
        /// <param name="recordTerminationChar">a character to use to mark the boundaries between records</param>
        /// <returns>a list of strings from the file</returns>
        /// <exception cref="ArgumentNullException">if path is null</exception>
        /// <exception cref="ArgumentException">if path is not null but is an empty string or just white-space</exception>
        /// <exception cref="FileNotFoundException">if path represents a non-existing file</exception>
        public static List<string> ReadTextAsListOfRecords( string pathname,
                                                            char recordTerminationChar )
        {
            if (pathname == null)
            {
                throw new ArgumentNullException( "pathname" );
            }
            if (StringLib.HasNothing( pathname ))
            {
                throw new ArgumentException( "The pathname must not be an empty string" );
            }
            if (!FileExists( pathname ))
            {
                throw new FileNotFoundException( "The pathname ('" + pathname + "') is not found" );
            }
            var result = new List<string>();
            var sb = new StringBuilder();
            var encoding = new UTF8Encoding( false, true );
            int iChar = -1;
            int iTerminator = (int)recordTerminationChar;

            using (var fileStream = new FileStream( CreateFileHandle( pathname,
                                                              Native.CreationDisposition.OpenAlways,
                                                              Native.FileAccess.GenericRead,
                                                              Native.FileShare.Read,
                                                              0 ),
                                            System.IO.FileAccess.Read ))
            using (var sr = new StreamReader( fileStream, encoding ))
            {
                // Loop through the file, one character at a time..
                while (!sr.EndOfStream)
                {
                    iChar = sr.Read();
                    // If Read returns -1 that indicates end-of-file.
                    // Regard a NUL as a termination character also.
                    if (iChar <= 0)
                    {
                        break;
                    }
                    else if (iChar == iTerminator)
                    {
                        // We encountered a record-termination character.
                        // Gather the text accumulated thus far into a record and add that to our result.
                        string thisLine = sb.ToString();
                        if (!StringLib.HasNothing( thisLine ))
                        {
                            result.Add( thisLine );
                        }
#if !PRE_4
                        sb.Clear();
#else
                        sb.Length = 0;
#endif
                    }
                    else
                    {
                        // It is not the end of the file, nor of a record, so just accumulate this character.
                        sb.Append( (char)iChar );
                    }
                }
                // We finished the file. Gather the last text into a final record, if any..
                string lastLine = sb.ToString();
                if (!StringLib.HasNothing( lastLine ))
                {
                    result.Add( lastLine );
                }
            }
            return result;
        }

        /// <summary>
        /// Read all of the text from the given file into a List of strings,
        /// using the given record-terminator text as a delimiter to break up the text.
        /// </summary>
        /// <param name="pathname">the filesystem-file to read the text from</param>
        /// <param name="recordTermination">a string that marks the end of a record</param>
        /// <returns>a list of strings from the file</returns>
        /// <exception cref="ArgumentNullException">if path is null</exception>
        /// <exception cref="ArgumentException">if path is not null but is an empty string or just white-space</exception>
        /// <exception cref="FileNotFoundException">if path represents a non-existing file</exception>
        public static List<string> ReadTextAsListOfRecords( string pathname,
                                                            string recordTermination )
        {
            if (pathname == null)
            {
                throw new ArgumentNullException( "pathname" );
            }
            if (StringLib.HasNothing( pathname ))
            {
                throw new ArgumentException( "Why are you calling ReadTextAsListOfRecords with an empty pathname?" );
            }
            if (!FileExists( pathname ))
            {
                throw new FileNotFoundException( "It makes no sense to try to read text of a non-existent file.", pathname );
            }
            //CBL Redundant code - combine this method with the above.
            var result = new List<string>();
            var sb = new StringBuilder();
            var encoding = new UTF8Encoding( false, true );
            int iChar = -1;

            using (var fs = new FileStream( CreateFileHandle( pathname,
                                                              Native.CreationDisposition.OpenAlways,
                                                              Native.FileAccess.GenericRead,
                                                              Native.FileShare.Read,
                                                              0 ),
                                           System.IO.FileAccess.Read ))
            using (var sr = new StreamReader( fs, encoding ))
            {
                // Loop through the file, one character at a time..
                while (!sr.EndOfStream)
                {
                    iChar = sr.Read();
                    // If Read returns -1 that indicates end-of-file.
                    // Regard a NUL as a termination character also.
                    if (iChar <= 0)
                    {
                        break;
                    }
                    else
                    {
                        // It is not the end of the file, nor of a record, so just accumulate this character.
                        sb.Append( (char)iChar );

                        // See if we have reached a record-termination.
                        string thisLine = sb.ToString();
                        if (thisLine.EndsWith( recordTermination ))
                        {
                            // We encountered a record-termination character.
                            // Gather the text accumulated thus far into a record and add that to our result.
                            result.Add( thisLine );
#if !PRE_4
                            sb.Clear();
#else
                            sb.Length = 0;
#endif
                        }
                    }
                }
                // We finished the file. Gather the last text into a final record, if any..
                string lastLine = sb.ToString();
                if (!StringLib.HasNothing( lastLine ))
                {
                    result.Add( lastLine );
                }
            }
            return result;
        }
        #endregion

        #region Rollover
        /// <summary>
        /// Given a filesystem-pathname that identifies a specific file, rename it on the drive
        /// if it already exists - such that a new one may be placed there.
        /// ALERT: Use named arguments for this method; you have overloads with 2-3 strings in a row - an invitation to errors.
        /// </summary>
        /// <param name="outputFilePathname">the full pathname of the file to 'rollover' to a new name, if necessary</param>
        /// <param name="archiveFileFolder">This dictates where to move the rollover files to (may be null, to indicate no distinct archive-folder).</param>
        /// <param name="maxBackups">This limits the number of rolled-over copies that may be produced. A default value of 100 is normally used.</param>
        /// <returns>true if a rollover (renaming) was in fact required</returns>
        /// <exception cref="ArgumentNullException">The argument supplied for <paramref name="outputFilePathname"/> must not be null.</exception>
        /// <exception cref="ArgumentException">the value provided for <paramref name="outputFilePathname"/> must be a syntactically-valid filename with no slashes</exception>
        /// <remarks>
        /// This creates backup copies ("rollovers") of the named file according to this pattern:
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
        public static bool Rollover( string outputFilePathname, string archiveFileFolder, int maxBackups )
        {
            if (outputFilePathname == null)
            {
                throw new ArgumentNullException( "outputFilePathname" );
            }
            string folder = FileStringLib.GetDirectoryPathNameFromFilePath( outputFilePathname );
            string filename = FileStringLib.GetFileNameFromFilePath( outputFilePathname );
#if PRE_4
            return Rollover( filename, folder, archiveFileFolder, maxBackups );
#else
            return Rollover( outputFilename: filename, outputFileFolder: folder, archiveFileFolder: archiveFileFolder, maxBackups: maxBackups );
#endif
        }

        /// <summary>
        /// Given a folder-path and filename that identifies a specific file, rename it on the drive
        /// if it already exists - such that a new one may be placed there.
        /// ALERT: Use named arguments for this method; you have 3 strings in a row - an invitation to errors.
        /// </summary>
        /// <param name="outputFilename">the name of the file to 'rollover' to a new name, if necessary</param>
        /// <param name="outputFileFolder">the filesystem-folder in which that file exists</param>
        /// <param name="archiveFileFolder">This dictates where to move the rollover files to (may be null, to indicate no distinct archive-folder).</param>
        /// <param name="maxBackups">This limits the number of rolled-over copies that may be produced. A default value of 100 is normally used.</param>
        /// <returns>true if a rollover (renaming) was in fact required</returns>
        /// <exception cref="ArgumentNullException">The arguments supplied for <paramref name="outputFilename"/> and <paramref name="outputFileFolder"/> must not be null.</exception>
        /// <exception cref="ArgumentException">the value provided for <paramref name="outputFilename"/> must be a syntactically-valid filename with no slashes</exception>
        /// <remarks>
        /// This creates backup copies ("rollovers") of the named file according to this pattern:
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
        public static bool Rollover( string outputFilename, string outputFileFolder, string archiveFileFolder, int maxBackups )
        {
            bool wasRolledOver = false;
            if (outputFilename == null)
            {
                throw new ArgumentNullException( "outputFilename" );
            }
            if (outputFileFolder == null)
            {
                throw new ArgumentNullException( "outputFileFolder" );
            }
            if (outputFilename.Contains( "/" ) || outputFilename.Contains( @"\" ))
            {
                throw new ArgumentException( @"This (""" + outputFilename + @""") should be just a filename, with no slashes.", "outputFilename" );
            }

            // Ensure the outputFileFolder is not just a drive-letter..
            if (!outputFileFolder.EndsWith( @"\" ))
            {
                outputFileFolder = outputFileFolder + @"\";
            }

            string pathnameOfOutputFile = Path.Combine( outputFileFolder, outputFilename );
            if (FileExists( pathnameOfOutputFile ))
            {
                // Derive the folder to use for the rolled-over files.
                string folderForArchivedFiles;
                if (archiveFileFolder == null)
                {
                    folderForArchivedFiles = outputFileFolder;
                }
                else
                {
                    folderForArchivedFiles = archiveFileFolder;
                    CreateDirectory( folderForArchivedFiles );
                }

                bool emptySlotFound = false;
                for (int i = 1; i <= maxBackups; i++)
                {
                    string filenameToCheck = FileStringLib.FilenameForRollover( outputFilename, i );
                    // Do the filename checks without regarding the extension, as that can vary
                    // (it may be .txt, or .zip).
                    //          string filenameWithoutExtension = filenameToCheck.PathnameWithoutExtension();
                    //          string outputFolder = LogManager.FileOutputFolder;
                    //          string pattern = filenameWithoutExtension + ".*";
                    //          if (!ZIOHelper.FilesExistThatMatchPattern(outputFolder, pattern))
                    string pathnameToCheck = Path.Combine( folderForArchivedFiles, filenameToCheck );
                    if (!FileExists( pathnameToCheck ))
                    {
                        emptySlotFound = true;
                        // Rename the existing pathName to filenameToCheck.
                        // But if it happens to be locked and this fails - then redirect it to a different file.
                        MoveFile( pathnameOfOutputFile, pathnameToCheck, s => Debug.WriteLine( s ), FilesystemLib.DefaultRetryTimeLimit );
                        wasRolledOver = true;
                        break;
                    }
                }
                //CBL
                // This is unfinished. Trying to account for ZIPped files!
                if (!emptySlotFound)
                {
                    // All N backups have been created already.
                    // Delete backup 1 (the oldest), and renumber the rest to be 1 less.
                    // eg
                    //   Log.txt     -> rename to Log(3).txt
                    //   Log(1).txt  -> delete
                    //   Log(2).txt  -> rename to Log(1).txt
                    //   Log(3).txt  -> rename to Log(2).txt
                    //
                    string outputFilePath = Path.Combine( folderForArchivedFiles, outputFilename );
                    string pathnameToDelete = FileStringLib.FilenameForRollover( outputFilePath, 1 );
                    DeleteFile( pathnameToDelete );
                    for (int i = 2; i <= maxBackups; i++)
                    {
                        string pathnameToRename = FileStringLib.FilenameForRollover( outputFilePath, i );
                        string pathnameToRenameTo = FileStringLib.FilenameForRollover( outputFilePath, i - 1 );
                        MoveFile( pathnameToRename, pathnameToRenameTo );
                    }
                    string filenameToRenameCurrentFileTo = FileStringLib.FilenameForRollover( outputFilePath, maxBackups );
                    MoveFile( pathnameOfOutputFile, filenameToRenameCurrentFileTo );
                    //CBL  I'm actually not sure what I'm doing with this return-value yet.
                    wasRolledOver = true;
                }
            }
            return wasRolledOver;
        }
        #endregion Rollover

        #region RunProgram
#if !NETFX_CORE
        /// <summary>
        /// Run the program that is named by the given string. If a full path isn't given then we try to guess at it.
        /// </summary>
        /// <param name="programName">the name of the program, it's executable file, or the full pathname</param>
        /// <param name="arguments">a string that denotes the complete set of command-line arguments to supply to the program upon invocation</param>
        /// <returns>0 if successful, -1 if otherwise</returns>
        public static int RunProgram( string programName, string arguments )
        {
            int r = 0;
            string sPath;
            if (FileStringLib.IsAbsolutePath( programName ))
            {
                sPath = programName;
            }
            else
            {
                sPath = FindProgram( programName );
            }
            if (String.IsNullOrEmpty( sPath ))
            {
                r = -1;
            }
            else
            {
                if (String.IsNullOrEmpty( arguments ))
                {
                    System.Diagnostics.Process process = System.Diagnostics.Process.Start( sPath );
                }
                else
                {
                    System.Diagnostics.Process process = System.Diagnostics.Process.Start( sPath, arguments );
                }
                //TODO  Clearly we'll want to use ProcessStartInfo later to do more sophisticated stuff like set user credentials, working directory, etc.
                //System.Diagnostics.ProcessStartInfo startinfo = process.StartInfo;
                //startinfo.
            }
            return r;
        }
#endif
        #endregion

        #region Seek
#if !NETFX_CORE
        /// <summary>
        /// This encapsulates the Win32 Seek function.
        /// </summary>
        /// <param name="handle">the IntPtr file handle of the file to apply the seek to</param>
        /// <param name="offset">how much to move the current position by</param>
        /// <param name="seekOrigin">dictates whether to seek relative to the start of file, current position, or end</param>
        /// <remarks>
        /// I added this. (James Hurst)
        /// This is from URL: http://www.pinvoke.net/default.aspx/kernel32.setfilepointer
        /// </remarks>
        /// <returns></returns>
        public static long Seek( IntPtr handle, long offset, SeekOrigin seekOrigin )
        {
            uint moveMethod = 0;

            switch (seekOrigin)
            {
                case SeekOrigin.Begin:
                    moveMethod = 0;
                    break;

                case SeekOrigin.Current:
                    moveMethod = 1;
                    break;

                case SeekOrigin.End:
                    moveMethod = 2;
                    break;
            }

            int lo = (int)(offset & 0xffffffff);
            int hi = (int)(offset >> 32);

            lo = Native.Win32.SetFilePointer( handle, lo, out hi, moveMethod );

            if (lo == -1)
            {
                var lastWin32Error = Marshal.GetLastWin32Error();
                if (lastWin32Error != 0)
                {
                    throw new Exception( "INVALID_SET_FILE_POINTER" );
                }
            }

            return (((long)hi << 32) | (uint)lo);
        }

        /// <summary>
        /// This encapsulates the Win32 Seek function.
        /// </summary>
        /// <param name="safeFileHandle">a SafeFileHandle that contains the IntPtr file handle of the file to apply the seek to</param>
        /// <param name="offset">how much to move the current position by</param>
        /// <param name="seekOrigin">dictates whether to seek relative to the start of file, current position, or end</param>
        /// <returns></returns>
        /// <remarks>
        /// I added this. (James Hurst)
        /// This simply calls the other Seek method, but with a SafeFileHandle.
        /// </remarks>
        public static long Seek( SafeFileHandle safeFileHandle, long offset, SeekOrigin seekOrigin )
        {
            if (!safeFileHandle.IsInvalid)
            {
                IntPtr handle = safeFileHandle.DangerousGetHandle();
                return Seek( handle, offset, seekOrigin );
            }
            else
            {
                return 0L;
            }
        }
#endif
        #endregion

        #region SetDirectoryAttributes
        /// <summary>
        /// Set the attributes of the filesystem-folder denoted by the given directoryPath.
        /// </summary>
        /// <param name="directoryPath">the directory to set the attributes of</param>
        /// <param name="attributes">the new values to set the directory's attributes to</param>
        public static void SetDirectoryAttributes( string directoryPath, FileAttributes attributes )
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException( paramName: directoryPath );
            }
            if (!DirectoryExists( directoryPath ))
            {
                throw new DirectoryNotFoundException( "The given directory ('" + directoryPath + "') was not found" );
            }
#if NETFX_CORE
            Directory.SetAttributes( path: folderPath, fileAttributes: attributes );
#else
            if (!Native.Win32.SetFileAttributes( directoryPath, attributes ))
            {
                // http://msdn.microsoft.com/en-us/library/ms681382(VS.85).aspx.
                //CBL How do we really want to convey this information?
                //var lastWin32Error = Marshal.GetLastWin32Error();
                //throw new Win32Exception(
                //    lastWin32Error,
                //    string.Format(
                //        "Error {0} setting file attribute of folder '{1}' to '{2}': {3}",
                //        lastWin32Error,
                //        directoryPath,
                //        attributes,
                //        FileStringLib.CheckAddDotEnd( new Win32Exception( lastWin32Error ).Message ) ) );
            }
#endif
        }
        #endregion

        #region SetDirectoryCreationTime
        /// <summary>
        /// Given a folder path, set the creation-time of that folder.
        /// </summary>
        /// <param name="directoryPath">the filesystem-path of the folder</param>
        /// <param name="toWhen">the DateTime to set the creation-time of this folder to</param>
        /// <exception cref="ArgumentNullException">The directory-path must not be null</exception>
        /// <exception cref="DirectoryNotFoundException">The directory must already exist</exception>
        public static void SetDirectoryCreationTime( string directoryPath,
                                                     DateTime toWhen )
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }
#if NETFX_CORE
            File.SetCreationTime( path: filePathname, creationTime: toWhen );
#else
            if (!DirectoryExists( directoryPath ))
            {
                throw new DirectoryNotFoundException( "The given directory ('" + directoryPath + "') was not found" );
            }

            using (var handle = FilesystemLib.CreateFileHandle( directoryPath,
                                                 Native.CreationDisposition.OpenExisting,
                                                 Native.FileAccess.GenericWrite,
                                                 Native.FileShare.None,
                                                 0 ))
            {
                var d = toWhen.ToFileTime();

                if (!Native.Win32.SetFileTime1( handle.DangerousGetHandle(), ref d, IntPtr.Zero, IntPtr.Zero ))
                {
                    var lastWin32Error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(
                        lastWin32Error,
                        string.Format(
                            "Error {0} setting folder creation time '{1}': {2}",
                            lastWin32Error,
                            directoryPath,
                            FileStringLib.CheckAddDotEnd( new Win32Exception( lastWin32Error ).Message ) ) );
                }
            }
#endif
        }
        #endregion

        #region SetDirectoryHidden
        /// <summary>
        /// Set the "Hidden"-attribute of the given directory.
        /// </summary>
        /// <param name="directoryPath">the filesystem-path of the directory to set the attribute of</param>
        /// <param name="isToBeHidden">true to turn the Hidden attribute on, false to turn it off</param>
        /// <returns>true if the directory was not already Hidden and had to be changed</returns>
        /// <exception cref="ArgumentNullException">The argument supplied for <paramref name="directoryPath"/> must not be null.</exception>
        /// <remarks>
        /// The reason for the return-value is to provide a way for unit-tests to tell whether this had to actually change the directory-attributes,
        /// since if it's already the desired state of Hidden then we want to ensure no attempt is made to change it.
        /// </remarks>
        public static bool SetDirectoryHidden( string directoryPath, bool isToBeHidden )
        {
            //UT: done
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }
            if (!DirectoryExists( directoryPath ))
            {
                throw new DirectoryNotFoundException( "The given directory ('" + directoryPath + "') was not found" );
            }
            bool isChanged = false;
            FileAttributes originalAttributes = GetDirectoryAttributes( directoryPath );
            FileAttributes newAttributes;
            // If originally is NOT hidden..
            if ((originalAttributes & FileAttributes.Hidden) == 0)
            {
                // and we are setting it to hidden
                if (isToBeHidden)
                {
                    newAttributes = originalAttributes | FileAttributes.Hidden;
                    SetDirectoryAttributes( directoryPath, newAttributes );
                    isChanged = true;
                }
            }
            else // was originally hidden
            {
                if (!isToBeHidden)
                {
                    newAttributes = originalAttributes & ~FileAttributes.Hidden;
                    SetDirectoryAttributes( directoryPath, newAttributes );
                    isChanged = true;
                }
            }
            return isChanged;
        }
        #endregion

        #region SetDirectoryReadOnly
        /// <summary>
        /// Set the "ReadOnly"-attribute of the given directory.
        /// </summary>
        /// <param name="directoryPath">the filesystem-path of the directory to set the attribute of</param>
        /// <param name="isToBeReadOnly">true to turn the ReadOnly attribute on, false to turn it off</param>
        /// <returns>true if the directory was not already ReadOnly and had to be changed</returns>
        /// <exception cref="ArgumentNullException">The argument supplied for <paramref name="directoryPath"/> must not be null.</exception>
        /// <remarks>
        /// The reason for the return-value is to provide a way for unit-tests to tell whether this had to actually change the directory-attributes,
        /// since if it's already the desired state of ReadOnly then we want to ensure no attempt is made to change it.
        ///
        /// Test-status: Needed!
        /// </remarks>
        public static bool SetDirectoryReadOnly( string directoryPath, bool isToBeReadOnly )
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }
            if (!DirectoryExists( directoryPath ))
            {
                throw new DirectoryNotFoundException( "The given directory ('" + directoryPath + "') was not found" );
            }
            bool isChanged = false;
            FileAttributes originalAttributes = GetDirectoryAttributes( directoryPath );
            FileAttributes newAttributes;
            // If originally is NOT ReadOnly..
            if ((originalAttributes & FileAttributes.ReadOnly) == 0)
            {
                // and we are setting it to ReadOnly
                if (isToBeReadOnly)
                {
                    newAttributes = originalAttributes | FileAttributes.ReadOnly;
                    SetDirectoryAttributes( directoryPath, newAttributes );
                    isChanged = true;
                }
            }
            else // was originally ReadOnly
            {
                if (!isToBeReadOnly)
                {
                    newAttributes = originalAttributes & ~FileAttributes.ReadOnly;
                    SetDirectoryAttributes( directoryPath, newAttributes );
                    isChanged = true;
                }
            }
            return isChanged;
        }
        #endregion

        #region SetFileAttributes
        /// <summary>
        /// Set the attributes of the filesystem-file denoted by the given filePath.
        /// </summary>
        /// <param name="filePathname">the file to set the attributes of</param>
        /// <param name="attributes">the new values to set the file's attributes to</param>
        public static void SetFileAttributes( string filePathname, FileAttributes attributes )
        {
            //UT: done
            if (filePathname == null)
            {
                throw new ArgumentNullException( "filePathname" );
            }
            if (!FileExists( filePathname ))
            {
                throw new FileNotFoundException( message: "That filePathname does not seem to be present.", fileName: filePathname );
            }
#if NETFX_CORE
            File.SetAttributes( path: filePathname, fileAttributes: attributes );
#else
            if (!Native.Win32.SetFileAttributes( filePathname, attributes ))
            {
                // http://msdn.microsoft.com/en-us/library/ms681382(VS.85).aspx.

                var lastWin32Error = Marshal.GetLastWin32Error();
                if (lastWin32Error != Win32.ERROR_SUCCESS)
                {
                    throw new Win32Exception( lastWin32Error,
                                              "Error " + lastWin32Error + " setting file attribute of file \"" + filePathname + "\" to " + attributes + ": " +
                                              FileStringLib.CheckAddDotEnd( new Win32Exception( lastWin32Error ).Message ) );
                }
            }
#endif
        }
        #endregion

        #region SetFileCreationTime
        /// <summary>
        /// Given a pathname, set the creation-time of that file or folder.
        /// </summary>
        /// <param name="filePathname">the filesystem-path of the file or folder</param>
        /// <param name="toWhen">the DateTime to set the creation-time of this file to</param>
        /// <exception cref="ArgumentNullException">The pathname must not be null</exception>
        /// <exception cref="FileNotFoundException">The pathname must already exist</exception>
        public static void SetFileCreationTime( string filePathname,
                                                DateTime toWhen )
        {
            if (filePathname == null)
            {
                throw new ArgumentNullException( "filePathname" );
            }
#if NETFX_CORE
            File.SetCreationTime( path: filePathname, creationTime: toWhen );
#else
            if (!FileExists( filePathname ))
            {
                throw new FileNotFoundException( "The given file pathname was not found.", filePathname );
            }

            using (var handle = FilesystemLib.CreateFileHandle( filePathname,
                                                 Native.CreationDisposition.OpenExisting,
                                                 Native.FileAccess.GenericWrite,
                                                 Native.FileShare.None,
                                                 0 ))
            {
                var d = toWhen.ToFileTime();

                if (!Native.Win32.SetFileTime1( handle.DangerousGetHandle(), ref d, IntPtr.Zero, IntPtr.Zero ))
                {
                    var lastWin32Error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(
                        lastWin32Error,
                        string.Format(
                            "Error {0} setting file creation time '{1}': {2}",
                            lastWin32Error,
                            filePathname,
                            FileStringLib.CheckAddDotEnd( new Win32Exception( lastWin32Error ).Message ) ) );
                }
            }
#endif
        }
        #endregion

        #region SetFileLastAccessTime
        /// <summary>
        /// Given a pathname, set the last-access-time of that file or folder.
        /// </summary>
        /// <param name="filePathname">the filesystem-path of the file or folder</param>
        /// <param name="toWhen">the DateTime to set the last-access-time of this file to</param>
        /// <exception cref="ArgumentNullException">The pathname must not be null</exception>
        /// <exception cref="FileNotFoundException">The pathname must already exist</exception>
        public static void SetFileLastAccessTime( string filePathname,
                                                  DateTime toWhen )
        {
            if (filePathname == null)
            {
                throw new ArgumentNullException( "filePathname" );
            }
#if NETFX_CORE
            File.SetLastAccessTime( path: filePathname, lastAccessTime: toWhen );
#else
            if (!FileExists( filePathname ))
            {
                throw new FileNotFoundException( "The given file pathname was not found.", filePathname );
            }

            using (var handle = CreateFileHandle( filePathname,
                                                 Native.CreationDisposition.OpenExisting,
                                                 Native.FileAccess.GenericWrite,
                                                 Native.FileShare.None,
                                                 0 ))
            {
                var d = toWhen.ToFileTime();

                if (!Native.Win32.SetFileTime2( handle.DangerousGetHandle(), IntPtr.Zero, ref d, IntPtr.Zero ))
                {
                    var lastWin32Error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(
                        lastWin32Error,
                        string.Format(
                            "Error {0} setting file last access time '{1}': {2}",
                            lastWin32Error,
                            filePathname,
                            FileStringLib.CheckAddDotEnd( new Win32Exception( lastWin32Error ).Message ) ) );
                }
            }
#endif
        }
        #endregion

        #region SetFileLastWriteTime
        /// <summary>
        /// Given a pathname, set the last-write-time of that file or folder.
        /// </summary>
        /// <param name="filePathname">the filesystem-path of the file or folder</param>
        /// <param name="toWhen">the DateTime to set the last-write-time of this file to</param>
        /// <exception cref="ArgumentNullException">The pathname must not be null</exception>
        /// <exception cref="FileNotFoundException">The pathname must already exist</exception>
        public static void SetFileLastWriteTime( string filePathname,
                                                 DateTime toWhen )
        {
            if (filePathname == null)
            {
                throw new ArgumentNullException( "filePathname" );
            }

#if NETFX_CORE
            File.SetLastWriteTime( path: filePathname, lastWriteTime: toWhen );
#else
            if (!FileExists( filePathname ))
            {
                throw new FileNotFoundException( "The given file pathname was not found.", filePathname );
            }

            // Create a new value of the file-path that would be valid for long pathnames.
            string longCapableFilePath = FileStringLib.CheckAddLongPathPrefix( filePathname );

            using (var handle = CreateFileHandle( longCapableFilePath,
                                                  CreationDisposition.OpenExisting,
                                                  Native.FileAccess.GenericWrite,
                                                  Native.FileShare.Read,
                                                  0 ))
            {
                var d = toWhen.ToFileTime();

                if (!Native.Win32.SetFileTime3( handle.DangerousGetHandle(), IntPtr.Zero, IntPtr.Zero, ref d ))
                {
                    var lastWin32Error = Marshal.GetLastWin32Error();
                    throw new Win32Exception(
                        lastWin32Error,
                        string.Format(
                            "Error {0} setting file last write time '{1}': {2}",
                            lastWin32Error,
                            filePathname,
                            FileStringLib.CheckAddDotEnd( new Win32Exception( lastWin32Error ).Message ) ) );
                }
            }
#endif
        }
        #endregion

        #region SetFileHidden
        /// <summary>
        /// Set the "Hidden"-attribute of the given file.
        /// </summary>
        /// <param name="filePathname">the filesystem-pathname of the file to set the attribute of</param>
        /// <param name="isToBeHidden">true to turn the Hidden attribute on, false to turn it off</param>
        /// <returns>true if the file was not already Read-Only and had to be changed</returns>
        /// <exception cref="ArgumentNullException">The argument supplied for <paramref name="filePathname"/> must not be null.</exception>
        /// <remarks>
        /// The reason for the return-value is to provide a way for unit-tests to tell whether this had to actually change the file-attributes,
        /// since if it's already the desired state of Read-Only then we want to ensure no attempt is made to change it.
        ///
        /// Test-status: complete.
        /// </remarks>
        public static bool SetFileHidden( string filePathname, bool isToBeHidden )
        {
            if (filePathname == null)
            {
                throw new ArgumentNullException( "filePathname" );
            }
            bool isChanged = false;
            FileAttributes originalAttributes = GetFileAttributes( filePathname );
            FileAttributes newAttributes;
            // If originally is NOT hidden..
            if ((originalAttributes & FileAttributes.Hidden) == 0)
            {
                // and we are setting it to hidden
                if (isToBeHidden)
                {
                    newAttributes = originalAttributes | FileAttributes.Hidden;
                    SetFileAttributes( filePathname, newAttributes );
                    isChanged = true;
                }
            }
            else // was originally hidden
            {
                if (!isToBeHidden)
                {
                    newAttributes = originalAttributes & ~FileAttributes.Hidden;
                    SetFileAttributes( filePathname, newAttributes );
                    isChanged = true;
                }
            }
            return isChanged;
        }
        #endregion

        #region SetFileReadOnly
        /// <summary>
        /// Set the readonly-attribute of the given file.
        /// </summary>
        /// <param name="filePathname">the filesystem-pathname of the file to set the attribute of</param>
        /// <param name="isToBeReadonly">true to turn the readonly-attribute on, false to turn it off</param>
        /// <returns>true if the file was not already Read-Only and had to be changed</returns>
        /// <exception cref="ArgumentNullException">The argument supplied for <paramref name="filePathname"/> must not be null.</exception>
        /// <remarks>
        /// The reason for the return-value is to provide a way for unit-tests to tell whether this had to actually change the file-attributes,
        /// since if it's already the desired state of Read-Only then we want to ensure no attempt is made to change it.
        ///
        /// Test-status: complete.
        /// </remarks>
        public static bool SetFileReadonly( string filePathname, bool isToBeReadonly )
        {
            if (filePathname == null)
            {
                throw new ArgumentNullException( "filePathname" );
            }
            bool isChanged = false;
            FileAttributes originalAttributes = GetFileAttributes( filePathname );
            FileAttributes newAttributes;
            // If originally is NOT read-only..
            if ((originalAttributes & FileAttributes.ReadOnly) == 0)
            {
                // and we are setting it to read-only
                if (isToBeReadonly)
                {
                    newAttributes = originalAttributes | FileAttributes.ReadOnly;
                    SetFileAttributes( filePathname, newAttributes );
                    isChanged = true;
                }
            }
            else // was originally read-only
            {
                if (!isToBeReadonly)
                {
                    newAttributes = originalAttributes & ~FileAttributes.ReadOnly;
                    SetFileAttributes( filePathname, newAttributes );
                    isChanged = true;
                }
            }
            return isChanged;
        }
        #endregion

        #region WriteAllBytes
        /// <summary>
        /// Write all of the given byte-array content to a file located at the given path.
        /// </summary>
        /// <param name="path">the filesystem-path to write to</param>
        /// <param name="contents">the byte-array to write to it</param>
        public static void WriteAllBytes( string path,
                                         byte[] contents )
        {
            using (var fs =
                new FileStream(
                    CreateFileHandle( path,
                                     Native.CreationDisposition.CreateAlways,
                                     Native.FileAccess.GenericWrite,
                                     Native.FileShare.Read,
                                     0 ),
                    System.IO.FileAccess.Write ))
            {
                fs.Write( contents, 0, contents.Length );
            }
        }
        #endregion

        /// <summary>
        /// Just tinkering.
        /// </summary>
        /// <param name="pathname"></param>
        /// <param name="textToWrite"></param>
        /// <param name="isToFlushToDisk"></param>
        public static void AppendText( string pathname,
                                       string textToWrite,
                                       bool isToFlushToDisk )
        {
            if (pathname == null)
            {
                throw new ArgumentNullException( paramName: nameof( pathname ) );
            }
            // If the directory of the target pathname does not exist, create it first.
            string folder = FileStringLib.GetDirectoryOfPath( pathname );
            if (!DirectoryExists( folder ))
            {
                CreateDirectory( folder );
            }
            //CBL ???  We should already know whether we are creating anew or appending, so we should not need to test for whether Directory.Exists,
            // and for creating new - we should not need to seek to the end.
        }

        #region WriteTextToFile( pathname, contents, isToAppend, isToWriteThrough )
        /// <summary>
        /// Write the given text to the file denoted by the given pathname.
        /// This creates the file if it does not exist, and if it does exist - either appends to it, or truncates and writes to it.
        /// </summary>
        /// <param name="pathname">the filesystem-pathname of the file to write to</param>
        /// <param name="contents">the text string to write to it</param>
        /// <param name="isToAppend">if the file already exists then this signals to append the given text to the existing content</param>
        /// <param name="isToWriteThrough">set this to true if you want this to write to disk without buffering</param>
        /// <remarks>
        /// It is not an error-condition to call this with a pathname that represents a file that exists, or does not exist -
        /// regardless of whether it is desired to append or not.
        ///
        /// If isToAppend is false, then this truncates and recreates an existing file, or creates the file if it does not already exist.
        /// If isToAppend is true, then this appends the text to that file, and if the file does not exist - it creates one and writes the text to ti.
        /// 
        /// Note: This is the method that LogNut calls to write file-output.
        /// </remarks>
        public static void WriteTextToFile( string pathname,
                                            string contents,
                                            bool isToAppend,
                                            bool isToWriteThrough )
        {
            // This is a slight variation from WriteAllText, intended for adding the additional OS-cache-flush option.

            if (pathname == null)
            {
                throw new ArgumentNullException( "pathname" );
            }

            CreationDisposition creationDisposition;
            if (isToAppend)
            {
                creationDisposition = CreationDisposition.OpenAlways;
            }
            else // not appending.
            {
                creationDisposition = CreationDisposition.CreateAlways;
            }
            // CreationDisposition:
            //   CreationDisposition.CreateAlways;     // Creates a new file always. If it already exists, sets last-error to ERROR_ALREADY_EXISTS (183).
            //   CreationDisposition.New;              // Creates new file if it does not exist. If it does already exist - ERROR_FILE_EXISTS (80).
            //   CreationDisposition.OpenAlways;       // Opens the file. If it already exists, it succeeds and sets last-error to ERROR_ALREADY_EXISTS (183)
            //   CreationDisposition.OpenExisting;     // Open if it exists, error if it does not exist
            //   CreationDisposition.TruncateExisting; // Opens the file and truncates it so it's size is zero bytes, if it exists. If it does not exist - the function fails and last-error is set to ERROR_FILE_NOT_FOUND (2).
            // See https://docs.microsoft.com/en-us/windows/desktop/fileio/creating-and-opening-files

            // If the directory of the target pathname does not exist, create it first.
            string directory = FileStringLib.GetDirectoryOfPath( pathname );
            if (!DirectoryExists( directory ))
            {
                CreateDirectory( directory );
            }

            //CBL Should I change this to default?
            var encoding = new UTF8Encoding( false, true );

            // See https://stackoverflow.com/questions/4921498/whats-the-difference-between-filestream-flush-and-filestream-flushtrue

            NativeFileAttributes nativeFileAttributes;
            if (isToWriteThrough)
            {
                nativeFileAttributes = NativeFileAttributes.SequentialScan | NativeFileAttributes.Write_Through;
            }
            else
            {
                nativeFileAttributes = NativeFileAttributes.SequentialScan;
            }

            SafeFileHandle safeFileHandle = null;
            try
            {
                safeFileHandle = CreateFileHandle( pathname,
                                                   creationDisposition,
                                                   Native.FileAccess.GenericWrite,
                                                   Native.FileShare.None,
                                                   nativeFileAttributes );
                if (!safeFileHandle.IsInvalid)
                {
                    // Seek to the end of the file, if we want to append to it..
                    if (isToAppend)
                    {
                        Seek( safeFileHandle, 0, SeekOrigin.End );
                    }
                    using (var fs = new FileStream( safeFileHandle, System.IO.FileAccess.Write ))
                    {
                        using (var streamWriter = new StreamWriter( fs, encoding ))
                        {
                            streamWriter.Write( contents );
                            if (isToWriteThrough)
                            {
#if !PRE_4
                                fs.Flush( true );
#else
                                fs.Flush();
#endif
                            }
                        }
                    }
                }
            }
            finally
            {
                if (safeFileHandle != null)
                {
                    if (!safeFileHandle.IsInvalid && !safeFileHandle.IsClosed)
                    {
#if PRE_5
                        // Apparently in .NET Framework 4.0 or earlier, FileStream.Flush(true) does not necessarily flush the OS cache-butters to disk.
                        // Thus this is intended to ensure the data does actually arrive upon disk before this method exits.
                        if (isToWriteThrough)
                        {
                            FlushOsFileBuffersToDisk( safeFileHandle, pathname );
                        }
#endif
                        safeFileHandle.Close();
                    }
                    safeFileHandle.Dispose();
                }
            }
        }
        #endregion

        #region FlushOsFileBuffersToDisk
        /// <summary>
        /// Call the Win32 function FlushFileBuffers to ensure the Operating System file-cache is written to disk.
        /// This should only be needed for .NET Framework 4.0 or earlier.
        /// </summary>
        /// <param name="safeFileHandle"></param>
        /// <param name="pathname"></param>
        public static void FlushOsFileBuffersToDisk( SafeFileHandle safeFileHandle, string pathname )
        {
            // This, is needed for versions of .NET Framework 4.0 or earlier.
            // There is some ambiguity as to whether this issue was fixed in .NET 4.0, thus this method if provided.

            // See  https://stackoverflow.com/questions/36580446/handle-intptr-obsolete-kernel32-dll-safefilehandle-to-intptr
            //      https://stackoverflow.com/questions/383324/how-to-ensure-all-data-has-been-physically-written-to-disk

#pragma warning disable 618, 612 // disable stream.Handle deprecation warning.
            if (!FlushFileBuffers( safeFileHandle.DangerousGetHandle() ))   // Flush OS file cache to disk.
#pragma warning restore 618, 612
            {
                Int32 err = Marshal.GetLastWin32Error();
                throw new Win32Exception( err, "Win32 FlushFileBuffers returned error for " + pathname );
            }
        }

        // This is the interop definition for the Win32 function.
        [DllImport( "kernel32", SetLastError = true )]
        private static extern bool FlushFileBuffers( IntPtr handle );

        #endregion

        #region WriteAllText( pathname, contents, fileMode, isToWriteThrough )
        /// <summary>
        /// Write the given text to the file denoted by the given pathname.
        /// If you specify Append for the fileMode, it creates the file if it does not exist.
        /// </summary>
        /// <param name="pathname">the filesystem-pathname of the file to write to</param>
        /// <param name="contents">the text string to write to it</param>
        /// <param name="fileMode">the mode for writing to the file</param>
        /// <param name="isToWriteThrough">set this to true if you want this to write to disk without buffering</param>
        /// <remarks>
        /// Under the Universal Windows Platform (whenever NETFX_CORE is defined),
        /// <paramref name="fileMode"/> has only two relevant values:
        ///   <c>FileMode.Append</c> - append the given text to the file,
        ///   <c>FileMode.CreateNew</c> (or any other value) - overwrite the existing contents of the file.
        ///
        /// Note: This was the method that LogNut calls to write file-output.
        /// </remarks>
        public static void WriteAllText( string pathname,
                                         string contents,
                                         FileMode fileMode,
                                         bool isToWriteThrough )
        {
#if NETFX_CORE
            //CBL  Needs implementing, obviously.
            if (fileMode == FileMode.Append)
            {
                File.AppendAllText( path: pathname, contents: contents );
            }
            else
            {
                File.WriteAllText( path: pathname, contents: contents );
            }
#else
            var creationDisposition = CreationDisposition.CreateAlways;
            switch (fileMode)
            {
                case FileMode.Append:
                    creationDisposition = CreationDisposition.OpenExisting;
                    break;
                case FileMode.Create:
                    // Create a new file if it doesn't already exist; truncate the file if it does exist.
                    break;
                case FileMode.CreateNew:
                    break;
                case FileMode.Truncate:
                    creationDisposition = CreationDisposition.TruncateExisting;
                    break;
            }

            // If the directory of the target pathname does not exist, create it first.
            string folder = FileStringLib.GetDirectoryOfPath( pathname );
            if (!DirectoryExists( folder ))
            {
                CreateDirectory( folder );
            }

            //CBL Should I change this to default?
            var encoding = new UTF8Encoding( false, true );

            // See https://stackoverflow.com/questions/4921498/whats-the-difference-between-filestream-flush-and-filestream-flushtrue

            NativeFileAttributes nativeFileAttributes;
            if (isToWriteThrough)
            {
                nativeFileAttributes = NativeFileAttributes.SequentialScan | NativeFileAttributes.Write_Through;
            }
            else
            {
                nativeFileAttributes = NativeFileAttributes.SequentialScan;
            }

            SafeFileHandle safeFileHandle = null;
            try
            {
                safeFileHandle = CreateFileHandle( pathname,
                                                   creationDisposition,
                                                   Native.FileAccess.GenericWrite,
                                                   Native.FileShare.None,
                                                   nativeFileAttributes );
                if (!safeFileHandle.IsInvalid)
                {
                    // Seek to the end of the file, if we want to append to it..
                    if (fileMode == FileMode.Append)
                    {
                        Seek( safeFileHandle, 0, SeekOrigin.End );
                    }
                    using (var fs = new FileStream( safeFileHandle, System.IO.FileAccess.Write ))
                    {
                        using (var streamWriter = new StreamWriter( fs, encoding ))
                        {
                            streamWriter.Write( contents );
                            if (isToWriteThrough)
                            {
#if !PRE_4
                                fs.Flush( true );
#else
                                fs.Flush();
#endif
                            }
                        }
                    }
                }
            }
            finally
            {
                if (safeFileHandle != null)
                {
                    if (!safeFileHandle.IsInvalid && !safeFileHandle.IsClosed)
                    {
                        safeFileHandle.Close();
                    }
                    safeFileHandle.Dispose();
                }
            }
#endif
        }
        #endregion

        #region WriteText
        /// <summary>
        /// Write the given contents to the given file pathname, creating the file if it does not exist.
        /// </summary>
        /// <param name="path">the filesystem pathname of the file to write to</param>
        /// <param name="contents">the text to write to the file</param>
        /// <remarks>
        /// This method DOES permit you to create and write to a file, and then immediately
        /// do something else to it without getting an Access Denied error
        /// such as File.Move (but not FilesystemLib.SetFileLastWriteTime).  CBL
        /// </remarks>
        public static void WriteText( string path, string contents )
        {
            //CBL Am I doing this optimally? Does this file close immediately such that a subsequent call can 
            // move or delete this file?
            //CBL  Why have this in addition to WriteAllText ?
            //var encoding = new UTF8Encoding( false, true );
            var encoding = Encoding.Default;
            using (var fileStream = new FileStream( CreateFileHandle( path,
                                                                      Native.CreationDisposition.CreateAlways,
                                                                      Native.FileAccess.GenericWrite,
                                                                      Native.FileShare.None,
                                                                      0 ),
                                                    System.IO.FileAccess.Write ))
            {
                using (var streamWriter = new StreamWriter( fileStream, encoding ))
                {
                    streamWriter.AutoFlush = true;
                    streamWriter.Write( contents );
                }
            }
        }
        #endregion

        #region internal implementation

        #region CreateFileHandle
#if !NETFX_CORE
        /// <summary>
        /// Pass the file handle to the <see cref="System.IO.FileStream"/> constructor. 
        /// The <see cref="System.IO.FileStream"/> will close the handle.
        /// </summary>
        public static SafeFileHandle CreateFileHandle( string filePath,
                                                      CreationDisposition creationDisposition,
                                                      Native.FileAccess fileAccess,
                                                      Native.FileShare fileShare,
                                                      NativeFileAttributes flagsAndAttributes )
        {
            //CBL  This had been internal. Made it public to clear an error with unit-testing. Why was it working before?

            filePath = FileStringLib.CheckAddLongPathPrefix( filePath );

            // Create a file with generic write access
            var fileHandle =
                Native.Win32.CreateFile(
                    filePath,
                    fileAccess,
                    fileShare,
                    IntPtr.Zero,
                    creationDisposition,
                    flagsAndAttributes,
                    IntPtr.Zero );

            // Check for errors.
            var lastWin32Error = Marshal.GetLastWin32Error();
            if (fileHandle.IsInvalid)
            {
                throw new Win32Exception(
                    lastWin32Error,
                    string.Format(
                        "Error {0} creating file handle for file path '{1}': {2}",
                        lastWin32Error,
                        filePath,
                        FileStringLib.CheckAddDotEnd( new Win32Exception( lastWin32Error ).Message ) ) );
            }

            // Pass the file handle to FileStream. FileStream will close the handle.
            return fileHandle;
        }
#endif
        #endregion

        #region RecurseDirectoryCounting
        /// <summary>
        /// Recursively go through the given directory, counting the number of folders and files
        /// and return the total size of the bits thusly found, in units of bytes.
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="level"></param>
        /// <param name="numberOfFiles"></param>
        /// <param name="numberOfFolders"></param>
        /// <returns></returns>
        private static long RecurseDirectoryCounting( string directory, int level, out int numberOfFiles, out int numberOfFolders )
        {
            //IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);
            long size = 0;
            numberOfFiles = 0;
            numberOfFolders = 0;
            Win32.WIN32_FIND_DATA findData;

            // please note that the following line won't work if you try this on a network folder, like \\Machine\C$
            // simply remove the \\?\ part in this case or use \\?\UNC\ prefix
            using (Win32.SafeFindHandle findHandle = Kernel32UsingSafeHandle.FindFirstFile( @"\\?\" + directory + @"\*", out findData ))
            {
                if (!findHandle.IsInvalid)
                {
                    do
                    {
                        if ((findData.dwFileAttributes & FileAttributes.Directory) != 0)
                        {
                            if (findData.cFileName != "." && findData.cFileName != "..")
                            {
                                numberOfFolders++;

                                string subdirectory = directory + (directory.EndsWith( @"\" ) ? "" : @"\") +
                                    findData.cFileName;
                                if (level != 0)  // allows -1 to do complete search.
                                {
                                    int numberOfFilesInSubfolders, numberOfSubfolders;
                                    size += RecurseDirectoryCounting( subdirectory, level - 1, out numberOfFilesInSubfolders, out numberOfSubfolders );

                                    numberOfFolders += numberOfSubfolders;
                                    numberOfFiles += numberOfFilesInSubfolders;
                                }
                            }
                        }
                        else
                        {
                            numberOfFiles++;
                            size += (long)findData.nFileSizeLow + (long)findData.nFileSizeHigh * 4294967296;
                        }
                    }
                    while (Kernel32UsingSafeHandle.FindNextFile( findHandle, out findData ));
                }
            }
            return size;
        }
        #endregion RecurseDirectoryCounting

        #endregion internal implementation
    }
}
