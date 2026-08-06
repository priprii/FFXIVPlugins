using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Resolvers;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Lumina.Excel.GeneratedSheets;
using Lumina.Text;
using PvPyon.Api.Icons;
using PvPyon.Api.Nameplates.Tools;
using PvPyon.Api.Tools.Strings;

namespace PvPyon;

public class NameplateTagTargetModifier : TagTargetModifier
{
	private readonly Config Config;

	private readonly PluginData m_PluginData;

	private readonly StatusIconPriorizer statusiconPriorizer;

	private readonly JobIconSets jobIconSets = new JobIconSets();

	private Nameplate? m_Nameplate;

	public NameplateTagTargetModifier(Config config, PluginData pluginData)
	{
		Config = config;
		m_PluginData = pluginData;
		statusiconPriorizer = new StatusIconPriorizer();
		PluginServices.ClientState.Login += ClientState_Login;
		PluginServices.ClientState.Logout += ClientState_Logout;
		Hook();
	}

	public override void Dispose()
	{
		Unhook();
		PluginServices.ClientState.Logout -= ClientState_Logout;
		PluginServices.ClientState.Login -= ClientState_Login;
		base.Dispose();
	}

	private void Hook()
	{
		if (m_Nameplate == null)
		{
			m_Nameplate = new Nameplate();
			if (!m_Nameplate.IsValid)
			{
				m_Nameplate = null;
			}
			if (m_Nameplate != null)
			{
				m_Nameplate.PlayerNameplateUpdated += Nameplate_PlayerNameplateUpdated;
			}
		}
	}

	private void Unhook()
	{
		if (m_Nameplate != null)
		{
			m_Nameplate.PlayerNameplateUpdated -= Nameplate_PlayerNameplateUpdated;
			m_Nameplate.Dispose();
			m_Nameplate = null;
		}
	}

	private void ClientState_Login()
	{
		Hook();
	}

	private void ClientState_Logout()
	{
		Unhook();
	}

	protected override bool IsIconVisible(Tag tag)
	{
		if (tag.IsRoleIconVisibleInNameplates.InheritedValue.HasValue)
		{
			return tag.IsRoleIconVisibleInNameplates.InheritedValue.Value;
		}
		return false;
	}

	protected override bool IsTextVisible(Tag tag)
	{
		if (tag.IsTextVisibleInNameplates.InheritedValue.HasValue)
		{
			return tag.IsTextVisibleInNameplates.InheritedValue.Value;
		}
		return false;
	}

	private void Nameplate_PlayerNameplateUpdated(PlayerNameplateUpdatedArgs args)
	{
		if (Config.Enabled)
		{
			args.Title.Encode();
			int statusIcon = args.IconId;
			AddTagsToNameplate((GameObject)(object)args.PlayerCharacter, args.Name, args.Title, args.FreeCompany, ref statusIcon);
			args.IconId = statusIcon;
		}
	}

	private void AddPayloadChanges(NameplateElement nameplateElement, TagPosition tagPosition, IEnumerable<Payload> payloadChanges, NameplateChanges nameplateChanges, bool forceUsingSingleAnchorPayload)
	{
		if (payloadChanges.Any())
		{
			StringChanges changes = nameplateChanges.GetChanges((NameplateElements)nameplateElement);
			AddPayloadChanges((StringPosition)tagPosition, payloadChanges, changes, forceUsingSingleAnchorPayload);
		}
	}

	private NameplateChanges GenerateEmptyNameplateChanges(SeString name, SeString title, SeString freeCompany)
	{
		NameplateChanges nameplateChanges = new NameplateChanges();
		nameplateChanges.GetProps(NameplateElements.Name).Destination = name;
		nameplateChanges.GetProps(NameplateElements.Title).Destination = title;
		nameplateChanges.GetProps(NameplateElements.FreeCompany).Destination = freeCompany;
		return nameplateChanges;
	}

