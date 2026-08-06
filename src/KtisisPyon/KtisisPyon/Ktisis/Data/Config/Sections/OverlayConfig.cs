using Ktisis.Structs.Objects;

namespace Ktisis.Data.Config.Sections;

public class OverlayConfig
{
	public bool Visible = true;

	public bool BulkVisOverride;

	public bool DrawLines = true;

	public bool DrawLinesGizmo = true;

	public bool DrawDotsGizmo = true;

	public bool DimOverlayForInactiveActors;

	public bool PresetsOnActiveActor;

	public ActiveState ActiveStateType;

	public float InactiveOpacity = 0.5f;

	public float DotRadius = 7f;

	public float LineThickness = 2f;

	public float LineOpacity = 0.95f;

	public float LineOpacityUsing = 0.15f;

	public bool DrawReferenceTitle = true;

	public float WorldNodeRadius = 7f;

	public float WorldNodeOutlineWidth = 1f;

	public float WorldNodeScaleFactor = 0.6f;

	public uint WorldNodeColor = uint.MaxValue;

	public uint ActorNodeColor = 4294901869u;

	public uint LightNodeColor = 4278247167u;

	public OutlineChoice WorldOutlineColor = OutlineChoice.Yellow;

	public float WorldCameraRange = 30f;

	public uint DefaultLineColor = uint.MaxValue;

	public bool ColorSelectedBoneParentLine = true;

	public bool ColorSelectedBoneDescendantLine = true;

	public uint SelectedBoneParentLineColor = 4281545727u;

	public uint SelectedBoneDescendantLineColor = 4281597747u;
}
