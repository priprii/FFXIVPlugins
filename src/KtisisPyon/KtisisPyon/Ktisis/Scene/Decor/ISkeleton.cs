using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok.Animation.Rig;

namespace Ktisis.Scene.Decor;

public interface ISkeleton
{
	unsafe Skeleton* GetSkeleton();

	unsafe hkaPose* GetPose(int index);
}
