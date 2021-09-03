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

		[Test]
		public async Task GetTimeTest()
		{
			using CustomDataSharingManager cdsManager = new();

			cdsManager.CreateOneDataSharing<TimeSpan>(GetTimeTest_SMemName);
			for (int i = 0; i < 10; i++)
			{
				Console.WriteLine(cdsManager.TryGetValue(GetTimeTest_SMemName, out TimeSpan value) ? value.ToString() : "failed");
				await Task.Delay(250);
			}

			Assert.Pass();
		}

		[Test]
		public void SetAndGetTest([Random(10)] int testCase)
		{
			string smem_name = nameof(SetAndGetTest) + testCase.ToString();

			using CustomDataSharingManager reader = new();
			using CustomDataSharingManager writer = new();

			reader.CreateOneDataSharing<int>(smem_name);
			writer.CreateOneDataSharing<int>(smem_name);

			Assert.IsTrue(writer.TrySetValue(smem_name, testCase));

			Assert.IsTrue(reader.TryGetValue(smem_name, out int result));

			Assert.AreEqual(testCase, result);
		}
	}
}
