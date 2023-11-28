using System;
using System.Diagnostics;
using System.IO;
using System.Security.AccessControl;
using System.Threading;
#if !PRE_4
using System.Threading.Tasks;
#endif


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// This class provides a simplified way to acquire a lock on a filesystem-folder.
    /// It implements IDisposable and is intended for testing filesystem-related code.
    /// </summary>
    public class FolderLocker : IDisposable
    {
        // See
        // http://stackoverflow.com/questions/4198048/how-to-lock-folder-in-c-sharp

        /// <summary>
        /// Create a new <c>FolderLocker</c> object that refers to the given filesystem-folder.
        /// </summary>
        /// <param name="folderPath">the filesystem-folder that this object may want to lock</param>
        public FolderLocker( string folderPath )
        {
            _folderPath = folderPath;
        }

        /// <summary>
        /// Acquire a lock (that is, set Full-Control to Deny for the current user) on the filesystem-folder that this FolderLocker references.
        /// </summary>
        public void Lock()
        {
            //Debug.WriteLine( "begin FolderLocker.Lock on folder " + _folderPath );
            // getting your adminUserName
            string adminUserName = Environment.UserName;
            DirectorySecurity ds = Directory.GetAccessControl( _folderPath );
            FileSystemAccessRule fsa = new FileSystemAccessRule( adminUserName, FileSystemRights.FullControl, AccessControlType.Deny );
            ds.AddAccessRule( fsa );
            Directory.SetAccessControl( _folderPath, ds );
            _isLocked = true;
            //Debug.WriteLine( "end FolderLocker.Lock" );
        }

        /// <summary>
        /// Acquire a lock (that is, set Full-Control to Deny for the current user) on the filesystem-folder that this FolderLocker references,
        /// for the given interval of time, and then unlock it.
        /// </summary>
        /// <param name="forHowLong">a <see cref="TimeSpan"/> that denotes how long to lock the folder, before unlocking it</param>
        public void Lock( TimeSpan forHowLong )
        {
            //Debug.WriteLine("FolderLocker.Lock for " + forHowLong + " of folder " + _folderPath);
            Lock();

#if !PRE_4
            Task.Factory.StartNew( _ =>
            {
                Thread.Sleep( forHowLong );
            }, TaskCreationOptions.PreferFairness ).ContinueWith( _ =>
            {
                //Debug.WriteLine("FolderLocker.Unlock");
                Unlock();
            } );
#else
            // The .NET 3.5 way
            int delay = (int)forHowLong.TotalMilliseconds;
            System.Threading.Timer timer = null;
            timer = new System.Threading.Timer( s =>
            {
                timer.Dispose();
                Debug.WriteLine( "FolderLocker.Unlock" );
                Unlock();
            }, null, delay, Timeout.Infinite );
#endif
        }

        /// <summary>
        /// Acquire a lock (that is, set Full-Control to Deny for the current user) for the given interval of time,
        /// and then unlock it.
        /// </summary>
        /// <param name="folderPath">this denotes the folder to lock and then unlock</param>
        /// <param name="forHowLong">a <see cref="TimeSpan"/> that denotes how long to lock the folder, before unlocking it</param>
        /// <returns>a new <c>FolderLocker</c> object which may be used to unlock the folder</returns>
        public static FolderLocker TakeLockOn( string folderPath, TimeSpan forHowLong )
        {
            var folderLocker = new FolderLocker( folderPath );
            folderLocker.Lock( forHowLong );
            return folderLocker;
        }

        /// <summary>
        /// If the referenced filesystem-folder is still locked by this <c>FolderLocker</c> object - then unlock it.
        /// If it has already been unlocked (that is, had the lock imposed by this object, removed) - then this does nothing.
        /// </summary>
        public void Unlock()
        {
            //Debug.WriteLine( "FolderLocker.Unlock, _isLocked = " + _isLocked );
            if (_isLocked)
            {
                string adminUserName = Environment.UserName; // getting your adminUserName
                DirectorySecurity ds = Directory.GetAccessControl( _folderPath );
                FileSystemAccessRule fsa = new FileSystemAccessRule( adminUserName, FileSystemRights.FullControl, AccessControlType.Deny );
                ds.RemoveAccessRule( fsa );
                Directory.SetAccessControl( _folderPath, ds );
                _isLocked = false;
            }
        }

        /// <summary>
        /// Release any resources held by this object - which in this case consists of unlocking the folder.
        /// </summary>
        public void Dispose()
        {
            if (_isLocked)
            {
                Unlock();
            }
        }

        /// <summary>
        /// This flag serves to remember when the folder has been locked, and when it has been unlocked.
        /// It does not necessarily indicate whether that folder was already locked by some other means.
        /// </summary>
        private bool _isLocked;

        /// <summary>
        /// This string denotes the filesystem-folder that this FolderLocker refers to.
        /// </summary>
        private readonly string _folderPath;
    }
}
