using Ktisis.Data.Config.Gobos;
using Ktisis.Data.Files;
using Ktisis.Data.Serialization;
using Ktisis.Scene.Entities.World;
using Ktisis.Structs.Lights;

namespace Ktisis.Editor.Lights;

public class EntityLightConverter
{
	private readonly GoboSchema _goboSchema;

	private LightEntity _light;

	public EntityLightConverter(LightEntity light)
	{
		_light = light;
		_goboSchema = SchemaReader.ReadGobos();
	}

	public unsafe void Apply(LightFile file)
	{
		SceneLight* ptr = _light.GetObject();
		RenderLight* ptr2 = ((ptr != null) ? ptr->RenderLight : null);
		if (ptr2 == null)
		{
			return;
		}
		_light.Flags |= LightEntityFlags.Update;
		_light.Name = file.Nickname;
		ptr2->Flags = file.Flags;
		ptr2->LightType = file.LightType;
		ptr2->Color.RGB = file.RGB;
		ptr2->Color.Intensity = file.Intensity;
		ptr2->ShadowNear = file.ShadowNear;
		ptr2->ShadowFar = file.ShadowFar;
		ptr2->FalloffType = file.FalloffType;
		ptr2->AreaAngle = file.AreaAngle;
		ptr2->Falloff = file.Falloff;
		ptr2->LightAngle = file.LightAngle;
		ptr2->FalloffAngle = file.FalloffAngle;
		ptr2->Range = file.Range;
		ptr2->CharaShadowRange = file.CharaShadowRange;
		if (file.Gobo != null)
		{
			GoboEntry goboEntry = _goboSchema.Gobos.Find((GoboEntry gob) => gob.Path == file.Gobo);
			if (goboEntry != null)
			{
				_light.SetGobo(goboEntry);
			}
		}
	}

	public LightFile Save()
	{
		LightFile lightFile = new LightFile
		{
			Nickname = _light.Name
		};
		Write(lightFile);
		return lightFile;
	}

	private unsafe void Write(LightFile file)
	{
		SceneLight* ptr = _light.GetObject();
		RenderLight* ptr2 = ((ptr != null) ? ptr->RenderLight : null);
		if (ptr2 != null)
		{
			file.Flags = ptr2->Flags;
			file.LightType = ptr2->LightType;
			file.RGB = ptr2->Color.RGB;
			file.Intensity = ptr2->Color.Intensity;
			file.ShadowNear = ptr2->ShadowNear;
			file.ShadowFar = ptr2->ShadowFar;
			file.FalloffType = ptr2->FalloffType;
			file.AreaAngle = ptr2->AreaAngle;
			file.Falloff = ptr2->Falloff;
			file.LightAngle = ptr2->LightAngle;
			file.FalloffAngle = ptr2->FalloffAngle;
			file.Range = ptr2->Range;
			file.CharaShadowRange = ptr2->CharaShadowRange;
			file.Gobo = _light.Gobo?.Path;
		}
	}
}
