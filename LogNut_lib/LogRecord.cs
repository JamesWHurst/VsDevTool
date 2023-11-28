using System;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using Hurst.LogNut.Util;


// define PRE_4 if this is to be compiled for versions of .NET earlier than 4.0


namespace Hurst.LogNut
{
    /// <summary>
    /// A LogRecord object represents a single logger output event, encapsulating all the
    /// information that gives meaning to this specific record within the log facility.
    /// It consists of a Message, and additional information that gives context to that Message.
    /// </summary>
    public class LogRecord
    {
        #region constructors
        /// <summary>
        /// The default constructor
        /// </summary>
        public LogRecord()
        {
        }

        /// <summary>
        /// Create a new LogRecord that has the given message and optionally an Id.
        /// </summary>
        /// <param name="id">a string denoting the Id to give it. If null - creates one. If empty string - just leaves that as the value.</param>
        /// <param name="message">the textual content to give the new log-record</param>
        /// <param name="isToCreateId">this dictates whether to give the new log-record an Id value</param>
        public LogRecord( string id, string message, bool isToCreateId )
        {
            this.Message = message;
            if (id == null)
            {
                if (isToCreateId)
                {
                    this.GetIdCreateIfEmpty();
                }
            }
            else
            {
                this.Id = id;
            }
        }

        /// <summary>
        /// The constructor that composes a LogRecord completely determined from the argument values.
        /// </summary>
        /// <param name="id">a string denoting the Id to give it. Can be null.</param>
        /// <param name="message">The textual content of this record</param>
        /// <param name="level">The LogLevel assigned to this record</param>
        /// <param name="cat">the LogCategory assigned to this record, which can serve to help further segregate the log records into some sort of organization beyond that of levels</param>
        /// <param name="when">the timestamp for this record</param>
        /// <param name="sourceHost">The name of the computer that the program was running on when it called this method</param>
        /// <param name="sourceLogger">The name to assign to this logger</param>
        /// <param name="subjectProgramName">The name of the subject-program that is making this log-record</param>
        /// <param name="subjectProgramVersion">The version of the subject-program that is making this log-record</param>
        /// <param name="threadId">The identity of the execution-thread within the subject-program that is making this log-record</param>
        /// <param name="user">The name of the user of the subject-program that is making this log-record</param>
        /// <param name="isInDesignMode">This indicates whether this program is in design-time mode, ie running within Blend or Cider's design surface</param>
        public LogRecord( string id,
                          string message,
                          LogLevel level,
                          LogCategory cat,
                          DateTime when,
                          string sourceHost,
                          string sourceLogger,
                          string subjectProgramName,
                          string subjectProgramVersion,
                          int threadId,
                          string user,
                          bool isInDesignMode )
        {
            Id = id;
            Level = level;
            Category = cat;
            _timeStamp = when;
            IsInDesignMode = isInDesignMode;
            Message = message;
            SubjectProgramName = subjectProgramName;
            SubjectProgramVersion = subjectProgramVersion;
            SourceHost = sourceHost;
            SourceLogger = sourceLogger;
            ThreadId = threadId;
            Username = user;
        }
        #endregion constructors

        #region public properties

        #region Category
        /// <summary>
        /// Get or set the LogCategory associated with this log-record.
        /// The initial default value of this is <see cref="LogCategory.Emtpy"/>,
        /// which means no category is specified.
        /// </summary>
        public LogCategory Category
        {
            get { return _category; }
            set
            {
                _category = value;
            }
        }
        #endregion

        #region DesignModeText
        /// <summary>
        /// Get the string that denotes whether this log-record
        /// was generated during design-time,
        /// with True being a string containing the letter v,
        /// and False simply an empty string.
        /// </summary>
        [XmlIgnore]
        public string DesignModeText
        {
            get { return (this.IsInDesignMode ? "v" : String.Empty); }
        }
        #endregion

        #region Id
        /// <summary>
        /// Get or set the string that serves as the unique identifier for this log-record.
        /// This contains the GUID encoded as Base64 if it has been created - otherwise it is null.
        /// </summary>
        [DataMember]
        [XmlAttribute( "id" )]
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }
        #endregion

