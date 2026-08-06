using System;
using System.Collections.Generic;

namespace Ktisis.Events;

public abstract class EventBase<T> : IDisposable where T : Delegate
{
	protected readonly HashSet<object> _subscribers = new HashSet<object>();

	public bool Add(T subscriber)
	{
		lock (_subscribers)
		{
			return _subscribers.Add(subscriber);
		}
	}

	public bool Remove(T subscriber)
	{
		lock (_subscribers)
		{
			return _subscribers.Remove(subscriber);
		}
	}

	public void Dispose()
	{
		_subscribers.Clear();
	}
}
