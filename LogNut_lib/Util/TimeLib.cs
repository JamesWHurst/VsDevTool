using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32;
#if NETFX_CORE
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
#endif


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// This type denotes a simplistic, course frequency, such as for how often to do something.
    /// Values range from EveryMinute to Hourly.
    /// </summary>
    public enum TimeIntervalRate
    {
        /// <summary>
        /// Happen at the rate of once per hour
        /// </summary>
        Hourly = 0,
        /// <summary>
        /// Happen once every 30 minutes (twice an hour)
        /// </summary>
        Every30Minutes,
        /// <summary>
        /// Happen once every 15 minutes (4 times per hour)
        /// </summary>
        Every15Minutes,
        /// <summary>
        /// Happen once every 10 minutes
        /// </summary>
        Every10Minutes,
        /// <summary>
        /// Happen once every 5 minutes
        /// </summary>
        Every5Minutes,
        /// <summary>
        /// Happen every minute
        /// </summary>
        EveryMinute
    };

    /// <summary>
    /// This enum indicates the order in which the date components are presented: YearMonthDAy, DayMonthYear, or MonthDayYear.
    /// </summary>
    public enum DateOrder
    {
        /// <summary>
        /// The default - this is the standard YYYY-MM-DD order of expressing a date.
        /// </summary>
        YearMonthDay,
        /// <summary>
        /// The date is expressed as DD-MM-YYYY
        /// </summary>
        DayMonthYear,
        /// <summary>
        /// The date is expressed as MM-DD-YYYY
        /// </summary>
        MonthDayYear
    }

    /// <summary>
    /// This provides time-related things
    /// </summary>
    public static class TimeLib
    {
        #region GetHintForDateFormat
        /// <summary>
        /// Given a DateOrder, return a string that supplies a hint in how to enter it
        /// - ie "yyyy/mm/dd", "dd/mm/yyyy", or "mm/dd/yyyy".
        /// </summary>
        /// <param name="dateOrder">We are only being concerned here with the order of the year, month, and day. This enum-type specifes which.</param>
        /// <param name="isChinese">Set this to true if you want the result expressed in Traditional Chinese.</param>
        /// <returns></returns>
        public static string GetHintForDateFormat( DateOrder dateOrder, bool isChinese )
        {
            string result;
            switch (dateOrder)
            {
                case DateOrder.DayMonthYear:
                    if (isChinese)
                    {
                        result = "日/月/年";
                    }
                    else
                    {
                        result = "dd/mm/yyyy";
                    }
                    break;
                case DateOrder.MonthDayYear:
                    if (isChinese)
                    {
                        result = "月/日/年";
                    }
                    else
                    {
                        result = "mm/dd/yyyy";
                    }
                    break;
                default:
                    if (isChinese)
                    {
                        result = "年/月/日";
                    }
                    else
                    {
                        result = "yyyy/mm/dd";
                    }
                    break;
            }
            return result;
        }
        #endregion

        /// <summary>
        /// Give a restricted format string, return the corresponding DateOrder.
        /// The format-hint must be of the form "dd/mm/yyyy" (for example) or may be in Chinese characters: "日/月/年".
        /// </summary>
        /// <param name="formatHint">a string of the form "dd/mm/yyyy" (for example) or may be in Chinese characters: "日/月/年"</param>
        /// <returns>a DateOrder that denotes what order the date components are in based upon the given format-hint</returns>
        public static DateOrder GetDateOrderFromFormatHint( string formatHint )
        {
            string hint = formatHint.ToLower();
            DateOrder result;
            switch (hint)
            {
                case @"dd/mm/yyyy":
                case "日/月/年":
                    result = DateOrder.DayMonthYear;
                    break;
                case @"mm/dd/yyyy":
                case "月/日/年":
                    result = DateOrder.MonthDayYear;
                    break;
                case @"yyyy/mm/dd":
                case "年/月/日":
                    result = DateOrder.YearMonthDay;
                    break;
                default:
                    throw new InvalidOperationException( message: "Invalid formatHint: " + StringLib.AsQuotedString( formatHint ) );
            }
            return result;
        }

        /// <summary>
        /// Get a list of strings that denote the 3 possible date-part orders: "yyyy/mm/dd", "dd/mm/yyyy", and "mm/dd/yyyy".
        /// </summary>
        public static IList<string> PossibleDateFormats
        {
            get
            {
                if (_possibleDateFormats == null)
                {
                    _possibleDateFormats = new List<string>( 3 );
                    _possibleDateFormats.Add( "yyyy/mm/dd" );
                    _possibleDateFormats.Add( "dd/mm/yyyy" );
                    _possibleDateFormats.Add( "mm/dd/yyyy" );
                }
                return _possibleDateFormats;
            }
        }
        private static IList<string> _possibleDateFormats;

        #region AddWeekdays
        /// <summary>
        /// Given a DateTime and a number of days, return a DateTime that represents that number of days past the given DateTime.
        /// </summary>
        /// <param name="date">the given DateTime</param>
        /// <param name="days">the number of days to add to the given DateTime</param>
        /// <returns>a new DateTime with the days added</returns>
        public static DateTime AddWeekdays( this DateTime date, int days )
        {
            // from http://dotnetslackers.com/articles/aspnet/5-Helpful-DateTime-Extension-Methods.aspx
            var sign = days < 0 ? -1 : 1;
            var unsignedDays = Math.Abs( days );
            var weekdaysAdded = 0;
            while (weekdaysAdded < unsignedDays)
            {
                date = date.AddDays( sign );
                if (date.DayOfWeek != DayOfWeek.Saturday && date.DayOfWeek != DayOfWeek.Sunday)
                    weekdaysAdded++;
            }
            return date;
        }
        #endregion

        #region As24HourDateTimeString
        /// <summary>
        /// Return the given <see cref="DateTime"/> formatted as a string in the form "2016-03-19_1605".
        /// </summary>
        /// <param name="t">the DateTime to render as a string</param>
        /// <returns>a string in the form "2016-03-19_1605" (for example)</returns>
        public static string As24HourDateTimeString( this DateTime t )
        {
            string text1 = String.Format( "{0:yyyy-MM-dd HH:mm}", t );
            // Replace the space with an underscore, and remove any colons.
            string result = text1.Replace( " ", "_" ).Replace( ":", "" );
            return result;
        }
        #endregion

        #region AsDateString
        /// <summary>
        /// Return the given DateTime formatted as a string in the ISO 8601 extended format (YYYY-MM-DD).
        /// </summary>
        /// <param name="t">the DateTime object to express as a string</param>
        public static string AsDateString( this DateTime t )
        {
            // See the ISO 8601 standard: https://en.wikipedia.org/wiki/ISO_8601
            return String.Format( "{0:yyyy/MM/dd}", t );
        }
        #endregion

        #region AsDateTimeString
        /// <summary>
        /// Return the given <see cref="DateTime"/> formatted as a string using format {0:yyyy-M-dd h:mmtt}
        /// </summary>
        /// <param name="t">the DateTime to render as a string</param>
        /// <returns>a string in the form "7:42PM" (for example)</returns>
        public static string AsDateTimeString( this DateTime t )
        {
            return String.Format( _sDateTimeFormat, t );
        }
        #endregion

        #region AsDateTimeWithoutYearString
        /// <summary>
        /// Return the given DateTime formatted as a string using format {0:M-dd h:mmtt}
        /// </summary>
        public static string AsDateTimeWithoutYearString( this DateTime t )
        {
            return String.Format( _sDateTimeFormatNoYear, t );
        }
        #endregion

        #region AsStandardDateTimeString
        /// <summary>
        /// Return the given DateTime formatted as a string in the from YYYY-MM-DD HH:MM:SSZ.
        /// </summary>
        public static string AsStandardDateTimeString( this DateTime t )
        {
            return String.Format( "{0:u}", t );
        }
        #endregion

        #region AsString (TimeSpan)
        /// <summary>
        /// Return a string representing the given TimeSpan,
        /// in the form N days HH:MM:SS .
        /// </summary>
        /// <param name="howLong">the TimeSpan to convert to text</param>
        /// <param name="excludeSecondsUnlessLessThanMinute">if true, then seconds is only included if the TimeSpan is less than a minute</param>
        /// <param name="addSmallSpace">set this to true to add Unicode-2006 character as delimiters around the colons, the "Six-per-EM" space (optional - default is false)</param>
        /// <returns>a textual representation of the given TimeSpan</returns>
        /// <remarks>
        /// This does NOT display milliseconds, unless it is less than 1 second.
        /// If there are no hours, minutes, seconds - but there are days, then only the days are included.
        /// 
        /// The default value for instances of the <c>TimeSpan</c> class is <c>TimeSpan.Zero</c>
        /// which is the same as <c>TimeSpan.MinValue</c>.
        /// If <paramref name="howLong"/> has that value, then the return value is set to "0".
        /// </remarks>
        public static string AsString( this TimeSpan howLong, bool excludeSecondsUnlessLessThanMinute, bool addSmallSpace )
        {
            if (howLong == TimeSpan.Zero)
            {
                return "0";
            }
            var sb = new StringBuilder();
            if (howLong.Days == 0 && howLong.Hours == 0 && howLong.Minutes == 0)
            {
                if (howLong.Seconds == 1 && howLong.Milliseconds > 0)
                {
                    double seconds = 1.0 + ((double)howLong.Milliseconds / 1000.0);
                    sb.AppendFormat( "{0} seconds", seconds );
                }
                else if (howLong.Seconds == 0 && howLong.Milliseconds != 0)
                {
                    sb.AppendFormat( "{0} ms", howLong.Milliseconds );
                }
                else if (howLong.Seconds == 1)
                {
                    sb.Append( "1 second" );
                }
                else
                {
                    sb.AppendFormat( "{0} seconds", howLong.Seconds );
                }
            }
            else
            {
                if (howLong.Days > 0)
                {
#if !PRE_4
                    sb.Append( howLong.ToString( "%d" ) );
#else
                    sb.Append(howLong);  //CBL
#endif
                    if (howLong.Days == 1)
                    {
                        sb.Append( " day" );
                    }
                    else
                    {
                        sb.Append( " days" );
                    }
                }
                if (!(howLong.Days > 0 && howLong.Hours == 0 && howLong.Minutes == 0 && (excludeSecondsUnlessLessThanMinute || howLong.Seconds == 0)))
                {
                    if (howLong.Days == 0 || (howLong.Hours != 0 || howLong.Minutes != 0 || howLong.Seconds != 0))
                    {
                        if (howLong.Days > 0)
                        {
                            sb.Append( " " );
                        }
                        sb.Append( howLong.Hours );
                        if (addSmallSpace)
                        {
                            sb.Append( StringLib.SmallSpace );
                        }
                        sb.Append( ":" );
                        if (addSmallSpace)
                        {
                            sb.Append( StringLib.SmallSpace );
                        }
                        sb.Append( String.Format( "{0:mm}", howLong ) );
                        if (addSmallSpace)
                        {
                            sb.Append( StringLib.SmallSpace );
                        }
                        if (!excludeSecondsUnlessLessThanMinute)
                        {
                            sb.Append( ":" );
                            if (addSmallSpace)
                            {
                                sb.Append( StringLib.SmallSpace );
                            }
                            sb.Append( String.Format( "{0:ss}", howLong ) );
                        }
                    }
                }
            }
            return sb.ToString();
        }
        #endregion

        #region AsString  extension method for TimeZoneInfo
        /// <summary>
        /// Given a TimeZoneInfo, return it's ToString representation unless it's a timezone for which we have
        /// our own string representation defined, in which case return that instead.
        /// </summary>
        /// <param name="timeZoneInfo">The TimeZoneInfo to return a string identification of</param>
        /// <returns>Either our simplified standard string identifier, or it's ToString result</returns>
        public static string AsString( this TimeZoneInfo timeZoneInfo )
        {
            if (timeZoneInfo.Id.Equals( "Eastern Standard Time" ))
            {
                return _sEST;
            }
            else if (timeZoneInfo.Id.Equals( "Central Standard Time" ))
            {
                return _sCST;
            }
            else if (timeZoneInfo.Id.Equals( "Mountain Standard Time" ))
            {
                return _sMST;
            }
            else if (timeZoneInfo.Id.Equals( "Pacific Standard Time" ))
            {
                return _sPST;
            }
            return timeZoneInfo.ToString();
        }
        #endregion

        #region AsTimeInterval
        /// <summary>
        /// Return a string respresenting (in English) the time interval bounded by t1 and t2.
        /// </summary>
        /// <param name="earliestTime">The earlier of the two DateTimes</param>
        /// <param name="latestTime">The latter of the two DateTimes</param>
        public static string AsTimeInterval( this DateTime? earliestTime, DateTime? latestTime )
        {
            string sResult;
            if (earliestTime == null && latestTime == null)
            {
                sResult = "? .. ?";
            }
            else if (earliestTime == null)
            {
                sResult = "? .. " + latestTime.Value.ToStringMinimum();
            }
            else if (latestTime == null)
            {
                sResult = earliestTime.Value.ToStringWithoutYearIfSame() + " .. ?";
            }
            else // neither is null.
            {
                DateTime t1 = earliestTime.Value;
                DateTime t2 = latestTime.Value;
                if (t1.IsSameDayAs( t2 ))
                {
                    if (t1.Hour == t2.Hour && t1.Minute == t2.Minute)
                    {
                        // Same minute. Are the seconds the same?
                        if (t1.Second == t2.Second)
                        {
                            // They are the same right down to the second, so only show one time.
                            sResult = earliestTime.Value.ToStringMinimum();
                        }
                        else // second is not the same.
                        {
                            // Show the 2nd time without the date portion.
                            // Only the seconds component distinquishes them, so show the seconds.
                            sResult = earliestTime.Value.ToStringMinimum( true ) + " .. " + String.Format( "{0:h:mm:sstt}", latestTime );
                        }
                    }
                    else // not the same minute.
                    {
                        // Show the 2nd time without the date portion.
                        sResult = earliestTime.Value.ToStringMinimum() + " .. " + String.Format( "{0:h:mmtt}", latestTime );
                    }
                }
                else // not the same day.
                {
                    if (t1.Year == t2.Year)
                    {
                        // Show the 2nd time without the year portion.
                        sResult = String.Format( _sDateTimeFormat, earliestTime ) + " .. " + String.Format( "{0:M-dd h:mmtt}", latestTime );
                    }
                    else
                    {
                        // Show the complete long form.
                        sResult = String.Format( _sDateTimeFormat, earliestTime ) + " .. " + String.Format( _sDateTimeFormat, latestTime );
                    }
                }
            }
            return sResult;
        }
        #endregion AsTimeInterval

        #region AsTimeStringLocal
        /// <summary>
        /// Return the given <see cref="DateTime"/> formatted as a string expressing it in local-time,
        /// in the form (for example): "7:42:01PM"
        /// </summary>
        /// <param name="t">the DateTime to render as a string</param>
        /// <returns>a string in the form "7:42:01PM" (for example)</returns>
        public static string AsTimeStringLocal( this DateTime t )
        {
            return String.Format( "{0:h:mmT}", t );
        }
        #endregion

        #region DateTimeFullFormat
        /// <summary>
        /// Get the String.Format formatting-string for our date-time display (excluding seconds).
        /// </summary>
        public static string DateTimeFullFormat
        {
            get { return _sDateTimeFormat; }
        }
        #endregion

        #region FirstDayOfMonth, LastDayOfMonth
        /// <summary>
        /// Given a DateTime, return a new DateTime that denotes the first day of that month.
        /// </summary>
        /// <param name="date">the DateTime to get the first-day-of-month of</param>
        /// <returns>a new DateTime that is the first day of the month</returns>
        public static DateTime FirstDayOfMonth( this DateTime date )
        {
            // from http://dotnetslackers.com/articles/aspnet/5-Helpful-DateTime-Extension-Methods.aspx
            return new DateTime( date.Year, date.Month, 1 );
        }

        /// <summary>
        /// Given a DateTime, return a new DateTime that denotes the last day of that month.
        /// </summary>
        /// <param name="date">the DateTime to get the last-day-of-month of</param>
        /// <returns>a new DateTime that is the last day of the month</returns>
        public static DateTime LastDayOfMonth( this DateTime date )
        {
            return new DateTime( date.Year, date.Month, DateTime.DaysInMonth( date.Year, date.Month ) );
        }
        #endregion

        #region GetAvailableTimeZoneStrings
        /// <summary>
        /// Return a collection of strings representing the available local time zones to choose from.
        /// </summary>
        /// <returns>A ReadOnlyCollection of strings denoting time zones</returns>
        public static ReadOnlyCollection<string> GetAvailableTimeZoneStrings()
        {
            // This is provided just for VS2008 since that does not support default parameter values.
            return GetAvailableTimeZoneStrings( false );
        }

        /// <summary>
        /// Return a collection of strings representing the available time zones to choose from.
        /// </summary>
        /// <param name="justTheLocalOnes">Indicates whether to return only a simplified list of local zones</param>
        /// <returns>A ReadOnlyCollection of strings denoting time zones</returns>
        public static ReadOnlyCollection<string> GetAvailableTimeZoneStrings( bool justTheLocalOnes )
        {
            ReadOnlyCollection<string> availableTimeZoneStrings;
            if (justTheLocalOnes)
            {
                var list = new List<string>();
                list.Add( _sEST );
                list.Add( _sCST );
                list.Add( _sMST );
                list.Add( _sPST );
                availableTimeZoneStrings = list.AsReadOnly();
            }
            else
            {
                ReadOnlyCollection<TimeZoneInfo> timeZoneInfos = TimeZoneInfo.GetSystemTimeZones();
                List<string> list = (from tz in timeZoneInfos select tz.Id).ToList();
                availableTimeZoneStrings = list.AsReadOnly();
            }
            return availableTimeZoneStrings;
        }
        #endregion

        #region GetNetworkTime

#if NETFX_CORE
        public static async void ConnectToTimeServer()
        {
            var socket = new DatagramSocket();
            socket.MessageReceived += OnSocketMessageReceived;
            await socket.ConnectAsync( new HostName( "time.windows.com" ), "123" );

            using (var dataWriter = new DataWriter( socket.OutputStream ))
            {
                var ntpData = new byte[48];
                ntpData[0] = 0x1B;
                dataWriter.WriteBytes( ntpData );
                await dataWriter.StoreAsync();
            }
        }

        private static void OnSocketMessageReceived( DatagramSocket sender, DatagramSocketMessageReceivedEventArgs args )
        {
            try
            {
                using (var reader = args.GetDataReader())
                {
                    byte[] response = new byte[48];
                    reader.ReadBytes( response );

                }
            }
            catch (Exception )
            {

            }
        }
#endif

#if !NETFX_CORE
        /// <summary>
        /// Given a NTP (Network Time Protocol)-server address, connect a socket to it and fetch the network time from it.
        /// </summary>
        /// <param name="sServer">the address of the NTP server</param>
        /// <returns>a DateTime that is the time returned from the NTP-server</returns>
        public static DateTime GetNetworkTime( this string sServer )
        {
            //default Windows time server
            string ntpServer = sServer;

            // NTP message size - 16 bytes of the digest (RFC 2030)
            var ntpData = new byte[48];

            //Setting the Leap Indicator, Version Number and Mode values
            ntpData[0] = 0x1B; //LI = 0 (no warning), VN = 3 (IPv4 only), Mode = 3 (Client Mode)

            var addresses = Dns.GetHostEntry( ntpServer ).AddressList;

            //The UDP port number assigned to NTP is 123
            var ipEndPoint = new IPEndPoint( addresses[0], 123 );
            //NTP uses UDP
            var socket = new Socket( AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp );

            socket.Connect( ipEndPoint );

            // Stops code hang if NTP is blocked
            socket.ReceiveTimeout = 3000;

            socket.Send( ntpData );
            socket.Receive( ntpData );
            socket.Close();

            //Offset to get to the "Transmit Timestamp" field (time at which the reply 
            //departed the server for the client, in 64-bit timestamp format."
            const byte serverReplyTime = 40;

            //Get the seconds part
            ulong intPart = BitConverter.ToUInt32( ntpData, serverReplyTime );

            //Get the seconds fraction
            ulong fractPart = BitConverter.ToUInt32( ntpData, serverReplyTime + 4 );

            //Convert From big-endian to little-endian
            intPart = MathLib.SwapEndianness( intPart );
            fractPart = MathLib.SwapEndianness( fractPart );

            var milliseconds = (intPart * 1000) + ((fractPart * 1000) / 0x100000000L);

            //**UTC** time
            DateTime networkDateTime = (new DateTime( 1900, 1, 1 )).AddMilliseconds( (long)milliseconds );

            return networkDateTime;
        }
#endif
        #endregion

        #region IsMatchForThisHour
        /// <summary>
        /// Given a list of words, return true if they refer to a time-interval equiv to "the past hour".
        /// </summary>
        /// <param name="listExpression">an IList of 0, 1, 2, or 3 words</param>
        /// <returns>true if the words represents today</returns>
        public static bool IsMatchForThisHour( IList<string> listExpression )
        {
            // Note: This is far more succinctly implemented in F# thus:
            //let IsThisHour(x : List<string>) : bool =
            //    match x with
            //        | "this"::e2::_ when IsHour e2 -> true
            //        | "this"::"past"::e3::_ when IsHour e3 -> true
            //        | "the"::"past"::e3::_ when IsHour e3 -> true
            //        | _ -> false
            //
            // However I'm trying to minimize the dependencies at this moment.
            bool isMatch = false;
            if (listExpression != null)
            {
                int n = listExpression.Count;
                if (n >= 2)
                {
                    string word1 = listExpression[0].ToLower();
                    string word2 = listExpression[1].ToLower();
                    if (n == 2)
                    {
                        if (word1.Equals( "this" ) && (word2.Equals( "hour" ) || word2.Equals( "hr" )))
                        {
                            isMatch = true;
                        }
                    }
                    else if (n == 3)
                    {
                        string word3 = listExpression[2].ToLower();
                        if ((word1.Equals( "this" ) || word1.Equals( "the" )) && word2.Equals( "past" ) && (word3.Equals( "hour" ) || word3.Equals( "hr" )))
                        {
                            isMatch = true;
                        }
                    }
                }
            }
            return isMatch;
        }
        #endregion IsMatchForToday

        #region IsMatchForToday
        /// <summary>
        /// Return true if the given list of words can be loosely interpreted as meaning today (in English).
        /// </summary>
        /// <param name="listExpression">an IList of 0, 1, 2, or 3 words</param>
        /// <returns>true if the words represents today</returns>
        public static bool IsMatchForToday( IList<string> listExpression )
        {
            // Note: This is far more succinctly implemented in F# thus:
            //let IsThisToday(x : List<string>) : bool =
            //    match x with
            //    | ["today"] | ["this"; "day"] -> true
            //    | ["the"; "past"; "day"] -> true
            //    | ["since"; "yesterday"] -> true
            //    | _ -> false  
            //
            // However I'm trying to minimize the dependencies at this moment.
            bool isMatch = false;
            if (listExpression != null)
            {
                int n = listExpression.Count;
                if (n >= 1)
                {
                    string word1 = listExpression[0].ToLower();
                    if (n == 1)
                    {
                        if (word1.Equals( "today" ))
                        {
                            isMatch = true;
                        }
                    }
                    else
                    {
                        string word2 = listExpression[1].ToLower();
                        if (n == 2)
                        {
                            if (word1.Equals( "this" ) && word2.Equals( "day" ))
                            {
                                isMatch = true;
                            }
                            else if (word1.Equals( "since" ) && word2.Equals( "yesterday" ))
                            {
                                isMatch = true;
                            }
                        }
                        else if (n == 3)
                        {
                            string word3 = listExpression[2].ToLower();
                            if ((word1.Equals( "this" ) || word1.Equals( "the" )) && word2.Equals( "past" ) && word3.Equals( "day" ))
                            {
                                isMatch = true;
                            }
                        }
                    }
                }
            }
            return isMatch;
        }
        #endregion IsMatchForToday

        #region IsSameDayAs
        /// <summary>
        /// Return whether this DateTime corresponds to the same day as the given DateTime.
        /// </summary>
        public static bool IsSameDayAs( this DateTime thisTime, DateTime otherTime )
        {
            return thisTime.Year == otherTime.Year && thisTime.DayOfYear == otherTime.DayOfYear;
        }
        #endregion

        #region IsToday
        /// <summary>
        /// Return whether this DateTime corresponds to today (according to DateTime.NOw)
        /// </summary>
        public static bool IsToday( this DateTime when )
        {
            return when.IsSameDayAs( DateTime.Now );
        }
        #endregion

        #region IsYesterday
        /// <summary>
        /// Return true if this DateTime corresponds to yesterday.
        /// </summary>
        public static bool IsYesterday( this DateTime when )
        {
            var dayAfter = when.AddDays( 1 );
            return dayAfter.IsSameDayAs( DateTime.Now );
        }
        #endregion

        #region SecondsForInterval
        /// <summary>
        /// Return the number of seconds that corresponds to the given TimeIntervalRate,
        /// e.g. for EveryMinute return 60.
        /// </summary>
        public static int SecondsForInterval( TimeIntervalRate eEveryNMinutes )
        {
            int iSeconds = 60;
            switch (eEveryNMinutes)
            {
                case TimeIntervalRate.EveryMinute:
                    iSeconds = 60;
                    break;
                case TimeIntervalRate.Every5Minutes:
                    iSeconds = 300;
                    break;
                case TimeIntervalRate.Every10Minutes:
                    iSeconds = 600;
                    break;
                case TimeIntervalRate.Every15Minutes:
                    iSeconds = 15 * 60;
                    break;
                case TimeIntervalRate.Every30Minutes:
                    iSeconds = 30 * 60;
                    break;
                case TimeIntervalRate.Hourly:
                    iSeconds = 60 * 60;
                    break;
            }
            return iSeconds;
        }
        #endregion

        #region SecondsToIntervalBoundary
        /// <summary>
        /// Given a TimeIntervalRate, return the number of seconds until the next actual occurance
        /// of that boundary. Ie, if the rate is hourly, and it's presently 3:59pm, return 60.
        /// </summary>
        /// <param name="eEveryNMinutes">an enum that expresses a rate</param>
        /// <returns>The number of seconds before the next interval boundary</returns>
        public static int SecondsToIntervalBoundary( TimeIntervalRate eEveryNMinutes )
        {
            int iEveryNMinutes = 60;
            switch (eEveryNMinutes)
            {
                case TimeIntervalRate.EveryMinute:
                    iEveryNMinutes = 1;
                    break;
                case TimeIntervalRate.Every5Minutes:
                    iEveryNMinutes = 5;
                    break;
                case TimeIntervalRate.Every10Minutes:
                    iEveryNMinutes = 10;
                    break;
                case TimeIntervalRate.Every15Minutes:
                    iEveryNMinutes = 15;
                    break;
                case TimeIntervalRate.Every30Minutes:
                    iEveryNMinutes = 30;
                    break;
                case TimeIntervalRate.Hourly:
                    iEveryNMinutes = 60;
                    break;
            }
            return SecondsToIntervalBoundary( iEveryNMinutes );
        }

        /// <summary>
        /// Given a value representing how many minutes per interval,
        /// return the number of seconds until the next actual occurance
        /// of that interval boundary.
        /// </summary>
        /// <param name="iEveryNMinutes"></param>
        /// <returns></returns>
        public static int SecondsToIntervalBoundary( int iEveryNMinutes )
        {
            int iSecondsToGo = 0;
            DateTime tNow = DateTime.Now;
            int m = tNow.Minute % iEveryNMinutes;
            int iMinutesLater = iEveryNMinutes - m;
            int iSecondsLater = 0;
            if (tNow.Second > 1)
            {
                if (iMinutesLater == 0)
                {
                    iMinutesLater = iEveryNMinutes - 1;
                }
                else
                {
                    iMinutesLater--;
                }
                iSecondsLater = 60 - tNow.Second;
            }
            iSecondsToGo = iMinutesLater * 60 + iSecondsLater;
            return iSecondsToGo;
        }
        #endregion

        #region SetTime
        /// <summary>
        /// Return a DateTime that is the same as the given DateTime
        /// but with the time-portion set to the hour, and zero for minutes and seconds.
        /// </summary>
        /// <param name="date">the DateTime to create the result from</param>
        /// <param name="hour">the Hour to set within the result</param>
        /// <returns>a copy of the given DateTime but with the hour set</returns>
        public static DateTime SetTime( this DateTime date, int hour )
        {
            // from http://dotnetslackers.com/articles/aspnet/5-Helpful-DateTime-Extension-Methods.aspx
            return date.SetTime( hour, 0, 0, 0 );
        }

        /// <summary>
        /// Return a DateTime that is the same as the given DateTime
        /// but with the time-portion set to the given hour and minute.
        /// </summary>
        /// <param name="date">the DateTime to create the result from</param>
        /// <param name="hour">the Hour to set within the result</param>
        /// <param name="minute">the Minute to set within the result</param>
        /// <returns>a copy of the given DateTime but with the hour and minute set</returns>
        public static DateTime SetTime( this DateTime date, int hour, int minute )
        {
            return date.SetTime( hour, minute, 0, 0 );
        }

        /// <summary>
        /// Return a DateTime that is the same as the given DateTime
        /// but with the time-portion set to the given hour, minute, and second.
        /// </summary>
        /// <param name="date">the DateTime to create the result from</param>
        /// <param name="hour">the Hour to set within the result</param>
        /// <param name="minute">the Minute to set within the result</param>
        /// <param name="second">the Second to set within the result</param>
        /// <returns>a copy of the given DateTime but with the hour and minute set</returns>
        public static DateTime SetTime( this DateTime date, int hour, int minute, int second )
        {
            return date.SetTime( hour, minute, second, 0 );
        }

        /// <summary>
        /// Return a DateTime that is the same as the given DateTime
        /// but with the time-portion set to the given values.
        /// </summary>
        /// <param name="date">the DateTime to create the result from</param>
        /// <param name="hour">the Hour to set within the result</param>
        /// <param name="minute">the Minute to set within the result</param>
        /// <param name="second">the Second to set within the result</param>
        /// <param name="millisecond">the value to set the milliseconds to set within the result</param>
        /// <returns>a copy of the given DateTime but with the hour and minute set</returns>
        public static DateTime SetTime( this DateTime date, int hour, int minute, int second, int millisecond )
        {
            return new DateTime( date.Year, date.Month, date.Day, hour, minute, second, millisecond );
        }
        #endregion

        #region TimeZoneInfoFromStdString
        /// <summary>
        /// Given a string that corresponds to a TimeZoneInfo Id or ToString representation,
        /// or to one of our simplified representations, return the corresponding TimeZoneInfo.
        /// </summary>
        public static TimeZoneInfo TimeZoneInfoFromStdString( string standardString )
        {
            if (standardString.Equals( _sEST ))
            {
                return TimeZoneInfo.FindSystemTimeZoneById( "Eastern Standard Time" );
            }
            else if (standardString.Equals( _sCST ))
            {
                return TimeZoneInfo.FindSystemTimeZoneById( "Central Standard Time" );
            }
            else if (standardString.Equals( _sMST ))
            {
                return TimeZoneInfo.FindSystemTimeZoneById( "Mountain Standard Time" );
            }
            else if (standardString.Equals( _sPST ))
            {
                return TimeZoneInfo.FindSystemTimeZoneById( "Pacific Standard Time" );
            }
#if NETFX_CORE
            return TimeZoneInfo.FindSystemTimeZoneById( standardString );
#else
            return TimeZoneInfo.FromSerializedString( standardString );
#endif
        }
        #endregion

        #region ToRelativeDateString
        /// <summary>
        /// Return a string containing an English expression of when the given time occurred, relative to now.
        /// </summary>
        /// <param name="date">the <c>DateTime</c> that we want to express</param>
        /// <returns>an English-language expression of when it was</returns>
        public static string ToRelativeDateString( this DateTime date )
        {
            // from http://dotnetslackers.com/articles/aspnet/5-Helpful-DateTime-Extension-Methods.aspx
            return GetRelativeDateValue( date, DateTime.Now );
        }

        /// <summary>
        /// Return a string containing an English expression of when the given time occurred, relative to now as UTC time.
        /// </summary>
        /// <param name="date">the <c>DateTime</c> that we want to express</param>
        /// <returns>an English-language expression of when it was</returns>
        public static string ToRelativeDateStringUtc( this DateTime date )
        {
            return GetRelativeDateValue( date, DateTime.UtcNow );
        }

        /// <summary>
        /// Return a string containing an English expression of when the given time occurred, relative to another time.
        /// </summary>
        /// <param name="date">the <c>DateTime</c> that we want to express</param>
        /// <param name="comparedTo">the <c>DateTime</c> that is the reference</param>
        /// <returns>an English-language expression of when it was</returns>
        private static string GetRelativeDateValue( DateTime date, DateTime comparedTo )
        {
            TimeSpan diff = comparedTo.Subtract( date );
            if (diff.TotalDays >= 365)
                return string.Concat( "on ", date.ToString( "MMMM d, yyyy" ) );
            if (diff.TotalDays >= 7)
                return string.Concat( "on ", date.ToString( "MMMM d" ) );
            else if (diff.TotalDays > 1)
                return string.Format( "{0:N0} days ago", diff.TotalDays );
            else if (diff.TotalDays == 1)
                return "yesterday";
            else if (diff.TotalHours >= 2)
                return string.Format( "{0:N0} hours ago", diff.TotalHours );
            else if (diff.TotalMinutes >= 60)
                return "more than an hour ago";
            else if (diff.TotalMinutes >= 5)
                return string.Format( "{0:N0} minutes ago", diff.TotalMinutes );
            if (diff.TotalMinutes >= 1)
                return "a few minutes ago";
            else
                return "less than a minute ago";
        }
        #endregion

        #region nullable-DateTime ToString
        /// <summary>
        /// Return a string representation of the given nullable-DateTime.
        /// </summary>
        /// <param name="date">the nullable-DateTime to represent as a string</param>
        /// <returns>a string representation of the given nullable-DateTime</returns>
        public static string ToString( this DateTime? date )
        {
            // from http://dotnetslackers.com/articles/aspnet/5-Helpful-DateTime-Extension-Methods.aspx
            return date.ToString( null, DateTimeFormatInfo.CurrentInfo );
            //CBL  What about if date is null?
        }

        /// <summary>
        /// Return a string representation of the given nullable-DateTime.
        /// </summary>
        /// <param name="date">the nullable-DateTime to represent as a string</param>
        /// <param name="format">the format-string to use in converting the DateTime to a string</param>
        /// <returns>a string representation of the given nullable-DateTime</returns>
        public static string ToString( this DateTime? date, string format )
        {
            return date.ToString( format, DateTimeFormatInfo.CurrentInfo );
        }

        /// <summary>
        /// Return a string representation of the given nullable-DateTime.
        /// </summary>
        /// <param name="date">the nullable-DateTime to represent as a string</param>
        /// <param name="provider">the IFormatProvider to use in converting the DateTime to a string</param>
        /// <returns>a string representation of the given nullable-DateTime</returns>
        public static string ToString( this DateTime? date, IFormatProvider provider )
        {
            return date.ToString( null, provider );
        }

        /// <summary>
        /// Return a string representation of the given nullable-DateTime.
        /// </summary>
        /// <param name="date">the nullable-DateTime to represent as a string</param>
        /// <param name="format">the format-string to use in converting the DateTime to a string</param>
        /// <param name="provider">the IFormatProvider to use in converting the DateTime to a string</param>
        /// <returns>a string representation of the given nullable-DateTime, or an empty string if the given nullable-DateTime is null</returns>
        public static string ToString( this DateTime? date, string format, IFormatProvider provider )
        {
            if (date.HasValue)
                return date.Value.ToString( format, provider );
            else
                return string.Empty;
        }
        #endregion

        #region ToStringMinimum
        /// <summary>
        /// Given a DataTime, return a string representation of it in a concise, standardized format.
        /// This is provided for VS2008 since that does not support default parameter values.
        /// </summary>
        /// <param name="t">the DateTime to convert to a string</param>
        /// <returns>a concise textual representation of the DateTime</returns>
        public static string ToStringMinimum( this DateTime t )
        {
            return ToStringMinimum( t, false );
        }

        /// <summary>
        /// Return a string representation of this DateTime, in my own concise standardized way.
        /// Year is shown only if different that this year, and month/day is included only if other than today.
        /// </summary>
        /// <param name="t">the DateTime to convert to a string</param>
        /// <param name="isToShowSeconds">whether to include seconds in the time expression</param>
        /// <returns>a concise textual representation of the DateTime</returns>
        public static string ToStringMinimum( this DateTime t, bool isToShowSeconds )
        {
            string sResult;
            if (t.IsToday())
            {
                if (isToShowSeconds)
                {
                    sResult = String.Format( "{0:h:mm:sstt}", t );
                }
                else
                {
                    sResult = String.Format( "{0:h:mmtt}", t );
                }
            }
            else if (t.Year == DateTime.Now.Year)
            {
                if (isToShowSeconds)
                {
                    sResult = String.Format( "{0:M-dd h:mm:sstt}", t );
                }
                else
                {
                    sResult = String.Format( _sDateTimeFormatNoYear, t );
                }
            }
            else
            {
                if (isToShowSeconds)
                {
                    sResult = String.Format( "{0:yyyy-M-dd h:mm:sstt}", t );
                }
                else
                {
                    sResult = String.Format( _sDateTimeFormat, t );
                }
            }
            // Make the AM or PM lowercase.
            sResult = sResult.Replace( "AM", "am" );
            sResult = sResult.Replace( "PM", "pm" );
            return sResult;
        }

        /// <summary>
        /// Return a string representation of the given TimeSpan,
        /// showing Hours only if that is nonzero, Minutes, Seconds, and the Milliseconds only if Hours is zero.
        /// </summary>
        /// <param name="t">The TimeSpan to render as a string</param>
        /// <returns>a string representing the given TimeSpan t</returns>
        public static string ToStringMinimum( this TimeSpan t )
        {
            var sb = new StringBuilder();
            try
            {

                if (t.Hours != 0)
                {
                    sb.Append( t.Hours.ToString() ).Append( ":" );
                }
                if (t.Minutes != 0)
                {
                    sb.Append( String.Format( "{0:D2}", t.Minutes ) ).Append( ":" );
                }
                sb.Append( t.Seconds );
                // Only show milliseconds if the Hours is zero.
                if (t.Hours == 0)
                {
                    if (t.Milliseconds != 0)
                    {
                        sb.Append( "." ).Append( String.Format( "{0:D3}", t.Milliseconds ) );
                    }
                }
            }
            catch (Exception x)
            {
                Debug.WriteLine( "Exception in TimeLib.ToStringMinimum(TimeSpan): " + x.Message );
            }
            return sb.ToString();
        }
        #endregion

        #region ToStringWithoutYearIfSame
        /// <summary>
        /// Given a DateTime, return a string representation which, if the year is the same as the actual current year - omits the year part from the text.
        /// </summary>
        /// <param name="thisTime">the DateTime to create a textual representation of</param>
        /// <returns>a string representation of the DateTime</returns>
        public static string ToStringWithoutYearIfSame( this DateTime thisTime )
        {
            string sResult;
            if (thisTime.Year == DateTime.Now.Year)
            {
                sResult = String.Format( _sDateTimeFormatNoYear, thisTime );
                //sResult = String.Format("{0:M-dd h:mm:SS}", thisTime);
            }
            else
            {
                sResult = String.Format( _sDateTimeFormat, thisTime );
            }
            return sResult;
        }
        #endregion

        #region fields

        /// <summary>
        /// "{0:yyyy-M-dd h:mmtt}"
        /// </summary>
        private const string _sDateTimeFormat = "{0:yyyy-M-dd hh:mmtt}";
        private const string _sDateTimeFormatNoYear = "{0:M-dd h:mm:ss tt}";
        // Use these simplified identifiers for the mainland US time zones.
        private const string _sEST = "(UTC-5) Eastern Time";
        private const string _sCST = "(UTC-6) Central Time";
        private const string _sMST = "(UTC-7) Mountain Time";
        private const string _sPST = "(UTC-8) Pacific Time";

        #endregion fields
    }

    /// <summary>
    /// This class simply serves to hold some timezone-related functions.
    /// </summary>
    public static class SystemTimeZone
    {
        /// <summary>
        /// Set the current timezone. This has to elevate the caller's privileges.
        /// </summary>
        /// <param name="timeZoneDisplayName"></param>
        /// <returns></returns>
        public static bool SetTimeZone( string timeZoneDisplayName )
        {
            ElevatePriviliges();
            TimeZoneSetter s = new TimeZoneSetter();
            return s.SetTimeZone( timeZoneDisplayName );
        }

        /// <summary>
        /// Return the textual representation of the current timezone.
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentTimeZone()
        {
            // The default for EST would be: "(UTC-05:00) Eastern Time (US & Canada)"
            string result = TimeZoneInfo.GetSystemTimeZones().Where( tz => tz.StandardName == System.TimeZone.CurrentTimeZone.StandardName ).Single().DisplayName;
            return result;
        }

        /// <summary>
        /// Return an array of strings denoting all time-zones.
        /// </summary>
        /// <returns></returns>
        public static string[] GetAllTimeZones()
        {
            return TimeZoneInfo.GetSystemTimeZones().Select( tzi => tzi.DisplayName )/*Where(dn=>dn.Contains("US") || dn.Contains("Singapore")).Select(s=>s.Substring(s.IndexOf(')')+1) )*/.ToArray();
        }

        /// <summary>
        /// Set the current time-format to be either 24-hour or 12-hour.
        /// </summary>
        /// <param name="isToBe24Hour"></param>
        public static void SetTimeFormat( bool isToBe24Hour )
        {
            string timeFormat = isToBe24Hour ? "HH:mm" : "h:mm tt";
            CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.DateTimeFormat.ShortTimePattern = timeFormat;
            Thread.CurrentThread.CurrentCulture = culture;
        }

        /// <summary>
        /// Set the current date-format to the given format-string.
        /// </summary>
        /// <param name="format"></param>
        public static void SetDateFormat( string format )
        {
            CultureInfo culture = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            culture.DateTimeFormat.ShortDatePattern = format;
            Thread.CurrentThread.CurrentCulture = culture;
        }

        #region Private

        [DllImport( "kernel32.dll", CharSet = CharSet.Auto )]
        private static extern int GetStandardError();

        [DllImport( "advapi32.dll", ExactSpelling = true, SetLastError = true )]
        private static extern bool AdjustTokenPrivileges( IntPtr htok, bool disall,
        ref TokPriv1Luid newst, int len, IntPtr prev, IntPtr relen );

        [DllImport( "kernel32.dll", ExactSpelling = true )]
        private static extern IntPtr GetCurrentProcess();

        [DllImport( "advapi32.dll", ExactSpelling = true, SetLastError = true )]
        internal static extern bool OpenProcessToken( IntPtr h, int acc, ref IntPtr
        phtok );

        [DllImport( "advapi32.dll", SetLastError = true )]
        private static extern bool LookupPrivilegeValue( string host, string name,
        ref long pluid );

        [StructLayout( LayoutKind.Sequential, Pack = 1 )]
        private struct TokPriv1Luid
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        private const int SE_PRIVILEGE_ENABLED = 0x00000002;
        private const int TOKEN_QUERY = 0x00000008;
        private const int TOKEN_ADJUST_PRIVILEGES = 0x00000020;
        private const string SE_TIME_ZONE_NAMETEXT = "SeTimeZonePrivilege";

        private static void ElevatePriviliges()
        {
            bool retVal;
            TokPriv1Luid tp;
            IntPtr hproc = GetCurrentProcess();
            IntPtr htok = IntPtr.Zero;
            retVal = OpenProcessToken( hproc, TOKEN_ADJUST_PRIVILEGES | TOKEN_QUERY, ref htok );
            tp.Count = 1;
            tp.Luid = 0;
            tp.Attr = SE_PRIVILEGE_ENABLED;
            retVal = LookupPrivilegeValue( null, SE_TIME_ZONE_NAMETEXT, ref tp.Luid );
            retVal = AdjustTokenPrivileges( htok, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero );
        }
        #endregion
    }

    internal class TimeZoneSetter
    {
        [DllImport( "kernel32.dll", CharSet = CharSet.Auto )]
        private extern static bool SetTimeZoneInformation( ref TIME_ZONE_INFORMATION lpTimeZoneInformation );

        [StructLayout( LayoutKind.Sequential )]
        private struct SYSTEMTIME
        {
            public ushort wYear;
            public ushort wMonth;
            public ushort wDayOfWeek;
            public ushort wDay;
            public ushort wHour;
            public ushort wMinute;
            public ushort wSecond;
            public ushort wMilliseconds;
        }


        [StructLayout( LayoutKind.Sequential, CharSet = CharSet.Unicode )]
        private struct TIME_ZONE_INFORMATION
        {
            public int Bias;
            [MarshalAs( UnmanagedType.ByValTStr, SizeConst = 32 )]
            public string StandardName;
            public SYSTEMTIME StandardDate;
            public int StandardBias;
            [MarshalAs( UnmanagedType.ByValTStr, SizeConst = 32 )]
            public string DaylightName;
            public SYSTEMTIME DaylightDate;
            public int DaylightBias;
        }

        [StructLayout( LayoutKind.Sequential )]
        private struct REGTZI
        {
            public int Bias;
            public int StandardBias;
            public int DaylightBias;
            public SYSTEMTIME StandardDate;
            public SYSTEMTIME DaylightDate;
        }

        public bool SetTimeZone( string TimeZoneDisplayName )
        {
            TimeZoneInfo hwZone = TimeZoneInfo.GetSystemTimeZones().Where( tz => tz.DisplayName == TimeZoneDisplayName ).Single();
            string reg_key = string.Concat( @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones\", hwZone.Id );
            RegistryKey rkTZInfo = Registry.LocalMachine.OpenSubKey( reg_key );

            // Omitted (uses index to find the time zone in the registry so  that I can get the information about the time zone using the appropriate key's TZI  value

            // We've found the right time zone, get the TZI data
            object varValue = rkTZInfo.GetValue( "TZI" );
            byte[] baData = varValue as byte[];
            int iSize = baData.Length;
            IntPtr buffer = Marshal.AllocHGlobal( iSize );
            Marshal.Copy( baData, 0, buffer, iSize );
            REGTZI rtzi = (REGTZI)Marshal.PtrToStructure( buffer, typeof( REGTZI ) );
            Marshal.FreeHGlobal( buffer );

            // Now fill out TIME_ZONE_INFORMATION with that data
            TIME_ZONE_INFORMATION tZoneInfo = new TIME_ZONE_INFORMATION();
            tZoneInfo.Bias = rtzi.Bias;
            tZoneInfo.StandardBias = rtzi.StandardBias;
            tZoneInfo.DaylightBias = rtzi.DaylightBias;
            tZoneInfo.StandardDate = rtzi.StandardDate;
            tZoneInfo.DaylightDate = rtzi.DaylightDate;
            tZoneInfo.StandardName = (string)rkTZInfo.GetValue( "Std" );
            tZoneInfo.DaylightName = (string)rkTZInfo.GetValue( "Dlt" );

            return SetTimeZoneInformation( ref tZoneInfo );
        }
    }

} // end namespace.

