using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Data.Converters;
using Avalonia.Media;
using AgentManagement.Avalonia.Models;

namespace AgentManagement.Avalonia.Converters
{
    /// <summary>
    /// 布尔值到背景颜色的转换器（用于代理商列表项背景）
    /// </summary>
    public class BoolToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected)
            {
                return isSelected 
                    ? new SolidColorBrush(Color.Parse("#EFF6FF"))  // 选中：浅蓝色
                    : new SolidColorBrush(Colors.Transparent);      // 未选中：透明
            }
            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 布尔值到头像背景颜色的转换器
    /// </summary>
    public class BoolToAvatarBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected)
            {
                return isSelected 
                    ? new SolidColorBrush(Color.Parse("#BFDBFE"))  // 选中：蓝色
                    : new SolidColorBrush(Color.Parse("#F3F4F6")); // 未选中：灰色
            }
            return new SolidColorBrush(Color.Parse("#F3F4F6"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 布尔值到文本颜色的转换器
    /// </summary>
    public class BoolToTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isSelected)
            {
                return isSelected 
                    ? new SolidColorBrush(Color.Parse("#1E40AF"))  // 选中：深蓝色
                    : new SolidColorBrush(Color.Parse("#111827")); // 未选中：黑色
            }
            return new SolidColorBrush(Color.Parse("#111827"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 净利润到颜色的转换器（正值蓝色，负值红色
    /// </summary>
    public class ProfitColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal profit)
            {
                if (profit >= 0)
                {
                    // 正值：蓝色
                    return new SolidColorBrush(Color.Parse("#3B82F6"));
                }
                else
                {
                    // 负值：红色
                    return new SolidColorBrush(Color.Parse("#EF4444"));
                }
            }
            // 默认蓝色
            return new SolidColorBrush(Color.Parse("#3B82F6"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 净利润到绿色/红色颜色的转换器（正值绿色，负值红色）
    /// </summary>
    public class ProfitGreenRedColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal profit)
            {
                if (profit >= 0)
                {
                    // 正值：绿色
                    return new SolidColorBrush(Color.Parse("#059669"));
                }
                else
                {
                    // 负值：红色
                    return new SolidColorBrush(Color.Parse("#EF4444"));
                }
            }
            if (value is double doubleProfit)
            {
                return doubleProfit >= 0
                    ? new SolidColorBrush(Color.Parse("#059669"))
                    : new SolidColorBrush(Color.Parse("#EF4444"));
            }
            // 默认绿色
            return new SolidColorBrush(Color.Parse("#059669"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 净利润到标签背景色的转换器
    /// </summary>
    public class ProfitLabelBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal profit)
            {
                if (profit >= 0)
                {
                    // 正值：浅绿背景
                    return new SolidColorBrush(Color.Parse("#D1FAE5"));
                }
                else
                {
                    // 负值：浅红背景
                    return new SolidColorBrush(Color.Parse("#FEE2E2"));
                }
            }
            // 默认浅绿
            return new SolidColorBrush(Color.Parse("#D1FAE5"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 净利润到标签文字的转换器（盈利/亏损）
    /// </summary>
    public class ProfitLabelTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal profit)
            {
                if (profit >= 0)
                {
                    return "盈利";
                }
                else
                {
                    return "亏损";
                }
            }
            if (value is double doubleProfit)
            {                
                if (doubleProfit >= 0)
                {
                    return "盈利";
                }
                else
                {
                    return "亏损";
                }
            }
            return "盈利";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 判断数字是否大于0的转换器
    /// </summary>
    public class GreaterThanZeroConverter : IValueConverter
    {
        public static GreaterThanZeroConverter Instance { get; } = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue > 0;
            }
            if (value is double doubleValue)
            {
                return doubleValue > 0;
            }
            if (value is decimal decimalValue)
            {
                return decimalValue > 0;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 判断数字是否等于0的转换器
    /// </summary>
    public class EqualZeroConverter : IValueConverter
    {
        public static EqualZeroConverter Instance { get; } = new();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue == 0;
            }
            if (value is double doubleValue)
            {
                return doubleValue == 0;
            }
            if (value is decimal decimalValue)
            {
                return decimalValue == 0;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 小数到百分比字符串转换器（不带百分号）
    /// 例如：0.25 → "25.00"，用户输入 "25" → 0.25
    /// </summary>
    public class DecimalToPercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
            {
                return (decimalValue * 100).ToString("F2");
            }
            if (value is double doubleValue)
            {
                return (doubleValue * 100).ToString("F2");
            }
            return "0.00";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && decimal.TryParse(stringValue, out decimal parsedValue))
            {
                return parsedValue / 100;
            }
            return 0m;
        }
    }

    /// <summary>
    /// 利润进度条宽度转换器（MultiValueConverter）
    /// 根据单月利润和季度总利润计算进度条宽度百分比
    /// </summary>
    public class PercentageWidthConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values == null || values.Count != 2)
                return 0.0;

            decimal currentValue = 0;
            if (values[0] is decimal decValue)
                currentValue = decValue;
            else if (values[0] is int intValue)
                currentValue = intValue;
            else if (values[0] is long longValue)
                currentValue = longValue;
            else if (values[0] is double doubleValue)
                currentValue = (decimal)doubleValue;

            var list = values[1] as System.Collections.IEnumerable;
            if (list == null)
                return 0.0;

            decimal total = 0;
            int count = 0;
            foreach (var item in list)
            {
                if (item is RateMonthlyDetail detail && detail.AdjustedNetProfit.HasValue)
                {
                    total += detail.AdjustedNetProfit.Value;
                    count++;
                }
            }

            if (total <= 0 || count == 0)
                return 0.0;

            double percentage = (double)(currentValue / total) * 100;
            return percentage;
        }

        public object[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 计算季度月度列表总调整后净利润
    /// </summary>
    public class TotalAdjustedNetProfitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Collections.IEnumerable list)
            {
                decimal total = 0;
                foreach (var item in list)
                {
                    if (item is RateMonthlyDetail detail && detail.AdjustedNetProfit.HasValue)
                    {
                        total += detail.AdjustedNetProfit.Value;
                    }
                }
                return total;
            }
            return 0m;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// 利润进度条Grid列宽度转换器
    /// 根据单月利润占季度总利润的比例计算Grid列宽度（返回*格式）
    /// </summary>
    public class ProfitWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal currentValue = 0;
            if (value is decimal decValue)
                currentValue = decValue;
            else if (value is decimal?)
                currentValue = ((decimal?)value).Value;
            else if (value is int intValue)
                currentValue = intValue;
            else if (value is long longValue)
                currentValue = longValue;
            else if (value is double doubleValue)
                currentValue = (decimal)doubleValue;

            var list = parameter as System.Collections.IEnumerable;
            if (list == null)
                return new GridLength(1, GridUnitType.Star);

            decimal total = 0;
            int count = 0;
            foreach (var item in list)
            {
                if (item is RateMonthlyDetail detail && detail.AdjustedNetProfit.HasValue)
                {
                    total += detail.AdjustedNetProfit.Value;
                    count++;
                }
            }

            if (total <= 0 || count == 0)
                return new GridLength(1, GridUnitType.Star);

            double weight = (double)(currentValue / total);
            if (weight <= 0)
                weight = 0.01;

            return new GridLength(weight, GridUnitType.Star);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MonthlyStatusBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int isFinish)
            {
                return isFinish == 1 
                        ? new SolidColorBrush(Color.FromRgb(220, 38, 38)) 
                        : new SolidColorBrush(Color.FromRgb(16, 185, 129));
            }
            return new SolidColorBrush(Color.FromRgb(16, 185, 129));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MonthlyStatusTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int isFinish)
            {
                return isFinish == 1 ? "可修改" : "已结算·不可修改";
            }
            return "已结算·不可修改";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DecimalToCurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal amount)
            {
                return amount >= 0 ? $"¥{amount:N0}" : $"-¥{Math.Abs(amount):N0}";
            }
            return "¥0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
