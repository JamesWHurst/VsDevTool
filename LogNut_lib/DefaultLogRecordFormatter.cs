using System;
using System.Text;
using System.Text.RegularExpressions;
using Hurst.LogNut.Util;


namespace Hurst.LogNut
{
    /// <summary>
    /// This implementation of ILogRecordFormatter simply yields a textual representation of the given LogRecord
    /// in our simple standard format.
    /// </summary>
    public class DefaultLogRecordFormatter : ILogRecordFormatter
    {
        #region The
        /// <summary>
        /// Get the singleton-instance of the DefaultLogRecordFormatter class.
        /// </summary>
        public static ILogRecordFormatter The
        {
            get
            {
                if (_instance is null)
                {
                    _instance = new DefaultLogRecordFormatter();
                }
                return _instance;
            }
        }
        #endregion

        #region GetLogRecordAsText
        /// <summary>
        /// Return a textual representation of the given LogRecord.
        /// Implement this method to format the LogRecord according to your needs.
        /// </summary>
        /// <param name="logRecord">the LogRecord to render into your chosen format</param>
        /// <param name="config">the LogConfig that specifies what to include. If you pass null for this - a new LogConfig is created</param>
        /// <returns>a string representation of the given LogRecord</returns>
        public string GetLogRecordAsText( LogRecord logRecord, LogConfig config )
        {
            LogConfig configToUse;
            if (config is null)
            {
                configToUse = new LogConfig();
            }
            else
            {
                configToUse = config;
            }

            var sb = new StringBuilder();

            if (configToUse.IsFileOutputSpreadsheetCompatible)
            {
                var lines = Regex.Split( logRecord.Message, "\r\n|\r|\n" );
                string prefix = GetPrefix( logRecord, configToUse );

                // I use the for-loop instead of foreach, here,
                // because I need to treat the final line separately.
                for (int i = 0; i < lines.Length; i++)
                {
                    string lineOfText = lines[i];
                    sb.Append( prefix );
                    sb.Append( "\t" );
                    sb.Append( lineOfText );
                    // Do NOT append the new-line on the final line.
                    if (i < lines.Length - 1)
                    {
                        sb.AppendLine();
                    }
                }
            }
            else
            {
                if (configToUse.IsToShowPrefix)
                {
                    string prefix = GetPrefix( logRecord, configToUse );
                    sb.Append( prefix );
                    // Show a space (or tab is to be spreadsheet-compatible) between the prefix and the message-body,
                    // only if there is a non-empty message-body.
                    if (logRecord.Message.HasSomething())
                    {
                        sb.Append( " " );
                    }
                }
                sb.Append( logRecord.Message );
            }
            return sb.ToString();
        }
        #endregion

