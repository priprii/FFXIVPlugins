using System.Numerics;
using System.Runtime.InteropServices;

namespace Ktisis.Structs.Havok;

[StructLayout(LayoutKind.Explicit)]
public struct TwoJointsIkSetup
{
	[FieldOffset(0)]
	public short m_firstJointIdx;

	[FieldOffset(2)]
	public short m_secondJointIdx;

	[FieldOffset(4)]
	public short m_endBoneIdx;

	[FieldOffset(6)]
	public short m_firstJointTwistIdx;

	[FieldOffset(8)]
	public short m_secondJointTwistIdx;

	[FieldOffset(16)]
	public Vector4 m_hingeAxisLS;

	[FieldOffset(32)]
	public float m_cosineMaxHingeAngle;

	[FieldOffset(36)]
	public float m_cosineMinHingeAngle;

	[FieldOffset(40)]
	public float m_firstJointIkGain;

	[FieldOffset(44)]
	public float m_secondJointIkGain;

	[FieldOffset(48)]
	public float m_endJointIkGain;

	[FieldOffset(64)]
	public Vector4 m_endTargetMS;

	[FieldOffset(80)]
	public Quaternion m_endTargetRotationMS;

	[FieldOffset(96)]
	public Vector4 m_endBoneOffsetLS;

	[FieldOffset(112)]
	public Quaternion m_endBoneRotationOffsetLS;

	[FieldOffset(128)]
	public bool m_enforceEndPosition;

	[FieldOffset(129)]
	public bool m_enforceEndRotation;
}
