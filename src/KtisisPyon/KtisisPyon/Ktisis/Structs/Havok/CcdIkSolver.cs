using System.Runtime.InteropServices;
using FFXIVClientStructs.Havok.Common.Base.Object;

namespace Ktisis.Structs.Havok;

[StructLayout(LayoutKind.Explicit)]
public struct CcdIkSolver
{
	[FieldOffset(0)]
	public unsafe nint** _vfTable;

	[FieldOffset(0)]
	public hkReferencedObject hkRefObject;

	[FieldOffset(16)]
	public int m_iterations;

	[FieldOffset(20)]
	public float m_gain;
}
