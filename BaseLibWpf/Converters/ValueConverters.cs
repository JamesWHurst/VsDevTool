using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
#if USE_TRISTATEBOOLEAN
using System.Text;
#endif
using Hurst.LogNut.Util;


namespace Hurst.BaseLibWpf
{
    #region InverseBooleanConverter
    /// <summary>
    /// Converts a boolean value to it's opposite state (ie, true => false, and false => true)
    /// </summary>
    /// <remarks>
    /// Here is it's sample usage in XAML:
    /// Suggested resource code:<code> <baseLibWpf:InverseBooleanConverter x:Key="boolNotConv" /> </code>
    /// </remarks>
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            if (targetType != typeof( bool ))
            {
                throw new ArgumentException( "The target must be a boolean" );
            }
            return !(bool)value;
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
    #endregion

    #region The Visibility value converters: A set of value converters that map a Boolean (or a nullable-Boolean) to a Visibility value.

    #region BoolToVisibleVCollapsedConverter (deprecated, use this instead: <BooleanToVisibilityConverter x:Key="boolToVisiblityConv" /> )
    /// <summary>
    /// The Boolean-to-Visible-vs-Collapsed ValueConverter.
    /// This converts between a boolean and a WPF visibility enum value; true maps to Visible, false maps to Collapsed.
    /// </summary>
    /// <remarks>
    /// The source for this derived from this article: http://www.timhibbard.com/Blog.aspx
    /// It's for the scenario in which I wish to make a control visible based upon the state of a checkbox (for example).
    /// Here is it's sample usage in XAML:
    /// Suggested resource code:<code> <jh:BoolToVisibleVCollapsedConverter x:Key="boolToVisibleConv" /> </code>
    /// <code>
    /// <Window x:Class="DF.Windows.Window1"
    ///  xmlns:converters="clr-namespace:Hurst.Converters">
    ///     <StackPanel Orientation="Vertical">
    ///         <StackPanel.Resources>
    ///             <converters:BoolToHiddenVisibility x:Key="boolToVis"/>
    ///         </StackPanel.Resources>
    ///         <CheckBox Content="Check to show text box below me" Name="ckbxViewTextBox"/>
    ///         <TextBox Text="Only seen when above is checked!" Visibility="{Binding Path=IsChecked, ElementName=ckbxViewTextBox, Converter={StaticResource boolToVis}}"/>
    ///     </StackPanel>
    /// </Window>
    /// </code>
    /// </remarks>
    public class BoolToVisibleVCollapsedConverter : IValueConverter
    {
        #region Convert
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            Visibility rv = Visibility.Visible;
            try
            {
                bool bValue = true;
                if (bool.TryParse( value.ToString(), out bValue ))
                {
                    if (!bValue)
                    {
                        rv = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception x)
            {
                    Debug.WriteLine( "There was an exception within BoolToVisibleVCollapsedConverter: " + x.Message );
            }
            return rv;
        }
        #endregion

        #region ConvertBack
        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return value;
        }
        #endregion
    }
    #endregion

#if !SILVERLIGHT

    #region BoolToVisibleVHiddenConverter
    /// <summary>
    /// This subclass of IValueConverter converts a boolean, or a nullable-boolean (as from the IsChecked property of a CheckBox)
    /// to either Visibility.Visible (if the value is True) or Visibility.Hidden (if the value is False).
    /// </summary>
    /// <remarks>
    /// Suggested resource code: <jh:BoolToVisibleVHiddenConverter x:Key="boolToVisibleVHiddenConv" />
    /// </remarks>
    public class BoolToVisibleVHiddenConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            bool bOn = false;
            Visibility rv = Visibility.Visible;
            Boolean? nbValue = value as Boolean?;
            if (nbValue == null)
            {
                bool bV = (bool)value;
                if (bV)
                {
                    bOn = true;
                }
            }
            else
            {
                if (nbValue.HasValue && nbValue.Value)
                {
                    bOn = true;
                }
            }
            if (!bOn)
            {
                rv = Visibility.Hidden;
            }
            return rv;
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
    #endregion

    #region NullableBoolToVisibleVHiddenConverter
    /// <summary>
    /// This subclass of IValueConverter converts a nullable-boolean (as from the IsChecked property of a CheckBox)
    /// to either Visibility.Visible (if the value is True or null) or Visibility.Hidden (if the value is False).
    /// </summary>
    /// <remarks>
    /// Suggested resource code: <baselib:NullableBoolToVisibleVHiddenConverter x:Key="nullableBoolToVisibleVHiddenConv" />
    /// </remarks>
    public class NullableBoolToVisibleVHiddenConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            Boolean? nbValue = value as Boolean?;
            if (nbValue != null)
            {
                if (nbValue.HasValue && !nbValue.Value)
                {
                    return Visibility.Hidden;
                }
            }
            return Visibility.Visible;
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
    #endregion

