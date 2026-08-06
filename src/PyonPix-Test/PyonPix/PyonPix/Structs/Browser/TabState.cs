namespace PyonPix.Structs.Browser;

public enum TabState
{
	Uninitialized,
	WaitingForHost,
	Creating,
	Ready,
	Failed,
	Destroyed
}
