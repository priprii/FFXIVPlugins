using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using Dalamud.Interface.Colors;
using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;
using FFXIVClientStructs.FFXIV.Common.Math;

namespace TargetPyon;

public class SceneObjectEntityInfo : IObjectEntityInfo
{
	public unsafe Object* SceneObject;

	public nint? _Address;

	private string _Name;

	private string _TypeName;

	private ObjectType _Type;

	public Vector3 _Position;

	public float _Rotation;

	public float _Scale;

	public ulong Id { get; init; }

	public unsafe bool IsValid
	{
		get
		{
			if (SceneObject != null)
			{
				return (Object*?)SceneObject == (Object*?)_Address;
			}
			return false;
		}
	}

	public string Name => _Name;

	public string TypeName => _TypeName;

	public Vector4 TypeColour
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Expected I4, but got Unknown
			ObjectType type = _Type;
			return (int)type switch
			{
				1 => ImGuiColors.ParsedGreen, 
				0 => ImGuiColors.ParsedGreen, 
				2 => ImGuiColors.ParsedGreen, 
				4 => ImGuiColors.ParsedGreen, 
				3 => ImGuiColors.ParsedGreen, 
				8 => ImGuiColors.ParsedGreen, 
				7 => ImGuiColors.ParsedGreen, 
				5 => ImGuiColors.ParsedGreen, 
				9 => ImGuiColors.ParsedGreen, 
				6 => ImGuiColors.ParsedGreen, 
				10 => ImGuiColors.ParsedGreen, 
				_ => ImGuiColors.DalamudWhite, 
			};
		}
	}

	public unsafe Vector3 Position
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			return _Position;
		}
		set
		{
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0021: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			if (IsValid)
			{
				_Position = Unsafe.Write(&((Object)SceneObject).Position, new Vector3(value.X, value.Y, value.Z));
			}
		}
	}

	public unsafe float Rotation
	{
		get
		{
			return _Rotation;
		}
		set
		{
			if (IsValid)
			{
				_Rotation = (((Quaternion)(&((Object)SceneObject).Rotation)).X = value);
			}
		}
	}

	public unsafe float Scale
	{
		get
		{
			return _Scale;
		}
		set
		{
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			if (IsValid)
			{
				Unsafe.Write(&((Object)SceneObject).Scale, new Vector3(Math.Max(value, 0.01f), Math.Max(value, 0.01f), Math.Max(value, 0.01f)));
				_Scale = Math.Max(value, 0.01f);
			}
		}
	}

	public bool IsVisible => true;

	public unsafe SceneObjectEntityInfo(Object* baseObject, uint index)
	{
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0096: Unknown result type (might be due to invalid IL or missing references)
		//IL_009b: Unknown result type (might be due to invalid IL or missing references)
		SceneObject = baseObject;
		_Address = (nint)baseObject;
		Id = (ulong)_Address.Value;
		_Name = $"SceneObject #{index}";
		_TypeName = $"{((Object)SceneObject).GetObjectType()}";
		_Type = ((Object)SceneObject).GetObjectType();
		_Position = ((Object)SceneObject).Position;
		_Rotation = ((Quaternion)(&((Object)SceneObject).Rotation)).X;
		_Scale = ((Vector3)(&((Object)SceneObject).Scale)).X;
	}

	public void Hide()
	{
	}

	public void Show()
	{
	}
}
