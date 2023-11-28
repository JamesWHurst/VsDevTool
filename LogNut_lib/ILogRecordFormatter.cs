
using System;


namespace Hurst.LogNut
{
    /// <summary>
    /// The interface ILogRecordFormatter specifies methods GetLogRecordAsText, GetPrefix and GetTimeStamp.
    /// This may be implemented to define a new formatter for your log records.
    /// </summary>
    public interface ILogRecordFormatter
    {
        /// <summary>
        /// Return a textual representation of the given LogRecord.
        /// Implement this method to format the LogRecord according to your needs.
        /// </summary>
        /// <param name="logRecord">the LogRecord to render into your chosen format</param>
        /// <param name="config">the LogConfig that specifies what to include</param>
        /// <returns>a string representation of the given LogRecord</returns>
        /// <remarks>
        /// This method calls <see cref="GetPrefix"/> to help compose it's result.
        /// </remarks>
        string GetLogRecordAsText( LogRecord logRecord, LogConfig config );

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
        /// This method calls <see cref="GetTimeStamp"/> to help compose it's result.
        /// </remarks>
        string GetPrefix( LogRecord logRecord, LogConfig config );

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
        string GetTimeStamp( DateTime forWhen, LogConfig config, bool isSpreadsheetCompatible );
    }
}
