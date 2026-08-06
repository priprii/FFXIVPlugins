using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Textures.TextureWraps;
using Dalamud.Utility;
using SharpDX.Direct3D11;

namespace PyonPix.Structs.Browser;

public class Tab : IDisposable
{
	public string PixId = string.Empty;

	public bool GpuAcceleration = true;

	public bool SyncCookies = true;

	public TabState State;

	public NavigationState NavState;

	public string? PresentationUri;

	public string? PendingUri;

	public List<NavigationItem> History = new List<NavigationItem>();

	public int CurrentNavigationIndex = -1;

	public IDalamudTextureWrap? FavIcon;

	public nint SharedHandle = IntPtr.Zero;

	public ShaderResourceView? SRV;

	public uint Width;

	public uint Height;

	public Vector2 RenderPos;

	public Vector2 RenderSize;

	public NavigationItem? CurrentNavigationItem
	{
		get
		{
			if (CurrentNavigationIndex <= -1 || CurrentNavigationIndex >= History.Count)
			{
				return null;
			}
			return History[CurrentNavigationIndex];
		}
	}

	public bool CanNavigate => NavState == NavigationState.Ready;

	public bool CanGoBack
	{
		get
		{
			if (State == TabState.Ready && CanNavigate && CurrentNavigationIndex > 0)
			{
				return History.Count > 0;
			}
			return false;
		}
	}

	public bool CanGoForward
	{
		get
		{
			if (State == TabState.Ready && CanNavigate && CurrentNavigationIndex != -1)
			{
				return CurrentNavigationIndex < History.Count - 1;
			}
			return false;
		}
	}

	public bool CanReload
	{
		get
		{
			if (State == TabState.Ready && CanNavigate)
			{
				return CurrentNavigationItem != null;
			}
			return false;
		}
	}

	public bool CanCancel
	{
		get
		{
			if (State == TabState.Ready && !CanNavigate)
			{
				return CurrentNavigationItem != null;
			}
			return false;
		}
	}

	public string GetTitle()
	{
		if (string.IsNullOrWhiteSpace(CurrentNavigationItem?.Title))
		{
			if (string.IsNullOrWhiteSpace(CurrentNavigationItem?.Uri))
			{
				return PixId;
			}
			string host = new Uri(CurrentNavigationItem.Uri).Host;
			if (!host.Contains('.'))
			{
				return StringExtensions.FirstCharToUpper(host, (CultureInfo)null);
			}
			string[] array = host.Split('.');
			if (array.Length <= 2)
			{
				return StringExtensions.FirstCharToUpper(array[1], (CultureInfo)null);
			}
			if (array[^1].All(char.IsNumber))
			{
				return host;
			}
			if (array[^1].Length == 2)
			{
				_ = array[^2].Length;
				_ = 2;
				return StringExtensions.FirstCharToUpper(array[1], (CultureInfo)null);
			}
			return StringExtensions.FirstCharToUpper(array[1], (CultureInfo)null);
		}
		return CurrentNavigationItem.Title;
	}

	public string GetHomeUri(HomeUriType type, string homeUri)
	{
		switch (type)
		{
		case HomeUriType.Blank:
			return "pix://";
		case HomeUriType.Starry:
			return "pix://starry";
		default:
			if (!string.IsNullOrWhiteSpace(homeUri))
			{
				return homeUri;
			}
			return "pix://";
		}
	}

	public void Dispose()
	{
		((IDisposable)FavIcon)?.Dispose();
		SRV?.Dispose();
		SharedHandle = IntPtr.Zero;
		GC.SuppressFinalize(this);
	}
}
