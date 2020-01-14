using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace FRCCANViewer.Converters
{
    public class MemoryConverter : IValueConverter
    {
        public static MemoryConverter Singleton = new MemoryConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var memory = (Memory<byte>)value;
            StringBuilder builder = new StringBuilder();
            builder.Append('[');
            bool first = true;
            foreach (var item in memory.Span)
            {
                if (first)
                {
                    first = false;
                }
                else
                {
                    builder.Append(", ");
                }
                builder.Append(item);
            }
            builder.Append(']');
            return builder.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
