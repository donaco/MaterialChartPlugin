using MetroTrilithon.Serialization;

namespace MaterialChartPlugin.Models.Settings
{
	/// <summary>
	/// MetroTrilithon.Desktop の SerializableProperty をプラグイン内に内製化したクラスです。
	/// </summary>
	public sealed class SerializableProperty<T> : SerializablePropertyBase<T>
	{
		public SerializableProperty(string key, ISerializationProvider provider)
			: base(key, provider) { }

		public SerializableProperty(string key, ISerializationProvider provider, T defaultValue)
			: base(key, provider, defaultValue) { }
	}
}
