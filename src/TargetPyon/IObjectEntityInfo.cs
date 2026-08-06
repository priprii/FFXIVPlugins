using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Common.Math;
using TargetPyon.Extensions;

namespace TargetPyon;

public interface IObjectEntityInfo
{
	ulong Id { get; init; }

	bool IsValid { get; }

	string Name { get; }

	Vector4 NameColour => Vector4.op_Implicit(EntityManager.GetEntityNameColour(this));

	string TypeName { get; }

	Vector4 TypeColour { get; }

	Vector3 Position { get; set; }

	float Rotation { get; set; }

	float Scale { get; set; }

	unsafe double Distance
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

	unsafe double Angle
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

	bool IsVisible { get; }

	void DrawDirection(Vector2 center, float radius, float outline, Vector4 col, Vector4 outlineCol)
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

	void Hide();

	void Show();

	unsafe void FlagAndOpenMap(MapType mapType = (MapType)1u)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		AgentMap* ptr = AgentMap.Instance();
		((AgentMap)ptr).SetFlagMapMarker(((AgentMap)ptr).CurrentTerritoryId, ((AgentMap)ptr).CurrentMapId, Vector3.op_Implicit(Position), 60561u);
		((AgentMap)ptr).OpenMap(((AgentMap)ptr).CurrentMapId, ((AgentMap)ptr).CurrentTerritoryId, Name, mapType);
	}
}
