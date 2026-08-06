using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using FFXIVClientStructs.FFXIV.Common.Math;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace TriggerPyon;

public class EntityInfo
{
	public IGameObject GameObject;

	private string _Name = string.Empty;

	private string _HomeWorld = string.Empty;

	private string _CompanyTag = string.Empty;

	public bool IsMareSynced { get; set; }

	internal IPlayerCharacter Character => (IPlayerCharacter)GameObject;

	internal unsafe GameObject* GameObjectPtr => GameObject.ToCsGameObject();

	internal unsafe Character* CharacterPtr => GameObject.ToCsPlayerCharacter();

	internal string Name
	{
		get
		{
			if (string.IsNullOrWhiteSpace(_Name))
			{
				_Name = GameObject.Name.TextValue;
			}
			return _Name;
		}
	}

	internal string Forename
	{
		get
		{
			if (!Name.Contains(' '))
			{
				return string.Empty;
			}
			return Name.Split(' ')[0];
		}
	}

	internal string Surname
	{
		get
		{
			if (!Name.Contains(' '))
			{
				return string.Empty;
			}
			return Name.Split(' ')[1];
		}
	}

	internal bool IsLocalPlayer
	{
		get
		{
			if (PlayerManager.LocalPlayer != null)
			{
				return GameObject.GameObjectId == PlayerManager.LocalPlayer.GameObject.GameObjectId;
			}
			return false;
		}
	}

	internal unsafe Gender Gender
	{
		get
		{
			if (((Character)CharacterPtr).Sex != 0)
			{
				return Gender.Female;
			}
			return Gender.Male;
		}
	}

	internal unsafe Race Race
	{
		get
		{
			switch (((ModelContainer)(&((Character)CharacterPtr).ModelContainer)).ModelSkeletonId - 20000)
			{
			case 101:
			case 201:
				return Race.Midlander;
			case 301:
			case 401:
				return Race.Highlander;
			case 501:
			case 601:
				return Race.Elezen;
			case 701:
			case 801:
				return Race.Miqote;
			case 901:
			case 1001:
				return Race.Roegadyn;
			case 1101:
			case 1201:
				return Race.Lalafell;
			case 1301:
			case 1401:
				return Race.AuRa;
			case 1501:
			case 1601:
				return Race.Hrothgar;
			case 1701:
			case 1801:
				return Race.Viera;
			default:
				return Race.Unknown;
			}
		}
	}

	internal JobInfo Job => new JobInfo((Character != null) ? ((ICharacter)Character).ClassJob.RowId : 0u);

	internal unsafe string CompanyTag
	{
		get
		{
			if (string.IsNullOrWhiteSpace(_CompanyTag))
			{
				_CompanyTag = ((Character)CharacterPtr).FreeCompanyTagString;
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
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0047: Unknown result type (might be due to invalid IL or missing references)
			//IL_004c: Unknown result type (might be due to invalid IL or missing references)
			if (string.IsNullOrWhiteSpace(_HomeWorld))
			{
				object obj;
				if (Character == null)
				{
					obj = "";
				}
				else
				{
					World? valueNullable = Character.HomeWorld.ValueNullable;
					if (!valueNullable.HasValue)
					{
						obj = null;
					}
					else
					{
						World valueOrDefault = valueNullable.GetValueOrDefault();
						obj = ((object)((World)(ref valueOrDefault)).Name/*cast due to constrained. prefix*/).ToString();
					}
					if (obj == null)
					{
						obj = "";
					}
				}
				_HomeWorld = (string)obj;
			}
			return _HomeWorld;
		}
	}

	internal byte Level
	{
		get
		{
			IPlayerCharacter character = Character;
			if (character == null)
			{
				return 0;
			}
			return ((ICharacter)character).Level;
		}
	}

	internal Vector3 Position => Vector3.op_Implicit(GameObject.Position);

	internal unsafe float Angle => ((GameObject)GameObjectPtr).Rotation;

	internal unsafe double DistanceFromLocalPlayer
	{
		get
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0029: Unknown result type (might be due to invalid IL or missing references)
			EntityInfo localPlayer = PlayerManager.LocalPlayer;
			if (localPlayer == null)
			{
				return 0.0;
			}
			Vector3 val = ((Character)localPlayer.CharacterPtr).Position - Position;
			return ((Vector3)(ref val)).Magnitude;
		}
	}

	internal unsafe float WorldSpaceAngleFromLocalPlayer
	{
		get
		{
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			EntityInfo localPlayer = PlayerManager.LocalPlayer;
			if (localPlayer == null)
			{
				return 0f;
			}
			Vector3 vector = Vector3.op_Implicit(Position - ((Character)localPlayer.CharacterPtr).Position);
			float num = (float)Math.Atan2(vector.Z, 0f - vector.X);
			num -= (float)Math.PI / 2f;
			if ((double)num < -Math.PI)
			{
				num += (float)Math.PI * 2f;
			}
			if ((double)num > Math.PI)
			{
				num -= (float)Math.PI * 2f;
			}
			return num;
		}
	}

