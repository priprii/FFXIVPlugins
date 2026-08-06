using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ktisis.Actions.Attributes;
using Ktisis.Actions.Types;
using Ktisis.Core;
using Ktisis.Core.Attributes;
using Ktisis.Core.Types;

namespace Ktisis.Actions;

[Singleton]
public class ActionService
{
	private readonly DIBuilder _di;

	private readonly Dictionary<Type, ActionBase> Actions = new Dictionary<Type, ActionBase>();

	public ActionService(DIBuilder di)
	{
		_di = di;
	}

	public void RegisterActions(IPluginContext context)
	{
		Actions.Clear();
		foreach (var (type2, actionAttribute2) in ResolveActions())
		{
			try
			{
				ActionBase value = (ActionBase)_di.Create(type2, context);
				Actions.Add(type2, value);
			}
			catch (Exception value2)
			{
				Ktisis.Log.Error($"Failed to create action '{actionAttribute2.Name}'\n{value2}");
			}
		}
	}

	public T Get<T>() where T : ActionBase
	{
		return (T)Actions[typeof(T)];
	}

	public bool TryGet<T>(out T action) where T : ActionBase
	{
		T val = null;
		if (Actions.TryGetValue(typeof(T), out ActionBase value))
		{
			val = (T)value;
		}
		action = val;
		return val != null;
	}

	public IEnumerable<ActionBase> GetAll()
	{
		return Actions.Values;
	}

	public IEnumerable<KeyAction> GetBindable()
	{
		return (from action in GetAll()
			where action is KeyAction
			select action).Cast<KeyAction>();
	}

	private static Dictionary<Type, ActionAttribute> ResolveActions()
	{
		return (from type in Assembly.GetExecutingAssembly().GetTypes()
			select (type: type, attr: type.GetCustomAttribute<ActionAttribute>()) into pair
			where pair.attr != null
			select pair).ToDictionary<(Type, ActionAttribute), Type, ActionAttribute>(((Type type, ActionAttribute attr) k) => k.type, ((Type type, ActionAttribute attr) v) => v.attr);
	}
}
