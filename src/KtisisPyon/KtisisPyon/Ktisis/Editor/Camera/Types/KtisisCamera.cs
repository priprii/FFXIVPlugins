using System;
using FFXIVClientStructs.FFXIV.Client.Game;
using Ktisis.Interop;

namespace Ktisis.Editor.Camera.Types;

public class KtisisCamera : EditorCamera, IDisposable
{
	private Alloc<Camera>? Alloc = new Alloc<Camera>(8uL);

	public override nint Address => Alloc?.Address ?? IntPtr.Zero;

	public KtisisCamera(ICameraManager manager)
		: base(manager)
	{
	}

	public void Dispose()
	{
		Alloc?.Dispose();
		Alloc = null;
		GC.SuppressFinalize(this);
	}
}
