using System;
using System.Net;
using System.Text;
using Hurst.LogNut.Util;


namespace Hurst.LogNut.OutputPipes
{
    /// <summary>
    /// This class contains configuration-settings for LogNut
    /// that pertain to sending out log-records via email.
    /// </summary>
    public class EmailOutputPipe : IOutputPipe
    {
        #region public properties

        #region IsEnabled
        /// <summary>
        /// Get or set whether output to email (which is this OutputPipe) is enabled. This defaults to false.
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Enable logging output to this output.
        /// Calling this is the same as setting the IsEnabled property to true.
        /// </summary>
        /// <remarks>
        /// This duplicates the function of the IsEnabled property setter, in order to provide a fluent API.
        /// </remarks>
        /// <returns>a reference to this EmailOutputPipe object, such that further method calls may be chained</returns>
        public EmailOutputPipe Enable()
        {
            this.IsEnabled = true;
            return this;
        }

        /// <summary>
        /// Turn off logging output to this output.
        /// Calling this is the same as setting the IsEnabled property to false.
        /// </summary>
        /// <remarks>
        /// This duplicates the function of the IsEnabled property setter, in order to provide a fluent API.
        /// </remarks>
        /// <returns>a reference to this EmailOutputPipe object, such that further method calls may be chained</returns>
        public EmailOutputPipe Disable()
        {
            this.IsEnabled = false;
            return this;
        }
        #endregion

        #region IsFailing
        /// <summary>
        /// Get or set whether output to this OutputPipe is has been failing.
        /// This is used to avoid repeatedly trying to send a log over a non-working channel.
        /// </summary>
        public bool IsFailing { get; set; }
        #endregion

        #region LogRecordFormatter
        /// <summary>
        /// Get the formatter used to generate the log-records.
        /// Leave this null unless you wish to override it.
        /// </summary>
        public ILogRecordFormatter LogRecordFormatter
        {
            get
            {
                return LogManager.LogRecordFormatter;
            }
        }
        #endregion

        #region LowestLevelToSend
        /// <summary>
        /// Get or set the LogLevel that a given log-record has to be in order to be sent out via email.
        /// The default is Warning, which means that all levels at or above Warning are sent (provided that IsEmailOutputEnabled is set true).
        /// </summary>
        public LogLevel LowestLevelToSend
        {
            get { return _lowestLevelToSend; }
            set { _lowestLevelToSend = value; }
        }
        #endregion

        #region Name
        /// <summary>
        /// Get the Name - the string-literal that uniquely identifies this class of IOutputPipe, in this case "Email".
        /// </summary>
        public string Name
        {
            get { return "Email"; }
        }
        #endregion

        #region SmtpPortNumber
        /// <summary>
        /// Get or set the TCP Port-number to use when emailing out log notifications.
        /// The default is port 25 (SMTP). Alternatively, port 587 (Submission) may be used.
        /// </summary>
        public int SmtpPortNumber
        {
            get { return this.EmailClient.Port; }
            set
            {
                if (value != this.EmailClient.Port)
                {
                    this.EmailClient.Port = value;
                    // If we change the SMTP configuration, then give the email a chance to work.
                    IsFailing = false;
                }
            }
        }
        #endregion

        #region SmtpServer
        /// <summary>
        /// Get or set the address of the SMTP email server to use when emailing out log notifications.
        /// </summary>
        public string SmtpServer
        {
            get { return this.EmailClient.Host; }
            set
            {
                if (!value.Equals( this.EmailClient.Host ))
                {
                    this.EmailClient.Host = value;
                    // If we change the SMTP server-name, then give the email a chance to work.
                    IsFailing = false;
                }
            }
        }
        #endregion

        #region SmtpUsername
        /// <summary>
        /// Get or set the user-name to use when accessing the SMTP email server to send log notifications via email.
        /// </summary>
        public string SmtpUsername
        {
            get { return this.EmailClient.Credentials.UserName; }
            set
            {
                if (!value.Equals( this.EmailClient.Credentials.UserName ))
                {
                    this.EmailClient.Credentials.UserName = value;
                    // If we change the SMTP configuration, then give the email a chance to work.
                    IsFailing = false;
                }
            }
        }
        #endregion