	private void AddTagsToNameplate(GameObject gameObject, SeString name, SeString title, SeString freeCompany, ref int statusIcon)
	{
		//IL_0100: Unknown result type (might be due to invalid IL or missing references)
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_01f3: Expected O, but got Unknown
		GameObject obj = gameObject;
		PlayerCharacter val = (PlayerCharacter)(object)((obj is PlayerCharacter) ? obj : null);
		int? newStatusIcon = null;
		NameplateChanges nameplateChanges = GenerateEmptyNameplateChanges(name, title, freeCompany);
		ExcelResolver<ClassJob> classJob;
		if ((GameObject)(object)val != (GameObject)null)
		{
			classJob = ((Character)val).ClassJob;
			ClassJob val2 = classJob?.GameData;
			if (val2 != null && m_PluginData.JobTags.TryGetValue(SeString.op_Implicit(val2.Abbreviation), out Tag value) && value.TagTargetInNameplates.InheritedValue.HasValue && value.TagPositionInNameplates.InheritedValue.HasValue)
			{
				checkTag(value);
			}
		}
		bool flag = true;
		if (Config.FilterPlayers && ((Enum)((Character)val).StatusFlags).HasFlag((Enum)(object)(StatusFlags)1) && !((Enum)((Character)val).StatusFlags).HasFlag((Enum)(object)(StatusFlags)64) && ((Character)val).CompanyTag.TextValue != ((Character)PluginServices.ClientState.LocalPlayer).CompanyTag.TextValue)
		{
			flag = false;
			if (!string.IsNullOrWhiteSpace(Config.IncludedNames))
			{
				string[] array = Config.IncludedNames.Split(',', StringSplitOptions.RemoveEmptyEntries);
				for (int i = 0; i < array.Length; i++)
				{
					if (array[i].ToLower().Trim() == ((GameObject)val).Name.TextValue.ToLower())
					{
						flag = true;
						break;
					}
				}
			}
		}
		StringChange change = nameplateChanges.GetChange(NameplateElements.Name, StringPosition.Replace);
		TextPayload item = new TextPayload((flag ? ((GameObject)val).Name.TextValue : "") ?? "");
		change.Payloads.Insert(0, (Payload)(object)item);
		if (newStatusIcon.HasValue)
		{
			StringChange change2 = nameplateChanges.GetChange(NameplateElements.Name, StringPosition.Before);
			NameplateUpdateFactory.ApplyStatusIconWithPrio(ref statusIcon, newStatusIcon.Value, change2, base.ActivityContextManager.CurrentActivityContext, statusiconPriorizer, Config.StatusIconToNameplateText);
		}
		if ((GameObject)(object)val != (GameObject)null && ((GameObject)val).IsDead && Config.ColourDeadNameplate)
		{
			GrayOutNameplate(gameObject, nameplateChanges);
		}
		ApplyNameplateChanges(nameplateChanges);
		if ((GameObject)(object)val != (GameObject)null && ((Character)val).ClassJob.GameData != null && m_PluginData.JobTags.TryGetValue(SeString.op_Implicit(((Character)val).ClassJob.GameData.Abbreviation), out Tag value2))
		{
			applyTextFormatting(value2);
		}
		void applyTextFormatting(Tag tag)
		{
			_ = new SeString[3] { name, title, freeCompany };
			InheritableValue<bool>[] textColorApplied = new InheritableValue<bool>[3] { tag.IsTextColorAppliedToNameplateName, tag.IsTextColorAppliedToNameplateTitle, tag.IsTextColorAppliedToNameplateFreeCompany };
			ApplyTextFormatting(gameObject, tag, (SeString[])(object)new SeString[3] { name, title, freeCompany }, textColorApplied, null);
		}
		void checkTag(Tag tag)
		{
			if (tag.TagTargetInNameplates.InheritedValue.HasValue && tag.TagPositionInNameplates.InheritedValue.HasValue)
			{
				Payload[] payloads = GetPayloads(tag, gameObject);
				if (payloads.Any())
				{
					AddPayloadChanges(tag.TagTargetInNameplates.InheritedValue.Value, tag.TagPositionInNameplates.InheritedValue.Value, payloads, nameplateChanges, forceUsingSingleAnchorPayload: false);
				}
			}
			if (IsTagVisible(tag, gameObject) && !newStatusIcon.HasValue && classJob != null && tag.IsJobIconVisibleInNameplates?.InheritedValue == true)
			{
				newStatusIcon = jobIconSets.GetJobIcon(tag.JobIconSet?.InheritedValue ?? JobIconSetName.Framed, classJob.Id);
			}
		}
	}

	private void GrayOutNameplate(GameObject gameObject, NameplateChanges nameplateChanges)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_002e: Expected O, but got Unknown
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Expected O, but got Unknown
		if (gameObject is PlayerCharacter)
		{
			NameplateElements[] values = Enum.GetValues<NameplateElements>();
			foreach (NameplateElements element in values)
			{
				nameplateChanges.GetChange(element, StringPosition.Before).Payloads.Add((Payload)new UIForegroundPayload((ushort)3));
				nameplateChanges.GetChange(element, StringPosition.After).Payloads.Add((Payload)new UIForegroundPayload((ushort)0));
			}
		}
	}

	protected void ApplyNameplateChanges(NameplateChanges nameplateChanges)
	{
		NameplateUpdateFactory.ApplyNameplateChanges(new NameplateChangesProps
		{
			Changes = nameplateChanges
		});
	}
}
