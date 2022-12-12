using System;

namespace TR.BIDSSMemLib.RandomWriter;

public class RandomValueGenerator
{
	Random Rand { get; }

	public RandomValueGenerator() : this(new Random()) { }
	public RandomValueGenerator(int Seed) : this(new Random(Seed)) { }
	public RandomValueGenerator(Random random)
	{
		Rand = random;
	}

	public BIDSSharedMemoryData GetBSMD()
		=> new()
		{
			HandleData = GetHand(),

			IsDoorClosed = GetBool(),

			IsEnabled = true,

			SpecData = GetSpec(),

			StateData = GetState(),

			VersionNum = Rand.Next(),
		};

	public Hand GetHand()
		=> new()
		{
			B = GetUInt16(),
			P = GetInt32(),
			C = GetInt32(),
			R = GetInt8() switch
			{
				> 0 => 1,
				0 => 0,
				< 0 => -1
			},
		};

	public Spec GetSpec()
		=> new()
		{
			A = GetInt32(),
			B = GetInt32(),
			C = GetInt32(),
			J = GetInt32(),
			P = GetInt32(),
		};

	public State GetState()
		=> new()
		{
			BC = Rand.NextSingle(),
			BP = Rand.NextSingle(),
			ER = Rand.NextSingle(),
			I = GetFloat32(),
			MR = Rand.NextSingle(),
			SAP = Rand.NextSingle(),
			T = Rand.Next(),
			V = GetFloat32(),
			Z = GetFloat64(),
		};

	public int[] GetIntArray()
		=> GetIntArray(256);

	public int[] GetIntArray(int length)
	{
		int[] array = new int[length];

		for (int i = 0; i < array.Length; i++)
			array[i] = GetInt32();

		return array;
	}

	public bool GetBool()
		=> Rand.Next(2) == 1;

	public float GetFloat32()
		=> Rand.NextSingle() * GetSign();
	public double GetFloat64()
		=> Rand.NextDouble() * GetSign();

	public sbyte GetSign()
		=> GetBool() ? (sbyte)1 : (sbyte)-1;

	public sbyte GetInt8()
		=> (sbyte)Rand.Next(sbyte.MinValue, (int)sbyte.MaxValue + 1);
	public short GetInt16()
		=> (short)Rand.Next(short.MinValue, (int)short.MaxValue + 1);
	public int GetInt32()
		=> GetBool() ? Rand.Next(int.MinValue, 0) : Rand.Next();
	public long GetInt64()
		=> GetBool() ? Rand.NextInt64(long.MinValue, 0) : Rand.NextInt64();

	public byte GetUInt8()
	=> (byte)Rand.Next(byte.MaxValue);
	public ushort GetUInt16()
		=> (ushort)Rand.Next(ushort.MaxValue);
	public uint GetUInt32()
		=> (uint)Rand.NextInt64(uint.MaxValue);
	public ulong GetUInt64()
		=> (((ulong)Rand.NextInt64()) << 1) + (GetBool() ? 1u : 0);
}
