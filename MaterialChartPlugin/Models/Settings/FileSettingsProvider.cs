using System;
using System.Collections.Generic;
using System.IO;
using System.Xaml;

namespace MaterialChartPlugin.Models.Settings
{
	/// <summary>
	/// ファイルベースの設定プロバイダーです（MetroTrilithon.Desktop より内製化）。
	/// </summary>
	public class FileSettingsProvider : ISerializationProvider
	{
		private readonly string _path;
		private readonly object _sync = new object();
		private SortedDictionary<string, object> _settings = new SortedDictionary<string, object>();

		public bool IsLoaded { get; private set; }

		public FileSettingsProvider(string path)
		{
			_path = path;
		}

		public void SetValue<T>(string key, T value)
		{
			lock (_sync) { _settings[key] = value; }
		}

		public bool TryGetValue<T>(string key, out T value)
		{
			lock (_sync)
			{
				object obj;
				if (_settings.TryGetValue(key, out obj) && obj is T)
				{
					value = (T)obj;
					return true;
				}
			}
			value = default(T);
			return false;
		}

		public bool RemoveValue(string key)
		{
			lock (_sync) { return _settings.Remove(key); }
		}

		public void Save()
		{
			if (_settings.Count == 0) return;
			var dir = Path.GetDirectoryName(_path);
			if (dir == null) throw new DirectoryNotFoundException();
			if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
			lock (_sync)
			{
				using (var stream = new FileStream(_path, FileMode.Create, FileAccess.ReadWrite))
				{
					XamlServices.Save(stream, _settings);
				}
			}
		}

		public void Load()
		{
			if (File.Exists(_path))
			{
				using (var stream = new FileStream(_path, FileMode.Open, FileAccess.Read))
				{
					lock (_sync)
					{
						var source = XamlServices.Load(stream) as IDictionary<string, object>;
						_settings = source == null
							? new SortedDictionary<string, object>()
							: new SortedDictionary<string, object>(source);
					}
				}
			}
			else
			{
				lock (_sync) { _settings = new SortedDictionary<string, object>(); }
			}
			IsLoaded = true;
		}

		event EventHandler ISerializationProvider.Reloaded
		{
			add { }
			remove { }
		}
	}
}
