using System;
using System.Drawing;
using System.Numerics;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using KamiToolKit.Overlay.UiOverlay;
using KamiToolKit.Premade.Node.Simple;
using Lumina.Text.ReadOnly;

namespace Ktisis.Interface.KTK;

public class BalloonNode : OverlayNode
{
	private SimpleNineGridNode BalloonBg;

	private SimpleNineGridNode BalloonGradient;

	private SimpleImageNode BalloonArrow;

	private TextNode TalkText;

	public BalloonBackground BgChoice;

	public BalloonColor ColorChoice;

	public string Dialog;

	public bool ArrowVisible;

	public float ArrowX;

	public uint FontSize;

	public override OverlayLayer OverlayLayer => OverlayLayer.BehindUserInterface;

	public override bool HideWithNativeUi => false;

	public override bool HideWithUiToggled => false;

	public BalloonNode(BalloonBackground bgChoice, BalloonColor colorChoice, string dialog, bool arrowVisible, float arrowX, uint fontSize)
	{
		BgChoice = bgChoice;
		ColorChoice = colorChoice;
		Dialog = dialog;
		ArrowVisible = arrowVisible;
		ArrowX = arrowX;
		FontSize = fontSize;
		BalloonBg = SetBalloonBg();
		BalloonGradient = SetBalloonGradient();
		BalloonArrow = SetBalloonArrow();
		TalkText = SetTalkText();
		BalloonBg.AttachNode((NodeBase?)this, NodePosition.AsLastChild);
		BalloonGradient.AttachNode((NodeBase?)this, NodePosition.AsLastChild);
		BalloonArrow.AttachNode((NodeBase?)this, NodePosition.AsLastChild);
		TalkText.AttachNode((NodeBase?)this, NodePosition.AsLastChild);
	}

	protected override void OnUpdate()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		TalkText.String = ReadOnlySeString.op_Implicit(Dialog);
		TalkText.FontSize = FontSize;
		BalloonBg.TextureCoordinates = CoordinatesForBg();
		BalloonGradient.TextureCoordinates = CoordinatesForGradient();
		BalloonGradient.MultiplyColor = ColorForGradient();
		BalloonArrow.IsVisible = ArrowVisible;
		if (ArrowVisible)
		{
			BalloonArrow.Position = new Vector2(Math.Clamp(ArrowX, 32f, 130f), 70f);
		}
	}

	private SimpleNineGridNode SetBalloonBg()
	{
		return new SimpleNineGridNode
		{
			TexturePath = "ui/uld/MiniTalkPlayer_hr1.tex",
			TextureSize = new Vector2(200f, 90f),
			TextureCoordinates = CoordinatesForBg(),
			Position = Vector2.Zero,
			Size = new Vector2(200f, 90f),
			TopOffset = 51f,
			BottomOffset = 37f,
			LeftOffset = 162f,
			RightOffset = 36f
		};
	}

	private SimpleNineGridNode SetBalloonGradient()
	{
		return new SimpleNineGridNode
		{
			TexturePath = "ui/uld/MiniTalkPlayer_hr1.tex",
			TextureSize = new Vector2(200f, 90f),
			TextureCoordinates = CoordinatesForGradient(),
			MultiplyColor = ColorForGradient(),
			Position = Vector2.Zero,
			Size = new Vector2(200f, 90f),
			TopOffset = 51f,
			BottomOffset = 37f,
			LeftOffset = 162f,
			RightOffset = 36f
		};
	}

	private SimpleImageNode SetBalloonArrow()
	{
		return new SimpleImageNode
		{
			TexturePath = "ui/uld/MiniTalkPlayer_hr1.tex",
			TextureSize = new Vector2(32f, 32f),
			TextureCoordinates = new Vector2(0f, 992f),
			Position = new Vector2(49f, 70f),
			Size = new Vector2(32f, 32f)
		};
	}

	private TextNode SetTalkText()
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		return new TextNode
		{
			Size = new Vector2(151f, 17f),
			Position = new Vector2(24f, 43f),
			TextColor = ColorHelpers.Vector(KnownColor.Black),
			FontType = (FontType)0,
			TextFlags = (TextFlags)1024,
			AlignmentType = (AlignmentType)4,
			FontSize = FontSize,
			LineSpacing = 14u,
			String = ReadOnlySeString.op_Implicit(Dialog)
		};
	}

	private Vector2 CoordinatesForBg()
	{
		return BgChoice switch
		{
			BalloonBackground.Say => new Vector2(0f, 0f), 
			BalloonBackground.Party => new Vector2(0f, 90f), 
			BalloonBackground.Tell => new Vector2(0f, 180f), 
			BalloonBackground.Alliance => new Vector2(0f, 270f), 
			BalloonBackground.Yell => new Vector2(0f, 360f), 
			BalloonBackground.Shout => new Vector2(0f, 450f), 
			BalloonBackground.FC => new Vector2(0f, 540f), 
			BalloonBackground.LS => new Vector2(0f, 630f), 
			BalloonBackground.CWLS => new Vector2(0f, 720f), 
			BalloonBackground.Novice => new Vector2(0f, 810f), 
			BalloonBackground.PVP => new Vector2(0f, 900f), 
			_ => default(Vector2), 
		};
	}

	private Vector2 CoordinatesForGradient()
	{
		return BgChoice switch
		{
			BalloonBackground.Say => new Vector2(200f, 0f), 
			BalloonBackground.Party => new Vector2(200f, 90f), 
			BalloonBackground.Tell => new Vector2(200f, 180f), 
			BalloonBackground.Alliance => new Vector2(200f, 270f), 
			BalloonBackground.Yell => new Vector2(200f, 360f), 
			BalloonBackground.Shout => new Vector2(200f, 450f), 
			BalloonBackground.FC => new Vector2(200f, 540f), 
			BalloonBackground.LS => new Vector2(200f, 630f), 
			BalloonBackground.CWLS => new Vector2(200f, 720f), 
			BalloonBackground.Novice => new Vector2(200f, 810f), 
			BalloonBackground.PVP => new Vector2(200f, 900f), 
			_ => default(Vector2), 
		};
	}

	private Vector3 ColorForGradient()
	{
		return ColorChoice switch
		{
			BalloonColor.Default => new Vector3(83f, 76f, 58f), 
			BalloonColor.Lime => new Vector3(74f, 74f, 0f), 
			BalloonColor.Orange => new Vector3(87f, 60f, 28f), 
			BalloonColor.Violet => new Vector3(76f, 48f, 63f), 
			BalloonColor.SkyBlue => new Vector3(39f, 70f, 78f), 
			BalloonColor.Clay => new Vector3(72f, 40f, 22f), 
			BalloonColor.LightJeans => new Vector3(43f, 58f, 62f), 
			BalloonColor.GrassGreen => new Vector3(47f, 62f, 11f), 
			BalloonColor.Gray => new Vector3(50f, 50f, 50f), 
			BalloonColor.Pink => new Vector3(78f, 50f, 50f), 
			BalloonColor.DarkJeans => new Vector3(27f, 39f, 51f), 
			BalloonColor.Green => new Vector3(36f, 58f, 36f), 
			BalloonColor.Purple => new Vector3(40f, 32f, 46f), 
			BalloonColor.Brown => new Vector3(54f, 44f, 26f), 
			BalloonColor.CloudyBlue => new Vector3(40f, 63f, 80f), 
			BalloonColor.RoyalPurple => new Vector3(51f, 29f, 41f), 
			_ => default(Vector3), 
		} / 100f;
	}
}
