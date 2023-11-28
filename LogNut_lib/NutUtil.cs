using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Hurst.LogNut.Util.Annotations;
using Hurst.LogNut.Util;
using Hurst.LogNut.Util.Native;
using Microsoft.Win32.SafeHandles;

#if USE_IONIC_ZIP_COMPRESSION
using Ionic.Zip;
#endif
#if INCLUDE_JSON
using Newtonsoft.Json;
#endif


// define PRE_4 if this is to be compiled for versions of .NET earlier than 4.0
// define INCLUDE_JSON for this project if you want to produce JSON output.
// define USE_IONIC_ZIP_COMPRESSION for this project if you want to use ZIP-compression using the Ionic library.


namespace Hurst.LogNut
{
    #region Doc-stuff
    /// <summary>
    /// The <c>Hurst.LogNut</c> namespace encompasses the LogNut libraries and attendant utility software.
    /// </summary>
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute]
    class NamespaceDoc
    {
        // This class exists only to provide a hook for adding summary documentation
        // for the LogNut namespace, when using the Sandcastle Help File Builder.
        // See  https://ewsoftware.github.io/SHFB/html/48f5a893-acde-4e50-8c17-72b83d9c3f9d.htm
    }
    #endregion

    /// <summary>
    /// A class to contain misc LogNut utility methods.
    /// </summary>
    public static class NutUtil
    {
        public static void WriteTextToFileAndKeepOpen( string pathname, string text, bool doesFileExist, bool isToAppend )
        {
            //CBL Append, or truncate and start a new file?
            //CBL File exists, or are we creating a new one?

            // Possible cases:
            // 1. Open the file, truncating it if it already exists.
            // 2. Open the file, and seeking to the end of it in order to start appending to it.
            // 3. We are simply appending to an already-open file.

            if (pathnameToWriteTo == null)
            {
                pathnameToWriteTo = pathname;
            }
            // We are not intending to handle exceptions here. This try-catch block is intended
            // to ensure that, if a write to the file fails,
            // that the ivars are reset to null so that they are recreated next time.
            try
            {
                if (_safeFileHandle == null)
                {
                    CreationDisposition creationDisposition;
                    if (doesFileExist)
                    {
                        if (isToAppend)
                        {
                            creationDisposition = CreationDisposition.OpenAlways;
                        }
                        else
                        {
                            creationDisposition = CreationDisposition.TruncateExisting;
                        }
                    }
                    else // new file
                    {
                        creationDisposition = CreationDisposition.CreateAlways;
                        //creationDisposition = CreationDisposition.OpenExisting;
                    }

                    _safeFileHandle = FilesystemLib.CreateFileHandle( pathname,
                                                                      creationDisposition,
                                                                      Util.Native.FileAccess.GenericWrite,
                                                                      Util.Native.FileShare.None,
                                                                      NativeFileAttributes.RandomAccess );
                    //CBL Should I change this to default?
                    _encoding = new UTF8Encoding( false, true );
                    if (!_safeFileHandle.IsInvalid)
                    {
                        if (isToAppend)
                        {
                            // Seek to the end of the file, if we want to append to it..
                            FilesystemLib.Seek( _safeFileHandle, 0, SeekOrigin.End );
                        }
                        _fileStream = new FileStream( _safeFileHandle, System.IO.FileAccess.Write );
                        _streamWriter = new StreamWriter( _fileStream, _encoding );
                    }

                }

                if (!_safeFileHandle.IsInvalid)
                {
                    _streamWriter.Write( text );
                }
                else
                {
                    throw new FileNotFoundException( message: "The file-handle for this file seems to be invalid.", fileName: pathname );
                }
            }
            catch (Exception)
            {
                CloseTheOutputFile();
                throw;
            }
        }

        private static string pathnameToWriteTo;
        private static SafeFileHandle _safeFileHandle;
        private static UTF8Encoding _encoding;
        private static FileStream _fileStream;
        private static StreamWriter _streamWriter;

        public static void CloseTheOutputFile()
        {
            if (_streamWriter != null)
            {
                _streamWriter.Flush();
                _streamWriter.Close();
                _streamWriter.Dispose();
                _streamWriter = null;
            }
            if (_fileStream != null)
            {
                _fileStream.Dispose();
                _fileStream = null;
            }
            if (_safeFileHandle != null)
            {
                if (!_safeFileHandle.IsInvalid && !_safeFileHandle.IsClosed)
                {
                    _safeFileHandle.Close();
                }
                _safeFileHandle.Dispose();
                // Set to null so that other parts of the program may test for it.
                _safeFileHandle = null;
            }
            pathnameToWriteTo = null;
        }

        #region SetLogOutputToLogRcvrFlashDriveIfPresent
        /// <summary>
        /// Look for any removable-drive that contains a file named gLogOutput.gtk and, if found,
        /// set that as the secondary log-output destination. Return false if no such drive is found.
        /// </summary>
        /// <param name="subdirectoryForPlacingLogOutput">This denotes which sub-directory on that flash-drive to place the log-output into. Leaving this null means to put it into the root directory.</param>
        /// <returns>true if the log-receiver flash-drive was detected and used</returns>
        public static string GetLogOutputToFlashDriveIfPresent( string subdirectoryForPlacingLogOutput )
        {
            string logDirectory = null;
            DriveInfo[] thumbDrives = FilesystemLib.GetRemovableDrives();
            foreach (var drive in thumbDrives)
            {
                string rootDir = drive.RootDirectory.FullName;
                // Found the log-receiver drive.
                logDirectory = rootDir;
                if (subdirectoryForPlacingLogOutput != null)
                {
                    logDirectory = Path.Combine( rootDir, subdirectoryForPlacingLogOutput );
                }
                //LogManager.Config.SetFileOutputFolder( logDirectory );
                break;
            }
            return logDirectory;
        }
        #endregion

        #region Compress
        internal static byte[] Compress( byte[] raw )
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream( memoryStream, CompressionMode.Compress, true ))
                {
                    gzip.Write( raw, 0, raw.Length );
                }
                return memoryStream.ToArray();
            }
        }
        #endregion

        #region CreateOutputHeader
        /// <summary>
        /// INTERNAL-USE ONLY. A helper-method used within method 'Send' to compose the logging-header that would be written
        /// upon the first log to a file.
        /// </summary>
        /// <param name="logRecord">the log-record to be logged</param>
        /// <param name="config">the LogConfig that provides configuration-settings that dictate how it gets rendered</param>
        /// <returns>the file-output header</returns>
        /// <remarks>
        /// This method really should be marked internal instead of public, but for some reason it was not being visible to the test-project.
        /// </remarks>
        public static string CreateOutputHeader( LogRecord logRecord, LogConfig config )
        {
            //cbl Does this work as internal ?
            var sb = new StringBuilder();
            var sbHeader = new StringBuilder();
            bool beSpreadsheetCompatible = config.IsFileOutputSpreadsheetCompatible;
            string prefix = null;
            if (beSpreadsheetCompatible)
            {
                // Set the level to Infomation temporarily, so that the prefix reflects a level of only Infomation.
                LogLevel originalLevel = logRecord.Level;
                logRecord.Level = LogLevel.Infomation;
                prefix = LogManager.LogRecordFormatter.GetPrefix( logRecord, config );
                logRecord.Level = originalLevel;
            }

            // The banner-text is of this format:
            // ----[ First log upon start of {program-name} process-id n version {program-version} on Friday, 2011-05-20 5:27:48 ]----

            // Add the "--[ First log upon start" .
            sbHeader.Append( BannerTextPrefix );

            string programName = config.GetSubjectProgramName( null );
            string processInformation = " process-id " + Process.GetCurrentProcess().Id;
            if (StringLib.HasSomething( programName ))
            {
                sbHeader.Append( "of " ).Append( programName );
                sbHeader.Append( processInformation );
                sbHeader.Append( " version " ).Append( config.SubjectProgramVersion );
            }
            else
            {
                sbHeader.Append( "(no subject-program name)" );
                sbHeader.Append( processInformation );
            }
            // when
            var now = DateTime.Now;
            sbHeader.Append( " on " ).Append( LogManager.LogRecordFormatter.GetTimeStamp( now, null, false ) );

            // by whom
            if (StringLib.HasSomething( config.Username ))
            {
                sbHeader.Append( " by " );
                sbHeader.Append( config.Username );
            }

            // Add any additional information that this has been configured for.
            if (!String.IsNullOrEmpty( config.FileOutputAdditionalHeaderText ))
            {
                sbHeader.Append( "  " ).Append( config.FileOutputAdditionalHeaderText );
            }
            // Do not append the banner-suffix if we had additional header info,
            // because that might have been multi-line, and we don't want to wind up with a dangling banner-suffix.
            sbHeader.Append( BannerTextSuffix );
            if (config.IsFileOutputToUseStdTerminator)
            {
                sbHeader.Append( LogManager.FileRecordSeparator );
            }
            sbHeader.AppendLine();

            if (config.FileOutputAdditionalHeaderLines != null)
            {
                sbHeader.Append( config.FileOutputAdditionalHeaderLines );
            }

            if (!beSpreadsheetCompatible)
            {
                if (config.IsFileOutputToInsertLineBetweenTraces)
                {
                    sbHeader.AppendLine();
                }
            }

            // Prepend a prefix for every line of text, if Excel-compatibility is called for.
            if (beSpreadsheetCompatible)
            {
                var lines = Regex.Split( sbHeader.ToString(), "\r\n|\r|\n" );
                foreach (string lineOfText in lines)
                {
                    sb.Append( prefix );
                    sb.Append( "\t" );
                    sb.Append( lineOfText );
                    sb.AppendLine();
                }
            }
            else
            {
                sb.Append( sbHeader );
            }
            return sb.ToString();
        }
        #endregion CreateOutputHeader

        #region FindWorkingFileOutputFolder
        /// <summary>
        /// Attempt to verify that logging-output can be written to either of the given folders, trying the first, and then the second
        /// only if the first is unsuccessful, and then trying other locations and, if a useable folder is found - return that path.
        /// </summary>
        /// <param name="pathTester">an <see cref="IAccessibilityEvaluator"/> object to use for its <c>CanWriteTo</c> method</param>
        /// <param name="firstPathToTry">the first folder to try. This may be null.</param>
        /// <param name="secondPathToTry">the second folder to try. This too may be null.</param>
        /// <param name="descriptionOfPathChosen">description of the path that is finally selected</param>
        /// <param name="reason">a cumulative list of errors that results from each failed folder-test, or else null or an empty-string if no failures</param>
        /// <returns>a string denoting the folder that does work, or null if none do</returns>
        /// <remarks>
        /// This tries several folders in succession, trying to find one that we can write to.
        /// 
        /// 1. First, it tries <paramref name="firstPathToTry"/> if that is non-null,
        /// 2. and then <paramref name="secondPathToTry"/> if *that* is non-null,
        /// 3. After that, the default-location as dictated by the property <see cref="LogManager.FileOutputFolder_DefaultValue"/>,
        /// 4. and then the subject-program's execution folder is attempted (if not Universal Windows Platform),
        /// 5. then the local-application-data folder,
        /// 6. and then finally the Public Documents folder is tested.
        /// 
        /// This method does not change the value of the <see cref="LogConfig.FileOutputFolder"/> property, however it does have side-effects
        /// in that it may create a folder in order to ascertain whether that is a writable location.
        /// </remarks>
        public static string FindWorkingFileOutputFolder( IAccessibilityEvaluator pathTester,
                                                          string firstPathToTry,
                                                          string secondPathToTry,
                                                          out string descriptionOfPathChosen,
                                                          out string reason )
        {
            if (firstPathToTry == null && secondPathToTry != null)
            {
                throw new ArgumentException( "firstPathToTry must not be null if secondPathToTry is other than null." );
            }
            reason = null;
            string reasonForFailure;
            StringBuilder sbReport = new StringBuilder();

            if (firstPathToTry != null)
            {
                bool isValid = pathTester.CanWriteTo( firstPathToTry, out reasonForFailure );
                if (isValid)
                {
                    descriptionOfPathChosen = "First choice";
                    return firstPathToTry;
                }
                sbReport.Append( reasonForFailure );

                if (secondPathToTry != null)
                {
                    isValid = pathTester.CanWriteTo( secondPathToTry, out reasonForFailure );
                    if (isValid)
                    {
                        descriptionOfPathChosen = "Second choice";
                        reason = sbReport.ToString();
                        return secondPathToTry;
                    }
                    sbReport.AppendLine().Append( reasonForFailure );
                }
            }

            // Try the default location, which is the user's Documents folder - specifically a "Logs" folder within that, IF this is not a web application.
            if (!SystemLib.IsThisAWebApplication)
            {
                // not a web-app
                string documentsLogsFolder = LogManager.FileOutputFolder_DefaultValue;
                bool isDocumentsFolderValid = pathTester.CanWriteTo( documentsLogsFolder, out reasonForFailure );
                if (isDocumentsFolderValid)
                {
                    descriptionOfPathChosen = "Logs folder within the user's Documents folder";
                    if (sbReport.Length > 0)
                    {
                        reason = sbReport.ToString();
                    }
                    return documentsLogsFolder;
                }
                if (sbReport.Length > 0)
                {
                    sbReport.AppendLine();
                }
                sbReport.Append( reasonForFailure );
                //CBL  Need to test this for web-applications as well.
            }

            //CBL ALL of these locations need to be checked to see whether they apply for web applications!
            //CBL Does this next apply for UWP applications?
#if !NETFX_CORE
            // Try the local data folder.
            // On Windows 8, 8.1, and 10 this is C:\Users\{username}\AppData\Local. I here append "Logs".
            string localDataFolder = Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.LocalApplicationData ), "Logs" );
            bool isLocalDataFolderValid = pathTester.CanWriteTo( localDataFolder, out reasonForFailure );
            if (isLocalDataFolderValid)
            {
                descriptionOfPathChosen = "Logs folder within the local-application-data folder";
                reason = sbReport.ToString();
                return localDataFolder;
            }
            if (sbReport.Length > 0)
            {
                sbReport.AppendLine();
            }
            sbReport.Append( reasonForFailure );
