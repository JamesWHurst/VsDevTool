using System;
using System.IO;
using System.Text;
using System.Threading;
#if !PRE_4
using System.Threading.Tasks;
#endif


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// This class provides a simple way to acquire a lock on a file.
    /// It is an IDisposable and intended for testing filesystem-related code.
    /// </summary>
    public class FileLocker : IDisposable
    {
        /// <summary>
        /// Acquire a read/write lock on the given file pathname, for a given number of milliseconds at the most.
        /// The file contents itself are effected.
        /// </summary>
        /// <param name="filePathname">the pathname of the file to lock</param>
        /// <param name="forHowLong">the maximum amount of time, in milliseconds, to maintain the lock</param>
        /// <returns></returns>
        public static FileLocker TakeLockOn(string filePathname, int forHowLong)
        {
            var fileLocker = new FileLocker();
#if !PRE_4
            fileLocker.LockTheFile_UsingLock(filePathname, forHowLong);
#else
            fileLocker.LockTheFile(filePathname, forHowLong);
#endif
            return fileLocker;
        }

        /// <summary>
        /// Acquire a read/write lock on the given file pathname, with no time-limit.
        /// The file contents itself are effected.
        /// </summary>
        /// <param name="filePathname">the pathname of the file to lock</param>
        public static void TakeLockOn(string filePathname)
        {
            UnicodeEncoding uiEncoding = new UnicodeEncoding();
            FileStream fileStream = new FileStream(filePathname, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            string content = "This file was written simply for the purpose of locking it.";
            fileStream.Write(uiEncoding.GetBytes(content), 0, uiEncoding.GetByteCount(content));
        }

#if !PRE_4
        private void LockTheFile_UsingLock( string filePathname, int forHowLong )
        {
            // The call to CreateFileHandle fails if the file is Read-Only.
            // So, if that attribute is set on this file, clear it first, open it,
            // and then set that attribute again.
            bool wasReadOnly = false;
            if (FilesystemLib.IsFileReadonly( filePathname ))
            {
                wasReadOnly = true;
                FilesystemLib.SetFileReadonly( filePathname, false );
            }

            _fileStream = new FileStream( filePathname, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None );

            _fileStream.Lock( 0, 1 );

            // Restore the Read-Only attribute if that had been originally set.
            if (wasReadOnly)
            {
                FilesystemLib.SetFileReadonly( filePathname, true );
            }

            Task.Factory.StartNew( _ =>
            {
                Thread.Sleep( forHowLong );
            }, TaskCreationOptions.AttachedToParent ).ContinueWith( _ =>
            {
                if (_fileStream != null)
                {
                    Console.WriteLine( "FileLocker timed out, releasing lock on file." );
                    _fileStream.Unlock( 0, 1 );
                    _fileStream.Dispose();
                    _fileStream = null;
                }
            } );
        }
#endif

        private void LockTheFile(string filePathname, int forHowLong)
        {
            string longCapablePath = FileStringLib.CheckAddLongPathPrefix(filePathname);

            // The call to CreateFileHandle fails if the file is Read-Only.
            // So, if that attribute is set on this file, clear it first, open it,
            // and then set that attribute again.
            bool wasReadOnly = false;
            if (FilesystemLib.IsFileReadonly(longCapablePath))
            {
                wasReadOnly = true;
                FilesystemLib.SetFileReadonly(longCapablePath, false);
            }

            string content = "This file was written simply for the purpose of locking it.";
            UnicodeEncoding uiEncoding = new UnicodeEncoding();

            if (longCapablePath.StartsWith(@"\\?"))
            {
                // Using this alternate code-path just for long pathnames.
                _fileStream =
                    new FileStream(
                        FilesystemLib.CreateFileHandle(longCapablePath,
                                                        Native.CreationDisposition.CreateAlways,
                                                        Native.FileAccess.GenericWrite,
                                                        Native.FileShare.None,
                                                        0),
                        System.IO.FileAccess.Write);
            }
            else
            {
                _fileStream = new FileStream(longCapablePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }

            _fileStream.Write(uiEncoding.GetBytes(content), 0, uiEncoding.GetByteCount(content));

            // Restore the Read-Only attribute if that had been originally set.
            if (wasReadOnly)
            {
                FilesystemLib.SetFileReadonly(longCapablePath, true);
            }
#if !PRE_4
            Task.Factory.StartNew( _ =>
            {
                Thread.Sleep( forHowLong );
            }, TaskCreationOptions.AttachedToParent ).ContinueWith( _ =>
            {
                if (_fileStream != null)
                {
                    Console.WriteLine( "FileLocker timed out, releasing lock on file." );
                    _fileStream.Dispose();
                    _fileStream = null;
                }
            } );
#else
            // The .NET 3.5 way
            Timer timer = null;
            timer = new System.Threading.Timer(s =>
            {
                timer.Dispose();
                if (_fileStream != null)
                {
                    Console.WriteLine("FileLocker timed out, releasing lock on file.");
                    _fileStream.Dispose();
                    _fileStream = null;
                }
            }, null, forHowLong, Timeout.Infinite);
#endif
        }

        /// <summary>
        /// Release any contained resources held by this object.
        /// </summary>
        public void Dispose()
        {
            if (_fileStream != null)
            {
                _fileStream.Dispose();
                _fileStream = null;
            }
        }

        private FileStream _fileStream;
    }
}
