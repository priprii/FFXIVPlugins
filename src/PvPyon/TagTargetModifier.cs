using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using PvPyon.Api.ActivityContexts;
using PvPyon.Api.Tools.Strings;

namespace PvPyon;

public abstract class TagTargetModifier : IDisposable
{
	public ActivityContextManager ActivityContextManager { get; init; }

	public TagTargetModifier()
	{
		ActivityContextManager = new ActivityContextManager();
	}

	public virtual void Dispose()
	{
		ActivityContextManager.Dispose();
	}

	protected abstract bool IsIconVisible(Tag tag);

	protected abstract bool IsTextVisible(Tag tag);

	protected bool IsTagVisible(Tag tag, GameObject? gameObject)
	{
		if (!ActivityContextHelper.GetIsVisible(ActivityContextManager.CurrentActivityContext.ActivityType, tag.IsVisibleInPveDuties.InheritedValue == true, tag.IsVisibleInPvpDuties.InheritedValue == true, tag.IsVisibleInOverworld.InheritedValue == true))
		{
			return false;
		}
		PlayerCharacter val = (PlayerCharacter)(object)((gameObject is PlayerCharacter) ? gameObject : null);
		if (val != null && !PlayerContextHelper.GetIsVisible(val, tag.IsVisibleForSelf.InheritedValue == true, tag.IsVisibleForFriendPlayers.InheritedValue == true, tag.IsVisibleForPartyPlayers.InheritedValue == true, tag.IsVisibleForAlliancePlayers.InheritedValue == true, tag.IsVisibleForEnemyPlayers.InheritedValue == true, tag.IsVisibleForOtherPlayers.InheritedValue == true))
		{
			return false;
		}
		return true;
	}

	protected Payload[] GetPayloads(Tag tag, GameObject? gameObject)
	{
		if (!IsTagVisible(tag, gameObject))
		{
			return Array.Empty<Payload>();
		}
		return CreatePayloads(tag);
	}

