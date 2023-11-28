#if PRE_4
#define PRE_5
#endif
#if PRE_5
#define PRE_6
#endif
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// class StringFileLib exists to contain filesystem-related methods that only involve strings,
    /// and no actual manipulation of the filesystem.
    /// </summary>
    public static class FileStringLib
    {
        /// <summary>
        /// "System Volume Information" - a special folder that Windows places on drives
        /// </summary>
        public const string NameOfSystemVolInforDirectory = "System Volume Information";

        #region AsDigitalMemorySize
        /// <summary>
        /// Return a string denoting the numeric value as a metric in Bytes, KB (Kilobytes), MB (Megabytes), etc.
        /// </summary>
        /// <param name="bytes">the number to express</param>
        /// <param name="includeUnits">where to include the units of bytes</param>
        /// <returns>a string that represents the given size</returns>
        /// <remarks>
        /// The parameter <c>includeUnits</c> only has an effect when indicating a value in bytes.
        /// If the size is such that it is expressed in Kilobytes, Megabytes and so on - then a suffix is always appended.
        /// </remarks>
        public static string AsDigitalMemorySize( this ulong bytes, bool includeUnits )
        {
            // Note: Found some examples at http://sharpertutorials.com/pretty-format-bytes-kb-mb-gb/
            //                         also http://stackoverflow.com/questions/128618/c-file-size-format-provider
            // If I wanted greater capacity, could use “YB”, “ZB”, “EB”, “PB”, “TB”, “GB”, “MB”, “KB”, “Bytes”.
            // 2 to the 20th power is 1,048,576
            if (bytes < 1048576)
            {
                int number = (int)bytes;
                if (includeUnits)
                {
                    if (bytes == 1)
                    {
                        return "1 byte";
                    }
#if PRE_6
                    return String.Format( "{0:n0} bytes", number );
#else
                    return $"{number:n0} bytes";
#endif
                }
                else
                {
#if PRE_6
                    return String.Format( "{0:n0}", number );
#else
                    return $"{number:n0}";
#endif
                }
            }
            else if (bytes == 1048576)
            {
                return "1 MB";
            }
            //var units = new string[] {"B", "KB", "MB", "GB", "TB"};
            //long n = Math.Max(bytes, 0);
            //long pow = (long)Math.Floor(Math.Exp(n)/Math.Exp(1024));
            //pow = Math.Min(pow, units.Length - 1);


            const int scale = 1024;
            var orders = new string[] { "TB", "GB", "MB", "KB", "bytes" };
            ulong max = (ulong)Math.Pow( scale, orders.Length - 1 );

            foreach (var order in orders)
            {
                if (bytes > max)
                    return String.Format( "{0:##.##} {1}", Decimal.Divide( bytes, max ), order );

                max /= scale;
            }
            return "0 Bytes";
        }
        #endregion

        #region ChangeExtension
        /// <summary>
        /// Return the given filesystem-path with the filename extension changed to the given value
        /// (no change is made to the filesystem itself).
        /// </summary>
        /// <param name="path">the filesystem-path of what to change the extension of</param>
        /// <param name="extension">the new value to set the extension to</param>
        /// <returns>the filesystem-path with the changed extension</returns>
        /// <exception cref="ArgumentNullException">The provided path must not be null.</exception>
        /// <remarks>
        /// The <paramref name="extension"/> may include a leading period, or not.
        /// 
        /// If <paramref name="extension"/> is null or empty, then the existing path is returned with any path-extension removed, if found.
        /// </remarks>
        public static string ChangeExtension( string path, string extension )
        {
            if (path == null)
            {
                throw new ArgumentNullException( "path" );
            }
            string text = path;
            int num = path.Length;

            while (--num >= 0)
            {
                char c = path[num];
                if (c == '.')
                {
                    text = path.Substring( 0, num );
                    break;
                }
                if (c == Path.DirectorySeparatorChar || c == Path.AltDirectorySeparatorChar || c == Path.VolumeSeparatorChar)
                {
                    break;
                }
            }
            if (StringLib.HasSomething( extension ) && path.Length != 0)
            {
                if (extension.Length == 0 || extension[0] != '.')
                {
                    text += ".";
                }
                text += extension;
            }
            return text;
        }
        #endregion

        #region CheckPath
        /// <summary>
        /// Examine the given filesystem-path for syntactical correctness and throw an exception if it is invalid.
        /// Return the path, prefixed with the long-path prefix "\\?\" if it's length warrants it.
        /// </summary>
        /// <param name="path">the filesystem-path to validate</param>
        /// <param name="argumentName">the name of the parameter (in the calling code)</param>
        /// <returns>the path, possibly with a long-path prefix</returns>
        public static string CheckPath( string path, string argumentName )
        {
            if (path == null)
            {
                throw new ArgumentNullException( argumentName );
            }
            // Check that the path is valid..
            string reason;
            bool isPathTooLong;
            if (!GetWhetherPathIsSyntacticallyValid( path, out reason, out isPathTooLong ))
            {
                if (isPathTooLong)
                {
                    throw new PathTooLongException( argumentName + ": " + reason );
                }
                throw new ArgumentException( reason, argumentName );
            }
            // If it is excessively long then change it into a special format.
#if NO_LONGPATH
            return path;
#else
            return CheckAddLongPathPrefix( path );
#endif
        }
        #endregion

        #region CombineDriveAndFolder
        /// <summary>
        /// Given strings denoting a filesystem drive, and a filesystem folder,
        /// return those combined into a valid path.
        /// </summary>
        /// <param name="drive">a drive-specification, which may be of the form "C", "C:", "C:\" or "\\C"</param>
        /// <param name="folder">a folder-specification, which may or may not start with a leading back-slash</param>
        /// <returns>a valid filesystem-path with the given drive and folder</returns>
        /// <exception cref="ArgumentNullException">The drive and folder arguments must not be null</exception>
        public static string CombineDriveAndFolder( string drive, string folder )
        {
            // Test Cases:
            // "C", "folder1"
            // "C:", "folder1"
            // "C", "\folder1"
            // "C:", "\folder1"
            // "C", "/folder1"
            // "C:", "/folder1"
            // "C:\", "\folder1"
            // "\\C", "\folder1"
            // "\\C\", "folder1"
            // "C", "C:\folder" (remove the redundant drive)

            if (drive == null)
            {
                throw new ArgumentNullException( "drive" );
            }
            if (folder == null)
            {
                throw new ArgumentNullException( "folder" );
            }

            string driveSpec = drive;
            if (drive.Length == 1 && drive[0].IsEnglishAlphabetLetter())
            {
                driveSpec = drive.Substring( 0, 1 ) + ":";
            }
            else if (drive.Length == 3 && drive[0].IsEnglishAlphabetLetter() && drive[1] == ':' && drive[2] == '\\')
            {
                driveSpec = drive.Substring( 0, 2 );
            }
            else if (drive.StartsWith( @"\\" ))
            {
                // If the drive is of the form "\\C\", remove the final back-slash since the folder will have that.
                if (drive.Length == 4 && (drive[2].IsEnglishAlphabetLetter() && drive[3] == '\\'))
                {
                    driveSpec = drive.Substring( 0, 3 );
                }
            }
            // Now we have a driveSpec that is either "C:" or else "\\C" .

            string folderSpec;
            // If folder starts with a drive, remove that.
            if (folder.Length > 1 && folder[1] == ':')
            {
                folderSpec = folder.Substring( 2 );
            }
            else
            {
                folderSpec = folder;
            }
            string firstCharacter = folderSpec.Substring( 0, 1 );

            // If it starts with a leading forward-slash, change that to a back-slash.
            if (firstCharacter == "/")
            {
                folderSpec = @"\" + folderSpec.Substring( 1 );
            }
            else if (firstCharacter != @"\")
            {
                // Add a leading back-slash if it does not have one.
                folderSpec = @"\" + folderSpec;
            }

            string answer = driveSpec + folderSpec;
            return answer;
            //return driveSpec + folderSpec;
        }
        #endregion

        #region FilenameExtensionWithoutPeriod
        /// <summary>
        /// Given a filesystem-path, return the extension without the period-separator, if it has one,
        /// and return an empty string if it does not.
        /// </summary>
        /// <param name="pathname">the filesystem-path to get the extension of</param>
        /// <returns>the filename-extension, or an empty string</returns>
        /// <exception cref="ArgumentNullException">The given pathname must not be null</exception>
        public static string FilenameExtensionWithoutPeriod( this string pathname )
        {
            if (pathname == null)
            {
                throw new ArgumentNullException( "pathname" );
            }
            int indexOfLastPeriod = pathname.LastIndexOf( '.' );
            if (indexOfLastPeriod >= 0 && indexOfLastPeriod < pathname.Length - 1)
            {
                return pathname.Substring( indexOfLastPeriod + 1 );
            }
            else
            {
                return String.Empty;
            }
        }
        #endregion

        #region FilenameForRollover
        /// <summary>
        /// Given a base filename or pathname, produce the new name that would be used for the rolled-over file
        /// that corresponds to the given backup-number.
        /// </summary>
        /// <param name="baseName">The name (whether pathname or filename) to add the backup-number suffix to</param>
        /// <param name="backupNumber">The rolling-backup number, which should be non-negative</param>
        /// <returns>A new name of the form baseFileName.backupNumber, or just baseFileName if backupNumber is zero</returns>
        public static string FilenameForRollover( string baseName, int backupNumber )
        {
            if (baseName == null)
            {
                throw new ArgumentNullException( "baseName" );
            }
            if (backupNumber < 0)
            {
                throw new ArgumentOutOfRangeException( "backupNumber", "backupNumber (" + backupNumber + ") must be >= zero." );
            }
            if (backupNumber == 0)
            {
                return baseName;
            }
            // Create a new pathname from the original, of the form:
            //   If the original is LogNuts.log, and backupNumber is 2,
            //   then the result would be LogNuts(2).log
            string parenClosePart;
            if (baseName.Contains( "." ))
            {
                parenClosePart = ").";
            }
            else
            {
                parenClosePart = ")";
            }
            return baseName.PathnameWithoutExtension() + "(" + backupNumber + parenClosePart + baseName.FilenameExtensionWithoutPeriod();
        }
        #endregion

        //CBL  How do these next three methods differ ?
        // GetDirectoryNameOnlyFromPath gets just the final folder-name, not the entire directory-path.

        #region GetDirectoryNameOnlyFromPath
        /// <summary>
        /// Return the name of the directory that is denoted by the given filesystem-path.
        /// Note: This just gets the final folder-or-file, it cannot distinguish between them.
        /// </summary>
        /// <param name="path">the filesystem-path denoting what to get the lowest-level directory name of</param>
        /// <exception cref="ArgumentNullException">The value provided for the path argument must not be null</exception>
        /// <returns>the directory name</returns>
        public static string GetDirectoryNameOnlyFromPath( string path )
        {
            if (path == null)
            {
                throw new ArgumentNullException( "path" );
            }
            string trimmedPath = path.Trim().Replace( '/', '\\' );

            // This is from ZLongPathLib.
            int indexOfLastSlash = trimmedPath.LastIndexOf( '\\' );

            if (indexOfLastSlash < 0)
            {
                return trimmedPath;
            }
            else if (indexOfLastSlash == trimmedPath.Length - 1)
            {
                // The slash is the last character, so the "root" folder is the desired result.
                string drive, folder;
                bool ok = GetDriveAndDirectory( trimmedPath, out drive, out folder );
                if (ok)
                {
                    return folder;
                }
                return trimmedPath;
            }
            else
            {
                return trimmedPath.Substring( indexOfLastSlash + 1 );
            }
        }
        #endregion

        #region GetDirectoryOfPath
        /// <summary>
        /// Given a pathname, return the directory part (ie - without the file part).
        /// This returns the FULL directory-path including the drive-spec if present.
        /// </summary>
        /// <param name="pathname">the pathname to get the directory part of</param>
        /// <returns>just the directory - which may include a drive</returns>
        /// <exception cref="ArgumentNullException">The value provided for the path argument must not be null</exception>
        /// <remarks>
        /// The result which is returned, has forward-slashes replaced with back-slashes,
        /// and if the argument has "file:///" at the start - that is removed.
        /// 
        /// Essentially, this simply returns the given path up to the last backslash,
        /// or just return it as given if there're no backslashes.
        /// That last part AFTER the back-slash may be a file or it may be a folder-name, impossible to tell,
        /// so the assumption is that it is a file-name.
        /// </remarks>
        public static string GetDirectoryOfPath( string pathname )
        {
            if (pathname == null)
            {
                throw new ArgumentNullException( "pathname" );
            }
            string path1 = pathname.WithoutAtStart( "file:///" ).Trim();
            string path = StringLib.ConvertForwardSlashsToBackSlashs( path1 );
            // If it is of the form "C:\", then return it unchanged.
            if (path.Length == 3 && StringLib.IsEnglishAlphabetLetter( path[0] ) && path[1] == ':' && path[2] == '\\')
            {
                return path;
            }
            // Remove the end - the program-file itself.
            int n = path.Length;
            int indexOfLastBackSlash = -1;
            for (int i = n - 1; i > 0; i--)
            {
                if (path[i] == '\\')
                {
                    indexOfLastBackSlash = i;
                    break;
                }
            }
            //int indexOfLastBackSlash = path.LastIndexOf('\\');  //CBL Why did I not use this? !

            // If no slash - then just return the path as-is.
            if (indexOfLastBackSlash == -1)
            {
                return path;
            }
            string result = path.Substring( 0, indexOfLastBackSlash );

            // If the result is "C:", add the root-slash back.
            if (result.Length == 2 && result[1] == ':')
            {
                result = result + @"\";
            }
            return result;
        }
        #endregion

        //CBL This next seems to be redundant. Ensure the unit-tests are thorough before eliminating this

        #region GetDirectoryPathNameFromFilePath
        /// <summary>
        /// Returns the given path up to the last backslash, or just return it as given if there're no backslashes.
        /// </summary>
        public static string GetDirectoryPathNameFromFilePath( string filePath )
        {
            //CBL  This is from ZLongPathLib.
            // Seems redundant?
            int indexOfLastBackSlash = filePath.LastIndexOf( '\\' );

            // If there are no backslashes, then just return the path as given.
            if (indexOfLastBackSlash < 0)
            {
                return filePath;
            }
            else // otherwise, return everything up to that final backslash.
            {
                return filePath.Substring( 0, indexOfLastBackSlash );
            }
        }
        #endregion

        //CBL End redundancy. Create unit-tests that thoroughly work these out, then merge or rename them.

        #region GetDrive
        /// <summary>
        /// Given a filesystem-path, return the disk-drive portion including the colon, or null if none found.
        /// </summary>
        /// <param name="path">the filesystem-path to get the disk-drive from</param>
        /// <returns>a string containing the disk-drive letter and colon, if present, or else null</returns>
        /// <remarks>
        /// If you supply null or an empty string for the path, this returns <c>null</c>.
        /// </remarks>
        public static string GetDrive( string path )
        {
            if (StringLib.HasNothing( path ))
            {
                return null;
            }
            else
            {
                string pathTrimmed = StringLib.ConvertForwardSlashsToBackSlashs( path ).Trim();

                var colonPos = pathTrimmed.IndexOf( ':' );
                var slashPos = pathTrimmed.IndexOf( '\\' );

                if (colonPos <= 0)
                {
                    return null;
                }
                else
                {
                    if (slashPos < 0 || slashPos > colonPos)
                    {
                        return pathTrimmed.Substring( 0, colonPos + 1 );
                    }
                    else
                    {
                        return null;
                    }
                }
            }
        }
        #endregion

        #region GetDriveAndDirectory
        /// <summary>
        /// Given a filesystem-path, separate out the disk-drive and the directory and provide those to the caller
        /// as out-parameters.
        /// </summary>
        /// <param name="path">the filesystem-pathname to disect</param>
        /// <param name="drive">the disk-drive part of path, always uppercase</param>
        /// <param name="directory">this gets set to the directory portion of the given path</param>
        /// <returns>true if the drive and directory can be positively identified</returns>
        /// <exception cref="ArgumentNullException">The argument supplied for <paramref name="path"/> must not be null.</exception>
        public static bool GetDriveAndDirectory( string path, out string drive, out string directory )
        {
            if (path == null)
            {
                throw new ArgumentNullException( "path" );
            }
            drive = directory = String.Empty;
            bool answer = false;
            if (StringLib.HasSomething( path ))
            {
                if (path.Length >= 2 && Char.IsLetter( path[0] ) && path[1] == ':')
                {
                    answer = true;
                    drive = path[0].ToString().ToUpper();
                    if (path.Length > 2)
                    {
                        directory = path.Substring( 2 );
                    }
                }
                else if (path.Length >= 3 && path[0] == '\\' && path[1] == '\\')
                {
                    //  \\D\folder
                    int indexOfThirdSlash = path.IndexOf( '\\', 2 );
                    if (indexOfThirdSlash >= 3)
                    {
                        answer = true;
                        // Return everything after the double-slash as the drive, up to the next slash.
                        drive = path.Substring( 2, indexOfThirdSlash - 2 ).ToUpper();
                        directory = path.Substring( indexOfThirdSlash );
                    }
                    else if (indexOfThirdSlash == -1)
                    {
                        answer = true;
                        // Return everything after the double-slash as the drive, up to the next slash.
                        drive = path.Substring( 2 ).ToUpper();
                    }
                }
                else
                {
                    directory = path;
                }
            }
            return answer;
        }
        #endregion

        #region GetDriveOrShare
        /// <summary>
        /// Given a filesystem-path, return the disk-drive letter and colon, or else the folder-share-name portion,
        /// whichever of those is present.
        /// </summary>
        /// <param name="path">the filesystem-path to get the drive-letter or share-name from</param>
        /// <returns>a string containing the drive-letter or share-name, if present, or else an empty string</returns>
        public static string GetDriveOrShare( string path )
        {
            if (string.IsNullOrEmpty( path ))
            {
                return path;
            }
            else
            {
                if (!string.IsNullOrEmpty( GetDrive( path ) ))
                {
                    return GetDrive( path );
                }
                else if (!string.IsNullOrEmpty( GetShare( path ) ))
                {
                    return GetShare( path );
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        #endregion

        #region GetExtension
        /// <summary>
        /// Given a filesystem-path, return the 'extension' - that part that comes after the final period, if any.
        /// The string that is returned does NOT include the period.
        /// If no extension if found, then return an empty string.
        /// </summary>
        /// <param name="path">the filesystem-path to get the extension of</param>
        /// <returns>the extension part of that path, without the period</returns>
        public static string GetExtension( string path )
        {
            if (string.IsNullOrEmpty( path ))
            {
                return path;
            }
            else
            {
                var splitted = path.Split(
                    Path.DirectorySeparatorChar,
                    Path.AltDirectorySeparatorChar,
                    Path.VolumeSeparatorChar );

                if (splitted.Length > 0)
                {
                    var p = splitted[splitted.Length - 1];

                    var pos = p.LastIndexOf( '.' );
                    if (pos < 0)
                    {
                        return string.Empty;
                    }
                    else
                    {
                        return p.Substring( pos + 1 );
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }
        #endregion GetExtension

        #region GetFileNameFromFilePath
        /// <summary>
        /// Return the name of the file that is denoted by the given filesystem-path
        /// </summary>
        /// <param name="filePath">the filesystem-path denoting what to get the filename of</param>
        /// <returns>the file name</returns>
        public static string GetFileNameFromFilePath( string filePath )
        {
            int indexOfLastSlash = filePath.LastIndexOf( '\\' );

            if (indexOfLastSlash < 0)
            {
                return filePath;
            }
            else
            {
                return filePath.Substring( indexOfLastSlash + 1 );
            }
        }
        #endregion

        #region GetFileNameWithoutExtension
        /// <summary>
        /// Return the name of the file, without the extension (and without the period), that is denoted by the given filesystem-path,
        /// or just returns the original argument-value if no period is found within the filename.
        /// </summary>
        /// <param name="filePath">the filesystem-path denoting what to get the filename of</param>
        /// <returns>the file name without the extension, and without the period</returns>
        public static string GetFileNameWithoutExtension( string filePath )
        {
            if (filePath == null)
            {
                throw new ArgumentNullException( paramName: nameof( filePath ) );
            }
            var fn = GetFileNameFromFilePath( filePath );
            var ls = fn.LastIndexOf( '.' );

            if (ls < 0)
            {
                return filePath;
            }
            else
            {
                return fn.Substring( 0, ls );
            }
        }
        #endregion

        #region GetPathWithNewExtension
        /// <summary>
        /// Return the given filesystem-path with the new extension in place of it's existing extension.
        /// </summary>
        /// <param name="pathnameOriginal">the original pathname that we want to get a version of that has this other extension</param>
        /// <param name="newExtension">a new filename-extension such as .TXT, with or without the period</param>
        /// <returns>the value of the given pathname but with the given extension</returns>
        public static string GetPathWithNewExtension( string pathnameOriginal, string newExtension )
        {
            if (pathnameOriginal == null)
            {
                throw new ArgumentNullException( paramName: nameof( pathnameOriginal ) );
            }
            if (newExtension == null)
            {
                throw new ArgumentNullException( paramName: nameof( newExtension ) );
            }
            string nameWithoutExtenion = GetPathnameWithoutExtension( pathnameOriginal );
            if (newExtension.StartsWith( "." ))
            {
                return nameWithoutExtenion + newExtension;
            }
            return nameWithoutExtenion + "." + newExtension;
        }
        #endregion

        #region GetAbsolutePath
        /// <summary>
        /// Return the given path transformed into an absolute path, based on <paramref name="basePathToWhichToMakeAbsoluteTo"/> .
        /// If the given path is already an absolute path, then it is returned unchanged.
        /// </summary>
        /// <param name="pathToMakeAbsolute">The path to make absolute.</param>
        /// <param name="basePathToWhichToMakeAbsoluteTo">The base path to use when making an
        /// absolute path.</param>
        /// <returns>Returns the absolute path.</returns>
        /// <exception cref="ArgumentNullException">The values provided for either argument must not be null</exception>
        public static string GetAbsolutePath( string pathToMakeAbsolute, string basePathToWhichToMakeAbsoluteTo )
        {
            // Notes
            //   I don't understand what the point of this is (what is that 2nd parameter really?)
            //   and, was this ever finished?  2023/11/27
            //   Doesn't GetFullPath provide this?

            if (pathToMakeAbsolute == null)
            {
                throw new ArgumentNullException("pathToMakeAbsolute");
            }
            if (basePathToWhichToMakeAbsoluteTo == null)
            {
                throw new ArgumentNullException("basePathToWhichToMakeAbsoluteTo");
            }

            // Remove any leading or trailing spaces.
            string pathTrimmed = pathToMakeAbsolute.Trim();
            string basePathTrimmed = basePathToWhichToMakeAbsoluteTo.Trim();

            //CBL
            // http://stackoverflow.com/questions/623333/pathcanonicalize-equivalent-in-c-sharp
            // Needs work:

            if (IsAbsolutePath(pathTrimmed))
            {
                return pathTrimmed;
            }

            // At this point we know that it is not an absolute path.

            //#if NETFX_CORE
            string combinedPath = ZPath.Combine(basePathTrimmed, pathTrimmed);
            return Path.GetFullPath(combinedPath);

            //#else
            //            return GetFullPath( Combine( basePathTrimmed, pathTrimmed ) );
            //#endif
        }
        #endregion

        #region GetFullPath
#if !NETFX_CORE
        /// <summary>
        /// Given a filesystem-path that contains something other than an absolute path (such as ".."),
        /// return that path converted into an absolute pathname.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">The provided path must not be null</exception>
        public static string GetFullPath( string path )
        {
            if (path == null)
            {
                throw new ArgumentNullException( "path" );
            }
            path = CheckAddLongPathPrefix( path );

            // Determine length.

            var sb = new StringBuilder();

            var realLength = Native.Win32.GetFullPathName( path, 0, sb, IntPtr.Zero );


            sb.Length = realLength;
            realLength = Native.Win32.GetFullPathName( path, sb.Length, sb, IntPtr.Zero );

            if (realLength <= 0)
            {
                var lastWin32Error = Marshal.GetLastWin32Error();
                throw new Win32Exception(
                    lastWin32Error,
                    string.Format(
                        "Error {0} getting full path for '{1}': {2}",
                        lastWin32Error,
                        path,
                        CheckAddDotEnd( new Win32Exception( lastWin32Error ).Message ) ) );
            }
            else
            {
                return sb.ToString();
            }
        }
#endif
        #endregion GetFullPath

        #region GetLowestCommonRoot
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pathnames"></param>
        /// <param name="isDifferentDrives"></param>
        /// <param name="isRoot"></param>
        /// <param name="reason"></param>
        /// <returns></returns>
        public static string GetLowestCommonRoot( IEnumerable<string> pathnames, out bool isDifferentDrives, out bool isRoot, out string reason )
        {
            reason = null;
            isDifferentDrives = false;
            isRoot = false;
            string commonRoot = String.Empty;
            if (pathnames == null)
            {
                throw new ArgumentNullException( "pathnames" );
            }
            var listOfPaths = pathnames.ToList();
            if (listOfPaths.Count > 0)
            {
                string commonDrive = String.Empty;
                foreach (var path in listOfPaths)
                {
                    bool isPathTooLong;
                    if (GetWhetherPathIsSyntacticallyValid( path, out reason, out isPathTooLong ))
                    {
                        string drive = GetDrive( path );
                        if (String.IsNullOrEmpty( drive ))
                        {
                            reason = "Path \"" + path + "\" should have drive";
                            throw new ArgumentException( reason );
                        }
                        string dir = GetDirectoryOfPath( path );
                        // For the first one, simply take the directory part of the path.
                        if (String.IsNullOrEmpty( commonRoot ))
                        {
                            commonDrive = drive;
                            commonRoot = dir;
                        }
                        else
                        {
                            // Ensure the drive is still the same.
                            if (drive.Equals( commonDrive, StringComparison.OrdinalIgnoreCase ))
                            {
                                // The drive is the same.

                                while (dir != commonRoot && !IsRootDirectory( dir ) && !IsParentOf( dir, commonRoot ))
                                {
                                    dir = Directory.GetParent( dir ).FullName;
                                }
                                if (IsRootDirectory( dir ))
                                {
                                    commonRoot = dir;
                                    isRoot = true;
                                    reason = @"Lowest common root of " + path + " with the rest of the set is the root folder";
                                    break;
                                }

                                if (dir.Length < commonRoot.Length)
                                {
                                    if (commonRoot.StartsWith( dir ))
                                    {
                                        commonRoot = dir;
                                    }
                                }
                            }
                            else // different drive.
                            {
                                reason = "Path \"" + path + "\" has no common root with the rest of the set as it is on a different drive";
                                isDifferentDrives = true;
                                commonRoot = String.Empty;
                                break;
                            }
                        }
                    }
                    else
                    {
                        if (isPathTooLong)
                        {
                            throw new PathTooLongException( "path: " + reason );
                        }
                        throw new ArgumentException( reason, "path" );
                    }
                }
                if (IsRootDirectory( commonRoot ))
                {
                    isRoot = true;
                }
            }
            else // the pathnames list is empty.
            {
                reason = "Lowest common root of an empty list is an empty string";
            }
            return commonRoot;
        }
        #endregion

        #region GetPathnameWithoutExtension
        /// <summary>
        /// Return the given pathname, without the extension (and without the period) - if present,
        /// or return the pathname unchanged if no period is present within it.
        /// </summary>
        /// <param name="pathname">the filesystem-pathname that may include a period + extension part</param>
        /// <returns>the pathname without the extension, and without the period</returns>
        public static string GetPathnameWithoutExtension( string pathname )
        {
            if (pathname == null)
            {
                throw new ArgumentNullException( "pathname" );
            }

            int indexOfLastPeriod = pathname.LastIndexOf( '.' );

            if (indexOfLastPeriod < 0)
            {
                return pathname;
            }
            else
            {
                return pathname.Substring( 0, indexOfLastPeriod );
            }
        }
        #endregion

        #region GetPathRoot
        /// <summary>
        /// Return the top of the given filesystem-path, which is the drive or share-name.
        /// </summary>
        /// <param name="path">the filesystem-path to get the root of</param>
        /// <returns>just the root, or top-most part, of the given filesystem-path</returns>
        public static string GetPathRoot( string path )
        {
            path = ForceRemoveLongPathPrefix( path );
            return GetDriveOrShare( path );
        }
        #endregion

        #region GetRelativePath
        /// <summary>
        /// Makes a path relative to another.
        /// (i.e. what to type in a "cd" command to get from
        /// the PATH1 folder to PATH2). works like e.g. developer studio,
        /// when you add a file to a project: there, only the relative
        /// path of the file to the project is stored, too.
        /// e.g.:
        /// path1  = "c:\folder1\folder2\folder4\"
        /// path2  = "c:\folder1\folder2\folder3\file1.txt"
        /// result = "..\folder3\file1.txt"
        /// </summary>
        /// <param name="pathToWhichToMakeRelativeTo">The path to which to make relative to.</param>
        /// <param name="pathToMakeRelative">The path to make relative.</param>
        /// <returns>
        /// Returns the relative path, IF POSSIBLE.
        /// If not possible (i.e. no same parts in PATH2 and the PATH1),
        /// returns the complete PATH2.
        /// </returns>
        public static string GetRelativePath( string pathToWhichToMakeRelativeTo, string pathToMakeRelative )
        {
            if (string.IsNullOrEmpty( pathToWhichToMakeRelativeTo ) ||
                string.IsNullOrEmpty( pathToMakeRelative ))
            {
                return pathToMakeRelative;
            }
            else
            {
                var o = pathToWhichToMakeRelativeTo.ToLowerInvariant().Replace( '/', '\\' ).TrimEnd( '\\' );
                var t = pathToMakeRelative.ToLowerInvariant().Replace( '/', '\\' );

                // --
                // Handle special cases for Driveletters and UNC shares.

                var td = GetDriveOrShare( t );
                var od = GetDriveOrShare( o );

                td = td.Trim();
                td = td.Trim( '\\', '/' );

                od = od.Trim();
                od = od.Trim( '\\', '/' );

                // Different drive or share, i.e. nothing common, skip.
                if (td != od)
                {
                    return pathToMakeRelative;
                }
                else
                {
                    var ol = o.Length;
                    var tl = t.Length;

                    // compare each one, until different.
                    var pos = 0;
                    while (pos < ol && pos < tl && o[pos] == t[pos])
                    {
                        pos++;
                    }
                    if (pos < ol)
                    {
                        pos--;
                    }

                    // after comparison, make normal (i.e. NOT lowercase) again.
                    t = pathToMakeRelative;

                    // --

                    // noting in common.
                    if (pos <= 0)
                    {
                        return t;
                    }
                    else
                    {
                        // If not matching at a slash-boundary, navigate back until slash.
                        if (!(pos == ol || o[pos] == '\\' || o[pos] == '/'))
                        {
                            while (pos > 0 && (o[pos] != '\\' && o[pos] != '/'))
                            {
                                pos--;
                            }
                        }

                        // noting in common.
                        if (pos <= 0)
                        {
                            return t;
                        }
                        else
                        {
                            // --
                            // grab and split the reminders.

                            var oRemaining = o.Substring( pos );
                            oRemaining = oRemaining.Trim( '\\', '/' );

                            // Count how many folders are following in 'path1'.
                            // Count by splitting.
                            var oRemainingParts = oRemaining.Split( '\\' );

                            var tRemaining = t.Substring( pos );
                            tRemaining = tRemaining.Trim( '\\', '/' );

                            // --

                            var result = new StringBuilder();

                            // Path from path1 to common root.
                            foreach (var oRemainingPart in oRemainingParts)
                            {
                                if (!string.IsNullOrEmpty( oRemainingPart ))
                                {
                                    result.Append( @"..\" );
                                }
                            }

                            // And up to 'path2'.
                            result.Append( tRemaining );

                            // --

                            return result.ToString();
                        }
                    }
                }
            }
        }
        #endregion GetRelativePath

        #region GetShare
        /// <summary>
        /// Given a path of the form \\Server\Share\Other, return the \\Server\Share portion if that is present, otherwise an empty string.
        /// </summary>
        /// <param name="path">the filesystem-path to get the \\Server\Share-name from</param>
        /// <returns>a string containing the server and share-name, if present, or else an empty string</returns>
        public static string GetShare( string path )
        {
            //CBL  Learn the German words below, and properly document this code.
            if (String.IsNullOrEmpty( path ))
            {
                return String.Empty;
            }
            else
            {
                var str = path;

                // Nach Doppel-Slash suchen.
                // Kann z.B. "\\server\share\" sein,
                // aber auch "http:\\www.xyz.com\".

                // If there is no double-slash, return empty-string.
                const string dblslsh = @"\\";
                var n = str.IndexOf( dblslsh, StringComparison.Ordinal );
                if (n < 0)
                {
                    return String.Empty;
                }
                else  // there IS a double-slash.
                {
                    // Übernehme links von Doppel-Slash alles in Rückgabe
                    // (inkl. Doppel-Slash selbst).

                    // About open left of double-slash all in 
                    // ( ? double-slash even)

                    // Set ret to all the text up to and including that first double-slash.
                    var ret = str.Substring( 0, n + dblslsh.Length );
                    // Remove the text up to and including that first double-slash from str.
                    str = str.Remove( 0, n + dblslsh.Length );

                    // Jetzt nach Slash nach Server-Name suchen.
                    // Dieser Slash darf nicht unmittelbar nach den 2 Anfangsslash stehen.

                    // If there are no more slashes, return the empty-string.
                    n = str.IndexOf( '\\' );
                    if (n <= 0)
                    {
                        return string.Empty;
                    }
                    else  // there ARE additional slashes.
                    {
                        // Wiederum übernehmen in Rückgabestring.

                        // Append to ret - the text up to and including that next slash.
                        ret += str.Substring( 0, n + 1 );
                        // Remove from str the text up to and including that next slash.
                        str = str.Remove( 0, n + 1 );

                        // Jetzt nach Slash nach Share-Name suchen.      = Now according to Slash according to share-name search,
                        // Dieser Slash darf ebenfalls nicht unmittelbar = This slash must ? not immediately
                        // nach dem jetzigen Slash stehen.               = according to the current slash stand

                        //
                        n = str.IndexOf( '\\' );
                        if (n < 0)
                        {
                            n = str.Length;
                        }
                        else if (n == 0)
                        {
                            return string.Empty;
                        }

                        // Wiederum übernehmen in Rückgabestring, 
                        // aber ohne letzten Slash.

                        // Append n characters from str
                        ret += str.Substring( 0, n );

                        // The last character must not be a slash.
                        if (ret[ret.Length - 1] == '\\')
                        {
                            return string.Empty;
                        }
                        else
                        {
                            return ret;
                        }
                    }
                }
            }
        }
        #endregion

        #region GetWhetherPathIsSyntacticallyValid
        /// <summary>
        /// Return true if the given filesystem-path is syntactically valid on the Windows operating-system,
        /// check for null, empty, invalid characters, and the length of the individual parts.
        /// </summary>
        /// <param name="path">a filesystem-pathname to examine for invalid characters</param>
        /// <param name="reason">an English-language description of the problem found</param>
        /// <param name="isPathTooLong">this gets set to true if any part of the path is too long</param>
        /// <returns>true if the given pathname contains only valid filename-characters</returns>
        /// <remarks>
        /// If you want to throw exceptions in response to an invalid path, use the wrapper-method CheckPath.
        /// </remarks>
        public static bool GetWhetherPathIsSyntacticallyValid( string path, out string reason, out bool isPathTooLong )
        {
            isPathTooLong = false;

            if (path == null)
            {
                reason = "path is null";
                return false;
            }

            if (path.Length == 0)
            {
                reason = "path is empty";
                return false;
            }
            if (StringLib.HasNothing( path ))
            {
                reason = "contains only empty space";
                return false;
            }

            // Check for invalid characters..
            string pathname = path.Trim();
            // If the path starts with "//?/ then ignore that part.
            string pathWithOnlyBackSlashes = pathname.Replace( '/', '\\' );
            int i = 0;
            bool isLongPath = false;
            if (pathWithOnlyBackSlashes.StartsWith( @"\\?\" ))
            {
                isLongPath = true;
                i = 4;
            }
            // Check each character..
            for (; i < pathname.Length; i++)
            {
                if (!IsPathnameCharacterValid( pathname[i] ))
                {
                    string textOfBadChar = StringLib.CharacterDescription( pathname[i] );
                    string locationText = (i + 1).ToOrdinal();
                    reason = "The " + locationText + " character (" + textOfBadChar + "), is invalid for Windows filesystem-paths.";
                    return false;
                }
            }

            // Check the individual parts for excessive length (only for .NET Framework, not portable class libraries)..
#if !NETFX_CORE
            string basePart;
            string[] childParts = FileStringLib.SplitFolderPath( pathname, out basePart );
            foreach (var childPart in childParts)
            {
                if (childPart.Length > FilesystemLib.MAX_PATH)
                {
                    isPathTooLong = true;
                    string textOfPart = StringLib.Shortened( childPart, 20 );
                    reason = @"A folder or file-name within the path (""" + textOfPart + @""") has " + childPart.Length + " characters, but must not have more than " + FilesystemLib.MAX_PATH + ".";
                    return false;
                }
            }
#endif

            // Check for multiple colons..
            // Any existing colon may only be at index 1: "C:\Folder..." or 5: "\\?\C:\Tests.."
            if (pathname.Length > 2)
            {
                int indexPastPermissibleColon = isLongPath ? 6 : 2;

                int indexOf2ndColon = pathname.IndexOf( ':', indexPastPermissibleColon );
                if (indexOf2ndColon > 1)
                {
                    reason = "Extraneous colon at location index " + indexOf2ndColon + ", pathname = \"" + pathname + "\"";
                    return false;
                }
            }

            //CBL Should I also check the length of basePart ?

            reason = null;
            return true;
        }
        #endregion

        #region IsAbsolutePath
        /// <summary>
        /// Return true if the given path either starts with a drive-letter and colon,
        /// or else is a UNC path.
        /// </summary>
        /// <param name="path">the filesystem-pathname to test</param>
        /// <returns>true if the path starts with C:\ (for any drive C) or \\server\ </returns>
        /// <exception cref="ArgumentNullException">The given path must not be null</exception>
        public static bool IsAbsolutePath( this string path )
        {
            if (path == null)
            {
                throw new ArgumentNullException( "path" );
            }
            path = path.Replace( '/', '\\' );

            if (path.Length < 2)
            {
                return false;
            }
            else if (path.Substring( 0, 2 ) == @"\\")
            {
                // UNC.
                return IsUncPath( path );
            }
            else if (path.Substring( 1, 1 ) == @":")
            {
                // "C:"
                return StartsWithDriveLetter( path );
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region IsParentOf
        /// <summary>
        /// Return true if the given directory has possibleParentDirectory within it's parent-directory path.
        /// Eg, if directory is \Red\Green\Blue, and possibleParentDirectory is \Red, then the answer is true.
        /// If they are the same - return false.
        /// directory may actually a file - same result. 
        /// </summary>
        /// <param name="directory"></param>
        /// <param name="possibleParentDirectory"></param>
        /// <returns>true if possibleParentDirectory is within the parent-path of directory</returns>
        /// <exception cref="ArgumentNullException">The values provided for possibleParentDirectory and directory must not be null.</exception>
        public static bool IsParentOf( string possibleParentDirectory, string directory )
        {
            if (possibleParentDirectory == null)
            {
                throw new ArgumentNullException( "possibleParentDirectory" );
            }
            if (directory == null)
            {
                throw new ArgumentNullException( "directory" );
            }
            if (StringLib.HasNothing( possibleParentDirectory ))
            {
                throw new ArgumentException( "possibleParentDirectory is empty" );
            }
            if (StringLib.HasNothing( directory ))
            {
                throw new ArgumentException( "directory is empty" );
            }
            bool answer = false;

            // Uri parentUri = new Uri(possibleParentDirectory.ToUpper());
            // Uri childUri = new Uri(directory.ToUpper());
            // if (parentUri != childUri && parentUri.IsBaseOf(childUri))
            // {
            //     answer = true;
            // }

            // Trim leading and trailing space, and make uppercase.
            string parent = possibleParentDirectory.ToUpper().Trim();
            string folder = directory.ToUpper().Trim();

            // Deal with drives.
            string parentDrive, parentDirectory;
            bool parentHasDrive = GetDriveAndDirectory( parent, out parentDrive, out parentDirectory );

            string folderDrive, folderDirectory;
            bool folderHasDrive = GetDriveAndDirectory( directory, out folderDrive, out folderDirectory );

            //TODO
            bool couldBe = true;
            if (parentHasDrive)
            {
                if (folderHasDrive)
                {
                    if (parentDrive.Equals( folderDrive ))
                    {
                        parent = parentDirectory;
                        folder = folderDirectory.ToUpper();
                    }
                    else
                    {
                        couldBe = false;
                    }
                }
                else
                {
                    couldBe = false;
                }
            }
            else if (folderHasDrive)
            {
                couldBe = false;
            }

            // Require folder to have an initial slash.
            if (couldBe && folder.StartsWith( @"\" ))
            {
                // Ensure they're not equal.
                if (!folder.Equals( parent ))
                {
                    if (folder.StartsWith( parent ))
                    {
                        answer = true;
                    }
                }
            }
            return answer;
        }
        #endregion IsParentOf

        #region IsPathnameCharacterValid
        /// <summary>
        /// Return true if the given character is valid to use within a Windows pathname. This permits non-English UNICODE characters.
        /// Excluded characters include *, ?, |, ", and angle-brackets.
        /// </summary>
        /// <param name="pathnameCharacter">The character to evaluate for validity</param>
        /// <returns>true if the character may be used within a pathname</returns>
        public static bool IsPathnameCharacterValid( char pathnameCharacter )
        {
            // Most characters can be used in naming files.
            // These cannot: < > : " / \ | ? * although the fore and back slashes are valid in pathnames.
            bool isValid;
            // Note: I exclude the colon from this list of invalid chars because it can be used to denote the drive letter, but only there.
            switch (pathnameCharacter)
            {
                case '<':
                case '>':
                //case ':':  
                case '\"':
                case '|':
                case '?':
                case '*':
                    isValid = false;
                    break;
                default:
                    isValid = true;
                    break;
            }
            return isValid;
        }
        #endregion

        #region IsRootDirectory
        /// <summary>
        /// Given a filesystem path, return true if it consists of only the root folder (whether or not it includes the drive-letter).
        /// Eg, returns true for "\", and also for "C:\". Spaces are trimmed first.
        /// </summary>
        /// <param name="directoryPath">the filesystem-path to test</param>
        /// <returns>true if the given path is the root folder</returns>
        /// <exception cref="ArgumentNullException">The value provided for directoryPath must not be null.</exception>
        public static bool IsRootDirectory( this string directoryPath )
        {
            if (directoryPath == null)
            {
                throw new ArgumentNullException( "directoryPath" );
            }
            bool result = false;
            string path = directoryPath.Trim();
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

        #region IsThisDriveLetter
        /// <summary>
        /// Return true if this string has the same disk-drive letter as that of the given string.
        /// This simply compares the first letter, and ignores case.
        /// If either is null or empty - returns false.
        /// </summary>
        /// <param name="thisString">this string which starts with a drive-letter, to compare driveText against</param>
        /// <param name="driveText">the other string which starts with a drive-letter</param>
        /// <returns>true only if thisString and driveText start with the same letter</returns>
        public static bool IsThisDriveLetter( this string thisString, string driveText )
        {
            bool isIt = false;
            if (StringLib.HasSomething( thisString ) && StringLib.HasSomething( driveText ))
            {
                isIt = thisString.Substring( 0, 1 ).ToUpper() == driveText.Substring( 0, 1 ).ToUpper();
            }
            return isIt;
        }
        #endregion

        #region IsTheSameFilesystemPath
        /// <summary>
        /// Compare two filesystem pathnames and return true if they equate to the same thing,
        /// ignoring any drive-letter that either may have. However, if one is root and the other not - return false even if they could possibly be the same.
        /// </summary>
        /// <param name="pathReference">one pathname to compare</param>
        /// <param name="pathToTest">the other pathname to compare</param>
        /// <param name="isAbsolutePathRequired">false indicates a simple folder-name may match an absolute path, true means they do have to exactly match</param>
        /// <returns>true if both paths potentially refer to the same filesystem object, ignoring drive-letters</returns>
        /// <remarks>The argument for isAbsolutePathRequired controls whether a simple directory-name
        ///   may match against an absolute path.
        ///   For example, (if true) then "obj" matches "C:\Proj\obj".
        ///   This is used, for example, when iterating over filesystem paths and checking each against an exclusion-list.
        /// </remarks>
        public static bool IsTheSameFilesystemPath( this string pathReference, string pathToTest, bool isAbsolutePathRequired )
        {
            if (StringLib.HasNothing( pathReference ))
            {
                throw new ArgumentNullException( "pathReference" );
            }
            if (StringLib.HasNothing( pathToTest ))
            {
                throw new ArgumentNullException( "pathToTest" );
            }
            // Normalize the inputs by trimming space, and converting to uppercase.
            string pathReference2 = pathReference.Trim().ToUpper();
            string pathToTest2 = pathToTest.Trim().ToUpper();
            int n1 = pathReference2.Length;
            int n2 = pathToTest2.Length;
            // Remove any drive-letter, and the colon that follows it.
            if (n1 >= 2 && pathReference2[1] == ':' && pathReference2[0].IsEnglishAlphabetLetter())
            {
                pathReference2 = pathReference2.Substring( 2 );
            }
            if (n2 >= 2 && pathToTest2[1] == ':' && pathToTest2[0].IsEnglishAlphabetLetter())
            {
                pathToTest2 = pathToTest2.Substring( 2 );
            }
            // Remove any leading slashes.
            //if (pathReference2[0] == '\\')
            //{
            //    pathReference2 = pathReference2.Substring(1);
            //}
            //if (pathToTest2[0] == '\\')
            //{
            //    pathToTest2 = pathToTest2.Substring(1);
            //}
            // Remove any trailing slashes.
            n1 = pathReference2.Length;
            if (pathReference2[n1 - 1] == '\\')
            {
                pathReference2 = pathReference2.Substring( 0, n1 - 1 );
            }
            n2 = pathToTest2.Length;
            if (pathToTest2[n2 - 1] == '\\')
            {
                pathToTest2 = pathToTest2.Substring( 0, n2 - 1 );
            }
            // Finally, compare the normalized paths.
            if (pathToTest2.Equals( pathReference2 ))
            {
                return true;
            }
            else // need to consider 
            {
                if (isAbsolutePathRequired)
                {
                    return false;
                }
                else // an absolute path is not required.
                {
                    // See if one is an absolute path, and the other is a simple folder-name (with no slashes in it).
                    bool isReferencePathAbsolute = pathReference2[0] == '\\';
                    bool isTestPathAbsolute = pathToTest2[0] == '\\';
                    if (isReferencePathAbsolute && isTestPathAbsolute)
                    {
                        return false;
                    }
                    else
                    {
                        //CBL  This is clearly wrong!
                        // Need to measure from the end, proceeding toward the beginning of the string.

                        if (isReferencePathAbsolute)
                        {
                            return pathReference2.EndsWith( pathToTest2 );
                        }
                        else if (isTestPathAbsolute)
                        {
                            return pathToTest2.EndsWith( pathReference2 );
                        }
                        else // neither path is absolute.
                        {
                            // Return true if the longer path ends with the shorter path.
                            if (pathReference2.Length > pathToTest2.Length)
                            {
                                return pathReference2.EndsWith( pathToTest2 );
                            }
                            else
                            {
                                return pathToTest2.EndsWith( pathReference2 );
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region IsUncPath
        /// <summary>
        /// Return true if the given filesystem-path is in Universal Naming Convention (UNC)
        /// form,
        /// which is '\\{server}\{share-name}\{file-path}' or '\\?\UNC\{server}\{share-name}' (as long as share-name is not empty).
        /// </summary>
        /// <param name="path">the given filesystem-path to check</param>
        /// <returns>true if the path is a valid UNC path</returns>
        /// <exception cref="ArgumentNullException">The value provided for the path argument must not be null.</exception>
        /// <remarks>
        /// UNC names are used to identify network resources using a specific notation. UNC names consist of three parts -
        /// a server name, a share name, and an optional file path.
        /// 
        /// The three elements are combined using backslashes thusly:
        /// 
        /// <code>
        /// \\server\share\file_path
        /// </code>
        /// 
        /// In most versions of Windows, the built-in share name <c>admin$</c> refers to the root directory of the operating-system installation
        /// - usually <code>C:\WINNT</code> or <code>C:\WINDOWS</code>.
        /// 
        /// An empty (but non-null) value for the path argument results in a return-value of <c>false</c>.
        /// 
        /// If the path string has forward-slashes, those will be converted to back-slashes.
        /// </remarks>
        public static bool IsUncPath( string path )
        {
            // Which method is better?  CBL

            // See also https://msdn.microsoft.com/en-us/library/gg465305.aspx
            // See  http://stackoverflow.com/questions/520753/what-is-the-correct-way-to-check-if-a-path-is-an-unc-path-or-a-local-path
            //return Win32.PathIsUNC(path);

            if (path == null)
            {
                throw new ArgumentNullException( "path" );
            }
            //CBL This following is from ZLongPathLib.
            if (string.IsNullOrEmpty( path ))
            {
                return false;
            }
            else
            {
                string pathCorrected = StringLib.ConvertForwardSlashsToBackSlashs( path );

                if (pathCorrected.StartsWith( @"\\" ))
                {
                    if (pathCorrected.StartsWith( @"\\?\UNC\" ))
                    {
                        string share = GetShare( pathCorrected );
                        return !String.IsNullOrEmpty( share ) && share != @"\\?\UNC";
                    }
                    else if (pathCorrected.StartsWith( @"\\?\" ))
                    {
                        return false;
                    }
                    else
                    {
                        return !string.IsNullOrEmpty( GetShare( pathCorrected ) );
                    }
                }
                else
                {
                    return false;
                }
            }
        }
        #endregion

        #region NameMatchesPattern
        /// <summary>
        /// Check whether the given file-or-directory name matches the given file-spec expression (e.g. "*.txt").
        /// </summary>
        /// <param name="name">an actual file or directory name</param>
        /// <param name="expression">a file-spec expression to match the name against</param>
        /// <returns>true if the name matches the expression</returns>
        public static bool NameMatchesPattern( string name, string expression )
        {
            // See  https://stackoverflow.com/questions/725341/how-to-determine-if-a-file-matches-a-file-mask

            expression = expression.ToLowerInvariant();
            name = name.ToLowerInvariant();
            int num9;
            char ch = '\0';
            char ch2 = '\0';
            int[] sourceArray = new int[16];
            int[] numArray2 = new int[16];
            bool flag = false;
            if (((name == null) || (name.Length == 0)) || ((expression == null) || (expression.Length == 0)))
            {
                return false;
            }
            if (expression.Equals( "*" ) || expression.Equals( "*.*" ))
            {
                return true;
            }
            if ((expression[0] == '*') && (expression.IndexOf( '*', 1 ) == -1))
            {
                int length = expression.Length - 1;
                if ((name.Length >= length) && (string.Compare( expression, 1, name, name.Length - length, length, StringComparison.OrdinalIgnoreCase ) == 0))
                {
                    return true;
                }
            }
            sourceArray[0] = 0;
            int num7 = 1;
            int num = 0;
            int num8 = expression.Length * 2;
            while (!flag)
            {
                int num3;
                if (num < name.Length)
                {
                    ch = name[num];
                    num3 = 1;
                    num++;
                }
                else
                {
                    flag = true;
                    if (sourceArray[num7 - 1] == num8)
                    {
                        break;
                    }
                }
                int index = 0;
                int num5 = 0;
                int num6 = 0;
                while (index < num7)
                {
                    int num2 = (sourceArray[index++] + 1) / 2;
                    num3 = 0;
                Label_00F2:
                    if (num2 != expression.Length)
                    {
                        num2 += num3;
                        num9 = num2 * 2;
                        if (num2 == expression.Length)
                        {
                            numArray2[num5++] = num8;
                        }
                        else
                        {
                            ch2 = expression[num2];
                            num3 = 1;
                            if (num5 >= 14)
                            {
                                int num11 = numArray2.Length * 2;
                                int[] destinationArray = new int[num11];
                                Array.Copy( numArray2, destinationArray, numArray2.Length );
                                numArray2 = destinationArray;
                                destinationArray = new int[num11];
                                Array.Copy( sourceArray, destinationArray, sourceArray.Length );
                                sourceArray = destinationArray;
                            }
                            if (ch2 == '*')
                            {
                                numArray2[num5++] = num9;
                                numArray2[num5++] = num9 + 1;
                                goto Label_00F2;
                            }
                            if (ch2 == '>')
                            {
                                bool flag2 = false;
                                if (!flag && (ch == '.'))
                                {
                                    int num13 = name.Length;
                                    for (int i = num; i < num13; i++)
                                    {
                                        char ch3 = name[i];
                                        num3 = 1;
                                        if (ch3 == '.')
                                        {
                                            flag2 = true;
                                            break;
                                        }
                                    }
                                }
                                if ((flag || (ch != '.')) || flag2)
                                {
                                    numArray2[num5++] = num9;
                                    numArray2[num5++] = num9 + 1;
                                }
                                else
                                {
                                    numArray2[num5++] = num9 + 1;
                                }
                                goto Label_00F2;
                            }
                            num9 += num3 * 2;
                            switch (ch2)
                            {
                                case '<':
                                    if (flag || (ch == '.'))
                                    {
                                        goto Label_00F2;
                                    }
                                    numArray2[num5++] = num9;
                                    goto Label_028D;

                                case '"':
                                    if (flag)
                                    {
                                        goto Label_00F2;
                                    }
                                    if (ch == '.')
                                    {
                                        numArray2[num5++] = num9;
                                        goto Label_028D;
                                    }
                                    break;
                            }
                            if (!flag)
                            {
                                if (ch2 == '?')
                                {
                                    numArray2[num5++] = num9;
                                }
                                else if (ch2 == ch)
                                {
                                    numArray2[num5++] = num9;
                                }
                            }
                        }
                    }
                Label_028D:
                    if ((index < num7) && (num6 < num5))
                    {
                        while (num6 < num5)
                        {
                            int num14 = sourceArray.Length;
                            while ((index < num14) && (sourceArray[index] < numArray2[num6]))
                            {
                                index++;
                            }
                            num6++;
                        }
                    }
                }
                if (num5 == 0)
                {
                    return false;
                }
                int[] numArray4 = sourceArray;
                sourceArray = numArray2;
                numArray2 = numArray4;
                num7 = num5;
            }
            num9 = sourceArray[num7 - 1];
            return (num9 == num8);
        }
        private const char ANSI_DOS_QM = '<';
        private const char ANSI_DOS_STAR = '>';
        private const char DOS_DOT = '"';
        private const int MATCHES_ARRAY_SIZE = 16;
        #endregion NameMatchesPattern

        #region PathnameWithoutExtension
        /// <summary>
        /// Return the given filesystem pathname without the extension,
        /// which is the portion that comes after the final period.
        /// The string that is returned does not include that final period.
        /// </summary>
        /// <param name="pathname">a string representing a filesystem-path, that we want to get a portion of</param>
        /// <returns>pathname without the extension nor the final period</returns>
        /// <exception cref="ArgumentNullException">The value provided for the pathname argument must not be null.</exception>
        public static string PathnameWithoutExtension( this string pathname )
        {
            if (pathname == null)
            {
                throw new ArgumentNullException( "pathname" );
            }
            int indexOfLastPeriod = pathname.LastIndexOf( '.' );
            if (indexOfLastPeriod >= 0)
            {
                return pathname.Substring( 0, indexOfLastPeriod );
            }
            else
            {
                return pathname;
            }
        }
        #endregion

        #region StartsWithDriveAndRoot
        /// <summary>
        /// Give a filesystem path, return true if it starts with a drive-letter and colon (with any whitespace trimmed),
        /// or if it begins with "ED:\" which is used to indicate the whichever drive the executable is running from.
        /// </summary>
        /// <param name="filesystemPath">the path to check</param>
        /// <param name="drive">the drive letter (or "ED") that is found is written to this</param>
        /// <param name="folder">the part of filesystemPath apart from the drive specification</param>
        /// <returns>true if the path starts with a disk-drive spec of the form "C:"</returns>
        /// <remarks>
        /// Eg, if <c>filesystemPath</c> is "C:\folder1\folder2", then this method-call would
        /// set <c>drive</c> to "C" and <c>folder</c> to "\folder1\folder2".
        /// </remarks>
        public static bool StartsWithDriveAndRoot( this string filesystemPath, out string drive, out string folder )
        {
            drive = folder = null;
            if (StringLib.HasSomething( filesystemPath ))
            {
                string path = filesystemPath.Trim();
                if (path.Length >= 3 && StringLib.IsEnglishAlphabetLetter( path[0] ) && path[1] == ':' && (path[2] == '\\' || path[2] == '/'))
                {
                    // Path begins with "X:\" or "X:/".
                    drive = path.Substring( 0, 1 );
                    // For the folder, capture everything after that colon.
                    folder = path.Substring( 2 );
                    return true;
                }
                else if (path.Length >= 4 && path[0] == 'E' && path[1] == 'D' && path[2] == ':' && (path[3] == '\\' || path[3] == '/'))
                {
                    // Path begins with "ED:\" or "ED:/", indicating the executable-drive.
                    drive = "ED";
                    // For the folder, capture everything after that colon.
                    folder = path.Substring( 3 );
                    return true;
                }
                else
                {
                    folder = filesystemPath;
                }
            }
            return false;
        }
        #endregion

        #region StartsWithDriveLetter
        /// <summary>
        /// Give a filesystem path, return true if it starts with a drive-letter and colon (with any whitespace trimmed).
        /// </summary>
        /// <param name="filesystemPath">the path to check</param>
        /// <returns>true if the path starts with a disk-drive spec of the form "C:"</returns>
        public static bool StartsWithDriveLetter( this string filesystemPath )
        {
            if (StringLib.HasSomething( filesystemPath ))
            {
                string path = filesystemPath.Trim();
                if (path.Length >= 2 && StringLib.IsEnglishAlphabetLetter( path[0] ) && path[1] == ':')
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region StartsWithRootButNoDrive
        /// <summary>
        /// Give a filesystem path, return true if it starts with a root-directory (with any whitespace trimmed).
        /// </summary>
        /// <param name="filesystemPath">the path to check</param>
        /// <returns>true if the <c>filesystemPath</c> starts with a root-directory, ie a slash or backslash</returns>
        public static bool StartsWithRootButNoDrive( this string filesystemPath )
        {
            if (StringLib.HasSomething( filesystemPath ))
            {
                string path = filesystemPath.Trim();
                if (path[0] == '\\' || path[0] == '/')
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region TopOfFilesystemPath
        /// <summary>
        /// Given a string that represents a filesystem directory (such as "Folder1\Folder2\Folder3"),
        /// return the topmost folder (like "Folder1") and put the remainder into pathRemainder (e.g. "Folder2\Folder3").
        /// If the string is empty then return null.
        /// If there is no path-separator (ie, there is only the one folder) then that folder is returned and pathRemainder is set to null.
        /// If path begins with a disk-drive in the form "C:\Folder1\Folder2", for example,
        /// then this returns "C:\" and pathRemainder is set to "Folder1\Folder2".
        /// </summary>
        /// <param name="filesystemFolder">the filesystem directory from which to get the top-most folder</param>
        /// <param name="pathRemainder">what remainds of path after extracting the topmost folder</param>
        /// <returns>the name of the folder that appears uppermost on the given path, or null if there is none</returns>
        public static string TopOfFilesystemPath( string filesystemFolder, out string pathRemainder )
        {
            string result = null;
            pathRemainder = null;
            if (StringLib.HasSomething( filesystemFolder ))
            {
                string path = filesystemFolder.Trim();
                if (StringLib.HasSomething( path ))
                {
                    // If it starts with a slash, remove that..
                    bool doesStartWithSlash = path[0] == Path.DirectorySeparatorChar;
                    if (doesStartWithSlash)
                    {
                        if (path.Length == 1)
                        {
                            result = null;
                            return result;
                        }
                        path = path.Substring( 1 );
                    }
                    if (path.StartsWithDriveLetter())
                    {
                        if (path.Length > 2 && path[2] != Path.DirectorySeparatorChar)
                        {
                            result = path.Substring( 0, 2 ) + Path.DirectorySeparatorChar;
                            pathRemainder = path.Substring( 2 );
                        }
                        else if (path.Length == 2)
                        {
                            result = path.Substring( 0, 2 ) + Path.DirectorySeparatorChar;
                            // The length is 2 so there is no string left.
                            pathRemainder = null;
                        }
                        else // Length is > 2 and the 3rd character is the slash
                        {
                            result = Path.GetPathRoot( path );
                            if (path.Length > 3)
                            {
                                pathRemainder = path.Substring( 3 );
                            }
                            else
                            {
                                pathRemainder = null;
                            }
                        }
                        return result;
                    }
                    int i = path.IndexOf( Path.DirectorySeparatorChar );
                    if (i == -1)
                    {
                        return path;
                    }
                    else
                    {
                        result = path.Substring( 0, i );
                        if (path.Length > i + 1)
                        {
                            pathRemainder = path.Substring( i + 1 ).WithoutAtEnd( Path.DirectorySeparatorChar );
                        }
                    }
                }
            }
            return result;
        }
        #endregion

        #region internal implementation

        #region CheckAddDotEnd
        /// <summary>
        /// Given a string, return a copy of that with a period appended to it (if it doesn't already have one).
        /// </summary>
        /// <param name="text">the string to append the period to</param>
        /// <returns>the given text with a period on the end of it</returns>
        public static string CheckAddDotEnd( string text )
        {
            //CBL From ZLongPathLib

            //CBL  Should I make this public?  But it's not file-related.
            if (string.IsNullOrEmpty( text ))
            {
                return @".";
            }
            else
            {
                text = text.Trim();
                if (text.EndsWith( @"." ))
                {
                    return text;
                }
                else
                {
                    return text + @".";
                }
            }
        }
        #endregion

        #region CheckAddLongPathPrefix
        /// <summary>
        /// If the given path is longer than MAX_PATH characters, return it with (either "\\?\" or "\\?\UNC\") prefixed to it,
        /// that prefix being "\\?\UNC\" if the path is in UNC form, or "\\?\" otherwise.
        /// </summary>
        /// <param name="path">the filesystem-path to add the prefix to</param>
        /// <returns>a copy of the given path with "\\?\" prefixed to it if necessary</returns>
        public static string CheckAddLongPathPrefix( string path )
        {
            //CBL  From Microsoft's own error-message: The filename must be less than 260 characters, and the directory name must be less than 248 characters.
            string result;
#if NETFX_CORE
            // I am not doing any special long-path processing for other than .NET Framework libraries.
            result = path;
#else
            if (path.StartsWith( @"\\?\" ))
            {
                result = path;
            }
            else if (path.Length >= Native.Win32.MAX_PATH)
            {
                result = ForceAddLongPathPrefix( path );
            }
            else
            {
                result = path;
            }
#endif
            return result.Trim();
        }
        #endregion

        #region ForceAddLongPathPrefix
        /// <summary>
        /// Return the given path with "\\?\UNC\" prefixed if it was already in UNC form, or "\\?\" otherwise,
        /// unless it already starts with "\\?\".
        /// </summary>
        /// <param name="path">a string denotting a filesystem-path</param>
        /// <returns>a copy of path with "\\?\" or "\\?\UNC\" prefixed to it</returns>
        public static string ForceAddLongPathPrefix( string path )
        {
            if (string.IsNullOrEmpty( path ) || path.StartsWith( @"\\?\" ))
            {
                return path;
            }
            else
            {
                // http://msdn.microsoft.com/en-us/library/aa365247.aspx

                if (path.StartsWith( @"\\" ))
                {
                    // UNC.
                    return @"\\?\UNC\" + path.Substring( 2 );
                }
                else
                {
                    return @"\\?\" + path;
                }
            }
        }
        #endregion

        #region ForceRemoveLongPathPrefix
        internal static string ForceRemoveLongPathPrefix( string path )
        {
#if NETFX_CORE
            // I am not doing any special long-path processing for other than .NET Framework libraries.
            return path;
#else
            //CBL This is from ZLongPathLib

            if (string.IsNullOrEmpty( path ) || !path.StartsWith( @"\\?\" ))
            {
                return path;
            }
            else if (path.StartsWith( @"\\?\UNC\", StringComparison.OrdinalIgnoreCase ))
            {
                return path.Substring( @"\\?\UNC\".Length );
            }
            else
            {
                return path.Substring( @"\\?\".Length );
            }
#endif
        }
        #endregion

        #region SplitFolderPath
        /// <summary>
        /// Given a directory-path, get the (drive or share-server-name) as basePart,
        /// and return the individual folder-names as an array of strings.
        /// </summary>
        /// <param name="directoryPath">the path to get the parts of</param>
        /// <param name="basePart">the path's disk-drive or share-name gets written to this</param>
        /// <returns>an array of strings that contains the folder-names that were within the given directory-path</returns>
        public static string[] SplitFolderPath( string directoryPath, out string basePart )
        {
            //CBL This comes from ZLongPathLib.

            directoryPath = ForceRemoveLongPathPrefix( directoryPath );

            basePart = GetDriveOrShare( directoryPath );
            var remaining = directoryPath.Substring( basePart.Length );

            string[] childParts = remaining.Trim( '\\' ).Split( new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries );
            return childParts;
        }
        #endregion

        #endregion internal implementation
    }
}
