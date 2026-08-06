using System.Collections.Generic;
using Dalamud.Game.Text.SeStringHandling;

namespace PvPyon.Api.Tools.Strings;

public class StringChange
{
	public List<Payload> Payloads { get; init; } = new List<Payload>();

	public bool ForceUsingSingleAnchorPayload { get; set; }
}
