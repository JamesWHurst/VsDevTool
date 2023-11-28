#if PRE_4
#define PRE_5
#endif
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// StringLib is our library of purely string-related utilities.
    /// </summary>
    public static class StringLib
    {
        #region Append
        /// <summary>
        /// Return the given text <paramref name="toWhat"/> with the text of <paramref name="whatToAppend"/>
        /// appended to it. This is valid even if either argument is null. If both are - an empty string is returned.
        /// </summary>
        /// <param name="toWhat">the text to append to</param>
        /// <param name="whatToAppend">the text to append to <paramref name="toWhat"/></param>
        /// <returns>a juxtaposition of both strings - never null</returns>
        public static string Append( this string toWhat, string whatToAppend )
        {
            if (toWhat == null)
            {
                if (whatToAppend == null)
                {
                    return String.Empty;
                }
                return whatToAppend;
            }
            if (whatToAppend == null)
            {
                return toWhat;
            }
            return toWhat + whatToAppend;
        }
        #endregion

        #region AsExponentialIfLargeOrSmall
        /// <summary>
        /// Formats the given numeric value as a string and returns it,
        /// formatting it as exponential if it's very small or very large.
        /// </summary>
        /// <param name="fValue">The numeric value to format as a string</param>
        /// <returns>a string representation of the given numeric value</returns>
        public static string AsExponentialIfLargeOrSmall( double fValue )
        {
            string sValueAsString;
            // If the offset is between .001 and 1,000,000, inclusive
            // then we probably don't need to show it in exponential format.
            if (fValue >= 0.001 && fValue <= 1000000)
            {
                sValueAsString = String.Format( "{0}", fValue.ToString( "F4" ) );
            }
            else
            {
                sValueAsString = String.Format( "{0}", fValue.ToString( "E" ) );
            }
            return sValueAsString;
        }
        #endregion

        #region AsHexString
        /// <summary>
        /// Return the 32-bit integer (Int32) value as hexadecimal text.
        /// </summary>
        /// <param name="integerValue">the 32-bit integer value to express in textual hexadecimal form</param>
        /// <returns>a string containing the word-value expressed in hexadecimal form</returns>
        /// <remarks>
        /// The string that is returned, has leading zeros removed such that the text consists of even
        /// numbers of digits.
        /// 
        /// For example,
        ///   AsHexString(1) yields "01"
        ///   AsHexString(0x111) yields "0111".
        /// </remarks>
        public static string AsHexString( this int integerValue )
        {
            return AsHexString( integerValue, false );
        }

        /// <summary>
        /// Return the 32-bit integer (Int32) value as hexadecimal text.
        /// </summary>
        /// <param name="integerValue">the 32-bit integer value to express in textual hexadecimal form</param>
        /// <param name="includeSuffix">if true, a single lowercase 'x' is appended to flag it as being in hexadecimal</param>
        /// <returns>a string containing the word-value expressed in hexadecimal form</returns>
        /// <remarks>
        /// The string that is returned, has leading zeros removed such that the text consists of even
        /// numbers of digits.
        /// 
        /// For example,
        ///   AsHexString(1) yields "01"
        ///   AsHexString(0x111) yields "0111".
        /// </remarks>
        public static string AsHexString( this int integerValue, bool includeSuffix )
        {
            string hexValue = String.Format( "{0:X}", integerValue );
            if (includeSuffix)
            {
                string result = hexValue.WithoutLeadingZerosInBlocksOfTwo();
                return result + "x";
            }
            else
            {
                return hexValue.WithoutLeadingZerosInBlocksOfTwo();
            }
        }

        /// <summary>
        /// Return the given byte (unsigned-8-bit) value as hexadecimal text.
        /// </summary>
        /// <param name="byteValue">the 8-bit unsigned value to express in textual hexadecimal form</param>
        /// <returns>a string containing the word-value expressed in hexadecimal form</returns>
        /// <remarks>
        /// The string that is returned, has leading zeros removed such that the text consists of even
        /// numbers of digits.
        /// 
        /// For example,
        ///   AsHexString(1) yields "01"
        ///   AsHexString(0x11) yields "11".
        /// </remarks>
        public static string AsHexString( this byte byteValue )
        {
            return AsHexString( byteValue, false );
        }

        /// <summary>
        /// Return the given byte (unsigned-8-bit) value as hexadecimal text.
        /// </summary>
        /// <param name="byteValue">the 8-bit unsigned value to express in textual hexadecimal form</param>
        /// <param name="includeSuffix">if true, a single lowercase 'x' is appended to flag it as being in hexadecimal</param>
        /// <returns>a string containing the word-value expressed in hexadecimal form</returns>
        /// <remarks>
        /// The string that is returned, has leading zeros removed such that the text consists of even
        /// numbers of digits.
        /// 
        /// For example,
        ///   AsHexString(1) yields "01"
        ///   AsHexString(0x11) yields "11".
        /// </remarks>
        public static string AsHexString( this byte byteValue, bool includeSuffix )
        {
            string hexValue = String.Format( "{0:X}", byteValue ).ToUpper();
            if (includeSuffix)
            {
                string result = hexValue.WithoutLeadingZerosInBlocksOfTwo();
                return result + "x";
            }
            else
            {
                return hexValue.WithoutLeadingZerosInBlocksOfTwo();
            }
        }

        /// <summary>
        /// Return the given ushort (unsigned-word) value as hexadecimal text.
        /// </summary>
        /// <param name="wordValue">the 16-bit unsigned value to express in textual hexadecimal form</param>
        /// <returns>a string containing the word-value expressed in hexadecimal form</returns>
        /// <remarks>
        /// The string that is returned, has leading zeros removed such that the text consists of even
        /// numbers of digits.
        /// 
        /// For example,
        ///   AsHexString(1) yields "01"
        ///   AsHexString(0x111) yields "0111".
        /// </remarks>
        public static string AsHexString( this ushort wordValue )
        {
            return AsHexString( wordValue, false );
        }

        /// <summary>
        /// Return the given ushort (unsigned-word) value as hexadecimal text.
        /// </summary>
        /// <param name="wordValue">the 16-bit unsigned value to express in textual hexadecimal form</param>
        /// <param name="includeSuffix">if true, a single lowercase 'x' is appended to flag it as being in hexadecimal</param>
        /// <returns>a string containing the word-value expressed in hexadecimal form</returns>
        /// <remarks>
        /// The string that is returned, has leading zeros removed such that the text consists of even
        /// numbers of digits.
        /// 
        /// For example,
        ///   AsHexString(1) yields "01"
        ///   AsHexString(0x111) yields "0111".
        /// </remarks>
        public static string AsHexString( this ushort wordValue, bool includeSuffix )
        {
            string hexValue = String.Format( "{0:X}", wordValue ).ToUpper();
            if (includeSuffix)
            {
                string result = hexValue.WithoutLeadingZerosInBlocksOfTwo();
                return result + "x";
            }
            else
            {
                return hexValue.WithoutLeadingZerosInBlocksOfTwo();
            }
        }

        /// <summary>
        /// Given an array of bytes, return a string that represents the values of the bytes in that array in hexadecimal form.
        /// </summary>
        /// <param name="arrayOfBytes">the array of bytes to show the values of</param>
        /// <returns>a string denoting the given byte-array</returns>
        /// <returns>
        /// This method simply calls the overload of <c>AsHexString</c> that provides the <c>includeGroupDelimiters</c>
        /// parameter, with a value of false for that argument.
        /// </returns>
        public static string AsHexString( this byte[] arrayOfBytes )
        {
            return AsHexString( arrayOfBytes, arrayOfBytes.Length );
        }

        /// <summary>
        /// Given an array of bytes, return a string that represents the values of the bytes in that array in hexadecimal form.
        /// </summary>
        /// <param name="arrayOfBytes">the array of bytes to show the values of</param>
        /// <param name="lengthToUse">this indicates how many bytes within the array to use</param>
        /// <returns>a string denoting the given byte-array</returns>
        public static string AsHexString( this byte[] arrayOfBytes, int lengthToUse )
        {
            return AsHexString( arrayOfBytes, lengthToUse, true, true, 8 );
        }

        /// <summary>
        /// Given an array of bytes, return a string that represents the values of the bytes in that array in hexadecimal form, using commas-space as the delimiter.
        /// If the given argument is null, the string "null" is returned.
        /// </summary>
        /// <param name="arrayOfBytes">the array of bytes to show the values of</param>
        /// <param name="lengthToUse">this indicates how many bytes within the array to use</param>
        /// <param name="spaceBetweenBytes">set this true to insert a space between bytes</param>
        /// <param name="withGroupDelimiters">set this true to insert a comma-delimiter every n bytes</param>
        /// <param name="groupSize">this denotes the number of bytes to consider to be one 'group' for separating with delimiters, if called-for</param>
        /// <returns>a string denoting the given byte-array</returns>
        /// <exception cref="ArgumentOutOfRangeException">the specified lengthToUse must be non-negative</exception>
        /// <remarks>
        /// If the argument <paramref name="arrayOfBytes"/> is an array of zero length, then an empty string is returned.
        /// 
        /// If the argument <paramref name="lengthToUse"/> specifies more elements than the array contains, then all elements
        /// of the array are included in the result.
        /// 
        /// If the value of the argument <paramref name="arrayOfBytes"/> is null, then this returns the string-literal "null".
        /// By allowing the argument to be null, this avoids the necessity of null-checks when calling this method.
        /// </remarks>
        public static string AsHexString( this byte[] arrayOfBytes, int lengthToUse, bool spaceBetweenBytes,
                                          bool withGroupDelimiters, int groupSize )
        {
            return AsHexString( arrayOfBytes, lengthToUse, spaceBetweenBytes, withGroupDelimiters, groupSize, ", " );
        }

        /// <summary>
        /// Given an array of bytes, return a string that represents the values of the bytes in that array in hexadecimal form.
        /// If the given argument is null, the string "null" is returned.
        /// </summary>
        /// <param name="arrayOfBytes">the array of bytes to show the values of</param>
        /// <param name="lengthToUse">this indicates how many bytes within the array to use</param>
        /// <param name="spaceBetweenBytes">set this true to insert a space between bytes</param>
        /// <param name="withGroupDelimiters">set this true to insert a comma-delimiter every n bytes</param>
        /// <param name="groupSize">this denotes the number of bytes to consider to be one 'group' for separating with delimiters, if called-for</param>
        /// <param name="valueForDelimiter">the text to use for the group-delimiter (optional - default is a comma+space)</param>
        /// <returns>a string denoting the given byte-array</returns>
        /// <exception cref="ArgumentOutOfRangeException">the specified lengthToUse must be non-negative</exception>
        /// <remarks>
        /// If the argument <paramref name="arrayOfBytes"/> is an array of zero length, then an empty string is returned.
        /// 
        /// If the argument <paramref name="lengthToUse"/> specifies more elements than the array contains, then all elements
        /// of the array are included in the result.
        /// 
        /// If the value of the argument <paramref name="arrayOfBytes"/> is null, then this returns the string-literal "null".
        /// By allowing the argument to be null, this avoids the necessity of null-checks when calling this method.
        /// </remarks>
        public static string AsHexString( this byte[] arrayOfBytes, int lengthToUse, bool spaceBetweenBytes, bool withGroupDelimiters, int groupSize, string valueForDelimiter )
        {
            if (arrayOfBytes == null)
            {
                return "null";
            }
            if (lengthToUse < 0)
            {
                throw new ArgumentOutOfRangeException( "lengthToUse", lengthToUse, "lengthToUse must not be negative." );
            }
            if (lengthToUse == 0)
            {
                return String.Empty;
            }
            StringBuilder sb = new StringBuilder();
            int lengthToInclude = Math.Min( lengthToUse, arrayOfBytes.Length );
            for (int i = 0; i < lengthToInclude; i++)
            {
                if (i > 0)
                {
                    if (withGroupDelimiters && (i % groupSize == 0))
                    {
                        sb.Append( valueForDelimiter );
                    }
                    else if (spaceBetweenBytes)
                    {
                        sb.Append( " " );
                    }
                }
                sb.AppendFormat( "{0:X2}", arrayOfBytes[i] );
            }
            return sb.ToString();
        }
        #endregion

        #region AsInnerType
        /// <summary>
        /// Return a somewhat-simplified string representation of the type of the given object.
        /// "System" is stripped, Boolean is "bool", Int32 is "int", and Nullables are represented with "?".
        /// </summary>
        /// <param name="thisObject">the Object to give the type of</param>
        /// <returns>a simplifed representation of the type of the given object</returns>
        public static string AsInnerType( Object thisObject )
        {
            Type thisType = thisObject.GetType();
            Type elementType = thisType.GetElementType();
            string typeName = elementType.Name;
            string s = typeName;
            if (s.StartsWith( "Nullable" ))
            {
                Type underlyingType = Nullable.GetUnderlyingType( elementType );
                if (underlyingType != null)
                {
                    s = AsSimplerTypeString( underlyingType ) + "?";
                }
            }
            else
            {
                s = AsSimplerTypeString( elementType );
            }
            return s;
        }
        #endregion

        #region AsSimplerTypeString
        /// <summary>
        /// Return a string representation of the given C# Type, shortened in a few ways.
        /// </summary>
        /// <param name="aType"></param>
        /// <returns></returns>
        public static string AsSimplerTypeString( Type aType )
        {
            string s = aType.ToString();
            if (s.StartsWith( "System." ))
            {
                s = s.Substring( 7 );
            }
            if (s.Equals( "Boolean" ))
            {
                s = "bool";
            }
            if (s.Equals( "Int32" ))
            {
                s = "int";
            }
            return s;
        }
        #endregion

        #region AsString ( Process )
        /// <summary>
        /// Return a string containing some detailed information about the given Process object.
        /// </summary>
        /// <param name="p">the Process object to get information about</param>
        /// <returns>information about the given Process object</returns>
        public static string AsString( this Process p )
        {
            return "Process( ProcessName: " + p.ProcessName + ", Id: " + p.Id + ", HasExited:" + p.HasExited + ", MainWindowTitle: " + StringLib.AsQuotedString( p.MainWindowTitle ) + ", StartTime: " + p.StartTime + ", StartInfo.FileName: " + AsQuotedString( p.StartInfo.FileName ) + ", StartInfo.Arguments: " + AsQuotedString( p.StartInfo.Arguments );
        }
        #endregion

        #region AsString (object)
        /// <summary>
        /// This is an extension method that simply returns the given thing converted to a string, in general using ToString
        /// but in a little more robust way.
        /// It produces a useful result even if the given object is null, an array, or a nullable type.
        /// This is used where you want to display what the object is, but don't want your code to choke on a null value.
        /// </summary>
        /// <param name="someObject">The given object to return expressed as a string</param>
        /// <returns>the given object expressed as a string</returns>
        public static string AsString( this object someObject )
        {
            return AsString( someObject, false );
        }

        /// <summary>
        /// This is an extension method that simply returns the given thing converted to a string, in general using ToString
        /// but in a little more robust way.
        /// It produces a useful result even if the given object is null, an array, or a nullable type.
        /// This is used where you want to display what the object is, but don't want your code to choke on a null value.
        /// </summary>
        /// <param name="someObject">The given object to return expressed as a string</param>
        /// <param name="isToShowByteArraysAsHex">if the given object is a byte-array, this dictates whether to express that in hexadecimal</param>
        /// <returns>the given object expressed as a string</returns>
        public static string AsString( this object someObject, bool isToShowByteArraysAsHex )
        {
            string resultString;
            if (someObject == null)
            {
                resultString = "null";
            }
            else
            {
                string stringObject = someObject as String;
                if (stringObject == null)
                {
                    Type thisType = someObject.GetType();
                    if (thisType.IsArray)
                    {
                        System.Array systemArray = someObject as System.Array;
                        int len = systemArray.Length;
                        var sb = new StringBuilder();
                        string elementTypeAsString = AsInnerType( someObject );
                        sb.Append( elementTypeAsString ).Append( "[" ).Append( len ).Append( "]" );
                        if (len > 0)
                        {
                            sb.Append( " = {" );
                            for (int i = 0; i < len; i++)
                            {
                                object o = systemArray.GetValue( i );
                                if (o == null)
                                {
                                    sb.Append( "null" );
                                }
                                else
                                {
                                    if (isToShowByteArraysAsHex)
                                    {
                                        sb.AppendFormat( "{0:X}", o );
                                    }
                                    else
                                    {
                                        sb.Append( o.ToString() );
                                    }
                                }
                                if (i < len - 1)
                                {
                                    sb.Append( ", " );
                                }
                            }
                            sb.Append( "}" );
                        }
                        resultString = sb.ToString();
                    }
                    else
                    {
                        resultString = someObject.ToString();
                    }
                }
                else if (stringObject == String.Empty)
                {
                    resultString = "Empty";
                }
                else if (HasNothing( stringObject ))
                {
                    resultString = "whitespace";
                }
                else
                {
                    resultString = stringObject.WithinDoubleQuotes();
                }
            }
            return resultString;
        }
        #endregion

        #region AsString( stringObject )
        /// <summary>
        /// Return this given String as a non-null string.
        /// If it is null - return "null".
        /// If it is an empty string - return "empty".
        /// If it has nothing but white-space - return "whitespace".
        /// Otherwise just return the string as-is.
        /// </summary>
        /// <param name="stringObject">the given String to return the text of</param>
        /// <returns>a clear indication of what the given String is</returns>
        public static string AsString( this string stringObject )
        {
            if (stringObject == null)
            {
                return "null";
            }
            if (stringObject.Length == 0)
            {
                return "empty";
            }
            if (stringObject.Trim().Length == 0)
            {
                return "whitespace";
            }
            return stringObject;
        }
        #endregion

        #region AsString (string[])
        /// <summary>
        /// Render the given array of strings, as a single human-readable string
        /// </summary>
        /// <param name="aryWhat"></param>
        /// <returns></returns>
        public static string AsString( string[] aryWhat )
        {
            StringBuilder sb = new StringBuilder();
            sb.Append( "(string[" );
            int n = aryWhat.Length;
            sb.Append( n.ToString() ).Append( "] with {" );
            bool bFirst = true;
            for (int i = 0; i < n; i++)
            {
                string s = aryWhat[i];
                sb.Append( "\"" ).Append( s ).Append( "\"" );
                if (bFirst)
                {
                    bFirst = false;
                }
                if (i < n - 1)
                {
                    sb.Append( ", " );
                }
            }
            sb.Append( "})" );
            return sb.ToString();
        }
        #endregion

        #region AsString (List<string>)
        /// <summary>
        /// Render the given List of strings, as a single human-readable string
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string AsString( List<string> list )
        {
            //CBL Do we need this in addition to AsString( List<object> ?
            var sb = new StringBuilder();
            if (list == null)
            {
                sb.Append( "null" );
            }
            else
            {
                int n = list.Count;
                bool bFirst = true;
                for (int i = 0; i < n; i++)
                {
                    string s = list[i];
                    sb.Append( @"""" ).Append( s ).Append( @"""" );
                    if (bFirst)
                    {
                        bFirst = false;
                    }
                    if (i < n - 1)
                    {
                        sb.Append( ", " );
                    }
                }
            }
            return sb.ToString();
        }
        #endregion

        #region AsString (List<object>)
        /// <summary>
        /// Render the given List of objects, as a single human-readable string
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string AsString( List<object> list )
        {
            var sb = new StringBuilder();
            if (list == null)
            {
                sb.Append( "null" );
            }
            else
            {
                int n = list.Count;
                bool bFirst = true;
                for (int i = 0; i < n; i++)
                {
                    object thisElement = list[i];
                    sb.Append( thisElement.ToString() );
                    if (bFirst)
                    {
                        bFirst = false;
                    }
                    if (i < n - 1)
                    {
                        sb.Append( ", " );
                    }
                }
            }
            return sb.ToString();
        }
        #endregion

        #region AsList
        /// <summary>
        /// Return a List of strings, derived from the given text which should contain a comma-delimited list of those items.
        /// </summary>
        /// <param name="stringExpressionOfList">a string that has the list items separated by commas</param>
        /// <returns></returns>
        public static List<string> AsList( this string stringExpressionOfList )
        {
            var arrayOfItems = stringExpressionOfList.Split( ',' );
            List<string> result = new List<string>();
            foreach (var item in arrayOfItems)
            {
                result.Add( item.Trim() );
            }
            return result;
        }
        #endregion

        #region AsQuotedString
        /// <summary>
        /// Return the given text surrounded by double-quotes, unless that text is:
        /// 1. null, whereupon return "null".
        /// 2. an empty string, whereupon return "empty-string"
        /// 3. All other cases: return the given value surrounded by double-quotes.
        /// </summary>
        /// <param name="what">the text to give quotes to</param>
        /// <returns>the given text enclosed within double-quotes</returns>
        public static string AsQuotedString( this string what )
        {
            string result;
            if (what == null)
            {
                result = "\"null\"";
            }
            else if (what.Length == 0)
            {
                result = "\"empty-string\"";
            }
            else
            {
                result = @"""" + what + @"""";
            }
            return result;
        }
        #endregion AsQuotedString

        #region AsQuoted
        /// <summary>
        /// Return the given text surrounded by double-quotes, unless that text is:
        /// 1. null, whereupon return "null".
        /// 2. an empty string, whereupon return ""
        /// 3. All other cases: return the given value surrounded by double-quotes.
        /// </summary>
        /// <param name="what">the text to give quotes to</param>
        /// <returns>the given text enclosed within double-quotes</returns>
        public static string AsQuoted( string what )
        {
            string result;
            if (what == null)
            {
                result = "\"null\"";
            }
            else if (what.Length == 0)
            {
                result = @"""""";
            }
            else
            {
                result = @"""" + what + @"""";
            }
            return result;
        }
        #endregion AsQuotedString

        #region CharacterDescription
        /// <summary>
        /// Provides a spelled-out name for the given character, if possible,
        /// otherwise expresses it as the Unicode codepoint value.
        ///  The intent is to identify it in an obvious human-readable form.
        /// </summary>
        /// <param name="thisCharacter">The character to supply a name for</param>
        /// <returns>The name of the given character if known, otherwise the UNICODE codepoint</returns>
        /// <remarks>This is intended for responding to the user concerning something he/she has typed into an English keyboard,
        /// thus the non-English characters are not accounted for here. Other alphabets yet need to be implemented.</remarks>
        public static string CharacterDescription( this Char thisCharacter )
        {
            string result = "?";
            if (Char.IsDigit( thisCharacter ))
            {
                int asciiIndex = (int)thisCharacter;
                int integerValue = asciiIndex - 48;
                if (integerValue == 0)
                {
                    result = "digit zero";
                }
                else
                {
                    result = "digit " + integerValue;
                }
            }
            else if (Char.IsLetter( thisCharacter ))
            {
                result = "letter \"" + thisCharacter + "\"";
            }
            else
            {
                // These are presented in (roughly) the order as they appear on my keyboard, top-to-bottom, left-to-right.
                switch (thisCharacter)
                {
                    case '`':
                        return "grave accent";
                    case '~':
                        return "tilde";
                    case '!':
                        return "exclamation-mark";
                    case '@':
                        return "at-sign";
                    case '#':
                        return "pound-sign";
                    case '$':
                        return "dollar-sign";
                    case '%':
                        return "percentage-sign";
                    case '^':
                        return "caret or circumflex accent";
                    case '&':
                        return "ampersand";
                    case '*':
                        return "asterisk";
                    case '(':
                        return "left parenthesis";
                    case ')':
                        return "right parenthesis";
                    case '-':
                        return "hyphen";
                    case '_':
                        return "underscore";
                    case '=':
                        return "equal-sign";
                    case '+':
                        return "plus-sign";
                    case '\t':
                        return "tab";
                    case '[':
                        return "left bracket";
                    case '{':
                        return "left brace";
                    case ']':
                        return "right bracket";
                    case '}':
                        return "right brace";
                    case '\\':
                        return "back-slash";
                    case '|':
                        return "vertical bar";
                    case ';':
                        return "semicolon";
                    case ':':
                        return "colon";
                    case '\'':
                        return "single-quote";
                    case '\"':
                        return "double-quote";
                    case '\r':
                        return "carriage-return";
                    case '\n':
                        return "newline";
                    case ',':
                        return "comma";
                    case '<':
                        return "less-than";
                    case '.':
                        return "period";
                    case '>':
                        return "greater-than";
                    case '/':
                        return "forward-slash";
                    case '?':
                        return "question-mark";
                    case ' ':
                        return "space";
                    default:
                        int n = Convert.ToInt32( thisCharacter );
                        if (n > 127)
                        {
                            result = "(UNICODE codepoint " + n.ToString() + ")";
                        }
                        else
                        {
                            if (Char.IsPunctuation( thisCharacter ))
                            {
                                result = "punctuation-mark \"" + thisCharacter + "\"";
                            }
                            else if (Char.IsControl( thisCharacter ))
                            {
                                result = "(ASCII control-code " + n.ToString() + ")";
                            }
                            else
                            {
                                result = "(ASCII code " + n.ToString() + ")";
                            }
                        }
                        break;
                }
            }
            return result;
        }
        #endregion CharacterDescription

        #region CharacterDescriptions
        /// <summary>
        /// Given an arbitrary string, return another string that expresses the characters of that given string
        /// in plain English, with the Unicode code-points for non-obvious characters.
        /// </summary>
        /// <param name="s">The string to express</param>
        /// <returns>A string detailing the content of the given string</returns>
        /// <remarks>This is intended for giving feedback to a user regarding what he typed into a field,
        /// so it's only concerned with the normal keyboard-characters at this point.</remarks>
        public static string CharacterDescriptions( this string s )
        {
            string sR;
            if (!String.IsNullOrEmpty( s ))
            {
                if (s.Length == 1)
                {
                    sR = "{ " + CharacterDescription( s[0] ) + " }";
                }
                else
                {
                    int n = s.Length;
                    var sb = new StringBuilder( "{" );
                    for (int i = 0; i < n; i++)
                    {
                        sb.Append( CharacterDescription( s[i] ) );
                        if (i >= 0 && i < n - 1)
                        {
                            sb.Append( ", " );
                        }
                    }
                    sb.Append( "}" );
                    sR = sb.ToString();
                }
            }
            else
            {
                sR = "(empty-string)";
            }
            return sR;
        }
        #endregion

        #region CharacterDescriptionsOfNonPrintables
        /// <summary>
        /// Given an arbitrary string, return a string that expresses the characters of that given string
        /// in plain English, with the Unicode code-points for non-obvious characters,
        /// for OTHER than the common printable ASCII-characters.
        /// </summary>
        /// <param name="s">The string to express</param>
        /// <returns>A string detailing the content of the given string</returns>
        public static string CharacterDescriptionsOfNonPrintables( this string s )
        {
            string sR;
            if (!String.IsNullOrEmpty( s ))
            {
                var sb = new StringBuilder();
                int n = s.Length;
                for (int i = 0; i < n; i++)
                {
                    Char c = s[i];
                    if (Char.IsPunctuation( c ) || Char.IsLetterOrDigit( c ))
                    {
                        sb.Append( c );
                    }
                    else
                    {
                        sb.Append( "{" );
                        sb.Append( CharacterDescription( s[i] ) );
                        sb.Append( "}" );
                    }
                }
                sR = sb.ToString();
            }
            else
            {
                sR = "(empty-string)";
            }
            return sR;
        }
        #endregion

        #region Clear
#if PRE_4
        /// <summary>
        /// Set the given StringBuilder to zero length, such that it contains no text.
        /// This method is only provided for .NET Framework versions before 4.0, since that had no StringBuilder.Clear method.
        /// </summary>
        /// <param name="stringBuilder"></param>
        public static void Clear( this StringBuilder stringBuilder )
        {
            // This substitutes for
            // stringBuilder.Clear();
            stringBuilder.Length = 0;
        }
#endif
        #endregion

        #region ConcatLeftToRight
        /// <summary>
        /// Concatenate this string with another string, in a left-to-right FlowDirection.
        /// The need for this arises due to the fact that the normal C# string-concatenation methods insist upon falling back to a different order
        /// for, for example, Arabic characters.
        /// </summary>
        /// <param name="sLeft">the string to append to</param>
        /// <param name="sRight">the string to be appended</param>
        /// <returns></returns>
        public static string ConcatLeftToRight( this string sLeft, string sRight )
        {
            // We'll do this on a character-by-character basis.
            int nLeftLength = sLeft.Length;
            int nRightLength = sRight.Length;
            int nNewLength = nLeftLength + nRightLength;
            char[] aryResult = new char[nNewLength];
            for (int i = 0; i < nNewLength; i++)
            {
                if (i < nLeftLength)
                {
                    aryResult[i] = sLeft[i];
                }
                else
                {
                    aryResult[i] = sRight[i - nLeftLength];
                }
            }
            return new string( aryResult );
        }
        #endregion

        #region ContainsDelimited
        /// <summary>
        /// Given a string, return true if it contains the given text-pattern that is bound at either end by something
        /// other than letters; in other words - the text-pattern does not run into other text without some kind of
        /// delimiters to separate it. This is intended for unit-testing.
        /// </summary>
        /// <param name="what">the string that is to be checked as to whether it contains the text-pattern</param>
        /// <param name="textPattern">the text-pattern that you want to test whether is contained within what</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">the value of textPattern must not be null</exception>
        /// <exception cref="ArgumentException">the value of textPattern must not be an empty string</exception>
        /// <remarks>
        /// When unit-testing sometimes you want to confirm that a string contains something,
        /// but you don't want to restrict the implementer from fine-tuning the format of that string, ie.
        /// you do not want your unit-tests to fail just because of a benign change.
        /// 
        /// Thus, this confirms that a string does indeed contain the given text-pattern,
        /// and further that it is formatted at least correctly enough so that it is visually separate.
        /// 
        /// For example, if your method-under-test is outputing this: "Red\nGree\nBlue",
        /// and you provide a unit-tests that checks for that using NUnit's <c>Assert.AreEqual</c>,
        /// then if the designer decides the output would look better as "Red,Green,Blue"
        /// - you may not want your unit-tests to break just from this change.
        /// Thus, instead, you can use this (again, using NUnit syntax):
        ///   Assert.IsTrue(s.ContainsDelimited("Red"));
        ///   Assert.IsTrue(s.ContainsDelimited("Green"));
        ///   Assert.IsTrue(s.ContainsDelimited("Blue"));
        /// 
        /// This ensures that all three words are in there, but catches the mistake if they run together
        /// - as in "RedGreen,Blue".
        /// 
        /// Note: textPattern is trimmed of whitespace both fore and aft, before testing for containment.
        /// 
        /// The argument value for <paramref name="what"/> may be null or an empty string, in which case this returns false.
        /// </remarks>
        public static bool ContainsDelimited( this string what, string textPattern )
        {
            if (textPattern == null)
            {
                throw new ArgumentNullException( "textPattern" );
            }
            if (HasNothing( textPattern ))
            {
#if PRE_4
                throw new ArgumentException( "Argument textPattern must not be empty", "textPattern" );
#else
                throw new ArgumentException( message: "Argument " + nameof( textPattern ) + " must not be empty", paramName: nameof( textPattern ) );
#endif
            }
            bool answer = false;

            if (HasSomething( what ))
            {
                string patternTrimmed = textPattern.Trim();
                int nStartSearch = 0;
                int indexOfText;
                do
                {
#if PRE_4
                    indexOfText = what.IndexOf( patternTrimmed, nStartSearch, StringComparison.Ordinal );
#else
                    indexOfText = what.IndexOf( value: patternTrimmed, startIndex: nStartSearch, comparisonType: StringComparison.Ordinal );
#endif
                    if (indexOfText >= 0)
                    {
                        // Check characters in front of it, if there are any.
                        if (indexOfText > 0)
                        {
                            if (!IsDelimiterForContains( what[indexOfText - 1] ))
                            {
                                Debug.WriteLine( String.Format( "ContainsDelimited found textPattern '{0}' at index {1} but failed fore-delimiter-test.", patternTrimmed, indexOfText ) );
                                nStartSearch = indexOfText + 1;
                                continue;
                            }
                        }
                        // Check the characters after it, if there are any.
                        int lenOfContainer = what.Length;
                        int lenOfText = patternTrimmed.Length;
                        if (indexOfText + lenOfText < lenOfContainer)
                        {
                            int indexOfFollowingCharacter = indexOfText + lenOfText;
                            if (!IsDelimiterForContains( what[indexOfFollowingCharacter] ))
                            {
                                Debug.WriteLine( String.Format( "ContainsDelimited found textPattern '{0}' at index {1} but failed aft-delimiter-test.", patternTrimmed, indexOfText ) );
                                nStartSearch = indexOfText + 1;
                                continue;
                            }
                        }
                        answer = true;
                        break;
                    }
                    nStartSearch = indexOfText + 1;
                } while (indexOfText != -1);
            }
            return answer;
        }

        /// <summary>
        /// Return true if the given character is considered a 'delimiter' for the purpose of the <c>ContainsDelimited</c> method.
        /// </summary>
        /// <param name="characterToTest">the char to test</param>
        /// <returns>true only if the given char is one of those commonly used for delimiters</returns>
        internal static bool IsDelimiterForContains( char characterToTest )
        {
            char[] arrayOfDelimiters = { ',', '.', ' ', '\t', '-', '[', ']', '(', ')', '{', '}', '<', '>', '\n', '\r', '/', ';', ':', '\"', '\'' };
            return IsCharacterFrom( characterToTest, arrayOfDelimiters );
        }
        #endregion

        #region ContainsIgnoreCase
        /// <summary>
        /// Return true if this string contains the given string, ignoring case. This uses the Ordinal ordering.
        /// </summary>
        /// <param name="text">the text string to test for containment of what</param>
        /// <param name="what">what to see if is contained within text</param>
        /// <returns></returns>
        public static bool ContainsIgnoreCase( this string text, string what )
        {
            //return CultureInfo.CurrentCulture.CompareInfo.IndexOf( text, what, CompareOptions.IgnoreCase ) >= 0;
            return text.IndexOf( what, StringComparison.OrdinalIgnoreCase ) >= 0;
        }
        #endregion

        #region ContainsDigits
#if !PRE_4
        /// <summary>
        /// Return a Typle with a boolean indicating whether the given string contains a numeric digit,
        /// a string containing what was found,
        /// and an integer that is the index of the location of the digit.
        /// </summary>
        /// <param name="thisText">the string to search for any digits</param>
        /// <returns>a Typle with infor regarding what was found</returns>
        public static Tuple<bool, string, int> ContainsDigit( this string thisText )
        {
            if (thisText == null)
            {
                throw new ArgumentNullException( "thisText" );
            }
            bool foundADigit = false;
            string whatWasFound = String.Empty;
            int indexOfDigit = -1;
            int n = thisText.Length;
            for (int i = 0; i < n; i++)
            {
                char c = thisText[i];
                if (Char.IsDigit( c ))
                {
                    foundADigit = true;
                    whatWasFound = c.ToString();
                    indexOfDigit = i;
                    break;
                }
            }
            return Tuple.Create( foundADigit, whatWasFound, indexOfDigit );
        }
#endif

        /// <summary>
        /// Returns true if the given string contains any ASCII digits (that is, '0'..'9') and also puts a string representation of that in digitFound.
        /// </summary>
        /// <param name="thisText">The given string to check for digits</param>
        /// <param name="digitFound">If any digits are found, a string representation of the first digit found is placed into this</param>
        /// <param name="indexOfDigit">If any digits are found, the zero-based index into the given text is placed into this</param>
        /// <returns>false if the given string has no digits, true if it does</returns>
        /// <exception cref="ArgumentNullException"/>
        public static bool ContainsDigit( this string thisText, out string digitFound, out int indexOfDigit )
        {
            if (thisText == null)
            {
                throw new ArgumentNullException( "thisText" );
            }
            int n = thisText.Length;
            for (int i = 0; i < n; i++)
            {
                char c = thisText[i];
                if (Char.IsDigit( c ))
                {
                    digitFound = c.ToString();
                    indexOfDigit = i;
                    return true;
                }
            }
            digitFound = String.Empty;
            indexOfDigit = -1;
            return false;
        }
        #endregion

        #region ContainsRoot
        /// <summary>
        /// Indicate whether the string Contains the given word, ignoring case,
        /// or the simple plural of that word - doing a whole-word match.
        /// For now, the plural is assumed to end with "s".
        /// </summary>
        /// <param name="sThis">The text to test against</param>
        /// <param name="sWordSingular">The singular form of the word to match against</param>
        /// <returns>true if the string contains the given word or it's plural</returns>
        public static bool ContainsRoot( this string sThis, string sWordSingular )
        {
            string sWordFound;
            return ContainsRoot( sThis, sWordSingular, out sWordFound );
        }

        /// <summary>
        /// Indicate whether the string Contains the given word, ignoring case,
        /// or the simple plural of that word - doing a whole-word match.
        /// For now, the plural is assumed to end with "s".
        /// </summary>
        /// <param name="sThis">The text to test against</param>
        /// <param name="sWordSingular">The singular form of the word to match against</param>
        /// <param name="sWordFound">The actual word that was found, if any (as lowercase).</param>
        /// <returns>true if the string contains the given word or it's plural</returns>
        public static bool ContainsRoot( this string sThis, string sWordSingular, out string sWordFound )
        {
            // TODO: This also needs to guard against embedded, incidental pattern matches.
            sWordFound = String.Empty;
            string sPatternSingular = sWordSingular.ToLower();
            bool bFound = false;
            int LENp = sPatternSingular.Length;
            int LENme = sThis.Length;
            int i = sThis.IndexOf( sPatternSingular );
            if (i >= 0)
            {
                bFound = true;
                // Check the preceding boundary.
                if (i > 0)
                {
                    if (Char.IsLetter( sThis[i - 1] ))
                    {
                        // There is a letter immediately preceding the start of this word, so that was a false match.
                        bFound = false;
                    }
                }
                else if (i + LENp < LENme && Char.IsLetter( sThis[i + LENp] ))
                {
                    // There is a letter immediately following, so that was a false match.
                    bFound = false;
                }
                // We found the word as specified.
                if (bFound)
                {
                    sWordFound = sWordSingular;
                }
            }
            if (!bFound)
            {
                // TODO  This is the part that needs to be globalized.
                string sPatternPlural = sPatternSingular + "s";
                LENp = sPatternPlural.Length;
                i = sThis.IndexOf( sPatternPlural );
                if (i >= 0)
                {
                    bFound = true;
                    // Check the preceding boundary.
                    if (i > 0)
                    {
                        if (Char.IsLetter( sThis[i - 1] ))
                        {
                            // There is a letter immediately preceding the start of this word, so that was a false match.
                            bFound = false;
                        }
                    }
                    else if (i + LENp < LENme && Char.IsLetter( sThis[i + LENp] ))
                    {
                        // There is a letter immediately following, so that was a false match.
                        bFound = false;
                    }
                    // We found the plural form of the given word.
                    if (bFound)
                    {
                        sWordFound = sPatternPlural;
                    }
                }
            }
            return bFound;
        }
        #endregion

        #region ConvertForwardSlashsToBackSlashs
        /// <summary>
        /// Return a string that is a copy of the given text with all forward-slashes replaced with back-slashes.
        /// </summary>
        /// <param name="text">the text to convert</param>
        /// <returns>a copy of text with the slashes replaced</returns>
        public static string ConvertForwardSlashsToBackSlashs( string text )
        {
            return string.IsNullOrEmpty( text ) ? text : text.Replace( '/', '\\' );
        }
        #endregion

        #region DoubleQuoted and SingleQuoted
        /// <summary>
        /// Returns the given string enclosed within proper typographic Double-Quotes
        /// (ie, within Unicode characters for 201C and 201D).
        /// </summary>
        /// <param name="sText">The string you want to enclose</param>
        /// <returns>sText with quotes added</returns>
        public static string DoubleQuoted( this string sText )
        {
            return "\u201C" + sText + "\u201D";
        }

        /// <summary>
        /// Returns the given string enclosed within proper typographic single-quotes
        /// (ie, within Unicode characters for 2018 and 2019).
        /// </summary>
        /// <param name="sText">The string you want to enclose</param>
        /// <returns>sText with quotes added</returns>
        public static string SingleQuoted( this string sText )
        {
            return "\u2018" + sText + "\u2019";
        }
        #endregion

        #region EndsWith
#if SILVERLIGHT
        /// <summary>
        /// Return true if the final portion of thisString is the same as suffix.
        /// This is built-in with the full .NET Framework, but unavailable with Silverlight.
        /// </summary>
        /// <param name="thisString">The string in question</param>
        /// <param name="suffix">A string that we want to test whether is the same as the end of thisString</param>
        /// <param name="isIgnoringCase">Dictates whether to ignore capitalization in the comparison</param>
        /// <returns>true if thisString includes suffix at it's end</returns>
        public static bool EndsWith(this string thisString, string suffix, bool isIgnoringCase)
        {
            // The full .NET Framework has EndsWith, and we could simply return, for example: thisString.EndsWith(suffix, true, CultureInfo.InvariantCulture)
#if DEBUG
            if (thisString == null)
            {
                throw new ArgumentNullException("thisString");
            }
            if (suffix == null)
            {
                throw new ArgumentNullException("suffix");
            }
#endif
            int lengthOfThisString = thisString.Length;
            int lengthOfSuffix = suffix.Length;

            if (lengthOfSuffix > lengthOfThisString)
            {
                return false;
            }

            string pertinentPartOfThisString;
            if (lengthOfSuffix == lengthOfThisString)
            {
                pertinentPartOfThisString = thisString;
            }
            else
            {
                pertinentPartOfThisString = thisString.Substring(lengthOfThisString - lengthOfSuffix);
            }
            // At this point we know that pertinentPartOfThisString is the same length as prefix.
            if (isIgnoringCase)
            {
                return suffix.Equals(pertinentPartOfThisString, StringComparison.InvariantCultureIgnoreCase);
            }
            else
            {
                return suffix.Equals(pertinentPartOfThisString, StringComparison.InvariantCulture);
            }
        }
#endif
        #endregion EndsWith

        #region ExceptionDetails
        /// <summary>
        /// Return a textual description of the given Exception,
        /// including any inner exceptions and any Data property content.
        /// </summary>
        /// <param name="x">the Exception to describe</param>
        /// <param name="includeNewlines">this dictates whether it is permissible to include newline characters within the returned value</param>
        /// <param name="additionalInformation">(optional) additional text to add</param>
        /// <param name="memberName">the class-method from which this was called</param>
        /// <param name="sourceFilePath">the filesystem-path of the sourcecode-file from which this was called</param>
        /// <param name="sourceLineNumber">the line-number within the sourcecode-file from which this was called</param>
        /// <param name="showStackInformation">whether to also include the stack-trace of the exception</param>
        /// <returns>a concise description of the Exception</returns>
        /// <remarks>
        /// This description includes:
        ///   exception name (shortened)
        ///   exception Message property
        ///   data dictionary
        ///   stack-trace (optional)
        ///   member-name
        ///   source-file path
        ///   source line-number
        ///   recurse into the inner exceptions, adding a description of each
        /// 
        /// If the given exception is null, then an empty-string is returned.
        /// </remarks>
        public static string ExceptionDetails( Exception x,
                                               bool includeNewlines,
                                               string additionalInformation,
                                               string memberName,
                                               string sourceFilePath,
                                               int sourceLineNumber,
                                               bool showStackInformation )
        {
            return ExceptionDetails( x, includeNewlines, additionalInformation, memberName, sourceFilePath, sourceLineNumber, showStackInformation, "" );
        }

        /// <summary>
        /// Return a textual description of the given Exception,
        /// including any inner exceptions and any Data property content.
        /// </summary>
        /// <param name="x">the Exception to describe</param>
        /// <param name="includeNewlines">this dictates whether it is permissible to include newline characters within the returned value</param>
        /// <returns>a concise description of the Exception</returns>
        /// <remarks>
        /// This is the same as calling the method <c>ExceptionDetails</c> with 
        /// <c>true</c> for <c>showStackInformation</c>, zero for <c>sourceLineNumber</c>
        /// and <c>null</c> for the remaining arguments.
        /// 
        /// This description includes:
        ///   exception name (shortened)
        ///   exception Message property
        ///   data dictionary
        ///   stack-trace (optional)
        ///   member-name
        ///   source-file path
        ///   source line-number
        ///   recurse into the inner exceptions, adding a description of each
        /// 
        /// If the given exception is null, then an empty-string is returned.
        /// </remarks>
        public static string ExceptionDetails( Exception x, bool includeNewlines )
        {
            return ExceptionDetails( x, includeNewlines, null, null, null, 0, true, "" );
        }

        /// <summary>
        /// Return a textual description of the given Exception,
        /// including any inner exceptions and any Data property content.
        /// </summary>
        /// <param name="x">the exception to describe</param>
        /// <param name="includeNewlines">this dictates whether it is permissible to include newline characters within the returned value</param>
        /// <param name="additionalInformation">(optional) additional text to add</param>
        /// <param name="memberName">the class-method from which this was called</param>
        /// <param name="sourceFilePath">the filesystem-path of the sourcecode-file from which this was called</param>
        /// <param name="sourceLineNumber">the line-number within the sourcecode-file from which this was called</param>
        /// <param name="showStackInformation">whether to also include the stack-trace of the exception</param>
        /// <param name="indentation">how much to indent the beginning of each line</param>
        /// <returns>a concise description of the Exception</returns>
        /// <remarks>
        /// This description includes:
        ///   exception name (shortened)
        ///   exception Message property
        ///   data dictionary
        ///   stack-trace (optional)
        ///   member-name
        ///   source-file path
        ///   source line-number
        ///   recurse into the inner exceptions, adding a description of each
        /// 
        /// If the given exception is null, then an empty-string is returned.
        /// </remarks>
        private static string ExceptionDetails( Exception x,
                                                bool includeNewlines,
                                                string additionalInformation,
                                                string memberName,
                                                string sourceFilePath,
                                                int sourceLineNumber,
                                                bool showStackInformation,
                                                string indentation )
        {
            var sb = new StringBuilder();
            // Add the exception type-name, message, and data-dictionary..
            if (x != null)
            {
                string exceptionTypeName = ExceptionNameShortened( x );
                sb.Append( exceptionTypeName );
                if (x.Message.StartsWith( "Exception of type" ) && x.Message.EndsWith( "was thrown." ))
                {
                    //CBL When does THIS happen?
                    sb.Append( ", " );
                }
                else
                {
                    sb.Append( ": " );

                    // Put the Message.
                    if (String.IsNullOrEmpty( indentation ))
                    {
                        if (includeNewlines)
                        {
                            sb.Append( x.Message );
                        }
                        else
                        {
                            string message = x.Message.Replace( Environment.NewLine, ", " );
                            sb.Append( message );
                        }
                    }
                    else
                    {
                        // If Message has multiple lines, indent them all.
                        string m;
                        if (includeNewlines)
                        {
                            m = x.Message.Replace( Environment.NewLine, Environment.NewLine + indentation );
                        }
                        else
                        {
                            m = x.Message.Replace( Environment.NewLine, ", " );
                        }
                        sb.Append( m );
                    }
                    sb.Append( ", " );

                    // Add the Source.
                    if (HasSomething( x.Source ))
                    {
                        sb.Append( "Source = " ).Append( x.Source ).Append( ", " );
                    }

                    // Attend to any exception-specific properties.
                    if (x is BadImageFormatException)
                    {
                        BadImageFormatException badImageFormatException = x as BadImageFormatException;
                        if (HasSomething( badImageFormatException.FileName ))
                        {
                            sb.Append( "FileName = " ).Append( badImageFormatException.FileName ).Append( ", " );
                        }
#if !NETFX_CORE
                        if (HasSomething( badImageFormatException.FusionLog ))
                        {
                            sb.Append( "FusionLog = " ).Append( badImageFormatException.FusionLog ).Append( ", " );
                        }
#endif
                    }
                    else if (x is COMException)
                    {
                        COMException comException = x as COMException;
#if !NETFX_CORE
                        sb.Append( "ErrorCode = " ).Append( comException.ErrorCode ).Append( ", " );
#endif
#if !PRE_5
                        // .NET Framework 4.0 did not have the HResult property defined as 'public'.
                        sb.Append( "HResult = " ).Append( comException.HResult ).Append( ", " );
#endif
                    }
                    else if (x is FileNotFoundException)
                    {
                        // If it is a FileNotFoundException, include the FileName value.
                        FileNotFoundException fileNotFoundException = x as FileNotFoundException;
                        if (HasSomething( fileNotFoundException.FileName ))
                        {
                            sb.Append( "FileName = " ).Append( fileNotFoundException.FileName ).Append( ", " );
                        }
                    }
#if !PRE_4 && !NETFX_CORE
                    else if (x is InvalidAsynchronousStateException)
                    {
                        // If it is a InvalidAsynchronousStateException, include the ParamName value.
                        //CBL Test, and ensure it is not redundant.
                        InvalidAsynchronousStateException invalidAsynchronousStateException = x as InvalidAsynchronousStateException;
                        sb.Append( "ParamName = " ).Append( invalidAsynchronousStateException.ParamName ).Append( ", " );
                    }
#endif
#if !NETFX_CORE
                    else if (x is NotFiniteNumberException)
                    {
                        NotFiniteNumberException notFiniteNumberException = x as NotFiniteNumberException;
                        sb.Append( "OffendingNumber = " ).Append( notFiniteNumberException.OffendingNumber ).Append( ", " );
                    }
#endif
                    else if (x is SEHException)
                    {
                        SEHException sehException = x as SEHException;
#if !NETFX_CORE
                        sb.Append( "ErrorCode = " ).Append( sehException.ErrorCode ).Append( ", " );
#endif
#if !PRE_5
                        // .NET Framework 4.0 did not have the HResult property defined as 'public'.
                        sb.Append( "HResult = " ).Append( sehException.HResult ).Append( ", " );
#endif
                    }
                    else if (x is Win32Exception)
                    {
                        // If it is a Win32Exception, include the NativeErrorCode value.
                        Win32Exception win32Exception = x as Win32Exception;
                        sb.Append( "NativeErrorCode = " ).Append( win32Exception.NativeErrorCode ).Append( ", " );
                    }
                    //CBL Check against all these
                    //https://www.infoq.com/articles/Exceptions-API-Design?utm_campaign=rightbar_v2&utm_source=infoq&utm_medium=articles_link&utm_content=link_text
                }

                // Write out any information that is in the Data dictionary of this exception.
                string textOfDataDictionary = TextOfExceptionDataDictionary( x, includeNewlines, indentation );
                if (textOfDataDictionary.HasSomething())
                {
                    if (includeNewlines)
                    {
                        sb.AppendLine( textOfDataDictionary );
                    }
                    else
                    {
                        sb.Append( textOfDataDictionary ).Append( ", " );
                    }
                }
            }
            // Add the additional-information, if any.
            if (additionalInformation != null)
            {
                sb.Append( additionalInformation ).Append( ", " );
            }
            // Either show the stack-trace, or else list the source location information.
            if (x != null && showStackInformation && x.StackTrace != null)
            {
                string stackText = x.StackTrace.Trim();
                if (!includeNewlines)
                {
                    stackText = stackText.Replace( Environment.NewLine, ", " );
                }
                sb.Append( stackText );
                if (includeNewlines)
                {
                    sb.AppendLine().Append( indentation );
                }
            }
            else
            {
                if (HasSomething( memberName ))
                {
                    sb.Append( "member-name: " ).Append( memberName ).Append( ", " );
                }
                if (HasSomething( sourceFilePath ))
                {
                    sb.Append( "source-file: " ).Append( sourceFilePath ).Append( ", " );
                }
                if (sourceLineNumber > 0)
                {
                    sb.Append( "line-number: " ).Append( sourceLineNumber );
                }
            }
            // Describe any inner exceptions..
            if (x != null)
            {
                bool hasGottenTheInnerExceptions = false;
#if !PRE_4
                if (x is AggregateException)
                {
                    AggregateException aggregateException = x as AggregateException;
                    if (aggregateException.InnerExceptions != null && aggregateException.InnerExceptions.Count > 0)
                    {
                        hasGottenTheInnerExceptions = true;
                        if (includeNewlines)
                        {
                            sb.AppendLine().Append( indentation );
                        }
                        int n = 0;
                        foreach (var innerException in aggregateException.InnerExceptions)
                        {
                            n++;
                            sb.Append( "  inner-exception " ).Append( n ).Append( " --> " );
                            // Recurse back into this method to describe this inner exception.
                            sb.Append( ExceptionDetails( innerException, includeNewlines, null, null, null, 0, false, indentation + "    " ) );
                            if (includeNewlines)
                            {
                                sb.AppendLine();
                            }
                        }
                    }
                }
#endif
                if (!hasGottenTheInnerExceptions && x.InnerException != null)
                {
                    if (includeNewlines)
                    {
                        sb.AppendLine().Append( indentation );
                    }
                    //CBL If we are recursing here down a chain of inner exceptions, then ensure that we do not list multiple base exceptions.
                    // This, may list a separate one for every inner exception -- not correct. It may also be redundant.
                    Exception baseException = x.GetBaseException();
                    sb.Append( "  base-exception --> " );
                    // Recurse back into this method to describe this baseException.
                    sb.Append( ExceptionDetails( baseException, includeNewlines, null, null, null, 0, false, indentation + "  " ) );
                    Exception innerException = x.InnerException;
                    if (innerException != baseException)
                    {
                        sb.Append( "  inner-exception --> " );
                        // Recurse back into this method to describe this inner exception.
                        sb.Append( ExceptionDetails( innerException, includeNewlines, null, null, null, 0, false, indentation + "  " ) );
                    }
                }

            }
            return sb.ToString().WithoutAtEnd( ',' );
        }

        private static string TextOfExceptionDataDictionary( Exception exception, bool includeNewlines, string indentation )
        {
            // Write out any information that is in the Data dictionary of the Exception..
            if (exception.Data.Count > 0)
            {
                var sb = new StringBuilder();
                if (includeNewlines)
                {
                    sb.AppendLine();
                }
                else
                {
                    sb.Append( " " );
                }
                bool isFirst = true;
                // For each entry within the Data dictionary..
                foreach (var dataKey in exception.Data.Keys)
                {
                    string keyString = dataKey as String;
                    // As long as it's not a HelpLink,
                    if (!(keyString != null && keyString.StartsWith( "HelpLink" )))
                    {
                        // Separate them with commas.
                        if (!isFirst)
                        {
                            if (includeNewlines)
                            {
                                sb.AppendLine( "," );
                            }
                            else
                            {
                                sb.Append( ", " );
                            }
                        }
                        // Show the dictionary-item's key and value.
                        sb.Append( indentation ).Append( dataKey ).Append( ": " ).Append( exception.Data[dataKey] );
                        isFirst = false;
                    }
                }
                return sb.ToString();
            }
            else
            {
                return String.Empty;
            }
        }
        #endregion

        #region ExceptionNameShortened
        /// <summary>
        /// Return the name of the given exception, with some of the more common namespace prefices stripped off.
        /// </summary>
        /// <param name="x">the exception to return the name of</param>
        /// <returns>the name of the given exception</returns>
        /// <remarks>
        /// I'm trying to keep these method-names worded to make them easier to find.
        /// Thus, to find methods related to "Exception" - you only have to look for the prefix "Exception".
        /// </remarks>
        public static string ExceptionNameShortened( this Exception x )
        {
            string exceptionTypeName = x.GetType().ToString();
            if (exceptionTypeName.IndexOf( "System.IO." ) == 0)
            {
                exceptionTypeName = exceptionTypeName.Substring( 10 );
            }
            else if (exceptionTypeName.IndexOf( "System.ComponentModel." ) == 0)
            {
                exceptionTypeName = exceptionTypeName.Substring( 22 );
            }
            else if (exceptionTypeName.IndexOf( "System." ) == 0)
            {
                exceptionTypeName = exceptionTypeName.Substring( 7 );
            }
            return exceptionTypeName;
        }
        #endregion

        #region ExceptionShortSummary
        /// <summary>
        /// Return a textual, cursory summary description of the given Exception.
        /// </summary>
        /// <param name="x">the Exception to describe</param>
        /// <param name="additionalInformation">additional text to add</param>
        /// <returns>a summary of this exception - shorter than ExceptionDetails would provide</returns>
        /// <remarks>
        /// This provides only the name of the exception and it's Message property.
        /// </remarks>
        public static string ExceptionShortSummary( this Exception x, string additionalInformation )
        {
            var sb = new StringBuilder();
            sb.Append( ExceptionNameShortened( x ) );
            sb.Append( ": " );
            sb.Append( x.Message );
            if (HasSomething( additionalInformation ))
            {
                sb.Append( ", " );
                sb.Append( additionalInformation );
            }
            return sb.ToString();
        }

        /// <summary>
        /// Return a textual, cursory summary description of the given Exception.
        /// </summary>
        /// <param name="x">the Exception to describe</param>
        /// <returns>a summary of this exception - shorter than ExceptionDetails would provide</returns>
        /// <remarks>
        /// This provides only the name of the exception and it's Message property.
        /// </remarks>
        public static string ExceptionShortSummary( this Exception x )
        {
            var sb = new StringBuilder();
            sb.Append( ExceptionNameShortened( x ) );
            sb.Append( ": " );
            sb.Append( x.Message );
            return sb.ToString();
        }
        #endregion ExceptionShortSummary

        #region ExpandTo
        /// <summary>
        /// Given a piece of text, expand it to the desired length by simply repeating the text, or shortening it as necessary.
        /// </summary>
        /// <param name="originalText">the original text-pattern to expand or shorten</param>
        /// <param name="desiredLength">the length of the result</param>
        /// <returns>a new string that is the original expanded or shortened to the specified length</returns>
        public static string ExpandTo( this string originalText, int desiredLength )
        {
            // Note: This is intended for things like running unit-tests.
            // In fact, I wrote this using pure TDD: I wrote a comprehensive series of unit-tests, then composed this method to pass those tests.
            if (originalText == null)
            {
                throw new ArgumentNullException( "originalText" );
            }
            if (HasNothing( originalText ))
            {
                throw new ArgumentException( "originalText must not be empty" );
            }
            if (desiredLength < 0)
            {
                throw new ArgumentOutOfRangeException( "desiredLength must not be negative!" );
            }
            else if (desiredLength == 0)
            {
                return String.Empty;
            }
            else if (desiredLength == originalText.Length)
            {
                return originalText;
            }
            else if (desiredLength < originalText.Length)
            {
                return originalText.Substring( 0, desiredLength );
            }
            int len = originalText.Length;
            int numberOfTimesItFits = desiredLength / len;
            var sb = new StringBuilder();
            for (int i = 0; i < numberOfTimesItFits; i++)
            {
                sb.Append( originalText );
            }
            int remainderLength = desiredLength % len;
            // Add any partial-pattern..
            if (remainderLength > 0)
            {
                string remainder = originalText.Substring( 0, remainderLength );
                sb.Append( remainder );
            }
            return sb.ToString();
        }
        #endregion ExpandTo

        #region FormatInvariant
        /// <summary>
        /// Return the given string and objects, formatted into once string using the invariant culture
        /// (same as String.Format except the culture need not be specified).
        /// </summary>
        /// <param name="formatString">the format string</param>
        /// <param name="objects">the objects to be inserted into the result</param>
        /// <returns></returns>
        public static string FormatInvariant( this string formatString, params object[] objects )
        {
            return string.Format( CultureInfo.InvariantCulture, formatString, objects );
        }
        #endregion

        #region FormatCurrentCulture
        /// <summary>
        /// Return the given string and objects, formatted into one string, using the current culture
        /// (same as String.Format except the culture need not be specified).
        /// </summary>
        /// <param name="formatString">the format string</param>
        /// <param name="objects">the objects to be inserted into the result</param>
        /// <returns></returns>
        public static string FormatCurrentCulture( this string formatString, params object[] objects )
        {
            return string.Format( CultureInfo.CurrentCulture, formatString, objects );
        }
        #endregion

        #region GetByteArrayFromText
        /// <summary>
        /// Get the bytes that the given text represents and write it to the elements of the given byte-array.
        /// </summary>
        /// <param name="dataText">the text string to get the bytes from, which is presumed to be hexadecimal format</param>
        /// <param name="byteArrayBuffer">the byte-array to write the byte values to</param>
        public static void GetByteArrayFromText( string dataText, byte[] byteArrayBuffer )
        {
            char[] letters = new char[] { ',', '\t', ' ', '\n', '\r' };
            string[] strData = dataText.Split( letters, StringSplitOptions.RemoveEmptyEntries );

            for (int i = 0; i < strData.Length; i++)
            {
                try
                {
                    byteArrayBuffer[i] = Convert.ToByte( strData[i], 16 );
                }
                catch (FormatException x)
                {
                    string thisData = strData[i];
                    //CBL  The ideal here would be to use CharacterDescription(thisData), but I moved that method to HumanLanguageLib. 2015-8-26
                    string msg = String.Format( "At character position {0}, had an issue with this: {1}", i, thisData );
                    x.Data.Add( "Description", msg );
                    throw;
                }
            }
        }
        #endregion

        #region HasNothing
        /// <summary>
        /// Return true if the given string is null or contains no characters other than whitespace.
        /// This substitutes for String.IsNullOrWhitespace for pre .NET 4.0 applications.
        /// </summary>
        /// <param name="text">the string to test</param>
        /// <returns>true if it is an empty string or null (or only whitespace characters)</returns>
        public static bool HasNothing( this string text )
        {
#if PRE_4
            if (text == null)
            {
                return true;
            }
            string textTrimmed = text.Trim();
            return textTrimmed.Length == 0;
#else
            return String.IsNullOrWhiteSpace( text );
#endif
        }
        #endregion

        #region HasSomething
        /// <summary>
        /// Return true if the given string contain at least one character other than whitespace.
        /// This substitutes for !String.IsNullOrWhitespace for pre .NET 4.0 applications.
        /// </summary>
        /// <param name="text">the string to test</param>
        /// <returns>true if it is an empty string or null (or only whitespace characters)</returns>
        public static bool HasSomething( this string text )
        {
#if PRE_4
            if (text == null)
            {
                return false;
            }
            string textTrimmed = text.Trim();
            return textTrimmed.Length > 0;
#else
            return !String.IsNullOrWhiteSpace( text );
#endif
        }
        #endregion

        #region IndexOfNotInCsComment
        /// <summary>
        /// Return the position of the given pattern within the given string,
        /// or -1 if it is not present -- ignoring any text that comes after a C# comment ( "//" ).
        /// </summary>
        /// <param name="withinWhat">the text wtihin which to search for the pattern</param>
        /// <param name="pattern">the pattern of text to search for</param>
        /// <returns>the zero-based index of where the pattern is found, or minus-1 if not found</returns>
        public static int IndexOfNotInCsComment( this string withinWhat, string pattern )
        {
            if (withinWhat == null)
            {
                throw new ArgumentNullException( "withinWhat" );
            }
            int indexOfComment = withinWhat.IndexOf( "//" );
            if (indexOfComment == -1)
            {
                return withinWhat.IndexOf( pattern );
            }
            else
            {
                string newText = withinWhat.Substring( 0, indexOfComment );
                return newText.IndexOf( pattern );
            }
        }
        #endregion

        #region IsAllDigits
        /// <summary>
        /// Indicates whether the given string is composed only of numeric digits
        /// </summary>
        /// <param name="sInput">The string to check</param>
        /// <returns>true if the input IS composed only of digits</returns>
        public static bool IsAllDigits( this string sInput )
        {
            bool r = true;
            if (sInput.Length > 0)
            {
                foreach (char ch in sInput)
                {
                    if (!Char.IsDigit( ch ))
                    {
                        r = false;
                        break;
                    }
                }
            }
            else
            {
                //CBL How, really, do I want this to behave for empty inputs?
                r = false;
            }
            return r;
        }
        #endregion

        #region IsAllHexDigits
        /// <summary>
        /// Indicates whether the given string is composed only of numeric digits or A..F
        /// </summary>
        /// <param name="sInput">The string to check</param>
        /// <returns>true if the input IS composed only of digits</returns>
        public static bool IsAllHexDigits( this string sInput )
        {
            bool r = true;
            if (sInput.Length > 0)
            {
                string sInputUpperCase = sInput.ToUpper();
                foreach (char ch in sInputUpperCase)
                {
                    if (!(Char.IsDigit( ch ) || ch == 'A' || ch == 'B' || ch == 'C' || ch == 'D' || ch == 'E' || ch == 'F'))
                    {
                        r = false;
                        break;
                    }
                }
            }
            return r;
        }
        #endregion

        #region IsValidHexCharacter
        /// <summary>
        /// Return true if the given character is valid within a hexadecimal textual value.
        /// </summary>
        /// <param name="characterToCheck">the character in question</param>
        /// <returns>true only if characterToCheck is valid within a hexadecimal representation</returns>
        public static bool IsValidHexCharacter( this char characterToCheck )
        {
            return (Char.IsDigit( characterToCheck ) || _hexCharacters.Contains( characterToCheck ));
        }

        private static char[] _hexCharacters = new char[] { 'A', 'B', 'C', 'D', 'E', 'F', 'a', 'b', 'c', 'd', 'e', 'f' };

        #endregion

        #region IsAllLowercase
        /// <summary>
        /// Indicate whether the given string is composed of only lowercase letters (or no letters at all)
        /// </summary>
        /// <param name="sWhat">The given string to check</param>
        /// <returns>true if there are no uppercase letters in the given string</returns>
        /// <exception cref="ArgumentException"/>
        public static bool IsAllLowercase( this string sWhat )
        {
            if (HasNothing( sWhat ))
            {
                throw new ArgumentException( "sWhat" );
            }
            return (sWhat == sWhat.ToLower());
        }
        #endregion

        #region IsAllUppercase
        /// <summary>
        /// Indicate whether the given string is composed of only uppercase letters (or no letters at all)
        /// </summary>
        /// <param name="sWhat">The given string to check</param>
        /// <returns>true if there are no lowercase letters in the given string</returns>
        /// <exception cref="ArgumentException"/>
        public static bool IsAllUppercase( this string sWhat )
        {
            if (HasNothing( sWhat ))
            {
                throw new ArgumentException( "sWhat must not be empty" );
            }
            return (sWhat == sWhat.ToUpper());
        }
        #endregion

        #region IsADayOfTheWeek
        /// <summary>
        /// Returns true if the given sInput represents a valid (English!) day of the week,
        /// and sets eValue to what day-of-the-week that input represents.
        /// </summary>
        /// <param name="what">Some text that we will check to see whether it is a day-of-the-week</param>
        /// <param name="eValue">The System.DayOfWeek value that the input represents, or Sunday by default</param>
        /// <returns>true if the input is indeed a valid DayOfWeek value</returns>
        public static bool IsADayOfTheWeek( this string what, out DayOfWeek eValue )
        {
            eValue = DayOfWeek.Sunday;
            string s = what.ToLower();
            switch (s)
            {
                case "sun":
                case "sunday":
                    eValue = DayOfWeek.Sunday;
                    return true;
                case "mon":
                case "monday":
                    eValue = DayOfWeek.Monday;
                    return true;
                case "tue":
                case "tues":
                case "tuesday":
                case "tueday":
                    eValue = DayOfWeek.Tuesday;
                    return true;
                case "wed":
                case "wednesday":
                case "wedsday":
                case "wensday":
                    eValue = DayOfWeek.Wednesday;
                    return true;
                case "thursday":
                case "thurday":
                case "thurs":
                case "thur":
                    eValue = DayOfWeek.Thursday;
                    return true;
                case "friday":
                case "fri":
                    eValue = DayOfWeek.Friday;
                    return true;
                case "saturday":
                case "sat":
                case "satday":
                    eValue = DayOfWeek.Saturday;
                    return true;
                default:
                    return false;
            }
        }
        #endregion IsDayOfWeek

        #region IsANumberWord
        /// <summary>
        /// Indicates whether the given word can be recognized as a number value, and gives the value
        /// </summary>
        /// <param name="sWord">The given word</param>
        /// <param name="niValue">Sets this to the numeric value of the word, or null</param>
        /// <param name="bIsOrdinal">This gets set to true if the given sWord is an ordinal number (eg, "first")</param>
        /// <returns>true if it is a number, false if not</returns>
        public static bool IsANumberWord( this string sWord, out int? niValue, out bool bIsOrdinal )
        {
            bIsOrdinal = false;
            bool bYes = true;
            niValue = 0;
            // Handle empty inputs.
            if (String.IsNullOrEmpty( sWord ))
            {
                niValue = null;
                return false;
            }
            string s = sWord.Trim().ToLower();
            // Check for a "st", "nd", "th" or "rd" suffix.
            bool bHasOrdinalSuffix = false;
            if (s != "first" && s.EndsWith( "st" ))
            {
                // Remove the ordinal suffix, but remember that it was there.
                bHasOrdinalSuffix = true;
                s = s.Substring( 0, s.Length - 2 );
            }
            else if (s != "second" && s.EndsWith( "nd" ))
            {
                // Remove the ordinal suffix, but remember that it was there.
                bHasOrdinalSuffix = true;
                s = s.Substring( 0, s.Length - 2 );
            }
            else if (s.EndsWith( "th" ) && s != "fifth" && s != "eighth" && s != "ninth")
            {
                // Remove the ordinal suffix, but remember that it was there.
                bHasOrdinalSuffix = true;
                s = s.Substring( 0, s.Length - 2 );
            }
            else if (s.EndsWith( "rd" ) && s != "third")
            {
                // Remove the ordinal suffix, but remember that it was there.
                bHasOrdinalSuffix = true;
                s = s.Substring( 0, s.Length - 2 );
            }
            if (bHasOrdinalSuffix)
            {
                bIsOrdinal = true;
            }
            switch (s)
            {
                case "nothing":
                case "nuthin":
                case "nada":
                case "zip":
                case "unknown":
                case "unsure":
                case "unspecified":
                case "dunno":
                    niValue = null;
                    break;
                case "zero":
                case "o":
                    niValue = 0;
                    break;
                case "one":
                    niValue = 1;
                    break;
                case "two":
                    niValue = 2;
                    break;
                case "three":
                    niValue = 3;
                    break;
                case "four":
                    niValue = 4;
                    break;
                case "five":
                    niValue = 5;
                    break;
                case "six":
                    niValue = 6;
                    break;
                case "seven":
                    niValue = 7;
                    break;
                case "eight":
                    niValue = 8;
                    break;
                case "nine":
                    niValue = 9;
                    break;
                case "ten":
                    niValue = 10;
                    break;
                case "eleven":
                    niValue = 11;
                    break;
                case "twelve":
                    niValue = 12;
                    break;
                case "thirteen":
                    niValue = 13;
                    break;
                case "fourteen":
                case "forteen":
                    niValue = 14;
                    break;
                case "fifteen":
                    niValue = 15;
                    break;
                case "sixteen":
                    niValue = 16;
                    break;
                case "seventeen":
                    niValue = 17;
                    break;
                case "eighteen":
                    niValue = 18;
                    break;
                case "nineteen":
                case "ninteen":
                    niValue = 19;
                    break;
                case "twenty":
                    niValue = 20;
                    break;
                case "thirty":
                    niValue = 30;
                    break;
                case "forty":
                case "fourty":
                    niValue = 40;
                    break;
                case "fifty":
                    niValue = 50;
                    break;
                case "sixty":
                    niValue = 60;
                    break;
                case "seventy":
                    niValue = 70;
                    break;
                case "eighty":
                    niValue = 80;
                    break;
                case "ninty":
                case "ninety":
                    niValue = 90;
                    break;
                case "first":
                    bIsOrdinal = true;
                    niValue = 1;
                    break;
                case "second":
                    bIsOrdinal = true;
                    niValue = 2;
                    break;
                case "third":
                    bIsOrdinal = true;
                    niValue = 3;
                    break;
                // "fourth" already handled
                case "fifth":
                    bIsOrdinal = true;
                    niValue = 5;
                    break;
                // "sixth" already handled
                // "seventh" already handled
                case "eighth":
                    bIsOrdinal = true;
                    niValue = 8;
                    break;
                case "ninth":
                    bIsOrdinal = true;
                    niValue = 9;
                    break;
                default:
                    bYes = false;
                    break;
            }
            return bYes;
        }
        #endregion IsANumberWord

        #region IsCharacterFrom
        /// <summary>
        /// Indicate whether the given character is found within the given array of characters.
        /// </summary>
        /// <param name="theGivenCharacter">The Character to test against the array of characters</param>
        /// <param name="allValidCharacters">The array of characters that we consider to be valid in this instance</param>
        /// <returns>true if the given character is present in allValidCharacters</returns>
        public static bool IsCharacterFrom( this Char theGivenCharacter, char[] allValidCharacters )
        {
            bool isValid = false;
            foreach (char thisValidChar in allValidCharacters)
            {
                if (theGivenCharacter == thisValidChar)
                {
                    isValid = true;
                    break;
                }
            }
            return isValid;
        }
        #endregion

        #region IsEnglishAlphabetLetter
        /// <summary>
        /// Return true if the given character is a letter of the English alphabet (ie, A through Z), regardless of whether it's upper or lower case.
        /// </summary>
        /// <param name="chr">The character to test</param>
        /// <returns>True if it is a letter of the English alphabet</returns>
        public static bool IsEnglishAlphabetLetter( this char chr )
        {
            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex( @"[a-zA-Z]" );
            string testInput = chr.ToString();
            return regex.IsMatch( testInput );
        }
        #endregion

        #region IsEqualIgnoringCase
        /// <summary>
        /// Extension method that provides a slightly simpler way to do a case-insensitive test of two strings for equality
        /// when you don't want to consider case and beginning-or-trailing whitespace.
        /// </summary>
        /// <param name="thisString">one string to compare</param>
        /// <param name="toWhat">the other string to compare against</param>
        /// <returns>true if they're equal regardless of case</returns>
        public static bool IsEqualIgnoringCase( this string thisString, string toWhat )
        {
            string sThisTrimmed = thisString.Trim();
            string sToWhatTrimmed = toWhat.Trim();
#if SILVERLIGHT
            return String.Compare(sThisTrimmed, sToWhatTrimmed, StringComparison.InvariantCultureIgnoreCase) == 0;
#else
            return String.Compare( sThisTrimmed, sToWhatTrimmed, true ) == 0;
#endif
        }
        #endregion

        #region IsExpressedInHexadecimal
        /// <summary>
        /// Indicate whether the string seems to be an integer value that is expressed in hexadecimal,
        /// by detecting the presense of a prefix or suffix, or the inclusion of A..F as digits.
        /// </summary>
        /// <param name="text">the string to test</param>
        /// <returns>True if it looks like it's being expressed as hex</returns>
        public static bool IsExpressedInHexadecimal( this string text )
        {
            bool r = false;
            if (!String.IsNullOrEmpty( text ))
            {
                string trimmedText = text.Trim();
#if SILVERLIGHT
                if (trimmedText.StartsWith("0x", true)
                    || trimmedText.StartsWith("&h", true)
                    || trimmedText.StartsWith("h", true)
                    || trimmedText.EndsWith("h", true)
                    || trimmedText.EndsWith("hex", true)
                    || trimmedText.EndsWith("x", true))
#else
                //CBL
#if NETFX_CORE
                if (trimmedText.StartsWith( "0x" )
                    || trimmedText.StartsWith( "&h" )
                    || trimmedText.StartsWith( "h" )
                    || trimmedText.EndsWith( "h" )
                    || trimmedText.EndsWith( "hex" )
                    || trimmedText.EndsWith( "x" ))
#else
                if (trimmedText.StartsWith( "0x", true, CultureInfo.InvariantCulture )
                    || trimmedText.StartsWith( "&h", true, CultureInfo.InvariantCulture )
                    || trimmedText.StartsWith( "h", true, CultureInfo.InvariantCulture )
                    || trimmedText.EndsWith( "h", true, CultureInfo.InvariantCulture )
                    || trimmedText.EndsWith( "hex", true, CultureInfo.InvariantCulture )
                    || trimmedText.EndsWith( "x", true, CultureInfo.InvariantCulture ))
#endif
#endif
                {
                    return true;
                }
                var regex = new Regex( @"[a-fA-F]+" );
                if (regex.IsMatch( trimmedText ))
                {
                    return true;
                }
            }
            return r;
        }
        #endregion

        #region IsHelpRequest
        /// <summary>
        /// Return true if the argument contains text indicating a command-line request for help with a given command,
        /// as with "help", or "h".
        /// </summary>
        /// <param name="inputArgument">the text to test</param>
        /// <returns>true if the given argument contains a request for help</returns>
        public static bool IsHelpRequest( this string inputArgument )
        {
            bool result = false;
            if (!String.IsNullOrEmpty( inputArgument ))
            {
                if (inputArgument.StartsWith( "-" ) || inputArgument.StartsWith( "/" ))
                {
                    inputArgument = inputArgument.Substring( 1 );
                }
                if (inputArgument.Equals( "H", StringComparison.OrdinalIgnoreCase ) || inputArgument.Equals( "HELP", StringComparison.OrdinalIgnoreCase ))
                {
                    result = true;
                }
            }
            return result;
        }
        #endregion

        #region IsInArray
        /// <summary>
        /// Return true if the given string does not match any of the strings within the given array-of-strings, ignoring case.
        /// </summary>
        /// <param name="what">the given string value which we want to whether the array contains</param>
        /// <param name="arrayOfStrings">the array of strings that we want to check for containing what</param>
        /// <returns>true if arrayOfStrings has a copy (without regard to case) of the value what</returns>
        public static bool IsInArray( this string what, string[] arrayOfStrings )
        {
            if (what == null)
            {
                throw new ArgumentNullException( "arrayOfStrings" );
            }
            if (arrayOfStrings != null)
            {
                foreach (string element in arrayOfStrings)
                {
                    if (what.Equals( element, StringComparison.OrdinalIgnoreCase ))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region IsInTitlecase
        /// <summary>
        /// Indicates whether the given string has the initial letter in uppercase and the remainder all lowercase.
        /// </summary>
        /// <param name="sWhat">The given string to check</param>
        /// <returns>true if only the first letter is in uppercase</returns>
        /// <exception cref="ArgumentException"/>
        /// <remarks>
        /// This method is named thus in order to reflect the naming of the titlecase function in Python.
        /// </remarks>
        public static bool IsInTitlecase( this string sWhat )
        {
            if (HasNothing( sWhat ))
            {
                throw new ArgumentException( "sWhat must not be empty" );
            }
            bool bAnswer = false;
            string sFirstCharacter = sWhat.Substring( 0, 1 );
            if (sFirstCharacter == sFirstCharacter.ToUpper())
            {
                string sRestOfIt = sWhat.Substring( 1 );
                if (sRestOfIt == sRestOfIt.ToLower())
                {
                    bAnswer = true;
                }
            }
            return bAnswer;
        }
        #endregion

        #region IsNumeric
        /// <summary>
        /// Validates a string to see if it can be converted into a numeric value.
        /// Models the same function in VB.Net
        /// </summary>
        public static bool IsNumeric( this string stringToCheck )
        {
            double dd;
            return Double.TryParse( stringToCheck, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out dd );
        }
        #endregion

        #region IsOnlyDigitsAndPeriod
        /// <summary>
        /// Indicates whether the given string is composed only of numeric digits and an optional decimal-point
        /// </summary>
        /// <param name="sInput">The string to check</param>
        /// <returns>true if the input IS composed only of digits and an optional decimal-point</returns>
        public static bool IsOnlyDigitsAndPeriod( this string sInput )
        {
            bool r = true;
            bool bDecPoint = false;
            if (sInput.Length > 0)
            {
                foreach (char ch in sInput)
                {
                    if (!Char.IsDigit( ch ))
                    {
                        if (ch == '.')
                        {
                            if (bDecPoint)
                            {
                                // Too many decimal-points! fail.
                                r = false;
                                break;
                            }
                            else
                            {
                                // Flag that we've already seen the decimal-point
                                bDecPoint = true;
                            }
                        }
                        else
                        {
                            r = false;
                            break;
                        }
                    }
                }
            }
            return r;
        }
        #endregion

        #region IsOnlyDigitsPeriodsAndCommas
        /// <summary>
        /// Indicates whether the given string is composed only of numeric digits
        /// and possibly commas and/or decimal-points
        /// </summary>
        /// <param name="sInput">The string to check</param>
        /// <returns>true if the input IS composed only of digits, periods, and commas</returns>
        public static bool IsOnlyDigitsPeriodsAndCommas( this string sInput )
        {
            bool r = true;
            //bool bDecPoint = false;
            //bool bComma = false;
            if (sInput.Length > 0)
            {
                foreach (char ch in sInput)
                {
                    if (!Char.IsDigit( ch ))
                    {
                        if (ch == '.')
                        {
                            // Flag that we've already seen the decimal-point
                            //bDecPoint = true;
                        }
                        else if (ch == ',')
                        {
                            //bComma = true;
                        }
                        else
                        {
                            r = false;
                            break;
                        }
                    }
                }
            }
            return r;
        }
        #endregion

        #region Join
        /// <summary>
        /// Given a bit of "glue text", append the left and right strings together with the glue-text in-between.
        /// For example, " ".Join("Left", "Right) returns "Left Right".
        /// </summary>
        /// <param name="sGlue">the text to insert between the other parts. This may be an empty string, but not null.</param>
        /// <param name="sLeft">the left-string to concatenate on the left</param>
        /// <param name="sRight">the right-string, to concatenate to the right of sGlue</param>
        /// <returns>sLeft concatenated with sGlue concatenated with sRight</returns>
        /// <exception cref="ArgumentNullException">
        /// Throws an ArgumentNullException if sGlue is null.
        /// </exception>
        /// <remarks>
        /// If sGlue is an empty string, then sLeft is simply concatenated with sRight.
        /// If sLeft is null or empty, then sRight is returned and sGlue is ignored.
        /// If sRight is null or empty, then sLeft is returned and sGlue is ignored.
        /// </remarks>
        public static string Join2( this string sGlue, string sLeft, string sRight )
        {
            //CBL: This method may be entirely bogus -- the String class already has a Join method.
            if (sGlue == null)
            {
                throw new ArgumentNullException( "sGlue" );
            }
            if (String.IsNullOrEmpty( sRight ))
            {
                if (String.IsNullOrEmpty( sLeft ))
                {
                    return String.Empty;
                }
                else
                {
                    return sLeft;
                }
            }
            else if (String.IsNullOrEmpty( sLeft ))
            {
                if (String.IsNullOrEmpty( sRight ))
                {
                    return String.Empty;
                }
                else
                {
                    return sRight;
                }
            }
            return sLeft + sGlue + sRight;
        }
        #endregion

        #region Matches
        /// <summary>
        /// Indicate whether the given string (1st parameter) matches the given pattern (2nd parameter),
        /// ignoring case and common syntactical differences in spelling.
        /// </summary>
        /// <param name="sourceText">The given string to match the pattern against</param>
        /// <param name="pattern">The pattern string to test our source string against</param>
        /// <returns>true if they match, false otherwise</returns>
        /// <remarks>
        /// The comparison is case-insensitive.
        /// If either is null, then this returns true if they are both null, false if only one is null.
        /// Leading and trailing whitespace, and also hyphens, are removed for the comparison.
        /// If the pattern is a match except that it has an extra letter 's' suffixed - true if returned.
        /// If pattern is digit zero ('0'), then true is returned if sourceText is 0, 0.0, 0.0F, or 0F.
        /// </remarks>
        public static bool Matches( this string sourceText, string pattern )
        {
            if (pattern == null)
            {
                if (sourceText == null)
                {
                    return true;
                }
                return false;
            }
            if (sourceText.Equals( pattern, StringComparison.OrdinalIgnoreCase ))
            {
                return true;
            }
            else
            {
                string sSource2 = sourceText.Trim().Replace( "-", "" );
                string sPattern2 = pattern.Trim().Replace( "-", "" );
                if (sSource2.Equals( sPattern2, StringComparison.OrdinalIgnoreCase ))
                {
                    return true;
                }
                else if (sSource2.Equals( sPattern2 + "s", StringComparison.OrdinalIgnoreCase ))
                {
                    return true;
                }
                else
                {
                    // Match 0 against 0.0, 0F, 0.0F, and "zero"
                    if (pattern.Equals( "0" ))
                    {
                        if (sourceText.Equals( "0" ) || sourceText.Equals( "0.0" ) || sourceText.Equals( "0.0F" ) || sourceText.Equals( "0F" ) || sourceText.Equals( "0M" ))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// Indicate whether the given string (1st parameter) matches either of the given patterns (2nd and 3rd parameters),
        /// ignoring case and common syntactical differences in spelling.
        /// </summary>
        /// <param name="sourceText">The given string to match against the patterns</param>
        /// <param name="pattern1">a pattern string to test our source against</param>
        /// <param name="pattern2">another pattern string to test our source against</param>
        /// <returns>true if there's a match, false otherwise</returns>
        public static bool Matches( this string sourceText, string pattern1, string pattern2 )
        {
            return Matches( sourceText, pattern1 ) || Matches( sourceText, pattern2 );
        }

        /// <summary>
        /// Indicate whether the given string matches either of the given patterns (the subsequent),
        /// ignoring case and common syntactical differences in spelling.
        /// </summary>
        /// <param name="sourceText">The given string to match against the patterns</param>
        /// <param name="pattern1">a pattern string to test our source against</param>
        /// <param name="pattern2">another pattern string to test our source against</param>
        /// <param name="pattern3">another pattern string to test our source against</param>
        /// <returns>true if there's a match, false otherwise</returns>
        public static bool Matches( this string sourceText, string pattern1, string pattern2, string pattern3 )
        {
            return Matches( sourceText, pattern1, pattern2 ) || Matches( sourceText, pattern3 );
        }

        /// <summary>
        /// Indicate whether the given string matches either of the given patterns (the subsequent),
        /// ignoring case and common syntactical differences in spelling.
        /// </summary>
        /// <param name="sourceText">The given string to match against the patterns</param>
        /// <param name="pattern1">a pattern string to test our source against</param>
        /// <param name="pattern2">another pattern string to test our source against</param>
        /// <param name="pattern3">another pattern string to test our source against</param>
        /// <param name="pattern4">another pattern string to test our source against</param>
        /// <returns>true if there's a match, false otherwise</returns>
        public static bool Matches( this string sourceText, string pattern1, string pattern2, string pattern3, string pattern4 )
        {
            return Matches( sourceText, pattern1, pattern2 ) || Matches( sourceText, pattern3, pattern4 );
        }

        /// <summary>
        /// Indicate whether the given string matches either of the given patterns (the subsequent),
        /// ignoring case and common syntactical differences in spelling.
        /// </summary>
        /// <param name="sourceText">The given string to match against the patterns</param>
        /// <param name="pattern1">a pattern string to test our source against</param>
        /// <param name="pattern2">another pattern string to test our source against</param>
        /// <param name="pattern3">another pattern string to test our source against</param>
        /// <param name="pattern4">another pattern string to test our source against</param>
        /// <param name="pattern5">another pattern string to test our source against</param>
        /// <returns>true if there's a match, false otherwise</returns>
        public static bool Matches( this string sourceText, string pattern1, string pattern2, string pattern3, string pattern4, string pattern5 )
        {
            return Matches( sourceText, pattern1, pattern2 ) || Matches( sourceText, pattern3, pattern4, pattern5 );
        }
        #endregion

        #region NumberOf
        /// <summary>
        /// Return the number of ocurrances of the given character.
        /// </summary>
        /// <param name="sSource">The source-string to test for the given character</param>
        /// <param name="characterToTestFor">The character to scan the source-string for</param>
        /// <returns>the number of ocurrances of that character within the given string</returns>
        public static int NumberOf( this string sSource, char characterToTestFor )
        {
            if (HasSomething( sSource ))
            {
                string sourceLowercase = sSource.ToLower();
                char charToTestForLowercase = Char.ToLower( characterToTestFor );
                int n = 0;
                for (int i = 0; i < sourceLowercase.Length; i++)
                {
                    if (sourceLowercase[i] == charToTestForLowercase)
                    {
                        n++;
                    }
                }
                return n;
            }
            else
            {
                if (sSource == null)
                {
                    throw new ArgumentNullException( "sSource" );
                }
                else
                {
                    return 0;
                }
            }
        }
        #endregion

        #region NumberOfLeadingSpaces
        /// <summary>
        /// Return the number of space characters that begin the given string, if any.
        /// </summary>
        /// <param name="sSource">The source-string to test for spaces</param>
        /// <returns>the number of leading spaces</returns>
        public static int NumberOfLeadingSpaces( this string sSource )
        {
            if (!String.IsNullOrEmpty( sSource ))
            {
                int n = 0;
                for (int i = 0; i < sSource.Length; i++)
                {
                    if (sSource[i] == ' ')
                    {
                        n = i + 1;
                    }
                    else
                    {
                        break;
                    }
                }
                return n;
            }
            else
            {
                return 0;
            }
        }
        #endregion

        #region PadLineToLength
        /// <summary>
        /// Given a <c>StringBuilder</c> - append spaces to it to make it be of the given <paramref name="desiredLength"/>, if its length is less than that.
        /// If it contains multiple lines, then this padding is applied only to the last line.
        /// </summary>
        /// <param name="stringBuilder">the <c>StringBuilder</c> to append space characters to, if necessary</param>
        /// <param name="desiredLength">the desired number of characters to have in the <c>StringBuilder</c> - or within its' last line</param>
        /// <returns>a reference to this <c>StringBuilder</c> such that you may chain these method-calls</returns>
        /// <exception cref="ArgumentNullException">The argument supplied for the <c>StringBuilder</c> must not be null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">The argument supplied for the <paramref name="desiredLength"/> must not be negative.</exception>
        /// <remarks>
        /// This is intended for formatting text for reports, wherein columns are aligned using spaces as opposed to tabs.
        /// For example, if you append a number to a <c>StringBuilder</c>, and then want to begin the next column at the 60th character-position,
        /// you could do:
        /// <code>
        /// sb.Append(myNumber).PadLineToLength(60).Append(anotherNumber);
        /// </code>
        /// If the <c>StringBuilder</c> contains multiple lines of text, then this padding only applies to the final line.
        /// </remarks>
        public static StringBuilder PadLineToLength( this StringBuilder stringBuilder, int desiredLength )
        {
            if (stringBuilder == null)
            {
                throw new ArgumentNullException( "stringBuilder" );
            }
            if (desiredLength < 0)
            {
#if PRE_4
                throw new ArgumentOutOfRangeException( "desiredLength", desiredLength, "Argument value for desiredLength must not be negative" );
#else
                throw new ArgumentOutOfRangeException( paramName: nameof( desiredLength ), actualValue: desiredLength, message: "Argument value for desiredLength must not be negative" );
#endif
            }

            string s = stringBuilder.ToString();
            if (s.Contains( Environment.NewLine ))
            {
                int index = s.LastIndexOf( Environment.NewLine );
                string partAfterLastNewline = s.Substring( index + (Environment.NewLine.Length) );

                if (partAfterLastNewline.Length < desiredLength)
                {
                    int additionalNeeded = desiredLength - partAfterLastNewline.Length;
                    stringBuilder.Append( Spaces( additionalNeeded ) );
                }
            }
            else
            {
                if (stringBuilder.Length < desiredLength)
                {
                    int additionalNeeded = desiredLength - stringBuilder.Length;
                    stringBuilder.Append( Spaces( additionalNeeded ) );
                }
            }
            return stringBuilder;
        }
        #endregion

        #region PadToCenter
        /// <summary>
        /// return the given string padded on both sides with spaces to make it the desired length,
        /// leaving the given non-space part of the original string roughly centered.
        /// </summary>
        /// <param name="toWhat">the string to be padded with spaces</param>
        /// <param name="desiredLength">the length to make the resulting string</param>
        /// <returns>the padded result - of the desired length</returns>
        /// <remarks>
        /// If there are an odd number of spaces that need to be added to yield the desired length,
        /// then one additional space is added to the end relative to what is added to the start.
        /// 
        /// If the desired length is LESS than that of the given string,
        /// that string is returned truncated to the desired length.
        /// </remarks>
        /// <exception cref="ArgumentNullException">the given string must not be null</exception>
        public static string PadToCenter( this string toWhat, int desiredLength )
        {
            if (toWhat == null)
            {
                throw new ArgumentNullException( "toWhat" );
            }
            if (desiredLength == 0)
            {
                return String.Empty;
            }
            string trimmedInput = toWhat.Trim();
            if (desiredLength == trimmedInput.Length)
            {
                return trimmedInput;
            }
            if (desiredLength < trimmedInput.Length)
            {
                return trimmedInput.Substring( 0, desiredLength );
            }
            if (trimmedInput.Length == 0)
            {
                return Spaces( desiredLength );
            }
            // At this point we know that desiredLength is > the string.
            int amountToAdd = desiredLength - trimmedInput.Length;
            // For even number of spaces to add..
            if (amountToAdd % 2 == 0)
            {

                string spaces = Spaces( amountToAdd / 2 );
                string result = spaces + trimmedInput + spaces;
                return result;
            }
            // For odd number of spaces to add..
            int numberOfSpacesOnLeft = amountToAdd / 2;
            string spacesOnLeft = Spaces( numberOfSpacesOnLeft );
            int numberOfSpacesOnRight = amountToAdd - numberOfSpacesOnLeft;
            string spacesOnRight = Spaces( numberOfSpacesOnRight );
            return spacesOnLeft + trimmedInput + spacesOnRight;
        }
        #endregion

        #region PartAfter
        /// <summary>
        /// Return that part of a string that comes after the first occurrance of a given string pattern, if found,
        /// otherwise return String.Empty.
        /// </summary>
        /// <param name="containingText">the given string for which you want to extract a part and return it</param>
        /// <param name="pattern">the text-pattern that comes before the part that we want to return</param>
        /// <returns>the portion of containingText that comes after pattern, or String.Empty if pattern is not found</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <remarks>
        /// If pattern is null or an empty string, the result would be indeterminant - so ArgumentNullException or ArgumentException are thrown.
        /// </remarks>
        public static string PartAfter( this string containingText, string pattern )
        {
            if (pattern == null)
            {
                throw new ArgumentNullException( "pattern" );
            }
            if (pattern == String.Empty)
            {
                throw new ArgumentException( "pattern must not be an empty string" );
            }
            string result = String.Empty;
            if (HasSomething( containingText ))
            {
                string containingTextLowercase = containingText.ToLower();
                string patternLowercase = pattern.ToLower();
                int indexOfPattern = containingTextLowercase.IndexOf( patternLowercase );
                if (indexOfPattern != -1)
                {
                    result = containingText.Substring( indexOfPattern + pattern.Length );
                }
            }
            return result;
        }

        /// <summary>
        /// Return that part of a string that comes after the first occurrance of a given character pattern, if found,
        /// otherwise return String.Empty.
        /// </summary>
        /// <param name="containingText">the given string for which you want to extract a part and return it</param>
        /// <param name="charPattern">the character that comes before the part that we want to return</param>
        /// <returns>the portion of containingText that comes after charPattern, or String.Empty if that character is not found</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <remarks>
        /// If pattern is null or an empty string, the result would be indeterminant - so ArgumentNullException or ArgumentException are thrown.
        /// </remarks>
        public static string PartAfter( this string containingText, char charPattern )
        {
            return PartAfter( containingText, charPattern.ToString() );
        }
        #endregion

        #region PartAfterLast
        /// <summary>
        /// Return that part of a string that comes after the last occurrance of a given string pattern, if found,
        /// otherwise return String.Empty.
        /// </summary>
        /// <param name="containingText">the given string for which you want to extract a part and return it</param>
        /// <param name="pattern">the text-pattern that comes before the part that we want to return</param>
        /// <param name="isCaseSensitive">this dictates whether to regard the pattern as case-sensitive</param>
        /// <returns>the portion of containingText that comes after pattern, or String.Empty if pattern is not found</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentException"/>
        /// <remarks>
        /// If pattern is null or an empty string, the result would be indeterminant - so ArgumentNullException or ArgumentException are thrown.
        /// </remarks>
        public static string PartAfterLast( this string containingText, char pattern, bool isCaseSensitive )
        {
            string result = String.Empty;
            if (HasSomething( containingText ))
            {
                if (isCaseSensitive)
                {
                    for (int i = containingText.Length - 1; i >= 0; i--)
                    {
                        if (containingText[i] == pattern)
                        {
                            result = containingText.Substring( i + 1 );
                            break;
                        }
                    }
                }
                else
                {
                    string containingTextLowercase = containingText.ToLower();
                    char patternLowercase = Char.ToLower( pattern );
                    for (int i = containingText.Length - 1; i >= 0; i--)
                    {
                        if (containingTextLowercase[i] == patternLowercase)
                        {
                            result = containingText.Substring( i + 1 );
                            break;
                        }
                    }
                }
            }
            return result;
        }
        #endregion

        #region PartBefore
        /// <summary>
        /// Return that part of a given string that comes before the given string patterns, if found,
        /// otherwise return String.Empty.
        /// </summary>
        /// <param name="containingText">the given string for which you want to extract a part and return it</param>
        /// <param name="pattern">the text-pattern which marks the spot past the end of the part of the text that we want to return</param>
        /// <returns>the part of containingText that comes between the pattern, or String.Empty if not found</returns>
        public static string PartBefore( this string containingText, string pattern )
        {
            if (containingText == null)
            {
                throw new ArgumentNullException( "containingText" );
            }
            if (pattern == null)
            {
                throw new ArgumentNullException( "pattern" );
            }
            if ( pattern == null)
            {
                throw new ArgumentNullException( "pattern" );
            }
            string result = String.Empty;
            if (HasSomething( containingText ))
            {
                string containingTextLowercase = containingText.ToLower();
                string patternLowercase = pattern.ToLower();
                int indexOfPattern = containingTextLowercase.IndexOf( patternLowercase );
                if (indexOfPattern != -1)
                {
                    result = containingText.Substring( 0, indexOfPattern );
                }
            }
            return result;
        }

        /// <summary>
        /// Return that part of a given string that comes before the given character-patterns, if found,
        /// otherwise return String.Empty.
        /// </summary>
        /// <param name="containingText">the given string for which you want to extract a part and return it</param>
        /// <param name="charPattern">the single-character pattern which marks the spot past the end of the part of the text that we want to return</param>
        /// <returns>the part of containingText that comes before the charPattern, or String.Empty if not found</returns>
        public static string PartBefore( this string containingText, char charPattern )
        {
            return PartBefore( containingText, charPattern.ToString() );
        }
        #endregion

        #region PartBetween
        /// <summary>
        /// Return that part of a given string that comes between two string patterns, if found,
        /// otherwise return String.Empty.
        /// </summary>
        /// <param name="containingText">the given string for which you want to extract a part and return it</param>
        /// <param name="pattern1">the first text-pattern that comes before the part that we want to return</param>
        /// <param name="pattern2">the second text-pattern that comes after the part that we want to return</param>
        /// <param name="includeDelimiters">if true - the returned string contains the end-patterns, assuming they're 1 character each</param>
        /// <returns>the part of containingText that comes between the two patterns, or String.Empty if not found</returns>
#if !PRE_4
        public static string PartBetween( this string containingText, string pattern1, string pattern2, bool includeDelimiters = false )
        {
            string result = String.Empty;
            if (HasSomething( containingText ))
            {
                string containingTextLowercase = containingText.ToLower();
                string pattern1Lowercase = pattern1.ToLower();
                string pattern2Lowercase = pattern2.ToLower();
                int indexOfFirstPattern = containingTextLowercase.IndexOf( pattern1Lowercase );
                if (indexOfFirstPattern != -1)
                {
                    int endOfPattern1 = indexOfFirstPattern + pattern1.Length;
                    int indexOfSecondPattern = containingTextLowercase.IndexOf( pattern2Lowercase, endOfPattern1 );
                    if (indexOfSecondPattern != -1)
                    {
                        if (includeDelimiters)
                        {
                            int indexOfPart = indexOfFirstPattern + pattern1.Length - 1;
                            int lengthOfPart = indexOfSecondPattern - indexOfPart + 1;
                            result = containingText.Substring( indexOfPart, lengthOfPart );
                        }
                        else
                        {
                            int indexOfPart = indexOfFirstPattern + pattern1.Length;
                            int lengthOfPart = indexOfSecondPattern - indexOfPart;
                            result = containingText.Substring( indexOfPart, lengthOfPart );
                        }
                    }
                }
            }
            return result;
        }
#else
        public static string PartBetween( this string containingText, string pattern1, string pattern2, bool includeDelimiters )
        {
            string result = String.Empty;
            if (HasSomething( containingText ))
            {
                string containingTextLowercase = containingText.ToLower();
                string pattern1Lowercase = pattern1.ToLower();
                string pattern2Lowercase = pattern2.ToLower();
                int indexOfFirstPattern = containingTextLowercase.IndexOf( pattern1Lowercase );
                if (indexOfFirstPattern != -1)
                {
                    int endOfPattern1 = indexOfFirstPattern + pattern1.Length;
                    int indexOfSecondPattern = containingTextLowercase.IndexOf( pattern2Lowercase, endOfPattern1 );
                    if (indexOfSecondPattern != -1)
                    {
                        if (includeDelimiters)
                        {
                            int indexOfPart = indexOfFirstPattern + pattern1.Length - 1;
                            int lengthOfPart = indexOfSecondPattern - indexOfPart + 1;
                            result = containingText.Substring( indexOfPart, lengthOfPart );
                        }
                        else
                        {
                            int indexOfPart = indexOfFirstPattern + pattern1.Length;
                            int lengthOfPart = indexOfSecondPattern - indexOfPart;
                            result = containingText.Substring( indexOfPart, lengthOfPart );
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Return that part of a given string that comes between two string patterns, if found,
        /// otherwise return String.Empty.
        /// </summary>
        /// <param name="containingText">the given string for which you want to extract a part and return it</param>
        /// <param name="pattern1">the first text-pattern that comes before the part that we want to return</param>
        /// <param name="pattern2">the second text-pattern that comes after the part that we want to return</param>
        /// <returns>the part of containingText that comes between the two patterns, or String.Empty if not found</returns>
        public static string PartBetween( this string containingText, string pattern1, string pattern2 )
        {
            string result = String.Empty;
            if (HasSomething( containingText ))
            {
                string containingTextLowercase = containingText.ToLower();
                string pattern1Lowercase = pattern1.ToLower();
                string pattern2Lowercase = pattern2.ToLower();
                int indexOfFirstPattern = containingTextLowercase.IndexOf( pattern1Lowercase );
                if (indexOfFirstPattern != -1)
                {
                    int endOfPattern1 = indexOfFirstPattern + pattern1.Length;
                    int indexOfSecondPattern = containingTextLowercase.IndexOf( pattern2Lowercase, endOfPattern1 );
                    if (indexOfSecondPattern != -1)
                    {
                        int indexOfPart = indexOfFirstPattern + pattern1.Length;
                        int lengthOfPart = indexOfSecondPattern - indexOfPart;
                        result = containingText.Substring( indexOfPart, lengthOfPart );
                    }
                }
            }
            return result;
        }
#endif
        #endregion

        #region RemoveAll
        /// <summary>
        /// Return this string with all occurrences of the given character removed.
        /// </summary>
        /// <param name="sThis">this string, from which we want to remove characters</param>
        /// <param name="charToRemove">the character to remove</param>
        /// <returns>a copy of the given string with all ocurrences of the indicated character removed</returns>
        public static string RemoveAll( this string sThis, char charToRemove )
        {
            //public static string RemoveSpecialCharacters(string str)
            if (sThis.IndexOf( charToRemove ) > -1)
            {
                var sb = new StringBuilder();
                foreach (char c in sThis)
                {
                    //if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') | || (c >= 'a' && c <= 'z') | c == '.' || c == '_')
                    if (c != charToRemove)
                    {
                        sb.Append( c );
                    }
                }
                return sb.ToString();
            }
            else
            {
                return sThis;
            }
        }

        /// <summary>
        /// Return this string with all occurrences of the given characters removed.
        /// </summary>
        /// <param name="sThis">this string, from which we want to remove characters</param>
        /// <param name="charactersToRemove">an array containing the characters to remove</param>
        /// <returns>a copy of the given string with all occurrences of the indicated characters removed</returns>
        public static string RemoveAll( this string sThis, char[] charactersToRemove )
        {
            if (sThis.IndexOfAny( charactersToRemove ) > -1)
            {
                var sb = new StringBuilder();
                foreach (char c in sThis)
                {
                    if (!charactersToRemove.Contains( c ))
                    {
                        sb.Append( c );
                    }
                }
                return sb.ToString();
            }
            else
            {
                return sThis;
            }
        }
        #endregion RemoveAll

        #region Shortened
        /// <summary>
        /// Return the given string limited (if necessary) to the indicated length.
        /// If it's longer than that, then it's shortened and an ellipsis suffixed to it.
        /// </summary>
        /// <param name="text">The string that we want to limit in length</param>
        /// <param name="desiredMaxLength">The maximum length that we want it to be</param>
        /// <returns>The input text (sText) appropriately shortened (and the ellipsis applied if need be)</returns>
        public static string Shortened( this string text, int desiredMaxLength )
        {
            string sResult = "";
            if (!String.IsNullOrEmpty( text ))
            {
                string sInput = text.Trim().Replace( '\n', ' ' ).Replace( '\r', ' ' );
                int iLength = sInput.Length;
                if (iLength <= desiredMaxLength)
                {
                    sResult = sInput;
                }
                else
                {
                    if (desiredMaxLength > 2)
                    {
                        string sPartToShow = sInput.Substring( 0, desiredMaxLength - 2 );
                        sResult = sPartToShow + "..";
                    }
                    else
                    {
                        sResult = sInput.Substring( 0, desiredMaxLength );
                    }
                }
            }
            return sResult;
        }
        #endregion

        #region SmallSpace
        /// <summary>
        /// For the "small" space character, this is the Unicode "Six-Per-EM" character, Unicode U+2006. It is more narrow than a regular "space"-character.
        /// </summary>
        public const char SmallSpace = '\u2006';

        #endregion

        #region Spaces
        /// <summary>
        /// Return a string of space-characters that is of the specified length.
        /// </summary>
        /// <param name="desiredLength">the number of spaces to include within the result</param>
        /// <returns>a string consisting only of spaces</returns>
        public static string Spaces( int desiredLength )
        {
            if (desiredLength < 0)
            {
#if PRE_4
                throw new ArgumentOutOfRangeException( "desiredLength", desiredLength, "desiredLength must not be negative" );
#else
                throw new ArgumentOutOfRangeException( paramName: nameof( desiredLength ), actualValue: desiredLength, message: "desiredLength must not be negative" );
#endif
            }
            if (desiredLength == 0)
            {
                return String.Empty;
            }
            var sb = new StringBuilder( desiredLength );
            for (int i = 0; i < desiredLength; i++)
            {
                sb.Append( " " );
            }
            return sb.ToString();
        }
        #endregion

        #region SplitQuoted
        /// <summary>
        /// Split a string, dealing correctly with quoted items. Double-quotes are assumed to denote quoted items.
        /// This simply calls SplitQuoted but with a space for the separator.
        /// This is s
        /// </summary>
        /// <param name="sText">the text string to split</param>
        /// <returns>an array of strings that represents the items found within the text</returns>
        /// <remarks>
        /// This is only provided to maintain compatibility with older versions of .NET
        /// </remarks>
        public static string[] SplitQuoted( string sText )
        {
            return SplitQuoted( sText, " ", null );
        }

        /// <summary>
        /// Split a string, dealing correctly with quoted items.
        /// The quotes parameter is the character pair used to quote strings
        /// (default is "", the double-quote).
        /// You can also use a character pair (eg "{}") if the opening and closing quotes are different.
        /// </summary>
        /// <remarks>
        /// For example, you can split this:
        /// string[] fields = SplitQuoted("[one,two],three,[four,five]",,"[]")
        /// into 3 items, because commas inside [] are not taken into account.
        /// Multiple separators are ignored, so splitting "a,,b" using a comma as the separator
        /// will return two fields, not three. To get this behavior, you could use ", " (comma and space)
        /// as separators and default quotes,
        /// then set the string to something like 'a,"",b' to get the empty field.
        /// You could also use comma as *only separator and put a space to get a space field like'a, ,b'.
        /// </remarks>
        /// <param name="sText">text to split</param>
        /// <param name="sSeparators">The separator char(s) as a string</param>
        /// <param name="sQuotes">The char pair used to quote a string</param>
        /// <returns>An array of strings</returns>
        /// <exception cref="ArgumentNullException"/>
        public static string[] SplitQuoted( string sText, string sSeparators, string sQuotes )
        {
            // Default separators is a space and tab (e.g. "\t")
            // All separators not inside quote pair are ignored
            // Default quotes pair is two double quotes (e.g. '""""')
            if (sText == null)
            {
                throw new ArgumentNullException( "sText" );
            }
            if (String.IsNullOrEmpty( sSeparators ))
            {
                sSeparators = "\t";
            }
            if (String.IsNullOrEmpty( sQuotes ))
            {
                sQuotes = "\"\"";
            }
            List<string> res = new List<string>();

            // Get the open and close chars, escape them for use in regular expressions.
            string openChar = Regex.Escape( sQuotes[0].ToString() );
            string closeChar = Regex.Escape( sQuotes[sQuotes.Length - 1].ToString() );

            // Build the pattern that searches for both quoted and unquoted elements.
            // Notice that the quoted element is defined by group #2
            // and the unquoted elment is defined by group #3.
            string sPattern = @"\s*(" + openChar + "([^" + closeChar + "]*)" +
                    closeChar + @"|([^" + sSeparators + @"]+))\s*";

            // Search the string.
            foreach (Match m in Regex.Matches( sText, sPattern ))
            {
                string g3 = m.Groups[3].Value;
                if (g3.Length > 0)
                    res.Add( g3 );
                else
                {
                    // Get the quoted string, but without the quotes.
                    res.Add( m.Groups[2].Value );
                }
            }
            return res.ToArray();
        }
        #endregion

        #region StartsWith
#if SILVERLIGHT
        /// <summary>
        /// Return true if the first portion of thisString is the same as prefix.
        /// This is built-in with the full .NET Framework, but unavailable with Silverlight.
        /// </summary>
        /// <param name="thisString">The string in question</param>
        /// <param name="prefix">A string that we want to test whether is the same as the beginning of thisString</param>
        /// <param name="isIgnoringCase">Dictates whether to ignore capitalization in the comparison</param>
        /// <returns>true if thisString includes prefix in it's beginning</returns>
        public static bool StartsWith(this string thisString, string prefix, bool isIgnoringCase)
        {
            // The full .NET Framework has StartsWith, and we could simply return, for example: trimmedText.EndsWith("hex", true, CultureInfo.InvariantCulture)
#if DEBUG
            if (thisString == null)
            {
                throw new ArgumentNullException("thisString");
            }
            if (prefix == null)
            {
                throw new ArgumentNullException("prefix");
            }
#endif
            int lengthOfThisString = thisString.Length;
            int lengthOfPrefix = prefix.Length;

            if (lengthOfPrefix > lengthOfThisString)
            {
                return false;
            }

            string pertinentPartOfThisString;
            if (lengthOfPrefix == lengthOfThisString)
            {
                pertinentPartOfThisString = thisString;
            }
            else
            {
                pertinentPartOfThisString = thisString.Substring(0, lengthOfPrefix);
            }
            // At this point we know that pertinentPartOfThisString is the same length as prefix.
            if (isIgnoringCase)
            {
                return prefix.Equals(pertinentPartOfThisString, StringComparison.InvariantCultureIgnoreCase);
            }
            else
            {
                return prefix.Equals(pertinentPartOfThisString, StringComparison.InvariantCulture);
            }
        }
#endif
        #endregion StartsWith

        #region TestStringOfLength
        /// <summary>
        /// Return a string consisting of digits 0 through 9, repeated as many times as needed to make it the specified length.
        /// </summary>
        /// <remarks>
        /// This is envisioned as being for testing utilities that take a string input.
        /// </remarks>
        /// <param name="length">The length (in characters) of the string to return</param>
        /// <returns>A string of the given length, consisting of repeating digits</returns>
        public static string TestStringOfLength( int length )
        {
            var sb = new StringBuilder( length );
            int digit = 0;
            for (int i = 0; i < length; i++)
            {
                sb.Append( digit.ToString() );
                digit++;
                if (digit > 9)
                {
                    digit = 0;
                }
            }
            return sb.ToString();
        }
        #endregion

        #region TestIfContainsAndRemove
        /// <summary>
        /// Check whether the given string contains the given word or it's plural, returning true if it does and also returning it with the test-pattern removed.
        /// The test is case-insensitive, and requires word-boundaries (ie, a word embedded within other letters is not matched).
        /// </summary>
        /// <param name="sThis">The given string to test against</param>
        /// <param name="sWord1">The pattern to check for</param>
        /// <param name="sWithout">If the pattern (or it's plural) is found within the given string, that string is returned in this argument with the pattern removed</param>
        /// <returns>true if the pattern (or it's plural) is found</returns>
        public static bool TestIfContainsAndRemove( this string sThis, string sWord1, out string sWithout )
        {
            return TestIfContainsAndRemove( sThis, sWord1, null, null, out sWithout );
        }

        /// <summary>
        /// Check whether the given string contains either of the given words or their plural, returning true if it does and also returning it with the test-pattern removed.
        /// The test is case-insensitive, and requires word-boundaries (ie, a word embedded within other letters is not matched).
        /// </summary>
        /// <param name="sThis">The given string to test against</param>
        /// <param name="sWord1">The first pattern to check for</param>
        /// <param name="sWord2">The second pattern to check for</param>
        /// <param name="sWithout">If the patterns(or their plural) are found within the given string, that string is returned in this argument with the pattern removed</param>
        /// <returns>true if either of the patterns (or plural forms) is found</returns>
        public static bool TestIfContainsAndRemove( this string sThis, string sWord1, string sWord2, out string sWithout )
        {
            return TestIfContainsAndRemove( sThis, sWord1, sWord2, null, out sWithout );
        }

        /// <summary>
        /// Check whether the given string contains any of the given words or their plural, returning true if it does and also returning it with the test-pattern removed.
        /// The test is case-insensitive, and requires word-boundaries (ie, a word embedded within other letters is not matched).
        /// </summary>
        /// <param name="sThis">The given string to test against</param>
        /// <param name="sWord1">The first pattern to check for</param>
        /// <param name="sWord2">The second pattern to check for</param>
        /// <param name="sWord3">The third pattern to check for</param>
        /// <param name="sWithout">If the patterns(or their plural) are found within the given string, that string is returned in this argument with the pattern removed from the end</param>
        /// <returns>true if any of the patterns (or plural forms) is found</returns>
        public static bool TestIfContainsAndRemove( this string sThis, string sWord1, string sWord2, string sWord3, out string sWithout )
        {
            bool bFound = false;
            string sWordFound;
            if (sThis.ContainsRoot( sWord1, out sWordFound ))
            {
                bFound = true;
                sWithout = sThis.WithoutAtEnd( sWordFound );
            }
            else if (!String.IsNullOrEmpty( sWord2 ) && sThis.ContainsRoot( sWord2, out sWordFound ))
            {
                bFound = true;
                sWithout = sThis.WithoutAtEnd( sWordFound );
            }
            else if (!String.IsNullOrEmpty( sWord3 ) && sThis.ContainsRoot( sWord3, out sWordFound ))
            {
                bFound = true;
                sWithout = sThis.WithoutAtEnd( sWordFound );
            }
            else
            {
                sWithout = sThis;
            }
            return bFound;
        }

        /// <summary>
        /// Check whether the given string contains any of the given words or their plural, returning true if it does and also returning it with the test-pattern removed.
        /// The test is case-insensitive, and requires word-boundaries (ie, a word embedded within other letters is not matched).
        /// </summary>
        /// <param name="thisText">The given string to test against</param>
        /// <param name="word1">The first pattern to check for</param>
        /// <param name="word2">The second pattern to check for</param>
        /// <param name="word3">The third pattern to check for</param>
        /// <param name="word4">The fourth pattern to check for</param>
        /// <param name="thisTextWithoutThat">If the patterns(or their plural) are found within the given string, that string is returned in this argument with the pattern removed from the end</param>
        /// <returns>true if any of the patterns (or plural forms) is found</returns>
        public static bool TestIfContainsAndRemove( this string thisText, string word1, string word2, string word3, string word4, out string thisTextWithoutThat )
        {
            bool wasFound = false;
            string sWordFound;
            if (thisText.ContainsRoot( word1, out sWordFound ))
            {
                wasFound = true;
                thisTextWithoutThat = thisText.WithoutAtEnd( sWordFound );
            }
            else if (!String.IsNullOrEmpty( word2 ) && thisText.ContainsRoot( word2, out sWordFound ))
            {
                wasFound = true;
                thisTextWithoutThat = thisText.WithoutAtEnd( sWordFound );
            }
            else if (!String.IsNullOrEmpty( word3 ) && thisText.ContainsRoot( word3, out sWordFound ))
            {
                wasFound = true;
                thisTextWithoutThat = thisText.WithoutAtEnd( sWordFound );
            }
            else if (!String.IsNullOrEmpty( word4 ) && thisText.ContainsRoot( word4, out sWordFound ))
            {
                wasFound = true;
                thisTextWithoutThat = thisText.WithoutAtEnd( sWordFound );
            }
            else
            {
                thisTextWithoutThat = thisText;
            }
            return wasFound;
        }
        #endregion // TestIfContainsAndRemove

        #region Titlecase
        /// <summary>
        /// This is an extension-method that returns the given string with the first character in uppercase, and the rest in lowercase.
        /// </summary>
        /// <param name="sText">the string to return as first-character-capitalized</param>
        /// <returns>the given string with only the first character capitalized</returns>
        /// <remarks>
        /// This method is named after the equivalent string function in Python.
        /// This IS completely unit-tested.
        /// </remarks>
        public static string Titlecase( this string sText )
        {
            string sResult = "";
            if (!String.IsNullOrEmpty( sText ))
            {
                string sFirstChar = sText.Substring( 0, 1 ).ToUpper();
                string sRest = sText.Substring( 1 ).ToLower();
                sResult = sFirstChar + sRest;
            }
            return sResult;
        }
        #endregion

        #region ToBoolean
        /// <summary>
        /// Given a string, return the boolean value that reflects the value denoted by that text (if applicable)
        /// or null if it fails to clearly say true or false.
        /// </summary>
        /// <param name="text"></param>
        /// <returns>a nullable-boolean value that indicates what the given <paramref name="text"/> denotes regarding true or false</returns>
        /// <remarks>
        /// This method returns a nullable-boolean so that it may use the null value to indicate neither true nor false.
        /// If the given <paramref name="text"/> is true, false, yes, no, 1, or 0 - then the corresponding boolean value is returned.
        /// If the given text is none of the above (that is, fails to clearly denote true or false), then <c>null</c> is returned.
        /// </remarks>
        /// <exception cref="ArgumentNullException">The given text must not be null.</exception>
        public static bool? ToBoolean( this string text )
        {
            if (text == null)
            {
                throw new ArgumentNullException( "text" );
            }
            if (HasNothing( text ))
            {
                return null;
            }
            string textLowercase = text.Trim().ToLowerInvariant();
            if (textLowercase.Equals( "true" ) || textLowercase.Equals( "yes" ) || textLowercase.Equals( "1" ))
            {
                return true;
            }
            else if (textLowercase.Equals( "false" ) || textLowercase.Equals( "no" ) || textLowercase.Equals( "0" ))
            {
                return false;
            }
            return null;
        }
        #endregion

        #region ToOrdinal
        /// <summary>
        /// Extension method that returns a string that expresses the given integer as an ordinal (eg, "1st").
        /// </summary>
        /// <param name="fromNumber">The integer to be expressed as an ordinal</param>
        /// <returns>The ordinal expression, eg "1st" for 1, "99th" for 99</returns>
        /// <exception cref="ArgumentException">The argument value must be non-negative</exception>
        public static string ToOrdinal( this int fromNumber )
        {
            if (fromNumber < 0)
            {
                throw new ArgumentException( "The argument value must not be negative", "fromNumber" );
            }
            string numberText = fromNumber.ToString();
            // Compute the position-text.
            string positionText;
            if (fromNumber >= 10 && numberText[numberText.Length - 2] == '1')
            {
                // teen numbers always end in 'th'
                positionText = "th";
            }
            else
            {
                switch (numberText[numberText.Length - 1])
                {
                    case '0':
                        positionText = "th";
                        break;
                    case '1':
                        positionText = "st";
                        break;
                    case '2':
                        positionText = "nd";
                        break;
                    case '3':
                        positionText = "rd";
                        break;
                    default:
                        positionText = "th";
                        break;
                }
            }
            // Append that to the number to yield the result.
            return numberText + positionText;
        }
        #endregion

        #region ToStringWithoutNuls
        /// <summary>
        /// Convert the given array of Chars, into a string - leaving off any zero characters.
        /// </summary>
        public static string ToStringWithoutNuls( this char[] sourceCharArray )
        {
            var newArray = (from c in sourceCharArray where c != 0 select c).ToArray();
            string stringResult = new string( newArray );
            return stringResult;
        }
        #endregion

        #region ToStringWithoutCommaBeforeParenthesis
        /// <summary>
        /// Return the string that this StringBuilder contains (same as calling ToString) except,
        /// if it ends with a right-parenthesis AND has a comma just before that, then that comma
        /// is removed.
        /// </summary>
        /// <param name="stringBuilder">the StringBuilder to get the string from</param>
        /// <returns>the result of stringBuilder.ToString(), except with any final comma removed</returns>
        /// <remarks>
        /// The purpose of this is to simplify the building of lists using StringBuilder, so that
        /// thought does not have to be put into not putting that final comma.
        /// 
        /// For example, when the StringBuilder contains "(one,two,three,)"
        /// this returns "(one,two,three)".
        /// </remarks>
        public static string ToStringWithoutCommaBeforeParenthesis( this StringBuilder stringBuilder )
        {
            if (stringBuilder == null)
            {
                throw new ArgumentNullException( "stringBuilder" );
            }
            string rawString = stringBuilder.ToString();
            if (rawString.EndsWith( ",)" ))
            {
                return rawString.Substring( 0, rawString.Length - 2 ) + ")";
            }
            else
            {
                return rawString;
            }
        }
        #endregion

        #region ToStringAndEndList
        /// <summary>
        /// Return the string that this StringBuilder contains (same as calling ToString) except,
        /// remove any final comma and append a right-parenthesis.
        /// </summary>
        /// <param name="stringBuilder">the StringBuilder to get the string from</param>
        /// <returns>the result of stringBuilder.ToString(), except with any final comma removed and a right-parenthesis added</returns>
        /// <remarks>
        /// The purpose of this is to simplify the building of lists using StringBuilder, so that
        /// thought does not have to be put into not putting that final comma.
        /// 
        /// For example, when the StringBuilder contains "(one,two,three,"
        /// this returns "(one,two,three)".
        /// </remarks>
        public static string ToStringAndEndList( this StringBuilder stringBuilder )
        {
            if (stringBuilder == null)
            {
                throw new ArgumentNullException( "stringBuilder" );
            }
            string rawString = stringBuilder.ToString().TrimEnd();
            if (rawString.EndsWith( "," ))
            {
                return rawString.Substring( 0, rawString.Length - 1 ) + ")";
            }
            else
            {
                return rawString + ")";
            }
        }
        #endregion

        #region TrimForXmlText
        /// <summary>
        /// Given a string that is assumed to contain the content for an XML document --
        /// return a copy of it with any of the leading-space from each line that would prevent it from being valid
        /// trimmed off.
        /// </summary>
        /// <param name="text">the raw XML-document text to trim space from</param>
        /// <returns>a string that has the XML document text with extraneous leading spaces trimmed off</returns>
        public static string TrimForXmlText( string text )
        {
            if (text == null)
            {
                throw new ArgumentNullException( "text" );
            }
            // Remove any leading new-lines and spaces,
            // and upon encountering the first substantive text (which would be the character '<'),
            // remember how many spaces preceeded it on that line,
            // and remove up to that same number of spaces from each succeeding line.
            var sb = new StringBuilder();
            int numberSpacesToStripFromEachLine = 0;

            bool hasSeenOpeningTag = false;
            // Replace any tabs with spaces.
            string textWithoutTabs = text.Replace( '\t', ' ' );

            // Go through the input-text line by line..
            using (StringReader sr = new StringReader( textWithoutTabs ))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (!hasSeenOpeningTag)
                    {
                        int indexOfOpeningTag = line.IndexOf( "<" );
                        if (indexOfOpeningTag > -1)
                        {
                            hasSeenOpeningTag = true;
                            string validPortion = line.Substring( indexOfOpeningTag );
                            numberSpacesToStripFromEachLine = indexOfOpeningTag;
                            sb.Append( validPortion );
                        }
                    }
                    else
                    {
                        sb.AppendLine();
                        int leadingSpaces = StringLib.NumberOfLeadingSpaces( line );
                        string validPortion;
                        if (numberSpacesToStripFromEachLine > leadingSpaces)
                        {
                            validPortion = line.Substring( leadingSpaces );
                        }
                        else
                        {
                            validPortion = line.Substring( numberSpacesToStripFromEachLine );
                        }
                        sb.Append( validPortion );
                    }
                }
            }
            return sb.ToString();
        }
        #endregion

        #region TrimFromEnd
        /// <summary>
        /// If this text ends with the given textToRemove, return a copy of this text with that removed
        /// - otherwise just return this text unchanged. The matching IS case-sensitive.
        /// </summary>
        /// <param name="fromWhat">this text to remove that from</param>
        /// <param name="textToRemove">that text to remove</param>
        /// <returns>this string with the given text removed from the end of it</returns>
        /// <exception cref="ArgumentNullException">the argument values must not be null</exception>
        public static string TrimFromEnd( this string fromWhat, string textToRemove )
        {
            if (fromWhat == null)
            {
                throw new ArgumentNullException( "fromWhat" );
            }
            if (fromWhat.EndsWith( textToRemove ))
            {
                return fromWhat.Substring( 0, fromWhat.Length - textToRemove.Length );
            }
            else
            {
                return fromWhat;
            }
        }

        /// <summary>
        /// If this text ends with the given textToRemove, return a copy of this text with that removed
        /// - otherwise just return this text unchanged. The matching is NOT case-sensitive.
        /// </summary>
        /// <param name="fromWhat">this text to remove that from</param>
        /// <param name="textToRemove">that text to remove</param>
        /// <returns>this string with the given text removed from the end of it</returns>
        /// <exception cref="ArgumentNullException">the argument values must not be null</exception>
        public static string TrimFromEndCaseInsensitve( this string fromWhat, string textToRemove )
        {
            if (fromWhat == null)
            {
                throw new ArgumentNullException( "fromWhat" );
            }
            if (textToRemove == null)
            {
                throw new ArgumentNullException( "textToRemove" );
            }
            if (fromWhat.ToUpper().EndsWith( textToRemove.ToUpper() ))
            {
                return fromWhat.Substring( 0, fromWhat.Length - textToRemove.Length );
            }
            else
            {
                return fromWhat;
            }
        }
        #endregion

        #region TrimSlashFromEndAndConvertToBackSlashes
        /// <summary>
        /// Return the given string with no back-slash on the end, unless it denotes a path root,
        /// and all forward-slashes are converted to back-slashes.
        /// </summary>
        /// <param name="inputText">the string to convert</param>
        /// <returns>the given path without a trailing slash, unless root</returns>
        public static string TrimSlashFromEndAndConvertToBackSlashes( this string inputText )
        {
            //UT: done
            if (inputText == null)
            {
                throw new ArgumentNullException( "inputText" );
            }
            if (HasNothing( inputText ))
            {
                return inputText;
            }

            string inputConvertedToBackSlashes = ConvertForwardSlashsToBackSlashs( inputText );
            int n = inputConvertedToBackSlashes.Length;
            // If it consists of only one slash,
            if (n == 1 && inputConvertedToBackSlashes[0] == '\\')
            {
                // then just return the one back-slash.
                return inputConvertedToBackSlashes;
            }
            // If it consists of only root-directory with drive ( "C:\" ),
            if (n == 3 && inputConvertedToBackSlashes[0].IsEnglishAlphabetLetter() && inputConvertedToBackSlashes[1] == ':' && inputConvertedToBackSlashes[2] == '\\')
            {
                // then just return it as-is, but changing the final forward-slash to a back-slash if needed.
                return inputConvertedToBackSlashes;
            }
            if (inputConvertedToBackSlashes[n - 1] == '\\')
            {
                return inputConvertedToBackSlashes.Substring( 0, n - 1 );
            }
            else
            {
                return inputConvertedToBackSlashes;
            }
        }
        #endregion

        #region TrimSlashesAndConvertToBackSlashes
        /// <summary>
        /// Return the given string with no back-slash on the beginning nor the end,
        /// and all forward-slashes are converted to back-slashes.
        /// </summary>
        /// <param name="inputText">the string to convert</param>
        /// <returns>the given path without a leading nor trailing slash</returns>
        public static string TrimSlashesAndConvertToBackSlashes( this string inputText )
        {
            //UT: done
            if (inputText == null)
            {
                throw new ArgumentNullException( "inputText" );
            }
            if (HasNothing( inputText ))
            {
                return inputText;
            }

            string inputConvertedToBackSlashes = ConvertForwardSlashsToBackSlashs( inputText );
            int n = inputConvertedToBackSlashes.Length;
            // If it consists of only one slash,
            if (n == 1)
            {
                if (inputConvertedToBackSlashes[0] == '\\')
                {
                    // then just return an empty string.
                    return String.Empty;
                }
                else
                {
                    return inputConvertedToBackSlashes;
                }
            }
            // 
            if (inputConvertedToBackSlashes[0] == '\\')
            {
                if (inputConvertedToBackSlashes[n - 1] == '\\')
                {
                    return inputConvertedToBackSlashes.Substring( 1, n - 2 );
                }
                else
                {
                    return inputConvertedToBackSlashes.Substring( 1 );
                }
            }
            if (inputConvertedToBackSlashes[n - 1] == '\\')
            {
                return inputConvertedToBackSlashes.Substring( 0, n - 1 );
            }
            else
            {
                return inputConvertedToBackSlashes;
            }
        }
        #endregion

        #region TryToParseToEnum
        /// <summary>
        /// Given a string value, attempt to determine which value of an enumeration-type it represents
        /// and return true if it does represent a value value of that enumeration-type, otherwise return false.
        /// This is more robust than Enum.TryParse in that it trims the string of spaces and is not case-sensitive,
        /// plus it is portable down to .NET 3.5
        /// </summary>
        /// <typeparam name="TEnum">this is the enumeration-type in question</typeparam>
        /// <param name="stringValue">the string value that is to be checked against the enumeration-type</param>
        /// <param name="resultEnumValue">this gets assigned the TEnum value that stringValue represents, or else is set to the default value</param>
        /// <returns>true if the stringValue is a valid value of TEnum</returns>
        public static bool TryToParseToEnum<TEnum>( this string stringValue, out TEnum resultEnumValue ) where TEnum : struct
        {
#if DEBUG
#if !NETFX_CORE
            if (!typeof( TEnum ).IsEnum)
            {
                throw new ArgumentException( "TEnum must be an enumeration type" );
            }
#endif
#endif
            if (!String.IsNullOrEmpty( stringValue ))
            {
                string value = stringValue.ToLower().Replace( " ", String.Empty );
                foreach (TEnum item in Enum.GetValues( typeof( TEnum ) ))
                {
                    if (item.ToString().ToLower().Equals( value ))
                    {
                        resultEnumValue = item;
                        return true;
                    }
                }
            }
            resultEnumValue = default( TEnum );
            return false;
        }
        #endregion

        #region TypeOfCasing, methods GetTypeOfCasing and PutIntoTypeOfCasing

        /// <summary>
        /// This enum is used to denote a simple letter-case pattern: Unknown, Mixed, AllLowercase, AllUppercase, or Titlecased.
        /// </summary>
        public enum TypeOfCasing
        {
            /// <summary>
            /// No indication of what the letter-casing is.
            /// </summary>
            Unknown,
            /// <summary>
            /// The letters are of mixed case, not corresponding to any simple pattern as denoted here.
            /// </summary>
            Mixed,
            /// <summary>
            /// All letters are in lower-case
            /// </summary>
            AllLowercase,
            /// <summary>
            /// All letters are in upper-case
            /// </summary>
            AllUppercase,
            /// <summary>
            /// Only the initial letter is upper-case
            /// </summary>
            Titlecased
        };

        /// <summary>
        /// Get an enum type value that indicates the capitalization scheme of the given string,
        /// that is, whether it's all uppercase letters, all lowercase letters, mixed, titlecased, or unknown.
        /// </summary>
        /// <param name="ofWhat">The text that we want to know the capitalization of</param>
        /// <returns>One of the enum TypeOfCasing values</returns>
        /// <exception cref="ArgumentException"/>
        /// <remarks>
        /// "Titlecase" means with only the initial letter in uppercase and the rest in lowercase,
        /// as it means in the Python language.
        /// </remarks>
        public static TypeOfCasing GetTypeOfCasing( this string ofWhat )
        {
            if (HasNothing( ofWhat ))
            {
                throw new ArgumentException( "ofWhat must not be empty" );
            }
            TypeOfCasing capitalization = TypeOfCasing.Unknown;
            if (ofWhat.IsAllLowercase())
            {
                capitalization = TypeOfCasing.AllLowercase;
            }
            else if (ofWhat.IsAllUppercase())
            {
                capitalization = TypeOfCasing.AllUppercase;
            }
            else if (ofWhat.IsInTitlecase())
            {
                capitalization = TypeOfCasing.Titlecased;
            }
            else
            {
                capitalization = TypeOfCasing.Mixed;
            }
            return capitalization;
        }

        /// <summary>
        /// Given a string and an enum type value that indicates the capitalization scheme desired for that string,
        /// return a string that is that value changed to match that capitalization scheme.
        /// </summary>
        /// <param name="what">the string that we want to return a (properly-cased) copy of</param>
        /// <param name="how">A TypeOfCasing value that indicates how we want to change the capitalization</param>
        /// <returns>a copy of the string cast into the given TypeOfCasing</returns>
        /// <exception cref="ArgumentException"/>
        public static string PutIntoTypeOfCasing( this string what, TypeOfCasing how )
        {
            //CBL This needs unit-tests!
            if (HasNothing( what ))
            {
                throw new ArgumentException( "what must not be empty" );
            }
            switch (how)
            {
                case TypeOfCasing.AllLowercase:
                    return what.ToLower();
                case TypeOfCasing.AllUppercase:
                    return what.ToUpper();
                case TypeOfCasing.Titlecased:
                    string sResult = "";
                    if (what.Length > 0)
                    {
                        string sFirstChar = what.Substring( 0, 1 ).ToUpper();
                        string sRest = what.Substring( 1 ).ToLower();
                        sResult = sFirstChar + sRest;
                    }
                    return sResult;
                default:
                    return what;
            }
        }
        #endregion The enum type TypeOfCasing, methods GetTypeOfCasing and PutIntoTypeOfCasing

        #region WhatComesAfterLastSlash
        /// <summary>
        /// If the given filesystem-path contains any path-separators (either a forward or a back-slash),
        /// return the part that comes after the final path-seperator, otherwise just return the argument unchanged.
        /// </summary>
        /// <param name="path">a string that denotes the given filesystem-path</param>
        /// <returns>the part of the path that comes after the last filesystem-separator</returns>
        public static string WhatComesAfterLastSlash( this string path )
        {
            int indexOfLastSlash = path.LastIndexOf( '\\' );
            if (indexOfLastSlash == -1)
            {
                indexOfLastSlash = path.LastIndexOf( '/' );
            }
            if (indexOfLastSlash == -1)
            {
                return path;
            }
            else
            {
                return path.Substring( indexOfLastSlash + 1 );
            }
        }
        #endregion

        #region WithFirstLetterCapitalized
        /// <summary>
        /// Extension-method that returns the given string with the first character of each word in uppercase, and the remainder left as-is.
        /// </summary>
        /// <param name="text">The string to return as first-character-capitalized</param>
        /// <returns>The given string with only the first character made to be uppercase</returns>
        public static string WithFirstLetterCapitalized( this string text )
        {
            string result = "";
            if (!String.IsNullOrEmpty( text ))
            {
                //CBL Set up a speed-test for this, and try some more efficient solutions. This might be rather expensive.
                char[] separators = { ' ' };
                var parts = text.Split( separator: separators, options: StringSplitOptions.RemoveEmptyEntries );
                if (parts.Length > 1)
                {
                    var sb = new StringBuilder();
                    foreach (string part in parts)
                    {
                        if (sb.Length > 0)
                        {
                            sb.Append( " " );
                        }
                        string firstChar = part.Substring( 0, 1 ).ToUpper();
                        string restOfIt = part.Substring( 1 );
                        sb.Append( firstChar ).Append( restOfIt );
                    }
                    result = sb.ToString();
                }
                else
                {
                    string firstChar = text.Substring( 0, 1 ).ToUpper();
                    string restOfIt = text.Substring( 1 );
                    result = firstChar + restOfIt;
                }
            }
            return result;
        }
        #endregion

        #region WithFirstLetterInLowercase
        /// <summary>
        /// Extension-method that returns the given string with the first character in lowercase, and the remainder left as-is.
        /// </summary>
        /// <param name="sText">The string to return a copy of with it's first letter in lowercase</param>
        /// <returns>The given string with only the first character made to be lowercase</returns>
        public static string WithFirstLetterInLowercase( this string sText )
        {
            string sResult = "";
            if (!String.IsNullOrEmpty( sText ))
            {
                string sFirstChar = sText.Substring( 0, 1 ).ToLower();
                string sRest = sText.Substring( 1 );
                sResult = sFirstChar + sRest;
            }
            return sResult;
        }
        #endregion

        #region WithFlowDocumentOuterTag  and  WithoutFlowDocumentOuterTag

        /// <summary>
        /// Given a string containing XAML markup, if it is enclosed within a FlowDocument tag
        /// strip off that tag and return the result. Otherwise return it as-is.
        /// </summary>
        /// <param name="sXAML">the XAML markup</param>
        /// <returns>the XAML markup (as a string) without the enclosing FlowDocument tag</returns>
        public static string WithoutFlowDocumentOuterTag( this string sXAML )
        {
            if (sXAML.StartsWith( s_StandardOuterTagStart ) && sXAML.EndsWith( s_StandardOuterTagEnd ))
            {
                return sXAML.WithoutAtStart( s_StandardOuterTagStart ).WithoutAtEnd( s_StandardOuterTagEnd );
            }
            else
            {
                return sXAML;
            }
        }

        /// <summary>
        /// Given a string containing XAML markup, enclose it within a FlowDocument tag and return the result.
        /// </summary>
        /// <param name="sXAML">the XAML markup</param>
        /// <returns>a new string with a FlowDocument tag enclosing the given XAML markup</returns>
        public static string WithFlowDocumentOuterTag( this string sXAML )
        {
            string sResult;
            if (!sXAML.StartsWith( s_StandardOuterTagStart ))
            {
                sResult = s_StandardOuterTagStart + sXAML;
            }
            else
            {
                sResult = sXAML;
            }
            if (!sXAML.EndsWith( s_StandardOuterTagEnd ))
            {
                sResult += s_StandardOuterTagEnd;
            }
            return sResult;
        }

        static string s_StandardOuterTagStart = "<FlowDocument PagePadding=\"5,0,5,0\" AllowDrop=\"True\" xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\">";
        static string s_StandardOuterTagEnd = "</FlowDocument>";

        #endregion

        #region WithinDoubleQuotes
        /// <summary>
        /// Return the given string surrounded by double-quotes. If what is null, represent it as "null".
        /// </summary>
        /// <param name="what">the string to return surrounded by double-quotes</param>
        /// <returns>the given string surrounded by double-quotes</returns>
        public static string WithinDoubleQuotes( this string what )
        {
            if (what == null)
            {
                return "\"null\"";
            }
            else
            {
                return "\"" + what + "\"";
            }
        }
        #endregion

        #region WithoutDoubleQuotes
        /// <summary>
        /// Return the given string, without the surrounding double-quotes if present, otherwise unchanged.
        /// </summary>
        /// <param name="what">the string to return</param>
        /// <returns>the given string without any double-quotes</returns>
        /// <remarks>
        /// This ALSO strips off any leading or trailing spaces.
        /// </remarks>
        public static string WithoutDoubleQuotes( this string what )
        {
            if (HasNothing( what ))
            {
                return String.Empty;
            }
            else
            {
                string trimmedValue = what.Trim();
                int len = trimmedValue.Length;
                if (len == 1 && trimmedValue[0] == '\"')
                {
                    return String.Empty;
                }
                else if (len == 2 && trimmedValue[0] == '\"' && trimmedValue[1] == '\"')
                {
                    return String.Empty;
                }
                else
                {
                    string result;
                    // For these next, I recurse back into this same method in order to deal with multiple layers of double-quotes.
                    if (len > 2 && trimmedValue[0] == '\"' && trimmedValue[len - 1] == '\"')
                    {
                        result = trimmedValue.Substring( 1, len - 2 ).Trim().WithoutDoubleQuotes();
                    }
                    else if (len > 2 && trimmedValue[0] == '\"')
                    {
                        result = trimmedValue.Substring( 1, len - 1 ).Trim().WithoutDoubleQuotes();
                    }
                    else if (len > 2 && trimmedValue[len - 1] == '\"')
                    {
                        result = trimmedValue.Substring( 0, len - 1 ).Trim().WithoutDoubleQuotes();
                    }
                    else
                    {
                        result = trimmedValue;
                    }
                    return result;
                }
            }
        }
        #endregion

        #region WithoutSingleQuotes
        /// <summary>
        /// Return the given string, without the surrounding quotes if present, otherwise unchanged.
        /// </summary>
        /// <param name="what">the string to return</param>
        /// <returns>the given string without any single-quotes</returns>
        public static string WithoutSingleQuotes( this string what )
        {
            string result = String.Empty;
            if (what != null)
            {
                int len = what.Length;
                if (len == 2 && what[0] == '\'' && what[1] == '\'')
                {
                }
                else
                {
                    if (len > 2 && what[0] == '\'' && what[len - 1] == '\'')
                    {
                        result = what.Substring( 1, len - 2 );
                    }
                    else if (len > 1 && what[0] == '\'')
                    {
                        result = what.Substring( 1, len - 1 );
                    }
                    else if (len > 1 && what[len - 1] == '\'')
                    {
                        result = what.Substring( 0, len - 1 );
                    }
                    else
                    {
                        result = what;
                    }
                }
            }
            return result;
        }
        #endregion

        #region WithinParentheses
        /// <summary>
        /// Return the given string surrounded by parentheses. If what is null, represent it as "null".
        /// </summary>
        /// <param name="what">the string to return surrounded by parentheses</param>
        /// <returns>the given string surrounded by parentheses</returns>
        public static string WithinParentheses( this string what )
        {
            if (what == null)
            {
                return "(null)";
            }
            else
            {
                return "(" + what + ")";
            }
        }
        #endregion

        #region WithoutAtStart
        /// <summary>
        /// Return this string with the given text removed from the beginning of it, if it's present.
        /// </summary>
        /// <param name="thisString">the string to remove the text from</param>
        /// <param name="charToRemoveAtStart">the character to remove</param>
        /// <returns>the given string without that character if it was present, otherwise just returns itself unchanged</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <remarks>
        /// The arguments do NOT get stripped of leading-space, and the text-comparison is case-sensitive.
        /// </remarks>
        public static string WithoutAtStart( this string thisString, Char charToRemoveAtStart )
        {
            if (thisString == null)
            {
                throw new ArgumentNullException( "thisString" );
            }
            if (thisString.Length > 0)
            {
                if (thisString[0] == charToRemoveAtStart)
                {
                    return thisString.Substring( 1 );
                }
            }
            return thisString;
        }

        /// <summary>
        /// Return this string with the given text removed from the beginning of it, if it's present.
        /// </summary>
        /// <param name="thisString">the string to remove the text from</param>
        /// <param name="textToRemoveAtStart">the text to remove</param>
        /// <returns>the string without the given text at the beginning if it was present, otherwise just returns itself unchanged</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <remarks>
        /// The arguments do NOT get stripped of leading-space, and the text-comparison is case-sensitive.
        /// </remarks>
        public static string WithoutAtStart( this string thisString, string textToRemoveAtStart )
        {
            if (thisString == null)
            {
                throw new ArgumentNullException( "thisString" );
            }
            if (!String.IsNullOrEmpty( textToRemoveAtStart ))
            {
                int n = textToRemoveAtStart.Length;
                if (thisString.Length >= n)
                {
                    if (thisString.StartsWith( textToRemoveAtStart ))
                    {
                        return thisString.Substring( n );
                    }
                }
            }
            return thisString;
        }

        /// <summary>
        /// Return this string with the given text removed from the beginning of it, if it's present.
        /// </summary>
        /// <param name="thisString">what to remove it from</param>
        /// <param name="textToRemoveAtStart">The text to remove</param>
        /// <param name="isToIgnoreCase">Indicates whether to ignore whether the prefix to remove appears as uppercase or lower (it gets removed regardless)</param>
        /// <returns>This, without the given text at the beginning, if it was present, otherwise just returns itself unchanged</returns>
        /// <exception cref="ArgumentNullException"/>
        /// <remarks>
        /// The arguments do NOT get stripped of leading-space, and the text-comparison is case-sensitive only if ignoreCase is false.
        /// </remarks>
        public static string WithoutAtStart( this string thisString, string textToRemoveAtStart, bool isToIgnoreCase )
        {
            if (thisString == null)
            {
                throw new ArgumentNullException( "thisString" );
            }
            if (!String.IsNullOrEmpty( textToRemoveAtStart ))
            {
                int n = textToRemoveAtStart.Length;
                if (thisString.Length >= n)
                {
                    bool doesStartWith = false;
#if SILVERLIGHT
                    doesStartWith = thisString.StartsWith(textToRemoveAtStart, isToIgnoreCase);
#else
                    //CBL
#if NETFX_CORE
                    if (isToIgnoreCase)
                    {
                        doesStartWith = thisString.StartsWith(textToRemoveAtStart, StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        doesStartWith = thisString.StartsWith(textToRemoveAtStart, StringComparison.Ordinal);
                    }
#else
                    doesStartWith = thisString.StartsWith( textToRemoveAtStart, isToIgnoreCase, CultureInfo.InvariantCulture );
#endif
#endif
                    if (doesStartWith)
                    {
                        return thisString.Substring( n );
                    }
                }
            }
            return thisString;
        }
        #endregion

        #region WithoutAtEnd
        /// <summary>
        /// Return this string with the given character removed from the end, if it's present.
        /// Spaces are trimmed from the end first.
        /// </summary>
        /// <param name="thisString"></param>
        /// <param name="characterToRemoveFromEnd">The char to remove from the end of it, if it is present</param>
        /// <returns>thisString, without the given char at the end, if it was present, otherwise just returns itself unchanged</returns>
        public static string WithoutAtEnd( this string thisString, char characterToRemoveFromEnd )
        {
            //CBL Is this a duplicatte? See TrimFromEnd.
            if (HasNothing( thisString ))
            {
                return String.Empty;
            }
            else
            {
                string trimmedString = thisString.TrimEnd();
                int n = trimmedString.Length;
                if (trimmedString[n - 1] == characterToRemoveFromEnd)
                {
                    return trimmedString.Substring( 0, n - 1 );
                }
                return trimmedString;
            }
        }

        /// <summary>
        /// Return this string with the given text removed from the end, if it's present. The given string may be null.
        /// </summary>
        /// <param name="thisString"></param>
        /// <param name="textToRemoveAtEnd">The text to remove from the end of it, if it is present</param>
        /// <returns>thisString, without the given text at the end, if it was present, otherwise just returns itself unchanged</returns>
        public static string WithoutAtEnd( this string thisString, string textToRemoveAtEnd )
        {
            //CBL Create unit-tests, and see if I can remove a few steps here.
            if (!String.IsNullOrEmpty( textToRemoveAtEnd ))
            {
                int nToRemove = textToRemoveAtEnd.Length;
                int n = thisString.Length;
                if (n >= nToRemove)
                {
                    if (thisString.EndsWith( textToRemoveAtEnd ))
                    {
                        return thisString.Substring( 0, n - nToRemove );
                    }
                }
            }
            return thisString;
        }

        /// <summary>
        /// Return this string with the given text removed from the end, if it's present.
        /// </summary>
        /// <param name="thisString"></param>
        /// <param name="textToRemoveAtEnd">The text to remove from the end of it, if it is present</param>
        /// <param name="isToIgnoreCase">Indicates whether to ignore whether the suffix to remove appears as uppercase or lower</param>
        /// <returns>thisString, without the given text at the end, if it was present, otherwise just returns itself unchanged</returns>
        public static string WithoutAtEnd( this string thisString, string textToRemoveAtEnd, bool isToIgnoreCase )
        {
            if (!String.IsNullOrEmpty( textToRemoveAtEnd ))
            {
                int nToRemove = textToRemoveAtEnd.Length;
                int n = thisString.Length;
                if (n >= nToRemove)
                {
                    bool doesEndWith = false;
#if SILVERLIGHT
                    doesEndWith = thisString.EndsWith(textToRemoveAtEnd, isToIgnoreCase);
#elif NETFX_CORE
                    doesEndWith = thisString.EndsWith( textToRemoveAtEnd, (isToIgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal) );
#else
                    //CBL
                    doesEndWith = thisString.EndsWith( textToRemoveAtEnd, isToIgnoreCase, CultureInfo.InvariantCulture );
#endif
                    if (doesEndWith)
                    {
                        return thisString.Substring( 0, n - nToRemove );
                    }
                }
            }
            return thisString;
        }
        #endregion WithoutAtEnd

        #region WithoutLeadingLineEndCharacters
        /// <summary>
        /// Return the given inputText with any of the (common patterns of leading carriage-return or new-line characters) removed.
        /// Spaces are preserved.
        /// </summary>
        /// <param name="inputText">the input string to strip off any leading CR/LF from</param>
        /// <returns>a copy of the input string without the leading CR-or-LF characters</returns>
        public static string WithoutLeadingLineEndCharacters( this string inputText )
        {
            // Remove from the beginning either CR, LF, CR+LF, or LF+CR.
            if (inputText == null)
            {
                throw new ArgumentNullException( "inputText" );
            }

            string newText = inputText;
            bool isToStop = false;
            while (!isToStop)
            {
                if (newText.Length > 0)
                {
                    // If the first character is a newline or carriage-return,
                    char firstChar = newText[0];
                    if (firstChar == '\n' || firstChar == '\r')
                    {
                        // Remove that first character.
                        newText = newText.Substring( 1 );
                    }
                    else
                    {
                        isToStop = true;
                    }
                }
                else
                {
                    isToStop = true;
                }
            }
            return newText;

            //CBL
            //if (HasNothing( inputText ))
            //{
            //    return String.Empty;
            //}
            //if (inputText.Length >= 2)
            //{
            //    if (inputText[0] == '\r' && inputText[1] == '\n')
            //    {
            //        return inputText.Substring( 2 );
            //    }
            //    else if (inputText[0] == '\n' && inputText[1] == '\r')
            //    {
            //        return inputText.Substring( 2 );
            //    }
            //}
            //if (inputText[0] == '\n')
            //{
            //    return inputText.Substring( 1 );
            //}
            //else if (inputText[0] == '\r')
            //{
            //    return inputText.Substring( 1 );
            //}
            //return inputText;
        }
        #endregion

        #region WithoutLeadingZeros
        /// <summary>
        /// Given a string that presumably represents a number,
        /// return that string without any leading zeros.
        /// </summary>
        /// <param name="sNumberString">The source string to (potentially) trim leading zeros from</param>
        /// <returns>a copy of the source string, trimmed</returns>
        /// <remarks>
        /// Providing a null or empty-string argument results in an empty string being returned.
        /// </remarks>
        public static string WithoutLeadingZeros( this string sNumberString )
        {
            if (String.IsNullOrEmpty( sNumberString ))
            {
                return String.Empty;
            }
            else
            {
                string inputTrimmed = sNumberString.Trim();
                int iFirstNonzero = -1;
                for (int i = 0; i < inputTrimmed.Length; i++)
                {
                    if (inputTrimmed[i] != '0')
                    {
                        iFirstNonzero = i;
                        break;
                    }
                }
                string result = inputTrimmed;
                if (iFirstNonzero > 0)
                {
                    result = inputTrimmed.Substring( iFirstNonzero );
                }
                else if (iFirstNonzero < 0) // it is all zeros.
                {
                    if (inputTrimmed.Length > 1)
                    {
                        result = "0";
                    }
                }
                return result;
            }
        }
        #endregion

        #region WithoutLeadingZerosInBlocksOfTwo
        /// <summary>
        /// Given a string that presumably represents a number,
        /// return that string without any leading zeros except what is needed
        /// to make the result have an even number of characters.
        /// </summary>
        /// <param name="sNumberString">The source string to (potentially) trim leading zeros from</param>
        /// <returns>a copy of the source string, trimmed</returns>
        /// <remarks>
        /// Providing a null or empty-string argument results in an empty string being returned.
        /// </remarks>
        public static string WithoutLeadingZerosInBlocksOfTwo( this string sNumberString )
        {
            if (String.IsNullOrEmpty( sNumberString ))
            {
                return String.Empty;
            }
            else
            {
                string inputTrimmed = sNumberString.Trim();
                int iFirstNonzero = -1;
                for (int i = 0; i < inputTrimmed.Length; i++)
                {
                    if (inputTrimmed[i] != '0')
                    {
                        iFirstNonzero = i;
                        break;
                    }
                }
                string result = inputTrimmed;
                if (iFirstNonzero > 0)
                {
                    int lengthOfResult = inputTrimmed.Substring( iFirstNonzero ).Length;
                    if (lengthOfResult == 1 || (iFirstNonzero > 0 && lengthOfResult % 2 != 0))
                    {
                        iFirstNonzero--;
                    }
                    result = inputTrimmed.Substring( iFirstNonzero );
                }
                else if (iFirstNonzero < 0) // it is all zeros.
                {
                    if (inputTrimmed.Length > 1)
                    {
                        result = "00";
                    }
                }
                if (result.Length % 2 != 0)
                {
                    return "0" + result;
                }
                return result;
            }
        }
        #endregion

        #region WithoutTrailingLineEndCharacters
        /// <summary>
        /// Return the given inputText with any of the (common patterns of trailing carriage-return or new-line characters) removed.
        /// </summary>
        /// <param name="inputText">the input string to strip off any trailing CR/LF from</param>
        /// <returns>a copy of the input string without the trailing 1 or 2 characters if they are CR, LF, CR+LF or LF+CR</returns>
        public static string WithoutTrailingLineEndCharacters( this string inputText )
        {
            // Remove from the end either CR, LF, CR+LF, or LF+CR.
            if (inputText == null)
            {
                throw new ArgumentNullException( "inputText" );
            }
            if (HasNothing( inputText ))
            {
                return String.Empty;
            }
            int iLastChar = inputText.Length - 1;
            if (inputText.Length >= 2)
            {
                if (inputText[iLastChar - 1] == '\r' && inputText[iLastChar] == '\n')
                {
                    return inputText.Substring( 0, iLastChar - 1 );
                }
                else if (inputText[iLastChar - 1] == '\n' && inputText[iLastChar] == '\r')
                {
                    return inputText.Substring( 0, iLastChar - 1 );
                }
            }
            if (inputText[iLastChar] == '\n')
            {
                return inputText.Substring( 0, iLastChar );
            }
            else if (inputText[iLastChar] == '\r')
            {
                return inputText.Substring( 0, iLastChar );
            }
            return inputText;
        }
        #endregion

        #region WithoutTrailingZeros
        /// <summary>
        /// Given a string that presumably represents a decimal number,
        /// return that string without any excess trailing zeros after the decimal-point.
        /// </summary>
        /// <param name="sNumberString">The source string to (potentially) trim zeros from</param>
        /// <returns>A copy of the source string, trimmed</returns>
        public static string WithoutTrailingZeros( this string sNumberString )
        {
            if (String.IsNullOrEmpty( sNumberString ))
            {
                return String.Empty;
            }
            else
            {
                int n = sNumberString.Length;
                // Get the position of the decimal point (English culture),
                // or whichever symbol the current culture uses for the decimal-point.
                NumberFormatInfo formatInfo = CultureInfo.CurrentCulture.NumberFormat;
                string sDecimalSymbol = formatInfo.NumberDecimalSeparator;
                // Determine what we should be using for a comma-separator.
                string sGroupSeparator = formatInfo.NumberGroupSeparator;
                char cGroupSeparator = ',';
                if (!String.IsNullOrEmpty( sGroupSeparator ))
                {
                    cGroupSeparator = sGroupSeparator[0];
                }
                int iPositionOfDecimal = sNumberString.IndexOf( sDecimalSymbol );
                if (iPositionOfDecimal < 0)
                {
                    // No decimal-point, so just return the string unchanged.
                    return sNumberString;
                }
                else // there is a decimal-point.
                {
                    // Are trailing zeros?
                    int iPositionOfLastCharToKeep = n - 1;
                    for (int i = n - 1; i >= iPositionOfDecimal; i--)
                    {
                        if (sNumberString[i] == '0')
                        {
                            iPositionOfLastCharToKeep = i - 1;
                        }
                        else if (i == iPositionOfDecimal)
                        {
                            // There are only zeros (or nothing) beyond the decimal-point,
                            // so remove the decimal-point itself.
                            iPositionOfLastCharToKeep = iPositionOfDecimal - 1;
                            break;
                        }
                        else if (sNumberString[i] != cGroupSeparator)
                        {
                            // This character is not a zero, and it's not a decimal - so we're done.
                            break;
                        }
                    }
                    return sNumberString.Substring( 0, iPositionOfLastCharToKeep + 1 );
                }
            }
        }
        #endregion

        #region WithSlashOnEnd
        /// <summary>
        /// Return the given string with a back-slash on the end, adding one if it doesn't already have one.
        /// </summary>
        /// <param name="inputText">the string to add a back-slash to</param>
        /// <returns>the inputText with a back-slash appended if necessary</returns>
        public static string WithSlashOnEnd( this string inputText )
        {
            if (inputText == null)
            {
                throw new ArgumentNullException( "inputText" );
            }
            if (HasNothing( inputText ))
            {
                return @"\";
            }
            int n = inputText.Length;
            if (inputText[n - 1] != '\\')
            {
                return inputText + @"\";
            }
            else
            {
                return inputText;
            }
        }
        #endregion
    }
}
