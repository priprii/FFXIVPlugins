using System;
using System.Runtime.InteropServices;

namespace Ktisis.Structs.Vfx.Apricot;

[StructLayout(LayoutKind.Explicit)]
public struct ApricotCore
{
	[StructLayout(LayoutKind.Explicit)]
	public struct DataContainer
	{
		[FieldOffset(8240)]
		public unsafe fixed byte Instances[278528];

		public unsafe InstanceContainer* GetIndex(uint index)
		{
			if (index > 2048)
			{
				throw new IndexOutOfRangeException($"Index {index} is out of range.");
			}
			fixed (byte* instances = Instances)
			{
				return (InstanceContainer*)instances + index;
			}
		}

		public unsafe Span<InstanceContainer> GetInstancesSpan()
		{
			fixed (byte* instances = Instances)
			{
				return new Span<InstanceContainer>(instances, 136);
			}
		}
	}

	[FieldOffset(3376)]
	public unsafe DataContainer* Data;
}
