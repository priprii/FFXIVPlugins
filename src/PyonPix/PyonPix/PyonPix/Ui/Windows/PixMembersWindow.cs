using System.Collections.Generic;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using PyonPix.Config;
using PyonPix.Config.Pix;
using PyonPix.Events;
using PyonPix.Extensions;
using PyonPix.Services;
using PyonPix.Services.Core;
using PyonPix.Services.Game;
using PyonPix.Shared.Extensions;
using PyonPix.Shared.Structs.Pix;
using PyonPix.Shared.Sync.Dto;
using PyonPix.Shared.Sync.Dto.Client;
using PyonPix.Ui.Components;
using PyonPix.Utility;

namespace PyonPix.Ui.Windows;

public class PixMembersWindow : BaseWindow
{
	public IPix? SelectedPix;

	private bool IsOwner;

	private List<SyncedPixMemberDto> Members = new List<SyncedPixMemberDto>();

	private long _selectedMemberCharacterId = -1L;

	private ContextMenu? _memberContextMenu;

	private SyncService SyncService => Services.Get<SyncService>();

	private StateService StateService => Services.Get<StateService>();

	protected override WindowState State
	{
		get
		{
			if (!Config.UI.PixMembers.Collapsed)
			{
				return WindowState.Expanded;
			}
			return WindowState.Collapsed;
		}
	}

	protected override Vector2 ExpandedSize => Config.UI.PixMembers.ExpandedSize;

	protected override Vector2 ExpandedMinSize => new Vector2(420f, 190f);

	protected override Vector2 ExpandedMaxSize => UiUtil.GameResolution;

	protected override void OnCollapsed(Vector2 windowSize)
	{
		Config.UI.PixMembers.ExpandedSize = windowSize;
		Config.Save();
	}

	protected override void SetState(WindowState newState)
	{
		if (State != newState)
		{
			Config.UI.PixMembers.Collapsed = newState == WindowState.Collapsed;
			Config.Save();
		}
	}

	protected override void OnCloseClicked()
	{
		SelectedPix = null;
		((Window)this).IsOpen = false;
	}

	public PixMembersWindow(Configuration config, IServiceContext services, IWindowContext windows)
		: base("Pix Members###PyonPixPixMembers", config, services, windows, (ImGuiWindowFlags)0)
	{
		((Window)this).SizeCondition = (ImGuiCond)4;
		((Window)this).Size = new Vector2(420f, 420f) * ImGuiHelpers.GlobalScale;
		((Window)this).PositionCondition = (ImGuiCond)4;
		((Window)this).Position = UiUtil.CenterWindow(((Window)this).Size.Value);
		SyncService.PixMemberChangeRankSuccess += delegate(PixMemberChangeRankSuccessDto dto)
		{
			_ = dto.PixId == SelectedPix?.Id;
		};
		SyncService.PixMemberChangeRankFailed += delegate(PixMemberChangeRankFailedDto dto)
		{
			_ = dto.PixId == SelectedPix?.Id;
		};
		SyncService.PixMemberRemoveSuccess += delegate(PixMemberRemoveSuccessDto dto)
		{
			_ = dto.PixId == SelectedPix?.Id;
		};
		SyncService.PixMemberRemoveFailed += delegate(PixMemberRemoveFailedDto dto)
		{
			_ = dto.PixId == SelectedPix?.Id;
		};
		SyncService.PremiumStatusChanged += delegate
		{
		};
		SyncService.SyncedPixMembersUpdated += delegate(SyncedPixMembersResponseDto dto)
		{
			if (!(dto.PixId != SelectedPix?.Id))
			{
				Members = dto.Members;
			}
		};
		SyncService.StateChanged += delegate(ConnectionState connectionState, string? statusMessage, StatusType statusType)
		{
			if (connectionState == ConnectionState.Disconnected)
			{
				Toggle(null, isOwner: false);
			}
		};
		SyncService.SyncedPixUnsubscribed += delegate(string pixId)
		{
			if (SelectedPix != null && SelectedPix.Id == pixId)
			{
				Toggle(null, isOwner: false);
			}
		};
		SyncService.SyncedPixDeleted += delegate(string pixId, LocalPix? local)
		{
			if (SelectedPix != null && SelectedPix.Id == pixId)
			{
				Toggle(null, isOwner: false);
			}
		};
	}

