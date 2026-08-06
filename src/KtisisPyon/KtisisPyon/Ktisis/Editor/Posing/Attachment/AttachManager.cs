using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using Ktisis.Scene.Decor;
using Ktisis.Scene.Entities.World;
using Ktisis.Structs.Attachment;

namespace Ktisis.Editor.Posing.Attachment;

public class AttachManager : IAttachManager, IDisposable
{
	internal readonly HashSet<IAttachable> Attachments = new HashSet<IAttachable>();

	public void Attach(IAttachable child, IAttachTarget target)
	{
		Ktisis.Log.Info($"Attaching {child} {child.GetHashCode():X}");
		if (child.IsValid && target.TryAcceptAttach(child))
		{
			Attachments.Add(child);
		}
		if (child is LightEntity lightEntity)
		{
			lightEntity.SetAttach(target);
		}
	}

	public void Detach(IAttachable child)
	{
		if (!child.IsValid)
		{
			return;
		}
		Ktisis.Log.Info($"Detaching {child} {child.GetHashCode():X}");
		try
		{
			child.Detach();
		}
		finally
		{
			Attachments.RemoveWhere((IAttachable item) => item.Equals(child));
		}
	}

	public unsafe void Invalidate(Skeleton* parent)
	{
		foreach (IAttachable item in Attachments.Where((IAttachable x) => x.IsValid).ToList())
		{
			Attach* attach = item.GetAttach();
			if (attach != null && attach->Parent == parent)
			{
				Detach(item);
			}
		}
	}

	private void Clear()
	{
		foreach (IAttachable attachment in Attachments)
		{
			attachment.Detach();
		}
		Attachments.Clear();
	}

	public void Dispose()
	{
		try
		{
			Clear();
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to clear attachments:\n{value}");
		}
		GC.SuppressFinalize(this);
	}
}
