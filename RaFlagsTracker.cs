using System;
using System.Collections.Generic;

namespace RaFlags
{
	public class RaFlagsTracker : IDisposable
	{
		public delegate void FlagHandler(object flag, RaFlagsTracker tracker);
		public event FlagHandler FlagRegisteredEvent;
		public event FlagHandler FlagUnregisteredEvent;
		public event FlagHandler FlagsChangedEvent;

		private HashSet<object> _flags = new HashSet<object>();

		private FlagHandler _flagsChangedCallback = null;

		public RaFlagsTracker(FlagHandler flagsChangedCallback = null)
		{
			_flagsChangedCallback = flagsChangedCallback;
		}

		public IReadOnlyCollection<object> Flags => _flags;

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
			if(_flags.Add(flag))
			{
				_flagsChangedCallback?.Invoke(flag, this);
				FlagRegisteredEvent?.Invoke(flag, this);
				FlagsChangedEvent?.Invoke(flag, this);
				return true;
			}
			return false;
		}

		public bool Unregister(object flag)
		{
			if(_flags.Remove(flag))
			{
				_flagsChangedCallback?.Invoke(flag, this);
				FlagUnregisteredEvent?.Invoke(flag, this);
				FlagsChangedEvent?.Invoke(flag, this);
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

			_flagsChangedCallback = null;
			_flags.Clear();
		}
	}
}