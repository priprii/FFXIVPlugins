using Google.FlatBuffers;

namespace PyonPix.Ipc;

public static class MessagePayloadVerify
{
	public static bool Verify(Verifier verifier, byte typeId, uint tablePos)
	{
		bool flag = true;
		return (MessagePayload)typeId switch
		{
			MessagePayload.Command => CommandVerify.Verify(verifier, tablePos), 
			MessagePayload.Log => LogVerify.Verify(verifier, tablePos), 
			MessagePayload.InitializeBrowser => InitializeBrowserVerify.Verify(verifier, tablePos), 
			MessagePayload.HostInitializeState => HostInitializeStateVerify.Verify(verifier, tablePos), 
			MessagePayload.TabInitializeState => TabInitializeStateVerify.Verify(verifier, tablePos), 
			MessagePayload.CreateTab => CreateTabVerify.Verify(verifier, tablePos), 
			MessagePayload.DestroyTab => DestroyTabVerify.Verify(verifier, tablePos), 
			MessagePayload.UpdateFrame => UpdateFrameVerify.Verify(verifier, tablePos), 
			MessagePayload.CursorChanged => CursorChangedVerify.Verify(verifier, tablePos), 
			MessagePayload.NavigationStarting => NavigationStartingVerify.Verify(verifier, tablePos), 
			MessagePayload.HistoryChanged => HistoryChangedVerify.Verify(verifier, tablePos), 
			MessagePayload.TitleChanged => TitleChangedVerify.Verify(verifier, tablePos), 
			MessagePayload.NavigationCompleted => NavigationCompletedVerify.Verify(verifier, tablePos), 
			MessagePayload.NavigationCanceled => NavigationCanceledVerify.Verify(verifier, tablePos), 
			MessagePayload.FavIconChanged => FavIconChangedVerify.Verify(verifier, tablePos), 
			MessagePayload.WebMessageReceived => WebMessageReceivedVerify.Verify(verifier, tablePos), 
			MessagePayload.UpdateMediaState => UpdateMediaStateVerify.Verify(verifier, tablePos), 
			MessagePayload.ToggleTheatreMode => ToggleTheatreModeVerify.Verify(verifier, tablePos), 
			MessagePayload.ExtensionOperation => ExtensionOperationVerify.Verify(verifier, tablePos), 
			MessagePayload.Navigate => NavigateVerify.Verify(verifier, tablePos), 
			MessagePayload.Reload => ReloadVerify.Verify(verifier, tablePos), 
			MessagePayload.StopNavigation => StopNavigationVerify.Verify(verifier, tablePos), 
			MessagePayload.Resize => ResizeVerify.Verify(verifier, tablePos), 
			MessagePayload.Reposition => RepositionVerify.Verify(verifier, tablePos), 
			MessagePayload.SetFocusedTab => SetFocusedTabVerify.Verify(verifier, tablePos), 
			MessagePayload.SendMouseEvent => SendMouseEventVerify.Verify(verifier, tablePos), 
			MessagePayload.UpdateSpatialAudio => UpdateSpatialAudioVerify.Verify(verifier, tablePos), 
			MessagePayload.OpenDevTools => OpenDevToolsVerify.Verify(verifier, tablePos), 
			MessagePayload.InstallExtension => InstallExtensionVerify.Verify(verifier, tablePos), 
			MessagePayload.UninstallExtension => UninstallExtensionVerify.Verify(verifier, tablePos), 
			MessagePayload.EnableExtension => EnableExtensionVerify.Verify(verifier, tablePos), 
			MessagePayload.DisableExtension => DisableExtensionVerify.Verify(verifier, tablePos), 
			_ => true, 
		};
	}
}
