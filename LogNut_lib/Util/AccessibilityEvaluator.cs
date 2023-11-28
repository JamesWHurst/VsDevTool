using System;
using System.ComponentModel;
using System.IO;


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// class AccessibilityEvaluator exists to implement IAccessibilityEvaluator in order to provide unit-testability.
    /// </summary>
    public class AccessibilityEvaluator : IAccessibilityEvaluator
    {
        #region CanWriteTo
        /// <summary>
        /// Test the given folder to see whether it exists (or can be created) and can be written to.
        /// If the folder does not exist, this creates it (if it can).
        /// </summary>
        /// <param name="folderPath">the directory to test for writeability</param>
        /// <param name="reason">If the test fails - a description of the failure is assigned to this. Otherwise this is set to null.</param>
        /// <returns>true if the folder can be written to</returns>
        /// <remarks>
        /// If the given <paramref name="folderPath"/> does not exist - this attempts to create it, and if that happens with no exceptions
        /// then it is considered writeable (this folder is not deleted afterward).
        /// </remarks>
        public bool CanWriteTo( string folderPath, out string reason )
        {
            bool isWriteable = false;
            reason = null;

#if !NETFX_CORE
            // First see whether it specifies the drive-letter of a drive that does not appear to exist.
            string drive = FileStringLib.GetDrive( folderPath );
            if (drive != null && !FilesystemLib.DriveExists( drive ))
            {
                reason = @"Testing path """ + folderPath + @""" for writeability yields NO: Invalid drive """ + drive + @""".";
                isWriteable = false;
            }
            else // The drive-letter appears to be good.
#endif
            {
                if (!FilesystemLib.DirectoryExists( folderPath ))
                {
                    try
                    {
                        FilesystemLib.CreateDirectory( folderPath );
                        // If we accomplished that without an exception, then consider this folder to be writeable.
                        isWriteable = true;
                    }
                    catch (Exception x)
                    {
                        isWriteable = false;
                        string thisReason;
                        bool isPathTooLong;
                        if (!FileStringLib.GetWhetherPathIsSyntacticallyValid( folderPath, out thisReason, out isPathTooLong ))
                        {
                            reason = @"Testing folder """ + folderPath + @""" for writeability, but the path itself is improperly formed: " + thisReason;
                        }
                        else
                        {
                            reason = "Testing folder for writeability yields NO: " + StringLib.ExceptionNameShortened( x ) + " trying to create folder: " + x.Message + @". ";
                        }
                    }
                }
                else
                {
                    // The folder does exist. Check to see whether we can write to a file within it.
                    string testFilename = "";
                    string testPathname = "";
                    string testContent = "This is just to test for writability.";
                    bool isTryingToDelete = false;

                    try
                    {
                        // Find a filename that does not already exist within this folder.
                        int i = 0;
                        for (; i < 1000; i++)
                        {
                            testFilename = String.Format( "XXLogNutTestFile{0}.txt", i );
                            testPathname = Path.Combine( folderPath, testFilename );
                            if (!File.Exists( testPathname ))
                            {
                                break;
                            }
                        }

                        // Verify that we can create it.
                        FilesystemLib.WriteText( testPathname, testContent );
                        if (FilesystemLib.FileExists( testPathname ))
                        {
                            isTryingToDelete = true;
                            FilesystemLib.DeleteFile( testPathname );
                            // If we accomplished that without an exception, then consider this folder to be writeable.
                            isWriteable = true;
                        }
                    }
                    catch (Exception x)
                    {
                        isWriteable = false;
                        if (isTryingToDelete)
                        {
                            reason = String.Format( @"Testing folder ""{0}"" for writeability: {1} trying to delete a test-file: {2}", folderPath, StringLib.ExceptionNameShortened( x ), x.Message );
                        }
                        else
                        {
                            if (x is Win32Exception && x.Message.Contains( "Access is denied" ) && x.Message.Contains( "Error 5 creating file" ))
                            {
                                // Shorten it just a bit for this case.
                                reason = String.Format( @"Testing folder ""{0}"" for writeability: Access is denied", folderPath );
                            }
                            else
                            {
                                reason = String.Format( @"Testing folder ""{0}"" for writeability: {1} trying to write to a test-file: {2}", folderPath, StringLib.ExceptionNameShortened( x ), x.Message );
                            }
                        }
                    }
                }
            }
            return isWriteable;
        }
        #endregion
    }
}
