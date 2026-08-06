using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace TriggerPyon;

public static class GradientBuilder
{
	public class FixedColour(ushort position, uint colour)
	{
		public ushort Position = position;

		public uint Colour = colour;

		public Guid Guid { get; init; } = Guid.NewGuid();
	}

	public record Pair(FixedColour Begin, FixedColour End)
	{
		public int Length => End.Position - Begin.Position;

		public FixedColour ColourAt(float t)
		{
			ushort position = (ushort)MathF.Round((float)(int)Begin.Position + t * (float)Length);
			return new FixedColour(position, Mode switch
			{
				0 => LerpOpaque(Begin.Colour, End.Colour, t), 
				1 => LerpHueOpaque(Begin.Colour, End.Colour, t), 
				_ => 0u, 
			});
		}
	}

	public static int Length = 64;

	public static readonly List<FixedColour> FixedColours;

	public static readonly List<Pair> Pairs;

	public static Guid Editing;

	public static int Mode;

	public static GradientAnimationStyle AnimationStyle;

	public static string PreviewText;

	public static Vector3 PreviewTextColour;

	public static GradientStyle? GeneratedStyle;

	public static void UpdatePairs()
	{
		Pairs.Clear();
		FixedColour fixedColour = FixedColours.Find((FixedColour f) => f.Position == 0);
		if (fixedColour == null)
		{
			fixedColour = new FixedColour(0, uint.MaxValue);
			FixedColours.Insert(0, fixedColour);
		}
		if (FixedColours.Find((FixedColour f) => f.Position == ushort.MaxValue) == null)
		{
			FixedColours.Add(new FixedColour(ushort.MaxValue, fixedColour.Colour));
		}
		List<FixedColour> list = FixedColours.OrderBy((FixedColour f) => f.Position).ToList();
		for (int num = 0; num < list.Count - 1; num++)
		{
			FixedColour begin = list[num];
			FixedColour end = list[num + 1];
			Pairs.Add(new Pair(begin, end));
		}
	}

	public static void GenerateStyle(int? steps = null)
	{
		int valueOrDefault = steps.GetValueOrDefault();
		if (!steps.HasValue)
		{
			valueOrDefault = Length;
			steps = valueOrDefault;
		}
		if (steps < 2)
		{
			steps = 2;
		}
		if (steps > 1024)
		{
			steps = 1024;
		}
		UpdatePairs();
		List<RGB> list = new List<RGB>();
		double num = 65535.0 / (double)((steps ?? Length) - 1);
		for (int i = 0; i < steps; i++)
		{
			float pos = (float)num * (float)i;
			FixedColour fixedColour = FixedColours.Find((FixedColour f) => f.Position == (ushort)MathF.Round(pos));
			uint num2 = 0u;
			if (fixedColour != null)
			{
				num2 = fixedColour.Colour;
			}
			else
			{
				Pair pair = Pairs.Find((Pair p) => (float)(int)p.Begin.Position < pos && (float)(int)p.End.Position > pos);
				if (pair == null)
				{
					throw new Exception($"Failed to get pair at position: {pos}");
				}
				float t = (pos - (float)(int)pair.Begin.Position) / (float)(pair.End.Position - pair.Begin.Position);
				num2 = pair.ColourAt(t).Colour;
			}
			list.Add(UintToRGB(num2));
		}
		byte[,] array = new byte[list.Count, 3];
		for (int num3 = 0; num3 < list.Count; num3++)
		{
			array[num3, 0] = list[num3].R;
			array[num3, 1] = list[num3].G;
			array[num3, 2] = list[num3].B;
		}
		GeneratedStyle = new GradientStyle("Generated Style", array, AnimationStyle);
	}

	public static uint LerpOpaque(uint start, uint end, float t)
	{
		return ImGui.ColorConvertFloat4ToU32(LerpOpaque(ImGui.ColorConvertU32ToFloat4(start), ImGui.ColorConvertU32ToFloat4(end), t));
	}

	public static Vector4 LerpOpaque(Vector4 start, Vector4 end, float t)
	{
		t = Math.Clamp(t, 0f, 1f);
		Vector4 result = start + (end - start) * t;
		result.W = 1f;
		return result;
	}

