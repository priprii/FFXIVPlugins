using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using GLib.Widgets;
using Ktisis.Core.Attributes;
using Ktisis.Editor.Characters.Make;
using Ktisis.Editor.Characters.Types;
using Ktisis.Editor.Context.Types;
using Ktisis.Interface.Components.Chara.Popup;
using Ktisis.Services.Data;
using Ktisis.Structs.Characters;

namespace Ktisis.Interface.Components.Chara;

[Transient]
public class CustomizeEditorTab
{
	private readonly IDataManager _data;

	private readonly ITextureProvider _tex;

	private readonly CustomizeService _discovery;

	private IEditorContext? _context;

	private readonly MakeTypeData _makeTypeData = new MakeTypeData();

	private readonly ParamColorSelectPopup _colorPopup = new ParamColorSelectPopup();

	private readonly FeatureSelectPopup _featurePopup;

	private bool _isSetup;

	private const float SideRatio = 0.35f;

	private const string LegacyTexPath = "chara/common/texture/decal_equip/_stigma.tex";

	private static readonly Vector2 MaxButtonSize = new Vector2(64f, 64f);

	private Vector2 ButtonSize = MaxButtonSize;

	private static readonly CustomizeIndex[] FeatIconParams;

	public ICustomizeEditor Editor { private get; set; }

	public CustomizeEditorTab(IDataManager data, ITextureProvider tex, CustomizeService discovery)
	{
		_data = data;
		_tex = tex;
		_discovery = discovery;
		_featurePopup = new FeatureSelectPopup(tex);
	}

	public void Setup(IEditorContext ctx)
	{
		if (_isSetup)
		{
			return;
		}
		_context = ctx;
		_isSetup = true;
		_makeTypeData.Build(_data, _discovery).ContinueWith(delegate(Task task)
		{
			if (task.Exception != null)
			{
				Ktisis.Log.Error($"Failed to build customize data:\n{task.Exception}");
			}
		});
	}

	public void Draw()
	{
		IEditorContext context = _context;
		if (context != null && context.IsValid)
		{
			ButtonSize = CalcButtonSize();
			Tribe customization = (Tribe)Editor.GetCustomization((CustomizeIndex)4);
			Gender customization2 = (Gender)Editor.GetCustomization((CustomizeIndex)1);
			MakeTypeRace data = _makeTypeData.GetData(customization, customization2);
			if (data != null)
			{
				Draw(data);
				_colorPopup.Draw(Editor);
				_featurePopup.Draw(Editor);
			}
		}
	}

	private void Draw(MakeTypeRace data)
	{
		IEditorContext context = _context;
		if (context != null && context.IsValid)
		{
			DrawSideFrame(data);
			ImGui.SameLine();
			DrawMainFrame(data);
		}
	}

