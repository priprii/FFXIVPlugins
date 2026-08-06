using System.Collections.Generic;
using System.IO;
using Ktisis.Data.Config;
using Ktisis.Scene.Decor;
using Ktisis.Scene.Types;

namespace Ktisis.Scene.Entities.Utility;

public class ReferenceImage : SceneEntity, IVisibility, IDeletable
{
	public record SetupData
	{
		public string Id = string.Empty;

		public string FilePath = string.Empty;

		public float Opacity = 1f;

		public bool Visible = true;
	}

	public readonly SetupData Data;

	private Configuration Config => Scene.Context.Config;

	public bool Visible
	{
		get
		{
			return Data.Visible;
		}
		set
		{
			Data.Visible = value;
		}
	}

	public ReferenceImage(ISceneManager scene, SetupData data)
		: base(scene)
	{
		Data = data;
		base.Type = EntityType.RefImage;
	}

	public void Save()
	{
		List<SetupData> referenceImages = Config.Editor.ReferenceImages;
		Data.Id = $"{referenceImages.Count}-{Data.GetHashCode():X}";
		referenceImages.Add(Data);
	}

	public bool Delete()
	{
		Config.Editor.ReferenceImages.Remove(Data);
		Remove();
		return true;
	}

	public void SetFilePath(string newPath)
	{
		string filePath = Data.FilePath;
		Data.FilePath = newPath;
		if (Name == Path.GetFileName(filePath))
		{
			Name = Path.GetFileName(newPath);
		}
	}
}
