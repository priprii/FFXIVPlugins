using Dalamud.Game.ClientState.Objects.Types;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;

namespace Ktisis.Editor.Characters.Types;

public unsafe delegate void DisableDrawHandler(IGameObject gameObject, DrawObject* drawObject);
