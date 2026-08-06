using System;

namespace PvPyon.Api.Nameplates.EventArgs;

public abstract class HookBaseEventArgs
{
	internal event Action CallOriginal;

	public void Original()
	{
		this.CallOriginal?.Invoke();
	}
}
