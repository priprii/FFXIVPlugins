namespace PyonPix.Ipc;

public enum MessageType : sbyte
{
	None,
	Shutdown,
	ShutdownAck,
	MouseEvent,
	KeyEvent
}
