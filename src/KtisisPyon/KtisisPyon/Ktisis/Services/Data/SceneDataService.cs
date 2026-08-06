using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Common.Math;
using Ktisis.Common.Extensions;
using Ktisis.Common.Utility;
using Ktisis.Core.Attributes;
using Ktisis.Data.Files;
using Ktisis.Data.Json;
using Ktisis.Editor.Camera.Types;
using Ktisis.Editor.Context.Types;
using Ktisis.Editor.Posing.Data;
using Ktisis.Editor.Posing.Types;
using Ktisis.Interop.Ipc;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Character;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Utility;
using Ktisis.Scene.Entities.World;
using Ktisis.Scene.Factory.Builders;
using Ktisis.Scene.Factory.Creators;
using Ktisis.Scene.Modules;
using Ktisis.Scene.Modules.Actors;
using Ktisis.Scene.Types;
using Ktisis.Structs.Env;

namespace Ktisis.Services.Data;

[Singleton]
public class SceneDataService
{
	private IEditorContext? _ctx;

	private IObjectTable _objectTable;

	private IFramework _framework;

	private IDataManager _data;

	private Task? _task;

	private Dictionary<ushort, ActorEntity> _idMap;

	private ISceneManager Scene => _ctx.Scene;

	private IPosingManager Posing => _ctx.Posing;

	public SceneDataService(IEditorContext ctx, IObjectTable objectTable, IFramework framework)
	{
		_ctx = ctx;
		_objectTable = objectTable;
		_framework = framework;
		_idMap = new Dictionary<ushort, ActorEntity>();
	}

	public void WriteFile(string path)
	{
		try
		{
			SceneFile obj = Save();
			JsonFileSerializer jsonFileSerializer = new JsonFileSerializer();
			jsonFileSerializer.GetConverter<Vector3>();
			jsonFileSerializer.GetConverter<Transform>();
			File.WriteAllText(path, jsonFileSerializer.Serialize(obj));
		}
		catch
		{
			Ktisis.Log.Warning("Failed to write Scene file");
		}
	}

