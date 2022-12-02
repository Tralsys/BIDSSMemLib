using NUnit.Framework;

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TR
{
	public class DataConverterManagerTests
	{

		DataForConverter CreateEmptyDataForConverter(CustomDataSharingManager cdsManager) => new(new(), cdsManager, new(IntPtr.Zero), new(IntPtr.Zero), default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default);

		[Test]
		public async Task LoadScriptsFromAssemblyTest()
		{
			using DataConverterManager manager = new();
			using CustomDataSharingManager cdsManager = new();
			IntPtr panelPtr = Marshal.AllocHGlobal(sizeof(int));

			manager.LoadScriptsFromAssembly(Assembly.GetExecutingAssembly());

			var testArg = CreateEmptyDataForConverter(cdsManager) with { Panel = new(panelPtr) };

			//ここでAssert.Passが入る
			await manager.RunAsync(testArg);

			Assert.AreEqual(TestingWithIScriptingModule.TestData, testArg.Panel[0]);
			Assert.AreEqual(TestingWithIScriptingModule.TestData, Marshal.ReadInt32(panelPtr));

			Marshal.FreeHGlobal(panelPtr);
		}

		const int NumberInTestScript = 12345;
		static readonly string TestScript = $"Panel[0] = {NumberInTestScript};";
		[Test]
		public async Task CreateActionFromScriptStringTest()
		{
			Func<DataForConverter, Task>? func = DataConverterManager.CreateActionFromScriptString(TestScript);
			Assert.That(func, Is.Not.Null);
			if (func is null)
				return;

			IntPtr panelPtr = Marshal.AllocHGlobal(sizeof(int));

			try
			{
				using CustomDataSharingManager cdsManager = new();

				DataForConverter data = CreateEmptyDataForConverter(cdsManager) with { Panel = new(panelPtr) };

				await func.Invoke(data);

				int result = Marshal.ReadInt32(panelPtr);

				Assert.AreEqual(NumberInTestScript, result);
			}
			finally
			{
				Marshal.FreeHGlobal(panelPtr);
			}
		}
	}

	public class TestingWithIScriptingModule : IScriptingModule
	{
		public const int TestData = 12345;
		public async Task RunAsync(DataForConverter data)
		{
			await Task.Delay(1);

			data.Panel[0] = TestData;
		}
	}
}