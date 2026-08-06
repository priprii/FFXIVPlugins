using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;

namespace TriggerPyon;

public class UpdatesWindow : Window
{
	private readonly Plugin plugin;

	public UpdatesWindow(Plugin plugin)
		: base("TriggerPyon Changelog", (ImGuiWindowFlags)2097184, false)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		this.plugin = plugin;
		((Window)this).SizeCondition = (ImGuiCond)4;
		((Window)this).Size = new Vector2(660f, 440f) * ImGuiHelpers.GlobalScale;
		WindowSizeConstraints value = default(WindowSizeConstraints);
		((WindowSizeConstraints)(ref value))._002Ector();
		((WindowSizeConstraints)(ref value)).MinimumSize = new Vector2(400f, 400f) * ImGuiHelpers.GlobalScale;
		((Window)this).SizeConstraints = value;
		((Window)this).AllowClickthrough = false;
		((Window)this).AllowPinning = false;
	}

	public override void Draw()
	{
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		if (((Window)this).IsOpen)
		{
			ImGui.Separator();
			ImGui.Spacing();
			DrawUpdateList();
			ImGui.Spacing();
			ImGui.Separator();
			ImGui.Spacing();
			if (ImGuiEx.Checkbox("Show Changelog on Update##showUpdates", Plugin.Config.ShowUpdates, delegate(bool x)
			{
				Plugin.Config.ShowUpdates = x;
			}))
			{
				Plugin.Config.Save();
			}
			ImGuiEx.SetItemTooltip("Whether to display this changelog when the TriggerPyon plugin is updated.", (ImGuiHoveredFlags)0);
			ImGui.Spacing();
			if (ImGui.Button(ImU8String.op_Implicit("Close Changelog"), default(Vector2)))
			{
				((Window)this).IsOpen = false;
			}
			ImGui.SameLine();
			if (ImGui.Button(ImU8String.op_Implicit("Open TriggerPyon"), default(Vector2)))
			{
				((Window)plugin.MainWindow).IsOpen = true;
			}
		}
	}

	private void DrawUpdateList()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00de: Unknown result type (might be due to invalid IL or missing references)
		//IL_010e: Unknown result type (might be due to invalid IL or missing references)
		//IL_011d: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_013b: Unknown result type (might be due to invalid IL or missing references)
		//IL_014a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Unknown result type (might be due to invalid IL or missing references)
		//IL_0168: Unknown result type (might be due to invalid IL or missing references)
		//IL_0198: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fb: Unknown result type (might be due to invalid IL or missing references)
		//IL_020a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_0269: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_0287: Unknown result type (might be due to invalid IL or missing references)
		//IL_0296: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_02c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0328: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_0346: Unknown result type (might be due to invalid IL or missing references)
		//IL_035a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0369: Unknown result type (might be due to invalid IL or missing references)
		//IL_037d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0391: Unknown result type (might be due to invalid IL or missing references)
		//IL_03a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_03c8: Unknown result type (might be due to invalid IL or missing references)
		//IL_03d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_03e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_03fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0409: Unknown result type (might be due to invalid IL or missing references)
		//IL_0439: Unknown result type (might be due to invalid IL or missing references)
		//IL_0448: Unknown result type (might be due to invalid IL or missing references)
		//IL_0457: Unknown result type (might be due to invalid IL or missing references)
		//IL_0466: Unknown result type (might be due to invalid IL or missing references)
		//IL_0496: Unknown result type (might be due to invalid IL or missing references)
		//IL_04a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_04c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0516: Unknown result type (might be due to invalid IL or missing references)
		//IL_0546: Unknown result type (might be due to invalid IL or missing references)
		if (ImGuiEx.TreeNode("v1.2.1.0 - 2026.04.29", null, default(Vector4), (ImGuiTreeNodeFlags)32))
		{
			ImGui.TextWrapped(ImU8String.op_Implicit("• Api15 Update."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Fixed issue of text reactions queueing incorrectly."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added internal filter for instigator to prevent triggering from blocked players."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• In addition to the above, you can assign player names to an instigator blacklist without having the player blocked."));
			ImGui.TreePop();
		}
		ImGui.Spacing();
		if (ImGuiEx.TreeNode("v1.2.0.3 - 2026.03.04", null, default(Vector4), (ImGuiTreeNodeFlags)0))
		{
			ImGui.TextWrapped(ImU8String.op_Implicit("v1.2.0.1"));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Fixed chat trigger events for tell/party/alliance and when instigator is assigned to a friend group."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added race/gender conditions."));
			ImGui.Separator();
			ImGui.TextWrapped(ImU8String.op_Implicit("v1.2.0.2"));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Minor update to fix index error."));
			ImGui.Separator();
			ImGui.TextWrapped(ImU8String.op_Implicit("v1.2.0.3"));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Increase timeout for Discord connection."));
			ImGui.TreePop();
		}
		ImGui.Spacing();
		if (ImGuiEx.TreeNode("v1.2.0.0 - 2026.01.21", null, default(Vector4), (ImGuiTreeNodeFlags)0))
		{
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added 'Status' condition to Text trigger Event Receiver."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added 'Copy Instigator' option to Text reactions for copying message sent by instigator."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added 'Playing' & 'Custom' Discord activity types."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added 'Restrict Territory' reaction option."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Fixed issue where Honorific title would persist when changing zone."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Fixed Text Channel receiver."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Fixed 'MareSynced' relation type."));
			ImGui.TreePop();
		}
		ImGui.Spacing();
		if (ImGuiEx.TreeNode("v1.1.0.7 - 2025.12.28", null, default(Vector4), (ImGuiTreeNodeFlags)0))
		{
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added support for Honorific animated glow options."));
			ImGui.TreePop();
		}
		ImGui.Spacing();
		if (ImGuiEx.TreeNode("v1.1.0.6 - 2025.12.18", null, default(Vector4), (ImGuiTreeNodeFlags)0))
		{
			ImGui.TextWrapped(ImU8String.op_Implicit("• Updated for Patch 7.4"));
			ImGui.TreePop();
		}
		ImGui.Spacing();
		if (ImGuiEx.TreeNode("v1.1.0.5 - 2025.10.30", null, default(Vector4), (ImGuiTreeNodeFlags)0))
		{
			ImGui.TextWrapped(ImU8String.op_Implicit("v1.1.0.2"));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added 'Count Failed Conditions' reaction option which determines whether counter should increment/show title regardless of whether conditions for reaction failed."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added 'Restrict Range' reaction option to only allow reaction to be performed when the instigator is within a specific range around you."));
			ImGui.Indent();
			ImGui.TextWrapped(ImU8String.op_Implicit("• Adjusted the 'Spank Reaction' preset to include range restriction with sensible values as an example."));
			ImGui.Unindent();
			ImGui.Separator();
			ImGui.TextWrapped(ImU8String.op_Implicit("v1.1.0.3"));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Fixed issue with 'Count Failed Conditions' option for triggers that do not have reactions."));
			ImGui.Separator();
			ImGui.TextWrapped(ImU8String.op_Implicit("v1.1.0.4"));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added a few extra preset triggers."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added 'Passthrough Restrictions' reaction option which causes a trigger to be aborted & passed to a lower priority trigger if state/range restrictions fail."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added range restriction condition to text triggers."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Adjusted range restriction logic to support local player as instigator, whereby range restriction will instead be receiver's position relative to you."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Fixed 'Other' emote receiver type incorrectly passing when no receiver exists."));
			ImGui.Separator();
			ImGui.TextWrapped(ImU8String.op_Implicit("v1.1.0.5"));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added 'Targeter' instigator type to trigger events for players targeting you."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Changed 'Player' instigator/receiver type to allow adding multiple player names to match for."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Fixed Status/Relation conditions."));
			ImGui.TreePop();
		}
		ImGui.Spacing();
		if (ImGuiEx.TreeNode("v1.1.0.1 - 2025.10.28", null, default(Vector4), (ImGuiTreeNodeFlags)0))
		{
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added option to change target when performing emote reaction."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added more LookAt options."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added condition to forbid reaction from being performed under specific states."));
			ImGui.Indent();
			ImGui.TextWrapped(ImU8String.op_Implicit("• Eg. Ignore reaction if currently performing a looping dance emote."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• This can also be used to prevent spamming an emote reaction when the event is triggered repeatedly."));
			ImGui.Unindent();
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added option to restore specific states after an emote reaction is performed."));
			ImGui.Indent();
			ImGui.TextWrapped(ImU8String.op_Implicit("• Eg. If you were doing a dance emote prior to the emote reaction, you can continue dancing after the reaction."));
			ImGui.Unindent();
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added initial support for Discord activity triggers."));
			ImGui.Indent();
			ImGui.TextWrapped(ImU8String.op_Implicit("• Currently only supports 'Listening' activity."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Discord triggers can be persistent with frequency set to '0'."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Priority for which Discord trigger is displayed depends on listed order."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Option for whether standard triggers can interrupt a Discord activity trigger."));
			ImGui.Unindent();
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added some preset triggers for easy setup."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Fixed Pyon Discord link."));
			ImGui.TreePop();
		}
		ImGui.Spacing();
		if (ImGuiEx.TreeNode("v1.0.3.0 - 2025.08.27", null, default(Vector4), (ImGuiTreeNodeFlags)0))
		{
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added interruptable reaction queueing."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added command text reactions."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added toast/echo counter messages."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added buttons for previewing counters/reactions."));
			ImGui.TreePop();
		}
		ImGui.Spacing();
		if (ImGuiEx.TreeNode("v1.0.2.1 - 2025.08.14", null, default(Vector4), (ImGuiTreeNodeFlags)0))
		{
			ImGui.TextWrapped(ImU8String.op_Implicit("v1.0.2.0"));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Support for Api13."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added text triggers/reactions."));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Fixed various conditional properties."));
			ImGui.Separator();
			ImGui.TextWrapped(ImU8String.op_Implicit("v1.0.2.1"));
			ImGui.TextWrapped(ImU8String.op_Implicit("• Fixed sig offsets."));
			ImGui.TreePop();
		}
		ImGui.Spacing();
		if (ImGuiEx.TreeNode("v1.0.1.0 - 2025.08.01", null, default(Vector4), (ImGuiTreeNodeFlags)0))
		{
			ImGui.TextWrapped(ImU8String.op_Implicit("• Added maintain LookAt option for emote reactions."));
			ImGui.TreePop();
		}
		ImGui.Spacing();
		if (ImGuiEx.TreeNode("v1.0.0.0 - 2025.07.31", null, default(Vector4), (ImGuiTreeNodeFlags)0))
		{
			ImGui.TextWrapped(ImU8String.op_Implicit("• Initial beta release."));
			ImGui.TreePop();
		}
	}

	public override void OnClose()
	{
		((Window)this).OnClose();
	}
}
