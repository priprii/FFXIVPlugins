using System.Collections.Generic;
using Dalamud.Interface;
using Ktisis.Data.Config.Entity;
using Ktisis.Scene.Entities.Utility;
using Ktisis.Scene.Types;

namespace Ktisis.Data.Config.Sections;

public class EditorConfig
{
	public bool OpenOnEnterGPose = true;

	public bool ToggleOpenWindows = true;

	public bool ConfirmExit;

	public bool UseToolbar;

	public bool OpenTrayOnWorkspaceClose = true;

	public bool ShowHints = true;

	public bool ToggleEditorOnSelect = true;

	public bool CloseEditorOnDeselect;

	public bool SelectOnTarget;

	public bool IncognitoPlayerNames;

	public bool UseLegacyWindowBehavior;

	public bool UseLegacyPoseViewTabs;

	public bool UseLegacyLightEditor;

	public float WorkcamMoveSpeed = 0.1f;

	public float WorkcamFastMulti = 2.5f;

	public float WorkcamSlowMulti = 0.25f;

	public float WorkcamVertMulti = 1f;

	public float WorkcamSens = 0.215f;

	public Dictionary<EntityType, EntityDisplay> Display = EntityDisplay.GetDefaults();

	public bool LinkedGaze;

	public List<ReferenceImage.SetupData> ReferenceImages = new List<ReferenceImage.SetupData>();

	public bool TransformHide;

	public bool PlayEmoteStart = true;

	public bool ForceLoop = true;

	public bool AutoResizeObjectEditor;

	public bool FlyoutOpen;

	public EntityDisplay GetDisplayForType(EntityType type)
	{
		return Display.GetValueOrDefault(type, new EntityDisplay(uint.MaxValue, (FontAwesomeIcon)0));
	}
}
