using System;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Client.System.Resource.Handle;
using Ktisis.Common.Utility;
using Ktisis.Data.Config.Gobos;
using Ktisis.Editor.Posing.Types;
using Ktisis.Scene.Decor;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Scene.Modules.Lights;
using Ktisis.Scene.Types;
using Ktisis.Structs.Attachment;
using Ktisis.Structs.Lights;
using Ktisis.Structs.Objects;

namespace Ktisis.Scene.Entities.World;

public class LightEntity : WorldEntity, IDeletable, IHideable, IAttachable, ICharacter
{
	private IAttachTarget? _attachTarget;

	public LightEntityFlags Flags { get; set; }

	public GoboEntry? Gobo { get; set; }

	public WorldObject? WorldLight { get; set; }

	public unsafe bool IsHidden
	{
		get
		{
			SceneLight* ptr = GetObject();
			if (ptr != null)
			{
				return !((DrawObject)(&ptr->DrawObject)).IsVisible;
			}
			return false;
		}
		set
		{
			SceneLight* ptr = GetObject();
			if (ptr != null)
			{
				((DrawObject)(&ptr->DrawObject)).IsVisible = !((DrawObject)(&ptr->DrawObject)).IsVisible;
			}
		}
	}

	public new unsafe SceneLight* GetObject()
	{
		return (SceneLight*)base.GetObject();
	}

	public void SetAttach(IAttachTarget attachTarget)
	{
		_attachTarget = attachTarget;
	}

	public bool IsAttached()
	{
		return _attachTarget != null;
	}

	public unsafe Attach* GetAttach()
	{
		return null;
	}

	public PartialBoneInfo? GetParentBone()
	{
		if (_attachTarget is BoneNode boneNode)
		{
			return boneNode.Info;
		}
		if (_attachTarget is BoneNodeGroup boneNodeGroup)
		{
			return (from b in boneNodeGroup.GetIndividualBones()
				where b.Info.PartialIndex == 0
				select b).MinBy((BoneNode b) => b.Info.BoneIndex)?.Info;
		}
		return null;
	}

	public void Detach()
	{
		_attachTarget = null;
	}

	public unsafe CharacterBase* GetCharacter()
	{
		return null;
	}

	public LightEntity(ISceneManager scene)
		: base(scene)
	{
		base.Type = EntityType.Light;
	}

	private LightModule GetModule()
	{
		return Scene.GetModule<LightModule>();
	}

	public unsafe void SetType(LightType type)
	{
		SceneLight* ptr = GetObject();
		if (ptr != null && ptr->RenderLight != null)
		{
			ptr->RenderLight->LightType = type;
		}
	}

	public override void Update()
	{
		if (!IsValid)
		{
			return;
		}
		if (IsAttached() && _attachTarget is ITransform transform)
		{
			Transform transform2 = transform.GetTransform();
			if (transform2 != null)
			{
				base.SetTransform(transform2);
			}
		}
		if (Flags.HasFlag(LightEntityFlags.Update))
		{
			GetModule().UpdateLightObject(this);
		}
		if (WorldLight.HasValue)
		{
			UpdateWorldLight();
		}
		base.Update();
	}

	public override void SetTransform(Transform trans)
	{
		base.SetTransform(trans);
		Flags |= LightEntityFlags.Update;
	}

	public void ToggleHidden()
	{
		IsHidden = !IsHidden;
	}

	public unsafe void RemoveGobo()
	{
		Gobo = null;
		SceneLight* ptr = GetObject();
		if (ptr != null && ptr->Texture != null)
		{
			((TextureResourceHandle)ptr->Texture).DecRef();
			ptr->Texture = null;
		}
		if (ptr != null && ptr->RenderLight != null && ptr->RenderLight->Texture != null)
		{
			ptr->RenderLight->Texture = null;
		}
	}

	public unsafe void SetGobo(GoboEntry selected)
	{
		Gobo = selected;
		Scene.GetModule<LightModule>().UpdateSceneLightTexture(GetObject(), selected.Path);
	}

	private unsafe void UpdateWorldLight()
	{
		SceneLight* address = (SceneLight*)WorldLight.Value.Address;
		if (address != null)
		{
			((DrawObject)(&address->DrawObject)).IsVisible = false;
		}
	}

	private unsafe void ResetWorldLight()
	{
		if (WorldLight.HasValue)
		{
			SceneLight* address = (SceneLight*)WorldLight.Value.Address;
			if (address != null)
			{
				((DrawObject)(&address->DrawObject)).IsVisible = true;
			}
		}
	}

	public bool Delete()
	{
		ResetWorldLight();
		GetModule().Delete(this);
		return base.Address == IntPtr.Zero;
	}

	public override void Remove()
	{
		ResetWorldLight();
		base.Remove();
	}
}