#endif

            // Finally, try the public documents folder.
            // This should give you something like C:\Users\Public\Documents.  I here append "Logs".
            string publicFolder = Path.Combine( FilesystemLib.GetTheSharedLocalFolderPath(), "Logs" );
            bool isPublicFolderValid = pathTester.CanWriteTo( publicFolder, out reasonForFailure );
            if (isPublicFolderValid)
            {
                descriptionOfPathChosen = "Logs folder within the Public Documents folder";
                reason = sbReport.ToString();
                return publicFolder;
            }
            sbReport.AppendLine().Append( reasonForFailure );

            // If we reach this point, then none of the folders we tried worked. So just say "fuckit" and return null.
            reason = sbReport.ToString();
            descriptionOfPathChosen = null;
            return null;
        }
        #endregion FindWorkingFileOutputFolder

        #region GetLogRecordsFromFile
        /// <summary>
        /// Get the log records from the given file and return them as a List of LogRecords.
        /// </summary>
        /// <param name="logFilePath">the filesystem-path to the logfile to read</param>
        /// <returns>a List of the LogRecords, or an empty List if the file is not found or else fails to contain any records</returns>
        public static List<LogRecord> GetLogRecordsFromFile( string logFilePath )
        {
            if (logFilePath == null)
            {
                throw new ArgumentNullException( "logFilePath" );
            }
            var zFileInfo = new ZFileInfo( logFilePath );
            if (!zFileInfo.Exists)
            {
                throw new FileNotFoundException( "Cannot get log-records - that file is not found.", logFilePath );
            }

            var theList = new List<LogRecord>();
            bool dummyResult;
            ProcessRecordsInFile(
                zFileInfo,
                ( eachRecord ) =>
                {
                    theList.Add( eachRecord );
                    return false;
                },
                out dummyResult );
            return theList;
        }
        #endregion

        #region HasRecordThatContains

        //CBL: Make this next method NOT read all of the text at once, but rather to sequentially read through it only as far as it needs to.

        /// <summary>
        /// Given a path to a log-file, return true if that log-file contains the given text.
        /// </summary>
        /// <param name="logFilePath">the pathname to the log-file to check the content of</param>
        /// <param name="text">the text string to test for, anywhere within that log-file</param>
        /// <returns>true if the given log-file contains the given text</returns>
        public static bool HasRecordThatContains( string logFilePath, string text )
        {
            bool isTextFound;
            var zFileInfo = new ZFileInfo( logFilePath );
            ProcessRecordsInFile( zFileInfo, ( eachRecord ) => { return eachRecord.Message.Contains( text ); }, out isTextFound );
            return isTextFound;
        }
        #endregion HasRecordThatContains

        #region MagicallyCreateTestRecord
        /// <summary>
        /// Return a new LogRecord based upon the given text-message, and also as a function of a previous-existing LogRecord.
        /// </summary>
        /// <param name="message">the text to use for the content of the LogRecord (the Message property) - if null then take it from previousRecord</param>
        /// <param name="previousRecord">if non-null, then most of the properties of the new LogRecord are taken from this</param>
        /// <param name="isShownAsBeingInDesignMode">this dictates whether to make the LogRecord think it was in design-mode</param>
        /// <param name="logLevel">if this nullable-LogLevel is other than null, it dictates which LogLevel to give the new log-record</param>
        /// <returns>a new <c>LogRecord</c> with property-values either from previousRecord, or if that is null - give it sample value</returns>
        public static LogRecord MagicallyCreateTestRecord( string message, LogRecord previousRecord, bool isShownAsBeingInDesignMode, LogLevel? logLevel )
        {
            LogRecord newRecord;
            LogLevel level;

            if (previousRecord == null)
            {
                if (logLevel == null)
                {
                    level = default( LogLevel );
                }
                else
                {
                    level = logLevel.Value;
                }
                LogCategory cat = LogCategory.MethodTrace;
#if !PRE_4
                newRecord = new LogRecord( id: null,
                                          message: message,
                                          level: level,
                                          cat: cat,
                                          when: DateTime.Now,
                                          sourceHost: "SourceHost",
                                          sourceLogger: "LoggerName",
                                          subjectProgramName: "ProgramName",
                                          subjectProgramVersion: "1.0.0.x",
                                          threadId: SystemLib.CurrentThreadId,
                                          user: SystemLib.Username,
                                          isInDesignMode: isShownAsBeingInDesignMode );
#else
                newRecord = new LogRecord( null,
                                          message,
                                          level,
                                          cat,
                                          DateTime.Now,
                                          "SourceHost",
                                          "LoggerName",
                                          "ProgramName",
                                          "1.0.0.x",
                                          SystemLib.CurrentThreadId,
                                          SystemLib.Username,
                                          isShownAsBeingInDesignMode );
#endif
            }
            else // there is a previousRecord
            {
                if (logLevel == null)
                {
                    // Cycle through the possible values of LogLevel..
                    switch (previousRecord.Level)
                    {
                        case LogLevel.Trace:
                            level = LogLevel.Debug;
                            break;
                        case LogLevel.Debug:
                            level = LogLevel.Infomation;
                            break;
                        case LogLevel.Infomation:
                            level = LogLevel.Warning;
                            break;
                        case LogLevel.Warning:
                            level = LogLevel.Error;
                            break;
                        case LogLevel.Error:
                            level = LogLevel.Critical;
                            break;
                        default:
                            level = LogLevel.Trace;
                            break;
                    }
                }
                else
                {
                    level = logLevel.Value;
                }

                // If no explicit message text was provided, use the one from the previous record.
                if (StringLib.HasNothing( message ))
                {
                    message = previousRecord.Message;
                }
                // We will let the When property vary as a function of the actual time,
                // and cycle level amongst the full range of possible values,
                // but the other properties we'll copy from the previousRecord.
#if !PRE_4
                newRecord = new LogRecord( id: null,
                                          message: message,
                                          level: level,
                                          cat: previousRecord.Category,
                                          when: DateTime.Now,
                                          sourceHost: previousRecord.SourceHost,
                                          sourceLogger: previousRecord.SourceLogger,
                                          subjectProgramName: previousRecord.SubjectProgramName,
                                          subjectProgramVersion: previousRecord.SubjectProgramVersion,
                                          threadId: previousRecord.ThreadId,
                                          user: previousRecord.Username,
                                          isInDesignMode: previousRecord.IsInDesignMode );
#else
                newRecord = new LogRecord( null,
                                          message,
                                          level,
                                          previousRecord.Category,
                                          DateTime.Now,
                                          previousRecord.SourceHost,
                                          previousRecord.SourceLogger,
                                          previousRecord.SubjectProgramName,
                                          previousRecord.SubjectProgramVersion,
                                          previousRecord.ThreadId,
                                          previousRecord.Username,
                                          previousRecord.IsInDesignMode );
#endif
            }
            return newRecord;
        }
        #endregion

        #region PostProcessLogFile
        /// <summary>
        /// Perform any processing that needs to be done on a log-output file after we have rolled it over to a new file,
        /// such as archiving or compression.
        /// </summary>
        /// <param name="pathname">the filesystem path of the rolled-over log-file to process</param>
        internal static void PostProcessLogFile( string pathname )
        {
            //CBL: This does not currently work. The rollover mechanism relies upon the filename being a certain way: if we add .zip then it won't find them.
            //      Should we add an option to combine all of the log files into one single ZIP file?
            // Need to fix!
            // and also, need to allow for long file pathnames here.
            // http://dotnetzip.codeplex.com/

            //CBL Temp, just to see what's happening..
            //NutUtil.WriteToConsole(String.Format("PostProcessLogFile({0})", pathname));
            //CBL Why do I need the Ionic.Zip library for this?
            //#if USE_IONIC_ZIP_COMPRESSION
            if (LogManager.Config.IsFileOutputToCompressFiles)
            {
                try
                {
                    // For the new filename, replace the ".log" extension with a ".zip".
                    string newPathname = FileStringLib.PathnameWithoutExtension(pathname) + ".zip";
                    string baseFilename = FileStringLib.GetFileNameFromFilePath( pathname );
                    //var z = System.IO.Compression.
                    //using (var zip = new ZipFile())
                    //{
                    //    zip.AddFile( pathname ).FileName = baseFilename;
                    //    zip.Save( newPathname );
                    //}

                    string fileContent = FilesystemLib.ReadAllText( pathname );
                    byte[] rawBytes = Encoding.ASCII.GetBytes( fileContent );
                    byte[] compressedBytes = Compress( (rawBytes) );
                    File.WriteAllBytes( newPathname, compressedBytes );

                    // Remove the original, uncompressed file.
                    FilesystemLib.DeleteFile( pathname );
                }
                catch (Exception x)
                {
                    LogManager.HandleInternalFault( x, null, "In PostProcessLogFile({0}) attempting to compress the archive file.", pathname );
                }
            }
        }
        #endregion

        #region ProcessRecordsInFile
        /// <summary>
        /// Read all of the text from the given file, and - if it happens to consist of properly-formatted log-record traces,
        /// perform the given action upon them.
        /// Note: This does not work if the standard <see cref="LogManager.FileRecordSeparator"/> character is not appended to each record.
        /// </summary>
        /// <param name="zFileInfo">the file to retrieve textual evidence of the log-records from</param>
        /// <param name="continuationTest">a predicate to execute for each record. When this evaluates to true - processing of records stops and result is set to true.</param>
        /// <param name="result">this is set to true if continuationTest ever evaluates to true</param>
        public static void ProcessRecordsInFile( ZFileInfo zFileInfo,
                                                 System.Predicate<LogRecord> continuationTest,
                                                 out bool result )
        {
            result = false;

            string firstBitOfText = zFileInfo.ReadText( 10 );
            if (firstBitOfText.Contains( @"{""" ))
            {
                // It is JSON.
#if INCLUDE_JSON
                string propertyName = null;
                List<string> listOfStringsRepresentingRecords = zFileInfo.ReadTextAsListOfRecords('}');
                foreach (string thisRecordText in listOfStringsRepresentingRecords)
                {
                    var newRecord = new LogRecord();
                    JsonTextReader reader = new JsonTextReader(new StringReader(thisRecordText));
                    while (reader.Read())
                    {
                        switch (reader.TokenType)
                        {
                            case JsonToken.PropertyName:
                                object value = reader.Value;
                                propertyName = (string)value;
                                break;
                            case JsonToken.Integer:
                                if (propertyName == "Level")
                                {
                                    Int64 numericValue = (Int64)reader.Value;
                                    int levelInteger = (int)numericValue;
                                    newRecord.Level = (LogLevel)levelInteger;
                                }
                                break;
                            case JsonToken.String:
                                string stringValue = (string)reader.Value;
                                switch (propertyName)
                                {
                                    case "Id":
                                        newRecord.Id = stringValue;
                                        break;
                                    case "Message":
                                        newRecord.Message = stringValue;
                                        break;
                                    case "SourceHost":
                                        newRecord.SourceHost = stringValue;
                                        break;
                                    case "SourceLogger":
                                        newRecord.SourceLogger = stringValue;
                                        break;
                                    case "SubjectProgramName":
                                        newRecord.SubjectProgramName = stringValue;
                                        break;
                                    case "SubjectProgramVersion":
                                        newRecord.SubjectProgramVersion = stringValue;
                                        break;
                                    case "Username":
                                        newRecord.Username = stringValue;
                                        break;
                                    default:
                                        Debug.WriteLine(
                                            String.Format(
                                                "in NutUtil.ProcessRecordsInFile, unexpected propertyName: {0}",
                                                propertyName));
                                        break;
                                }
                                break;
                            case JsonToken.Boolean:
                                bool booleanValue = (bool)reader.Value;
                                if (propertyName == "IsInDesignMode")
                                {
                                    newRecord.IsInDesignMode = booleanValue;
                                }
                                else
                                {
                                    Debug.WriteLine(String.Format("in NutUtil.ProcessRecordsInFile, got TokenType of Boolean but expected propertyName IsInDesignMode, not {0}", propertyName));
                                }
                                break;
                            case JsonToken.Date:
                                DateTime t = (DateTime)reader.Value;
                                newRecord.When = t;
                                break;
                            case JsonToken.StartObject:
                                break;
                            case JsonToken.Null:
                                break;
                            default:
                                {
                                    Debug.WriteLine(
                                        String.Format("in NutUtil.ProcessRecordsInFile, unexpected TokenType: {0}",
                                                      reader.TokenType));
                                    break;
                                }
                        }
                    }

                    // If there was an action to perform on this record, do that..
                    if (continuationTest != null)
                    {
                        if (continuationTest(newRecord))
                        {
                            result = true;
                            // break;
                        }
                    }
                }
#else
                throw new NotImplementedException( "JSON is not available unless you compile this code with the INCLUDE_JSON pragma." );
#endif
            }
            else if (firstBitOfText.Contains( @"<LogRecord" ))
            {
                // It is XML.
                XmlSerializer deserializer = new XmlSerializer( typeof( LogRecord ) );
                List<string> listOfStringsRepresentingRecords = zFileInfo.ReadTextAsListOfRecords( "</LogRecord>" );
                foreach (string thisRecordText in listOfStringsRepresentingRecords)
                {
                    // Take out the When field first. For some reason XmlSerializer has trouble with that.
                    string textOfWhen = null;
                    string s;
                    LogRecord newRecord = null;
                    if (thisRecordText.Contains( "When=" ))
                    {
                        int indexOfWhen = thisRecordText.IndexOf( "When=" );
                        int indexOfEndOfWhen = thisRecordText.IndexOf( "\"", indexOfWhen + 6 );
                        int lengthOfWhenAttrib = indexOfEndOfWhen - indexOfWhen;
                        textOfWhen = thisRecordText.Substring( indexOfWhen + 6, lengthOfWhenAttrib - 6 );
                        s = thisRecordText.Substring( 0, indexOfWhen ) + thisRecordText.Substring( indexOfEndOfWhen + 1 );
                    }
                    else
                    {
                        s = thisRecordText;
                    }
                    using (StringReader reader = new StringReader( s ))
                    {
                        object obj = deserializer.Deserialize( reader );
                        newRecord = (LogRecord)obj;
                    }
                    // Put When back in.
                    if (textOfWhen != null)
                    {
                        DateTime t;
                        if (DateTime.TryParse( textOfWhen, out t ))
                        {
                            newRecord.When = t;
                        }
                        else
                        {
                            WriteToConsole( "Invalid When text: " + textOfWhen );
                        }
                    }

                    // If there was an action to perform on this record, do that..
                    if (continuationTest != null)
                    {
                        if (continuationTest( newRecord ))
                        {
                            result = true;
                            break;
                        }
                    }
                }

                //string s =
                //    "<LogRecord When=\"2015-04-05 4:05:06\" Host=\"OBSIDIAN\" Prog=\"Hurst.LogNut\" Level=\"Debug\"><Message>Red.</Message></LogRecord>";


            }
            else // Assume it is SimpleText.
            {
                //CBL I need to filter out the banner-text here!

                List<string> listOfStringsRepresentingRecords = zFileInfo.ReadTextAsListOfRecords( LogManager.FileRecordSeparator );
                foreach (string thisRecordText in listOfStringsRepresentingRecords)
                {
                    // Get rid of any leading end-of-line characters.
                    string recordText = thisRecordText.WithoutLeadingLineEndCharacters();

                    // Is there a file-truncation line stuck on the beginning of this?
                    if (recordText.StartsWith( LogManager.TruncationPrefix ))
                    {
                        //cbl What about when multiple truncation-texts and banner-texts are found?
                        int indexOfEndOfTruncation = recordText.IndexOf( LogManager.TruncationSuffix ) + LogManager.TruncationSuffix.Length;
                        recordText = thisRecordText.Substring( indexOfEndOfTruncation );
                    }

                    // Remove any leading banner-text..
                    if (recordText.Contains( BannerTextPrefix ))
                    {
                        // Just skip the banner-part.
                        continue;
                        //int indexOfEndOfBanner = recordText.IndexOf( BannerTextSuffix ) + BannerTextSuffix.Length;
                        //recordText = thisRecordText.Substring( indexOfEndOfBanner );
                    }

                    // Parse the remaining text into a LogRecord.
                    LogRecord newRecord = LogRecord.FromText( recordText );

                    // If a log-record did indeed result from that line of text (which it may not have - could be a banner-text or something),
                    if (newRecord != null)
                    {
                        // If there was an action to perform on this record, do that..
                        if (continuationTest != null)
                        {
                            if (continuationTest( newRecord ))
                            {
                                result = true;
                                break;
                            }
                        }
                    }
                }
            }
        }
        #endregion ProcessRecordsInFile

        #region RenderFileOutput( LogRecord logRecord, LogConfig config, bool isFirstFileOutput )
        /// <summary>
        /// This produces the text of a log-record for LogNut's file output.
        /// </summary>
        /// <param name="logRecord">the log-record that is being rendered</param>
        /// <param name="config">the LogConfig that provides configuration-settings that dictate how it gets rendered</param>
        /// <param name="isFirstFileOutput">this should be set true if this is the first time output is being written</param>
        /// <returns>the string to write to the log file</returns>
        public static string RenderFileOutput( LogRecord logRecord, LogConfig config, bool isFirstFileOutput )
        {
            //CBL This had been 'internal static' until I moved EtwOutput to it's own project. Does this need to be public tho?

            string sRecord;

            switch (config.FileOutputFormatType)
            {
                case LogFileFormatType.Xml:
                    sRecord = RenderAsXML( logRecord, config: config );
                    break;
                case LogFileFormatType.Json:
#if INCLUDE_JSON
                        sRecord = JsonConvert.SerializeObject(logRecord, JsonSerializerSettings);
                        break;
#else
                    throw new NotImplementedException( "JSON is not available unless you compile this code with the INCLUDE_JSON pragma." );
#endif
                default:
                    bool beSpreadsheetCompatible = config.IsFileOutputSpreadsheetCompatible;
                    var sb = new StringBuilder();
                    // Put a special "banner" timestamp line if this is the first log on this program run,
                    // if that option is on.
                    if (isFirstFileOutput == true)
                    {
                        // For spreadsheet outputs, put a line of column-header fields for the top row.
                        if (beSpreadsheetCompatible)
                        {
                            // CBL This needs to be implemented and tested.
                            sb.Append( "Timestamp\tDelta\tThread   \tLogger\tLevel\tContent" );
                            sb.AppendLine();
                        }
                        if (config.IsFileOutputToInsertHeader)
                        {
                            sb.Append( CreateOutputHeader( logRecord, config ) );
                        }
                    }

                    // Put the actual message, and then cap off the end with a special symbol.

                    string s = logRecord.AsText( config );

                    sb.Append( s );

                    //sb.Append(logRecord.AsText(Config)); //CBL

                    if (!beSpreadsheetCompatible)
                    {
                        if (config.IsFileOutputToUseStdTerminator)
                        {
                            sb.Append( LogManager.FileRecordSeparator );
                        }
                    }
                    //CBL
                    //else
                    //{
                    //    sb.AppendLine();
                    //}
                    // Append a new-line after each log-record, so that when the log-file is opened
                    // in an editor each starts at the beginning of a new line.
                    sb.AppendLine();
                    // Separate log traces with an empty line (if that option is on).
                    if (!beSpreadsheetCompatible)
                    {
                        if (config.IsFileOutputToInsertLineBetweenTraces)
                        {
                            sb.AppendLine();
                        }
                    }
                    sRecord = sb.ToString();
                    break;
            }
            return sRecord;
        }
        #endregion RenderFileOutput

        #region RenderFileOutput( LogSendRequest logSendRequest, LogConfig config, bool isFirstFileOutput )
        /// <summary>
        /// This produces the text of a log-record for LogNut's file output.
        /// </summary>
        /// <param name="logSendRequest">the log to be rendered</param>
        /// <param name="config">the LogConfig that provides configuration-settings that dictate how it gets rendered</param>
        /// <param name="isFirstFileOutput">this should be set true if this is the first time output is being written</param>
        /// <returns>the string to write to the log file</returns>
        internal static string RenderFileOutput( LogSendRequest logSendRequest, LogConfig config, bool isFirstFileOutput )
        {
            LogRecord logRecord = logSendRequest.Record;

            var sb = new StringBuilder();
            // Put a special "banner" timestamp line if this is the first log on this program run,
            // if that option is on.
            if (isFirstFileOutput == true)
            {
                // For spreadsheet outputs, put a line of column-header fields for the top row.
                if (config.IsFileOutputToInsertHeader)
                {
                    sb.Append( CreateOutputHeader( logRecord, config ) );
                }
            }

            // Put the actual message, and then cap off the end with a special symbol.

            string s = logRecord.AsText( config );
            sb.Append( s );

            if (config.IsFileOutputToUseStdTerminator)
            {
                sb.Append( LogManager.FileRecordSeparator );
            }
            sb.AppendLine();
            // Separate log traces with an empty line (if that option is on).
            if (config.IsFileOutputToInsertLineBetweenTraces)
            {
                sb.AppendLine();
            }
            return sb.ToString();
        }
        #endregion RenderFileOutput

        #region RenderAsXML
        /// <summary>
        /// Return a string with the XML representation of the given LogRecord.
        /// </summary>
        /// <param name="logRecord">the LogRecord to get a string representation of</param>
        /// <param name="config">the LogConfig that provides configuration-settings that dictate how it gets rendered</param>
        /// <returns>The XML representation</returns>
        public static string RenderAsXML( LogRecord logRecord, LogConfig config )
        {
            var sb = new StringBuilder( "<LogRecord" );
            if (logRecord.Id != null)
            {
                sb.Append( " id=\"" ).Append( logRecord.Id ).Append( "\"" );
            }
            if (logRecord.IsInDesignMode)
            {
                sb.Append( " V=\"true\"" );
            }
            // Put the level, unless it is at it's default value.
            if (logRecord.Level != LogLevel.Infomation)
            {
                sb.Append( " Level=\"" ).Append( logRecord.Level ).Append( "\"" );
            }
            if (logRecord.SourceHost != null)
            {
                sb.Append( " Host=\"" ).Append( logRecord.SourceHost ).Append( "\"" );
            }
            // Put the name of the logger.
            if (logRecord.SourceLogger != null && logRecord.SourceLogger != LogManager.NameOfDefaultLogger)
            {
                sb.Append( " Logger=\"" ).Append( logRecord.SourceLogger ).Append( "\"" );
            }
            if (logRecord.SubjectProgramName != null)
            {
                sb.Append( " Prog=\"" ).Append( logRecord.SubjectProgramName ).Append( "\"" );
            }
            if (logRecord.SubjectProgramVersion != null)
            {
                sb.Append( " Ver=\"" ).Append( logRecord.SubjectProgramVersion ).Append( "\"" );
            }
            // Put the thread identifier, if that is non-zero.
            if (logRecord.ThreadId != 0)
            {
                sb.Append( " Thread=\"" ).Append( logRecord.ThreadId ).Append( "\"" );
            }
            if (logRecord.Username != null)
            {
                sb.Append( " User=\"" ).Append( logRecord.Username ).Append( "\"" );
            }
            if (logRecord.When != default( DateTime ))
            {
                sb.Append( " When=\"" ).Append( LogManager.LogRecordFormatter.GetTimeStamp( logRecord.When, config, false ) ).Append( "\"" );
            }
            sb.Append( ">" ).Append( Environment.NewLine ).Append( "\t" ).Append( "<Message>" )
                .Append( logRecord.Message ).Append( "</Message>" )
                .AppendLine().Append( "</LogRecord>" ).AppendLine();
            return sb.ToString();
        }
        #endregion RenderAsXML

        #region TryParseToLogLevel
        /// <summary>
        /// Given a string that is expected to contain a representation of a LogLevel value,
        /// return true if it parses successfully into a LogLevel and set level to that value.
        /// </summary>
        /// <param name="text">the string to try to convert to a LogLevel</param>
        /// <param name="level">the resulting LogLevel</param>
        /// <returns>true if the given text does represent a LogLevel</returns>
        /// <remarks>
        /// This is necessary since Enum TryParse is not available in .NET 3.5 or earlier
        /// </remarks>
        public static bool TryParseToLogLevel( string text, out LogLevel level )
        {
            // Handle this special-case spelling variation.
            if (text.IsEqualIgnoringCase( "warning" ))
            {
                text = "warn";
            }
            bool ok = text.TryToParseToEnum<LogLevel>( out level );
            if (!ok)
            {
                WriteToConsole( "Invalid LogLevel text: " + StringLib.AsString( text ) );
            }
            return ok;
        }
        #endregion

        #region WriteToConsole
