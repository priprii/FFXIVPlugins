using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using GLib.Popups;
using GLib.Widgets;
using Ktisis.Common.Extensions;
using Ktisis.Common.Utility;
using Ktisis.Core.Attributes;
using Ktisis.Data.Config.Props;
using Ktisis.Data.Serialization;
using Ktisis.Editor.Characters.State;
using Ktisis.Editor.Characters.Types;
using Ktisis.GameData.Excel;
using Ktisis.Interface.Components.Chara.Types;
using Lumina.Data;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;

namespace Ktisis.Interface.Components.Chara;

[Transient]
public class EquipmentEditorTab
{
	private readonly IDataManager _data;

	private readonly ITextureProvider _tex;

	private readonly PopupList<ItemSheet> _itemSelectPopup;

	private readonly PopupList<Stain> _dyeSelectPopup;

	private readonly PopupList<Glasses> _glassesSelectPopup;

	private readonly PopupList<PropEntry> _propSelectPopup;

	private Task _fetchData;

	private IEquipmentEditor _editor;

	private PropSchema _propSchema;

	private static readonly EquipSlot[] EquipSlots = (from index in Enum.GetValues<EquipIndex>()
		select index.ToEquipSlot()).ToArray();

	private static readonly Vector2 ButtonSize = new Vector2(42f, 42f);

	private EquipSlot ItemSelectSlot;

	private List<ItemSheet> ItemSelectList = new List<ItemSheet>();

	private EquipSlot DyeSelectSlot;

	private int DyeSelectIndex;

	private int GlassesSelectIndex;

	private bool _itemsRaii;

	private readonly List<ItemSheet> Items = new List<ItemSheet>();

	private readonly List<Stain> Stains = new List<Stain>();

	private readonly List<Glasses> Glasses = new List<Glasses>();

	private readonly List<PropEntry> Props = new List<PropEntry>();

	private readonly object _equipUpdateLock = new object();

	private readonly Dictionary<EquipSlot, ItemInfo> Equipped = new Dictionary<EquipSlot, ItemInfo>();

	public IEquipmentEditor Editor
	{
		private get
		{
			return _editor;
		}
		set
		{
			_editor = value;
			InvalidateCache();
		}
	}

	public EquipmentEditorTab(IDataManager data, ITextureProvider tex)
	{
		_data = data;
		_tex = tex;
		_propSchema = SchemaReader.ReadProps();
		_itemSelectPopup = new PopupList<ItemSheet>("##ItemSelectPopup", ItemSelectDrawRow).WithSearch(ItemSelectSearchPredicate);
		_dyeSelectPopup = new PopupList<Stain>("##DyeSelectPopup", DyeSelectDrawRow).WithSearch(DyeSelectSearchPredicate);
		_glassesSelectPopup = new PopupList<Glasses>("##GlassesSelectPopup", GlassesSelectDrawRow).WithSearch(GlassesSelectSearchPredicate);
		_propSelectPopup = new PopupList<PropEntry>("##PropSelectPopup", PropDrawRow).WithSearch(PropSearchPredicate);
	}

