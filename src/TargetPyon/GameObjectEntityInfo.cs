using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Colors;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Common.Math;
using TargetPyon.Extensions;

namespace TargetPyon;

public class GameObjectEntityInfo : IObjectEntityInfo
{
	public IGameObject? GameObject;

	public ulong Id { get; init; }

	public bool IsValid
	{
		get
		{
			if (GameObject != null && GameObject.IsValid())
			{
				return Id == GameObject.GameObjectId;
			}
			return false;
		}
	}

	public string Name
	{
		get
		{
			if (IsValid)
			{
				if (!StringExtensions.IsNullOrWhitespace(GameObject.Name.TextValue))
				{
					return GameObject.Name.TextValue;
				}
				return $"({Id})";
			}
			return "Invalid";
		}
	}

	public string TypeName
	{
		get
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			if (IsValid)
			{
				return $"{GameObject.ObjectKind}";
			}
			return "Invalid";
		}
	}

	internal bool IsEventObj
	{
		get
		{
			if (IsValid)
			{
				return GameObject is IEventObj;
			}
			return false;
		}
	}

	internal bool IsNpc
	{
		get
		{
			if (IsValid)
			{
				return GameObject is INpc;
			}
			return false;
		}
	}

	internal bool IsChara
	{
		get
		{
			if (IsValid)
			{
				return GameObject is ICharacter;
			}
			return false;
		}
	}

	internal bool IsBattleNpc
	{
		get
		{
			if (IsValid)
			{
				return GameObject is IBattleNpc;
			}
			return false;
		}
	}

	internal bool IsBattleChara
	{
		get
		{
			if (IsValid)
			{
				return GameObject is IBattleChara;
			}
			return false;
		}
	}

	internal IEventObj? EventObj
	{
		get
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			if (!IsEventObj)
			{
				return null;
			}
			return (IEventObj)GameObject;
		}
	}

	internal INpc? Npc
	{
		get
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			if (!IsNpc)
			{
				return null;
			}
			return (INpc)GameObject;
		}
	}

	internal ICharacter? Chara
	{
		get
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			if (!IsChara)
			{
				return null;
			}
			return (ICharacter)GameObject;
		}
	}

	internal IBattleNpc? BattleNpc
	{
		get
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			if (!IsBattleNpc)
			{
				return null;
			}
			return (IBattleNpc)GameObject;
		}
	}

	internal IBattleChara? BattleChara
	{
		get
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			if (!IsBattleChara)
			{
				return null;
			}
			return (IBattleChara)GameObject;
		}
	}

	internal unsafe GameObject* GameObjectPtr => GameObject.ToCsGameObject();

	internal unsafe Character* CharacterPtr => GameObject.ToCsPlayerCharacter();

	internal unsafe BattleChara* BattleNpcPtr => GameObject.ToCsBattleChara();

	public Vector4 TypeColour
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Expected I4, but got Unknown
			IGameObject? gameObject = GameObject;
			switch ((gameObject != null) ? new ObjectKind?(gameObject.ObjectKind) : ((ObjectKind?)null))
			{
			case (ObjectKind)0L:
				return ImGuiColors.DalamudRed;
			case (ObjectKind)1L:
				return ImGuiColors.DalamudViolet;
			case (ObjectKind)5L:
			case (ObjectKind)9L:
			case (ObjectKind)10L:
			case (ObjectKind)11L:
			case (ObjectKind)13L:
				return ImGuiColors.ParsedPink;
			case (ObjectKind)2L:
			case (ObjectKind)3L:
			case (ObjectKind)4L:
			case (ObjectKind)14L:
				return ImGuiColors.ParsedBlue;
			case (ObjectKind)6L:
			case (ObjectKind)7L:
			case (ObjectKind)8L:
				return ImGuiColors.ParsedOrange;
			default:
				return ImGuiColors.DalamudWhite;
			}
		}
	}

	public unsafe Vector3 Position
	{
		get
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			return Vector3.op_Implicit((!IsValid) ? default(Vector3) : GameObject.Position);
		}
		set
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001a: Unknown result type (might be due to invalid IL or missing references)
			if (IsValid)
			{
				((GameObject)GameObjectPtr).SetPosition(value.X, value.Y, value.Z);
			}
		}
	}

	public unsafe float Rotation
	{
		get
		{
			if (IsValid)
			{
				return GameObject.Rotation;
			}
			return 0f;
		}
		set
		{
			if (IsValid)
			{
				((GameObject)GameObjectPtr).SetRotation(value);
			}
		}
	}

	public unsafe float Scale
	{
		get
		{
			if (IsValid)
			{
				return ((GameObject)GameObjectPtr).Scale;
			}
			return 1f;
		}
		set
		{
			if (IsValid)
			{
				((GameObject)GameObjectPtr).Scale = Math.Max(value, 0.01f);
			}
		}
	}

	internal bool IsTargetable
	{
		get
		{
			if (IsValid)
			{
				return GameObject.IsTargetable;
			}
			return false;
		}
	}

	internal bool IsCamTarget
	{
		get
		{
			if (IsValid)
			{
				return IPC.GetCamTarget() == GameObject.GameObjectId;
			}
			return false;
		}
	}

	public unsafe bool IsVisible
	{
		get
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			if (IsValid)
			{
				return !((Enum)((GameObject)GameObjectPtr).RenderFlags).HasFlag((Enum)(object)(VisibilityFlags)2);
			}
			return false;
		}
	}

	public GameObjectEntityInfo(IGameObject baseObject)
	{
		Id = baseObject.GameObjectId;
		GameObject = baseObject;
	}

	internal void SetAsTarget()
	{
		if (IsValid && GameObject.IsTargetable)
		{
			GameObject.SetAsTarget();
		}
	}

	internal void SetAsFocusTarget()
	{
		if (IsValid && GameObject.IsTargetable)
		{
			GameObject.SetAsFocusTarget();
		}
	}

	internal void ToggleCamTarget()
	{
		if (IsCamTarget)
		{
			IPC.ResetCamTarget();
			Plugin.MainWindow.RemoveResetCamTitleButton();
		}
		else if (IsValid && IPC.SetCamTarget(GameObject.GameObjectId))
		{
			Plugin.MainWindow.AddResetCamTitleButton();
		}
	}

	public unsafe void Hide()
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		if (IsValid && IsVisible)
		{
			Unsafe.Write(&((GameObject)GameObjectPtr).RenderFlags, (VisibilityFlags)2050);
		}
	}

	public unsafe void Show()
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		if (IsValid && !IsVisible)
		{
			Unsafe.Write(&((GameObject)GameObjectPtr).RenderFlags, (VisibilityFlags)0);
		}
	}
}
