using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using Hurst.LogNut.Util;
using Hurst.LogNut.OutputPipes;
#if NETFX_CORE
using Windows.ApplicationModel;
using Windows.UI.Xaml;
#else
using System.Windows.Interop;
using System.Windows.Threading;
#endif


// Note: Add a reference to Ionic.Zip to this project. That assembly should be within the VendorLibs folder.


namespace Hurst.LogNut
{
    /// <summary>
    /// This class contains configuration-settings for LogNut.
    /// </summary>
    [XmlRoot("LogNutConfiguration")]
    public class LogConfig : IDisposable
    {
        #region constructor and factory method (implementing the singleton pattern)
        /// <summary>
        /// Create a new LogConfig object.
        /// </summary>
        public LogConfig()
        {
            IsFileOutputEnabled = true;
            IsToCreateNewOutputFileUponStartup = true;
        }

        #region copy-ctor
        /// <summary>
        /// Copy-constructor: copies the property-state of the source object.
        /// </summary>
        /// <param name="source">the object to copy the field-state from</param>
        public LogConfig(LogConfig source)
        {
            //CBL Is this needed? Following what was in ContentConfig..
            _decimalPlacesForSeconds = source.DecimalPlacesForSeconds;
            _fileOutputAdditionalHeaderLines = source.FileOutputAdditionalHeaderLines;
            _fileOutputAdditionalHeaderText = source.FileOutputAdditionalHeaderText;
            _fileOutputArchiveFolder = source.FileOutputArchiveFolder;
            _fileOutputFilename = source.FileOutputFilename;
            _fileOutputFolder = source.FileOutputFolder;
            _fileOutputFormatType = source.FileOutputFormatType;
            _fileOutputRolloverMode = source.FileOutputRolloverMode;
            _fileOutputRollPoint = source.FileOutputRollPoint;
            //CBL Need more yet!
            //_fileOutputArchiveFolder_FullPath = source.FileOutputArchiveFolder_FullPath;
            _isArchiveFolderRelative = source._isArchiveFolderRelative;
            _isAsynchronous = source.IsAsynchronous;
            _isFileOutputPrefixOfFixedWidth = source.IsFileOutputPrefixOfFixedWidth;
            _isFileOutputSpreadsheetCompatible = source.IsFileOutputSpreadsheetCompatible;
            _isFileOutputToCompressFiles = source.IsFileOutputToCompressFiles;
            _isFileOutputToInsertHeader = source.IsFileOutputToInsertHeader;
            _isFileOutputToInsertLineBetweenTraces = source.IsFileOutputToInsertLineBetweenTraces;
            _isToShowCategory = source.IsToShowCategory;
            _isToShowFractionsOfASecond = source._isToShowFractionsOfASecond;
            _isToShowId = source._isToShowId;
            _isToShowLevel = source._isToShowLevel;
            _isToShowLoggerName = source._isToShowLoggerName;
            _isToShowPrefix = source._isToShowPrefix;
            _isToShowSourceHost = source._isToShowSourceHost;
            _isToShowStackTraceForExceptions = source._isToShowStackTraceForExceptions;
            _isToShowSubjectProgram = source._isToShowSubjectProgram;
            _isToShowSubjectProgramVersion = source._isToShowSubjectProgramVersion;
            _isToShowThread = source._isToShowThread;
            _isToShowTimestamp = source._isToShowTimestamp;
            _isToShowUser = source._isToShowUser;
            _lowestLevelThatIsEnabled = source.LowestLevelThatIsEnabled;
            _removableDrivePreferredOutputFolder = source.RemovableDrivePreferredFileOutputFolder;
        }
        #endregion

        /// <summary>
        /// Return the singleton object instance of the Configurator class.
        /// </summary>
        [XmlIgnore]
        public static LogConfig The
        {
            get
            {
                if (_theConfiguration == null)
                {
                    //CBL Do I really want to just assume that we only want to read from a file?
                    //if (DoesConfigurationFileExist)
                    //{
                    //    _theConfiguration = LogConfig.DeserializeFromXml();
                    //}
                    //else
                    //{
                    //Console.WriteLine("No LogNut configuration settings file " + _filePathnameForSettings + " within the executable's folder.");
                    _theConfiguration = new LogConfig();
                    //}
                    //if (_theConfiguration.IsToWatchConfigurationFile)
                    //{
                    //    _theConfiguration.BeginWatchingFilesystem();
                    //}
                    //_theConfiguration.AllowForHysteresis();
                }
                return _theConfiguration;
            }
        }

        /// <summary>
        /// Get an instance of LogConfig that is suitable for outputing to the command-line or IDE console.
        /// </summary>
        /// <param name="primaryConfig">this provides the settings for whether to shows the prefix, thread, nor logger-name</param>
        /// <returns>Either a new or a saved LogConfig that is suitable for displaying output to the IDE output window or command-line console.</returns>
        /// <remarks>
        /// Subsequent gets of this method will retrieve the same object. Thus if you set properties
        /// on this object - those will remain throughout the current program-execution.
        /// </remarks>
        public static LogConfig GetForConsoleOutput(LogConfig primaryConfig)
        {
            if (_configurationForConsoleOutput == null)
            {
                _configurationForConsoleOutput = new LogConfig();
                _configurationForConsoleOutput.IsFileOutputSpreadsheetCompatible = false;

                _configurationForConsoleOutput.IsToShowLevelOnlyForWarningsAndAbove = true;
                _configurationForConsoleOutput.IsToShowStackTraceForExceptions = false;
                _configurationForConsoleOutput.IsToShowTimestamp = false;

                // Import a few properties from the given primaryConfig:
                _configurationForConsoleOutput.IsToShowPrefix = primaryConfig.IsToShowPrefix;
                _configurationForConsoleOutput.IsToShowThread = primaryConfig.IsToShowThread;
                _configurationForConsoleOutput.IsToShowLoggerName = primaryConfig.IsToShowLoggerName;
            }
            return _configurationForConsoleOutput;
        }

        #endregion constructor and factory method (implementing the singleton pattern)

        #region persistence

        #region ConfigurationFile
#if !NETFX_CORE
        /// <summary>
        /// Get or set the pathname of the configuration file.
        /// </summary>
        [XmlIgnore]
        public string ConfigurationFile
        {
            get { return _filePathnameForSettings; }
            set { _filePathnameForSettings = value; }
        }
#endif
        #endregion

        #region OverrideConfigurationFromFile
        /// <summary>
        /// Read the LogNut configuration settings from a text-file.
        /// </summary>
        /// <param name="pathnameOfConfigurationFile">If this is non-null, then it determines where the configuration settings is read from</param>
        /// <param name="isToTraceTheSettings">Set this to true if you want to see a trace of the configuration-settings on the console-output as they are found. Default is true.</param>
        /// <returns>a new <c>LogConfig</c> object deserialized from the given file</returns>
        public void OverrideConfigurationFromFile(string pathnameOfConfigurationFile, bool isToTraceTheSettings = true)
        {
            //CBL  I am not convinced of the proper naming of this method, nor the preceding one. Need to cogitate upon that.
            //CBL  I want to eliminate this, however -- I'd like to retain the console-output for when it does find specific settings and overrides them.
            if (File.Exists(pathnameOfConfigurationFile))
            {
                try
                {
#if DEBUG
                    NutUtil.WriteToConsole("Found Logging Configuration File " + pathnameOfConfigurationFile);
#endif
                    foreach (string line in FilesystemLib.ReadLines(pathnameOfConfigurationFile))
                    {
                        if (!line.StartsWith("//"))
                        {
                            string propertyName = line.PartBefore(":").Trim();
                            string propertyText = line.PartAfter(":").Trim();

                            switch (propertyName)
                            {
                                case "ContentConfig":
                                case "EmailOutput":
                                    // Ignore.
                                    break;
                                case "DecimalPlacesForSeconds":
                                    this.DecimalPlacesForSeconds = Int32.Parse(propertyText.RemoveAll(','));
                                    break;
                                case "FileOutputAdditionalHeaderLines":
                                    this.FileOutputAdditionalHeaderLines = propertyText;
                                    break;
                                case "FileOutputAdditionalHeaderText":
                                    this.FileOutputAdditionalHeaderText = propertyText;
                                    break;
                                case "FileOutputArchiveFolder":
                                    if (isToTraceTheSettings)
                                    {
                                        NutUtil.WriteToConsole("Setting FileOutputArchiveFolder to " + propertyText);
                                    }
                                    this.FileOutputArchiveFolder = propertyText;
                                    break;
                                case "FileOutputFilename":
                                    //CBL Should we add this for all of the properties?  This seems wordy.
                                    if (isToTraceTheSettings)
                                    {
                                        NutUtil.WriteToConsole("Setting FileOutputFilename to " + propertyText);
                                    }
                                    this.FileOutputFilename = propertyText;
                                    break;
                                case "FileOutputFolder":
                                    if (isToTraceTheSettings)
                                    {
                                        NutUtil.WriteToConsole("Setting FileOutputFolder to " + propertyText);
                                    }
                                    this.FileOutputFolder = propertyText;
                                    break;
                                case "FileOutputFormatType":
                                    {
                                        if (isToTraceTheSettings)
                                        {
                                            NutUtil.WriteToConsole("Setting FileOutputFormatType to " + propertyText);
                                        }
#if !PRE_4
                                        LogFileFormatType value;
                                        if (Enum.TryParse<LogFileFormatType>(propertyText, true, out value))
                                        {
                                            this.FileOutputFormatType = value;
                                        }
                                        else
                                        {
                                            throw new ArgumentException("Invalid value " + propertyText, "FileOutputFormatType");
                                        }
#else
                                        this.FileOutputFormatType = (LogFileFormatType)Enum.Parse( typeof( LogFileFormatType ), propertyText.PutIntoTypeOfCasing( StringLib.TypeOfCasing.Titlecased ) );
#endif
                                    }
                                    break;
                                case "FileOutputRolloverMode":
                                    {
                                        if (isToTraceTheSettings)
                                        {
                                            NutUtil.WriteToConsole("Setting FileOutputRolloverMode to " + propertyText);
                                        }
                                        this.FileOutputRolloverMode = (RolloverMode)Enum.Parse(typeof(RolloverMode), propertyText.PutIntoTypeOfCasing(StringLib.TypeOfCasing.Titlecased));
                                        //RolloverMode value;
                                        //if (Enum.TryParse<RolloverMode>(propertyText, true, out value))
                                        //{
                                        //    this.FileOutputRolloverMode = value;
                                        //}
                                        //else
                                        //{
                                        //    throw new ArgumentException(String.Format("Invalid value {0}", propertyText), "FileOutputRolloverMode");
                                        //}
                                    }
                                    break;
                                case "FileOutputRollPoint":
                                    {
#if !PRE_4
                                        RollPoint value;
                                        if (Enum.TryParse<RollPoint>(propertyText, true, out value))
                                        {
                                            this.FileOutputRollPoint = value;
                                        }
                                        else
                                        {
                                            throw new ArgumentException("Invalid value on line: " + line, "FileOutputRollPoint");
                                        }
#else
                                        this.FileOutputRollPoint = (RollPoint)Enum.Parse( typeof( RollPoint ), propertyText.PutIntoTypeOfCasing( StringLib.TypeOfCasing.Titlecased ) );

                                        //bool wasFound = false;
                                        //foreach (var name in Enum.GetNames(typeof (RollPoint)))
                                        //{
                                        //    if (StringLib.IsEqualIgnoringCase(name, propertyText))
                                        //    {
                                        //        wasFound = true;
                                        //        this.FileOutputRollPoint = (RollPoint)Enum.Parse(typeof(RollPoint), name);
                                        //        break;
                                        //    }
                                        //}
                                        //if (!wasFound)
                                        //    throw new ArgumentException(String.Format("Invalid value on line: {0}", line), "FileOutputRollPoint");
#endif
                                    }
                                    break;
                                case "IsAsynchronous":
                                    if (isToTraceTheSettings)
                                    {
                                        NutUtil.WriteToConsole("Setting IsAsynchronous to " + propertyText);
                                    }
                                    this.IsAsynchronous = Boolean.Parse(propertyText);
                                    break;
                                case "IsFileOutputEnabled":
                                    if (isToTraceTheSettings)
                                    {
                                        NutUtil.WriteToConsole("Setting IsFileOutputEnabled to " + propertyText);
                                    }
                                    this.IsFileOutputEnabled = Boolean.Parse(propertyText);
                                    break;
                                case "IsFileOutputPrefixOfFixedWidth":
                                    this.IsFileOutputPrefixOfFixedWidth = Boolean.Parse(propertyText);
                                    break;
                                case "IsFileOutputSpreadsheetCompatible":
                                    this.IsFileOutputSpreadsheetCompatible = Boolean.Parse(propertyText);
                                    break;
                                case "IsFileOutputToCompressFiles":
                                    this.IsFileOutputToCompressFiles = Boolean.Parse(propertyText);
                                    break;
                                case "IsFileOutputToInsertHeader":
                                    this.IsFileOutputToInsertHeader = Boolean.Parse(propertyText);
                                    break;
                                case "IsFileOutputToInsertLineBetweenTraces":
                                    this.IsFileOutputToInsertLineBetweenTraces = Boolean.Parse(propertyText);
                                    break;
                                case "IsToCreateNewOutputFileUponStartup":
                                    this.IsToCreateNewOutputFileUponStartup = Boolean.Parse(propertyText);
                                    break;
                                case "IsFileOutputToUseStdTerminator":
                                    this.IsFileOutputToUseStdTerminator = Boolean.Parse(propertyText);
                                    break;
                                case "IsLoggingEnabled":
                                    if (isToTraceTheSettings)
                                    {
                                        NutUtil.WriteToConsole("Setting IsLoggingEnabled to " + propertyText);
                                    }
                                    this.IsLoggingEnabled = Boolean.Parse(propertyText);
                                    break;
                                case "IsToShowCategory":
                                    this.IsToShowCategory = Boolean.Parse(propertyText);
                                    break;
                                case "IsSuppressingExceptions":
                                    this.IsSuppressingExceptions = Boolean.Parse(propertyText);
                                    break;
                                case "IsFileOutputToWriteThrough":
                                    this.IsFileOutputToWriteThrough = Boolean.Parse(propertyText);
                                    break;
                                case "IsToOutputToConsole":
                                    this.IsToOutputToConsole = Boolean.Parse(propertyText);
                                    break;
                                case "IsToWatchConfigurationFile":
                                    this.IsToWatchConfigurationFile = (Boolean)Enum.Parse(typeof(Boolean), propertyText);
                                    break;
                                case "LowestLevelThatIsEnabled":
                                    if (isToTraceTheSettings)
                                    {
                                        NutUtil.WriteToConsole("Setting LowestLevelThatIsEnabled to " + propertyText);
                                    }
                                    this.LowestLevelThatIsEnabled = (LogLevel)Enum.Parse(typeof(LogLevel), propertyText.PutIntoTypeOfCasing(StringLib.TypeOfCasing.Titlecased));
                                    break;
                                case "MaxFileSize":
                                    this.MaxFileSize = Int64.Parse(propertyText.RemoveAll(','));
                                    break;
                                case "MaxNumberOfFileRollovers":
                                    this.MaxNumberOfFileRollovers = Int32.Parse(propertyText.RemoveAll(','));
                                    break;
                                case "IsToShowDateInTimestamp":
                                    this.IsToShowDateInTimestamp = Boolean.Parse(propertyText);
                                    break;
                                case "IsToShowElapsedTime":
                                    this.IsToShowElapsedTime = Boolean.Parse(propertyText);
                                    break;
                                case "IsToShowElapsedTimeInSeconds":
                                    this.IsToShowElapsedTimeInSeconds = Boolean.Parse(propertyText);
                                    break;
                                case "IsToShowFractionsOfASecond":
                                    this.IsToShowFractionsOfASecond = Boolean.Parse(propertyText);
                                    break;
                                case "IsToShowId":
                                    this.IsToShowId = Boolean.Parse(propertyText);
                                    break;
                                case "IsToShowLevel":
                                    this.IsToShowLevel = Boolean.Parse(propertyText);
                                    break;
                                case "IsToShowLevelOnlyForWarningsAndAbove":
                                    this.IsToShowLevelOnlyForWarningsAndAbove = Boolean.Parse(propertyText);
                                    break;
                                case "IsToShowLoggerName":
                                    this.IsToShowLoggerName = Boolean.Parse(propertyText);
                                    break;
                                case "IsToShowPrefix":
                                    this.IsToShowPrefix = Boolean.Parse(propertyText);
                                    break;
                                case "IsToShowSourceHost":
                                    this.IsToShowSourceHost = Boolean.Parse(propertyText);
                                    break;
                                case "IsToShowStackTraceForExceptions":
                                    this.IsToShowStackTraceForExceptions = Boolean.Parse(propertyText);
                                    break;
                                case "IsToShowSubjectProgram":
                                    this.IsToShowSubjectProgram = Boolean.Parse(propertyText);
                                    break;
                                case "IsToShowSubjectProgramVersion":
                                    this.IsToShowSubjectProgramVersion = Boolean.Parse(propertyText);
                                    break;
                                case "IsToShowThread":
                                    this.IsToShowThread = Boolean.Parse(propertyText);
                                    break;
                                case "IsToShowTimestamp":
                                    this.IsToShowTimestamp = Boolean.Parse(propertyText);
                                    break;
                                case "IsToShowUser":
                                    this.IsToShowUser = Boolean.Parse(propertyText);
                                    break;
                                default:
                                    bool wasRecognized = false;
                                    if (LogManager._outputPipes != null)
                                    {
                                        foreach (var pipe in LogManager.OutputPipes)
                                        {
                                            bool isAccepted = pipe.SetConfigurationFromText(propertyName, propertyText);
                                            if (isAccepted)
                                            {
                                                wasRecognized = true;
                                                break;
                                            }
                                        }
                                    }

                                    if (!wasRecognized)
                                    {
                                        throw new ArgumentException("Invalid property-name: " + line);
                                    }
                                    break;
                            }
                        }
                    }
                }
                catch (Exception x)
                {
                    if (this.IsSuppressingExceptions)
                    {
                        NutUtil.WriteToConsoleAndInternalLog("{0} in LogNut.LogConfig.OverrideConfigurationFromFile({1}: {2}, continuing without reading logging configuration from file.",
                                x.GetType(), pathnameOfConfigurationFile, x.Message);
                    }
                    else
                    {
                        x.Data.Add("Context: ", "pathnameOfConfigurationFile is " + pathnameOfConfigurationFile);
                        throw;
                    }
                }
            }
            else
            {
                NutUtil.WriteToConsole("File not found in LogNut.LogConfig.OverrideConfigurationFromFile: " + pathnameOfConfigurationFile + ", continuing without reading logging configuration from that file.");
            }
        }
        #endregion OverrideConfigurationFromFile

