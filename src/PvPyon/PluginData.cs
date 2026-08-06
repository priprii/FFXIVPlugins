using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Party;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Lumina.Excel;

namespace PvPyon;

public class PluginData
{
	public DefaultPluginData Default;

	public Tag AllTags;

	public Tag AllRoleTags;

	public Dictionary<Role, Tag> RoleTags;

	public Dictionary<DpsRole, Tag> DpsRoleTags;

	public Dictionary<RangedDpsRole, Tag> RangedDpsRoleTags;

	public Dictionary<LandHandRole, Tag> LandHandRoleTags;

	public Dictionary<string, Tag> JobTags;

	public Tag AllCustomTags;

	public List<Tag> CustomTags;

	public List<Identity> Identities;

	private Config Config;

	public PluginData(Config config)
	{
		Config = config;
		ReloadDefault();
	}

	public void ReloadDefault()
	{
		Default = new DefaultPluginData(DefaultPluginDataTemplate.Simple);
		AllTags = new Tag(new LiteralPluginString("AllTags"), Default.AllTags);
		AllRoleTags = new Tag(new LiteralPluginString("AllRoleTags"), Default.AllRoleTags);
		RoleTags = new Dictionary<Role, Tag>();
		Role[] values = Enum.GetValues<Role>();
		for (int i = 0; i < values.Length; i++)
		{
			Role key = values[i];
			if (Default.RoleTags.TryGetValue(key, out Tag value))
			{
				RoleTags[key] = new Tag(new LiteralPluginString(key.ToString()), value);
			}
		}
		DpsRoleTags = new Dictionary<DpsRole, Tag>();
		DpsRole[] values2 = Enum.GetValues<DpsRole>();
		for (int i = 0; i < values2.Length; i++)
		{
			DpsRole key2 = values2[i];
			if (Default.DpsRoleTags.TryGetValue(key2, out Tag value2))
			{
				DpsRoleTags[key2] = new Tag(new LiteralPluginString(key2.ToString()), value2);
			}
		}
		RangedDpsRoleTags = new Dictionary<RangedDpsRole, Tag>();
		RangedDpsRole[] values3 = Enum.GetValues<RangedDpsRole>();
		for (int i = 0; i < values3.Length; i++)
		{
			RangedDpsRole key3 = values3[i];
			if (Default.RangedDpsRoleTags.TryGetValue(key3, out Tag value3))
			{
				RangedDpsRoleTags[key3] = new Tag(new LiteralPluginString(key3.ToString()), value3);
			}
		}
		LandHandRoleTags = new Dictionary<LandHandRole, Tag>();
		LandHandRole[] values4 = Enum.GetValues<LandHandRole>();
		for (int i = 0; i < values4.Length; i++)
		{
			LandHandRole key4 = values4[i];
			if (Default.LandHandRoleTags.TryGetValue(key4, out Tag value4))
			{
				LandHandRoleTags[key4] = new Tag(new LiteralPluginString(key4.ToString()), value4);
			}
		}
		JobTags = new Dictionary<string, Tag>();
		string key5;
		Role key6;
		foreach (KeyValuePair<string, Role> item in RoleHelper.RolesByJobAbbreviation)
		{
			item.Deconstruct(out key5, out key6);
			string text = key5;
			if (Default.JobTags.TryGetValue(text, out Tag value5))
			{
				JobTags[text] = new Tag(new LiteralPluginString(text), value5);
			}
		}
		AllCustomTags = new Tag(new LiteralPluginString("AllCustomTags"), Default.AllCustomTags);
		CustomTags = new List<Tag>();
		AllRoleTags.Parent = AllTags;
		Tag value6;
		foreach (KeyValuePair<Role, Tag> roleTag in RoleTags)
		{
			roleTag.Deconstruct(out key6, out value6);
			Role role = key6;
			Tag tag = value6;
			tag.Parent = AllRoleTags;
			switch (role)
			{
			case Role.Dps:
				foreach (KeyValuePair<DpsRole, Tag> dpsRoleTag in DpsRoleTags)
				{
					dpsRoleTag.Deconstruct(out var key8, out value6);
					DpsRole num = key8;
					Tag tag2 = value6;
					tag2.Parent = tag;
					if (num != DpsRole.Ranged)
					{
						continue;
					}
					foreach (KeyValuePair<RangedDpsRole, Tag> rangedDpsRoleTag in RangedDpsRoleTags)
					{
						rangedDpsRoleTag.Deconstruct(out var _, out value6);
						value6.Parent = tag2;
					}
				}
				break;
			case Role.LandHand:
				foreach (KeyValuePair<LandHandRole, Tag> landHandRoleTag in LandHandRoleTags)
				{
					landHandRoleTag.Deconstruct(out var _, out value6);
					value6.Parent = tag;
				}
				break;
			}
		}
		foreach (KeyValuePair<string, Tag> jobTag in JobTags)
		{
			jobTag.Deconstruct(out key5, out value6);
			string key10 = key5;
			Tag tag3 = value6;
			if (!RoleHelper.RolesByJobAbbreviation.TryGetValue(key10, out var _))
			{
				continue;
			}
			LandHandRole value10;
			if (RoleHelper.DpsRolesByJobAbbreviation.TryGetValue(key10, out var value8))
			{
				if (RoleHelper.RangedDpsRolesByJobAbbreviation.TryGetValue(key10, out var value9))
				{
					tag3.Parent = RangedDpsRoleTags[value9];
				}
				else
				{
					tag3.Parent = DpsRoleTags[value8];
				}
			}
			else if (RoleHelper.LandHandRolesByJobAbbreviation.TryGetValue(key10, out value10))
			{
				tag3.Parent = LandHandRoleTags[value10];
			}
			else
			{
				tag3.Parent = RoleTags[RoleHelper.RolesByJobAbbreviation[key10]];
			}
		}
		AllCustomTags.Parent = AllTags;
		foreach (Tag customTag in CustomTags)
		{
			customTag.Parent = AllCustomTags;
		}
		bool flag = false;
		foreach (Tag customTag2 in CustomTags)
		{
			if (customTag2.CustomId.Value == Guid.Empty)
			{
				customTag2.CustomId.Behavior = InheritableBehavior.Enabled;
				customTag2.CustomId.Value = Guid.NewGuid();
				flag = true;
			}
			string[] identitiesToAddTo = customTag2.IdentitiesToAddTo;
			foreach (string identityToAddTo in identitiesToAddTo)
			{
				Identity identity = Identities.FirstOrDefault((Identity identity2) => identity2.Name.ToLower() == identityToAddTo.ToLower());
				if (identity == null)
				{
					identity = new Identity(identityToAddTo);
					Identities.Add(identity);
				}
				if (identity != null)
				{
					identity.CustomTagIds.Add(customTag2.CustomId.Value);
					flag = true;
				}
			}
			if (customTag2.GameObjectNamesToApplyTo.Behavior != InheritableBehavior.Inherit)
			{
				customTag2.GameObjectNamesToApplyTo.Behavior = InheritableBehavior.Inherit;
				customTag2.GameObjectNamesToApplyTo.Value = "";
				flag = true;
			}
		}
		if (flag)
		{
			Config.Save();
		}
	}

