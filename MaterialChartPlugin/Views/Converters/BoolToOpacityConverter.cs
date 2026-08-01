using System;
using System.Globalization;
using System.Windows.Data;

namespace MaterialChartPlugin.Views.Converters
{
	/// <summary>
	/// bool値を透明度に変換するコンバーター。
	/// true = 1.0 (完全表示), false = 0.3 (グレーアウト)
	/// </summary>
	public class BoolToOpacityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is bool boolValue)
			{
				return boolValue ? 1.0 : 0.3;
			}
			return 1.0;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
