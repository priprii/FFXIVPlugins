using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Text;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using Newtonsoft.Json;

namespace SoundPyon;

public class MainWindow : Window
{
	private readonly Plugin plugin;

	private int SelectedGroupIndex = -1;

	public bool LogSounds;

	public bool LogFilteredSounds;

	private string LogSearch = string.Empty;

	private FilterGroup? SelectedGroup
	{
		get
		{
			if (SelectedGroupIndex < 0 || SelectedGroupIndex >= Plugin.Config.Filters.Count)
			{
				return null;
			}
			return Plugin.Config.Filters[SelectedGroupIndex];
		}
	}

	public MainWindow(Plugin plugin)
		: base("SoundPyon")
	{
		this.plugin = plugin;
		((Window)this).SizeCondition = (ImGuiCond)4;
		((Window)this).Size = new Vector2(600f, 600f) * ImGuiHelpers.GlobalScale;
	}

	public override void Draw()
	{
		if (((Window)this).IsOpen)
		{
			if (Plugin.Config.Filters.Count > 0 && SelectedGroupIndex == -1)
			{
				SelectedGroupIndex = 0;
			}
			DrawHeader();
			DrawGroupList();
		}
	}

	private void DrawHeader()
	{
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		bool flag = SelectedGroup != null;
		if (ImGuiEx.IconButton((FontAwesomeIcon)61525, "newgroup"))
		{
			FilterGroup item = new FilterGroup();
			Plugin.Config.Filters.Add(item);
			Plugin.Config.Save();
		}
		ImGuiEx.SetItemTooltip("Create new filter group.", (ImGuiHoveredFlags)0);
		ImGui.SameLine();
		if (ImGuiEx.IconButton((FontAwesomeIcon)61610, "movegroupup") && flag)
		{
			FilterGroup selectedGroup = SelectedGroup;
			Plugin.Config.Filters.RemoveAt(SelectedGroupIndex);
			SelectedGroupIndex = Math.Max(SelectedGroupIndex - 1, 0);
			Plugin.Config.Filters.Insert(SelectedGroupIndex, selectedGroup);
			Plugin.Config.Save();
		}
		ImGuiEx.SetItemTooltip("Move selected filter group up.", (ImGuiHoveredFlags)0);
		ImGui.SameLine();
		if (ImGuiEx.IconButton((FontAwesomeIcon)61611, "movegroupdown") && flag)
		{
			FilterGroup selectedGroup2 = SelectedGroup;
			Plugin.Config.Filters.RemoveAt(SelectedGroupIndex);
			SelectedGroupIndex = Math.Min(SelectedGroupIndex + 1, Plugin.Config.Filters.Count);
			Plugin.Config.Filters.Insert(SelectedGroupIndex, selectedGroup2);
			Plugin.Config.Save();
		}
		ImGuiEx.SetItemTooltip("Move selected filter group down.", (ImGuiHoveredFlags)0);
		ImGui.SameLine();
		ImGuiEx.IconButton((FontAwesomeIcon)62189, "removegroup");
		ImGuiEx.SetItemTooltip("Remove selected filter group.", (ImGuiHoveredFlags)0);
		if (flag && ImGui.BeginPopupContextItem(ImU8String.op_Implicit("##removegroupcitem"), (ImGuiPopupFlags)0))
		{
			if (ImGuiEx.IconSelectable((FontAwesomeIcon)62189))
			{
				Plugin.Config.Filters.RemoveAt(SelectedGroupIndex);
				SelectedGroupIndex = Math.Min(SelectedGroupIndex, Plugin.Config.Filters.Count - 1);
				Plugin.Config.Save();
			}
			ImGui.EndPopup();
		}
		ImGui.SameLine();
		if (ImGuiEx.IconButton((FontAwesomeIcon)61637, "copygroup") && flag)
		{
			FilterGroup selectedGroup3 = SelectedGroup;
			ImGui.SetClipboardText(ImU8String.op_Implicit(CompressToBase64(JsonConvert.SerializeObject((object)selectedGroup3))));
		}
		ImGuiEx.SetItemTooltip("Copy the selected group to clipboard.", (ImGuiHoveredFlags)0);
		ImGui.SameLine();
		if (ImGuiEx.IconButton((FontAwesomeIcon)61674, "pastegroup") && TryImportFilterGroup<FilterGroup>(ImGui.GetClipboardText().Trim(), out FilterGroup result) && result != null)
		{
			result.Guid = Guid.NewGuid();
			result.Enabled = true;
			Plugin.Config.Filters.Add(result);
			Plugin.Config.Save();
		}
		ImGuiEx.SetItemTooltip("Create a new filter group from a copied group.", (ImGuiHoveredFlags)0);
		ImGui.SameLine();
		if (ImGui.Checkbox(ImU8String.op_Implicit("Enable Sound Filter"), ref Plugin.Config.Enabled))
		{
			if (Plugin.Config.Enabled)
			{
				plugin.Filter.Enable();
			}
			else
			{
				plugin.Filter.Disable();
			}
			Plugin.Config.Save();
		}
	}

