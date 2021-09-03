using NUnit.Framework;

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace TR
{
	public class DataConverterManagerTests
	{

		DataForConverter CreateEmptyDataForConverter(CustomDataSharingManager cdsManager) => new(cdsManager, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default, default);

		[Test]
		public Task LoadScriptsFromAssemblyTest()
		{
			using DataConverterManager manager = new();
			using CustomDataSharingManager cdsManager = new();
			manager.LoadScriptsFromAssembly(Assembly.GetExecutingAssembly());

			var testArg = CreateEmptyDataForConverter(cdsManager);

			//‚±‚±‚ÅAssert.Pass‚ª“ü‚é
			return manager.RunAsync(testArg);
		}

		const int NumberInTestScript = 12345;
		static readonly string TestScript = $"unsafe {{ Panel[0] = {NumberInTestScript}; }}";
		[Test]
		public async Task CreateActionFromScriptStringTest()
		{
			Func<DataForConverter, Task> func =  DataConverterManager.CreateActionFromScriptString(TestScript);

			IntPtr panelPtr = Marshal.AllocHGlobal(sizeof(int));

			using CustomDataSharingManager cdsManager = new();

			DataForConverter data = CreateEmptyDataForConverter(cdsManager) with { PanelIntPtr = panelPtr };

			await func.Invoke(data);

			int result = Marshal.ReadInt32(panelPtr);

			Assert.AreEqual(NumberInTestScript, result);
		}

	}

	public class TestingWithIScriptingModule : IScriptingModule
	{
		public async Task RunAsync(DataForConverter data)
		{
			await Task.Delay(1);
			Assert.Pass();
		}
	}
}