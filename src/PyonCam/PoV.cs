using System;
using System.Runtime.CompilerServices;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Graphics.Render;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Client.System.Input;
using FFXIVClientStructs.FFXIV.Common.Math;
using FFXIVClientStructs.Havok.Animation.Rig;
using FFXIVClientStructs.Havok.Common.Base.Math.QsTransform;
using FFXIVClientStructs.Havok.Common.Base.Math.Quaternion;
using PyonCam.Config;
using PyonCam.Config.Cam;
using PyonCam.Services;

namespace PyonCam;

public class PoV
{
	private readonly Configuration _config;

	private readonly IServiceContext _services;

	public bool Enabled;

	private float prevZoom;

	private float prevFoV;

	private float prevMinFoV;

	private float prevMinVRotation;

	private float prevMaxVRotation;

	private float prevTilt;

	private const int HeadBoneIndex = 46;

	private Vector3? prevActorPos;

	private Quaternion rotPrev = Quaternion.Identity;

	private Quaternion rotCur = Quaternion.Identity;

	private float yawPrev;

	private float yawCur;

	private float yawDelta;

	private float pitchPrev;

	private float pitchCur;

	private float pitchDelta;

	private PresetService PresetService => _services.Get<PresetService>();

	private CameraService CameraService => _services.Get<CameraService>();

	private InputService InputService => _services.Get<InputService>();

