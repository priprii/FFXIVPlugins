using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using ImGuiNET;

namespace PartyRangePyon;

public class PartyMemberOverlay
{
	private readonly int memberIndex;

	private static ImDrawListPtr DrawList => ImGui.GetBackgroundDrawList();

	private unsafe static AddonPartyList* Addon => (AddonPartyList*)Plugin.GameGui.GetAddonByName("_PartyList", 1);

	private unsafe static AgentHUD* Agent => AgentHUD.Instance();

	private unsafe HudPartyMember HudData => ((AgentHUD)Agent).PartyMembers[memberIndex];

	private unsafe PartyListMemberStruct AddonData => ((AddonPartyList)Addon).PartyMembers[memberIndex];

	public uint ObjectId => HudData.EntityId;

	public float Distance { get; set; }

	public unsafe bool IsVisible
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0010: Unknown result type (might be due to invalid IL or missing references)
			if (AddonData.Name != null)
			{
				return ((AtkResNode)(&((AtkTextNode)AddonData.Name).AtkResNode)).IsVisible();
			}
			return false;
		}
	}

	public PartyMemberOverlay(int memberId)
	{
		memberIndex = memberId;
	}

	public void DrawRange()
	{
		string text = $"{Distance:00}";
		try
		{
			text = Distance.ToString(Plugin.Config.TextFormat);
		}
		catch
		{
		}
		DrawRangeText(text, (Distance <= Plugin.Config.CloseRangeMax) ? Plugin.Config.CloseRangeColour : ((Distance <= Plugin.Config.MidRangeMax) ? Plugin.Config.MidRangeColour : Plugin.Config.FarRangeColour));
	}

	private unsafe void DrawRangeText(string text, Vector4 color)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0033: Unknown result type (might be due to invalid IL or missing references)
		//IL_00eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f4: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		if (!Plugin.GameFont.Available)
		{
			return;
		}
		Plugin.GameFont.Push();
		ImFontPtr font = ImGui.GetFont();
		Vector2 screenPosition = GetScreenPosition((AtkResNode*)AddonData.Name);
		ImGuiViewportPtr mainViewport = ImGui.GetMainViewport();
		Vector2 vector = screenPosition + ((ImGuiViewportPtr)(ref mainViewport)).Pos;
		Vector2 vector2 = new Vector2(vector.X + Plugin.Config.TextPosX, vector.Y + Plugin.Config.TextPosY);
		int textOutline = Plugin.Config.TextOutline;
		ImDrawListPtr drawList;
		if (textOutline != 0)
		{
			for (int i = -textOutline; i <= textOutline; i++)
			{
				for (int j = -textOutline; j <= textOutline; j++)
				{
					if (i != 0 || j != 0)
					{
						drawList = DrawList;
						((ImDrawListPtr)(ref drawList)).AddText(font, ((ImFontPtr)(ref font)).FontSize * Plugin.Config.FontScale, vector2 + new Vector2(i, j), ImGui.GetColorU32(Plugin.Config.OutlineColour), text);
					}
				}
			}
		}
		drawList = DrawList;
		((ImDrawListPtr)(ref drawList)).AddText(font, ((ImFontPtr)(ref font)).FontSize * Plugin.Config.FontScale, vector2, ImGui.GetColorU32(color), text);
		Plugin.GameFont.Pop();
	}

	private unsafe void HideJobIcon()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		((AtkResNode)(&((AtkImageNode)AddonData.ClassJobIcon).AtkResNode)).ToggleVisibility(false);
	}

	private unsafe void ShowJobIcon()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		((AtkResNode)(&((AtkImageNode)AddonData.ClassJobIcon).AtkResNode)).ToggleVisibility(true);
	}

	private void ColorPlayerName()
	{
	}

	private unsafe void ResetColorPlayerName()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		Unsafe.Write(&((AtkTextNode)AddonData.Name).EdgeColor, ((AtkTextNode)((PartyListMemberStruct)(&((AddonPartyList)Addon).Chocobo)).Name).EdgeColor);
	}

	public void Reset()
	{
	}

	private unsafe static Vector2 GetScreenPosition(AtkResNode* element)
	{
		AtkResNode* ptr = null;
		AtkResNode* ptr2 = element;
		Stack<Vector2> stack = new Stack<Vector2>();
		while (ptr2 != null)
		{
			stack.Push(new Vector2(((AtkResNode)ptr2).X, ((AtkResNode)ptr2).Y));
			ptr = ptr2;
			ptr2 = ((AtkResNode)ptr2).ParentNode;
		}
		Vector2 vector = stack.Pop();
		Vector2 vector2 = stack.Aggregate((Vector2 a, Vector2 b) => a + b);
		return vector + new Vector2(vector2.X * ((AtkResNode)ptr).ScaleX, vector2.Y * ((AtkResNode)ptr).ScaleY);
	}
}
