﻿using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.Scripting.Hosting;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TR
{
	public class DataConverterManager : IDisposable, IScriptingModule
	{
		public static readonly string CurrentDllLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

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
		static IReadOnlyList<string> AssemblyExtensions { get; } = new List<string>()
		{
			".dll"
		};

		private List<Func<DataForConverter, Task>> Runners { get; } = new();

		static DataConverterManager()
		{
			UsingScriptOptions = ScriptOptions.Default
				.WithAllowUnsafe(true)
				.WithImports(ScriptsImports)
				.AddReferences(Assembly.GetAssembly(typeof(SMemCtrler<int>)), Assembly.GetAssembly(typeof(CustomDataSharingManager)))
				.WithFileEncoding(System.Text.Encoding.UTF8);
		}


		public Task RunAsync(DataForConverter data)
		{
			List<Task> taskList = new();

			foreach (var i in Runners)
				taskList.Add(i.Invoke(data));

			return Task.WhenAll(taskList);
		}

		public void LoadScriptsFromFilePathArray(in string[] scriptFilePathArr)
		{
			foreach(var path in scriptFilePathArr)
			{
				string extension = Path.GetExtension(path);

				//相対パスは絶対パスに変換する
				string nPath = path;
				if (!Path.IsPathRooted(nPath))
					nPath = CurrentDllLocation + nPath;

				if(ScriptsExtensions.Contains(extension))
				{
					using StreamReader reader = new(nPath);
					var runner = CreateActionFromScriptString(reader.ReadToEnd(), nPath);
					if (runner is not null)
						Runners.Add(runner);
				}
				else if(AssemblyExtensions.Contains(extension))
				{
					//IScriptingModuleを実装したクラスをロードする
					Assembly asm = Assembly.LoadFrom(nPath);

					LoadScriptsFromAssembly(asm);
				}
			}
		}
		public void LoadScriptsFromAssembly(in Assembly asm)
		{
			foreach (var type in asm.GetTypes())
				CheckIScriptingModuleAndAddToRunners(type);
		}

		public void CheckIScriptingModuleAndAddToRunners(in Type type)
		{
			//IScriptingModuleインターフェイスを実装しているか
			//実装しているなら引数なしコンストラクタを実行し, 本当にIScriptingModuleにキャストできるか確認
			if (typeof(IScriptingModule).IsAssignableFrom(type)
				&& type.GetConstructor(Array.Empty<Type>())?.Invoke(Array.Empty<object>()) is IScriptingModule scriptingModule)
				//キャスト出来たら, それに含まれるメソッドをAdd
				Runners.Add(scriptingModule.RunAsync);
		}


		public static Func<DataForConverter, Task>? CreateActionFromScriptString(in string scriptString)
			=> CreateActionFromScriptString(scriptString, UsingScriptOptions);
		public static Func<DataForConverter, Task>? CreateActionFromScriptString(in string scriptString, in string scriptFilePath)
			=> CreateActionFromScriptString(scriptString, UsingScriptOptions.WithFilePath(scriptFilePath));
		public static Func<DataForConverter, Task>? CreateActionFromScriptString(in string scriptString, ScriptOptions options)
		{
			Console.WriteLine($"{nameof(DataConverterManager)}.{nameof(CreateActionFromScriptString)} : Loading Script from `{options.FilePath}`");

			var scriptRunner = CSharpScript.Create(
				scriptString,
				options,
				typeof(DataForConverter),
				new InteractiveAssemblyLoader(new MetadataShadowCopyProvider(
					Path.GetDirectoryName(options.FilePath)
				)));

			//var compileResults = scriptRunner.Compile();
			//foreach (var i in compileResults)
			//	if (i.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error)
			//		return null;

			try {
				var createdDelegate = scriptRunner.CreateDelegate();
				if (createdDelegate is not null)
					return (value) => createdDelegate(value);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"{nameof(DataConverterManager)}.{nameof(CreateActionFromScriptString)} : Exception has thrown on `CreateDelegate` step");
				Console.WriteLine(ex);
			}

			return null;
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