#if !PRE_4
        /// <summary>
        /// Output the given text to the 'console' (using either Console.Write or Debug.Write).
        /// </summary>
        /// <param name="what">some text to write</param>
        /// <param name="x">(optional) an Exception to show</param>
        /// <remarks>
        /// This is provided in order to isolate the issue of how to write to the IDE or terminal-window (depending upon the platform)
        /// to one place.
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage( "Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters", MessageId = "System.Console.WriteLine(System.String)" )]
        public static void WriteToConsole( string what, Exception x = null )
        {
            //CBL: Do we need to use this under Silverlight?
            // System.Diagnostics.Debug.WriteLine
            // In WPF projects, I was having - sometimes Debug.WriteLine works, and sometimes not. So - one might want to use Console here.
            // I do not want Console applications to spew log output onto the console, however.
            //#if SILVERLIGHT
            //                System.Diagnostics.Debug.WriteLine(sb.ToString());
            //#else
            //            // In WPF projects, sometimes Debug.WriteLine works, and sometimes not. So - I use Console here.
            //            if (LogManager.IsToOutputToConsole)
            //            {
            //                Console.WriteLine(sb.ToString());
            //            }
            //            else
            //            {
            //                System.Diagnostics.Debug.WriteLine(sb.ToString());
            //            }
            //#endif
            string msg;
            if (x != null)
            {
                msg = x.GetType() + ":" + x.Message + ", " + what;
            }
            else
            {
                msg = what;
            }
            if (LogManager.IsUsingTraceForConsoleOutput)
            {
                //CBL I guess I had not known about Trace output before.
                //    May want to refactor this.
                //Debug.WriteLine( msg );
                Trace.WriteLine( msg );
            }
#if !NETFX_CORE
            else
            {
                //CBL But what to do where for UWP code?
                Console.WriteLine( msg );
            }
#endif
        }
