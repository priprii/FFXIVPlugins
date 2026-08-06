using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace PvPyon;

public class DefaultPluginData
{
	public Tag AllTags { get; private set; }

	public Tag AllRoleTags { get; private set; }

	public Dictionary<Role, Tag> RoleTags { get; private set; }

	public Dictionary<DpsRole, Tag> DpsRoleTags { get; private set; }

	public Dictionary<RangedDpsRole, Tag> RangedDpsRoleTags { get; private set; }

	public Dictionary<LandHandRole, Tag> LandHandRoleTags { get; private set; }

	public Dictionary<string, Tag> JobTags { get; private set; }

	public Tag AllCustomTags { get; private set; }

	public DefaultPluginData(DefaultPluginDataTemplate template)
	{
		SetupTemplate(template);
	}

	private void SetupTemplate(DefaultPluginDataTemplate template)
	{
		Clear();
		switch (template)
		{
		case DefaultPluginDataTemplate.None:
			SetupTemplateNone();
			break;
		case DefaultPluginDataTemplate.Basic:
			SetupTemplateBasic();
			break;
		case DefaultPluginDataTemplate.Simple:
			SetupTemplateSimple();
			break;
		case DefaultPluginDataTemplate.Full:
			SetupTemplateFull();
			break;
		}
		SetupJobTags();
	}

	private void Clear()
	{
		RoleTags = new Dictionary<Role, Tag>();
		DpsRoleTags = new Dictionary<DpsRole, Tag>();
		RangedDpsRoleTags = new Dictionary<RangedDpsRole, Tag>();
		LandHandRoleTags = new Dictionary<LandHandRole, Tag>();
	}

	private void SetupTemplateNone()
	{
		AllTags = new Tag
		{
			IsSelected = true,
			IsExpanded = true
		};
		AllRoleTags = new Tag
		{
			IsSelected = false,
			IsExpanded = true
		};
		RoleTags[Role.LandHand] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		RoleTags[Role.Tank] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		RoleTags[Role.Healer] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		RoleTags[Role.Dps] = new Tag
		{
			IsSelected = false,
			IsExpanded = true
		};
		DpsRoleTags[DpsRole.Melee] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		DpsRoleTags[DpsRole.Ranged] = new Tag
		{
			IsSelected = false,
			IsExpanded = true
		};
		RangedDpsRoleTags[RangedDpsRole.Magical] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		RangedDpsRoleTags[RangedDpsRole.Physical] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		LandHandRoleTags[LandHandRole.Land] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		LandHandRoleTags[LandHandRole.Hand] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		AllCustomTags = new Tag
		{
			IsSelected = false,
			IsExpanded = true
		};
	}

	private void SetupTemplateBasic()
	{
		AllTags = new Tag
		{
			IsSelected = true,
			IsExpanded = true,
			TagPositionInChat = TagPosition.Before,
			InsertBehindNumberPrefixInChat = true,
			TagPositionInNameplates = TagPosition.Replace,
			TagTargetInNameplates = NameplateElement.Title,
			TargetChatTypes = new List<XivChatType>(Enum.GetValues<XivChatType>()),
			TargetChatTypesIncludeUndefined = true
		};
		AllRoleTags = new Tag
		{
			IsSelected = false,
			IsExpanded = true
		};
		RoleTags[Role.LandHand] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		RoleTags[Role.Tank] = new Tag
		{
			IsSelected = false,
			IsExpanded = false,
			Icon = (BitmapFontIcon)82,
			TextColor = (ushort)546
		};
		RoleTags[Role.Healer] = new Tag
		{
			IsSelected = false,
			IsExpanded = false,
			Icon = (BitmapFontIcon)83,
			TextColor = (ushort)43
		};
		RoleTags[Role.Dps] = new Tag
		{
			IsSelected = false,
			IsExpanded = true,
			Icon = (BitmapFontIcon)84,
			TextColor = (ushort)508
		};
		DpsRoleTags[DpsRole.Melee] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		DpsRoleTags[DpsRole.Ranged] = new Tag
		{
			IsSelected = false,
			IsExpanded = true
		};
		RangedDpsRoleTags[RangedDpsRole.Magical] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		RangedDpsRoleTags[RangedDpsRole.Physical] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		LandHandRoleTags[LandHandRole.Land] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		LandHandRoleTags[LandHandRole.Hand] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		AllCustomTags = new Tag
		{
			IsSelected = false,
			IsExpanded = true,
			IsTextVisibleInChat = false,
			IsTextVisibleInNameplates = true
		};
	}

