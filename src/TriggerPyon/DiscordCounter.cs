using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.Text.SeStringHandling;
using Lumina.Text;
using Newtonsoft.Json;

namespace TriggerPyon;

public class DiscordCounter : CounterBase
{
	public override CounterType ObjType => CounterType.Discord;

	public List<string> TitleTemplates { get; set; } = new List<string>();

	public bool TitlePrefix { get; set; } = true;

	public Vector3 TitleColour { get; set; } = new Vector3(1f, 1f, 1f);

	public Vector3? TitleGlow { get; set; } = new Vector3(1f, 1f, 1f);

	public int? TitleGradientColorSet { get; set; }

	public GradientAnimationStyle? TitleGradientAnimationStyle { get; set; }

	public DiscordActivityType ActivityType { get; set; } = DiscordActivityType.Listening;

	public bool Interruptable { get; set; } = true;

	public int Duration { get; set; } = 5000;

	public int Frequency { get; set; } = 30000;

	[JsonIgnore]
	public int EditingIndex { get; set; } = -1;

	public SeString ToSeString(string TitleTemplate, bool includeQuotes = true, bool includeColor = true, bool animate = true)
	{
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0040: Expected O, but got Unknown
		if (string.IsNullOrEmpty(TitleTemplate))
		{
			return SeString.Empty;
		}
		SeStringBuilder builder = new SeStringBuilder();
		if (includeQuotes)
		{
			builder.Append("《");
		}
		if (includeColor)
		{
			builder.PushColorRgba(new Vector4(TitleColour, 1f));
		}
		AppendTitle();
		if (includeColor)
		{
			builder.PopColor();
		}
		if (includeQuotes)
		{
			builder.Append("》");
		}
		return SeString.Parse(builder.GetViewAsSpan());
		void AppendTitle()
		{
			if (!includeColor)
			{
				builder.Append(TitleTemplate);
			}
			else
			{
				if (TitleGradientColorSet.HasValue)
				{
					GradientStyle style = GradientSystem.GetStyle(TitleGradientColorSet.Value, TitleGradientAnimationStyle);
					if (style != null)
					{
						style.Apply(builder, TitleTemplate, animate);
						return;
					}
				}
				if (TitleGlow.HasValue)
				{
					builder.PushEdgeColorRgba(new Vector4(TitleGlow.Value, 1f));
					builder.Append(TitleTemplate);
					builder.PopEdgeColor();
				}
				else
				{
					builder.Append(TitleTemplate);
				}
			}
		}
	}
}
