using System;

namespace TargetPyon;

public class PlayerEntity(PlayerEntityInfo instance)
{
	public DateTime TargetTime { get; set; } = DateTime.Now;

	internal PlayerEntityInfo Instance { get; set; } = instance;

	internal PlayerEntityInfo? TargetInstance { get; set; }
}