        #region ReadFromXmlFile
        /// <summary>
        /// Create and return a new LogConfig object with property-values determined from the content of the given file.
        /// </summary>
        /// <param name="pathnameOfConfigurationFile">the filesystem-pathname of the file to read from</param>
        /// <returns>a new LogConfig object derived from the given file, or null if that file is not found</returns>
        public static LogConfig ReadFromXmlFile(string pathnameOfConfigurationFile)
        {
            if (pathnameOfConfigurationFile == null)
            {
                throw new ArgumentNullException("pathnameOfConfigurationFile");
            }

            LogConfig newLogConfig = null;
            if (FilesystemLib.FileExists(pathnameOfConfigurationFile))
            {
                NutUtil.WriteToConsole("Reading LogNut configuration settings from file " + pathnameOfConfigurationFile);
                try
                {
                    FileInfo fileInfo = new FileInfo(pathnameOfConfigurationFile);
                    string extension = fileInfo.Extension;

                    //CBL  See these notes for how to implement this in UWP code.
                    // http://stackoverflow.com/questions/14299410/where-is-filestream-at-the-net-for-windows-store
                    // http://stackoverflow.com/questions/31950552/streamreader-in-windows-10-universal-app

#if NETFX_CORE
//newLogConfig = await Hurst.BaseLib.XmlIO.ReadObjectFromXmlFileAsync<LogConfig>( pathnameOfConfigurationFile );
#else
                    var xmlDeserializer = new XmlSerializer(typeof(LogConfig));
                    using (var textReader = new StreamReader(pathnameOfConfigurationFile))
                    {
                        Object obj = xmlDeserializer.Deserialize(textReader);
                        newLogConfig = (LogConfig)obj;
                    }
#endif
                }
                catch (Exception x)
                {
                    string msg = "Unable to reading logging configuration from file " + pathnameOfConfigurationFile + Environment.NewLine + "In LogNut.LogConfig.ReadFromFile: " + x.Message + ".";
                    LogManager.HandleInternalFault(x, null, msg);
                }
            }
            else
            {
                //CBL  Should I just throw a normal exception here? So that the application's API can know and handle it?
                //     or Log this, or call NutUtil.WriteToConsole ?
                const string msg = "File not found when attempting to read LogNut configuration settings from XML file";
                LogManager.LogError(msg + ": " + pathnameOfConfigurationFile);
#if PRE_4
                throw new FileNotFoundException( msg, pathnameOfConfigurationFile );
#else
                throw new FileNotFoundException(message: msg, fileName: pathnameOfConfigurationFile);
#endif
            }
            return newLogConfig;
        }
        #endregion ReadFromXmlFile

#if !NETFX_CORE
        #region ReadFromYamlFile
        /// <summary>
        /// Read LogNut configuration settings from the given text-file, overriding the current settings with any property-values that it finds therein.
        /// The format is assumed to be a simplified YAML.
        /// </summary>
        /// <param name="pathnameOfConfigurationFile">the filesystem-pathname of the file to read from</param>
        /// <returns>true if all is okay, false if it finds any unrecognized values or there is some other error</returns>
        /// <remarks>
        /// This reads the given file and sets only the specified configuration properties that it finds within the file. The properties that are not mentioned within that file,
        /// are unaffected. Thus, this method is useful when you want to set some properties in code, and then read in a file to potentially override one or more of those properties.
        /// 
        /// You can create your configuration-settings file using the LogNut Control-Panel, or you can just just create one with a text-editor. The syntax is very simple.
        /// Create a text-file with the extension ".yaml", and enter each property on a separate line. For example..
        /// 
        /// FileOutputFolder: C:\OtherLogs
        /// IsAsynchronous: true
        /// LowestLevelThatIsEnabled: Error
        /// 
        /// Note that for enumeration and boolean types, the value you put into the file is not case-sensitive.
        /// For numerical values you may insert commas for readability (the commas will be ignored).
        /// 
        /// You may include comment-lines, which must start with a double-slash C#-style comment: "// This is a comment."  CBL Can we? Test this.
        /// </remarks>
        public bool ReadFromYamlFile(string pathnameOfConfigurationFile)
        {
            bool r = true;
            if (_filePathnameForSettings == null)
            {
                if (pathnameOfConfigurationFile == null)
                {
                    throw new ArgumentNullException("pathnameOfConfigurationFile");
                }
                _filePathnameForSettings = pathnameOfConfigurationFile;
            }

            if (FilesystemLib.FileExists(pathnameOfConfigurationFile))
            {
                NutUtil.WriteToConsole("Reading LogNut configuration settings from file " + pathnameOfConfigurationFile);
                try
                {
                    //CBL  See these notes for how to implement this in UWP code.
                    // http://stackoverflow.com/questions/14299410/where-is-filestream-at-the-net-for-windows-store
                    // http://stackoverflow.com/questions/31950552/streamreader-in-windows-10-universal-app

                    r = YamlReaderWriter.ReadPropertiesFromFile<LogConfig>(pathnameOfConfigurationFile, this);


                    //                    using (var textReader = File.OpenText( pathnameOfConfigurationFile ))
                    //                    {
                    //#if !PRE_4
                    //                        var yamlDeserializer = new Deserializer();
                    //#else
                    //                        var yamlDeserializer = new Deserializer(null, null, false);
                    //#endif
                    //                        newLogConfig = yamlDeserializer.Deserialize<LogConfig>( textReader );
                    //                    }
                }
                catch (Exception x)
                {
                    string msg = "Unable to reading logging configuration from file " + pathnameOfConfigurationFile + Environment.NewLine + "In LogNut.LogConfig.ReadFromFile: " + x.Message + ".";
                    LogManager.HandleInternalFault(x, null, msg);
                    r = false;
                }
            }
            else
            {
                //CBL  Should I just throw a normal exception here? So that the application's API can know and handle it?
                //     or Log this, or call NutUtil.WriteToConsole ?
                LogManager.LogError("File not found in LogNut.LogConfig.ReadFromFile: " + pathnameOfConfigurationFile + ", continuing without reading setting configuration properties.");
                r = false;
                //#if PRE_4
                //                throw new FileNotFoundException( "Attempt to read LogNut configuration settings from nonexistent file.", pathnameOfConfigurationFile );
                //#else
                //                throw new FileNotFoundException( message: "Attempt to read LogNut configuration settings from nonexistent file.", fileName: pathnameOfConfigurationFile );
                //#endif
            }
            return r;
        }
        #endregion ReadFromYamlFile
#endif

        #region Save
        /// <summary>
        /// Store this LogConfig to either the Registry, or to a file - depending upon the IsUsingRegistry property.
        /// </summary>
        public void Save()
        {
#if !NETFX_CORE
            {
                WriteToFile(_filePathnameForSettings, false);
            }
            //CBL We need to implement this for .NET Core !
#endif
        }
        #endregion

#if !NETFX_CORE
        #region WriteToFile
        /// <summary>
        /// Save the state of this LogConfig object to the given file.
        /// If the file's extension is ".xml", then that is the format usedL form, otherwise it is written as YAML.
        /// </summary>
        /// <param name="fileNameOrPath">the filename or complete pathname of the file to write the values to. If null - try to use the last path written to or read from.</param>
        /// <param name="isToWriteAllValues">Set this to false if you want to write only those properties which are not at their default value. Not applicable for XML file output.</param>
        public void WriteToFile(string fileNameOrPath, bool isToWriteAllValues)
        {
            if (fileNameOrPath == null)
            {
                if (_filePathnameForSettings == null)
                {
                    throw new ArgumentNullException("fileNameOrPath");
                }
            }
            else
            {
                _filePathnameForSettings = fileNameOrPath;
            }

            string pathname = fileNameOrPath;
            FileInfo fileInfo = new FileInfo(pathname);
            string extension = fileInfo.Extension;
            if (extension.Equals(".xml", StringComparison.OrdinalIgnoreCase))
            {
                // The file extension is .xml, so write it as XML..
                var xmlSerializer = new XmlSerializer(typeof(LogConfig));
                using (StreamWriter textWriter = new StreamWriter(pathname))
                {
                    //These next 2 lines cause the XML to not include the namespace.
                    var ns = new XmlSerializerNamespaces();
                    ns.Add("", "");
                    // Write this object to the filestream in XML form.
                    xmlSerializer.Serialize(textWriter, this, ns);
                }
            }
            else
            {
                // The extension is NOT .xml, so write it as YAML..
                YamlReaderWriter.WritePropertiesThatHaveDefaultValueAttributeTo(pathname: pathname, ofWhat: this, isToWriteAllValues: isToWriteAllValues);
            }
        }
        #endregion
#endif

        #endregion persistence

        #region configuration properties

        #region Username
        /// <summary>
        /// Get or set the name of the user who is currently running the subject-program,
        /// getting it from the object that implements <see cref="IApp"/> if one was supplied,
        /// otherwise from System.Security.Principal.WindowsIdentity.GetCurrent().
        /// </summary>
        /// <remarks>
        /// The first time this property is read, if unable to ascertain the username -- it is set to an empty-string (not null)
        /// so that on subsequent reads no attempt is made to re-execute the code that tries to ascertain the user.
        /// 
        /// This property is NOT saved in configuration files.
        /// </remarks>
        [XmlIgnore]
        public string Username
        {
            get
            {
                //CBL
                // http://stackoverflow.com/questions/33394019/get-username-in-a-windows-10-c-sharp-uwp-universal-windows-app

                if (_userName == null)
                {
                    //CBL  Note: If the developer has not set the LogManager.Config.Application property yet,
                    // this implementation will forever ignore that and set _userName to this other value.
                    // Is this really desired?
                    _userName = SystemLib.Username;
                }
                return _userName;
            }
            set { _userName = value; }
        }
        #endregion

        #region SourceHostName
        /// <summary>
        /// Get or set the name of the computer that is originating these log-records.
        /// </summary>
        /// <remarks>
        /// On .NET this is set to Environment.MachineName and normally you would not need to set this.
        /// But you can if you want to. Just sayin..
        /// 
        /// This property is NOT saved in configuration files.
        /// </remarks>
        [XmlIgnore]
        public string SourceHostName
        {
            get
            {
                if (_sourceHostName == null)
                {
                    _sourceHostName = SystemLib.ComputerName;
                }
                return _sourceHostName;
            }
            set { _sourceHostName = value; }
        }

        /// <summary>
        /// This is the computer-name of the host that the program-under-test was running on when it created a log-record.
        /// </summary>
        private string _sourceHostName;

        #endregion

        #region subject-program

        #region SubjectProgramName
        /// <summary>
        /// Return the name (reasonably shortened) of the subject-program
        /// (the program that is using the LogNut facility to do logging).
        /// This does not need to be explicitly set if the subject-program implements the <see cref="IApp"/> interface.
        /// </summary>
        /// <param name="typeFromApplication">put here any <c>type</c> from the application that is to do the logging, such that LogNut may look up the domain of the application</param>
        /// <returns>a string denoting the name of the subject-program</returns>
        /// <remarks>
        /// Unless the <see cref="SubjectProgram"/> property is set to an object of a class that implements the <see cref="IApp"/> interface,
        /// if you are going to set this property you would need to do it before doing any logging.
        /// 
        /// This property is NOT saved in configuration files.
        /// </remarks>
        public string GetSubjectProgramName(Type typeFromApplication)
        {
            if (_subjectProgramName == null)
            {
                _subjectProgramName = SystemLib.GetProgramName(typeFromApplication);
                // If this is LogNut_Net, then convert it to our standard LogNut.
                if (_subjectProgramName.Equals("LogNut_Net", StringComparison.InvariantCultureIgnoreCase)
                || _subjectProgramName.Equals("LogNut_lib", StringComparison.InvariantCultureIgnoreCase))
                {
                    _subjectProgramName = "LogNut";
                }
            }
            return _subjectProgramName;
        }

        /// <summary>
        /// Set the name of the subject-program (the program that is using the LogNut facility to do logging).
        /// </summary>
        /// <param name="name">a string denoting the name to assign as that of the subject-program</param>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        /// <remarks>
        /// You do not normally need to call this -- LogNut will retrieve the name automatically from the assembly,
        /// or from the <see cref="SubjectProgram"/> property if that has been set
        /// (since that implements the <see cref="IApp"/> interface and therefore has the <see cref="IApp.ProgramName"/> property.
        /// 
        /// This is provided in case you want to override that.
        /// </remarks>
        public LogConfig SetSubjectProgramName(string name)
        {
            _subjectProgramName = name;
            return this;
        }
        #endregion

        #region SubjectProgramVersion
        /// <summary>
        /// Get or set text to use the indicate the running version of the subject-program (the program that is using the LogNut facility to do logging).
        /// </summary>
        /// <returns>the version of the subject-program as a string</returns>
        /// <remarks>
        /// If you set the <see cref="SubjectProgram"/> property before this property is read, then that provides the version
        /// value since that implements the <see cref="IApp"/> interface.
        /// Otherwise, when this property is read the version value is retrieved from the assembly.
        /// 
        /// This property is NOT saved in configuration files.
        /// </remarks>
        [XmlIgnore]
        public string SubjectProgramVersion
        {
            get
            {
                // Only get the value once, and then save it within _subjectProgramVersion.
                if (_subjectProgramVersion == null)
                {
                    _subjectProgramVersion = SystemLib.GetVersion();
                }
                return _subjectProgramVersion;
            }
            set { _subjectProgramVersion = value; }
        }

        /// <summary>
        /// Set or set the version of the subject-program (the program that is using the LogNut facility to do logging).
        /// This is the same as setting the <see cref="SubjectProgramVersion"/> property.
        /// </summary>
        /// <param name="versionText">the string text to assign as the subject-program's version</param>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        /// <remarks>
        /// Calling this method is the same as setting the property <see cref="SubjectProgramVersion"/>.
        /// </remarks>
        public LogConfig SetSubjectProgramVersion(string versionText)
        {
            SubjectProgramVersion = versionText;
            return this;
        }
        #endregion

        #endregion subject-program

        #region IsAsynchronous
        /// <summary>
        /// Get or set whether to send the log records out asynchronously, which means any call to send out a log record
        /// returns to the caller immediately while some other thread or process accomplishes the writing or transmission of the log for us.
        /// Default is false.
        /// </summary>
        [DefaultValue(false)]
        public bool IsAsynchronous
        {
            get { return _isAsynchronous; }
            set
            {
                _isAsynchronous = value;
            }
        }

        /// <summary>
        /// Set logging to be asynchronous, meaning the logging output methods return immediately
        /// and the actual output will happen later.
        /// Calling this is the same as setting the IsAsynchronous property to true.
        /// </summary>
        /// <remarks>
        /// This duplicates the function of the IsAsynchronous property setter, in order to provide a fluent API.
        /// </remarks>
        /// <returns>a reference to this LogConfig object, such that further method calls may be chained</returns>
        public LogConfig SetToLogAsynchronously()
        {
            IsAsynchronous = true;
            return this;
        }

        /// <summary>
        /// Set logging to be synchronous (as in NOT asynchronous), meaning it happens as the logging output methods are called
        /// and the methods do not return until the output is complete.
        /// Calling this is the same as setting the IsAsynchronous property to false.
        /// </summary>
        /// <remarks>
        /// This duplicates the function of the IsAsynchronous property setter, in order to provide a fluent API.
        /// </remarks>
        /// <returns>a reference to this LogConfig object, such that further method calls may be chained</returns>
        public LogConfig SetToLogSynchronously()
        {
            _isAsynchronous = false;
            return this;
        }
        #endregion

        #region IsLoggingEnabledByDefault
        /// <summary>
        /// Get or set whether logging within the LogNut facility for this AppDomain is enabled by default, which means that
        /// logging-output happens unless this setting is overridden by IsLoggingEnabled, by individual loggers, or by categories.
        /// Default is true (logging is, of course, by default permitted).
        /// </summary>
        /// <remarks>
        /// When this property is false - logging is OFF except for those loggers that are explicitly enabled or for categories that are
        /// explicitly turned-on.
        /// 
        /// If isLoggingEnabled is turned off - this property (IsLoggingEnabledByDefault) has no effect because no logging happens regardless
        /// (while IsLoggingEnabled is false).
        /// 
        /// You would not use this property normally, unless you are wanting to turn off all your logging for some reason EXCEPT
        /// only specific, selected loggers or categories.
        /// things back 
        /// </remarks>
        [DefaultValue(true)]
        public bool IsLoggingEnabledByDefault
        {
            get { return _isLoggingEnabledByDefault; }
            set
            {
                _isLoggingEnabledByDefault = value;
                // If we here set this to it's non-default value, then clearly LogManager is not 'cleared' any longer.
                if (!_isLoggingEnabledByDefault)
                {
                    LogManager.IsCleared = false;
                }
            }
        }
        private bool _isLoggingEnabledByDefault = true;
        #endregion

        #region IsLoggingEnabled
        /// <summary>
        /// Get or set whether logging within the LogNut facility for this AppDomain is enabled, at all
        /// (this property is NOT overridden by any other property).
        /// Default is true (logging is permitted).
        /// </summary>
        /// <remarks>
        /// When this property is false (turned off) - this overrides Config.LowestLevelThatIsEnabled and also IsFileOutputEnabled.
        /// The enabled properties of the individual loggers, and also categories, do NOT override this.
        /// </remarks>
        [DefaultValue(true)]
        public bool IsLoggingEnabled
        {
            get { return _isLoggingEnabled; }
            set
            {
                _isLoggingEnabled = value;
                if (!_isLoggingEnabled)
                {
                    LogManager.IsCleared = false;
                }
            }
        }
        private bool _isLoggingEnabled = true;