        #region GetPrefix
        /// <summary>
        /// Return the prefix-text as it would be transmitted in a log record with the given parameters set as indicated.
        /// This also appends the "v" if in design-mode.
        /// </summary>
        /// <param name="logRecord">the log-record to base the prefix upon</param>
        /// <param name="config">this LogConfig dictates what to include within the prefix</param>
        /// <returns>A string prefix for displaying along with the Message for this log-record</returns>
        /// <remarks>
        /// This is the one method that has the responsibility for actually creating the prefix, when using the LogNut-standard
        /// plain-text prefix style, for the log traces. It composes it based entirely upon the argument values and does not
        /// use any settings, thus it is useful for unit-testing also.
        /// 
        /// It does not end with a tab-character, even when IsFileOutputSpreadsheetCompatible is set.
        /// </remarks>
        public string GetPrefix( LogRecord logRecord, LogConfig config )
        {
            // The general format is: 2011-04-15 23:51:10.001[hostname/subjectProgram:1.0.1.3,James|7;myLogger\Warning*v] Message
            // If the Id is to be included, it is prefixed to that with a comma-space delimiter.
            // Note: There is always a space at the end of the prefix.
            var sb = new StringBuilder();
            bool beSpreadsheetCompatible = config.IsFileOutputSpreadsheetCompatible;

            // The Id.
            if (config.IsToShowId && logRecord.Id != null)
            {
                sb.Append( logRecord.Id ).Append( ", " );
            }
            // The timestamp.
            bool doShowTimestamp = config.IsToShowTimestamp && logRecord.When != default( DateTime );
            if (doShowTimestamp)
            {
                sb.Append( GetTimeStamp( logRecord.When, config, beSpreadsheetCompatible ) );
            }
            // Add the elapsed time, if called for..
            if (config.IsToShowElapsedTime)
            {
                if (doShowTimestamp)
                {
                    // For spreadsheet-compatible output, separate the elapsed-time from the timestamp with a TAB-character.
                    if (beSpreadsheetCompatible)
                    {
                        sb.Append( "\t" );
                    }
                    else
                    {
                        sb.Append( " (" );
                    }
                }
                else
                {
                    if (!beSpreadsheetCompatible)
                    {
                        sb.Append( "(" );
                    }
                }
                TimeSpan elapsedTime = logRecord.When - LogManager.ReferenceTime;
                string timeText;
                if (config.IsToShowElapsedTimeInSeconds)
                {
                    int numberOfDigits;
                    if (config.DecimalPlacesForSeconds.HasValue)
                    {
                        numberOfDigits = config.DecimalPlacesForSeconds.Value;
                    }
                    else
                    {
                        // Use six as the default value.
                        numberOfDigits = 6;
                    }
                    string formatString;
                    if (config.IsFileOutputPrefixOfFixedWidth)
                    {
                        formatString = "{0:00." + new String( '0', numberOfDigits ) + "}";
                    }
                    else
                    {
                        formatString = "{0:0." + new String( '0', numberOfDigits ) + "}";
                    }
                    timeText = String.Format( formatString, elapsedTime.TotalSeconds );
                }
                else
                {
                    timeText = elapsedTime.ToString();
                    if (timeText.StartsWith( "00:00:" ))
                    {
                        if (timeText.StartsWith( "00:00:0" ))
                        {
                            timeText = timeText.Substring( 7 );
                        }
                        else
                        {
                            timeText = timeText.Substring( 6 );
                        }
                    }
                    if (config.DecimalPlacesForSeconds.HasValue && config.DecimalPlacesForSeconds.Value != 7)
                    {
                        int posOfPeriod = timeText.IndexOf( '.' );
                        if (posOfPeriod > -1)
                        {
                            timeText = timeText.Substring( 0, posOfPeriod + config.DecimalPlacesForSeconds.Value + 1 );
                        }
                    }
                }
                if (beSpreadsheetCompatible)
                {
                    // For spreadsheet-compatibility, pad the elapsed-time to the left with spaces to make it 9 characters wide.
                    sb.Append( timeText.PadLeft( 9 ) );
                    //sb.Append( "\t" );
                }
                else
                {
                    sb.Append( timeText );
                    sb.Append( ")" );
                }
            }
            bool isInDesignView = logRecord.IsInDesignMode;
            bool doShowHost = config.IsToShowSourceHost && StringLib.HasSomething( logRecord.SourceHost );
            bool doShowProgram = config.IsToShowSubjectProgram && StringLib.HasSomething( logRecord.SubjectProgramName );
            bool doShowVersion = config.IsToShowSubjectProgramVersion && StringLib.HasSomething( logRecord.SubjectProgramVersion );
            bool doShowUser = config.IsToShowUser && StringLib.HasSomething( logRecord.Username );

            // Decide whether to show the logger-name.
            string loggerName = logRecord.SourceLogger;
            bool doShowLoggerName = false;
            if (config.IsToShowLoggerName)
            {
                if (config.IsToShowLoggerNameWhenDefault)
                {
                    doShowLoggerName = true;
                    if (StringLib.HasNothing( loggerName ))
                    {
                        loggerName = LogManager.NameOfDefaultLogger;
                    }
                }
                else
                {
                    doShowLoggerName = StringLib.HasSomething( loggerName ) && !loggerName.Equals( LogManager.NameOfDefaultLogger );
                }
            }

            // Only add the begin/end brackets if there is something to go within them.
            bool showLevel = (config.IsToShowLevel && (!config.IsToShowLevelOnlyForWarningsAndAbove || logRecord.Level >= LogLevel.Warning));
            //CBL  Ok, so which is it -- above or below?
            //bool showLevel = (config.ContentConfig.IsToShowLevel && ((!config.ContentConfig.IsToShowLevelOnlyForWarningsAndAbove || logger.OverrideOfIsToShowLevelOnlyForWarningsAndAbove == false) || logRecord.Level >= LogLevel.Warning));
            bool doShowCat = config.IsToShowCategory && ( logRecord.Category != null && !logRecord.Category.IsEmpty );

            bool hasSomethingToShow = doShowHost || doShowProgram || doShowVersion ||
                                      doShowUser || config.IsToShowThread ||
                                      doShowLoggerName || showLevel || doShowCat || isInDesignView;

            if (hasSomethingToShow)
            {
                var sbPart = new StringBuilder();
                bool hasOutputYet = false;
                if (!beSpreadsheetCompatible)
                {
                    sbPart.Append( "[" );
                }

                // Put the host-name..
                if (doShowHost)
                {
                    sbPart.Append( logRecord.SourceHost ).Append( LogManager.DelimiterAfterHost );
                    hasOutputYet = true;
                }
                // Put the name of the subject-program..
                if (doShowProgram)
                {
                    sbPart.Append( logRecord.SubjectProgramName ).Append( LogManager.DelimiterAfterProgram );
                    hasOutputYet = true;
                }
                // Put the version of the subject-program..
                if (doShowVersion)
                {
                    sbPart.Append( logRecord.SubjectProgramVersion ).Append( LogManager.DelimiterAfterVersion );
                    hasOutputYet = true;
                }
                // Put the user-name..
                if (doShowUser)
                {
                    sbPart.Append( logRecord.Username ).Append( LogManager.DelimiterAfterUser );
                    hasOutputYet = true;
                }
                // Put the thread-id..
                if (config.IsToShowThread && logRecord.ThreadId != 0)
                {
                    if (hasOutputYet && beSpreadsheetCompatible)
                    {
                        sbPart.Append( "\t" );
                    }
                    // Zero is never a valid thread-id, at least in Windows.
                    // See: http://blogs.msdn.com/b/oldnewthing/archive/2004/02/23/78395.aspx
                    sbPart.Append( logRecord.ThreadId );
                    if (!beSpreadsheetCompatible)
                    {
                        sbPart.Append( LogManager.DelimiterAfterThread );
                    }
                    hasOutputYet = true;
                }
                // Put the logger-name..
                if (doShowLoggerName)
                {
                    if (hasOutputYet && beSpreadsheetCompatible)
                    {
                        sbPart.Append( "\t" );
                    }
                    sbPart.Append( loggerName );
                    if ((showLevel || doShowCat) && !beSpreadsheetCompatible)
                    {
                        sbPart.Append( LogManager.DelimiterAfterLoggerName );
                    }
                    hasOutputYet = true;
                }
                //CBL Check the logic of this next line!
                else if (config.IsToShowLoggerName && hasOutputYet && beSpreadsheetCompatible)
                {
                    // Wants to show it, but there was no name or it was the default name.
                    // When outputing for spreadsheets, put a tab to go to the next field.
                    sbPart.Append( "\t" );
                    hasOutputYet = true;
                }

                // Put the level..
                if (config.IsToShowLevel)
                {
                    if (hasOutputYet && beSpreadsheetCompatible)
                    {
                        sbPart.Append( "\t" );
                    }
                    if (showLevel)
                    {
                        string sLevel;
                        if (logRecord.Level == LogLevel.Infomation)
                        {
                            sLevel = "Info";
                        }
                        else
                        {
                            sLevel = logRecord.Level.ToString();
                        }
                        sbPart.Append( sLevel );
                    }
                    hasOutputYet = true;
                }

                // Put the category..
                if (doShowCat)
                {
                    sbPart.Append( LogManager.DelimiterBeforeCategory );
                    sbPart.Append( logRecord.Category.Name );
                    sbPart.Append( LogManager.DelimiterAfterCategory );
                }
                hasOutputYet = true;

                // Put the visual-designer indicator..
                if (isInDesignView)
                {
                    if (sbPart.Length > 0)
                    {
                        sbPart.Append( LogManager.DelimiterBeforeDesignViewIndicator );
                    }
                    sbPart.Append( LogManager.VisualDesignerIndicator );
                }

                // Determine whether to make it fixed-width.
                if (config.IsFileOutputPrefixOfFixedWidth)
                {
                    string part = sbPart.ToString();
                    if (part.Length < config._prefixWidth)
                    {
                        // The part-between-brackets is too small. Add the right bracket, and then pad with spaces
                        // to the right to bring it up to the required width.
                        if (!beSpreadsheetCompatible)
                        {
                            sbPart.Append( "]" );
                        }
                        string partTerminated = sbPart.ToString();
                        string partPadded = partTerminated.PadRight( config._prefixWidth );
                        sb.Append( partPadded );
                    }
                    else
                    {
                        if (part.Length > config._prefixWidth)
                        {
                            // Add 1 to account for the right-bracket character.
                            config._prefixWidth = part.Length + 1;
                        }
                        sb.Append( part );
                        if (!beSpreadsheetCompatible)
                        {
                            sb.Append( "]" );
                        }
                    }
                }
                else // does not have to be of fixed width.
                {
                    if (beSpreadsheetCompatible)
                    {
                        sb.Append( "\t" );
                    }
                    if (!beSpreadsheetCompatible)
                    {
                        sbPart.Append( "]" );
                    }
                    sb.Append( sbPart );
                }
            }
            else
            {
                // CBL There was nothing within brackets, and thus no brackets, so append a space after the timestamp if that was shown.
                //if (doShowTimestamp || config.ContentConfig.IsToShowElapsedTime)
                //{
                //    sb.Append(" ");
                //}
            }

            return sb.ToString();
        }
        #endregion GetPrefix

