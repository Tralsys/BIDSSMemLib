using System;
using System.Runtime.InteropServices;

namespace TR.SMemIF.Mock;

public class SMemIFMock : ISMemIF
{
	public string SMemName { get; }

	public long Capacity { get; }

	public byte[] Memory { get; }

	public SMemIFMock(string smemName, long capacity)
	{
		if (string.IsNullOrWhiteSpace(SMemName))
			throw new ArgumentException("SMemName cannot be null, empty or only whitespace", nameof(SMemName));
		if (capacity <= 0)
			throw new ArgumentOutOfRangeException("Capacity cannot be 0 or less", nameof(capacity));

		SMemName = smemName;
		Capacity = capacity;

		Memory = new byte[Capacity];
	}

	public void Dispose()
	{
	}

	public bool Read<T>(long pos, out T buf) where T : struct
	{
		if (int.MaxValue < pos)
			throw new ArgumentOutOfRangeException("must be in the range of int", nameof(pos));

		buf = (T)Read<T>((int)pos);

		return true;
	}

	public object Read<T>(long pos) where T : struct
	{
		if (int.MaxValue < pos)
			throw new ArgumentOutOfRangeException("must be in the range of int", nameof(pos));

		return Read<T>(pos);
	}

	public object Read<T>(int pos) where T : struct
		=> typeof(T) switch
		{
			Type t when t == typeof(bool) => BitConverter.ToBoolean(Memory, pos),

			Type t when t == typeof(char) => BitConverter.ToChar(Memory, pos),

			Type t when t == typeof(sbyte) => Convert.ToSByte(Memory[pos]),
			Type t when t == typeof(short) => BitConverter.ToInt16(Memory, pos),
			Type t when t == typeof(int) => BitConverter.ToInt32(Memory, pos),
			Type t when t == typeof(long) => BitConverter.ToInt64(Memory, pos),

			Type t when t == typeof(byte) => Memory[pos],
			Type t when t == typeof(ushort) => BitConverter.ToUInt16(Memory, pos),
			Type t when t == typeof(uint) => BitConverter.ToUInt32(Memory, pos),
			Type t when t == typeof(ulong) => BitConverter.ToUInt64(Memory, pos),

			Type t when t == typeof(float) => BitConverter.ToSingle(Memory, pos),
			Type t when t == typeof(double) => BitConverter.ToDouble(Memory, pos),

			_ => throw new TypeLoadException($"The type {typeof(T)} is not supported")
		};

	public bool ReadArray<T>(long pos, T[] buf, int offset, int count) where T : struct
	{
		int memoryStep = Marshal.SizeOf<T>();

		for (int i = offset; i < buf.Length && (i - offset) < count; i++)
		{
			buf[i] = (T)Read<T>(pos);
			pos += memoryStep;
		}

		return true;
	}

	public bool Write<T>(long pos, ref T buf) where T : struct
	{
		if (int.MaxValue < pos)
			throw new ArgumentOutOfRangeException("must be in the range of int", nameof(pos));

		byte[] bytes = buf switch
		{
			bool v => BitConverter.GetBytes(v),
			char v => BitConverter.GetBytes(v),

			sbyte v => new byte[1] { Convert.ToByte(v) },
			short v => BitConverter.GetBytes(v),
			int v => BitConverter.GetBytes(v),
			long v => BitConverter.GetBytes(v),

			byte v => new byte[1] { v },
			ushort v => BitConverter.GetBytes(v),
			uint v => BitConverter.GetBytes(v),
			ulong v => BitConverter.GetBytes(v),

			float v => BitConverter.GetBytes(v),
			double v => BitConverter.GetBytes(v),

			_ => throw new ArgumentException($"The type {typeof(T)} is not supported", nameof(buf))
		};

		Buffer.BlockCopy(bytes, 0, Memory, (int)pos, bytes.Length);

		return true;
	}

	public bool WriteArray<T>(long pos, T[] buf, int offset, int count) where T : struct
	{
		int memoryStep = Marshal.SizeOf<T>();

		for (int i = offset; i < buf.Length && (i - offset) < count; i++)
		{
			Write(pos, ref buf[i]);
			pos += memoryStep;
		}

		return true;
	}
}
