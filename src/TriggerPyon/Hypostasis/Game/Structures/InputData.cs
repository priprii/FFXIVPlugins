using System.Runtime.InteropServices;
using Hypostasis.Dalamud;

namespace Hypostasis.Game.Structures;

[StructLayout(LayoutKind.Explicit, Size = 2592)]
[GameStructure("48 89 5C 24 08 48 89 74 24 10 57 48 83 EC 20 48 8D 05 ?? ?? ?? ?? 48 63 DA")]
public struct InputData : IHypostasisStructure
{
	public unsafe delegate Bool InputIDDelegate(InputData* inputData, uint id);

	public unsafe delegate int GetAxisInputDelegate(InputData* inputData, uint id);

	public delegate sbyte GetMouseWheelStatusDelegate();

	public unsafe delegate void* GetInputBindingDelegate(InputData* inputData, uint id);

	[FieldOffset(2484)]
	public int inputIDCount;

	public static readonly GameFunction<InputIDDelegate> isInputIDHeld = new GameFunction<InputIDDelegate>("E9 ?? ?? ?? ?? B9 4F 01 00 00");

	public static readonly GameFunction<InputIDDelegate> isInputIDPressed = new GameFunction<InputIDDelegate>("E9 ?? ?? ?? ?? 83 7F 44 02");

	public static readonly GameFunction<InputIDDelegate> isInputIDLongPressed = new GameFunction<InputIDDelegate>("E8 ?? ?? ?? ?? 84 C0 74 37 EB 06");

	public static readonly GameFunction<InputIDDelegate> isInputIDReleased = new GameFunction<InputIDDelegate>("E8 ?? ?? ?? ?? 88 43 0F");

	public static readonly GameFunction<GetAxisInputDelegate> getAxisInput = new GameFunction<GetAxisInputDelegate>("E8 ?? ?? ?? ?? 66 44 0F 6E C3");

	public static readonly GameFunction<GetMouseWheelStatusDelegate> getMouseWheelStatus = new GameFunction<GetMouseWheelStatusDelegate>("E8 ?? ?? ?? ?? F7 D8 48 8B CB");

	public static readonly GameFunction<GetInputBindingDelegate> getInputBinding = new GameFunction<GetInputBindingDelegate>("48 63 C2 48 6B C0 0B");

	public unsafe bool IsInputIDHeld(uint id)
	{
		fixed (InputData* inputData = &this)
		{
			return isInputIDHeld.Invoke(inputData, id);
		}
	}

	public unsafe bool IsInputIDPressed(uint id)
	{
		fixed (InputData* inputData = &this)
		{
			return isInputIDPressed.Invoke(inputData, id);
		}
	}

	public unsafe bool IsInputIDLongPressed(uint id)
	{
		fixed (InputData* inputData = &this)
		{
			return isInputIDLongPressed.Invoke(inputData, id);
		}
	}

	public unsafe bool IsInputIDReleased(uint id)
	{
		fixed (InputData* inputData = &this)
		{
			return isInputIDReleased.Invoke(inputData, id);
		}
	}

	public unsafe int GetAxisInput(uint id)
	{
		fixed (InputData* inputData = &this)
		{
			return getAxisInput.Invoke(inputData, id);
		}
	}

	public float GetAxisInputFloat(uint id)
	{
		return (float)GetAxisInput(id) / 100f;
	}

	public static sbyte GetMouseWheelStatus()
	{
		return getMouseWheelStatus.Invoke();
	}

	public unsafe void* GetInputBinding(uint id)
	{
		fixed (InputData* inputData = &this)
		{
			return getInputBinding.Invoke(inputData, id);
		}
	}

	public bool Validate()
	{
		return true;
	}
}
