using PyonPix.Structs.Browser;

namespace PyonPix.Config.Global.Properties;

public class BrowserGlobalProperties
{
	public HomeUriType HomeUriType = HomeUriType.Starry;

	public string HomeUri = string.Empty;

	public SpawnBehaviour TerritorySpawnBehaviour = SpawnBehaviour.Navigate;

	public DespawnBehaviour TerritoryDespawnBehaviour = DespawnBehaviour.Shutdown;

	public bool CheckUpdateExtensions;

	public bool AutoUpdateExtensions;

	public bool ScreenInteractionFrontFace = true;

	public bool ScreenInteractionReqCtrl = true;

	public bool ScreenInteractionReqShift;

	public bool ScreenInteractionCaptureLButton = true;

	public bool ScreenInteractionCaptureRButton;

	public bool ScreenInteractionCaptureMButton;

	public bool ScreenInteractionCaptureScroll;

	public bool ScreenInteractionCursorChanges;
}