	private static Vector3 GetHSV(Vector4 v)
	{
		Vector3 result = default(Vector3);
		ImGui.ColorConvertRGBtoHSV(v.X, v.Y, v.Z, ref result.X, ref result.Y, ref result.Z);
		return result;
	}

	private static float DeltaAngle(float a, float b)
	{
		float num = (b - a) % 360f;
		if (num > 180f)
		{
			num -= 360f;
		}
		if (num < -180f)
		{
			num += 360f;
		}
		return num;
	}

	public static float Lerp(float a, float b, float t)
	{
		return a + (b - a) * t;
	}

	public static uint LerpHueOpaque(uint start, uint end, float t)
	{
		return ImGui.ColorConvertFloat4ToU32(LerpHueOpaque(ImGui.ColorConvertU32ToFloat4(start), ImGui.ColorConvertU32ToFloat4(end), t));
	}

	public static Vector4 LerpHueOpaque(Vector4 start, Vector4 end, float t)
	{
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		Vector3 hSV = GetHSV(start);
		Vector3 hSV2 = GetHSV(end);
		float num = DeltaAngle(hSV.X * 360f, hSV2.X * 360f) / 360f;
		float num2 = hSV.X + num * t;
		if (num2 < 0f)
		{
			num2 += 1f;
		}
		else if (num2 > 1f)
		{
			num2 -= 1f;
		}
		float num3 = Lerp(hSV.Y, hSV2.Y, t);
		float num4 = Lerp(hSV.Z, hSV2.Z, t);
		return ImGui.HSV(num2, num3, num4, 1f).Value;
	}

	public static RGB UintToRGB(uint color)
	{
		return new RGB((byte)(color & 0xFF), (byte)((color >> 8) & 0xFF), (byte)((color >> 16) & 0xFF));
	}

