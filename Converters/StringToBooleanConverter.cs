using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AgentManagement.Avalonia.Converters
{
    public class StringToBooleanConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string viewMode && parameter is string param)
            {
                return viewMode == param;
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MonthlyViewBackgroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string viewMode && viewMode == "monthly")
            {
                return new SolidColorBrush(Color.Parse("#3B82F6"));
            }
            return new SolidColorBrush(Color.Parse("#FFFFFF"));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MonthlyViewForegroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string viewMode && viewMode == "monthly")
            {
                return new SolidColorBrush(Color.Parse("#FFFFFF"));
            }
            return new SolidColorBrush(Color.Parse("#6B7280"));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class YearlyViewBackgroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string viewMode && viewMode == "yearly")
            {
                return new SolidColorBrush(Color.Parse("#3B82F6"));
            }
            return new SolidColorBrush(Color.Parse("#FFFFFF"));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class YearlyViewForegroundConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string viewMode && viewMode == "yearly")
            {
                return new SolidColorBrush(Color.Parse("#FFFFFF"));
            }
            return new SolidColorBrush(Color.Parse("#6B7280"));
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
