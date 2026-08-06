using System;
using Ktisis.Core.Attributes;

namespace Ktisis.Events;

[Transient]
public class Event<T> : EventBase<T> where T : Delegate
{
	private void Enumerate(Action<object> func)
	{
		foreach (object subscriber in _subscribers)
		{
			try
			{
				func(subscriber);
			}
			catch (Exception ex)
			{
				Ktisis.Log.Error(ex.ToString());
			}
		}
	}

	public void Invoke()
	{
		Enumerate(delegate(object sub)
		{
			((Action)sub)();
		});
	}

	public void Invoke<T1>(T1 a1)
	{
		Enumerate(delegate(object sub)
		{
			((Action<T1>)sub)(a1);
		});
	}

	public void Invoke<T1, T2>(T1 a1, T2 a2)
	{
		Enumerate(delegate(object sub)
		{
			((Action<T1, T2>)sub)(a1, a2);
		});
	}

	public void Invoke<T1, T2, T3>(T1 a1, T2 a2, T3 a3)
	{
		Enumerate(delegate(object sub)
		{
			((Action<T1, T2, T3>)sub)(a1, a2, a3);
		});
	}
}
