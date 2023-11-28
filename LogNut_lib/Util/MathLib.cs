using System;
using System.Globalization;
using System.Linq;
#if !PRE_4
using System.Numerics;
#endif


namespace Hurst.LogNut.Util
{
    /// <summary>
    /// A static class for holding a few misc mathematical utilities
    /// </summary>
    public static class MathLib
    {
        #region AlignDecimalPointOfText
        /// <summary>
        /// Given a textual representation of a number, using a format that represents the decimal-point
        /// as a period, return that text with enough spaces prefixed onto it
        /// such that it has the given number of characters-or-spaces before that decimal-point.
        /// </summary>
        /// <param name="numberAsText">a textual representation of the number</param>
        /// <param name="lengthBeforeDecimalPoint">the desired number of total characters (spaces plus whatever) to come before the decimal-point</param>
        /// <returns>the same text but with spaces prepended to make the decimal-point fall into the desired position</returns>
        public static string AlignDecimalPointOfText( this string numberAsText, int lengthBeforeDecimalPoint )
        {
            if (numberAsText == null)
            {
                throw new ArgumentNullException( "numberAsText" );
            }
            if (lengthBeforeDecimalPoint < 0)
            {
#if PRE_4
                throw new ArgumentOutOfRangeException( "lengthBeforeDecimalPoint", lengthBeforeDecimalPoint, "lengthBeforeDecimalPoint must not be negative" );
#else
                throw new ArgumentOutOfRangeException( paramName: nameof( lengthBeforeDecimalPoint ), actualValue: lengthBeforeDecimalPoint, message: "lengthBeforeDecimalPoint must not be negative" );
#endif
            }
            int positionOfDecimal = numberAsText.IndexOf( '.' );
            if (positionOfDecimal > -1)
            {
                if (positionOfDecimal < lengthBeforeDecimalPoint)
                {
                    int spacesNeeded = lengthBeforeDecimalPoint - positionOfDecimal;
                    return StringLib.Spaces( spacesNeeded ) + numberAsText;
                }
            }
            else // no decimal-point at all.
            {
                int lengthOfText = numberAsText.Length;
                if (lengthOfText < lengthBeforeDecimalPoint)
                {
                    int spacesNeeded = lengthBeforeDecimalPoint - lengthOfText;
                    return StringLib.Spaces( spacesNeeded ) + numberAsText;
                }
            }
            return numberAsText;
        }
        #endregion

        #region AreEqual
        /// <summary>
        /// Return true if the two numeric <c>Double</c> values are essentially equal in value,
        /// ie within about <c>Double.Epsilon</c> of each other.
        /// </summary>
        /// <param name="x">one value to compare</param>
        /// <param name="y">the other value to compare against it</param>
        /// <returns>true if they're within Double.Epsilon of each other</returns>
        public static bool AreEqual( this double x, double y )
        {
            return Math.Abs( x - y ) <= Double.Epsilon;
        }
        #endregion

        #region AreEqualToWithin
        /// <summary>
        /// Return true if the two numeric-<c>Double</c> values are near enough to being equal in value,
        /// such that their difference is less-than-or-equal to the given <paramref name="precision"/>.
        /// </summary>
        /// <param name="x">one value to compare</param>
        /// <param name="y">the other value to compare against it</param>
        /// <param name="precision">the maximum-allowed difference between <paramref name="x"/> and <paramref name="y"/> for them to be considered equal</param>
        /// <returns>true if they're within <paramref name="precision"/> of each other</returns>
        public static bool AreEqualToWithin( this double x, double y, double precision )
        {
            double actualDifference = Math.Abs( x - y );
            bool isEqual = actualDifference <= precision;
            return isEqual;
            //return Math.Abs( x - y ) <= precision;
        }
        #endregion

        #region IsEssentiallyEqual
        /// <summary>
        /// Return true if the two numeric values are essentially equal in value,
        /// ie within about Double.Epsilon of each other.
        /// </summary>
        /// <param name="x">one value to compare</param>
        /// <param name="y">the other value to compare against it</param>
        /// <returns>true if they're within Double.Epsilon of each other</returns>
        public static bool IsEssentiallyEqual( this double x, double y )
        {
            return Math.Abs( x - y ) <= Double.Epsilon;
            //double epsilon = 0.0001;
            //return (y - epsilon) < x && x < (y + epsilon);
        }

        /// <summary>
        /// Return true if the two numeric values are essentially equal in value,
        /// ie within about Epsilon of each other.
        /// </summary>
        /// <param name="x">one value to compare</param>
        /// <param name="y">the other value to compare against it</param>
        /// <returns>true if they're within Epsilon of each other</returns>
        public static bool IsEssentiallyEqual( this float x, float y )
        {
            // See this article:
            // https://stackoverflow.com/questions/3874627/floating-point-comparison-functions-for-c-sharp
            //return Math.Abs( x - y ) <= float.Epsilon;

            if (x == y)
            {
                // Shortcut; handles infinities.
                return true;
            }

            float epsilon = float.Epsilon;
            const float floatNormal = (1 << 23) * float.Epsilon;
            float diff = Math.Abs( x - y );

            if (x == 0.0f || y == 0.0f || diff < floatNormal)
            {
                // x or y is zero, or both are extremely close to it.
                // Relative error is less meaningful here.
                return diff < ( epsilon * floatNormal );
            }

            // Use relative error
            float absX = Math.Abs( x );
            float absY = Math.Abs( y );
            return diff / Math.Min( ( absX + absY ), float.MaxValue ) < epsilon;
        }

        #endregion

        #region AreUnequal
        /// <summary>
        /// Return true if the two <see cref="double"/> values are unequal in value
        /// by an amount greater than <see cref="double.Epsilon"/>.
        /// </summary>
        /// <param name="x">one value to compare</param>
        /// <param name="y">the other value to compare against it</param>
        /// <returns>true if they're further apart in value than Double.Epsilon</returns>
        public static bool AreUnequal( this double x, double y )
        {
            return Math.Abs( x - y ) > Double.Epsilon;
            //double epsilon = 0.0001;
            //return (y - epsilon) < x && x < (y + epsilon);
        }

        /// <summary>
        /// Return true if the two <see cref="Decimal"/> values are unequal in value.
        /// This is the same as "x != y", but is useful if you want your code to easily port between Double and Decimal values.
        /// </summary>
        /// <param name="x">one value to compare</param>
        /// <param name="y">the other value to compare against it</param>
        /// <returns>true if they're unequal</returns>
        public static bool AreUnequal( this decimal x, decimal y )
        {
            return x != y;
        }
        #endregion

