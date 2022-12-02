using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace TR
{
	public static class Ats
	{
		static string CurrentDllPath { get; } = Assembly.GetExecutingAssembly().Location;
		static string CurrentDllDirectory { get; } = Path.GetDirectoryName(CurrentDllPath) ?? string.Empty;
		static string CurrentDllFileNameWithoutExtension { get; } = Path.GetFileNameWithoutExtension(CurrentDllPath);
		static string ScriptsDirectoryName { get; } = "scripts";
		static string ScriptsDirectoryPath { get; } = Path.Combine(CurrentDllDirectory, CurrentDllFileNameWithoutExtension, ScriptsDirectoryName);

		[DllImport("kernel32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool AllocConsole();

		static Ats()
		{
#if DEBUG
			if (!Debugger.IsAttached)
				Debugger.Launch();

			AllocConsole();
#endif

			AppDomain.CurrentDomain.AssemblyResolve += (s, arg) =>
			{
				if (arg.Name is null)
					return null;

				AssemblyName asmName = new(arg.Name);

				string targetDllFileName = asmName.Name + ".dll";

				//まずは実行中のコードが格納されたdllと同じディレクトリで検索
				string path = Path.Combine(CurrentDllDirectory, targetDllFileName);
				bool isExist = File.Exists(path);

				//存在しなければ, DLL名と同名のフォルダの直下を検索する
				if (!isExist)
				{
					//CurrentDllと同じ名前を持つディレクトリの下にないか確認する
					path = Path.Combine(CurrentDllDirectory, CurrentDllFileNameWithoutExtension, targetDllFileName);

					//DLLが存在するか確認する
					isExist = File.Exists(path);
				}

				//それでも見つからなければ, このDLLが存在するディレクトリの子ディレクトリも含めてすべて検索する
				//セキュリティ的にかなり危ないので, デフォルトでは有効にしない
#if SEARCH_ALL_CHILD_DIRECTORIES
				if (!isExist)
				{
					string[]? dirs = Directory.GetDirectories(CurrentDllPath, "*", SearchOption.AllDirectories);

					if (dirs?.Length > 0)
						foreach(string dir in dirs)
						{
							isExist = File.Exists(Path.Combine(dir, targetDllFileName));

							if (isExist)
							{
								//見つかったらパスを保存する
								path = Path.Combine(dir, targetDllFileName);
								break;
							}
						}
				}
#endif

				//ファイルが見つかればそれをロードし, 見つからなければnullを返す
				return isExist ? Assembly.LoadFrom(path) : null;
			};
		}

		static DataConverterManager? DataConverterManager { get; set; } = null;
		static DataForConverter? Data { get; set; } = null;

		public static uint VersionNum = 0x00020000;

		[DllExport(CallingConvention = CallingConvention.StdCall)]
		public static void Load()
		{
			if (!Directory.Exists(ScriptsDirectoryPath))
				return;

			//指定のディレクトリ以下にあるファイルをすべて取得する
			string[]? sarr = Directory.GetFiles(ScriptsDirectoryPath, "*", SearchOption.AllDirectories);

			//ファイルが存在しない場合は実行しない
			if (sarr?.Length > 0)
			{
				DataConverterManager = new DataConverterManager();
				try
				{
					DataConverterManager.LoadScriptsFromFilePathArray(sarr);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Exception from {nameof(CustomDataSharingManager)}.{nameof(Load)} Method (Load Scripts from File Path Array)");
					Console.WriteLine(ex);
					return;
				}
			}
			else
				return;

			Data = new DataForConverter(
				new Dictionary<string, object>(),
				new CustomDataSharingManager(),
				new UnmanagedArray(IntPtr.Zero),
				new UnmanagedArray(IntPtr.Zero),
				default,
				default,
				default,
				default,
				default,
				default,
				default,
				default,
				default,
				default,
				default,
				default,
				default,
				default,
				default,
				default,
				default,
				default
			);
		}

		[DllExport(CallingConvention = CallingConvention.StdCall)]
		public static void Dispose()
		{
			//まずはスクリプト群が実行されないようにする
			DataConverterManager?.Dispose();
			DataConverterManager = null;

			//次に, 共有メモリ群を解放する
			Data?.DataSharingManager.Dispose();

			Data = null;
		}

		[DllExport(CallingConvention = CallingConvention.StdCall)]
		public static void SetVehicleSpec(Spec s)
		{
			if (Data is DataForConverter v)
				Data = v with { CarBrakeNotchCount = s.B, CarPowerNotchCount = s.P, CarATSCheckPos = s.A, CarB67Pos = s.J, CarCount = s.C };
		}

		[DllExport(CallingConvention = CallingConvention.StdCall)]
		public static void Initialize(int s) { }

		static Hand returnHand = new();

		[DllExport(CallingConvention = CallingConvention.StdCall)]
		public static Hand Elapse(State s, IntPtr Pa, IntPtr So)
		{
			if(Data is DataForConverter v)
			{
				Data = v with
				{
					Panel = new UnmanagedArray(Pa),
					Sound = new UnmanagedArray(So),
					BCPressure = s.BC,
					BPPressure = s.BP,
					Current = s.I,
					ERPressure = s.ER,
					MRPressure = s.MR,
					Location = s.Z,
					SAPPressure = s.SAP,
					Speed = s.V,
					Time = TimeSpan.FromMilliseconds(s.T)
				};

				DataConverterManager?.RunAsync(Data).Wait();
			}

			return returnHand;
		}

		[DllExport(CallingConvention = CallingConvention.StdCall)]
		public static void SetPower(int p)
		{
			returnHand.P = p;

			if (Data is DataForConverter v)
				Data = v with
				{
					CurrentPowerPos = p
				};
		}

		[DllExport(CallingConvention = CallingConvention.StdCall)]
		public static void SetBrake(int b)
		{
			returnHand.B = b;

			if (Data is DataForConverter v)
				Data = v with
				{
					CurrentBrakePos = b
				};
		}

		[DllExport(CallingConvention = CallingConvention.StdCall)]
		public static void SetReverser(int r)
		{
			returnHand.R = r;

			if (Data is DataForConverter v)
				Data = v with
				{
					CurrentReverserPos = r
				};
		}

		[DllExport(CallingConvention = CallingConvention.StdCall)]
		public static void KeyDown(int _) { }

		[DllExport(CallingConvention = CallingConvention.StdCall)]
		public static void KeyUp(int _) { }

		[DllExport(CallingConvention = CallingConvention.StdCall)]
		public static void HornBlow(int _) { }

		[DllExport(CallingConvention = CallingConvention.StdCall)]
		public static void DoorOpen()
		{
			if (Data is DataForConverter v)
				Data = v with
				{
					IsDoorClosed = false
				};
		}

		[DllExport(CallingConvention = CallingConvention.StdCall)]
		public static void DoorClose()
		{
			if (Data is DataForConverter v)
				Data = v with
				{
					IsDoorClosed = true
				};
		}

		[DllExport(CallingConvention = CallingConvention.StdCall)]
		public static void SetSignal(int _) { }

		[DllExport(CallingConvention = CallingConvention.StdCall)]
		public static void SetBeaconData(Beacon _) { }

		[DllExport(CallingConvention = CallingConvention.StdCall)]
		public static uint GetPluginVersion() => VersionNum;


		/// <summary>車両のスペック</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct Spec
		{
			/// <summary>ブレーキ段数</summary>
			public int B;
			/// <summary>ノッチ段数</summary>
			public int P;
			/// <summary>ATS確認段数</summary>
			public int A;
			/// <summary>常用最大段数</summary>
			public int J;
			/// <summary>編成車両数</summary>
			public int C;
		};
		/// <summary>車両の状態</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct State
		{
			/// <summary>列車位置[m]</summary>
			public double Z;
			/// <summary>列車速度[km/h]</summary>
			public float V;
			/// <summary>0時からの経過時間[ms]</summary>
			public int T;
			/// <summary>BC圧力[kPa]</summary>
			public float BC;
			/// <summary>MR圧力[kPa]</summary>
			public float MR;
			/// <summary>ER圧力[kPa]</summary>
			public float ER;
			/// <summary>BP圧力[kPa]</summary>
			public float BP;
			/// <summary>SAP圧力[kPa]</summary>
			public float SAP;
			/// <summary>電流[A]</summary>
			public float I;
		};
		/// <summary>車両のハンドル位置</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct Hand
		{
			/// <summary>ブレーキハンドル位置</summary>
			public int B;
			/// <summary>ノッチハンドル位置</summary>
			public int P;
			/// <summary>レバーサーハンドル位置</summary>
			public int R;
			/// <summary>定速制御状態</summary>
			public int C;
		};
		/// <summary>Beaconに関する構造体</summary>
		[StructLayout(LayoutKind.Sequential)]
		public struct Beacon
		{
			/// <summary>Beaconの番号</summary>
			public int Num;
			/// <summary>対応する閉塞の現示番号</summary>
			public int Sig;
			/// <summary>対応する閉塞までの距離[m]</summary>
			public float Z;
			/// <summary>Beaconの第三引数の値</summary>
			public int Data;
		};

	}

}
