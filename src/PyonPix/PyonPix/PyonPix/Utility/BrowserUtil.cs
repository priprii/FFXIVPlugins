using System;
using System.Collections.Specialized;
using System.Linq;
using System.Web;
using Dalamud.Bindings.ImGui;
using PyonPix.Structs.Browser;

namespace PyonPix.Utility;

public static class BrowserUtil
{
	private static readonly string[] InternalSchemes = new string[7] { "pix://", "file:///", "about:", "edge://", "extension://", "chrome://", "chrome-extension://" };

	public static string NormalizeUri(string? uri)
	{
		if (string.IsNullOrWhiteSpace(uri) || uri == "about:blank")
		{
			return "pix://";
		}
		uri = uri.Trim();
		if (InternalSchemes.Any((string x) => uri.StartsWith(x, StringComparison.CurrentCultureIgnoreCase)))
		{
			return uri;
		}
		if (Uri.TryCreate(uri, UriKind.Absolute, out Uri result) && IsNavigableHost(result))
		{
			return result.ToString();
		}
		if (Uri.TryCreate("https://" + uri, UriKind.Absolute, out result) && IsNavigableHost(result))
		{
			return result.ToString();
		}
		return "https://google.com/search?q=" + Uri.EscapeDataString(uri);
	}

	public static string NormalizeUriForSync(string uri)
	{
		if (string.IsNullOrWhiteSpace(uri))
		{
			return uri;
		}
		if (!Uri.TryCreate(uri, UriKind.Absolute, out Uri result))
		{
			return uri;
		}
		if (!result.Host.Contains("youtube.com") && !result.Host.Contains("youtu.be"))
		{
			return uri;
		}
		NameValueCollection nameValueCollection = HttpUtility.ParseQueryString(result.Query);
		nameValueCollection.Remove("index");
		return new UriBuilder(result)
		{
			Port = -1,
			Query = (nameValueCollection.ToString() ?? string.Empty)
		}.ToString();
	}

	private static bool IsNavigableHost(Uri uri)
	{
		return uri.HostNameType switch
		{
			UriHostNameType.Dns => uri.Host.Contains('.') && !uri.Host.EndsWith('.'), 
			UriHostNameType.IPv4 => true, 
			UriHostNameType.IPv6 => true, 
			_ => false, 
		};
	}

	public static bool IsFileScheme(string? uri)
	{
		return uri?.StartsWith("file:///", StringComparison.CurrentCultureIgnoreCase) ?? false;
	}

	public static string FormatUriForDisplay(string uri)
	{
		if (string.IsNullOrEmpty(uri))
		{
			return uri;
		}
		if (!Uri.TryCreate(uri, UriKind.Absolute, out Uri result))
		{
			return uri;
		}
		string host = result.Host;
		string absolutePath = result.AbsolutePath;
		if (!string.IsNullOrWhiteSpace(absolutePath) && !(absolutePath == "/"))
		{
			return host + absolutePath;
		}
		if (!string.IsNullOrWhiteSpace(host))
		{
			return host;
		}
		return uri;
	}

	public static ImGuiMouseCursor TranslateCursor(uint id)
	{
		//IL_006e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		//IL_006a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		return (ImGuiMouseCursor)(id switch
		{
			32512u => 0, 
			32513u => 1, 
			32649u => 7, 
			32644u => 4, 
			32645u => 3, 
			32642u => 6, 
			32643u => 5, 
			32646u => 2, 
			32514u => 8, 
			0u => 2, 
			_ => 0, 
		});
	}

	public static MouseButton GetMouseButtonsState(Span<bool> state)
	{
		MouseButton mouseButton = MouseButton.None;
		if (state[0])
		{
			mouseButton |= MouseButton.Left;
		}
		if (state[1])
		{
			mouseButton |= MouseButton.Right;
		}
		if (state[2])
		{
			mouseButton |= MouseButton.Middle;
		}
		return mouseButton;
	}
}
