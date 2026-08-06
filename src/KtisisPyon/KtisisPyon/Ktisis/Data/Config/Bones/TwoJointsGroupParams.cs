using System.Collections.Generic;

namespace Ktisis.Data.Config.Bones;

public class TwoJointsGroupParams
{
	public TwoJointsType Type;

	public List<string> FirstBone = new List<string>();

	public List<string> FirstTwist = new List<string>();

	public List<string> SecondBone = new List<string>();

	public List<string> SecondTwist = new List<string>();

	public List<string> EndBone = new List<string>();
}
