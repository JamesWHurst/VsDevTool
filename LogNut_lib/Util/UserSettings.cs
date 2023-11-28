using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Xml.Serialization;
#if NETFX_CORE
using Windows.UI.Xaml;
#endif


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// This serves as a superclass for program-settings classes that same their information in XML-formatted text files.
    /// </summary>
#if !NETFX_CORE
    [Serializable]
#endif
    [XmlRoot( "UserSettings" )]
    public class UserSettings
    {
        #region public properties

        #region FilePath
        /// <summary>
        /// Get the full filesystem-path of the file to which these settings are saved,
        /// or "(null)" if it has not been computed as yet.
        /// </summary>
        public static string FilePath
        {
            get
            {
                if (s_settingsXMLFilePath == null)
                {
                    return "(null)";
                }
                else
                {
                    return s_settingsXMLFilePath;
                }
            }
        }
        #endregion

        #region IsChanged
        /// <summary>
        /// Get or set the flag that indicates whether any values of this object have changed since the last time this flag was cleared.
        /// </summary>
        [XmlIgnore]
        public bool IsChanged
        {
            get { return _isChanged; }
            set { _isChanged = value; }
        }
        #endregion

        #region IsFound
        /// <summary>
        /// Get whether the UserSettings object found it's underlying persistent-data file.
        /// </summary>
        [XmlIgnore]
        public static bool IsFound
        {
            get { return s_isFound; }
        }
        #endregion

        #endregion public properties

        #region public methods

        #region Clear
        /// <summary>
        /// Reset this class back to it's initial state. Useful for testing.
        /// </summary>
        public static void Clear()
        {
            _instance = null;
            s_isFound = false;
            s_settingsXMLFilePath = null;
        }
        #endregion

        #region GetInstance
        /// <summary>
        /// Return the singleton-instance of your subclass of <see cref="UserSettings"/>, creating it if necessary.
        /// This overload of GetInstance selects true for isToSaveLocation and false for isToSaveSize.
        /// </summary>
        /// <typeparam name="T">the subtype of <see cref="UserSettings"/> to create</typeparam>
        /// <param name="forApplication">the <see cref="Hurst.BaseLib.IApp"/> that this settings class is for</param>
        /// <returns>a new instance, or the existing instance, of your type T</returns>
        public static T GetInstance<T>( IApp forApplication ) where T : UserSettings, new()
        {
            return GetInstance<T>( forApplication, true, false );
        }
        /// <summary>
        /// Return the singleton-instance of your subclass of <see cref="UserSettings"/>, creating it if necessary.
        /// </summary>
        /// <typeparam name="T">the subtype of <see cref="UserSettings"/> to create</typeparam>
        /// <param name="forApplication">the <see cref="Hurst.BaseLib.IApp"/> that this settings class is for</param>
        /// <param name="isToSaveLocation">set this to true to save the location (as distinct from size) of the application's main-window</param>
        /// <param name="isToSaveSize">set this to true to save the size (as distinct from location) of the application's main-window</param>
        /// <returns>a new instance, or the existing instance, of your type T</returns>
        public static T GetInstance<T>( IApp forApplication, bool isToSaveLocation, bool isToSaveSize ) where T : UserSettings, new()
        {
            if (_instance == null)
            {
                if (forApplication == null)
                {
                    throw new ArgumentNullException( "forApplication" );
                    //Debug.WriteLine( "UserSettings.GetInstance, forApplication is null - instantiating new instance with all default values." );
                    //_instance = new T();
                    //_instance._isToSaveLocation = isToSaveLocation;
                    //_instance._isToSaveSize = isToSaveSize;
                }
                else
                {
                    string settingsFilePath = "?";
                    try
                    {
                        settingsFilePath = GetXmlDataFilePath( forApplication );
                        if (File.Exists( settingsFilePath ))
                        {
                            _instance = DeserializeFromXML<T>( settingsFilePath );
                            _instance.IsChanged = false;
                        }
                        else
                        {
                            Debug.WriteLine( "in UserSettings.GetInstance, no XML file found - constructing anew." );
                        }
                        if (_instance == null)
                        {
                            _instance = new T();
                        }
                        else
                        {
                            s_isFound = true;
                        }
                        _instance._isToSaveLocation = isToSaveLocation;
                        _instance._isToSaveSize = isToSaveSize;
                    }
                    catch (Exception x)
                    {
                        //CBL Verify that this keeps the original infor that x contained, plus contributes this new shit, in both build-modes.
                        string s = "GetInstance( forApplication = " + StringLib.AsString( forApplication ) + " ..) and settingsFilePath is " + StringLib.AsString( settingsFilePath );
                        x.Data.Add( "Details", s );
                        throw;
                    }
                }
            }
#if !PRE_4
            return _instance;
#else
            return (T)_instance;
#endif
        }

        /// <summary>
        /// Return the singleton-instance of your subclass of <see cref="UserSettings"/>, creating it if necessary.
        /// This is provided for applications that do not implement <see cref="Hurst.BaseLib.IApp"/>.
        /// </summary>
        /// <typeparam name="T">the subtype of <see cref="UserSettings"/> to create</typeparam>
        /// <param name="vendorName">what to use for the part of the path that denotes the vendor-name (must not be null)</param>
        /// <param name="programName">the name of the application-program that this is for (must not be null)</param>
        /// <param name="isToSaveLocation">set this to true to save the location (as distinct from size) of the application's main-window</param>
        /// <param name="isToSaveSize">set this to true to save the size (as distinct from location) of the application's main-window</param>
        /// <returns>a new instance, or the existing instance, of your type T</returns>
        public static T GetInstance<T>( string vendorName, string programName, bool isToSaveLocation, bool isToSaveSize ) where T : UserSettings, new()
        {
            if (_instance == null)
            {
                if (vendorName == null)
                {
                    throw new ArgumentNullException( "vendorName" );
                }
                if (programName == null)
                {
                    throw new ArgumentNullException( "programName" );
                }

                string settingsFilePath = "?";
                try
                {
                    settingsFilePath = GetXmlDataFilePath( vendorName, programName, null );
                    if (File.Exists( settingsFilePath ))
                    {
                        _instance = DeserializeFromXML<T>( settingsFilePath );
                        _instance.IsChanged = false;
                    }
                    else
                    {
                        Debug.WriteLine( "in UserSettings.GetInstance, no XML file found - constructing anew." );
                    }
                    if (_instance == null)
                    {
                        _instance = new T();
                    }
                    else
                    {
                        s_isFound = true;
                    }
                    _instance._isToSaveLocation = isToSaveLocation;
                    _instance._isToSaveSize = isToSaveSize;
                }
                catch (Exception x)
                {
                    string s = "GetInstance( vendorName = " + StringLib.AsString( vendorName ) + ", programName = " + StringLib.AsString( programName ) + " ..) and settingsFilePath is " + StringLib.AsString( settingsFilePath );
                    x.Data.Add( "Details", s );
                    throw;
                }
            }
#if !PRE_4
            return _instance;
#else
            return (T)_instance;
#endif
        }
        #endregion

        #region GetThisApplicationCurrentUserDataFolder
        /// <summary>
        /// Get the filesystem-path for storing this application's data - that is specific to the current, non-roaming user.
        /// This path value contains the vendor-name and this product-name.
        /// With Windows 8.1, this is C:\Users\{username}\AppData\Local\{vendor-name}\{product-name}\
        /// </summary>
        public static string GetThisApplicationCurrentUserDataFolder( string vendorName, string productName )
        {
            if (String.IsNullOrEmpty( _thisApplicationCurrentUserDataFolder ))
            {
                //CBL
                //             return Windows.Storage.ApplicationData.Current.LocalSettings.

                string partUnderSpecialFolder = Path.Combine( vendorName, productName );
                _thisApplicationCurrentUserDataFolder = Path.Combine( FilesystemLib.GetLocalApplicationDataFolderPath(), partUnderSpecialFolder );
            }
            return _thisApplicationCurrentUserDataFolder;
        }
        #endregion

        #region GetXmlDataFilePath
        /// <summary>
        /// Return the pathname used to persist this object to/from the XML data file.
        /// If you do not set this value explicitly, it derives the path from the applications data folder.
        /// </summary>
        public static string GetXmlDataFilePath( IApp forApplication )
        {
            if (String.IsNullOrEmpty( s_settingsXMLFilePath ))
            {
                if (forApplication == null)
                {
                    throw new ArgumentNullException( "forApplication" );
                }

                // Remove any hypens or underscores from the program-name.
                string programName = forApplication.ProgramName;
                string sanitizedProgramName = programName.RemoveAll( '-' ).RemoveAll( '_' );
                // and append "UserSettings.xml" to it to arrive at the filename.
                s_settingsXMLFilename = sanitizedProgramName + "UserSettings.xml";
                string dataFolder = forApplication.ThisApplicationCurrentUserDataFolder;
                //Debug.WriteLine( @"data folder = """ + dataFolder + @"""" );
                bool isDirectoryThere = false;
                if (Directory.Exists( dataFolder ))
                {
                    isDirectoryThere = true;
                }
                else
                {
                    try
                    {
                        // that data-folder is not there, so create it.
                        var directory = Directory.CreateDirectory( dataFolder );
                        if (directory.Exists)
                        {
                            Debug.WriteLine( @"Created the standard data folder """ + dataFolder + @""" for program settings." );
                            isDirectoryThere = true;
                        }
                    }
                    catch (Exception x)
                    {
                        Debug.WriteLine( x.GetType() + " (" + x.Message + ") trying to create the standard data folder " + dataFolder + " -- using the program folder instead." );
                    }
                }
                if (isDirectoryThere)
                {
                    s_settingsXMLFilePath = Path.Combine( dataFolder, s_settingsXMLFilename );
                }
                else
                {
                    // By giving it ONLY the filename, with no folder - I am assuming it will be placed within the program's own executable's folder.
                    s_settingsXMLFilePath = s_settingsXMLFilename;
                }
            }
            return s_settingsXMLFilePath;
        }

        /// <summary>
        /// Return the pathname to use to persist this object to/from the XML data file.
        /// </summary>
        /// <param name="vendorName">what to use for the part of the path that denotes the vendor-name (must not be null)</param>
        /// <param name="programName">the name to use for the part of the path that denotes the program (must not be null)</param>
        /// <param name="dataFolderToUse">This specifies the folder to use (optional: if you supply null then <c>SpecialFolder.LocalApplicationData</c> is used).</param>
        public static string GetXmlDataFilePath( string vendorName, string programName, string dataFolderToUse )
        {
            if (vendorName == null)
            {
                throw new ArgumentNullException( "vendorName" );
            }
            if (programName == null)
            {
                throw new ArgumentNullException( "programName" );
            }
            if (String.IsNullOrEmpty( s_settingsXMLFilePath ))
            {
                if (dataFolderToUse == null)
                {
                    string localData = FilesystemLib.GetLocalApplicationDataFolderPath();
#if !PRE_4
                    dataFolderToUse = Path.Combine( localData, vendorName, programName );
#else
                    dataFolderToUse = Path.Combine( localData, Path.Combine(vendorName, programName ));  // .NET 3.5 does not have Path.Combine with > 2 arguments.
#endif
                }
                s_settingsXMLFilename = programName + "UserSettings.xml";
                bool isDirectoryThere = false;
                if (Directory.Exists( dataFolderToUse ))
                {
                    isDirectoryThere = true;
                }
                else
                {
                    try
                    {
                        // that data-folder is not there, so create it.
                        var directory = Directory.CreateDirectory( dataFolderToUse );
                        if (directory.Exists)
                        {
                            Debug.WriteLine( "Created the folder " + dataFolderToUse + " for program settings." );
                            isDirectoryThere = true;
                        }
                    }
                    catch (Exception x)
                    {
                        Debug.WriteLine( x.GetType() + " (" + x.Message + @") trying to create the folder """ + dataFolderToUse + @""" -- using the program folder instead." );
                    }
                }
                if (isDirectoryThere)
                {
                    s_settingsXMLFilePath = Path.Combine( dataFolderToUse, s_settingsXMLFilename );
                }
                else
                {
                    // By giving it ONLY the filename, with no folder - I am assuming it will be placed within the program's own executable's folder.
                    s_settingsXMLFilePath = s_settingsXMLFilename;
                }
            }
            return s_settingsXMLFilePath;
        }
        #endregion

        #region SetXmlDataFilePath
        /// <summary>
        /// Set the pathname to use for the XML data file to use to save-to or retrieve-from the program settings.
        /// You don't need to call this unless you want to override the default location.
        /// </summary>
        /// <param name="pathname">a complete filesystem-pathname, including drive, folder, and filename</param>
        public static void SetXmlDataFilePath( string pathname )
        {
            s_settingsXMLFilePath = pathname;
        }
        #endregion

        #region Save
        /// <summary>
        /// Save the state of this UserSettings object to disk, and clear the IsChanged flag.
        /// </summary>
        public void Save()
        {
            try
            {
                SerializeToXML();
                IsChanged = false;
                s_isFound = true;
            }
            catch (Exception x)
            {
                Debug.WriteLine( StringLib.ExceptionDetails( x, true ) );
            }
        }
        #endregion

        #region SaveIfChanged
        /// <summary>
        /// Save the state of this UserSettings object (including the position of the specified Window) to disk
        /// if they have changed (or no settings file was found yet)
        /// and then clear the <see cref="UserSettings.IsChanged"/> flag.
        /// </summary>
        public void SaveIfChanged()
        {
            if (IsChanged || !s_isFound)
            {
                Save();
            }
        }
        #endregion

        #endregion public methods

        #region non-public methods

        #region DeserializeFromXML
        /// <summary>
        /// Return a new instance of the given type from the XML file at the given location.
        /// </summary>
        /// <param name="settingsFilePath">the pathname of the XML file</param>
        /// <returns>a newly-created instance of the given type</returns>
        protected static T DeserializeFromXML<T>( string settingsFilePath ) where T : UserSettings, new()
        {
            T settings;
            try
            {
                XmlSerializer deserializer = new XmlSerializer( typeof( T ) );
                using (var textReader = new StreamReader( settingsFilePath ))
                {
                    settings = (T)deserializer.Deserialize( textReader );
                    settings._isChanged = false;
                }
            }
            catch (Exception x)
            {
                if (File.Exists( settingsFilePath ))
                {
                    string directoryPath = FileStringLib.GetDirectoryOfPath( settingsFilePath );
                    if (!String.IsNullOrEmpty( directoryPath ) && !directoryPath.Equals( settingsFilePath ))
                    {
                        FilesystemLib.Rollover( settingsFilePath, null, 10 );
                        Debug.WriteLine( "Unable to read the darn UserSettings file " + settingsFilePath + ", - so rolling-over that file and starting anew." );
                        Debug.WriteLine( "The exception was: " + x.Message );
                    }
                    else
                    {
                        Debug.WriteLine( "Unable to read the UserSettings file " + settingsFilePath + " nor to ascertain it's folder - so deleting it to start anew." );
                        Debug.WriteLine( "The exception was: " + x.Message );
                        File.Delete( settingsFilePath );
                    }
                }
                Debug.WriteLine( "Creating new " + typeof( T ) );
                settings = new T();
            }
            return settings;
        }
        #endregion

        #region SerializeToXML
        /// <summary>
        /// You must override this in your subclass to accomplish the serialization of your class to the XML file.
        /// </summary>
        protected virtual void SerializeToXML()
        {
            if (_instance == null)
            {
                throw new InvalidOperationException( "_instance should not be null." );
            }
            XmlSerializer serializer = new XmlSerializer( _instance.GetType() );
            using (var textWriter = new StreamWriter( s_settingsXMLFilePath ))
            {
                serializer.Serialize( textWriter, this );
            }
        }
        #endregion

        #endregion non-public methods

        #region fields

#if PRE_4
        /// <summary>
        /// This is the single instance of this class for this app-domain. It needs to be cast to the appropriate subclass of of UserSettings.
        /// </summary>
        private static UserSettings _instance;
#else
        /// <summary>
        /// This is the single instance of this class for this app-domain, and is actually an instance of a child-class of UserSettings.
        /// </summary>
        private static dynamic _instance;
#endif

        /// <summary>
        /// This is the filename (without the folder) of the XML settings file.
        /// </summary>
        protected static string s_settingsXMLFilename = @"ProgramUserSettings.xml";

        /// <summary>
        /// This denotes the full filesystem-path of the file to which these settings are saved.
        /// </summary>
        protected static string s_settingsXMLFilePath;

        /// <summary>
        /// This flag indicates whether the UserSettings object found it's underlying persistent-data file.
        /// </summary>
        protected static bool s_isFound;

        /// <summary>
        /// This flag indicates whether any of the property values have changed since the last time this was saved to disk.
        /// </summary>
        protected bool _isChanged;

        private static string _thisApplicationCurrentUserDataFolder;

        #endregion fields
    }
}
