namespace PvPyon;

public interface IInheritable
{
	IInheritable? Parent { get; set; }

	InheritableBehavior Behavior { get; set; }

	void SetData(InheritableData inheritableData);

	InheritableData GetData();
}
