using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game;

namespace TriggerPyon;

internal static class PlayerManager
{
	internal static List<EntityInfo> NearbyPlayers = new List<EntityInfo>();

	internal static EntityInfo? _localPlayer = null;

	internal static EntityInfo? LocalPlayer
	{
		get
		{
			Plugin.Framework.RunOnFrameworkThread((Action)delegate
			{
				IPlayerCharacter localPlayer = Plugin.Objects.LocalPlayer;
				if (localPlayer == null)
				{
					_localPlayer = null;
				}
				else if (_localPlayer == null || _localPlayer.Character != localPlayer)
				{
					_localPlayer = new EntityInfo(localPlayer)
					{
						IsMareSynced = true
					};
				}
			});
			return _localPlayer;
		}
	}

	internal unsafe static short CurrentWard => (short)(((HousingManager)HousingManager.Instance()).GetCurrentWard() + 1);

	internal static bool IsInWard => CurrentWard > 0;

	internal unsafe static short CurrentPlot => (short)(((HousingManager)HousingManager.Instance()).GetCurrentPlot() + 1);

	internal static bool IsInPlot => CurrentPlot > 0;

	internal unsafe static short CurrentRoom => ((HousingManager)HousingManager.Instance()).GetCurrentRoom();

	internal static bool IsInRoom => CurrentRoom > 0;

	internal unsafe static bool IsInside => ((HousingManager)HousingManager.Instance()).IsInside();

	internal unsafe static bool IsOutside => ((HousingManager)HousingManager.Instance()).IsOutside();

	internal unsafe static bool IsInWorkshop => ((HousingManager)HousingManager.Instance()).IsInWorkshop();

	internal static bool IsInWardArea
	{
		get
		{
			if (IsInWard && !IsInPlot)
			{
				return IsOutside;
			}
			return false;
		}
	}

	internal static bool IsInPlotOutside
	{
		get
		{
			if (IsInWard && IsInPlot)
			{
				return IsOutside;
			}
			return false;
		}
	}

	internal static bool IsInPlotInside
	{
		get
		{
			if (IsInWard && IsInPlot && !IsInRoom)
			{
				return IsInside;
			}
			return false;
		}
	}

	internal static bool IsInFCRoom
	{
		get
		{
			if (IsInWard && IsInPlot && IsInRoom)
			{
				return IsInside;
			}
			return false;
		}
	}

	internal static bool IsInAptRoom
	{
		get
		{
			if (IsInWard && !IsInPlot && IsInRoom)
			{
				return IsInside;
			}
			return false;
		}
	}

	internal static bool IsInAptLobby
	{
		get
		{
			if (IsInWard && !IsInPlot && !IsInRoom)
			{
				return IsInside;
			}
			return false;
		}
	}

	internal static bool NotResidential
	{
		get
		{
			if (!IsInWard && !IsInPlot && !IsInRoom && !IsInside && !IsOutside)
			{
				return !IsInWorkshop;
			}
			return false;
		}
	}

	internal static EntityInfo? GetTargetAsEntity()
	{
		IPlayerCharacter localPlayer = Plugin.Objects.LocalPlayer;
		IGameObject val = ((localPlayer != null) ? ((IGameObject)localPlayer).TargetObject : null);
		if (val == null)
		{
			return null;
		}
		return new EntityInfo(val);
	}

	internal static void UpdatePlayerList()
	{
		if (!Plugin.Config.Enabled)
		{
			return;
		}
		List<EntityInfo> list = new List<EntityInfo>();
		if (LocalPlayer == null)
		{
			NearbyPlayers = new List<EntityInfo>();
			return;
		}
		HashSet<nint> hashSet = Mare.MareGetNearbyPlayerAddresses();
		LocalPlayer.IsMareSynced = hashSet != null;
		list.Add(LocalPlayer);
		foreach (IPlayerCharacter character in ((IEnumerable<IGameObject>)Plugin.Objects).Where((IGameObject x) => x.IsValid() && x.GameObjectId != ((IGameObject)LocalPlayer.Character).GameObjectId).OfType<IPlayerCharacter>())
		{
			bool isMareSynced = hashSet != null && hashSet.FirstOrDefault((nint x) => x == (nint)((IGameObject)character).Address) != 0;
			EntityInfo item = new EntityInfo(character)
			{
				IsMareSynced = isMareSynced
			};
			list.Add(item);
		}
		NearbyPlayers = list;
	}
}