	private void DrawSideFrame(MakeTypeRace data)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		Vector2 vector = ImGui.GetContentRegionAvail();
		if (_context.Config.Editor.UseToolbar)
		{
			vector = new Vector2(MathF.Max(vector.X * 0.35f, 240f), 420f) * ImGuiHelpers.GlobalScale;
		}
		else
		{
			vector.X = MathF.Max(vector.X * 0.35f, 240f);
		}
		ChildDisposable val = ImRaii.Child(ImU8String.op_Implicit("##CustomizeSideFrame"), vector, true);
		try
		{
			float cursorPosX = ImGui.GetCursorPosX();
			DrawBodySelect(data.Gender);
			ImGuiStylePtr style = ImGui.GetStyle();
			ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
			ImGui.SetNextItemWidth(ImGui.CalcItemWidth() - (ImGui.GetCursorPosX() - cursorPosX));
			DrawTribeSelect(data.Tribe);
			ImGui.Spacing();
			DrawFeatSlider((CustomizeIndex)3, data);
			DrawFeatSlider((CustomizeIndex)23, data);
			DrawFeatSlider((CustomizeIndex)21, data);
			ImGui.Spacing();
			DrawFeatParams((CustomizeIndex)16, data);
			DrawEyeColorSwitch();
			DrawIrisSizeSwitch();
			ImGui.Spacing();
			DrawFeatParams((CustomizeIndex)19, data);
			DrawLipColorSwitch();
			ImGui.Spacing();
			DrawFeatParams((CustomizeIndex)14, data);
			DrawFeatParams((CustomizeIndex)17, data);
			DrawFeatParams((CustomizeIndex)18, data);
		}
		finally
		{
			((ChildDisposable)(ref val)).Dispose();
		}
	}

	private void DrawBodySelect(Gender current)
	{
		if (Buttons.IconButton((FontAwesomeIcon)((current == Gender.Masculine) ? 61986 : 61985)))
		{
			Editor.SetCustomization((CustomizeIndex)1, (current != Gender.Feminine) ? ((byte)1) : ((byte)0));
		}
	}

	private void DrawTribeSelect(Tribe current)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		ComboDisposable val = ImRaii.Combo(ImU8String.op_Implicit(Ktisis.Locale.Translate("common.chara_parts.body")), ImU8String.op_Implicit(current.ToString()));
		try
		{
			if (!val.Success)
			{
				return;
			}
			Tribe[] values = Enum.GetValues<Tribe>();
			for (int i = 0; i < values.Length; i++)
			{
				Tribe tribe = values[i];
				if (ImGui.Selectable(ImU8String.op_Implicit(tribe.ToString()), tribe == current, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					Editor.Prepare().SetCustomization((CustomizeIndex)4, (byte)tribe).SetCustomization((CustomizeIndex)0, (byte)Math.Floor(((decimal)(byte)tribe + 1m) / 2m))
						.Apply();
				}
			}
		}
		finally
		{
			((ComboDisposable)(ref val)).Dispose();
		}
	}

	private void DrawSlider(string label, CustomizeIndex index)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		int customization = Editor.GetCustomization(index);
		if (ImGui.SliderInt(ImU8String.op_Implicit(label), ref customization, 0, 100, default(ImU8String), (ImGuiSliderFlags)0))
		{
			Editor.SetCustomization(index, (byte)customization);
		}
	}

	private void DrawFeatSlider(CustomizeIndex index, MakeTypeRace data)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		MakeTypeFeature feature = data.GetFeature(index);
		if (feature != null)
		{
			DrawSlider(feature.Name, index);
		}
	}

	private void DrawFeatParams(CustomizeIndex index, MakeTypeRace data)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		MakeTypeFeature feature = data.GetFeature(index);
		if (feature != null)
		{
			byte customization = Editor.GetCustomization(index);
			byte num = (byte)(customization & -129);
			bool flag = feature.Params.FirstOrDefault()?.Value == 0;
			int num2 = num;
			if (flag)
			{
				num2++;
			}
			if (ImGui.InputInt(ImU8String.op_Implicit(feature.Name), ref num2, 1, 0, default(ImU8String), (ImGuiInputTextFlags)0) && num2 >= (flag ? 1 : 0))
			{
				byte b = (byte)((!flag) ? num2 : (--num2));
				Editor.SetCustomization(index, (byte)(b | (customization & 0x80)));
			}
		}
	}

	private void DrawIrisSizeSwitch()
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		float cursorPosX = ImGui.GetCursorPosX();
		ImGui.SameLine(0f, 0f);
		GroupDisposable val = ImRaii.Group();
		try
		{
			ImGui.SetCursorPosX(cursorPosX);
			byte customization = Editor.GetCustomization((CustomizeIndex)16);
			bool flag = (customization & 0x80) != 0;
			if (ImGui.Checkbox(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.customize.iris")), ref flag))
			{
				Editor.SetCustomization((CustomizeIndex)16, (byte)(customization ^ 0x80));
			}
		}
		finally
		{
			((GroupDisposable)(ref val)).Dispose();
		}
	}

	private void DrawMainFrame(MakeTypeRace data)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f3: Unknown result type (might be due to invalid IL or missing references)
		ChildDisposable val = ImRaii.Child(ImU8String.op_Implicit("##CustomizeMainFrame"), _context.Config.Editor.UseToolbar ? (new Vector2(300f, 420f) * ImGuiHelpers.GlobalScale) : ImGui.GetContentRegionAvail());
		try
		{
			if (val.Success)
			{
				ImGui.Spacing();
				DrawSkinHairColors(data);
				ImGui.Spacing();
				DrawFacePaintOptions(data);
				ImGui.Spacing();
				if (ImGui.CollapsingHeader(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.customize.primary")), (ImGuiTreeNodeFlags)0))
				{
					DrawFeatIconParams(data);
				}
				ImGui.Spacing();
				string text = Ktisis.Locale.Translate("chara_edit.customize.face");
				MakeTypeFeature feature = data.GetFeature((CustomizeIndex)12);
				if (feature != null && HasUniqueFeature(data.Tribe))
				{
					text = text + " / " + feature.Name;
				}
				text = text + " / " + Ktisis.Locale.Translate("chara_edit.customize.face_tat");
				if (ImGui.CollapsingHeader(ImU8String.op_Implicit(text), (ImGuiTreeNodeFlags)0))
				{
					DrawFacialFeatures(data);
				}
			}
		}
		finally
		{
			((ChildDisposable)(ref val)).Dispose();
		}
	}

	private static bool HasUniqueFeature(Tribe tribe)
	{
		if (tribe == Tribe.Wildwood || tribe == Tribe.MoonKeeper || tribe - 11 <= Tribe.Midlander)
		{
			return true;
		}
		return false;
	}

	private static Vector2 CalcButtonSize()
	{
		float num = ImGui.GetWindowSize().X * 0.65f;
		return Vector2.Min(value2: new Vector2(num, num) / 8f, value1: MaxButtonSize);
	}

	private void DrawFeatIconParams(MakeTypeRace data)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		ItemWidthDisposable val = ImRaii.ItemWidth(ImGui.GetContentRegionAvail().X / 2f - ButtonSize.X - (((ImGuiStylePtr)(ref style)).FramePadding.X + ((ImGuiStylePtr)(ref style)).ItemSpacing.X) * 2f);
		try
		{
			int num = 0;
			bool flag = false;
			CustomizeIndex[] featIconParams = FeatIconParams;
			foreach (CustomizeIndex index in featIconParams)
			{
				if (DrawFeatIconParams(data, index))
				{
					flag = ++num % 2 != 0;
					if (flag)
					{
						ImGui.SameLine();
					}
				}
			}
			if (flag)
			{
				ImGui.Dummy(Vector2.Zero);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private bool DrawFeatIconParams(MakeTypeRace data, CustomizeIndex index)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Invalid comparison between Unknown and I4
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_010d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0113: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_011f: Invalid comparison between Unknown and I4
		//IL_0143: Unknown result type (might be due to invalid IL or missing references)
		MakeTypeFeature feature = data.GetFeature(index);
		if (feature == null)
		{
			return false;
		}
		byte customization = Editor.GetCustomization(index);
		bool flag = (int)index == 24;
		byte value = (flag ? ((byte)(customization & -129)) : customization);
		MakeTypeParam param = feature.Params.FirstOrDefault((MakeTypeParam makeTypeParam) => makeTypeParam.Value == value);
		if (DrawFeatIconButton($"{value}", param))
		{
			_featurePopup.Open(feature);
		}
		float y = ImGui.GetItemRectSize().Y;
		ImGui.SameLine();
		GroupDisposable val = ImRaii.Group();
		try
		{
			float y2 = y / 2f - (ImGui.GetFrameHeightWithSpacing() + UiBuilder.DefaultFontSizePx);
			Vector2 zero = Vector2.Zero;
			zero.Y = y2;
			ImGui.Dummy(zero);
			ImGui.Text(ImU8String.op_Implicit(feature.Name));
			int num = value;
			ImU8String val2 = new ImU8String(8, 1);
			((ImU8String)(ref val2)).AppendLiteral("##Input_");
			((ImU8String)(ref val2)).AppendFormatted<CustomizeIndex>(feature.Index);
			if (ImGui.InputInt(val2, ref num, 1, 0, default(ImU8String), (ImGuiInputTextFlags)0) && ((int)index != 5 || feature.Params.Any((MakeTypeParam p) => p.Value == value)))
			{
				Editor.SetCustomization(index, flag ? ((byte)(num | (customization & 0x80))) : ((byte)num));
			}
			return true;
		}
		finally
		{
			((GroupDisposable)(ref val)).Dispose();
		}
	}

	private bool DrawFeatIconButton(string fallback, MakeTypeParam? param)
	{
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Unknown result type (might be due to invalid IL or missing references)
		ColorDisposable val = ImRaii.PushColor((ImGuiCol)21, 0u, true);
		try
		{
			ISharedImmediateTexture val2 = null;
			if (param != null && param.Graphic != 0)
			{
				ITextureProvider tex = _tex;
				GameIconLookup val3 = GameIconLookup.op_Implicit(param.Graphic);
				tex.TryGetFromGameIcon(ref val3, ref val2);
			}
			if (val2 != null)
			{
				return ImGui.ImageButton(val2.GetWrapOrEmpty().Handle, ButtonSize);
			}
			ImU8String val4 = ImU8String.op_Implicit(fallback);
			Vector2 buttonSize = ButtonSize;
			ImGuiStylePtr style = ImGui.GetStyle();
			return ImGui.Button(val4, buttonSize + ((ImGuiStylePtr)(ref style)).FramePadding * 2f);
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void DrawFacePaintOptions(MakeTypeRace data)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		float cursorPosX = ImGui.GetCursorPosX();
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SetCursorPosX(cursorPosX + ((ImGuiStylePtr)(ref style)).FramePadding.X);
		GroupDisposable val = ImRaii.Group();
		try
		{
			DrawFeatColor((CustomizeIndex)25, data);
			ImGui.SameLine(0f);
			byte customization = Editor.GetCustomization((CustomizeIndex)24);
			bool flag = (customization & 0x80) != 0;
			if (ImGui.Checkbox(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.customize.facepaint_flip")), ref flag))
			{
				Editor.SetCustomization((CustomizeIndex)24, (byte)(customization ^ 0x80));
			}
		}
		finally
		{
			((GroupDisposable)(ref val)).Dispose();
		}
	}

	private void DrawFacialFeatures(MakeTypeRace data)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_007b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fd: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		byte customization = Editor.GetCustomization((CustomizeIndex)12);
		DrawFacialFeatureToggles(data, customization);
		ImGui.Spacing();
		float num = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X + (ButtonSize.X + ((ImGuiStylePtr)(ref style)).FramePadding.X * 2f) * 4f;
		ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ((ImGuiStylePtr)(ref style)).FramePadding.X);
		ImGui.SetNextItemWidth(num / 2f);
		int num2 = customization;
		if (ImGui.InputInt(ImU8String.op_Implicit("##FaceFeatureFlags"), ref num2, 1, 0, default(ImU8String), (ImGuiInputTextFlags)0))
		{
			Editor.SetCustomization((CustomizeIndex)12, (byte)num2);
		}
		MakeTypeFeature feature = data.GetFeature((CustomizeIndex)13);
		if (feature != null)
		{
			uint[] colors = _makeTypeData.GetColors((CustomizeIndex)13);
			ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemSpacing.X);
			DrawColorButton((CustomizeIndex)13, colors);
			ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
			ImGui.Text(ImU8String.op_Implicit(feature.Name));
		}
	}

	private void DrawFacialFeatureToggles(MakeTypeRace data, byte current)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		GroupDisposable val = ImRaii.Group();
		try
		{
			ImGuiStylePtr style = ImGui.GetStyle();
			byte customization = Editor.GetCustomization((CustomizeIndex)5);
			if (!data.FaceFeatureIcons.TryGetValue(customization, out uint[] value))
			{
				value = data.FaceFeatureIcons.Values.FirstOrDefault();
			}
			if (value == null)
			{
				value = Array.Empty<uint>();
			}
			IEnumerable<ISharedImmediateTexture> enumerable = value.Select(delegate(uint id)
			{
				//IL_0007: Unknown result type (might be due to invalid IL or missing references)
				//IL_000c: Unknown result type (might be due to invalid IL or missing references)
				ITextureProvider tex = _tex;
				GameIconLookup val4 = GameIconLookup.op_Implicit(id);
				return tex.GetFromGameIcon(ref val4);
			}).Append(_tex.GetFromGame("chara/common/texture/decal_equip/_stigma.tex"));
			int num = 0;
			foreach (ISharedImmediateTexture item in enumerable)
			{
				if (num++ % 4 != 0)
				{
					ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
				}
				byte b = (byte)Math.Pow(2.0, num - 1);
				bool flag = (current & b) != 0;
				ColorDisposable val2 = ImRaii.PushColor((ImGuiCol)21, flag ? ImGui.GetColorU32((ImGuiCol)23) : 0u, true);
				try
				{
					bool flag2;
					if (item != null)
					{
						flag2 = ImGui.ImageButton(item.GetWrapOrEmpty().Handle, ButtonSize);
					}
					else
					{
						ImU8String val3 = new ImU8String(0, 1);
						((ImU8String)(ref val3)).AppendFormatted<int>(num);
						flag2 = ImGui.Button(val3, ButtonSize + ((ImGuiStylePtr)(ref style)).FramePadding * 2f);
					}
					if (flag2)
					{
						Editor.SetCustomization((CustomizeIndex)12, (byte)(current ^ b));
					}
				}
				finally
				{
					((IDisposable)val2)?.Dispose();
				}
			}
		}
		finally
		{
			((GroupDisposable)(ref val)).Dispose();
		}
	}

	private void DrawSkinHairColors(MakeTypeRace data)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ((ImGuiStylePtr)(ref style)).CellPadding.X);
		GroupDisposable val = ImRaii.Group();
		try
		{
			DrawFeatColor((CustomizeIndex)8, data);
			ImGui.SameLine();
			DrawFeatColor((CustomizeIndex)10, data);
			ImGui.SameLine();
			DrawHighlights();
		}
		finally
		{
			((GroupDisposable)(ref val)).Dispose();
		}
	}

	private void DrawColorButton(CustomizeIndex index, uint[] colors)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		byte b = Editor.GetCustomization(index);
		if (colors.Length == 128)
		{
			b = (byte)(b & -129);
		}
		Vector4 vector = ImGui.ColorConvertU32ToFloat4((b < colors.Length) ? colors[b] : 0u);
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(2, 2);
		((ImU8String)(ref val)).AppendFormatted<byte>(b);
		((ImU8String)(ref val)).AppendLiteral("##");
		((ImU8String)(ref val)).AppendFormatted<CustomizeIndex>(index);
		if (ImGui.ColorButton(val, ref vector, (ImGuiColorEditFlags)0, default(Vector2)))
		{
			_colorPopup.Open(index, colors);
		}
	}

	private void DrawFeatColor(CustomizeIndex index, MakeTypeRace data)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		MakeTypeFeature feature = data.GetFeature(index);
		if (feature == null)
		{
			return;
		}
		GroupDisposable val = ImRaii.Group();
		try
		{
			uint[] colors = _makeTypeData.GetColors(index, data.Tribe, data.Gender);
			DrawColorButton(index, colors);
			ImGuiStylePtr style = ImGui.GetStyle();
			ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
			ImGui.Text(ImU8String.op_Implicit(feature.Name));
		}
		finally
		{
			((GroupDisposable)(ref val)).Dispose();
		}
	}

	private void DrawHighlights()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		GroupDisposable val = ImRaii.Group();
		try
		{
			byte customization = Editor.GetCustomization((CustomizeIndex)7);
			bool flag = (customization & 0x80) != 0;
			if (ImGui.Checkbox(ImU8String.op_Implicit("##HighlightToggle"), ref flag))
			{
				Editor.SetCustomization((CustomizeIndex)7, (byte)(customization ^ 0x80));
			}
			ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
			uint[] colors = _makeTypeData.GetColors((CustomizeIndex)11);
			DisabledDisposable val2 = ImRaii.Disabled(!flag);
			try
			{
				DrawColorButton((CustomizeIndex)11, colors);
				ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
				ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.customize.highlights")));
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
		}
		finally
		{
			((GroupDisposable)(ref val)).Dispose();
		}
	}

	private void DrawEyeColorSwitch()
	{
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_013e: Unknown result type (might be due to invalid IL or missing references)
		uint[] colors = _makeTypeData.GetColors((CustomizeIndex)9);
		if (colors.Length == 0)
		{
			return;
		}
		bool flag = Editor.GetHeterochromia();
		ImGuiStylePtr style = ImGui.GetStyle();
		float frameHeight = ImGui.GetFrameHeight();
		ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.CalcItemWidth() - frameHeight * 3f - ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X * 2f);
		GroupDisposable val = ImRaii.Group();
		try
		{
			ColorDisposable val2 = ImRaii.PushColor((ImGuiCol)21, 0u, true);
			try
			{
				if (Buttons.IconButton((FontAwesomeIcon)(flag ? 61735 : 61633), new Vector2(frameHeight, frameHeight)))
				{
					flag = !flag;
					Editor.SetHeterochromia(flag);
				}
			}
			finally
			{
				((IDisposable)val2)?.Dispose();
			}
			ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
			DisabledDisposable val3 = ImRaii.Disabled(!flag);
			try
			{
				DrawColorButton((CustomizeIndex)(flag ? 9 : 15), colors);
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
			ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
			DrawColorButton((CustomizeIndex)(flag ? 15 : 9), colors);
			ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
			ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.customize.eye_color")));
		}
		finally
		{
			((GroupDisposable)(ref val)).Dispose();
		}
	}

	private void DrawLipColorSwitch()
	{
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		uint[] colors = _makeTypeData.GetColors((CustomizeIndex)20);
		if (colors.Length != 0)
		{
			ImGuiStylePtr style = ImGui.GetStyle();
			float frameHeight = ImGui.GetFrameHeight();
			ImGui.SetCursorPosX(ImGui.GetCursorPosX() + ImGui.CalcItemWidth() - frameHeight * 2f - ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
			byte customization = Editor.GetCustomization((CustomizeIndex)19);
			bool flag = (customization & 0x80) != 0;
			if (ImGui.Checkbox(ImU8String.op_Implicit("##ToggleLipColor"), ref flag))
			{
				Editor.SetCustomization((CustomizeIndex)19, (byte)(customization ^ 0x80));
			}
			ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
			DisabledDisposable val = ImRaii.Disabled(!flag);
			try
			{
				DrawColorButton((CustomizeIndex)20, colors);
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
			ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
			ImGui.Text(ImU8String.op_Implicit(Ktisis.Locale.Translate("chara_edit.customize.lipstick")));
		}
	}

	static CustomizeEditorTab()
	{
		CustomizeIndex[] array = new CustomizeIndex[4];
		RuntimeHelpers.InitializeArray(array, (RuntimeFieldHandle)/*OpCode not supported: LdMemberToken*/);
		FeatIconParams = (CustomizeIndex[])(object)array;
	}
}
