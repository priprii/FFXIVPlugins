using System.Collections.Generic;
using Ktisis.Editor.Characters;
using Ktisis.Editor.Posing.Data;

namespace Ktisis.Data.Config.Sections;

public class FileConfig
{
	public Dictionary<string, string> LastOpenedPaths = new Dictionary<string, string>();

	public SaveModes ImportCharaModes = SaveModes.All;

	public bool ImportNpcApplyOnSelect;

	public bool ImportPoseSelectedBones;

	public bool SelectedBonesIncludeDescendants;

	public bool AnchorPoseSelectedBones;

	public bool ExcludePoseEarBones;

	public PoseTransforms ImportPoseTransforms = PoseTransforms.Rotation;

	public PoseMode ImportPoseModes = PoseMode.All;

	public string DefaultLocation = string.Empty;

	public List<(string Path, string Name)> CustomLocations { get; set; } = new List<(string, string)>();
}
