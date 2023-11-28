#if (NETFW_462_OR_ABOVE)
#define NO_LONGPATH
#endif
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
#if !PRE_4
using System.Threading.Tasks;
#endif
#if !NETFX_CORE
using Hurst.LogNut.Util.Native;
#endif


// Note: .NET Framework at 4.62 and above, no longer has the path-length limitation.
// Define the compiler-pragma NETFW_462_OR_ABOVE when targetting this version of .NET.


namespace Hurst.LogNut.Util
{
    public static partial class FilesystemLib
    {
        #region DeleteFile( fileInfo )
        /// <summary>
        /// Remove the given file that this <c>FileInfo</c> object represents, if it exists.
        /// </summary>
        /// <param name="fileInfo">a FileInfo that denotes the file to delete</param>
        /// <exception cref="ArgumentNullException">the value provided for fileInfo must not be null</exception>
        /// <remarks>
        /// This can safely be called with a path to a file that does not exist.
        /// 
        /// Any non-normal attributes that are set on the file - such as the Read-Only attribute - are cleared first.
        /// </remarks>
        public static void DeleteFile( FileInfo fileInfo )
        {
            if (fileInfo == null)
            {
                throw new ArgumentNullException( "fileInfo" );
            }

            if (fileInfo.Exists)
            {
                SetFileAttributes( fileInfo.FullName, FileAttributes.Normal );
                fileInfo.Delete();
            }
        }
        #endregion

        #region DeleteFile( pathname )
        /// <summary>
        /// Remove the given file, if it exists.
        /// </summary>
        /// <param name="pathname">the pathname of the file to delete</param>
        /// <exception cref="ArgumentNullException">the value provided for pathname must not be null</exception>
        /// <remarks>
        /// This can safely be called with a path to a file that does not exist.
        /// 
        /// Any non-normal attributes that are set on the file - such as the Read-Only attribute - are cleared first.
        /// </remarks>
        public static void DeleteFile( string pathname )
        {
            if (pathname == null)
            {
                throw new ArgumentNullException( "pathname" );
            }

            if (FileExists( pathname ))
            {
                SetFileAttributes( pathname, FileAttributes.Normal );
#if NO_LONGPATH
                File.Delete( pathname );
#else
                bool isDeletedOk = false;
                try
                {
                    isDeletedOk = Win32.DeleteFile( pathname );
                }
                catch (Exception x)
                {
                    int lastWin32Error = Marshal.GetLastWin32Error();
                    // Don't include lastWin32Error if it is 0.
                    if (lastWin32Error != Native.Win32.ERROR_SUCCESS)
                    {
                        x.Data.Add( "LastWin32Error", lastWin32Error );
                        string errorMessage = new Win32Exception( lastWin32Error ).Message;
                        if (!String.IsNullOrEmpty( errorMessage ))
                        {
                            x.Data.Add( "Win32ErrorMessage", errorMessage );
                        }
                    }
                    throw x;
                }
#endif
            }
        }
        #endregion

