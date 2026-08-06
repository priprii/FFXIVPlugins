using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Interface.Colors;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Math;
using Lumina.Excel.Sheets;

namespace TargetPyon;

internal static class EntityManager
{
	internal static List<PlayerEntityInfo> NearbyPlayers = new List<PlayerEntityInfo>();

	internal static List<IObjectEntityInfo> NearbyObjects = new List<IObjectEntityInfo>();

	internal static PlayerEntityInfo? GetPlayerEntityInfoFromObject(IGameObject? obj)
	{
		if (obj != null)
		{
			return NearbyPlayers.Find((PlayerEntityInfo x) => x.GameObject == obj);
		}
		return null;
	}

	internal static void UpdatePlayerList()
	{
		List<PlayerEntityInfo> list = new List<PlayerEntityInfo>();
		new List<PlayerEntityInfo>();
		if (Plugin.Objects.LocalPlayer == null)
		{
			NearbyPlayers = new List<PlayerEntityInfo>();
			return;
		}
		List<nint> list2 = IPC.MareGetNearbyPlayerAddresses();
		foreach (IPlayerCharacter character in ((IEnumerable<IGameObject>)Plugin.Objects).Where((IGameObject x) => x.IsValid() && x.SubKind != 0).OfType<IPlayerCharacter>())
		{
			bool isMareSynced = list2 != null && list2.Find((nint x) => x == (nint)((IGameObject)character).Address) != IntPtr.Zero;
			list.Add(new PlayerEntityInfo(character)
			{
				IsNearby = true,
				IsMareSynced = isMareSynced
			});
		}
		NearbyPlayers = list;
	}

