using System;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.Interop;
using Ktisis.Structs.Lights;

namespace Ktisis.Structs.GPose;

[StructLayout(LayoutKind.Explicit)]
public struct GPoseState
{
	private const int LightCount = 3;

	[FieldOffset(224)]
	public unsafe fixed ulong Lights[3];

	[FieldOffset(480)]
	public unsafe GameObject* GPoseTarget;

	public unsafe SceneLight* GetLight(uint index)
	{
		return (SceneLight*)Lights[index];
	}

	public unsafe Span<Pointer<SceneLight>> GetLights()
	{
		fixed (ulong* lights = Lights)
		{
			return new Span<Pointer<SceneLight>>(lights, 3);
		}
	}
}
