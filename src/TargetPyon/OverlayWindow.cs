using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Interface.Colors;
using Dalamud.Interface.GameFonts;
using Dalamud.Interface.ManagedFontAtlas;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace TargetPyon;

public class OverlayWindow : Window
{
	public enum ContentTypes
	{
		NoDuty,
		PvEDuty,
		PvPDuty
	}

	private enum FormatType
	{
		CurrentTarget,
		PreviousTarget,
		PlayersTargetOrTargeter
	}

	private enum Direction
	{
		None,
		Left,
		Right,
		Both
	}

	private readonly Plugin plugin;

	private IFontHandle GameFont;

	private bool FontDirty;

	private DateTime LastFontUpdateTime = DateTime.Now;

	private DateTime LastUpdateTime = DateTime.Now;

	internal List<PlayerEntity> TargetList = new List<PlayerEntity>();

	public ContentTypes ContentType;

	private const ImGuiWindowFlags OverlayFlags = (ImGuiWindowFlags)4201;

	private string TargetDirectionIconBoth
	{
		get
		{
			if (Plugin.Config.Font >= 2)
			{
				return "<>";
			}
			return "⇔";
		}
	}

	private string TargetDirectionIconLeft
	{
		get
		{
			if (Plugin.Config.Font >= 2)
			{
				return "<";
			}
			return "←";
		}
	}

	private string TargetDirectionIconRight
	{
		get
		{
			if (Plugin.Config.Font >= 2)
			{
				return ">";
			}
			return "→";
		}
	}

	public OverlayWindow(Plugin plugin)
		: base("TargetOverlay", (ImGuiWindowFlags)24, false)
	{
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		this.plugin = plugin;
		WindowSizeConstraints value = default(WindowSizeConstraints);
		((WindowSizeConstraints)(ref value))._002Ector();
		((WindowSizeConstraints)(ref value)).MinimumSize = new Vector2(20f, 20f);
		((WindowSizeConstraints)(ref value)).MaximumSize = new Vector2(float.MaxValue, float.MaxValue);
		((Window)this).SizeConstraints = value;
	}

	public void UpdateFont(bool delayUpdate = false)
	{
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		if (delayUpdate)
		{
			FontDirty = true;
			LastFontUpdateTime = DateTime.Now;
		}
		else
		{
			GameFont = Plugin.PluginInterface.UiBuilder.FontAtlas.NewGameFontHandle(new GameFontStyle((GameFontFamily)((Plugin.Config.Font < 1 || Plugin.Config.Font > 6) ? 1 : Plugin.Config.Font), (float)Plugin.Config.FontSize));
		}
	}

