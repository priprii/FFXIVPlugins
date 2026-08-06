using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Gui.Toast;
using Discord;

namespace TriggerPyon;

public class CounterManager
{
	private Plugin plugin;

	private int discordCurrentLine;

	private DateTime? discordCycleStartTime;

	private DateTime? discordNextUpdateTime;

	private List<string> resolvedTemplates = new List<string>();

	private Trigger? LastTrigger { get; set; }

	private DateTime? LastTriggerStartTime { get; set; }

	private DateTime? LastTriggerEndTime { get; set; }

	public Trigger? LastDiscordTrigger { get; set; }

	public string? LastDiscordTitle { get; set; }

	public CounterManager(Plugin plugin)
	{
		this.plugin = plugin;
	}

	public void Update()
	{
		try
		{
			if (!Plugin.Config.Enabled || PlayerManager.LocalPlayer == null)
			{
				Dispose();
			}
			else
			{
				if (LastTrigger == null && LastDiscordTrigger == null)
				{
					return;
				}
				if (LastTrigger != null)
				{
					DateTime now = DateTime.Now;
					DateTime? lastTriggerEndTime = LastTriggerEndTime;
					if (now > lastTriggerEndTime)
					{
						ClearTitle();
						return;
					}
				}
				if (LastTrigger != null || LastDiscordTrigger == null)
				{
					return;
				}
				if (!LastDiscordTrigger.Enabled)
				{
					ClearDiscordTitle();
				}
				else if (!(LastDiscordTrigger.Counter is DiscordCounter discordCounter))
				{
					ClearDiscordTitle();
				}
				else if (resolvedTemplates.Count == 0)
				{
					ClearDiscordTitle();
				}
				else
				{
					if (!discordNextUpdateTime.HasValue || !(DateTime.Now >= discordNextUpdateTime.Value))
					{
						return;
					}
					if (discordCurrentLine >= resolvedTemplates.Count)
					{
						discordCurrentLine = 0;
						if (discordCounter.Frequency > 0)
						{
							plugin.Honorific?.ClearTitle();
							LastDiscordTitle = null;
							discordCycleStartTime = DateTime.Now;
							discordNextUpdateTime = discordCycleStartTime.Value.AddMilliseconds(discordCounter.Frequency);
							return;
						}
					}
					LastDiscordTitle = resolvedTemplates[discordCurrentLine];
					discordNextUpdateTime = DateTime.Now.AddMilliseconds(discordCounter.Duration);
					if (!string.IsNullOrEmpty(LastDiscordTitle))
					{
						plugin.Honorific?.SetTitle(LastDiscordTitle, discordCounter.TitlePrefix, discordCounter.TitleColour, discordCounter.TitleGlow, discordCounter.TitleGradientColorSet, discordCounter.TitleGradientAnimationStyle);
					}
					discordCurrentLine++;
				}
			}
		}
		catch (Exception ex)
		{
			Plugin.Log.Warning(ex, "CounterManager Update Exception", Array.Empty<object>());
		}
	}

	public void UpdateDiscordCounter(IActivity activity, Trigger trigger)
	{
		if (!(trigger.Counter is DiscordCounter discordCounter))
		{
			return;
		}
		List<string> second = discordCounter.TitleTemplates.Select((string t) => ResolveDiscordTemplate(t, activity)).ToList();
		if (!resolvedTemplates.SequenceEqual(second))
		{
			LastDiscordTrigger = trigger;
			resolvedTemplates = second;
			discordCycleStartTime = DateTime.Now;
			discordCurrentLine = 0;
			discordNextUpdateTime = discordCycleStartTime.Value.AddMilliseconds(Math.Max(1000, discordCounter.Duration));
			LastDiscordTitle = ((resolvedTemplates.Count > 0) ? resolvedTemplates[0] : null);
			if (!string.IsNullOrEmpty(LastDiscordTitle))
			{
				plugin.Honorific?.SetTitle(LastDiscordTitle, discordCounter.TitlePrefix, discordCounter.TitleColour, discordCounter.TitleGlow, discordCounter.TitleGradientColorSet, discordCounter.TitleGradientAnimationStyle);
				discordCurrentLine++;
			}
		}
	}