    #region NullableBoolNonNullToVisibleVHiddenConverter
    /// <summary>
    /// This subclass of IValueConverter converts a nullable-boolean
    /// to either Visibility.Visible (if the value is false or true) or Visibility.Hidden (if the value is null).
    /// </summary>
    /// <remarks>
    /// Suggested resource code: <baselib:NullableBoolNonNullToVisibleVHiddenConverter x:Key="nullableBoolNonNullToVisibleVHiddenConv" />
    /// </remarks>
    public class NullableBoolNonNullToVisibleVHiddenConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            Boolean? nbValue = value as Boolean?;
            if (nbValue != null)
            {
                if (nbValue.HasValue)
                {
                    return Visibility.Visible;
                }
            }
            return Visibility.Hidden;
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
    #endregion

    #region BoolToHiddenConverter
    /// <summary>
    /// The logical-not of the BoolToVisibilityConverter, this maps True to Hidden, and False to Visible.
    /// </summary>
    /// <remarks>
    /// Suggested resource code: <jh:BoolToHiddenConverter x:Key="boolToHiddenConv" />
    /// </remarks>
    public class BoolToHiddenConverter : IValueConverter
    {

        /// <summary>
        /// Convert from source-type to target-type
        /// </summary>
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            bool bOn = false;
            Visibility rv = Visibility.Visible;
            Boolean? nbValue = value as Boolean?;
            if (nbValue == null)
            {
                bool bV = (bool)value;
                if (bV)
                {
                    bOn = true;
                }
            }
            else // it's a nullable-Boolean
            {
                if (nbValue.HasValue && nbValue.Value)
                {
                    bOn = true;
                }
            }
            if (bOn)
            {
                rv = Visibility.Hidden;
            }
            return rv;
        }

        /// <summary>
        /// Convert-back from target to source (unimplemented in this case)
        /// </summary>
        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
    #endregion

    #region BoolToCollapsedConverter
    /// <summary>
    /// The opposite of the above.
    /// true maps to Collapsed, false maps to Visible
    /// </summary>
    /// <remarks>
    /// Suggested resource code: <jh:BoolToCollapsedConverter x:Key="boolToCollapsedConv" />
    /// </remarks>
    public class BoolToCollapsedConverter : IValueConverter
    {
        #region Convert
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            Visibility rv = Visibility.Visible;
            try
            {
                bool bValue = true;
                if (bool.TryParse( value.ToString(), out bValue ))
                {
                    if (bValue)
                    {
                        rv = Visibility.Collapsed;
                    }
                }
            }
            catch (Exception x)
            {
                //TODO: Probably should NOT be using an exception here like this.  jh
                // At least, we should log it.
                Debug.WriteLine( "There was an exception within BoolToCollapsedConverter: " + x.Message );
            }
            return rv;
        }
        #endregion

        #region ConvertBack
        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return value;
        }
        #endregion
    }
    #endregion

#if USE_TRISTATEBOOLEAN
    #region TriStateBoolToVisibleVHiddenConverter
    /// <summary>
    /// This subclass of IValueConverter converts a TriStateBoolean
    /// to either Visibility.Visible (if the value is True or null) or Visibility.Hidden (if the value is False).
    /// </summary>
    /// <remarks>
    /// Suggested resource code: <jh:TriStateBoolToVisibleVHiddenConverter x:Key="tristateToVisibleVHiddenConv" />
    /// </remarks>
    public class TriStateBoolToVisibleVHiddenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility rv = Visibility.Visible;
            if (value != null)
            {
#if DEBUG
                try
                {
#endif
                    TriStateBoolean theValue = (TriStateBoolean)value;
                    if (theValue.HasValue)
                    {
                        if (theValue.IsFalse)
                        {
                            rv = Visibility.Hidden;
                        }
                    }
#if DEBUG
                }
                catch (Exception x)
                {
                    var loggingProvider = Application.Current as ILoggingProvider;
                    if (loggingProvider != null)
                    {
                        var sb = new StringBuilder("in TriStateBoolToVisibleVHiddenConverter, value=");
                        sb.Append(value).Append("; ").Append(StringLib.ExceptionDetails(x,true,null,null,null,0,false));
                        loggingProvider.Logger.LogError(sb);
                    }
                }
#endif
            }
            return rv;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
    #endregion