        /// <summary>
        /// Turn on logging from LogNut (although this may be overridden by individual loggers and categories).
        /// Calling this is the same as setting the <see cref="IsLoggingEnabled"/> property to true
        /// (that is it's initial value).
        /// </summary>
        /// <remarks>
        /// This duplicates the function of the <see cref="IsLoggingEnabled"/> property setter, in order to provide a fluent API.
        /// </remarks>
        /// <returns>a reference to this configuration object, such that further method calls may be chained</returns>
        public LogConfig EnableLogging()
        {
            IsLoggingEnabled = true;
            return this;
        }

        /// <summary>
        /// Shut off logging from LogNut, unless this is overriden by a logger or category.
        /// Calling this is the same as setting the <see cref="IsLoggingEnabled"/> property to true
        /// (it's default value is true).
        /// </summary>
        /// <remarks>
        /// This duplicates the function of the <see cref="IsLoggingEnabled"/> property setter, in order to provide a fluent API.
        /// </remarks>
        /// <returns>a reference to this configuration object, such that further method calls may be chained</returns>
        public LogConfig DisableLogging()
        {
            IsLoggingEnabled = false;
            return this;
        }
        #endregion

        #region console/trace output

        /// <summary>
        /// Get or set whether to use (either Debug.Write or Console.Write) to write the log traces to the command-line output "console",
        /// which when running within Visual Studio means outputing to the "Output" console window.
        /// Default is true.
        /// </summary>
        [DefaultValue(true)]
        public bool IsToOutputToConsole
        {
            get { return _isToOutputToConsole; }
            set { _isToOutputToConsole = value; }
        }

        /// <summary>
        /// Turn on writing of log content to the command-line console or Visual Studio output.
        /// It is on by default.
        /// </summary>
        /// <param name="enable">this flag determines whether to turn it on, or not</param>
        /// <returns>a reference to this LogConfig object, such that further method calls may be chained</returns>
        public LogConfig EnableOutputToConsole(bool enable)
        {
            IsToOutputToConsole = enable;
            return this;
        }

        /// <summary>
        /// Turn off writing of log content to the command-line console or Visual Studio output.
        /// It is on by default.
        /// </summary>
        /// <returns>a reference to this LogConfig object, such that further method calls may be chained</returns>
        public LogConfig DisableOutputToConsole()
        {
            IsToOutputToConsole = false;
            return this;
        }

        #endregion console/trace output

        #region level enablement

        #region EnableAllLevels
        /// <summary>
        /// Allow output for all logging levels down to the lowest level,
        /// i.e. all log-levels.
        /// </summary>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        public LogConfig EnableAllLevels()
        {
            _lowestLevelThatIsEnabled = default(LogLevel);
            return this;
        }
        #endregion

        #region EnableLevelsDownTo
        /// <summary>
        /// Given a LogLevel, enable the levels from Critical down to the given level (inclusive),
        /// and disable the remaining, lower levels.
        /// </summary>
        /// <param name="level">The LogLevel to enable down to, from the highest priority down to this one</param>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        /// <remarks>
        /// Given a LogLevel, enable the levels for this logger from <c>Critical</c> down to the given level (inclusive),
        /// descending by order of 'severity', and disable the remaining, lower levels.
        /// Ie <c>Infomation</c> -> enable all levels;
        /// <c>Debug</c> -> enable <c>Debug</c>, <c>Warning</c>, <c>Error</c>, <c>Critical</c>;
        /// <c>Warning</c> -> <c>Warning</c>, <c>Error</c>, <c>Critical</c>;
        /// <c>Error</c> -> <c>Error</c>, <c>Critical</c>,
        /// and <c>Critical</c> -> enable only <c>Critical</c>.
        /// </remarks>
        public LogConfig EnableLevelsDownTo(LogLevel level)
        {
            LowestLevelThatIsEnabled = level;
            return this;
        }
        #endregion

        #region LowestLevelThatIsEnabled
        /// <summary>
        /// Get or set the minimum log-level that is enabled for output.
        /// Setting this to Trace, enables all levels.
        /// </summary>
        /// <remarks>
        /// This may be overridden by individual loggers, and both may be overridden by categories.
        /// </remarks>
        [DefaultValue(LogLevel.Trace)]
        public LogLevel LowestLevelThatIsEnabled
        {
            get { return _lowestLevelThatIsEnabled; }
            set
            {
                _lowestLevelThatIsEnabled = value;
                if (_lowestLevelThatIsEnabled != LogLevel.Trace)
                {
                    LogManager.IsCleared = false;
                }
            }
        }
        #endregion

        #region IsTraceEnabled
        /// <summary>
        /// Get whether Trace-level output is enabled.
        /// </summary>
        public bool IsTraceEnabled
        {
            get { return _lowestLevelThatIsEnabled == LogLevel.Trace; }
        }
        #endregion

        #region IsDebugEnabled
        /// <summary>
        /// Get whether Debug-level output is enabled.
        /// </summary>
        public bool IsDebugEnabled
        {
            get { return _lowestLevelThatIsEnabled <= LogLevel.Debug; }
        }
        #endregion

        #region IsInfoEnabled
        /// <summary>
        /// Get whether Infomation-level output is enabled, for all LogNut loggers of this LogManager. Default is true.
        /// </summary>
        public bool IsInfoEnabled
        {
            get { return _lowestLevelThatIsEnabled <= LogLevel.Infomation; }
        }
        #endregion

        #region IsWarnEnabled
        /// <summary>
        /// Get whether Warning-level output is enabled.
        /// </summary>
        public bool IsWarnEnabled
        {
            get { return _lowestLevelThatIsEnabled <= LogLevel.Warning; }
        }
        #endregion

        #region IsErrorEnabled
        /// <summary>
        /// Get whether Error-level output is enabled.
        /// </summary>
        public bool IsErrorEnabled
        {
            get { return _lowestLevelThatIsEnabled <= LogLevel.Error; }
        }
        #endregion

        #endregion level enablement

        #region IsSuppressingExceptions
        /// <summary>
        /// Get or set whether to prevent any exceptions from propogating up from the LogNut logging framework upward to the application itself.
        /// This is false by default (exceptions are NOT suppressed).
        /// </summary>
        /// <remarks>
        /// Normally, during development you would not set this, as you may want to know when a fault occurrs within your code.
        /// However, for production code -- you may want to set this to true, so that the logging system cannot itself be the cause of errors in your running application.
        /// </remarks>
        [DefaultValue(false)]
        public bool IsSuppressingExceptions { get; set; }

        /// <summary>
        /// Set LogNut to prevent any exceptions from propogating up from the LogNut logging framework upward to the application itself.
        /// This is the same as setting the <see cref="IsSuppressingExceptions"/> property to <c>true</c>.
        /// The default value of the property is <c>false</c> (exceptions are NOT suppressed).
        /// </summary>
        /// <returns>a reference to this <c>LogConfig</c> object so that methods may be chained together</returns>
        public LogConfig SuppressExceptions()
        {
            IsSuppressingExceptions = true;
            return this;
        }
        /// <summary>
        /// Set to NOT prevent any exceptions from propogating up from the LogNut logging framework upward to the application itself.
        /// This is the same as setting the <see cref="IsSuppressingExceptions"/> property to <c>false</c> (exceptions are NOT suppressed).
        /// </summary>
        /// <returns>a reference to this <c>LogConfig</c> object so that methods may be chained together</returns>
        public LogConfig DoNotSuppressExceptions()
        {
            IsSuppressingExceptions = false;
            return this;
        }
        #endregion

        #region IsToLogMethodBeginWithClassName
        /// <summary>
        /// Get or set whether, when calling LogMethodBegin, to log the class-name of the method
        /// instead of the source filename. The default is true.
        /// </summary>
        [DefaultValue(true)]
        public bool IsToLogMethodBeginWithClassName
        {
            get { return _isToLogMethodBeginWithClassName; }
            set { _isToLogMethodBeginWithClassName = value; }
        }
        #endregion

        #region output to the LogNut Windows Service

        #region IsWindowsServiceOutputEnabled
        /// <summary>
        /// Get or set whether to enable output to the LogNut Windows Service.
        /// Defaults to false - no output to the LogNut Windows Service.
        /// </summary>
        [DefaultValue(false)]
        public bool IsWindowsServiceOutputEnabled
        {
            get { return _isWindowsServiceOutputEnabled; }
            set
            {
                if (value != _isWindowsServiceOutputEnabled)
                {
                    _isWindowsServiceOutputEnabled = value;
                    if (_isWindowsServiceOutputEnabled)
                    {
                        if (!_hasServiceApplicBeenLaunchedYet)
                        {
                            //SystemLib.LaunchProgram( pathnameOfApplication: "LognutServiceApplic.exe" );
                            _hasServiceApplicBeenLaunchedYet = true;
                        }
                    }
                }
            }
        }
        private bool _isWindowsServiceOutputEnabled;
        private bool _hasServiceApplicBeenLaunchedYet;
        #endregion

        #region SetOutputToUseWindowsService
        /// <summary>
        /// Turn on output to the LogNut Windows Service.
        /// This is the same as setting the <see cref="IsWindowsServiceOutputEnabled"/> property to true.
        /// The initial value of that property is <c>false</c>.
        /// </summary>
        /// <returns>a reference to this configuration object so that methods may be chained together</returns>
        /// <remarks>
        /// Calling this method is the same as setting the <see cref="IsWindowsServiceOutputEnabled"/> property to true.
        /// </remarks>
        public LogConfig SetOutputToUseWindowsService()
        {
            IsWindowsServiceOutputEnabled = true;
            return this;
        }
        #endregion

        #endregion output to the LogNut Windows Service

        #region output to file

        #region FileOutputAdditionalHeaderText
        /// <summary>
        /// Get or set additional text to append to the standard header information that is written before the first log
        /// is written to a file.
        /// </summary>
        /// <remarks>
        /// This is distinct from <see cref="FileOutputAdditionalHeaderLines"/> in that this is inserted into the
        /// first header line, whereas that other property represents text to be added as additional lines.
        /// 
        /// If the value supplied for the <c>additionalText</c> argument is null, empty, or "Not Set"
        /// then the <see cref="FileOutputAdditionalHeaderText"/> property is set to null.
        /// </remarks>
        [DefaultValue(null)]
        public string FileOutputAdditionalHeaderText
        {
            get { return _fileOutputAdditionalHeaderText; }
            set
            {
                if (StringLib.HasNothing(value) || value.Equals("Not Set"))
                {
                    _fileOutputAdditionalHeaderText = null;
                }
                else
                {
                    _fileOutputAdditionalHeaderText = value;
                    LogManager.IsCleared = false;
                }
            }
        }

        /// <summary>
        /// Specify any additional text to append to the standard header information that is written before the first log
        /// is written to a file.
        /// This is the same as setting the <see cref="FileOutputAdditionalHeaderText"/> property.
        /// </summary>
        /// <param name="additionalText">the text to append to the end of the header</param>
        /// <returns>a reference to this <c>LogConfig</c> object, such that further method calls may be chained</returns>
        /// <remarks>
        /// This duplicates the function of the <see cref="FileOutputAdditionalHeaderText"/> property setter, for the purpose of providing a fluent API.
        /// 
        /// If the value supplied for the <c>additionalText</c> argument is null, empty, or "Not Set"
        /// then the <see cref="FileOutputAdditionalHeaderText"/> property is set to null.
        /// </remarks>
        public LogConfig SetFileOutputAdditionalHeaderText(string additionalText)
        {
            FileOutputAdditionalHeaderText = additionalText;
            return this;
        }

        /// <summary>
        /// Specify any additional text to append to the whatever has already been added to the standard header information that is written before the first log
        /// is written to a file.
        /// This is the same as appending to the value of the <see cref="FileOutputAdditionalHeaderText"/> property.
        /// </summary>
        /// <param name="additionalTextToAdd">the text to append to the end of whatever already has been added to the header</param>
        /// <returns>a reference to this <c>LogConfig</c> object, such that further method calls may be chained</returns>
        /// <remarks>
        /// This duplicates the function of the <see cref="FileOutputAdditionalHeaderText"/> property setter, for the purpose of providing a fluent API.
        /// 
        /// If the value supplied for the <c>additionalText</c> argument is null, empty, or "Not Set"
        /// then nothing is added and the value of the <see cref="FileOutputAdditionalHeaderText"/> property is left unchanged.
        /// </remarks>
        public LogConfig ForFileOutputAddHeaderText(string additionalTextToAdd)
        {
            if (StringLib.HasSomething(additionalTextToAdd) && !additionalTextToAdd.Equals("Not Set"))
            {
                if (_fileOutputAdditionalHeaderText == null)
                {
                    _fileOutputAdditionalHeaderText = additionalTextToAdd;
                }
                else
                {
                    _fileOutputAdditionalHeaderText += additionalTextToAdd;
                }
                LogManager.IsCleared = false;
            }
            return this;
        }
        #endregion

        #region FileOutputAdditionalHeaderLines
        /// <summary>
        /// Get or set additional text to append after the standard header (that which is written before the first log)
        /// one separate lines. The default is null (no additional lines).
        /// </summary>
        /// <remarks>
        /// This is distinct from <see cref="FileOutputAdditionalHeaderText"/> in that that is inserted into the
        /// first header line, whereas this property represents text to be added as additional lines.
        /// </remarks>
        [DefaultValue(null)]
        public string FileOutputAdditionalHeaderLines
        {
            get { return _fileOutputAdditionalHeaderLines; }
            set
            {
                if (StringLib.HasSomething(value))
                {
                    _fileOutputAdditionalHeaderLines = value;
                }
                else
                {
                    _fileOutputAdditionalHeaderLines = null;
                }
            }
        }

        /// <summary>
        /// Specify any additional text to append after the standard header, on separate lines that come after the initial header-line.
        /// This is the same as setting the <see cref="FileOutputAdditionalHeaderLines"/> property.
        /// </summary>
        /// <param name="additionalText">the text to append as additional lines to the end of the header</param>
        /// <returns>a reference to this <c>LogConfig</c> object, such that further method calls may be chained</returns>
        /// <remarks>
        /// This duplicates the function of the <see cref="FileOutputAdditionalHeaderLines"/> property setter, for the purpose of providing a fluent API.
        /// 
        /// If the value supplied for the <c>additionalText</c> argument is null or empty,
        /// then the <see cref="FileOutputAdditionalHeaderLines"/> property is set to null.
        /// </remarks>
        public LogConfig SetFileOutputAdditionalHeaderLines(string additionalText)
        {
            if (StringLib.HasNothing(additionalText))
            {
                FileOutputAdditionalHeaderLines = null;
            }
            else
            {
                FileOutputAdditionalHeaderLines = additionalText;
            }
            return this;
        }
        #endregion

        #region file-output location

        #region FileOutputFilename
        /// <summary>
        /// Get or set the name (not the folder) of the log file to write file logging output to.
        /// Alert: The default is null, and this will return null unless you have already set this.
        /// </summary>
        /// <remarks>
        /// If you don't explicitly set this, then the first time something is logged to a file,
        /// this property is set to a default value as computed by calling the method <see cref="GetFileOutputFilenameDefaultValue"/>.
        /// 
        /// The default value is (assuming normal plain-text output)
        /// in the format:   "{subject-program name}_Log.txt"  (if there is a value for subject-program)
        /// otherwise it is: "LogNut_Log.txt".
        /// 
        /// If you set this to null, then upon re-reading this property the default value will again be computed.
        /// 
        /// If the <see cref="FileOutputFormatType"/> is <c>SimpleText</c>, the file extension is .txt
        /// For <c>Xml</c> the extension is .xml
        /// For <c>Json</c> the extension is .json.
        /// </remarks>
        [DefaultValue(null)]
        public string FileOutputFilename
        {
            get
            {
                return _fileOutputFilename;
            }
            set
            {
                // If the value is any empty value (null, an empty string, is just whitespace)
                // then make it set the value to null.
                if (StringLib.HasNothing(value))
                {
                    _fileOutputFilename = null;
                }
                else
                {
                    _fileOutputFilename = value;
                    LogManager.IsCleared = false;
                }
            }
        }

        /// <summary>
        /// Return the effective value for the <see cref="FileOutputFilename"/> property
        /// - which is the value to which it has been set already, or if it has not -
        /// then a computed default value via a call to method <see cref="GetFileOutputFilenameDefaultValue"/> and return that.
        /// This DOES NOT EVER return null.
        /// </summary>
        /// <returns>the effective value for the file-output folder</returns>
        /// <remarks>
        /// This computed-default value is (assuming normal plain-text output)
        /// in the format:   "{subject-program name}_Log.txt"  (if there is a value for subject-program)
        /// otherwise it is: "LogNut_Log.txt".
        /// It assumes there is no value for the subject-program name, if it is set to null, empty string, or "?".
        /// 
        /// The file extension depends upon the value of the <see cref="FileOutputFormatType"/>.
        /// For <c>SimpleText</c>, it is  ".txt"
        /// For <c>Xml</c>         it is  ".xml"
        /// For <c>Json</c>        it is  ".json"
        /// </remarks>
        public string GetFileOutputFilename_ExplicitlySetOrDefault()
        {
            if (StringLib.HasNothing(_fileOutputFilename))
            {
                return GetFileOutputFilenameDefaultValue();
            }
            else
            {
                return _fileOutputFilename;
            }
        }

