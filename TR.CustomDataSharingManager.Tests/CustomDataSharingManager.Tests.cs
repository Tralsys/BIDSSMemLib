using NUnit.Framework;

using System;
using System.Threading.Tasks;

namespace TR
{
	public class CustomDataSharingManagerTests
	{

#if false
//SampleScript.csx
const string SMemName = "SampleDataSharing";

DataSharingManager.CreateOneDataSharing<TimeSpan>(SMemName);

DataSharingManager.TrySetValue<TimeSpan>(SMemName, Time);
#endif

		const string GetTimeTest_SMemName = "SampleDataSharing";

		class SampleClass
		{
			public int Value;

			public SampleClass() { }
			public SampleClass(int Value)
			{
				this.Value = Value;
			}
		}

		[Test]
		public void SetAndGetTest([Range(0, 9)] int Seed)
		{
			int testCase = new Random(Seed).Next();
			string smem_name = nameof(SetAndGetTest) + testCase.ToString();

			using CustomDataSharingManager reader = new();
			using CustomDataSharingManager writer = new();

			reader.CreateDataSharing<SampleClass>(smem_name);
			writer.CreateDataSharing<SampleClass>(smem_name);

			Assert.IsTrue(writer.TrySetValue(smem_name, new SampleClass(testCase)));

			Assert.IsTrue(reader.TryGetValue(smem_name, out SampleClass result));

			Assert.AreEqual(testCase, result.Value);
		}
	}
}