	public unsafe SceneFile Save(bool saveActors = true, bool saveLights = true, bool saveCameras = true, bool saveEnv = true, bool saveOverlays = true)
	{
		//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0106: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0313: Unknown result type (might be due to invalid IL or missing references)
		//IL_031b: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_032e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0338: Unknown result type (might be due to invalid IL or missing references)
		//IL_033a: Unknown result type (might be due to invalid IL or missing references)
		SceneFile sceneFile = new SceneFile();
		sceneFile.SceneOrigin = _ctx.Scene.GetSceneOrigin();
		sceneFile.MapID = GetCurrentMapID();
		List<CharaEntity> list = Scene.Children.Where((SceneEntity entity) => entity is CharaEntity).Cast<CharaEntity>().ToList();
		List<LightEntity> list2 = Scene.Children.Where((SceneEntity entity) => entity is LightEntity).Cast<LightEntity>().ToList();
		List<OverlayEntity> list3 = Scene.Children.OfType<OverlayEntity>().ToList();
		if (saveActors)
		{
			foreach (CharaEntity item4 in list)
			{
				DrawObject* drawObject = ((ActorEntity)item4).Actor.GetDrawObject();
				Transform location = new Transform(Scene.GetActorRelativePosition(Vector3.op_Implicit(((DrawObject)drawObject).Position)), Quaternion.op_Implicit(((DrawObject)drawObject).Rotation), Vector3.op_Implicit(((DrawObject)drawObject).Scale));
				CharaFile resultSafely = TaskExtensions.GetResultSafely<CharaFile>(_ctx.Characters.SaveCharaFile((ActorEntity)item4));
				PoseFile pose = new EntityPoseConverter(item4.Pose).SaveFile();
				float rotation = ((GameObject)((ActorEntity)item4).CsGameObject).Rotation;
				IpcManager ipc = _ctx.Plugin.Ipc;
				Guid penumbraCollection = Guid.Empty;
				Guid customizePlus = Guid.Empty;
				if (ipc.IsPenumbraActive && ((ActorEntity)item4).MCDF == null)
				{
					(Guid, string) collectionForObject = ipc.GetPenumbraIpc().GetCollectionForObject(((ActorEntity)item4).Actor);
					if (collectionForObject.Item1 != Guid.Empty)
					{
						(penumbraCollection, _) = collectionForObject;
					}
				}
				if (ipc.IsCustomizeActive && ((ActorEntity)item4).MCDF == null && ((ActorEntity)item4).AssignedProfile != Guid.Empty)
				{
					Guid? assignedProfile = ((ActorEntity)item4).AssignedProfile;
					if (assignedProfile.HasValue)
					{
						customizePlus = assignedProfile.Value;
					}
				}
				sceneFile.Actors.Add(new SceneFile.ActorInfo
				{
					Chara = resultSafely,
					Pose = pose,
					Location = location,
					MCDF = _ctx.Characters.Mcdf.LoadedMCDFPath(((ActorEntity)item4).Actor),
					DefaultRotation = rotation,
					Index = ((ActorEntity)item4).Actor.ObjectIndex,
					PenumbraCollection = penumbraCollection,
					CustomizePlus = customizePlus
				});
			}
		}
		if (saveLights)
		{
			foreach (LightEntity item5 in list2)
			{
				LightFile result = Scene.SaveLightFile(item5).Result;
				Transform transform = item5.GetObject()->Transform;
				Transform location2 = new Transform(Scene.GetActorRelativePosition(Vector3.op_Implicit(transform.Position)), Quaternion.op_Implicit(transform.Rotation), Vector3.op_Implicit(transform.Scale));
				SceneFile.LightInfo item = new SceneFile.LightInfo
				{
					Light = result,
					Location = location2,
					Name = item5.Name,
					State = !item5.IsHidden
				};
				sceneFile.Lights.Add(item);
			}
		}
		if (saveCameras)
		{
			foreach (EditorCamera camera in _ctx.Cameras.GetCameras())
			{
				SceneFile.CameraInfo item2 = new SceneFile.CameraInfo
				{
					OrbitTarget = (camera.OrbitTarget ?? ((GameObject)((Camera)camera.GameCamera).GetCameraTargetObject()).ObjectIndex),
					IsDelmited = camera.IsDelimited,
					Angle = new Vector3(camera.Camera->Angle.X, camera.Camera->Angle.Y, camera.Camera->Distance),
					FixedPosition = Scene.GetActorRelativePosition(camera.GetPosition().Value),
					Flags = (uint)camera.Flags,
					IsActive = (_ctx.Cameras.Current == camera),
					Name = camera.Name,
					OrthographicZoom = camera.OrthographicZoom
				};
				sceneFile.Cameras.Add(item2);
			}
		}
		if (saveOverlays)
		{
			foreach (OverlayEntity item6 in list3)
			{
				SceneFile.OverlayInfo.Type overlayType = SceneFile.OverlayInfo.Type.None;
				string dialog = string.Empty;
				if (item6.Type == EntityType.BalloonOverlay)
				{
					overlayType = SceneFile.OverlayInfo.Type.Balloon;
					dialog = ((BalloonOverlay)item6).Dialog;
				}
				else if (item6.Type == EntityType.StatusOverlay)
				{
					overlayType = SceneFile.OverlayInfo.Type.Status;
					dialog = ((StatusOverlay)item6).StatusText;
				}
				else if (item6.Type == EntityType.TalkOverlay)
				{
					overlayType = SceneFile.OverlayInfo.Type.Talk;
					dialog = ((TalkOverlay)item6).Dialog;
				}
				SceneFile.OverlayInfo item3 = new SceneFile.OverlayInfo
				{
					OverlayType = overlayType,
					Dialog = dialog,
					Position = item6.Position,
					Opacity = item6.Alpha,
					Scale = item6.Scale,
					Visible = item6.Visible,
					Name = item6.Name
				};
				switch (item6.Type)
				{
				case EntityType.BalloonOverlay:
					item3.ArrowPosition = ((BalloonOverlay)item6).ArrowX;
					item3.ShowArrow = ((BalloonOverlay)item6).Arrow;
					item3.BalloonBackground = ((BalloonOverlay)item6).Background;
					item3.BalloonColor = ((BalloonOverlay)item6).Color;
					item3.FontSize = ((BalloonOverlay)item6).FontSize;
					break;
				case EntityType.TalkOverlay:
					item3.TalkBackground = ((TalkOverlay)item6).Background;
					item3.TalkCursor = ((TalkOverlay)item6).Cursor;
					item3.Speaker = ((TalkOverlay)item6).Speaker;
					item3.FontSize = ((TalkOverlay)item6).FontSize;
					break;
				case EntityType.StatusOverlay:
					item3.StatusIcon = ((StatusOverlay)item6).IconPath;
					item3.StatusType = ((StatusOverlay)item6).StatusType;
					break;
				}
				sceneFile.Overlays.Add(item3);
			}
		}
		if (saveEnv)
		{
			EnvModule module = _ctx.Scene.GetModule<EnvModule>();
			uint num = (uint)module.Override;
			sceneFile.Environment = new SceneFile.EnvironmentInfo
			{
				Override = num,
				State = Marshal.PtrToStructure<EnvManagerEx>((nint)EnvManagerEx.Instance()).EnvState,
				Day = module.Day,
				Time = module.Time,
				Weather = module.Weather
			};
		}
		return sceneFile;
	}

