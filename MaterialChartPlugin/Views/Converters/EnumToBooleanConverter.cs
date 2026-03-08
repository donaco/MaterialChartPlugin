using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace MaterialChartPlugin.Views.Converters
{
    /// <summary>
    /// enum値とbooleanを相互変換するコンバーター。
    /// ConverterParameterに指定したenum値と一致する場合にtrueを返します。
    /// </summary>
    public class EnumToBooleanConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            var enumValue = value.ToString();
            var targetValue = parameter.ToString();
            return enumValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Binding.DoNothing;

            if ((bool)value)
                return Enum.Parse(targetType, parameter.ToString());

            return Binding.DoNothing;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}