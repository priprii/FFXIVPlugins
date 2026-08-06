using System;
using System.Numerics;
using Dalamud.Game.ClientState.Keys;
using Ktisis.Editor.Context.Types;
using Ktisis.Structs.Input;

namespace Ktisis.Editor.Camera.Types;

public class WorkCamera : KtisisCamera
{
	private readonly IEditorContext _ctx;

	private static readonly Vector3 UpVector = Vector3.UnitY;

	private const float ClampY = 1.57072f;

	private const float ReferenceFrameMs = 16.666666f;

	private const float MaxDeltaMs = 1000f;

	public Vector3 Position;

	public Vector3 Rotation;

	private float MoveSpeed;

	private float MoveSpeedModifier = 1f;

	private Vector3 Velocity;

	private Vector2 MouseDelta;

	private Vector3 InterpPos;

	private DateTime LastTime;

	private float DefaultSpeed => _ctx.Config.Editor.WorkcamMoveSpeed;

	public WorkCamera(ICameraManager manager, IEditorContext context)
		: base(manager)
	{
		_ctx = context;
	}

	public void SetInitialPosition(Vector3 pos, Vector3 rot)
	{
		Position = pos;
		InterpPos = pos;
		Rotation = rot;
		MoveSpeedModifier = 1f;
	}

	public unsafe void UpdateControl(MouseDeviceData* mouseData, KeyboardDeviceData* keyData)
	{
		bool leftHeld = false;
		bool rightHeld = false;
		int scrollDelta = 0;
		if (mouseData != null)
		{
			UpdateMouse(mouseData, out leftHeld, out rightHeld, out scrollDelta);
		}
		if (keyData != null)
		{
			UpdateKeyboard(keyData, leftHeld, rightHeld, scrollDelta);
		}
	}

	private unsafe void UpdateMouse(MouseDeviceData* mouseData, out bool leftHeld, out bool rightHeld, out int scrollDelta)
	{
		Vector2 delta = mouseData->GetDelta();
		leftHeld = mouseData->IsButtonHeld(MouseButton.Left);
		rightHeld = mouseData->IsButtonHeld(MouseButton.Right);
		if (rightHeld)
		{
			MouseDelta += delta;
		}
		scrollDelta = mouseData->ScrollDelta;
		if (_ctx.Cameras.Current != null && _ctx.Cameras.Current is WorkCamera)
		{
			mouseData->ScrollDelta = 0;
		}
	}

	private unsafe void UpdateKeyboard(KeyboardDeviceData* keyData, bool leftHeld, bool rightHeld, int scrollDelta)
	{
		//IL_0031: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_0123: Unknown result type (might be due to invalid IL or missing references)
		//IL_015a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0191: Unknown result type (might be due to invalid IL or missing references)
		//IL_01c6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a0: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		MoveSpeed = DefaultSpeed;
		if (keyData->IsKeyDown(_ctx.Config.Keybinds.Keybinds["Camera_Work_Fast"].Combo.Key))
		{
			MoveSpeed *= _ctx.Config.Editor.WorkcamFastMulti;
		}
		else if (keyData->IsKeyDown(_ctx.Config.Keybinds.Keybinds["Camera_Work_Slow"].Combo.Key))
		{
			MoveSpeed *= _ctx.Config.Editor.WorkcamSlowMulti;
		}
		if (scrollDelta != 0)
		{
			MoveSpeedModifier = Math.Clamp(MoveSpeedModifier * 1f + 0.04f * (float)scrollDelta, 0.01f, 5f);
		}
		MoveSpeed *= MoveSpeedModifier;
		int num = 0;
		bool flag = leftHeld && rightHeld;
		if (IsKeyDown(keyData, _ctx.Config.Keybinds.Keybinds["Camera_Work_Forward"].Combo.Key) || flag)
		{
			num--;
		}
		if (IsKeyDown(keyData, _ctx.Config.Keybinds.Keybinds["Camera_Work_Back"].Combo.Key))
		{
			num++;
		}
		int num2 = 0;
		if (IsKeyDown(keyData, _ctx.Config.Keybinds.Keybinds["Camera_Work_Left"].Combo.Key))
		{
			num2--;
		}
		if (IsKeyDown(keyData, _ctx.Config.Keybinds.Keybinds["Camera_Work_Right"].Combo.Key))
		{
			num2++;
		}
		Velocity.X = (float)num * MathF.Sin(Rotation.X) * MathF.Cos(Rotation.Y) + (float)num2 * MathF.Cos(Rotation.X);
		Velocity.Y = (float)num * MathF.Sin(Rotation.Y);
		Velocity.Z = (float)num * MathF.Cos(Rotation.X) * MathF.Cos(Rotation.Y) + (float)(-num2) * MathF.Sin(Rotation.X);
		if (IsKeyDown(keyData, _ctx.Config.Keybinds.Keybinds["Camera_Work_Up"].Combo.Key))
		{
			Velocity.Y += _ctx.Config.Editor.WorkcamVertMulti;
		}
		if (IsKeyDown(keyData, _ctx.Config.Keybinds.Keybinds["Camera_Work_Down"].Combo.Key))
		{
			Velocity.Y -= _ctx.Config.Editor.WorkcamVertMulti;
		}
	}