        /// <summary>
        /// Get the value that would be provided by default for the filename to use for file output.
        /// </summary>
        /// <returns>the computed-default filename for logging file-output</returns>
        /// <remarks>
        /// This computed-default value is (assuming normal plain-text output)
        /// in the format:   "{subject-program name}_Log.txt"  (if there is a value for subject-program)
        /// otherwise it is of the form: "LogNut_Net*_Log.txt" (on .NET).
        /// It assumes there is no value for the subject-program name, if it is null.
        /// 
        /// The file extension depends upon the value of the <see cref="FileOutputFormatType"/>.
        /// For <c>SimpleText</c>, it is  ".txt"
        /// For <c>Xml</c>         it is  ".xml"
        /// For <c>Json</c>        it is  ".json"
        /// </remarks>
        public string GetFileOutputFilenameDefaultValue()
        {
            // A bit of thought went into deciding how this should be named.
            // Since multiple subject-programs might be writing logs to the same folder, it should by default have
            // the subject-program name in it somewhere.
            // Since the program's home folder might be being used, or a folder populated with the subject-program's
            // other files, the prefix "LogNut_" helps provide a visual aid to quickly spot this.
            // The suffix _Log is useful to also identify what this is.
            string extension;
            switch (FileOutputFormatType)
            {
                case LogFileFormatType.Json:
                    extension = ".json";
                    break;
                case LogFileFormatType.Xml:
                    extension = ".xml";
                    break;
                default:
                    extension = ".txt";
                    break;
            }
            if (StringLib.HasNothing(_subjectProgramName))
            {
                // This is called for it's side-effect of setting the _subjectProgramName instance-variable.
                GetSubjectProgramName(null);
            }
            // That should have given a value to _subjectProgramName, but if it failed to - give it a passable default..
            if (StringLib.HasNothing(_subjectProgramName))
            {
                // If SubjectProgramName is empty, then just put "LogNut" in it's place.
                return "LogNut_log" + extension;
            }
            else
            {
                string cleanSubjectProgram;
                // Leave off the "Hurst" namespace-prefix if it's there..
                if (_subjectProgramName.StartsWith("Hurst."))
                {
                    // If the program is one that starts with "Hurst.", remove that superfluous part.
                    cleanSubjectProgram = _subjectProgramName.WithoutAtStart("Hurst.");
                }
                else
                {
                    // Clean the subject-program name by removing all periods from it.
                    cleanSubjectProgram = _subjectProgramName.RemoveAll('.');
                }
                return cleanSubjectProgram + "_Log" + extension;
            }
        }

        /// <summary>
        /// Set the name of the log file to write file logging output to.
        /// If you don't set this then a default value will be computed. See <see cref="FileOutputFilename"/>.
        /// This is the same as setting the <see cref="FileOutputFilename"/> property.
        /// </summary>
        /// <param name="filename">the name of the file to log to - without the drive or directory information</param>
        /// <remarks>
        /// If you set this to null, then upon re-reading this property the default value will again be returned.
        /// This duplicates the function of the <see cref="FileOutputFilename"/> property setter, for the purpose of providing a fluent API.
        /// </remarks>
        /// <returns>a reference to this <c>LogConfig</c> object, such that further method calls may be chained</returns>
        public LogConfig SetFileOutputFilename(string filename)
        {
            FileOutputFilename = filename;
            return this;
        }
        #endregion

        #region FileOutputFolder
        /// <summary>
        /// Get or set the folder that loggers will write the log file output to.
        /// Unless this is explicitly set, it defaults to the value of the property <see cref="LogManager.FileOutputFolder_DefaultValue"/>.
        /// 
        /// This shall NOT EVER return null nor an empty string.
        /// 
        /// Special case: If you set this to a folder-value that begins with "ED:", it sets it to that folder but with the drive-letter of the running executable.
        /// </summary>
        /// <remarks>
        /// If you're intending to set <see cref="FileOutputArchiveFolder"/> to a folder
        /// that will be placed within this <c>FileOutputFolder</c>, and you intend to use other than the default file-output folder,
        /// set <c>FileOutputFolder</c> before you set <c>FileOutputArchiveFolder</c>.
        /// 
        /// If you set this property to null, then it is reset back to its default value the next time it is got.
        ///
        /// To make the logs appear within the executing program's own folder (assuming it's not Silverlight),
        /// call <see cref="SetFileOutputFolderToThatOfSubjectProgram"/>.
        /// 
        /// To set the file-output to a specific folder on the same disk-drive as that of the subject-program's executable
        /// call <c>SetFileOutputToFolderOnDriveOfExecutable</c>
        /// or else set this property to a value that begins with the text "ED:" (not applicable for Universal Windows Platform).
        /// For example, to set the file-output folder to be the folder "\ToolsLogOutput" but on the same drive as the subject-program
        /// is running from, set <c>FileOutputFolder</c> to "ED:\ToolsLogOutput".
        /// 
        /// Important Note when using "ED:": If the specified directory on that disk-drive does not already exist, this attempts to create it.
        /// If it is unable to create the directory, then it shifts to the same folder but on the C: drive
        /// assuming that to be more generally accessible to the user.
        /// 
        /// The reason for having this protocol is so that this behavior may be dictated via a configuration-file, which can only cause properties
        /// to be set (it cannot make method-calls).
        /// 
        /// Note: The DefaultValueAttribute is just to allow it to be written by the YamlReaderWriter class. The actual value has no effect.
        /// </remarks>
        [DefaultValue("Varies")]
        public string FileOutputFolder
        {
            get
            {
                // If the default value has not been overridden, then return the default value.
                if (_fileOutputFolder == null)
                {
                    _fileOutputFolder = LogManager.FileOutputFolder_DefaultValue;
                }
                return _fileOutputFolder;
            }
            set
            {
                // Handle the value (null or empty) first..
                if (StringLib.HasNothing(value))
                {
                    // Null indicates the default folder location.
                    _fileOutputFolder = null;
                }
                else
                {
                    _fileOutputFolder = value;
                }
                // If the Archive folder was defined to be within this, then that value is no longer valid.
                if (_isArchiveFolderRelative)
                {
                    // Setting this null forces it to be recomputed the next time it is accessed.
                    _fileOutputArchiveFolder_FullPath = null;
                }

#if !PRE_4 && OUTPUT_SVC
                //CBL The strategy of this policy of using the log-servicing process,
                // and setting it's values, needs to be carefully considered.
                if (IsWindowsServiceOutputEnabled)
                {
                    if (_fileOutputFolder != null)
                    {
                        IpcMessage ipcMessage = new IpcMessage();
                        ipcMessage.Operation = LogOperationId.SetFileOutputDir;
                        ipcMessage.Content = value;
                        ipcMessage.SubjectProgram = GetSubjectProgramName( typeof( LogManager ) );

                        if (LogManager.QueueOfLogMessagesToSendToSvc.TryAdd( ipcMessage, TimeSpan.FromSeconds( 5 ) ))
                        {
                            //CBL ?
                        }
                        else
                        {
                            NutUtil.WriteToConsoleAndInternalLog( "QueueOfLogMessagesToSend is full, unable to send message to set the output-dir. Sorry!" );
                        }
                    }
                }
#endif
            }
        }
        #endregion

        #region SetFileOutputFolder
        /// <summary>
        /// Set the directory to write the log file output to.
        /// If you don't set this then the default value from LogManager will be returned.
        /// This is the same as setting the FileOutputFolder property.
        /// Set this to String.Empty to write the log to the executable's folder.
        /// </summary>
        /// <param name="directoryToPutLogFilesInto">the directory in which the log file is to be placed</param>
        /// <returns>a reference to this LogConfig object, so that further method calls may be chained</returns>
        /// <remarks>
        /// This duplicates the function of the <see cref="FileOutputFolder"/> property setter. This is to provide a fluent API.
        /// 
        /// Special case:
        /// 
        /// To set the file-output to a specific folder on the same disk-drive as that of the subject-program's executable
        /// call <c>SetFileOutputToFolderOnDriveOfExecutable</c>
        /// or else call this method with a value that begins with the text "ED:" - which stands for 'the Executable's Drive'.
        /// For example, to set the file-output folder to be the folder "\ToolsLogOutput" but on the same drive as the subject-program
        /// is running from, call this with a <c>directoryToPutLogFilesInto</c> value of "ED:\ToolsLogOutput".
        /// 
        /// Important Note when using "ED:"
        /// 
        /// If the specified directory on that disk-drive does not already exist, this attempts to create it.
        /// If it is unable to create the directory, then it shifts to the same folder but on the C: drive
        /// assuming that to be more generally accessible to the user.
        /// </remarks>
        public LogConfig SetFileOutputFolder(string directoryToPutLogFilesInto)
        {
            FileOutputFolder = directoryToPutLogFilesInto;
            return this;
        }
        #endregion

        #region RemovableDrive
        /// <summary>
        /// After calling SetLogOutputToLogRcvrFlashDriveIfPresent, if the removable-drive is found
        /// - this returns the drive-letter (in the form "D:" where D is the drive-letter).
        /// When property <see cref="RemovableDrivePreferredFileOutputFolder"/> is set, this is also set.
        /// </summary>
        public string RemovableDrive { get; set; }

        #endregion

        #region RemovableDrivePreferredFileOutputFolder
        /// <summary>
        /// Get or set the preferred directory to write the log file output to - WHEN that drive is available.
        /// Setting this to null turns off secondary file-output.
        /// Default is null.
        /// </summary>
        /// <remarks>
        /// The default is null, indicating no secondary file-output is desired.
        ///
        /// Setting this property also sets the property <see cref="RemovableDrive"/> to hold just the drive-spec
        /// of the form "D:" (where D is the drive-letter).
        ///
        /// Some processing is done depending upon the value:
        /// 
        /// If you set this to "D": this property is set to "D:\" and RemovableDrive gets set to "D:".
        /// If you set this to "D:": this property is set to "D:\" and RemovableDrive gets set to "D:".
        /// If you set this to "D:\": this property is set to "D:\" and RemovableDrive gets set to "D:".
        /// If you set this to "D:\LogDir": this property is set to "D:\LogDir" and RemovableDrive gets set to "D:".
        /// </remarks>
        [DefaultValue(null)]
        public string RemovableDrivePreferredFileOutputFolder
        {
            get
            {
                return _removableDrivePreferredOutputFolder;
            }
            set
            {
                // Handle the value (null or empty) first..
                if (StringLib.HasNothing(value))
                {
                    _removableDrivePreferredOutputFolder = null;
                    RemovableDrive = null;
                }
                else
                {
                    if (value[0].IsEnglishAlphabetLetter())
                    {
                        // If the value being assigned consists only of a drive-letter, append a colon and back-slash
                        // to make it a legit folder, of the form "D:\" .
                        if (value.Length == 1)
                        {
                            RemovableDrive = value + ":";
                            _removableDrivePreferredOutputFolder = RemovableDrive + Path.DirectorySeparatorChar;
                            return;
                        }
                        else if (value.Length == 2 && value[1] == ':')
                        {
                            // If the value being assigned is of the form "D:", append a back-slash
                            // to make it a legit folder.
                            RemovableDrive = value;
                            _removableDrivePreferredOutputFolder = value + Path.DirectorySeparatorChar;
                            return;
                        }
                    }
                    _removableDrivePreferredOutputFolder = value;
                    RemovableDrive = FileStringLib.GetDrive(value);
                }
            }
        }
        #endregion

        #region SetFileOutputToPreferRemovableDrive
        /// <summary>
        /// Set the (preferred) directory to write the log file output to, when that preferred destination is on a
        /// removable drive which may be only intermittently available.
        /// This is intended to be an additional output destination, wherein the property FileOutputFolder is the fallback destination.
        /// 
        /// The filename is the same as for the primary log-output.
        /// 
        /// Setting this to null turns off secondary file-output.
        /// </summary>
        /// <param name="pathOfRemovableDriveOutputFolder">the path specifying the drive and directory to write log output to</param>
        /// <returns>a reference to this LogConfig object, so that further method calls may be chained</returns>
        public LogConfig SetFileOutputToPreferRemovableDrive(string pathOfRemovableDriveOutputFolder)
        {
            RemovableDrivePreferredFileOutputFolder = pathOfRemovableDriveOutputFolder;
            return this;
        }
        #endregion

        #region FileOutputPath
        /// <summary>
        /// Get the full pathname used for writing the logging output to, when writing to a file.
        /// If no folder has been specified, then only the filename is returned
        /// -- indicating possibly that it will be written into the executable's home folder..
        /// </summary>
        /// <remarks>
        /// This property is NOT saved in configuration files. You would include either or both of the propertiers
        /// <see cref="FileOutputFolder"/> and/or <see cref="FileOutputFilename"/>
        /// to control what the <c>FileOutputPath</c> would be.
        /// </remarks>
        [XmlIgnore]
        public string FileOutputPath
        {
            get
            {
                //CBL  I am thinking that I should be CERTAIN that this returns a complete path, never just a filename.
                string filenameToUse;
                if (StringLib.HasNothing(_fileOutputFilename))
                {
                    filenameToUse = GetFileOutputFilenameDefaultValue();
                }
                else
                {
                    filenameToUse = _fileOutputFilename;
                }
                string directory = FileOutputFolder;
                if (!String.IsNullOrEmpty(directory))
                {
                    return Path.Combine(directory, filenameToUse);
                }
                else
                {
                    return filenameToUse;
                }
            }
        }
        #endregion

        #region FileOutputArchiveFolder
        /// <summary>
        /// Get or set the filesystem-folder into which output files are to be moved
        /// when they rollover (that is - are "archived").
        /// If null (which is the default value) - the files are not moved.
        /// </summary>
        /// <remarks>
        /// If you're intending to set this to a folder that will be placed within the <c>FileOutputFolder</c>,
        /// set <c>FileOutputFolder</c> before you set this.
        /// 
        /// If you set this to an absolute path such as "C:\ArchiveFolder", then that is set as the value.
        /// 
        /// If you set this to a path with no drive, such as "ArchiveFolder", then
        /// it is placed within the FileOutputFolder.
        /// 
        /// If you set this to a path that has a root folder, but no drive, then
        /// it is placed within the same drive as the FileOutputFolder, but in the folder-location as given.
        /// 
        /// If you set this to a path that begins with "ED:", then the archive is set as the given folder
        /// but on the drive that this program is currently executing on.
        /// 
        /// Note: The DefaultValueAttirbute is just to allow it to be written by the YamlReaderWriter class. The actual value has no effect.
        /// </remarks>
        [DefaultValue("Varies")]
        public string FileOutputArchiveFolder
        {
            get { return _fileOutputArchiveFolder; }
            set
            {
                _fileOutputArchiveFolder = value;
                _fileOutputArchiveFolder_FullPath = null;

                if (StringLib.HasNothing(value))
                {
                    _fileOutputArchiveFolder = null;
                    _isArchiveFolderRelative = false;
                }
                else
                {
                    string drive, folder;
                    if (FileStringLib.StartsWithDriveAndRoot(value, out drive, out folder))
                    {
                        _isArchiveFolderRelative = false;
                    }
                    else if (FileStringLib.StartsWithRootButNoDrive(value))
                    {
                        _isArchiveFolderRelative = false;
                    }
                    else // neither drive nor root
                    {
                        _isArchiveFolderRelative = true;
                    }
                }
            }
        }

        /// <summary>
        /// Get the fuly-resolved filesystem-path of the Archive Folder.
        /// </summary>
        /// <remarks>
        /// The property <see cref="FileOutputArchiveFolder"/> is what you assign to,
        /// to control what to use for the Archive Folder.
        /// However, the actual property that you want, may be different.
        /// 
        /// For example, if you set <see cref="FileOutputArchiveFolder"/> to a folder
        /// that is within the <see cref="FileOutputFolder"/>, such as when
        /// <see cref="FileOutputFolder"/> is "C:\Logs" and you assign the value "Archive"
        /// to <see cref="FileOutputArchiveFolder"/>,
        /// then <see cref="FileOutputArchiveFolder"/> simply returns the exact same value
        /// that you assigned to it.
        /// You use *this* property, <see cref="FileOutputArchiveFolder_FullPath"/>,
        /// to get the fully-resoved path - which in this example would be "C:\Logs\Archive" .
        /// </remarks>
        public string FileOutputArchiveFolder_FullPath
        {
            get
            {
                if (_fileOutputArchiveFolder_FullPath == null)
                {
                    if (StringLib.HasSomething(_fileOutputArchiveFolder))
                    {
                        string drive, folder;
                        if (FileStringLib.StartsWithDriveAndRoot(_fileOutputArchiveFolder, out drive, out folder))
                        {
                            _fileOutputArchiveFolder_FullPath = _fileOutputArchiveFolder;
                        }
                        else if (FileStringLib.StartsWithRootButNoDrive(_fileOutputArchiveFolder))
                        {
                            // It does start with a \, but no drive.
                            string fileOutputFolder = this.FileOutputFolder;
                            string driveOfFileOutputFolder, folderOfFileOutputFolder;
                            bool ok = FileStringLib.GetDriveAndDirectory(fileOutputFolder, out driveOfFileOutputFolder, out folderOfFileOutputFolder);
                            if (ok && !String.IsNullOrEmpty(driveOfFileOutputFolder))
                            {
                                _fileOutputArchiveFolder_FullPath = driveOfFileOutputFolder + ":" + _fileOutputArchiveFolder;
                            }
                            else
                            {
                                _fileOutputArchiveFolder_FullPath = "C:" + _fileOutputArchiveFolder;
                            }
                        }
                        else // neither drive nor root
                        {
                            // This path value does not start with a root,
                            // so put it underneath the existing file-output folder.
                            string fileOutputFolder = this.FileOutputFolder;
                            _fileOutputArchiveFolder_FullPath = Path.Combine(fileOutputFolder, _fileOutputArchiveFolder);
                        }
                    }
                }
                return _fileOutputArchiveFolder_FullPath;
            }
        }

        /// <summary>
        /// Set the filesystem-folder into which output files are to be moved
        /// when they rollover (that is - are "archived").
        /// If null (which is the default value) - the files are not moved.
        /// </summary>
        /// <param name="archiveFolder">the filesystem-folder to use to put archived files into</param>
        /// <returns>a reference to this <c>LogConfig</c> object, such that further method calls may be chained</returns>
        public LogConfig SetFileOutputArchiveFolder(string archiveFolder)
        {
            FileOutputArchiveFolder = archiveFolder;
            return this;
        }

        #endregion

        #endregion  file-output location

        #region FileOutputFormatType
        /// <summary>
        /// Get or set the effective value of which format to use for writing log records to files - SimpleText, Xml, or Json.
        /// The default is <c>SimpleText</c>.
        /// </summary>
        [DefaultValue(LogFileFormatType.SimpleText)]
        public LogFileFormatType FileOutputFormatType
        {
            get { return _fileOutputFormatType; }
            set { _fileOutputFormatType = value; }
        }

