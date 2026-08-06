using System.IO;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;
using GLib.Popups.ImFileDialog;
using GLib.Popups.ImFileDialog.Data;
using Ktisis.Core.Attributes;

namespace Ktisis.Services.Meta;

[Singleton]
public class ImageDataProvider
{
	private readonly ITextureProvider _tex;

	private readonly FileMetaHandler _handler;

	public ImageDataProvider(ITextureProvider tex)
	{
		_tex = tex;
		_handler = new FileMetaHandler(tex);
	}

	public void Initialize()
	{
		_handler.AddFileType("*", BuildMeta);
	}

	public void BindMetadata(FileDialog dialog)
	{
		dialog.WithMetadata(_handler);
	}

	public ISharedImmediateTexture GetFromFile(string path)
	{
		return _tex.GetFromFile(path);
	}

	private FileMeta BuildMeta(string path)
	{
		ISharedImmediateTexture fromFile = GetFromFile(path);
		return new FileMeta(Path.GetFileName(path))
		{
			Texture = fromFile
		};
	}
}
