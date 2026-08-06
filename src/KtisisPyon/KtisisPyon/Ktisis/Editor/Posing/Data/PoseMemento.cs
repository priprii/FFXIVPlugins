using System.Collections.Generic;
using Ktisis.Actions.Types;
using Ktisis.Editor.Posing.Types;

namespace Ktisis.Editor.Posing.Data;

public class PoseMemento(EntityPoseConverter converter) : IMemento
{
	public required PoseMode Modes { get; init; }

	public required PoseTransforms Transforms { get; init; }

	public required List<PartialBoneInfo>? Bones { get; init; }

	public required PoseContainer Initial { get; init; }

	public required PoseContainer Final { get; init; }

	public void Restore()
	{
		Apply(Initial);
	}

	public void Apply()
	{
		Apply(Final);
	}

	private void Apply(PoseContainer pose)
	{
		if (converter.IsPoseValid)
		{
			if (Bones != null)
			{
				IEnumerable<PartialBoneInfo> bones = converter.IntersectBonesByName(Bones);
				converter.LoadBones(pose, bones, Transforms);
			}
			else
			{
				converter.Load(pose, Modes, Transforms);
			}
		}
	}
}
