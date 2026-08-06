using Ktisis.Editor.Posing.Ik.Ccd;

namespace Ktisis.Scene.Decor.Ik;

public interface ICcdNode : IIkNode
{
	CcdGroup Group { get; }
}
