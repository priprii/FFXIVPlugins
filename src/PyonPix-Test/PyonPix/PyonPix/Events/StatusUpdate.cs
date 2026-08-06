namespace PyonPix.Events;

public class StatusUpdate(string status, StatusType statusType = StatusType.Info, int displayTime = 5000, bool overlay = true)
{
	public string Status { get; set; } = status;

	public StatusType StatusType { get; set; } = statusType;

	public int DisplayTime { get; set; } = displayTime;

	public bool Overlay { get; set; } = overlay;
}