        #region DecimalToText
        /// <summary>
        /// Given a Decimal numeric value, return it as a string - optionally with the groups of three digits
        /// delimited by commas, in en-US form and in accordance with the International Bureau of Weights and Measures.
        /// </summary>
        /// <param name="value">the numeric value to convert to a string</param>
        /// <param name="withGroupDelimiters">set this true to get group-delimiters in the result (defaults to true)</param>
        /// <param name="useSmallSpaceForDelimiter">set this to true to add Unicode-2006 character as delimiters around the colons, the "Six-per-EM" space (optional - default is false)</param>
        /// <param name="decimalPlaces">this dictates the number of decimal-places to show, if non-null. Null means the value itself dictates the number of decimal-places (defaults to null)</param>
        /// <returns>a string representation of that numeric value</returns>
        /// <remarks>
        /// When the value is 4 digits (whole digits, not fractional parts)
        /// no delimiter is used (we do not isolate the leftmost digit in that case).
        /// </remarks>
        public static string DecimalToText( this decimal value, bool withGroupDelimiters, bool useSmallSpaceForDelimiter, int? decimalPlaces )
        {
            try
            {
                string text;
                if (withGroupDelimiters)
                {
                    // If there is no fractional part
                    bool hasNoFractionalPart = (value % 1) <= (decimal)Double.Epsilon;
                    if (hasNoFractionalPart)
                    {
                        //BigInteger bigIntegerValue = (BigInteger)value;
                        // and the whole value is less than 10000 (and greater than -10000)
                        //if (bigIntegerValue < 10000)
                        if (Math.Abs( value ) < 10000)
                        {
                            // then nothing more is needed.
                            string textNotPadded = value.ToString();
                            if (decimalPlaces.HasValue)
                            {
                                text = textNotPadded.PadToRightAfterPeriodWithZeros( decimalPlaces.Value, true, useSmallSpaceForDelimiter );
                            }
                            else
                            {
                                text = textNotPadded;
                            }
                            return text;
                        }
                    }

                    string textWithNoDelimiters;
                    if (decimalPlaces.HasValue)
                    {
                        _cultureInfo.NumberFormat.NumberDecimalDigits = decimalPlaces.Value;
                        textWithNoDelimiters = value.ToString( "F", _cultureInfo );
                    }
                    else
                    {
                        textWithNoDelimiters = value.ToString();
                    }

                    int indexOfPt = textWithNoDelimiters.IndexOf( ".", StringComparison.Ordinal );

                    // Compute the text for the whole part.
                    string wholePart;
                    if (indexOfPt == -1)
                    {
                        wholePart = textWithNoDelimiters;
                    }
                    else
                    {
                        wholePart = textWithNoDelimiters.Substring( 0, indexOfPt );
                    }
                    string wholePartWithDelimiters = AddDelimitersToTheLeft( wholePart, 4, useSmallSpaceForDelimiter );

                    if (decimalPlaces.HasValue && decimalPlaces.Value == 0)
                    {
                        return wholePartWithDelimiters;
                    }

                    // Compute the text for the fractional part.
                    if (indexOfPt > -1)
                    {
                        string fractionalPartWithoutDelimiters;
                        if (decimalPlaces.HasValue)
                        {
                            string textPadded = textWithNoDelimiters.PadToRightAfterPeriodWithZeros( decimalPlaces.Value, withGroupDelimiters, useSmallSpaceForDelimiter );
                            fractionalPartWithoutDelimiters = textPadded.Substring( indexOfPt + 1 );
                        }
                        else
                        {
                            fractionalPartWithoutDelimiters = textWithNoDelimiters.Substring( indexOfPt + 1 );
                        }

                        if (fractionalPartWithoutDelimiters.Length > 4)
                        {
                            text = wholePartWithDelimiters + "." + AddDelimitersToTheRight( fractionalPartWithoutDelimiters, 4, useSmallSpaceForDelimiter );
                        }
                        else
                        {
                            text = wholePartWithDelimiters + "." + fractionalPartWithoutDelimiters;
                        }
                    }
                    else
                    {
                        text = wholePartWithDelimiters;
                    }
                }
                else
                {
                    if (decimalPlaces.HasValue)
                    {
                        _cultureInfo.NumberFormat.NumberDecimalDigits = decimalPlaces.Value;
                        text = value.ToString( "F", _cultureInfo );
                    }
                    else
                    {
                        text = value.ToString();
                    }
                }
                return text;
            }
            catch (Exception x)
            {
                string s = String.Format( "value is {0}, withGroupDelimiters is {1}, decimalPlaces is {2}", value, withGroupDelimiters, StringLib.AsString( decimalPlaces ) );
                x.Data.Add( "Arguments", s );
                throw;
            }
        }

        /// <summary>
        /// Given a Decimal numeric value, return it as a string with the groups of three digits
        /// delimited by commas, in en-US form and in accordance with the International Bureau of Weights and Measures.
        /// </summary>
        /// <param name="value">the numeric value to convert to a string</param>
        /// <returns>a string representation of that numeric value</returns>
        /// <remarks>
        /// When the value is 4 digits (whole digits, not fractional parts)
        /// no delimiter is used (we do not isolate the leftmost digit in that case).
        /// </remarks>
        public static string DecimalToText( this decimal value )
        {
            return DecimalToText( value, true, false, null );
        }
        #endregion

