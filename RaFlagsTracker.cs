using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RaFlags
{
	public class RaFlagsTracker : IDisposable
	{
		public delegate void FlagHandler(object flag, RaFlagsTracker tracker);
		public delegate void IsEmptyHandler(bool isEmpty, RaFlagsTracker tracker);

		public event FlagHandler FlagRegisteredEvent;
		public event FlagHandler FlagUnregisteredEvent;
		public event FlagHandler FlagsChangedEvent;
		public event IsEmptyHandler IsEmptyChangedEvent;

		private HashSet<object> _flags = new HashSet<object>();
		private bool _isLoggingEnabled = false;
		private string _loggingTag = string.Empty;

		private IsEmptyHandler _isEmptyChangedCallback = null;

		public RaFlagsTracker(IsEmptyHandler isEmptyChangedCallback = null)
		{
			_isEmptyChangedCallback = isEmptyChangedCallback;
		}

		public IReadOnlyCollection<object> Flags => _flags;

		public void EnableLogging(string tag)
		{
			if(_loggingTag == tag)
			{
				return;
			}

			_isLoggingEnabled = true;
			_loggingTag = tag;
			Log("Enabled Logging");
		}

		public void DisableLogging()
		{
			if(!_isLoggingEnabled)
			{
				return;
			}

			Log("Disabled Logging");
			_isLoggingEnabled = false;
			_loggingTag = default;
		}

		public bool IsEmpty(params object[] flagsToExclude)
		{
			if(flagsToExclude != null && flagsToExclude.Length > 0)
			{
				HashSet<object> temp = new HashSet<object>(_flags);
				for(int i = 0; i < flagsToExclude.Length; i++)
				{
					temp.Remove(flagsToExclude[i]);
				}
				return temp.Count == 0;
			}
			else
			{
				return _flags.Count == 0;
			}
		}

		public bool HasNone(object[] flags)
		{
			if(flags == null || flags.Length == 0)
			{
				return true;
			}

			foreach(object flag in flags)
			{
				if(HasFlag(flag))
				{
					return false;
				}
			}

			return true;
		}

		public bool HasAll(object[] flags)
		{
			if(flags == null || flags.Length == 0)
			{
				return true;
			}

			foreach(object flag in flags)
			{
				if(!HasFlag(flag))
				{
					return false;
				}
			}
			return true;
		}

		public bool HasAny(object[] flags)
		{
			if(flags == null || flags.Length == 0)
			{
				return true;
			}

			foreach(object flag in flags)
			{
				if(HasFlag(flag))
				{
					return true;
				}
			}
			return false;
		}

		public bool HasFlag(object flag)
		{
			return _flags.Contains(flag);
		}

		public bool Register(object flag)
		{
			bool isEmpty = IsEmpty();
			if(_flags.Add(flag))
			{
				bool newIsEmptyState = IsEmpty();
				bool hasChanged = isEmpty != newIsEmptyState;

				Log($"Registered {flag}. HasChanged: {hasChanged}");

				if(hasChanged)
				{
					_isEmptyChangedCallback?.Invoke(newIsEmptyState, this);
				}

				FlagRegisteredEvent?.Invoke(flag, this);
				FlagsChangedEvent?.Invoke(flag, this);

				if(hasChanged)
				{
					IsEmptyChangedEvent?.Invoke(newIsEmptyState, this);
				}
				return true;
			}
			return false;
		}

		public bool Unregister(object flag)
		{
			bool isEmpty = IsEmpty();
			if(_flags.Remove(flag))
			{
				bool newIsEmptyState = IsEmpty();
				bool hasChanged = isEmpty != newIsEmptyState;

				Log($"Unregister {flag}. HasChanged: {hasChanged}");

				if(hasChanged)
				{
					_isEmptyChangedCallback?.Invoke(newIsEmptyState, this);
				}

				FlagUnregisteredEvent?.Invoke(flag, this);
				FlagsChangedEvent?.Invoke(flag, this);

				if(hasChanged)
				{
					IsEmptyChangedEvent?.Invoke(newIsEmptyState, this);
				}
				return true;
			}
			return false;
		}

		public void Clear()
		{
			HashSet<object> temp = new HashSet<object>(_flags);
			foreach(object flag in temp)
			{
				Unregister(flag);
			}
		}

		public void Dispose()
		{
			FlagRegisteredEvent = null;
			FlagUnregisteredEvent = null;
			FlagsChangedEvent = null;
			IsEmptyChangedEvent = null;

			_isEmptyChangedCallback = null;
			_flags.Clear();

			_isLoggingEnabled = default;
			_loggingTag = default;
		}

		private void Log(string message)
		{
			if(_isLoggingEnabled)
			{
				Debug.WriteLine($"{nameof(RaFlagsTracker)} - {_loggingTag}: {message}");
			}
		}
	}
}