	public void Toggle(IPix? pix, bool isOwner)
	{
		Members.Clear();
		if (pix == null || pix == SelectedPix)
		{
			SelectedPix = null;
			IsOwner = false;
			((Window)this).IsOpen = false;
		}
		else
		{
			((Window)this).WindowName = pix.Id + " Members###PyonPixPixMembers";
			SelectedPix = pix;
			IsOwner = isOwner;
			((Window)this).IsOpen = true;
			SyncService.RequestPixMembersAsync(pix.Id);
		}
	}

	public override void Draw()
	{
		base.Draw();
	}

	protected override void DrawContent()
	{
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0155: Unknown result type (might be due to invalid IL or missing references)
		//IL_0169: Unknown result type (might be due to invalid IL or missing references)
		//IL_016b: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_01cd: Unknown result type (might be due to invalid IL or missing references)
		//IL_0226: Unknown result type (might be due to invalid IL or missing references)
		if (SelectedPix == null)
		{
			((Window)this).IsOpen = false;
		}
		if (!((Window)this).IsOpen)
		{
			return;
		}
		ImGui.BeginChild(ImU8String.op_Implicit("##pixMembers"), ImGui.GetContentRegionAvail(), false, (ImGuiWindowFlags)0);
		foreach (SyncedPixMemberDto member in Members)
		{
			ImU8String val = new ImU8String(0, 1);
			((ImU8String)(ref val)).AppendFormatted<long>(member.CharacterId);
			ImGui.PushID(val);
			Vector4 value = member.State switch
			{
				SyncedPixMemberState.Active => UiUtil.RGBA(0, 255, 0, 255f), 
				SyncedPixMemberState.Connected => UiUtil.RGBA(255, 165, 0, 255f), 
				_ => UiUtil.RGBA(255, 0, 0, 255f), 
			};
			string text = member.State switch
			{
				SyncedPixMemberState.Active => "Active", 
				SyncedPixMemberState.Connected => "Connected", 
				_ => "Disconnected", 
			};
			ImGuiEx.IconLabel(id: $"##state_{member.CharacterId}", tooltip: text, tooltipSub: null, color: value, icon: (FontAwesomeIcon)61713, size: UIShared.NormalIconSize, iconScale: 0.5f);
			ImGui.SameLine();
			FontAwesomeIcon val2 = (FontAwesomeIcon)(member.Rank switch
			{
				PixRank.Owner => 62753, 
				PixRank.CoOwner => 62753, 
				_ => 61447, 
			});
			Vector4 value2 = member.Rank switch
			{
				PixRank.Owner => UIShared.PixRankOwner, 
				PixRank.CoOwner => UIShared.PixRankCoOwner, 
				_ => UIShared.PixRankMember, 
			};
			string text2 = member.Rank switch
			{
				PixRank.Owner => "Owner", 
				PixRank.CoOwner => "Co-Owner", 
				_ => "Member", 
			};
			ImGuiEx.IconLabel(val2, $"##rank_{member.CharacterId}", text2, null, color: value2, size: UIShared.NormalIconSize, iconScale: 0.8f);
			ImGui.SameLine();
			ImGuiEx.StyledText(ImU8String.op_Implicit(member.Alias), UIShared.NormalFontSize, 0.8f, 0f, 4f, 0.2f, member.AliasStyle?.AnimationType ?? AnimationType.Static, member.AliasStyle?.ColourA?.ToVector3(), member.AliasStyle?.ColourB?.ToVector3(), member.AliasStyle?.GlowA?.ToVector3(), member.AliasStyle?.GlowB?.ToVector3(), null, null, null, null, float.MaxValue);
			if (member.CharacterId != StateService.LocalPlayerContentId)
			{
				ImGui.SameLine(ImGui.GetContentRegionAvail().X - 30f);
				if (ImGuiEx.IconButton((FontAwesomeIcon)61762, $"##member{member.CharacterId}"))
				{
					_selectedMemberCharacterId = member.CharacterId;
					_memberContextMenu = BuildMemberContextMenu(member);
					_memberContextMenu.Open();
				}
				_memberContextMenu?.Draw();
			}
			ImGui.PopID();
		}
		ImGui.EndChild();
	}

