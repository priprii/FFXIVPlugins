using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using FFXIVClientStructs.FFXIV.Client.UI.Shell;
using FFXIVClientStructs.FFXIV.Common.Math;
using Hypostasis.Dalamud;
using Hypostasis.Game.Structures;

namespace Hypostasis.Game;

public static class Common
{
	public unsafe delegate GameObject* GetGameObjectFromPronounIDDelegate(PronounModule* pronounModule, PronounID id);

	public unsafe delegate Bool GetWorldBonePositionDelegate(GameObject* o, uint bone, Vector3* outPosition);

	[HypostasisSignatureInjection("48 8D 0D ?? ?? ?? ?? 0F B6 D8 E8 ?? ?? ?? ?? 44 0F B6 C0", Static = true, Required = true)]
	private unsafe static ContentsReplayModule* contentsReplayModule;

	[HypostasisClientStructsInjection<CameraManager>(Required = true)]
	private unsafe static CameraManager* cameraManager;

	[HypostasisClientStructsInjection<ActionManager>(Required = true)]
	private unsafe static ActionManager* actionManager;

	[HypostasisClientStructsInjection<Framework>(Required = true)]
	private unsafe static Framework* framework;

	private unsafe static UIModule* uiModule;

	private unsafe static InputData* inputData;

	private unsafe static RaptureShellModule* raptureShellModule;

	private unsafe static PronounModule* pronounModule;

	public static readonly GameFunction<GetGameObjectFromPronounIDDelegate> getGameObjectFromPronounID = new GameFunction<GetGameObjectFromPronounIDDelegate>("E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 0F 85 ?? ?? ?? ?? 8D 4F DD");

	public static readonly GameFunction<GetWorldBonePositionDelegate> getWorldBonePosition = new GameFunction<GetWorldBonePositionDelegate>("E8 ?? ?? ?? ?? 48 8B C3 48 83 C4 20 5B C3 CC 0F 57 C0 C3");

	public unsafe static ContentsReplayModule* ContentsReplayModule
	{
		get
		{
			if (contentsReplayModule == null)
			{
				InjectMember("contentsReplayModule");
			}
			return contentsReplayModule;
		}
	}

	public unsafe static CameraManager* CameraManager
	{
		get
		{
			if (cameraManager == null)
			{
				InjectMember("cameraManager");
			}
			return cameraManager;
		}
	}

	public unsafe static ActionManager* ActionManager
	{
		get
		{
			if (actionManager == null)
			{
				InjectMember("actionManager");
			}
			return actionManager;
		}
	}

	public unsafe static Framework* Framework
	{
		get
		{
			if (framework == null)
			{
				InjectMember("framework");
			}
			return framework;
		}
	}

	public unsafe static UIModule* UIModule
	{
		get
		{
			if (uiModule != null)
			{
				return uiModule;
			}
			uiModule = ((Framework)Framework).UIModule;
			return uiModule;
		}
	}

	public unsafe static InputData* InputData
	{
		get
		{
			if (inputData != null)
			{
				return inputData;
			}
			inputData = (InputData*)((UIModule)UIModule).GetUIInputData();
			return inputData;
		}
	}

	public unsafe static RaptureShellModule* RaptureShellModule
	{
		get
		{
			if (raptureShellModule != null)
			{
				return raptureShellModule;
			}
			raptureShellModule = ((UIModule)UIModule).GetRaptureShellModule();
			return raptureShellModule;
		}
	}

	public unsafe static PronounModule* PronounModule
	{
		get
		{
			if (pronounModule != null)
			{
				return pronounModule;
			}
			pronounModule = ((UIModule)UIModule).GetPronounModule();
			return pronounModule;
		}
	}

	public unsafe static bool IsMacroRunning => ((RaptureShellModule)RaptureShellModule).MacroCurrentLine >= 0;

	public unsafe static GameObject* UITarget => ((PronounModule)PronounModule).UiMouseOverTarget;

	public unsafe static GameObject* GetGameObjectFromPronounID(PronounID id)
	{
		return getGameObjectFromPronounID.Invoke(PronounModule, id);
	}

	public static IEnumerable<nint> GetPartyMembers()
	{
		for (uint i = 0u; i < 8; i++)
		{
			nint num = f(i);
			if (num != IntPtr.Zero)
			{
				yield return num;
			}
		}
		unsafe static nint f(uint num2)
		{
			return (nint)GetGameObjectFromPronounID((PronounID)(43 + num2));
		}
	}

	public static IEnumerable<nint> GetEnemies()
	{
		for (uint i = 0u; i < 26; i++)
		{
			nint num = f(i);
			if (num != IntPtr.Zero)
			{
				yield return num;
			}
		}
		unsafe static nint f(uint num2)
		{
			return (nint)GetGameObjectFromPronounID((PronounID)(9 + num2));
		}
	}

	public unsafe static Vector3 GetBoneWorldPosition(GameObject* o, uint bone)
	{
		Vector3 zero = Vector3.Zero;
		getWorldBonePosition.Invoke(o, bone, &zero);
		return zero;
	}

	public unsafe static Vector3 GetBoneLocalPosition(GameObject* o, uint bone)
	{
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return GetBoneWorldPosition(o, bone) - Vector3.op_Implicit((((GameObject)o).DrawObject != null) ? ((Object)(&((DrawObject)((GameObject)o).DrawObject).Object)).Position : ((GameObject)o).Position);
	}

	private static void InjectMember(string member)
	{
		DalamudApi.SigScanner.InjectMember(typeof(Common), null, member);
	}

	public unsafe static bool IsValid<T>(T* o) where T : unmanaged, IHypostasisStructure
	{
		if (o == null)
		{
			return false;
		}
		try
		{
			T deref = *o;
			if (!CheckGameFunctions(deref, BindingFlags.Static | BindingFlags.Public))
			{
				return false;
			}
			VirtualTable virtualTable = (from propertyInfo in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
				select propertyInfo.GetValue(deref) as VirtualTable).FirstOrDefault((VirtualTable p) => p != null);
			if (virtualTable != null && !CheckGameFunctions(virtualTable, BindingFlags.Instance | BindingFlags.Public))
			{
				return false;
			}
			if (!deref.Validate())
			{
				return false;
			}
		}
		catch
		{
			return false;
		}
		return true;
		static bool CheckGameFunctions(object obj2, BindingFlags bindingFlags)
		{
			return (from fieldInfo in obj2.GetType().GetFields(bindingFlags)
				select fieldInfo.GetValue(obj2) as IGameFunction).All((IGameFunction f) => f == null || f.IsValid);
		}
	}

	public static void Initialize()
	{
	}

	public static void Dispose()
	{
	}
}