        #region GetTimeStamp
        /// <summary>
        /// Helper-method - returns the time-stamp as a string, as to be inserted into the log record prefix
        /// for output to a file or whatever.
        /// </summary>
        /// <param name="forWhen">the DateTime object to render as a string</param>
        /// <param name="config">a <c>LogConfig</c> that dictates how to present it (may be null)</param>
        /// <param name="isSpreadsheetCompatible">this dictates whether to format the output to be appropriate for a spreadsheet. The corresponding property on config is ignored (optional - default is false)</param>
        /// <returns>a string representing the given DateTime in ISO 8601 format</returns>
        /// <remarks>
        /// You would commonly set makeFixedWidth true when you want to make the columns "line up" in a text file.
        /// This is set true when you elect to make the output spreadsheet-compatible.
        /// 
        /// I put the date into the standard ISO 8601 format, because.. Obama.
        /// It makes sense to use a standard layout everywhere when reasonabley possible,
        /// plus it's sortable on the year/month/day.
        /// </remarks>
        public string GetTimeStamp( DateTime forWhen, LogConfig config, bool isSpreadsheetCompatible )
        {
            // Previous version of this method had: DateTime, bool, bool, bool
            // which was when, showDate, showFractionsOfASecond, isFixedWidth

            // I am putting this in ISO 8601 format, which is the nearest thing we have to an international standard.
            // See http://www.iso.org/iso/date_and_time_format

            // This is to allow config to be null.
            bool isToShowDate = true;
            bool isToShowFractionsOfASecond = false;
            bool isFileOutputPrefixFixedWidth = false;
            if (config != null)
            {
                isToShowDate = config.IsToShowDateInTimestamp;
                isToShowFractionsOfASecond = config.IsToShowFractionsOfASecond;
                isFileOutputPrefixFixedWidth = config.IsFileOutputPrefixOfFixedWidth;
            }

            var sb = new StringBuilder();
            if (isToShowDate)
            {
                if (isFileOutputPrefixFixedWidth)
                {
                    sb.AppendFormat( "{0:yyyy-MM-dd HH:mm:ss}", forWhen );
                }
                else
                {
                    sb.AppendFormat( "{0:yyyy-M-d H:mm:ss}", forWhen );
                }
            }
            else // no date part
            {
                sb.AppendFormat( "{0:H:mm:ss}", forWhen );
            }

            if (isToShowFractionsOfASecond)
            {
                if (isSpreadsheetCompatible)
                {
                    sb.Append( "\t" ).AppendFormat( "{0:D3}", forWhen.Millisecond );
                }
                else
                {
                    if (forWhen.Millisecond != 0)
                    {
                        sb.Append( "." ).AppendFormat( "{0:D3}", forWhen.Millisecond );
                    }
                }
            }
            return sb.ToString();
        }
        #endregion

        #region internal implementation

        private DefaultLogRecordFormatter()
        {
        }

        private static ILogRecordFormatter _instance;

        #endregion internal implementation
    }
}
