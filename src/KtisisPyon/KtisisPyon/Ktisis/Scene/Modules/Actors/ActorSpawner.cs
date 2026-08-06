using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using Dalamud.Utility.Signatures;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.System.Memory;
using FFXIVClientStructs.FFXIV.Common.Math;
using Ktisis.Interop.Hooking;
using Ktisis.Structs.Events;

namespace Ktisis.Scene.Modules.Actors;

public class ActorSpawner : HookModule
{
	private unsafe delegate nint GPoseActorEventCtorDelegate(GPoseActorEvent* self, Character* target, Vector3* position, uint a4, int a5, int a6, uint a7, bool a8);

	private unsafe delegate nint DispatchEventDelegate(nint handler, GPoseActorEvent* task);

	private unsafe delegate void FinalizeDelegate(GPoseActorEvent* a1, nint a2, nint a3);

	private readonly IObjectTable _objectTable;

	private readonly IFramework _framework;

	private const int VfSize = 9;

	[Signature(/*Could not decode attribute arguments.*/)]
	private unsafe nint* _eventVfTable = null;

	[Signature("80 61 0C FC 48 8D 05 ?? ?? ?? ?? 4C 8B C9")]
	private GPoseActorEventCtorDelegate _gPoseActorEventCtor;

	[Signature("48 89 5C 24 ?? 48 89 54 24 ?? 57 48 83 EC 20 48 8B 02")]
	private DispatchEventDelegate _dispatchEvent;

	private unsafe nint* _hookVfTable = null;

	private static FinalizeDelegate _finalizeOriginal;

	public unsafe ActorSpawner(IHookMediator hook, IObjectTable objectTable, IFramework framework)
		: base(hook)
	{
		_objectTable = objectTable;
		_framework = framework;
	}

	public void TryInitialize()
	{
		try
		{
			Initialize();
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Failed to initialize actor spawner:\n{value}");
		}
	}

	protected override bool OnInitialize()
	{
		Setup();
		return true;
	}

	private unsafe void Setup()
	{
		nint* ptr = (nint*)Marshal.AllocHGlobal(sizeof(nint) * 9);
		for (int i = 0; i < 9; i++)
		{
			nint num = _eventVfTable[i];
			if (i == 2)
			{
				_finalizeOriginal = Marshal.GetDelegateForFunctionPointer<FinalizeDelegate>(num);
				ptr[i] = Marshal.GetFunctionPointerForDelegate<FinalizeDelegate>(FinalizeHook);
			}
			else
			{
				ptr[i] = num;
			}
		}
		_hookVfTable = ptr;
	}

	public async Task<nint> CreateActor(IGameObject original)
	{
		using CancellationTokenSource source = new CancellationTokenSource();
		source.CancelAfter(10000);
		return await CreateActor(original, source.Token);
	}

	private async Task<nint> CreateActor(IGameObject original, CancellationToken token)
	{
		uint index = await _framework.RunOnFrameworkThread<uint>((Func<uint>)delegate
		{
			if (!TryDispatch(original, out var index2))
			{
				Ktisis.Log.Error("Object table is full.");
				return uint.MaxValue;
			}
			return index2;
		});
		if (index == uint.MaxValue)
		{
			return IntPtr.Zero;
		}
		while (!token.IsCancellationRequested)
		{
			nint num = await _framework.RunOnFrameworkThread<nint>((Func<nint>)delegate
			{
				IGameObject val = _objectTable[(int)index];
				return (val == null || !val.IsValid()) ? IntPtr.Zero : ((nint)val.Address);
			});
			if (num != IntPtr.Zero)
			{
				return num;
			}
			await Task.Delay(10, CancellationToken.None);
		}
		throw new TaskCanceledException($"Actor spawn at index {index} timed out.");
	}

	private unsafe bool TryDispatch(IGameObject original, out uint index)
	{
		if ((index = ((ClientObjectManager)ClientObjectManager.Instance()).CalculateNextAvailableIndex()) == uint.MaxValue)
		{
			return false;
		}
		index += 200u;
		Ktisis.Log.Info($"Dispatching, expecting spawn on {index}");
		DispatchSpawn(original);
		return true;
	}

	private unsafe void DispatchSpawn(IGameObject original)
	{
		if (_hookVfTable == null)
		{
			throw new Exception("Hook vtable is not initialized!");
		}
		Character* address = (Character*)original.Address;
		if (address == null || !((GameObject)(&((Character)address).GameObject)).IsCharacter())
		{
			throw new Exception($"Original object '{original.Name}' ({original.ObjectIndex}) is invalid.");
		}
		GPoseActorEvent* ptr = ((IMemorySpace)IMemorySpace.GetDefaultSpace()).Malloc<GPoseActorEvent>(8uL);
		_gPoseActorEventCtor(ptr, address, &((GameObject)(&((Character)address).GameObject)).Position, 64u, 30, 0, 4294934523u, a8: true);
		ptr->__vfTable = _hookVfTable;
		nint handler = (nint)((byte*)EventFramework.Instance() + 432 + 152);
		_dispatchEvent(handler, ptr);
	}

	private unsafe static void FinalizeHook(GPoseActorEvent* self, nint a2, nint a3)
	{
		if (self->Character != null)
		{
			self->EntityID = 3758096384uL;
		}
		_finalizeOriginal(self, a2, a3);
	}

	public unsafe override void Dispose()
	{
		base.Dispose();
		Ktisis.Log.Verbose("Disposing actor spawn manager...");
		if (_hookVfTable != null)
		{
			Ktisis.Log.Verbose("Freeing hookVfTable from spawn manager");
			Marshal.FreeHGlobal((nint)_hookVfTable);
			_hookVfTable = null;
		}
		GC.SuppressFinalize(this);
	}
}
