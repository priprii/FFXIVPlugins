using Ktisis.Data.Config.Sections;

namespace Ktisis.Editor.Transforms;

public record TransformSetup
{
	public MirrorMode MirrorRotation;

	public bool ParentBones = true;

	public bool RelativeBones = true;

	public void Configure(GizmoConfig cfg)
	{
		MirrorRotation = cfg.MirrorRotation;
		ParentBones = cfg.ParentBones;
		RelativeBones = cfg.RelativeBones;
	}
}
