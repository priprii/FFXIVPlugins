using System;

namespace PvPyon.Api.Nameplates.EventArgs;

public abstract class HookWithResultBaseEventArgs<TResult>
{
	public TResult Result { get; set; }

	internal event Func<TResult> CallOriginal;

	public TResult Original()
	{
		return this.CallOriginal();
	}
}
