using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;

namespace Ktisis.Structs.Env;

[StructLayout(LayoutKind.Explicit, Size = 2320)]
public struct EnvManagerEx
{
	[FieldOffset(0)]
	public EnvManager _base;

	[FieldOffset(88)]
	public EnvState EnvState;

	[FieldOffset(1248)]
	public EnvSimulator EnvSimulator;

	public unsafe static EnvManagerEx* Instance()
	{
		return (EnvManagerEx*)EnvManager.Instance();
	}
}
