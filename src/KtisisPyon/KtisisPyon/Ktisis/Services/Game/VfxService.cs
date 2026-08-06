using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using Ktisis.Core.Attributes;
using Ktisis.Structs.Vfx.Apricot;

namespace Ktisis.Services.Game;

[Singleton]
public class VfxService
{
	private unsafe delegate ApricotCore* GetApricotCoreDelegate();

	[Signature("E8 ?? ?? ?? ?? 48 8B 14 1E")]
	private GetApricotCoreDelegate? GetApricotCoreFunc;

	public VfxService(IGameInteropProvider interop)
	{
		interop.InitializeFromAttributes((object)this);
	}

	public unsafe ApricotCore* GetApricotCore()
	{
		if (GetApricotCoreFunc == null)
		{
			return null;
		}
		return GetApricotCoreFunc();
	}
}
