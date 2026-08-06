using System;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game;
using Ktisis.Core.Attributes;
using Ktisis.Services.Game;

namespace Ktisis.Services.Data;

[Transient]
public class HousingDataService : IDisposable
{
	[Singleton]
	public class SSAOHook
	{
		private unsafe delegate nint ToggleSSAO(HousingManager* Instance, bool option);

		[Signature("48 89 5C 24 ?? 57 48 83 EC ?? 48 8B 79 ?? 0F B6 DA")]
		private readonly ToggleSSAO toggle;

		public SSAOHook(IGameInteropProvider interopProvider)
		{
			interopProvider.InitializeFromAttributes((object)this);
		}

		public unsafe void Set(bool state)
		{
			toggle(HousingManager.Instance(), state);
		}
	}

	private unsafe HousingManager* _housingManager;

	private readonly GPoseService _gposeService;

	private readonly SSAOHook _ssaoHook;

	public unsafe bool IsInHousing
	{
		get
		{
			if (_housingManager != null)
			{
				return ((HousingManager)_housingManager).IsInside();
			}
			return false;
		}
	}

	public unsafe IndoorTerritory* IndoorTerritory => ((HousingManager)_housingManager).IndoorTerritory;

	public unsafe float IndoorLight
	{
		get
		{
			if (!IsInHousing || IndoorTerritory == null)
			{
				return float.NaN;
			}
			return ((IndoorTerritory)IndoorTerritory).BrightnessTarget;
		}
		set
		{
			if (IsInHousing && IndoorTerritory != null)
			{
				float brightnessTransitionSpeed = value - ((IndoorTerritory)IndoorTerritory).BrightnessCurrent;
				((IndoorTerritory)IndoorTerritory).BrightnessTarget = value;
				((IndoorTerritory)IndoorTerritory).BrightnessTransitionSpeed = brightnessTransitionSpeed;
				((IndoorTerritory)IndoorTerritory).IsBrightnessTransitioning = true;
			}
		}
	}

	public unsafe bool SSAOEnabled
	{
		get
		{
			if (_housingManager != null && IndoorTerritory != null)
			{
				return ((IndoorTerritory)IndoorTerritory).SSAOEnable;
			}
			return false;
		}
		set
		{
			if (IsInHousing && IndoorTerritory != null)
			{
				_ssaoHook.Set(value);
			}
		}
	}

	public unsafe HousingDataService(GPoseService gPoseService, SSAOHook ssaoHook)
	{
		_housingManager = HousingManager.Instance();
		_gposeService = gPoseService;
		_ssaoHook = ssaoHook;
		gPoseService.StateChanged += GPoseServiceOnStateChanged;
	}

	public void Dispose()
	{
		_gposeService.StateChanged -= GPoseServiceOnStateChanged;
	}

	internal unsafe void ResetLighting()
	{
		if (IsInHousing && IndoorTerritory != null)
		{
			float num = 1f - (float)(int)((IndoorTerritory)IndoorTerritory).SavedInvertedBrightness * 0.2f;
			float brightnessTransitionSpeed = (float)Math.Sign(((IndoorTerritory)IndoorTerritory).BrightnessCurrent - num) * 0.02f;
			((IndoorTerritory)IndoorTerritory).BrightnessTarget = num;
			((IndoorTerritory)IndoorTerritory).BrightnessTransitionSpeed = brightnessTransitionSpeed;
			((IndoorTerritory)IndoorTerritory).IsBrightnessTransitioning = true;
		}
	}

	public unsafe void ResetSSAO()
	{
		if (IsInHousing && IndoorTerritory != null)
		{
			SSAOEnabled = ((IndoorTerritory)IndoorTerritory).SavedSSAOEnable;
		}
	}

	private void GPoseServiceOnStateChanged(GPoseService sender, bool state)
	{
		if (state)
		{
			OnEnabled();
		}
		else
		{
			OnDisabled();
		}
	}

	private void OnDisabled()
	{
		ResetLighting();
		ResetSSAO();
	}

	private unsafe void OnEnabled()
	{
		_housingManager = HousingManager.Instance();
	}
}
