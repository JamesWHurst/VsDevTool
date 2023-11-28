using System;
using System.Windows.Data;


// See this article for the source of this: http://www.codeproject.com/Tips/720497/Binding-Radio-Buttons-to-a-Single-Property

namespace Hurst.BaseLibWpf.Converters
{
    public class RadioButtonCheckedConverter : IValueConverter
    {
        public object Convert( object value, Type targetType, object parameter,
                               System.Globalization.CultureInfo culture )
        {
            return value.Equals( parameter );
        }

        public object ConvertBack( object value, Type targetType, object parameter,
                                   System.Globalization.CultureInfo culture )
        {
            return value.Equals( true ) ? parameter : Binding.DoNothing;
        }
    }
}