        /// <summary>
        /// Set the format to use for writing log records to files.
        /// </summary>
        /// <param name="whichBasicFormatToUse">a LogFileFormatType value to say what type of format to use for the log files</param>
        /// <returns>a reference to this <c>LogConfig</c> object, such that further method calls may be chained</returns>
        /// <remarks>
        /// Calling this method is the same as setting the property FileOutputFormatType.
        /// </remarks>
        public LogConfig SetFileOutputFormatType(LogFileFormatType whichBasicFormatToUse)
        {
            _fileOutputFormatType = whichBasicFormatToUse;
            return this;
        }
        #endregion

        #region FileOutputRolloverMode
        /// <summary>
        /// Get or set the <see cref="RolloverMode"/> - which is the basis on which logging output is to be rolled over to new files.
        /// Possible values are <c>Size</c> (the default), <c>Date</c>, and <c>Composite</c>.
        /// Default is Size.
        /// </summary>
        [DefaultValue(RolloverMode.Size)]
        public RolloverMode FileOutputRolloverMode
        {
            get { return _fileOutputRolloverMode; }
            set { _fileOutputRolloverMode = value; }
        }

        /// <summary>
        /// Set the basis on which logging output is to be rolled over to new files.
        /// Possible values are <c>Size</c> (the default), <c>Date</c>, and <c>Composite</c>.
        /// </summary>
        /// <param name="newBasisForRollingOverFiles">a <see cref="RolloverMode"/> value to say how we want to create new generations of log-output files</param>
        /// <returns>a reference to this LogConfig object, such that further method calls may be chained</returns>
        /// <remarks>
        /// Calling this method is the same as setting the property <see cref="FileOutputRolloverMode"/>.
        /// </remarks>
        public LogConfig SetFileOutputRolloverMode(RolloverMode newBasisForRollingOverFiles)
        {
            FileOutputRolloverMode = newBasisForRollingOverFiles;
            return this;
        }
        #endregion

        #region FileOutputRollPoint
        /// <summary>
        /// Get or set the frequency with which the logging output is rolled over to a new file,
        /// if the <see cref="FileOutputRolloverMode"/> is set to either Date or Composite.
        /// Default is RollPoint.TopOfDay.
        /// </summary>
        /// <remarks>
        /// Possible values of the RollPoint enumeration type are: NoneSpecified, TopOfHour, TopOfDay (the default), TopOfWeek, and TopOfMonth.
        /// </remarks>
        [DefaultValue(RollPoint.TopOfDay)]
        public RollPoint FileOutputRollPoint
        {
            get { return _fileOutputRollPoint; }
            set { _fileOutputRollPoint = value; }
        }

        /// <summary>
        /// Set the RollPoint, which dictates the frequency with which the logging output is rolled over to new files,
        /// if the RolloverMode is set to either Date or Composite.
        /// The initial, default value of the property is RollPoint.TopOfDay.
        /// </summary>
        /// <param name="newRollPoint">a RollPoint value to say when to roll over files</param>
        /// <returns>a reference to this LogConfig object, such that further method calls may be chained</returns>
        /// <remarks>
        /// Possible values of the RollPoint enumeration type are: NoneSpecified, TopOfHour, TopOfDay (the default), TopOfWeek, and TopOfMonth.
        /// Calling this method is the same as setting the property FileOutputRollPoint.
        /// </remarks>
        public LogConfig SetFileOutputRollPoint(RollPoint newRollPoint)
        {
            FileOutputRollPoint = newRollPoint;
            return this;
        }
        #endregion

        #region IsFileOutputEnabled
        /// <summary>
        /// Get or set whether to enable output to a log file for all LogNut loggers on this AppDomain.
        /// If this is true, then file output happens on all loggers unless an individual logger has been disabled.
        /// If this is false, then file output happens only for those individual loggers which have set their respective property to true.
        /// Defaults to true - file output IS enabled.
        /// </summary>
        [DefaultValue(true)]
        public bool IsFileOutputEnabled { get; set; }

        /// <summary>
        /// Turn log-file output off.
        /// This is the same as setting the <see cref="IsFileOutputEnabled"/> property to false (it's initial-default value is <c>true</c>).
        /// </summary>
        /// <returns>a reference to this configuration object so that methods may be chained together</returns>
        /// <remarks>
        /// Calling this method is the same as setting the <see cref="IsFileOutputEnabled"/> property to false.
        /// The initial value of that property is <c>true</c>.
        /// </remarks>
        public LogConfig DisableFileOutput()
        {
            IsFileOutputEnabled = false;
            return this;
        }

        /// <summary>
        /// Turn on log-file output.
        /// This is the same as setting the <see cref="IsFileOutputEnabled"/> property to <c>true</c>.
        /// The initial value of that property is <c>true</c>.
        /// </summary>
        /// <returns>a reference to this configuration object so that methods may be chained together</returns>
        /// <remarks>
        /// Calling this method is the same as setting the <see cref="IsFileOutputEnabled"/> property to <c>true</c>.
        /// </remarks>
        public LogConfig EnableFileOutput()
        {
            IsFileOutputEnabled = true;
            return this;
        }
        #endregion

        #region IsFileOutputPrefixOfFixedWidth
        /// <summary>
        /// Get or set whether, for file-output, the prefix portion is to be of fixed width.
        /// The default is false.
        /// </summary>
        /// <remarks>
        /// This only applies when the <see cref="FileOutputFormatType"/> is <c>SimpleText</c>.
        /// </remarks>
        [DefaultValue(false)]
        public bool IsFileOutputPrefixOfFixedWidth
        {
            get { return _isFileOutputPrefixOfFixedWidth; }
            set { _isFileOutputPrefixOfFixedWidth = value; }
        }

        /// <summary>
        /// Set the flag that dictates that, for all file-output, the prefix portion is to be of fixed width.
        /// The default value of the <see cref="IsFileOutputPrefixOfFixedWidth"/> property is false.
        /// </summary>
        /// <param name="prefixWidth">this integer specifies the minimum width of the part of the prefix within brackets. Set this to zero to just let it grow as needed</param>
        /// <returns>a reference to this <c>LogConfig</c> object, such that further method calls may be chained</returns>
        /// <remarks>
        /// This only applies when the <see cref="FileOutputFormatType"/> is <c>SimpleText</c>.
        /// 
        /// Calling this method is the same as setting the property <see cref="IsFileOutputPrefixOfFixedWidth"/> to true,
        /// except that it provides you with a way to specify the <c>prefixWidth</c> also.
        /// 
        /// The parameter <c>prefixWidth</c> is useful for making the columns line up in
        /// the file output when outputing <see cref="LogFileFormatType.SimpleText"/>.
        /// It specifies the minimum width of the prefix -- or rather, that part of the "prefix" other than the timestamp.
        /// This refers to the part that comes within the "[]" brackets, inclusive of the brackets themselves.
        /// When a prefix merits a width GREATER than <c>prefixWidth</c>, that number is bumped up internally
        /// such that all subsequent output adopts that new value. Thus you can leave it at zero if you wish.
        /// </remarks>
        public LogConfig SetFileOutputPrefixToBeOfFixedWidth(int prefixWidth)
        {
            IsFileOutputPrefixOfFixedWidth = true;
            _prefixWidth = prefixWidth;
            return this;
        }

        /// <summary>
        /// Set the flag that dictates that, for all file-output, the prefix portion is to be of fixed width.
        /// The default value of the <see cref="IsFileOutputPrefixOfFixedWidth"/> property is false.
        /// </summary>
        /// <returns>a reference to this <c>LogConfig</c> object, such that further method calls may be chained</returns>
        /// <remarks>
        /// This only applies when the <see cref="FileOutputFormatType"/> is <c>SimpleText</c>.
        /// 
        /// Calling this method is the same as setting the property <see cref="IsFileOutputPrefixOfFixedWidth"/> to true.
        /// 
        /// This is useful for making the columns line up in
        /// the file output when outputing <see cref="LogFileFormatType.SimpleText"/>.
        /// The initial value of the minimum width of the prefix -- or rather, that part of the "prefix" other than the timestamp,
        /// is zero, and that is increased with successive log-outputs to grow as needed.
        /// This refers to the part that comes within the "[]" brackets, inclusive of the brackets themselves.
        /// </remarks>
        public LogConfig SetFileOutputPrefixToBeOfFixedWidth()
        {
            IsFileOutputPrefixOfFixedWidth = true;
            _prefixWidth = 0;
            return this;
        }
        #endregion

        #region IsFileOutputSpreadsheetCompatible
        /// <summary>
        /// Get or set whether all file-output must be capable of being pasted into an Excel spreadsheet.
        /// The default is false.
        /// </summary>
        /// <remarks>
        /// This only applies when the <see cref="FileOutputFormatType"/> is <c>SimpleText</c>.
        /// </remarks>
        [DefaultValue(false)]
        public bool IsFileOutputSpreadsheetCompatible
        {
            get { return _isFileOutputSpreadsheetCompatible; }
            set { _isFileOutputSpreadsheetCompatible = value; }
        }

        /// <summary>
        /// Set the flag that denotes whether all file-output must be capable of being pasted into an Excel spreadsheet.
        /// The default value is false.
        /// </summary>
        /// <returns>a reference to this <c>LogConfig</c> object, such that further method calls may be chained</returns>
        /// <remarks>
        /// This only applies when the <see cref="FileOutputFormatType"/> is <c>SimpleText</c>.
        /// 
        /// Calling this method is the same as setting the property <see cref="IsFileOutputSpreadsheetCompatible"/> to <c>true</c>.
        /// </remarks>
        public LogConfig SetFileOutputToBeSpreadsheetCompatible()
        {
            _isFileOutputSpreadsheetCompatible = true;
            return this;
        }
        #endregion

        #region IsFileOutputToCompressFiles
        /// <summary>
        /// Get or set the effective value that dictates whether to compress the output files
        /// when we roll-over to a new file.
        /// The default is false.
        /// </summary>
        [DefaultValue(false)]
        public bool IsFileOutputToCompressFiles
        {
            get { return _isFileOutputToCompressFiles; }
            set { _isFileOutputToCompressFiles = value; }
        }

        /// <summary>
        /// Set whether to compress the output files when they are rolled-over to new files.
        /// The default behavior is not to do so.
        /// </summary>
        /// <param name="isToCompress">true to compress, false to leave them as they are</param>
        /// <returns>a reference to this LogConfig object, such that further method calls may be chained</returns>
        /// <remarks>
        /// Calling this method is the same as setting the IsFileOutputToCompressFiles property.
        /// </remarks>
        public LogConfig SetFileOutputToCompressFilesOnRollover(bool isToCompress)
        {
            IsFileOutputToCompressFiles = isToCompress;
            return this;
        }
        #endregion

        #region IsFileOutputToInsertHeader
        /// <summary>
        /// Get or set whether to insert a line of header text into the log-file
        /// at the first log transmission after each time the subject-program has launched.
        /// Default is true.
        /// </summary>
        /// <remarks>
        /// The header text is of this format:
        /// ----[ LogNut start of {program-name}, version {program-version} on Friday, 2011-05-20 5:27:48 ]----
        /// </remarks>
        [DefaultValue(true)]
        public bool IsFileOutputToInsertHeader
        {
            get { return _isFileOutputToInsertHeader; }
            set { _isFileOutputToInsertHeader = value; }
        }

        /// <summary>
        /// Set whether to insert a line of header text into the log-file
        /// at the first log transmission after each time the subject-program has launched.
        /// This is the same as setting the property <see cref="IsFileOutputToInsertHeader"/> .
        /// The initial value of that property is true.
        /// </summary>
        /// <param name="isToInsertHeader">if true, insert the header - otherwise don't</param>
        /// <returns>a reference to this <c>LogConfig</c> object so that methods may be chained together</returns>
        /// <remarks>
        /// Calling this method is the same as setting the LogManager property <see cref="IsFileOutputToInsertHeader"/>.
        /// The banner-text is of this format:
        /// ----[ LogNut start of {program-name}, version {program-version} on Friday, 2011-05-20 5:27:48 ]----
        /// </remarks>
        public LogConfig SetFileOutputToInsertHeader(bool isToInsertHeader)
        {
            IsFileOutputToInsertHeader = isToInsertHeader;
            return this;
        }
        #endregion

        #region IsFileOutputToInsertLineBetweenTraces
        /// <summary>
        /// Get whether to separate log traces within the log file with an empty line.
        /// The default is true.
        /// </summary>
        [DefaultValue(true)]
        public bool IsFileOutputToInsertLineBetweenTraces
        {
            get { return _isFileOutputToInsertLineBetweenTraces; }
            set { _isFileOutputToInsertLineBetweenTraces = value; }
        }

        /// <summary>
        /// Turn on or off the inserting of blank lines between traces within the log-file output.
        /// This is the same as setting the <see cref="IsFileOutputToInsertLineBetweenTraces"/> property.
        /// The default value of that property is true.
        /// </summary>
        /// <param name="isToInsertNewlinesBetweenTraces">if true, insert the lines - otherwise don't</param>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        /// <remarks>
        /// CBL Rename this?
        /// Calling this method is the same as setting the LogManager property IsFileOutputToInsertLineBetweenTraces to true.
        /// </remarks>
        public LogConfig SetFileOutputToInsertLineBetweenTraces(bool isToInsertNewlinesBetweenTraces)
        {
            IsFileOutputToInsertLineBetweenTraces = isToInsertNewlinesBetweenTraces;
            return this;
        }
        #endregion

        #region IsToCreateNewOutputFileUponStartup
        /// <summary>
        /// Get or set whether to create a new, empty output-file upon the first log-output when the subject-program is run,
        /// rolling-over any pre-existing output file to a new pathname in order to save it's content -
        /// as opposed to just appending to the exiting contents of the file.
        /// The default value is true (rollover any pre-existing output file and start with a fresh, new empty output file).
        /// Note: This was renamed from IsFileOutputToOverwriteFile.
        /// </summary>
        /// <remarks>
        /// Setting this property has no effect on the <see cref="FileOutputRolloverMode"/> setting. The output-file will still be
        /// rolled-over at the proper times.
        /// 
        /// If you do NOT want your output-file to rollover, then set the <see cref="MaxNumberOfFileRollovers"/> property to 0 (zero).
        /// </remarks>
        [DefaultValue(true)]
        public bool IsToCreateNewOutputFileUponStartup { get; set; }

        /// <summary>
        /// Set LogNut to, upon startup, just continue appending to the output-file as it had before
        /// as opposed to creating a fresh output-file at each run.
        /// This is not the default behavior, which is to create a new, empty file upon each startup of the subject-program
        /// while rolling-over any pre-existing output file.
        /// </summary>
        /// <returns>a reference to this LogConfig object, such that further method calls may be chained</returns>
        /// <remarks>
        /// Calling this method is the same as setting the <see cref="IsToCreateNewOutputFileUponStartup"/> property to false.
        /// 
        /// Note: After calling this method, the output-file will still be rolled-over in accordance with the <see cref="FileOutputRolloverMode"/> setting.
        /// 
        /// If you do NOT want your output-file to rollover, then set the <see cref="MaxNumberOfFileRollovers"/> property to 0 (zero).
        /// </remarks>
        public LogConfig SetFileOutputToAppendToExistingFile()
        {
            IsToCreateNewOutputFileUponStartup = false;
            return this;
        }

        /// <summary>
        /// Set LogNut to create a new, empty output-file upon the first log-output when the subject-program is run,
        /// rolling-over any pre-existing output file to a new pathname in order to save it's content -
        /// as opposed to just appending to the exiting file.
        /// This is the default behavior.
        /// </summary>
        /// <returns>a reference to this LogConfig object, such that further method calls may be chained</returns>
        /// <remarks>
        /// Calling this method is the same as setting the <see cref="IsToCreateNewOutputFileUponStartup"/> property to true.
        /// 
        /// Note: After calling this method, when the first log output occurs the output-file will be
        /// rolled-over in accordance with the <see cref="FileOutputRolloverMode"/> setting.
        /// 
        /// If you do NOT want your output-file to rollover, then set the <see cref="MaxNumberOfFileRollovers"/> property to 0 (zero).
        /// </remarks>
        public LogConfig SetFileOutputToCreateNewFileUponStartup()
        {
            IsToCreateNewOutputFileUponStartup = true;
            return this;
        }
        #endregion

        #region IsFileOutputToUseStdTerminator
        /// <summary>
        /// Get or set whether to append our standard terminator-character (which is Unicode 00B6)
        /// to every log-record. The default is true.
        /// </summary>
        [DefaultValue(true)]
        public bool IsFileOutputToUseStdTerminator
        {
            get { return _isFileOutputToUseStdTerminator; }
            set { _isFileOutputToUseStdTerminator = value; }
        }

        private bool _isFileOutputToUseStdTerminator = true;

        /// <summary>
        /// Set to append our standard terminator-character (which is Unicode 00B6)
        /// to every log-record, as opposed to just new-lines. The default is true.
        /// </summary>
        /// <returns>a reference to this LogConfig object, such that further method calls may be chained</returns>
        /// <remarks>
        /// Calling this method is the same as setting the IsFileOutputToUseStdTerminator property to true.
        /// </remarks>
        public LogConfig SetFileOutputToUseStdTerminator()
        {
            IsFileOutputToUseStdTerminator = true;
            return this;
        }

        /// <summary>
        /// Set to append a new-line to every log-record
        /// as opposed to our standard terminator (which is Unicode 00B6) which is used by default..
        /// </summary>
        /// <returns>a reference to this LogConfig object, such that further method calls may be chained</returns>
        /// <remarks>
        /// Calling this method is the same as setting the IsFileOutputToUseStdTerminator property to false.
        /// </remarks>
        public LogConfig SetFileOutputToUseNewlineInsteadOfStdTerminator()
        {
            IsFileOutputToUseStdTerminator = false;
            return this;
        }
        #endregion

        #region IsFileOutputToWriteThrough
        /// <summary>
        /// Get or set whether to explicitly flush all buffered file intermediate buffers to disk upon every log-output operation that 
        /// includes file-output. There is some speed-penalty for this.
        /// Default is false - do NOT flush output.
        /// </summary>
        [DefaultValue(false)]
        public bool IsFileOutputToWriteThrough { get; set; }