	public override void PreDraw()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0043: Unknown result type (might be due to invalid IL or missing references)
		//IL_0048: Unknown result type (might be due to invalid IL or missing references)
		ImGuiStylePtr style = ImGui.GetStyle();
		Vector4 vector = ((ImGuiStylePtr)(ref style)).Colors[2];
		ImGui.PushStyleColor((ImGuiCol)2, new Vector4(vector.X, vector.Y, vector.Z, Plugin.Config.OverlayBGOpacity));
		style = ImGui.GetStyle();
		Vector4 vector2 = ((ImGuiStylePtr)(ref style)).Colors[5];
		ImGui.PushStyleColor((ImGuiCol)5, new Vector4(vector2.X, vector2.Y, vector2.Z, Plugin.Config.OverlayBGOpacity));
	}

	private string FormatText(string name, DateTime targetTime, FormatType formatType, Direction direction = Direction.None)
	{
		string[] array = name.Split(' ');
		string text = array[0];
		string text2 = array[1];
		int hour = targetTime.Hour;
		int minute = targetTime.Minute;
		int second = targetTime.Second;
		switch (formatType)
		{
		case FormatType.CurrentTarget:
			if (string.IsNullOrWhiteSpace(Plugin.Config.CurrentTargetFormat))
			{
				return $"[{hour:00}:{minute:00}] %dir% {name}";
			}
			return Plugin.Config.CurrentTargetFormat.Replace("%h%", $"{hour:00}").Replace("%m%", $"{minute:00}").Replace("%s%", $"{second:00}")
				.Replace("%fn%", text ?? "")
				.Replace("%sn%", text2 ?? "");
		case FormatType.PreviousTarget:
			if (string.IsNullOrWhiteSpace(Plugin.Config.PreviousTargetFormat))
			{
				return $"[{hour:00}:{minute:00}] %dir% {name}";
			}
			return Plugin.Config.PreviousTargetFormat.Replace("%h%", $"{hour:00}").Replace("%m%", $"{minute:00}").Replace("%s%", $"{second:00}")
				.Replace("%fn%", text ?? "")
				.Replace("%sn%", text2 ?? "");
		case FormatType.PlayersTargetOrTargeter:
			if (string.IsNullOrWhiteSpace(Plugin.Config.PlayersTargetFormat))
			{
				return GetDirectionCharacter(direction) + " " + name;
			}
			return Plugin.Config.PlayersTargetFormat.Replace("%d%", GetDirectionCharacter(direction) ?? "").Replace("%fn%", text ?? "").Replace("%sn%", text2 ?? "");
		default:
			return "";
		}
	}

	private string GetDirectionCharacter(Direction direction)
	{
		switch (direction)
		{
		case Direction.Left:
			if (!string.IsNullOrWhiteSpace(Plugin.Config.CustomDirLeft))
			{
				return Plugin.Config.CustomDirLeft;
			}
			return TargetDirectionIconLeft;
		case Direction.Right:
			if (!string.IsNullOrWhiteSpace(Plugin.Config.CustomDirRight))
			{
				return Plugin.Config.CustomDirRight;
			}
			return TargetDirectionIconRight;
		case Direction.Both:
			if (!string.IsNullOrWhiteSpace(Plugin.Config.CustomDirBoth))
			{
				return Plugin.Config.CustomDirBoth;
			}
			return TargetDirectionIconBoth;
		default:
			return "";
		}
	}

	private void DrawText(PlayerEntityInfo? entity, Vector4 col, Vector4 outlineCol, string name, DateTime targetTime, FormatType formatType, Direction direction = Direction.None)
	{
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		string text = FormatText(name, targetTime, formatType, direction);
		if (text.Contains("%dir%"))
		{
			if (entity != null && entity.IsValid && entity.IsNearby && entity.Distance >= (double)Plugin.Config.DirectionIconMinDistance)
			{
				string[] array = text.Split("%dir%");
				string text2 = array[0].Replace("%", "");
				string text3 = array[1].Replace("%", "");
				Vector2 vector = ImGui.CalcTextSize(ImU8String.op_Implicit(text2), false, -1f);
				Vector2 cursorPos = ImGui.GetCursorPos();
				DrawTextWithOutline(col, outlineCol, text2);
				float num = (ImGui.GetTextLineHeightWithSpacing() - ImGui.GetTextLineHeight()) * 3f;
				float num2 = ImGui.GetTextLineHeight() / 3f + (float)Plugin.Config.DirectionIconSizeOffset;
				Vector2 center = ImGui.GetCursorScreenPos() + new Vector2(cursorPos.X + vector.X + num + (float)Plugin.Config.DirectionIconLeftOffset, (0f - vector.Y) / 2f);
				entity.DrawDirection(center, num2, Plugin.Config.FontOutline, col, outlineCol);
				ImGui.SetCursorPos(cursorPos + new Vector2(vector.X + (num2 + num * 2f + (float)Plugin.Config.DirectionIconLeftOffset + (float)Plugin.Config.DirectionIconRightOffset), 0f));
				DrawTextWithOutline(col, outlineCol, text3);
				return;
			}
			text = text.Replace("%dir%", "");
		}
		DrawTextWithOutline(col, outlineCol, text.Replace("%", ""));
	}

	private void DrawTextWithOutline(Vector4 col, Vector4 outlineCol, string text)
	{
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		int fontOutline = Plugin.Config.FontOutline;
		if (fontOutline > 0)
		{
			Vector2 cursorPos = ImGui.GetCursorPos();
			for (int i = -fontOutline; i <= fontOutline; i++)
			{
				for (int j = -fontOutline; j <= fontOutline; j++)
				{
					ImGui.SetCursorPos(cursorPos + new Vector2(i, j));
					ImGui.TextColored(ref outlineCol, ImU8String.op_Implicit(text));
				}
			}
			ImGui.SetCursorPos(cursorPos);
		}
		ImGui.TextColored(ref col, ImU8String.op_Implicit(text));
	}

	public override void Draw()
	{
		//IL_0086: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Unknown result type (might be due to invalid IL or missing references)
		if (!Plugin.Config.Enabled || Plugin.Objects.LocalPlayer == null)
		{
			return;
		}
		if (FontDirty && LastFontUpdateTime.AddMilliseconds(250.0) < DateTime.Now)
		{
			FontDirty = false;
			LastFontUpdateTime = DateTime.Now;
			UpdateFont();
		}
		if (!GameFont.Available)
		{
			return;
		}
		((Window)this).Flags = (ImGuiWindowFlags)4201;
		if (Plugin.Config.LockPosition)
		{
			((Window)this).Flags = (ImGuiWindowFlags)(((Window)this).Flags | 4);
		}
		if (Plugin.Config.ClickThrough)
		{
			bool num = Plugin.Config.ClickThroughBypassCtrl && Input.IsCtrlDown;
			bool flag = Plugin.Config.ClickThroughBypassShift && Input.IsShiftDown;
			bool flag2 = Plugin.Config.ClickThroughBypassAlt && Input.IsAltDown;
			if (!num && !flag && !flag2)
			{
				((Window)this).Flags = (ImGuiWindowFlags)(((Window)this).Flags | 0xC0200);
			}
		}
		((Window)this).Size = new Vector2(Plugin.Config.OverlayWidth, Plugin.Config.OverlayHeight);
		if (Plugin.Config.CustomizationMode)
		{
			DrawCustomizationMode();
			return;
		}
		UpdateEntities();
		DrawPlayerList();
	}

	private void UpdateEntities()
	{
		if (Plugin.Objects.LocalPlayer == null || !(LastUpdateTime.AddMilliseconds(Plugin.Config.UpdateMs) < DateTime.Now))
		{
			return;
		}
		LastUpdateTime = DateTime.Now;
		EntityManager.UpdateObjectVisibility();
		EntityManager.UpdatePlayerVisibility();
		if (EntityManager.NearbyPlayers == null)
		{
			return;
		}
		foreach (PlayerEntityInfo player in EntityManager.NearbyPlayers)
		{
			if (player.Character == null)
			{
				continue;
			}
			if (player.IsTargetingMe)
			{
				bool flag = CanPlaySound(player.Character);
				PlayerEntity playerEntity = TargetList.Find((PlayerEntity x) => x.Instance.Name == player.Name);
				if (playerEntity != null)
				{
					if (flag && playerEntity.TargetTime.AddSeconds(10.0) < DateTime.Now)
					{
						plugin.PlaySound(playerEntity.Instance.Name);
					}
					playerEntity.TargetTime = DateTime.Now;
					playerEntity.Instance = player;
				}
				else
				{
					playerEntity = new PlayerEntity(player);
					if (flag)
					{
						plugin.PlaySound(playerEntity.Instance.Name);
					}
					TargetList.Add(playerEntity);
				}
				TargetList.Sort((PlayerEntity x, PlayerEntity y) => y.TargetTime.CompareTo(x.TargetTime));
			}
			else
			{
				PlayerEntity playerEntity2 = TargetList.Find((PlayerEntity x) => x.Instance.Name == player.Name);
				if (playerEntity2 != null)
				{
					playerEntity2.Instance = player;
				}
			}
		}
		foreach (PlayerEntity player2 in TargetList)
		{
			player2.Instance.IsNearby = EntityManager.NearbyPlayers.Find((PlayerEntityInfo x) => x.Name == player2.Instance.Name) != null;
		}
		int num = (Plugin.Config.OnlyShowNearbyPlayers ? TargetList.Count((PlayerEntity x) => x.Instance.IsNearby) : TargetList.Count);
		if (num > Plugin.Config.MaxPlayers && TargetList.Count((PlayerEntity x) => x.Instance.IsNearby && x.Instance.IsTargetingMe) < num)
		{
			try
			{
				TargetList.RemoveAt(TargetList.Count - 1);
			}
			catch
			{
			}
		}
		if (Plugin.Config.DisplayTime != 0)
		{
			TargetList.RemoveAll((PlayerEntity x) => x.TargetTime.AddMinutes(Plugin.Config.DisplayTime) < DateTime.Now);
		}
	}

	private bool CanPlaySound(IPlayerCharacter player)
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		if (plugin.IsOccupied())
		{
			return false;
		}
		if (ContentType == ContentTypes.PvPDuty)
		{
			bool flag = ((Enum)((ICharacter)player).StatusFlags).HasFlag((Enum)(object)(StatusFlags)1);
			if (!flag || !Plugin.Config.PvPEnemyAlert)
			{
				if (!flag)
				{
					return Plugin.Config.PvPAllyAlert;
				}
				return false;
			}
			return true;
		}
		if (ContentType != ContentTypes.PvEDuty || !Plugin.Config.PvEAllyAlert)
		{
			if (ContentType == ContentTypes.NoDuty)
			{
				return Plugin.Config.NoDutyAllyAlert;
			}
			return false;
		}
		return true;
	}

	private void DrawCustomizationMode()
	{
		GameFont.Push();
		try
		{
			ImGui.SetWindowFontScale(Plugin.Config.FontScale);
			DrawText(null, Plugin.Config.TargetColour, Plugin.Config.OutlineColour, "Pyon Pyon", DateTime.Now, FormatType.CurrentTarget);
			if (Plugin.Config.ShowTargeters == 1)
			{
				ImGui.Indent((float)Plugin.Config.PlayersTargetIndent * ImGuiHelpers.GlobalScale);
				DrawText(null, Plugin.Config.PlayersTargetColour, Plugin.Config.PlayersTargetOutlineColour, "Myon Myon", DateTime.Now, FormatType.PlayersTargetOrTargeter, Direction.Left);
				ImGui.Indent((float)(-Plugin.Config.PlayersTargetIndent) * ImGuiHelpers.GlobalScale);
			}
			DrawText(null, Plugin.Config.NoTargetColour, Plugin.Config.NoTargetOutlineColour, "Myon Myon", DateTime.Now.AddMinutes(-1.0), FormatType.PreviousTarget);
			if (Plugin.Config.ShowTarget == 1)
			{
				ImGui.Indent((float)Plugin.Config.PlayersTargetIndent * ImGuiHelpers.GlobalScale);
				DrawText(null, Plugin.Config.TargetColour, Plugin.Config.OutlineColour, "Pyon Pyon", DateTime.Now, FormatType.PlayersTargetOrTargeter, Direction.Right);
				ImGui.Indent((float)(-Plugin.Config.PlayersTargetIndent) * ImGuiHelpers.GlobalScale);
			}
			DrawText(null, Plugin.Config.NoTargetColour, Plugin.Config.NoTargetOutlineColour, "Kyon Kyon", DateTime.Now.AddMinutes(-2.0), FormatType.PreviousTarget);
			if (Plugin.Config.ShowTarget == 1 || Plugin.Config.ShowTargeters == 1)
			{
				ImGui.Indent((float)Plugin.Config.PlayersTargetIndent * ImGuiHelpers.GlobalScale);
				DrawText(null, Plugin.Config.PlayersTargetColour, Plugin.Config.PlayersTargetOutlineColour, "Nyan Nyan", DateTime.Now, FormatType.PlayersTargetOrTargeter, Direction.Both);
				ImGui.Indent((float)(-Plugin.Config.PlayersTargetIndent) * ImGuiHelpers.GlobalScale);
			}
		}
		catch
		{
		}
		GameFont.Pop();
		ImGui.Spacing();
	}

	private void DrawPlayerList()
	{
		//IL_0546: Unknown result type (might be due to invalid IL or missing references)
		if (TargetList.Count == 0 || !Plugin.Config.OverlayVisible)
		{
			return;
		}
		GameFont.Push();
		try
		{
			foreach (PlayerEntity p in TargetList)
			{
				if (Plugin.Config.OnlyShowNearbyPlayers && !p.Instance.IsNearby)
				{
					continue;
				}
				bool flag = p.Instance.IsValid && p.Instance.IsNearby && p.Instance.IsTargetingMe;
				ImGui.SetWindowFontScale(Plugin.Config.FontScale);
				DrawText(p.Instance, flag ? Plugin.Config.TargetColour : Plugin.Config.NoTargetColour, flag ? Plugin.Config.OutlineColour : Plugin.Config.NoTargetOutlineColour, p.Instance.Name ?? "", p.TargetTime, (!flag) ? FormatType.PreviousTarget : FormatType.CurrentTarget);
				HandleOverlayItemInteractions(p.Instance, p.Instance.Name);
				if (Plugin.Config.RemoveClickButton != ClickButton.None && ImGui.IsItemClicked((ImGuiMouseButton)Plugin.Config.RemoveClickButton))
				{
					try
					{
						TargetList.Remove(p);
					}
					catch
					{
					}
				}
				else
				{
					if (!p.Instance.IsValid || !p.Instance.IsNearby || !p.Instance.IsPlayerCharacter)
					{
						continue;
					}
					string text = "";
					if (p.Instance.IsTargetValid)
					{
						p.TargetInstance = EntityManager.GetPlayerEntityInfoFromObject(p.Instance.Target);
					}
					if (Plugin.Config.ShowTarget > 0 && p.Instance.IsTargetValid && !p.Instance.IsTargetingMe && p.TargetInstance != null && p.TargetInstance.IsPlayerCharacter)
					{
						if (Plugin.Config.ShowTarget == 1)
						{
							ImGui.Indent((float)Plugin.Config.PlayersTargetIndent * ImGuiHelpers.GlobalScale);
							Direction direction = ((p.TargetInstance.TargetObjectId == p.Instance.ObjectId) ? Direction.Both : Direction.Right);
							flag = p.TargetInstance.IsTargetValid && p.TargetInstance.IsTargetingMe;
							DrawText(p.TargetInstance, flag ? Plugin.Config.TargetColour : Plugin.Config.PlayersTargetColour, flag ? Plugin.Config.OutlineColour : Plugin.Config.PlayersTargetOutlineColour, p.TargetInstance.Name, p.TargetTime, FormatType.PlayersTargetOrTargeter, direction);
							HandleOverlayItemInteractions(p.TargetInstance, p.Instance.Name + p.TargetInstance.Name);
							ImGui.Indent((float)(-Plugin.Config.PlayersTargetIndent) * ImGuiHelpers.GlobalScale);
						}
						else if (ImGui.IsItemHovered())
						{
							text = "Targeting: " + p.TargetInstance.Name;
						}
					}
					if (Plugin.Config.ShowTargeters > 0)
					{
						IEnumerable<PlayerEntityInfo> enumerable = EntityManager.NearbyPlayers.Where((PlayerEntityInfo o) => p.Instance.IsTargetOf(o.GameObject));
						if (enumerable != null && enumerable.Count() > 0)
						{
							if (Plugin.Config.ShowTargeters == 1)
							{
								ImGui.Indent((float)Plugin.Config.PlayersTargetIndent * ImGuiHelpers.GlobalScale);
								foreach (PlayerEntityInfo item in enumerable)
								{
									if (Plugin.Config.ShowTarget != 1 || !item.IsTargetOf(p.Instance.GameObject))
									{
										Direction direction2 = ((!item.IsTargetOf(p.Instance.GameObject)) ? Direction.Left : Direction.Both);
										DrawText(item, Plugin.Config.PlayersTargetColour, Plugin.Config.PlayersTargetOutlineColour, item.Name, p.TargetTime, FormatType.PlayersTargetOrTargeter, direction2);
										HandleOverlayItemInteractions(item, p.Instance.Name + item.Name);
									}
								}
								ImGui.Indent((float)(-Plugin.Config.PlayersTargetIndent) * ImGuiHelpers.GlobalScale);
							}
							else if (ImGui.IsItemHovered())
							{
								if (text != "")
								{
									text += "\n";
								}
								text = text + "Targeted By:\n" + string.Join('\n', enumerable.Select((PlayerEntityInfo x) => x.Name));
							}
						}
					}
					if (text != "")
					{
						ImGui.SetTooltip(ImU8String.op_Implicit(text));
					}
				}
			}
		}
		catch
		{
		}
		GameFont.Pop();
		ImGui.Spacing();
	}

	private void HandleOverlayItemInteractions(PlayerEntityInfo p, string hash)
	{
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		if (p.IsValid && p.IsNearby && Plugin.Config.MarkerSize > 0f && p.IsPlayerCharacter && ((ImGui.IsItemHovered() && Plugin.Config.OnlyShowMarkerOnHover) || (!Plugin.Config.OnlyShowMarkerOnHover && p.IsTargetingMe)) && !p.IsEnemyPlayer)
		{
			MarkPlayer(p, Plugin.Config.MarkerColour, Plugin.Config.MarkerSize);
		}
		if (Plugin.Config.TargetClickButton != ClickButton.None && ImGui.IsItemClicked((ImGuiMouseButton)Plugin.Config.TargetClickButton))
		{
			p.SetAsTarget();
		}
		else if (p.IsPlayerCharacter)
		{
			if (p.IsValid && Plugin.Config.PlateClickButton != ClickButton.None && ImGui.IsItemClicked((ImGuiMouseButton)Plugin.Config.PlateClickButton))
			{
				p.OpenPlate();
			}
			else if (p.IsValid && Plugin.Config.InspectClickButton != ClickButton.None && ImGui.IsItemClicked((ImGuiMouseButton)Plugin.Config.InspectClickButton))
			{
				p.OpenExamine();
			}
			else if (Plugin.Config.ContextClickButton != ClickButton.None && ImGui.IsItemClicked((ImGuiMouseButton)Plugin.Config.ContextClickButton))
			{
				ImU8String val = default(ImU8String);
				((ImU8String)(ref val))._002Ector(19, 2);
				((ImU8String)(ref val)).AppendFormatted<string>(p.Name);
				((ImU8String)(ref val)).AppendFormatted<string>(hash);
				((ImU8String)(ref val)).AppendLiteral("##playerContextMenu");
				ImGui.OpenPopup(val, (ImGuiPopupFlags)0);
			}
			else if (p.IsValid && p.IsNearby && Plugin.Config.CamOrbitClickButton != ClickButton.None && ImGui.IsItemClicked((ImGuiMouseButton)Plugin.Config.CamOrbitClickButton))
			{
				p.ToggleCamTarget();
			}
			DrawPlayerContextMenu(p, hash);
		}
	}

	private unsafe void DrawPlayerContextMenu(PlayerEntityInfo p, string hash)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_014e: Unknown result type (might be due to invalid IL or missing references)
		//IL_017b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_0255: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0222: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_037b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0334: Unknown result type (might be due to invalid IL or missing references)
		//IL_0407: Unknown result type (might be due to invalid IL or missing references)
		//IL_03bd: Unknown result type (might be due to invalid IL or missing references)
		ImU8String val = default(ImU8String);
		((ImU8String)(ref val))._002Ector(19, 2);
		((ImU8String)(ref val)).AppendFormatted<string>(p.Name);
		((ImU8String)(ref val)).AppendFormatted<string>(hash);
		((ImU8String)(ref val)).AppendLiteral("##playerContextMenu");
		if (!ImGui.BeginPopup(val, (ImGuiWindowFlags)0))
		{
			return;
		}
		Vector4 vector = (p.IsValid ? ImGuiColors.DalamudWhite : (*ImGui.GetStyleColorVec4((ImGuiCol)1)));
		ImU8String val2 = default(ImU8String);
		((ImU8String)(ref val2))._002Ector(0, 2);
		((ImU8String)(ref val2)).AppendFormatted<string>(p.Name);
		((ImU8String)(ref val2)).AppendFormatted<string>((!string.IsNullOrWhiteSpace(p.HomeWorld)) ? ("@" + p.HomeWorld) : "");
		ImGui.TextColored(ref vector, val2);
		vector = ImGuiColors.DalamudGrey;
		ImU8String val3 = default(ImU8String);
		((ImU8String)(ref val3))._002Ector(2, 1);
		((ImU8String)(ref val3)).AppendLiteral("Lv");
		((ImU8String)(ref val3)).AppendFormatted<byte>(p.Level);
		ImGui.TextColored(ref vector, val3);
		ImGui.SameLine();
		JobInfo job = p.Job;
		ref readonly Vector4 jobColour = ref job.JobColour;
		ImU8String val4 = default(ImU8String);
		((ImU8String)(ref val4))._002Ector(0, 1);
		((ImU8String)(ref val4)).AppendFormatted<string>(p.Job.Name);
		ImGui.TextColored(ref jobColour, val4);
		if (p.CompanyTag != "")
		{
			ImGui.SameLine();
			vector = ImGuiColors.DalamudGrey;
			ImU8String val5 = default(ImU8String);
			((ImU8String)(ref val5))._002Ector(2, 1);
			((ImU8String)(ref val5)).AppendLiteral("«");
			((ImU8String)(ref val5)).AppendFormatted<string>(p.CompanyTag);
			((ImU8String)(ref val5)).AppendLiteral("»");
			ImGui.TextColored(ref vector, val5);
		}
		if (p.IsMareSynced)
		{
			vector = ImGuiColors.ParsedPink;
			ImU8String val6 = default(ImU8String);
			((ImU8String)(ref val6))._002Ector(11, 0);
			((ImU8String)(ref val6)).AppendLiteral("Mare Synced");
			ImGui.TextColored(ref vector, val6);
		}
		vector = ImGuiColors.DalamudGrey;
		ImU8String val7 = default(ImU8String);
		((ImU8String)(ref val7))._002Ector(10, 1);
		((ImU8String)(ref val7)).AppendLiteral("Distance: ");
		((ImU8String)(ref val7)).AppendFormatted<string>((p.IsValid && p.IsNearby) ? (p.Distance + "y") : "Not Nearby");
		ImGui.TextColored(ref vector, val7);
		ImGui.Separator();
		ImGui.Dummy(new Vector2(0f, 2f));
		if (p.IsValid && p.IsNearby && IPC.PyonCamEnabled && ImGui.Selectable(ImU8String.op_Implicit(p.IsCamTarget ? "Reset Camera" : "Camera Orbit"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			p.ToggleCamTarget();
		}
		if (p.IsValid && p.IsNearby && ImGui.Selectable(ImU8String.op_Implicit("Target"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			p.SetAsTarget();
		}
		if (p.IsValid && p.IsNearby && ImGui.Selectable(ImU8String.op_Implicit("Focus Target"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			p.SetAsFocusTarget();
		}
		if (ImGui.Selectable(ImU8String.op_Implicit("Send Tell"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			p.SendTell();
		}
		if (p.IsValid && ImGui.Selectable(ImU8String.op_Implicit("Adventure Plate"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			p.OpenPlate();
		}
		if (p.IsValid && ImGui.Selectable(ImU8String.op_Implicit("Examine"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			p.OpenExamine();
		}
		if (p.IsValid && p.IsNearby && ImGui.Selectable(ImU8String.op_Implicit("Locate on Map"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			p.FlagAndOpenMap((MapType)1);
		}
		if (ImGui.Selectable(ImU8String.op_Implicit("Open Lodestone"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			p.SearchPlayerOnLodestone();
		}
		if (ImGui.Selectable(ImU8String.op_Implicit("Open Tomestone"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			p.SearchPlayerOnTomestone();
		}
		if (p.IsValid && p.IsNearby && ImGui.Selectable(ImU8String.op_Implicit(p.IsVisible ? "Hide Character" : "Show Character"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			if (p.IsVisible)
			{
				p.Hide();
			}
			else
			{
				p.Show();
			}
		}
		if (p.IsValid && ImGui.Selectable(ImU8String.op_Implicit(p.IsBlocked ? "Remove from Blacklist" : "Add to Blacklist"), false, (ImGuiSelectableFlags)0, default(Vector2)))
		{
			if (p.IsBlocked)
			{
				p.Unblock();
			}
			else
			{
				p.Block();
			}
		}
		ImGui.EndPopup();
	}

	private void MarkPlayer(PlayerEntityInfo p, Vector4 colour, float size)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0054: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		//IL_0079: Unknown result type (might be due to invalid IL or missing references)
		Vector2 vector = default(Vector2);
		if (p.IsValid && p.IsNearby && Plugin.GameGui.WorldToScreen(Vector3.op_Implicit(p.Position), ref vector))
		{
			ImGuiViewportPtr mainViewport = ImGuiHelpers.MainViewport;
			Vector2 pos = ((ImGuiViewportPtr)(ref mainViewport)).Pos;
			mainViewport = ImGuiHelpers.MainViewport;
			Vector2 pos2 = ((ImGuiViewportPtr)(ref mainViewport)).Pos;
			mainViewport = ImGuiHelpers.MainViewport;
			ImGui.PushClipRect(pos, pos2 + ((ImGuiViewportPtr)(ref mainViewport)).Size, false);
			ImDrawListPtr windowDrawList = ImGui.GetWindowDrawList();
			mainViewport = ImGuiHelpers.MainViewport;
			((ImDrawListPtr)(ref windowDrawList)).AddCircleFilled(((ImGuiViewportPtr)(ref mainViewport)).Pos + new Vector2(vector.X, vector.Y), size, ImGui.GetColorU32(colour), 100);
			ImGui.PopClipRect();
		}
	}

	public override void PostDraw()
	{
		ImGui.PopStyleColor(2);
	}
}
