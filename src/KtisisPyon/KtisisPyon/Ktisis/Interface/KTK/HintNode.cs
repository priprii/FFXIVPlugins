using System;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Component.GUI;
using KamiToolKit;
using KamiToolKit.Classes;
using KamiToolKit.Enums;
using KamiToolKit.Nodes;
using KamiToolKit.Overlay.UiOverlay;
using KamiToolKit.Premade.Node.Simple;
using KamiToolKit.Timelines;
using Lumina.Text.ReadOnly;

namespace Ktisis.Interface.KTK;

public class HintNode : OverlayNode
{
	private IconImageNode SpeakerImage;

	private SimpleNineGridNode BTextBg;

	private SimpleNineGridNode SpeakerBg;

	private TextNode SpeakerText;

	private TextNode BText;

	private ImageNode Countdown;

	public override OverlayLayer OverlayLayer => OverlayLayer.Foreground;

	public override bool HideWithNativeUi => false;

	public override bool HideWithUiToggled => false;

	protected override void OnUpdate()
	{
	}

	public HintNode(uint iconId, string hint, int hintNum, int? countdownFrames)
	{
		//IL_01cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_0253: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ee: Unknown result type (might be due to invalid IL or missing references)
		SpeakerImage = new IconImageNode
		{
			IconId = iconId,
			TextureSize = new Vector2(640f, 512f),
			Size = new Vector2(320f, 256f),
			Position = new Vector2(-99f, -155f),
			FitTexture = true
		};
		BTextBg = new SimpleNineGridNode
		{
			TexturePath = "ui/uld/BattleTalk_hr1.tex",
			TextureSize = new Vector2(128f, 48f),
			TopOffset = 20f,
			BottomOffset = 26f,
			LeftOffset = 48f,
			RightOffset = 48f,
			Size = new Vector2(625f, 64f),
			Position = new Vector2(0f, 12f)
		};
		SpeakerBg = new SimpleNineGridNode
		{
			TexturePath = "ui/uld/BattleTalkNameBase_hr1.tex",
			TextureSize = new Vector2(188f, 18f),
			LeftOffset = 24f,
			Size = new Vector2(192f, 18f),
			Position = new Vector2(0f, 4f)
		};
		SpeakerText = new TextNode
		{
			Size = new Vector2(167f, 25f),
			Position = new Vector2(5f, 0f),
			TextColor = ColorHelpers.Vector(KnownColor.White),
			TextOutlineColor = ColorHelpers.Vector(KnownColor.Black),
			FontType = (FontType)0,
			FontSize = 14u,
			AlignmentType = (AlignmentType)3,
			TextFlags = (TextFlags)8,
			String = ReadOnlySeString.op_Implicit($"Ktisis Tip #{hintNum}")
		};
		BText = new TextNode
		{
			Size = new Vector2(576f, 44f),
			Position = new Vector2(22f, 22f),
			TextColor = ColorHelpers.Vector(KnownColor.Black),
			TextOutlineColor = ColorHelpers.Vector(KnownColor.Black),
			FontType = (FontType)0,
			FontSize = 16u,
			AlignmentType = (AlignmentType)3,
			TextFlags = (TextFlags)448,
			LineSpacing = 18u,
			String = ReadOnlySeString.op_Implicit(hint)
		};
		SpeakerImage.AttachNode((NodeBase?)this, NodePosition.AsLastChild);
		BTextBg.AttachNode((NodeBase?)this, NodePosition.AsLastChild);
		SpeakerBg.AttachNode((NodeBase?)this, NodePosition.AsLastChild);
		SpeakerText.AttachNode((NodeBase?)this, NodePosition.AsLastChild);
		BText.AttachNode((NodeBase?)this, NodePosition.AsLastChild);
		if (countdownFrames.HasValue)
		{
			SetCountdown(countdownFrames.Value);
			Countdown?.AttachNode((NodeBase?)this, NodePosition.AsLastChild);
			base.Timeline?.PlayAnimation(101);
		}
		CollisionNode collisionNode = CollisionNode;
		collisionNode.NodeFlags = (NodeFlags)(collisionNode.NodeFlags | 0x10);
		CollisionNode.AddEvent((AtkEventType)3, (Action)base.DetachNode);
		AddEvent((AtkEventType)74, (Action)base.DetachNode);
	}

	private unsafe void SetCountdown(int countdownFrames)
	{
		Countdown = new ImageNode
		{
			Size = new Vector2(20f, 20f),
			Position = new Vector2(592f, 40f),
			WrapMode = WrapMode.Tile,
			NodeFlags = (NodeFlags)8240
		};
		foreach (int item in Enumerable.Range(0, 9))
		{
			foreach (int item2 in Enumerable.Range(0, 10))
			{
				Vector2 textureCoordinates = new Vector2((float)item2 * 20f, (float)item * 20f);
				Countdown.AddPart(new Part
				{
					TexturePath = "ui/uld/BattleTalk_Timer_hr1.tex",
					TextureCoordinates = textureCoordinates,
					Size = new Vector2(20f, 20f),
					Id = (uint)(item2 + item)
				});
			}
		}
		ImageNode countdown = Countdown;
		FrameSetBuilder frameSetBuilder = new TimelineBuilder().BeginFrameSet(11, countdownFrames);
		uint? partId = 0u;
		FrameSetBuilder frameSetBuilder2 = frameSetBuilder.AddFrame(11, null, null, null, null, null, null, null, null, partId);
		uint? partId2 = 89u;
		countdown.AddTimeline(frameSetBuilder2.AddFrame(countdownFrames, null, null, null, null, null, null, null, null, partId2).EndFrameSet().Build());
		AddTimeline(new TimelineBuilder().BeginFrameSet(11, countdownFrames).AddLabel(11, 101, (AtkTimelineJumpBehavior)0, 0).AddLabel(countdownFrames, 0, (AtkTimelineJumpBehavior)1, 0)
			.EndFrameSet()
			.Build());
	}
}
