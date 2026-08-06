using System.Numerics;
using Ktisis.Editor.Posing.Ik.Types;

namespace Ktisis.Editor.Posing.Ik.TwoJoints;

public record TwoJointsGroup : IIkGroup
{
	public bool IsEnabled { get; set; }

	public uint SkeletonId { get; set; }

	public TwoJointsMode Mode;

	public short FirstBoneIndex = -1;

	public short FirstTwistIndex = -1;

	public short SecondBoneIndex = 1;

	public short SecondTwistIndex = 1;

	public short EndBoneIndex = -1;

	public float FirstBoneGain = 1f;

	public float SecondBoneGain = 1f;

	public float EndBoneGain = 1f;

	public float MaxHingeAngle = -1f;

	public float MinHingeAngle = 1f;

	public Vector3 HingeAxis = new Vector3(0f, 0f, 1f);

	public bool EnforcePosition = true;

	public Vector3 TargetPosition = Vector3.Zero;

	public bool EnforceRotation = true;

	public Quaternion TargetRotation = Quaternion.Identity;
}