        #region SmtpPassword
        /// <summary>
        /// Get or set the password to use when accessing the SMTP email server to send log notifications via email.
        /// </summary>
        public string SmtpPassword
        {
            get { return this.EmailClient.Credentials.Password; }
            set
            {
                if (!value.Equals( this.EmailClient.Credentials.Password ))
                {
                    this.EmailClient.Credentials.Password = value;
                    // If we change the SMTP configuration, then give the email a chance to work.
                    IsFailing = false;
                }
            }
        }
        #endregion

        #region SourceAddress
        /// <summary>
        /// Get or set the email address from whom the log-records are to be sent.
        /// </summary>
        public string SourceAddress
        {
            get { return _sourceAddress; }
            set { _sourceAddress = value; }
        }
        #endregion

        #region DestinationAddress
        /// <summary>
        /// Get or set the email address to whom the log-records are to be sent.
        /// </summary>
        public string DestinationAddress
        {
            get { return _destinationAddress; }
            set { _destinationAddress = value; }
        }
        #endregion

        #endregion public properties

        #region public methods

        #region Clear
        /// <summary>
        /// Reset any resources used by this output-pipe back to their initial state.
        /// </summary>
        public void Clear()
        {
            IsFailing = false;
        }
        #endregion

        #region FinalizeWriteToFile
        /// <summary>
        /// Write whatever logging needs to be written to the log-output text-file and close it.
        /// In the case of this EmailOutputPipe, this does nothing.
        /// </summary>
        /// <param name="filenameExtension">Extension to change the filename to. Null = accept the appropriate default.</param>
        /// <remarks>
        /// You may not need to call this for most logging-output pipes.
        /// The original motivation for this was to provide a way for ETW output to be rendered
        /// from the .ETL file to a text file at the end of logging.
        /// </remarks>
        public void FinalizeWriteToFile( string filenameExtension )
        {
            // NOP
        }
        #endregion

        #region InitializeUponAttachment
        /// <summary>
        /// This gets called whenever an instance of this EmailOutputPipe gets attached to the logging system
        /// to perform any needed initialization.
        /// </summary>
        /// <param name="logConfig">This is provided in case this output-pipe needs to affect it.</param>
        public void InitializeUponAttachment( LogConfig logConfig )
        {
        }
        #endregion

        #region Read.. methods

#if !NETFX_CORE
        /// <summary>
        /// Retrieve the values from the Windows Registry
        /// to use as the actual values for the configuration settings within this class.
        /// </summary>
        public void ReadSettingsFromRegistry()
        {
            //CBL ?
        }
#endif

        #endregion Read.. methods

        #region SaveConfiguration
        /// <summary>
        /// Save the configuration values to the persistent store.
        /// </summary>
        public void SaveConfiguration()
        {
            // CBL Implement
        }
        #endregion

        #region SetConfigurationFromText
        /// <summary>
        /// Given strings that are intended to represent a property-name and a value,
        /// if these apply to this output-pipe, set the appropriate configuration-value.
        /// </summary>
        /// <param name="propertyName">the name of the configuration-property to set</param>
        /// <param name="propertyValue">a string representing the value to set it to</param>
        /// <returns>true only if the given property-name matched a configuration-setting for this particular IOutputPipe implementation</returns>
        public bool SetConfigurationFromText( string propertyName, string propertyValue )
        {
            bool ok = false;
            switch (propertyName)
            {
                case "LowestLevelToSend":
                    LowestLevelToSend = (LogLevel)Enum.Parse( typeof( LogLevel ), propertyValue.PutIntoTypeOfCasing( StringLib.TypeOfCasing.Titlecased ) );
                    break;
                case "SmtpPortNumber":
                    SmtpPortNumber = Int32.Parse( propertyValue.RemoveAll( ',' ) );
                    break;
                default:
                    break;
            }
            return ok;
        }
        #endregion

        #region SetToDefaults
        /// <summary>
        /// Set any and all configuration-settings for this output-pipe to their default values.
        /// </summary>
        public void SetToDefaults()
        {
            IsEnabled = false;
            _sourceAddress = null;
            _destinationAddress = null;
            if (_emailClient != null)
            {
                _emailClient.SetToDefaults();
            }
        }
        #endregion

