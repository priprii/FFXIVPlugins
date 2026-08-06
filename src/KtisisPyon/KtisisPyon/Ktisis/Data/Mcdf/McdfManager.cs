using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Client.System.Resource.Handle;
using Ktisis.Common.Extensions;
using Ktisis.Core.Attributes;
using Ktisis.Interop.Ipc;
using Ktisis.Scene.Entities.Game;
using Ktisis.Services.Game;

namespace Ktisis.Data.Mcdf;

[Singleton]
public sealed class McdfManager : IDisposable
{
	private readonly GPoseService _gpose;

	private readonly IFramework _framework;

	private readonly IpcManager _ipc;

	private readonly IObjectTable _objectTable;

	private Dictionary<IGameObject, Guid?> actors;

	private Dictionary<IGameObject, string> mcdfLocation;

	public McdfManager(GPoseService gpose, IFramework framework, IpcManager ipc, IObjectTable objectTable)
	{
		_gpose = gpose;
		_gpose.StateChanged += OnGPoseEvent;
		_gpose.Subscribe();
		_framework = framework;
		_ipc = ipc;
		_objectTable = objectTable;
		actors = new Dictionary<IGameObject, Guid?>();
		mcdfLocation = new Dictionary<IGameObject, string>();
	}

	private void OnGPoseEvent(object sender, bool active)
	{
		if (!active)
		{
			RevertAll();
		}
	}

	public void LoadAndApplyTo(string path, IGameObject actor)
	{
		LoadAndApplyToAsync(path, actor).ContinueWith(delegate(Task task)
		{
			if (task.Exception != null)
			{
				Ktisis.Log.Error($"Failed to load MCDF:\n{task.Exception.InnerException}");
			}
		}, TaskContinuationOptions.OnlyOnFaulted);
	}

	private async Task LoadAndApplyToAsync(string path, IGameObject actor)
	{
		mcdfLocation[actor] = path;
		using McdfReader reader = McdfReader.FromPath(path);
		string tempPath = GetTempPath(create: true);
		Ktisis.Log.Debug("Reading and extracting MCDF file");
		McdfData data = reader.GetData();
		Dictionary<string, string> extracted = reader.Extract(tempPath);
		Dictionary<string, string> dictionary = extracted.ToDictionary();
		foreach (var item in data.FileSwaps.SelectMany((McdfData.FileSwap x) => x.GamePaths, (McdfData.FileSwap k, string p) => (GamePath: p, FilePath: k.FileSwapPath)))
		{
			dictionary[item.GamePath] = item.FilePath;
		}
		Ktisis.Log.Debug("Applying MCDF data");
		if (actors.Keys.Contains(actor))
		{
			Ktisis.Log.Debug($"Actor {actor.ObjectIndex} was applied this session, reverting and redrawing...");
			Revert(actor);
		}
		else
		{
			actors.Add(actor, null);
		}
		Guid? collectionId = ApplyPenumbraMods(actor, data, dictionary);
		ApplyGlamourerData(actor, data);
		await RedrawAndWait(actor);
		if (collectionId.HasValue)
		{
			_ipc.GetPenumbraIpc().DeleteTemporaryCollection(collectionId.Value);
		}
		actors[actor] = ApplyCustomizeData(actor, data);
		Ktisis.Log.Debug("Cleaning up extracted files");
		foreach (string value in extracted.Values)
		{
			File.Delete(value);
		}
	}

	private Guid? ApplyCustomizeData(IGameObject actor, McdfData data)
	{
		string customizePlusData = data.CustomizePlusData;
		if (!_ipc.IsCustomizeActive)
		{
			if (!StringExtensions.IsNullOrEmpty(customizePlusData))
			{
				Ktisis.WarningNotification("MCDF has Customize+ data, but no IPC was found!\nCheck to make sure all plugins are enabled.");
			}
			return null;
		}
		(int, Guid?) tuple = _ipc.GetCustomizeIpc().SetTemporaryProfile(profileJson: (!StringExtensions.IsNullOrEmpty(customizePlusData)) ? Encoding.UTF8.GetString(Convert.FromBase64String(customizePlusData)) : "{}", gameObjectIndex: actor.ObjectIndex);
		if (!tuple.Item2.HasValue)
		{
			Ktisis.Log.Warning($"Customize+ SetTemporaryProfile returned null Guid! status: {tuple.Item1}");
		}
		return tuple.Item2;
	}

	private void ApplyGlamourerData(IGameObject actor, McdfData data)
	{
		string glamourerData = data.GlamourerData;
		if (!_ipc.IsGlamourerActive)
		{
			if (!StringExtensions.IsNullOrEmpty(glamourerData))
			{
				Ktisis.WarningNotification("MCDF has Glamourer data, but no IPC was found!\nCheck to make sure all plugins are enabled.");
			}
		}
		else
		{
			_ipc.GetGlamourerIpc().ApplyState(glamourerData, actor.ObjectIndex);
		}
	}