        #region IsInDesignMode
        /// <summary>
        /// This indicates whether this code is being executed in the context of Visual Studio "Cider" (the visual designer) or Blend,
        /// as opposed to running normally as an application.
        /// </summary>
        [DataMember]
        [XmlAttribute( "V" )]
        public bool IsInDesignMode
        {
            get { return _isInDesignMode; }
            set { _isInDesignMode = value; }
        }
        #endregion

        #region Level
        /// <summary>
        /// Get or set the LogLevel that is assigned to this log record.
        /// </summary>
        [DataMember]
        [XmlAttribute( "Level" )]
        public LogLevel Level { get; set; }
        #endregion

        #region Message
        /// <summary>
        /// Get or set the text that comprises the actual information that this log record is intended to carry.
        /// </summary>
        [DataMember]
        public string Message
        {
            get { return _messageText; }
            set { _messageText = value; }
        }
        #endregion

        #region SourceHost
        /// <summary>
        /// Get or set the machine-name of the host that sent this log record.
        /// </summary>
        [DataMember]
        [XmlAttribute( "Host" )]
        public string SourceHost
        {
            get { return _sourceHost; }
            set { _sourceHost = value; }
        }
        #endregion

        #region SourceLogger
        /// <summary>
        /// Get or set the name of the Logger that sent this log record.
        /// Loggers are created with either a unique name, or else assigned a default name if non is specified as in the case of a "default" logger.
        /// </summary>
        [DataMember]
        [XmlAttribute( "Logger" )]
        public string SourceLogger
        {
            get { return _sourceLogger; }
            set { _sourceLogger = value; }
        }
        #endregion

        #region SubjectProgramName
        /// <summary>
        /// Get or set the name of the subject-program that sent this log record.
        /// </summary>
        [DataMember]
        [XmlAttribute( "Prog" )]
        public string SubjectProgramName
        {
            get { return _subjectProgramName; }
            set { _subjectProgramName = value; }
        }
        #endregion

        #region SubjectProgramVersion
        /// <summary>
        /// Get or set the version of the subject-program that sent this log record.
        /// </summary>
        [DataMember]
        [XmlAttribute( "Ver" )]
        public string SubjectProgramVersion
        {
            get { return _subjectProgramVersion; }
            set { _subjectProgramVersion = value; }
        }
        #endregion

        #region ThreadId
        /// <summary>
        /// Get or set the identifier associated with the thread that originated this log record.
        /// </summary>
        [XmlAttribute( "Thread" )]
        public int ThreadId
        {
            get { return _threadId; }
            set { _threadId = value; }
        }
        #endregion

        #region Username
        /// <summary>
        /// Get or set the "username" string that identifies the user who is executing the subject-program
        /// at the time this log-record was created.
        /// </summary>
        [DataMember]
        [XmlAttribute( "User" )]
        public string Username
        {
            get { return _username; }
            set { _username = value; }
        }
        #endregion

        #region When
        /// <summary>
        /// Get or set the timestamp that indicates when this log record was generated by it's source Logger.
        /// It's default value is DateTime.MinValue.
        /// </summary>
        /// <remarks>
        /// This will be formatted as YYYY-MM-DD HH:MM:SS.S
        /// </remarks>
        [DataMember]
        [XmlAttribute( "When" )]
        public DateTime When
        {
            get { return _timeStamp; }
            set { _timeStamp = value; }
        }
        #endregion

        #endregion public properties

        #region public methods

        #region AsText
        /// <summary>
        /// Return a string representation of this LogRecord
        /// in the 'Standard' form, as would be written to an output file or non-grid view.
        /// </summary>
        /// <param name="config">a LogConfig that controls what parts get included; if this is null then everything is</param>
        /// <returns>this in Standard text form</returns>
        /// <remarks>
        /// Std File Output Format (but without the spaces):
        /// 2011-4-15 23:51:10.123[Host / Subjectprogram:1.0.3, Username | 7; Logger1 \ Error {Cat} *v]
        ///
        /// For that case where it is desired to be able to paste the file-output into a spreadsheet such as Excel,
        /// set argument <c>isToBeSpreadsheetCompatible</c> true.
        /// In that case, every line of text has the prefix placed at the beginning,
        /// and a TAB character is used to separate the prefix from the message.
        /// </remarks>
        public string AsText( LogConfig config )
        {
            // This functionality is performed by the replaceable LogRecordFormatter.
            // to follow the general design of Java logging.

            return LogManager.LogRecordFormatter.GetLogRecordAsText( this, config );
        }
        #endregion

