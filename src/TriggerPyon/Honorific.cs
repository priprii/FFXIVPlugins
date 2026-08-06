using System;
using System.Collections.Generic;
using System.Numerics;
using Dalamud.Plugin.Ipc;
using Newtonsoft.Json;

namespace TriggerPyon;

public class Honorific
{
	private ICallGateSubscriber<int, string, object>? SetCharacterTitle { get; init; }

	private ICallGateSubscriber<int, object>? ClearCharacterTitle { get; init; }

	public bool IsClearing { get; set; }

	public bool IsSet { get; set; }

	public Honorific()
	{
		try
		{
			SetCharacterTitle = Plugin.PluginInterface.GetIpcSubscriber<int, string, object>("Honorific.SetCharacterTitle");
			ClearCharacterTitle = Plugin.PluginInterface.GetIpcSubscriber<int, object>("Honorific.ClearCharacterTitle");
		}
		catch (Exception ex)
		{
			Plugin.Log.Warning(ex, "Honorific IPC Exception", Array.Empty<object>());
		}
	}

	public void SetTitle(string template, bool isPrefix, Vector3 color, Vector3? glow, int? gradientColorSet, GradientAnimationStyle? gradientAnimationStyle)
	{
		try
		{
			if (Plugin.Config.Enabled && PlayerManager.LocalPlayer != null && SetCharacterTitle != null && (((ICallGateSubscriber)SetCharacterTitle).HasFunction || ((ICallGateSubscriber)SetCharacterTitle).HasAction))
			{
				IsClearing = false;
				Dictionary<string, object> value = new Dictionary<string, object>
				{
					{ "Title", template },
					{ "IsPrefix", isPrefix },
					{ "Color", color },
					{ "Glow", glow },
					{
						"RainbowMode",
						GetRainbowMode(gradientColorSet, gradientAnimationStyle)
					},
					{ "GradientColourSet", gradientColorSet },
					{ "GradientAnimationStyle", gradientAnimationStyle }
				};
				string title = JsonConvert.SerializeObject(value);
				Plugin.Framework.RunOnFrameworkThread((Action)delegate
				{
					SetCharacterTitle.InvokeAction(0, title);
				});
				IsSet = true;
			}
		}
		catch (Exception ex)
		{
			Plugin.Log.Warning(ex, "Honorific IPC Exception", Array.Empty<object>());
		}
	}

	private int GetRainbowMode(int? titleGradientColourSet, GradientAnimationStyle? titleGradientAnimationStyle)
	{
		if (!titleGradientColourSet.HasValue)
		{
			return 0;
		}
		if ((!titleGradientAnimationStyle.HasValue || titleGradientAnimationStyle == GradientAnimationStyle.Static) ? true : false)
		{
			return 0;
		}
		if (titleGradientColourSet >= 5)
		{
			return 0;
		}
		return titleGradientColourSet.Value * 2 + ((titleGradientAnimationStyle == GradientAnimationStyle.Wave) ? 1 : 2);
	}

	public void ClearTitle()
	{
		try
		{
			if (!IsClearing && ClearCharacterTitle != null && (((ICallGateSubscriber)ClearCharacterTitle).HasFunction || ((ICallGateSubscriber)ClearCharacterTitle).HasAction))
			{
				IsClearing = true;
			}
		}
		catch (Exception ex)
		{
			Plugin.Log.Warning(ex, "Honorific IPC Exception", Array.Empty<object>());
		}
	}

	public void Update()
	{
		try
		{
			if (IsClearing && PlayerManager.LocalPlayer != null && ClearCharacterTitle != null && (((ICallGateSubscriber)ClearCharacterTitle).HasFunction || ((ICallGateSubscriber)ClearCharacterTitle).HasAction))
			{
				Plugin.Framework.RunOnFrameworkThread((Action)delegate
				{
					ClearCharacterTitle.InvokeAction(0);
				});
				IsClearing = false;
				IsSet = false;
			}
		}
		catch (Exception ex)
		{
			Plugin.Log.Warning(ex, "Honorific IPC Exception", Array.Empty<object>());
		}
	}

	public void ForceClear()
	{
		try
		{
			if (IsSet && PlayerManager.LocalPlayer != null && ClearCharacterTitle != null && (((ICallGateSubscriber)ClearCharacterTitle).HasFunction || ((ICallGateSubscriber)ClearCharacterTitle).HasAction))
			{
				Plugin.Framework.RunOnFrameworkThread((Action)delegate
				{
					ClearCharacterTitle.InvokeAction(0);
				});
				IsClearing = false;
				IsSet = false;
			}
		}
		catch (Exception ex)
		{
			Plugin.Log.Warning(ex, "Honorific IPC Exception", Array.Empty<object>());
		}
	}

	public void Dispose()
	{
		ForceClear();
	}
}
