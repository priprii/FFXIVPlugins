using System;
using System.Linq;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace TriggerPyon;

public class RestoreContext
{
	private Plugin plugin;

	public RestoreType Type { get; }

	public ushort? EmoteId { get; }

	public EntityInfo? Target { get; }

	public double Rotation { get; }

	public Vector3 Position { get; }

	public RestoreContext(Plugin plugin, RestoreType type)
	{
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		this.plugin = plugin;
		Type = type;
		EntityInfo localPlayer = PlayerManager.LocalPlayer;
		if (localPlayer != null)
		{
			EmoteId = (localPlayer.IsLoopingEmote ? new ushort?(localPlayer.EmoteId) : ((ushort?)null));
			Target = (localPlayer.IsTargetValid ? new EntityInfo(localPlayer.Target) : null);
			Rotation = localPlayer.Angle;
			Position = localPlayer.Position;
		}
	}

	public void Restore()
	{
		Plugin.Framework.RunOnFrameworkThread((Action)delegate
		{
			EntityInfo localPlayer = PlayerManager.LocalPlayer;
			if (localPlayer != null)
			{
				if (Type.HasFlag(RestoreType.Emote) && EmoteId.HasValue)
				{
					Emote emote = plugin.Emotes.FirstOrDefault((Emote x) => x.ID == EmoteId);
					if (emote != null && !emote.IsPose)
					{
						Game.ForceDisableMovement++;
						localPlayer.SetEmote(emote.ID);
						Game.ForceDisableMovement--;
					}
				}
				if (Type.HasFlag(RestoreType.Target))
				{
					if (Target != null && Target.IsValid)
					{
						Target.SetAsTarget();
					}
					else
					{
						Plugin.Targets.Target = null;
					}
				}
				if (Type.HasFlag(RestoreType.Rotation))
				{
					localPlayer.SetRotation((float)Rotation);
				}
				Type.HasFlag(RestoreType.Position);
			}
		});
	}
}
