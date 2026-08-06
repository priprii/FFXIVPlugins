namespace Hypostasis.Game;

public unsafe abstract class VirtualTable(nint* v)
{
	protected unsafe readonly nint* vtbl = v;

	public unsafe nint this[int i] => vtbl[i];
}
