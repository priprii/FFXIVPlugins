using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Utility.Raii;
using Ktisis.Actions;
using Ktisis.Actions.Binds;
using Ktisis.Actions.Types;
using Ktisis.Common.Utility;
using Ktisis.Core.Attributes;
using Ktisis.Data.Config.Actions;
using Ktisis.Localization;

namespace Ktisis.Interface.Components.Config;

[Transient]
public class ActionKeybindEditor
{
	private readonly ActionService _actions;

	private readonly LocaleManager _locale;

	private readonly List<KeyAction> Actions = new List<KeyAction>();

	private static readonly Vector2 CellPadding = new Vector2(8f, 8f);

	private ActionKeybind? Editing;

	private KeyCombo? KeyCombo;

	private readonly List<VirtualKey> KeysHandled = new List<VirtualKey>();

	public ActionKeybindEditor(ActionService actions, LocaleManager locale)
	{
		_actions = actions;
		_locale = locale;
	}

	public void Setup()
	{
		IEnumerable<KeyAction> bindable = _actions.GetBindable();
		Actions.Clear();
		Actions.AddRange(bindable);
		SetEditing(null);
	}

	public void Draw(string? pattern = null, bool allowToolbar = false)
	{
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_010f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0125: Unknown result type (might be due to invalid IL or missing references)
		StyleDisposable val = ImRaii.PushStyle((ImGuiStyleVar)1, Vector2.Zero, true);
		try
		{
			List<KeyAction> list = Actions;
			if (pattern != null)
			{
				Regex regex = new Regex(pattern);
				IEnumerable<KeyAction> source = list.Where((KeyAction x) => regex.IsMatch(x.GetName().ToLower()));
				if (!allowToolbar)
				{
					source = source.Where((KeyAction x) => !x.GetName().StartsWith("Toolbar")).ToList();
				}
				list = source.ToList();
			}
			ChildDisposable val2 = ImRaii.Child(ImU8String.op_Implicit("##CfgStyleFrame"), new Vector2(ImGui.GetContentRegionAvail().X - 0.1f, (float)list.Count * (ImGui.GetTextLineHeightWithSpacing() + CellPadding.Y * 2f)), false);
			try
			{
				if (!val2.Success)
				{
					return;
				}
				StyleDisposable val3 = ImRaii.PushStyle((ImGuiStyleVar)16, Vector2.Zero, true);
				try
				{
					TableDisposable val4 = ImRaii.Table(ImU8String.op_Implicit("##KeyActionTable"), 2, (ImGuiTableFlags)1921);
					try
					{
						if (!val4.Success)
						{
							return;
						}
						if (!ImGui.IsWindowFocused())
						{
							SetEditing(null);
						}
						ImGui.TableSetupColumn(ImU8String.op_Implicit("Keys"), (ImGuiTableColumnFlags)0, 0f, 0u);
						ImGui.TableSetupColumn(ImU8String.op_Implicit("Action"), (ImGuiTableColumnFlags)0, 0f, 0u);
						foreach (KeyAction item in list)
						{
							DrawAction(item);
						}
					}
					finally
					{
						((TableDisposable)(ref val4)).Dispose();
					}
				}
				finally
				{
					((IDisposable)val3)?.Dispose();
				}
			}
			finally
			{
				((ChildDisposable)(ref val2)).Dispose();
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}

	private void DrawAction(KeyAction action)
	{
		//IL_004d: Unknown result type (might be due to invalid IL or missing references)
		ImGui.TableNextRow();
		ImGui.TableNextColumn();
		DrawKeybind(action.GetKeybind());
		ImGui.TableNextColumn();
		string text = _locale.Translate("actions." + action.GetName());
		ImGui.SetCursorPos(ImGui.GetCursorPos() + CellPadding);
		ImGui.Text(ImU8String.op_Implicit(text));
	}

	private void DrawKeybind(ActionKeybind keybind)
	{
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(8, 1);
		((ImU8String)(ref val)).AppendLiteral("Keybind_");
		((ImU8String)(ref val)).AppendFormatted<int>(keybind.GetHashCode(), "X");
		IdDisposable val2 = ImRaii.PushId(val, true);
		try
		{
			StyleDisposable val3 = ImRaii.PushStyle((ImGuiStyleVar)11, 0f, true);
			try
			{
				bool flag = Editing == keybind;
				uint colorU = ImGui.GetColorU32((ImGuiCol)9);
				ColorDisposable val4 = ImRaii.PushColor((ImGuiCol)21, flag ? colorU : 0u, true);
				try
				{
					ColorDisposable val5 = ImRaii.PushColor((ImGuiCol)23, colorU, true);
					try
					{
						ColorDisposable val6 = ImRaii.PushColor((ImGuiCol)22, flag ? colorU : ImGui.GetColorU32((ImGuiCol)8), true);
						try
						{
							float columnWidth = ImGui.GetColumnWidth();
							Vector2 vector = new Vector2(columnWidth, ImGui.GetFrameHeightWithSpacing()) + CellPadding;
							if (flag)
							{
								ImGuiStylePtr style = ImGui.GetStyle();
								Vector2 itemSpacing = ((ImGuiStylePtr)(ref style)).ItemSpacing;
								ImGui.SetCursorPos(ImGui.GetCursorPos() + CellPadding);
								ImGui.SetNextItemWidth(columnWidth - CellPadding.X - itemSpacing.X);
								EditKeybind(keybind);
								ImGui.Dummy(CellPadding - itemSpacing);
							}
							else if (ImGui.Button(ImU8String.op_Implicit(keybind.Combo.GetShortcutString()), vector))
							{
								SetEditing(keybind);
							}
							if (ImGui.IsItemClicked((ImGuiMouseButton)1))
							{
								keybind.Combo = new KeyCombo((VirtualKey)0);
							}
							else if (Editing != null && Editing != keybind && ImGui.IsItemFocused())
							{
								SetEditing(null);
							}
						}
						finally
						{
							((IDisposable)val6)?.Dispose();
						}
					}
					finally
					{
						((IDisposable)val5)?.Dispose();
					}
				}
				finally
				{
					((IDisposable)val4)?.Dispose();
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

	private void SetEditing(ActionKeybind? keybind)
	{
		FinishEdit();
		Editing = keybind;
		KeyCombo = null;
		KeysHandled.Clear();
	}

	private void FinishEdit()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		if (Editing != null && KeyCombo != null)
		{
			if ((int)KeyCombo.Key != 0)
			{
				Editing.Combo = KeyCombo;
			}
			Ktisis.Log.Info("Applying edit (" + KeyCombo.GetShortcutString() + ")");
		}
	}

	private void EditKeybind(ActionKeybind keybind)
	{
		//IL_0053: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Invalid comparison between Unknown and I4
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b0: Invalid comparison between Unknown and I4
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0176: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		//IL_0116: Unknown result type (might be due to invalid IL or missing references)
		//IL_011e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0120: Unknown result type (might be due to invalid IL or missing references)
		//IL_012b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0102: Unknown result type (might be due to invalid IL or missing references)
		IdDisposable val = ImRaii.PushId(keybind.GetHashCode(), true);
		try
		{
			ColorDisposable val2 = ImRaii.PushColor((ImGuiCol)49, 0u, true);
			try
			{
				if (KeyCombo == null)
				{
					KeyCombo = new KeyCombo((VirtualKey)0);
				}
				List<VirtualKey> list = KeyHelpers.GetKeysDown().ToList();
				List<VirtualKey> list2 = list.Except(KeysHandled).ToList();
				if ((int)KeyCombo.Key != 0 && !list.Contains(KeyCombo.Key))
				{
					SetEditing(null);
					return;
				}
				KeysHandled.AddRange(list2);
				foreach (VirtualKey item in list2)
				{
					if ((int)item == 13)
					{
						SetEditing(null);
						return;
					}
					if ((int)item == 8)
					{
						KeyCombo = null;
						SetEditing(null);
						return;
					}
					if ((int)KeyCombo.Key == 0)
					{
						KeyCombo.Key = item;
						continue;
					}
					if (KeyHelpers.IsModifierKey(item) && !KeyHelpers.IsModifierKey(KeyCombo.Key))
					{
						KeyCombo.AddModifier(item);
						continue;
					}
					VirtualKey key = KeyCombo.Key;
					KeyCombo.Key = item;
					KeyCombo.AddModifier(key);
				}
				string shortcutString = ((KeysHandled.Count > 0) ? KeyCombo : keybind.Combo).GetShortcutString();
				ImGui.InputText(ImU8String.op_Implicit("##EditKeybind"), ref shortcutString, 256, (ImGuiInputTextFlags)16384, (ImGuiInputTextCallbackDelegate)null);
				ImGui.SetKeyboardFocusHere(-1);
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

	internal void ResetBinds(string pattern, bool allowToolbar = false)
	{
		Regex regex = new Regex(pattern);
		IEnumerable<KeyAction> enumerable = Actions.Where((KeyAction x) => regex.IsMatch(x.GetName().ToLower()));
		if (!allowToolbar)
		{
			enumerable = enumerable.Where((KeyAction x) => !x.GetName().StartsWith("Toolbar"));
		}
		foreach (KeyAction item in enumerable)
		{
			item.GetKeybind().Combo = item.BindInfo.Default.Combo;
		}
	}
}