        #region FromText
        /// <summary>
        /// Create a return a new LogRecord object from the given textual <c>SimpleText</c> representation.
        /// </summary>
        /// <param name="text">a string value that corresponds to how a log-record is displayed</param>
        /// <returns>a new LogRecord created from that text, or null if unable to parse the text</returns>
        public static LogRecord FromText( string text )
        {
            string reason;
            return FromText( text, out reason );
        }

        /// <summary>
        /// Create a return a new LogRecord object from the given textual <c>SimpleText</c> representation.
        /// </summary>
        /// <param name="text">a string value that corresponds to how a log-record is displayed</param>
        /// <param name="reason">if the text fails to parse - this is set to a description of the reason for it, otherwise this is set to null</param>
        /// <returns>a new LogRecord created from that text, or null if unable to parse the text</returns>
        public static LogRecord FromText( string text, out string reason )
        {
            // Std File Output Format (but without the spaces):
            // 2011-4-15 23:51:10.123[Host / Subjectprogram:1.0.3, Username | 7; Logger1 \ Error {Cat} *v]

            // Todo: Make this work for XML formats as well.
            reason = null;
            if (StringLib.HasNothing( text ))
            {
                reason = "text is empty";
                return null;
            }

            DateTime t = default( DateTime );
            string idText = null;
            string host, subjectProgram, subjectProgramVersion, user, loggerName, content;
            int threadId;
            LogLevel level = default( LogLevel );
            string catName = null;
            bool isInDesignMode = false;

            string prefixText = text;
            string messageText = null;

            // Note: If there is an id, then there must also be a prefix and thus
            //       the matching pair of brackets.

            int indexOfLeftBracket = text.IndexOf( '[' );
            int indexOfRightBracket = text.IndexOf( ']' );
            if (indexOfLeftBracket >= 0 && indexOfRightBracket > indexOfLeftBracket)
            {
                // There are brackets.

                // Isolate out the prefix and the message..
                prefixText = text.Substring( 0, indexOfRightBracket + 1 );
                messageText = text.Substring( indexOfRightBracket + 2 );

                // Check for the id..
                int indexOfFirstComma = prefixText.IndexOf( ',' );
                if (indexOfFirstComma > 0 && indexOfFirstComma < indexOfLeftBracket)
                {
                    // There is a comma before the left-bracket. Thus - an id is present.
                    idText = prefixText.Substring( 0, indexOfFirstComma );
                    prefixText = prefixText.Substring( indexOfFirstComma + 1 );
                    indexOfLeftBracket = prefixText.IndexOf( '[' );
                    indexOfRightBracket = prefixText.IndexOf( ']' );
                }

                // Timestamp..
                string partAfterDate = prefixText.Substring( indexOfLeftBracket + 1 );
                string datePart = prefixText.PartBefore( "[" );

                if (StringLib.HasSomething( datePart ))
                {
                    if (DateTime.TryParse( datePart, out t ))
                    {
                        partAfterDate = prefixText.PartAfter( "[" );
                    }
                    else
                    {
                        reason = "unrecognized date/time part";
                        return null;
                    }
                }
                // Hostname..
                string partAfterHost;
                host = partAfterDate.PartBefore( LogManager.DelimiterAfterHost );
                if (StringLib.HasSomething( host ))
                {
                    partAfterHost = partAfterDate.PartAfter( LogManager.DelimiterAfterHost );
                }
                else
                {
                    partAfterHost = partAfterDate;
                    host = null;
                }
                // subject-program..
                string partAfterProgram = partAfterHost;
                subjectProgram = partAfterHost.PartBefore( LogManager.DelimiterAfterProgram );
                if (StringLib.HasSomething( subjectProgram ))
                {
                    partAfterProgram = partAfterHost.PartAfter( LogManager.DelimiterAfterProgram );
                }
                else
                {
                    subjectProgram = null;
                }
                // subject-program version..
                string partAfterVersion = partAfterProgram;
                subjectProgramVersion = partAfterProgram.PartBefore( LogManager.DelimiterAfterVersion );
                if (StringLib.HasSomething( subjectProgramVersion ))
                {
                    partAfterVersion = partAfterProgram.PartAfter( LogManager.DelimiterAfterVersion );
                }
                else
                {
                    subjectProgramVersion = null;
                }
                // user..
                string partAfterUser = partAfterVersion;
                user = partAfterVersion.PartBefore( LogManager.DelimiterAfterUser );
                if (StringLib.HasSomething( user ))
                {
                    partAfterUser = partAfterVersion.PartAfter( LogManager.DelimiterAfterUser );
                }
                else
                {
                    user = null;
                }
                // thread
                string partAfterThread = partAfterUser;
                string threadIdText = partAfterUser.PartBefore( LogManager.DelimiterAfterThread );
                if (StringLib.HasSomething( threadIdText ))
                {
                    if (!Int32.TryParse( threadIdText, out threadId ))
                    {
                        reason = "thread part is not recognized as an integere";
                        return null;
                    }
                    partAfterThread = partAfterUser.PartAfter( LogManager.DelimiterAfterThread );
                }
                else
                {
                    threadId = 0;
                }
                // logger-name..
                string partAfterLoggerName = partAfterThread;
                loggerName = partAfterThread.PartBefore( LogManager.DelimiterAfterLoggerName );
                if (StringLib.HasSomething( loggerName ))
                {
                    partAfterLoggerName = partAfterThread.PartAfter( LogManager.DelimiterAfterLoggerName );
                }
                else
                {
                    loggerName = null;
                }
                // level,
                string levelText;
                // category,
                // and whether it was in design-mode..
                int indexOfCatStartDelimiter = partAfterLoggerName.IndexOf( LogManager.DelimiterBeforeCategory );
                if (indexOfCatStartDelimiter >= 0)
                {
                    int indexOfCatEndDelimiter = partAfterLoggerName.IndexOf( LogManager.DelimiterAfterCategory );
                    if (indexOfCatEndDelimiter > indexOfCatStartDelimiter)
                    {
                        catName = partAfterLoggerName.Substring( indexOfCatStartDelimiter + 1, indexOfCatEndDelimiter - indexOfCatStartDelimiter - 1 );

                        //CBL Duplicate of below. This should be cleaned-up..

                        if (partAfterLoggerName.IndexOf( "*v" ) >= 0)
                        {
                            isInDesignMode = true;
                        }
                        levelText = partAfterLoggerName.PartBefore( LogManager.DelimiterBeforeCategory );
                        if (StringLib.HasSomething( levelText ))
                        {
                            if (!NutUtil.TryParseToLogLevel( levelText, out level ))
                            {
                                reason = "invalid LogLevel";
                                return null;
                            }
                        }
                    }
                }

                if (catName == null)
                {
                    if (partAfterLoggerName.IndexOf( "*v" ) >= 0)
                    {
                        isInDesignMode = true;
                        levelText = partAfterLoggerName.PartBefore( LogManager.DelimiterBeforeDesignViewIndicator );
                    }
                    else
                    {
                        levelText = partAfterLoggerName.PartBefore( "]" );
                    }
                    if (StringLib.HasSomething( levelText ))
                    {
                        if (!NutUtil.TryParseToLogLevel( levelText, out level ))
                        {
                            reason = "invalid LogLevel";
                            return null;
                        }
                    }
                }
                // message..
                content = messageText;
                //content = partAfterLoggerName.PartAfter( "]" ).TrimStart();
            }
            else // no brackets-squarepants
            {
                host = subjectProgram = subjectProgramVersion = user = loggerName = null;
                threadId = 0;
                content = text;
            }
            //CBL I need to decide how to represent the Category within that header.
            LogCategory cat;
            if (catName == null)
            {
                cat = LogCategory.Empty;
            }
            else
            {
                cat = LogManager.GetCategory( catName );
            }
#if !PRE_4
            var newRecord = new LogRecord( id: idText,
                                          message: content,
                                          level: level,
                                          cat: cat,
                                          when: t,
                                          sourceHost: host,
                                          sourceLogger: loggerName,
                                          subjectProgramName: subjectProgram,
                                          subjectProgramVersion: subjectProgramVersion,
                                          threadId: threadId,
                                          user: user,
                                          isInDesignMode: isInDesignMode );
#else
            var newRecord = new LogRecord( idText,           // id
                                          content,           // message
                                          level,             // level
                                          LogCategory.Empty, // cat
                                          t,                 // when
                                          host,              // sourceHost
                                          loggerName,        // sourceLogger
                                          subjectProgram,
                                          subjectProgramVersion,
                                          threadId,
                                          user,
                                          isInDesignMode );
#endif
            return newRecord;
        }
        #endregion FromText

