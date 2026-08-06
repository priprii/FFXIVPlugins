using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Common.Math;
using Lumina.Excel.Sheets;
using Lumina.Text.ReadOnly;
using Newtonsoft.Json;

namespace TriggerPyon;

public class MainWindow : Window
{
	private readonly Plugin plugin;

	private int SelectedTriggerIndex = -1;

	private bool IsComboOpen_TriggerEmotes;

	private bool IsComboOpen_ReactionEmotes;

	private bool DrawRangePreview;

	private float RangePreviewOpacity = 0.2f;

	private static bool ResidentialOnly = true;

	private static List<(uint Id, string Name, bool IsResidential)>? TerritoryUiList;

	private Trigger? SelectedTrigger
	{
		get
		{
			if (SelectedTriggerIndex < 0 || SelectedTriggerIndex >= Plugin.Config.Triggers.Count)
			{
				return null;
			}
			return Plugin.Config.Triggers[SelectedTriggerIndex];
		}
	}

	public MainWindow(Plugin plugin)
		: base("TriggerPyon v1.2.1.0")
	{
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		this.plugin = plugin;
		((Window)this).SizeCondition = (ImGuiCond)4;
		((Window)this).Size = new Vector2(600f, 600f) * ImGuiHelpers.GlobalScale;
		WindowSizeConstraints value = default(WindowSizeConstraints);
		((WindowSizeConstraints)(ref value))._002Ector();
		((WindowSizeConstraints)(ref value)).MinimumSize = new Vector2(400f, 400f) * ImGuiHelpers.GlobalScale;
		((Window)this).SizeConstraints = value;
	}

	public override void Draw()
	{
		if (((Window)this).IsOpen)
		{
			if (Plugin.Config.Triggers.Count > 0 && SelectedTriggerIndex == -1)
			{
				SelectedTriggerIndex = 0;
			}
			DrawHeader();
			DrawTriggersList();
		}
	}

