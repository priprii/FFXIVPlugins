using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Ktisis.Common.Utility;
using Ktisis.Scene.Decor;
using Ktisis.Scene.Types;
using Ktisis.Structs.Objects;

namespace Ktisis.Scene.Entities.World;

public class ObjectEntity : WorldEntity, IHideable
{
	public WorldObject Object;

	public unsafe bool IsHidden
	{
		get
		{
			DrawObject* address = (DrawObject*)base.Address;
			if (address != null)
			{
				return !((DrawObject)address).IsVisible;
			}
			return false;
		}
		set
		{
			DrawObject* address = (DrawObject*)base.Address;
			if (address != null)
			{
				((DrawObject)address).IsVisible = !((DrawObject)address).IsVisible;
			}
		}
	}

	public ObjectEntity(ISceneManager scene, WorldObject obj)
		: base(scene)
	{
		base.Type = EntityType.Model;
		Object = obj;
		base.Visible = true;
	}

	public override void SetTransform(Transform trans)
	{
		base.SetTransform(trans);
		Object.Update();
	}

	public void ToggleHidden()
	{
		IsHidden = !IsHidden;
	}

	public unsafe void Reset()
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Invalid comparison between Unknown and I4
		SetTransform(Object.InitialTransform);
		if ((int)Object.ObjectType == 2 && Object.InitialFlags.HasValue)
		{
			DrawObject* address = (DrawObject*)base.Address;
			((DrawObject)address).Flags = Object.InitialFlags.Value;
		}
	}

	public override void Remove()
	{
		try
		{
			Reset();
		}
		finally
		{
			base.Remove();
		}
	}
}