        #region ToString
#if DEBUG
        /// <summary>
        /// Override the ToString method to provide a more useful display.
        /// </summary>
        /// <returns>a string the denotes the state of this object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder( "EmailOutputPipe(" );
            // Display the properties that are at other than their default state.
            if (IsEnabled)
            {
                sb.Append( " IsEnabled," );
            }
            if (_sourceAddress != null)
            {
                sb.Append( " SourceAddress=" ).Append( SourceAddress ).Append( "," );
            }
            if (_destinationAddress != null)
            {
                sb.Append( " DestinationAddress=" ).Append( DestinationAddress ).Append( "," );
            }
            if (_lowestLevelToSend != default( LogLevel ))
            {
                sb.Append( " LowestLevelToSend=" ).Append( LowestLevelToSend ).Append( "," );
            }
            if (_emailClient != null)
            {
                sb.Append( " EmailClient=" ).Append( EmailClient ).Append( "," );
            }
            return sb.ToStringAndEndList();
        }
#endif
        #endregion

        #region Write
        /// <summary>
        /// Send output to this OutputPipe.
        /// </summary>
        /// <param name="request">what to write</param>
        /// <returns>true if the request actually went out</returns>
        public bool Write( LogSendRequest request )
        {
            bool wasSent = false;
            if (IsEnabled && !IsFailing)
            {
                LogRecord logRecord = request.Record;
                LogLevel levelAllowed = LowestLevelToSend;
                if (logRecord.IsAtLeastOfLevel( levelAllowed ))
                {
                    // First check the parameters..
                    if (StringLib.HasNothing( SmtpServer ))
                    {
                        NutUtil.WriteToConsole( "You need to set the host-name for the SMTP server before you can send out emails." );
                        IsFailing = true;
                        return false;
                    }
                    if (StringLib.HasNothing( SourceAddress ))
                    {
                        NutUtil.WriteToConsole( "You need to set the EmailFromAddress before you can send out emails." );
                        IsFailing = true;
            return false;
        }
                    if (StringLib.HasNothing( DestinationAddress ))
                    {
                        NutUtil.WriteToConsole( "You need to set the EmailRecipient before you can send out emails." );
                        IsFailing = true;
                        return false;
                    }

                    string fromAddress = SourceAddress;
                    string toAddress = DestinationAddress;
                    //CBL  Here, we need to adjust the configuration properties to show what we want for email,
                    //     which would include setting IsToShowSourceHost to true.
                    //var configForEmail = new LogConfig();
                    //configForEmail.IsToShowSourceHost = true;
                    //CBL This content-configuration needs to be adjusted, otherwise there is no need in deriving it twice here.
                    string subject = "LogNut log-record, " + LogRecordFormatter.GetPrefix( logRecord, LogManager.Config );
                    var sb = new StringBuilder();
                    // For the body of the email, include ALL of the optional fields within the prefix.
                    // CBL This copy of 'Config' needs to be set! ?
                    string prefix = "LogNut log-record, " + LogRecordFormatter.GetPrefix( logRecord, LogManager.Config );

                    sb.Append( prefix ).Append( " " ).Append( logRecord.Message );
                    string messageBody = sb.ToString();

                    EmailClient.SendEmailMessage( fromAddress, toAddress, subject, messageBody );
                    wasSent = true;
                }
            }
            return wasSent;
        }
        #endregion

        #region WriteSettingsToRegistry
        /// <summary>
        /// Retrieve the values from the Windows Registry
        /// to use as the actual values for the configuration settings within this class.
        /// </summary>
        public void WriteSettingsToRegistry()
        {
            //CBL ?
        }
        #endregion

        #endregion public methods

        #region internal implementation

        internal NetworkCredential NetworkCredential
        {
            get
            {
                return this.EmailClient.Credentials;
            }
        }

        internal EmailClient EmailClient
        {
            get
            {
                if (_emailClient == null)
                {
                    _emailClient = new EmailClient();
                }
                return _emailClient;
            }
        }

        #region fields

        /// <summary>
        /// This is the minimum LogLevel that a given log-record has to be in order to get sent out via email.
        /// For example, if this is set to Error, then only Error and Critical log-records get emailed.
        /// The default is Warning.
        /// </summary>
        private LogLevel _lowestLevelToSend = LogLevel.Warning;

        /// <summary>
        /// This is the email address from which the email notifications of a log-record are sent.
        /// </summary>
        private string _sourceAddress;

        /// <summary>
        /// This is the email address of the recipient to whom to send the email notifications of a log-record.
        /// </summary>
        private string _destinationAddress;

        internal static EmailClient _emailClient;

        #endregion fields

        #endregion internal implementation
    }
}