	private void SetupTemplateSimple()
	{
		AllTags = new Tag
		{
			IsSelected = true,
			IsExpanded = true,
			TagPositionInChat = TagPosition.Before,
			InsertBehindNumberPrefixInChat = true,
			TagPositionInNameplates = TagPosition.After,
			TagTargetInNameplates = NameplateElement.Name,
			IsTextItalic = false,
			IsVisibleInOverworld = false,
			IsVisibleInPveDuties = false,
			IsVisibleInPvpDuties = true,
			IsVisibleForSelf = true,
			IsVisibleForFriendPlayers = true,
			IsVisibleForPartyPlayers = true,
			IsVisibleForAlliancePlayers = true,
			IsVisibleForEnemyPlayers = true,
			IsVisibleForOtherPlayers = true,
			TargetChatTypes = new List<XivChatType>(Enum.GetValues<XivChatType>()),
			TargetChatTypesIncludeUndefined = true
		};
		AllRoleTags = new Tag
		{
			IsSelected = false,
			IsExpanded = true,
			IsRoleIconVisibleInChat = false,
			IsTextVisibleInChat = false,
			IsRoleIconVisibleInNameplates = true,
			IsTextVisibleInNameplates = true,
			IsTextColorAppliedToChatName = false
		};
		RoleTags[Role.LandHand] = new Tag
		{
			IsSelected = false,
			IsExpanded = false,
			Icon = (BitmapFontIcon)85,
			TextColor = (ushort)3
		};
		RoleTags[Role.Tank] = new Tag
		{
			IsSelected = false,
			IsExpanded = false,
			Icon = (BitmapFontIcon)82,
			TextColor = (ushort)546
		};
		RoleTags[Role.Healer] = new Tag
		{
			IsSelected = false,
			IsExpanded = false,
			Icon = (BitmapFontIcon)83,
			TextColor = (ushort)43
		};
		RoleTags[Role.Dps] = new Tag
		{
			IsSelected = false,
			IsExpanded = true,
			Icon = (BitmapFontIcon)84,
			TextColor = (ushort)508
		};
		DpsRoleTags[DpsRole.Melee] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		DpsRoleTags[DpsRole.Ranged] = new Tag
		{
			IsSelected = false,
			IsExpanded = true
		};
		RangedDpsRoleTags[RangedDpsRole.Magical] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		RangedDpsRoleTags[RangedDpsRole.Physical] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		LandHandRoleTags[LandHandRole.Land] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		LandHandRoleTags[LandHandRole.Hand] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		AllCustomTags = new Tag
		{
			IsSelected = false,
			IsExpanded = true,
			IsTextVisibleInChat = false,
			IsTextVisibleInNameplates = true
		};
	}

	private void SetupTemplateFull()
	{
		AllTags = new Tag
		{
			IsSelected = true,
			IsExpanded = true,
			TagPositionInChat = TagPosition.Before,
			InsertBehindNumberPrefixInChat = true,
			TagPositionInNameplates = TagPosition.Replace,
			TagTargetInNameplates = NameplateElement.Title,
			IsTextItalic = true,
			IsVisibleInOverworld = true,
			IsVisibleInPveDuties = true,
			IsVisibleInPvpDuties = true,
			IsVisibleForSelf = true,
			IsVisibleForFriendPlayers = true,
			IsVisibleForPartyPlayers = true,
			IsVisibleForAlliancePlayers = true,
			IsVisibleForEnemyPlayers = true,
			IsVisibleForOtherPlayers = true,
			TargetChatTypes = new List<XivChatType>(Enum.GetValues<XivChatType>()),
			TargetChatTypesIncludeUndefined = true
		};
		AllRoleTags = new Tag
		{
			IsSelected = false,
			IsExpanded = true,
			IsRoleIconVisibleInChat = true,
			IsTextVisibleInChat = true,
			IsRoleIconVisibleInNameplates = true,
			IsTextVisibleInNameplates = true,
			IsTextColorAppliedToNameplateName = true,
			IsTextColorAppliedToChatName = true,
			IsJobIconVisibleInNameplates = true
		};
		RoleTags[Role.LandHand] = new Tag
		{
			IsSelected = false,
			IsExpanded = false,
			Icon = (BitmapFontIcon)85,
			TextColor = (ushort)3
		};
		RoleTags[Role.Tank] = new Tag
		{
			IsSelected = false,
			IsExpanded = false,
			Icon = (BitmapFontIcon)82,
			TextColor = (ushort)546
		};
		RoleTags[Role.Healer] = new Tag
		{
			IsSelected = false,
			IsExpanded = false,
			Icon = (BitmapFontIcon)83,
			TextColor = (ushort)43
		};
		RoleTags[Role.Dps] = new Tag
		{
			IsSelected = false,
			IsExpanded = true,
			Icon = (BitmapFontIcon)84,
			TextColor = (ushort)508
		};
		DpsRoleTags[DpsRole.Melee] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		DpsRoleTags[DpsRole.Ranged] = new Tag
		{
			IsSelected = false,
			IsExpanded = true
		};
		RangedDpsRoleTags[RangedDpsRole.Magical] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		RangedDpsRoleTags[RangedDpsRole.Physical] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		LandHandRoleTags[LandHandRole.Land] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		LandHandRoleTags[LandHandRole.Hand] = new Tag
		{
			IsSelected = false,
			IsExpanded = false
		};
		AllCustomTags = new Tag
		{
			IsSelected = false,
			IsExpanded = true,
			IsTextVisibleInChat = true,
			IsTextVisibleInNameplates = true
		};
	}

	private void SetupJobTags()
	{
		JobTags = new Dictionary<string, Tag>();
		ExcelSheet<ClassJob> excelSheet = PluginServices.DataManager.GetExcelSheet<ClassJob>();
		if (excelSheet == null)
		{
			return;
		}
		foreach (KeyValuePair<Role, Tag> roleTag in RoleTags)
		{
			var (role2, _) = (KeyValuePair<Role, Tag>)(ref roleTag);
			foreach (ClassJob item in ((IEnumerable<ClassJob>)excelSheet).Where((ClassJob classJob) => RoleHelper.RolesByRoleId[classJob.Role] == role2 && !string.IsNullOrEmpty(classJob.Abbreviation.RawString)))
			{
				if (!JobTags.ContainsKey(item.Abbreviation.RawString))
				{
					JobTags[item.Abbreviation.RawString] = new Tag
					{
						IsSelected = false,
						IsExpanded = false,
						Text = item.Abbreviation.RawString
					};
				}
			}
		}
	}
}
