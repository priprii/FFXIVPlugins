using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using Ktisis.Structs.Lights;

namespace Ktisis.Data.Files;

public class LightFile : JsonFile
{
	public const int CurrentVersion = 2;

	public new string FileExtension { get; set; } = ".ktlight";

	public new string TypeName { get; set; } = "Ktisis Light";

	public new int FileVersion { get; set; } = 2;

	public string? Nickname { get; set; }

	public LightFlags Flags { get; set; }

	public LightType LightType { get; set; }

	public Transform? Transform { get; set; }

	public Vector3 RGB { get; set; }

	public float Intensity { get; set; }

	public float ShadowNear { get; set; }

	public float ShadowFar { get; set; }

	public FalloffType FalloffType { get; set; }

	public Vector2 AreaAngle { get; set; } = Vector2.Zero;

	public float Falloff { get; set; }

	public float LightAngle { get; set; }

	public float FalloffAngle { get; set; }

	public float Range { get; set; }

	public float CharaShadowRange { get; set; }

	public string? Gobo { get; set; }
}