        /// <summary>
        /// Set to explicitly flush all buffered file intermediate buffers to disk upon every log-output operation that
        /// includes file-output. There is some speed-penalty for this.
        /// Default is false - do NOT flush output.
        /// </summary>
        /// <returns>a reference to this configuration object so that methods may be chained together</returns>
        /// <remarks>
        /// Calling this method is the same as setting the <see cref="IsFileOutputToWriteThrough"/> property.
        /// </remarks>
        public LogConfig SetFileOutputToWriteThrough()
        {
            IsFileOutputToWriteThrough = true;
            return this;
        }
        #endregion

        #region IsToOpenCloseOutputFileEveryTime

        /// <summary>
        /// Get or set whether to open the output-file and then close it for every log-request, as opposed to keeping that file open.
        /// It is faster to keep it open, but also more likely to lose log-output in case of a crash.
        /// you can also call <see cref="SetToOpenAndCloseOutputFileEveryTime"/> to set
        /// and <see cref="SetToKeepOutputFileOpenBetweenWrites"/> to clear this.
        /// Default is true - open-close every time.
        /// </summary>
        [DefaultValue(true)]
        public bool IsToOpenCloseOutputFileEveryTime
        {
            get { return _isToOpenCloseOutputFileEveryTime; }
            set { _isToOpenCloseOutputFileEveryTime = value; }
        }

        public LogConfig SetToOpenAndCloseOutputFileEveryTime()
        {
            IsToOpenCloseOutputFileEveryTime = true;
            return this;
        }

        /// <summary>
        /// Set whether to open the output-file and then close it for every log-request, as opposed to keeping that file open.
        /// It is faster to keep it open, but also more likely to lose log-output in case of a crash.
        /// This is the same as setting property <see cref="LogConfig.IsToOpenCloseOutputFileEveryTime"/> to false.
        /// The initial, default value of that property is true - open-close every time.
        /// </summary>
        /// <remarks>
        /// It is faster to keep it open between writes, but also more likely to lose log-output in case of a crash.
        /// </remarks>
        public LogConfig SetToKeepOutputFileOpenBetweenWrites()
        {
            IsToOpenCloseOutputFileEveryTime = false;
            return this;
        }

        private bool _isToOpenCloseOutputFileEveryTime = true;

        #endregion IsToOpenCloseOutputFileEveryTime

        #region MaxFileSize
        /// <summary>
        /// Get or set the maximum that you want to allow your log-file to grow to,
        /// before it is forced to rollover as new log records come in.
        /// The default is 2 Gigabytes (2,000,000,000 - not 2,147,483,648).
        /// Special note: Setting this to zero results in it being set to it's upper limit.
        /// </summary>
        /// <remarks>
        /// <c>MaxFileSize_DefaultValue</c> denotes the default value for this property.
        /// </remarks>
        [DefaultValue(2000000000)]
        public Int64 MaxFileSize
        {
            get { return _maxFileSize; }
            set
            {
                if (value == 0)
                {
                    _maxFileSize = MaxFileSize_UpperLimit;
                }
                else if (value > MaxFileSize_UpperLimit)
                {
                    throw new ArgumentOutOfRangeException("value",
                                                          value,
                                                          String.Format("The value ({0}) is too great for MaxFileSize.", value));
                }
                else if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value",
                                                          value,
                                                          "The value for MaxFileSize must be >= zero (zero would be the equivalent of setting it to it's uppermost limit).");
                }
                else
                {
                    _maxFileSize = value;
                }
            }
        }

        /// <summary>
        /// Get the default value for the MaxFileSize property - which is 2 Gigabytes.
        /// </summary>
        public Int64 MaxFileSize_DefaultValue
        {
            get { return _maxFileSize_DefaultValue; }
        }

        /// <summary>
        /// This constant number is the absolute upper-limit to what the maximum size of log files may be set to.
        /// This is 100 Gigabytes.
        /// </summary>
        /// <remarks>
        /// I chose this value to be > UInt32, because with contemporary filesystems it's reasonable to assume a user may desire
        /// such large files. I don't find a clear, universal limit to the practical size on NTFS.
        /// </remarks>
        public readonly static Int64 MaxFileSize_UpperLimit = 100000000000;


        /// <summary>
        /// Set the maximum that you want to allow your log-file to grow to, before it is forced to rollover as new log records come in.
        /// This is the same as setting the MaxFileSize property.
        /// The initial default value of that property is 2 Gigabytes.
        /// </summary>
        /// <param name="upperLimitInBytes">the limit that you want to set on the file size, in bytes (roughly)</param>
        /// <remarks>
        /// This duplicates the function of the MaxFileSize property setter (to provide a fluent API).
        /// </remarks>
        /// <returns>a reference to this FileOutputConfiguration object, such that further method calls may be chained</returns>
        public LogConfig SetMaxFileSize(Int64 upperLimitInBytes)
        {
            MaxFileSize = upperLimitInBytes;
            return this;
        }
        #endregion

        #region MaxNumberOfFileRollovers
        /// <summary>
        /// Get or set the maximum number of backup files that are kept before the oldest is erased.
        /// Default is 100.
        /// Upper limit is 1000, as denoted by MaxNumberOfFileRollovers_UpperLimit.
        /// Zero means NO rollovers.
        /// </summary>
        /// <remarks>
        /// If this is set to zero, there will be no backup files and the log file
        /// will simply be truncated when it reaches MaxFileSize.
        /// </remarks>
        /// <exception cref="ArgumentOutOfRangeException">The value must be within 0..1000</exception>
        [DefaultValue(100)]
        public int MaxNumberOfFileRollovers
        {
            //CBL  TODO:  This limit is not working currently (2022/5/20)
            // in a default configuration.
            get
            {
                return _maxNumberOfFileRollovers;
            }
            set
            {
                if (value > MaxNumberOfFileRollovers_UpperLimit)
                {
                    throw new ArgumentOutOfRangeException("value",
                                                           value,
                                                           "The upper limit is " + MaxNumberOfFileRollovers_UpperLimit);
                }
                else if (value < 0)
                {
                    throw new ArgumentOutOfRangeException("value",
                                                           value,
                                                           "must be >= zero");
                }
                _maxNumberOfFileRollovers = value;
            }
        }

        /// <summary>
        /// This represents the upper-limit that the property MaxNumberOfFileRollovers can be set to.
        /// This value is 1,000 (one thousand).
        /// </summary>
        public const int MaxNumberOfFileRollovers_UpperLimit = 1000;

        /// <summary>
        /// Set the maximum number of backup files that are kept before the oldest is erased.
        /// This is the same as setting the MaxNumberOfFileRollovers property.
        /// Zero means NO rollovers.
        /// The initial default value of the <c>MaxNumberOfFileRollovers</c> property is 100.
        /// </summary>
        /// <param name="upperLimitOfNumberOfRollovers">the maximum number of times the log-output-file will be rolled over into a new file</param>
        /// <remarks>
        /// The can be zero, and the upper limit is 1000, as denoted by <c>MaxNumberOfFileRollovers_UpperLimit</c>.
        /// This duplicates the function of the MaxNumberOfFileRollovers property setter. This is to provide a fluent API.
        /// </remarks>
        /// <returns>a reference to this LogConfig object, such that further method calls may be chained</returns>
        public LogConfig SetMaxNumberOfFileRollovers(int upperLimitOfNumberOfRollovers)
        {
            MaxNumberOfFileRollovers = upperLimitOfNumberOfRollovers;
            return this;
        }
        #endregion

        #region when filename includes the date and time

        #region IsFileOutputFilenameToIncludeDateTime
        /// <summary>
        /// Get or set whether to we are appending the current date-and-time to the file-output filename starting with the next log-output.
        /// Call the method <see cref="SetFileOutputFilenameToIncludeDateTime"/> to set this.
        /// The initial, default value of this property is false - do NOT append anything to the filename.
        /// </summary>
        [DefaultValue(false)]
        public bool IsFileOutputFilenameToIncludeDateTime
        {
            get { return NutFileLib.IsFileOutputFilenameUsingDate; }
            set { NutFileLib.IsFileOutputFilenameUsingDate = value; }
        }
        #endregion

        #region SetFileOutputFilenameToIncludeDateTime
        /// <summary>
        /// Set to append the current date-and-time to the file-output filename starting with the next log-output.
        /// The initial, default behavior is NOT to append anything to the configured filename.
        /// </summary>
        /// <returns>a reference to this configuration object so that methods may be chained together</returns>
        /// <remarks>
        /// You would configure this behavior by calling this method instead of <see cref="SetFileOutputFilename"/>.
        /// 
        /// The resulting filename will be of the form {base-filename}_YYYYMMDD-HHMMSS.{ext},
        /// where {base-filename} is <paramref name="originalFilenameWithoutExtension"/>,
        /// YYYYMMDD-HHMMSS is the date and time of the log output:
        ///   YYYY = 4-digit year
        ///   MM   = 2-digit month (zero-padded to the left for single-digit month values)
        ///   DD   = 2-digit day of the month
        ///   HH   = 2-digit hour (using a 24-hour format)
        ///   MM   = 2-digit minutes
        ///   SS   = 2-digit seconds.
        /// and {ext}  is the filename-extension as set by the argument provided to <paramref name="extenstion"/>.
        /// 
        /// After calling this method the property <see cref="IsFileOutputToWriteThrough"/> will return true.
        /// </remarks>
        public LogConfig SetFileOutputFilenameToIncludeDateTime(string originalFilenameWithoutExtension, string extension)
        {
            if (extension.Equals("ETL", StringComparison.OrdinalIgnoreCase))
            {
                if (!LogManager.IsUsingETW)
                {
                    throw new ArgumentException(message: "You must set this to use ETW before assigning an extension of .ETL", paramName: nameof(extension));
                }
            }
            FileOutputFilename = NutFileLib.CreateLogOutputFilenameWithDate(originalFilenameWithoutExtension: originalFilenameWithoutExtension, extension: extension);
            return this;
        }
        #endregion

        #region SetFileOutputFilenameForCurrentTime
        /// <summary>
        /// Set the file-output filename suffix to reflect the date and time of right now.
        /// This has no effect if the usage of date-time in the filename has not been set by calling <see cref="SetFileOutputFilenameToIncludeDateTime"/>.
        /// </summary>
        public void SetFileOutputFilenameForCurrentTime()
        {
            if (IsFileOutputFilenameToIncludeDateTime)
            {
                FileOutputFilename = NutFileLib.RecreateFileOutputFilename();
            }
        }
        #endregion

        #endregion when filename includes the date and time

        #endregion output to file

        #region log prefix content

        #region IsToShowCategory
        /// <summary>
        /// Get or set whether to display the "Category" within a log output. 
        /// False by default.
        /// CBL  This is still being debated.
        /// </summary>
        [DefaultValue(false)]
        public bool IsToShowCategory
        {
            get { return _isToShowCategory; }
            set { _isToShowCategory = value; }
        }
        #endregion

        #region IsToShowId
        /// <summary>
        /// Get or set whether to display the log-record Id in the prefix for each log record.
        /// Default is false. 
        /// </summary>
        [DefaultValue(false)]
        public bool IsToShowId
        {
            get { return _isToShowId; }
            set { _isToShowId = value; }
        }

        /// <summary>
        /// Enable the showing of the Id for each log-record.
        /// This is the same as setting the IsToShowId property to true.
        /// That property is initially false.
        /// </summary>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        public LogConfig ShowId()
        {
            IsToShowId = true;
            return this;
        }
        #endregion

        #region IsToShowLevel
        /// <summary>
        /// Get or set whether to include the log-level in the prefix for each log record.
        /// Default is true.
        /// </summary>
        [DefaultValue(true)]
        public bool IsToShowLevel
        {
            get { return _isToShowLevel; }
            set { _isToShowLevel = value; }
        }

        /// <summary>
        /// Disable the showing of the Level for each log-record.
        /// This is the same as setting the IsToShowLevel property to false.
        /// That property is initially true.
        /// </summary>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        public LogConfig DontShowLevel()
        {
            IsToShowLevel = false;
            return this;
        }
        #endregion

        #region IsToShowLevelOnlyForWarningsAndAbove
        /// <summary>
        /// Get or set whether to include the log-level in the prefix for each log record only when that level is a Warning, Error, or Critical.
        /// Default is false - all levels are displayed.
        /// </summary>
        [DefaultValue(false)]
        public bool IsToShowLevelOnlyForWarningsAndAbove
        {
            get { return _isToShowLevelOnlyForWarningsAndAbove; }
            set { _isToShowLevelOnlyForWarningsAndAbove = value; }
        }

        /// <summary>
        /// Enable the showing of the Level for each log-record only when that level is at LogLevel.Warning or higher,
        /// that is - for Warning, Error, and Critical.
        /// </summary>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        /// <remarks>
        /// The intent of this is to permit further uncluttering of the log-file for non-important log traces.
        /// It may be desired to see "Warning", "Error", or "Critical" but not to distinguish the level of output that is less critical than that. 
        /// </remarks>
        public LogConfig ShowLevelOnlyForWarningsAndAbove()
        {
            _isToShowLevelOnlyForWarningsAndAbove = true;
            _isToShowLevel = true;
            return this;
        }
        #endregion

        #region IsToShowLoggerName
        /// <summary>
        /// Get or set whether to include the name of the logger in the log output. 
        /// Default is true; show the logger's name, unless it is the default logger.
        /// </summary>
        [DefaultValue(true)]
        public bool IsToShowLoggerName
        {
            get { return _isToShowLoggerName; }
            set { _isToShowLoggerName = value; }
        }

        /// <summary>
        /// Turn on the inclusion of the name of the logger in the log output. By default this setting is on.
        /// This is the same as setting the IsToShowLoggerName property to true.
        /// </summary>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        public LogConfig ShowLoggerName()
        {
            IsToShowLoggerName = true;
            return this;
        }

        /// <summary>
        /// Turn off the inclusion of the name of the logger in the log output. By default this setting is on.
        /// This is the same as setting the IsToShowLoggerName property to false.
        /// </summary>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        public LogConfig DontShowLoggerName()
        {
            IsToShowLoggerName = false;
            return this;
        }
        #endregion

        #region IsToShowLoggerNameWhenDefault
        /// <summary>
        /// Get or set whether to include the logger's name within the prefix even when it's name is the default logger-name.
        /// This is false by default.
        /// </summary>
        [DefaultValue(false)]
        public bool IsToShowLoggerNameWhenDefault
        {
            get { return _isToShowLoggerNameWhenDefault; }
            set { _isToShowLoggerNameWhenDefault = value; }
        }

        /// <summary>
        /// Turn on the inclusion of the name of the logger in the log output *even* when it is the default-logger name. By default this setting is false.
        /// This is the same as setting the IsToShowLoggerNameWhenDefault property to true.
        /// </summary>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        public LogConfig ShowLoggerNameEvenWhenDefault()
        {
            IsToShowLoggerNameWhenDefault = true;
            return this;
        }

        /// <summary>
        /// Turn off the inclusion of the name of the logger in the log output when that is the default-logger name. By default this setting is false.
        /// This is the same as setting the IsToShowLoggerNameWhenDefault property to false.
        /// </summary>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        public LogConfig DontShowLoggerNameWhenDefault()
        {
            IsToShowLoggerNameWhenDefault = false;
            return this;
        }
        #endregion

        #region IsToShowPrefix
        /// <summary>
        /// Get or set whether include a prefix for each log record.
        /// Default is true.
        /// </summary>
        /// <remarks>
        /// Here "Prefix" refers to the entire additional part of what is written to file output in addition to the actual message body,
        /// meaning both the timestamp and/or elapsed time, and the fields within the brackets.
        /// </remarks>
        [DefaultValue(true)]
        public bool IsToShowPrefix
        {
            get { return _isToShowPrefix; }
            set { _isToShowPrefix = value; }
        }

        /// <summary>
        /// Disable showing the prefix for each log-record. This is the same as setting the IsToShowPrefix property to false.
        /// The default value of the property is true.
        /// </summary>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        /// <remarks>
        /// Here "Prefix" refers to the entire additional part of what is written to file output in addition to the actual message body,
        /// meaning both the timestamp and/or elapsed time, and the fields within the brackets.
        /// </remarks>
        public LogConfig DontShowPrefix()
        {
            IsToShowPrefix = false;
            return this;
        }
        #endregion

        #region IsToShowSourceFile
        /// <summary>
        /// Get or set whether to include the source-file-path of the logging statement within the contextual-information
        /// of the logging output, when calling any of the Log..WithContext methods (applies to .NET versions above 4.0).
        /// Default is <c>true</c>.
        /// </summary>
        [DefaultValue(true)]
        public bool IsToShowSourceFile
        {
            get { return _isToShowSourceFile; }
            set { _isToShowSourceFile = value; }
        }

        /// <summary>
        /// Include the source-file-path of the logging statement within the contextual-information
        /// of the logging output, when calling any of the Log..WithContext methods (applies to .NET versions above 4.0).
        /// That property defaults to true, so you do not need to call this unless it has been turned off in some way.
        /// </summary>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        /// <remarks>
        /// Calling this method is the same as setting the <see cref="IsToShowSourceFile"/> property to <c>true</c>.
        /// </remarks>
        public LogConfig ShowSourceFile()
        {
            IsToShowSourceFile = true;
            return this;
        }

        /// <summary>
        /// Do not include the source-file-path of the logging statement within the contextual-information
        /// of the logging output, when calling any of the Log..WithContext methods (applies to .NET versions above 4.0).
        /// That property defaults to true.
        /// </summary>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        /// <remarks>
        /// Calling this method is the same as setting the <see cref="IsToShowSourceFile"/> property to <c>false</c>.
        /// </remarks>
        public LogConfig DontShowSourceFile()
        {
            IsToShowSourceFile = false;
            return this;
        }
        #endregion

        #region IsToShowSourceHost
        /// <summary>
        /// Get or set whether to include the machine-name in the prefix for each log record.
        /// Default is false.
        /// </summary>
        [DefaultValue(false)]
        public bool IsToShowSourceHost
        {
            get { return _isToShowSourceHost; }
            set { _isToShowSourceHost = value; }
        }

        /// <summary>
        /// Enable the showing of the computer that the subject-program was running on, for each log-record.
        /// This is the same as setting the IsToShowSourceHost property to true.
        /// That property defaults to false.
        /// </summary>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        public LogConfig ShowSourceHost()
        {
            IsToShowSourceHost = true;
            return this;
        }
        #endregion

        #region IsToShowStackTraceForExceptions
        /// <summary>
        /// Get or set whether to include the full stack-trace in the log record when logging an Exception.
        /// Default is true (yes - show the stack-information).
        /// </summary>
        /// <remarks>
        /// Under .NET 4.5, if this is false then use is made of the compiler-generated caller information to show the source-file, method-name and line-number.
        /// CBL  is that correct?
        /// </remarks>
        [DefaultValue(true)]
        public bool IsToShowStackTraceForExceptions
        {
            get { return _isToShowStackTraceForExceptions; }
            set { _isToShowStackTraceForExceptions = value; }
        }
        #endregion

        #region IsToShowSubjectProgram
        /// <summary>
        /// Get or set whether to include the name of the subject-program within the prefix for each log record.
        /// Default is false (don't show it).
        /// </summary>
        [DefaultValue(false)]
        public bool IsToShowSubjectProgram
        {
            get { return _isToShowSubjectProgram; }
            set { _isToShowSubjectProgram = value; }
        }

        /// <summary>
        /// Enable the showing of the name of the subject-program within the prefix for each log-record.
        /// This is the same as setting the IsToShowSubjectProgram property to true.
        /// The default value of that property is false.
        /// </summary>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        public LogConfig ShowSubjectProgram()
        {
            IsToShowSubjectProgram = true;
            return this;
        }
        #endregion

        #region IsToShowSubjectProgramVersion
        /// <summary>
        /// Get or set whether to include the version of the subject-program
        /// within the prefix for each log record.
        /// Default is false (do not show the version).
        /// </summary>
        [DefaultValue(false)]
        public bool IsToShowSubjectProgramVersion
        {
            get { return _isToShowSubjectProgramVersion; }
            set { _isToShowSubjectProgramVersion = value; }
        }

        /// <summary>
        /// Enable the showing of the subject-program version within the prefix for each log-record.
        /// This is the same as setting the IsToShowSubjectProgramVersion property to true.
        /// The default value of that property is false.
        /// </summary>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        public LogConfig ShowSubjectProgramVersion()
        {
            IsToShowSubjectProgramVersion = true;
            return this;
        }
        #endregion

        #region IsToShowThread
        /// <summary>
        /// Get or set whether to include the thread-identifier in the log output.
        /// Default is false (not showing the thread).
        /// </summary>
        [DefaultValue(false)]
        public bool IsToShowThread
        {
            get { return _isToShowThread; }
            set { _isToShowThread = value; }
        }

        /// <summary>
        /// Enable the showing of the execution-thread identifier within the prefix for each log-record.
        /// This is the same as setting the IsToShowThread property to true.
        /// The default value of that property is false.
        /// </summary>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        public LogConfig ShowThread()
        {
            IsToShowThread = true;
            return this;
        }
        #endregion

        #region IsToShowUser
        /// <summary>
        /// Get or set whether to include the Username of the user who is executing the subject-program
        /// within the prefix for each log record.
        /// Default is false (do not display the user).
        /// </summary>
        /// <remarks>
        /// Note regarding the nomenclature: This is not necessarily the user's name,
        /// but is rather a "username" which is a distinct bit of information.
        /// That's why it is "Username" as opposed to "UserName".
        /// </remarks>
        [DefaultValue(false)]
        public bool IsToShowUser
        {
            get { return _isToShowUser; }
            set { _isToShowUser = value; }
        }

        /// <summary>
        /// Enable the showing of the Username of the user who is executing the subject-program within the prefix for each log-record.
        /// This is the same as setting the IsToShowUsername property to true.
        /// The default value of that property is false.
        /// </summary>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        public LogConfig ShowUser()
        {
            IsToShowUser = true;
            return this;
        }
        #endregion

        #region timestamp or elapsed-time

        #region IsToShowTimestamp
        /// <summary>
        /// Get or set whether to include the date-and-time in the prefix for each log record.
        /// Default is true (do show the timestamp).
        /// </summary>
        [DefaultValue(true)]
        public bool IsToShowTimestamp
        {
            get { return _isToShowTimestamp; }
            set { _isToShowTimestamp = value; }
        }

        /// <summary>
        /// Disable the showing of the date-and-time in the prefix for each log-record. This is the same as setting the IsToShowTimestamp property to false.
        /// The property is true by default.
        /// </summary>
        /// <returns>a reference to this <see cref="LogConfig"/> object so that methods may be chained together</returns>
        public LogConfig DontShowTimestamp()
        {
            IsToShowTimestamp = false;
            return this;
        }
        #endregion

        #region IsToShowDateInTimestamp
        /// <summary>
        /// Get or set whether to include the date in the prefix timestamp for each log record.
        /// Default is true (do show the date).
        /// </summary>
        [DefaultValue(true)]
        public bool IsToShowDateInTimestamp
        {
            get { return _isToShowDateInTimestamp; }
            set { _isToShowDateInTimestamp = value; }
        }

        /// <summary>
        /// Turn off the showing of the date within the prefix for each log-record. This is the same as setting the <see cref="IsToShowDateInTimestamp"/> property to false.
        /// That property defaults to true.
        /// </summary>
        /// <returns>a reference to this <see cref="LogConfig"/> object so that methods may be chained together</returns>
        public LogConfig DontShowDateInTimestamp()
        {
            IsToShowDateInTimestamp = false;
            return this;
        }
        #endregion

        #region IsToShowFractionsOfASecond
        /// <summary>
        /// Get or set whether to include fractions of a second within the timestamp for each log-record
        /// as opposed to showing only whole seconds, within the trace prefix.
        /// Default is false.
        /// </summary>
        [DefaultValue(false)]
        public bool IsToShowFractionsOfASecond
        {
            get { return _isToShowFractionsOfASecond; }
            set { _isToShowFractionsOfASecond = value; }
        }

        /// <summary>
        /// Show fractions of a second within the timestamp for each log-record,
        /// This is the same as setting the IsToShowFractionsOfASecond property to true.
        /// That property defaults to false.
        /// </summary>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        public LogConfig ShowFractionsOfASecond()
        {
            IsToShowFractionsOfASecond = true;
            return this;
        }

        /// <summary>
        /// Don't show fractions of a second within the timestamp for each log-record.
        /// This is the same as setting the IsToShowFractionsOfASecond property to false.
        /// That property defaults to false.
        /// </summary>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        public LogConfig DontShowFractionsOfASecond()
        {
            IsToShowFractionsOfASecond = false;
            return this;
        }
        #endregion

        #region IsToShowElapsedTime
        /// <summary>
        /// Get or set whether to display the cumulative elapsed time for each log output, including fractions of a second,
        /// as opposed to showing only whole seconds,
        /// within the trace prefix. Setting this to true also sets the reference-time to now.
        /// Default is false.
        /// </summary>
        [DefaultValue(false)]
        public bool IsToShowElapsedTime
        {
            get { return _isToShowElapsedTime; }
            set
            {
                LogManager.SetPointOfReferenceForElapsedTime();
                _isToShowElapsedTime = value;
            }
        }

        /// <summary>
        /// Show the cumulative elapsed time within the timestamp for each log-record,
        /// This is the same as setting the IsToShowElapsedTime property to true.
        /// This also sets the reference-time to now.
        /// That property defaults to false.
        /// </summary>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        public LogConfig ShowElapsedTime()
        {
            IsToShowElapsedTime = true;
            return this;
        }
        #endregion

        #region IsToShowElapsedTimeInSeconds
        /// <summary>
        /// Get or set whether to display the elapsed time using only seconds, as opposed to in HH:MM:SS format.
        /// Default is true (when showing the elapsed time, display it in units of seconds).
        /// This is only applicable when showing the elapsed time.
        /// </summary>
        [DefaultValue(true)]
        public bool IsToShowElapsedTimeInSeconds
        {
            get { return _isToShowElapsedTimeInSeconds; }
            set { _isToShowElapsedTimeInSeconds = value; }
        }

        /// <summary>
        /// Set whether to display the elapsed time in HH:MM:SS format, as opposed to in number of seconds only.
        /// This is the same as setting the IsToShowElapsedTimeInSeconds property to false - which is it's default value.
        /// This is only applicable when showing the elapsed time.
        /// </summary>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        /// <remarks>
        /// The value of the IsToShowElapsedTimeInSeconds property defaults to false.
        /// </remarks>
        public LogConfig ShowElapsedTimeInHHMMSSFormat()
        {
            IsToShowElapsedTimeInSeconds = false;
            return this;
        }
        #endregion

        #region DecimalPlacesForSeconds
        /// <summary>
        /// Set how many decimal places (of the seconds value) to display when showing the elapsed time.
        /// If null, then value has not been set - and the number of decimal-places that gets shown is generally 7.
        /// CBL  I would prefer that this default to 2 or 3.
        /// </summary>
        [DefaultValue(null)]
        public int? DecimalPlacesForSeconds
        {
            //CBL  Distinguish this from the setting for number of decimal places
            // within a normal log timestamp within the prefix.
            get { return _decimalPlacesForSeconds; }
            set { _decimalPlacesForSeconds = value; }
        }

        /// <summary>
        /// Set how many decimal places (of the seconds value) to display when showing the elapsed time.
        /// Generally seven decimal places are shown if this is not set.
        /// </summary>
        /// <param name="numberOrNull">an integer that gives the number of decimal places to show, or null to just accept the default value</param>
        /// <returns>a reference to this LogConfig object so that methods may be chained together</returns>
        public LogConfig SetNumberOfDecimalPlacesForSeconds(int? numberOrNull)
        {
            _decimalPlacesForSeconds = numberOrNull;
            return this;
        }
        #endregion

        #endregion timestamp or elapsed-time

        #endregion log prefix content

        #endregion configuration properties

        #region Clear
        /// <summary>
        /// Set all of the configuration parameters to null, as though no configuration-data file had been found.
        /// </summary>
        public void Clear()
        {
            this.SetToDefaults();

            if (LogManager._outputPipes != null)
            {
                foreach (IOutputPipe outputPipe in LogManager.OutputPipes)
                {
                    outputPipe.Clear();
                }
            }
        }

        #endregion

        #region filesystem-watching facility

        #region IsRealtimeReactionToConfigurationFileEnabled
        /// <summary>
        /// This determines whether we watch the XML configuration file and respond to changes to it, in real-time (that is, immediately).
        /// Defaults to true (yes).
        /// </summary>
        public bool IsRealtimeReactionToConfigurationFileEnabled
        {
            get { return _isConfigurationFileWatchingEnabled; }
            set { _isConfigurationFileWatchingEnabled = value; }
        }
        #endregion

