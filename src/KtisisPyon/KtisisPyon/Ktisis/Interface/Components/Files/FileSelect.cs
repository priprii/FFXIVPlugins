using System;
using System.IO;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using GLib.Widgets;
using Ktisis.Core.Attributes;

namespace Ktisis.Interface.Components.Files;

[Transient]
public class FileSelect<T> where T : notnull
{
	public delegate void OpenDialogHandler(FileSelect<T> sender);

	public class FileSelectState
	{
		public string Name;

		public string Path;

		public T File;
	}

	public OpenDialogHandler? OnOpenDialog;

	public bool IsFileOpened => Selected != null;

	public FileSelectState? Selected { get; private set; }

	public void SetFile(string path, T file)
	{
		Selected = new FileSelectState
		{
			Name = Path.GetFileName(path),
			Path = path,
			File = file
		};
	}

	public void Clear()
	{
		Selected = null;
	}

	public void Draw()
	{
		//IL_002e: Unknown result type (might be due to invalid IL or missing references)
		string text = Ktisis.Locale.Translate("file.select");
		string text2 = Selected?.Name ?? text;
		ImGui.InputText(ImU8String.op_Implicit("##FileSelectPath"), ref text2, 256, (ImGuiInputTextFlags)16384, (ImGuiInputTextCallbackDelegate)null);
		ImGui.SameLine();
		if (Buttons.IconButton((FontAwesomeIcon)62831))
		{
			OnOpenDialog?.Invoke(this);
		}
		DisabledDisposable val = ImRaii.Disabled(!IsFileOpened);
		try
		{
			ImGui.SameLine();
			if (Buttons.IconButton((FontAwesomeIcon)62186))
			{
				Selected = null;
			}
		}
		finally
		{
			((IDisposable)val)?.Dispose();
		}
	}
}
