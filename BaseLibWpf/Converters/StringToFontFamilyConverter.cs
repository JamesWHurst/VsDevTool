using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Hurst.BaseLib;
using Hurst.LogNut;


namespace Hurst.BaseLibWpf
{
    /// <summary>
    /// Converts a string to the corresponding FontFamily object
    /// </summary>
    /// <example>
    /// If the input value is "Arial Unicode MS", the result will be the System.Windows.Media.FontFamily that has that name.
    /// <code>
    /// Within your XAML:
    ///   conv:StringToFontFamilyConverter x:Key="stringToFontConv"
    /// 
    ///   FontFamily="{Binding MyFontString, Converter={StaticResource stringToFontConv}"
    /// </code>
    /// </example>
    public class StringToFontFamilyConverter : IValueConverter
    {
        public object Convert( object value, System.Type targetType, object parameter, CultureInfo culture )
        {
            if (value != null)
            {
                if (targetType == typeof( string ))
                {
                    Logger.Warn( "The value passed to StringToFontFamilyConverter is a " + value.GetType() + ", and targetType is String. Returning " + StringLib.AsString( value ) );
                    return value;
                }
                else if (targetType == typeof( FontFamily ))
                {
                    string fontName = value as string;
                    try
                    {
                        if (fontName != null)
                        {
                            FontFamily theFontFamily = new FontFamily(fontName);
                            // Having reached here, with no exception thrown -- we are in normal flow. Happy.
                            //Logger.LogDebug( "The value passed to StringToFontConverter is a string, and targetType is FontFamily. Returning FontFamily = " + StringLib.AsString( theFontFamily ) );
                            return theFontFamily;
                        }
                        else if (value.GetType() == typeof( FontFamily ))
                        {
                            // This may signal that the converter is being used redundantly?
                            Logger.LogDebug( "The value passed to StringToFontFamilyConverter is a FontFamily, and targetType is FontFamily. Returning FontFamily = " + StringLib.AsString( value ) );
                            return (FontFamily)value;
                        }
                        else // fontName is null, and value is not a FontFamily.
                        {
                            Logger.LogError( "StringToFontFamilyConverter: The value is a " + value.GetType() + " and targetType is FontFamily. Not able to convert - returning UnsetValue." );
                        }
                    }
                    catch (Exception x)
                    {
                        Logger.LogException( x, "value: " + StringLib.AsString( value ) + ", targetType: " + targetType + ". " );
                    }
                }
                else // targetType is neither FontFamily nor string.
                {
                    Logger.LogError( "StringToFontFamilyConverter: targetType must be either a String or a FontFamily. This is a " + StringLib.AsString( targetType ) );
                }
            }
            else // value is null.
            {
                Logger.LogError( "StringToFontFamilyConverter: value is null." );
            }
            return DependencyProperty.UnsetValue;
        }

        public object ConvertBack( object value, System.Type targetType, object parameter, CultureInfo culture )
        {
            throw new NotSupportedException();
        }

        #region Logger
        /// <summary>
        /// Get the Logger for this class to use.
        /// </summary>
        public static Logger Logger
        {
            get
            {
                if (_logger == null)
                {
                    _logger = LogManager.GetLogger( "StringToFontFamilyConverter" );
                }

                return _logger;
            }
        }
        private static Logger _logger;
        #endregion
    }
}