	private Guid? ApplyPenumbraMods(IGameObject actor, McdfData data, Dictionary<string, string> files)
	{
		if (!_ipc.IsPenumbraActive)
		{
			if (files.Count != 0)
			{
				Ktisis.WarningNotification("MCDF has Penumbra data, but no IPC was found!\nCheck to make sure all plugins are enabled.");
			}
			return null;
		}
		PenumbraIpcProvider penumbraIpc = _ipc.GetPenumbraIpc();
		Guid guid = penumbraIpc.CreateTemporaryCollection($"KtisisMCDF_{actor.ObjectIndex}");
		penumbraIpc.AssignTemporaryCollection(guid, actor.ObjectIndex);
		penumbraIpc.AssignTemporaryMods(guid, files);
		penumbraIpc.AssignManipulationData(guid, data.ManipulationData);
		return guid;
	}

	private void RevertGlamourerData(IGameObject actor)
	{
		if (_ipc.IsGlamourerActive)
		{
			_ipc.GetGlamourerIpc().RevertObject(actor);
		}
	}

	private void DeleteGlamourerData(IGameObject actor)
	{
		if (!_ipc.IsGlamourerActive || !_ipc.GetGlamourerIpc().DeleteState(actor, _objectTable.LocalPlayer))
		{
			Ktisis.WarningNotification("Unable to fully clear Glamourer IPC data for Actor " + actor.Name.TextValue + "!\nCheck /xllog for further details.");
		}
	}

	private void RevertCustomizeData(ushort index)
	{
		if (_ipc.IsCustomizeActive)
		{
			_ipc.GetCustomizeIpc().DeleteTemporaryProfile(index);
		}
	}

	private void DeleteCustomizeData(Guid id)
	{
		if (_ipc.IsCustomizeActive)
		{
			_ipc.GetCustomizeIpc().DeleteTemporaryProfileGuid(id);
		}
	}

	private async Task RedrawAndWait(IGameObject actor)
	{
		actor.Redraw();
		DateTime start = DateTime.Now;
		do
		{
			if (await _framework.RunOnFrameworkThread<bool>((Func<bool>)actor.IsDrawing))
			{
				return;
			}
			await Task.Delay(100);
		}
		while (actor.IsValid() && (DateTime.Now - start).TotalMilliseconds < 20000.0);
		Ktisis.Log.Warning($"Timed out waiting for '{actor.Name}' to redraw!");
	}

	private static string GetTempPath(bool create)
	{
		string text = Path.Join(Path.GetTempPath(), "Ktisis");
		if (create && !Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		return text;
	}

	public unsafe async void SetInvisibleSkin(ActorEntity actor)
	{
		PenumbraIpcProvider ipc = _ipc.GetPenumbraIpc();
		Guid? collectionId = ipc.AssignInvisibleSkin(actor.Actor);
		await RedrawAndWait(actor.Actor);
		Human* human = actor.GetHuman();
		if (human != null)
		{
			Model* ptr = ((Human)human).Models[10];
			if (ptr != null)
			{
				((Human)human).Models[10] = null;
				((ModelResourceHandle)((Model)ptr).ModelResourceHandle).DecRef();
				((Model)ptr).RefCount = 0u;
			}
			ptr = ((Human)human).Models[11];
			if (ptr != null)
			{
				((Human)human).Models[11] = null;
				((ModelResourceHandle)((Model)ptr).ModelResourceHandle).DecRef();
				((Model)ptr).RefCount = 0u;
			}
			ptr = ((Human)human).Models[12];
			if (ptr != null)
			{
				((Human)human).Models[12] = null;
				((ModelResourceHandle)((Model)ptr).ModelResourceHandle).DecRef();
				((Model)ptr).RefCount = 0u;
			}
		}
		ipc.RemoveTemporaryMod(collectionId);
	}

	public async void Revert(IGameObject actor)
	{
		Ktisis.Log.Debug($"IPC - Revert Actor '{actor.ObjectIndex}' ...");
		mcdfLocation.Remove(actor);
		RevertGlamourerData(actor);
		await RedrawAndWait(actor);
		RevertCustomizeData(actor.ObjectIndex);
		actors.Remove(actor);
	}

	public void RevertIfTouched(IGameObject actor)
	{
		if (actors.Keys.Contains(actor))
		{
			RevertNoDraw(actor, actors[actor]);
		}
	}

	private void RevertNoDraw(IGameObject actor, Guid? guid)
	{
		Ktisis.Log.Debug($"IPC - RevertNoDraw Actor '{actor.ObjectIndex}' ...");
		DeleteGlamourerData(actor);
		if (!guid.HasValue)
		{
			RevertCustomizeData(actor.ObjectIndex);
		}
		else
		{
			DeleteCustomizeData(guid.Value);
		}
		mcdfLocation.Remove(actor);
		actors.Remove(actor);
	}

	private void RevertAll()
	{
		foreach (var (actor, guid2) in actors)
		{
			RevertNoDraw(actor, guid2);
		}
		actors.Clear();
		actors.TrimExcess();
		if (_ipc.IsGlamourerActive)
		{
			_ipc.GetGlamourerIpc().Unlock();
		}
	}

	public string LoadedMCDFPath(IGameObject actor)
	{
		if (!mcdfLocation.ContainsKey(actor))
		{
			return string.Empty;
		}
		return mcdfLocation[actor];
	}

	public void Dispose()
	{
		Ktisis.Log.Info("Disposing MCDF manager.");
		string tempPath = GetTempPath(create: false);
		if (Directory.Exists(tempPath))
		{
			Directory.Delete(tempPath, recursive: true);
		}
		_gpose.StateChanged -= OnGPoseEvent;
	}
}