	private string ResolveDiscordTemplate(string template, IActivity activity)
	{
		if (activity is SpotifyGame spotifyGame)
		{
			string text = spotifyGame.Artists.FirstOrDefault() ?? "";
			string text2 = spotifyGame.TrackTitle ?? "";
			string text3 = template.Replace("%artist%", text).Replace("%title%", text2);
			int length = text3.Length;
			if (length > 32)
			{
				int num = Math.Abs(32 - length);
				if (template.Contains("%artist%"))
				{
					if (num > text.Length - 4)
					{
						return "";
					}
					text = text.Substring(0, text.Length - (num + 2)).Trim() + "..";
					text3 = template.Replace("%artist%", text);
				}
				else if (template.Contains("%title%"))
				{
					if (num > text2.Length - 4)
					{
						return "";
					}
					text2 = text2.Substring(0, text2.Length - (num + 2)).Trim() + "..";
					text3 = template.Replace("%title%", text2);
				}
				else
				{
					text3 = text3.Substring(0, text3.Length - (num + 2)).Trim() + "..";
				}
			}
			return text3;
		}
		if (activity is Discord.Game game && !string.Equals(game.Name, "Custom Status", StringComparison.OrdinalIgnoreCase))
		{
			string text4 = game.Name ?? "";
			string text5 = template.Replace("%game%", text4);
			int length2 = text5.Length;
			if (length2 > 32)
			{
				int num2 = Math.Abs(32 - length2);
				if (template.Contains("%game%"))
				{
					if (num2 > text4.Length - 4)
					{
						return "";
					}
					text4 = text4.Substring(0, text4.Length - (num2 + 2)).Trim() + "..";
					text5 = template.Replace("%game%", text4);
				}
				else
				{
					text5 = text5.Substring(0, text5.Length - (num2 + 2)).Trim() + "..";
				}
			}
			return text5;
		}
		if (activity is CustomStatusGame customStatusGame && !string.IsNullOrWhiteSpace(customStatusGame.State))
		{
			string text6 = customStatusGame.State ?? "";
			string text7 = template.Replace("%status%", text6);
			int length3 = text7.Length;
			if (length3 > 32)
			{
				int num3 = Math.Abs(32 - length3);
				if (template.Contains("%status%"))
				{
					if (num3 > text6.Length - 4)
					{
						return "";
					}
					text6 = text6.Substring(0, text6.Length - (num3 + 2)).Trim() + "..";
					text7 = template.Replace("%status%", text6);
				}
				else
				{
					text7 = text7.Substring(0, text7.Length - (num3 + 2)).Trim() + "..";
				}
			}
			return text7;
		}
		return template;
	}

	public void UpdateCounter(Trigger trigger, EntityInfo instigator, EntityInfo? receiver)
	{
		Counter counter = trigger.GetCounter();
		if (counter != null)
		{
			counter.Amount++;
			Plugin.Config.Save();
			if (counter.CanDisplayTitle())
			{
				SetTitle(trigger, counter, instigator.Forename, instigator.Surname, receiver?.Forename, receiver?.Surname);
			}
			if (counter.CanDisplayToast())
			{
				SetToast(counter, instigator.Forename, instigator.Surname, receiver?.Forename, receiver?.Surname);
			}
			if (counter.CanDisplayEcho())
			{
				SetEcho(counter, instigator.Forename, instigator.Surname, receiver?.Forename, receiver?.Surname);
			}
		}
	}

	public void UpdateCounter(Trigger trigger, string instigatorName)
	{
		Counter counter = trigger.GetCounter();
		if (counter != null)
		{
			counter.Amount++;
			Plugin.Config.Save();
			string forename = instigatorName.GetForename();
			string item = instigatorName.GetSurnameWorld().Item1;
			if (counter.CanDisplayTitle())
			{
				SetTitle(trigger, counter, forename, item);
			}
			if (counter.CanDisplayToast())
			{
				SetToast(counter, forename, item);
			}
			if (counter.CanDisplayEcho())
			{
				SetEcho(counter, forename, item);
			}
		}
	}

	public void SetTitle(Trigger trigger, Counter? counter, string instForename, string instSurname, string? recForename = "", string? recSurname = "")
	{
		try
		{
			if (Plugin.Config.Enabled && PlayerManager.LocalPlayer != null && counter != null && !plugin.HasInvalidConditionForTitle() && (!LastTriggerStartTime.HasValue || !(DateTime.Now < LastTriggerStartTime.Value.AddMilliseconds(Plugin.Config.CounterCooldown))) && (LastDiscordTrigger == null || LastDiscordTitle == null || !(LastDiscordTrigger.Counter is DiscordCounter { Interruptable: false })))
			{
				string template = GetTemplate(counter.TitleTemplate, counter.Amount, instForename, instSurname, recForename, recSurname);
				if (!string.IsNullOrWhiteSpace(template))
				{
					LastTrigger = trigger;
					LastTriggerStartTime = DateTime.Now;
					LastTriggerEndTime = DateTime.Now.AddMilliseconds(counter.GetDuration());
					plugin.Honorific?.SetTitle(template, counter.TitlePrefix, counter.TitleColour, counter.TitleGlow, counter.TitleGradientColorSet, counter.TitleGradientAnimationStyle);
				}
			}
		}
		catch (Exception ex)
		{
			Plugin.Log.Warning(ex, "Honorific IPC Exception", Array.Empty<object>());
		}
	}