	public static void Draw()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_00df: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_015d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0373: Unknown result type (might be due to invalid IL or missing references)
		//IL_0378: Unknown result type (might be due to invalid IL or missing references)
		//IL_0445: Unknown result type (might be due to invalid IL or missing references)
		//IL_044a: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0483: Unknown result type (might be due to invalid IL or missing references)
		//IL_0488: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0689: Unknown result type (might be due to invalid IL or missing references)
		//IL_06f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0707: Unknown result type (might be due to invalid IL or missing references)
		//IL_0734: Unknown result type (might be due to invalid IL or missing references)
		//IL_082f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0834: Unknown result type (might be due to invalid IL or missing references)
		//IL_0845: Unknown result type (might be due to invalid IL or missing references)
		//IL_0858: Unknown result type (might be due to invalid IL or missing references)
		//IL_085e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0886: Unknown result type (might be due to invalid IL or missing references)
		//IL_088d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0892: Unknown result type (might be due to invalid IL or missing references)
		//IL_08a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_093d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0970: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_08c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_08e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_09bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_09c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a52: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a57: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a73: Unknown result type (might be due to invalid IL or missing references)
		//IL_0abf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ac4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0afc: Unknown result type (might be due to invalid IL or missing references)
		IdDisposable val = ImRaii.PushId(ImU8String.op_Implicit("GradientBuilder"), true);
		try
		{
			if (ImGui.SmallButton(ImU8String.op_Implicit("Spread")) && FixedColours.Count > 2)
			{
				double num = 65535.0 / (double)(FixedColours.Count - 1);
				int num2 = 0;
				foreach (FixedColour item in FixedColours.OrderBy((FixedColour f) => f.Position))
				{
					item.Position = (ushort)Math.Round(num * (double)num2++);
				}
				UpdatePairs();
				GenerateStyle();
			}
			ImGui.SameLine();
			ImGui.SetNextItemWidth(100f);
			ImU8String val2 = ImU8String.op_Implicit("Mode");
			ImU8String val3 = default(ImU8String);
			if (ImGui.SliderInt(val2, ref Mode, 0, 1, val3, (ImGuiSliderFlags)0))
			{
				GenerateStyle();
			}
			ImGui.Dummy(new Vector2(ImGui.GetContentRegionAvail().X, 32f));
			ImGui.Dummy(new Vector2(16f));
			ImGui.SameLine();
			ImGui.Dummy(new Vector2(ImGui.GetContentRegionAvail().X - 16f, 100f));
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			Vector2 itemRectMin = ImGui.GetItemRectMin();
			Vector2 itemRectSize = ImGui.GetItemRectSize();
			UpdatePairs();
			for (int num3 = 0; (float)num3 < itemRectSize.X; num3++)
			{
				ushort pos = (ushort)MathF.Round((float)num3 / itemRectSize.X * 65535f);
				Vector2 vector = itemRectMin + new Vector2(num3, 0f);
				Vector2 vector2 = itemRectMin + new Vector2(num3, itemRectSize.Y);
				Pair pair = Pairs.Find((Pair p) => p.Begin.Position <= pos && p.End.Position > pos);
				if (!(pair == null))
				{
					float t = (float)(pos - pair.Begin.Position) / (float)(pair.End.Position - pair.Begin.Position);
					((ImDrawListPtr)(ref windowDrawList)).AddLine(vector, vector2, pair.ColourAt(t).Colour);
				}
			}
			foreach (FixedColour fixedColour3 in FixedColours)
			{
				Vector2 vector3 = itemRectMin + new Vector2(itemRectSize.X * (float)(int)fixedColour3.Position / 65535f, -16f);
				Vector2 vector4 = vector3 + new Vector2(0f, itemRectSize.Y + 32f);
				((ImDrawListPtr)(ref windowDrawList)).AddLine(vector3, vector4, fixedColour3.Colour, 4f);
				((ImDrawListPtr)(ref windowDrawList)).AddCircleFilled(vector3, 10f, fixedColour3.Colour, 16);
				((ImDrawListPtr)(ref windowDrawList)).AddCircleFilled(vector4, 10f, fixedColour3.Colour, 16);
				if (ImGui.IsMouseHoveringRect(vector3 - new Vector2(10f), vector3 + new Vector2(10f)) || ImGui.IsMouseHoveringRect(vector4 - new Vector2(10f), vector4 + new Vector2(10f)))
				{
					((ImDrawListPtr)(ref windowDrawList)).AddCircle(vector3, 10f, 4294967040u, 16, 2f);
					((ImDrawListPtr)(ref windowDrawList)).AddCircle(vector4, 10f, 4294967040u, 16, 2f);
					ImGuiIOPtr iO = ImGui.GetIO();
					if (((ImGuiIOPtr)(ref iO)).MouseClicked[0])
					{
						if (Editing == fixedColour3.Guid)
						{
							Editing = Guid.Empty;
						}
						else
						{
							Editing = fixedColour3.Guid;
						}
					}
				}
				else if (Editing == fixedColour3.Guid)
				{
					((ImDrawListPtr)(ref windowDrawList)).AddCircle(vector3, 10f, 4278190335u, 16, 2f);
					((ImDrawListPtr)(ref windowDrawList)).AddCircle(vector4, 10f, 4278190335u, 16, 2f);
				}
				else
				{
					((ImDrawListPtr)(ref windowDrawList)).AddCircle(vector3, 10f, uint.MaxValue, 16);
					((ImDrawListPtr)(ref windowDrawList)).AddCircle(vector4, 10f, uint.MaxValue, 16);
				}
			}
			ImDrawListPtr windowDrawList2 = ImGui.GetWindowDrawList();
			((ImDrawListPtr)(ref windowDrawList2)).AddRect(ImGui.GetItemRectMin(), ImGui.GetItemRectMax(), uint.MaxValue);
			if (ImGui.IsItemHovered())
			{
				float num4 = (ImGui.GetMousePos() - itemRectMin).X / itemRectSize.X;
				val3 = new ImU8String(3, 1);
				((ImU8String)(ref val3)).AppendLiteral("@ ");
				((ImU8String)(ref val3)).AppendFormatted<float>(MathF.Round(num4 * 100f, 1));
				((ImU8String)(ref val3)).AppendLiteral("%");
				ImGui.SetTooltip(val3);
				if (ImGui.IsMouseClicked((ImGuiMouseButton)0))
				{
					float pos2 = num4 * 65535f;
					ushort posShort = (ushort)pos2;
					bool flag = ((pos2 == 0f || pos2 == 65535f) ? true : false);
					if (flag || FixedColours.All((FixedColour p) => p.Position != posShort))
					{
						Pair pair2 = Pairs.Find((Pair p) => (float)(int)p.Begin.Position < pos2 && (float)(int)p.End.Position > pos2);
						if (pair2 != null)
						{
							float t2 = (pos2 - (float)(int)pair2.Begin.Position) / (float)(pair2.End.Position - pair2.Begin.Position);
							FixedColour fixedColour = pair2.ColourAt(t2);
							FixedColours.Add(fixedColour);
							Editing = fixedColour.Guid;
						}
					}
				}
			}
			ImGui.Dummy(new Vector2(32f));
			ImGui.SameLine();
			GroupDisposable val4 = ImRaii.Group();
			try
			{
				ImGui.Dummy(new Vector2(32f));
				FixedColour fixedColour2 = FixedColours.Find((FixedColour f) => f.Guid == Editing);
				if (fixedColour2 == null)
				{
					Editing = Guid.Empty;
				}
				DisabledDisposable val5 = ImRaii.Disabled(fixedColour2 == null);
				try
				{
					if (fixedColour2 == null)
					{
						fixedColour2 = new FixedColour(32767, 0u);
					}
					float num5 = (float)(int)fixedColour2.Position * 100f / 65535f;
					Vector4 vector5 = ImGui.ColorConvertU32ToFloat4((uint)(((int?)fixedColour2?.Colour) ?? (-1)));
					bool flag2 = false;
					ImGui.SetNextItemWidth(300f);
					DisabledDisposable val6 = ImRaii.Disabled(fixedColour2 == null || fixedColour2.Position == 0 || fixedColour2.Position == ushort.MaxValue);
					try
					{
						if (ImGui.SmallButton(ImU8String.op_Implicit("Delete Node")) && fixedColour2 != null)
						{
							FixedColours.Remove(fixedColour2);
						}
					}
					finally
					{
						((IDisposable)val6)?.Dispose();
					}
					bool flag = fixedColour2 == null;
					if (!flag)
					{
						ushort position = fixedColour2.Position;
						bool flag3 = ((position == 0 || position == ushort.MaxValue) ? true : false);
						flag = flag3;
					}
					val6 = ImRaii.Disabled(flag);
					try
					{
						flag2 |= ImGui.SliderFloat(ImU8String.op_Implicit("Position"), ref num5, 0f, 100f, ImU8String.op_Implicit("%.1f"), (ImGuiSliderFlags)0);
					}
					finally
					{
						((IDisposable)val6)?.Dispose();
					}
					ImGui.SetNextItemWidth(300f);
					if ((flag2 | ImGui.ColorPicker4(ImU8String.op_Implicit("Colour"), ref vector5, (ImGuiColorEditFlags)2)) && fixedColour2 != null && Editing != Guid.Empty)
					{
						FixedColours.Remove(fixedColour2);
						if (fixedColour2.Position == 0)
						{
							FixedColours.RemoveAll((FixedColour f) => f.Position == ushort.MaxValue);
						}
						ushort num6 = (ushort)(num5 / 100f * 65535f);
						ushort position = fixedColour2.Position;
						if ((position != 0 && position != ushort.MaxValue) || 1 == 0)
						{
							num6 = ushort.Clamp(num6, 1, 65534);
						}
						FixedColours.Add(new FixedColour(num6, ImGui.ColorConvertFloat4ToU32(vector5))
						{
							Guid = fixedColour2.Guid
						});
						GenerateStyle();
					}
				}
				finally
				{
					((IDisposable)val5)?.Dispose();
				}
				ImGui.Separator();
				GroupDisposable val7 = ImRaii.Group();
				try
				{
					ImGui.SetNextItemWidth(200f);
					ImU8String val8 = ImU8String.op_Implicit("Export Steps");
					ImU8String val9 = default(ImU8String);
					if (ImGui.SliderInt(val8, ref Length, 32, 512, val9, (ImGuiSliderFlags)0))
					{
						GenerateStyle();
					}
					ImGui.SetNextItemWidth(200f);
					ImU8String val10 = ImU8String.op_Implicit("Preview Animation Style");
					val9 = new ImU8String(0, 1);
					((ImU8String)(ref val9)).AppendFormatted<GradientAnimationStyle>(AnimationStyle);
					if (ImGui.BeginCombo(val10, val9, (ImGuiComboFlags)0))
					{
						GradientAnimationStyle[] values = Enum.GetValues<GradientAnimationStyle>();
						foreach (GradientAnimationStyle gradientAnimationStyle in values)
						{
							ImU8String val11 = new ImU8String(25, 2);
							((ImU8String)(ref val11)).AppendFormatted<GradientAnimationStyle>(gradientAnimationStyle);
							((ImU8String)(ref val11)).AppendLiteral("##gradientAnimationStyle+");
							((ImU8String)(ref val11)).AppendFormatted<GradientAnimationStyle>(gradientAnimationStyle);
							if (ImGui.Selectable(val11, AnimationStyle == gradientAnimationStyle, (ImGuiSelectableFlags)0, default(Vector2)))
							{
								AnimationStyle = gradientAnimationStyle;
								GenerateStyle();
							}
						}
						ImGui.EndCombo();
					}
					ImGui.SetNextItemWidth(200f);
					if (ImGui.InputText(ImU8String.op_Implicit("Preview Text"), ref PreviewText, 32, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null))
					{
						GenerateStyle();
					}
					ImGui.SetNextItemWidth(200f);
					if (ImGui.ColorEdit3(ImU8String.op_Implicit("Preview Colour"), ref PreviewTextColour, (ImGuiColorEditFlags)32))
					{
						GenerateStyle();
					}
				}
				finally
				{
					((GroupDisposable)(ref val7)).Dispose();
				}
			}
			finally
			{
				((GroupDisposable)(ref val4)).Dispose();
			}
			ImGui.SameLine();
			ImGui.Dummy(new Vector2(32f));
			ImGui.SameLine();
			GroupDisposable val12 = ImRaii.Group();
			try
			{
				ImGui.Dummy(new Vector2(32f));
				foreach (FixedColour item2 in FixedColours.OrderBy((FixedColour f) => f.Position))
				{
					ColorDisposable val13 = ImRaii.PushColor((ImGuiCol)23, item2.Colour & 0x80FFFFFFu, true);
					try
					{
						ColorDisposable val14 = ImRaii.PushColor((ImGuiCol)22, item2.Colour & 0x40FFFFFF, true);
						try
						{
							ColorDisposable val15 = ImRaii.PushColor((ImGuiCol)21, item2.Colour, true);
							try
							{
								ImU8String val16 = new ImU8String(8, 1);
								((ImU8String)(ref val16)).AppendLiteral("##color_");
								((ImU8String)(ref val16)).AppendFormatted<Guid>(item2.Guid);
								if (ImGui.Button(val16, new Vector2(ImGui.GetTextLineHeightWithSpacing())))
								{
									Editing = item2.Guid;
								}
							}
							finally
							{
								((IDisposable)val15)?.Dispose();
							}
						}
						finally
						{
							((IDisposable)val14)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val13)?.Dispose();
					}
					ImGui.SameLine();
					ImU8String val17 = new ImU8String(3, 1);
					((ImU8String)(ref val17)).AppendLiteral("@ ");
					((ImU8String)(ref val17)).AppendFormatted<float>(MathF.Round((float)(item2.Position * 100) / 65535f, 1));
					((ImU8String)(ref val17)).AppendLiteral("%");
					ImGui.Text(val17);
				}
			}
			finally
			{
				((GroupDisposable)(ref val12)).Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	static GradientBuilder()
	{
		int num = 1;
		List<FixedColour> list = new List<FixedColour>(num);
		CollectionsMarshal.SetCount(list, num);
		CollectionsMarshal.AsSpan(list)[0] = new FixedColour(32767, 4278190080u);
		FixedColours = list;
		Pairs = new List<Pair>();
		Editing = Guid.Empty;
		Mode = 0;
		AnimationStyle = GradientAnimationStyle.Wave;
		PreviewText = "Preview Title";
		PreviewTextColour = Vector3.Zero;
	}
}
