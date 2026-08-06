using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.Havok.Animation.Animation;
using FFXIVClientStructs.Havok.Animation.Playback;
using FFXIVClientStructs.Havok.Animation.Playback.Control;
using FFXIVClientStructs.Havok.Animation.Playback.Control.Default;
using Ktisis.Editor.Context.Types;

namespace Ktisis.Common.Extensions;

public static class GameObjectEx
{
	public unsafe static bool IsPcCharacter(this IGameObject gameObject)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Invalid comparison between Unknown and I4
		GameObject* address = (GameObject*)gameObject.Address;
		if (address != null)
		{
			return (int)((GameObject)address).GetObjectKind() == 1;
		}
		return false;
	}

	public static string GetNameOrFallback(this IGameObject gameObject, IEditorContext ctx, bool? forceIncognito = null)
	{
		bool num = forceIncognito ?? ctx.Config.Editor.IncognitoPlayerNames;
		bool flag = gameObject.IsPcCharacter();
		if (num && flag)
		{
			return $"Actor #{gameObject.ObjectIndex}";
		}
		string textValue = gameObject.Name.TextValue;
		if (StringExtensions.IsNullOrEmpty(textValue))
		{
			return $"Actor #{gameObject.ObjectIndex}";
		}
		return textValue;
	}

	public unsafe static DrawObject* GetDrawObject(this IGameObject gameObject)
	{
		GameObject* address = (GameObject*)gameObject.Address;
		if (address == null)
		{
			return null;
		}
		return ((GameObject)address).DrawObject;
	}

	public unsafe static Skeleton* GetSkeleton(this IGameObject gameObject)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		if (!gameObject.IsValid())
		{
			return null;
		}
		GameObject* address = (GameObject*)gameObject.Address;
		if (address == null || ((GameObject)address).DrawObject == null)
		{
			return null;
		}
		DrawObject* drawObject = ((GameObject)address).DrawObject;
		if ((int)((Object)(&((DrawObject)drawObject).Object)).GetObjectType() != 3)
		{
			return null;
		}
		return ((CharacterBase)drawObject).Skeleton;
	}

	public unsafe static hkaDefaultAnimationControl* GetDefaultControlForIndex(this IGameObject gameObject, int animationIndex)
	{
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0093: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		if (!gameObject.IsValid())
		{
			return null;
		}
		Skeleton* skeleton = gameObject.GetSkeleton();
		if (skeleton == null)
		{
			return null;
		}
		Span<PartialSkeleton> span = new Span<PartialSkeleton>(((Skeleton)skeleton).PartialSkeletons, ((Skeleton)skeleton).PartialSkeletonCount);
		for (int i = 0; i < span.Length; i++)
		{
			PartialSkeleton val = span[i];
			hkaAnimatedSkeleton* havokAnimatedSkeleton = ((PartialSkeleton)(ref val)).GetHavokAnimatedSkeleton(0);
			if (havokAnimatedSkeleton != null && ((hkaAnimatedSkeleton)havokAnimatedSkeleton).AnimationControls.Length != 0 && animationIndex < ((hkaAnimatedSkeleton)havokAnimatedSkeleton).AnimationControls.Length && ((hkaAnimatedSkeleton)havokAnimatedSkeleton).AnimationControls[animationIndex].Value != null)
			{
				hkaDefaultAnimationControl* value = ((hkaAnimatedSkeleton)havokAnimatedSkeleton).AnimationControls[animationIndex].Value;
				if (((hkaAnimationControl)(&((hkaDefaultAnimationControl)value).hkaAnimationControl)).Binding.ptr != null && ((hkaAnimationBinding)((hkaAnimationControl)(&((hkaDefaultAnimationControl)value).hkaAnimationControl)).Binding.ptr).Animation.ptr != null)
				{
					return value;
				}
			}
		}
		return null;
	}

	public unsafe static bool IsDrawing(this IGameObject gameObject)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Invalid comparison between Unknown and I8
		GameObject* address = (GameObject*)gameObject.Address;
		if (address == null)
		{
			return false;
		}
		return (long)((GameObject)address).RenderFlags == 0;
	}

	public unsafe static bool IsEnabled(this IGameObject gameObject)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Invalid comparison between Unknown and I8
		GameObject* address = (GameObject*)gameObject.Address;
		if (address == null)
		{
			return false;
		}
		return (long)(((GameObject)address).RenderFlags & 2) == 0;
	}

	public unsafe static void SetWorld(this IGameObject gameObject, ushort world)
	{
		Character* address = (Character*)gameObject.Address;
		if (address != null && ((GameObject)(&((Character)address).GameObject)).IsCharacter())
		{
			((Character)address).CurrentWorld = world;
			((Character)address).HomeWorld = world;
		}
	}

	public unsafe static void SetName(this IGameObject gameObject, string name)
	{
		GameObject* address = (GameObject*)gameObject.Address;
		if (address != null)
		{
			byte[] array = ((IEnumerable<byte>)Encoding.UTF8.GetBytes(name)).Append((byte)0).ToArray();
			for (int i = 0; i < array.Length; i++)
			{
				((GameObject)address).Name[i] = array[i];
			}
		}
	}

	public unsafe static void SetTargetable(this IGameObject gameObject, bool targetable)
	{
		GameObject* address = (GameObject*)gameObject.Address;
		if (address != null)
		{
			if (targetable)
			{
				((GameObject)address).TargetableStatus |= (ObjectTargetableFlags)2;
				return;
			}
			ObjectTargetableFlags* targetableStatus = &((GameObject)address).TargetableStatus;
			*targetableStatus = (ObjectTargetableFlags)((uint)(*targetableStatus) & 0xFDu);
		}
	}

	public unsafe static void SetGPoseTarget(this IGameObject gameObject)
	{
		if (gameObject.IsValid())
		{
			TargetSystem* ptr = TargetSystem.Instance();
			if (ptr != null && ((TargetSystem)ptr).GPoseTarget != null)
			{
				((TargetSystem)ptr).GPoseTarget = (GameObject*)gameObject.Address;
			}
		}
	}

	public unsafe static void Redraw(this IGameObject gameObject)
	{
		GameObject* address = (GameObject*)gameObject.Address;
		if (address != null)
		{
			((GameObject)address).DisableDraw();
			((GameObject)address).EnableDraw();
		}
	}
}
