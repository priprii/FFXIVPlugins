using System;

namespace PyonPix.Services.Core;

public sealed class OwnerFieldBinding<T>
{
	private readonly Action<T, bool> _commit;

	public T Value { get; private set; }

	public bool CanEdit { get; }

	public OwnerFieldBinding(T value, bool canEdit, Action<T, bool> commit)
	{
		Value = value;
		CanEdit = canEdit;
		_commit = commit;
	}

	public void Commit(T value, bool editFinished = true)
	{
		Value = value;
		_commit(value, editFinished);
	}
}
