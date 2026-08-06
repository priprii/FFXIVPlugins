using System.Numerics;
using System.Runtime.InteropServices;

namespace Ktisis.Structs.Havok;

[StructLayout(LayoutKind.Explicit)]
public struct CcdIkConstraint
{
	[FieldOffset(0)]
	public short m_startBone;

	[FieldOffset(2)]
	public short m_endBone;

	[FieldOffset(16)]
	public Vector4 m_targetMS;
}
