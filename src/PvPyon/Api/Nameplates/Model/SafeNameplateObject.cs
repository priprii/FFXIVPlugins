using System;
using System.Runtime.InteropServices;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace PvPyon.Api.Nameplates.Model;

public class SafeNameplateObject
{
	private int _Index;

	private SafeNameplateInfo _NamePlateInfo;

	public nint Pointer { get; }

	public NamePlateObject Data { get; }

	public int Index
	{
		get
		{
			int result = _Index;
			if (_Index == -1)
			{
				SafeNameplateObject namePlateObject = XIVApi.GetSafeAddonNamePlate().GetNamePlateObject(0);
				if (namePlateObject == null)
				{
					result = -1;
				}
				else
				{
					nint pointer = namePlateObject.Pointer;
					int num = Marshal.SizeOf(typeof(NamePlateObject));
					long num2 = (((IntPtr)Pointer).ToInt64() - ((IntPtr)pointer).ToInt64()) / num;
					result = ((num2 >= 0 && num2 < 50) ? (_Index = (int)num2) : (-2));
				}
			}
			return result;
		}
	}

	public SafeNameplateInfo NamePlateInfo
	{
		get
		{
			SafeNameplateInfo result = null;
			if (_NamePlateInfo != null)
			{
				nint raptureAtkModulePtr = XIVApi.RaptureAtkModulePtr;
				if (raptureAtkModulePtr != IntPtr.Zero)
				{
					nint pointer = raptureAtkModulePtr + ((IntPtr)Marshal.OffsetOf(typeof(RaptureAtkModule), "NamePlateInfoArray")).ToInt32() + Marshal.SizeOf(typeof(NamePlateInfo)) * Index;
					result = (_NamePlateInfo = new SafeNameplateInfo(pointer));
				}
			}
			return result;
		}
	}

	public nint IconImageNodeAddress => Marshal.ReadIntPtr(Pointer + ((IntPtr)Marshal.OffsetOf(typeof(NamePlateObject), "IconImageNode")).ToInt32());

	public nint NameNodeAddress => Marshal.ReadIntPtr(Pointer + ((IntPtr)Marshal.OffsetOf(typeof(NamePlateObject), "NameText")).ToInt32());

	public AtkImageNode IconImageNode => Marshal.PtrToStructure<AtkImageNode>(IconImageNodeAddress);

	public AtkTextNode NameTextNode => Marshal.PtrToStructure<AtkTextNode>(NameNodeAddress);

	public bool IsVisible
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			NamePlateObject data = Data;
			return ((NamePlateObject)(ref data)).IsVisible;
		}
	}

	public bool IsLocalPlayer
	{
		get
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			NamePlateObject data = Data;
			return ((NamePlateObject)(ref data)).IsLocalPlayer;
		}
	}

	public bool IsPlayer => Data.NameplateKind == 0;

	public SafeNameplateObject(nint pointer, int index = -1)
	{
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0014: Unknown result type (might be due to invalid IL or missing references)
		Pointer = pointer;
		Data = Marshal.PtrToStructure<NamePlateObject>(pointer);
		_Index = index;
	}

	public void SetIconPosition(short x, short y)
	{
		nint ptr = Pointer + ((IntPtr)Marshal.OffsetOf(typeof(NamePlateObject), "IconXAdjust")).ToInt32();
		nint ptr2 = Pointer + ((IntPtr)Marshal.OffsetOf(typeof(NamePlateObject), "IconYAdjust")).ToInt32();
		Marshal.WriteInt16(ptr, x);
		Marshal.WriteInt16(ptr2, y);
	}
}
