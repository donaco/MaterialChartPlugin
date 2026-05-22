using System;

namespace MaterialChartPlugin.Models.Settings
{
	/// <summary>
	/// MetroTrilithon.Serialization.ISerializationProvider をプラグイン内に内製化したインターフェースです。
	/// </summary>
	public interface ISerializationProvider
	{
		bool IsLoaded { get; }
		void Save();
		void Load();
		event EventHandler Reloaded;
		void SetValue<T>(string key, T value);
		bool TryGetValue<T>(string key, out T value);
		bool RemoveValue(string key);
	}
}
