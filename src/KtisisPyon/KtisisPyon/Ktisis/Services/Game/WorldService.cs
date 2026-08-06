using System;
using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Ktisis.Core.Attributes;
using Ktisis.Structs.Objects;

namespace Ktisis.Services.Game;

[Singleton]
public class WorldService : IDisposable
{
	private readonly GPoseService _gpose;

	private bool _init;

	public readonly List<WorldObject> Objects = new List<WorldObject>();

	public readonly List<WorldObject> Lights = new List<WorldObject>();

	public WorldService(GPoseService gpose)
	{
		_gpose = gpose;
		_gpose.StateChanged += OnGPoseEvent;
	}

	private void OnGPoseEvent(object sender, bool active)
	{
		Clean();
		if (active)
		{
			BuildWorld();
		}
	}

	public void Refresh()
	{
		Clean();
		BuildWorld();
	}

	private void BuildWorld()
	{
		Ktisis.Log.Debug("starting worldobject fetch...");
		List<WorldObject> source = RecurseWorld().ToList();
		Objects.AddRange(source.Where((WorldObject obj) => (int)obj.ObjectType == 2));
		Lights.AddRange(source.Where((WorldObject light) => (int)light.ObjectType == 5));
		Ktisis.Log.Debug($"finished!\n{Objects.Count} bgobjects found\n{Lights.Count} lights found");
		_init = true;
	}

	private IEnumerable<WorldObject> RecurseWorld()
	{
		WorldObject? worldObj = GetWorld();
		if (!worldObj.HasValue)
		{
			yield break;
		}
		foreach (WorldObject sibling in worldObj.Value.GetSiblings())
		{
			yield return sibling;
			foreach (WorldObject item in RecurseChildren(sibling))
			{
				yield return item;
			}
		}
		foreach (WorldObject item2 in RecurseChildren(worldObj.Value))
		{
			yield return item2;
		}
	}

	private IEnumerable<WorldObject> RecurseChildren(WorldObject worldObj)
	{
		foreach (WorldObject child in worldObj.GetChildren())
		{
			yield return child;
			foreach (WorldObject item in RecurseChildren(child))
			{
				yield return item;
			}
		}
	}

	private unsafe WorldObject? GetWorld()
	{
		World* ptr = World.Instance();
		if (ptr == null)
		{
			return null;
		}
		return new WorldObject(&((World)ptr).Object);
	}

	private void Clean()
	{
		if (_init)
		{
			Objects.Clear();
			_init = false;
		}
	}

	public void Dispose()
	{
		Clean();
		_gpose.StateChanged -= OnGPoseEvent;
	}
}