	private string CompressToBase64(string input)
	{
		byte[] bytes = Encoding.UTF8.GetBytes(input);
		using MemoryStream memoryStream = new MemoryStream();
		using (GZipStream gZipStream = new GZipStream(memoryStream, CompressionMode.Compress))
		{
			gZipStream.Write(bytes, 0, bytes.Length);
		}
		return Convert.ToBase64String(memoryStream.ToArray());
	}

	private string DecompressFromBase64(string base64)
	{
		using MemoryStream stream = new MemoryStream(Convert.FromBase64String(base64));
		using GZipStream gZipStream = new GZipStream(stream, CompressionMode.Decompress);
		using MemoryStream memoryStream = new MemoryStream();
		gZipStream.CopyTo(memoryStream);
		return Encoding.UTF8.GetString(memoryStream.ToArray());
	}

	private bool TryImportFilterGroup<FilterGroup>(string base64, out FilterGroup? result)
	{
		result = default(FilterGroup);
		if (string.IsNullOrWhiteSpace(base64))
		{
			return false;
		}
		string text;
		try
		{
			text = DecompressFromBase64(base64);
		}
		catch
		{
			return false;
		}
		try
		{
			FilterGroup val = JsonConvert.DeserializeObject<FilterGroup>(text);
			if (val == null)
			{
				return false;
			}
			if (val != null)
			{
				result = val;
				return true;
			}
		}
		catch
		{
		}
		return false;
	}