	internal unsafe double AngleFromLocalPlayer
	{
		get
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			EntityInfo localPlayer = PlayerManager.LocalPlayer;
			if (localPlayer == null)
			{
				return 0.0;
			}
			Vector3 value = Vector3.op_Implicit(Position - ((Character)localPlayer.CharacterPtr).Position);
			value = Vector3.Normalize(value);
			float angle = localPlayer.Angle;
			float num = (float)((double)(0f - value.X) * Math.Cos(0f - angle) - (double)value.Z * Math.Sin(0f - angle));
			return Math.Atan2((float)((double)(0f - value.X) * Math.Sin(angle) + (double)(0f - value.Z) * Math.Cos(angle)), num);
		}
	}

	internal unsafe double AngleFromTarget
	{
		get
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			IGameObject target = Target;
			if (target == null)
			{
				return 0.0;
			}
			Vector3 value = Vector3.op_Implicit(Position - ((GameObject)target.ToCsGameObject()).Position);
			value = Vector3.Normalize(value);
			float rotation = target.Rotation;
			float num = (float)((double)(0f - value.X) * Math.Cos(0f - rotation) - (double)value.Z * Math.Sin(0f - rotation));
			return Math.Atan2((float)((double)(0f - value.X) * Math.Sin(rotation) + (double)(0f - value.Z) * Math.Cos(rotation)), num);
		}
	}

	internal string DirectionStr
	{
		get
		{
			if (PlayerManager.LocalPlayer == null || !IsValid)
			{
				return "";
			}
			double num = AngleFromLocalPlayer * (180.0 / Math.PI);
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

	internal unsafe bool IsFriend => ((Character)CharacterPtr).IsFriend;

	internal unsafe bool IsBlocked => (int)((InfoProxyBlacklist)InfoProxyBlacklist.Instance()).GetBlockResultType(((Character)CharacterPtr).AccountId, ((Character)CharacterPtr).ContentId) != 1;

	internal unsafe bool IsEnemyPlayer => ((Character)CharacterPtr).IsHostile;

	internal unsafe bool IsInParty
	{
		get
		{
			if (IsValid)
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

	internal unsafe bool InCombat => ((Character)CharacterPtr).InCombat;

	internal unsafe ushort EmoteId => ((EmoteController)(&((Character)CharacterPtr).EmoteController)).EmoteId;

	internal bool IsEmote
	{
		get
		{
			if (!IsLoopingEmote && !IsSleeping)
			{
				return Plugin.SpecialEmotes.FirstOrDefault((SpecialEmote x) => x.ID == EmoteId) == null;
			}
			return false;
		}
	}

	internal unsafe bool IsLoopingEmote
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			if ((int)GetPoseType() == 0)
			{
				return ((Enum)((Character)CharacterPtr).Mode).HasFlag((Enum)(object)(CharacterModes)3);
			}
			return false;
		}
	}

	internal bool IsMoving => (int)GetPoseType() == 255;

	internal bool IsStandingIdle
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			if ((int)GetPoseType() == 0)
			{
				return !IsLoopingEmote;
			}
			return false;
		}
	}

	internal bool IsChairSitting => (int)GetPoseType() == 2;

	internal bool IsGroundSitting => (int)GetPoseType() == 3;

	internal bool IsSleeping => (int)GetPoseType() == 4;

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

	internal IGameObject Target => GameObject.TargetObject;

	internal ulong TargetObjectId => GameObject.TargetObjectId;

	internal unsafe ulong SoftTargetObjectId => GameObjectId.op_Implicit(((Character)CharacterPtr).GetSoftTargetId());

	internal bool IsTargetingMe
	{
		get
		{
			if (PlayerManager.LocalPlayer != null)
			{
				return TargetObjectId == ((IGameObject)PlayerManager.LocalPlayer.Character).GameObjectId;
			}
			return false;
		}
	}

	public EntityInfo(IPlayerCharacter baseObject)
	{
		GameObject = (IGameObject)(object)baseObject;
	}

	public EntityInfo(IGameObject baseObject)
	{
		GameObject = baseObject;
	}

	internal void FaceTowardsEntity(EntityInfo? entity, bool inverse = false)
	{
		EntityInfo localPlayer = PlayerManager.LocalPlayer;
		if (localPlayer != null && localPlayer.Character == Character && entity != null)
		{
			if (inverse)
			{
				SetRotation(entity.WorldSpaceAngleFromLocalPlayer + (float)Math.PI);
			}
			else
			{
				SetRotation(entity.WorldSpaceAngleFromLocalPlayer);
			}
		}
	}

	internal void FaceSameAsEntity(EntityInfo? entity, bool inverse = false)
	{
		EntityInfo localPlayer = PlayerManager.LocalPlayer;
		if (localPlayer != null && localPlayer.Character == Character && entity != null)
		{
			if (inverse)
			{
				SetRotation(entity.Angle + (float)Math.PI);
			}
			else
			{
				SetRotation(entity.Angle);
			}
		}
	}

	internal unsafe void SetRotation(float angle)
	{
		EntityInfo localPlayer = PlayerManager.LocalPlayer;
		if (localPlayer != null && localPlayer.Character == Character)
		{
			((GameObject)GameObjectPtr).SetRotation(angle);
		}
	}

	internal unsafe void SetRotationOffset(float angle)
	{
		EntityInfo localPlayer = PlayerManager.LocalPlayer;
		if (localPlayer != null && localPlayer.Character == Character)
		{
			((GameObject)GameObjectPtr).SetRotation(((GameObject)GameObjectPtr).Rotation + angle);
		}
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
		double angleFromLocalPlayer = AngleFromLocalPlayer;
		float x = (float)Math.Cos(angleFromLocalPlayer);
		float y = (float)Math.Sin(angleFromLocalPlayer);
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

	internal bool IsWithinReactionAngleAndDistanceToLocalPlayer(ReactionOptions options)
	{
		if (PlayerManager.LocalPlayer == null)
		{
			return false;
		}
		if (!options.RestrictRange)
		{
			return true;
		}
		double distanceFromLocalPlayer = DistanceFromLocalPlayer;
		if (distanceFromLocalPlayer < (double)options.RestrictedDistanceMin || distanceFromLocalPlayer > (double)options.RestrictedDistanceMax)
		{
			return false;
		}
		if (options.RestrictedAngleArea <= 0f)
		{
			return true;
		}
		double angleFromLocalPlayer = AngleFromLocalPlayer;
		double num = -Math.PI / 2.0;
		double num2 = options.RestrictedAngleDirection.DegreesToRadians() + num;
		double num3 = Math.PI * 2.0 * (double)Math.Clamp(options.RestrictedAngleArea, 0f, 1f) / 2.0;
		return Math.Abs(NormalizeAngle(angleFromLocalPlayer - num2)) <= num3;
	}

	private static double NormalizeAngle(double angle)
	{
		while (angle > Math.PI)
		{
			angle -= Math.PI * 2.0;
		}
		while (angle < -Math.PI)
		{
			angle += Math.PI * 2.0;
		}
		return angle;
	}

	internal unsafe PoseType GetPoseType()
	{
		return (PoseType)(byte)((EmoteController)(&((Character)CharacterPtr).EmoteController)).GetPoseKind();
	}

	internal unsafe void SetEmote(ushort emoteId)
	{
		((EmoteManager)EmoteManager.Instance()).ExecuteEmote(emoteId, (PlayEmoteOption*)null);
	}

	internal bool CanReactionInterruptCurrentState(ReactionOptions? reactionOptions)
	{
		if (reactionOptions == null)
		{
			return true;
		}
		if (reactionOptions.StateConditions.HasFlag(StateConditionType.Moving) && IsMoving)
		{
			return false;
		}
		if (reactionOptions.StateConditions.HasFlag(StateConditionType.LoopingEmote) && IsLoopingEmote)
		{
			return false;
		}
		if (reactionOptions.StateConditions.HasFlag(StateConditionType.Emote) && IsEmote)
		{
			return false;
		}
		if (reactionOptions.StateConditions.HasFlag(StateConditionType.Standing) && IsStandingIdle)
		{
			return false;
		}
		if (reactionOptions.StateConditions.HasFlag(StateConditionType.GroundSit) && IsGroundSitting)
		{
			return false;
		}
		if (reactionOptions.StateConditions.HasFlag(StateConditionType.ChairSit) && IsChairSitting)
		{
			return false;
		}
		if (reactionOptions.StateConditions.HasFlag(StateConditionType.Sleeping) && IsSleeping)
		{
			return false;
		}
		return true;
	}

	internal void Validate(IGameObject o)
	{
		GameObject = o;
	}

	internal void SetAsTarget()
	{
		if (IsValid)
		{
			GameObject.SetAsTarget();
		}
	}

	internal void SetAsMouseOverTarget()
	{
		if (IsValid)
		{
			GameObject.SetAsMouseOverTarget();
		}
	}

	internal void SetAsFocusTarget()
	{
		if (IsValid)
		{
			GameObject.SetAsFocusTarget();
		}
	}

	internal void SetAsSoftTarget()
	{
		if (IsValid)
		{
			GameObject.SetAsSoftTarget();
		}
	}

	internal bool IsTargetOf(IGameObject o)
	{
		return o.TargetObjectId == ObjectId;
	}

	internal unsafe void OpenPlate()
	{
		((AgentCharaCard)AgentCharaCard.Instance()).OpenCharaCard(GameObjectPtr);
	}

	internal unsafe void OpenExamine()
	{
		((AgentInspect)AgentInspect.Instance()).ExamineCharacter(GameObject.EntityId, false);
	}

	internal unsafe void SendTell()
	{
		((UIModule)UIModule.Instance()).ProcessChatBoxEntry(Utf8String.FromString("/tell " + Name + "@" + HomeWorld), (IntPtr)0, false);
	}

	internal unsafe void InviteToParty()
	{
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		((InfoProxyPartyInvite)InfoProxyPartyInvite.Instance()).InviteToParty(((Character)CharacterPtr).ContentId, ((Character)CharacterPtr).GetName(), ((Character)CharacterPtr).HomeWorld);
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