	public void ClearTitle()
	{
		if (LastTrigger == null)
		{
			return;
		}
		if (LastDiscordTrigger != null && LastDiscordTrigger.Counter is DiscordCounter discordCounter)
		{
			double totalMilliseconds = (LastTriggerEndTime.Value - LastTriggerStartTime.Value).TotalMilliseconds;
			discordCurrentLine = 0;
			discordCycleStartTime = DateTime.Now;
			if (discordCounter.Frequency == 0 || !discordNextUpdateTime.HasValue)
			{
				discordNextUpdateTime = discordCycleStartTime;
				return;
			}
			discordNextUpdateTime = discordNextUpdateTime.Value.AddMilliseconds(totalMilliseconds);
		}
		LastTrigger = null;
		plugin.Honorific?.ClearTitle();
	}

	public void ClearDiscordTitle()
	{
		if (LastDiscordTrigger == null)
		{
			plugin.Honorific?.ForceClear();
			return;
		}
		LastDiscordTrigger = null;
		LastDiscordTitle = null;
		resolvedTemplates.Clear();
		discordCycleStartTime = null;
		discordNextUpdateTime = null;
		discordCurrentLine = 0;
		if (LastTrigger == null)
		{
			plugin.Honorific?.ClearTitle();
		}
	}

	public void Dispose()
	{
		if (LastTrigger != null || LastDiscordTrigger != null)
		{
			LastTrigger = null;
			LastDiscordTrigger = null;
			LastDiscordTitle = null;
			resolvedTemplates.Clear();
			discordCycleStartTime = null;
			discordNextUpdateTime = null;
			discordCurrentLine = 0;
			plugin.Honorific?.ClearTitle();
		}
	}

	public void SetToast(Counter? counter, string instForename, string instSurname, string? recForename = "", string? recSurname = "")
	{
		if (!Plugin.Config.Enabled || PlayerManager.LocalPlayer == null || counter == null)
		{
			return;
		}
		string template = GetTemplate(counter.ToastTemplate, counter.Amount, instForename, instSurname, recForename, recSurname);
		if (string.IsNullOrWhiteSpace(template))
		{
			return;
		}
		Plugin.Framework.RunOnFrameworkThread((Action)delegate
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_007c: Expected O, but got Unknown
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0051: Expected O, but got Unknown
			if (counter.ToastDisplayType == ToastDisplayType.Normal)
			{
				Plugin.ToastGui.ShowNormal(template, new ToastOptions
				{
					Speed = (ToastSpeed)(counter.ToastDisplaySpeed == ToastDisplaySpeed.Fast),
					Position = (ToastPosition)(counter.ToastDisplayPosition != ToastDisplayPosition.Bottom)
				});
			}
			else if (counter.ToastDisplayType == ToastDisplayType.Quest)
			{
				Plugin.ToastGui.ShowQuest(template, new QuestToastOptions
				{
					Position = (QuestToastPosition)0
				});
			}
			else
			{
				Plugin.ToastGui.ShowError(template);
			}
		});
	}

	public void SetEcho(Counter? counter, string instForename, string instSurname, string? recForename = "", string? recSurname = "")
	{
		if (Plugin.Config.Enabled && PlayerManager.LocalPlayer != null && counter != null)
		{
			string template = GetTemplate(counter.EchoTemplate, counter.Amount, instForename, instSurname, recForename, recSurname);
			if (!string.IsNullOrWhiteSpace(template))
			{
				plugin.Chat.SendEcho(template);
			}
		}
	}

	private string GetTemplate(string template, int amount, string instForename, string instSurname, string? recForename = "", string? recSurname = "")
	{
		if (string.IsNullOrWhiteSpace(template))
		{
			return string.Empty;
		}
		return template.Replace("%n%", $"{amount}").Replace("%ifn%", instForename).Replace("%isn%", instSurname)
			.Replace("%rfn%", recForename)
			.Replace("%rsn%", recSurname);
	}
}
