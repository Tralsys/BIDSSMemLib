using NUnit.Framework;

using System;
using System.Collections.Generic;

namespace TR
{
	public class ArrayDataSMemCtrler_RWInObjTests
	{
		const int ARRLEN_MAX = 0x400;
		const int ARRLEN_DOUBLE_MAX = 0x200;
		const int ARRLEN_CUSTOMSTRUCT_MAX = 0xA9;
		const int ARRLEN_MANY_CUSTOMSTRUCT_MAX = 0x400;
		const int TESTCASE_COUNT = 10;
		static readonly Random random = new();
		static int rand_int => random.Next();
		static double rand_double => random.NextDouble();

		[Parallelizable]
		[Test]
		public void IntListRWTest([Range(1, TESTCASE_COUNT)] int randSeed)
		{
			int count = new Random(randSeed).Next(ARRLEN_MAX);
			string smem_name = $"{nameof(IntListRWTest)}_{rand_int}";

			RunTest(smem_name, count, i => i);
		}

		[Parallelizable]
		[Test]
		public void DoubleListRWTest([Range(1, TESTCASE_COUNT)] int randSeed)
		{
			int count = new Random(randSeed).Next(ARRLEN_MAX);
			string smem_name = $"{nameof(DoubleListRWTest)}_{rand_int}";

			RunTest(smem_name, count, i => rand_double);
		}

		public struct CustomStruct
		{
			public float A;
			public int B;
			public double C;
			public bool D;

			public override string ToString()
				=> $"A(float):{A},\tB(int):{B},\tC(double):{C},\tD(bool):{D}";
		}

		[Parallelizable]
		[Test]
		public void CustomStructListRWTest([Range(1, TESTCASE_COUNT)] int randSeed)
		{
			int count = new Random(randSeed).Next(ARRLEN_MAX);
			string smem_name = $"{nameof(CustomStructListRWTest)}_{count}";

			RunTest(smem_name, count, _ => new CustomStruct()
			{
				A = (float)rand_double,
				B = rand_int,
				C = rand_double,
				D = (rand_int % 2) == 0
			});
		}

#if NET5_0_OR_GREATER || NETCOREAPP
		[Test, Parallelizable]
		public void ManyCustomStructListRWTest([Range(1, TESTCASE_COUNT)] int randSeed)
		{
			int count = new Random(randSeed).Next(ARRLEN_MAX);
			string smem_name = $"{nameof(ManyCustomStructListRWTest)}_{count}";

			RunTest(smem_name, count, _ => new CustomStruct()
			{
				A = (float)rand_double,
				B = rand_int,
				C = rand_double,
				D = (rand_int % 2) == 0
			});
		}
#endif

		private static void RunTest<T>(string smem_name, int count, Func<int, T> generator) where T : struct
		{
			IReadWriteInObject? reader = null;
			IReadWriteInObject? writer = null;

			List<T> value = new();
			for (int i = 0; i < count; i++)
				value.Add(generator.Invoke(i));

			try
			{
				reader = new ArrayDataSMemCtrler<T>(smem_name, false, true, count);
				writer = new ArrayDataSMemCtrler<T>(smem_name, false, true, count);

				writer.WriteInObject(value);
				object result = reader.ReadInObject();
				CollectionAssert.AreEqual(value, result as System.Collections.IEnumerable);
			}
			finally
			{
				reader?.Dispose();
				writer?.Dispose();
			}
		}
	}
}