	public PoV(Configuration config, IServiceContext services)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		_config = config;
		_services = services;
	}

	public unsafe void Toggle(GameCamera* camera)
	{
		bool flag = camera->mode == 0;
		if (!Enabled && flag)
		{
			Enabled = true;
			Reset();
			prevZoom = camera->currentZoom;
			prevFoV = camera->currentFoV;
			prevMinFoV = camera->minFoV;
			prevMinVRotation = camera->minVRotation;
			prevMaxVRotation = camera->maxVRotation;
			prevTilt = camera->tilt;
		}
		if (Enabled && !flag)
		{
			Disable(camera);
		}
	}

	public static Quaternion ToQuaternion(hkQuaternionf hkQuat)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_000c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		return new Quaternion(hkQuat.X, hkQuat.Y, hkQuat.Z, hkQuat.W);
	}

	public unsafe bool Update(GameCamera* camera, GameObject* target, Vector3* position, bool swapPerson, CameraConfigPreset preset)
	{
		//IL_011b: Unknown result type (might be due to invalid IL or missing references)
		//IL_012d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0132: Unknown result type (might be due to invalid IL or missing references)
		//IL_0137: Unknown result type (might be due to invalid IL or missing references)
		//IL_013a: Unknown result type (might be due to invalid IL or missing references)
		//IL_013f: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_015e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0162: Unknown result type (might be due to invalid IL or missing references)
		//IL_0167: Unknown result type (might be due to invalid IL or missing references)
		//IL_0188: Unknown result type (might be due to invalid IL or missing references)
		//IL_018c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_019f: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_01b6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0223: Unknown result type (might be due to invalid IL or missing references)
		//IL_0228: Unknown result type (might be due to invalid IL or missing references)
		//IL_022d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0238: Unknown result type (might be due to invalid IL or missing references)
		//IL_023d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_024a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0262: Unknown result type (might be due to invalid IL or missing references)
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_026e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0278: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0280: Unknown result type (might be due to invalid IL or missing references)
		//IL_0285: Unknown result type (might be due to invalid IL or missing references)
		//IL_028a: Unknown result type (might be due to invalid IL or missing references)
		//IL_028c: Unknown result type (might be due to invalid IL or missing references)
		//IL_028e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0290: Unknown result type (might be due to invalid IL or missing references)
		//IL_0297: Unknown result type (might be due to invalid IL or missing references)
		//IL_029e: Unknown result type (might be due to invalid IL or missing references)
		//IL_042b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0430: Unknown result type (might be due to invalid IL or missing references)
		if (!Enabled || camera->mode != 0 || target == null)
		{
			return false;
		}
		try
		{
			if (!preset.EnablePoV)
			{
				Disable(camera);
				return false;
			}
			CharacterBase* drawObject = (CharacterBase*)((GameObject)target).GetDrawObject();
			if (drawObject == null)
			{
				return false;
			}
			Skeleton* skeleton = ((CharacterBase)drawObject).Skeleton;
			if (skeleton == null)
			{
				return false;
			}
			PartialSkeleton* partialSkeletons = ((Skeleton)skeleton).PartialSkeletons;
			if (partialSkeletons == null)
			{
				return false;
			}
			hkaPose* havokPose = ((PartialSkeleton)partialSkeletons).GetHavokPose(0);
			if (havokPose == null)
			{
				return false;
			}
			if (((hkaSkeleton)((hkaPose)havokPose).Skeleton).Bones.Length < 46)
			{
				return false;
			}
			float rotation = ((GameObject)target).Rotation;
			if (!prevActorPos.HasValue)
			{
				prevActorPos = ((GameObject)target).Position;
				return false;
			}
			camera->currentFoV = preset.PoVFoV;
			float num = MathF.Cos(rotation);
			float num2 = MathF.Sin(rotation);
			Vector3 val = new Vector3(preset.PoVForwardOffset * num2 + preset.PoVSideOffset * num, preset.PoVHeightOffset, preset.PoVForwardOffset * num - preset.PoVSideOffset * num2) + Vector3.op_Implicit(CameraService.GetWorldBonePosition(target, 1u));
			Vector3 position2 = ((GameObject)target).Position;
			Vector3? val2 = prevActorPos;
			Vector3? val3 = (val2.HasValue ? new Vector3?(position2 - val2.GetValueOrDefault()) : ((Vector3?)null));
			Vector3? val4 = (val3.HasValue ? new Vector3?(val + val3.GetValueOrDefault()) : ((Vector3?)null));
			prevActorPos = ((GameObject)target).Position;
			Unsafe.Write(position, val4.Value);
			bool isCursorVisible = ((Cursor)Cursor.Instance()).IsCursorVisible;
			camera->minVRotation = ((isCursorVisible && preset.PoVRotation) ? (-90f) : preset.PoVMinVRotation);
			camera->maxVRotation = ((isCursorVisible && preset.PoVRotation) ? 90f : preset.PoVMaxVRotation);
			if (preset.PoVRotation)
			{
				hkQsTransformf* ptr = ((hkaPose)havokPose).AccessBoneModelSpace(46, (PropagateOrNot)0);
				rotCur = Quaternion.Normalize(ToQuaternion(((hkQsTransformf)ptr).Rotation));
				_ = Quaternion.Invert((rotPrev == Quaternion.Identity) ? rotCur : rotPrev) * rotCur;
				Vector3 val5 = Vector3.Transform(Vector3.Forward, rotCur);
				Vector3 val6 = Vector3.Transform(Vector3.Up, rotCur);
				Vector3.Cross(val5, val6);
				yawCur = MathF.Atan2(val5.X, val5.Z);
				yawDelta = ((yawPrev == 0f) ? 0f : (yawCur - yawPrev));
				yawPrev = yawCur;
				float num3 = MathF.Atan2(2f * (rotCur.X * rotCur.Y + rotCur.Z * rotCur.W), 1f - 2f * (rotCur.X * rotCur.X + rotCur.Z * rotCur.Z));
				pitchCur = 0f - num3 + (float)Math.PI;
				pitchDelta = ((pitchPrev == 0f) ? 0f : (pitchCur + 10f - (pitchPrev + 10f)));
				pitchPrev = pitchCur;
				int num4 = ((Math.Floor(((double)(camera->currentVRotation + pitchDelta) * (180.0 / Math.PI) + 90.0) / 180.0) % 2.0 == 0.0) ? 1 : 0);
				camera->tilt = ((isCursorVisible && num4 == 0) ? 3.14159f : 0f);
				camera->currentHRotation += yawDelta;
				camera->currentVRotation += pitchDelta;
				rotPrev = rotCur;
			}
			return true;
		}
		catch (Exception value)
		{
			_services.Log.Warning($"{value}", Array.Empty<object>());
			return false;
		}
	}

	public unsafe void Disable(GameCamera* camera)
	{
		if (Enabled)
		{
			Enabled = false;
			Reset();
			camera->currentZoom = (camera->interpolatedZoom = prevZoom);
			camera->currentFoV = prevFoV;
			camera->minFoV = prevMinFoV;
			camera->minVRotation = prevMinVRotation;
			camera->maxVRotation = prevMaxVRotation;
			camera->tilt = prevTilt;
			CameraService.ApplyPreset(PresetService.CurrentPreset);
		}
	}

	private void Reset()
	{
		prevActorPos = null;
		yawPrev = 0f;
		yawDelta = 0f;
		yawCur = 0f;
		pitchPrev = 0f;
		pitchDelta = 0f;
		pitchCur = 0f;
	}
}
