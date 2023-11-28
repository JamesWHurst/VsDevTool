#if (NETFW_462_OR_ABOVE)
#define NO_LONGPATH
#endif
using System;
using System.Collections.Generic;
using System.IO;


// Compile with the pragma NO_LONGPATH if you want to use only the normal .NET filesystem facilities.


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// This is the Zeta-Long-Paths library object that extends the concept of a FileInfo object
    /// and supports paths that are longer than MAX_FILE characters.
    /// </summary>
    public class ZFileInfo
    {
        #region constructor
        /// <summary>
        /// Create a new ZFileInfo object that represents the file at the given filesystem-path.
        /// </summary>
        /// <param name="pathname">the filesystem-path that this object will represent</param>
        /// <exception cref="ArgumentNullException">The value provided for pathname must not be null.</exception>
        public ZFileInfo( string pathname )
        {
            if (pathname == null)
            {
                throw new ArgumentNullException( "pathname" );
            }
            _path = pathname;
        }
        #endregion

        #region properties

        #region Attributes
        /// <summary>
        /// Get or set the FileAttributes of this file.
        /// </summary>
        public FileAttributes Attributes
        {
            get
            {
#if NETFX_CORE
                return File.GetAttributes( _path );
#else
                return FilesystemLib.GetFileAttributes( _path );
#endif
            }
            set
            {
#if NETFX_CORE
                File.SetAttributes( path: _path, fileAttributes: value );
#else
                FilesystemLib.SetFileAttributes( _path, value );
#endif
            }
        }
        #endregion

        #region Directory
        /// <summary>
        /// Get a new ZDirectoryInfo object based upon the DirectoryName of this.
        /// </summary>
        public ZDirectoryInfo Directory
        {
            get { return new ZDirectoryInfo( DirectoryName ); }
        }
        #endregion

        #region DirectoryName
        /// <summary>
        /// Get the path without the filename.
        /// </summary>
        public string DirectoryName
        {
            get { return FileStringLib.GetDirectoryPathNameFromFilePath( _path ); }
        }
        #endregion

        #region Exists
        /// <summary>
        /// Get whether the pathname represented by this ZFileInfo does in fact exist.
        /// </summary>
        public bool Exists
        {
            get
            {
#if (NETFX_CORE || NO_LONGPATH)
                return File.Exists( _path );
#else
                return FilesystemLib.FileExists( _path );
#endif
            }
        }
        #endregion

        #region Extension
        /// <summary>
        /// Get the filename extension (the part after the final period) from the filesystem-path that this object represents,
        /// if any.
        /// </summary>
        public string Extension
        {
            get
            {
                return FileStringLib.GetExtension( _path );
            }
        }
        #endregion

        #region FullName
        /// <summary>
        /// Get the full path that this ZFileInfo object represents.
        /// </summary>
        public string FullName
        {
            get
            {
                return _path;
            }
        }
        #endregion

        #region IsReadOnly
        /// <summary>
        /// Return true if the the physical filesystem-file that this object denotes has it's read-only attribute set.
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
#if NETFX_CORE
                return (this.Attributes & FileAttributes.ReadOnly) != 0;
#else
                return FilesystemLib.IsFileReadonly( _path );
#endif
            }
            set
            {
#if NETFX_CORE
                FileAttributes originalAttributes = this.Attributes;
                FileAttributes newAttributes;
                if (value)
                {
                    newAttributes = originalAttributes | FileAttributes.ReadOnly;
                }
                else
                {
                    newAttributes = originalAttributes & ~FileAttributes.ReadOnly;
                }
                this.Attributes = newAttributes;
#else
                FilesystemLib.SetFileReadonly( _path, value );
#endif
            }
        }
        #endregion

        #region Length
        /// <summary>
        /// Get the size of the file that this ZFileInfo object represents, in bytes (as a 64-bit Long value).
        /// </summary>
        public long Length
        {
            get
            {
#if (NETFX_CORE || NO_LONGPATH)
                FileInfo fileInfo = new FileInfo( _path );
                return fileInfo.Length;
#else
                return FilesystemLib.GetFileLength( _path );
#endif
            }
        }
        #endregion

        #region Name
        /// <summary>
        /// Get the name of the file itself, without the drive or directory.
        /// </summary>
        public string Name
        {
            get
            {
                return FileStringLib.GetFileNameFromFilePath( _path );
            }
        }
        #endregion