	public SceneFile LoadFile(string path)
	{
		string json = File.ReadAllText(path);
		return new JsonFileSerializer().Deserialize<SceneFile>(json);
	}

	public unsafe async Task Load(SceneFile scene, bool autoSaveLoading = true, bool loadActors = true, bool loadLights = true, bool loadCameras = true, bool loadEnv = true, bool loadOverlays = true, bool preserveExistingActors = false)
	{
		_idMap = new Dictionary<ushort, ActorEntity>();
		if (loadActors && !preserveExistingActors)
		{
			foreach (ActorEntity item in Scene.Children.Where((SceneEntity entity) => entity is CharaEntity).ToList())
			{
				Scene.GetModule<ActorModule>().Delete(item, force: true);
				Scene.Remove(item);
			}
		}
		if (loadOverlays)
		{
			foreach (SceneEntity item2 in Scene.Children.Where((SceneEntity entity) => entity is OverlayEntity).ToList())
			{
				item2.Remove();
			}
		}
		Vector3 sceneOrigin = (autoSaveLoading ? scene.SceneOrigin : ((IGameObject)_objectTable.LocalPlayer).Position);
		if (loadActors)
		{
			foreach (SceneFile.ActorInfo loaded in scene.Actors.Where((SceneFile.ActorInfo info) => info.Chara.ModelType == 0))
			{
				loaded.Location.Position += sceneOrigin;
				await _framework.RunOnFrameworkThread((Action)delegate
				{
					SetupActor(loaded);
				});
				await _framework.DelayTicks(10L, default(CancellationToken));
			}
			await _framework.DelayTicks(30L, default(CancellationToken));
			foreach (SceneFile.ActorInfo loaded2 in scene.Actors.Where((SceneFile.ActorInfo info) => info.Chara.ModelType != 0))
			{
				loaded2.Location.Position += sceneOrigin;
				await _framework.RunOnFrameworkThread((Action)delegate
				{
					SetupActor(loaded2);
				});
				await _framework.DelayTicks(10L, default(CancellationToken));
				if ((int)loaded2.Chara.ObjectKind == 15)
				{
					ActorEntity actorEntity2 = _idMap[loaded2.Index];
					actorEntity2.Appearance.ModelId = loaded2.Chara.ModelType;
					actorEntity2.Redraw();
				}
			}
		}
		if (loadLights)
		{
			foreach (LightEntity item3 in Scene.Children.Where((SceneEntity entity) => entity is LightEntity).ToList())
			{
				item3.Delete();
			}
			foreach (SceneFile.LightInfo light in scene.Lights)
			{
				LightEntity result = _ctx.Scene.Factory.CreateLight().Spawn().Result;
				_ctx.Scene.ApplyLightFile(result, light.Light);
				light.Location.Position += sceneOrigin;
				result.SetTransform(light.Location);
				if (!light.State)
				{
					result.ToggleHidden();
				}
			}
		}
		if (loadCameras)
		{
			EditorCamera current2 = _ctx.Cameras.GetCameras().First((EditorCamera c) => c.IsDefault);
			_ctx.Cameras.SetCurrent(current2);
			int num = scene.Cameras.Count - _ctx.Cameras.GetCameras().Count();
			if (num > 0)
			{
				for (int num2 = 0; num2 < num; num2++)
				{
					_ctx.Cameras.Create(CameraFlags.None, setActive: false);
				}
			}
			else if (num < 0)
			{
				num *= -1;
				for (int num3 = 0; num3 < num; num3++)
				{
					_ctx.Cameras.DeleteCurrent();
				}
			}
			SceneFile.CameraInfo camera = scene.Cameras.First((SceneFile.CameraInfo c) => c.IsActive);
			EditorCamera currentKtCam = _ctx.Cameras.Current;
			IEnumerable<(SceneFile.CameraInfo First, EditorCamera Second)> enumerable = scene.Cameras.Where((SceneFile.CameraInfo c) => !c.IsActive).Zip(from c in _ctx.Cameras.GetCameras()
				where c != currentKtCam
				select c);
			ApplyCamera(camera, currentKtCam, sceneOrigin);
			foreach (var item4 in enumerable)
			{
				ApplyCamera(item4.First, item4.Second, sceneOrigin);
			}
		}
		if (loadOverlays)
		{
			foreach (SceneFile.OverlayInfo overlayInfo in scene.Overlays)
			{
				_framework.RunOnFrameworkThread((Action)delegate
				{
					OverlayEntity overlayEntity = null;
					switch (overlayInfo.OverlayType)
					{
					case SceneFile.OverlayInfo.Type.Balloon:
						overlayEntity = _ctx.Scene.Factory.BuildOverlay(OverlayTypes.Balloon).SetName(overlayInfo.Name).Add();
						((BalloonOverlay)overlayEntity).Arrow = overlayInfo.ShowArrow;
						((BalloonOverlay)overlayEntity).ArrowX = overlayInfo.ArrowPosition;
						((BalloonOverlay)overlayEntity).Background = overlayInfo.BalloonBackground;
						((BalloonOverlay)overlayEntity).Dialog = overlayInfo.Dialog;
						break;
					case SceneFile.OverlayInfo.Type.Talk:
						overlayEntity = _ctx.Scene.Factory.BuildOverlay(OverlayTypes.Talk).SetName(overlayInfo.Name).Add();
						((TalkOverlay)overlayEntity).Background = overlayInfo.TalkBackground;
						((TalkOverlay)overlayEntity).Cursor = overlayInfo.TalkCursor;
						((TalkOverlay)overlayEntity).Dialog = overlayInfo.Dialog;
						((TalkOverlay)overlayEntity).Speaker = overlayInfo.Speaker;
						break;
					case SceneFile.OverlayInfo.Type.Status:
						overlayEntity = _ctx.Scene.Factory.BuildOverlay(OverlayTypes.Status).SetName(overlayInfo.Name).Add();
						((StatusOverlay)overlayEntity).IconPath = overlayInfo.StatusIcon;
						((StatusOverlay)overlayEntity).StatusText = overlayInfo.Dialog;
						((StatusOverlay)overlayEntity).StatusType = overlayInfo.StatusType;
						break;
					}
					if (overlayEntity != null)
					{
						overlayEntity.Alpha = overlayInfo.Opacity / 255f;
						overlayEntity.Position = overlayInfo.Position;
						overlayEntity.Scale = overlayInfo.Scale;
						overlayEntity.Visible = overlayInfo.Visible;
					}
				});
			}
		}
		if (loadEnv)
		{
			uint num4 = scene.Environment.Override;
			EnvModule module = _ctx.Scene.GetModule<EnvModule>();
			module.Override = (EnvOverride)num4;
			if (num4 != 0)
			{
				Marshal.StructureToPtr(scene.Environment.State, (nint)((byte*)EnvManagerEx.Instance() + 88), fDeleteOld: false);
			}
			if (module.Override.HasFlag(EnvOverride.TimeWeather))
			{
				module.Day = scene.Environment.Day;
				module.Time = scene.Environment.Time;
				module.Weather = scene.Environment.Weather;
			}
		}
	}

