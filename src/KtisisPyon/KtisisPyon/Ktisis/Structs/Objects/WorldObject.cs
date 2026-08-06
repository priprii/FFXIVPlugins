using System;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Client.System.Resource.Handle;
using FFXIVClientStructs.FFXIV.Common.Math;
using FFXIVClientStructs.Interop;
using FFXIVClientStructs.STD;
using Ktisis.Common.Utility;

namespace Ktisis.Structs.Objects;

public struct WorldObject : IEquatable<WorldObject>
{
	private readonly Pointer<Object> Pointer;

	public Transform InitialTransform { get; }

	public byte? InitialFlags { get; }

	public nint Address { get; }

	public string Path { get; }

	public unsafe ObjectType ObjectType => ((Object)Pointer.Value).GetObjectType();

	public unsafe WorldObject(Object* ptr)
	{
		//IL_000e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0013: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Invalid comparison between Unknown and I4
		InitialFlags = null;
		Pointer = Pointer<Object>.op_Implicit(ptr);
		Address = (nint)ptr;
		InitialTransform = new Transform(Vector3.op_Implicit(((Object)Pointer.Value).Position), Quaternion.op_Implicit(((Object)Pointer.Value).Rotation), Vector3.op_Implicit(((Object)Pointer.Value).Scale));
		Path = $"{Address:X}";
		if ((int)((Object)ptr).GetObjectType() == 2)
		{
			BgObject* address = (BgObject*)Address;
			InitialFlags = ((BgObject)address).Flags;
			ModelResourceHandle* modelResourceHandle = ((BgObject)address).ModelResourceHandle;
			if (modelResourceHandle != null)
			{
				Path = ((object)(*(StdString*)(&((ModelResourceHandle)modelResourceHandle).FileName))/*cast due to constrained. prefix*/).ToString();
			}
		}
	}

	public unsafe void Update()
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0031: Invalid comparison between Unknown and I4
		if (Pointer.Value != null)
		{
			((Object)Pointer.Value).UpdateRender();
			if ((int)((Object)Pointer.Value).GetObjectType() == 2)
			{
				BgObject* address = (BgObject*)Address;
				((BgObject)address).UpdateCulling();
			}
		}
	}

	public unsafe void SetOutline(OutlineChoice color)
	{
		DrawObject* address = (DrawObject*)Address;
		if (address != null)
		{
			address->OutlineFlags = color;
		}
	}

	private unsafe WorldObject? GetFirstChild()
	{
		if (Pointer.Value == null)
		{
			return null;
		}
		Object* value = Pointer.Value;
		Object* childObject = ((Object)value).ChildObject;
		if (childObject == null || childObject == value)
		{
			return null;
		}
		return new WorldObject(childObject);
	}

	private unsafe WorldObject? NextSibling()
	{
		if (Pointer.Value == null)
		{
			return null;
		}
		Object* value = Pointer.Value;
		Object* nextSiblingObject = ((Object)value).NextSiblingObject;
		if (nextSiblingObject == null || nextSiblingObject == value)
		{
			return null;
		}
		return new WorldObject(nextSiblingObject);
	}

	public IEnumerable<WorldObject> GetChildren()
	{
		WorldObject? child = GetFirstChild();
		if (!child.HasValue)
		{
			yield break;
		}
		yield return child.Value;
		WorldObject? firstSibling = child.Value.NextSibling();
		WorldObject? sibling = firstSibling;
		while (sibling.HasValue && sibling.Value.Address != Address && sibling.Value.Address != child.Value.Address)
		{
			yield return sibling.Value;
			sibling = sibling.Value.NextSibling();
			if (sibling?.Address == firstSibling?.Address)
			{
				break;
			}
		}
	}

	public IEnumerable<WorldObject> GetSiblings()
	{
		WorldObject? sibling = NextSibling();
		while (sibling.HasValue && sibling.Value.Address != Address)
		{
			yield return sibling.Value;
			sibling = sibling.Value.NextSibling();
		}
	}

	public bool Equals(WorldObject other)
	{
		return Address == other.Address;
	}
}
