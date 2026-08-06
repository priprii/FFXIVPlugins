using System;
using System.Linq;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Character;
using Ktisis.Scene.Entities.World;
using Ktisis.Scene.Factory.Types;
using Ktisis.Scene.Types;
using Ktisis.Services.Data;
using Ktisis.Structs.Objects;
using Lumina.Extensions;

namespace Ktisis.Scene.Factory.Builders;

public sealed class ObjectBuilder : EntityBuilder<WorldEntity, IObjectBuilder>, IObjectBuilder, IEntityBuilder<WorldEntity, IObjectBuilder>, IEntityBuilderBase<WorldEntity, IObjectBuilder>
{
	private readonly IPoseBuilder _pose;

	private readonly INameResolver _naming;

	private nint Address = IntPtr.Zero;

	protected override IObjectBuilder Builder => this;

	public ObjectBuilder(ISceneManager scene, IPoseBuilder pose, INameResolver naming)
		: base(scene)
	{
		_pose = pose;
		_naming = naming;
	}

	public IObjectBuilder SetAddress(nint address)
	{
		Address = address;
		return this;
	}

	public unsafe IObjectBuilder SetAddress(Object* pointer)
	{
		Address = (nint)pointer;
		return this;
	}

	private ObjectType GetObjectType()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((Object)Address).GetObjectType();
	}

	private ModelType GetModelType()
	{
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		return ((CharacterBase)Address).GetModelType();
	}

	private void SetFallbackName(string name)
	{
		if (StringExtensions.IsNullOrEmpty(base.Name))
		{
			base.Name = name;
		}
	}

	protected unsafe override WorldEntity Build()
	{
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_001f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0021: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Expected I4, but got Unknown
		if (Address == IntPtr.Zero)
		{
			throw new Exception("Attempted to build object from null pointer.");
		}
		ObjectType objectType = GetObjectType();
		object obj = (objectType - 2) switch
		{
			3 => new LightEntity(Scene), 
			1 => BuildCharaBase(), 
			0 => BuildWorldObject(), 
			_ => BuildDefault(), 
		};
		SetFallbackName(((object)(*(ObjectType*)(&objectType))/*cast due to constrained. prefix*/).ToString());
		((SceneEntity)obj).Name = base.Name;
		((WorldEntity)obj).Address = Address;
		return (WorldEntity)obj;
	}

	private unsafe WorldEntity BuildCharaBase()
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0007: Unknown result type (might be due to invalid IL or missing references)
		//IL_0009: Invalid comparison between Unknown and I4
		ModelType modelType = GetModelType();
		CharaEntity result = (((int)modelType != 4) ? new CharaEntity(Scene, _pose) : BuildWeapon());
		SetFallbackName(((object)(*(ModelType*)(&modelType))/*cast due to constrained. prefix*/).ToString());
		return result;
	}

	private ObjectEntity BuildWorldObject()
	{
		WorldObject? worldObject = LinqExtensions.FirstOrNull<WorldObject>(Scene.World.Objects.Where((WorldObject w) => w.Address == Address));
		if (!worldObject.HasValue)
		{
			throw new Exception($"Attempted to build BgObject not present in WorldService.\nAddress: {Address:X}");
		}
		return new ObjectEntity(Scene, worldObject.Value);
	}

	private WeaponEntity BuildWeapon()
	{
		WeaponEntity result = new WeaponEntity(Scene, _pose);
		if (StringExtensions.IsNullOrEmpty(base.Name))
		{
			string weaponName = GetWeaponName();
			if (weaponName != null)
			{
				base.Name = weaponName;
			}
		}
		return result;
	}

	private unsafe string? GetWeaponName()
	{
		Weapon* address = (Weapon*)Address;
		return _naming.GetWeaponName(((Weapon)address).ModelSetId, ((Weapon)address).SecondaryId, ((Weapon)address).Variant);
	}

	private WorldEntity BuildDefault()
	{
		return new WorldEntity(Scene);
	}
}