        #region DoubleToText
        /// <summary>
        /// Given a <c>Double</c> numeric value, return it as a string - optionally with the groups of three digits
        /// delimited by commas, in en-US form and in accordance with the International Bureau of Weights and Measures.
        /// </summary>
        /// <param name="value">the numeric value to convert to a string</param>
        /// <param name="withGroupDelimiters">set this true to get group-delimiters in the result (defaults to true)</param>
        /// <param name="useSmallSpaceForDelimiter">set this to true to add Unicode-2006 character as delimiters around the colons, the "Six-per-EM" space (optional - default is false)</param>
        /// <param name="decimalPlaces">this dictates the number of decimal-places to show, if non-null. Null means the value itself dictates the number of decimal-places (defaults to null)</param>
        /// <returns>a string representation of that numeric value</returns>
        /// <remarks>
        /// When the value is 4 digits (whole digits, not fractional parts)
        /// no delimiter is used (we do not isolate the leftmost digit in that case).
        /// </remarks>
        public static string DoubleToText( this double value, bool withGroupDelimiters, bool useSmallSpaceForDelimiter, int? decimalPlaces )
        {
            try
            {
                string text;
                if (withGroupDelimiters)
                {
                    // If there is no fractional part
                    bool hasNoFractionalPart = (value % 1) <= Double.Epsilon;
                    if (hasNoFractionalPart)
                    {
                        //BigInteger bigIntegerValue = (BigInteger)value;
                        // and the whole value is less than 10000 (and greater than -10000)
                        //if (bigIntegerValue < 10000)
                        if (Math.Abs( value ) < 10000)
                        {
                            // then nothing more is needed.
                            string textNotPadded = value.ToString();
                            if (decimalPlaces.HasValue)
                            {
                                text = textNotPadded.PadToRightAfterPeriodWithZeros( decimalPlaces.Value, true, useSmallSpaceForDelimiter );
                            }
                            else
                            {
                                text = textNotPadded;
                            }
                            return text;
                        }
                    }

                    string textWithNoDelimiters;
                    if (decimalPlaces.HasValue)
                    {
                        _cultureInfo.NumberFormat.NumberDecimalDigits = decimalPlaces.Value;
                        textWithNoDelimiters = value.ToString( "F", _cultureInfo );
                    }
                    else
                    {
                        textWithNoDelimiters = value.ToString();
                    }

                    int indexOfPt = textWithNoDelimiters.IndexOf( ".", StringComparison.Ordinal );

                    // Compute the text for the whole part.
                    string wholePart;
                    if (indexOfPt == -1)
                    {
                        wholePart = textWithNoDelimiters;
                    }
                    else
                    {
                        wholePart = textWithNoDelimiters.Substring( 0, indexOfPt );
                    }
                    string wholePartWithDelimiters = AddDelimitersToTheLeft( wholePart, 4, useSmallSpaceForDelimiter );

                    if (decimalPlaces.HasValue && decimalPlaces.Value == 0)
                    {
                        return wholePartWithDelimiters;
                    }

                    // Compute the text for the fractional part.
                    if (indexOfPt > -1)
                    {
                        string fractionalPartWithoutDelimiters;
                        if (decimalPlaces.HasValue)
                        {
                            string textPadded = textWithNoDelimiters.PadToRightAfterPeriodWithZeros( decimalPlaces.Value, withGroupDelimiters, useSmallSpaceForDelimiter );
                            fractionalPartWithoutDelimiters = textPadded.Substring( indexOfPt + 1 );
                        }
                        else
                        {
                            fractionalPartWithoutDelimiters = textWithNoDelimiters.Substring( indexOfPt + 1 );
                        }

                        if (fractionalPartWithoutDelimiters.Length > 4)
                        {
                            text = wholePartWithDelimiters + "." + AddDelimitersToTheRight( fractionalPartWithoutDelimiters, 4, useSmallSpaceForDelimiter );
                        }
                        else
                        {
                            text = wholePartWithDelimiters + "." + fractionalPartWithoutDelimiters;
                        }
                    }
                    else
                    {
                        text = wholePartWithDelimiters;
                    }
                }
                else
                {
                    if (decimalPlaces.HasValue)
                    {
                        _cultureInfo.NumberFormat.NumberDecimalDigits = decimalPlaces.Value;
                        text = value.ToString( "F", _cultureInfo );
                    }
                    else
                    {
                        text = value.ToString();
                    }
                }
                return text;
            }
            catch (Exception x)
            {
                string s = String.Format( "value is {0}, withGroupDelimiters is {1}, decimalPlaces is {2}", value, withGroupDelimiters, StringLib.AsString( decimalPlaces ) );
                x.Data.Add( "Arguments", s );
                throw;
            }
        }

        /// <summary>
        /// Given a <c>Double</c> numeric value, return it as a string with the groups of three digits
        /// delimited by commas, in en-US form and in accordance with the International Bureau of Weights and Measures.
        /// </summary>
        /// <param name="value">the numeric value to convert to a string</param>
        /// <returns>a string representation of that numeric value</returns>
        /// <remarks>
        /// When the value is 4 digits (whole digits, not fractional parts)
        /// no delimiter is used (we do not isolate the leftmost digit in that case).
        /// </remarks>
        public static string DoubleToText( this double value )
        {
            return DoubleToText( value, true, false, null );
        }
        #endregion

        #region DecimalToTextHex

        /// <summary>
        /// Given a Decimal numeric value, return it as a string expressing that value in hexadecimal form
        /// - optionally padded to a specified with, and optionally with groups of digits
        /// delimited by a special characdter.
        /// </summary>
        /// <param name="decimalValue">the numeric value to convert to a string</param>
        /// <returns>a string that represents the number expressed in hexadecimal</returns>
        /// <remarks>
        /// This is an overload of the DecimalToTextHex( this decimal decimalValue,
        ///                                              int? desiredWidth,
        ///                                              bool withGroupDelimiters,
        ///                                              int groupSize,
        ///                                              char delimiter )
        /// with these default values supplied:
        ///   desiredWidth = null
        ///   withGroupDelimiters = true
        ///   groupSize = 4
        ///   delimiter = StringLib.SmallSpace.
        /// No provision is made for fractional parts, nor for negative numbers.
        /// </remarks>
        public static string DecimalToTextHex( this decimal decimalValue )
        {
            return DecimalToTextHex( decimalValue, null, true, 4, StringLib.SmallSpace );
        }

        /// <summary>
        /// Given a Decimal numeric value, return it as a string expressing that value in hexadecimal form
        /// - optionally padded to a specified with, and optionally with groups of digits
        /// delimited by a special characdter.
        /// </summary>
        /// <param name="decimalValue">the numeric value to convert to a string</param>
        /// <param name="desiredWidth">the desired minimum width of the result</param>
        /// <param name="withGroupDelimiters">set this true to get group-delimiters in the result (defaults to true)</param>
        /// <param name="groupSize">the number of digits to a group, for the purpose of adding delimiters</param>
        /// <param name="delimiter">the character to use for separating groups of digits, if that is desired (optional - default is Unicode-2006)</param>
        /// <returns>a string that represents the number expressed in hexadecimal</returns>
        /// <remarks>
        /// No provision is made for fractional parts, nor for negative numbers.
        /// </remarks>
        public static string DecimalToTextHex( this decimal decimalValue,
                                               int? desiredWidth,
                                               bool withGroupDelimiters,
                                               int groupSize,
                                               char delimiter )
        {
            if (desiredWidth.HasValue && desiredWidth.Value < 0)
            {
#if PRE_4
                throw new ArgumentOutOfRangeException( "desiredWidth", "The desired width must not be negative" );
#else
                throw new ArgumentOutOfRangeException( paramName: nameof( desiredWidth ), message: "The desired width must not be negative" );
#endif
            }
            string result;
#if !PRE_4
            BigInteger integerValue = (BigInteger)decimalValue;
#else
            Int64 integerValue = (Int64)decimalValue;
#endif
            // This next step magically returns "08" for 8, so we need to correct for that.
            string valueString = integerValue.ToString( "X" );

            // Pad to the specified width, if a minimum-width has been specfied.
            int minimumWidth = (desiredWidth.HasValue ? (Math.Max( 1, desiredWidth.Value )) : 1);
            if (valueString.Length < minimumWidth)
            {
                valueString = StringLib.ExpandTo( "0", minimumWidth - valueString.Length ) + valueString;
            }
            else
            {
                // Remove any extraneous "0", as when "08" is returned for value 8.
                if (valueString.Length > minimumWidth && valueString[0] == '0')
                {
                    valueString = valueString.Substring( 1 );
                }
            }

            if (withGroupDelimiters)
            {
                result = valueString.WithHexadecimalGroupDelimiters( groupSize, delimiter );
            }
            else
            {
                result = valueString;
            }
            return result;
        }
        #endregion DecimalToTextHex

