using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace MaterialChartPlugin.Models.Settings
{
	/// <summary>
	/// MetroTrilithon.Serialization.SerializablePropertyBase をプラグイン内に内製化したクラスです。
	/// </summary>
	[DebuggerDisplay("Value={Value}, Key={Key}, Default={Default}")]
	public abstract class SerializablePropertyBase<T> : INotifyPropertyChanged
	{
		private T _value;
		private bool _cached;

		public string Key { get; }
		public ISerializationProvider Provider { get; }
		public bool AutoSave { get; set; }
		public T Default { get; }

		public virtual T Value
		{
			get
			{
				if (_cached) return _value;
				if (!Provider.IsLoaded) Provider.Load();

				T val;
				if (Provider.TryGetValue(Key, out val))
				{
					_value = val;
					_cached = true;
				}
				else
				{
					_value = Default;
				}
				return _cached ? _value : Default;
			}
			set
			{
				if (_cached && Equals(_value, value)) return;
				if (!Provider.IsLoaded) Provider.Load();

				var old = _value;
				_value = value;
				_cached = true;
				Provider.SetValue(Key, value);
				OnValueChanged(old, value);

				if (AutoSave) Provider.Save();
			}
		}

		protected SerializablePropertyBase(string key, ISerializationProvider provider)
			: this(key, provider, default(T)) { }

		protected SerializablePropertyBase(string key, ISerializationProvider provider, T defaultValue)
		{
			if (key == null) throw new ArgumentNullException(nameof(key));
			if (provider == null) throw new ArgumentNullException(nameof(provider));

			Key = key;
			Provider = provider;
			Default = defaultValue;

			Provider.Reloaded += (sender, args) =>
			{
				if (_cached)
				{
					_cached = false;
					var oldValue = _value;
					var newValue = Value;
					if (!Equals(oldValue, newValue)) OnValueChanged(oldValue, newValue);
				}
				else
				{
					OnValueChanged(default(T), Value);
				}
			};
		}

		public virtual IDisposable Subscribe(Action<T> listener)
		{
			listener(Value);
			return new ValueChangedEventListener(this, listener);
		}

		public static implicit operator T(SerializablePropertyBase<T> property) => property.Value;

		public event EventHandler<ValueChangedEventArgs<T>> ValueChanged;

		protected virtual void OnValueChanged(T oldValue, T newValue)
		{
			ValueChanged?.Invoke(this, new ValueChangedEventArgs<T>(oldValue, newValue));
		}

		private readonly Dictionary<PropertyChangedEventHandler, EventHandler<ValueChangedEventArgs<T>>> _handlers
			= new Dictionary<PropertyChangedEventHandler, EventHandler<ValueChangedEventArgs<T>>>();

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add { ValueChanged += (_handlers[value] = (sender, args) => value(sender, new PropertyChangedEventArgs(nameof(Value)))); }
			remove
			{
				EventHandler<ValueChangedEventArgs<T>> handler;
				if (_handlers.TryGetValue(value, out handler))
				{
					ValueChanged -= handler;
					_handlers.Remove(value);
				}
			}
		}

		private class ValueChangedEventListener : IDisposable
		{
			private readonly Action<T> _listener;
			private readonly SerializablePropertyBase<T> _source;

			public ValueChangedEventListener(SerializablePropertyBase<T> property, Action<T> listener)
			{
				_listener = listener;
				_source = property;
				_source.ValueChanged += HandleValueChanged;
			}

			private void HandleValueChanged(object sender, ValueChangedEventArgs<T> args)
				=> _listener(args.NewValue);

			public void Dispose() => _source.ValueChanged -= HandleValueChanged;
		}
	}

	public class ValueChangedEventArgs<T> : EventArgs
	{
		public T OldValue { get; }
		public T NewValue { get; }
		public ValueChangedEventArgs(T oldValue, T newValue) { OldValue = oldValue; NewValue = newValue; }
	}
}
