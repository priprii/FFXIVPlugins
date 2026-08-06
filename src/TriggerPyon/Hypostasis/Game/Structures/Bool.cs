using System;
using System.Runtime.InteropServices;

namespace Hypostasis.Game.Structures;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public readonly struct Bool : IComparable<Bool>, IEquatable<Bool>
{
	[MarshalAs(UnmanagedType.U1)]
	private readonly bool b;

	public Bool(byte b2)
	{
		b = b2 != 0;
	}

	public Bool(bool b2)
	{
		b = b2;
	}

	public static bool operator ==(Bool l, Bool r)
	{
		return l.b == r.b;
	}

	public static bool operator !=(Bool l, Bool r)
	{
		return l.b != r.b;
	}

	public static implicit operator bool(Bool b)
	{
		return b.b;
	}

	public static implicit operator Bool(bool b)
	{
		return new Bool(b);
	}

	public static implicit operator byte(Bool b)
	{
		return b.b ? ((byte)1) : ((byte)0);
	}

	public static implicit operator Bool(byte b)
	{
		return new Bool(b);
	}

	public bool Equals(Bool b2)
	{
		return b.Equals(b2.b);
	}

	public override bool Equals(object o)
	{
		if (o is Bool b)
		{
			return Equals(b);
		}
		return false;
	}

	public int CompareTo(Bool b2)
	{
		return b.CompareTo(b2.b);
	}

	public override int GetHashCode()
	{
		return b.GetHashCode();
	}

	public override string ToString()
	{
		return b.ToString();
	}
}
