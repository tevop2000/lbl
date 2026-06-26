using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace AgentManagement.Avalonia.Converters
{
    /// <summary>
    /// 布尔值转换器：将整数转换为布尔值，支持反转
    /// </summary>
    public class BoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                bool result = count > 0;
                
                // 如果参数是 "Invert"，则反转结果
                if (parameter is string param && param == "Invert")
                {
                    return !result;
                }
                
                return result;
            }
            
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
