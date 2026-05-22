using System.Collections.Generic;
using System.Linq;
using Livet;

namespace MaterialChartPlugin.ViewModels
{
	/// <summary>
	/// MetroTrilithon.Desktop の DisplayViewModel をプラグイン内に内製化したクラスです。
	/// </summary>
	public static class DisplayViewModel
	{
		public static DisplayViewModel<T> Create<T>(T value, string display)
		{
			return new DisplayViewModel<T> { Value = value, Display = display };
		}
	}

	public class DisplayViewModel<T> : ViewModel
	{
		private T _value;
		public T Value
		{
			get { return _value; }
			set
			{
				if (!Equals(_value, value))
				{
					_value = value;
					RaisePropertyChanged();
				}
			}
		}

		private string _display;
		public string Display
		{
			get { return _display; }
			set
			{
				if (_display != value)
				{
					_display = value;
					RaisePropertyChanged();
				}
			}
		}

		public static implicit operator T(DisplayViewModel<T> dvm) => dvm.Value;

		public override string ToString() => Display;
	}
}
