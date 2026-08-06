using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Bindings.ImGuizmo;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using PyonPix.Extensions;
using PyonPix.Services.Game;
using PyonPix.Structs.Ui;
using PyonPix.Utility;

namespace PyonPix.Ui.Components;

public class TransformEditor
{
	private ImGuizmoOperation GizmoOperation;

	private bool WasUsingTable;

	private bool WasUsingGizmo;

	private bool IsGizmoVisible;

	private float IconSize => 20f * ImGuiHelpers.GlobalScale;

	private float Spacing => 1f * ImGuiHelpers.GlobalScale;

	public UIState DrawTable(string id, ref Vector3 pos, ref Quaternion rot, Action<string>? posAction = null, Action<string>? rotAction = null)
	{
		Vector3 scl = Vector3.Zero;
		return DrawTable(id, ref pos, ref rot, ref scl, posAction, rotAction);
	}

	public UIState DrawTable(string id, ref Vector3 pos, ref Quaternion rot, ref Vector3 scl, Action<string>? posAction = null, Action<string>? rotAction = null, Action<string>? sclAction = null)
	{
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Invalid comparison between Unknown and I4
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Invalid comparison between Unknown and I4
		//IL_022b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Invalid comparison between Unknown and I4
		//IL_0284: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026f: Invalid comparison between Unknown and I4
		//IL_038d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0397: Invalid comparison between Unknown and I4
		//IL_03ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Invalid comparison between Unknown and I4
		UIState result = UIState.None;
		float width = (ImGui.GetContentRegionAvail().X - IconSize) / 3f - Spacing * 2f;
		Vector3 vector = Vector3.Normalize(Vector3.Transform(Vector3.UnitX, rot));
		Vector3 vector2 = Vector3.Normalize(Vector3.Transform(Vector3.UnitY, rot));
		Vector3 vector3 = Vector3.Normalize(Vector3.Transform(Vector3.UnitZ, rot));
		Vector3 vector4 = new Vector3(Vector3.Dot(pos, vector), Vector3.Dot(pos, vector2), Vector3.Dot(pos, vector3));
		Vector3 vector5 = rot.QuaternionToEulerDeg();
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		ImGui.PushID(ImU8String.op_Implicit(id));
		ImGui.PushStyleVar((ImGuiStyleVar)13, new Vector2(Spacing));
		if (ImGuiEx.IconToggleButton((FontAwesomeIcon)61618, id + "posGizmo", IsGizmoVisible && (int)GizmoOperation == 7, disabled: false, "Toggle Position Gizmo", null, IconSize))
		{
			if (IsGizmoVisible && (int)GizmoOperation == 7)
			{
				IsGizmoVisible = false;
			}
			else
			{
				IsGizmoVisible = true;
				GizmoOperation = (ImGuizmoOperation)7;
			}
		}
		ImGui.SameLine();
		flag2 |= ImGuiEx.AxisXDrag(id + "posX", ref vector4.X, width);
		flag3 |= ImGui.IsItemActive();
		posAction?.Invoke(id + "posX");
		ImGui.SameLine();
		flag2 |= ImGuiEx.AxisYDrag(id + "posY", ref vector4.Y, width);
		flag3 |= ImGui.IsItemActive();
		posAction?.Invoke(id + "posY");
		ImGui.SameLine();
		flag2 |= ImGuiEx.AxisZDrag(id + "posZ", ref vector4.Z, width);
		flag3 |= ImGui.IsItemActive();
		posAction?.Invoke(id + "posZ");
		if (ImGuiEx.IconToggleButton((FontAwesomeIcon)58555, id + "rotGizmo", IsGizmoVisible && (int)GizmoOperation == 120, disabled: false, "Toggle Rotation Gizmo", null, IconSize))
		{
			if (IsGizmoVisible && (int)GizmoOperation == 120)
			{
				IsGizmoVisible = false;
			}
			else
			{
				IsGizmoVisible = true;
				GizmoOperation = (ImGuizmoOperation)120;
			}
		}
		ImGui.SameLine();
		flag2 |= ImGuiEx.AxisXDrag(id + "rotX", ref vector5.X, width, 0.01f);
		flag3 |= ImGui.IsItemActive();
		rotAction?.Invoke(id + "rotX");
		ImGui.SameLine();
		flag2 |= ImGuiEx.AxisYDrag(id + "rotY", ref vector5.Y, width, 0.01f);
		flag3 |= ImGui.IsItemActive();
		rotAction?.Invoke(id + "rotY");
		ImGui.SameLine();
		flag2 |= ImGuiEx.AxisZDrag(id + "rotZ", ref vector5.Z, width, 0.01f);
		flag3 |= ImGui.IsItemActive();
		rotAction?.Invoke(id + "rotZ");
		if (scl != Vector3.Zero)
		{
			if (ImGuiEx.IconToggleButton((FontAwesomeIcon)61541, id + "sclGizmo", IsGizmoVisible && (int)GizmoOperation == 896, disabled: false, "Toggle Scale Gizmo", null, IconSize))
			{
				if (IsGizmoVisible && (int)GizmoOperation == 896)
				{
					IsGizmoVisible = false;
				}
				else
				{
					IsGizmoVisible = true;
					GizmoOperation = (ImGuizmoOperation)896;
				}
			}
			ImGui.SameLine();
			flag2 |= ImGuiEx.AxisXDrag(id + "sclX", ref scl.X, width);
			flag3 |= ImGui.IsItemActive();
			sclAction?.Invoke(id + "sclX");
			ImGui.SameLine();
			flag2 |= ImGuiEx.AxisYDrag(id + "sclY", ref scl.Y, width);
			flag3 |= ImGui.IsItemActive();
			sclAction?.Invoke(id + "sclY");
			ImGui.SameLine();
			flag2 |= ImGuiEx.AxisZDrag(id + "sclZ", ref scl.Z, width);
			flag3 |= ImGui.IsItemActive();
			sclAction?.Invoke(id + "sclZ");
		}
		ImGui.PopStyleVar();
		if (flag2 || flag)
		{
			Quaternion quaternion = Quaternion.CreateFromYawPitchRoll(vector5.Y.DegToRad(), vector5.X.DegToRad(), vector5.Z.DegToRad());
			Vector3 vector6 = vector * vector4.X + vector2 * vector4.Y + vector3 * vector4.Z;
			pos = vector6;
			rot = quaternion;
			WasUsingTable = true;
			result = UIState.Using;
		}
		if (!flag3 && WasUsingTable)
		{
			WasUsingTable = false;
			result = UIState.Ended;
		}
		ImGui.PopID();
		return result;
	}

