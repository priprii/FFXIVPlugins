using Newtonsoft.Json;

namespace PvPyon;

public class InheritableReference<T> : IInheritable where T : class
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
				if (inheritable.Behavior == InheritableBehavior.Enabled && inheritable is InheritableReference<T> inheritableReference)
				{
					return inheritableReference.Value;
				}
				if (inheritable.Behavior == InheritableBehavior.Disabled)
				{
					return null;
				}
			}
			return null;
		}
	}

	public static implicit operator InheritableReference<T>(T value)
	{
		return new InheritableReference<T>(value)
		{
			Behavior = InheritableBehavior.Enabled
		};
	}

	public InheritableReference(T value)
	{
		Value = value;
	}

	public void SetData(InheritableData inheritableData)
	{
		Behavior = inheritableData.Behavior;
		Value = (T)inheritableData.Value;
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
