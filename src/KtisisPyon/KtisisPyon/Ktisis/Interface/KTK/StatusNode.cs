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
using Ktisis.Common.Utility;
using Lumina.Text.ReadOnly;

namespace Ktisis.Interface.KTK;

public class StatusNode : OverlayNode
{
	private SimpleImageNode StatusIcon;

	private TextNode StatusText;

	public StatusType Type;

	public string Text;

	public string IconPath;

	public override OverlayLayer OverlayLayer => OverlayLayer.BehindUserInterface;

	public override bool HideWithNativeUi => false;

	public override bool HideWithUiToggled => false;

	public StatusNode(StatusType type, string text, string iconPath)
	{
		Type = type;
		Text = text;
		IconPath = iconPath;
		StatusIcon = SetStatusIcon();
		StatusText = SetStatusText();
		StatusIcon.AttachNode((NodeBase?)this, NodePosition.AsLastChild);
		StatusText.AttachNode((NodeBase?)this, NodePosition.AsLastChild);
	}

	protected override void OnUpdate()
	{
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		StatusText.String = ReadOnlySeString.op_Implicit(GetTextForType());
		StatusText.TextColor = GetTextColorForType(Type);
		StatusText.TextOutlineColor = GetEdgeColorForType(Type);
		StatusIcon.TexturePath = IconPath;
	}

	private SimpleImageNode SetStatusIcon()
	{
		return new SimpleImageNode
		{
			TexturePath = IconPath,
			TextureSize = new Vector2(24f, 32f),
			TextureCoordinates = Vector2.Zero,
			Position = Vector2.Zero,
			Size = new Vector2(24f, 32f)
		};
	}

	private TextNode SetStatusText()
	{
		//IL_0081: Unknown result type (might be due to invalid IL or missing references)
		return new TextNode
		{
			Size = new Vector2(660f, 28f),
			Position = new Vector2(27f, 2f),
			TextColor = GetTextColorForType(Type),
			TextOutlineColor = GetEdgeColorForType(Type),
			FontType = (FontType)0,
			TextFlags = (TextFlags)1032,
			AlignmentType = (AlignmentType)3,
			FontSize = 18u,
			LineSpacing = 16u,
			String = ReadOnlySeString.op_Implicit(GetTextForType())
		};
	}

	private string GetTextForType()
	{
		return Type switch
		{
			StatusType.Buff => "+ " + Text, 
			StatusType.Debuff => "+ " + Text, 
			StatusType.Falloff => "- " + Text, 
			_ => Text, 
		};
	}

	private static Vector4 GetTextColorForType(StatusType type)
	{
		if (type == StatusType.Falloff)
		{
			return GuiHelpers.VectorColorFromString("#CCCCCCFF");
		}
		return ColorHelpers.Vector(KnownColor.White);
	}

	private static Vector4 GetEdgeColorForType(StatusType type)
	{
		return type switch
		{
			StatusType.Buff => GuiHelpers.VectorColorFromString("#2A5D00FF"), 
			StatusType.Debuff => GuiHelpers.VectorColorFromString("#8A0000FF"), 
			StatusType.Falloff => GuiHelpers.VectorColorFromString("#454545FF"), 
			_ => ColorHelpers.Vector(KnownColor.Black), 
		};
	}
}
