namespace PyonPix.Shared.Structs.Pix;

public readonly struct PixUpdate(IPix pix, PixUpdateType type, PixUpdateOrigin origin, bool editFinished = true, bool performLocalUpdate = true)
{
	public IPix Pix { get; } = pix;

	public PixUpdateType Type { get; } = type;

	public PixUpdateOrigin Origin { get; } = origin;

	public bool EditFinished { get; } = editFinished;

	public bool PerformLocalUpdate { get; } = performLocalUpdate;
}
