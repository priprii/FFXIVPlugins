using System.Numerics;
using Ktisis.Scene.Entities;

namespace Ktisis.Interface.Overlay;

public interface IItemSelect
{
	string Name { get; }

	SceneEntity Entity { get; }

	Vector2 ScreenPos { get; }

	float Distance { get; }

	float OpacityMultiplier { get; }

	bool IsHovered { get; set; }
}