#else
        /// <summary>
        /// Output the given text to the 'console' (using either Console.Write or Debug.Write).
        /// </summary>
        /// <param name="what">some text to write</param>
        /// <remarks>
        /// This is provided in order to isolate the issue of how to write to the IDE or terminal-window (depending upon the platform)
        /// to one place.
        /// </remarks>
        public static void WriteToConsole( string what )
        {
            //CBL Or use Trace?
            Console.WriteLine( what );
        }

        /// <summary>
        /// Output the given text to the 'console' (using either Console.Write or Debug.Write).
        /// </summary>
        /// <param name="what">some text to write</param>
        /// <param name="x">the Exception to show</param>
        /// <remarks>
        /// This is provided in order to isolate the issue of how to write to the IDE or terminal-window (depending upon the platform)
        /// to one place.
        /// </remarks>
        public static void WriteToConsole( string what, Exception x )
        {
            if (x != null)
            {
                //CBL Or use Trace?
                Console.Write( what );
                Console.Write( ": " );
                Console.Write( x.GetType() );
                Console.WriteLine( ": " + x.Message );
            }
            else
            {
                Console.WriteLine( what );
            }
        }
#endif
        #endregion

        #region WriteToConsoleAndInternalLog
        /// <summary>
        /// Output the given text to (Trace or Console) and also log it to our internal log file named "LogNutInternalLog.txt" .
        /// </summary>
        /// <param name="what">some text to write</param>
        /// <param name="x">an exception to show (this may be null)</param>
        /// <remarks>
        /// This is provided in order to isolate the issue of how to write to the IDE or terminal-window (depending upon the platform)
        /// to one place.
        /// 
        /// The "InternalLog" is used to record internal problems within LogNut, wherein it experiences an error trying to log something and thus
        /// cannot of course "log" the problem when it cannot log anything.
        /// 
        /// For this, a separate, "internal" log is used -- a simple text file with the name "LogNutInternalLog.txt".
        /// This attempts to place it within the same folder as the normal file-output, as dictated by <see cref="LogConfig.FileOutputFolder"/>,
        /// but if there is any problem writing to that -- then this goes to the default output folder
        /// as denoted by <see cref="LogManager.FileOutputFolder_DefaultValue"/> instead.
        /// </remarks>
        internal static void WriteToConsoleAndInternalLog( string what, Exception x )
        {
            string msg;
            if (x != null)
            {
                if (!String.IsNullOrEmpty( what ))
                {
                    msg = what + ": " + StringLib.ExceptionDetails( x, true );
                }
                else
                {
                    msg = StringLib.ExceptionDetails( x, true );
                }
            }
            else
            {
                msg = what;
            }
            if (LogManager.IsUsingTraceForConsoleOutput)
            {
                Trace.WriteLine( msg );
            }
#if !NETFX_CORE
            else
            {
                //CBL  But what should we do here, for UWP code?
                Console.WriteLine( msg );
            }
#endif
            WriteToInternalLog( msg + Environment.NewLine + Environment.NewLine );
        }

        /// <summary>
        /// Output the given text to Debug and also log it to our internal log file named "LogNutInternalLog.txt" .
        /// </summary>
        /// <param name="format">the format to use to express the message (as in String.Format)</param>
        /// <param name="args">an array of objects that represents the values to go into the Message we want to log</param>
        /// <remarks>
        /// This is provided in order to isolate the issue of how to write to the IDE or terminal-window (depending upon the platform)
        /// to one place.
        /// 
        /// The "InternalLog" is used to record internal problems within LogNut, wherein it experiences an error trying to log something and thus
        /// cannot of course "log" the problem when it cannot log anything.
        /// 
        /// For this, a separate, "internal" log is used -- a simple text file with the name "LogNutInternalLog.txt".
        /// This attempts to place it within the same folder as the normal file-output, as dictated by <see cref="LogConfig.FileOutputFolder"/>,
        /// but if there is any problem writing to that -- then this goes to the default output folder
        /// as denoted by <see cref="LogManager.FileOutputFolder_DefaultValue"/> instead.
        /// </remarks>
        [StringFormatMethod( "format" )]
        internal static void WriteToConsoleAndInternalLog( string format, params object[] args )
        {
            string what = String.Format( format, args );
            if (LogManager.IsUsingTraceForConsoleOutput)
            {
                Trace.WriteLine( what );
            }
#if !NETFX_CORE
            else
            {
                //CBL  But what should we do here, for UWP code?
                Console.WriteLine( what );
            }
#endif
            WriteToInternalLog( what + Environment.NewLine + Environment.NewLine );
        }
        #endregion

        #region WriteToInternalLog
        /// <summary>
        /// Write diagnostic or other important text to the currently-configured file-output log, or if unable to do that
        ///  - to a separate LogNut log file named "LogNutBackupLog.txt".
        /// </summary>
        /// <param name="what">the text to write to the log</param>
        /// <remarks>
        /// <para>This is used to record internal problems within LogNut, wherein it experiences an error trying to log something and thus
        /// cannot of course "log" the problem when it cannot log anything.</para>
        /// 
        /// <para>
        /// When unable to write LogNut-specific diagnostic information to the regular log-file,
        /// this tries to write to a separate, "backup" log file -- a text file with the name "LogNutBackupLog.txt".
        /// This attempts to place it within the same folder as the normal file-output, as dictated by <see cref="LogConfig.FileOutputFolder"/>,
        /// but if there is any problem writing to that -- then this goes to the default output folder
        /// as denoted by <see cref="LogManager.FileOutputFolder_DefaultValue"/> instead.</para>
        /// If unable to write to the currently-configured file-output folder - then this searches for a folder that it *can* write to
        /// using <see cref="NutUtil.FindWorkingFileOutputFolder"/>.
        /// </remarks>
        internal static void WriteToInternalLog( string what )
        {
            if (StringLib.HasSomething( what ))
            {
                string textToLog = LogManager.LogRecordFormatter.GetTimeStamp( DateTime.Now, null, false ) + "  " + what;
                string path = null;
                string folderToUse = null;
                // First try writing to the normal file-output file..
                try
                {
                    path = LogManager.Config.FileOutputPath;
                    ZFileInfo fileInfo1 = new ZFileInfo( path );
                    fileInfo1.AppendText( textToLog, false );
                }
                catch (Exception x1)
                {
                    string msg1 = StringLib.ExceptionDetails( x1, true, String.Format( "Failed to write to normal log file {0}", path ), "WriteToInternalLog", null, 0, true );
                    WriteToConsole( msg1 );
                    // Try using a different folder..
                    try
                    {
                        //CBL  This violates the reason for IFilesystem!
                        IAccessibilityEvaluator filesystem = new AccessibilityEvaluator();
                        string reason, descriptionOfPathChosen;
                        folderToUse = FindWorkingFileOutputFolder( filesystem, LogManager.Config.FileOutputFolder, null, out descriptionOfPathChosen, out reason );
                        if (folderToUse == null)
                        {
                            string msg2 = String.Format( "WriteToInternalLog( {0} ) failed - unable to find a writable directory: {1}", what, reason );
                            WriteToConsole( msg2 );
                        }
                        else
                        {
                            string msg3 = String.Format( "WriteToInternalLog( {0} ) has to switch to alternative path: {1}, reason: {2}", what, descriptionOfPathChosen, reason );
                            WriteToConsole( msg3 );
                            LogManager.Config.FileOutputFolder = folderToUse;
                            path = LogManager.Config.FileOutputPath;
                            ZFileInfo fileInfo2 = new ZFileInfo( path );
                            fileInfo2.AppendText( msg3 + Environment.NewLine, false );
                            fileInfo2.AppendText( textToLog, false );
                        }
                    }
                    catch (Exception x2)
                    {
                        string msg4 = StringLib.ExceptionDetails( x2, true, "Trying to write to alternate log file " + path, "WriteToInternalLog", null, 0, false );
                        WriteToConsole( msg4 );
                        // Try writing to a different file...
                        path = Path.Combine( folderToUse, FileNameForBackupLog );
                        ZFileInfo fileInfo2 = new ZFileInfo( path );
                        fileInfo2.AppendText( textToLog, false );
                    }
                }
            }
        }
        #endregion

        /// <summary>
        /// This is the text that the banner-text starts with, which is "----[ First log upon start ".
        /// </summary>
        internal const string BannerTextPrefix = "----[ First log upon start ";

        /// <summary>
        /// This is the text that the banner-text ends with, which is "]----".
        /// </summary>
        internal const string BannerTextSuffix = " ]----";

        /// <summary>
        /// This is the filename to use when LogNut is unable to write to the normal log-file.
        /// </summary>
        private const string FileNameForBackupLog = "LogNutBackupLog.txt";

        /// <summary>
        /// This is the name used for the Windows Service that LogNut uses.
        /// </summary>
        public const string LogNutWindowsServiceName = "LogNutService";
    }

    #region BlockingCollection
#if PRE_4
    //public class BlockingCollection<T> : System.Collections.Concurrent.ConcurrentQueue<T>
    //{
    //    public bool TryAdd(T item, TimeSpan timeout)
    //    {
    //        this.Enqueue(item);
    //        //CBL Implement a wait for the timeout and then re-checking.
    //        return true;
    //    }

    //    public bool TryTake(out T item, TimeSpan timeout)
    //    {
    //        if (this.Count > 0)
    //        {
    //            return TryDequeue(out item);
    //        }
    //        else
    //        {
    //            item = default(T);
    //            return false;
    //        }
    //    }
    //}
#endif
    #endregion
}
