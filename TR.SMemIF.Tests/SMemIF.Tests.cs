using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TR
{
	public class SMemIFTests
	{
		static readonly long CreateDisposeTest_Capacity = 16;
		static readonly long OneDataReadWriteTest_Capacity = 8;

		static readonly long IntDataRandomRWTest_Capacity = 8;
		static readonly int IntDataRandomRWTest_MaxPos = 0x10000;
		static readonly int IntDataRandomRWTest_Count = 10000;

		static int random_int => new Random().Next();
		static double random_double => new Random().NextDouble();

		static IEnumerable<TestCaseData> IntArgCases_0_32
		{
			get
			{
				for (int i = 0; i < 32; i++)
					yield return new TestCaseData(i);
			}
		}

		/// <summary>�P����SMem�̍쐬�Ɖ�����s���e�X�g</summary>
		[Test]
		public void CreateDisposeTest()
		{
			SMemIF target = new(nameof(CreateDisposeTest), CreateDisposeTest_Capacity);
			target.Dispose();
			Assert.Pass();
		}

		#region OneDataReadWriteTests
		/// <summary>int�^�̃f�[�^��RW����e�X�g</summary>
		/// <param name="pos">�������݂��J�n����ʒu [�o�C�g��]</param>
		[TestCaseSource(nameof(IntArgCases_0_32))]
		public void IntDataReadWriteTest(int pos)
		{
			string smem_name = $"{nameof(IntDataReadWriteTest)}_{random_int}";
			var test_data = random_int;

			OneDataRWTest(smem_name, pos, test_data);
		}

		/// <summary>double�^�̃f�[�^��RW����e�X�g</summary>
		/// <param name="pos">�������݂��J�n����ʒu [�o�C�g��]</param>
		[TestCaseSource(nameof(IntArgCases_0_32))]
		public void DoubleDataReadWriteTest(int pos)
		{
			string smem_name = $"{nameof(DoubleDataReadWriteTest)}_{random_int}";
			var test_data = random_double;

			OneDataRWTest(smem_name, pos, test_data);
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

		/// <summary>�C�ӂ̍\���̌^�̃f�[�^��RW����e�X�g</summary>
		/// <param name="pos">�������݂��J�n����ʒu [�o�C�g��]</param>
		[TestCaseSource(nameof(IntArgCases_0_32))]
		public void CustomStructDataReadWriteTest(int pos)
		{
			string smem_name = $"{nameof(CustomStructDataReadWriteTest)}_{random_int}";
			CustomStruct test_data = new()
			{
				A = (float)random_double,
				B = random_int,
				C = random_double,
				D = (random_int % 2) == 0
			};

			OneDataRWTest(smem_name, pos, test_data);
		}

		private static void OneDataRWTest<T>(string smem_name, long pos, T test_data) where T : struct
		{
			TestContext.WriteLine(
				$"smem_name:\t{smem_name}\n" +
				$"      pos:\t{pos}\n" +
				$"test_data:\t{test_data}"); //���s���e�m�F�p�̏o��

			using SMemIF target_reader = new(smem_name, OneDataReadWriteTest_Capacity);
			using SMemIF target_writer = new(smem_name, OneDataReadWriteTest_Capacity);

			//�������݂ɐ������Ă��邩�ǂ���
			Assert.IsTrue(target_writer.Write(pos, ref test_data));

			//�ǂݍ��݂ɐ������Ă��邩�ǂ���
			Assert.IsTrue(target_reader.Read(pos, out T result));

			//�ǂݍ��񂾃f�[�^���������񂾃f�[�^�ƈ�v���Ă��邩�ǂ���
			Assert.AreEqual(test_data, result);
		}
		#endregion

		#region ArrayDataReadWriteTests
		[Test]
		public void IntArrReadWriteTest([Random(1, 0x1000, 10)] int test_data_len)
		{
			string smem_name = $"{nameof(IntArrReadWriteTest)}_{random_int}";
			var test_data = new int[test_data_len];

			for(int i = 0; i < test_data.Length; i++)
				test_data[i] = new Random().Next();

			ArrDataRWTest(smem_name, 0, test_data);
		}

		[Test]
		public void DoubleArrReadWriteTest([Random(1, 0x1000, 10)] int test_data_len)
		{
			string smem_name = $"{nameof(DoubleArrReadWriteTest)}_{random_int}";
			var test_data = new double[test_data_len];

			for(int i = 0; i < test_data.Length; i++)
				test_data[i] = new Random().NextDouble();

			ArrDataRWTest(smem_name, 0, test_data);
		}

		[Test]
		public void CustomStructArrReadWriteTest([Random(1, 0x1000, 10)] int test_data_len)
		{
			string smem_name = $"{nameof(CustomStructArrReadWriteTest)}_{random_int}";
			var test_data = new CustomStruct[test_data_len];

			for (int i = 0; i < test_data.Length; i++)
				test_data[i] = new()
				{
					A = (float)random_double,
					B = random_int,
					C = random_double,
					D = (random_int % 2) == 0
				};

			ArrDataRWTest(smem_name, 0, test_data);
		}

		private static void ArrDataRWTest<T>(string smem_name, long pos, T[] test_data) where T : struct
		{
			TestContext.WriteLine(
				$"smem_name:\t{smem_name}\n" +
				$"      pos:\t{pos}\n" +
				$"   length:\t{test_data.Length}"); //���s���e�m�F�p�̏o��

			long Capacity_Request = Marshal.SizeOf(default(T)) * test_data.Length;
			using SMemIF target_reader = new(smem_name, Capacity_Request);
			using SMemIF target_writer = new(smem_name, Capacity_Request);

			//�������݂ɐ������Ă��邩�ǂ���
			Assert.IsTrue(target_writer.WriteArray(pos, test_data, 0, test_data.Length));

			var buf = new T[test_data.Length];
			//�ǂݍ��݂ɐ������Ă��邩�ǂ���
			Assert.IsTrue(target_reader.ReadArray(pos, buf, 0, buf.Length));

			//�ǂݍ��񂾃f�[�^���������񂾃f�[�^�ƈ�v���Ă��邩�ǂ���
			CollectionAssert.AreEqual(test_data, buf);
		}
		#endregion

		[Test]
		public void IntDataRandomRWTest()
		{
			string smem_name = $"{nameof(IntDataReadWriteTest)}_{random_int}";

			//Reader�͂�����Capacity��ݒ肵�Ȃ���, �f�[�^���傫�������Ƃ�(1000���ڈȏキ�炢)�ɐ����Read�ł��Ȃ��Ȃ�
			long Capacity_Request = sizeof(int) * (IntDataRandomRWTest_MaxPos + 1);
			using SMemIF target_reader = new(smem_name, Capacity_Request);
			using SMemIF target_writer = new(smem_name, Capacity_Request);

			for (int i = 0; i < IntDataRandomRWTest_Count; i++)
			{
				long pos = new Random().Next(IntDataRandomRWTest_MaxPos);
				var test_data = random_int;

				//�������݂ɐ������Ă��邩�ǂ���
				Assert.IsTrue(target_writer.Write(pos, ref test_data));

				//�ǂݍ��݂ɐ������Ă��邩�ǂ���
				Assert.IsTrue(target_reader.Read(pos, out int result));

				try
				{
					//�ǂݍ��񂾃f�[�^���������񂾃f�[�^�ƈ�v���Ă��邩�ǂ���
					Assert.AreEqual(test_data, result);
				}
				catch (AssertionException)
				{
					TestContext.WriteLine($"i:{i}, pos:{pos}");
					throw;
				}
			}
		}

	}
}