namespace PyonPix.Shared.Structs.Pix;

public interface ISynced<T>
{
	void ApplyTo(T target);
}