	public void HideGizmo()
	{
		IsGizmoVisible = false;
	}

	public UIState DrawGizmo(string id, ref Vector3 pos, ref Quaternion rot, ImGuizmoMode mode = (ImGuizmoMode)0)
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		Vector3 scl = Vector3.Zero;
		return DrawGizmo(id, ref pos, ref rot, ref scl, mode);
	}

	public UIState DrawGizmo(string id, ref Vector3 pos, ref Quaternion rot, ref Vector3 scl, ImGuizmoMode mode = (ImGuizmoMode)0)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		UIState result = UIState.None;
		if (!IsGizmoVisible)
		{
			return result;
		}
		ImGui.PushID(ImU8String.op_Implicit(id));
		ImGui.PushStyleColor((ImGuiCol)2, Vector4.Zero);
		ImGui.PushStyleColor((ImGuiCol)5, Vector4.Zero);
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(12, 1);
		((ImU8String)(ref val)).AppendFormatted<string>(id);
		((ImU8String)(ref val)).AppendLiteral("PyonPixGizmo");
		if (ImGui.Begin(val, ref IsGizmoVisible, (ImGuiWindowFlags)791467))
		{
			if (!IsGizmoVisible || ImGui.IsKeyReleased((ImGuiKey)526))
			{
				IsGizmoVisible = false;
				ImGui.End();
				ImGui.PopStyleColor(2);
				ImGui.PopID();
				return result;
			}
			ImGui.SetWindowPos(Vector2.Zero, (ImGuiCond)0);
			ImGui.SetWindowSize(UiUtil.GameResolution, (ImGuiCond)0);
			ImGuizmo.SetOrthographic(false);
			ImGuizmo.SetDrawlist(ImGui.GetWindowDrawList());
			ImGuizmo.SetRect(0f, 0f, (float)UiUtil.GameWidth, (float)UiUtil.GameHeight);
			Matrix4x4 viewMatrix = CameraService.GetViewMatrix();
			Matrix4x4 projectionMatrixForGizmo = CameraService.GetProjectionMatrixForGizmo();
			Matrix4x4 matrix = Matrix4x4.CreateScale((scl == Vector3.Zero) ? Vector3.One : scl) * Matrix4x4.CreateFromQuaternion(rot) * Matrix4x4.CreateTranslation(pos);
			bool num = ImGuizmo.IsUsing();
			if (ImGuizmo.Manipulate(ref viewMatrix, ref projectionMatrixForGizmo, GizmoOperation, (ImGuizmoMode)0, ref matrix) && Matrix4x4.Decompose(matrix, out var scale, out var rotation, out var translation))
			{
				float num2 = 0.0001f;
				bool num3 = Vector3.DistanceSquared(pos, translation) > num2 * num2;
				bool flag = scl != Vector3.Zero && Vector3.DistanceSquared(scl, scale) > num2 * num2;
				float x = Math.Clamp(MathF.Abs(Quaternion.Dot(rot, rotation)), -1f, 1f);
				bool flag2 = 2f * MathF.Acos(x) > num2;
				if (num3 || flag2 || flag)
				{
					pos = translation;
					rot = rotation;
					scl = scale;
					WasUsingGizmo = true;
					result = UIState.Using;
				}
			}
			if (!num && WasUsingGizmo)
			{
				WasUsingGizmo = false;
				result = UIState.Ended;
			}
			ImGui.End();
		}
		ImGui.PopStyleColor(2);
		ImGui.PopID();
		return result;
	}
}
