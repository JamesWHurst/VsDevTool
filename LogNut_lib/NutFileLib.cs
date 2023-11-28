using System;
using System.IO;
using System.Reflection;
using System.Text;
using Hurst.LogNut.Util;


namespace Hurst.LogNut
{
    public static class NutFileLib
    {
        public static string BaseFilename { get; set; }

        public static string BaseFilenameExtension { get; set; }

        public static bool IsFileOutputFilenameUsingDate { get; set; }

        #region default file-output location

        #region GetDirectoryProgramIsExecutingIn
        /// <summary>
        /// Return the directory of this executing program, getting it from the assembly.
        /// </summary>
        /// <returns>the directory-name (of the program or assembly) as a string</returns>
        public static string GetDirectoryProgramIsExecutingIn()
        {
            // Note: This module is not implemented for the Universal Windows Platform, since access to the execution directory is blocked
            //       to all uses other than "TrustedInstaller".

            //CBL There seems to another, probably better way. Try using
            //    var dir = $"{AppDomain.CurrentDomain.BaseDirectory}"
            if (_programExecutionDir == null)
            {
                // Attempt to get it for web applications.
                var httpContext = System.Web.HttpContext.Current;
                if (httpContext != null)
                {
                    var assembly = httpContext.ApplicationInstance.GetType().BaseType.Assembly;
                    var codebase = assembly.CodeBase;
                    _programExecutionDir = FileStringLib.GetDirectoryOfPath(codebase);
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
                    _programExecutionDir = FileStringLib.GetDirectoryOfPath(location);
                }
            }
            return _programExecutionDir;
        }
        #endregion

        /// <summary>
        /// If this is non-null, then it represents the cached value that is returned
        /// by the method <see cref="GetProgramExecutionDirectory"/>.
        /// </summary>
        private static string _programExecutionDir;

        public static void Clear()
        {
            _defaultFileOutputDir = null;
        }

        #region GetDefaultFileOutputDirectory
        public static string GetDefaultFileOutputDirectory()
        {
            //CBL Is this the only place wherein this infor originates?

            if (_defaultFileOutputDir == null)
            {
                string parentDir;
#if SILVERLIGHT
                    if (!Application.Current.HasElevatedPermissions)
                    {
                        // In Visual Studio 2010, right-click on your Silverlight application and select "Properties", then choose the "Silverlight" tab, and click on the "Out of Browser Settings" button.
                        // Ensure the "Require elevated trust when running outside the browser." checkbox is checked.
                        Debug.WriteLine( "**** For LogNut to write to a file, this Silverlight application must have elevated trust. However, Application.Current.HasElevatedPermissions returns false. This will throw a SecurityException. Just saying.. ****" );
                    }
#endif

#if NETFX_CORE
                    parentFolder = Windows.Storage.ApplicationData.Current.LocalFolder.Path;
#else
                // Attempt to get it for web applications.
                if (SystemLib.IsThisAWebApplication)
                {
                    //CBL  Need to implement this
                    //CBL Test this!  Including for .NET Core on ASP.NET
                    parentDir = GetDirectoryProgramIsExecutingIn();
                }
                else
                {
                    parentDir = FilesystemLib.GetMyLocalFolderPath();
                }
#endif
                //CBL  Test this for NETFX_CORE.
                _defaultFileOutputDir = Path.Combine(parentDir, "Logs");
            }
            return _defaultFileOutputDir;
        }
        #endregion

        /// <summary>
        /// This saves the computed value for the default file-output folder.
        /// </summary>
        private static string _defaultFileOutputDir;

        #endregion default file-output location