	public void AddCustomTagToIdentity(Tag customTag, Identity identity)
	{
		if (!identity.CustomTagIds.Contains(customTag.CustomId.Value))
		{
			identity.CustomTagIds.Add(customTag.CustomId.Value);
		}
		if (!Identities.Contains(identity))
		{
			Identities.Add(identity);
		}
	}

	public void RemoveCustomTagFromIdentity(Tag customTag, Identity identity)
	{
		identity.CustomTagIds.Remove(customTag.CustomId.Value);
		if (!identity.CustomTagIds.Any())
		{
			Identities.Remove(identity);
		}
	}

	public void RemoveCustomTagFromIdentities(Tag customTag)
	{
		Identity[] array = Identities.ToArray();
		foreach (Identity identity in array)
		{
			RemoveCustomTagFromIdentity(customTag, identity);
		}
	}

	public Identity GetIdentity(string name, uint? worldId)
	{
		return new Identity(name)
		{
			WorldId = worldId
		};
	}

	public Identity GetIdentity(PlayerCharacter playerCharacter)
	{
		return GetIdentity(((GameObject)playerCharacter).Name.TextValue, playerCharacter.HomeWorld.Id);
	}

	public Identity GetIdentity(PartyMember partyMember)
	{
		return GetIdentity(partyMember.Name.TextValue, partyMember.World.Id);
	}

	public Identity GetIdentity(PlayerPayload playerPayload)
	{
		return GetIdentity(playerPayload.PlayerName, ((ExcelRow)playerPayload.World).RowId);
	}
}