#if !NETFX_CORE
        #region BeginWatchingFilesystem method
        /// <summary>
        /// Set up to start receiving events whenever a changes are made to the XML configuration file.
        /// </summary>
        private void BeginWatchingFilesystem()
        {
            if (_filesystemWatcher == null)
            {
                // If _filePathnameForSettings includes a directory, that is other than this application's base directory,
                // then we need to watch that file and not our base directory.

                string directoryOfConfigurationFile = null;
                string nameOfConfigurationFile = null;

                // If the path for the configuration-settings-file includes a directory,
                char[] pathSeparators = new char[] { '/', '\\' };
                if (_filePathnameForSettings.IndexOfAny(pathSeparators) >= 0)
                {
                    // then try to use that to indicate what directory to watch for configuration-settings-file changes.
                    var fileInfo = new FileInfo(_filePathnameForSettings);
                    directoryOfConfigurationFile = fileInfo.DirectoryName;
                    nameOfConfigurationFile = fileInfo.Name;
                }
                else
                {
                    // The path does not include a directory-separation character, so assume it is just a simple filename,
                    // and watch for it within the executing application's base-directory.

                    // Either method here seems to work, to get the base directory.
                    directoryOfConfigurationFile = System.AppDomain.CurrentDomain.BaseDirectory;
                    //string executablePath = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                    nameOfConfigurationFile = _filePathnameForSettings;
                }

                // Don't watch it unless that directory actually does exist.
                if (Directory.Exists(directoryOfConfigurationFile))
                {
                    //Console.WriteLine("dir does exist, setting file watcher with Path=" + directoryOfConfigurationFile + ", Filter=" + nameOfConfigurationFile);
                    _filesystemWatcher = new FileSystemWatcher();
                    _filesystemWatcher.Path = directoryOfConfigurationFile;
                    _filesystemWatcher.Filter = nameOfConfigurationFile;
                    //_filesystemWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
                    _filesystemWatcher.Created += new FileSystemEventHandler(OnConfigFileCreated);
                    _filesystemWatcher.Changed += new FileSystemEventHandler(OnConfigFileChanged);
                    _filesystemWatcher.Renamed += new RenamedEventHandler(OnConfigFileRenamed);
                    _filesystemWatcher.Deleted += new FileSystemEventHandler(OnConfigFileDeleted);
                    _filesystemWatcher.EnableRaisingEvents = true;
                    AllowForHysteresis();
                }
            }
        }
        #endregion

        private void AllowForHysteresis()
        {
            //Console.WriteLine("Setting timer.");
            _isSensitiveToFileChangeEvents = false;
            if (_timerForFileChangeHysteresis == null)
            {
                // Set a timer to clear that status-text in 1 second.
                var timeout = TimeSpan.FromSeconds(1);
                _timerForFileChangeHysteresis = new DispatcherTimer();
                _timerForFileChangeHysteresis.Interval = timeout;
                _timerForFileChangeHysteresis.Tick += OnTimerTick;
            }
            _timerForFileChangeHysteresis.Start();
        }

        private void OnConfigFileCreated(object sender, FileSystemEventArgs a)
        {
            if (_isSensitiveToFileChangeEvents)
            {
                //Console.WriteLine("OnConfigFileCreated, " + a.ChangeType.ToString() + ", Name=" + a.Name + ", FullPath=" + a.FullPath);
                ReadFromYamlFile(null);
                AllowForHysteresis();
            }
        }

        private void OnConfigFileChanged(object sender, FileSystemEventArgs a)
        {
            //Console.WriteLine("OnConfigFileChanged, isSensitive=" + _isSensitiveToFileChangeEvents + ", ChangeType=" + a.ChangeType.ToString() + ", Name=" + a.Name + ", FullPath=" + a.FullPath);
            if (_isSensitiveToFileChangeEvents)
            {
                if (a.ChangeType == WatcherChangeTypes.Created)
                {
                }
                NutUtil.WriteToConsole("Change detected to LogNut configuration settings file.");
                //ReadFromFile();
                _isForRead = true;
                AllowForHysteresis();
            }
            else
            {
                _isSensitiveToFileChangeEvents = true;
            }
        }

        private void OnConfigFileRenamed(object sender, RenamedEventArgs a)
        {
            //Console.WriteLine("OnConfigFileRenamed, isSensitive=" + _isSensitiveToFileChangeEvents + ", ChangeType=" + a.ChangeType.ToString() + ", Name=" + a.Name + ", FullPath=" + a.FullPath + ", OldName=" + a.OldName);
            if (_isSensitiveToFileChangeEvents)
            {
                if (a.Name.Equals(_filePathnameForSettings))
                {
                    NutUtil.WriteToConsole("LogNut configuration settings file renamed, now matches configured name. ");
                    ReadFromYamlFile(null);
                }
                else
                {
                    NutUtil.WriteToConsole("LogNut configuration settings file renamed; clearing all associated settings.");
                    Clear();
                }
            }
        }

        private void OnConfigFileDeleted(object sender, FileSystemEventArgs a)
        {
            if (_isSensitiveToFileChangeEvents)
            {
                //Console.WriteLine("OnConfigFileDeleted, " + a.ChangeType.ToString() + ", Name=" + a.Name + ", FullPath=" + a.FullPath);
                NutUtil.WriteToConsole("LogNut configuration settings file deleted; clearing all associated settings.");
                Clear();
            }
        }
#endif
        #endregion filesystem-watching facility

        #region SetToDefaults
        /// <summary>
        /// Set all of the configuration parameters to their default values,
        /// which is the same as removing all of the the explicit values such that the properties revert to their default values.
        /// The default values retain their current settings.
        /// </summary>
        public void SetToDefaults()
        {
            //CBL Better check all of these with unit-tests, and perhaps do it using reflection?
            IsToOpenCloseOutputFileEveryTime = true;
            FileOutputAdditionalHeaderText = null;
            FileOutputAdditionalHeaderLines = null;
            FileOutputArchiveFolder = null;
            LowestLevelThatIsEnabled = default(LogLevel);
            IsLoggingEnabled = true;
            _fileOutputFilename = null;
            _fileOutputFolder = null;
            RemovableDrivePreferredFileOutputFolder = null;
            _fileOutputFormatType = default(LogFileFormatType);
            _fileOutputRolloverMode = default(RolloverMode);
            _fileOutputRollPoint = default(RollPoint);
            _isFileOutputSpreadsheetCompatible = false;
            _isFileOutputToCompressFiles = false;
            IsFileOutputEnabled = true;
            IsFileOutputToInsertHeader = true;
            IsFileOutputToInsertLineBetweenTraces = true;
            IsFileOutputToUseStdTerminator = true;
            IsToCreateNewOutputFileUponStartup = true;
            _maxFileSize = _maxFileSize_DefaultValue;
            _maxNumberOfFileRollovers = 100;
            _isToShowId = false;
            _isToShowElapsedTime = false;
            _isToShowFractionsOfASecond = false;
            _isToShowLevel = true;
            _isToShowLevelOnlyForWarningsAndAbove = false;
            _isToShowLoggerName = true;
            _isToShowPrefix = true;
            _isToShowSourceFile = true;
            _isToShowSourceHost = false;
            _isToShowStackTraceForExceptions = true;
            _isToShowSubjectProgram = false;
            _isToShowSubjectProgramVersion = false;
            _isToShowThread = false;
            _isToShowTimestamp = true;
            _isToShowUser = false;

            NutFileLib.IsFileOutputFilenameUsingDate = false;

            // Do the same for all of the output-pipes..
            if (LogManager._outputPipes != null)
            {
                foreach (IOutputPipe outputPipe in LogManager.OutputPipes)
                {
                    outputPipe.SetToDefaults();
                }
            }
        }
        #endregion

        #region IDisposable
        /// <summary>
        /// Release any resources held by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Release any resources held by this object.
        /// </summary>
        /// <param name="isDisposingManagedResources">true to indicate that managed resources are also being released</param>
        protected virtual void Dispose(bool isDisposingManagedResources)
        {
#if !NETFX_CORE
            if (isDisposingManagedResources)
            {
                if (_filesystemWatcher != null)
                {
                    _filesystemWatcher.Dispose();
                }
            }
#endif
        }
        #endregion

        #region ToString
        /// <summary>
        /// Override the ToString method to provide a more useful display.
        /// </summary>
        /// <returns>a string the denotes the state of this object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder("LogConfig(");
