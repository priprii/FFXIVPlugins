using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Ktisis.Common.Extensions;
using Ktisis.Core.Attributes;

namespace Ktisis.Services.Game;

[Singleton]
public class ActorService
{
	public const ushort GPoseIndex = 201;

	public const ushort GPoseCount = 42;

	private readonly IObjectTable _objectTable;

	public ActorService(IObjectTable objectTable)
	{
		_objectTable = objectTable;
	}

	public IGameObject? GetIndex(int index)
	{
		return _objectTable[index];
	}

	public IGameObject? GetAddress(nint address)
	{
		return _objectTable.CreateObjectReference((IntPtr)address);
	}

	public IEnumerable<IGameObject> GetGPoseActors()
	{
		for (ushort i = 201; i < 243; i++)
		{
			IGameObject index = GetIndex(i);
			if (index != null)
			{
				yield return index;
			}
		}
	}

	public IEnumerable<IGameObject> GetOverworldActors()
	{
		IEnumerable<IGameObject> enumerable = _objectTable.CharacterManagerObjects.Concat(_objectTable.ClientObjects.Where((IGameObject gameObject) => gameObject.ObjectIndex > 243)).Concat(_objectTable.StandObjects.Where(delegate(IGameObject gameObject)
		{
			//IL_0004: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Unknown result type (might be due to invalid IL or missing references)
			//IL_000a: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Invalid comparison between Unknown and I4
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Invalid comparison between Unknown and I4
			if (gameObject != null)
			{
				ObjectKind objectKind = gameObject.ObjectKind;
				if (objectKind - 2 <= 1 || objectKind - 8 <= 1)
				{
					return true;
				}
			}
			return false;
		}));
		foreach (IGameObject item in enumerable)
		{
			if (item.IsEnabled() && item.IsDrawing())
			{
				yield return item;
			}
		}
	}

	public unsafe IGameObject? GetSkeletonOwner(Skeleton* skeleton)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Invalid comparison between Unknown and I4
		foreach (IGameObject item in (IEnumerable<IGameObject>)_objectTable)
		{
			GameObject* address = (GameObject*)item.Address;
			if (address != null && ((GameObject)address).DrawObject != null && (int)((Object)(&((DrawObject)((GameObject)address).DrawObject).Object)).GetObjectType() == 3 && item.GetSkeleton() == skeleton)
			{
				return item;
			}
		}
		return null;
	}
}
