using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TR
{
	public class DataConverterManager : IDisposable
	{
		static readonly string CurrentDllLocation = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) ?? string.Empty;

		static ScriptOptions UsingScriptOptions { get; }
		static string[] ScriptsImports { get; } = new string[]
		{
			"System",
			"System.IO",
			"System.Collections.Generic"
		};

		static IReadOnlyList<string> ScriptsExtensions { get; } = new List<string>()
		{
			".cs", ".cscript", ".csx"
		};

		private List<Func<DataForConverter, Task>> Runners { get; } = new();

		static DataConverterManager()
		{
			UsingScriptOptions = ScriptOptions.Default.WithAllowUnsafe(true).WithImports(ScriptsImports);
		}

		public void LoadScripts(in string[] scriptFilePathArr)
		{
			foreach(var path in scriptFilePathArr)
			{
				string extension = Path.GetExtension(path);
				if(ScriptsExtensions.Contains(extension))
				{
					using StreamReader reader = new StreamReader(path);
					Runners.Add(GetActionFromScriptString(reader.ReadToEnd()));
				}
				else
				{
					//IScriptingModuleを実装したクラスをロードする
				}
			}
		}


		public static Func<DataForConverter, Task> GetActionFromScriptString(in string scriptString)
		{
			var scriptRunner = CSharpScript.Create(scriptString, UsingScriptOptions, typeof(DataForConverter));

			return (value) => scriptRunner.RunAsync(value);
		}

		#region IDisposable Support
		private bool disposedValue;
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					Runners.Clear();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