        #region DeleteFile( pathname, retryNotifier, timeoutInMilliseconds )
        /// <summary>
        /// Remove the given file, if it exists.
        /// This retries the deletion if it fails, and keeps retrying for up to the given amount of time.
        /// Set <paramref name="timeoutInMilliseconds"/> to zero to prevent it from retrying.
        /// </summary>
        /// <param name="pathname">the pathname of the file to delete</param>
        /// <param name="retryNotifier">an delegate to call when a retry occurs (may be null)</param>
        /// <param name="timeoutInMilliseconds">how long to keep re-trying the deletion, in milliseconds</param>
        /// <returns>true if the file was successfully removed (or was not there), false if it was unable to do so</returns>
        /// <exception cref="ArgumentNullException">the value provided for pathname must not be null</exception>
        /// <exception cref="ArgumentException">the value provided for pathname must be a syntactically-valid path</exception>
        /// <exception cref="PathTooLongException">pathname must not have folder or file names that are too long</exception>
        /// <exception cref="ArgumentOutOfRangeException">the value provided for timeoutInMilliseconds must be zero or positive</exception>
        /// <remarks>
        /// This can safely be called with a path to a file that does not exist.
        /// 
        /// If the Read-Only attribute is set on the file, that is ignored.
        /// 
        /// If <paramref name="timeoutInMilliseconds"/> is any positive value, then, the deletion is retried until it succeeds, up to that amount of time.
        /// The timeout value is in units of milliseconds, and the default if you call the overload of this method that does not provide this parameter, is five seconds.
        /// If it is set to zero, then the deletion is NOT retried - this simply returns false upon failure.
        /// 
        /// If retrying is called for - delay values are used in this sequence:
        /// 25ms, 50ms, 100ms, 200ms, 400ms, 800ms, 1s, 2s, 3s .. increasing thereafter by seconds up to your specified maximum timeout.
        /// </remarks>
#if NETFX_CORE
        public static async void DeleteFile( string pathname, int timeoutInMilliseconds )
#else
        public static bool DeleteFile( string pathname, FileProgressNotifier retryNotifier, int timeoutInMilliseconds )
#endif
        {
            // Check the argument...
            if (pathname == null)
            {
                throw new ArgumentNullException( "pathname" );
            }
            bool isSuccessful = false;
            //Debug.WriteLine( "DeleteFile(" + pathname + ")" ); //CBL
            if (timeoutInMilliseconds < 0)
            {
#if PRE_4
                throw new ArgumentOutOfRangeException( "timeoutInMilliseconds", "The value for the timeout (" + timeoutInMilliseconds + ") must not be negative." );
#else
                throw new ArgumentOutOfRangeException( paramName: nameof( timeoutInMilliseconds ), message: "The value for the timeout (" + timeoutInMilliseconds + ") must not be negative." );
#endif
            }

#if !NETFX_CORE
            string correctedPath = FileStringLib.CheckAddLongPathPrefix( pathname );
#else
            string correctedPath = pathname;
#endif

            // No need to do anything unless the file actually does exist.
            if (FileExists( correctedPath ))
            {

                // The extra code here is kept to a minimum, until an actual error-condition is detected.
                bool isDeletedOk;
                Exception exceptionThatWasThrown = null;

                try
                {
                    // The first attempt to delete the file.
                    //
#if NETFX_CORE
                    File.Delete( correctedPath );
                    // If that did not throw an exception, then assume it proceeded okay.
                    isDeletedOk = true;
#else
                    isDeletedOk = Win32.DeleteFile( correctedPath );
                    // Debug.WriteLine( "  in DeleteFile: Win32 call did not throw exception, isDeletedOk is " + isDeletedOk );  //CBL
#endif
                }
                catch (UnauthorizedAccessException x)
                {
                    exceptionThatWasThrown = x;
                    Debug.WriteLine( "  in DeleteFile, call to first Win32.DeleteFile threw: " + StringLib.ExceptionDetails( x, true ) );  //CBL
                    if (retryNotifier != null)
                    {
                        retryNotifier( isRetry: true, isDirectory: false, message: "In DeleteFile(" + pathname + "): call to first Win32.DeleteFile threw: " + StringLib.ExceptionDetails( x, true ) );
                    }
                    isDeletedOk = false;
                }

                //CBL  Q: Does the return-code from Win32.DeleteFile, and catching of an exception, comprise a reliable test
                //        of whether the deletion happened?  Should I also do a test of FileExists?

                if (isDeletedOk)
                {
                    isSuccessful = true;
                }

                if (!isDeletedOk)
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append( "DeleteFile(" ).Append( pathname ).Append( ")," );
                    if (exceptionThatWasThrown != null)
                    {
                        sb.Append( " first call to Win32.DeleteFile threw exception " ).Append( exceptionThatWasThrown ).Append( ". " );
                        if (retryNotifier != null)
                        {
                            retryNotifier( isRetry: true, isDirectory: false, message: sb.ToString() );
                        }
                    }

                    // Assume here that these two error-codes are not actually indicators of failure.
                    // System Error Codes (0..499)
                    // http://msdn.microsoft.com/en-us/library/ms681382(VS.85).aspx.
                    int lastWin32Error = Marshal.GetLastWin32Error();
                    string errorMessage1 = new Win32Exception( lastWin32Error ).Message;
#if DEBUG
                    Debug.WriteLine( "  in DeleteFile: lastWin32Error is " + lastWin32Error + ": " + StringLib.AsString( errorMessage1 ) );  //CBL
#endif
                    // Error-code 5 ("Access is denied") results when the file is Read-Only.

                    // Enter a retry-loop...
                    //
                    int nAttempt = 1;
                    var stopwatch = Stopwatch.StartNew();
                    int retryDelay = 100;
                    bool isTimedOut = false;

                    while (!isSuccessful)
                    {
                        nAttempt++;
                        try
                        {
                            // Clear any file-attributes such as ReadOnly, Hidden, or System.
                            SetFileAttributes( correctedPath, FileAttributes.Normal );
                            // Check the 'Hidden' attribute on this file.
                            //bool wasHidden = IsFileHidden( correctedPath );
                            //if (wasHidden)
                            //{
                            //    SetFileHidden( correctedPath, false );
                            //}

                            //// Check the 'Readonly' attribute on this file.
                            //bool wasReadonly = IsFileReadonly( correctedPath );
                            //if (wasReadonly)
                            //{
                            //    Debug.WriteLine( "  in DeleteFile, was Readonly" );
                            //    SetFileReadonly( correctedPath, false );
                            //}

                            // Try again to delete it.
                            //
#if NETFX_CORE
                            File.Delete( correctedPath );
                            // If that did not throw an exception, then assume it proceeded okay.
                            isDeletedOk = true;
#else
                            isDeletedOk = Win32.DeleteFile( correctedPath );
#endif
                            Debug.WriteLine( "  in DeleteFile: 2nd call to Win32 call did not throw exception, isDeletedOk is " + isDeletedOk );  //CBL
                        }
                        catch (UnauthorizedAccessException)
                        {
                            //Debug.WriteLine( "  in DeleteFile: UnauthorizedAccessException" );  //CBL
                            if (retryNotifier != null)
                            {
                                retryNotifier( isRetry: true, isDirectory: false, message: "In DeleteFile(" + pathname + "): UnauthorizedAccessException" );
                            }
                            isDeletedOk = false;
                        }
                        //catch (Exception x)
                        //{
                        //    Debug.WriteLine( "  in DeleteFile: Exception, " + StringLib.ExceptionDetails( x ) );  //CBL
                        //    isDeletedOk = false;
                        //}

                        if (isDeletedOk)
                        {
                            isSuccessful = true;
                            Debug.WriteLine( "  Win32.DeleteFile succeeded after " + nAttempt + " attempts." );
                        }
                        else // still a problem.
                        {
                            lastWin32Error = Marshal.GetLastWin32Error();
                            string msg = String.Format( "Upon attempt {0} FilesystemLib.DeleteFile({1}, {2}) failed with error: {3}", nAttempt, pathname, timeoutInMilliseconds, lastWin32Error );
                            Debug.WriteLine( msg ); //CBL
                            if (retryNotifier != null)
                            {
                                retryNotifier( isRetry: true, isDirectory: false, message: "In DeleteFile(" + pathname + "): " + msg );
                            }

                            // See if we have run out of time.
                            if (stopwatch.ElapsedMilliseconds > timeoutInMilliseconds)
                            {
                                stopwatch.Stop();
                                isTimedOut = true;
                                // Try one last time to see if it has actually gone away.
                                isDeletedOk = !FileExists( correctedPath );
                                if (isDeletedOk)
                                {
                                    isSuccessful = true;
                                    Debug.WriteLine( msg + Environment.NewLine + "- but the file appears to be gone now, so - alrighty then." );
                                }
                                else
                                {
                                    //CBL  Do I want to throw a TimeoutException?
                                    Debug.WriteLine( msg + Environment.NewLine + "- giving up." );
                                    break;
                                }
                            }
                            else // we still have time to keep trying.
                            {
                                // 32 is the error that I have gotten whenever the file is locked, so we will only retry on that particular error-condition.
                                //if (lastWin32Error == 32)
                                {
                                    // We still have a last-Win32-Error of 32, so we will try again..

                                    Debug.WriteLine( msg + Environment.NewLine + " will wait " + retryDelay + "ms and try again." );
                                    if (retryNotifier != null)
                                    {
                                        retryNotifier( isRetry: true, isDirectory: false, message: "In DeleteFile(" + pathname + "): wait " + retryDelay + " and try again." );
                                    }
#if NETFX_CORE
                                    await Task.Delay( millisecondsDelay: retryDelay );
#else
                                    Thread.Sleep( retryDelay );
#endif
                                    // Compute the next delay - proceeding through the values 50, 100, 200, 400, 800, 1000, 2000, 3000.. 10000.
                                    if (retryDelay < 500)
                                    {
                                        retryDelay *= 2;
                                    }
                                    else
                                    {
                                        retryDelay = 1000;
                                    }
                                }
                                //else // now it is a different error (other than 32). It is time to say "fuckit".
                                //{
                                //    break;
                                //}
                            }
                        }
                    } // end loop.

#if NETFX_CORE
                    //CBL
                    Task<string> taskThatKeepsTrying = KeepTryingToDeleteFile( pathname, timeoutInMilliseconds );

                    await taskThatKeepsTrying;
#endif

                    if (!isSuccessful)
                    {
                        //CBL Add all relevant debugging-information to the exception that we are going to throw.
                        string msg = "This despite " + nAttempt + " attempts.";
                        Exception x;
                        if (exceptionThatWasThrown == null)
                        {
                            if (lastWin32Error == 32)
                            {
                                x = new UnauthorizedAccessException( msg );
                            }
                            else if (isTimedOut)
                            {
                                x = new TimeoutException( msg );
                            }
                            else
                            {
                                x = new IOException( msg );
                            }
                        }
                        else
                        {
                            x = exceptionThatWasThrown;
                        }

                        x.Data.Add( "pathname", pathname );
                        x.Data.Add( "timeoutInMilliseconds", timeoutInMilliseconds );
#if !NETFX_CORE
                        // Don't include lastWin32Error if it is 0.
                        if (lastWin32Error != Native.Win32.ERROR_SUCCESS)
                        {
                            x.Data.Add( "LastWin32Error", lastWin32Error );
                            string errorMessage = new Win32Exception( lastWin32Error ).Message;
                            if (!String.IsNullOrEmpty( errorMessage ))
                            {
                                x.Data.Add( "Win32ErrorMessage", errorMessage );
                            }
                        }
#endif
                        throw x;
                    }

                }
            }

            return isSuccessful;
        }
        #endregion
    }
}
