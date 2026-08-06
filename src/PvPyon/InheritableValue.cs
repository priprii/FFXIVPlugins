using System;
using Newtonsoft.Json;

namespace PvPyon;

public class InheritableValue<T> : IInheritable where T : struct
{
	[JsonProperty]
	public T Value;

	public IInheritable? Parent { get; set; }

	public InheritableBehavior Behavior { get; set; }

	[JsonIgnore]
	public T? InheritedValue
	{
		get
		{
			for (IInheritable inheritable = this; inheritable != null; inheritable = inheritable.Parent)
			{
				if (inheritable.Behavior == InheritableBehavior.Enabled && inheritable is InheritableValue<T> inheritableValue)
				{
					return inheritableValue.Value;
				}
				if (inheritable.Behavior == InheritableBehavior.Disabled)
				{
					return null;
				}
			}
			return null;
		}
	}

	public static implicit operator InheritableValue<T>(T value)
	{
		return new InheritableValue<T>(value)
		{
			Behavior = InheritableBehavior.Enabled
		};
	}

	public InheritableValue(T value)
	{
		Value = value;
	}

	public void SetData(InheritableData inheritableData)
	{
		Behavior = inheritableData.Behavior;
		try
		{
			if (typeof(T).IsEnum && inheritableData.Value != null)
			{
				if (inheritableData.Value is string value)
				{
					Value = (T)Enum.Parse(typeof(T), value);
				}
				else
				{
					Value = (T)Enum.ToObject(typeof(T), inheritableData.Value);
				}
			}
			else if (inheritableData.Value == null)
			{
				PluginServices.PluginLog.Error($"Expected value of type {Value.GetType()} but received null", Array.Empty<object>());
			}
			else if (typeof(T) == typeof(Guid) && inheritableData.Value is string input)
			{
				Value = (T)(object)Guid.Parse(input);
			}
			else
			{
				Value = (T)Convert.ChangeType(inheritableData.Value, typeof(T));
			}
		}
		catch (Exception ex)
		{
			PluginServices.PluginLog.Error(ex, $"Failed to convert {inheritableData.Value.GetType()} value '{inheritableData.Value}' to {Value.GetType()}", Array.Empty<object>());
		}
	}

	public InheritableData GetData()
	{
		return new InheritableData
		{
			Behavior = Behavior,
			Value = Value
		};
	}
}
