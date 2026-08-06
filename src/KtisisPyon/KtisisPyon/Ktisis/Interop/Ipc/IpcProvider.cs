using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using FFXIVClientStructs.Havok.Common.Base.Math.Quaternion;
using FFXIVClientStructs.Havok.Common.Base.Math.Vector;
using Ktisis.Core.Attributes;
using Ktisis.Data.Files;
using Ktisis.Data.Json;
using Ktisis.Editor.Context;
using Ktisis.Editor.Posing;
using Ktisis.Editor.Posing.Data;
using Ktisis.Scene.Entities;
using Ktisis.Scene.Entities.Game;
using Ktisis.Scene.Entities.Skeleton;
using Ktisis.Scene.Modules.Actors;

namespace Ktisis.Interop.Ipc;

[Singleton]
public class IpcProvider(ContextManager ctxManager, IDalamudPluginInterface dpi, JsonFileSerializer fileSerializer) : IDisposable
{
	private ICallGateProvider<(int, int)> IpcVersion { get; } = dpi.GetIpcProvider<(int, int)>("Ktisis.ApiVersion");

	private ICallGateProvider<bool> IpcRefreshActions { get; } = dpi.GetIpcProvider<bool>("Ktisis.RefreshActors");

	private ICallGateProvider<bool> IpcIsPosing { get; } = dpi.GetIpcProvider<bool>("Ktisis.IsPosing");

	private ICallGateProvider<uint, string, Task<bool>> IpcLoadPose { get; } = dpi.GetIpcProvider<uint, string, Task<bool>>("Ktisis.LoadPose");

	private ICallGateProvider<uint, string, bool, bool, bool, Task<bool>> IpcLoadPoseExtended { get; } = dpi.GetIpcProvider<uint, string, bool, bool, bool, Task<bool>>("Ktisis.LoadPoseExtended");

	private ICallGateProvider<uint, Task<string?>> IpcSavePose { get; } = dpi.GetIpcProvider<uint, Task<string>>("Ktisis.SavePose");

	private ICallGateProvider<Task<Dictionary<int, HashSet<string>>>> IpcSelectedBones { get; } = dpi.GetIpcProvider<Task<Dictionary<int, HashSet<string>>>>("Ktisis.SelectedBones");

	private ICallGateProvider<bool, bool> IpcPosingChangedEvent { get; } = dpi.GetIpcProvider<bool, bool>("Ktisis.PosingChanged");

	private ICallGateProvider<uint, Dictionary<string, Matrix4x4>, Task<bool>> IpcApplyAbsolutePoses { get; } = dpi.GetIpcProvider<uint, Dictionary<string, Matrix4x4>, Task<bool>>("Ktisis.ApplyAbsolutePoses");

	private (int, int) GetVersion()
	{
		return (1, 0);
	}

	private bool RefreshActors()
	{
		ctxManager.Current?.Scene.GetModule<ActorModule>().RefreshGPoseActors();
		return true;
	}

	private bool IsActive()
	{
		return ctxManager.Current?.Posing.IsEnabled ?? false;
	}

	private async Task<bool> LoadPose(uint index, string json, bool rotation, bool position, bool scale)
	{
		PoseTransforms poseTransforms = PoseTransforms.None;
		if (rotation)
		{
			poseTransforms |= PoseTransforms.Rotation;
		}
		if (position)
		{
			poseTransforms |= PoseTransforms.Position;
		}
		if (scale)
		{
			poseTransforms |= PoseTransforms.Scale;
		}
		return await LoadPose(index, json, poseTransforms);
	}

	private async Task<bool> LoadPose(uint index, string json)
	{
		return await LoadPose(index, json, PoseTransforms.Rotation);
	}

	private async Task<bool> LoadPose(uint index, string json, PoseTransforms transforms)
	{
		if (ctxManager.Current == null)
		{
			return false;
		}
		PoseFile poseFile = fileSerializer.Deserialize<PoseFile>(json);
		ActorEntity entityForIndex = ctxManager.Current.Scene.GetEntityForIndex(index);
		if (entityForIndex == null || poseFile == null)
		{
			return false;
		}
		await ctxManager.Current.Posing.ApplyPoseFile(entityForIndex.Pose, poseFile, PoseMode.All, transforms);
		return true;
	}

	private async Task<string?> SavePose(uint index)
	{
		if (ctxManager.Current == null)
		{
			return null;
		}
		ActorEntity entityForIndex = ctxManager.Current.Scene.GetEntityForIndex(index);
		if (entityForIndex?.Pose == null)
		{
			return null;
		}
		PoseFile obj = await ctxManager.Current.Posing.SavePoseFile(entityForIndex.Pose);
		return fileSerializer.Serialize(obj);
	}

	private async Task<Dictionary<int, HashSet<string>>> SelectedBones()
	{
		List<ActorEntity> list = ctxManager.Current?.Scene?.Children.OfType<ActorEntity>().ToList();
		if (list == null || list.Count == 0)
		{
			return new Dictionary<int, HashSet<string>>();
		}
		Dictionary<int, HashSet<string>> dictionary = new Dictionary<int, HashSet<string>>();
		foreach (ActorEntity item in list)
		{
			if (item.IsValid && item.Pose != null)
			{
				dictionary[item.Actor.ObjectIndex] = (from s in (from s in item.Children.OfType<EntityPose>().SelectMany((EntityPose x) => x.Recurse().Append(x))
						where s.IsSelected
						select s).SelectMany((SceneEntity s) => s.Recurse().Append(s)).OfType<BoneNode>()
					select s.Info.Name).ToHashSet();
			}
		}
		return dictionary;
	}

