using System;
using System.Collections.Generic;
using System.IO;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Ktisis.Common.Utility;
using Newtonsoft.Json;
using Penumbra.Api.Enums;
using Penumbra.Api.IpcSubscribers;

namespace Ktisis.Interop.Ipc;

public class PenumbraIpcProvider
{
	private readonly IDalamudPluginInterface _dpi;

	private readonly GetCollections _getCollections;

	private readonly GetCollectionForObject _getCollectionForObject;

	private readonly SetCollectionForObject _setCollectionForObject;

	private readonly GetCutsceneParentIndex _getCutsceneParentIndex;

	private readonly SetCutsceneParentIndex _setCutsceneParentIndex;

	private readonly AssignTemporaryCollection _assignTemporaryCollection;

	private readonly ICallGateSubscriber<string, string, (PenumbraApiEc, Guid Guid)> _createTemporaryCollection;

	private readonly DeleteTemporaryCollection _deleteTemporaryCollection;

	private readonly AddTemporaryMod _addTemporaryMod;

	private readonly RemoveTemporaryMod _removeTemporaryMod;

	private readonly RedrawObject _redrawObject;

	public PenumbraIpcProvider(IDalamudPluginInterface dpi)
	{
		_dpi = dpi;
		_getCollections = new GetCollections(dpi);
		_getCollectionForObject = new GetCollectionForObject(dpi);
		_setCollectionForObject = new SetCollectionForObject(dpi);
		_getCutsceneParentIndex = new GetCutsceneParentIndex(dpi);
		_setCutsceneParentIndex = new SetCutsceneParentIndex(dpi);
		_assignTemporaryCollection = new AssignTemporaryCollection(dpi);
		_createTemporaryCollection = dpi.GetIpcSubscriber<string, string, (PenumbraApiEc, Guid)>("Penumbra.CreateTemporaryCollection.V6");
		_deleteTemporaryCollection = new DeleteTemporaryCollection(dpi);
		_addTemporaryMod = new AddTemporaryMod(dpi);
		_removeTemporaryMod = new RemoveTemporaryMod(dpi);
		_redrawObject = new RedrawObject(dpi);
	}

	public Dictionary<Guid, string> GetCollections()
	{
		return _getCollections.Invoke();
	}

	public (Guid Id, string Name) GetCollectionForObject(IGameObject gameObject)
	{
		return _getCollectionForObject.Invoke(gameObject.ObjectIndex).EffectiveCollection;
	}

	public bool SetCollectionForObject(IGameObject gameObject, Guid? id)
	{
		Ktisis.Log.Verbose($"Setting collection for '{gameObject.Name}' ({gameObject.ObjectIndex}) to '{id}'");
		PenumbraApiEc item = _setCollectionForObject.Invoke(gameObject.ObjectIndex, id).Item1;
		bool num = item == PenumbraApiEc.Success;
		if (!num)
		{
			Ktisis.Log.Warning($"Penumbra collection set failed with return code: {item}");
		}
		return num;
	}

	public int GetAssignedParentIndex(IGameObject gameObject)
	{
		return _getCutsceneParentIndex.Invoke(gameObject.ObjectIndex);
	}

	public void AssignTemporaryCollection(Guid collectionId, int actorIndex)
	{
		_assignTemporaryCollection.Invoke(collectionId, actorIndex);
	}

	public Guid CreateTemporaryCollection(string name)
	{
		return _createTemporaryCollection.InvokeFunc("Ktisis", name).Item2;
	}

	public void DeleteTemporaryCollection(Guid collectionId)
	{
		_deleteTemporaryCollection.Invoke(collectionId);
	}

	public bool SetAssignedParentIndex(IGameObject gameObject, int index)
	{
		Ktisis.Log.Verbose($"Setting assigned parent for '{gameObject.Name}' ({gameObject.ObjectIndex}) to {index}");
		PenumbraApiEc penumbraApiEc = _setCutsceneParentIndex.Invoke(gameObject.ObjectIndex, index);
		bool num = penumbraApiEc == PenumbraApiEc.Success;
		if (!num)
		{
			Ktisis.Log.Warning($"Penumbra parent set failed with return code: {penumbraApiEc}");
		}
		return num;
	}

	public void AssignTemporaryMods(Guid collectionId, Dictionary<string, string> paths)
	{
		PenumbraApiEc value = _removeTemporaryMod.Invoke("MareChara_Files", collectionId, 100);
		PenumbraApiEc value2 = _addTemporaryMod.Invoke("MareChara_Files", collectionId, paths, string.Empty, 100);
		Ktisis.Log.Info($"{value} {value2}");
	}

	public void RemoveTemporaryMod(Guid? Collection)
	{
		if (Collection.HasValue)
		{
			_removeTemporaryMod.Invoke("MareChara_Files", Collection.Value, 100);
		}
	}

	public Guid? AssignInvisibleSkin(IGameObject gameObject)
	{
		Ktisis.Log.Verbose($"Creating invisible skin collection for '{gameObject.Name}' ({gameObject.ObjectIndex})");
		(bool, bool, (Guid, string)) tuple = _getCollectionForObject.Invoke(gameObject.ObjectIndex);
		AssignTemporaryMods(tuple.Item3.Item1, BuildInvisibleSkinPaths());
		return tuple.Item3.Item1;
	}

	public void AssignManipulationData(Guid collectionId, string manipData)
	{
		_addTemporaryMod.Invoke("MareChara_Meta", collectionId, new Dictionary<string, string>(), manipData, 0);
	}

	public void Redraw(int index)
	{
		_redrawObject.Invoke(index);
	}

	private Dictionary<string, string> BuildInvisibleSkinPaths()
	{
		Stream manifestResource = ResourceUtil.GetManifestResource("Data.Library.skin-paths.json");
		string value = Path.Combine(Path.Combine(_dpi.AssemblyLocation.DirectoryName, "Assets"), "mt_c0101b0001_a.mtrl");
		using StreamReader streamReader = new StreamReader(manifestResource);
		Dictionary<string, string> dictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(streamReader.ReadToEnd());
		if (dictionary == null)
		{
			throw new Exception("Could not deserialize skin-paths.json!");
		}
		foreach (KeyValuePair<string, string> item in dictionary)
		{
			dictionary[item.Key] = value;
		}
		return dictionary;
	}
}
