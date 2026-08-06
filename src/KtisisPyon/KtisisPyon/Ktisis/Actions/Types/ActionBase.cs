using System.Reflection;
using Ktisis.Actions.Attributes;
using Ktisis.Core.Types;

namespace Ktisis.Actions.Types;

public abstract class ActionBase
{
	protected IPluginContext Context { get; }

	protected ActionBase(IPluginContext ctx)
	{
		Context = ctx;
	}

	public string GetName()
	{
		return GetAttribute().Name;
	}

	public ActionAttribute GetAttribute()
	{
		return GetType().GetCustomAttribute<ActionAttribute>();
	}

	public virtual bool CanInvoke()
	{
		return true;
	}

	public abstract bool Invoke();
}
