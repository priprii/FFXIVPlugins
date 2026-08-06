using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Bindings.ImGuizmo;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using GLib.Widgets;
using Ktisis.Common.Utility;
using Ktisis.Core.Attributes;
using Ktisis.Data.Config;
using Ktisis.Data.Config.Sections;
using Ktisis.Editor.Selection;
using Ktisis.Localization;

namespace Ktisis.Interface.Components.Transforms;

[Transient]
public class TransformTable
{
	private readonly ConfigManager _cfg;

	private readonly LocaleManager _locale;

	private bool IsUsed;

	private bool WasFocused;

	private string? WasStepping;

	private Vector3 Angles = Vector3.Zero;

	private Quaternion Value = Quaternion.Identity;

	private const ImGuizmoOperation PositionOp = (ImGuizmoOperation)7;

	private const ImGuizmoOperation RotateOp = (ImGuizmoOperation)120;

	private const ImGuizmoOperation ScaleOp = (ImGuizmoOperation)15232;

	private Transform Transform = new Transform();

	private static readonly Vector3 MinScale = new Vector3(0.001f, 0.001f, 0.001f);

	private static uint[] AxisColors = new uint[3] { 4281684991u, 4278243668u, 4294923264u };

	private static readonly float FastStep = 10f;

	private static readonly float SlowStep = 0.1f;

	private GizmoConfig GizmoConfig => _cfg.File.Gizmo;

	public bool IsActive { get; private set; }

	public bool IsDeactivated { get; private set; }

	public TransformTable(ConfigManager cfg, LocaleManager locale)
	{
		_cfg = cfg;
		_locale = locale;
	}