	public unsafe void ApplyCamera(SceneFile.CameraInfo camera, EditorCamera ktCam, Vector3 sceneOrigin)
	{
		if (camera.IsDelmited)
		{
			ktCam.FixedPosition = camera.FixedPosition + sceneOrigin;
		}
		else
		{
			ktCam.Camera->Angle.X = camera.Angle.Value.X;
			ktCam.Camera->Angle.Y = camera.Angle.Value.Y;
			ktCam.Camera->Distance = camera.Angle.Value.Z;
		}
		ktCam.OrthographicZoom = camera.OrthographicZoom;
		ktCam.Flags = (CameraFlags)camera.Flags;
		if (camera.OrbitTarget != 0)
		{
			ktCam.OrbitTarget = _idMap[camera.OrbitTarget].Actor.ObjectIndex;
			_idMap[camera.OrbitTarget].Actor.SetGPoseTarget();
		}
	}

	public unsafe uint GetCurrentMapID()
	{
		return ((AgentMap)AgentMap.Instance()).CurrentMapId;
	}

	internal bool ValidMCDFPath(SceneFile.ActorInfo a)
	{
		if (a.MCDF != string.Empty)
		{
			return Path.Exists(a.MCDF);
		}
		return false;
	}

	private void SetupActor(SceneFile.ActorInfo actor)
	{
		IActorCreator actorCreator = _ctx.Scene.Factory.CreateActor();
		if (ValidMCDFPath(actor))
		{
			actorCreator = actorCreator.WithMcdf(actor.MCDF);
		}
		else if (actor.MCDF != string.Empty)
		{
			actorCreator = actorCreator.WithAppearance(actor.Chara);
			Ktisis.WarningNotification("Couldn't find the MCDF linked to the actor " + actor.Chara.Nickname + ", please try and load it manually.");
		}
		else
		{
			actorCreator = actorCreator.WithAppearance(actor.Chara);
		}
		actorCreator.Spawn().ContinueWith((Func<Task<ActorEntity>, Task>)async delegate(Task<ActorEntity> p)
		{
			ActorEntity a = TaskExtensions.GetResultSafely<ActorEntity>(p);
			_idMap.Add(actor.Index, a);
			await _framework.DelayTicks(15L, default(CancellationToken));
			a.Name = actor.Chara.Nickname;
			IGameObject actor2 = a.Actor;
			if (actor.PenumbraCollection != Guid.Empty && _ctx.Plugin.Ipc.IsPenumbraActive && _ctx.Plugin.Ipc.GetPenumbraIpc().GetCollections().ContainsKey(actor.PenumbraCollection))
			{
				_ctx.Plugin.Ipc.GetPenumbraIpc().SetCollectionForObject(actor2, actor.PenumbraCollection);
			}
			if (actor.CustomizePlus != Guid.Empty && _ctx.Plugin.Ipc.IsCustomizeActive)
			{
				(int, string) profileByUniqueId = _ctx.Plugin.Ipc.GetCustomizeIpc().GetProfileByUniqueId(actor.CustomizePlus);
				if (profileByUniqueId.Item2 != string.Empty)
				{
					_ctx.Plugin.Ipc.GetCustomizeIpc().SetTemporaryProfile(a.Actor.ObjectIndex, profileByUniqueId.Item2);
					a.AssignedProfile = actor.CustomizePlus;
				}
			}
			SetupActorPosition(actor, a);
			await _framework.DelayTicks(45L, default(CancellationToken));
			_task?.Wait();
			_task = _ctx?.Posing.ApplyPoseFile(a.Pose, actor.Pose, PoseMode.All, PoseTransforms.Rotation | PoseTransforms.Position | PoseTransforms.Scale | PoseTransforms.PositionRoot);
		});
	}

	private unsafe void SetupActorPosition(SceneFile.ActorInfo loaded, ActorEntity actor)
	{
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		//IL_0019: Unknown result type (might be due to invalid IL or missing references)
		//IL_0084: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
		BattleChara* csGameObject = (BattleChara*)actor.CsGameObject;
		Unsafe.Write(&((BattleChara)csGameObject).DefaultPosition, Vector3.op_Implicit(loaded.Location.Position));
		((BattleChara)csGameObject).DefaultRotation = loaded.DefaultRotation;
		((BattleChara)csGameObject).SetPosition(loaded.Location.Position.X, loaded.Location.Position.Y, loaded.Location.Position.Z);
		((BattleChara)csGameObject).SetRotation(loaded.DefaultRotation);
		CharacterBase* character = actor.GetCharacter();
		Unsafe.Write(&((CharacterBase)character).Position, Vector3.op_Implicit(loaded.Location.Position));
		Unsafe.Write(&((CharacterBase)character).Rotation, Quaternion.op_Implicit(loaded.Location.Rotation));
		Unsafe.Write(&((CharacterBase)character).Scale, Vector3.op_Implicit(loaded.Location.Scale));
	}
}