	private ActorEntity? GetEntity(uint index)
	{
		return ctxManager.Current?.Scene?.GetEntityForIndex(index);
	}

	private unsafe async Task<bool> ApplyAbsolutePoses(uint index, Dictionary<string, Matrix4x4> matrices)
	{
		ActorEntity entity = GetEntity(index);
		if (entity?.Pose == null || matrices.Count == 0)
		{
			return false;
		}
		Skeleton* skeleton = entity.Pose.GetSkeleton();
		if (skeleton == null)
		{
			return false;
		}
		Dictionary<string, (Vector3, Quaternion, Vector3)> dictionary = new Dictionary<string, (Vector3, Quaternion, Vector3)>();
		foreach (KeyValuePair<string, Matrix4x4> matrix in matrices)
		{
			BoneNode boneNode = entity.Pose.FindBoneByName(matrix.Key);
			if (boneNode != null)
			{
				hkaPose* pose = boneNode.GetPose();
				if (pose != null && ((hkaPose)pose).LocalPose.Data != null)
				{
					Matrix4x4.Decompose(matrix.Value, out var scale, out var rotation, out var translation);
					dictionary[matrix.Key] = (translation, rotation, scale);
					byte* num = (byte*)((hkaPose)pose).LocalPose.Data + (nint)boneNode.Info.BoneIndex * (nint)Unsafe.SizeOf<hkQsTransformf>();
					Unsafe.Write(&((hkQsTransformf)num).Translation, new hkVector4f
					{
						X = translation.X,
						Y = translation.Y,
						Z = translation.Z,
						W = 0f
					});
					Unsafe.Write(&((hkQsTransformf)num).Rotation, new hkQuaternionf
					{
						X = rotation.X,
						Y = rotation.Y,
						Z = rotation.Z,
						W = rotation.W
					});
				}
			}
		}
		for (int i = 0; i < ((Skeleton)skeleton).PartialSkeletonCount; i++)
		{
			HavokPosing.SyncModelSpace(skeleton, i);
		}
		foreach (KeyValuePair<string, (Vector3, Quaternion, Vector3)> item2 in dictionary)
		{
			BoneNode boneNode2 = entity.Pose.FindBoneByName(item2.Key);
			if (boneNode2 != null)
			{
				hkaPose* pose2 = boneNode2.GetPose();
				if (pose2 != null && ((hkaPose)pose2).ModelPose.Data != null)
				{
					Vector3 item = item2.Value.Item3;
					Unsafe.Write(&((hkQsTransformf)((byte*)((hkaPose)pose2).ModelPose.Data + (nint)boneNode2.Info.BoneIndex * (nint)Unsafe.SizeOf<hkQsTransformf>())).Scale, new hkVector4f
					{
						X = item.X,
						Y = item.Y,
						Z = item.Z,
						W = 0f
					});
				}
			}
		}
		return true;
	}

	public void InvokePosingChanged(bool status)
	{
		IpcPosingChangedEvent.SendMessage(status);
	}

	public void RegisterIpc()
	{
		IpcVersion.RegisterFunc((Func<(int, int)>)GetVersion);
		IpcRefreshActions.RegisterFunc((Func<bool>)RefreshActors);
		IpcIsPosing.RegisterFunc((Func<bool>)IsActive);
		IpcLoadPose.RegisterFunc((Func<uint, string, Task<bool>>)LoadPose);
		IpcLoadPoseExtended.RegisterFunc((Func<uint, string, bool, bool, bool, Task<bool>>)LoadPose);
		IpcSavePose.RegisterFunc((Func<uint, Task<string>>)SavePose);
		IpcSelectedBones.RegisterFunc((Func<Task<Dictionary<int, HashSet<string>>>>)SelectedBones);
		IpcApplyAbsolutePoses.RegisterFunc((Func<uint, Dictionary<string, Matrix4x4>, Task<bool>>)ApplyAbsolutePoses);
	}

	private void UnregisterIpc()
	{
		((ICallGateProvider)IpcVersion).UnregisterFunc();
		((ICallGateProvider)IpcRefreshActions).UnregisterFunc();
		((ICallGateProvider)IpcIsPosing).UnregisterFunc();
		((ICallGateProvider)IpcLoadPose).UnregisterFunc();
		((ICallGateProvider)IpcLoadPoseExtended).UnregisterFunc();
		((ICallGateProvider)IpcSavePose).UnregisterFunc();
		((ICallGateProvider)IpcSelectedBones).UnregisterFunc();
		((ICallGateProvider)IpcPosingChangedEvent).UnregisterFunc();
		((ICallGateProvider)IpcApplyAbsolutePoses).UnregisterFunc();
	}

	public void Dispose()
	{
		Ktisis.Log.Info("Disposing KtisisPyon IPC Provider.");
		UnregisterIpc();
	}
}
