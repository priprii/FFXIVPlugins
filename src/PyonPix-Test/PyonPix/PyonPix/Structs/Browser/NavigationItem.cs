using PyonPix.Utility;

namespace PyonPix.Structs.Browser;

public class NavigationItem(string uri)
{
	public string Uri = uri;

	public string Title = string.Empty;

	public string GetDisplayTitle()
	{
		if (!string.IsNullOrWhiteSpace(Title))
		{
			return Title;
		}
		return BrowserUtil.FormatUriForDisplay(Uri);
	}
}
