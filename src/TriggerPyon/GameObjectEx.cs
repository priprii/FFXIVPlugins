using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Lumina.Excel.Sheets;

namespace TriggerPyon;

public static class GameObjectEx
{
	public unsafe static GameObject* ToCsGameObject(this IGameObject o)
	{
		return (GameObject*)o.Address;
	}

	public unsafe static GameObject* ToCsGameObject(this IPlayerCharacter o)
	{
		return (GameObject*)((IGameObject)o).Address;
	}

	public unsafe static GameObject* ToCsGameObject(this IBattleNpc o)
	{
		return (GameObject*)((IGameObject)o).Address;
	}

	public unsafe static Character* ToCsPlayerCharacter(this IGameObject o)
	{
		return (Character*)o.Address;
	}

	public unsafe static Character* ToCsPlayerCharacter(this IPlayerCharacter o)
	{
		return (Character*)((IGameObject)o).Address;
	}

	public unsafe static BattleChara* ToCsBattleChara(this IGameObject o)
	{
		return (BattleChara*)o.Address;
	}

	public unsafe static BattleChara* ToCsBattleChara(this IBattleNpc o)
	{
		return (BattleChara*)((IGameObject)o).Address;
	}

	public static IGameObject? ToDalamudGameObject(this IPlayerCharacter o)
	{
		return Plugin.Objects.CreateObjectReference(((IGameObject)o).Address);
	}

	public static IGameObject? ToDalamudGameObject(this IBattleNpc o)
	{
		return Plugin.Objects.CreateObjectReference(((IGameObject)o).Address);
	}

	public unsafe static void SetAsTarget(this IGameObject o)
	{
		((TargetSystem)TargetSystem.Instance()).Target = o.ToCsGameObject();
	}

	public unsafe static void SetAsSoftTarget(this IGameObject o)
	{
		((TargetSystem)TargetSystem.Instance()).SoftTarget = o.ToCsGameObject();
	}

	public unsafe static void SetAsFocusTarget(this IGameObject o)
	{
		((TargetSystem)TargetSystem.Instance()).FocusTarget = o.ToCsGameObject();
	}

	public unsafe static void SetAsMouseOverTarget(this IGameObject o)
	{
		((TargetSystem)TargetSystem.Instance()).MouseOverTarget = o.ToCsGameObject();
	}

	public static void SetAsTarget(this IPlayerCharacter o)
	{
		Plugin.Targets.Target = o.ToDalamudGameObject();
	}

	public static void SetAsSoftTarget(this IPlayerCharacter o)
	{
		Plugin.Targets.SoftTarget = o.ToDalamudGameObject();
	}

	public static void SetAsFocusTarget(this IPlayerCharacter o)
	{
		Plugin.Targets.FocusTarget = o.ToDalamudGameObject();
	}

	public static void SetAsMouseOverTarget(this IPlayerCharacter o)
	{
		Plugin.Targets.MouseOverTarget = o.ToDalamudGameObject();
	}

	public static void SetAsTarget(this IBattleNpc o)
	{
		Plugin.Targets.Target = o.ToDalamudGameObject();
	}

	public static void SetAsSoftTarget(this IBattleNpc o)
	{
		Plugin.Targets.SoftTarget = o.ToDalamudGameObject();
	}

	public static void SetAsFocusTarget(this IBattleNpc o)
	{
		Plugin.Targets.FocusTarget = o.ToDalamudGameObject();
	}

	public static void SetAsMouseOverTarget(this IBattleNpc o)
	{
		Plugin.Targets.MouseOverTarget = o.ToDalamudGameObject();
	}

	public static bool IsFromCurrentWorld(this IPlayerCharacter pc)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		return pc.CurrentWorld.RowId == pc.HomeWorld.RowId;
	}

	public static bool IsFromCurrentDatacenter(this IPlayerCharacter pc)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		World value = pc.CurrentWorld.Value;
		uint rowId = ((World)(ref value)).DataCenter.RowId;
		value = pc.HomeWorld.Value;
		return rowId == ((World)(ref value)).DataCenter.RowId;
	}

	public unsafe static void OpenCharaCard(this IPlayerCharacter pc)
	{
		((AgentCharaCard)AgentCharaCard.Instance()).OpenCharaCard(pc.ToCsGameObject());
	}

	public unsafe static void OpenExamine(this IPlayerCharacter pc)
	{
		((AgentInspect)AgentInspect.Instance()).ExamineCharacter(((IGameObject)pc).EntityId, false);
	}

	public static bool HasOnlineStatus(this IPlayerCharacter pc, OnlineStatusTypeRaw status)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((ICharacter)pc).OnlineStatus.RowId == (uint)status;
	}

	public static bool HasOnlineStatus(this IPlayerCharacter pc, uint status)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((ICharacter)pc).OnlineStatus.RowId == status;
	}
}
