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

public class TalkNode : OverlayNode
{
	private SimpleImageNode TalkBgNode;

	private NineGridNode SpeakerBgNode;

	private SimpleImageNode ClickyNode;

	private TextNode TalkText;

	private TextNode SpeakerText;

	public TalkBackground BgChoice;

	public TalkCursor CursorChoice;

	public string Speaker;

	public string Dialog;

	public uint FontSize;

	public override OverlayLayer OverlayLayer => OverlayLayer.BehindUserInterface;

	public override bool HideWithNativeUi => false;

	public override bool HideWithUiToggled => false;

	public TalkNode(TalkBackground bgChoice, TalkCursor cursorChoice, string speaker, string dialog, uint fontSize)
	{
		BgChoice = bgChoice;
		CursorChoice = cursorChoice;
		Speaker = speaker;
		Dialog = dialog;
		FontSize = fontSize;
		TalkBgNode = SetTalkBg();
		SpeakerBgNode = SetSpeakerBg();
		ClickyNode = SetClicky();
		TalkText = SetTalkText();
		SpeakerText = SetSpeakerText();
		TalkBgNode.AttachNode((NodeBase?)this, NodePosition.AsLastChild);
		SpeakerBgNode.AttachNode((NodeBase?)this, NodePosition.AsLastChild);
		ClickyNode.AttachNode((NodeBase?)this, NodePosition.AsLastChild);
		TalkText.AttachNode((NodeBase?)this, NodePosition.AsLastChild);
		SpeakerText.AttachNode((NodeBase?)this, NodePosition.AsLastChild);
	}

	protected override void OnUpdate()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0044: Unknown result type (might be due to invalid IL or missing references)
		TalkText.String = ReadOnlySeString.op_Implicit(Dialog);
		TalkText.TextColor = TextColorForBg();
		TalkText.FontSize = FontSize;
		SpeakerText.String = ReadOnlySeString.op_Implicit(Speaker);
		ClickyNode.TextureCoordinates = CoordinatesForCursor();
		ClickyNode.IsVisible = CursorChoice != TalkCursor.None;
		TalkBgNode.TexturePath = ((BgChoice <= TalkBackground.Echo) ? "ui/uld/Talk_Basic_hr1.tex" : "ui/uld/Talk_Other_hr1.tex");
		TalkBgNode.TextureCoordinates = CoordinatesForBg();
	}

	private SimpleImageNode SetTalkBg()
	{
		return new SimpleImageNode
		{
			Size = new Vector2(544f, 144f),
			WrapMode = WrapMode.Stretch,
			Scale = new Vector2(1.25f),
			TexturePath = ((BgChoice <= TalkBackground.Echo) ? "ui/uld/Talk_Basic_hr1.tex" : "ui/uld/Talk_Other_hr1.tex"),
			TextureCoordinates = CoordinatesForBg(),
			TextureSize = new Vector2(544f, 144f)
		};
	}

	private unsafe NineGridNode SetSpeakerBg()
	{
		NineGridNode nineGridNode = new NineGridNode();
		nineGridNode.Size = new Vector2(288f, 36f);
		nineGridNode.Position = new Vector2(18f, 0f);
		nineGridNode.Scale = new Vector2(1.25f);
		nineGridNode.TopOffset = 0f;
		nineGridNode.LeftOffset = 50f;
		nineGridNode.RightOffset = 1f;
		nineGridNode.BottomOffset = 0f;
		nineGridNode.AddPart(new Part
		{
			TexturePath = "ui/uld/Talk_hr1.tex",
			TextureCoordinates = new Vector2(0f, 0f),
			Size = new Vector2(288f, 36f),
			Id = 0u
		});
		return nineGridNode;
	}

	private SimpleImageNode SetClicky()
	{
		return new SimpleImageNode
		{
			Size = new Vector2(18f, 24f),
			Position = new Vector2(614f, 104f),
			WrapMode = WrapMode.Tile,
			TexturePath = "ui/uld/Talk_hr1.tex",
			TextureCoordinates = CoordinatesForCursor(),
			TextureSize = new Vector2(16f, 24f),
			IsVisible = (CursorChoice != TalkCursor.None)
		};
	}

	private TextNode SetTalkText()
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		return new TextNode
		{
			Size = new Vector2(556f, 90f),
			Position = new Vector2(62f, 42f),
			TextColor = TextColorForBg(),
			FontType = (FontType)0,
			TextFlags = (TextFlags)448,
			FontSize = FontSize,
			LineSpacing = 18u,
			String = ReadOnlySeString.op_Implicit(Dialog)
		};
	}

	private TextNode SetSpeakerText()
	{
		//IL_0074: Unknown result type (might be due to invalid IL or missing references)
		return new TextNode
		{
			Size = new Vector2(300f, 36f),
			Position = new Vector2(60f, 2f),
			TextColor = ColorHelpers.Vector(KnownColor.White),
			TextOutlineColor = ColorHelpers.Vector(KnownColor.Black),
			FontType = (FontType)0,
			FontSize = 18u,
			AlignmentType = (AlignmentType)3,
			TextFlags = (TextFlags)1032,
			String = ReadOnlySeString.op_Implicit(Speaker)
		};
	}

	private Vector4 TextColorForBg()
	{
		TalkBackground bgChoice = BgChoice;
		if (((uint)(bgChoice - 2) <= 1u || bgChoice == TalkBackground.Narration) ? true : false)
		{
			return ColorHelpers.Vector(KnownColor.White);
		}
		return ColorHelpers.Vector(KnownColor.Black);
	}

	private Vector2 CoordinatesForBg()
	{
		return BgChoice switch
		{
			TalkBackground.Basic => new Vector2(0f, 0f), 
			TalkBackground.Thought => new Vector2(0f, 144f), 
			TalkBackground.Echo => new Vector2(0f, 288f), 
			TalkBackground.Computer => new Vector2(0f, 0f), 
			TalkBackground.Yell => new Vector2(0f, 144f), 
			TalkBackground.Parchment => new Vector2(0f, 288f), 
			TalkBackground.Dragonspeak => new Vector2(0f, 432f), 
			TalkBackground.Linkpearl => new Vector2(0f, 576f), 
			TalkBackground.Narration => new Vector2(0f, 720f), 
			_ => default(Vector2), 
		};
	}

	private Vector2 CoordinatesForCursor()
	{
		return CursorChoice switch
		{
			TalkCursor.Pin => new Vector2(288f, 0f), 
			TalkCursor.Loop => new Vector2(306f, 0f), 
			_ => default(Vector2), 
		};
	}
}
