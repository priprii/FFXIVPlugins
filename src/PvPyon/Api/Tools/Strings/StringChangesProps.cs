using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;

namespace PvPyon.Api.Tools.Strings;

public class StringChangesProps
{
	public SeString Destination { get; set; }

	public StringChanges StringChanges { get; set; } = new StringChanges();

	public List<Payload> AnchorPayloads { get; set; } = new List<Payload>();

	public Payload AnchorPayload { get; set; }
}
