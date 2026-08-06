using System;
using System.IO;

namespace Ktisis.Data.Config.Sections;

public class AutoSaveConfig
{
	public bool Enabled;

	public int Interval = 60;

	public int Count = 5;

	public string FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Ktisis", "PoseAutoBackup");

	public string FolderFormat = "AutoSave - %Date% %Time%";

	public bool ClearOnExit;

	public bool OnDisconnect = true;

	public bool OnDisable;
}
