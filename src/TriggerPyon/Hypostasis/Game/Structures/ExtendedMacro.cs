using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Hypostasis.Game.Structures;

[StructLayout(LayoutKind.Sequential, Size = 3232)]
public readonly struct ExtendedMacro : IDisposable
{
	public const int numLines = 30;

	public const int size = 3232;

	public readonly uint icon;

	public readonly uint key;

	public readonly UTF8String title;

	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 30)]
	public readonly UTF8String[] lines;

	public ExtendedMacro(nint loc, string t, IReadOnlyList<string> commands)
	{
		icon = 66001u;
		key = 1u;
		title = new UTF8String(loc + 8, t);
		lines = new UTF8String[30];
		for (int i = 0; i < 30; i++)
		{
			string text = ((commands.Count > i) ? commands[i] : string.Empty);
			lines[i] = new UTF8String(loc + 8 + 104 * (i + 1), text);
		}
	}

	public void Dispose()
	{
		title.Dispose();
		for (int i = 0; i < 30; i++)
		{
			lines[i].Dispose();
		}
	}
}
