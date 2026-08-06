using System;
using System.Numerics;
using PyonPix.Services.Game;

namespace PyonPix.Config.Pix;

public class LocalPix : BasePix
{
	public LocalPix()
	{
	}

	public LocalPix(string id, StateService? state)
	{
		base.Id = id;
		if (state?.CurrentTerritory != null)
		{
			base.Territory.WorldId = state.CurrentTerritory.WorldId;
			base.Territory.TerritoryId = state.CurrentTerritory.TerritoryId;
			base.Territory.Ward = state.CurrentTerritory.Ward;
			base.Territory.Plot = state.CurrentTerritory.Plot;
			base.Territory.Room = state.CurrentTerritory.Room;
			base.Territory.Floor = state.CurrentTerritory.Floor;
		}
		base.Renderer.Position = ((state == null) ? default(Vector3) : new Vector3(state.LocalPlayerPosition.X, state.LocalPlayerPosition.Y + 1f, state.LocalPlayerPosition.Z));
		base.Renderer.Rotation = ((state == null) ? default(Quaternion) : (Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float)Math.PI) * state.LocalPlayerRotation));
		base.Renderer.Scale = new Vector3(3f, 1.6875f, 0.03f);
		base.Light.Position = Vector3.Zero;
		base.Light.Rotation = Quaternion.Identity;
	}
}