	private ContextMenu BuildMemberContextMenu(SyncedPixMemberDto member)
	{
		List<ContextMenuItem> list = new List<ContextMenuItem>();
		if (IsOwner)
		{
			if (member.Rank != PixRank.CoOwner)
			{
				list.Add(new ContextMenuButton("Promote to Co-Owner", delegate
				{
					if (ImGui.IsKeyDown((ImGuiKey)641))
					{
						ChangeRank(member, PixRank.CoOwner);
					}
				}, closeOnClick: true, (FontAwesomeIcon)62753, null, () => !ImGui.IsKeyDown((ImGuiKey)641), ContextMenuTint.Both, ContextMenuTint.Both, () => (!ImGui.IsKeyDown((ImGuiKey)641)) ? new(string, string)?(("Promote to Co-Owner", "Hold the Control key to confirm.")) : new(string, string)?(("Promote to Co-Owner", null))));
			}
			else
			{
				list.Add(new ContextMenuButton("Demote to Member", delegate
				{
					if (ImGui.IsKeyDown((ImGuiKey)641))
					{
						ChangeRank(member, PixRank.Member);
					}
				}, closeOnClick: true, (FontAwesomeIcon)61447, null, () => !ImGui.IsKeyDown((ImGuiKey)641), ContextMenuTint.Both, ContextMenuTint.Both, () => (!ImGui.IsKeyDown((ImGuiKey)641)) ? new(string, string)?(("Demote to Member", "Hold the Control key to confirm.")) : new(string, string)?(("Demote to Member", null))));
			}
			list.Add(new ContextMenuButton("Remove", delegate
			{
				if (ImGui.IsKeyDown((ImGuiKey)641))
				{
					RemoveMember(member);
				}
			}, closeOnClick: true, (FontAwesomeIcon)61944, null, () => !ImGui.IsKeyDown((ImGuiKey)641), ContextMenuTint.Both, ContextMenuTint.Both, () => (!ImGui.IsKeyDown((ImGuiKey)641)) ? new(string, string)?(("Remove User", "Hold the Control key to confirm.")) : new(string, string)?(("Remove User", null))));
		}
		list.Add(new ContextMenuButton("Report", delegate
		{
			if (ImGui.IsKeyDown((ImGuiKey)642))
			{
				SyncService.ReportUser(member.CharacterId);
			}
		}, closeOnClick: true, (FontAwesomeIcon)61553, null, () => !ImGui.IsKeyDown((ImGuiKey)642), ContextMenuTint.Both, ContextMenuTint.Both, () => (!ImGui.IsKeyDown((ImGuiKey)642)) ? new(string, string)?(("Report User", "Report this User for service violation.\nFalse reports may have consequences.\n\nHold the Shift key to confirm.")) : new(string, string)?(("Report User", null))));
		return new ContextMenu($"memberCtx{member.CharacterId}", list, 160f, 26f);
	}

	private void ChangeRank(SyncedPixMemberDto member, PixRank newRank)
	{
		SyncService.ChangePixMemberRank(SelectedPix.Id, member.CharacterId, newRank);
	}

	private void RemoveMember(SyncedPixMemberDto member)
	{
		SyncService.RemovePixMember(SelectedPix.Id, member.CharacterId);
	}
}