	private void DrawGroupList()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		bool flag = SelectedGroup != null;
		ImGui.BeginChild(ImU8String.op_Implicit("SoundPyonGroupList"), new Vector2(140f * ImGuiHelpers.GlobalScale, 0f), true, (ImGuiWindowFlags)0);
		Vector4 dalamudViolet = ImGuiColors.DalamudViolet;
		ImGui.TextColored(ref dalamudViolet, ImU8String.op_Implicit("Filter Groups"));
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		if (Plugin.Config.Filters.Count == 0)
		{
			dalamudViolet = ImGuiColors.DalamudRed;
			ImU8String val = default(ImU8String);
			((ImU8String)(ref val))._002Ector(19, 0);
			((ImU8String)(ref val)).AppendLiteral("No Groups Available");
			ImGui.TextColored(ref dalamudViolet, val);
			ImGuiEx.SetItemTooltip("Click the + button above to create a new filter group.", (ImGuiHoveredFlags)0);
		}
		else
		{
			for (int i = 0; i < Plugin.Config.Filters.Count; i++)
			{
				ImGui.PushID((IntPtr)i);
				FilterGroup filterGroup = Plugin.Config.Filters[i];
				bool enabled = filterGroup.Enabled;
				ImGui.PushStyleColor((ImGuiCol)0, enabled ? 4282711876u : 4289374890u);
				if (ImGui.Selectable(ImU8String.op_Implicit(filterGroup.Name), SelectedGroupIndex == i, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					SelectedGroupIndex = i;
				}
				ImGui.PopStyleColor();
				if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked((ImGuiMouseButton)0))
				{
					filterGroup.Enabled = !enabled;
					Plugin.Config.Save();
				}
				ImGui.PopID();
			}
		}
		ImGui.EndChild();
		if (flag)
		{
			ImGui.SameLine();
			ImGui.BeginChild(ImU8String.op_Implicit("SoundPyonGroupEditor"), Vector2.Zero, true, (ImGuiWindowFlags)0);
			DrawGroupEditor(SelectedGroup);
			ImGui.EndChild();
		}
	}

	private void DrawGroupEditor(FilterGroup group)
	{
		//IL_0022: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0142: Unknown result type (might be due to invalid IL or missing references)
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(8, 1);
		((ImU8String)(ref val)).AppendLiteral("##enable");
		((ImU8String)(ref val)).AppendFormatted<int>(SelectedGroupIndex);
		if (ImGui.Checkbox(val, ref group.Enabled))
		{
			Plugin.Config.Save();
		}
		ImGuiEx.SetItemTooltip("Enable filtering of sounds added to this group.", (ImGuiHoveredFlags)0);
		ImGui.SameLine();
		ImGui.SetNextItemWidth(200f);
		if (ImGui.InputText(ImU8String.op_Implicit("Name"), ref group.Name, 64, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null))
		{
			Plugin.Config.Save();
		}
		ImGui.Spacing();
		if (ImGui.BeginChild(ImU8String.op_Implicit("FilterGroup"), new Vector2(0f, (ImGui.GetContentRegionMax().Y - 50f) / 2f), true, (ImGuiWindowFlags)0))
		{
			int num = 0;
			foreach (string glob in group.Globs)
			{
				if (ImGuiEx.IconButton((FontAwesomeIcon)61944, $"removefilter-{SelectedGroupIndex}-{num}"))
				{
					group.Globs.RemoveAt(num);
					Plugin.Config.Save();
					return;
				}
				ImGuiEx.SetItemTooltip("Remove this filter from the group.", (ImGuiHoveredFlags)0);
				ImGui.SameLine();
				ImGui.TextUnformatted(ImU8String.op_Implicit(glob));
				num++;
			}
			ImGui.EndChild();
		}
		DrawSoundLog(group);
	}

	private void DrawSoundLog(FilterGroup group)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0065: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_032a: Unknown result type (might be due to invalid IL or missing references)
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		ImGui.Checkbox(ImU8String.op_Implicit("Enable Logging"), ref LogSounds);
		ImGuiEx.SetItemTooltip("Log sounds to the list below which can then be filtered.", (ImGuiHoveredFlags)0);
		ImGui.SameLine();
		ImGui.Checkbox(ImU8String.op_Implicit("Include Filtered"), ref LogFilteredSounds);
		ImGuiEx.SetItemTooltip("Include logging of sounds that have already been filtered.", (ImGuiHoveredFlags)0);
		ImGui.SetNextItemWidth(180f);
		ImGui.InputText(ImU8String.op_Implicit("Search"), ref LogSearch, 255, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null);
		ImGui.SameLine();
		ImGui.SetNextItemWidth(100f);
		int logLimit = (int)Plugin.Config.LogLimit;
		ImU8String val = ImU8String.op_Implicit("Max Sounds");
		ImU8String val2 = default(ImU8String);
		if (ImGui.InputInt(val, ref logLimit, 0, 0, val2, (ImGuiInputTextFlags)0))
		{
			Plugin.Config.LogLimit = (uint)Math.Min(10000, Math.Max(0, logLimit));
			Plugin.Config.Save();
		}
		ImGui.Separator();
		if (!ImGui.BeginChild(ImU8String.op_Implicit("Sounds"), new Vector2(0f, (ImGui.GetContentRegionMax().Y - 50f) / 2f), true, (ImGuiWindowFlags)0))
		{
			return;
		}
		int num = 0;
		foreach (KeyValuePair<string, LoggedSound> item in plugin.Filter.Recent.Reverse())
		{
			if (!string.IsNullOrWhiteSpace(LogSearch) && !ContainsIgnoreCase(item.Key, LogSearch))
			{
				continue;
			}
			if (group.Globs.Contains(item.Key))
			{
				if (ImGuiEx.IconButton((FontAwesomeIcon)62189, $"remove-{item.Key}-{num}"))
				{
					group.Globs.Remove(item.Key);
					Plugin.Config.Save();
				}
				ImGuiEx.SetItemTooltip("Remove this sound from the current filter group.", (ImGuiHoveredFlags)0);
			}
			else
			{
				if (ImGuiEx.IconButton((FontAwesomeIcon)61525, $"add-{item.Key}-{num}"))
				{
					group.Globs.Add(item.Key);
					Plugin.Config.Save();
				}
				ImGuiEx.SetItemTooltip("Add this sound to the current filter group.", (ImGuiHoveredFlags)0);
			}
			ImGui.SameLine();
			double totalSeconds = (DateTime.UtcNow - item.Value.LastPlayed).TotalSeconds;
			totalSeconds = Math.Min(totalSeconds, 1.0);
			Math.Max(0f, 1f - (float)(totalSeconds / 1.0));
			float num2 = Math.Min(1f, (float)(totalSeconds / 1.0));
			Vector4 vector = new Vector4(num2, 1f, num2, 1f);
			val2 = new ImU8String(3, 2);
			((ImU8String)(ref val2)).AppendLiteral("[");
			((ImU8String)(ref val2)).AppendFormatted<int>(item.Value.Count, "000");
			((ImU8String)(ref val2)).AppendLiteral("] ");
			((ImU8String)(ref val2)).AppendFormatted<string>(item.Key);
			ImGui.TextColored(ref vector, val2);
			num++;
		}
		ImGui.EndChild();
	}

	private bool ContainsIgnoreCase(string haystack, string needle)
	{
		return CultureInfo.InvariantCulture.CompareInfo.IndexOf(haystack, needle, CompareOptions.IgnoreCase) >= 0;
	}

	public override void OnClose()
	{
		LogSounds = false;
		LogFilteredSounds = false;
		((Window)this).OnClose();
	}
}