	private void DrawHeader()
	{
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
		//IL_0207: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b12: Unknown result type (might be due to invalid IL or missing references)
		//IL_0355: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bcf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0470: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c9a: Unknown result type (might be due to invalid IL or missing references)
		//IL_05af: Unknown result type (might be due to invalid IL or missing references)
		//IL_0746: Unknown result type (might be due to invalid IL or missing references)
		//IL_08fc: Unknown result type (might be due to invalid IL or missing references)
		bool flag = SelectedTrigger != null;
		ImGuiEx.IconButton((FontAwesomeIcon)61525, "newtrigger");
		ImGuiEx.SetItemTooltip("Create new trigger.", (ImGuiHoveredFlags)0);
		if (ImGui.BeginPopupContextItem(ImU8String.op_Implicit("##newTrigger"), (ImGuiPopupFlags)0))
		{
			if (ImGui.Selectable(ImU8String.op_Implicit("Create Empty Trigger"), false, (ImGuiSelectableFlags)0, default(Vector2)))
			{
				Trigger item = new Trigger();
				Plugin.Config.Triggers.Add(item);
				Plugin.Config.Save();
			}
			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();
			ImGui.PushStyleColor((ImGuiCol)0, ImGuiColors.DalamudViolet);
			ImGui.Selectable(ImU8String.op_Implicit("Create from Preset:"), false, (ImGuiSelectableFlags)1, default(Vector2));
			ImGui.PopStyleColor();
			ImGui.Separator();
			if (ImGui.Selectable(ImU8String.op_Implicit("Hug Counter"), false, (ImGuiSelectableFlags)0, default(Vector2)))
			{
				Trigger trigger = new Trigger();
				trigger.Enabled = true;
				trigger.Name = "Hug Counter";
				trigger.Description = "Count & display title when receiving a hug.";
				trigger.Type = TriggerType.Emote;
				EmoteAction emoteAction = new EmoteAction();
				int num = 2;
				List<ushort> list = new List<ushort>(num);
				CollectionsMarshal.SetCount(list, num);
				Span<ushort> span = CollectionsMarshal.AsSpan(list);
				span[0] = 112;
				span[1] = 113;
				emoteAction.IDs = list;
				trigger.ReceivedAction = emoteAction;
				trigger.Instigator = new Instigator
				{
					Type = PlayerType.Others
				};
				trigger.Receiver = new EmoteTargetReceiver
				{
					Type = PlayerType.Self
				};
				trigger.Counter = new Counter
				{
					DisplayTitle = true,
					TitleTemplate = "Hugged x%n%"
				};
				trigger.ReactionOptions = new ReactionOptions
				{
					ReactionCooldown = 0,
					InterruptType = ReactionInterruptType.Any,
					StateConditions = StateConditionType.None,
					RestoreType = RestoreType.None,
					RestrictRange = true,
					RestrictedDistanceMin = 0f,
					RestrictedDistanceMax = 0.5f,
					RestrictedAngleDirection = 0,
					RestrictedAngleArea = 1f
				};
				trigger.Reactions = null;
				Plugin.Config.Triggers.Add(trigger);
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Setup a trigger for counting the number of hugs received & display in Honorific title.\n\n- Requires 'Honorific' plugin.", (ImGuiHoveredFlags)0);
			if (ImGui.Selectable(ImU8String.op_Implicit("Pat Counter"), false, (ImGuiSelectableFlags)0, default(Vector2)))
			{
				Trigger trigger2 = new Trigger();
				trigger2.Enabled = true;
				trigger2.Name = "Pat Counter";
				trigger2.Description = "Count & display title when receiving a pat.";
				trigger2.Type = TriggerType.Emote;
				EmoteAction emoteAction2 = new EmoteAction();
				int num = 1;
				List<ushort> list2 = new List<ushort>(num);
				CollectionsMarshal.SetCount(list2, num);
				CollectionsMarshal.AsSpan(list2)[0] = 105;
				emoteAction2.IDs = list2;
				trigger2.ReceivedAction = emoteAction2;
				trigger2.Instigator = new Instigator
				{
					Type = PlayerType.Others
				};
				trigger2.Receiver = new EmoteTargetReceiver
				{
					Type = PlayerType.Self
				};
				trigger2.Counter = new Counter
				{
					DisplayTitle = true,
					TitleTemplate = "Patted x%n%"
				};
				trigger2.ReactionOptions = new ReactionOptions
				{
					ReactionCooldown = 0,
					InterruptType = ReactionInterruptType.Any,
					StateConditions = StateConditionType.None,
					RestoreType = RestoreType.None,
					RestrictRange = true,
					RestrictedDistanceMin = 0f,
					RestrictedDistanceMax = 0.6f,
					RestrictedAngleDirection = 0,
					RestrictedAngleArea = 1f
				};
				trigger2.Reactions = null;
				Plugin.Config.Triggers.Add(trigger2);
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Setup a trigger for counting the number of pats received & display in Honorific title.\n\n- Requires 'Honorific' plugin.", (ImGuiHoveredFlags)0);
			if (ImGui.Selectable(ImU8String.op_Implicit("Dote Counter"), false, (ImGuiSelectableFlags)0, default(Vector2)))
			{
				Trigger trigger3 = new Trigger();
				trigger3.Enabled = true;
				trigger3.Name = "Dote Counter";
				trigger3.Description = "Count & display title when receiving a dote/kiss.";
				trigger3.Type = TriggerType.Emote;
				EmoteAction emoteAction3 = new EmoteAction();
				int num = 3;
				List<ushort> list3 = new List<ushort>(num);
				CollectionsMarshal.SetCount(list3, num);
				Span<ushort> span2 = CollectionsMarshal.AsSpan(list3);
				span2[0] = 46;
				span2[1] = 146;
				span2[2] = 147;
				emoteAction3.IDs = list3;
				trigger3.ReceivedAction = emoteAction3;
				trigger3.Instigator = new Instigator
				{
					Type = PlayerType.Others
				};
				trigger3.Receiver = new EmoteTargetReceiver
				{
					Type = PlayerType.Self
				};
				trigger3.Counter = new Counter
				{
					DisplayTitle = true,
					TitleTemplate = "Chuu x%n%"
				};
				trigger3.ReactionOptions = null;
				trigger3.Reactions = null;
				Plugin.Config.Triggers.Add(trigger3);
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Setup a trigger for counting the number of dotes received & display in Honorific title.\n\n- Requires 'Honorific' plugin.", (ImGuiHoveredFlags)0);
			if (ImGui.Selectable(ImU8String.op_Implicit("Mimic Emotes"), false, (ImGuiSelectableFlags)0, default(Vector2)))
			{
				Trigger trigger4 = new Trigger();
				trigger4.Enabled = true;
				trigger4.Name = "Mimic Emotes";
				trigger4.Description = "Copy emotes that other players use while targeting you.";
				trigger4.Type = TriggerType.Emote;
				trigger4.ReceivedAction = new EmoteAction
				{
					IDs = new List<ushort>(),
					MatchAny = true
				};
				trigger4.Instigator = new Instigator
				{
					Type = PlayerType.Others
				};
				trigger4.Receiver = new EmoteTargetReceiver
				{
					Type = PlayerType.Self
				};
				trigger4.Counter = null;
				trigger4.ReactionOptions = new ReactionOptions
				{
					ReactionCooldown = 0,
					InterruptType = ReactionInterruptType.None,
					StateConditions = (StateConditionType.Moving | StateConditionType.Sleeping | StateConditionType.Emote),
					RestoreType = RestoreType.None
				};
				int num = 1;
				List<ReactionBase> list4 = new List<ReactionBase>(num);
				CollectionsMarshal.SetCount(list4, num);
				CollectionsMarshal.AsSpan(list4)[0] = new EmoteReaction
				{
					Duration = 1500,
					ID = 0,
					CopyInstigator = true,
					TargetType = ReactionTargetType.TargetInstigator,
					LookAtType = ReactionLookAtType.Target
				};
				trigger4.Reactions = list4;
				Plugin.Config.Triggers.Add(trigger4);
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Setup a trigger for copying emotes that other players use while targeting you.", (ImGuiHoveredFlags)0);
			if (ImGui.Selectable(ImU8String.op_Implicit("Spank Reaction"), false, (ImGuiSelectableFlags)0, default(Vector2)))
			{
				Trigger trigger5 = new Trigger();
				trigger5.Enabled = true;
				trigger5.Name = "Spank Reaction";
				trigger5.Description = "React to being spanked & display title.";
				trigger5.Type = TriggerType.Emote;
				EmoteAction emoteAction4 = new EmoteAction();
				int num = 1;
				List<ushort> list5 = new List<ushort>(num);
				CollectionsMarshal.SetCount(list5, num);
				CollectionsMarshal.AsSpan(list5)[0] = 213;
				emoteAction4.IDs = list5;
				trigger5.ReceivedAction = emoteAction4;
				trigger5.Instigator = new Instigator
				{
					Type = PlayerType.Others
				};
				trigger5.Receiver = new EmoteTargetReceiver
				{
					Type = PlayerType.Self
				};
				trigger5.Counter = new Counter
				{
					DisplayTitle = true,
					TitleTemplate = "Spank Count: %n%"
				};
				trigger5.ReactionOptions = new ReactionOptions
				{
					ReactionCooldown = 0,
					InterruptType = ReactionInterruptType.Same,
					StateConditions = (StateConditionType.Moving | StateConditionType.GroundSit | StateConditionType.ChairSit | StateConditionType.Sleeping),
					RestoreType = RestoreType.Emote,
					RestrictRange = true,
					RestrictedDistanceMin = 0.1f,
					RestrictedDistanceMax = 0.5f,
					RestrictedAngleDirection = 180,
					RestrictedAngleArea = 0.35f
				};
				num = 1;
				List<ReactionBase> list6 = new List<ReactionBase>(num);
				CollectionsMarshal.SetCount(list6, num);
				CollectionsMarshal.AsSpan(list6)[0] = new EmoteReaction
				{
					Duration = 1500,
					ID = 32,
					LookAtType = ReactionLookAtType.Maintain
				};
				trigger5.Reactions = list6;
				Plugin.Config.Triggers.Add(trigger5);
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Setup a trigger for reacting to being spanked.\n\n- Requires 'Spanked Reaction' mod or similar, replacing the 'Shocked' emote.", (ImGuiHoveredFlags)0);
			if (ImGui.Selectable(ImU8String.op_Implicit("Return Dote"), false, (ImGuiSelectableFlags)0, default(Vector2)))
			{
				Trigger trigger6 = new Trigger();
				trigger6.Enabled = true;
				trigger6.Name = "Return Dote";
				trigger6.Description = "Respond with a dote when receiving a dote.";
				trigger6.Type = TriggerType.Emote;
				EmoteAction emoteAction5 = new EmoteAction();
				int num = 3;
				List<ushort> list7 = new List<ushort>(num);
				CollectionsMarshal.SetCount(list7, num);
				Span<ushort> span3 = CollectionsMarshal.AsSpan(list7);
				span3[0] = 46;
				span3[1] = 146;
				span3[2] = 147;
				emoteAction5.IDs = list7;
				trigger6.ReceivedAction = emoteAction5;
				trigger6.Instigator = new Instigator
				{
					Type = PlayerType.Others
				};
				trigger6.Receiver = new EmoteTargetReceiver
				{
					Type = PlayerType.Self
				};
				trigger6.Counter = new Counter
				{
					DisplayTitle = true,
					TitleTemplate = "Chuu x%n%"
				};
				trigger6.ReactionOptions = new ReactionOptions
				{
					ReactionCooldown = 0,
					InterruptType = ReactionInterruptType.None,
					StateConditions = (StateConditionType.Moving | StateConditionType.GroundSit | StateConditionType.ChairSit | StateConditionType.Sleeping | StateConditionType.Emote | StateConditionType.LoopingEmote),
					RestoreType = RestoreType.None,
					RestrictRange = true,
					RestrictedDistanceMin = 0f,
					RestrictedDistanceMax = 17f,
					RestrictedAngleDirection = 0,
					RestrictedAngleArea = 1f
				};
				num = 1;
				List<ReactionBase> list8 = new List<ReactionBase>(num);
				CollectionsMarshal.SetCount(list8, num);
				CollectionsMarshal.AsSpan(list8)[0] = new EmoteReaction
				{
					Duration = 1500,
					ID = 146,
					TargetType = ReactionTargetType.TargetInstigator,
					LookAtType = ReactionLookAtType.Target
				};
				trigger6.Reactions = list8;
				Plugin.Config.Triggers.Add(trigger6);
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Setup a trigger for responding with a dote when receiving a dote within a limited range.", (ImGuiHoveredFlags)0);
			if (ImGui.Selectable(ImU8String.op_Implicit("Discord Spotify"), false, (ImGuiSelectableFlags)0, default(Vector2)))
			{
				Trigger trigger7 = new Trigger();
				trigger7.Enabled = true;
				trigger7.Name = "Spotify Activity";
				trigger7.Description = "Display current song as title.";
				trigger7.Type = TriggerType.Discord;
				trigger7.ReceivedAction = null;
				trigger7.Instigator = null;
				trigger7.Receiver = null;
				DiscordCounter obj = new DiscordCounter
				{
					ActivityType = DiscordActivityType.Listening
				};
				int num = 2;
				List<string> list9 = new List<string>(num);
				CollectionsMarshal.SetCount(list9, num);
				Span<string> span4 = CollectionsMarshal.AsSpan(list9);
				span4[0] = "♪ %artist% ♪";
				span4[1] = "♪ %title% ♪";
				obj.TitleTemplates = list9;
				trigger7.Counter = obj;
				trigger7.ReactionOptions = null;
				trigger7.Reactions = null;
				Plugin.Config.Triggers.Add(trigger7);
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Setup a Discord activity trigger for displaying the song you're listening to.\n\n- Requires Discord/Spotify setup explained in the trigger.\n- Requires 'Honorific' plugin.", (ImGuiHoveredFlags)0);
			ImGui.EndPopup();
		}
		ImGui.SameLine();
		if (ImGuiEx.IconButton((FontAwesomeIcon)61610, "movetriggerup") && flag)
		{
			Trigger selectedTrigger = SelectedTrigger;
			if (selectedTrigger != null)
			{
				Plugin.Config.Triggers.RemoveAt(SelectedTriggerIndex);
				SelectedTriggerIndex = Math.Max(SelectedTriggerIndex - 1, 0);
				Plugin.Config.Triggers.Insert(SelectedTriggerIndex, selectedTrigger);
				Plugin.Config.Save();
			}
		}
		ImGuiEx.SetItemTooltip("Move selected trigger up.", (ImGuiHoveredFlags)0);
		ImGui.SameLine();
		if (ImGuiEx.IconButton((FontAwesomeIcon)61611, "movetriggerdown") && flag)
		{
			Trigger selectedTrigger2 = SelectedTrigger;
			if (selectedTrigger2 != null)
			{
				Plugin.Config.Triggers.RemoveAt(SelectedTriggerIndex);
				SelectedTriggerIndex = Math.Min(SelectedTriggerIndex + 1, Plugin.Config.Triggers.Count);
				Plugin.Config.Triggers.Insert(SelectedTriggerIndex, selectedTrigger2);
				Plugin.Config.Save();
			}
		}
		ImGuiEx.SetItemTooltip("Move selected trigger down.", (ImGuiHoveredFlags)0);
		ImGui.SameLine();
		ImGuiEx.IconButton((FontAwesomeIcon)62189, "removetrigger");
		ImGuiEx.SetItemTooltip("Remove selected trigger.", (ImGuiHoveredFlags)0);
		if (flag && ImGui.BeginPopupContextItem(ImU8String.op_Implicit("##removeContext"), (ImGuiPopupFlags)0))
		{
			if (ImGuiEx.IconSelectable((FontAwesomeIcon)62189))
			{
				Trigger? selectedTrigger3 = SelectedTrigger;
				if (selectedTrigger3 != null && selectedTrigger3.Type == TriggerType.Discord)
				{
					plugin.DiscordManager.DisconnectIfAllTriggersDisabled();
				}
				Plugin.Config.Triggers.RemoveAt(SelectedTriggerIndex);
				SelectedTriggerIndex = Math.Min(SelectedTriggerIndex, Plugin.Config.Triggers.Count - 1);
				Plugin.Config.Save();
			}
			ImGui.EndPopup();
		}
		ImGui.SameLine();
		if (ImGuiEx.IconButton((FontAwesomeIcon)61637, "copytrigger") && flag)
		{
			Trigger selectedTrigger4 = SelectedTrigger;
			ImGui.SetClipboardText(ImU8String.op_Implicit(CompressToBase64(JsonConvert.SerializeObject(selectedTrigger4, Plugin.Converters.ToArray()))));
		}
		ImGuiEx.SetItemTooltip("Copy the selected trigger to clipboard.", (ImGuiHoveredFlags)0);
		ImGui.SameLine();
		if (ImGuiEx.IconButton((FontAwesomeIcon)61674, "pastetrigger") && TryImportTrigger<Trigger>(ImGui.GetClipboardText().Trim(), out Trigger result) && result != null)
		{
			result.Guid = Guid.NewGuid();
			result.Enabled = false;
			result.UseSharedCounter = false;
			if (result.Counter is Counter counter)
			{
				counter.Amount = 0;
			}
			else
			{
				result.Counter = ((result.Type == TriggerType.Discord) ? ((CounterBase)new DiscordCounter()) : ((CounterBase)new Counter()));
			}
			Plugin.Config.Triggers.Add(result);
			Plugin.Config.Save();
		}
		ImGuiEx.SetItemTooltip("Paste trigger from clipboard.", (ImGuiHoveredFlags)0);
		ImGui.SameLine();
		if (ImGui.Checkbox(ImU8String.op_Implicit("Enable Plugin"), ref Plugin.Config.Enabled))
		{
			Plugin.Config.Save();
			if (!Plugin.Config.Enabled)
			{
				plugin.DiscordManager.DisconnectIfAllTriggersDisabled();
			}
		}
		ImGui.SameLine();
		if (ImGuiEx.IconButton((FontAwesomeIcon)61530, "openChangelog"))
		{
			((Window)plugin.UpdatesWindow).IsOpen = true;
		}
		ImGuiEx.SetItemTooltip("Open the TriggerPyon Changelog", (ImGuiHoveredFlags)0);
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

	private bool TryImportTrigger<Trigger>(string base64, out Trigger? result)
	{
		result = default(Trigger);
		if (string.IsNullOrWhiteSpace(base64))
		{
			return false;
		}
		string value;
		try
		{
			value = DecompressFromBase64(base64);
		}
		catch
		{
			return false;
		}
		try
		{
			Trigger val = JsonConvert.DeserializeObject<Trigger>(value, Plugin.Converters.ToArray());
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

	private void DrawTriggersList()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_0352: Unknown result type (might be due to invalid IL or missing references)
		//IL_0357: Unknown result type (might be due to invalid IL or missing references)
		//IL_0121: Unknown result type (might be due to invalid IL or missing references)
		//IL_0291: Unknown result type (might be due to invalid IL or missing references)
		bool flag = SelectedTrigger != null;
		ImGui.BeginChild(ImU8String.op_Implicit("TriggerPyonList"), new Vector2(140f * ImGuiHelpers.GlobalScale, 0f), false, (ImGuiWindowFlags)0);
		ImGui.BeginChild(ImU8String.op_Implicit("TriggerPyonEventList"), new Vector2(140f * ImGuiHelpers.GlobalScale, ImGui.GetContentRegionAvail().Y / 2f), true, (ImGuiWindowFlags)0);
		Vector4 dalamudViolet = ImGuiColors.DalamudViolet;
		ImGui.TextColored(ref dalamudViolet, ImU8String.op_Implicit("Emote/Text Triggers"));
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		if (Plugin.Config.Triggers.Count == 0)
		{
			dalamudViolet = ImGuiColors.DalamudRed;
			ImU8String val = default(ImU8String);
			((ImU8String)(ref val))._002Ector(21, 0);
			((ImU8String)(ref val)).AppendLiteral("No Triggers Available");
			ImGui.TextColored(ref dalamudViolet, val);
			ImGuiEx.SetItemTooltip("Click the + button above to create a new trigger.", (ImGuiHoveredFlags)0);
		}
		else
		{
			for (int i = 0; i < Plugin.Config.Triggers.Count; i++)
			{
				Trigger trigger = Plugin.Config.Triggers[i];
				if (trigger.Type != TriggerType.Discord)
				{
					bool enabled = trigger.Enabled;
					ImGui.PushID((IntPtr)i);
					ImGui.PushStyleColor((ImGuiCol)0, enabled ? 4282711876u : 4289374890u);
					if (ImGui.Selectable(ImU8String.op_Implicit(trigger.Name), SelectedTriggerIndex == i, (ImGuiSelectableFlags)0, default(Vector2)))
					{
						SelectedTriggerIndex = i;
					}
					ImGui.PopStyleColor();
					if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked((ImGuiMouseButton)0))
					{
						trigger.Enabled = !enabled;
						Plugin.Config.Save();
					}
					ImGui.PopID();
				}
			}
		}
		ImGui.EndChild();
		ImGui.Spacing();
		ImGui.Spacing();
		ImGui.BeginChild(ImU8String.op_Implicit("TriggerPyonDiscordList"), new Vector2(140f * ImGuiHelpers.GlobalScale, 0f), true, (ImGuiWindowFlags)0);
		dalamudViolet = ImGuiColors.DalamudViolet;
		ImGui.TextColored(ref dalamudViolet, ImU8String.op_Implicit("Discord Triggers"));
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		if (Plugin.Config.Triggers.Count != 0)
		{
			bool flag2 = plugin.DiscordManager.IsConnected && Plugin.Config.Discord.UserKey != string.Empty;
			for (int j = 0; j < Plugin.Config.Triggers.Count; j++)
			{
				Trigger trigger2 = Plugin.Config.Triggers[j];
				if (trigger2.Type != TriggerType.Discord)
				{
					continue;
				}
				bool enabled2 = trigger2.Enabled;
				ImGui.PushID((IntPtr)j);
				ImGui.PushStyleColor((ImGuiCol)0, (!enabled2) ? 4289374890u : (flag2 ? 4282711876u : 4281545727u));
				if (ImGui.Selectable(ImU8String.op_Implicit(trigger2.Name), SelectedTriggerIndex == j, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					SelectedTriggerIndex = j;
				}
				ImGui.PopStyleColor();
				if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked((ImGuiMouseButton)0))
				{
					trigger2.Enabled = !enabled2;
					Plugin.Config.Save();
					if (!trigger2.Enabled)
					{
						plugin.DiscordManager.DisconnectIfAllTriggersDisabled();
					}
				}
				ImGui.PopID();
			}
		}
		ImGui.EndChild();
		ImGui.EndChild();
		if (flag)
		{
			ImGui.SameLine();
			ImGui.BeginChild(ImU8String.op_Implicit("TriggerPyonEditor"), Vector2.Zero, true, (ImGuiWindowFlags)0);
			ImGuiIOPtr iO = ImGui.GetIO();
			ImGui.Dummy(new Vector2(0f, 18f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale));
			ImGui.Separator();
			DrawTriggerEditor(SelectedTrigger);
			ImGui.EndChild();
		}
	}

	private void DrawTriggerEditor(Trigger? trigger)
	{
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0166: Unknown result type (might be due to invalid IL or missing references)
		//IL_0183: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		if (trigger == null)
		{
			return;
		}
		if (ImGuiEx.Checkbox($"Enable Trigger##enable{SelectedTriggerIndex}", trigger.Enabled, delegate(bool x)
		{
			trigger.Enabled = x;
		}))
		{
			Plugin.Config.Save();
			if (trigger.Type == TriggerType.Discord && !trigger.Enabled)
			{
				plugin.DiscordManager.DisconnectIfAllTriggersDisabled();
			}
		}
		ImGuiEx.SetItemTooltip("Enable this trigger.", (ImGuiHoveredFlags)0);
		ImGui.SameLine();
		ImGuiIOPtr iO = ImGui.GetIO();
		ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
		if (ImGuiEx.InputTextWithHint("##triggerName", "Trigger Name", trigger.Name, delegate(string x)
		{
			trigger.Name = x;
		}, 64))
		{
			Plugin.Config.Save();
		}
		ImGuiEx.SetItemTooltip("Name which describes this trigger's function.", (ImGuiHoveredFlags)0);
		iO = ImGui.GetIO();
		ImGui.SetNextItemWidth(276f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
		if (ImGuiEx.InputTextWithHint("##triggerDesc", "Trigger Description", trigger.Description, delegate(string x)
		{
			trigger.Description = x;
		}, 500))
		{
			Plugin.Config.Save();
		}
		ImGuiEx.SetItemTooltip("Optional description to further detail this trigger's function.", (ImGuiHoveredFlags)0);
		ImGui.Spacing();
		Vector2 contentRegionAvail = ImGui.GetContentRegionAvail();
		ImU8String val = ImU8String.op_Implicit("##triggerEditor");
		Vector2 vector = contentRegionAvail;
		vector.X = contentRegionAvail.X * 1f;
		ChildDisposable val2 = ImRaii.Child(val, vector);
		try
		{
			DrawTriggerEvent(trigger);
			if (trigger.Type != TriggerType.Discord)
			{
				DrawTriggerInstigator(trigger);
				DrawTriggerReceiver(trigger);
				DrawTriggerCounter(trigger);
				DrawReactionOptions(trigger);
				DrawTriggerReactionQueue(trigger);
			}
			else
			{
				DrawTriggerDiscordSetup(trigger);
				DrawTriggerDiscordCounter(trigger);
			}
		}
		finally
		{
			((ChildDisposable)(ref val2)).Dispose();
		}
	}

	private void DrawTriggerEvent(Trigger trigger)
	{
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0060: Unknown result type (might be due to invalid IL or missing references)
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_019e: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0400: Unknown result type (might be due to invalid IL or missing references)
		//IL_0405: Unknown result type (might be due to invalid IL or missing references)
		//IL_0419: Unknown result type (might be due to invalid IL or missing references)
		//IL_044c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_0493: Unknown result type (might be due to invalid IL or missing references)
		//IL_0519: Unknown result type (might be due to invalid IL or missing references)
		//IL_051e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0557: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0604: Unknown result type (might be due to invalid IL or missing references)
		//IL_0618: Unknown result type (might be due to invalid IL or missing references)
		//IL_0622: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d4: Unknown result type (might be due to invalid IL or missing references)
		if (!ImGuiEx.TreeNode(((trigger.Type == TriggerType.None) ? "Trigger Event - Event Type must be set." : "Trigger Event") + "##triggerEvent", null, (trigger.Type == TriggerType.None) ? ImGuiColors.DalamudRed : default(Vector4), (ImGuiTreeNodeFlags)0))
		{
			return;
		}
		ImGuiIOPtr iO = ImGui.GetIO();
		ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
		ImU8String val = ImU8String.op_Implicit("##triggerType");
		ImU8String val2 = default(ImU8String);
		((ImU8String)(ref val2))._002Ector(12, 1);
		((ImU8String)(ref val2)).AppendLiteral("Event Type: ");
		((ImU8String)(ref val2)).AppendFormatted<TriggerType>(trigger.Type);
		if (ImGui.BeginCombo(val, val2, (ImGuiComboFlags)0))
		{
			foreach (TriggerType value in Enum.GetValues(typeof(TriggerType)))
			{
				bool flag = trigger.Type == value;
				ImU8String val3 = new ImU8String(0, 1);
				((ImU8String)(ref val3)).AppendFormatted<TriggerType>(value);
				if (ImGui.Selectable(val3, flag, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					trigger.Type = value;
					Plugin.Config.Save();
					plugin.DiscordManager.DisconnectIfAllTriggersDisabled();
				}
			}
			ImGui.EndCombo();
		}
		ImGuiEx.SetItemTooltip("Emote: An event which will conditionally trigger reactions when an instigator performs specific emotes.\nText: An event which will conditionally trigger reactions when an instigator's message contains specific words/phrases.\nDiscord: An event which will trigger title changes when Discord activity is updated.", (ImGuiHoveredFlags)0);
		ImGui.Spacing();
		if (trigger.Type == TriggerType.Emote)
		{
			if (!(trigger.ReceivedAction is EmoteAction))
			{
				trigger.ReceivedAction = new EmoteAction();
				Plugin.Config.Save();
			}
			EmoteAction action = (EmoteAction)trigger.ReceivedAction;
			ImGui.BeginDisabled(action.MatchAny);
			iO = ImGui.GetIO();
			ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
			if (ImGui.BeginCombo(ImU8String.op_Implicit("##triggerEmotes"), ImU8String.op_Implicit(action.MatchAny ? "All Emotes Selected" : $"{action.IDs.Count} Emotes Selected"), (ImGuiComboFlags)0))
			{
				if (!IsComboOpen_TriggerEmotes)
				{
					IsComboOpen_TriggerEmotes = true;
					plugin.Emotes = plugin.Emotes.OrderByDescending((Emote emote) => action.IDs.Contains(emote.ID)).ThenBy<Emote, string>((Emote emote) => emote.Name, StringComparer.OrdinalIgnoreCase).ToList();
				}
				foreach (Emote emote in plugin.Emotes)
				{
					if (!emote.TriggersEmoteHook)
					{
						continue;
					}
					bool flag2 = action.IDs.Contains(emote.ID);
					ImGuiEx.IconCheckbox(flag2);
					ImGui.SameLine();
					if (ImGui.Selectable(ImU8String.op_Implicit(emote.ToString()), flag2, (ImGuiSelectableFlags)1, default(Vector2)))
					{
						if (flag2)
						{
							action.IDs.Remove(emote.ID);
						}
						else
						{
							action.IDs.Add(emote.ID);
						}
						Plugin.Config.Save();
					}
				}
				ImGui.EndCombo();
			}
			else if (IsComboOpen_TriggerEmotes)
			{
				IsComboOpen_TriggerEmotes = false;
			}
			ImGui.EndDisabled();
			ImGuiEx.SetItemTooltip("Select the emotes that will trigger counter/reactions.", (ImGuiHoveredFlags)0);
			ImGui.SameLine();
			if (ImGuiEx.Checkbox("Any##matchAnyEmotes", action.MatchAny, delegate(bool x)
			{
				action.MatchAny = x;
			}))
			{
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Allow any emote to trigger counter/reactions.\nThis is useful if you'd like to mimic any received emote.", (ImGuiHoveredFlags)0);
		}
		else if (trigger.Type == TriggerType.Text)
		{
			if (!(trigger.ReceivedAction is TextAction))
			{
				trigger.ReceivedAction = new TextAction();
				Plugin.Config.Save();
			}
			TextAction action2 = (TextAction)trigger.ReceivedAction;
			iO = ImGui.GetIO();
			ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
			ImU8String val4 = ImU8String.op_Implicit("##triggerTexts");
			ImU8String val5 = default(ImU8String);
			((ImU8String)(ref val5))._002Ector(16, 1);
			((ImU8String)(ref val5)).AppendFormatted<int>(action2.Inputs.Count);
			((ImU8String)(ref val5)).AppendLiteral(" Inputs to Match");
			if (ImGui.BeginCombo(val4, val5, (ImGuiComboFlags)0))
			{
				Action action3 = null;
				ImU8String val6 = default(ImU8String);
				ImU8String val7 = default(ImU8String);
				for (int num = 0; num < action2.Inputs.Count; num++)
				{
					((ImU8String)(ref val6))._002Ector(10, 1);
					((ImU8String)(ref val6)).AppendLiteral("##textItem");
					((ImU8String)(ref val6)).AppendFormatted<int>(num);
					ImGui.PushID(val6);
					string current2 = action2.Inputs[num];
					if (ImGuiEx.IconButton((FontAwesomeIcon)61944, $"##removeText{num}"))
					{
						action3 = delegate
						{
							action2.Inputs.Remove(current2);
							Plugin.Config.Save();
						};
					}
					ImGuiEx.SetItemTooltip("Remove this entry.", (ImGuiHoveredFlags)0);
					ImGui.SameLine(0f, 0f);
					iO = ImGui.GetIO();
					ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale - ImGuiEx.GetIconButtonWidth((FontAwesomeIcon)61944));
					((ImU8String)(ref val7))._002Ector(11, 1);
					((ImU8String)(ref val7)).AppendLiteral("##textInput");
					((ImU8String)(ref val7)).AppendFormatted<int>(num);
					if (ImGui.InputText(val7, ref current2, 128, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null))
					{
						action2.Inputs[num] = current2;
						Plugin.Config.Save();
					}
					ImGui.PopID();
				}
				action3?.Invoke();
				ImGui.Separator();
				ImGui.PushID(ImU8String.op_Implicit("##newItem"));
				string text = "";
				ImGuiStylePtr style = ImGui.GetStyle();
				float num2 = 160f - ((ImGuiStylePtr)(ref style)).FramePadding.X * 2f;
				iO = ImGui.GetIO();
				ImGui.SetNextItemWidth(num2 * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
				if (ImGui.InputTextWithHint(ImU8String.op_Implicit("##newTextItem"), ImU8String.op_Implicit("New Input"), ref text, 128, (ImGuiInputTextFlags)32, (ImGuiInputTextCallbackDelegate)null) && !string.IsNullOrWhiteSpace(text))
				{
					text = text.Trim();
					if (!action2.Inputs.Contains(text))
					{
						action2.Inputs.Add(text);
						Plugin.Config.Save();
					}
				}
				ImGuiEx.SetItemTooltip("Add a new input to match for.\nPress the Enter key to confirm entry.", (ImGuiHoveredFlags)0);
				ImGui.PopID();
				ImGui.EndCombo();
			}
			ImGuiEx.SetItemTooltip("One or more words/phrases to match for in received chat messages that will trigger counter/reactions.", (ImGuiHoveredFlags)0);
			if (action2.Inputs.Count > 1)
			{
				ImGui.SameLine();
				if (ImGuiEx.Checkbox("All Required##matchAllText", action2.MatchAll, delegate(bool x)
				{
					action2.MatchAll = x;
				}))
				{
					Plugin.Config.Save();
				}
				ImGuiEx.SetItemTooltip("Require all inputs be present in a message to trigger counter/reactions.", (ImGuiHoveredFlags)0);
			}
			ImGui.SameLine();
			if (ImGuiEx.Checkbox("Case Sensitive##caseSensitiveText", action2.CaseSensitive, delegate(bool x)
			{
				action2.CaseSensitive = x;
			}))
			{
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Whether matched inputs are case sensitive.", (ImGuiHoveredFlags)0);
		}
		ImGui.TreePop();
	}

	private void DrawTriggerInstigator(Trigger trigger)
	{
		//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_026b: Unknown result type (might be due to invalid IL or missing references)
		//IL_05af: Unknown result type (might be due to invalid IL or missing references)
		//IL_05b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_05c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0609: Unknown result type (might be due to invalid IL or missing references)
		//IL_0306: Unknown result type (might be due to invalid IL or missing references)
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_031f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0356: Unknown result type (might be due to invalid IL or missing references)
		//IL_0146: Unknown result type (might be due to invalid IL or missing references)
		//IL_078d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0792: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_039c: Unknown result type (might be due to invalid IL or missing references)
		//IL_08f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_08fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_090e: Unknown result type (might be due to invalid IL or missing references)
		//IL_094e: Unknown result type (might be due to invalid IL or missing references)
		//IL_041c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0421: Unknown result type (might be due to invalid IL or missing references)
		//IL_045a: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04ee: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0508: Unknown result type (might be due to invalid IL or missing references)
		//IL_050d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0521: Unknown result type (might be due to invalid IL or missing references)
		//IL_052b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a73: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a78: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a8c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a96: Unknown result type (might be due to invalid IL or missing references)
		//IL_0677: Unknown result type (might be due to invalid IL or missing references)
		//IL_067c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0687: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c72: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c77: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c8b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cc2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0854: Unknown result type (might be due to invalid IL or missing references)
		//IL_0859: Unknown result type (might be due to invalid IL or missing references)
		//IL_0864: Unknown result type (might be due to invalid IL or missing references)
		//IL_09bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_09c1: Unknown result type (might be due to invalid IL or missing references)
		//IL_09cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d07: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d87: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d8c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dc5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e43: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e59: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e5e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e73: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e78: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e8c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e96: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b44: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b49: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b54: Unknown result type (might be due to invalid IL or missing references)
		Instigator? instigator = trigger.Instigator;
		string text = ((instigator != null && instigator.Type == PlayerType.None) ? "Event Instigator - Instigator Type must be set." : "Event Instigator") + "##eventInstigator";
		Instigator? instigator2 = trigger.Instigator;
		if (!ImGuiEx.TreeNode(text, null, (instigator2 != null && instigator2.Type == PlayerType.None) ? ImGuiColors.DalamudRed : default(Vector4), (ImGuiTreeNodeFlags)0))
		{
			return;
		}
		if (trigger.Instigator == null)
		{
			trigger.Instigator = new Instigator();
			Plugin.Config.Save();
		}
		ImGuiIOPtr iO = ImGui.GetIO();
		ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
		ImU8String val = ImU8String.op_Implicit("##instigatorType");
		ImU8String val2 = default(ImU8String);
		((ImU8String)(ref val2))._002Ector(6, 1);
		((ImU8String)(ref val2)).AppendLiteral("Type: ");
		((ImU8String)(ref val2)).AppendFormatted<PlayerType>(trigger.Instigator.Type);
		if (ImGui.BeginCombo(val, val2, (ImGuiComboFlags)0))
		{
			foreach (PlayerType value2 in Enum.GetValues(typeof(PlayerType)))
			{
				if (value2 != PlayerType.Ignore)
				{
					bool flag = trigger.Instigator.Type == value2;
					if (ImGui.Selectable(ImU8String.op_Implicit(value2.ToString()), flag, (ImGuiSelectableFlags)0, default(Vector2)))
					{
						trigger.Instigator.Type = value2;
						Plugin.Config.Save();
					}
				}
			}
			ImGui.EndCombo();
		}
		ImGuiEx.SetItemTooltip("Which type of instigating player can trigger this event.\n\nNone: Nobody can trigger this event.\nAll: Any player including yourself.\nOthers: Other players excluding yourself.\nSelf: Only you.\nPlayer: Only specific named player(s).\nTarget: Only your target.\nTargeter: Only players targeting you.", (ImGuiHoveredFlags)0);
		if (trigger.Type == TriggerType.Text)
		{
			ImGui.SameLine();
			if (trigger.Instigator.Type != PlayerType.Self && trigger.Instigator.Type != PlayerType.None && trigger.Instigator.Type != PlayerType.Target && trigger.Instigator.Type != PlayerType.Targeter)
			{
				if (ImGuiEx.Checkbox("Nearby##instigatorNearby", trigger.Instigator.RequireNearby, delegate(bool x)
				{
					trigger.Instigator.RequireNearby = x;
				}))
				{
					Plugin.Config.Save();
				}
				ImGuiEx.SetItemTooltip("Whether the player triggering this text event must be nearby (within object drawing distance).", (ImGuiHoveredFlags)0);
			}
			else
			{
				bool flag2 = true;
				ImGui.BeginDisabled(true);
				ImU8String val3 = default(ImU8String);
				((ImU8String)(ref val3))._002Ector(24, 0);
				((ImU8String)(ref val3)).AppendLiteral("Nearby##instigatorNearby");
				ImGui.Checkbox(val3, ref flag2);
				ImGui.EndDisabled();
				ImGuiEx.SetItemTooltip("Whether the player triggering this text event must be nearby (within object drawing distance).", (ImGuiHoveredFlags)128);
			}
		}
		ImGuiStylePtr style;
		if (trigger.Instigator.Type == PlayerType.All || trigger.Instigator.Type == PlayerType.Others || trigger.Instigator.Type == PlayerType.Target || trigger.Instigator.Type == PlayerType.Targeter || trigger.Instigator.Type == PlayerType.Player)
		{
			if (trigger.Instigator.Type == PlayerType.Player)
			{
				iO = ImGui.GetIO();
				ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
				ImU8String val4 = ImU8String.op_Implicit("##instigatorNames");
				ImU8String val5 = default(ImU8String);
				((ImU8String)(ref val5))._002Ector(15, 1);
				((ImU8String)(ref val5)).AppendFormatted<int>(trigger.Instigator.Names.Count);
				((ImU8String)(ref val5)).AppendLiteral(" Names to Match");
				if (ImGui.BeginCombo(val4, val5, (ImGuiComboFlags)0))
				{
					Action action = null;
					ImU8String val6 = default(ImU8String);
					ImU8String val7 = default(ImU8String);
					for (int num = 0; num < trigger.Instigator.Names.Count; num++)
					{
						((ImU8String)(ref val6))._002Ector(10, 1);
						((ImU8String)(ref val6)).AppendLiteral("##nameItem");
						((ImU8String)(ref val6)).AppendFormatted<int>(num);
						ImGui.PushID(val6);
						string current = trigger.Instigator.Names[num];
						if (ImGuiEx.IconButton((FontAwesomeIcon)61944, $"##removeName{num}"))
						{
							action = delegate
							{
								trigger.Instigator.Names.Remove(current);
								Plugin.Config.Save();
							};
						}
						ImGui.SameLine(0f, 0f);
						iO = ImGui.GetIO();
						ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale - ImGuiEx.GetIconButtonWidth((FontAwesomeIcon)61944));
						((ImU8String)(ref val7))._002Ector(11, 1);
						((ImU8String)(ref val7)).AppendLiteral("##nameInput");
						((ImU8String)(ref val7)).AppendFormatted<int>(num);
						if (ImGui.InputText(val7, ref current, 40, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null))
						{
							trigger.Instigator.Names[num] = current;
							Plugin.Config.Save();
						}
						ImGui.PopID();
					}
					action?.Invoke();
					ImGui.Separator();
					ImGui.PushID(ImU8String.op_Implicit("##newName"));
					string text2 = "";
					style = ImGui.GetStyle();
					float num2 = 160f - ((ImGuiStylePtr)(ref style)).FramePadding.X * 2f;
					iO = ImGui.GetIO();
					ImGui.SetNextItemWidth(num2 * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
					if (ImGui.InputTextWithHint(ImU8String.op_Implicit("##newNameItem"), ImU8String.op_Implicit("Player Name"), ref text2, 40, (ImGuiInputTextFlags)32, (ImGuiInputTextCallbackDelegate)null) && !string.IsNullOrWhiteSpace(text2))
					{
						text2 = text2.Trim();
						if (!trigger.Instigator.Names.Contains(text2))
						{
							trigger.Instigator.Names.Add(text2);
							Plugin.Config.Save();
						}
					}
					ImGuiEx.SetItemTooltip("Add a new player name to match for.\nPress the Enter key to confirm entry.", (ImGuiHoveredFlags)0);
					ImGui.PopID();
					ImGui.EndCombo();
				}
				ImGuiEx.SetItemTooltip("One or more player names that can trigger this event.\nNames are not case-sensitive and should not include @World.", (ImGuiHoveredFlags)0);
			}
			iO = ImGui.GetIO();
			ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
			ImU8String val8 = ImU8String.op_Implicit("##instigatorCondition");
			ImU8String val9 = default(ImU8String);
			((ImU8String)(ref val9))._002Ector(10, 1);
			((ImU8String)(ref val9)).AppendLiteral("Relation: ");
			((ImU8String)(ref val9)).AppendFormatted<string>(ImGuiEx.EnumToSelectedCountString(trigger.Instigator.Condition, "None", ""));
			if (ImGui.BeginCombo(val8, val9, (ImGuiComboFlags)0))
			{
				foreach (PlayerCondition value3 in Enum.GetValues(typeof(PlayerCondition)))
				{
					if (value3 == PlayerCondition.None)
					{
						continue;
					}
					bool flag3 = trigger.Instigator.Condition.HasFlag(value3);
					ImGuiEx.IconCheckbox(flag3);
					ImGui.SameLine();
					ImU8String val10 = new ImU8String(0, 1);
					((ImU8String)(ref val10)).AppendFormatted<PlayerCondition>(value3);
					if (ImGui.Selectable(val10, flag3, (ImGuiSelectableFlags)1, default(Vector2)))
					{
						if (flag3)
						{
							trigger.Instigator.Condition &= ~value3;
						}
						else
						{
							trigger.Instigator.Condition |= value3;
						}
						Plugin.Config.Save();
					}
				}
				ImGui.EndCombo();
			}
			ImGuiEx.SetItemTooltip("Restrict triggering of this event to players known to you.\nNone: No relation required.", (ImGuiHoveredFlags)0);
			if (!trigger.Instigator.RequireNearby && trigger.Instigator.Condition != PlayerCondition.None)
			{
				ImGui.SameLine();
				ImGuiEx.IconWarningTooltip("Relation condition can only be determined if the player is nearby.\nWith the above 'Nearby' option disabled, this event can still trigger without this condition being met when the player is not nearby.");
			}
			ImGui.SameLine();
			if (ImGuiEx.Checkbox("All Selected##instigatorAllConditions", trigger.Instigator.RequireAllConditions, delegate(bool x)
			{
				trigger.Instigator.RequireAllConditions = x;
			}))
			{
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Whether a player must have all selected relations.", (ImGuiHoveredFlags)0);
			iO = ImGui.GetIO();
			ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
			ImU8String val11 = ImU8String.op_Implicit("##instigatorGender");
			ImU8String val12 = default(ImU8String);
			((ImU8String)(ref val12))._002Ector(8, 1);
			((ImU8String)(ref val12)).AppendLiteral("Gender: ");
			((ImU8String)(ref val12)).AppendFormatted<string>(ImGuiEx.EnumToSelectedCountString(trigger.Instigator.Gender, "Any", "Any"));
			if (ImGui.BeginCombo(val11, val12, (ImGuiComboFlags)0))
			{
				foreach (GenderCondition value4 in Enum.GetValues(typeof(GenderCondition)))
				{
					if (value4 == GenderCondition.Any)
					{
						continue;
					}
					bool flag4 = trigger.Instigator.Gender.HasFlag(value4);
					ImGuiEx.IconCheckbox(flag4);
					ImGui.SameLine();
					ImU8String val13 = new ImU8String(0, 1);
					((ImU8String)(ref val13)).AppendFormatted<GenderCondition>(value4);
					if (ImGui.Selectable(val13, flag4, (ImGuiSelectableFlags)1, default(Vector2)))
					{
						if (flag4)
						{
							trigger.Instigator.Gender &= ~value4;
						}
						else
						{
							trigger.Instigator.Gender |= value4;
						}
						Plugin.Config.Save();
					}
				}
				ImGui.EndCombo();
			}
			ImGuiEx.SetItemTooltip("Restrict triggering of this event to players of specific gender.", (ImGuiHoveredFlags)0);
			iO = ImGui.GetIO();
			ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
			ImU8String val14 = ImU8String.op_Implicit("##instigatorRace");
			ImU8String val15 = default(ImU8String);
			((ImU8String)(ref val15))._002Ector(6, 1);
			((ImU8String)(ref val15)).AppendLiteral("Race: ");
			((ImU8String)(ref val15)).AppendFormatted<string>(ImGuiEx.EnumToSelectedCountString(trigger.Instigator.Race, "Any", "Any"));
			if (ImGui.BeginCombo(val14, val15, (ImGuiComboFlags)0))
			{
				foreach (RaceCondition value5 in Enum.GetValues(typeof(RaceCondition)))
				{
					if (value5 == RaceCondition.Any)
					{
						continue;
					}
					bool flag5 = trigger.Instigator.Race.HasFlag(value5);
					ImGuiEx.IconCheckbox(flag5);
					ImGui.SameLine();
					ImU8String val16 = new ImU8String(0, 1);
					((ImU8String)(ref val16)).AppendFormatted<RaceCondition>(value5);
					if (ImGui.Selectable(val16, flag5, (ImGuiSelectableFlags)1, default(Vector2)))
					{
						if (flag5)
						{
							trigger.Instigator.Race &= ~value5;
						}
						else
						{
							trigger.Instigator.Race |= value5;
						}
						Plugin.Config.Save();
					}
				}
				ImGui.EndCombo();
			}
			ImGuiEx.SetItemTooltip("Restrict triggering of this event to players of specific race.", (ImGuiHoveredFlags)0);
		}
		if (trigger.Instigator.Type != PlayerType.None)
		{
			iO = ImGui.GetIO();
			ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
			if (ImGui.BeginCombo(ImU8String.op_Implicit("##instigatorStatus"), ImU8String.op_Implicit("Status"), (ImGuiComboFlags)0))
			{
				foreach (StatusType item in from f in typeof(StatusType).GetFields(BindingFlags.Static | BindingFlags.Public)
					orderby f.MetadataToken
					select (StatusType)f.GetValue(null))
				{
					TriState triState = TriState.Ignored;
					if (trigger.Instigator.Status.TryGetValue(item, out var value))
					{
						triState = value;
					}
					ImGuiEx.IconTriState(triState);
					ImGui.SameLine();
					ImU8String val17 = new ImU8String(0, 1);
					((ImU8String)(ref val17)).AppendFormatted<StatusType>(item);
					if (ImGui.Selectable(val17, triState != TriState.Ignored, (ImGuiSelectableFlags)1, default(Vector2)))
					{
						TriState triState2 = ImGuiEx.NextTriState(triState);
						if (triState2 == TriState.Ignored)
						{
							trigger.Instigator.Status.Remove(item);
						}
						else
						{
							trigger.Instigator.Status[item] = triState2;
						}
						Plugin.Config.Save();
					}
				}
				ImGui.EndCombo();
			}
			ImGuiEx.SetItemTooltip("Restrict triggering of this event depending on instigator's status.\nCheckmark: A status they must have. (If multiple are checked, they only need 1 of them)\nCross: A status they must not have.", (ImGuiHoveredFlags)0);
			if (!trigger.Instigator.RequireNearby && trigger.Instigator.Type != PlayerType.Self && trigger.Instigator.Status.Count != 0)
			{
				ImGui.SameLine();
				ImGuiEx.IconWarningTooltip("Status condition can only be determined if the player is nearby.\nWith the above 'Nearby' option disabled, this event can still trigger without this condition being met when the player is not nearby.");
			}
		}
		if (trigger.Instigator.Type != PlayerType.Self && trigger.Instigator.Type != PlayerType.Ignore && trigger.Instigator.Type != PlayerType.None)
		{
			iO = ImGui.GetIO();
			ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
			ImU8String val18 = ImU8String.op_Implicit("##blacklistNames");
			ImU8String val19 = default(ImU8String);
			((ImU8String)(ref val19))._002Ector(18, 1);
			((ImU8String)(ref val19)).AppendFormatted<int>(trigger.Instigator.BlacklistNames.Count);
			((ImU8String)(ref val19)).AppendLiteral(" Blacklisted Names");
			if (ImGui.BeginCombo(val18, val19, (ImGuiComboFlags)0))
			{
				Action action2 = null;
				ImU8String val20 = default(ImU8String);
				ImU8String val21 = default(ImU8String);
				for (int num3 = 0; num3 < trigger.Instigator.BlacklistNames.Count; num3++)
				{
					((ImU8String)(ref val20))._002Ector(8, 1);
					((ImU8String)(ref val20)).AppendLiteral("##blItem");
					((ImU8String)(ref val20)).AppendFormatted<int>(num3);
					ImGui.PushID(val20);
					string current3 = trigger.Instigator.BlacklistNames[num3];
					if (ImGuiEx.IconButton((FontAwesomeIcon)61944, $"##removeblName{num3}"))
					{
						action2 = delegate
						{
							trigger.Instigator.BlacklistNames.Remove(current3);
							Plugin.Config.Save();
						};
					}
					ImGui.SameLine(0f, 0f);
					iO = ImGui.GetIO();
					ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale - ImGuiEx.GetIconButtonWidth((FontAwesomeIcon)61944));
					((ImU8String)(ref val21))._002Ector(13, 1);
					((ImU8String)(ref val21)).AppendLiteral("##blnameInput");
					((ImU8String)(ref val21)).AppendFormatted<int>(num3);
					if (ImGui.InputText(val21, ref current3, 40, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null))
					{
						trigger.Instigator.BlacklistNames[num3] = current3;
						Plugin.Config.Save();
					}
					ImGui.PopID();
				}
				action2?.Invoke();
				ImGui.Separator();
				ImGui.PushID(ImU8String.op_Implicit("##newBlName"));
				string text3 = "";
				style = ImGui.GetStyle();
				float num4 = 160f - ((ImGuiStylePtr)(ref style)).FramePadding.X * 2f;
				iO = ImGui.GetIO();
				ImGui.SetNextItemWidth(num4 * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
				if (ImGui.InputTextWithHint(ImU8String.op_Implicit("##newBlNameItem"), ImU8String.op_Implicit("Player Name"), ref text3, 40, (ImGuiInputTextFlags)32, (ImGuiInputTextCallbackDelegate)null) && !string.IsNullOrWhiteSpace(text3))
				{
					text3 = text3.Trim();
					if (!trigger.Instigator.BlacklistNames.Contains(text3))
					{
						trigger.Instigator.BlacklistNames.Add(text3);
						Plugin.Config.Save();
					}
				}
				ImGuiEx.SetItemTooltip("Add a new player name to blacklist.\nPress the Enter key to confirm entry.", (ImGuiHoveredFlags)0);
				ImGui.PopID();
				ImGui.EndCombo();
			}
			ImGuiEx.SetItemTooltip("A list of player names to forbid from triggering this event.\nNames are not case-sensitive and should not include @World.", (ImGuiHoveredFlags)0);
		}
		ImGui.TreePop();
	}

	private void DrawTriggerReceiver(Trigger trigger)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a56: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a5b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a6f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ac9: Unknown result type (might be due to invalid IL or missing references)
		//IL_043a: Unknown result type (might be due to invalid IL or missing references)
		//IL_043f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0453: Unknown result type (might be due to invalid IL or missing references)
		//IL_048f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01af: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c21: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c26: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c3a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c44: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0620: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0720: Unknown result type (might be due to invalid IL or missing references)
		//IL_0725: Unknown result type (might be due to invalid IL or missing references)
		//IL_0739: Unknown result type (might be due to invalid IL or missing references)
		//IL_0774: Unknown result type (might be due to invalid IL or missing references)
		//IL_0240: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0383: Unknown result type (might be due to invalid IL or missing references)
		//IL_0388: Unknown result type (might be due to invalid IL or missing references)
		//IL_039d: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0895: Unknown result type (might be due to invalid IL or missing references)
		//IL_089a: Unknown result type (might be due to invalid IL or missing references)
		//IL_08ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_08b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0508: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b44: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b49: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b54: Unknown result type (might be due to invalid IL or missing references)
		//IL_0689: Unknown result type (might be due to invalid IL or missing references)
		//IL_068e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0699: Unknown result type (might be due to invalid IL or missing references)
		//IL_07dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_07e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_07ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cf3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cfe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0961: Unknown result type (might be due to invalid IL or missing references)
		//IL_0966: Unknown result type (might be due to invalid IL or missing references)
		//IL_0971: Unknown result type (might be due to invalid IL or missing references)
		if (!ImGuiEx.TreeNode("Event Receiver##eventReceiver", null, default(Vector4), (ImGuiTreeNodeFlags)0))
		{
			return;
		}
		ImGuiIOPtr iO;
		if (trigger.Type == TriggerType.Emote)
		{
			if (!(trigger.Receiver is EmoteTargetReceiver))
			{
				trigger.Receiver = new EmoteTargetReceiver();
				Plugin.Config.Save();
			}
			EmoteTargetReceiver receiver = (EmoteTargetReceiver)trigger.Receiver;
			iO = ImGui.GetIO();
			ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
			ImU8String val = ImU8String.op_Implicit("##receiverType");
			ImU8String val2 = default(ImU8String);
			((ImU8String)(ref val2))._002Ector(6, 1);
			((ImU8String)(ref val2)).AppendLiteral("Type: ");
			((ImU8String)(ref val2)).AppendFormatted<PlayerType>(receiver.Type);
			if (ImGui.BeginCombo(val, val2, (ImGuiComboFlags)0))
			{
				foreach (PlayerType value3 in Enum.GetValues(typeof(PlayerType)))
				{
					if (value3 != PlayerType.Targeter)
					{
						bool flag = receiver.Type == value3;
						if (ImGui.Selectable(ImU8String.op_Implicit(value3.ToString()), flag, (ImGuiSelectableFlags)0, default(Vector2)))
						{
							receiver.Type = value3;
							Plugin.Config.Save();
						}
					}
				}
				ImGui.EndCombo();
			}
			ImGuiEx.SetItemTooltip("Which type of receiving player triggers this event.\n\nIgnore: No conditions for receiver of this event will be set.\nNone: Instigator must have no target.\nAll: Instigator must target any player.\nOthers: Instigator must target other players.\nSelf: Instigator must target you.\nPlayer: Instigator must target specific named player(s).\nTarget: Instigator must target your target.", (ImGuiHoveredFlags)0);
			if (receiver.Type == PlayerType.All || receiver.Type == PlayerType.Others || receiver.Type == PlayerType.Target || receiver.Type == PlayerType.Player)
			{
				if (receiver.Type == PlayerType.Player)
				{
					iO = ImGui.GetIO();
					ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
					ImU8String val3 = ImU8String.op_Implicit("##receiverNames");
					ImU8String val4 = default(ImU8String);
					((ImU8String)(ref val4))._002Ector(15, 1);
					((ImU8String)(ref val4)).AppendFormatted<int>(receiver.Names.Count);
					((ImU8String)(ref val4)).AppendLiteral(" Names to Match");
					if (ImGui.BeginCombo(val3, val4, (ImGuiComboFlags)0))
					{
						Action action = null;
						ImU8String val5 = default(ImU8String);
						ImU8String val6 = default(ImU8String);
						for (int i = 0; i < receiver.Names.Count; i++)
						{
							((ImU8String)(ref val5))._002Ector(10, 1);
							((ImU8String)(ref val5)).AppendLiteral("##nameItem");
							((ImU8String)(ref val5)).AppendFormatted<int>(i);
							ImGui.PushID(val5);
							string current = receiver.Names[i];
							if (ImGuiEx.IconButton((FontAwesomeIcon)61944, $"##removeName{i}"))
							{
								action = delegate
								{
									receiver.Names.Remove(current);
									Plugin.Config.Save();
								};
							}
							ImGui.SameLine(0f, 0f);
							iO = ImGui.GetIO();
							ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale - ImGuiEx.GetIconButtonWidth((FontAwesomeIcon)61944));
							((ImU8String)(ref val6))._002Ector(11, 1);
							((ImU8String)(ref val6)).AppendLiteral("##nameInput");
							((ImU8String)(ref val6)).AppendFormatted<int>(i);
							if (ImGui.InputText(val6, ref current, 40, (ImGuiInputTextFlags)0, (ImGuiInputTextCallbackDelegate)null))
							{
								receiver.Names[i] = current;
								Plugin.Config.Save();
							}
							ImGui.PopID();
						}
						action?.Invoke();
						ImGui.Separator();
						ImGui.PushID(ImU8String.op_Implicit("##newName"));
						string text = "";
						ImGuiStylePtr style = ImGui.GetStyle();
						float num = 160f - ((ImGuiStylePtr)(ref style)).FramePadding.X * 2f;
						iO = ImGui.GetIO();
						ImGui.SetNextItemWidth(num * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
						if (ImGui.InputTextWithHint(ImU8String.op_Implicit("##newNameItem"), ImU8String.op_Implicit("Player Name"), ref text, 40, (ImGuiInputTextFlags)32, (ImGuiInputTextCallbackDelegate)null) && !string.IsNullOrWhiteSpace(text))
						{
							text = text.Trim();
							if (!receiver.Names.Contains(text))
							{
								receiver.Names.Add(text);
								Plugin.Config.Save();
							}
						}
						ImGuiEx.SetItemTooltip("Add a new player name to match for.\nPress the Enter key to confirm entry.", (ImGuiHoveredFlags)0);
						ImGui.PopID();
						ImGui.EndCombo();
					}
					ImGuiEx.SetItemTooltip("One or more player names that can trigger this event (as instigator target).\nNames are not case-sensitive and should not include @World.", (ImGuiHoveredFlags)0);
				}
				iO = ImGui.GetIO();
				ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
				ImU8String val7 = ImU8String.op_Implicit("##receiverCondition");
				ImU8String val8 = default(ImU8String);
				((ImU8String)(ref val8))._002Ector(10, 1);
				((ImU8String)(ref val8)).AppendLiteral("Relation: ");
				((ImU8String)(ref val8)).AppendFormatted<string>(ImGuiEx.EnumToSelectedCountString(receiver.Condition, "None", ""));
				if (ImGui.BeginCombo(val7, val8, (ImGuiComboFlags)0))
				{
					foreach (PlayerCondition value4 in Enum.GetValues(typeof(PlayerCondition)))
					{
						if (value4 == PlayerCondition.None)
						{
							continue;
						}
						bool flag2 = receiver.Condition.HasFlag(value4);
						ImGuiEx.IconCheckbox(flag2);
						ImGui.SameLine();
						ImU8String val9 = new ImU8String(0, 1);
						((ImU8String)(ref val9)).AppendFormatted<PlayerCondition>(value4);
						if (ImGui.Selectable(val9, flag2, (ImGuiSelectableFlags)1, default(Vector2)))
						{
							if (flag2)
							{
								receiver.Condition &= ~value4;
							}
							else
							{
								receiver.Condition |= value4;
							}
							Plugin.Config.Save();
						}
					}
					ImGui.EndCombo();
				}
				ImGuiEx.SetItemTooltip("Restrict triggering of this event to players (instigator target) known to you.\nNone: No relation required.", (ImGuiHoveredFlags)0);
				ImGui.SameLine();
				if (ImGuiEx.Checkbox("All Selected##receiverAllConditions", receiver.RequireAllConditions, delegate(bool x)
				{
					receiver.RequireAllConditions = x;
				}))
				{
					Plugin.Config.Save();
				}
				ImGuiEx.SetItemTooltip("Whether a player must have all selected relations.", (ImGuiHoveredFlags)0);
				iO = ImGui.GetIO();
				ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
				ImU8String val10 = ImU8String.op_Implicit("##receiverGender");
				ImU8String val11 = default(ImU8String);
				((ImU8String)(ref val11))._002Ector(8, 1);
				((ImU8String)(ref val11)).AppendLiteral("Gender: ");
				((ImU8String)(ref val11)).AppendFormatted<string>(ImGuiEx.EnumToSelectedCountString(receiver.Gender, "Any", "Any"));
				if (ImGui.BeginCombo(val10, val11, (ImGuiComboFlags)0))
				{
					foreach (GenderCondition value5 in Enum.GetValues(typeof(GenderCondition)))
					{
						if (value5 == GenderCondition.Any)
						{
							continue;
						}
						bool flag3 = receiver.Gender.HasFlag(value5);
						ImGuiEx.IconCheckbox(flag3);
						ImGui.SameLine();
						ImU8String val12 = new ImU8String(0, 1);
						((ImU8String)(ref val12)).AppendFormatted<GenderCondition>(value5);
						if (ImGui.Selectable(val12, flag3, (ImGuiSelectableFlags)1, default(Vector2)))
						{
							if (flag3)
							{
								receiver.Gender &= ~value5;
							}
							else
							{
								receiver.Gender |= value5;
							}
							Plugin.Config.Save();
						}
					}
					ImGui.EndCombo();
				}
				ImGuiEx.SetItemTooltip("Restrict triggering of this event to players (instigator target) of specific gender.", (ImGuiHoveredFlags)0);
				iO = ImGui.GetIO();
				ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
				ImU8String val13 = ImU8String.op_Implicit("##receiverRace");
				ImU8String val14 = default(ImU8String);
				((ImU8String)(ref val14))._002Ector(6, 1);
				((ImU8String)(ref val14)).AppendLiteral("Race: ");
				((ImU8String)(ref val14)).AppendFormatted<string>(ImGuiEx.EnumToSelectedCountString(receiver.Race, "Any", "Any"));
				if (ImGui.BeginCombo(val13, val14, (ImGuiComboFlags)0))
				{
					foreach (RaceCondition value6 in Enum.GetValues(typeof(RaceCondition)))
					{
						if (value6 == RaceCondition.Any)
						{
							continue;
						}
						bool flag4 = receiver.Race.HasFlag(value6);
						ImGuiEx.IconCheckbox(flag4);
						ImGui.SameLine();
						ImU8String val15 = new ImU8String(0, 1);
						((ImU8String)(ref val15)).AppendFormatted<RaceCondition>(value6);
						if (ImGui.Selectable(val15, flag4, (ImGuiSelectableFlags)1, default(Vector2)))
						{
							if (flag4)
							{
								receiver.Race &= ~value6;
							}
							else
							{
								receiver.Race |= value6;
							}
							Plugin.Config.Save();
						}
					}
					ImGui.EndCombo();
				}
				ImGuiEx.SetItemTooltip("Restrict triggering of this event to players (instigator target) of specific race.", (ImGuiHoveredFlags)0);
			}
			if (receiver.Type != PlayerType.None && receiver.Type != PlayerType.Ignore)
			{
				iO = ImGui.GetIO();
				ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
				if (ImGui.BeginCombo(ImU8String.op_Implicit("##receiverStatus"), ImU8String.op_Implicit("Status"), (ImGuiComboFlags)0))
				{
					foreach (StatusType item in from f in typeof(StatusType).GetFields(BindingFlags.Static | BindingFlags.Public)
						orderby f.MetadataToken
						select (StatusType)f.GetValue(null))
					{
						TriState triState = TriState.Ignored;
						if (receiver.Status.TryGetValue(item, out var value))
						{
							triState = value;
						}
						ImGuiEx.IconTriState(triState);
						ImGui.SameLine();
						ImU8String val16 = new ImU8String(0, 1);
						((ImU8String)(ref val16)).AppendFormatted<StatusType>(item);
						if (ImGui.Selectable(val16, triState != TriState.Ignored, (ImGuiSelectableFlags)1, default(Vector2)))
						{
							TriState triState2 = ImGuiEx.NextTriState(triState);
							if (triState2 == TriState.Ignored)
							{
								receiver.Status.Remove(item);
							}
							else
							{
								receiver.Status[item] = triState2;
							}
							Plugin.Config.Save();
						}
					}
					ImGui.EndCombo();
				}
				ImGuiEx.SetItemTooltip("Restrict triggering of this event depending on receiver's status.\nCheckmark: A status they must have. (If multiple are checked, they only need 1 of them)\nCross: A status they must not have.", (ImGuiHoveredFlags)0);
			}
		}
		else if (trigger.Type == TriggerType.Text)
		{
			if (!(trigger.Receiver is ChannelTextReceiver))
			{
				trigger.Receiver = new ChannelTextReceiver();
				Plugin.Config.Save();
			}
			ChannelTextReceiver receiver2 = (ChannelTextReceiver)trigger.Receiver;
			ImGui.BeginDisabled(receiver2.MatchAny);
			iO = ImGui.GetIO();
			ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
			ImU8String val17 = ImU8String.op_Implicit("##textReceiverChannels");
			ImU8String val18 = default(ImU8String);
			((ImU8String)(ref val18))._002Ector(9, 1);
			((ImU8String)(ref val18)).AppendLiteral("Channel: ");
			((ImU8String)(ref val18)).AppendFormatted<string>(receiver2.MatchAny ? "Any" : $"{receiver2.Channel}");
			if (ImGui.BeginCombo(val17, val18, (ImGuiComboFlags)0))
			{
				foreach (ChatType value7 in Enum.GetValues(typeof(ChatType)))
				{
					if (value7 == ChatType.None || value7 == ChatType.Command || value7 == ChatType.Echo)
					{
						continue;
					}
					bool flag5 = receiver2.Channel.HasFlag(value7);
					ImGuiEx.IconCheckbox(flag5);
					ImGui.SameLine();
					ImU8String val19 = new ImU8String(0, 1);
					((ImU8String)(ref val19)).AppendFormatted<ChatType>(value7);
					if (ImGui.Selectable(val19, flag5, (ImGuiSelectableFlags)1, default(Vector2)))
					{
						if (flag5)
						{
							receiver2.Channel &= ~value7;
						}
						else
						{
							receiver2.Channel |= value7;
						}
						Plugin.Config.Save();
					}
				}
				ImGui.EndCombo();
			}
			ImGuiEx.SetItemTooltip("Restrict triggering of this event to specific chat channels.", (ImGuiHoveredFlags)0);
			ImGui.EndDisabled();
			ImGui.SameLine();
			if (ImGuiEx.Checkbox("Any##matchAnyChannels", receiver2.MatchAny, delegate(bool x)
			{
				receiver2.MatchAny = x;
			}))
			{
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Matched inputs received in any channel can trigger counter/reactions.", (ImGuiHoveredFlags)0);
			iO = ImGui.GetIO();
			ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
			if (ImGui.BeginCombo(ImU8String.op_Implicit("##receiverStatus"), ImU8String.op_Implicit("Status"), (ImGuiComboFlags)0))
			{
				foreach (StatusType item2 in from f in typeof(StatusType).GetFields(BindingFlags.Static | BindingFlags.Public)
					orderby f.MetadataToken
					select (StatusType)f.GetValue(null))
				{
					TriState triState3 = TriState.Ignored;
					if (receiver2.Status.TryGetValue(item2, out var value2))
					{
						triState3 = value2;
					}
					ImGuiEx.IconTriState(triState3);
					ImGui.SameLine();
					ImU8String val20 = new ImU8String(0, 1);
					((ImU8String)(ref val20)).AppendFormatted<StatusType>(item2);
					if (ImGui.Selectable(val20, triState3 != TriState.Ignored, (ImGuiSelectableFlags)1, default(Vector2)))
					{
						TriState triState4 = ImGuiEx.NextTriState(triState3);
						if (triState4 == TriState.Ignored)
						{
							receiver2.Status.Remove(item2);
						}
						else
						{
							receiver2.Status[item2] = triState4;
						}
						Plugin.Config.Save();
					}
				}
				ImGui.EndCombo();
			}
			ImGuiEx.SetItemTooltip("Restrict triggering of this event depending on your status.\nCheckmark: A status you must have. (If multiple are checked, you only need 1 of them)\nCross: A status you must not have.", (ImGuiHoveredFlags)0);
		}
		ImGui.TreePop();
	}

	private void DrawTriggerCounter(Trigger trigger)
	{
		//IL_029c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_026a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016a: Unknown result type (might be due to invalid IL or missing references)
		//IL_016f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0184: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_035c: Unknown result type (might be due to invalid IL or missing references)
		//IL_075f: Unknown result type (might be due to invalid IL or missing references)
		//IL_039e: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c82: Unknown result type (might be due to invalid IL or missing references)
		//IL_079b: Unknown result type (might be due to invalid IL or missing references)
		//IL_07a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0401: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cbe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cc3: Unknown result type (might be due to invalid IL or missing references)
		//IL_07f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_07fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_045a: Unknown result type (might be due to invalid IL or missing references)
		//IL_045f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d1c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d21: Unknown result type (might be due to invalid IL or missing references)
		//IL_0857: Unknown result type (might be due to invalid IL or missing references)
		//IL_085c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d7a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d7f: Unknown result type (might be due to invalid IL or missing references)
		//IL_08d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_08da: Unknown result type (might be due to invalid IL or missing references)
		//IL_0df8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dfd: Unknown result type (might be due to invalid IL or missing references)
		//IL_092c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0931: Unknown result type (might be due to invalid IL or missing references)
		//IL_0946: Unknown result type (might be due to invalid IL or missing references)
		//IL_0966: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a31: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a36: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a4b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a6b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b26: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b2b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b40: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b60: Unknown result type (might be due to invalid IL or missing references)
		//IL_065b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0660: Unknown result type (might be due to invalid IL or missing references)
		//IL_09b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0abc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bb1: Unknown result type (might be due to invalid IL or missing references)
		if (!ImGuiEx.TreeNode("Counter Reaction##counterReaction", null, default(Vector4), (ImGuiTreeNodeFlags)0))
		{
			return;
		}
		if (ImGuiEx.Checkbox("Use Shared Counter##useSharedCounter", trigger.UseSharedCounter, delegate(bool x)
		{
			trigger.UseSharedCounter = x;
		}))
		{
			Plugin.Config.Save();
		}
		ImGuiEx.SetItemTooltip("Whether this trigger should share a counter owned by another trigger.", (ImGuiHoveredFlags)0);
		if (!trigger.UseSharedCounter && !(trigger.Counter is Counter))
		{
			trigger.Counter = new Counter();
			Plugin.Config.Save();
		}
		else if (trigger.UseSharedCounter && !(trigger.Counter is SharedCounter))
		{
			trigger.Counter = new SharedCounter();
			Plugin.Config.Save();
		}
		Counter resolvedCounter = null;
		ImGuiIOPtr iO;
		if (trigger.UseSharedCounter)
		{
			SharedCounter shared = (trigger.Counter as SharedCounter) ?? new SharedCounter();
			List<Trigger> list = Plugin.Config.Triggers.Where((Trigger t) => t.Guid != trigger.Guid && t.Counter is Counter).ToList();
			if (list != null && list.Count > 0)
			{
				int num = list.FindIndex(delegate(Trigger t)
				{
					Guid guid = t.Guid;
					Guid? obj = shared?.TriggerGuid;
					return guid == obj;
				});
				ImGui.SameLine();
				iO = ImGui.GetIO();
				ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
				if (ImGui.BeginCombo(ImU8String.op_Implicit("##sharedCounterTriggers"), ImU8String.op_Implicit((num >= 0) ? list[num].Name : "None"), (ImGuiComboFlags)0))
				{
					for (int num2 = 0; num2 < list.Count; num2++)
					{
						if (ImGui.Selectable(ImU8String.op_Implicit(list[num2].Name), num2 == num, (ImGuiSelectableFlags)0, default(Vector2)))
						{
							shared.TriggerGuid = list[num2].Guid;
							Plugin.Config.Save();
						}
					}
					ImGui.EndCombo();
				}
				ImGuiEx.SetItemTooltip("Select the trigger whose counter should be shared by this one.", (ImGuiHoveredFlags)0);
				resolvedCounter = Plugin.Config.Triggers.FirstOrDefault(delegate(Trigger t)
				{
					Guid guid = t.Guid;
					Guid? triggerGuid = shared.TriggerGuid;
					return guid == triggerGuid;
				})?.Counter as Counter;
			}
			else
			{
				Vector4 dalamudRed = ImGuiColors.DalamudRed;
				ImGui.TextColored(ref dalamudRed, ImU8String.op_Implicit("No available triggers with counters to share from."));
			}
		}
		else
		{
			resolvedCounter = trigger.Counter as Counter;
		}
		if (resolvedCounter != null)
		{
			iO = ImGui.GetIO();
			ImGui.SetNextItemWidth(60f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
			if (ImGuiEx.DragInt("Current Count##counterAmount", resolvedCounter.Amount, delegate(int x)
			{
				resolvedCounter.Amount = x;
			}))
			{
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Number of times this counter has been triggered.", (ImGuiHoveredFlags)0);
			if (ImGuiEx.TreeNode("Honorific Title##counterTitle", null, default(Vector4), (ImGuiTreeNodeFlags)0))
			{
				if (ImGuiEx.Checkbox("Display Honorific Title##counterDisplayTitle", resolvedCounter.DisplayTitle, delegate(bool x)
				{
					resolvedCounter.DisplayTitle = x;
				}))
				{
					Plugin.Config.Save();
				}
				ImGuiEx.SetItemTooltip("Whether to display a title using Honorific plugin.", (ImGuiHoveredFlags)0);
				if (resolvedCounter.DisplayTitle)
				{
					ImGui.SameLine();
					if (ImGui.Button(ImU8String.op_Implicit("Preview##previewTitle"), default(Vector2)))
					{
						plugin.TriggerManager.PreviewTitle(trigger, resolvedCounter);
					}
					ImGuiEx.SetItemTooltip("Show a preview of the title.", (ImGuiHoveredFlags)0);
					iO = ImGui.GetIO();
					ImGui.SetNextItemWidth(40f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
					if (ImGuiEx.DragInt("##counterTitleMinFreq", resolvedCounter.TitleMinFreq, delegate(int x)
					{
						resolvedCounter.TitleMinFreq = x;
					}, 1f, 1))
					{
						Plugin.Config.Save();
					}
					ImGuiEx.SetItemTooltip("The minimum amount of times the event is triggered before displaying title.", (ImGuiHoveredFlags)0);
					ImGui.SameLine();
					iO = ImGui.GetIO();
					ImGui.SetNextItemWidth(40f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
					if (ImGuiEx.DragInt("##counterTitleMaxFreq", resolvedCounter.TitleMaxFreq, delegate(int x)
					{
						resolvedCounter.TitleMaxFreq = x;
					}, 1f, 1))
					{
						Plugin.Config.Save();
					}
					ImGuiEx.SetItemTooltip("The maximum amount of times the event is triggered before displaying title.", (ImGuiHoveredFlags)0);
					ImGui.SameLine();
					iO = ImGui.GetIO();
					ImGui.SetNextItemWidth(60f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
					if (ImGuiEx.DragInt("##counterTitleFreqThreshold", resolvedCounter.TitleFreqThreshold, delegate(int x)
					{
						resolvedCounter.TitleFreqThreshold = x;
					}, 1f, 1))
					{
						Plugin.Config.Save();
					}
					ImGuiEx.SetItemTooltip("The count threshold to be reached before the maximum is absolute.\nWhile the count is under this threshold, the title will display with a relative frequency between min/max rounded to nearest 5.", (ImGuiHoveredFlags)0);
					ImGui.SameLine();
					ImGuiEx.IconTextUnformatted((FontAwesomeIcon)61529);
					ImGuiEx.SetItemTooltip(resolvedCounter.GetTitleFreqText(), (ImGuiHoveredFlags)0);
					ImGui.Spacing();
					iO = ImGui.GetIO();
					ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
					if (ImGuiEx.DrawHonorificTitle(resolvedCounter))
					{
						Plugin.Config.Save();
					}
					ImGuiEx.SetItemTooltip("Honorific Title template with the below formatting:\n%n% - Counter\n%ifn%/%isn% - Instigator Forename/Surname\n%rfn%/%rsn% - Receiver Forename/Surname", (ImGuiHoveredFlags)0);
					if (resolvedCounter.TitleTemplate.Length > 24)
					{
						ImGui.SameLine();
						ImGuiEx.IconWarningTooltip($"Current raw title length is {resolvedCounter.TitleTemplate.Length} characters (before template replacements).\nHonorific will not display title if it's over 32 characters in length.");
					}
					ImGui.SameLine();
					if (ImGuiEx.Checkbox("##counterPrefix", resolvedCounter.TitlePrefix, delegate(bool x)
					{
						resolvedCounter.TitlePrefix = x;
					}))
					{
						Plugin.Config.Save();
					}
					ImGuiEx.SetItemTooltip("Prefix this title above your player name.", (ImGuiHoveredFlags)0);
					ImGui.SameLine();
					if (ImGuiEx.ColorPicker3("", "counterColour", resolvedCounter.TitleColour, delegate(Vector3 x)
					{
						resolvedCounter.TitleColour = x;
					}))
					{
						Plugin.Config.Save();
					}
					ImGuiEx.SetItemTooltip("Title text colour.", (ImGuiHoveredFlags)0);
					ImGui.SameLine();
					if (ImGuiEx.HonorificGlowPicker("", "counterGlow", resolvedCounter.TitleGlow, resolvedCounter.TitleGradientColorSet, resolvedCounter.TitleGradientAnimationStyle, delegate(Vector3 glow, int? set, GradientAnimationStyle? style)
					{
						resolvedCounter.TitleGlow = glow;
						resolvedCounter.TitleGradientColorSet = set;
						resolvedCounter.TitleGradientAnimationStyle = style;
					}))
					{
						Plugin.Config.Save();
					}
					ImGuiEx.SetItemTooltip("Title text glow.", (ImGuiHoveredFlags)0);
					ImGui.SameLine();
					iO = ImGui.GetIO();
					ImGui.SetNextItemWidth(60f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
					if (ImGuiEx.DragInt("##counterDuration", resolvedCounter.TitleDuration, delegate(int x)
					{
						resolvedCounter.TitleDuration = x;
					}, 100f))
					{
						Plugin.Config.Save();
					}
					ImGuiEx.SetItemTooltip("Duration in milliseconds that the title will be displayed for.\n" + $"A value of '0' will use the global counter duration of {Plugin.Config.CounterDuration}ms.", (ImGuiHoveredFlags)0);
				}
				ImGui.TreePop();
			}
			if (ImGuiEx.TreeNode("Toast Message##counterToast", null, default(Vector4), (ImGuiTreeNodeFlags)0))
			{
				if (ImGuiEx.Checkbox("Display Toast##counterDisplayToast", resolvedCounter.DisplayToast, delegate(bool x)
				{
					resolvedCounter.DisplayToast = x;
				}))
				{
					Plugin.Config.Save();
				}
				ImGuiEx.SetItemTooltip("Whether to display a toast message.", (ImGuiHoveredFlags)0);
				if (resolvedCounter.DisplayToast)
				{
					ImGui.SameLine();
					if (ImGui.Button(ImU8String.op_Implicit("Preview##previewToast"), default(Vector2)))
					{
						plugin.TriggerManager.PreviewToast(resolvedCounter);
					}
					ImGuiEx.SetItemTooltip("Show a preview of the toast message.", (ImGuiHoveredFlags)0);
					iO = ImGui.GetIO();
					ImGui.SetNextItemWidth(40f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
					if (ImGuiEx.DragInt("##counterToastMinFreq", resolvedCounter.ToastMinFreq, delegate(int x)
					{
						resolvedCounter.ToastMinFreq = x;
					}, 1f, 1))
					{
						Plugin.Config.Save();
					}
					ImGuiEx.SetItemTooltip("The minimum amount of times the event is triggered before displaying toast.", (ImGuiHoveredFlags)0);
					ImGui.SameLine();
					iO = ImGui.GetIO();
					ImGui.SetNextItemWidth(40f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
					if (ImGuiEx.DragInt("##counterToastMaxFreq", resolvedCounter.ToastMaxFreq, delegate(int x)
					{
						resolvedCounter.ToastMaxFreq = x;
					}, 1f, 1))
					{
						Plugin.Config.Save();
					}
					ImGuiEx.SetItemTooltip("The maximum amount of times the event is triggered before displaying toast.", (ImGuiHoveredFlags)0);
					ImGui.SameLine();
					iO = ImGui.GetIO();
					ImGui.SetNextItemWidth(60f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
					if (ImGuiEx.DragInt("##counterToastFreqThreshold", resolvedCounter.ToastFreqThreshold, delegate(int x)
					{
						resolvedCounter.ToastFreqThreshold = x;
					}, 1f, 1))
					{
						Plugin.Config.Save();
					}
					ImGuiEx.SetItemTooltip("The count threshold to be reached before the maximum is absolute.\nWhile the count is under this threshold, the toast will display with a relative frequency between min/max rounded to nearest 5.", (ImGuiHoveredFlags)0);
					ImGui.SameLine();
					ImGuiEx.IconTextUnformatted((FontAwesomeIcon)61529);
					ImGuiEx.SetItemTooltip(resolvedCounter.GetToastFreqText(), (ImGuiHoveredFlags)0);
					ImGui.Spacing();
					iO = ImGui.GetIO();
					ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
					if (ImGuiEx.InputText("##counterToastTemplate", resolvedCounter.ToastTemplate, delegate(string x)
					{
						resolvedCounter.ToastTemplate = x;
					}))
					{
						Plugin.Config.Save();
					}
					ImGuiEx.SetItemTooltip("Toast message template with the below formatting:\n%n% - Counter\n%ifn%/%isn% - Instigator Forename/Surname\n%rfn%/%rsn% - Receiver Forename/Surname", (ImGuiHoveredFlags)0);
					iO = ImGui.GetIO();
					ImGui.SetNextItemWidth(80f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
					ImU8String val = ImU8String.op_Implicit("##toastType");
					ImU8String val2 = default(ImU8String);
					((ImU8String)(ref val2))._002Ector(0, 1);
					((ImU8String)(ref val2)).AppendFormatted<ToastDisplayType>(resolvedCounter.ToastDisplayType);
					if (ImGui.BeginCombo(val, val2, (ImGuiComboFlags)0))
					{
						foreach (ToastDisplayType value in Enum.GetValues(typeof(ToastDisplayType)))
						{
							bool flag = resolvedCounter.ToastDisplayType == value;
							if (ImGui.Selectable(ImU8String.op_Implicit(value.ToString()), flag, (ImGuiSelectableFlags)0, default(Vector2)))
							{
								resolvedCounter.ToastDisplayType = value;
								Plugin.Config.Save();
							}
						}
						ImGui.EndCombo();
					}
					ImGuiEx.SetItemTooltip("Toast Display Type", (ImGuiHoveredFlags)0);
					if (resolvedCounter.ToastDisplayType == ToastDisplayType.Normal)
					{
						ImGui.SameLine();
						iO = ImGui.GetIO();
						ImGui.SetNextItemWidth(80f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
						ImU8String val3 = ImU8String.op_Implicit("##toastSpeed");
						ImU8String val4 = default(ImU8String);
						((ImU8String)(ref val4))._002Ector(0, 1);
						((ImU8String)(ref val4)).AppendFormatted<ToastDisplaySpeed>(resolvedCounter.ToastDisplaySpeed);
						if (ImGui.BeginCombo(val3, val4, (ImGuiComboFlags)0))
						{
							foreach (ToastDisplaySpeed value2 in Enum.GetValues(typeof(ToastDisplaySpeed)))
							{
								bool flag2 = resolvedCounter.ToastDisplaySpeed == value2;
								if (ImGui.Selectable(ImU8String.op_Implicit(value2.ToString()), flag2, (ImGuiSelectableFlags)0, default(Vector2)))
								{
									resolvedCounter.ToastDisplaySpeed = value2;
									Plugin.Config.Save();
								}
							}
							ImGui.EndCombo();
						}
						ImGuiEx.SetItemTooltip("Toast Display Speed\n(Only available for Normal toasts)", (ImGuiHoveredFlags)0);
						ImGui.SameLine();
						iO = ImGui.GetIO();
						ImGui.SetNextItemWidth(80f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
						ImU8String val5 = ImU8String.op_Implicit("##toastPosition");
						ImU8String val6 = default(ImU8String);
						((ImU8String)(ref val6))._002Ector(0, 1);
						((ImU8String)(ref val6)).AppendFormatted<ToastDisplayPosition>(resolvedCounter.ToastDisplayPosition);
						if (ImGui.BeginCombo(val5, val6, (ImGuiComboFlags)0))
						{
							foreach (ToastDisplayPosition value3 in Enum.GetValues(typeof(ToastDisplayPosition)))
							{
								bool flag3 = resolvedCounter.ToastDisplayPosition == value3;
								if (ImGui.Selectable(ImU8String.op_Implicit(value3.ToString()), flag3, (ImGuiSelectableFlags)0, default(Vector2)))
								{
									resolvedCounter.ToastDisplayPosition = value3;
									Plugin.Config.Save();
								}
							}
							ImGui.EndCombo();
						}
						ImGuiEx.SetItemTooltip("Toast Display Position\n(Only available for Normal toasts)", (ImGuiHoveredFlags)0);
					}
				}
				ImGui.TreePop();
			}
			if (ImGuiEx.TreeNode("Echo Chat##counterEcho", null, default(Vector4), (ImGuiTreeNodeFlags)0))
			{
				if (ImGuiEx.Checkbox("Output Echo##counterDisplayEcho", resolvedCounter.DisplayEcho, delegate(bool x)
				{
					resolvedCounter.DisplayEcho = x;
				}))
				{
					Plugin.Config.Save();
				}
				ImGuiEx.SetItemTooltip("Whether to output an echo message.", (ImGuiHoveredFlags)0);
				if (resolvedCounter.DisplayEcho)
				{
					ImGui.SameLine();
					if (ImGui.Button(ImU8String.op_Implicit("Preview##previewEcho"), default(Vector2)))
					{
						plugin.TriggerManager.PreviewEcho(resolvedCounter);
					}
					ImGuiEx.SetItemTooltip("Show a preview of the echo message.", (ImGuiHoveredFlags)0);
					iO = ImGui.GetIO();
					ImGui.SetNextItemWidth(40f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
					if (ImGuiEx.DragInt("##counterEchoMinFreq", resolvedCounter.EchoMinFreq, delegate(int x)
					{
						resolvedCounter.EchoMinFreq = x;
					}, 1f, 1))
					{
						Plugin.Config.Save();
					}
					ImGuiEx.SetItemTooltip("The minimum amount of times the event is triggered before outputting echo message.", (ImGuiHoveredFlags)0);
					ImGui.SameLine();
					iO = ImGui.GetIO();
					ImGui.SetNextItemWidth(40f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
					if (ImGuiEx.DragInt("##counterEchoMaxFreq", resolvedCounter.EchoMaxFreq, delegate(int x)
					{
						resolvedCounter.EchoMaxFreq = x;
					}, 1f, 1))
					{
						Plugin.Config.Save();
					}
					ImGuiEx.SetItemTooltip("The maximum amount of times the event is triggered before outputting echo message.", (ImGuiHoveredFlags)0);
					ImGui.SameLine();
					iO = ImGui.GetIO();
					ImGui.SetNextItemWidth(60f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
					if (ImGuiEx.DragInt("##counterEchoFreqThreshold", resolvedCounter.EchoFreqThreshold, delegate(int x)
					{
						resolvedCounter.EchoFreqThreshold = x;
					}, 1f, 1))
					{
						Plugin.Config.Save();
					}
					ImGuiEx.SetItemTooltip("The count threshold to be reached before the maximum is absolute.\nWhile the count is under this threshold, the echo message will output with a relative frequency between min/max rounded to nearest 5.", (ImGuiHoveredFlags)0);
					ImGui.SameLine();
					ImGuiEx.IconTextUnformatted((FontAwesomeIcon)61529);
					ImGuiEx.SetItemTooltip(resolvedCounter.GetEchoFreqText(), (ImGuiHoveredFlags)0);
					ImGui.Spacing();
					iO = ImGui.GetIO();
					ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
					if (ImGuiEx.InputText("##counterEchoTemplate", resolvedCounter.EchoTemplate, delegate(string x)
					{
						resolvedCounter.EchoTemplate = x;
					}))
					{
						Plugin.Config.Save();
					}
					ImGuiEx.SetItemTooltip("Echo message template with the below formatting:\n%n% - Counter\n%ifn%/%isn% - Instigator Forename/Surname\n%rfn%/%rsn% - Receiver Forename/Surname", (ImGuiHoveredFlags)0);
				}
				ImGui.TreePop();
			}
		}
		ImGui.TreePop();
	}

	private void DrawReactionOptions(Trigger trigger)
	{
		//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_0247: Unknown result type (might be due to invalid IL or missing references)
		//IL_024c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0260: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_03af: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_01dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0597: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_031e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0629: Unknown result type (might be due to invalid IL or missing references)
		//IL_062e: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d9: Unknown result type (might be due to invalid IL or missing references)
		//IL_05de: Unknown result type (might be due to invalid IL or missing references)
		//IL_0477: Unknown result type (might be due to invalid IL or missing references)
		//IL_047c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0487: Unknown result type (might be due to invalid IL or missing references)
		//IL_0683: Unknown result type (might be due to invalid IL or missing references)
		//IL_0688: Unknown result type (might be due to invalid IL or missing references)
		//IL_0991: Unknown result type (might be due to invalid IL or missing references)
		//IL_0996: Unknown result type (might be due to invalid IL or missing references)
		//IL_09af: Unknown result type (might be due to invalid IL or missing references)
		//IL_09b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_09b8: Unknown result type (might be due to invalid IL or missing references)
		//IL_09bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b31: Unknown result type (might be due to invalid IL or missing references)
		//IL_07af: Unknown result type (might be due to invalid IL or missing references)
		//IL_07b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0805: Unknown result type (might be due to invalid IL or missing references)
		//IL_080a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b89: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b8e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0be5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0bec: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c01: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d6a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d6f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e91: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e96: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fb8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0fbd: Unknown result type (might be due to invalid IL or missing references)
		//IL_1173: Unknown result type (might be due to invalid IL or missing references)
		//IL_1178: Unknown result type (might be due to invalid IL or missing references)
		//IL_129d: Unknown result type (might be due to invalid IL or missing references)
		//IL_12a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dfb: Unknown result type (might be due to invalid IL or missing references)
		//IL_0e00: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f22: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f27: Unknown result type (might be due to invalid IL or missing references)
		//IL_1049: Unknown result type (might be due to invalid IL or missing references)
		//IL_104e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1204: Unknown result type (might be due to invalid IL or missing references)
		//IL_1209: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c57: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c5c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c88: Unknown result type (might be due to invalid IL or missing references)
		//IL_10da: Unknown result type (might be due to invalid IL or missing references)
		//IL_10df: Unknown result type (might be due to invalid IL or missing references)
		if (!ImGuiEx.TreeNode("Reaction Options##reactionOptions", null, default(Vector4), (ImGuiTreeNodeFlags)0))
		{
			return;
		}
		if (trigger.ReactionOptions == null)
		{
			trigger.ReactionOptions = new ReactionOptions();
			Plugin.Config.Save();
		}
		else
		{
			if (ImGuiEx.Checkbox("Passthrough Restrictions##passthroughRestrictions", trigger.ReactionOptions.PassthroughRestrictions, delegate(bool x)
			{
				trigger.ReactionOptions.PassthroughRestrictions = x;
			}))
			{
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("If the below state/range restrictions prevent the reaction queue from performing,\n this option will abort triggering of this event, allowing any similar lower priority event to trigger instead.", (ImGuiHoveredFlags)0);
			ImGui.SameLine();
			if (ImGuiEx.Checkbox("Count Failed Conditions##countFailedConditions", trigger.ReactionOptions.CountFailedConditions, delegate(bool x)
			{
				trigger.ReactionOptions.CountFailedConditions = x;
			}))
			{
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("If any of the below conditions prevent the reaction queue from performing,\n this option will allow the counter to increment & display title (if any) regardless.", (ImGuiHoveredFlags)0);
			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();
			ImGuiIOPtr iO = ImGui.GetIO();
			ImGui.SetNextItemWidth(60f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
			if (ImGuiEx.DragInt("Reaction Cooldown##reactionCooldown", trigger.ReactionOptions.ReactionCooldown, delegate(int x)
			{
				trigger.ReactionOptions.ReactionCooldown = x;
			}))
			{
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Cooldown in milliseconds for how frequent the below reactions can be triggered by this event.\nIf the event is triggered while on cooldown, reactions will be skipped but any counter attached to this event will still increment.", (ImGuiHoveredFlags)0);
			iO = ImGui.GetIO();
			ImGui.SetNextItemWidth(60f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
			ImU8String val = ImU8String.op_Implicit("Interrupt Behaviour##interruptType");
			ImU8String val2 = default(ImU8String);
			((ImU8String)(ref val2))._002Ector(0, 1);
			((ImU8String)(ref val2)).AppendFormatted<ReactionInterruptType>(trigger.ReactionOptions.InterruptType);
			if (ImGui.BeginCombo(val, val2, (ImGuiComboFlags)0))
			{
				foreach (ReactionInterruptType value2 in Enum.GetValues(typeof(ReactionInterruptType)))
				{
					bool flag = trigger.ReactionOptions.InterruptType == value2;
					if (ImGui.Selectable(ImU8String.op_Implicit(value2.ToString()), flag, (ImGuiSelectableFlags)0, default(Vector2)))
					{
						trigger.ReactionOptions.InterruptType = value2;
						Plugin.Config.Save();
					}
				}
				ImGui.EndCombo();
			}
			ImGuiEx.SetItemTooltip("Determines the behaviour for interrupting reaction queue when another event is triggered.\n\nNone: No triggers can interrupt this reaction queue.\nAny: Any triggers can interrupt this reaction queue.\nSame: Only same trigger can interrupt this reaction queue.\nOther: Only other triggers can interrupt this reaction queue.", (ImGuiHoveredFlags)0);
			iO = ImGui.GetIO();
			ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
			ImU8String val3 = ImU8String.op_Implicit("Restricted States##stateConditions");
			ImU8String val4 = default(ImU8String);
			((ImU8String)(ref val4))._002Ector(7, 1);
			((ImU8String)(ref val4)).AppendLiteral("State: ");
			((ImU8String)(ref val4)).AppendFormatted<string>(ImGuiEx.EnumToSelectedCountString(trigger.ReactionOptions.StateConditions, "None", ""));
			if (ImGui.BeginCombo(val3, val4, (ImGuiComboFlags)0))
			{
				foreach (StateConditionType value3 in Enum.GetValues(typeof(StateConditionType)))
				{
					if (value3 == StateConditionType.None)
					{
						continue;
					}
					bool flag2 = trigger.ReactionOptions.StateConditions.HasFlag(value3);
					ImGuiEx.IconCheckbox(flag2);
					ImGui.SameLine();
					ImU8String val5 = new ImU8String(0, 1);
					((ImU8String)(ref val5)).AppendFormatted<StateConditionType>(value3);
					if (ImGui.Selectable(val5, flag2, (ImGuiSelectableFlags)1, default(Vector2)))
					{
						if (flag2)
						{
							trigger.ReactionOptions.StateConditions &= ~value3;
						}
						else
						{
							trigger.ReactionOptions.StateConditions |= value3;
						}
						Plugin.Config.Save();
					}
				}
				ImGui.EndCombo();
			}
			ImGuiEx.SetItemTooltip("Prevent performing this reaction queue under specific player states:\n\nMoving: Prevent when you're moving.\nStanding: Prevent when you're standing idle.\nGroundSit: Prevent when you're sitting on ground.\nChairSit: Prevent when you're sitting on chair.\nSleeping: Prevent when you're sleeping.\nEmote: Prevent when you're performing a standard emote.\nLoopingEmote: Prevent when you're performing a looping emote (eg. dancing).", (ImGuiHoveredFlags)0);
			iO = ImGui.GetIO();
			ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
			ImU8String val6 = ImU8String.op_Implicit("State Restoration##restoreType");
			ImU8String val7 = default(ImU8String);
			((ImU8String)(ref val7))._002Ector(9, 1);
			((ImU8String)(ref val7)).AppendLiteral("Restore: ");
			((ImU8String)(ref val7)).AppendFormatted<string>(ImGuiEx.EnumToSelectedCountString(trigger.ReactionOptions.RestoreType, "None", ""));
			if (ImGui.BeginCombo(val6, val7, (ImGuiComboFlags)0))
			{
				foreach (RestoreType value4 in Enum.GetValues(typeof(RestoreType)))
				{
					if (value4 == RestoreType.None)
					{
						continue;
					}
					bool flag3 = trigger.ReactionOptions.RestoreType.HasFlag(value4);
					ImGuiEx.IconCheckbox(flag3);
					ImGui.SameLine();
					ImU8String val8 = new ImU8String(0, 1);
					((ImU8String)(ref val8)).AppendFormatted<RestoreType>(value4);
					if (ImGui.Selectable(val8, flag3, (ImGuiSelectableFlags)1, default(Vector2)))
					{
						if (flag3)
						{
							trigger.ReactionOptions.RestoreType &= ~value4;
						}
						else
						{
							trigger.ReactionOptions.RestoreType |= value4;
						}
						Plugin.Config.Save();
					}
				}
				ImGui.EndCombo();
			}
			ImGuiEx.SetItemTooltip("Determines which properties to restore when reaction queue ends.\n\nEmote: Restore looping emote (like dances/sit/sleep) if you were performing any prior to this event.\nTarget: Restore target if any reactions caused changes to it.\nRotation/Position: Restore character rotation/position if any reactions caused changes to them.", (ImGuiHoveredFlags)0);
			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();
			if (ImGuiEx.Checkbox("Restrict Range##restrictRange", trigger.ReactionOptions.RestrictRange, delegate(bool x)
			{
				trigger.ReactionOptions.RestrictRange = x;
			}))
			{
				Plugin.Config.Save();
				if (!trigger.ReactionOptions.RestrictRange)
				{
					DrawRangePreview = false;
				}
			}
			ImGuiEx.SetItemTooltip("Whether reactions will only be performed if the instigator is within a specified range relative to you.\nIf you are the instigator, this can be the receiver's position relative to you instead.\n\nIf the reaction queue is empty, this condition can still determine whether to trigger the counter if 'Count Failed Conditions' is disabled.", (ImGuiHoveredFlags)0);
			if (trigger.ReactionOptions.RestrictRange)
			{
				ImGui.SameLine();
				if (ImGui.Button(ImU8String.op_Implicit("Preview##previewRange"), default(Vector2)))
				{
					DrawRangePreview = !DrawRangePreview;
				}
				ImGuiEx.SetItemTooltip("Toggle previewing the reaction range around your character.", (ImGuiHoveredFlags)0);
				if (DrawRangePreview)
				{
					ImGui.SameLine();
					iO = ImGui.GetIO();
					ImGui.SetNextItemWidth(80f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
					ImGuiEx.DragFloat("Opacity##opacity", RangePreviewOpacity, delegate(float x)
					{
						RangePreviewOpacity = x;
					}, 0.01f, 0.05f, 1f);
					ImGuiEx.SetItemTooltip("Opacity of the drawn preview region.", (ImGuiHoveredFlags)0);
				}
				iO = ImGui.GetIO();
				ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
				if (ImGuiEx.DragFloat("Min Distance##minDistance", trigger.ReactionOptions.RestrictedDistanceMin, delegate(float x)
				{
					trigger.ReactionOptions.RestrictedDistanceMin = x;
				}, 0.01f, 0f, 99.99f))
				{
					Plugin.Config.Save();
				}
				iO = ImGui.GetIO();
				ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
				if (ImGuiEx.DragFloat("Max Distance##maxDistance", trigger.ReactionOptions.RestrictedDistanceMax, delegate(float x)
				{
					trigger.ReactionOptions.RestrictedDistanceMax = x;
				}, 0.01f, 0.01f, 100f))
				{
					Plugin.Config.Save();
				}
				if (trigger.ReactionOptions.RestrictedDistanceMin > trigger.ReactionOptions.RestrictedDistanceMax)
				{
					trigger.ReactionOptions.RestrictedDistanceMin = trigger.ReactionOptions.RestrictedDistanceMax - 0.01f;
					Plugin.Config.Save();
				}
				if (trigger.ReactionOptions.RestrictedDistanceMax < trigger.ReactionOptions.RestrictedDistanceMin)
				{
					trigger.ReactionOptions.RestrictedDistanceMax = trigger.ReactionOptions.RestrictedDistanceMin + 0.01f;
					Plugin.Config.Save();
				}
				if (DrawRangePreview && trigger.ReactionOptions.RestrictedDistanceMax > 4f)
				{
					ImGui.SameLine();
					ImGuiEx.IconWarningTooltip("Preview may not display correctly if max distance exceeds camera region.");
				}
				iO = ImGui.GetIO();
				ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
				if (ImGuiEx.DragInt("Angle Direction##angleDirection", trigger.ReactionOptions.RestrictedAngleDirection, delegate(int x)
				{
					trigger.ReactionOptions.RestrictedAngleDirection = x;
				}, 1f, 0, 360))
				{
					Plugin.Config.Save();
				}
				iO = ImGui.GetIO();
				ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
				if (ImGuiEx.DragFloat("Angle Area##angleArea", trigger.ReactionOptions.RestrictedAngleArea, delegate(float x)
				{
					trigger.ReactionOptions.RestrictedAngleArea = x;
				}, 0.01f, 0f, 1f))
				{
					Plugin.Config.Save();
				}
				if (DrawRangePreview)
				{
					DrawReactionRangePreview(trigger.ReactionOptions);
				}
			}
			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();
			if (ImGuiEx.Checkbox("Restrict Territory##restrictTerritory", trigger.ReactionOptions.RestrictTerritory, delegate(bool x)
			{
				trigger.ReactionOptions.RestrictTerritory = x;
			}))
			{
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Whether reactions will only be performed if you are in a specific territory.", (ImGuiHoveredFlags)0);
			if (trigger.ReactionOptions.RestrictTerritory)
			{
				if (TerritoryUiList == null)
				{
					BuildTerritoryUiList();
				}
				ImGui.SameLine();
				if (ImGuiEx.IconButton((FontAwesomeIcon)61525, "newTerritory"))
				{
					ReactionOptions reactionOptions = trigger.ReactionOptions;
					if (reactionOptions.AllowedTerritories == null)
					{
						List<Territory> list = (reactionOptions.AllowedTerritories = new List<Territory>());
					}
					trigger.ReactionOptions.AllowedTerritories.Add(new Territory());
					Plugin.Config.Save();
				}
				ImGuiEx.SetItemTooltip("Add new territory.", (ImGuiHoveredFlags)0);
				if (plugin.TryGetCurrentTerritory(out var res))
				{
					ImGui.SameLine();
					ImGuiEx.IconTextUnformatted((FontAwesomeIcon)61530);
					DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(33, 5);
					defaultInterpolatedStringHandler.AppendLiteral("Current Territory\n");
					defaultInterpolatedStringHandler.AppendLiteral("Name (Id): ");
					PlaceName? valueNullable = ((TerritoryType)(ref res)).PlaceName.ValueNullable;
					object value;
					if (!valueNullable.HasValue)
					{
						value = null;
					}
					else
					{
						PlaceName valueOrDefault = valueNullable.GetValueOrDefault();
						ReadOnlySeString name = ((PlaceName)(ref valueOrDefault)).Name;
						value = ((ReadOnlySeString)(ref name)).ExtractText();
					}
					defaultInterpolatedStringHandler.AppendFormatted((string?)value);
					defaultInterpolatedStringHandler.AppendLiteral(" (");
					defaultInterpolatedStringHandler.AppendFormatted(((TerritoryType)(ref res)).RowId);
					defaultInterpolatedStringHandler.AppendLiteral(")\n");
					defaultInterpolatedStringHandler.AppendFormatted(PlayerManager.IsInWard ? $"Ward: {PlayerManager.CurrentWard}\n" : "");
					defaultInterpolatedStringHandler.AppendFormatted(PlayerManager.IsInPlot ? $"Plot: {PlayerManager.CurrentPlot}\n" : "");
					defaultInterpolatedStringHandler.AppendFormatted(PlayerManager.IsInRoom ? $"Room: {PlayerManager.CurrentRoom}\n" : "");
					ImGuiEx.SetItemTooltip(defaultInterpolatedStringHandler.ToStringAndClear(), (ImGuiHoveredFlags)0);
				}
				for (int num = 0; num < trigger.ReactionOptions.AllowedTerritories.Count; num++)
				{
					Territory entry = trigger.ReactionOptions.AllowedTerritories[num];
					ImGui.PushID((IntPtr)num);
					bool flag4 = false;
					ImGuiEx.IconButton((FontAwesomeIcon)62189, "removeTerritory");
					ImGuiEx.SetItemTooltip("Remove this territory.", (ImGuiHoveredFlags)0);
					if (ImGui.BeginPopupContextItem(ImU8String.op_Implicit("##removeTerritoryContext"), (ImGuiPopupFlags)0))
					{
						if (ImGuiEx.IconSelectable((FontAwesomeIcon)62189))
						{
							trigger.ReactionOptions.AllowedTerritories.RemoveAt(num);
							Plugin.Config.Save();
							ImGui.PopID();
							flag4 = true;
						}
						ImGui.EndPopup();
					}
					if (flag4)
					{
						break;
					}
					ImGui.SameLine();
					iO = ImGui.GetIO();
					ImGui.SetNextItemWidth(250f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
					string text = ((entry.Id == 0) ? "Select Territory" : (TerritoryUiList?.FirstOrDefault<(uint, string, bool)>(((uint Id, string Name, bool IsResidential) x) => x.Id == entry.Id).Item2 ?? "Unknown"));
					if (ImGui.BeginCombo(ImU8String.op_Implicit("##territory"), ImU8String.op_Implicit(text), (ImGuiComboFlags)0))
					{
						if (ImGui.Checkbox(ImU8String.op_Implicit("Residential Only"), ref ResidentialOnly))
						{
							BuildTerritoryUiList();
						}
						ImGui.Separator();
						foreach (var territoryUi in TerritoryUiList)
						{
							uint item = territoryUi.Id;
							string item2 = territoryUi.Name;
							bool flag5 = entry.Id == item;
							ImU8String val9 = new ImU8String(3, 2);
							((ImU8String)(ref val9)).AppendFormatted<string>(item2);
							((ImU8String)(ref val9)).AppendLiteral(" (");
							((ImU8String)(ref val9)).AppendFormatted<uint>(item);
							((ImU8String)(ref val9)).AppendLiteral(")");
							if (ImGui.Selectable(val9, flag5, (ImGuiSelectableFlags)0, default(Vector2)))
							{
								entry.Id = item;
								entry.Ward = (entry.Plot = (entry.Room = 0u));
								Plugin.Config.Save();
							}
						}
						ImGui.EndCombo();
					}
					ResidentialTerritory residentialTerritory = Plugin.ResidentialTerritories.FirstOrDefault((ResidentialTerritory x) => x.Id == entry.Id);
					if (residentialTerritory == null || residentialTerritory.ResidentialType == ResidentialType.Workshop)
					{
						continue;
					}
					switch (residentialTerritory.ResidentialType)
					{
					case ResidentialType.Ward:
						ImGui.SameLine();
						iO = ImGui.GetIO();
						ImGui.SetNextItemWidth(30f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
						if (ImGuiEx.DragUInt($"W##ward{num}", entry.Ward, delegate(uint x)
						{
							entry.Ward = x;
						}, 0.1f, 0u, 30u))
						{
							Plugin.Config.Save();
						}
						ImGuiEx.SetItemTooltip("The Ward to match when in " + residentialTerritory.Name + "\nSet to '0' for any ward.", (ImGuiHoveredFlags)0);
						ImGui.SameLine();
						iO = ImGui.GetIO();
						ImGui.SetNextItemWidth(30f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
						if (ImGuiEx.DragUInt($"P##plot{num}", entry.Plot, delegate(uint x)
						{
							entry.Plot = x;
						}, 0.1f, 0u, 60u))
						{
							Plugin.Config.Save();
						}
						ImGuiEx.SetItemTooltip("The Plot to match when in " + residentialTerritory.Name + "\nThis restricts match to being within a plot's garden area.\nSet to '0' to ignore plot.", (ImGuiHoveredFlags)0);
						break;
					case ResidentialType.House:
						ImGui.SameLine();
						iO = ImGui.GetIO();
						ImGui.SetNextItemWidth(30f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
						if (ImGuiEx.DragUInt($"W##ward{num}", entry.Ward, delegate(uint x)
						{
							entry.Ward = x;
						}, 0.1f, 0u, 30u))
						{
							Plugin.Config.Save();
						}
						ImGuiEx.SetItemTooltip("The Ward to match when in " + residentialTerritory.Name + "\nSet to '0' for any ward.", (ImGuiHoveredFlags)0);
						ImGui.SameLine();
						iO = ImGui.GetIO();
						ImGui.SetNextItemWidth(30f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
						if (ImGuiEx.DragUInt($"P##plot{num}", entry.Plot, delegate(uint x)
						{
							entry.Plot = x;
						}, 0.1f, 0u, 60u))
						{
							Plugin.Config.Save();
						}
						ImGuiEx.SetItemTooltip("The Plot to match when in " + residentialTerritory.Name + "\nThis restricts match to being inside a specific house.\nSet to '0' to ignore plot.", (ImGuiHoveredFlags)0);
						break;
					case ResidentialType.Chambers:
						ImGui.SameLine();
						iO = ImGui.GetIO();
						ImGui.SetNextItemWidth(30f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
						if (ImGuiEx.DragUInt($"W##ward{num}", entry.Ward, delegate(uint x)
						{
							entry.Ward = x;
						}, 0.1f, 0u, 30u))
						{
							Plugin.Config.Save();
						}
						ImGuiEx.SetItemTooltip("The Ward to match when in " + residentialTerritory.Name + "\nSet to '0' for any ward.", (ImGuiHoveredFlags)0);
						ImGui.SameLine();
						iO = ImGui.GetIO();
						ImGui.SetNextItemWidth(30f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
						if (ImGuiEx.DragUInt($"P##plot{num}", entry.Plot, delegate(uint x)
						{
							entry.Plot = x;
						}, 0.1f, 0u, 60u))
						{
							Plugin.Config.Save();
						}
						ImGuiEx.SetItemTooltip("The Plot to match when in " + residentialTerritory.Name + "\nThis restricts match to being inside a specific house.\nSet to '0' to ignore plot.", (ImGuiHoveredFlags)0);
						ImGui.SameLine();
						iO = ImGui.GetIO();
						ImGui.SetNextItemWidth(30f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
						if (ImGuiEx.DragUInt($"R##room{num}", entry.Room, delegate(uint x)
						{
							entry.Room = x;
						}, 0.1f, 0u, 200u))
						{
							Plugin.Config.Save();
						}
						ImGuiEx.SetItemTooltip("The FC Room to match when in " + residentialTerritory.Name + "\nThis restricts match to being inside a specific FC room.\nSet to '0' to ignore room.", (ImGuiHoveredFlags)0);
						break;
					case ResidentialType.Apartment:
						ImGui.SameLine();
						iO = ImGui.GetIO();
						ImGui.SetNextItemWidth(30f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
						if (ImGuiEx.DragUInt($"W##ward{num}", entry.Ward, delegate(uint x)
						{
							entry.Ward = x;
						}, 0.1f, 0u, 30u))
						{
							Plugin.Config.Save();
						}
						ImGuiEx.SetItemTooltip("The Ward to match when in " + residentialTerritory.Name + "\nSet to '0' for any ward.", (ImGuiHoveredFlags)0);
						ImGui.SameLine();
						iO = ImGui.GetIO();
						ImGui.SetNextItemWidth(30f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
						if (ImGuiEx.DragUInt($"R##room{num}", entry.Room, delegate(uint x)
						{
							entry.Room = x;
						}, 0.1f, 0u, 200u))
						{
							Plugin.Config.Save();
						}
						ImGuiEx.SetItemTooltip("The Apartment Room to match when in " + residentialTerritory.Name + "\nThis restricts match to being inside a specific apartment room.\nSet to '0' to ignore room.", (ImGuiHoveredFlags)0);
						break;
					case ResidentialType.ApartmentLobby:
						ImGui.SameLine();
						iO = ImGui.GetIO();
						ImGui.SetNextItemWidth(30f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
						if (ImGuiEx.DragUInt($"W##ward{num}", entry.Ward, delegate(uint x)
						{
							entry.Ward = x;
						}, 0.1f, 0u, 30u))
						{
							Plugin.Config.Save();
						}
						ImGuiEx.SetItemTooltip("The Ward to match when in " + residentialTerritory.Name + "\nSet to '0' for any ward.", (ImGuiHoveredFlags)0);
						break;
					}
				}
			}
		}
		ImGui.TreePop();
	}

	private void DrawReactionRangePreview(ReactionOptions options)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0150: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		if (!options.RestrictRange)
		{
			return;
		}
		EntityInfo localPlayer = PlayerManager.LocalPlayer;
		if (localPlayer == null)
		{
			return;
		}
		ImGui.Text(ImU8String.op_Implicit("Target Test:"));
		ImGui.SameLine();
		if (!localPlayer.IsTargetValid || localPlayer.Target == localPlayer.GameObject)
		{
			Vector4 dalamudRed = ImGuiColors.DalamudRed;
			ImGui.TextColored(ref dalamudRed, ImU8String.op_Implicit("Target a player/npc to test with."));
		}
		else
		{
			EntityInfo targetAsEntity = PlayerManager.GetTargetAsEntity();
			if (targetAsEntity != null)
			{
				if (targetAsEntity.IsWithinReactionAngleAndDistanceToLocalPlayer(options))
				{
					Vector4 dalamudRed = ImGuiColors.ParsedGreen;
					ImGui.TextColored(ref dalamudRed, ImU8String.op_Implicit("In reaction area."));
				}
				else
				{
					Vector4 dalamudRed = ImGuiColors.DalamudRed;
					ImGui.TextColored(ref dalamudRed, ImU8String.op_Implicit("Not in reaction area."));
				}
			}
			else
			{
				Vector4 dalamudRed = ImGuiColors.DalamudRed;
				ImGui.TextColored(ref dalamudRed, ImU8String.op_Implicit("Invalid target."));
			}
		}
		Vector4 vector = new Vector4(0f, 1f, 0f, RangePreviewOpacity);
		int num = 64;
		float num2 = 128f;
		ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
		ImGuiViewportPtr mainViewport = ImGuiHelpers.MainViewport;
		Vector2 pos = ((ImGuiViewportPtr)(ref mainViewport)).Pos;
		mainViewport = ImGuiHelpers.MainViewport;
		Vector2 size = ((ImGuiViewportPtr)(ref mainViewport)).Size;
		Vector2 vector2 = pos;
		Vector2 vector3 = pos + size;
		float num3 = Math.Max(0f, options.RestrictedDistanceMin);
		float num4 = Math.Max(num3 + 0.001f, options.RestrictedDistanceMax);
		if (num4 <= 0f || num4 <= num3)
		{
			return;
		}
		Vector3 vector4 = Vector3.op_Implicit(localPlayer.Position);
		float angle = localPlayer.Angle;
		Vector2 vector5 = new Vector2(MathF.Sin(angle), MathF.Cos(angle));
		float num5 = (float)options.RestrictedAngleDirection % 360f;
		if (num5 < 0f)
		{
			num5 += 360f;
		}
		float x = (float)Math.PI * 2f * (num5 / 360f);
		float num6 = MathF.Cos(x);
		float num7 = MathF.Sin(x);
		Vector2 vector6 = new Vector2(vector5.X * num6 - vector5.Y * num7, vector5.X * num7 + vector5.Y * num6);
		float num8 = Math.Clamp(options.RestrictedAngleArea, 0f, 1f);
		float num9 = (float)Math.PI * 2f * num8;
		float num10 = num9 / 2f;
		List<(Vector2, Vector2)> list = new List<(Vector2, Vector2)>(num + 1);
		Vector2 item = default(Vector2);
		Vector2 item2 = default(Vector2);
		for (int i = 0; i <= num; i++)
		{
			float num11 = ((num == 0) ? 0f : ((float)i / (float)num));
			float x2 = 0f - num10 + num11 * num9;
			float num12 = MathF.Cos(x2);
			float num13 = MathF.Sin(x2);
			Vector2 vector7 = new Vector2(vector6.X * num12 - vector6.Y * num13, vector6.X * num13 + vector6.Y * num12);
			Vector3 vector8 = vector4 + new Vector3(vector7.X * num3, 0f, vector7.Y * num3);
			Vector3 vector9 = vector4 + new Vector3(vector7.X * num4, 0f, vector7.Y * num4);
			if (Plugin.GameGui.WorldToScreen(vector8, ref item) && Plugin.GameGui.WorldToScreen(vector9, ref item2) && !(item.X < vector2.X - num2) && !(item.X > vector3.X + num2) && !(item.Y < vector2.Y - num2) && !(item.Y > vector3.Y + num2) && !(item2.X < vector2.X - num2) && !(item2.X > vector3.X + num2) && !(item2.Y < vector2.Y - num2) && !(item2.Y > vector3.Y + num2))
			{
				list.Add((item, item2));
			}
		}
		if (list.Count >= 2)
		{
			uint colorU = ImGui.GetColorU32(vector);
			ImGui.PushClipRect(vector2, vector3, false);
			for (int j = 0; j < list.Count - 1; j++)
			{
				Vector2 item3 = list[j].Item2;
				Vector2 item4 = list[j + 1].Item2;
				Vector2 item5 = list[j].Item1;
				Vector2 item6 = list[j + 1].Item1;
				((ImDrawListPtr)(ref windowDrawList)).AddTriangleFilled(item3, item4, item5, colorU);
				((ImDrawListPtr)(ref windowDrawList)).AddTriangleFilled(item5, item4, item6, colorU);
			}
			ImGui.PopClipRect();
		}
	}

	private static void BuildTerritoryUiList()
	{
		TerritoryUiList = new List<(uint, string, bool)>();
		foreach (ResidentialTerritory residentialTerritory in Plugin.ResidentialTerritories)
		{
			TerritoryUiList.Add((residentialTerritory.Id, residentialTerritory.Name, true));
		}
		if (ResidentialOnly)
		{
			return;
		}
		foreach (NonResidentialTerritory item in Plugin.NonResidentialTerritories.OrderBy<NonResidentialTerritory, string>((NonResidentialTerritory x) => x.Name, StringComparer.OrdinalIgnoreCase))
		{
			TerritoryUiList.Add((item.Id, item.Name, false));
		}
	}

	private void DrawTriggerReactionQueue(Trigger trigger)
	{
		if (ImGuiEx.TreeNode("Reaction Queues##reactionQueues", null, default(Vector4), (ImGuiTreeNodeFlags)0))
		{
			DrawTriggerEmoteReactions(trigger);
			DrawTriggerTextReactions(trigger);
			ImGui.TreePop();
		}
	}

	private void DrawTriggerEmoteReactions(Trigger trigger)
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		if (!ImGuiEx.TreeNode("Emote Reactions##emoteReactions", null, default(Vector4), (ImGuiTreeNodeFlags)0))
		{
			return;
		}
		if (ImGuiEx.IconButton((FontAwesomeIcon)61525, "newEmoteReaction"))
		{
			if (trigger.Reactions == null)
			{
				List<ReactionBase> list = (trigger.Reactions = new List<ReactionBase>());
			}
			trigger.Reactions.Add(new EmoteReaction());
			Plugin.Config.Save();
		}
		ImGuiEx.SetItemTooltip("Add new emote reaction.", (ImGuiHoveredFlags)0);
		List<ReactionBase> list3 = ((trigger.Reactions == null) ? null : trigger.Reactions.Where((ReactionBase x) => x is EmoteReaction).ToList());
		if (trigger.Reactions == null || list3 == null)
		{
			ImGui.LabelText(ImU8String.op_Implicit("##noEmoteReactions"), ImU8String.op_Implicit("No Emote Reactions Added"));
		}
		else
		{
			if (list3.Count > 0)
			{
				ImGui.SameLine();
				if (ImGui.Button(ImU8String.op_Implicit("Preview##previewEmotes"), default(Vector2)))
				{
					plugin.TriggerManager.PreviewQueue(trigger);
				}
				ImGuiEx.SetItemTooltip("Preview the current emote/text reactions.\n\n- Certain emote options are not able to be previewed such as copying instigator emote.\n- Preview of text reactions will be performed in the echo chat channel.\n- If you have a valid player target, they will be treated as the instigator/receiver depending on \n   event instigator/receiver options above.", (ImGuiHoveredFlags)0);
			}
			Action action = null;
			int num = 1;
			foreach (ReactionBase item in list3)
			{
				trigger.Reactions.IndexOf(item);
				if (ImGuiEx.TreeNode($"{num}. Emote Reaction##emoteReaction{num}", null, default(Vector4), (ImGuiTreeNodeFlags)0))
				{
					action = DrawEmoteReaction(trigger, list3, (EmoteReaction)item);
					ImGui.TreePop();
				}
				num++;
			}
			action?.Invoke();
		}
		ImGui.TreePop();
	}

	private Action? DrawEmoteReaction(Trigger trigger, List<ReactionBase> emoteReactions, EmoteReaction reaction)
	{
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_012f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_048d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0492: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0256: Unknown result type (might be due to invalid IL or missing references)
		//IL_025b: Unknown result type (might be due to invalid IL or missing references)
		//IL_028d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0588: Unknown result type (might be due to invalid IL or missing references)
		//IL_058d: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_0523: Unknown result type (might be due to invalid IL or missing references)
		//IL_061e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0396: Unknown result type (might be due to invalid IL or missing references)
		Action result = null;
		if (trigger.Reactions != null && trigger.Reactions.Count != 0)
		{
			if (ImGuiEx.IconButton((FontAwesomeIcon)61610, "moveReactionUp"))
			{
				result = delegate
				{
					int num2 = trigger.Reactions.IndexOf(reaction);
					trigger.Reactions.RemoveAt(num2);
					num2 = Math.Max(num2 - 1, 0);
					trigger.Reactions.Insert(num2, reaction);
					Plugin.Config.Save();
				};
			}
			ImGuiEx.SetItemTooltip("Move this reaction up the chain.", (ImGuiHoveredFlags)0);
			ImGui.SameLine();
			if (ImGuiEx.IconButton((FontAwesomeIcon)61611, "moveReactionDown"))
			{
				result = delegate
				{
					int num2 = trigger.Reactions.IndexOf(reaction);
					trigger.Reactions.RemoveAt(num2);
					num2 = Math.Min(num2 + 1, trigger.Reactions.Count);
					trigger.Reactions.Insert(num2, reaction);
					Plugin.Config.Save();
				};
			}
			ImGuiEx.SetItemTooltip("Move this reaction down the chain.", (ImGuiHoveredFlags)0);
			ImGui.SameLine();
			ImGuiEx.IconButton((FontAwesomeIcon)62189, "removeReaction");
			ImGuiEx.SetItemTooltip("Remove this reaction.", (ImGuiHoveredFlags)0);
			if (ImGui.BeginPopupContextItem(ImU8String.op_Implicit("##removeContext"), (ImGuiPopupFlags)0))
			{
				if (ImGuiEx.IconSelectable((FontAwesomeIcon)62189))
				{
					result = delegate
					{
						int num2 = trigger.Reactions.IndexOf(reaction);
						trigger.Reactions.RemoveAt(num2);
						num2 = Math.Min(num2, trigger.Reactions.Count - 1);
						Plugin.Config.Save();
					};
				}
				ImGui.EndPopup();
			}
		}
		int num = emoteReactions.IndexOf(reaction);
		if (ImGuiEx.Checkbox("Perform Emote##performEmote", reaction.PerformEmote, delegate(bool x)
		{
			reaction.PerformEmote = x;
		}))
		{
			Plugin.Config.Save();
		}
		ImGuiEx.SetItemTooltip("Whether this emote reaction should actually perform an emote.\nOtherwise this reaction will only be used as a pause duration or switching target/adjusting position.", (ImGuiHoveredFlags)0);
		ImGuiIOPtr iO;
		if (num == 0)
		{
			iO = ImGui.GetIO();
			ImGui.SetNextItemWidth(60f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
			if (ImGuiEx.DragInt("Delay##reactionDelay", reaction.Delay, delegate(int x)
			{
				reaction.Delay = x;
			}))
			{
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Delay in milliseconds before this reaction will be performed from when the event is triggered.", (ImGuiHoveredFlags)0);
			ImGui.SameLine();
		}
		iO = ImGui.GetIO();
		ImGui.SetNextItemWidth(60f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
		if (ImGuiEx.DragInt("Duration##reactionDuration", reaction.Duration, delegate(int x)
		{
			reaction.Duration = x;
		}))
		{
			Plugin.Config.Save();
		}
		ImGuiEx.SetItemTooltip("Duration in milliseconds for this reaction before any proceeding reaction will be performed.\nThis is also a duration for which the reaction is considered interruptable.", (ImGuiHoveredFlags)0);
		if (emoteReactions.Count > 1 && reaction.Duration < 500 && num + 1 < emoteReactions.Count)
		{
			EmoteReaction emoteReaction = (EmoteReaction)emoteReactions[num + 1];
			if (emoteReaction != null && emoteReaction.PerformEmote)
			{
				ImGui.SameLine();
				ImGuiEx.IconWarningTooltip("This emote queue may not perform as expected due to the current duration being too short, the next emote may be skipped or may interrupt this emote sooner than desired.\nThe 'Preview' button can be used to test how this queue behaves so you can finetune the duration.");
			}
		}
		if (reaction.PerformEmote)
		{
			ImGui.BeginDisabled(reaction.CopyInstigator);
			iO = ImGui.GetIO();
			ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
			Emote emote = plugin.Emotes.FirstOrDefault((Emote e) => e.ID == reaction.ID && reaction.ID != ushort.MaxValue);
			if (ImGui.BeginCombo(ImU8String.op_Implicit("##reactionEmotes"), ImU8String.op_Implicit(reaction.CopyInstigator ? "Copy Instigator" : ((emote != null) ? ("Emote: " + emote.Name) : "No Emote Selected")), (ImGuiComboFlags)0))
			{
				if (!IsComboOpen_ReactionEmotes)
				{
					IsComboOpen_ReactionEmotes = true;
					plugin.Emotes = plugin.Emotes.OrderByDescending((Emote emote2) => reaction.ID == emote2.ID && emote2.ID != ushort.MaxValue).ThenBy<Emote, string>((Emote emote2) => emote2.Name, StringComparer.OrdinalIgnoreCase).ToList();
				}
				foreach (Emote emote2 in plugin.Emotes)
				{
					if (!string.IsNullOrWhiteSpace(emote2.Command) || emote2.IsPose)
					{
						bool flag = reaction.ID == emote2.ID;
						ImGuiEx.IconCheckbox(flag);
						ImGui.SameLine();
						if (ImGui.Selectable(ImU8String.op_Implicit(emote2.ToString()), flag, (ImGuiSelectableFlags)1, default(Vector2)))
						{
							reaction.ID = (flag ? ushort.MaxValue : emote2.ID);
							Plugin.Config.Save();
						}
					}
				}
				ImGui.EndCombo();
			}
			else if (IsComboOpen_ReactionEmotes)
			{
				IsComboOpen_ReactionEmotes = false;
			}
			ImGui.EndDisabled();
			ImGuiEx.SetItemTooltip("Select an emote to react with when this event is triggered.", (ImGuiHoveredFlags)0);
			if (trigger.Type == TriggerType.Emote)
			{
				ImGui.SameLine();
				if (ImGuiEx.Checkbox("Copy Instigator##copyInstigator", reaction.CopyInstigator, delegate(bool x)
				{
					reaction.CopyInstigator = x;
				}))
				{
					Plugin.Config.Save();
				}
				ImGuiEx.SetItemTooltip("Copy the emote that the instigator performed.\nIf you have not unlocked the emote, this reaction will not be performed.", (ImGuiHoveredFlags)0);
			}
			else if (reaction.CopyInstigator)
			{
				reaction.CopyInstigator = false;
				Plugin.Config.Save();
			}
		}
		iO = ImGui.GetIO();
		ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
		ImU8String val = ImU8String.op_Implicit("##targetType");
		ImU8String val2 = default(ImU8String);
		((ImU8String)(ref val2))._002Ector(8, 1);
		((ImU8String)(ref val2)).AppendLiteral("Target: ");
		((ImU8String)(ref val2)).AppendFormatted<ReactionTargetType>(reaction.TargetType);
		if (ImGui.BeginCombo(val, val2, (ImGuiComboFlags)0))
		{
			foreach (ReactionTargetType value in Enum.GetValues(typeof(ReactionTargetType)))
			{
				bool flag2 = reaction.TargetType == value;
				if (ImGui.Selectable(ImU8String.op_Implicit(value.ToString()), flag2, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					reaction.TargetType = value;
					Plugin.Config.Save();
				}
			}
			ImGui.EndCombo();
		}
		ImGuiEx.SetItemTooltip("Set target when performing reaction:\n\nNone: No target condition will be set, any current target will continue to be targeted.\nUntarget: Remove any current target.\nTarget Instigator/Receiver: Set target as instigator/receiver.\nTarget Self: Set target as yourself.", (ImGuiHoveredFlags)0);
		iO = ImGui.GetIO();
		ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
		ImU8String val3 = ImU8String.op_Implicit("##lookAtType");
		ImU8String val4 = default(ImU8String);
		((ImU8String)(ref val4))._002Ector(8, 1);
		((ImU8String)(ref val4)).AppendLiteral("LookAt: ");
		((ImU8String)(ref val4)).AppendFormatted<ReactionLookAtType>(reaction.LookAtType);
		if (ImGui.BeginCombo(val3, val4, (ImGuiComboFlags)0))
		{
			foreach (ReactionLookAtType value2 in Enum.GetValues(typeof(ReactionLookAtType)))
			{
				bool flag3 = reaction.LookAtType == value2;
				if (ImGui.Selectable(ImU8String.op_Implicit(value2.ToString()), flag3, (ImGuiSelectableFlags)0, default(Vector2)))
				{
					reaction.LookAtType = value2;
					Plugin.Config.Save();
				}
			}
			ImGui.EndCombo();
		}
		ImGuiEx.SetItemTooltip("Control which direction to face in when performing reaction:\n\nTarget: Normal behaviour, face target (if any).\nMaintain: Maintain your current facing direction.\nInstigator/Receiver: Face instigator/receiver.\nInstigator/Receiver Inverse: Face away from instigator/receiver.\nInstigator/Receiver Direction: Face in same direction as instigator/receiver.\nInstigator/Receiver Direction Inverse: Face in opposite direction of instigator/receiver.", (ImGuiHoveredFlags)0);
		return result;
	}

	private void DrawTriggerTextReactions(Trigger trigger)
	{
		//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		if (!ImGuiEx.TreeNode("Text Reactions##textReactions", null, default(Vector4), (ImGuiTreeNodeFlags)0))
		{
			return;
		}
		if (ImGuiEx.IconButton((FontAwesomeIcon)61525, "newTextReaction"))
		{
			if (trigger.Reactions == null)
			{
				List<ReactionBase> list = (trigger.Reactions = new List<ReactionBase>());
			}
			trigger.Reactions.Add(new TextReaction());
			Plugin.Config.Save();
		}
		ImGuiEx.SetItemTooltip("Add new text reaction.", (ImGuiHoveredFlags)0);
		List<ReactionBase> list3 = ((trigger.Reactions == null) ? null : trigger.Reactions.Where((ReactionBase x) => x is TextReaction).ToList());
		if (trigger.Reactions == null || list3 == null)
		{
			ImGui.LabelText(ImU8String.op_Implicit("##noTextReactions"), ImU8String.op_Implicit("No Text Reactions Added"));
		}
		else
		{
			if (list3.Count > 0)
			{
				ImGui.SameLine();
				if (ImGui.Button(ImU8String.op_Implicit("Preview##previewTexts"), default(Vector2)))
				{
					plugin.TriggerManager.PreviewQueue(trigger);
				}
				ImGuiEx.SetItemTooltip("Preview the current emote/text reactions.\n\n- Certain emote options are not able to be previewed such as copying instigator emote.\n- Preview of text reactions will be performed in the echo chat channel.\n- If you have a valid player target, they will be treated as the instigator/receiver depending on \n   event instigator/receiver options above.", (ImGuiHoveredFlags)0);
			}
			Action action = null;
			int num = 1;
			foreach (ReactionBase item in list3)
			{
				trigger.Reactions.IndexOf(item);
				if (ImGuiEx.TreeNode($"{num}. Text Reaction##textReaction{num}", null, default(Vector4), (ImGuiTreeNodeFlags)0))
				{
					action = DrawTextReaction(trigger, list3, (TextReaction)item);
					ImGui.TreePop();
				}
				num++;
			}
			action?.Invoke();
		}
		ImGui.TreePop();
	}

	private Action? DrawTextReaction(Trigger trigger, List<ReactionBase> textReactions, TextReaction reaction)
	{
		//IL_0152: Unknown result type (might be due to invalid IL or missing references)
		//IL_0157: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_01e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_023f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0467: Unknown result type (might be due to invalid IL or missing references)
		//IL_046c: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
		Action result = null;
		if (trigger.Reactions != null && trigger.Reactions.Count != 0)
		{
			if (ImGuiEx.IconButton((FontAwesomeIcon)61610, "moveReactionUp"))
			{
				result = delegate
				{
					int num = trigger.Reactions.IndexOf(reaction);
					trigger.Reactions.RemoveAt(num);
					num = Math.Max(num - 1, 0);
					trigger.Reactions.Insert(num, reaction);
					Plugin.Config.Save();
				};
			}
			ImGuiEx.SetItemTooltip("Move this reaction up the chain.", (ImGuiHoveredFlags)0);
			ImGui.SameLine();
			if (ImGuiEx.IconButton((FontAwesomeIcon)61611, "moveReactionDown"))
			{
				result = delegate
				{
					int num = trigger.Reactions.IndexOf(reaction);
					trigger.Reactions.RemoveAt(num);
					num = Math.Min(num + 1, trigger.Reactions.Count);
					trigger.Reactions.Insert(num, reaction);
					Plugin.Config.Save();
				};
			}
			ImGuiEx.SetItemTooltip("Move this reaction down the chain.", (ImGuiHoveredFlags)0);
			ImGui.SameLine();
			ImGuiEx.IconButton((FontAwesomeIcon)62189, "removeReaction");
			ImGuiEx.SetItemTooltip("Remove this reaction.", (ImGuiHoveredFlags)0);
			if (ImGui.BeginPopupContextItem(ImU8String.op_Implicit("##removeContext"), (ImGuiPopupFlags)0))
			{
				if (ImGuiEx.IconSelectable((FontAwesomeIcon)62189))
				{
					result = delegate
					{
						int num = trigger.Reactions.IndexOf(reaction);
						trigger.Reactions.RemoveAt(num);
						num = Math.Min(num, trigger.Reactions.Count - 1);
						Plugin.Config.Save();
					};
				}
				ImGui.EndPopup();
			}
		}
		ImGuiIOPtr iO;
		if (textReactions.IndexOf(reaction) == 0)
		{
			iO = ImGui.GetIO();
			ImGui.SetNextItemWidth(60f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
			if (ImGuiEx.DragInt("Delay##reactionDelay", reaction.Delay, delegate(int x)
			{
				reaction.Delay = x;
			}))
			{
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Delay in milliseconds before this reaction will be performed from when the event is triggered.", (ImGuiHoveredFlags)0);
			ImGui.SameLine();
		}
		iO = ImGui.GetIO();
		ImGui.SetNextItemWidth(60f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
		if (ImGuiEx.DragInt("Duration##reactionDuration", reaction.Duration, delegate(int x)
		{
			reaction.Duration = x;
		}, 1f, 500))
		{
			Plugin.Config.Save();
		}
		ImGuiEx.SetItemTooltip("Duration in milliseconds for this reaction before any proceeding reaction will be performed.", (ImGuiHoveredFlags)0);
		ImGui.BeginDisabled(reaction.SameChannel);
		iO = ImGui.GetIO();
		ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
		_ = reaction.Channel;
		if (ImGui.BeginCombo(ImU8String.op_Implicit("##reactionChannels"), ImU8String.op_Implicit(reaction.SameChannel ? "Copy Instigator" : ((reaction.Channel != ChatType.None) ? $"Channel: {reaction.Channel}" : "No Channel Selected")), (ImGuiComboFlags)0))
		{
			foreach (ChatType value in Enum.GetValues(typeof(ChatType)))
			{
				if (value == ChatType.None || value == ChatType.Emote)
				{
					continue;
				}
				bool flag = reaction.Channel == value;
				ImGuiEx.IconCheckbox(flag);
				ImGui.SameLine();
				ImU8String val = new ImU8String(0, 1);
				((ImU8String)(ref val)).AppendFormatted<ChatType>(value);
				if (ImGui.Selectable(val, flag, (ImGuiSelectableFlags)1, default(Vector2)))
				{
					if (flag)
					{
						reaction.Channel &= ~value;
					}
					else
					{
						reaction.Channel = value;
					}
					Plugin.Config.Save();
				}
			}
			ImGui.EndCombo();
		}
		ImGui.EndDisabled();
		ImGuiEx.SetItemTooltip("Select a chat channel to send this reaction to when this event is triggered.\nThe 'Command' channel can be used for performing vanilla/plugin commands.", (ImGuiHoveredFlags)0);
		if (trigger.Type == TriggerType.Text)
		{
			Instigator? instigator = trigger.Instigator;
			if (instigator != null && instigator.Type == PlayerType.Self)
			{
				ChannelTextReceiver channelTextReceiver = trigger.Receiver as ChannelTextReceiver;
				if (reaction.SameChannel)
				{
					ImGui.SameLine();
					ImGuiEx.IconAlertTooltip("This reaction will be ignored with the current properties to prevent crashing.\nUnable to send message to same chat channel when instigator is self.");
				}
				else if (reaction.Channel != ChatType.Echo && reaction.Channel != ChatType.Command && ((channelTextReceiver != null && channelTextReceiver.MatchAny) || (channelTextReceiver != null && channelTextReceiver.Channel.HasFlag(reaction.Channel))))
				{
					ImGui.SameLine();
					ImGuiEx.IconWarningTooltip("This reaction may be ignored with the current properties to prevent crashing.\nUnable to send message to same chat channel when instigator is self.\nWill only trigger if the reaction channel is not the same as the receiving channel.");
				}
			}
			ImGui.SameLine();
			if (ImGuiEx.Checkbox("Copy Instigator##copyInstigator", reaction.SameChannel, delegate(bool x)
			{
				reaction.SameChannel = x;
			}))
			{
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Copy the chat channel used by the instigator.", (ImGuiHoveredFlags)0);
		}
		else if (reaction.SameChannel)
		{
			reaction.SameChannel = false;
			Plugin.Config.Save();
		}
		ImGui.BeginDisabled(reaction.CopyInstigator);
		iO = ImGui.GetIO();
		ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
		if (reaction.CopyInstigator)
		{
			ImGuiEx.InputText("##messageTemplate", "Copy Instigator", delegate(string x)
			{
				x = x;
			}, 450);
		}
		else if (ImGuiEx.InputText("##messageTemplate", reaction.Template, delegate(string x)
		{
			reaction.Template = x;
		}, 450))
		{
			Plugin.Config.Save();
		}
		ImGui.EndDisabled();
		ImGuiEx.SetItemTooltip("The message to send to the selected channel, with the below formatting:\n%ifn%/%isn% - Instigator Forename/Surname", (ImGuiHoveredFlags)0);
		if (trigger.Type == TriggerType.Text)
		{
			ImGui.SameLine();
			if (ImGuiEx.Checkbox("Copy Instigator##copyInstigatorMessage", reaction.CopyInstigator, delegate(bool x)
			{
				reaction.CopyInstigator = x;
			}))
			{
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Copy the message sent by the instigator.", (ImGuiHoveredFlags)0);
		}
		else if (reaction.CopyInstigator)
		{
			reaction.CopyInstigator = false;
			Plugin.Config.Save();
		}
		return result;
	}

	private void DrawTriggerDiscordSetup(Trigger trigger)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_0130: Unknown result type (might be due to invalid IL or missing references)
		//IL_0174: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0293: Unknown result type (might be due to invalid IL or missing references)
		//IL_0374: Unknown result type (might be due to invalid IL or missing references)
		//IL_048c: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0581: Unknown result type (might be due to invalid IL or missing references)
		//IL_0590: Unknown result type (might be due to invalid IL or missing references)
		//IL_05bd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_05e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_05ef: Unknown result type (might be due to invalid IL or missing references)
		//IL_05fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_060d: Unknown result type (might be due to invalid IL or missing references)
		//IL_061c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0630: Unknown result type (might be due to invalid IL or missing references)
		//IL_0644: Unknown result type (might be due to invalid IL or missing references)
		//IL_0658: Unknown result type (might be due to invalid IL or missing references)
		//IL_066c: Unknown result type (might be due to invalid IL or missing references)
		//IL_067b: Unknown result type (might be due to invalid IL or missing references)
		//IL_068f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0525: Unknown result type (might be due to invalid IL or missing references)
		//IL_0534: Unknown result type (might be due to invalid IL or missing references)
		//IL_0759: Unknown result type (might be due to invalid IL or missing references)
		//IL_0768: Unknown result type (might be due to invalid IL or missing references)
		if (!ImGuiEx.TreeNode("Discord Setup##discordSetup", null, (plugin.DiscordManager.IsDisconnected || Plugin.Config.Discord.UserKey == string.Empty) ? ImGuiColors.DalamudRed : default(Vector4), (ImGuiTreeNodeFlags)0))
		{
			return;
		}
		bool isConnected = plugin.DiscordManager.IsConnected;
		ImGui.Spacing();
		if (plugin.DiscordManager.IsDisconnecting)
		{
			Vector4 dalamudYellow = ImGuiColors.DalamudYellow;
			ImGui.TextColored(ref dalamudYellow, ImU8String.op_Implicit("Disconnecting.."));
		}
		else if (plugin.DiscordManager.IsConnecting)
		{
			Vector4 dalamudYellow = ImGuiColors.DalamudYellow;
			ImGui.TextColored(ref dalamudYellow, ImU8String.op_Implicit("Connecting.."));
		}
		else if (plugin.DiscordManager.IsDisconnected || StringExtensions.IsNullOrWhitespace(Plugin.Config.Discord.UserKey))
		{
			if (plugin.DiscordManager.IsDisconnected)
			{
				ImGuiEx.IconAlertText("Disconnected: Discord triggers will not function.");
			}
			else
			{
				ImGuiEx.IconWarningText("Connected (Unlinked): Discord triggers will not function.");
			}
		}
		else if (isConnected)
		{
			if (!Plugin.Config.Enabled)
			{
				Vector4 dalamudYellow = ImGuiColors.DalamudOrange;
				ImGui.TextColored(ref dalamudYellow, ImU8String.op_Implicit("Connected (Not Monitoring): Plugin is disabled."));
			}
			else if (!plugin.DiscordManager.AnyTriggerEnabled)
			{
				Vector4 dalamudYellow = ImGuiColors.DalamudOrange;
				ImGui.TextColored(ref dalamudYellow, ImU8String.op_Implicit("Connected (Not Monitoring): No discord triggers enabled."));
			}
			else
			{
				Vector4 dalamudYellow = ImGuiColors.ParsedGreen;
				ImGui.TextColored(ref dalamudYellow, ImU8String.op_Implicit("Connected: Monitoring activity updates."));
			}
		}
		ImGui.Spacing();
		ImGui.Separator();
		ImGui.Spacing();
		ImGuiEx.DrawStyledLinkText("Pyon Discord Server", "https://discord.gg/3wBtUrVDJh", 16u, "Open invite link to the Pyon Discord Server in your default browser.");
		ImGui.SameLine();
		ImGuiEx.DrawStyledText(" - ##linkSep", 1u);
		ImGui.SameLine();
		ImGuiEx.DrawStyledLinkText("Discord Dev Portal", "https://discord.com/developers/applications", 555u, "Open link to the Discord Developer Portal in your default browser.\nYou can ignore this if you're using the Pyon Server.");
		ImGui.Spacing();
		ImGui.Spacing();
		ImGui.BeginDisabled(plugin.DiscordManager.IsConnecting || plugin.DiscordManager.IsDisconnecting);
		if (ImGuiEx.Checkbox("Use Pyon Server", Plugin.Config.Discord.UsePyonServer, delegate(bool x)
		{
			Plugin.Config.Discord.UsePyonServer = x;
		}))
		{
			Plugin.Config.Save();
			if (isConnected)
			{
				plugin.DiscordManager.Disconnect();
			}
		}
		ImGui.EndDisabled();
		ImGuiEx.SetItemTooltip("Whether to use Pyon Server for easier setup.\nOtherwise you will need to create your own Discord Server/Bot.", (ImGuiHoveredFlags)0);
		ImGuiIOPtr iO;
		if (!Plugin.Config.Discord.UsePyonServer)
		{
			ImGui.SameLine();
			iO = ImGui.GetIO();
			ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
			if (ImGuiEx.InputTextWithHint("##discordBotToken", "Bot Token", Plugin.Config.Discord.BotToken, delegate(string x)
			{
				Plugin.Config.Discord.BotToken = x;
			}))
			{
				Plugin.Config.Save();
			}
		}
		ImGui.SameLine();
		iO = ImGui.GetIO();
		ImGui.SetNextItemWidth(100f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
		ImGui.BeginDisabled(plugin.DiscordManager.IsConnecting || plugin.DiscordManager.IsDisconnecting || (!isConnected && !Plugin.Config.Discord.UsePyonServer && StringExtensions.IsNullOrWhitespace(Plugin.Config.Discord.BotToken)));
		if (ImGui.Button(ImU8String.op_Implicit(isConnected ? "Disconnect" : "Connect"), default(Vector2)))
		{
			if (isConnected)
			{
				plugin.DiscordManager.Disconnect();
			}
			else
			{
				plugin.DiscordManager.ConnectAsync();
			}
		}
		ImGui.EndDisabled();
		if (!StringExtensions.IsNullOrWhitespace(Plugin.Config.Discord.UserKey))
		{
			ImGui.SameLine();
			ImGui.BeginDisabled(plugin.DiscordManager.IsConnecting || plugin.DiscordManager.IsDisconnecting);
			if (ImGui.Button(ImU8String.op_Implicit("Unlink"), default(Vector2)))
			{
				if (isConnected)
				{
					plugin.DiscordManager.Disconnect();
				}
				plugin.DiscordManager.UnlinkDiscordUser();
			}
			ImGui.EndDisabled();
			ImGuiEx.SetItemTooltip("Unlink your Discord ID.\nThis is only necessary if you're switching to another Discord account.", (ImGuiHoveredFlags)0);
		}
		if (Plugin.Config.Discord.UsePyonServer && isConnected && StringExtensions.IsNullOrWhitespace(Plugin.Config.Discord.UserKey))
		{
			ImGuiEx.IconWarningText("TriggerPyon needs to link your Discord ID to retrieve your Discord activity.", wrapped: true);
			ImGui.Indent();
			ImGui.Text(ImU8String.op_Implicit("1. Join this server:"));
			ImGui.SameLine();
			ImGuiEx.DrawStyledLinkText("Pyon Discord Server", "https://discord.gg/3wBtUrVDJh", 16u, "Open invite link to the Pyon Discord Server in your default browser.");
			ImGui.Text(ImU8String.op_Implicit("2. Copy this key:"));
			if (StringExtensions.IsNullOrWhitespace(plugin.DiscordManager.VerificationKey))
			{
				plugin.DiscordManager.GenerateVerificationKey();
			}
			else
			{
				ImGui.SameLine();
				ImGuiEx.DrawStyledText(plugin.DiscordManager.VerificationKey + "##genKey", 16u, "Click to copy the key.", delegate
				{
					//IL_0010: Unknown result type (might be due to invalid IL or missing references)
					ImGui.SetClipboardText(ImU8String.op_Implicit(plugin.DiscordManager.VerificationKey));
				});
			}
			ImGui.Text(ImU8String.op_Implicit("3. Paste key in the #triggerpyon channel."));
			ImGui.Text(ImU8String.op_Implicit("4. After linking, you can setup Honorific Title below with any Discord activity."));
			ImGui.Unindent();
		}
		else if (!Plugin.Config.Discord.UsePyonServer && !isConnected)
		{
			if (ImGuiEx.TreeNode("Custom Bot Setup##customBot", null, default(Vector4), (ImGuiTreeNodeFlags)0))
			{
				ImGui.Text(ImU8String.op_Implicit("1. Create a Discord server."));
				ImGui.Text(ImU8String.op_Implicit("2. Login to:"));
				ImGui.SameLine();
				ImGuiEx.DrawStyledLinkText("Discord Dev Portal", "https://discord.com/developers/applications", 555u, "Open link to the Discord Developer Portal in your default browser.");
				ImGui.Text(ImU8String.op_Implicit("3. Create New Application"));
				ImGui.Text(ImU8String.op_Implicit("4. Installation Tab:"));
				ImGui.Indent();
				ImGui.Text(ImU8String.op_Implicit("Installation Contexts: Guild Install"));
				ImGui.Text(ImU8String.op_Implicit("Install Link: Discord Provided Link"));
				ImGui.Text(ImU8String.op_Implicit("Default Install Settings - Scopes: bot"));
				ImGui.Text(ImU8String.op_Implicit("Click 'Save' button."));
				ImGui.Text(ImU8String.op_Implicit("Open the 'Discord Provided Link' & add the bot to your server."));
				ImGui.Unindent();
				ImGui.Text(ImU8String.op_Implicit("5. Bot Tab:"));
				ImGui.Indent();
				ImGui.Text(ImU8String.op_Implicit("Token: Click 'Reset Token' if necessary, copy the generated token."));
				ImGui.Indent();
				ImGui.Text(ImU8String.op_Implicit("Paste this token into the 'Bot Token' input box above."));
				ImGui.Unindent();
				ImGui.Text(ImU8String.op_Implicit("Privileged Gateway Intents: Enable all 3"));
				ImGui.Text(ImU8String.op_Implicit("Click 'Save' button."));
				ImGui.Unindent();
				ImGui.Text(ImU8String.op_Implicit("6. Click 'Connect' above."));
				ImGui.TreePop();
			}
		}
		else if (!Plugin.Config.Discord.UsePyonServer && isConnected && StringExtensions.IsNullOrWhitespace(Plugin.Config.Discord.UserKey))
		{
			ImGuiEx.IconWarningText("TriggerPyon needs to link your Discord ID to retrieve your Discord activity.", wrapped: true);
			ImGui.Indent();
			ImGui.Text(ImU8String.op_Implicit("1. Copy this key:"));
			if (StringExtensions.IsNullOrWhitespace(plugin.DiscordManager.VerificationKey))
			{
				plugin.DiscordManager.GenerateVerificationKey();
			}
			else
			{
				ImGui.SameLine();
				ImGuiEx.DrawStyledText(plugin.DiscordManager.VerificationKey + "##genKey", 16u, "Click to copy the key.", delegate
				{
					//IL_0010: Unknown result type (might be due to invalid IL or missing references)
					ImGui.SetClipboardText(ImU8String.op_Implicit(plugin.DiscordManager.VerificationKey));
				});
			}
			ImGui.Text(ImU8String.op_Implicit("2. Paste key in private DM to your bot."));
			ImGui.Text(ImU8String.op_Implicit("3. After linking, you can setup Honorific Title below with any Discord activity."));
			ImGui.Unindent();
		}
		ImGui.TreePop();
	}

	private void DrawTriggerDiscordCounter(Trigger trigger)
	{
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0305: Unknown result type (might be due to invalid IL or missing references)
		//IL_030a: Unknown result type (might be due to invalid IL or missing references)
		//IL_010c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0366: Unknown result type (might be due to invalid IL or missing references)
		//IL_036b: Unknown result type (might be due to invalid IL or missing references)
		//IL_03cb: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d0: Unknown result type (might be due to invalid IL or missing references)
		if (!ImGuiEx.TreeNode("Honorific Title##counterTitle", null, default(Vector4), (ImGuiTreeNodeFlags)0))
		{
			return;
		}
		if (!(trigger.Counter is DiscordCounter))
		{
			trigger.Counter = new DiscordCounter();
			Plugin.Config.Save();
		}
		DiscordCounter resolvedCounter = trigger.Counter as DiscordCounter;
		if (resolvedCounter != null)
		{
			ImGuiIOPtr iO = ImGui.GetIO();
			ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
			ImU8String val = ImU8String.op_Implicit("##activityType");
			ImU8String val2 = default(ImU8String);
			((ImU8String)(ref val2))._002Ector(10, 1);
			((ImU8String)(ref val2)).AppendLiteral("Activity: ");
			((ImU8String)(ref val2)).AppendFormatted<DiscordActivityType>(resolvedCounter.ActivityType);
			if (ImGui.BeginCombo(val, val2, (ImGuiComboFlags)0))
			{
				foreach (DiscordActivityType value in Enum.GetValues(typeof(DiscordActivityType)))
				{
					if (value == DiscordActivityType.Streaming || value == DiscordActivityType.Watching)
					{
						continue;
					}
					bool flag = resolvedCounter.ActivityType == value;
					if (ImGui.Selectable(ImU8String.op_Implicit(value.ToString()), flag, (ImGuiSelectableFlags)0, default(Vector2)))
					{
						bool num = resolvedCounter.ActivityType != value;
						resolvedCounter.ActivityType = value;
						if (num || resolvedCounter.TitleTemplates.Count == 0)
						{
							ApplyDefaultDiscordActivity(resolvedCounter);
						}
						Plugin.Config.Save();
					}
				}
				ImGui.EndCombo();
			}
			ImGuiEx.SetItemTooltip("Which Discord activity to display for this title:\n\nPlaying: The game you are playing.\nListening: The song you are listening to.\nCustom: A custom activity you have set.", (ImGuiHoveredFlags)0);
			ImGui.SameLine();
			if (ImGui.Button(ImU8String.op_Implicit("Apply Default##applyDefault"), default(Vector2)))
			{
				ApplyDefaultDiscordActivity(resolvedCounter);
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Apply default template setup for the selected activity.", (ImGuiHoveredFlags)0);
			ImGui.Spacing();
			if (ImGuiEx.Checkbox("##interruptable", resolvedCounter.Interruptable, delegate(bool x)
			{
				resolvedCounter.Interruptable = x;
			}))
			{
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Whether display of this title can be interrupted by titles set by text/emote triggers.", (ImGuiHoveredFlags)0);
			ImGui.SameLine();
			if (ImGuiEx.Checkbox("##counterPrefix", resolvedCounter.TitlePrefix, delegate(bool x)
			{
				resolvedCounter.TitlePrefix = x;
			}))
			{
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Prefix this title above your player name.", (ImGuiHoveredFlags)0);
			ImGui.SameLine();
			if (ImGuiEx.ColorPicker3("", "counterColour", resolvedCounter.TitleColour, delegate(Vector3 x)
			{
				resolvedCounter.TitleColour = x;
			}))
			{
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Title text colour.", (ImGuiHoveredFlags)0);
			ImGui.SameLine();
			if (ImGuiEx.HonorificGlowPicker("", "counterGlow", resolvedCounter.TitleGlow, resolvedCounter.TitleGradientColorSet, resolvedCounter.TitleGradientAnimationStyle, delegate(Vector3 glow, int? set, GradientAnimationStyle? style)
			{
				resolvedCounter.TitleGlow = glow;
				resolvedCounter.TitleGradientColorSet = set;
				resolvedCounter.TitleGradientAnimationStyle = style;
			}))
			{
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Title text glow.", (ImGuiHoveredFlags)0);
			ImGui.SameLine();
			iO = ImGui.GetIO();
			ImGui.SetNextItemWidth(60f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
			if (ImGuiEx.DragInt("##counterDuration", resolvedCounter.Duration, delegate(int x)
			{
				resolvedCounter.Duration = x;
			}, 100f, 1000))
			{
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Duration in milliseconds that each title line will be displayed for.", (ImGuiHoveredFlags)0);
			ImGui.SameLine();
			iO = ImGui.GetIO();
			ImGui.SetNextItemWidth(60f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
			if (ImGuiEx.DragInt("##counterFreq", resolvedCounter.Frequency, delegate(int x)
			{
				resolvedCounter.Frequency = x;
			}, 100f))
			{
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Frequency in milliseconds determining the time to wait between displaying the title.\nA value of '0' will cause the title to always be displayed unless interrupted by another trigger.", (ImGuiHoveredFlags)0);
			ImGui.Spacing();
			for (int num2 = 0; num2 < resolvedCounter.TitleTemplates.Count; num2++)
			{
				iO = ImGui.GetIO();
				ImGui.SetNextItemWidth(160f * ((ImGuiIOPtr)(ref iO)).FontGlobalScale);
				if (ImGuiEx.DrawHonorificTitle(resolvedCounter, $"##lineTemplate{num2}", num2))
				{
					Plugin.Config.Save();
				}
				ImGuiEx.SetItemTooltip("Honorific Title template.\n\n" + $"{resolvedCounter.ActivityType} Activity supports the below formatting:\n" + GetDiscordActivityFormatTooltip(resolvedCounter), (ImGuiHoveredFlags)0);
				if (resolvedCounter.TitleTemplates[num2].Length > 24)
				{
					ImGui.SameLine();
					ImGuiEx.IconWarningTooltip($"Current raw title line length is {resolvedCounter.TitleTemplates[num2].Length} characters (before template replacements).\nHonorific will not display title if it's over 32 characters in length.");
				}
				if (num2 != 0)
				{
					ImGui.SameLine();
					if (ImGuiEx.IconButton((FontAwesomeIcon)61526, $"removeLine{num2}"))
					{
						resolvedCounter.TitleTemplates.RemoveAt(num2);
						Plugin.Config.Save();
						break;
					}
					ImGuiEx.SetItemTooltip("Remove this line.", (ImGuiHoveredFlags)0);
				}
				ImGui.SameLine();
				if (ImGuiEx.IconButton((FontAwesomeIcon)61525, $"addLine{num2}"))
				{
					resolvedCounter.TitleTemplates.Add("");
					Plugin.Config.Save();
					break;
				}
				ImGuiEx.SetItemTooltip("Add new line.", (ImGuiHoveredFlags)0);
			}
			ImGui.Spacing();
			DrawDiscordActivityHelp(resolvedCounter);
		}
		ImGui.TreePop();
	}

	private void ApplyDefaultDiscordActivity(DiscordCounter counter)
	{
		counter.TitleTemplates.Clear();
		switch (counter.ActivityType)
		{
		case DiscordActivityType.Listening:
			counter.TitleTemplates.Add("♪ %artist% ♪");
			counter.TitleTemplates.Add("♪ %title% ♪");
			break;
		case DiscordActivityType.Playing:
			counter.TitleTemplates.Add("\ue03a %game% \ue03a");
			break;
		case DiscordActivityType.Custom:
			counter.TitleTemplates.Add("%status%");
			break;
		case DiscordActivityType.Streaming:
		case DiscordActivityType.Watching:
			break;
		}
	}

	private string GetDiscordActivityFormatTooltip(DiscordCounter counter)
	{
		return counter.ActivityType switch
		{
			DiscordActivityType.Listening => "%artist% - Song Artist\n%title% - Song Title", 
			DiscordActivityType.Playing => "%game% - Game Name", 
			DiscordActivityType.Custom => "%status% - Custom Status", 
			_ => "", 
		};
	}

	private void DrawDiscordActivityHelp(DiscordCounter counter)
	{
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_0124: Unknown result type (might be due to invalid IL or missing references)
		//IL_0138: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_015b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		switch (counter.ActivityType)
		{
		case DiscordActivityType.Listening:
			if (ImGuiEx.TreeNode("Spotify Activity Setup##spotifySetup", null, default(Vector4), (ImGuiTreeNodeFlags)0))
			{
				ImGui.Text(ImU8String.op_Implicit("1. Discord > User Settings > Activity Privacy"));
				ImGui.Indent();
				ImGui.Text(ImU8String.op_Implicit("Ensure 'Share my activity' is enabled & share it with the connected server."));
				ImGui.Unindent();
				ImGui.Text(ImU8String.op_Implicit("2. Discord > User Settings > Connections"));
				ImGui.Indent();
				ImGui.Text(ImU8String.op_Implicit("Connect your Spotify account to Discord."));
				ImGui.Text(ImU8String.op_Implicit("Ensure 'Display Spotify as your status' is enabled."));
				ImGui.Unindent();
				ImGui.Text(ImU8String.op_Implicit("3. Play a song on Spotify & your title should update."));
				ImGui.Indent();
				ImGui.Text(ImU8String.op_Implicit("Also ensure you have the 'Honorific' plugin installed."));
				ImGui.Unindent();
				ImGui.TreePop();
			}
			break;
		case DiscordActivityType.Playing:
			if (ImGuiEx.TreeNode("Game Activity Setup##gameSetup", null, default(Vector4), (ImGuiTreeNodeFlags)0))
			{
				ImGui.Text(ImU8String.op_Implicit("1. Discord > User Settings > Activity Privacy"));
				ImGui.Indent();
				ImGui.Text(ImU8String.op_Implicit("Ensure 'Share my activity' is enabled & share it with the connected server."));
				ImGui.Unindent();
				ImGui.Text(ImU8String.op_Implicit("2. Discord > User Settings > Registered Games"));
				ImGui.Indent();
				ImGui.Text(ImU8String.op_Implicit("Ensure any game you want to monitor is listed & detection is enabled."));
				ImGui.Unindent();
				ImGui.Text(ImU8String.op_Implicit("3. When any detected game is started, your title should update."));
				ImGui.Indent();
				ImGui.Text(ImU8String.op_Implicit("Also ensure you have the 'Honorific' plugin installed."));
				ImGui.Text(ImU8String.op_Implicit("Note that FFXIV will be ignored for obvious reasons."));
				ImGui.Unindent();
				ImGui.TreePop();
			}
			break;
		case DiscordActivityType.Custom:
			if (ImGuiEx.TreeNode("Custom Activity Setup##customSetup", null, default(Vector4), (ImGuiTreeNodeFlags)0))
			{
				ImGui.Text(ImU8String.op_Implicit("1. Discord > User Settings > Activity Privacy"));
				ImGui.Indent();
				ImGui.Text(ImU8String.op_Implicit("Ensure 'Share my activity' is enabled & share it with the connected server."));
				ImGui.Unindent();
				ImGui.Text(ImU8String.op_Implicit("2. When your custom status changes, your title should update."));
				ImGui.Indent();
				ImGui.Text(ImU8String.op_Implicit("Also ensure you have the 'Honorific' plugin installed."));
				ImGui.Unindent();
				ImGui.TreePop();
			}
			break;
		case DiscordActivityType.Streaming:
		case DiscordActivityType.Watching:
			break;
		}
	}

	public override void OnClose()
	{
		DrawRangePreview = false;
		((Window)this).OnClose();
	}
}