        #region FloatToText
        /// <summary>
        /// Given a <c>float</c> numeric value, return it as a string - optionally with the groups of three digits
        /// delimited by commas, in en-US form and in accordance with the International Bureau of Weights and Measures.
        /// </summary>
        /// <param name="value">the numeric value to convert to a string</param>
        /// <param name="withGroupDelimiters">set this true to get group-delimiters in the result (defaults to true)</param>
        /// <param name="useSmallSpaceForDelimiter">set this to true to add Unicode-2006 character as delimiters around the colons, the "Six-per-EM" space (optional - default is false)</param>
        /// <param name="decimalPlaces">this dictates the number of decimal-places to show, if non-null. Null means the value itself dictates the number of decimal-places (defaults to null)</param>
        /// <returns>a string representation of that numeric value</returns>
        /// <remarks>
        /// When the value is 4 digits (whole digits, not fractional parts)
        /// no delimiter is used (we do not isolate the leftmost digit in that case).
        /// </remarks>
        public static string FloatToText( this float value, bool withGroupDelimiters, bool useSmallSpaceForDelimiter, int? decimalPlaces )
        {
            //CBL There is a lot of common-code with the other numeric methods here. Need to simplify.
            try
            {
                string text;
                if (withGroupDelimiters)
                {
                    // If there is no fractional part
                    bool hasNoFractionalPart = (value % 1) <= float.Epsilon;
                    if (hasNoFractionalPart)
                    {
                        //BigInteger bigIntegerValue = (BigInteger)value;
                        // and the whole value is less than 10000 (and greater than -10000)
                        //if (bigIntegerValue < 10000)
                        if (Math.Abs( value ) < 10000)
                        {
                            // then nothing more is needed.
                            string textNotPadded = value.ToString();
                            if (decimalPlaces.HasValue)
                            {
                                text = textNotPadded.PadToRightAfterPeriodWithZeros( decimalPlaces.Value, true, useSmallSpaceForDelimiter );
                            }
                            else
                            {
                                text = textNotPadded;
                            }
                            return text;
                        }
                    }

                    string textWithNoDelimiters;
                    if (decimalPlaces.HasValue)
                    {
                        _cultureInfo.NumberFormat.NumberDecimalDigits = decimalPlaces.Value;
                        textWithNoDelimiters = value.ToString( "F", _cultureInfo );
                    }
                    else
                    {
                        textWithNoDelimiters = value.ToString();
                    }

                    int indexOfPt = textWithNoDelimiters.IndexOf( ".", StringComparison.Ordinal );

                    // Compute the text for the whole part.
                    string wholePart;
                    if (indexOfPt == -1)
                    {
                        wholePart = textWithNoDelimiters;
                    }
                    else
                    {
                        wholePart = textWithNoDelimiters.Substring( 0, indexOfPt );
                    }
                    string wholePartWithDelimiters = AddDelimitersToTheLeft( wholePart, 4, useSmallSpaceForDelimiter );

                    if (decimalPlaces.HasValue && decimalPlaces.Value == 0)
                    {
                        return wholePartWithDelimiters;
                    }

                    // Compute the text for the fractional part.
                    if (indexOfPt > -1)
                    {
                        string fractionalPartWithoutDelimiters;
                        if (decimalPlaces.HasValue)
                        {
                            string textPadded = textWithNoDelimiters.PadToRightAfterPeriodWithZeros( decimalPlaces.Value, withGroupDelimiters, useSmallSpaceForDelimiter );
                            fractionalPartWithoutDelimiters = textPadded.Substring( indexOfPt + 1 );
                        }
                        else
                        {
                            fractionalPartWithoutDelimiters = textWithNoDelimiters.Substring( indexOfPt + 1 );
                        }

                        if (fractionalPartWithoutDelimiters.Length > 4)
                        {
                            text = wholePartWithDelimiters + "." + AddDelimitersToTheRight( fractionalPartWithoutDelimiters, 4, useSmallSpaceForDelimiter );
                        }
                        else
                        {
                            text = wholePartWithDelimiters + "." + fractionalPartWithoutDelimiters;
                        }
                    }
                    else
                    {
                        text = wholePartWithDelimiters;
                    }
                }
                else
                {
                    if (decimalPlaces.HasValue)
                    {
                        _cultureInfo.NumberFormat.NumberDecimalDigits = decimalPlaces.Value;
                        text = value.ToString( "F", _cultureInfo );
                    }
                    else
                    {
                        text = value.ToString();
                    }
                }
                return text;
            }
            catch (Exception x)
            {
                string s = String.Format( "value is {0}, withGroupDelimiters is {1}, decimalPlaces is {2}", value, withGroupDelimiters, StringLib.AsString( decimalPlaces ) );
                x.Data.Add( "Arguments", s );
                throw;
            }
        }

        /// <summary>
        /// Given a <c>float</c> numeric value, return it as a string.
        /// </summary>
        /// <param name="value">the numeric value to convert to a string</param>
        /// <returns>a string representation of that numeric value</returns>
        public static string FloatToText( this float value )
        {
            return FloatToText( value, false, false, null );
        }
        #endregion FloatToText

        #region GetDecimalPlaces
        /// <summary>
        /// An extension method that counts and returns the number of decimal places in this numeric value
        /// </summary>
        public static int GetDecimalPlaces( this double value )
        {
            const int MAX_DECIMAL_PLACES = 10;
            double THRESHOLD = Math.Pow( 0.1, 10 );
            if (value == 0.0)
            {
                return 0;
            }
            int nDecimal = 0;
            while (value - Math.Floor( value ) > THRESHOLD && nDecimal < MAX_DECIMAL_PLACES)
            {
                value *= 10.0;
                nDecimal++;
            }
            return nDecimal;
        }

        /// <summary>
        /// An extension method that counts and returns the number of decimal places in this numeric value
        /// </summary>
        public static int GetDecimalPlaces( this decimal value )
        {
            const int MAX_DECIMAL_PLACES = 10;
            decimal THRESHOLD = (decimal)Math.Pow( 0.1, 10 );
            if (value == 0.0m)
            {
                return 0;
            }
            int nDecimal = 0;
            while (value - Math.Floor( value ) > THRESHOLD && nDecimal < MAX_DECIMAL_PLACES)
            {
                value *= 10.0m;
                nDecimal++;
            }
            return nDecimal;
        }
        #endregion

