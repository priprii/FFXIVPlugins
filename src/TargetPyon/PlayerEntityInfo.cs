using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Bindings.ImGui;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Common.Math;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using TargetPyon.Extensions;

namespace TargetPyon;

public class PlayerEntityInfo
{
	public IGameObject GameObject;

	private string _Name = string.Empty;

	private string _HomeWorld = string.Empty;

	private string _CompanyTag = string.Empty;

	internal bool IsNearby { get; set; }

	public bool IsMareSynced { get; set; }

	internal bool IsPlayerCharacter => GameObject is IPlayerCharacter;

	internal bool IsBattleNpc => GameObject is IBattleNpc;

	internal IPlayerCharacter? Character
	{
		get
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_0016: Expected O, but got Unknown
			if (!IsPlayerCharacter)
			{
				return null;
			}
			return (IPlayerCharacter)GameObject;
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

	internal unsafe GameObject* GameObjectPtr => GameObject.ToCsGameObject();

	internal unsafe Character* CharacterPtr => GameObject.ToCsPlayerCharacter();

	internal unsafe BattleChara* BattleNpcPtr => GameObject.ToCsBattleChara();

	internal string Name
	{
		get
		{
			if (string.IsNullOrWhiteSpace(_Name))
			{
				_Name = (StringExtensions.IsNullOrWhitespace(GameObject.Name.TextValue) ? "???" : GameObject.Name.TextValue);
			}
			return _Name;
		}
	}

	internal Vector4 NameColour => Vector4.op_Implicit(EntityManager.GetEntityNameColour(this));

	internal JobInfo Job => new JobInfo((Character != null) ? ((ICharacter)Character).ClassJob.RowId : 0u);

	internal unsafe string CompanyTag
	{
		get
		{
			if (string.IsNullOrWhiteSpace(_CompanyTag))
			{
				_CompanyTag = (IsPlayerCharacter ? ((Character)CharacterPtr).FreeCompanyTagString : "");
			}
			return _CompanyTag;
		}
	}

	internal string HomeWorld
	{
		get
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_0038: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrWhiteSpace(_HomeWorld))
			{
				object homeWorld;
				if (Character == null)
				{
					homeWorld = "";
				}
				else
				{
					World value = Character.HomeWorld.Value;
					homeWorld = ((object)((World)(ref value)).Name/*cast due to constrained. prefix*/).ToString();
				}
				_HomeWorld = (string)homeWorld;
			}
			return _HomeWorld;
		}
	}

	internal byte Level
	{
		get
		{
			IPlayerCharacter? character = Character;
			if (character == null)
			{
				IBattleNpc? battleNpc = BattleNpc;
				if (battleNpc == null)
				{
					return 0;
				}
				return ((ICharacter)battleNpc).Level;
			}
			return ((ICharacter)character).Level;
		}
	}

	internal Vector3 Position => Vector3.op_Implicit(GameObject.Position);

	internal unsafe double Distance
	{
		get
		{
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			IPlayerCharacter localPlayer = Plugin.Objects.LocalPlayer;
			if (localPlayer == null)
			{
				return 0.0;
			}
			Vector3 val = ((GameObject)localPlayer.ToCsGameObject()).Position - Position;
			return Math.Round(((Vector3)(ref val)).Magnitude, MidpointRounding.ToEven);
		}
	}

	internal unsafe double Angle
	{
		get
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			IPlayerCharacter localPlayer = Plugin.Objects.LocalPlayer;
			if (localPlayer == null)
			{
				return 0.0;
			}
			Vector3 value = Vector3.op_Implicit(Position - ((GameObject)localPlayer.ToCsGameObject()).Position);
			value = Vector3.Normalize(value);
			float rotation = ((IGameObject)localPlayer).Rotation;
			float num = (float)((double)(0f - value.X) * Math.Cos(0f - rotation) - (double)value.Z * Math.Sin(0f - rotation));
			return Math.Atan2((float)((double)(0f - value.X) * Math.Sin(rotation) + (double)(0f - value.Z) * Math.Cos(rotation)), num);
		}
	}

	internal string DirectionStr
	{
		get
		{
			if (Plugin.Objects.LocalPlayer == null || !IsValid || !IsNearby)
			{
				return "";
			}
			double num = Angle * (180.0 / Math.PI);
			if (num < 0.0)
			{
				num += 360.0;
			}
			if (num >= 337.5 || num < 22.5)
			{
				return "→";
			}
			if (num >= 22.5 && num < 67.5)
			{
				return "↗";
			}
			if (num >= 67.5 && num < 112.5)
			{
				return "↑";
			}
			if (num >= 112.5 && num < 157.5)
			{
				return "↖";
			}
			if (num >= 157.5 && num < 202.5)
			{
				return "←";
			}
			if (num >= 202.5 && num < 247.5)
			{
				return "↙";
			}
			if (num >= 247.5 && num < 292.5)
			{
				return "↓";
			}
			if (num >= 292.5 && num < 337.5)
			{
				return "↘";
			}
			return "";
		}
	}

	internal unsafe bool IsFriend
	{
		get
		{
			if (IsPlayerCharacter)
			{
				return ((Character)CharacterPtr).IsFriend;
			}
			return false;
		}
	}

	internal unsafe bool IsBlocked
	{
		get
		{
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Invalid comparison between Unknown and I4
			if (IsPlayerCharacter)
			{
				return (int)((InfoProxyBlacklist)InfoProxyBlacklist.Instance()).GetBlockResultType(((Character)CharacterPtr).AccountId, ((Character)CharacterPtr).ContentId) != 1;
			}
			return false;
		}
	}

	internal unsafe bool IsEnemyPlayer
	{
		get
		{
			if (IsPlayerCharacter)
			{
				return ((Character)CharacterPtr).IsHostile;
			}
			return false;
		}
	}

	internal unsafe bool IsInParty
	{
		get
		{
			if (IsValid && IsPlayerCharacter)
			{
				return ((Character)CharacterPtr).IsPartyMember;
			}
			return false;
		}
	}

	internal bool IsKnownPlayer
	{
		get
		{
			if (!IsInParty)
			{
				return IsFriend;
			}
			return true;
		}
	}

	internal bool IsDead => GameObject.IsDead;

	internal ulong ObjectId => GameObject.GameObjectId;

	internal bool IsValid
	{
		get
		{
			if (GameObject != null && GameObject.IsValid())
			{
				return Name == GameObject.Name.TextValue;
			}
			return false;
		}
	}

	internal bool IsTargetValid
	{
		get
		{
			if (GameObject.TargetObject != null && GameObject.TargetObject.IsValid())
			{
				return GameObject.TargetObject.IsTargetable;
			}
			return false;
		}
	}

	internal IGameObject? Target => GameObject.TargetObject;

	internal ulong TargetObjectId => GameObject.TargetObjectId;

	internal unsafe ulong SoftTargetObjectId => GameObjectId.op_Implicit(IsPlayerCharacter ? ((Character)CharacterPtr).GetSoftTargetId() : ((BattleChara)BattleNpcPtr).GetSoftTargetId());

	internal bool IsHardTargetingMe
	{
		get
		{
			if (Plugin.Objects.LocalPlayer != null)
			{
				return TargetObjectId == ((IGameObject)Plugin.Objects.LocalPlayer).GameObjectId;
			}
			return false;
		}
	}

	internal bool IsSoftTargetingMe
	{
		get
		{
			if (Plugin.Config.IncludeSoftTarget && Plugin.Objects.LocalPlayer != null)
			{
				return SoftTargetObjectId == ((IGameObject)Plugin.Objects.LocalPlayer).GameObjectId;
			}
			return false;
		}
	}

	internal bool IsTargetingMe
	{
		get
		{
			if (!IsHardTargetingMe)
			{
				return IsSoftTargetingMe;
			}
			return true;
		}
	}

	internal bool IsCamTarget => IPC.GetCamTarget() == GameObject.EntityId;

	internal unsafe bool IsVisible => !((Enum)((GameObject)GameObjectPtr).RenderFlags).HasFlag((Enum)(object)(VisibilityFlags)2);

	public PlayerEntityInfo(IPlayerCharacter baseObject)
	{
		GameObject = (IGameObject)(object)baseObject;
	}

	internal void DrawDirection(Vector2 center, float radius, float outline, Vector4 col, Vector4 outlineCol)
	{
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0118: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009a: Unknown result type (might be due to invalid IL or missing references)
		double angle = Angle;
		float x = (float)Math.Cos(angle);
		float y = (float)Math.Sin(angle);
		ImDrawListPtr windowDrawList;
		if (outline > 0f)
		{
			float num = radius + outline;
			windowDrawList = ImGui.GetWindowDrawList();
			((ImDrawListPtr)(ref windowDrawList)).AddCircleFilled(center, num, ImGui.GetColorU32(outlineCol));
			Vector2 vector = new Vector2(x, y);
			Vector2 vector2 = center + vector * (num * 2f);
			Vector2 vector3 = new Vector2(0f - vector.Y, vector.X);
			Vector2 vector4 = center + vector3 * num;
			Vector2 vector5 = center - vector3 * num;
			windowDrawList = ImGui.GetWindowDrawList();
			((ImDrawListPtr)(ref windowDrawList)).AddTriangleFilled(vector2, vector4, vector5, ImGui.GetColorU32(outlineCol));
		}
		windowDrawList = ImGui.GetWindowDrawList();
		((ImDrawListPtr)(ref windowDrawList)).AddCircleFilled(center, radius, ImGui.GetColorU32(col));
		Vector2 vector6 = new Vector2(x, y);
		Vector2 vector7 = center + vector6 * (radius * 2f);
		Vector2 vector8 = new Vector2(0f - vector6.Y, vector6.X);
		Vector2 vector9 = center + vector8 * radius;
		Vector2 vector10 = center - vector8 * radius;
		windowDrawList = ImGui.GetWindowDrawList();
		((ImDrawListPtr)(ref windowDrawList)).AddTriangleFilled(vector7, vector9, vector10, ImGui.GetColorU32(col));
	}

	internal void Validate(IGameObject o)
	{
		GameObject = o;
	}

	internal void SetAsTarget()
	{
		if (IsValid && IsNearby)
		{
			GameObject.SetAsTarget();
		}
	}

	internal void SetAsMouseOverTarget()
	{
		if (IsValid && IsNearby)
		{
			GameObject.SetAsMouseOverTarget();
		}
	}

	internal void SetAsFocusTarget()
	{
		if (IsValid && IsNearby)
		{
			GameObject.SetAsFocusTarget();
		}
	}

	internal void SetAsSoftTarget()
	{
		if (IsValid && IsNearby)
		{
			GameObject.SetAsSoftTarget();
		}
	}

	internal unsafe bool IsTargetOf(IGameObject o)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		if (o.TargetObjectId != ObjectId)
		{
			if (Plugin.Config.IncludeSoftTarget && o is IPlayerCharacter)
			{
				return ((Character)o.ToCsPlayerCharacter()).GetSoftTargetId() == GameObjectId.op_Implicit(ObjectId);
			}
			return false;
		}
		return true;
	}

	internal unsafe void OpenPlate()
	{
		if (IsPlayerCharacter)
		{
			((AgentCharaCard)AgentCharaCard.Instance()).OpenCharaCard(GameObjectPtr);
		}
	}

	internal unsafe void OpenExamine()
	{
		if (IsPlayerCharacter)
		{
			((AgentInspect)AgentInspect.Instance()).ExamineCharacter(GameObject.EntityId, false);
		}
	}

	internal unsafe void SendTell()
	{
		if (IsPlayerCharacter)
		{
			((UIModule)UIModule.Instance()).ProcessChatBoxEntry(Utf8String.FromString("/tell " + Name + "@" + HomeWorld), (IntPtr)0, false);
		}
	}

	internal unsafe void InviteToParty()
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		if (IsPlayerCharacter)
		{
			((InfoProxyPartyInvite)InfoProxyPartyInvite.Instance()).InviteToParty(((Character)CharacterPtr).ContentId, ((Character)CharacterPtr).GetName(), ((Character)CharacterPtr).HomeWorld);
		}
	}

	internal void ToggleCamTarget()
	{
		if (IsCamTarget)
		{
			IPC.ResetCamTarget();
			Plugin.MainWindow.RemoveResetCamTitleButton();
		}
		else if (IPC.SetCamTarget(GameObject.GameObjectId))
		{
			Plugin.MainWindow.AddResetCamTitleButton();
		}
	}

	internal unsafe void Hide()
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		if (IsVisible)
		{
			Unsafe.Write(&((GameObject)GameObjectPtr).RenderFlags, (VisibilityFlags)2050);
		}
	}

	internal unsafe void Show()
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		if (!IsVisible)
		{
			Unsafe.Write(&((GameObject)GameObjectPtr).RenderFlags, (VisibilityFlags)0);
		}
	}

	internal void Block()
	{
		if (IsPlayerCharacter)
		{
			Plugin.BlacklistManager.Block(Name, HomeWorld);
		}
	}

	internal void Unblock()
	{
		if (IsPlayerCharacter)
		{
			Plugin.BlacklistManager.Unblock(Name, HomeWorld);
		}
	}

	internal unsafe void FlagAndOpenMap(MapType mapType = (MapType)1u)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		AgentMap* ptr = AgentMap.Instance();
		((AgentMap)ptr).SetFlagMapMarker(((AgentMap)ptr).CurrentTerritoryId, ((AgentMap)ptr).CurrentMapId, Vector3.op_Implicit(Position), 60561u);
		((AgentMap)ptr).OpenMap(((AgentMap)ptr).CurrentMapId, ((AgentMap)ptr).CurrentTerritoryId, Name, mapType);
	}

	internal void SearchPlayerOnLodestone()
	{
		if (IsPlayerCharacter)
		{
			string regionCode = GetRegionCode(HomeWorld);
			if (!string.IsNullOrWhiteSpace(regionCode))
			{
				Util.OpenLink($"https://{regionCode}.finalfantasyxiv.com/lodestone/character/?q={Name}&worldname={HomeWorld}");
			}
		}
	}

	internal void SearchPlayerOnTomestone()
	{
		if (IsPlayerCharacter)
		{
			Util.OpenLink("http://tomestone.gg/character-name/" + HomeWorld + "/" + Name);
		}
	}

	internal string GetRegionCode(string worldName)
	{
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		ExcelSheet<World> excelSheet = Plugin.DataManager.GetExcelSheet<World>((ClientLanguage?)null, (string)null);
		if (!this.TryGetFirst<World>((IEnumerable<World>)excelSheet, (Predicate<World>)((World x) => ((object)((World)(ref x)).Name/*cast due to constrained. prefix*/).ToString().Equals(worldName, StringComparison.InvariantCultureIgnoreCase)), out World result) || !IsWorldValid(result))
		{
			return string.Empty;
		}
		return GetRegionCode(result);
	}

	internal unsafe bool IsWorldValid(World world)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		ReadOnlySeString name = ((World)(ref world)).Name;
		if (((ReadOnlySeString)(ref name)).IsEmpty || GetRegionCode(world) == string.Empty)
		{
			return false;
		}
		name = ((World)(ref world)).Name;
		return char.IsUpper(((object)(*(ReadOnlySeString*)(&name))/*cast due to constrained. prefix*/).ToString()[0]);
	}

	internal string GetRegionCode(World world)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		WorldDCGroupType? valueNullable = ((World)(ref world)).DataCenter.ValueNullable;
		uint? num;
		if (!valueNullable.HasValue)
		{
			num = null;
		}
		else
		{
			WorldDCGroupType valueOrDefault = valueNullable.GetValueOrDefault();
			num = ((WorldDCGroupType)(ref valueOrDefault)).Region.RowId;
		}
		return num switch
		{
			1u => "jp", 
			2u => "na", 
			3u => "eu", 
			4u => "eu", 
			_ => string.Empty, 
		};
	}

	internal bool TryGetFirst<T>(IEnumerable<T> values, Predicate<T> predicate, out T result) where T : struct
	{
		using IEnumerator<T> enumerator = values.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (predicate(enumerator.Current))
			{
				result = enumerator.Current;
				return true;
			}
		}
		result = default(T);
		return false;
	}
}