        #region GetIdCreateIfEmpty
        /// <summary>
        /// Get or set the string that serves as the unique identifier for this log-record.
        /// This string contains the Base64-encoded text that denotes it's value.
        /// </summary>
        /// <returns>the id</returns>
        public string GetIdCreateIfEmpty()
        {
            // Delay creating this value until this property is needed.
            if (_id == null)
            {
                Guid guid = Guid.NewGuid();
                byte[] byteArray = guid.ToByteArray();
                string rawId = Convert.ToBase64String( byteArray, 0, byteArray.Length );
                // Remove the trailing "==" if present.
                _id = rawId.WithoutAtEnd( "==" );
            }
            return _id;
        }
        #endregion

        #region IsAtLeastOfLevel
        /// <summary>
        /// Return true if the record-level of this LogRecord is at least as high (as measured by the
        /// somewhat-arbitrarily assigned degree of urgency) as the given record-level.
        /// </summary>
        /// <param name="levelInQuestion">the given LogLevel to match this log-record against</param>
        /// <returns>true if this log-record is at the given level or higher</returns>
        /// <remarks>
        /// The ordering of levels is indicated by it's integer value and is in this order: Info, Debug, Warning, Error, Critical.
        /// </remarks>
        public bool IsAtLeastOfLevel( LogLevel levelInQuestion )
        {
            return (this.Level >= levelInQuestion);
        }
        #endregion