        #region GetNumberOfDecimalPlacesPresent
        /// <summary>
        /// Return the number of decimal-places (digits to the right of the decimal-separator) that are present within the given
        /// text, skipping group-delimiters (commas in English).
        /// </summary>
        /// <param name="numericValueText">the given text - a string that is assumed to represent a decimal value</param>
        /// <returns>the number of digits to the right of the decimal-separator</returns>
        /// <exception cref="ArgumentNullException">the given value-text must not be null</exception>
        public static int GetNumberOfDecimalPlacesPresent( this string numericValueText )
        {
            if (numericValueText == null)
            {
                throw new ArgumentNullException( "numericValueText" );
            }
            string groupDelimiterString = CultureInfo.CurrentUICulture.NumberFormat.NumberGroupSeparator;
            char groupDelimiter = groupDelimiterString[0];

            string numberDecimalSeparatorString = CultureInfo.CurrentUICulture.NumberFormat.NumberDecimalSeparator;
            char numberDecimalSeparator = numberDecimalSeparatorString[0];

            string textWithoutDelimiters = numericValueText.Trim().RemoveAll( groupDelimiter );
            return textWithoutDelimiters.SkipWhile( c => c != numberDecimalSeparator ).Skip( 1 ).Count();
        }
        #endregion

        #region Int32ToText
        /// <summary>
        /// Given a Int32 numeric value, return it as a string with the groups of three digits
        /// delimited by commas, in en-US form and in accordance with the International Bureau of Weights and Measures.
        /// </summary>
        /// <param name="value">the numeric value to convert to a string</param>
        /// <param name="withGroupDelimiters">set this true to get group-delimiters in the result (defaults to true)</param>
        /// <returns>a string representation of that numeric value</returns>
        /// <remarks>
        /// When the value is 4 digits (whole digits, not fractional parts)
        /// no delimiter is used (we do not isolate the leftmost digit in that case).
        /// </remarks>
        public static string Int32ToText( this int value, bool withGroupDelimiters )
        {
            if (withGroupDelimiters)
            {
                if (Math.Abs( value ) < 10000)
                {
                    return value.ToString();
                }
                return String.Format( "{0:n0}", value );
            }
            else
            {
                return value.ToString();
            }
        }

        /// <summary>
        /// Given a Int32 numeric value, return it as a string with the groups of three digits
        /// delimited by commas, in en-US form and in accordance with the International Bureau of Weights and Measures.
        /// </summary>
        /// <param name="value">the numeric value to convert to a string</param>
        /// <returns>a string representation of that numeric value</returns>
        /// <remarks>
        /// When the value is 4 digits (whole digits, not fractional parts)
        /// no delimiter is used (we do not isolate the leftmost digit in that case).
        /// </remarks>
        public static string Int32ToText( this int value )
        {
            return Int32ToText( value, true );
        }

        /// <summary>
        /// Given a Int32 numeric value, return it as a string - padded to the left with zeros as needed
        /// to make it equal to the specified width.
        /// </summary>
        /// <param name="value">the numeric value to convert to a string</param>
        /// <param name="desiredWidth">this specifies the desired width of the resulting text</param>
        /// <returns>a string representation of that numeric value</returns>
        /// <remarks>
        /// If the width of the textual representation of value is of a greater length than desiredWidth,
        /// then <c>desiredWidth</c> is ignored.
        /// </remarks>
        public static string Int32ToText( this int value, int desiredWidth )
        {
            string rawResult = value.ToString();
            string result;
            if (rawResult.Length < desiredWidth)
            {
                int numberOfZeros = desiredWidth - rawResult.Length;
                if (value < 0)
                {
                    result = "-" + StringLib.ExpandTo( "0", numberOfZeros ) + Math.Abs( value ).ToString();
                }
                else
                {
                    result = StringLib.ExpandTo( "0", numberOfZeros ) + rawResult;
                }
            }
            else
            {
                result = rawResult;
            }
            return result;
        }

        #endregion

        #region Int64ToText
        /// <summary>
        /// Given a Int64 numeric value, return it as a string with the groups of three digits
        /// delimited by commas, in en-US form and in accordance with the International Bureau of Weights and Measures.
        /// </summary>
        /// <param name="value">the numeric value to convert to a string</param>
        /// <param name="withGroupDelimiters">set this true to get group-delimiters in the result (defaults to true)</param>
        /// <returns>a string representation of that numeric value</returns>
        /// <remarks>
        /// When the value is 4 digits (whole digits, not fractional parts)
        /// no delimiter is used (we do not isolate the leftmost digit in that case).
        /// </remarks>
        public static string Int64ToText( this Int64 value, bool withGroupDelimiters )
        {
            if (withGroupDelimiters)
            {
                if (Math.Abs( value ) < 10000)
                {
                    return value.ToString();
                }
                return String.Format( "{0:n0}", value );
            }
            else
            {
                return value.ToString();
            }
        }

        /// <summary>
        /// Given a Int64 numeric value, return it as a string with the groups of three digits
        /// delimited by commas, in en-US form and in accordance with the International Bureau of Weights and Measures.
        /// </summary>
        /// <param name="value">the numeric value to convert to a string</param>
        /// <returns>a string representation of that numeric value</returns>
        /// <remarks>
        /// When the value is 4 digits (whole digits, not fractional parts)
        /// no delimiter is used (we do not isolate the leftmost digit in that case).
        /// </remarks>
        public static string Int64ToText( this Int64 value )
        {
            return Int64ToText( value, true );
        }
        #endregion

        #region IsEssentiallyEqualToOrLessThan
        /// <summary>
        /// Return true if the first numeric values is less than the second, or they're essentially equal,
        /// ie within about 0.0001 of each other.
        /// </summary>
        /// <param name="x">one value to compare</param>
        /// <param name="y">the other value to compare against it</param>
        /// <returns>true if x is equal to or less than y</returns>
        public static bool IsEssentiallyEqualToOrLessThan( this double x, double y )
        {
            double epsilon = 0.0001;
            return x < (y + epsilon);
        }
        #endregion

        #region IsEssentiallyEqualToOrGreaterThan
        /// <summary>
        /// Return true if the first numeric values is greater than the second, or they're essentially equal,
        /// ie within about 0.0001 of each other.
        /// </summary>
        /// <param name="x">one value to compare</param>
        /// <param name="y">the other value to compare against it</param>
        /// <returns>true if x is equal to or greater than y</returns>
        public static bool IsEssentiallyEqualToOrGreaterThan( this double x, double y )
        {
            double epsilon = 0.0001;
            return x > (y - epsilon);
        }
        #endregion

        #region IsInRange
        /// <summary>
        /// Return true if this value (x) is within the range denoted by the two parameters X1,x2 regardless of their order or whether they're negative.
        /// </summary>
        /// <param name="x">this numeric value to compare against the range limits</param>
        /// <param name="x1">one range limit (may be greater or less than the other limit)</param>
        /// <param name="x2">the other range limit</param>
        /// <returns></returns>
        public static bool IsInRange( this double x, double x1, double x2 )
        {
            double lowerLimit = Math.Min( x1, x2 );
            double upperLimit = Math.Max( x1, x2 );
            return (x >= lowerLimit && x <= upperLimit);
        }