	private unsafe static bool IsKeyDown(KeyboardDeviceData* keyData, VirtualKey key)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		return keyData->IsKeyDown(key, consume: true);
	}

	public unsafe void Update()
	{
		DateTime now = DateTime.Now;
		float num = Math.Clamp((float)(now - LastTime).TotalMilliseconds, 1f, 1000f);
		LastTime = now;
		float num2 = Math.Abs(base.Camera->RenderEx->FoV);
		MouseDelta = MouseDelta * num2 * _ctx.Config.Editor.WorkcamSens * 0.0175f;
		Rotation.X -= MouseDelta.X;
		Rotation.Y = Math.Clamp(Rotation.Y + MouseDelta.Y, -1.57072f, 1.57072f);
		MouseDelta = Vector2.Zero;
		Position += Velocity * MoveSpeed * num2 * (num / 16.666666f);
		InterpPos = Vector3.Lerp(InterpPos, Position, 1f - MathF.Pow(0.5f, num * 0.05f));
	}

	public Matrix4x4 CalculateViewMatrix()
	{
		Vector3 interpPos = InterpPos;
		Vector3 vector = CalculateLookDirection();
		Vector3 upVector = UpVector;
		float num = MathF.Sqrt(vector.X * vector.X + vector.Y * vector.Y + vector.Z * vector.Z);
		Vector3 vector2 = vector / num;
		Vector3 vector3 = new Vector3(upVector.Y * vector2.Z - upVector.Z * vector2.Y, upVector.Z * vector2.X - upVector.X * vector2.Z, upVector.X * vector2.Y - upVector.Y * vector2.X);
		float num2 = MathF.Sqrt(vector3.X * vector3.X + vector3.Y * vector3.Y + vector3.Z * vector3.Z);
		Vector3 vector4 = vector3 / num2;
		Vector3 vector5 = new Vector3(vector2.Y * vector4.Z - vector2.Z * vector4.Y, vector2.Z * vector4.X - vector2.X * vector4.Z, vector2.X * vector4.Y - vector2.Y * vector4.X);
		Vector3 vector6 = new Vector3((0f - interpPos.X) * vector4.X - interpPos.Y * vector4.Y - interpPos.Z * vector4.Z, (0f - interpPos.X) * vector5.X - interpPos.Y * vector5.Y - interpPos.Z * vector5.Z, (0f - interpPos.X) * vector2.X - interpPos.Y * vector2.Y - interpPos.Z * vector2.Z);
		return new Matrix4x4(vector4.X, vector5.X, vector2.X, 0f, vector4.Y, vector5.Y, vector2.Y, 0f, vector4.Z, vector5.Z, vector2.Z, 0f, vector6.X, vector6.Y, vector6.Z, 1f);
	}

	public Vector3 CalculateLookDirection()
	{
		return new Vector3(MathF.Sin(Rotation.X) * MathF.Cos(Rotation.Y), MathF.Sin(Rotation.Y), MathF.Cos(Rotation.X) * MathF.Cos(Rotation.Y));
	}
}
