using System.Collections.Generic;
using System.Linq;

namespace MaterialChartPlugin.ViewModels
{
	using CommunityToolkit.Mvvm.ComponentModel;

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

	public class DisplayViewModel<T> : ObservableObject
	{
		public T Value
		{
			get { return field; }
			set
			{
				if (!Equals(field, value))
				{
					field = value;
					this.OnPropertyChanged();
				}
			}
		}

		public string Display
		{
			get { return field; }
			set
			{
				if (field != value)
				{
					field = value;
					this.OnPropertyChanged();
				}
			}
		}

		public static implicit operator T(DisplayViewModel<T> dvm) => dvm.Value;

		public override string ToString() => Display;
	}
}