        #region ToString
        /// <summary>
        /// Override the ToString method to provide a more useful display.
        /// </summary>
        /// <returns>a string representing the entire record in the normal textual format</returns>
        public override string ToString()
        {
            var sb = new StringBuilder( "LogRecord(" );
            if (_id != null)
            {
                sb.Append( "ID = " ).Append( _id ).Append( ", " );
            }
            sb.Append( this.AsText( null ) ).Append( ")" );
            return sb.ToString();
        }
        #endregion

        #endregion public methods

        #region fields

        /// <summary>
        /// This denotes the LogCategory that this log-output is in, if any.
        /// </summary>
        private LogCategory _category = LogCategory.Empty;

        /// <summary>
        /// This serves as the unique identifier for this log-record.
        /// It is a string that contains the 20-byte ASCII85-encoded GUID.
        /// </summary>
        private string _id;

        /// <summary>
        /// This indicates whether this code is being executed in the context of Visual Studio "Cider" (the visual designer) or Blend,
        /// as opposed to running normally as an application.
        /// </summary>
        private bool _isInDesignMode;

        /// <summary>
        /// This represents the text that comprises the actual information that this log record is intended to carry.
        /// </summary>
        private string _messageText;

        /// <summary>
        /// The machine-name of the host that sent this log record.
        /// </summary>
        private string _sourceHost;

        private string _sourceLogger;

        /// <summary>
        /// This is the name of the 'subject' program - i.e. the application whose operation is being logged.
        /// </summary>
        private string _subjectProgramName;

        /// <summary>
        /// The version, in simple text form, of the subject-program.
        /// </summary>
        private string _subjectProgramVersion;

        /// <summary>
        /// The identifier associated with the thread that originated this log record.
        /// </summary>
        private int _threadId;

        /// <summary>
        /// This denotes the moment in time when this log-output was generated.
        /// </summary>
        private DateTime _timeStamp;

        /// <summary>
        /// The "username" string that identifies the user who is executing the subject-program
        /// at the time this log-record was created.
        /// </summary>
        private string _username;

        #endregion fields
    }
}