	private static float CalcItemHeight()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		float textLineHeight = ImGui.GetTextLineHeight();
		ImGuiStylePtr style = ImGui.GetStyle();
		return (textLineHeight + ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.Y) * 2f;
	}

	public void Draw()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		FetchData();
		ImGuiStylePtr style = ImGui.GetStyle();
		ItemWidthDisposable val = ImRaii.ItemWidth(ImGui.GetWindowSize().X / 2f - ((ImGuiStylePtr)(ref style)).ItemSpacing.X);
		try
		{
			lock (_equipUpdateLock)
			{
				DrawItemSlots(EquipSlots.Take(5).Prepend(EquipSlot.MainHand));
				ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemSpacing.X);
				DrawItemSlots(EquipSlots.Skip(5).Prepend(EquipSlot.OffHand));
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
		DrawGlassesSelect();
		DrawPopups();
	}

	private void DrawPopups()
	{
		DrawItemSelectPopup();
		DrawDyeSelectPopup();
		DrawGlassesSelectPopup();
		DrawPropPopup();
	}

	private void DrawItemSlots(IEnumerable<EquipSlot> slots)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		GroupDisposable val = ImRaii.Group();
		try
		{
			foreach (EquipSlot slot in slots)
			{
				DrawItemSlot(slot);
			}
		}
		finally
		{
			((GroupDisposable)(ref val)).Dispose();
		}
	}

	private void DrawItemSlot(EquipSlot slot)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00be: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_012a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0145: Unknown result type (might be due to invalid IL or missing references)
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_024d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		UpdateSlot(slot);
		if (!Equipped.TryGetValue(slot, out ItemInfo value))
		{
			return;
		}
		float cursorPosX = ImGui.GetCursorPosX();
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		DrawItemButton(value);
		ImGui.SameLine(0f, x);
		GroupDisposable val = ImRaii.Group();
		try
		{
			PrepareItemLabel(value.Item, value.ModelId, cursorPosX, x);
			ImU8String val4;
			ImU8String val6;
			if (value is WeaponInfo weaponInfo)
			{
				int[] array = new int[3]
				{
					weaponInfo.Model.Id,
					weaponInfo.Model.Type,
					weaponInfo.Model.Variant
				};
				ImU8String val2 = new ImU8String(7, 1);
				((ImU8String)(ref val2)).AppendLiteral("##Input");
				((ImU8String)(ref val2)).AppendFormatted<EquipSlot>(slot);
				ImU8String val3 = val2;
				Span<int> span = array;
				val4 = default(ImU8String);
				if (ImGui.InputInt(val3, span, 0, 0, val4, (ImGuiInputTextFlags)0))
				{
					weaponInfo.SetModel((ushort)array[0], (ushort)array[1], (byte)array[2]);
				}
			}
			else if (value is EquipInfo equipInfo)
			{
				int[] array2 = new int[2]
				{
					equipInfo.Model.Id,
					equipInfo.Model.Variant
				};
				val4 = new ImU8String(7, 1);
				((ImU8String)(ref val4)).AppendLiteral("##Input");
				((ImU8String)(ref val4)).AppendFormatted<EquipSlot>(slot);
				ImU8String val5 = val4;
				Span<int> span2 = array2;
				val6 = default(ImU8String);
				if (ImGui.InputInt(val5, span2, 0, 0, val6, (ImGuiInputTextFlags)0))
				{
					equipInfo.SetModel((ushort)array2[0], (byte)array2[1]);
				}
			}
			ImGui.SameLine(0f, x);
			DrawDyeButton(value, 0);
			ImGui.SameLine(0f, x);
			DrawDyeButton(value, 1);
			if (value.IsHideable)
			{
				val6 = new ImU8String(13, 1);
				((ImU8String)(ref val6)).AppendLiteral("EqSetVisible_");
				((ImU8String)(ref val6)).AppendFormatted<EquipSlot>(slot);
				IdDisposable val7 = ImRaii.PushId(val6, true);
				try
				{
					ColorDisposable val8 = ImRaii.PushColor((ImGuiCol)21, 0u, true);
					try
					{
						ImGui.SameLine(0f, x);
						bool isVisible = value.IsVisible;
						if (Buttons.IconButtonTooltip((FontAwesomeIcon)(isVisible ? 61550 : 61552), "Toggle item visibility"))
						{
							value.SetVisible(!isVisible);
						}
					}
					finally
					{
						((IDisposable)val8)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val7)?.Dispose();
				}
			}
			if (value.IsVisor)
			{
				ImU8String val9 = new ImU8String(12, 1);
				((ImU8String)(ref val9)).AppendLiteral("EqSetToggle_");
				((ImU8String)(ref val9)).AppendFormatted<EquipSlot>(slot);
				IdDisposable val10 = ImRaii.PushId(val9, true);
				try
				{
					ColorDisposable val11 = ImRaii.PushColor((ImGuiCol)21, 0u, true);
					try
					{
						ImGui.SameLine(0f, x);
						bool isVisorToggled = value.IsVisorToggled;
						ColorDisposable val12 = ImRaii.PushColor((ImGuiCol)0, ImGui.GetColorU32((ImGuiCol)1), isVisorToggled);
						try
						{
							if (Buttons.IconButtonTooltip((FontAwesomeIcon)63226, "Toggle visor"))
							{
								value.SetVisorToggled(!isVisorToggled);
							}
						}
						finally
						{
							((IDisposable)val12)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val11)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val10)?.Dispose();
				}
			}
			if (slot != EquipSlot.MainHand)
			{
				return;
			}
			ColorDisposable val13 = ImRaii.PushColor((ImGuiCol)21, 0u, true);
			try
			{
				ImGui.SameLine(0f, x);
				if (Buttons.IconButtonTooltip((FontAwesomeIcon)58058, "Search equippable props...\nNote: Weapon may need to be unsheathed!"))
				{
					OpenPropPopup();
				}
			}
			finally
			{
				((IDisposable)val13)?.Dispose();
			}
		}
		finally
		{
			((GroupDisposable)(ref val)).Dispose();
		}
	}

	private static void PrepareItemLabel(ItemSheet? item, ushort modelId, float cursorStart, float innerSpace)
	{
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		float num = ImGui.CalcItemWidth() - (ImGui.GetCursorPosX() - cursorStart);
		ImGui.SetNextItemWidth(num);
		ImGui.Text(ImU8String.op_Implicit((item?.Name ?? ((modelId == 0) ? "Empty" : "Unknown")).FitToWidth(num)));
		ImGui.SetNextItemWidth(CalcItemWidth(cursorStart));
	}

	private void DrawItemButton(ItemInfo info)
	{
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		ColorDisposable val = ImRaii.PushColor((ImGuiCol)21, 0u, true);
		try
		{
			ImU8String val2 = new ImU8String(13, 1);
			((ImU8String)(ref val2)).AppendLiteral("##ItemButton_");
			((ImU8String)(ref val2)).AppendFormatted<EquipSlot>(info.Slot);
			IdDisposable val3 = ImRaii.PushId(val2, true);
			bool flag;
			try
			{
				flag = ((info.Texture == null) ? ImGui.Button(ImU8String.op_Implicit(info.Slot.ToString()), ButtonSize) : ImGui.ImageButton(info.Texture.GetWrapOrEmpty().Handle, ButtonSize));
			}
			finally
			{
				((IDisposable)val3)?.Dispose();
			}
			if (flag)
			{
				OpenItemSelectPopup(info.Slot);
			}
			if (ImGui.IsItemClicked((ImGuiMouseButton)1))
			{
				info.Unequip();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void OpenItemSelectPopup(EquipSlot slot)
	{
		ItemSelectSlot = slot;
		ItemSelectList.Clear();
		lock (Items)
		{
			ItemSelectList = Items.Where((ItemSheet item) => item.IsEquippable(slot)).ToList();
		}
		_itemSelectPopup.Open();
	}

	private void DrawItemSelectPopup()
	{
		if (!_itemSelectPopup.IsOpen || !_itemSelectPopup.Draw(ItemSelectList, out var selected))
		{
			return;
		}
		lock (Equipped)
		{
			if (Equipped.TryGetValue(ItemSelectSlot, out ItemInfo value))
			{
				value.SetEquipItem(selected);
			}
		}
	}

	private static bool ItemSelectDrawRow(ItemSheet item, bool isFocus)
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		return ImGui.Selectable(ImU8String.op_Implicit(item.Name), isFocus, (ImGuiSelectableFlags)0, default(Vector2));
	}

	private static bool ItemSelectSearchPredicate(ItemSheet item, string query)
	{
		return item.Name.Contains(query, StringComparison.OrdinalIgnoreCase);
	}

	private static uint CalcStainColor(Stain? stain)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		uint num = 4278190080u;
		if (stain.HasValue)
		{
			uint num2 = num;
			Stain value = stain.Value;
			num = num2 | (((Stain)(ref value)).Color << 8).FlipEndian();
		}
		return num;
	}

	private void DrawDyeButton(ItemInfo info, int index)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		Stain? stain = null;
		if (!_fetchData.IsCompleted)
		{
			return;
		}
		foreach (Stain stain2 in Stains)
		{
			Stain current = stain2;
			if (((Stain)(ref current)).RowId == info.StainIds[index])
			{
				lock (Stains)
				{
					stain = current;
				}
			}
		}
		uint num = CalcStainColor(stain);
		Vector4 colorVec = ImGui.ColorConvertU32ToFloat4(num);
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(13, 2);
		((ImU8String)(ref val)).AppendLiteral("##DyeSelect_");
		((ImU8String)(ref val)).AppendFormatted<EquipSlot>(info.Slot);
		((ImU8String)(ref val)).AppendLiteral("_");
		((ImU8String)(ref val)).AppendFormatted<int>(index);
		if (ImGui.ColorButton(val, ref colorVec, (ImGuiColorEditFlags)64, default(Vector2)))
		{
			OpenDyeSelectPopup(info.Slot, index);
		}
		if (ImGui.IsItemClicked((ImGuiMouseButton)1))
		{
			info.SetStainId(0, index);
		}
		if (ImGui.IsItemHovered())
		{
			DrawDyeTooltip(stain, num, colorVec);
		}
	}

	private static void DrawDyeTooltip(Stain? stain, uint color, Vector4 colorVec4)
	{
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		ColorDisposable val = ImRaii.PushColor((ImGuiCol)0, color, (colorVec4.X + colorVec4.Y + colorVec4.Z) / 3f > 0.1f);
		try
		{
			TooltipDisposable val2 = ImRaii.Tooltip();
			try
			{
				object obj;
				Stain valueOrDefault;
				if (!stain.HasValue)
				{
					obj = null;
				}
				else
				{
					valueOrDefault = stain.GetValueOrDefault();
					ReadOnlySeString name = ((Stain)(ref valueOrDefault)).Name;
					obj = ((ReadOnlySeString)(ref name)).ExtractText();
				}
				string text = (string)obj;
				ImGui.Text(ImU8String.op_Implicit((!StringExtensions.IsNullOrEmpty(text)) ? text : "No dye set."));
				int num;
				if (!stain.HasValue)
				{
					num = 0;
				}
				else
				{
					valueOrDefault = stain.GetValueOrDefault();
					num = (int)((Stain)(ref valueOrDefault)).Color;
				}
				uint num2 = (uint)num;
				if (num2 != 0)
				{
					ImGuiStylePtr style = ImGui.GetStyle();
					ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
					ImU8String val3 = new ImU8String(3, 1);
					((ImU8String)(ref val3)).AppendLiteral("(#");
					((ImU8String)(ref val3)).AppendFormatted<uint>(num2, "X6");
					((ImU8String)(ref val3)).AppendLiteral(")");
					ImGui.TextDisabled(val3);
				}
			}
			finally
			{
				((TooltipDisposable)(ref val2)).Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void OpenDyeSelectPopup(EquipSlot slot, int index)
	{
		DyeSelectSlot = slot;
		DyeSelectIndex = index;
		_dyeSelectPopup.Open();
	}

	private void DrawDyeSelectPopup()
	{
		if (!_dyeSelectPopup.IsOpen)
		{
			return;
		}
		lock (Stains)
		{
			if (_dyeSelectPopup.Draw(Stains, out var selected) && Equipped.TryGetValue(DyeSelectSlot, out ItemInfo value))
			{
				value.SetStainId((byte)((Stain)(ref selected)).RowId, DyeSelectIndex);
			}
		}
	}

	private static bool DyeSelectDrawRow(Stain stain, bool isFocus)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		uint num = CalcStainColor(stain);
		ImGuiStylePtr style = ImGui.GetStyle();
		float num2 = ((ImGuiStylePtr)(ref style)).ItemSpacing.Y / 2f;
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		Vector2 cursorScreenPos = ImGui.GetCursorScreenPos();
		cursorScreenPos.X -= ((ImGuiStylePtr)(ref style)).WindowPadding.X + num2;
		Vector2 vector = cursorScreenPos;
		Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
		contentRegionAvail.Y = UiBuilder.DefaultFontSizePx + ((ImGuiStylePtr)(ref style)).FramePadding.Y + num2;
		Vector2 vector2 = vector + contentRegionAvail;
		((ImDrawListPtr)(ref windowDrawList)).AddRectFilled(cursorScreenPos, vector2, num);
		ColorDisposable val = ImRaii.PushColor((ImGuiCol)0, GuiHelpers.CalcBlackWhiteTextColor(num), true);
		try
		{
			ColorDisposable val2 = ImRaii.PushColor((ImGuiCol)26, num, true);
			try
			{
				ColorDisposable val3 = ImRaii.PushColor((ImGuiCol)25, num, true);
				try
				{
					object obj;
					if (((Stain)(ref stain)).RowId != 0)
					{
						ReadOnlySeString name = ((Stain)(ref stain)).Name;
						obj = ((ReadOnlySeString)(ref name)).ExtractText();
					}
					else
					{
						obj = "None";
					}
					return ImGui.Selectable(ImU8String.op_Implicit((string)obj), isFocus, (ImGuiSelectableFlags)0, default(Vector2));
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
	}

	private static bool DyeSelectSearchPredicate(Stain stain, string query)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		ReadOnlySeString name = ((Stain)(ref stain)).Name;
		return ((ReadOnlySeString)(ref name)).ExtractText().Contains(query, StringComparison.OrdinalIgnoreCase);
	}

	private void DrawGlassesSelect(int index = 0)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0122: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		ushort glassesId = Editor.GetGlassesId(index);
		Glasses? glasses;
		lock (Glasses)
		{
			glasses = Glasses.FirstOrDefault((Glasses x) => x.RowId == glassesId);
		}
		float cursorPosX = ImGui.GetCursorPosX();
		DrawGlassesButton(index, glasses);
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SameLine(0f, ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X);
		GroupDisposable val = ImRaii.Group();
		try
		{
			ImGui.Text(ImU8String.op_Implicit(((glasses?.RowId ?? 1) != 0) ? glasses.Value.Name : "None"));
			float num = CalcItemWidth(cursorPosX);
			float frameHeight = ImGui.GetFrameHeight();
			style = ImGui.GetStyle();
			ImGui.SetNextItemWidth(num + (frameHeight + ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X) * 2f);
			int num2 = glassesId;
			ImU8String val2 = new ImU8String(10, 1);
			((ImU8String)(ref val2)).AppendLiteral("##Glasses_");
			((ImU8String)(ref val2)).AppendFormatted<int>(index);
			if (ImGui.InputInt(val2, ref num2, 0, 0, default(ImU8String), (ImGuiInputTextFlags)0))
			{
				Editor.SetGlassesId(index, (ushort)num2);
			}
		}
		finally
		{
			((GroupDisposable)(ref val)).Dispose();
		}
	}

	private void DrawGlassesButton(int index, Glasses? glasses)
	{
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		ColorDisposable val = ImRaii.PushColor((ImGuiCol)21, 0u, true);
		try
		{
			uint num = (((glasses?.Icon ?? 0) != 0) ? glasses.Value.Icon : GetFallbackIcon(EquipSlot.Glasses));
			ITextureProvider tex = _tex;
			GameIconLookup val2 = GameIconLookup.op_Implicit(num);
			if (ImGui.ImageButton(tex.GetFromGameIcon(ref val2).GetWrapOrEmpty().Handle, ButtonSize))
			{
				OpenGlassesSelectPopup(index);
			}
			if (ImGui.IsItemClicked((ImGuiMouseButton)1))
			{
				Editor.SetGlassesId(index, 0);
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static bool GlassesSelectSearchPredicate(Glasses glasses, string query)
	{
		return glasses.Name.Contains(query, StringComparison.OrdinalIgnoreCase);
	}

	private void OpenGlassesSelectPopup(int index)
	{
		GlassesSelectIndex = index;
		_glassesSelectPopup.Open();
	}

	private void DrawGlassesSelectPopup()
	{
		if (!_glassesSelectPopup.IsOpen)
		{
			return;
		}
		lock (Glasses)
		{
			if (_glassesSelectPopup.Draw(Glasses, out var selected))
			{
				Editor.SetGlassesId(GlassesSelectIndex, (ushort)selected.RowId);
			}
		}
	}

	private static bool GlassesSelectDrawRow(Glasses glasses, bool isFocus)
	{
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		return ImGui.Selectable(ImU8String.op_Implicit((!StringExtensions.IsNullOrEmpty(glasses.Name)) ? glasses.Name : "None"), isFocus, (ImGuiSelectableFlags)0, default(Vector2));
	}

	private void OpenPropPopup()
	{
		ItemSelectSlot = EquipSlot.MainHand;
		ItemSelectList.Clear();
		_propSelectPopup.Open();
	}

	private void DrawPropPopup()
	{
		if (!_propSelectPopup.IsOpen)
		{
			return;
		}
		lock (Props)
		{
			if (!_propSelectPopup.Draw(Props, Props.Count, out PropEntry selected, CalcItemHeight()))
			{
				return;
			}
			lock (Equipped)
			{
				if (Equipped.TryGetValue(ItemSelectSlot, out ItemInfo value) && value is WeaponInfo weaponInfo)
				{
					weaponInfo.SetModel((ushort)selected.Model, (ushort)selected.Submodel, (byte)selected.Variant);
				}
			}
		}
	}

	private static bool PropDrawRow(PropEntry prop, bool isFocus)
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		float y = CalcItemHeight();
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		float cursorPosX = ImGui.GetCursorPosX();
		string str = (StringExtensions.IsNullOrEmpty(prop.Description) ? $"{prop.Model}, {prop.Submodel}, {prop.Variant}" : prop.Description);
		bool result = ImGui.Button(ImU8String.op_Implicit(string.Empty), new Vector2(ImGui.GetContentRegionAvail().X, y));
		ImGui.SameLine(cursorPosX, x);
		ImGui.Text(ImU8String.op_Implicit(prop.Item.FitToWidth(ImGui.GetContentRegionAvail().X)));
		ColorDisposable val = ImRaii.PushColor((ImGuiCol)0, ImGui.GetColorU32((ImGuiCol)0).SetAlpha(175), true);
		try
		{
			ImGui.SameLine(cursorPosX, x);
			ImGui.SetCursorPosY(ImGui.GetCursorPosY() + ImGui.GetTextLineHeight());
			ImGui.Text(ImU8String.op_Implicit(str.FitToWidth(ImGui.GetContentRegionAvail().X)));
			return result;
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private static bool PropSearchPredicate(PropEntry prop, string query)
	{
		if (!prop.Item.Contains(query, StringComparison.OrdinalIgnoreCase))
		{
			return prop.Description.Contains(query, StringComparison.OrdinalIgnoreCase);
		}
		return true;
	}

	private void FetchData()
	{
		if (_itemsRaii)
		{
			return;
		}
		_itemsRaii = true;
		_fetchData = LoadItems();
		_fetchData.ContinueWith(delegate(Task task)
		{
			if (task.Exception != null)
			{
				Ktisis.Log.Error($"Failed to fetch items:\n{task.Exception}");
			}
		});
	}

	private async Task LoadItems()
	{
		await Task.Yield();
		IEnumerable<Stain> collection = ((IEnumerable<Stain>)_data.Excel.GetSheet<Stain>((Language?)null, (string)null)).Where(delegate(Stain stain)
		{
			//IL_000b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			if (((Stain)(ref stain)).RowId != 0)
			{
				ReadOnlySeString name = ((Stain)(ref stain)).Name;
				return !((ReadOnlySeString)(ref name)).IsEmpty;
			}
			return true;
		});
		lock (Stains)
		{
			Stains.AddRange(collection);
		}
		foreach (ItemSheet[] item in ((IEnumerable<ItemSheet>)_data.Excel.GetSheet<ItemSheet>((Language?)null, (string)null)).Where((ItemSheet item) => item.IsEquippable()).Chunk(1000))
		{
			lock (Items)
			{
				Items.AddRange(item);
			}
			lock (_equipUpdateLock)
			{
				foreach (KeyValuePair<EquipSlot, ItemInfo> item2 in Equipped.Where<KeyValuePair<EquipSlot, ItemInfo>>((KeyValuePair<EquipSlot, ItemInfo> pair) => !pair.Value.Item.HasValue))
				{
					var (slot, info) = (KeyValuePair<EquipSlot, ItemInfo>)(ref item2);
					if (item.Any((ItemSheet item) => item.IsEquippable(slot) && info.IsItemPredicate(item)))
					{
						info.FlagUpdate = true;
					}
				}
			}
		}
		IEnumerable<Glasses> collection2 = ((IEnumerable<Glasses>)_data.Excel.GetSheet<Glasses>((Language?)null, (string)null)).Where((Glasses x) => x.RowId == 0 || !StringExtensions.IsNullOrEmpty(x.Name));
		lock (Glasses)
		{
			Glasses.AddRange(collection2);
		}
		List<PropEntry> props = _propSchema.Props;
		lock (Props)
		{
			Props.AddRange(props);
		}
	}

	private void UpdateSlot(EquipSlot slot)
	{
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0067: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0035: Unknown result type (might be due to invalid IL or missing references)
		//IL_0049: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_015f: Unknown result type (might be due to invalid IL or missing references)
		if (Equipped.TryGetValue(slot, out ItemInfo value) && !value.FlagUpdate && value.IsCurrent())
		{
			return;
		}
		ItemInfo itemInfo;
		if (slot < EquipSlot.Head)
		{
			WeaponModelId weaponIndex = Editor.GetWeaponIndex((WeaponIndex)slot);
			itemInfo = new WeaponInfo(Editor)
			{
				Index = (WeaponIndex)slot,
				Model = weaponIndex
			};
		}
		else
		{
			EquipIndex index = slot.ToEquipIndex();
			EquipmentModelId equipIndex = Editor.GetEquipIndex(index);
			itemInfo = new EquipInfo(Editor)
			{
				Index = index,
				Model = equipIndex
			};
		}
		try
		{
			lock (Items)
			{
				foreach (ItemSheet item in Items)
				{
					if (item.IsEquippable(slot) && itemInfo.IsItemPredicate(item))
					{
						itemInfo.Item = item;
						break;
					}
				}
			}
			ItemInfo itemInfo2 = itemInfo;
			object texture;
			if (!itemInfo.Item.HasValue)
			{
				texture = null;
			}
			else
			{
				ITextureProvider tex = _tex;
				GameIconLookup val = GameIconLookup.op_Implicit((uint)itemInfo.Item.Value.Icon);
				texture = tex.GetFromGameIcon(ref val);
			}
			itemInfo2.Texture = (ISharedImmediateTexture?)texture;
			ItemInfo itemInfo3 = itemInfo;
			if (itemInfo3.Texture == null)
			{
				ITextureProvider tex2 = _tex;
				GameIconLookup val = GameIconLookup.op_Implicit(GetFallbackIcon(slot));
				ISharedImmediateTexture val2 = (itemInfo3.Texture = tex2.GetFromGameIcon(ref val));
			}
		}
		finally
		{
			Equipped[slot] = itemInfo;
		}
	}

	private void InvalidateCache()
	{
		lock (Equipped)
		{
			Equipped.Clear();
		}
	}

	private static float CalcItemWidth(float cursorStart)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		float x = ((ImGuiStylePtr)(ref style)).ItemInnerSpacing.X;
		return Math.Min(UiBuilder.DefaultFontSizePx * 4f * 2f + x, ImGui.CalcItemWidth() - (ImGui.GetCursorPosX() - cursorStart) - x - ImGui.GetFrameHeight());
	}

	private static uint GetFallbackIcon(EquipSlot slot)
	{
		switch (slot)
		{
		case EquipSlot.MainHand:
			return 60102u;
		case EquipSlot.OffHand:
			return 60110u;
		case EquipSlot.Head:
			return 60124u;
		case EquipSlot.Chest:
			return 60125u;
		case EquipSlot.Hands:
			return 60129u;
		case EquipSlot.Legs:
			return 60127u;
		case EquipSlot.Feet:
			return 60130u;
		case EquipSlot.Necklace:
			return 60132u;
		case EquipSlot.Earring:
			return 60133u;
		case EquipSlot.Bracelet:
			return 60134u;
		case EquipSlot.RingLeft:
		case EquipSlot.RingRight:
			return 60135u;
		case EquipSlot.Glasses:
			return 60189u;
		default:
			return 0u;
		}
	}
}
