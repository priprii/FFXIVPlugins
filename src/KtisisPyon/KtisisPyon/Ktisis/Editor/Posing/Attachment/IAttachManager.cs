using System;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using Ktisis.Scene.Decor;

namespace Ktisis.Editor.Posing.Attachment;

public interface IAttachManager : IDisposable
{
	void Attach(IAttachable child, IAttachTarget target);

	void Detach(IAttachable child);

	unsafe void Invalidate(Skeleton* parent);
}
