using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Hypostasis.Game.Structures;

[StructLayout(LayoutKind.Sequential, Size = 1672)]
public readonly struct Macro : IDisposable
{
	public const int numLines = 15;

	public const int size = 1672;

	public readonly uint icon;

	public readonly uint key;

	public readonly UTF8String title;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 15)]
	public readonly UTF8String[] lines;

	public Macro(nint loc, string t, IReadOnlyList<string> commands)
	{
		icon = 66001u;
		key = 1u;
		title = new UTF8String(loc + 8, t);
		lines = new UTF8String[15];
		for (int i = 0; i < 15; i++)
		{
			string text = ((commands.Count > i) ? commands[i] : string.Empty);
			lines[i] = new UTF8String(loc + 8 + 104 * (i + 1), text);
		}
	}

	public void Dispose()
	{
		title.Dispose();
		for (int i = 0; i < 15; i++)
		{
			lines[i].Dispose();
		}
	}
}
