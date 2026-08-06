using FFXIVClientStructs.Havok.Common.Base.Container.Array;

namespace Ktisis.Common.Extensions;

public static class HavokEx
{
	public static T[] Copy<T>(this hkArray<T> array) where T : unmanaged
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		int length = array.Length;
		T[] array2 = new T[length];
		for (int i = 0; i < length; i++)
		{
			array2[i] = array[i];
		}
		return array2;
	}

	public unsafe static void Initialize<T>(hkArray<T>* array, T* data = null, int length = 0) where T : unmanaged
	{
		array->Data = data;
		array->Length = length;
		array->CapacityAndFlags = int.MinValue;
	}
}
