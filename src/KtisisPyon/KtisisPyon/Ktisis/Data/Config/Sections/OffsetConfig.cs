using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Dalamud.Bindings.ImGui;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Skeleton;
using Newtonsoft.Json;

namespace Ktisis.Data.Config.Sections;

public class OffsetConfig
{
	public Dictionary<string, Dictionary<string, Vector3>> BoneOffsets = new Dictionary<string, Dictionary<string, Vector3>>();

	public Vector3? GetOffset(BoneNode bone)
	{
		if (BoneOffsets == null)
		{
			return null;
		}
		if (!(bone.Pose.Parent is ActorEntity actorEntity))
		{
			return null;
		}
		string raceSexId = actorEntity.GetRaceSexId();
		if (raceSexId == null)
		{
			return null;
		}
		if (BoneOffsets.TryGetValue(raceSexId, out Dictionary<string, Vector3> value) && value.TryGetValue(bone.Info.Name, out var value2))
		{
			return value2;
		}
		return default(Vector3);
	}

	public void UpsertOffset(string raceSexId, string boneName, Vector3 offset)
	{
		Vector3 value2;
		if (!BoneOffsets.TryGetValue(raceSexId, out Dictionary<string, Vector3> value))
		{
			BoneOffsets.Add(raceSexId, new Dictionary<string, Vector3> { { boneName, offset } });
		}
		else if (!value.TryGetValue(boneName, out value2))
		{
			BoneOffsets[raceSexId].Add(boneName, offset);
		}
		else
		{
			BoneOffsets[raceSexId][boneName] = offset;
		}
	}

	public void RemoveOffset(string raceSexId, string boneName)
	{
		if (BoneOffsets.TryGetValue(raceSexId, out Dictionary<string, Vector3> _))
		{
			BoneOffsets[raceSexId].Remove(boneName);
		}
	}

	public void RemoveOffsetsForId(string raceSexId)
	{
		if (BoneOffsets.TryGetValue(raceSexId, out Dictionary<string, Vector3> _))
		{
			BoneOffsets[raceSexId] = new Dictionary<string, Vector3>();
		}
	}

	public bool SaveToClipboard()
	{
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		try
		{
			ImGui.SetClipboardText(ImU8String.op_Implicit(Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject((object)BoneOffsets)))));
			return true;
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Could not serialize offsets to clipboard: {value}");
			return false;
		}
	}

	public bool LoadFromClipboard()
	{
		try
		{
			BoneOffsets = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, Vector3>>>(Encoding.UTF8.GetString(Convert.FromBase64String(ImGui.GetClipboardText())));
			return true;
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Could not deserialize offsets from clipboard: {value}");
			return false;
		}
	}

	public bool LoadLegacyFromClipboard(string? raceSexId)
	{
		if (raceSexId == null)
		{
			return false;
		}
		try
		{
			Dictionary<string, Vector3> dictionary = JsonConvert.DeserializeObject<Dictionary<string, Vector3>>(Encoding.UTF8.GetString(Convert.FromBase64String(ImGui.GetClipboardText())));
			if (dictionary == null)
			{
				return false;
			}
			LoadLegacy(raceSexId, dictionary);
		}
		catch (Exception value)
		{
			Ktisis.Log.Error($"Could not deserialize legacy offsets from clipboard: {value}");
			return false;
		}
		return true;
	}

	public void LoadLegacy(string raceSexId, Dictionary<string, Vector3> offsets)
	{
		foreach (string key in offsets.Keys)
		{
			offsets[key] = new Vector3(offsets[key].X, offsets[key].Y, offsets[key].Z);
		}
		BoneOffsets[raceSexId] = offsets;
	}
}