	public bool Draw(Transform transIn, out Transform transOut, TransformTableFlags flags = TransformTableFlags.Default | TransformTableFlags.Operation)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(15, 1);
		((ImU8String)(ref val)).AppendLiteral("TransformTable_");
		((ImU8String)(ref val)).AppendFormatted<int>(GetHashCode(), "X");
		IdDisposable val2 = ImRaii.PushId(val, true);
		try
		{
			if (!IsActive && !transIn.Rotation.Equals(Value))
			{
				Angles = HkaEulerAngles.ToEuler(transIn.Rotation);
				Value = transIn.Rotation;
			}
			IsUsed = false;
			IsActive = false;
			IsDeactivated = false;
			ItemWidthDisposable val3 = ImRaii.ItemWidth(flags.HasFlag(TransformTableFlags.UseAvailable) ? (CalcTableAvail() - (_cfg.File.Editor.UseToolbar ? 3f : 0f)) : CalcTableWidth());
			try
			{
				bool op = flags.HasFlag(TransformTableFlags.Operation);
				transOut = Transform.Set(transIn);
				if (flags.HasFlag(TransformTableFlags.Position))
				{
					DrawPosition(ref transOut.Position, op);
				}
				if (flags.HasFlag(TransformTableFlags.Rotation))
				{
					DrawRotate(ref transOut.Rotation, op);
				}
				if (flags.HasFlag(TransformTableFlags.Scale) && DrawScale(ref transOut.Scale, op))
				{
					transOut.Scale = Vector3.Max(transOut.Scale, MinScale);
				}
				WasFocused = ImGui.IsWindowFocused();
				return IsUsed;
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	public bool DrawPosition(ref Vector3 position, TransformTableFlags flags = TransformTableFlags.Default | TransformTableFlags.Operation)
	{
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(15, 1);
		((ImU8String)(ref val)).AppendLiteral("TransformTable_");
		((ImU8String)(ref val)).AppendFormatted<int>(GetHashCode(), "X");
		IdDisposable val2 = ImRaii.PushId(val, true);
		try
		{
			IsUsed = false;
			IsDeactivated = false;
			ItemWidthDisposable val3 = ImRaii.ItemWidth(flags.HasFlag(TransformTableFlags.UseAvailable) ? (ImGui.GetContentRegionAvail().X - (_cfg.File.Editor.UseToolbar ? 0.1f : 0f)) : CalcTableWidth());
			try
			{
				bool op = flags.HasFlag(TransformTableFlags.Operation);
				DrawPosition(ref position, op);
				return IsUsed;
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val2)?.Dispose();
		}
	}

	private bool DrawPosition(ref Vector3 pos, bool op)
	{
		bool result = DrawLinear("##TransformTable_Pos", ref pos);
		if (op)
		{
			DrawOperation((ImGuizmoOperation)7, (FontAwesomeIcon)61732, "transform.position");
		}
		return result;
	}

	private bool DrawRotate(ref Quaternion rot, bool op)
	{
		bool num = DrawEuler("##TransformTable_Rotate", ref Angles);
		if (num)
		{
			rot = HkaEulerAngles.ToQuaternion(Angles);
			Value = rot;
		}
		if (op)
		{
			DrawOperation((ImGuizmoOperation)120, (FontAwesomeIcon)58555, "transform.rotation");
		}
		return num;
	}

	private bool DrawScale(ref Vector3 scale, bool op)
	{
		bool result = DrawLinear("##TransformTable_Scale", ref scale);
		if (op)
		{
			DrawOperation((ImGuizmoOperation)15232, (FontAwesomeIcon)61541, "transform.scale");
		}
		return result;
	}

	private void DrawOperation(ImGuizmoOperation op, FontAwesomeIcon icon, string hint)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Invalid comparison between Unknown and I4
		//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemSpacing.X;
		ImGui.SameLine(0f, x);
		bool flag = ((Enum)GizmoConfig.Operation).HasFlag((Enum)(object)(ImGuizmoOperation)8) && !((Enum)GizmoConfig.Operation).HasFlag((Enum)(object)(ImGuizmoOperation)64);
		bool flag2 = ((Enum)GizmoConfig.Operation).HasFlag((Enum)(object)op);
		uint num = (((int)op == 120 && flag) ? 2937061375u : ((!flag2) ? 2952790015u : uint.MaxValue));
		uint num2 = num;
		ColorDisposable val = ImRaii.PushColor((ImGuiCol)0, num2, true);
		try
		{
			if (Buttons.IconButtonTooltip(icon, _locale.Translate(hint)))
			{
				ChangeOperation(op);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void ChangeOperation(ImGuizmoOperation op)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Invalid comparison between Unknown and I4
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		if ((int)op == 120)
		{
			ImGuiIOPtr iO = ImGui.GetIO();
			if (((ImGuiIOPtr)(ref iO)).KeyShift && (!((Enum)GizmoConfig.Operation).HasFlag((Enum)(object)(ImGuizmoOperation)8) || ((Enum)GizmoConfig.Operation).HasFlag((Enum)(object)(ImGuizmoOperation)64)))
			{
				op = (ImGuizmoOperation)(op ^ 0x40);
			}
		}
		if (GuiHelpers.GetSelectMode() == SelectMode.Multiple)
		{
			GizmoConfig gizmoConfig = GizmoConfig;
			gizmoConfig.Operation |= op;
		}
		else
		{
			GizmoConfig.Operation = op;
		}
	}

	private bool DrawLinear(string id, ref Vector3 vec)
	{
		bool flag = DrawXYZ(id, ref vec, 0.001f);
		IsUsed |= flag;
		return flag;
	}

	private bool DrawEuler(string id, ref Vector3 vec)
	{
		bool flag = DrawXYZ(id, ref vec, 0.2f);
		if (flag)
		{
			vec = vec.NormalizeAngles();
		}
		IsUsed |= flag;
		return flag;
	}

	private bool DrawXYZ(string id, ref Vector3 vec, float speed)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		ItemWidthDisposable val = ImRaii.ItemWidth((ImGui.CalcItemWidth() - x * 2f) / 3f);
		try
		{
			flag |= DrawAxis(id + "_X", ref vec.X, speed, AxisColors[0]);
			ImGui.SameLine(0f, x);
			flag |= DrawAxis(id + "_Y", ref vec.Y, speed, AxisColors[1]);
			ImGui.SameLine(0f, x);
			return flag | DrawAxis(id + "_Z", ref vec.Z, speed, AxisColors[2]);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private bool DrawAxis(string id, ref float value, float speed, uint col)
	{
		//IL_0004: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		bool flag = false;
		ImGuiStylePtr style = ImGui.GetStyle();
		StyleDisposable val = ImRaii.PushStyle((ImGuiStyleVar)10, ((ImGuiStylePtr)(ref style)).FramePadding + new Vector2(0.1f, 0.1f), true);
		bool result;
		try
		{
			StyleDisposable val2 = ImRaii.PushStyle((ImGuiStyleVar)12, 0.1f, true);
			try
			{
				ColorDisposable val3 = ImRaii.PushColor((ImGuiCol)5, col, true);
				try
				{
					result = ImGui.DragFloat(ImU8String.op_Implicit(id), ref value, speed, 0f, 0f, ImU8String.op_Implicit("%.3f"), (ImGuiSliderFlags)64);
					if (_cfg.File.Keybinds.ScrollAllow)
					{
						if (ImGui.IsItemHovered() && (!_cfg.File.Keybinds.ScrollModifier || ImGui.IsKeyDown((ImGuiKey)643)))
						{
							ImGuiP.SetItemUsingMouseWheel();
							ImGuiIOPtr iO = ImGui.GetIO();
							int num = (int)((ImGuiIOPtr)(ref iO)).MouseWheel;
							if (num != 0)
							{
								float num2 = speed * 10f;
								if (ImGui.IsKeyDown((ImGuiKey)642))
								{
									num2 *= FastStep;
								}
								else if (ImGui.IsKeyDown((ImGuiKey)641))
								{
									num2 *= SlowStep;
								}
								value += (float)num * num2;
								result = true;
								WasStepping = id;
							}
						}
						else if (WasStepping == id)
						{
							flag = true;
						}
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		IsActive |= ImGui.IsItemActive();
		IsDeactivated |= ImGui.IsItemDeactivatedAfterEdit() | (WasStepping == id && flag) | (WasFocused && !ImGui.IsWindowFocused());
		if (flag)
		{
			WasStepping = null;
		}
		return result;
	}

	private static float CalcTableAvail()
	{
		return ImGui.GetContentRegionAvail().X - CalcIconSpacing();
	}

	private static float CalcTableWidth()
	{
		return UiBuilder.DefaultFontSizePx * 4f * 3f * ImGuiHelpers.GlobalScale;
	}

	private static float CalcIconSpacing()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		float defaultFontSizePx = UiBuilder.DefaultFontSizePx;
		ImGuiStylePtr style = ImGui.GetStyle();
		return (defaultFontSizePx + ((ImGuiStylePtr)(ref style)).ItemSpacing.X * 2f) * ImGuiHelpers.GlobalScale;
	}

	public static float CalcWidth()
	{
		return CalcTableWidth() + CalcIconSpacing();
	}
}
