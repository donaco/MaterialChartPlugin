namespace MaterialChartPlugin.Models.Settings
{
	/// <summary>
	/// 内製化した SerializableProperty クラスです。
	/// </summary>
	public sealed class SerializableProperty<T> : SerializablePropertyBase<T>
	{
		public SerializableProperty(string key, ISerializationProvider provider)
			: base(key, provider) { }

		public SerializableProperty(string key, ISerializationProvider provider, T defaultValue)
			: base(key, provider, defaultValue) { }
	}
}
