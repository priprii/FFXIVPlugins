namespace PyonPix.Ipc;

public enum CommandType : sbyte
{
	MediatorInitializeRequest,
	MediatorInitializeSuccess,
	BrowserInitializeRequest,
	BrowserInitializeSuccess,
	BrowserInitializeFailed,
	BrowserHeartbeat,
	BrowserShutdown,
	BrowserLostFocus
}
