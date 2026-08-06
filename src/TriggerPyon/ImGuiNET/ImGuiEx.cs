using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Dalamud.Bindings.ImGui;
using Dalamud.Game;
using Dalamud.Interface;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Interface.Utility;
using Hypostasis;
using Hypostasis.Dalamud;
using Lumina.Excel;

namespace ImGuiNET;

public static class ImGuiEx
{
	public sealed class IDBlock : IDisposable
	{
		private static readonly IDBlock instance = new IDBlock();

		private IDBlock()
		{
		}

		public static IDBlock Begin(int id)
		{
			ImGui.PushID((IntPtr)id);
			return instance;
		}

		public static IDBlock Begin(uint id)
		{
			return Begin((int)id);
		}

		public static IDBlock Begin(nint id)
		{
			ImGui.PushID((IntPtr)id);
			return instance;
		}

		public static IDBlock Begin(nuint id)
		{
			return Begin((nint)id);
		}

		public static IDBlock Begin(string id)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			ImGui.PushID(ImU8String.op_Implicit(id));
			return instance;
		}

		public unsafe static IDBlock Begin(void* ptr)
		{
			ImGuiNative.PushID(ptr);
			return instance;
		}

		public void Dispose()
		{
			ImGui.PopID();
		}
	}

	public sealed class StyleVarBlock : IDisposable
	{
		private static readonly StyleVarBlock instance = new StyleVarBlock();

		private StyleVarBlock()
		{
		}

		public static StyleVarBlock Begin(ImGuiStyleVar idx, float val, bool conditional = true)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			if (!conditional)
			{
				return null;
			}
			ImGui.PushStyleVar(idx, val);
			return instance;
		}

		public static StyleVarBlock Begin(ImGuiStyleVar idx, Vector2 val, bool conditional = true)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			if (!conditional)
			{
				return null;
			}
			ImGui.PushStyleVar(idx, val);
			return instance;
		}

		public void Dispose()
		{
			ImGui.PopStyleVar();
		}
	}

	public sealed class StyleColorBlock : IDisposable
	{
		private static readonly StyleColorBlock instance = new StyleColorBlock();

		private StyleColorBlock()
		{
		}

		public static StyleColorBlock Begin(ImGuiCol idx, uint val, bool conditional = true)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			if (!conditional)
			{
				return null;
			}
			ImGui.PushStyleColor(idx, val);
			return instance;
		}

		public static StyleColorBlock Begin(ImGuiCol idx, Vector4 val, bool conditional = true)
		{
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			if (!conditional)
			{
				return null;
			}
			ImGui.PushStyleColor(idx, val);
			return instance;
		}

		public void Dispose()
		{
			ImGui.PopStyleColor();
		}
	}

	public sealed class IndentBlock : IDisposable
	{
		private static readonly IndentBlock instance = new IndentBlock();

		private IndentBlock()
		{
		}

		public static IndentBlock Begin()
		{
			PushIndent();
			return instance;
		}

		public static IndentBlock Begin(float indent)
		{
			if (indent == 0f)
			{
				return null;
			}
			PushIndent(indent);
			return instance;
		}

		public void Dispose()
		{
			PopIndent();
		}
	}

	public sealed class FontBlock : IDisposable
	{
		private static readonly FontBlock instance = new FontBlock();

		private FontBlock()
		{
		}

		public static FontBlock Begin(ImFontPtr font)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			ImGui.PushFont(font);
			return instance;
		}

		public void Dispose()
		{
			ImGui.PopFont();
		}
	}

	public sealed class GroupBlock : IDisposable
	{
		private static readonly GroupBlock instance = new GroupBlock();

		private GroupBlock()
		{
		}

		public static GroupBlock Begin()
		{
			ImGui.BeginGroup();
			return instance;
		}

		public void Dispose()
		{
			ImGui.EndGroup();
		}
	}

	public sealed class ClipRectBlock : IDisposable
	{
		private static readonly ClipRectBlock instance = new ClipRectBlock();

		private ClipRectBlock()
		{
		}

		public static ClipRectBlock Begin(Vector2 min, Vector2 max, bool overlap = true)
		{
			ImGui.PushClipRect(min, max, overlap);
			return instance;
		}

		public void Dispose()
		{
			ImGui.PopClipRect();
		}
	}

	public sealed class TooltipBlock : IDisposable
	{
		private static readonly TooltipBlock instance = new TooltipBlock();

		private TooltipBlock()
		{
		}

		public static TooltipBlock Begin()
		{
			ImGui.BeginTooltip();
			return instance;
		}

		public void Dispose()
		{
			ImGui.EndTooltip();
		}
	}

	public sealed class DisabledBlock : IDisposable
	{
		private static readonly DisabledBlock instance = new DisabledBlock();

		private DisabledBlock()
		{
		}

		public static DisabledBlock Begin(bool conditional = true)
		{
			ImGui.BeginDisabled(conditional);
			return instance;
		}

		public void Dispose()
		{
			ImGui.EndDisabled();
		}
	}

	public sealed class AllowKeyboardFocusBlock : IDisposable
	{
		private static readonly AllowKeyboardFocusBlock instance = new AllowKeyboardFocusBlock();

		private AllowKeyboardFocusBlock()
		{
		}

		public static AllowKeyboardFocusBlock Begin(bool allow = false)
		{
			ImGui.PushAllowKeyboardFocus(allow);
			return instance;
		}

		public void Dispose()
		{
			ImGui.PopAllowKeyboardFocus();
		}
	}

	public sealed class ButtonRepeatBlock : IDisposable
	{
		private static readonly ButtonRepeatBlock instance = new ButtonRepeatBlock();

		private ButtonRepeatBlock()
		{
		}

		public static ButtonRepeatBlock Begin(bool repeat = true)
		{
			ImGui.PushButtonRepeat(repeat);
			return instance;
		}

		public void Dispose()
		{
			ImGui.PopButtonRepeat();
		}
	}

	public sealed class ItemWidthBlock : IDisposable
	{
		private static readonly ItemWidthBlock instance = new ItemWidthBlock();

		private ItemWidthBlock()
		{
		}

		public static ItemWidthBlock Begin(float width)
		{
			ImGui.PushItemWidth(width);
			return instance;
		}

		public void Dispose()
		{
			ImGui.PopItemWidth();
		}
	}

	public sealed class TextWrapPosBlock : IDisposable
	{
		private static readonly TextWrapPosBlock instance = new TextWrapPosBlock();

		private TextWrapPosBlock()
		{
		}

		public static TextWrapPosBlock Begin()
		{
			ImGui.PushTextWrapPos();
			return instance;
		}

		public static TextWrapPosBlock Begin(float posX)
		{
			ImGui.PushTextWrapPos(posX);
			return instance;
		}

		public void Dispose()
		{
			ImGui.PopTextWrapPos();
		}
	}

	public record ExcelSheetOptions<T> where T : struct, IExcelRow<T>
	{
		public Func<T, string> FormatRow { get; init; }

		public Func<T, string, bool> SearchPredicate { get; init; }

		public Func<T, bool, bool> DrawSelectable { get; init; }

		public IEnumerable<T> FilteredSheet { get; init; }

		public Vector2? Size { get; init; }
	}

	public record ExcelSheetComboOptions<T> : ExcelSheetOptions<T> where T : struct, IExcelRow<T>
	{
		public Func<T, string> GetPreview { get; init; }

		public ImGuiComboFlags ComboFlags { get; init; }
	}

	public record ExcelSheetPopupOptions<T> : ExcelSheetOptions<T> where T : struct, IExcelRow<T>
	{
		public ImGuiPopupFlags PopupFlags { get; init; }

		public bool CloseOnSelection { get; init; }

		public Func<T, bool> IsRowSelected { get; init; }
	}

	public record GroupBoxOptions
	{
		public bool Collapsible { get; init; }

		public uint HeaderTextColor { get; init; } = ImGui.GetColorU32((ImGuiCol)0);

		public Action HeaderTextAction { get; init; }

		public uint BorderColor { get; init; } = ImGui.GetColorU32((ImGuiCol)5);

		public Vector2 BorderPadding { get; init; }

		public float BorderRounding { get; init; }

		public ImDrawFlags DrawFlags { get; init; }

		public float BorderThickness { get; init; }

		public float Width { get; set; }

		public float MaxX { get; set; }

		public GroupBoxOptions()
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			ImGuiStylePtr style = ImGui.GetStyle();
			BorderPadding = ((ImGuiStylePtr)(ref style)).WindowPadding;
			style = ImGui.GetStyle();
			BorderRounding = ((ImGuiStylePtr)(ref style)).FrameRounding;
			BorderThickness = 2f;
			base._002Ector();
		}
	}

	public record HeaderIconOptions
	{
		public Vector2 Offset { get; init; } = Vector2.Zero;

		public ImGuiMouseButton MouseButton { get; init; }

		public string Tooltip { get; init; } = string.Empty;

		public uint Color { get; init; } = uint.MaxValue;

		public bool ToastTooltipOnClick { get; init; }

		public ImGuiMouseButton ToastTooltipOnClickButton { get; init; }
	}

	public class ListClipper : IEnumerable<(int, int)>, IEnumerable, IDisposable
	{
		private ImGuiListClipperPtr clipper;

		private readonly int rows;

		private readonly int columns;

		private readonly bool twoDimensional;

		private readonly int itemRemainder;

		public int FirstRow { get; private set; } = -1;

		public int LastRow => CurrentRow;

		public int CurrentRow { get; private set; }

		public bool IsStepped => CurrentRow == DisplayStart;

		public int DisplayStart => ((ImGuiListClipperPtr)(ref clipper)).DisplayStart;

		public int DisplayEnd => ((ImGuiListClipperPtr)(ref clipper)).DisplayEnd;

		public IEnumerable<int> Rows
		{
			get
			{
				while (((ImGuiListClipperPtr)(ref clipper)).Step())
				{
					if (((ImGuiListClipperPtr)(ref clipper)).ItemsHeight > 0f && FirstRow < 0)
					{
						FirstRow = (int)(ImGui.GetScrollY() / ((ImGuiListClipperPtr)(ref clipper)).ItemsHeight);
					}
					for (int i = ((ImGuiListClipperPtr)(ref clipper)).DisplayStart; i < ((ImGuiListClipperPtr)(ref clipper)).DisplayEnd; i++)
					{
						CurrentRow = i;
						yield return twoDimensional ? i : (i * columns);
					}
				}
			}
		}

		public IEnumerable<int> Columns
		{
			get
			{
				int cols = ((itemRemainder == 0 || rows != DisplayEnd || CurrentRow != DisplayEnd - 1) ? columns : itemRemainder);
				for (int j = 0; j < cols; j++)
				{
					yield return j;
				}
			}
		}

		public unsafe ListClipper(int items, int cols = 1, bool twoD = false, float itemHeight = 0f)
		{
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			twoDimensional = twoD;
			columns = cols;
			rows = (twoDimensional ? items : ((int)MathF.Ceiling((float)items / (float)columns)));
			itemRemainder = ((!twoDimensional) ? (items % columns) : 0);
			clipper = new ImGuiListClipperPtr(ImGuiNative.ImGuiListClipper());
			((ImGuiListClipperPtr)(ref clipper)).Begin(rows, itemHeight);
		}

		public IEnumerator<(int, int)> GetEnumerator()
		{
			return (from i in Rows
				from j in Columns
				select (i: i, j: j)).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Dispose()
		{
			((ImGuiListClipperPtr)(ref clipper)).Destroy();
			GC.SuppressFinalize(this);
		}
	}

	[CompilerGenerated]
	private sealed class _003C_003Ec__DisplayClass29_0<T> where T : struct, IExcelRow<T>
	{
		public PropertyInfo[] properties;

		public ImGuiTableSortSpecsPtr sortSpecs;

		internal bool _003CExcelSheetTable_003Eb__3(T row)
		{
			if (!(((IExcelRow<T>)row).RowId.ToString() == tableSearchText))
			{
				return _003CExcelSheetTable_003Eg__GetPropertiesAsStrings_007C29_1<T>(properties, row).Any((string valueStr) => valueStr.Contains(tableSearchText, StringComparison.CurrentCultureIgnoreCase));
			}
			return true;
		}

		internal IComparable _003CExcelSheetTable_003Eb__4(T row)
		{
			return _003CExcelSheetTable_003Eg__GetComparable_007C29_2<T>(properties[((ImGuiTableColumnSortSpecsPtr)(ref ((ImGuiTableSortSpecsPtr)(ref sortSpecs)).Specs)).ColumnIndex - 1].GetValue(row));
		}

		internal IComparable _003CExcelSheetTable_003Eb__5(T row)
		{
			return _003CExcelSheetTable_003Eg__GetComparable_007C29_2<T>(properties[((ImGuiTableColumnSortSpecsPtr)(ref ((ImGuiTableSortSpecsPtr)(ref sortSpecs)).Specs)).ColumnIndex - 1].GetValue(row));
		}
	}

	private static string sheetSearchText;

	private static uint[] filteredSearchIDs;

	private static string prevSearchID;

	private static Type prevSearchType;

	private static string tableSearchText = string.Empty;

	private static uint[] filteredTableSearchSheetIDs;

	private static string prevTableSearchID;

	private static Type prevTableSearchType;

	private static bool tableCompatMode = false;

	private static readonly Stack<GroupBoxOptions> groupBoxOptionsStack = new Stack<GroupBoxOptions>();

	private static uint headerLastWindowID = 0u;

	private static ulong headerLastFrame = 0uL;

	private static uint headerCurrentPos = 0u;

	private static float headerImGuiButtonWidth = 0f;

	private static readonly Stack<float> fontScaleStack = new Stack<float>();

	private static float curScale = 1f;

	private static readonly Stack<float> indentStack = new Stack<float>();

	private static object dragID = null;

	private static bool isDraggingItem = false;

	private static bool initialPosition = false;

	private static Vector2 lastGridPosition = Vector2.Zero;

	private static Vector2 gridCenter = Vector2.Zero;

	private static void ExcelSheetSearchInput<T>(string id, IEnumerable<T> filteredSheet, Func<T, string, bool> searchPredicate) where T : struct, IExcelRow<T>
	{
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007f: Unknown result type (might be due to invalid IL or missing references)
		if (ImGui.IsWindowAppearing() && ImGui.IsWindowFocused() && !ImGui.IsAnyItemActive())
		{
			if (id != prevSearchID)
			{
				if (typeof(T) != prevSearchType)
				{
					sheetSearchText = string.Empty;
					prevSearchType = typeof(T);
				}
				filteredSearchIDs = null;
				prevSearchID = id;
			}
			ImGui.SetKeyboardFocusHere(0);
		}
		if (ImGui.InputTextWithHint(ImU8String.op_Implicit("##ExcelSheetSearch"), ImU8String.op_Implicit("Search"), ref sheetSearchText, 128, (ImGuiInputTextFlags)16, (ImGuiInputTextCallbackDelegate)null))
		{
			filteredSearchIDs = null;
		}
		if (filteredSearchIDs == null)
		{
			filteredSearchIDs = (from r in filteredSheet
				where searchPredicate(r, sheetSearchText)
				select ((IExcelRow<T>)r).RowId).ToArray();
		}
	}

	public static bool ExcelSheetCombo<T>(string id, ref uint selectedRow, ExcelSheetComboOptions<T> options = null) where T : struct, IExcelRow<T>
	{
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
		if ((object)options == null)
		{
			options = new ExcelSheetComboOptions<T>();
		}
		ExcelSheet<T> excelSheet = DalamudApi.DataManager.GetExcelSheet<T>((ClientLanguage?)null, (string)null);
		Func<T, string> func = options.GetPreview ?? options.FormatRow;
		ImU8String val = ImU8String.op_Implicit(id);
		T? rowOrDefault = excelSheet.GetRowOrDefault(selectedRow);
		string text;
		if (rowOrDefault.HasValue)
		{
			T valueOrDefault = rowOrDefault.GetValueOrDefault();
			text = func(valueOrDefault);
		}
		else
		{
			text = selectedRow.ToString();
		}
		if (!ImGui.BeginCombo(val, ImU8String.op_Implicit(text), (ImGuiComboFlags)(options.ComboFlags | 0x10)))
		{
			return false;
		}
		ExcelSheetSearchInput(id, (IEnumerable<T>)(((object)options.FilteredSheet) ?? ((object)excelSheet)), options.SearchPredicate ?? ((Func<T, string, bool>)((T row, string s) => options.FormatRow(row).Contains(s, StringComparison.CurrentCultureIgnoreCase))));
		ImGui.BeginChild(ImU8String.op_Implicit("ExcelSheetSearchList"), options.Size ?? new Vector2(0f, 200f * ImGuiHelpers.GlobalScale), true, (ImGuiWindowFlags)0);
		bool flag = false;
		Func<T, bool, bool> func2 = options.DrawSelectable ?? ((Func<T, bool, bool>)((T row, bool selected) => ImGui.Selectable(ImU8String.op_Implicit(options.FormatRow(row)), selected, (ImGuiSelectableFlags)0, default(Vector2))));
		using (ListClipper listClipper = new ListClipper(filteredSearchIDs.Length))
		{
			foreach (int row in listClipper.Rows)
			{
				rowOrDefault = excelSheet.GetRowOrDefault(filteredSearchIDs[row]);
				if (!rowOrDefault.HasValue)
				{
					continue;
				}
				T valueOrDefault2 = rowOrDefault.GetValueOrDefault();
				using (IDBlock.Begin(row))
				{
					if (!func2(valueOrDefault2, selectedRow == ((IExcelRow<T>)valueOrDefault2).RowId))
					{
						continue;
					}
					selectedRow = ((IExcelRow<T>)valueOrDefault2).RowId;
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			ImGui.CloseCurrentPopup();
		}
		ImGui.EndChild();
		ImGui.EndCombo();
		return flag;
	}

	public static bool ExcelSheetPopup<T>(string id, out uint selectedRow, ExcelSheetPopupOptions<T> options = null) where T : struct, IExcelRow<T>
	{
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		if ((object)options == null)
		{
			options = new ExcelSheetPopupOptions<T>();
		}
		ExcelSheet<T> excelSheet = DalamudApi.DataManager.GetExcelSheet<T>((ClientLanguage?)null, (string)null);
		selectedRow = 0u;
		ImGui.SetNextWindowSize(options.Size ?? new Vector2(0f, 250f * ImGuiHelpers.GlobalScale));
		if (!ImGui.BeginPopupContextItem(ImU8String.op_Implicit(id), options.PopupFlags))
		{
			return false;
		}
		ExcelSheetSearchInput(id, (IEnumerable<T>)(((object)options.FilteredSheet) ?? ((object)excelSheet)), options.SearchPredicate ?? ((Func<T, string, bool>)((T row, string s) => options.FormatRow(row).Contains(s, StringComparison.CurrentCultureIgnoreCase))));
		ImGui.BeginChild(ImU8String.op_Implicit("ExcelSheetSearchList"), Vector2.Zero, true, (ImGuiWindowFlags)0);
		bool flag = false;
		Func<T, bool, bool> func = options.DrawSelectable ?? ((Func<T, bool, bool>)((T row, bool selected) => ImGui.Selectable(ImU8String.op_Implicit(options.FormatRow(row)), selected, (ImGuiSelectableFlags)0, default(Vector2))));
		using (ListClipper listClipper = new ListClipper(filteredSearchIDs.Length))
		{
			foreach (int row in listClipper.Rows)
			{
				T? rowOrDefault = excelSheet.GetRowOrDefault(filteredSearchIDs[row]);
				if (!rowOrDefault.HasValue)
				{
					continue;
				}
				T valueOrDefault = rowOrDefault.GetValueOrDefault();
				using (IDBlock.Begin(row))
				{
					if (func(valueOrDefault, options.IsRowSelected(valueOrDefault)))
					{
						selectedRow = ((IExcelRow<T>)valueOrDefault).RowId;
						flag = true;
					}
				}
			}
		}
		if (flag && options.CloseOnSelection)
		{
			ImGui.CloseCurrentPopup();
		}
		ImGui.EndChild();
		ImGui.EndPopup();
		return flag;
	}

	public static bool ExcelSheetMultiselectPopup<T>(string id, ICollection<uint> selectedRows, ExcelSheetPopupOptions<T> options = null) where T : struct, IExcelRow<T>
	{
		if ((object)options == null)
		{
			options = new ExcelSheetPopupOptions<T>();
		}
		options = options with
		{
			IsRowSelected = (T row) => selectedRows.Contains(((IExcelRow<T>)row).RowId)
		};
		if (!ExcelSheetPopup(id, out var selectedRow, options))
		{
			return false;
		}
		if (!selectedRows.Remove(selectedRow))
		{
			selectedRows.Add(selectedRow);
		}
		return true;
	}

	public static void ExcelSheetTable<T>(string id) where T : struct, IExcelRow<T>
	{
		_003C_003Ec__DisplayClass29_0<T> _003C_003Ec__DisplayClass29_1 = new _003C_003Ec__DisplayClass29_0<T>();
	}

	public static bool BeginGroupBox(string id = null, float minimumWindowPercent = 1f, GroupBoxOptions options = null)
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c1: Unknown result type (might be due to invalid IL or missing references)
		if ((object)options == null)
		{
			options = new GroupBoxOptions();
		}
		ImGui.BeginGroup();
		bool flag = true;
		if (!string.IsNullOrEmpty(id))
		{
			if (!options.Collapsible)
			{
				Vector4 vector = ImGui.ColorConvertU32ToFloat4(options.HeaderTextColor);
				ImGui.TextColored(ref vector, ImU8String.op_Implicit(id));
			}
			else
			{
				ImGui.PushStyleColor((ImGuiCol)0, options.HeaderTextColor);
				flag = ImGui.TreeNodeEx(ImU8String.op_Implicit(id), (ImGuiTreeNodeFlags)40, default(ImU8String));
				ImGui.PopStyleColor();
			}
			options.HeaderTextAction?.Invoke();
			ImGui.Indent();
			ImGui.Unindent();
		}
		ImGuiStylePtr style = ImGui.GetStyle();
		float num = ((ImGuiStylePtr)(ref style)).ItemSpacing.X * (1f - minimumWindowPercent);
		GroupBoxOptions result;
		float num2 = Math.Max((groupBoxOptionsStack.TryPeek(out result) ? (result.Width - result.BorderPadding.X * 2f) : (ImGui.GetWindowContentRegionMax().X - ((ImGuiStylePtr)(ref style)).WindowPadding.X)) * minimumWindowPercent - num, 1f);
		options.Width = ((minimumWindowPercent > 0f) ? num2 : 0f);
		ImGui.BeginGroup();
		ImGui.PushStyleVar((ImGuiStyleVar)13, Vector2.Zero);
		Vector2 borderPadding = options.BorderPadding;
		borderPadding.X = num2;
		ImGui.Dummy(borderPadding);
		ImGui.PopStyleVar();
		Vector2 itemRectMax = ImGui.GetItemRectMax();
		options.MaxX = itemRectMax.X;
		if (options.Width > 0f)
		{
			Vector2 itemRectMin = ImGui.GetItemRectMin();
			borderPadding = itemRectMax;
			borderPadding.Y = 10000f;
			ImGui.PushClipRect(itemRectMin, borderPadding, true);
		}
		ImGui.Indent(Math.Max(options.BorderPadding.X, 0.01f));
		ImGui.PushItemWidth(MathF.Floor((num2 - options.BorderPadding.X * 2f) * 0.65f));
		groupBoxOptionsStack.Push(options);
		if (flag)
		{
			return true;
		}
		ImGui.TextDisabled(ImU8String.op_Implicit(". . ."));
		EndGroupBox();
		return false;
	}

	public static bool BeginGroupBox(string text, GroupBoxOptions options)
	{
		return BeginGroupBox(text, 1f, options);
	}

	public static bool BeginGroupBox(uint borderColor, float minimumWindowPercent = 1f)
	{
		return BeginGroupBox(null, minimumWindowPercent, new GroupBoxOptions
		{
			BorderColor = borderColor
		});
	}

	public unsafe static void EndGroupBox()
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		GroupBoxOptions groupBoxOptions = groupBoxOptionsStack.Pop();
		bool num = groupBoxOptions.Width <= 0f;
		ImGui.PopItemWidth();
		ImGui.Unindent(Math.Max(groupBoxOptions.BorderPadding.X, 0.01f));
		if (!num)
		{
			ImGui.PopClipRect();
		}
		float cursorPosY = ImGui.GetCursorPosY();
		ImGuiStylePtr style = ImGui.GetStyle();
		ImGui.SetCursorPosY(cursorPosY - ((ImGuiStylePtr)(ref style)).ItemSpacing.Y);
		Vector2 borderPadding = groupBoxOptions.BorderPadding;
		borderPadding.X = 0f;
		ImGui.Dummy(borderPadding);
		if (!num)
		{
			ImGuiWindow* currentWindow = GetCurrentWindow();
			borderPadding = currentWindow->CursorMaxPos;
			borderPadding.X = groupBoxOptions.MaxX;
			currentWindow->CursorMaxPos = borderPadding;
		}
		ImGui.EndGroup();
		Vector2 itemRectMin = ImGui.GetItemRectMin();
		Vector2 vector;
		if (!num)
		{
			borderPadding = ImGui.GetItemRectMax();
			borderPadding.X = groupBoxOptions.MaxX;
			vector = borderPadding;
		}
		else
		{
			vector = ImGui.GetItemRectMax();
		}
		Vector2 vector2 = vector;
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		((ImDrawListPtr)(ref windowDrawList)).AddRect(itemRectMin, vector2, groupBoxOptions.BorderColor, groupBoxOptions.BorderRounding, groupBoxOptions.DrawFlags, groupBoxOptions.BorderThickness);
		ImGui.EndGroup();
	}

	public static bool AddHeaderIcon(string id, FontAwesomeIcon icon, HeaderIconOptions options = null)
	{
		//IL_0078: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0215: Unknown result type (might be due to invalid IL or missing references)
		//IL_021f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0234: Unknown result type (might be due to invalid IL or missing references)
		//IL_0266: Unknown result type (might be due to invalid IL or missing references)
		//IL_0194: Unknown result type (might be due to invalid IL or missing references)
		//IL_0199: Unknown result type (might be due to invalid IL or missing references)
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0205: Expected O, but got Unknown
		if (ImGui.IsWindowCollapsed())
		{
			return false;
		}
		float globalScale = ImGuiHelpers.GlobalScale;
		uint iD = ImGui.GetID((IntPtr)0);
		if (iD != headerLastWindowID || headerLastFrame != DalamudApi.PluginInterface.UiBuilder.FrameCount)
		{
			headerLastWindowID = iD;
			headerLastFrame = DalamudApi.PluginInterface.UiBuilder.FrameCount;
			headerCurrentPos = 0u;
			headerImGuiButtonWidth = 0f;
			if (CurrentWindowHasCloseButton())
			{
				headerImGuiButtonWidth += 17f * globalScale;
			}
			if (!((Enum)GetCurrentWindowFlags()).HasFlag((Enum)(object)(ImGuiWindowFlags)32))
			{
				headerImGuiButtonWidth += 17f * globalScale;
			}
		}
		if ((object)options == null)
		{
			options = new HeaderIconOptions();
		}
		Vector2 cursorPos = ImGui.GetCursorPos();
		Vector2 vector = new Vector2(20f * globalScale);
		float num = ImGui.GetWindowWidth() - vector.X - headerImGuiButtonWidth - (float)(20 * headerCurrentPos++) * globalScale;
		ImGuiStylePtr style = ImGui.GetStyle();
		Vector2 cursorPos2 = new Vector2(num - ((ImGuiStylePtr)(ref style)).FramePadding.X * 2f, ImGui.GetScrollY() + 1f);
		ImGui.SetCursorPos(cursorPos2);
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		((ImDrawListPtr)(ref windowDrawList)).PushClipRectFullScreen();
		bool result = false;
		ImGui.InvisibleButton(ImU8String.op_Implicit(id), vector, (ImGuiButtonFlags)0);
		Vector2 itemRectMin = ImGui.GetItemRectMin();
		Vector2 itemRectMax = ImGui.GetItemRectMax();
		Vector2 vector2 = ImGui.GetItemRectSize() / 2f;
		Vector2 vector3 = itemRectMin + vector2;
		if (ImGui.IsWindowHovered() && ImGui.IsMouseHoveringRect(itemRectMin, itemRectMax, false))
		{
			if (!string.IsNullOrEmpty(options.Tooltip))
			{
				ImGui.SetTooltip(ImU8String.op_Implicit(options.Tooltip));
			}
			ImDrawListPtr windowDrawList2 = ImGui.GetWindowDrawList();
			((ImDrawListPtr)(ref windowDrawList2)).AddCircleFilled(vector3, vector2.X, ImGui.GetColorU32((ImGuiCol)(ImGui.IsMouseDown((ImGuiMouseButton)0) ? 23 : 22)));
			if (ImGui.IsMouseReleased(options.MouseButton))
			{
				result = true;
			}
			if (options.ToastTooltipOnClick && ImGui.IsMouseReleased(options.ToastTooltipOnClickButton))
			{
				DalamudApi.NotificationManager.AddNotification(new Notification
				{
					Type = (NotificationType)4,
					Content = options.Tooltip
				});
			}
		}
		ImGui.SetCursorPos(cursorPos2);
		ImGui.PushFont(UiBuilder.IconFont);
		string text = FontAwesomeExtensions.ToIconString(icon);
		((ImDrawListPtr)(ref windowDrawList)).AddText(UiBuilder.IconFont, ImGui.GetFontSize(), itemRectMin + vector2 - ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f) / 2f + options.Offset, options.Color, ImU8String.op_Implicit(text), 0f);
		ImGui.PopFont();
		ImGui.PopClipRect();
		ImGui.SetCursorPos(cursorPos);
		return result;
	}

	public static void AddDonationHeader(string link = "https://ko-fi.com/unknownx7")
	{
		if (AddHeaderIcon("_DONATE", (FontAwesomeIcon)61444, new HeaderIconOptions
		{
			Color = 4281348304u,
			MouseButton = (ImGuiMouseButton)1,
			Tooltip = "\ue052 Right click to go to " + link,
			ToastTooltipOnClick = true
		}))
		{
			Util.StartProcess(link);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static void SetItemTooltip(string s, ImGuiHoveredFlags flags = (ImGuiHoveredFlags)0)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Unknown result type (might be due to invalid IL or missing references)
		if (ImGui.IsItemHovered(flags))
		{
			ImGui.SetTooltip(ImU8String.op_Implicit(s));
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsItemDoubleClicked(ImGuiMouseButton button = (ImGuiMouseButton)0, ImGuiHoveredFlags flags = (ImGuiHoveredFlags)0)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (ImGui.IsMouseDoubleClicked(button))
		{
			return ImGui.IsItemHovered(flags);
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool IsItemReleased(ImGuiMouseButton button = (ImGuiMouseButton)0, ImGuiHoveredFlags flags = (ImGuiHoveredFlags)0)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		if (ImGui.IsMouseReleased(button))
		{
			return ImGui.IsItemHovered(flags);
		}
		return false;
	}

	public static void PushFontScale(float scale)
	{
		ImGui.SetWindowFontScale(scale);
		fontScaleStack.Push(curScale);
		curScale = scale;
	}

	public static void PopFontScale()
	{
		curScale = fontScaleStack.Pop();
		ImGui.SetWindowFontScale(curScale);
	}

	public static void PushFontSize(float size)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		ImFontPtr font = ImGui.GetFont();
		PushFontScale(size / ((ImFontPtr)(ref font)).FontSize);
	}

	public static void PopFontSize()
	{
		PopFontScale();
	}

	public static float GetFontScale()
	{
		return curScale;
	}

	public static float GetFontSize()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		float num = curScale;
		ImFontPtr font = ImGui.GetFont();
		return num * ((ImFontPtr)(ref font)).FontSize;
	}

	public static void PushIndent(float indent = 0f)
	{
		ImGui.Indent(indent);
		indentStack.Push(indent);
	}

	public static void PopIndent()
	{
		ImGui.Unindent(indentStack.Pop());
	}

	public static void ClampWindowPosToViewport()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		ImGuiViewportPtr windowViewport = ImGui.GetWindowViewport();
		if (!ImGui.IsWindowAppearing())
		{
			uint iD = ((ImGuiViewportPtr)(ref windowViewport)).ID;
			ImGuiViewportPtr mainViewport = ImGuiHelpers.MainViewport;
			if (iD == ((ImGuiViewportPtr)(ref mainViewport)).ID)
			{
				Vector2 pos = ((ImGuiViewportPtr)(ref windowViewport)).Pos;
				ClampWindowPos(pos, pos + ((ImGuiViewportPtr)(ref windowViewport)).Size);
			}
		}
	}

	public static void ClampWindowPos(Vector2 max)
	{
		ClampWindowPos(Vector2.Zero, max);
	}

	public static void ClampWindowPos(Vector2 min, Vector2 max)
	{
		Vector2 windowPos = ImGui.GetWindowPos();
		Vector2 windowSize = ImGui.GetWindowSize();
		float x = Math.Min(Math.Max(windowPos.X, min.X), max.X - windowSize.X);
		float y = Math.Min(Math.Max(windowPos.Y, min.Y), max.Y - windowSize.Y);
		ImGui.SetWindowPos(new Vector2(x, y), (ImGuiCond)0);
	}

	public static bool IsWindowInMainViewport()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		ImGuiViewportPtr val = ImGui.GetWindowViewport();
		uint iD = ((ImGuiViewportPtr)(ref val)).ID;
		val = ImGuiHelpers.MainViewport;
		return iD == ((ImGuiViewportPtr)(ref val)).ID;
	}

	public static bool ShouldDrawInViewport()
	{
		if (!IsWindowInMainViewport())
		{
			return Util.IsWindowFocused;
		}
		return true;
	}

	public static void ShouldDrawInViewport(out bool b)
	{
		b = ShouldDrawInViewport();
	}

	public static bool SetBoolOnGameFocus(ref bool b)
	{
		if (!b)
		{
			b = Util.IsWindowFocused;
		}
		return b;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static string GetClipboardTextOrDefault(string def = "")
	{
		try
		{
			return ImGui.GetClipboardText();
		}
		catch
		{
			return def;
		}
	}

	public static void PushClipRectFullScreen()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		((ImDrawListPtr)(ref windowDrawList)).PushClipRectFullScreen();
	}

	public static void TextCopyable(string text)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		ImGui.TextUnformatted(ImU8String.op_Implicit(text));
		if (ImGui.IsItemHovered())
		{
			ImGui.SetMouseCursor((ImGuiMouseCursor)7);
			if (ImGui.IsItemClicked())
			{
				ImGui.SetClipboardText(ImU8String.op_Implicit(text));
			}
		}
	}

	public static void TextCopyable(Vector4 color, string text)
	{
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		ImGui.TextColored(ref color, ImU8String.op_Implicit(text));
		if (ImGui.IsItemHovered())
		{
			ImGui.SetMouseCursor((ImGuiMouseCursor)7);
			if (ImGui.IsItemClicked())
			{
				ImGui.SetClipboardText(ImU8String.op_Implicit(text));
			}
		}
	}

	public static void TextMarquee(string text, float speed = 0.1f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		float x = ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f).X;
		float num = ImGui.GetContentRegionMax().X + x;
		float num2 = (float)DalamudApi.PluginInterface.LoadTimeDelta.TotalSeconds * (num * speed) % num - x;
		ImGui.Indent(num2);
		ImGui.TextUnformatted(ImU8String.op_Implicit(text));
		ImGui.Unindent(num2);
	}

	public static void TextMarquee(Vector4 color, string text, float speed = 0.1f)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		float x = ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f).X;
		float num = ImGui.GetContentRegionMax().X + x;
		float num2 = (float)DalamudApi.PluginInterface.LoadTimeDelta.TotalSeconds * (num * speed) % num - x;
		ImGui.Indent(num2);
		ImGui.TextColored(ref color, ImU8String.op_Implicit(text));
		ImGui.Unindent(num2);
	}

	public static bool FontButton(string label, ImFontPtr font)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(font);
		bool result = ImGui.Button(ImU8String.op_Implicit(label), default(Vector2));
		ImGui.PopFont();
		return result;
	}

	public static bool FontButton(string label, ImFontPtr font, Vector2 size)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		ImGui.PushFont(font);
		bool result = ImGui.Button(ImU8String.op_Implicit(label), size);
		ImGui.PopFont();
		return result;
	}

	public static bool DeleteConfirmationButton(Vector2 size = default(Vector2))
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		using (FontBlock.Begin(UiBuilder.IconFont))
		{
			ImGui.Button(ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString((FontAwesomeIcon)61453)), size);
			if (IsItemReleased((ImGuiMouseButton)1, (ImGuiHoveredFlags)0))
			{
				return true;
			}
			using (StyleVarBlock.Begin((ImGuiStyleVar)9, 1f))
			{
				if (!ImGui.BeginPopupContextItem(ImU8String.op_Implicit("##confirmDelete"), (ImGuiPopupFlags)0))
				{
					return false;
				}
				bool result = ImGui.Selectable(ImU8String.op_Implicit(FontAwesomeExtensions.ToIconString((FontAwesomeIcon)62189)), false, (ImGuiSelectableFlags)0, default(Vector2));
				ImGui.EndPopup();
				return result;
			}
		}
	}

	public static void BlockWindowDrag()
	{
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		ImGuiIOPtr io = ImGui.GetIO();
		bool prev = ((ImGuiIOPtr)(ref io)).ConfigWindowsMoveFromTitleBarOnly;
		((ImGuiIOPtr)(ref io)).ConfigWindowsMoveFromTitleBarOnly = true;
		DalamudApi.Framework.RunOnTick<bool>((Func<bool>)(() => ((ImGuiIOPtr)(ref io)).ConfigWindowsMoveFromTitleBarOnly = prev), default(TimeSpan), 0, default(CancellationToken));
	}

	private static void AddTextCentered(Vector2 pos, string text, uint color)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		Vector2 vector = ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f);
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		((ImDrawListPtr)(ref windowDrawList)).AddText(pos - vector / 2f, color, ImU8String.op_Implicit(text));
	}

	public static void Prefix(string prefix = "◇")
	{
		Vector2 vector = new Vector2(ImGui.GetFrameHeight());
		ImGui.Dummy(vector);
		AddTextCentered(ImGui.GetItemRectMin() + vector / 2f, prefix, ImGui.GetColorU32((ImGuiCol)0));
		ImGui.SameLine();
	}

	public static void Prefix(bool isLast)
	{
		Prefix(isLast ? "└" : "├");
	}

	public static bool RadioBox(string label, ref int v, string[] optionsArray, bool vertical)
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		if (!BeginGroupBox(label, 0f))
		{
			return false;
		}
		bool flag = false;
		int num = optionsArray.Length;
		float num2 = 0f;
		ImGui.PushID(ImU8String.op_Implicit(label));
		for (int i = 0; i < num; i++)
		{
			string text = optionsArray[i];
			bool flag2 = v == i;
			flag |= ImGui.RadioButton<int>(ImU8String.op_Implicit(vertical ? text : $"##{i}"), ref v, i) && !flag2;
			float x = ImGui.GetItemRectSize().X;
			num2 = Math.Max(x, num2);
			if (i == num - 1)
			{
				num2 -= x;
			}
			if (!vertical)
			{
				SetItemTooltip(text, (ImGuiHoveredFlags)0);
				if (i != num - 1)
				{
					ImGui.SameLine();
				}
			}
		}
		ImGui.PopID();
		if (vertical)
		{
			ImGui.SameLine();
			ImGui.Dummy(new Vector2(num2, 0f));
		}
		else if (v >= 0 && v < num)
		{
			ImGui.SameLine();
			string text2 = optionsArray[v];
			ImGui.TextUnformatted(ImU8String.op_Implicit(text2));
			ImGui.SameLine();
			ImGui.Dummy(new Vector2(optionsArray.Select((string s) => ImGui.CalcTextSize(ImU8String.op_Implicit(s), false, -1f).X).Max() - ImGui.CalcTextSize(ImU8String.op_Implicit(text2), false, -1f).X, 0f));
		}
		EndGroupBox();
		return flag;
	}

	public static bool RadioBox(string label, ref int v, string options, bool vertical)
	{
		return RadioBox(label, ref v, options.Split('\0'), vertical);
	}

	public static bool RadioBox<T>(string label, ref T e, bool vertical) where T : struct, Enum
	{
		string[] names = Enum.GetNames<T>();
		int v = Array.IndexOf<string>(names, Enum.GetName(e));
		bool num = RadioBox(label, ref v, names.Select((string name) => typeof(T).GetField(name)?.GetCustomAttribute<DisplayAttribute>()?.Name ?? name).ToArray(), vertical);
		if (num)
		{
			e = Enum.Parse<T>(names[v]);
		}
		return num;
	}

	public static bool RadioBox<T>(string label, ref T e, T[] optionsArray, bool vertical) where T : struct, Enum
	{
		int v = Array.IndexOf(optionsArray, e);
		bool num = RadioBox(label, ref v, optionsArray.Select(Util.GetDisplayName).ToArray(), vertical);
		if (num)
		{
			e = optionsArray[v];
		}
		return num;
	}

	public static bool EnumCombo<T>(string label, ref T e, ImGuiComboFlags flags = (ImGuiComboFlags)0) where T : struct, Enum
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		if (!ImGui.BeginCombo(ImU8String.op_Implicit(label), ImU8String.op_Implicit(e.GetDisplayName()), flags))
		{
			return false;
		}
		string[] names = Enum.GetNames<T>();
		string name = Enum.GetName(e);
		string[] array = names;
		ImU8String val = default(ImU8String);
		foreach (string text in array)
		{
			((ImU8String)(ref val))._002Ector(2, 2);
			((ImU8String)(ref val)).AppendFormatted<string>(typeof(T).GetField(text)?.GetCustomAttribute<DisplayAttribute>()?.Name ?? text);
			((ImU8String)(ref val)).AppendLiteral("##");
			((ImU8String)(ref val)).AppendFormatted<string>(text);
			if (ImGui.Selectable(val, text == name, (ImGuiSelectableFlags)0, default(Vector2)))
			{
				e = Enum.Parse<T>(text);
				ImGui.EndCombo();
				return true;
			}
		}
		ImGui.EndCombo();
		return false;
	}

	public static bool CheckboxTristate(string label, ref bool? v)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		bool flag2;
		if (!v.HasValue)
		{
			bool flag = false;
			flag2 = ImGui.Checkbox(ImU8String.op_Implicit(label), ref flag);
			if (flag2)
			{
				v = true;
			}
			float frameHeight = ImGui.GetFrameHeight();
			float value = Math.Max(MathF.Floor(frameHeight / 4f), 1f);
			Vector2 vector = new Vector2(value);
			Vector2 itemRectMin = ImGui.GetItemRectMin();
			Vector2 vector2 = itemRectMin + new Vector2(frameHeight);
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			Vector2 vector3 = itemRectMin + vector;
			Vector2 vector4 = vector2 - vector;
			uint colorU = ImGui.GetColorU32((ImGuiCol)18);
			ImGuiStylePtr style = ImGui.GetStyle();
			((ImDrawListPtr)(ref windowDrawList)).AddRect(vector3, vector4, colorU, ((ImGuiStylePtr)(ref style)).FrameRounding, (ImDrawFlags)0, 3f * ImGuiHelpers.GlobalScale);
		}
		else
		{
			bool value2 = v.Value;
			bool num = !value2;
			if (num)
			{
				ImGui.PushStyleColor((ImGuiCol)18, Vector4.Zero);
			}
			flag2 = ImGui.Checkbox(ImU8String.op_Implicit(label), ref value2);
			if (flag2)
			{
				v = (value2 ? ((bool?)null) : new bool?(false));
			}
			if (num)
			{
				ImGui.PopStyleColor();
			}
		}
		return flag2;
	}

	public static void FloatingDrawable(Action<ImDrawListPtr, float, Vector2> draw, uint timerMS = 1000u)
	{
		//IL_0015: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		ImGuiViewportPtr windowViewport = ImGui.GetWindowViewport();
		ImGuiViewportPtr viewport = ((!((ImGuiViewportPtr)(ref windowViewport)).IsNull) ? windowViewport : ImGui.GetMainViewport());
		Vector2 pos = ImGui.GetMousePos();
		Stopwatch timer = Stopwatch.StartNew();
		DalamudApi.PluginInterface.UiBuilder.Draw += f;
		void f()
		{
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			float num = Math.Min((float)timer.ElapsedMilliseconds / (float)timerMS, 1f);
			if (num < 1f && !((Enum)((ImGuiViewportPtr)(ref viewport)).Flags).HasFlag((Enum)(object)(ImGuiViewportFlags)16))
			{
				draw(ImGui.GetForegroundDrawList(viewport), num, pos);
			}
			else
			{
				DalamudApi.PluginInterface.UiBuilder.Draw -= f;
			}
		}
	}

	public static void FloatingText(string text, uint color = uint.MaxValue, uint timerMS = 1000u)
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		Vector2 textSize = ImGui.CalcTextSize(ImU8String.op_Implicit(text), false, -1f);
		uint startingAlpha = color >> 24;
		FloatingDrawable(delegate(ImDrawListPtr drawList, float percentElapsed, Vector2 pos)
		{
			//IL_008d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			uint num = ((percentElapsed > 0.75f) ? ((uint)((float)startingAlpha * (percentElapsed - 0.75f) * 4f) << 24) : 0u);
			pos = new Vector2(pos.X - textSize.X / 2f, pos.Y - textSize.Y - 20f * percentElapsed * ImGuiHelpers.GlobalScale);
			((ImDrawListPtr)(ref drawList)).AddText(pos + Vector2.One * ImGuiHelpers.GlobalScale, (startingAlpha << 24) - num, ImU8String.op_Implicit(text));
			((ImDrawListPtr)(ref drawList)).AddText(pos, color - num, ImU8String.op_Implicit(text));
		}, timerMS);
	}

	public static bool IsItemDragged(object id, ImGuiMouseButton button, float gridSize, bool drawGrid, out Vector2 pos)
	{
		if (!GetDragLock(id, (ImGuiMouseButton)0))
		{
			pos = Vector2.Zero;
			return false;
		}
		pos = GetMouseGridPosition(gridSize) * gridSize;
		if (drawGrid)
		{
			DrawGrid(gridSize, gridSize / 10f + 1f, Vector2.Zero);
		}
		if (initialPosition)
		{
			lastGridPosition = pos;
			initialPosition = false;
			return false;
		}
		bool num = pos != lastGridPosition;
		if (num)
		{
			lastGridPosition = pos;
		}
		return num;
	}

	public static bool IsItemDraggedDelta(object id, ImGuiMouseButton button, float gridSize, bool drawGrid, out Vector2 delta)
	{
		if (!GetDragLock(id, (ImGuiMouseButton)0))
		{
			delta = Vector2.Zero;
			return false;
		}
		Vector2 offset = new Vector2(MathF.Round(gridCenter.X % gridSize), MathF.Round(gridCenter.Y % gridSize));
		Vector2 mouseGridPosition = GetMouseGridPosition(gridSize, offset);
		delta = mouseGridPosition - lastGridPosition;
		if (drawGrid)
		{
			DrawGrid(gridSize, gridSize / 10f + 1f, offset);
		}
		if (initialPosition)
		{
			lastGridPosition = mouseGridPosition;
			initialPosition = false;
			return false;
		}
		bool num = mouseGridPosition != lastGridPosition;
		if (num)
		{
			lastGridPosition = mouseGridPosition;
		}
		return num;
	}

	private static bool GetDragLock(object id, ImGuiMouseButton button)
	{
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		if (isDraggingItem && !ImGui.IsAnyMouseDown())
		{
			dragID = null;
			isDraggingItem = false;
		}
		if (!isDraggingItem && ImGui.IsItemHovered())
		{
			ImGui.SetMouseCursor((ImGuiMouseCursor)2);
		}
		object obj = ((!(id is string text)) ? ((!Util.IsNumeric(id)) ? id : ((object)ImGui.GetID(ImU8String.op_Implicit(id.ToString())))) : ((object)ImGui.GetID(ImU8String.op_Implicit(text))));
		object obj2 = obj;
		if ((!isDraggingItem && !ImGui.IsItemClicked(button)) || (dragID != null && ((dragID is uint num && obj2 is uint num2) ? (num != num2) : (dragID != obj2))))
		{
			return false;
		}
		if (!isDraggingItem)
		{
			BlockWindowDrag();
			dragID = obj2;
			isDraggingItem = true;
			gridCenter = ImGui.GetMousePos();
			initialPosition = true;
		}
		if (ImGui.IsMouseDragging(button, 0f))
		{
			return true;
		}
		dragID = null;
		isDraggingItem = false;
		return false;
	}

	private static Vector2 GetMouseGridPosition(float gridSize, Vector2 offset = default(Vector2))
	{
		Vector2 vector = ImGui.GetMousePos() - offset;
		return new Vector2(MathF.Round(vector.X / gridSize), MathF.Round(vector.Y / gridSize));
	}

	private static void DrawGrid(float size, float circleRadius = 0f, Vector2 offset = default(Vector2))
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000b: Unknown result type (might be due to invalid IL or missing references)
		ImDrawListPtr foregroundDrawList = ImGui.GetForegroundDrawList();
		ImGuiViewportPtr mainViewport = ImGuiHelpers.MainViewport;
		Vector2 size2 = ((ImGuiViewportPtr)(ref mainViewport)).Size;
		for (float num = ((offset.X == 0f) ? size : offset.X); num < size2.X; num += size)
		{
			Vector2 vector = new Vector2(num, 0f);
			Vector2 vector2 = size2;
			vector2.X = num;
			((ImDrawListPtr)(ref foregroundDrawList)).AddLine(vector, vector2, uint.MaxValue);
		}
		for (float num2 = ((offset.Y == 0f) ? size : offset.Y); num2 < size2.Y; num2 += size)
		{
			Vector2 vector3 = new Vector2(0f, num2);
			Vector2 vector2 = size2;
			vector2.Y = num2;
			((ImDrawListPtr)(ref foregroundDrawList)).AddLine(vector3, vector2, uint.MaxValue);
		}
		if (circleRadius != 0f)
		{
			((ImDrawListPtr)(ref foregroundDrawList)).AddCircleFilled(GetMouseGridPosition(size, offset) * size + offset, circleRadius, uint.MaxValue);
		}
	}

	[DllImport("cimgui", ExactSpelling = true)]
	[LibraryImport("cimgui")]
	[UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
	private static extern nint igGetCurrentWindow();

	public unsafe static ImGuiWindow* GetCurrentWindow()
	{
		return (ImGuiWindow*)igGetCurrentWindow();
	}

	public unsafe static ImGuiWindowFlags GetCurrentWindowFlags()
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		return GetCurrentWindow()->Flags;
	}

	public unsafe static bool CurrentWindowHasCloseButton()
	{
		return GetCurrentWindow()->HasCloseButton != 0;
	}
}
