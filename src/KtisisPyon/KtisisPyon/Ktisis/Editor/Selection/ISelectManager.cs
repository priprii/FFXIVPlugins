using System.Collections.Generic;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Game;

namespace Ktisis.Editor.Selection;

public interface ISelectManager
{
	int Count { get; }

	event SelectChangedHandler Changed;

	void Update();

	IEnumerable<SceneEntity> GetSelected();

	SceneEntity? GetFirstSelected();

	bool IsActorSelected(ActorEntity actor);

	bool IsSelected(SceneEntity entity);

	void Select(SceneEntity entity, SelectMode mode = SelectMode.Default);

	void Unselect(SceneEntity entity);

	void Clear();
}
