using System;

namespace PyonPix.Services.Core;

public sealed class PixFieldBinding<T>
{
	private readonly Action<T, bool> _commit;

	private readonly Action<bool> _clearOverride;

	public T Value { get; private set; }

	public bool HasOverride { get; private set; }

	public bool CanSyncEdit { get; }

	public PixFieldBinding(T value, bool hasOverride, bool canSyncEdit, Action<T, bool> commit, Action<bool> clearOverride)
	{
		Value = value;
		HasOverride = hasOverride;
		CanSyncEdit = canSyncEdit;
		_commit = commit;
		_clearOverride = clearOverride;
	}

	public void Commit(T value, bool editFinished = true)
	{
		Value = value;
		if (!CanSyncEdit)
		{
			HasOverride = true;
		}
		_commit(value, editFinished);
	}

	public void ResetOverride(bool editFinished = true)
	{
		HasOverride = false;
		_clearOverride(editFinished);
	}
}