        /// <summary>
        /// Return true if this value (x) is within the range denoted by the two parameters X1,x2 regardless of their order or whether they're negative.
        /// </summary>
        /// <param name="x">this numeric value to compare against the range limits</param>
        /// <param name="x1">one range limit (may be greater or less than the other limit)</param>
        /// <param name="x2">the other range limit</param>
        /// <returns></returns>
        public static bool IsInRange( this int x, int x1, int x2 )
        {
            int lowerLimit = Math.Min( x1, x2 );
            int upperLimit = Math.Max( x1, x2 );
            return (x >= lowerLimit && x <= upperLimit);
        }
        #endregion

        #region IsValidNumericInput
        /// <summary>
        /// Return true if the given text represents a valid number, in the current culture.
        /// </summary>
        /// <param name="text">the string input to test</param>
        /// <returns>true only if the given text expresses a number</returns>
        public static bool IsValidNumericInput( this string text )
        {
            string str = text.Trim();
            if (str == NumberFormatInfo.CurrentInfo.CurrencyDecimalSeparator |
                str == NumberFormatInfo.CurrentInfo.CurrencyGroupSeparator |
                str == NumberFormatInfo.CurrentInfo.CurrencySymbol |
                str == NumberFormatInfo.CurrentInfo.NegativeSign |
                str == NumberFormatInfo.CurrentInfo.NegativeInfinitySymbol |
                str == NumberFormatInfo.CurrentInfo.NumberDecimalSeparator |
                str == NumberFormatInfo.CurrentInfo.NumberGroupSeparator |
                str == NumberFormatInfo.CurrentInfo.PercentDecimalSeparator |
                str == NumberFormatInfo.CurrentInfo.PercentGroupSeparator |
                str == NumberFormatInfo.CurrentInfo.PercentSymbol |
                str == NumberFormatInfo.CurrentInfo.PerMilleSymbol |
                str == NumberFormatInfo.CurrentInfo.PositiveInfinitySymbol |
                str == NumberFormatInfo.CurrentInfo.PositiveSign)
            {
                return true;
            }
            foreach (Char c in str)
            {
                if (!Char.IsNumber( c ))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region PadToRightAfterPeriodWithZeros
        /// <summary>
        /// Return the given string with the specified number of decimal-places (assuming it represents a numeric value).
        /// This pads to the given number of decimal-places -- not characters.
        /// </summary>
        /// <param name="what">the string to pad zeros to the end of</param>
        /// <param name="desiredNumbereOfDecimalPlaces">how many decimal-places to show</param>
        /// <param name="addGroupDelimiters">this dictates whether to add group-delimiters if the given text was not long enough to have any such that this additional padding requires a decision</param>
        /// <param name="useSmallSpaceForDelimiter">set this to true to add Unicode-2006 character as delimiters around the colons, the "Six-per-EM" space</param>
        /// <returns>a copy of the given string, padded to the given number of decimal places</returns>
        /// <exception cref="ArgumentNullException">The what argument must not be null</exception>
        /// <exception cref="ArgumentOutOfRangeException">The desiredLength argument must not be negative</exception>
        /// <remarks>
        /// Leading and trailing spaces are removed.
        /// 
        /// This does not do any rounding of the number that the given string represents.
        /// If the desired number of decimal-places is to be reduced, then the string is simply cut.
        /// For example, "1.129", with a desiredLength of 2, would yield "1.12".
        /// 
        /// If the given input-string contains group-delimiters, then this is preserved - and group-delimiters (commas)
        /// are added when padding as appropriate if the existing string already had them, or if <paramref name="addGroupDelimiters"/> is <c>true</c>.
        /// </remarks>
        public static string PadToRightAfterPeriodWithZeros( this string what, int desiredNumbereOfDecimalPlaces, bool addGroupDelimiters, bool useSmallSpaceForDelimiter )
        {
            if (what == null)
            {
                throw new ArgumentNullException( "what" );
            }
            if (desiredNumbereOfDecimalPlaces < 0)
            {
#if PRE_4
                throw new ArgumentOutOfRangeException( "desiredNumbereOfDecimalPlaces", desiredNumbereOfDecimalPlaces, "Argument desiredNumbereOfDecimalPlaces must be non-negative" );
#else
                throw new ArgumentOutOfRangeException( paramName: nameof( desiredNumbereOfDecimalPlaces ), actualValue: desiredNumbereOfDecimalPlaces, message: "Argument desiredNumbereOfDecimalPlaces must be non-negative" );
#endif
            }
            // Clean up any evil spaces.
            string inputTrimmed = what.Trim();
            string result;
            // Treat empty strings specially.
            if (inputTrimmed.Length == 0)
            {
                if (desiredNumbereOfDecimalPlaces == 0)
                {
                    return "0";
                }
                else
                {
                    result = "0." + "0".ExpandTo( desiredNumbereOfDecimalPlaces );
                    if (addGroupDelimiters)
                    {
                        return result.WithCorrectGroupDelimiters( useSmallSpaceForDelimiter );
                    }
                    else
                    {
                        return result;
                    }
                }
            }
            int originalDecimalPlaces = inputTrimmed.GetNumberOfDecimalPlacesPresent();
            bool hadCommas = inputTrimmed.IndexOf( ',' ) > -1;
            // We will add group-delimiters IF the original string already had them, or the argument specifies to.
            bool isToAddCommas = hadCommas || addGroupDelimiters;
            int indexOfDecimalPt = inputTrimmed.IndexOf( "." );
            if (desiredNumbereOfDecimalPlaces > originalDecimalPlaces)
            {
                // Need to add some.
                string textWithDecimalPt = inputTrimmed;
                // Add a decimal-point if it does not yet exist.
                if (indexOfDecimalPt == -1)
                {
                    textWithDecimalPt = inputTrimmed + ".";
                }
                result = textWithDecimalPt + "0".ExpandTo( desiredNumbereOfDecimalPlaces - originalDecimalPlaces );
                if (isToAddCommas)
                {
                    return result.WithCorrectGroupDelimiters( useSmallSpaceForDelimiter );
                }
                return result;
            }
            else if (desiredNumbereOfDecimalPlaces < originalDecimalPlaces)
            {
                inputTrimmed = inputTrimmed.RemoveAll( ',' );
                indexOfDecimalPt = inputTrimmed.IndexOf( '.' );
                if (desiredNumbereOfDecimalPlaces == 0)
                {
                    // One less, to exclude the decimal-point itself.
                    result = inputTrimmed.Substring( 0, indexOfDecimalPt + desiredNumbereOfDecimalPlaces );
                }
                else
                {
                    result = inputTrimmed.Substring( 0, indexOfDecimalPt + desiredNumbereOfDecimalPlaces + 1 );
                }
                if (isToAddCommas)
                {
                    return result.WithCorrectGroupDelimiters( useSmallSpaceForDelimiter );
                }
                return result;
            }
            return inputTrimmed;
        }

        /// <summary>
        /// Return the given string with the specified number of decimal-places (assuming it represents a numeric value).
        /// This pads to the given number of decimal-places -- not characters. No group-delimiters are added.
        /// </summary>
        /// <param name="what">the string to pad zeros to the end of</param>
        /// <param name="desiredNumbereOfDecimalPlaces">how many decimal-places to show</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">The what argument must not be null</exception>
        /// <exception cref="ArgumentOutOfRangeException">The desiredLength argument must not be negative</exception>
        /// <remarks>
        /// Leading and trailing spaces are removed.
        /// 
        /// This does not do any rounding of the number that the given string represents.
        /// If the desired number of decimal-places is to be reduced, then the string is simply cut.
        /// For example, "1.129", with a desiredLength of 2, would yield "1.12".
        /// 
        /// If the given input-string contains group-delimiters, then this is preserved - and group-delimiters (commas)
        /// are added when padding as appropriate if the existing string already had them, or if <c>addGroupDelimiters</c> is <c>true</c>.
        /// </remarks>
        public static string PadToRightAfterPeriodWithZeros( this string what, int desiredNumbereOfDecimalPlaces )
        {
            return PadToRightAfterPeriodWithZeros( what, desiredNumbereOfDecimalPlaces, false, false );
        }
        #endregion

        #region ParseToDecimal
        /// <summary>
        /// Return the Decimal value that the given text represents.
        /// </summary>
        /// <param name="sourceText">the textual representation of a Decimal value</param>
        /// <returns>a Decimal value that it represents - or else zero</returns>
        public static decimal ParseToDecimal( this string sourceText )
        {
            if (sourceText == null)
            {
                return 0m;
            }
            //var sb = new StringBuilder("ParseToDecimal( ");
            //sb.Append(source);
            //sb.Append(" )");
            //CBL Should probably throw exception or something in case of a parse-failure.
            string inputText = sourceText.RemoveAll( ',' ).Trim();
            decimal value;
            //CBL What if this fails? !
            Decimal.TryParse( inputText, out value );

            //sb.Append(", value set to ").Append(value);
            //Log(sb.ToString());
            return value;
        }
        #endregion

        #region RadiansToDegrees and DegreesToRadians
        /// <summary>
        /// Convert the given angular value in radians, to the equivalent in degrees.
        /// </summary>
        /// <param name="numberOfRadians">the angular value in radians</param>
        /// <returns>the equivalent angle expressed in units of degrees</returns>
        public static double RadiansToDegrees( double numberOfRadians )
        {
            return 180.0 * numberOfRadians / Math.PI;
        }

        /// <summary>
        /// Convert the given angular value in degrees, to the equivalent in radians.
        /// </summary>
        /// <param name="numberOfDegrees">the angular value in degrees</param>
        /// <returns>the equivalent angle expressed in units of radians</returns>
        public static double DegreesToRadians( double numberOfDegrees )
        {
            return numberOfDegrees * Math.PI / 180.0;
        }
        #endregion

        #region RotateRight
        /// <summary>
        /// Return the given byte-value rotated to the right by a specified number of bit-positions.
        /// </summary>
        /// <param name="value">the byte-value to rotate</param>
        /// <param name="count">the number of bit-positions to shift the byte-value by</param>
        /// <returns>a copy of the byte-value, whose bits are rotated to the right</returns>
        public static byte RotateRight( byte value, int count )
        {
            // by Ben Landry.

            //PURPOSE:
            //  Rotate a byte Right by count bits.
            //  If count = 2
            //   In: 0x31                    Out: 0x4C
            //   b7 b6 b5 b4 b3 b2 b1 b0     b1 b0 b7 b6 b5 b4 b3 b2
            //  |___________.___________|   |___________.___________|

            count &= 0x07;  // Override promotion of shift operator
            return (byte)((value >> count) | (value << (8 - count)));
        }
        #endregion

        #region SwapEndianness
        /// <summary>
        /// Given a 64-bit unsigned integer (8 bytes in length), switch the endian-ness of it
        /// - but only operating on the lower-order 32 bits.
        /// That is, considering only the lower 32 bits, swap the lower-order and higher-order words around.
        /// </summary>
        /// <param name="x">the 64-bit unsigned-integer to operate upon (a copy of)</param>
        /// <returns>a new 32-bit unsigned-integer, with the 16-bit words in reverse order</returns>
        public static uint SwapEndianness( ulong x )
        {
            return (uint)(((x & 0x000000ff) << 24) +
                           ((x & 0x0000ff00) << 8) +
                           ((x & 0x00ff0000) >> 8) +
                           ((x & 0xff000000) >> 24));
        }
        #endregion

        #region TryParseFromHex
        /// <summary>
        /// Given a string representation of a number that is in hexadecimal form,
        /// try to parse it into a Decimal value and return true if successful.
        /// </summary>
        /// <param name="hexText">the text containing a value expressed as hexadecimal</param>
        /// <param name="result">the result is written to this unsigned-integer</param>
        /// <returns>true if successful, false on error</returns>
        /// <remarks>
        /// This places an upper-limit of <c>UInt64.MaxValue</c> on the parsable value.
        /// 
        /// There are overloads of this method provided for parsing to UInt32, and ushort.
        /// 
        /// Input-text may include hexadecimal indicators such as
        /// suffice "x" or "hex", which (if present) will be ignored.
        /// </remarks>
        public static bool TryParseFromHex( this string hexText, out Decimal result )
        {
            result = 0;
            if (StringLib.HasSomething( hexText ))
            {
                string trimmedValue = hexText.Trim().TrimFromEndCaseInsensitve( "HEX" ).Trim().TrimFromEndCaseInsensitve( "X" ).RemoveAll( ' ' ).RemoveAll( StringLib.SmallSpace );
                UInt64 tentativeValue;
                try
                {
                    bool convertedTo64Ok = UInt64.TryParse( trimmedValue, NumberStyles.HexNumber, null, out tentativeValue );
                    if (convertedTo64Ok)
                    {
                        result = (decimal)tentativeValue;
                    }
                    else
                    {
                        return false;
                    }
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Given a string representation of a number that is in hexadecimal form,
        /// try to parse it into an unsigned-integer and return true if successful.
        /// </summary>
        /// <param name="hexText">the text containing a value expressed as hexadecimal</param>
        /// <param name="result">the result is written to this unsigned-integer</param>
        /// <returns>true if successful, false on error</returns>
        /// <remarks>
        /// There are overloads of this method provided for parsing to UInt32, and ushort.
        /// 
        /// Input-text may include several typical hexadecimal indicators such as
        /// prefix "0x" or suffice "x" and "hex", which (if present) will be ignored.
        /// </remarks>
        public static bool TryParseFromHex( this string hexText, out UInt32 result )
        {
            result = 0;
            if (hexText == null)
            {
                return false;
            }
            string trimmedValue = hexText.Trim().TrimFromEndCaseInsensitve( "HEX" ).Trim().TrimFromEndCaseInsensitve( "X" ).Trim();
            try
            {
                result = Convert.ToUInt32( trimmedValue, 16 );
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Given a string representation of a number that is in hexadecimal form,
        /// try to parse it into an unsigned-integer and return true if successful.
        /// </summary>
        /// <param name="hexText">the text containing a value expressed as hexadecimal</param>
        /// <param name="result">the result is written to this unsigned-integer</param>
        /// <returns>true if successful, false on error</returns>
        /// <remarks>
        /// There are overloads of this method provided for parsing to UInt32, and ushort.
        /// 
        /// Input-text may include several typical hexadecimal indicators such as
        /// prefix "0x" or suffice "x" and "hex", which (if present) will be ignored.
        /// </remarks>
        public static bool TryParseFromHex( this string hexText, out ushort result )
        {
            result = 0;
            if (hexText == null)
            {
                return false;
            }
            string trimmedValue = hexText.Trim().TrimFromEndCaseInsensitve( "HEX" ).Trim().TrimFromEndCaseInsensitve( "X" ).Trim();
            try
            {
                result = Convert.ToUInt16( trimmedValue, 16 );
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

        #region WithCorrectGroupDelimiters
        /// <summary>
        /// Given an input-text that hopefully represents a numerical value, return a copy of it with the group-delimiters added (or corrected).
        /// </summary>
        /// <param name="inputText">the input-text to add the group-delimiters to</param>
        /// <param name="useSmallSpaceForDelimiter">set this to true to add Unicode-2006 character as delimiters around the colons, the "Six-per-EM" space (optional - default is false)</param>
        /// <returns>a copy of the input-text with the group-delimiters corrected</returns>
        /// <remarks>
        /// "Group-delimiters" means the separator character that is used to mark groups of three zeros, which is the comma within the en-US culture.
        /// 
        /// Space characters are trimmed from the front and back of the input-text before doing anything else.
        /// 
        /// If the input-text is an empty string, then this returns an empty string.
        /// </remarks>
        public static string WithCorrectGroupDelimiters( this string inputText, bool useSmallSpaceForDelimiter )
        {
            if (inputText == null)
            {
                throw new ArgumentNullException( "inputText" );
            }
            string inputTrimmmed = inputText.Trim().RemoveAll( ',' );
            if (inputTrimmmed.Length == 0)
            {
                return inputTrimmmed;
            }
            string integerPart = "";
            string fractionalPart = "";
            int indexOfDecimalPt = inputTrimmmed.IndexOf( '.' );
            if (indexOfDecimalPt > -1)
            {
                integerPart = inputTrimmmed.Substring( 0, indexOfDecimalPt );
                fractionalPart = inputTrimmmed.Substring( indexOfDecimalPt + 1 );
            }
            else
            {
                integerPart = inputTrimmmed;
            }
            string correctedIntegerPart = "";
            if (integerPart.Length > 0)
            {
                correctedIntegerPart = integerPart.AddDelimitersToTheLeft( 4, useSmallSpaceForDelimiter );
            }
            string correctedFractionalPart = "";
            if (fractionalPart.Length > 0)
            {
                correctedFractionalPart = fractionalPart.AddDelimitersToTheRight( 4, useSmallSpaceForDelimiter );
            }
            string result = "";
            if (correctedIntegerPart.Length > 0)
            {
                result = correctedIntegerPart;
            }
            else
            {
                result = "0";
            }
            if (correctedFractionalPart.Length > 0)
            {
                result = result + "." + correctedFractionalPart;
            }
            return result;
        }
        #endregion

        #region WithHexadecimalGroupDelimiters
        /// <summary>
        /// Given an input-text that hopefully represents a numerical value, return a copy of it with group-delimiters that
        /// are appropriate for hexadecimal representation added (or corrected).
        /// </summary>
        /// <param name="inputText">the input-text to add the group-delimiters to</param>
        /// <returns></returns>
        /// <remarks>
        /// This is an overload of the method
        /// WithHexadecimalGroupDelimiters( inputText,
        ///                                 int groupSize
        ///                                 char delimiter )
        /// with groupSize = 4, delimiter = StringLib.SmallSpace.
        /// </remarks>
        public static string WithHexadecimalGroupDelimiters( this string inputText )
        {
            return WithHexadecimalGroupDelimiters( inputText, 4, StringLib.SmallSpace );
        }

        /// <summary>
        /// Given an input-text that hopefully represents a numerical value, return a copy of it with group-delimiters that
        /// are appropriate for hexadecimal representation added (or corrected).
        /// </summary>
        /// <param name="inputText">the input-text to add the group-delimiters to</param>
        /// <param name="groupSize">the number of digits to regard as a single group (default is 4)</param>
        /// <param name="delimiter">the character to use to separate groups of digits (optional - default is Unicode-2006)</param>
        /// <returns></returns>
        public static string WithHexadecimalGroupDelimiters( this string inputText, int groupSize, char delimiter )
        {
            if (inputText == null)
            {
                throw new ArgumentNullException( "inputText" );
            }
            string inputTrimmmed = inputText.Trim().RemoveAll( ' ' ).RemoveAll( StringLib.SmallSpace ).RemoveAll( delimiter );
            if (inputTrimmmed.Length == 0 || groupSize == 0)
            {
                return inputTrimmmed;
            }
            string result = inputTrimmmed.AddSpacesToTheLeftForHex( groupSize, delimiter );
            return result;
        }
        #endregion

        #region internal implementation

        #region static ctor
        static MathLib()
        {
            //_cultureInfo = (CultureInfo)CultureInfo.CurrentCulture.Clone();
            _cultureInfo = new CultureInfo( "en-US" );
        }
        #endregion

        private static string AddSpacesToTheLeftForHex( this string input, int groupSize, char delimiter )
        {
            if (input.Length > groupSize)
            {
                return AddSpacesToTheLeftForHex( input.Substring( 0, input.Length - groupSize ), groupSize, delimiter ) + delimiter + input.Substring( input.Length - groupSize );
            }
            else
            {
                return input;
            }
        }

        private static string AddDelimitersToTheLeft( this string input, int minLengthForDelimiter, bool useSmallSpaceForDelimiter )
        {
            if (input.Length > minLengthForDelimiter)
            {
                return AddDelimitersToTheLeft( input.Substring( 0, input.Length - 3 ), 3, useSmallSpaceForDelimiter ) + (useSmallSpaceForDelimiter ? StringLib.SmallSpace : ',') + input.Substring( input.Length - 3 );
            }
            else
            {
                return input;
            }
        }

        private static string AddDelimitersToTheRight( this string input, int minLengthForDelimiter, bool useSmallSpaceForDelimiter )
        {
            if (input.Length > minLengthForDelimiter)
            {
                return input.Substring( 0, 3 ) + (useSmallSpaceForDelimiter ? StringLib.SmallSpace : ',') + AddDelimitersToTheRight( input.Substring( 3 ), 3, useSmallSpaceForDelimiter );
            }
            else
            {
                return input;
            }
        }

        #region fields

        private static CultureInfo _cultureInfo;

        #endregion fields

        #endregion internal implementation
    }
}