#endif
#endif

    #region TextToVisibleConverter
    /// <summary>
    /// Given a text value, map it to Visibility.Visible if it is non-empty, otherwise to Collapsed.
    /// </summary>
    /// <remarks>
    /// Suggested resource code: <jh:TextToVisibleConverter x:Key="textToVisibleConv" />
    /// </remarks>
    public class TextToVisibleConverter : IValueConverter
    {
        /// <summary>
        /// Convert from source-type to target-type
        /// </summary>
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            Visibility rv = Visibility.Collapsed;
            if (value != null)
            {
                string text = value.ToString();
                if (StringLib.HasSomething( text ))
                {
                    rv = Visibility.Visible;
                }
            }
            return rv;
        }

        /// <summary>
        /// Convert-back from target to source (unimplemented in this case)
        /// </summary>
        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
    #endregion

    #endregion The Visibility value converters.

    #region ColorConverter

#if SILVERLIGHT
    public class ColorConverter : IValueConverter
    {
        // Thanks to Alex Golesh for this one: http://blogs.microsoft.co.il/blogs/alex_golesh/archive/2008/04/29/colorconverter-in-silverlight-2.aspx

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string val = value.ToString();
            // Get rid of the #
            val = val.Replace("#", "");

            byte a = System.Convert.ToByte("ff", 16);
            byte pos = 0;
            if (val.Length == 8)
            {
                a = System.Convert.ToByte(val.Substring(pos, 2), 16);
                pos = 2;
            }

            byte r = System.Convert.ToByte(val.Substring(pos, 2), 16);
            pos += 2;

            byte g = System.Convert.ToByte(val.Substring(pos, 2), 16);
            pos += 2;

            byte b = System.Convert.ToByte(val.Substring(pos, 2), 16);

            Color col = Color.FromArgb(a, r, g, b);
            if (targetType == typeof(Color))
            {
                return col;
            }
            else
            {
            return new SolidColorBrush(col);
        }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            SolidColorBrush val = value as SolidColorBrush;
            //TODO: Check that this produces a string with the right number of characters for each component. Seems like this would be wrong. jh
            return "#" + val.Color.A.ToString() + val.Color.R.ToString() + val.Color.G.ToString() + val.Color.B.ToString();
        }
    }