#if DEBUG
            // For most - display the properties that are at other than their default state.
            if (IsAsynchronous)
            {
                sb.Append(" IsAsynchronous,");
            }
            if (!IsLoggingEnabled)
            {
                sb.Append(" IsLoggingEnabled=false,");
            }
            if (!IsFileOutputEnabled)
            {
                sb.Append(" IsFileOutputEnabled=false,");
            }
            if (IsToWatchConfigurationFile)
            {
                sb.Append(" IsToWatchConfigurationFile,");
            }
            if (LowestLevelThatIsEnabled != default(LogLevel))
            {
                sb.Append(" LowestLevelThatIsEnabled=").Append(LowestLevelThatIsEnabled).Append(",");
            }
            if (FileOutputAdditionalHeaderText != null)
            {
                sb.Append(@" FileOutputAdditionalHeaderText=""").Append(_fileOutputAdditionalHeaderText).Append(@""",");
            }
            if (FileOutputFilename != null)
            {
                sb.Append(" FileOutputFilename=").Append(FileOutputFilename).Append(",");
            }
            if (FileOutputFolder != null)
            {
                sb.Append(" FileOutputFolder=").Append(_fileOutputFolder).Append(",");
            }
            if (FileOutputFormatType != default(LogFileFormatType))
            {
                sb.Append(" FileOutputFormatType=").Append(FileOutputFormatType).Append(",");
            }
            if (FileOutputRolloverMode != default(RolloverMode))
            {
                sb.Append(" FileOutputRolloverMode=").Append(FileOutputRolloverMode).Append(",");
            }
            if (FileOutputRollPoint != default(RollPoint))
            {
                sb.Append(" FileOutputRollPoint=").Append(FileOutputRollPoint).Append(",");
            }
            if (!IsToCreateNewOutputFileUponStartup)
            {
                sb.Append(" not IsToCreateNewOutputFileUponStartup,");
            }
            if (IsFileOutputToCompressFiles)
            {
                sb.Append(" IsFileOutputToCompressFiles,");
            }
            if (!IsFileOutputToInsertHeader)
            {
                sb.Append(" IsFileOutputToInsertHeader=false,");
            }
            if (_maxFileSize != _maxFileSize_DefaultValue)
            {
                sb.Append(" MaxFileSize=").Append(MaxFileSize).Append(",");
            }
            if (_maxNumberOfFileRollovers != 100)
            {
                sb.Append(" MaxNumberOfFileRollovers=").Append(MaxNumberOfFileRollovers).Append(",");
            }
            if (!IsFileOutputToUseStdTerminator)
            {
                sb.Append(" IsFileOutputToUseStdTerminator=false,");
            }
            // Display the properties that are at other than their default state.
            bool isNotFirst = false;
            if (IsToShowId)
            {
                sb.Append("IsToShowId,");
                isNotFirst = true;
            }
            if (IsToShowElapsedTime)
            {
                if (isNotFirst)
                {
                    sb.Append(",");
                }
                sb.Append("IsToShowElapsedTime");
                isNotFirst = true;
            }
            if (IsToShowElapsedTimeInSeconds)
            {
                if (isNotFirst)
                {
                    sb.Append(",");
                }
                sb.Append("IsToShowElapsedTimeInSeconds");
                isNotFirst = true;
            }
            if (IsToShowFractionsOfASecond)
            {
                if (isNotFirst)
                {
                    sb.Append(",");
                }
                sb.Append("IsToShowFractionsOfASecond");
                isNotFirst = true;
            }
            if (IsToShowPrefix)
            {
                if (isNotFirst)
                {
                    sb.Append(",");
                }
                sb.Append("IsToShowPrefix");
                isNotFirst = true;
            }
            if (IsToShowLevel)
            {
                if (isNotFirst)
                {
                    sb.Append(",");
                }
                sb.Append("IsToShowLevel");
                isNotFirst = true;
            }
            if (IsToShowThread)
            {
                if (isNotFirst)
                {
                    sb.Append(",");
                }
                sb.Append("IsToShowThread");
                isNotFirst = true;
            }
            if (IsToShowLoggerName)
            {
                if (isNotFirst)
                {
                    sb.Append(",");
                }
                sb.Append("IsToShowLoggerName");
                isNotFirst = true;
            }
            if (IsToShowUser)
            {
                if (isNotFirst)
                {
                    sb.Append(",");
                }
                sb.Append("IsToShowUser");
                isNotFirst = true;
            }
            if (IsToShowCategory)
            {
                if (isNotFirst)
                {
                    sb.Append(",");
                }
                sb.Append("IsToShowCategory");
                isNotFirst = true;
            }



#endif
            return sb.ToStringAndEndList();
        }
        #endregion

        #region internal implementation

        #region ClearTimer
#if !NETFX_CORE
        /// <summary>
        /// Dispose of the timer.
        /// </summary>
        private void ClearTimer()
        {
            // Dispose of the timer, if that's still around.
            if (_timerForFileChangeHysteresis != null)
            {
                if (_timerForFileChangeHysteresis.IsEnabled)
                {
                    _timerForFileChangeHysteresis.Stop();
                }
            }
        }
#endif
        #endregion

        #region OnTimerTick
#if !NETFX_CORE
        /// <summary>
        /// Handle the "Tick" event of either of the timers.
        /// </summary>
        private void OnTimerTick(object sender, EventArgs args)
        {
            //Console.WriteLine("OnTimerTick");
            ClearTimer();
            if (_isForRead)
            {
                //CBL  Does this really make sense?
                // or get from file. ??

                //ReadFromPersistentStorage();

                _isForRead = false;
            }
            _isSensitiveToFileChangeEvents = true;
        }
#endif
        #endregion

        #region fields

        private static LogConfig _theConfiguration;

        /// <summary>
        /// This is an instance of LogConfig that is suitable for outputing to the command-line or IDE console.
        /// </summary>
        private static LogConfig _configurationForConsoleOutput;

        /// <summary>
        /// When showing elapsed-time, this dictates how many decimal-places to show for the seconds.
        /// If null, this is not set.
        /// </summary>
        private int? _decimalPlacesForSeconds;

        /// <summary>
        /// This represents additional text to append after the standard header (that which is written before the first log)
        /// one separate lines.
        /// </summary>
        private string _fileOutputAdditionalHeaderLines;

        /// <summary>
        /// This holds the full filesystem-path of the archive folder, which may be distinct from the value of 
        /// _fileOutputArchiveFolder in that this will be set to the actual path.
        /// </summary>
        private string _fileOutputArchiveFolder_FullPath;

        /// <summary>
        /// This denotes the value set for the filesystem-folder into which output files are to be moved
        /// when they rollover (that is - are "archived").
        /// The value to use to actually place the files to be archived, is _fileOutputArchiveFolder_FullPath
        /// as *this* value may be relative to the FileOutputFolder.
        /// If null (which is the default value) - the files are not moved.
        /// </summary>
        private string _fileOutputArchiveFolder;

        /// <summary>
        /// This flag, when true, indicates that the "Archive" folder is set to be within the file-output-folder.
        /// It is important because it may need to be updated whenever the file-output-folder is changed.
        /// </summary>
        private bool _isArchiveFolderRelative;

#if !NETFX_CORE
        /// <summary>
        /// This watches for any changes to the XML configuration file.
        /// </summary>
        private FileSystemWatcher _filesystemWatcher;
#endif

        /// <summary>
        /// This dictates whether to send the log records out asynchronously. The intial default value is false.
        /// </summary>
        private bool _isAsynchronous;

        /// <summary>
        /// This dictates whether, for file-output, the prefix portion is to be of fixed width.
        /// The default value is false.
        /// </summary>
        private bool _isFileOutputPrefixOfFixedWidth;

        /// <summary>
        /// This flag, when true, dictates that all file-output must be capable of being pasted into an Excel spreadsheet.
        /// </summary>
        private bool _isFileOutputSpreadsheetCompatible;

        /// <summary>
        /// This flag dictates whether to insert a seperate line giving just the timestamp
        /// at the first log transmission after each time the subject-program has launched.
        /// Default is true.
        /// </summary>
        private bool _isFileOutputToInsertHeader = true;

        /// <summary>
        /// This flag determines whether to separate the log traces within a file, with a blank line.
        /// Default is true.
        /// </summary>
        private bool _isFileOutputToInsertLineBetweenTraces = true;

        /// <summary>
        /// This determines whether to watch the configuration file in realtime and react instantly to any changes to it, or not.
        /// Defaults is false.
        /// </summary>
        internal bool IsToWatchConfigurationFile;
        //CBL This is not yet implemented.

#if !NETFX_CORE
        /// <summary>
        /// If false, this prevents the Configurator from caring about events raised by the filesystem-watcher.
        /// </summary>
        private bool _isSensitiveToFileChangeEvents;
#endif

        private bool _isConfigurationFileWatchingEnabled;

        /// <summary>
        /// This denotes lowest-level LogLevel that is currently enabled. The default is LogLevel.Trace, which means all levels are enabled.
        /// </summary>
        private LogLevel _lowestLevelThatIsEnabled;

#if !NETFX_CORE
        /// <summary>
        /// This flag indicates when the configuration-settings file should be read when the timer expires.
        /// </summary>
        private bool _isForRead;

#endif

#if !NETFX_CORE
        /// <summary>
        /// This DispatcherTimer is used to prevent being overly sensitive to extraneous file-watcher events.
        /// </summary>
        private DispatcherTimer _timerForFileChangeHysteresis;
#endif

        /// <summary>
        /// This string contains text that may be configured to append to the header information (that which is written the first time a log is sent).
        /// It is null by default.
        /// </summary>
        private string _fileOutputAdditionalHeaderText;

        /// <summary>
        /// The filesystem-file into which the log-records are placed, if logging to file is indeed enabled.
        /// </summary>
        private string _fileOutputFilename;

        /// <summary>
        /// This is the folder that the loggers will write the log file output to.
        /// </summary>
        private string _fileOutputFolder;

        /// <summary>
        /// If this is non-null, then this path denotes the drive and directory of a removable drive which is the preferred destination
        /// for file output, but which is too unreliable to serve as the primary output.
        /// Default value is null, indicating that no such destination is set.
        /// </summary>
        private string _removableDrivePreferredOutputFolder;

        /// <summary>
        /// This dictates the format to use for writing log records to files.
        /// Default is SimpleText.
        /// </summary>
        private LogFileFormatType _fileOutputFormatType;

        /// <summary>
        /// The basis on which logging output is to be rolled over to new files.
        /// The intial value is Size.
        /// </summary>
        private RolloverMode _fileOutputRolloverMode;

        /// <summary>
        /// This dictates the frequency with which the logging output is rolled over to new files,
        /// if the RolloverMode is set to either Date or Composite.
        /// Default is TopOfDay.
        /// </summary>
        private RollPoint _fileOutputRollPoint;

        /// <summary>
        /// This is the filename that is used by default for persisting LogNut configuration settings. It is "LogNutSettings.yaml".
        /// </summary>
        protected string _filePathnameForSettings = @"LogNutSettings.yaml";

        /// <summary>
        /// This flag dictates whether to compress the output files when we roll-over to a new file.
        /// Default is false.
        /// </summary>
        private bool _isFileOutputToCompressFiles;

        /// <summary>
        /// This determines whether to include the Category within the prefix
        /// of a log-record. Default is false.
        /// </summary>
        private bool _isToShowCategory;

        /// <summary>
        /// This flag dictates whether to include the date in the prefix timestamp for each log record.
        /// Default is true (do show the date).
        /// </summary>
        private bool _isToShowDateInTimestamp = true;

        /// <summary>
        /// This determines whether to include the cumulative elapsed-time, since some point of reference, within the text that represents the time-stamp
        /// of a given log-record. Default is false.
        /// </summary>
        private bool _isToShowElapsedTime;

        /// <summary>
        /// When showing the elapsed time, this flag dictates whether to
        /// display it using only seconds, as opposed to in HH:MM:SS format.
        /// Default is true.
        /// </summary>
        private bool _isToShowElapsedTimeInSeconds = true;

        /// <summary>
        /// This determines whether to include the fraction-of-a-second within the text that represents the time-stamp
        /// of a given log-record.
        /// Default is false; only whole values for the number of seconds is shown.
        /// </summary>
        private bool _isToShowFractionsOfASecond;

        /// <summary>
        /// This dictates whether to display the log-record Id in the prefix for each log record.
        /// Default is false.
        /// </summary>
        private bool _isToShowId;

        /// <summary>
        /// This dictates whether to include the logging level (the LogLevel) in the prefix for each log record.
        /// Default is true.
        /// </summary>
        private bool _isToShowLevel = true;

        /// <summary>
        /// When this is true, the log-level is included within the prefix only when it is Warning or Error.
        /// Default is false (do show the level always).
        /// </summary>
        private bool _isToShowLevelOnlyForWarningsAndAbove;

        /// <summary>
        /// This dictates whether to include the logger's name within the prefix for each log-record, when writing it out in the std text file-format.
        /// Default is true.
        /// </summary>
        private bool _isToShowLoggerName = true;

        /// <summary>
        /// This dictates whether to include the logger's name within the prefix even when it's name is the default logger-name.
        /// Default is false.
        /// </summary>
        private bool _isToShowLoggerNameWhenDefault;

        /// <summary>
        /// This dictates whether to include a prefix for each log record. Other flags enable the various portions of the prefix.
        /// Default is true.
        /// </summary>
        private bool _isToShowPrefix = true;

        /// <summary>
        /// This dictates whether to include the source-file-path of the logging statement within the contextual-information
        /// of the logging output, when calling any of the Log..WithContext methods (applies to .NET versions above 4.0).
        /// Default is <c>true</c>.
        /// </summary>
        private bool _isToShowSourceFile = true;

        /// <summary>
        /// This dictates whether to include the source-host name (the name of the computer that originated a given log-record)
        /// within the prefix portion of the log trace.
        /// Default is false.
        /// </summary>
        private bool _isToShowSourceHost;

        /// <summary>
        /// This flag dictates whether to include the full stack-trace in the log record when logging an Exception.
        /// Default is true.
        /// </summary>
        private bool _isToShowStackTraceForExceptions = true;

        /// <summary>
        /// This dictates whether to include the name of the subject-program (ie., the program that is doing the logging)
        /// within the prefix portion of the log trace.
        /// Default is false.
        /// </summary>
        private bool _isToShowSubjectProgram;

        /// <summary>
        /// This determines whether to include the version of the subject-program within the prefix.
        /// Default is false.
        /// </summary>
        private bool _isToShowSubjectProgramVersion;

        /// <summary>
        /// This flag dictates whether to include the thread-id within the log output.
        /// Default is false.
        /// </summary>
        private bool _isToShowThread;

        /// <summary>
        /// This dictates whether to include a timestamp within the prefix for each log record, to indicate when that log record was generated.
        /// Default is true.
        /// </summary>
        private bool _isToShowTimestamp = true;

        /// <summary>
        /// This flag dictates whether to display the current user (name, or id) within the prefix of the log record.
        /// Default is false.
        /// </summary>
        private bool _isToShowUser;

        /// <summary>
        /// Get or set whether, when calling LogMethodBegin, to log the class-name of the method
        /// instead of the source filename. Default is true.
        /// </summary>
        private bool _isToLogMethodBeginWithClassName = true;

        /// <summary>
        /// This flag dictates whether logging output is to also be echoed to the current command-line console,
        /// which when Visual Studio is running means to echo it to the Output pane.
        /// Default is true.
        /// </summary>
        private bool _isToOutputToConsole = true;

        /// <summary>
        /// This is the maximum size that we want to permit the log-file to grow to, in bytes,
        /// this maximum being settable according to the developer's needs.
        /// The default is 2,147,483,648, which is 2 Gigabytes.
        /// The absolute limit is _maxFileSize_UpperLimit.
        /// </summary>
        private Int64 _maxFileSize = _maxFileSize_DefaultValue;

        /// <summary>
        /// This is the default value for MaxFileSize, initially set to (2 Gigabytes, or 2,000,000,000).
        /// </summary>
        /// <remarks>
        /// A value of binary 2GB, ie 2,147,483,648 causes an overflow-exception when saving to YAML.
        /// </remarks>
        private const Int64 _maxFileSize_DefaultValue = 2000000000; //2147483648;

        /// <summary>
        /// The maximum number of backup files that are kept before the oldest is erased, when the rollover
        /// is a result of the log file reaching it's maximum size limit.
        /// Default is null, meaning no-value.
        /// The initial default value is 100.
        /// </summary>
        private int _maxNumberOfFileRollovers = 100;

        /// <summary>
        /// This specifies the minimum width of the prefix -- or rather, that part of the "prefix" other than the timestamp.
        /// This refers to the part that comes within the "[]" brackets, inclusive of the brackets themselves.
        /// When a prefix merits a width GREATER than this, this variable number is bumped up
        /// such that all subsequent output adopts that new value.
        /// </summary>
        internal int _prefixWidth;

        /// <summary>
        /// This is used to save the name of the subject-program.
        /// </summary>
        private string _subjectProgramName;

        /// <summary>
        /// This stores the text that we will display as the version (as a string) of the running subject-program.
        /// </summary>
        private string _subjectProgramVersion;

        /// <summary>
        /// This saves the value of the user-name of the user who is executing the executing subject-program.
        /// </summary>
        private string _userName;

        #endregion fields

        #endregion internal implementation
    }
}
