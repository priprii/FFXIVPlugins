using System;
using System.Collections.Generic;
using Dalamud.Game.ClientState.Objects.Types;
using Ktisis.Editor.Camera.Types;

namespace Ktisis.Editor.Camera;

public interface ICameraManager : IDisposable
{
	bool IsValid { get; }

	EditorCamera? Current { get; }

	bool IsWorkCameraActive { get; }

	void Initialize();

	IEnumerable<EditorCamera> GetCameras();

	void SetCurrent(EditorCamera camera);

	void SetNext();

	void SetPrevious();

	void SetWorkCameraMode(bool enabled);

	void ToggleWorkCameraMode();

	KtisisCamera Create(CameraFlags flags = CameraFlags.None, bool setActive = true);

	bool DeleteCurrent();

	IGameObject? ResolveOrbitTarget(EditorCamera camera);
}
