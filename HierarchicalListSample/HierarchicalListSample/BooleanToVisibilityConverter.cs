using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HierarchicalListSample
{
    // Optional: Bold font for parent nodes
    public class BoolToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is bool hasChildren && hasChildren)
                ? FontWeights.SemiBold
                : FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    } 
}