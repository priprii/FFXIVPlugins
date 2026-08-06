using System.Collections.Generic;
using Ktisis.Data.Config.Bones;
using Ktisis.Editor.Posing.Ik.Ccd;
using Ktisis.Editor.Posing.Ik.TwoJoints;
using Ktisis.Editor.Posing.Ik.Types;
using Ktisis.Scene.Decor;

namespace Ktisis.Editor.Posing.Ik;

public interface IIkController
{
	int GroupCount { get; }

	void Setup(ISkeleton skeleton);

	IEnumerable<(string name, IIkGroup group)> GetGroups();

	bool TrySetupGroup(string name, CcdGroupParams param, out CcdGroup? group);

	bool TrySetupGroup(string name, TwoJointsGroupParams param, out TwoJointsGroup? group);

	void Solve(bool frozen = false);

	bool IsEnabled();

	void Destroy();
}
