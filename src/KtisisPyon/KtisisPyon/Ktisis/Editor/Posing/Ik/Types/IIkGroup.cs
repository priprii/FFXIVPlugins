namespace Ktisis.Editor.Posing.Ik.Types;

public interface IIkGroup
{
	bool IsEnabled { get; set; }

	uint SkeletonId { get; set; }
}
