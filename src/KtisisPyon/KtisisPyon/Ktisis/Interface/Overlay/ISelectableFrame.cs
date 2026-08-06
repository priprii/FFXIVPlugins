using System.Collections.Generic;
using System.Numerics;
using Ktisis.Editor.Context.Types;
using Ktisis.Scene.Entities;

namespace Ktisis.Interface.Overlay;

public interface ISelectableFrame
{
	IEnumerable<IItemSelect> GetItems();

	void AddItem(SceneEntity entity, Vector3 worldPos, IEditorContext ctx, float opacityMultiplier = 1f);
}
