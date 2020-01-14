using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace FRCCANViewer.Converters
{
    public class HexidecimalConverter : IValueConverter
    {
        public static HexidecimalConverter EightCharHexConverter = new HexidecimalConverter("X8");
        public static HexidecimalConverter ThreeCharHexConverter = new HexidecimalConverter("X3");

        private readonly string conversionString;

        private HexidecimalConverter(string convStr)
        {
            this.conversionString = convStr;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "0x" + ((uint)value).ToString(conversionString);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