#if !NETFX_CORE
        #region Owner
        /// <summary>
        /// Get the name of the owner of this file, as a string.
        /// </summary>
        public string Owner
        {
            get
            {
                return FilesystemLib.GetFileOwner( _path );
            }
        }
        #endregion
#endif

        #region times
        /// <summary>
        /// Get or set the DateTime representing the creation-time of this file.
        /// </summary>
        public DateTime CreationTime
        {
            get
            {
#if NETFX_CORE
                return File.GetCreationTime( _path );
#else
                return FilesystemLib.GetFileCreationTime( _path );
#endif
            }
            set
            {
#if NETFX_CORE
                File.SetCreationTime( path: _path, creationTime: value );
#else
                FilesystemLib.SetFileCreationTime( _path, value );
#endif
            }
        }

        /// <summary>
        /// Get or set the DateTime representing the last-access-time of this file.
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

        /// <summary>
        /// Get or set the DateTime representing the last-write-time of this file.
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
        #endregion times

        #endregion properties

        #region AppendText
        /// <summary>
        /// Append the given text to the file denoted by this ZFileInfo's path.
        /// Creates the file if it does not exist.
        /// </summary>
        /// <param name="contents">the text string to write to it</param>
        /// <param name="isToWriteThrough">set this to true if you want this to write to disk without buffering</param>
        /// <remarks>
        /// I added this to the library. James Hurst
        /// </remarks>
        public void AppendText( string contents, bool isToWriteThrough )
        {
#if NETFX_CORE
//CBL But what about isToWriteThrough ?
            File.AppendAllText( path: _path, contents: contents );
#else
            if (Exists)
            {
                FilesystemLib.WriteAllText( _path, contents, FileMode.Append, isToWriteThrough );
            }
            else
            {
                FilesystemLib.WriteAllText( _path, contents, FileMode.CreateNew, isToWriteThrough );
            }
#endif
        }
        #endregion

        #region CopyTo
        /// <summary>
        /// Copy this file (which this ZFileInfo represents) to the given destination path.
        /// </summary>
        /// <param name="destinationFilePath">the filesystem-path to copy this file to</param>
        public void CopyTo( string destinationFilePath )
        {
#if NETFX_CORE
            File.Copy( sourceFileName: _path, destFileName: destinationFilePath );
#else
            FilesystemLib.CopyFile( _path, destinationFilePath );
#endif
        }

        /// <summary>
        /// Copy this file (which this ZFileInfo represents) to the destination represented by the given ZFileInfo object.
        /// </summary>
        /// <param name="destinationFileInfo">a ZFileInfo object representing the path to copy this to</param>
        public void CopyTo( ZFileInfo destinationFileInfo )
        {
#if NETFX_CORE
            File.Copy( sourceFileName: _path, destFileName: destinationFileInfo._path );
#else
            FilesystemLib.CopyFile( _path, destinationFileInfo._path );
#endif
        }
        #endregion

        #region Delete
        /// <summary>
        /// Delete the file that this object represents from the physical filesystem, if it exists.
        /// </summary>
        /// <remarks>
        /// This can safely be called with a path to a file that does not exist.
        /// </remarks>
        public void Delete()
        {
            FilesystemLib.DeleteFile( _path );
        }

        /// <summary>
        /// Delete the file that this object represents from the physical filesystem, if it exists.
        /// </summary>
        /// <param name="timeoutInMilliseconds">how long to keep re-trying the deletion, in milliseconds (optional - default is 5,000 ms)</param>
        /// <exception cref="ArgumentOutOfRangeException">the value provided for timeoutInMilliseconds must be zero or positive</exception>
        /// <remarks>
        /// Under Universal Windows Platform -- the argument timeoutInMilliseconds has no effect.
        /// 
        /// This can safely be called with a path to a file that does not exist.
        /// 
        /// If <paramref name="timeoutInMilliseconds"/> is any positive value, then, the deletion is retried until it succeeds, up to that amount of time.
        /// The timeout value is in units of milliseconds, and defaults to 5 seconds.
        /// If it is set to zero, then the deletion is NOT retried - this simply returns false upon failure.
        /// </remarks>
        public void Delete( int timeoutInMilliseconds )
        {
            FilesystemLib.DeleteFile( _path, null, timeoutInMilliseconds );
        }
        #endregion

        #region MakeTempCopy
        /// <summary>
        /// Derive and return the pathname of a temporary copy of this file, in the same folder with "_TEMP" added to the filename
        /// before the extension. This does not actually create the file that this pathname denotes.
        /// </summary>
        /// <returns>the pathname of the new copy</returns>
        public string MakeTempCopy()
        {
            string pathWithoutExt = FileStringLib.GetPathnameWithoutExtension( _path );
            string extension = FileStringLib.GetExtension( _path );
            string pathnameOfCopy;
            if (StringLib.HasNothing( extension ))
            {
                pathnameOfCopy = _path + "_TEMP";
            }
            else
            {
                pathnameOfCopy = pathWithoutExt + "_TEMP." + extension;
            }
            //CBL  WTF?!
#if NETFX_CORE
            File.Delete( pathnameOfCopy );
#else
            FilesystemLib.DeleteFile( pathnameOfCopy );
#endif
            return pathnameOfCopy;
        }
        #endregion

        #region ReadAllText
        /// <summary>
        /// Read all of the text from the file at the given path.
        /// </summary>
        /// <returns>a string containing all of the text read from the file</returns>
        public string ReadAllText()
        {
#if NETFX_CORE
            return File.ReadAllText( path: _path );
#else
            return FilesystemLib.ReadAllText( _path );
#endif
        }
        #endregion

        #region ReadText
        /// <summary>
        /// Read text from this file, up to the given number of characters,
        /// and return that text as a string.
        /// </summary>
        /// <param name="numberOfCharacters">the number of text characters to read</param>
        /// <returns>a string containing characters read from the file</returns>
        /// <exception cref="ArgumentNullException">if path is null</exception>
        /// <exception cref="FileNotFoundException">if path represents a non-existing file</exception>
        public string ReadText( int numberOfCharacters )
        {
#if NETFX_CORE
            var sb = new StringBuilder();
            //string result = "";
            //byte[] buffer;
            int n = 0;
            using (var fileStream = File.OpenRead( _path ))
            {
                using (var sr = new StreamReader( fileStream ))
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
            }
            return sb.ToString();
#else
            return FilesystemLib.ReadText( _path, numberOfCharacters );
#endif
        }
        #endregion

        #region ReadTextAsListOfRecords
        /// <summary>
        /// Read all of the text from this file into a List of strings,
        /// using the given record-terminator character as a delimiter to break up the text.
        /// </summary>
        /// <param name="recordTerminationChar">a character used to mark the boundaries between records</param>
        /// <returns>a list of strings from the file</returns>
        public List<string> ReadTextAsListOfRecords( char recordTerminationChar )
        {
#if NETFX_CORE
            var result = new List<string>();
            var sb = new StringBuilder();
            int iTerminator = (int)recordTerminationChar;

            using (var fileStream = File.OpenRead( _path ))
            {
                using (var sr = new StreamReader( fileStream ))
                {
                    // Loop through the file, one character at a time..
                    while (!sr.EndOfStream)
                    {
                        int iChar = sr.Read();
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
                            sb.Clear();
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
#else
            return FilesystemLib.ReadTextAsListOfRecords( _path, recordTerminationChar );
#endif
        }

        /// <summary>
        /// Read all of the text from this file into a List of strings,
        /// using the given record-terminator text as a delimiter to break up the text.
        /// </summary>
        /// <param name="recordTermination">a string that marks the end of a record</param>
        /// <returns>a list of strings from the file</returns>
        /// <exception cref="ArgumentNullException">if path is null</exception>
        /// <exception cref="ArgumentException">if path is not null but is an empty string or just white-space</exception>
        /// <exception cref="FileNotFoundException">if path represents a non-existing file</exception>
        public List<string> ReadTextAsListOfRecords( string recordTermination )
        {
#if NETFX_CORE
            var result = new List<string>();

            using (var fileStream = File.OpenRead( _path ))
            {
                using (var sr = new StreamReader( fileStream ))
                {
                    var sb = new StringBuilder();
                    // Loop through the file, one character at a time..
                    while (!sr.EndOfStream)
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
                            // It is not the end of the file, so accumulate this character.
                            sb.Append( (char)iChar );

                            // See whether this character causes the end of this line to match a recordTermination.
                            string lineThusFar = sb.ToString();
                            if (lineThusFar.EndsWith( recordTermination ))
                            {
                                // We encountered a record-termination string.
                                // Gather the text accumulated thus far into a record and add that to our result.
                                result.Add( lineThusFar );
                                sb.Clear();
                            }
                            //CBL Set up a unit-test to see how this handles new-lines.
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
#else
            return FilesystemLib.ReadTextAsListOfRecords( _path, recordTermination );
#endif
        }
        #endregion

        #region ToString
        /// <summary>
        /// Override the ToString method to provide more useful information.
        /// </summary>
        /// <returns>an indication of what the path is</returns>
        public override string ToString()
        {
            string pathValue = "null";
            if (_path == String.Empty)
            {
                pathValue = "empty";
            }
            else if (_path != null)
            {
                pathValue = _path;
            }
            return "ZFileInfo object with path " + pathValue;
        }
        #endregion

        #region WriteAllText
        /// <summary>
        /// Write the given text to the file denoted by this ZFileInfo's path.
        /// If you specify Append for the fileMode, it creates the file if it does not exist.
        /// </summary>
        /// <param name="contents">the text string to write to it</param>
        /// <param name="fileMode">the mode for writing to the file</param>
        /// <param name="isToWriteThrough">set this to true if you want this to write to disk without buffering</param>
        /// <remarks>
        /// I added this to the library. James Hurst
        /// </remarks>
        public void WriteAllText( string contents, FileMode fileMode, bool isToWriteThrough )
        {
#if NETFX_CORE
            if (fileMode == FileMode.Append)
            {
                File.AppendAllText( path: _path, contents: contents );
            }
            else
            {
                File.WriteAllText( path: _path, contents: contents );
            }
#else
            FilesystemLib.WriteAllText( _path, contents, fileMode, isToWriteThrough );
#endif
        }
        #endregion

        #region internal implementation

        #region CreateHandle
#if FALSE
        /// <summary>
        /// Pass the file handle to the <see cref="System.IO.FileStream"/> constructor. 
        /// The <see cref="System.IO.FileStream"/> will close the handle.
        /// </summary>
        //internal SafeFileHandle CreateHandle(CreationDisposition creationDisposition,
        //                                     FileAccess fileAccess,
        //                                     FileShare fileShare)
        //{
        //    return FilesystemLib.CreateFileHandle(_path, creationDisposition, fileAccess, fileShare, 0);
        //}
#endif
        #endregion

        #region fields

        /// <summary>
        /// This is the filesystem-path of the file that this ZFileInfo object represents.
        /// </summary>
        private readonly string _path;

        #endregion fields

        #endregion internal implementation
    }
}