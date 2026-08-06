using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;

namespace Ktisis.Editor.Posing;

public unsafe delegate void SkeletonInitHandler(IGameObject owner, Skeleton* skeleton, ushort partialId);