	private Payload[] CreatePayloads(Tag tag)
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0038: Unknown result type (might be due to invalid IL or missing references)
		//IL_003d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Expected O, but got Unknown
		//IL_00ca: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Expected O, but got Unknown
		//IL_0095: Unknown result type (might be due to invalid IL or missing references)
		//IL_009f: Expected O, but got Unknown
		//IL_010b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0115: Expected O, but got Unknown
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0109: Expected O, but got Unknown
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Expected O, but got Unknown
		//IL_014f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0159: Expected O, but got Unknown
		//IL_0185: Unknown result type (might be due to invalid IL or missing references)
		//IL_018f: Expected O, but got Unknown
		List<Payload> list = new List<Payload>();
		BitmapFontIcon? val = null;
		if (IsIconVisible(tag))
		{
			val = tag.Icon.InheritedValue;
		}
		if (val.HasValue && (int)val.Value != 0)
		{
			list.Add((Payload)new IconPayload(val.Value));
		}
		string text = null;
		if (IsTextVisible(tag))
		{
			text = tag.Text.InheritedValue;
		}
		if (!string.IsNullOrWhiteSpace(text))
		{
			if (tag.IsTextItalic.InheritedValue.HasValue && tag.IsTextItalic.InheritedValue.Value)
			{
				list.Add((Payload)new EmphasisItalicPayload(true));
			}
			if (tag.TextGlowColor.InheritedValue.HasValue)
			{
				list.Add((Payload)new UIGlowPayload(tag.TextGlowColor.InheritedValue.Value));
			}
			if (tag.TextColor.InheritedValue.HasValue)
			{
				list.Add((Payload)new UIForegroundPayload(tag.TextColor.InheritedValue.Value));
			}
			list.Add((Payload)new TextPayload(text));
			if (tag.TextColor.InheritedValue.HasValue)
			{
				list.Add((Payload)new UIForegroundPayload((ushort)0));
			}
			if (tag.TextGlowColor.InheritedValue.HasValue)
			{
				list.Add((Payload)new UIGlowPayload((ushort)0));
			}
			if (tag.IsTextItalic.InheritedValue.HasValue && tag.IsTextItalic.InheritedValue.Value)
			{
				list.Add((Payload)new EmphasisItalicPayload(false));
			}
		}
		return list.ToArray();
	}

	protected static string BuildPlayername(string name)
	{
		LogNameType? logNameType = GameConfigHelper.Instance.GetLogNameType();
		string text = string.Empty;
		if (logNameType.HasValue && !string.IsNullOrEmpty(name))
		{
			string[] array = name.Split(' ');
			if (array.Length > 1)
			{
				string text2 = array[0];
				string text3 = array[1];
				switch (logNameType)
				{
				case LogNameType.FullName:
					text = text2 + " " + text3;
					break;
				case LogNameType.LastNameShorted:
					text = text2 + " " + text3.Substring(0, 1) + ".";
					break;
				case LogNameType.FirstNameShorted:
					text = text2.Substring(0, 1) + ". " + text3;
					break;
				case LogNameType.Initials:
					text = text2.Substring(0, 1) + ". " + text3.Substring(0, 1) + ".";
					break;
				}
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			text = name;
		}
		return text;
	}

	protected void AddPayloadChanges(StringPosition tagPosition, IEnumerable<Payload> payloads, StringChanges stringChanges, bool forceUsingSingleAnchorPayload)
	{
		if (payloads != null && payloads.Any() && stringChanges != null)
		{
			StringChange change = stringChanges.GetChange(tagPosition);
			change.Payloads.AddRange(payloads);
			change.ForceUsingSingleAnchorPayload = forceUsingSingleAnchorPayload;
		}
	}

	protected void ApplyStringChanges(SeString seString, StringChanges stringChanges, List<Payload> anchorPayloads = null, Payload anchorReplacePayload = null)
	{
		StringUpdateFactory.ApplyStringChanges(new StringChangesProps
		{
			Destination = seString,
			AnchorPayload = anchorReplacePayload,
			AnchorPayloads = anchorPayloads,
			StringChanges = stringChanges
		});
	}

	protected void ApplyTextFormatting(GameObject gameObject, Tag tag, SeString[] destStrings, InheritableValue<bool>[] textColorApplied, List<Payload> preferedPayloads, ushort? overwriteTextColor = null)
	{
		if (IsTagVisible(tag, gameObject))
		{
			for (int i = 0; i < destStrings.Length; i++)
			{
				SeString destPayload = destStrings[i];
				InheritableValue<bool> enableFlag = textColorApplied[i];
				applyTextColor(destPayload, enableFlag, tag.TextColor);
			}
		}
		void applyTextColor(SeString destPayload2, InheritableValue<bool> inheritableValue, InheritableValue<ushort> colorValue)
		{
			//IL_0061: Unknown result type (might be due to invalid IL or missing references)
			//IL_0067: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Expected O, but got Unknown
			//IL_0072: Expected O, but got Unknown
			ushort? num = overwriteTextColor ?? colorValue?.InheritedValue;
			if (shouldApplyFormattingPayloads(destPayload2) && inheritableValue.InheritedValue.HasValue && inheritableValue.InheritedValue.Value && num.HasValue)
			{
				applyTextFormattingPayloads(destPayload2, (Payload)new UIForegroundPayload(num.Value), (Payload)new UIForegroundPayload((ushort)0));
			}
		}
		static void applyTextFormattingPayloadToStartAndEnd(SeString val, Payload startPayload, Payload endPayload)
		{
			val.Payloads.Insert(0, startPayload);
			val.Payloads.Add(endPayload);
		}
		void applyTextFormattingPayloads(SeString destPayload2, Payload startPayload, Payload endPayload)
		{
			if (preferedPayloads == null || !preferedPayloads.Any())
			{
				applyTextFormattingPayloadToStartAndEnd(destPayload2, startPayload, endPayload);
			}
			else
			{
				applyTextFormattingPayloadsToSpecificPosition(destPayload2, startPayload, endPayload, preferedPayloads);
			}
		}
		void applyTextFormattingPayloadsToSpecificPosition(SeString val, Payload startPayload, Payload endPayload, List<Payload> preferedPayload)
		{
			int index = val.Payloads.IndexOf(preferedPayloads.First());
			val.Payloads.Insert(index, startPayload);
			int num = val.Payloads.IndexOf(preferedPayloads.Last());
			val.Payloads.Insert(num + 1, endPayload);
		}
		static bool shouldApplyFormattingPayloads(SeString val)
		{
			return val.Payloads.Any((Payload payload) => payload is TextPayload || payload is PlayerPayload);
		}
	}
}