        /// <summary>
        /// Append the current date/time to the given filename, in the form "_YYYYMMDD-HHMMSS", and then append the given extension onto it.
        /// This is a helper method for forming file-output filenames.
        /// </summary>
        /// <param name="originalFilenameWithoutExtension">the 'base' filename, WITHOUT the extension, to add the timestamp to</param>
        /// <param name="extension">the filename-extension to add onto the end</param>
        /// <returns>the new file-output filename, reflecting the current date-and-time</returns>
        public static string CreateLogOutputFilenameWithDate( string originalFilenameWithoutExtension, string extension )
        {
            IsFileOutputFilenameUsingDate = true;
            BaseFilename = originalFilenameWithoutExtension;
            BaseFilenameExtension = extension;
            // GTSetup_Log_YYYYMMDD-HHMMSS.txt
            var sb = new StringBuilder(originalFilenameWithoutExtension);
            sb.Append( "_" );

            DateTime now = DateTime.Now;
            string text = String.Format( "{0:yyyy-MM-dd HH:mm:ss}", now );
            // Replace the space with an underscore, and remove any colons.
            string whenPart = text.Replace( " ", "_" ).Replace( ":", "" );
            sb.Append( whenPart );
            // Append the extension.
            if (extension[0] != '.')
            {
                sb.Append( "." );
            }
            sb.Append( extension );
            return sb.ToString();
        }

        public static string RecreateFileOutputFilename()
        {
            var sb = new StringBuilder(BaseFilename);
            sb.Append( "_" );

            DateTime now = DateTime.Now;
            string text = String.Format( "{0:yyyy-MM-dd HH:mm:ss}", now );
            // Relace the space with an underscore, and remove any colons.
            string whenPart = text.Replace( " ", "_" ).Replace( ":", "" );
            sb.Append( whenPart );
            // Append the extension.
            if (BaseFilenameExtension[0] != '.')
            {
                sb.Append( "." );
            }
            sb.Append( BaseFilenameExtension );
            return sb.ToString();
        }

        #region MoveLogsToLogReceiverIfPresent
        /// <summary>
        /// Shove the existing log-files over to an attached removable-drive that qualifies as a 'log-reciever', if present.
        /// </summary>
        /// <param name="subdirectoryForPlacingLogOutput"></param>
        /// <returns>true if this does find that a 'log-reciever' is present</returns>
        public static bool MoveLogsToLogReceiverIfPresent( string normalFileOutputDir, string subdirectoryForPlacingLogOutput )
        {
            //CBL The only thing this fails to do that the method in LogManager does, is call NutUtil.CloseTheOutputFile.
            bool isLogRcvrPresent = false;
            const string keyFile = "gLogOutput.gtk";
            foreach (DriveInfo drive in DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Removable)
                {
                    string rootDir = drive.RootDirectory.FullName;
                    string logOutputKeyFile = Path.Combine(rootDir, keyFile);
                    if (File.Exists( logOutputKeyFile ))
                    {
                        // Found the log-receiver drive.
                        isLogRcvrPresent = true;

                        //LogManager.RemovableDrive = rootDir;
                        string destinationDir = rootDir;
                        if (subdirectoryForPlacingLogOutput != null)
                        {
                            destinationDir = Path.Combine( rootDir, subdirectoryForPlacingLogOutput );
                        }
                        else
                        {
                            destinationDir = rootDir;
                        }

                        if (!Directory.Exists( destinationDir ))
                        {
                            Directory.CreateDirectory( destinationDir );
                        }

                        // NutUtil.CloseTheOutputFile();

                        FilesystemLib.MoveDirectoryContent( sourceDirectory: normalFileOutputDir,
                                                            destinationParentDirectory: destinationDir,
                                                            fileMatchExpression: null,
                                                            isToRolloverExistingDestinFile: true );
                        // Also get anything that's in here..
                        string userLogDir = @"C:\Users\LuVivaSystem\Documents\Logs";
                        if (FilesystemLib.HasContent( userLogDir ))
                        {
                            string destinationUserLogsDir = Path.Combine( destinationDir, "UsersLogs" );
                            if (!Directory.Exists( destinationUserLogsDir ))
                            {
                                Directory.CreateDirectory( destinationUserLogsDir );
                            }
                            FilesystemLib.MoveDirectoryContent( sourceDirectory: userLogDir,
                                                                destinationParentDirectory: destinationUserLogsDir,
                                                                fileMatchExpression: null,
                                                                isToRolloverExistingDestinFile: true );
                        }
                        break;
                    }
                }
            }
            return isLogRcvrPresent;
        }
        #endregion
    }
}