	internal unsafe static void UpdateObjectList()
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Invalid comparison between Unknown and I4
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Invalid comparison between Unknown and I4
		//IL_02f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0300: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Invalid comparison between Unknown and I4
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Invalid comparison between Unknown and I4
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0321: Invalid comparison between Unknown and I4
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Invalid comparison between Unknown and I4
		//IL_0348: Unknown result type (might be due to invalid IL or missing references)
		//IL_034e: Invalid comparison between Unknown and I4
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0151: Invalid comparison between Unknown and I4
		//IL_0375: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Invalid comparison between Unknown and I4
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_017a: Invalid comparison between Unknown and I4
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a8: Invalid comparison between Unknown and I4
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a6: Invalid comparison between Unknown and I4
		//IL_03cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d5: Invalid comparison between Unknown and I4
		//IL_01cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d2: Invalid comparison between Unknown and I4
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ff: Invalid comparison between Unknown and I4
		//IL_0428: Unknown result type (might be due to invalid IL or missing references)
		//IL_042e: Invalid comparison between Unknown and I4
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		//IL_022b: Invalid comparison between Unknown and I4
		//IL_0455: Unknown result type (might be due to invalid IL or missing references)
		//IL_045c: Invalid comparison between Unknown and I4
		//IL_0251: Unknown result type (might be due to invalid IL or missing references)
		//IL_0258: Invalid comparison between Unknown and I4
		//IL_0480: Unknown result type (might be due to invalid IL or missing references)
		//IL_0486: Invalid comparison between Unknown and I4
		//IL_027b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0282: Invalid comparison between Unknown and I4
		//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b1: Invalid comparison between Unknown and I4
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Invalid comparison between Unknown and I4
		List<IObjectEntityInfo> list = new List<IObjectEntityInfo>();
		if (Plugin.Objects.LocalPlayer == null)
		{
			NearbyObjects = new List<IObjectEntityInfo>();
			return;
		}
		foreach (IGameObject item in from x in (IEnumerable<IGameObject>)Plugin.Objects
			where x.IsValid()
			where !(x is IPlayerCharacter)
			select x)
		{
			if (((int)item.ObjectKind != 5 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.Aetheryte)) && ((int)item.ObjectKind != 11 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.Area)) && ((int)item.ObjectKind != 2 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.BattleNpc)) && ((int)item.ObjectKind != 16 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.CardStand)) && ((int)item.ObjectKind != 13 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.Cutscene)) && ((int)item.ObjectKind != 9 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.Companion)) && ((int)item.ObjectKind != 3 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.EventNpc)) && ((int)item.ObjectKind != 7 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.EventObj)) && ((int)item.ObjectKind != 6 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.GatheringPoint)) && ((int)item.ObjectKind != 12 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.Housing)) && ((int)item.ObjectKind != 8 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.Mount)) && ((int)item.ObjectKind != 15 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.Ornament)) && ((int)item.ObjectKind != 10 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.Retainer)) && ((int)item.ObjectKind != 4 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.Treasure)))
			{
				list.Add(new GameObjectEntityInfo(item));
			}
		}
		uint num = 0u;
		SiblingEnumerator childObjects = ((World)World.Instance()).ChildObjects;
		SiblingEnumerator enumerator2 = ((SiblingEnumerator)(ref childObjects)).GetEnumerator();
		while (((SiblingEnumerator)(ref enumerator2)).MoveNext())
		{
			Object* current2 = ((SiblingEnumerator)(ref enumerator2)).Current;
			num++;
			if (((int)((Object)current2).GetObjectType() != 2 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.BgObject)) && ((int)((Object)current2).GetObjectType() != 3 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.CharacterBase)) && ((int)((Object)current2).GetObjectType() != 8 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.EnvLocation)) && ((int)((Object)current2).GetObjectType() != 7 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.EnvSpace)) && ((int)((Object)current2).GetObjectType() != 5 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.Light)) && ((int)((Object)current2).GetObjectType() != 0 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.Object)) && ((int)((Object)current2).GetObjectType() != 4 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.VfxObject)) && ((int)((Object)current2).GetObjectType() != 9 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.Unknown)) && ((int)((Object)current2).GetObjectType() != 6 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.Unknown)) && ((int)((Object)current2).GetObjectType() != 10 || Plugin.Config.ListObjectsTypeFilter.HasFlag(ObjectTypeFilter.Unknown)))
			{
				list.Add(new SceneObjectEntityInfo(current2, num));
			}
		}
		NearbyObjects = list;
	}

	internal static List<PlayerEntityInfo> GetFormattedNearbyPlayers(int maxPlayers, bool orderByDistance, string searchText, bool filterAfk = false, bool prioritizeKnownPlayers = true)
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		List<PlayerEntityInfo> list = new List<PlayerEntityInfo>();
		IOrderedEnumerable<PlayerEntityInfo> orderedEnumerable = NearbyPlayers?.OrderBy((PlayerEntityInfo x) => x.Distance);
		if (orderedEnumerable == null)
		{
			return list;
		}
		foreach (PlayerEntityInfo item in orderedEnumerable)
		{
			if (list.Count >= maxPlayers)
			{
				break;
			}
			if (filterAfk)
			{
				IPlayerCharacter? character = item.Character;
				if (character != null)
				{
					OnlineStatus value = ((ICharacter)character).OnlineStatus.Value;
					if (((OnlineStatus)(ref value)).RowId == 17)
					{
						continue;
					}
				}
			}
			list.Add(item);
		}
		List<PlayerEntityInfo> list2 = new List<PlayerEntityInfo>();
		list = (prioritizeKnownPlayers ? ((!orderByDistance) ? (from p in list
			orderby p.IsKnownPlayer descending, p.Name
			select p).ToList() : (from p in list
			orderby p.IsKnownPlayer descending, p.Distance
			select p).ToList()) : ((!orderByDistance) ? list.OrderBy((PlayerEntityInfo p) => p.Name).ToList() : list.OrderBy((PlayerEntityInfo p) => p.Distance).ToList()));
		foreach (PlayerEntityInfo item2 in list)
		{
			if (StringExtensions.IsNullOrWhitespace(searchText) || item2.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
			{
				list2.Add(item2);
			}
		}
		return list2;
	}

	internal static List<IObjectEntityInfo> GetFormattedObjects(int maxObjects, bool orderByDistance, string searchText)
	{
		List<IObjectEntityInfo> list = new List<IObjectEntityInfo>();
		IOrderedEnumerable<IObjectEntityInfo> orderedEnumerable = NearbyObjects?.OrderBy((IObjectEntityInfo x) => x.Distance);
		if (orderedEnumerable == null)
		{
			return list;
		}
		foreach (IObjectEntityInfo item in orderedEnumerable)
		{
			if (list.Count >= maxObjects)
			{
				break;
			}
			list.Add(item);
		}
		List<IObjectEntityInfo> list2 = new List<IObjectEntityInfo>();
		list = ((!orderByDistance) ? list.OrderBy((IObjectEntityInfo p) => p.Name).ToList() : list.OrderBy((IObjectEntityInfo p) => p.Distance).ToList());
		foreach (IObjectEntityInfo item2 in list)
		{
			if (StringExtensions.IsNullOrWhitespace(searchText) || item2.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase))
			{
				list2.Add(item2);
			}
		}
		return list2;
	}

	internal static void UpdatePlayerVisibility()
	{
		if (NearbyPlayers == null || Plugin.Objects.LocalPlayer == null)
		{
			return;
		}
		foreach (PlayerEntityInfo nearbyPlayer in NearbyPlayers)
		{
			if (Plugin.Config.PlayerVisibilityFilter && nearbyPlayer.GameObject.GameObjectId != ((IGameObject)Plugin.Objects.LocalPlayer).GameObjectId)
			{
				bool flag = !nearbyPlayer.IsCamTarget && !nearbyPlayer.IsInParty && !nearbyPlayer.IsFriend;
				if (nearbyPlayer.IsVisible && flag)
				{
					nearbyPlayer.Hide();
				}
				else if (!nearbyPlayer.IsVisible && !flag)
				{
					nearbyPlayer.Show();
				}
			}
		}
	}

	internal static void UpdateObjectVisibility()
	{
		if (NearbyObjects == null)
		{
			return;
		}
		foreach (IObjectEntityInfo nearbyObject in NearbyObjects)
		{
			if (Plugin.Config.ObjectVisibilityFilter && nearbyObject is GameObjectEntityInfo gameObjectEntityInfo)
			{
				bool flag = !gameObjectEntityInfo.IsCamTarget;
				if (nearbyObject.IsVisible && flag)
				{
					nearbyObject.Hide();
				}
				else if (!nearbyObject.IsVisible && !flag)
				{
					nearbyObject.Show();
				}
			}
		}
	}

	internal static bool IsPlayerInParty(IPlayerCharacter playerCharacter)
	{
		foreach (IPartyMember item in (IEnumerable<IPartyMember>)Plugin.PartyList)
		{
			IGameObject gameObject = item.GameObject;
			if (((gameObject != null) ? new ulong?(gameObject.GameObjectId) : ((ulong?)null)) == ((IGameObject)playerCharacter).GameObjectId)
			{
				return true;
			}
		}
		return false;
	}

	internal static Vector4 GetEntityNameColour(PlayerEntityInfo entityInfo)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		if (!entityInfo.IsPlayerCharacter)
		{
			return Vector4.op_Implicit(ImGuiColors.DalamudWhite);
		}
		if (entityInfo.IsCamTarget)
		{
			return Vector4.op_Implicit(ImGuiColors.DalamudViolet);
		}
		if (entityInfo.IsBlocked)
		{
			return Vector4.op_Implicit(ImGuiColors.DalamudRed);
		}
		if (entityInfo.IsInParty)
		{
			return Vector4.op_Implicit(ImGuiColors.ParsedBlue);
		}
		if (entityInfo.IsFriend)
		{
			return Vector4.op_Implicit(ImGuiColors.ParsedOrange);
		}
		if (!entityInfo.IsVisible)
		{
			return Vector4.op_Implicit(ImGuiColors.DalamudGrey3);
		}
		if (entityInfo.IsMareSynced)
		{
			return Vector4.op_Implicit(ImGuiColors.ParsedPink);
		}
		return Vector4.op_Implicit(ImGuiColors.DalamudWhite);
	}

	internal static Vector4 GetEntityNameColour(IObjectEntityInfo entityInfo)
	{
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (entityInfo is GameObjectEntityInfo { IsCamTarget: not false })
		{
			return Vector4.op_Implicit(ImGuiColors.DalamudViolet);
		}
		if (!entityInfo.IsVisible)
		{
			return Vector4.op_Implicit(ImGuiColors.DalamudGrey3);
		}
		return Vector4.op_Implicit(ImGuiColors.DalamudWhite);
	}
}