#endif
    #endregion

    #region DateTimeToJustTimeConverter
    public class DateTimeToJustTimeConverter : System.Windows.Data.IValueConverter
    {
        public object Convert( object value, System.Type targetType, object parameter, CultureInfo culture )
        {
            string sResult = "";
            DateTime? t = value as DateTime?;
            if (t != null)
            {
                sResult = String.Format( "{0:h:mm tt}", t );
            }
            return sResult;
        }

        public object ConvertBack( object value, System.Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
    #endregion DateTimeToJustTimeConverter

    #region DateTimeToMinimumConverter
    /// <summary>
    /// Convert the given nullable-DateTime to a string, omitting the date portion if that happens to be today,
    /// or the year portion if it's not today but is within the same year.
    /// </summary>
    public class DateTimeToMinimumConverter : System.Windows.Data.IValueConverter
    {
        public object Convert( object value, System.Type targetType, object parameter, CultureInfo culture )
        {
            string sResult = "";
            DateTime? nt = value as DateTime?;
            if (nt != null)
            {
                DateTime t = nt.Value;
                sResult = t.ToStringMinimum();
            }
            return sResult;
        }

        public object ConvertBack( object value, System.Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }
    }
    #endregion DateTimeToMinimumConverter

    #region EnumToBooleanConverter
    /// <summary>
    /// This is for providing a simplified means of binding to enumeration types from WPF.
    /// </summary>
    public class EnumToBooleanConverter : IValueConverter
    {
        // http://stackoverflow.com/questions/397556/how-to-bind-radiobuttons-to-an-enum

        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return value.Equals( parameter );
        }

        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            return value.Equals( true ) ? parameter : Binding.DoNothing;
        }
    }
    #endregion

    #region IntegerToStringConverter
    //TODO: Doesn't WPF have this already?
    public class IntegerToStringConverter : IValueConverter
    {
        #region Convert
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            string sValue = "?";
            try
            {
                sValue = value.ToString();
            }
            catch (Exception x)
            {
                Debug.WriteLine( "There was an exception within IntegerToStringConverter: " + x.Message );
            }
            return sValue;
        }
        #endregion

        #region ConvertBack
        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            int iValue = 0;
            string sInputValue = value as String;
            if (StringLib.HasSomething( sInputValue ))
            {
                if (!Int32.TryParse( sInputValue, out iValue ))
                {
                    Debug.WriteLine( "in IntegerToStringConverter.ConvertBack, parse of sInputValue \"" + sInputValue + "\" failed." );
                }
            }
            else
            {
                Debug.WriteLine( "in IntegerToStringConverter.ConvertBack, sInputValue is null or empty." );
            }
            return iValue;
        }
        #endregion
    }
    #endregion

    #region BooleanToYesNoConverter
    /// <summary>
    /// Given a Boolean value, map it to the corresponding string representation of it
    /// with a true being represented by "Yes", and false by "No".
    /// </summary>
    /// <remarks>
    /// Suggested resource code: <jh:BooleanToYesNoConverter x:Key="boolToYesNoConv" />
    /// </remarks>
    public class BooleanToYesNoConverter : System.Windows.Data.IValueConverter
    {
        /// <summary>
        /// Convert from source-type to target-type
        /// </summary>
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            string result;
            bool theBoolean = (bool)value;
            if (theBoolean == true)
            {
                result = "Yes";
            }
            else
            {
                result = "No";
            }
            return result;
        }

        /// <summary>
        /// Convert-back from target to source.
        /// </summary>
        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            throw new Exception( "Not freakn implemented!" );
        }
    }
    #endregion

    #region NullableBooleanToNotValueConverter
    /// <summary>
    /// Given a nullable bool, map it to the corresponding opposite state if non-null,
    /// otherwise just leave it as null.
    /// </summary>
    /// <remarks>
    /// Suggested resource code: <baselibWpf:NullableBooleanToNotValueConverter x:Key="nullableBoolToNotConv" />
    /// </remarks>
    public class NullableBooleanToNotValueConverter : IValueConverter
    {
        /// <summary>
        /// Convert from source-type to target-type
        /// </summary>
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            if (targetType == typeof( bool ))
            {
                return !(bool)value;
            }
            else if (targetType == typeof( bool? ))
            {
                bool? result = null;
                bool? nullableBoolean = value as bool?;
                if (nullableBoolean.HasValue)
                {
                    if (nullableBoolean.Value == true)
                    {
                        result = false;
                    }
                    else
                    {
                        result = true;
                    }
                }
                return result;
            }
            else
            {
                throw new ArgumentException( "The target must be either a boolean or a nullable-boolean" );
            }
        }

        /// <summary>
        /// Convert-back from target to source.
        /// </summary>
        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            if (targetType == typeof( bool ))
            {
                return !(bool)value;
            }
            else if (targetType == typeof( bool? ))
            {
                bool? result = null;
                bool? nullableBoolean = value as bool?;
                if (nullableBoolean.HasValue)
                {
                    if (nullableBoolean.Value == true)
                    {
                        result = false;
                    }
                    else
                    {
                        result = true;
                    }
                }
                return result;
            }
            else
            {
                throw new ArgumentException( "To ConvertBack, the target must be either a boolean or a nullable-boolean" );
            }
        }
    }
    #endregion

    #region NullableBooleanToYesNoOrNoValueConverter
    /// <summary>
    /// Given a nullable bool, map it to the corresponding string representation of it
    /// with a null being indicated by "no value", true by "Yes", and false by "No".
    /// </summary>
    /// <remarks>
    /// Suggested resource code: <jh:NullableBooleanToYesNoOrNoValueConverter x:Key="nullableBoolToYesNoConv" />
    /// </remarks>
    public class NullableBooleanToYesNoOrNoValueConverter : IValueConverter
    {
        /// <summary>
        /// Convert from source-type to target-type
        /// </summary>
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            string result = "no value";
            bool? nullableBoolean = value as bool?;
            if (nullableBoolean.HasValue)
            {
                if (nullableBoolean.Value == true)
                {
                    result = "Yes";
                }
                else
                {
                    result = "No";
                }
            }
            return result;
        }

        /// <summary>
        /// Convert-back from target to source.
        /// </summary>
        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            bool? result = null;
            string stringValue = value as string;
            if (StringLib.HasSomething( stringValue ))
            {
                string x = stringValue.Trim().ToLower();
                if (x == "0" || x == "no" || x == "false" || x == "off" || x == "zero")
                {
                    return false;
                }
                else if (x == "1" || x == "yes" || x == "true" || x == "on" || x == "one")
                {
                    return true;
                }
            }
            return result;
        }
    }
    #endregion

    #region NullableIntegerToValueConverter
    /// <summary>
    /// Given a nullable integer, map it to the corresponding integer value (or lack thereof)
    /// with a null being indicated by an empty field, as needed for binding to a Telerik RadNumericUpDown control.
    /// </summary>
    /// <remarks>
    /// Suggested resource code: <jh:NullableIntegerToValueConverter x:Key="nullableIntegerToValueConv" />
    /// </remarks>
    public class NullableIntegerToValueConverter : IValueConverter
    {
        /// <summary>
        /// Convert from source-type to target-type, ie from int? to int
        /// </summary>
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            int? nullableInteger = value as int?;
            if (nullableInteger != null && nullableInteger.HasValue)
            {
                return nullableInteger.Value;
            }
            else
            {
                return DependencyProperty.UnsetValue;
            }
        }

        /// <summary>
        /// Convert-back from target to source, ie from int to int?
        /// </summary>
        public object ConvertBack( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            int? result = null;
            double numericValue = (double)value;
            string stringValue = StringLib.AsString( value );
            Debug.WriteLine( "NullableIntegerToValueConverter.ConvertBack, value is " + stringValue + ", value.GetType() = " + value.GetType() );
            //var r = ParseLib.ParseForInteger(stringValue, false);
            //if (!r.IsEmpty && r.IsOk)
            //{
            //    Debug.WriteLine("  and r.IntegerValue is " + r.IntegerValue);
            //    result = r.IntegerValue;
            //}
            result = (int)numericValue;
            return result;
        }
    }
    #endregion

    #region StringFormatConverter
    /// <summary>
    /// Enables XAML string formatting.
    /// </summary>
    /// <example>
    /// <code>
    /// Text="{Binding Text, Converter=stringFormatConv, ConverterParameter=0.00}"
    /// </code>
    /// </example>
    //[ValueConversion(typeof(object), typeof(string))]
    //public class StringFormatConverter : IValueConverter, IMultiValueConverter
    //{
    // From http://lambert.geek.nz/2009/03/03/stringformatconverter/
    // Even though as of .NET 3.5 we have the StringFormat property, that only works when the target property
    // is exactly a string. For object properties -- it fails. Thus this is still needed.

    // Usage:
    //
    // ToolTip="{Binding Path=Hostname,
    //          Converter={StaticResource stringFormatConv},
    //          ConverterParameter=Connected to {0}}"
    //
    // Or, for a multi-binding:
    //<TextBlock>
    //  <TextBlock.Text>
    //    <Binding Converter="{StaticResource stringFormatConv}"
    //             ConverterParameter="{}{0}: {1}">
    //      <Binding Path="When" />
    //      <Binding Path="Message" />
    //    </Binding>
    //  </TextBlock.Text>
    //</TextBlock>

    //public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    //{
    //    return Convert(new object[] { value }, targetType, parameter, culture);
    //}

    //public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    //{
    //    System.Diagnostics.Trace.TraceError("StringFormatConverter: does not support TwoWay or OneWayToSource bindings.");
    //    return DependencyProperty.UnsetValue;
    //}

    //public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    //{
    //    try
    //    {
    //        string format = (parameter == null) ? null : parameter.ToString();
    //        if (String.IsNullOrEmpty(format))
    //        {
    //            var sb = new System.Text.StringBuilder();
    //            for (int index = 0; index < values.Length; ++index)
    //            {
    //                sb.Append("{" + index + "}");
    //            }
    //            format = sb.ToString();
    //        }
    //        return String.Format(/*culture,*/ (string)parameter, values);
    //    }
    //    catch (Exception x)
    //    {
    //        System.Diagnostics.Trace.TraceError("StringFormatConverter({0}): {1}", parameter, ex.Message);
    //        return DependencyProperty.UnsetValue;
    //    }
    //}

    //public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    //{
    //    System.Diagnostics.Trace.TraceError("StringFormatConverter: does not support TwoWay or OneWayToSource bindings.");
    //    return null;
    //}

    //-----
    // This is taken straight from the article by Jan-Cornelius Molnar
    //public object Convert(object value, System.Type targetType, object parameter, CultureInfo culture) {
    //    string sFormat = (string)parameter;
    //    return String.Format("{0:" + sFormat + "}", value);
    //}
    //public object ConvertBack(object value, System.Type targetType, object parameter, CultureInfo culture) {
    //    return System.Convert.ChangeType(value, targetType);
    //}
    //}
    #endregion

    #region TimeSpanConverter
    /// <summary>
    /// This converts TimeSpan objects into a string representation in the format "{0:D2}:{1:D2}" with minutes/seconds.
    /// </summary>
    public class TimeSpanConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter, System.Globalization.CultureInfo culture )
        {
            TimeSpan timeSpan = (TimeSpan)value;
            String result = String.Empty;
            if (timeSpan != null)
            {
                result = String.Format( "{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds );
            }
            return result;
        }

        public object ConvertBack( object value, Type targetType, object paramter, CultureInfo culture )
        {
            return null;
        }
    }
    #endregion
}
