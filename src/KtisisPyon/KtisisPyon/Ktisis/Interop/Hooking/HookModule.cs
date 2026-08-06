using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.Hooking;

namespace Ktisis.Interop.Hooking;

public abstract class HookModule : IHookModule, IDisposable
{
	private readonly IHookMediator _hook;

	private readonly List<IHookWrapper> Hooks = new List<IHookWrapper>();

	private bool _init;

	private bool IsDisposed;

	public bool IsInit
	{
		get
		{
			if (_init)
			{
				return !IsDisposed;
			}
			return false;
		}
	}

	protected HookModule(IHookMediator hook)
	{
		_hook = hook;
	}

	public virtual void EnableAll()
	{
		Hooks.ForEach(delegate(IHookWrapper hook)
		{
			hook.Enable();
		});
	}

	public virtual void DisableAll()
	{
		Hooks.ForEach(delegate(IHookWrapper hook)
		{
			hook.Disable();
		});
	}

	public void SetEnabled(bool enabled)
	{
		if (enabled)
		{
			EnableAll();
		}
		else
		{
			DisableAll();
		}
	}

	public bool TryGetHook<T>(out HookWrapper<T>? result) where T : Delegate
	{
		result = null;
		foreach (IHookWrapper hook in Hooks)
		{
			if (hook is HookWrapper<T> hookWrapper)
			{
				result = hookWrapper;
				return true;
			}
		}
		return false;
	}

	public virtual bool Initialize()
	{
		if (IsDisposed)
		{
			throw new Exception("Attempted to initialize disposed module.");
		}
		bool flag = _hook.Init(this);
		List<IHookWrapper> collection = GetHookWrappers().ToList();
		if (flag)
		{
			Hooks.AddRange(collection);
			flag &= OnInitialize();
		}
		if (!flag)
		{
			Dispose();
		}
		return _init = flag;
	}

	protected virtual bool OnInitialize()
	{
		return true;
	}

	private IEnumerable<IHookWrapper> GetHookWrappers()
	{
		FieldInfo[] fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		FieldInfo[] array = fields;
		foreach (FieldInfo fieldInfo in array)
		{
			IHookWrapper hookFromField;
			try
			{
				hookFromField = GetHookFromField(fieldInfo);
			}
			catch (Exception value)
			{
				Ktisis.Log.Error($"Failed to resolve hook for field '{fieldInfo.Name}':\n{value}");
				continue;
			}
			if (hookFromField != null)
			{
				yield return hookFromField;
			}
		}
	}

	private IHookWrapper? GetHookFromField(FieldInfo field)
	{
		Type fieldType = field.FieldType;
		if (!fieldType.IsGenericType || fieldType.GetGenericTypeDefinition() != typeof(Hook<>))
		{
			return null;
		}
		object value = field.GetValue(this);
		if (value == null)
		{
			return null;
		}
		return (IHookWrapper)Activator.CreateInstance(typeof(HookWrapper<>).GetGenericTypeDefinition().MakeGenericType(fieldType.GenericTypeArguments), value);
	}

	public virtual void Dispose()
	{
		if (IsDisposed)
		{
			return;
		}
		try
		{
			Hooks.ForEach(delegate(IHookWrapper hook)
			{
				hook.Dispose();
			});
			Hooks.Clear();
			_hook.Remove(this);
		}
		finally
		{
			IsDisposed = true;
			GC.SuppressFinalize(this);
		}
	}
}
