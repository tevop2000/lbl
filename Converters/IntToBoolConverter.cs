using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;

namespace AgentManagement.Avalonia.Converters
{
    /// <summary>
    /// 整数到布尔值转换器：判断整数是否等于0（无参数时）或等于指定参数值
    /// </summary>
    public class IntToBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                if (parameter is string param && int.TryParse(param, out int targetValue))
                {
                    return intValue == targetValue;
                }
                // 无参数时，默认判断是否等于0
                return intValue == 0;
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
            {
                if (parameter is string param && int.TryParse(param, out int targetValue))
                {
                    return targetValue;
                }
                return 0;
            }
            return BindingOperations.DoNothing;
        }
    }

    /// <summary>
    /// 反向整数到布尔值转换器：判断整数是否不等于0（无参数时）或不等于指定参数值
    /// </summary>
    public class InverseIntToBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                if (parameter is string param && int.TryParse(param, out int targetValue))
                {
                    return intValue != targetValue;
                }
                // 无参数时，默认判断是否不等于0
                return intValue != 0;
            }
            return false;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool boolValue && boolValue)
            {
                if (parameter is string param && int.TryParse(param, out int targetValue))
                {
                    // 返回一个不等于targetValue的值
                    return targetValue + 1;
                }
                return 1;
            }
            return BindingOperations.DoNothing;
        }
    }

    /// <summary>
    /// 当IsFinish为1（可修改）时返回空值，否则返回原始值
    /// </summary>
    public class IsFinishToEmptyConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            // values[0] 是 IsFinish，values[1] 是要转换的值
            if (values.Count < 2)
                return null;

            if (values[0] is int isFinish && isFinish == 1)
            {
                // 可修改状态，返回空值
                return null;
            }

            // 已结算状态，返回原始值
            return values[1];
        }
    }

    /// <summary>
    /// 根据IsFinish值返回不同的标题文本：IsFinish=1返回"盈亏平衡点销量"，否则返回"实际销量"
    /// </summary>
    public class IsFinishTextConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            // values[0] 是 IsFinish（int），values[1] 是 IsFinish 转换后的 bool（IsFinish==1时为true）
            if (values.Count < 1)
                return "实际销量";

            if (values[0] is int isFinish && isFinish == 1)
            {
                return "盈亏平衡点销量";
            }

            return "实际销量";
        }
    }

    /// <summary>
    /// 根据IsFinish值返回不同的值：IsFinish=1返回BreakSales，否则返回ActualSales
    /// </summary>
    public class IsFinishValueConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            // values[0] 是 IsFinish，values[1] 是 BreakSales，values[2] 是 ActualSales
            if (values.Count < 3)
                return string.Empty;

            if (values[0] is int isFinish && isFinish == 1)
            {
                // 可修改状态，返回盈亏平衡点销量
                return values[1] ?? string.Empty;
            }

            // 已结算状态，返回实际销量
            return values[2] ?? string.Empty;
        }
    }
}
