using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Math;
using Ktisis.Common.Utility;
using Ktisis.Scene.Decor;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Entities.World;

public class WorldEntity : SceneEntity, ITransform, IVisibility
{
	public nint Address { get; set; }

	public bool Visible { get; set; }

	public unsafe virtual bool IsObjectValid => GetObject() != null;

	public WorldEntity(ISceneManager scene)
		: base(scene)
	{
	}

	public unsafe virtual Object* GetObject()
	{
		return (Object*)Address;
	}

	public virtual void Setup()
	{
		Clear();
	}

	public override void Update()
	{
		if (IsObjectValid)
		{
			UpdateChildren();
			base.Update();
		}
	}

	private unsafe void UpdateChildren()
	{
		Object* ptr = GetObject();
		if (ptr == null)
		{
			return;
		}
		List<nint> list = new List<nint>();
		Object* childObject = ((Object)ptr).ChildObject;
		Object* ptr2 = childObject;
		while (ptr2 != null)
		{
			list.Add((nint)ptr2);
			ptr2 = ((Object)ptr2).NextSiblingObject;
			if (ptr2 == childObject)
			{
				break;
			}
		}
		foreach (WorldEntity item in Children.Where((SceneEntity x) => x is WorldEntity).Cast<WorldEntity>().ToList())
		{
			if (list.Contains(item.Address))
			{
				list.Remove(item.Address);
			}
			else
			{
				item.Remove();
			}
		}
		foreach (nint item2 in list)
		{
			CreateObjectEntity((Object*)item2);
		}
	}

	private unsafe void CreateObjectEntity(Object* ptr)
	{
		Ktisis.Log.Verbose($"Creating object entity for {(nint)ptr:X}");
		Scene.Factory.BuildObject().SetAddress(ptr).Add(this);
	}

	public unsafe Transform? GetTransform()
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_001a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		Object* ptr = GetObject();
		if (ptr == null)
		{
			return null;
		}
		return new Transform(Vector3.op_Implicit(((Object)ptr).Position), Quaternion.op_Implicit(((Object)ptr).Rotation), Vector3.op_Implicit(((Object)ptr).Scale));
	}

	public unsafe virtual void SetTransform(Transform trans)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_002a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003b: Unknown result type (might be due to invalid IL or missing references)
		Object* ptr = GetObject();
		if (ptr != null)
		{
			Unsafe.Write(&((Object)ptr).Position, Vector3.op_Implicit(trans.Position));
			Unsafe.Write(&((Object)ptr).Rotation, Quaternion.op_Implicit(trans.Rotation));
			Unsafe.Write(&((Object)ptr).Scale, Vector3.op_Implicit(trans.Scale));
		}
	}
}
