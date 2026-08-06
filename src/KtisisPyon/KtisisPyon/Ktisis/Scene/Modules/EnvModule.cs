using System;
using System.Linq;
using Dalamud.Hooking;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Graphics.Environment;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.System.Timer;
using Ktisis.Interface.Components.Environment.Editors;
using Ktisis.Interop.Hooking;
using Ktisis.Scene.Types;
using Ktisis.Structs.Env;

namespace Ktisis.Scene.Modules;

public class EnvModule : SceneModule, IEnvModule, IHookModule, IDisposable
{
	private unsafe delegate nint EnvStateCopyDelegate(EnvState* dest, EnvState* src);

	private unsafe delegate nint EnvManagerUpdateDelegate(EnvManagerEx* env, float a2, float a3);

	private delegate void UpdateTimeDelegate(nint a1);

	private delegate nint UpdateWaterDelegate(nint a1);

	private readonly WaterEditor _water;

	[Signature("E8 ?? ?? ?? ?? 49 3B F5 75 0D")]
	private EnvStateCopyDelegate EnvStateCopy;

	[Signature("E8 ?? ?? ?? ?? 49 3B F5 75 0D", DetourName = "EnvStateCopyDetour")]
	private Hook<EnvStateCopyDelegate> EnvStateCopyHook;

	[Signature("40 53 48 83 EC 30 48 8B 05 ?? ?? ?? ?? 48 8B D9 0F 29 74 24 ??", DetourName = "EnvUpdateDetour")]
	private Hook<EnvManagerUpdateDelegate> EnvUpdateHook;

	[Signature("48 89 5C 24 ?? 57 48 83 EC 30 4C 8B 15 ?? ?? ?? ??", DetourName = "UpdateTimeDetour")]
	private Hook<UpdateTimeDelegate> UpdateTimeHook;

	[Signature("48 8B C4 48 89 58 18 57 48 81 EC ?? ?? ?? ?? 0F B6 B9 ?? ?? ?? ??", DetourName = "UpdateWaterDetour")]
	private Hook<UpdateWaterDelegate> UpdateWaterHook;

	public EnvOverride Override { get; set; }

	public float Time { get; set; }

	public int Day { get; set; }

	public byte Weather { get; set; }

	public EnvModule(IHookMediator hook, ISceneManager scene, WaterEditor water)
		: base(hook, scene)
	{
		_water = water;
	}

	protected override bool OnInitialize()
	{
		EnableAll();
		return true;
	}

	private unsafe void ApplyState(EnvState* dest, EnvState state)
	{
		foreach (EnvOverride item in from flag in Enum.GetValues<EnvOverride>()
			where flag > EnvOverride.TimeWeather && Override.HasFlag(flag)
			select flag)
		{
			switch (item)
			{
			case EnvOverride.SkyId:
				dest->SkyId = state.SkyId;
				break;
			case EnvOverride.Lighting:
				dest->Lighting = state.Lighting;
				break;
			case EnvOverride.Stars:
				dest->Stars = state.Stars;
				break;
			case EnvOverride.Fog:
				dest->Fog = state.Fog;
				break;
			case EnvOverride.Clouds:
				dest->Clouds = state.Clouds;
				break;
			case EnvOverride.Rain:
				dest->Rain = state.Rain;
				break;
			case EnvOverride.Dust:
				dest->Dust = state.Dust;
				break;
			case EnvOverride.Wind:
				dest->Wind = state.Wind;
				break;
			}
		}
	}

	private unsafe nint EnvStateCopyDetour(EnvState* dest, EnvState* src)
	{
		EnvState? envState = null;
		if (Scene.IsValid && Override != EnvOverride.None)
		{
			envState = *dest;
		}
		nint result = EnvStateCopyHook.Original(dest, src);
		if (envState.HasValue)
		{
			ApplyState(dest, envState.Value);
		}
		return result;
	}

	private unsafe nint EnvUpdateDetour(EnvManagerEx* env, float a2, float a3)
	{
		if (Scene.IsValid && Override.HasFlag(EnvOverride.TimeWeather))
		{
			((EnvManager)(&env->_base)).DayTimeSeconds = Time;
			((EnvManager)(&env->_base)).ActiveWeather = Weather;
		}
		return EnvUpdateHook.Original(env, a2, a3);
	}

	private unsafe void UpdateTimeDetour(nint a1)
	{
		if (Scene.IsValid && Override.HasFlag(EnvOverride.TimeWeather))
		{
			long num = (long)((float)(Day * 86400) + Time);
			ClientTime* num2 = &((Framework)Framework.Instance()).ClientTime;
			((ClientTime)num2).EorzeaTime = num;
			((ClientTime)num2).EorzeaTimeOverride = num;
		}
		UpdateTimeHook.Original(a1);
	}

	private nint UpdateWaterDetour(nint a1)
	{
		if (_water.Frozen)
		{
			return 0;
		}
		return UpdateWaterHook.Original(a1);
	}
}
