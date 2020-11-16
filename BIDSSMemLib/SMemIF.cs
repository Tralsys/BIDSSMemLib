using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Runtime.CompilerServices;
#if NET35
using System.IO;
#else
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;
#endif
//ref : https://docs.microsoft.com/ja-jp/dotnet/standard/library-guidance/cross-platform-targeting
namespace TR
{
	/// <summary>TargetFramework別にSharedMemoryを提供します.</summary>
	public class SMemIF : IDisposable
	{
		private const MethodImplOptions MIOpt = (MethodImplOptions)256;//MethodImplOptions.AggressiveInlining;
		const byte TRUE_VALUE = 1;
		const byte FALSE_VALUE = 0;
		
#if NET35//Unmanaged
		#region 関数のリンク
		const string DLL_NAME = "kernel32.dll";
		[DllImport(DLL_NAME, CharSet = CharSet.Unicode)]
		static extern IntPtr CreateFileMappingA(
			IntPtr hFile,
			IntPtr lpFileMappingAttributes,
			uint flProtect,
			uint dwMaximumSizeHigh,
			uint dwMaximumSizeLow,
			string lpName);

		[DllImport(DLL_NAME, CharSet = CharSet.Unicode)]
		static extern IntPtr OpenFileMapping(
			uint dwDesiredAccess,
			byte bInheritHandle,
			string lpName);

		[DllImport(DLL_NAME, CharSet = CharSet.Unicode)]
		static extern IntPtr MapViewOfFile(
	IntPtr hFileMappingObject,
	uint dwDesiredAccess,
	uint dwFileOffsetHigh,
	uint dwFileOffsetLow,
	uint dwNumberOfBytesToMap
);
		[DllImport(DLL_NAME, CharSet = CharSet.Unicode)]
		static extern byte CloseHandle(IntPtr hObject);

		[DllImport(DLL_NAME, CharSet = CharSet.Unicode)]
		static extern byte UnmapViewOfFile(IntPtr lpBaseAddress);

		[DllImport(DLL_NAME, CharSet = CharSet.Unicode)]
		static extern byte FlushViewOfFile(
	IntPtr lpBaseAddress,
	uint dwNumberOfBytesToFlush
);

		[DllImport(DLL_NAME, CharSet = CharSet.Unicode)]
		static extern uint GetLastError();
		#endregion

		#region FileMappingまわりの定数
		const uint ERROR_ALREADY_EXISTS = 0xB7;
		#region flProtect
		const uint PAGE_EXECUTE_READ = 0x20;
		const uint PAGE_EXECUTE_READWRITE = 0x40;
		const uint PAGE_EXECUTE_WRITECOPY = 0x80;
		const uint PAGE_READONLY = 0x02;
		const uint PAGE_READWRITE = 0x04;
		const uint PAGE_WRITECOPY = 0x08;

		const uint SEC_COMMIT = 0x8000000;
		const uint SEC_IMAGE = 0x1000000;
		const uint SEC_IMAGE_NO_EXECUTE = 0x11000000;
		const uint SEC_LARGE_PAGES = 0x80000000;
		const uint SEC_NOCACHE = 0x10000000;
		const uint SEC_RESERVE = 0x4000000;
		const uint SEC_WRITECOMBINE = 0x40000000;
		#endregion

		#region dwDesiredAccess
		const uint FILE_MAP_ALL_ACCESS = 0xf001f;
		const uint FILE_MAP_READ = 0x04;
		const uint FILE_MAP_WRITE = 0x02;
		const uint FILE_MAP_COPY = 0x01;
		//const uint FILE_MAP_EXECUTE = 0;
		//const uint FILE_MAP_LARGE_PAGES = 0;
		//const uint FILE_MAP_TARGETS_INVALID = 0;
		#endregion

		#endregion


		IntPtr MMF = IntPtr.Zero;//Memory Mapped File
		IntPtr MMVA = IntPtr.Zero;//Memory Mapped View Accessor (object格納場所)
#else
		MemoryMappedFile MMF = null;
		MemoryMappedViewAccessor MMVA = null;
#endif

		long Capacity
#if NET35
		{ get; set; } = 0;
#else
			=> MMVA?.Capacity ?? 0;
#endif

		string SMem_Name { get; }
		RWSemap semap = null;


		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public SMemIF(string SMemName, long capacity)
		{
			if (string.IsNullOrEmpty(SMemName) || capacity <= 0) Dispose();
			SMem_Name = SMemName;
			semap = new RWSemap();

			CheckReOpen(capacity);
		}

		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public bool Read<T>(long pos, out T buf) where T : struct
		{
			buf = default;
			if (pos < 0) throw new ArgumentOutOfRangeException("posに負の値は使用できません.");
			if (disposing) return false;

			CheckReOpen(pos + Marshal.SizeOf(default(T)));
			T retT = default;
			_ = semap.Read(() =>
			{
				#region SMemへの操作
#if NET35
				if (MMVA == IntPtr.Zero) return;
				retT = (T)Marshal.PtrToStructure(MMVA, typeof(T));
#else
				MMVA.Read(pos, out retT);
#endif
				#endregion
			});
			buf = retT;
			return true;
		}

		/// <summary>SMemから連続的に値を読み取ります.</summary>
		/// <typeparam name="T">値の型(NET35モードではint型/bool型のみ使用可能)</typeparam>
		/// <param name="pos">SMem内でのデータ開始位置</param>
		/// <param name="buf">読み取り結果を格納する配列</param>
		/// <param name="offset">配列内で書き込みを開始する位置</param>
		/// <param name="count">読み取りを行う数</param>
		/// <returns>読み取りに成功したかどうか</returns>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public bool ReadArray<T>(long pos, T[] buf, int offset, int count) where T : struct
		{
			if (disposing) return false;

			long neededCap = pos + Marshal.SizeOf((T)default) * (count - offset);
			CheckReOpen(neededCap);
			_ = semap.Read(() =>
				{
					#region SMemへの操作
#if NET35
					if (MMVA == IntPtr.Zero) return;
					IntPtr ip_toRead = new IntPtr(MMVA.ToInt64() + pos);//読み取り開始位置適用済みのポインタ
					if (buf is int[] iarr)//int型配列にキャスト(iarrへの書き込みはbufにも反映される)
					{
						Marshal.Copy(ip_toRead, iarr, offset, count);//iarrにSMemの値を書き込み
					}
					else if(buf is bool[] barr)//bool型配列にキャスト
					{
						byte[] SMem_ba = new byte[sizeof(bool) * count];//SMemから読んだRAW値のキャッシュ

						//ip_toReadの初めから, SMem_ba[offset]~ count個コピーする.
						Marshal.Copy(ip_toRead, SMem_ba, 0, SMem_ba.Length);//SMemからキャッシュにコピー

						for(int i = 0; i < SMem_ba.Length; i++)
							barr[i + offset] = SMem_ba[i] == TRUE_VALUE;//読み取り結果の書き込み
					}
					else throw new ArrayTypeMismatchException("NET35モードでBuildされています.  Array操作はint型/bool型のみ受け付けます.");

#else
				MMVA.ReadArray(pos, buf, offset, count);
#endif
					#endregion
				});

			return true;
		}
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public bool Write<T>(long pos, ref T buf) where T : struct
		{
			if (pos < 0) throw new ArgumentOutOfRangeException("posに負の値は使用できません.");
			if (disposing) return false;
			CheckReOpen(pos + Marshal.SizeOf(default(T)));
			T retT = buf;
			_ = semap.Write(() =>
				{
					#region SMemへの操作
#if NET35
					if (MMVA == IntPtr.Zero) return;//Viewが無効
					IntPtr ip_writeTo = new IntPtr(MMVA.ToInt64() + pos);//読み取り開始位置適用済みのポインタ
					Marshal.StructureToPtr(retT, MMVA, false);//予め確保してた場所にStructureを書き込む
#else
				MMVA.Write(pos, ref retT);
#endif
					#endregion
				});
			return true;
		}

		/// <summary>SMemに連続した値を書き込みます.</summary>
		/// <typeparam name="T">書き込む値の型 (NET35モードではint/bool型のみ使用可能)</typeparam>
		/// <param name="pos">書き込みを開始するSMem内の位置</param>
		/// <param name="buf">SMemに書き込む配列</param>
		/// <param name="offset">配列内で書き込みを開始する位置</param>
		/// <param name="count">書き込む要素数</param>
		/// <returns>書き込みに成功したかどうか</returns>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public bool WriteArray<T>(long pos, T[] buf, int offset, int count) where T : struct
		{
			if (disposing) return false;
			long neededCap = pos + Marshal.SizeOf((T)default) * (count - offset);
			CheckReOpen(neededCap);
			_ = semap.Write(() =>
				{
					#region SMemへの操作
#if NET35
					if (MMVA == IntPtr.Zero) return;
					IntPtr ip_writeTo = new IntPtr(MMVA.ToInt64() + pos);//読み取り開始位置適用済みのポインタ
					if (buf is int[] iarr)//int型配列にキャスト(iarrへの書き込みはbufにも反映される)
					{
						Marshal.Copy(iarr, offset, ip_writeTo, count);
					}
					else if(buf is bool[] barr)
					{
						byte[] ba2w = new byte[count];//SMemに書き込む配列
						for (int i = 0; i < ba2w.Length; i++)
							ba2w[i] = barr[i + offset] ? TRUE_VALUE : FALSE_VALUE;//SMemに実際に書き込む配列に, 入力された値を書き込む

						Marshal.Copy(ba2w, 0, ip_writeTo, ba2w.Length);//SMemに書き込む
					}
					else throw new ArrayTypeMismatchException("NET35モードでBuildされています.  Array操作はint/bool型のみ受け付けます.");
#else
				MMVA.WriteArray(pos, buf, offset, count);
#endif
					#endregion
				});
			return true;
		}

		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		void CheckReOpen(long capacity)
		{
			if (Capacity > capacity) return;//保持キャパが要求キャパより大きい
			_ = semap.Write(() =>
				{
					if (Capacity > capacity) return;//保持キャパが要求キャパより大きい 再確認
#if NET35
					if (MMVA != IntPtr.Zero)
					{
						UnmapViewOfFile(MMVA);
						CloseHandle(MMVA);
					}//Viewを閉じる
					if (MMF != IntPtr.Zero) CloseHandle(MMF);//FileハンドルをRelease

					if (capacity > uint.MaxValue) throw new ArgumentOutOfRangeException("NET35モードでは, CapacityはUInt32.MaxValue以下である必要があります.");

					MMF = OpenFileMapping(FILE_MAP_ALL_ACCESS, FALSE_VALUE, SMem_Name);//最初はOpenを試行
					if (MMF == IntPtr.Zero) MMF = CreateFileMappingA(unchecked((IntPtr)(int)0xFFFFFFFF), IntPtr.Zero, PAGE_READWRITE, 0, (uint)capacity, SMem_Name);//Openできない=>つくる
					if (MMF == IntPtr.Zero) throw new FileLoadException(string.Format("SMemIF.CheckReOpen({0}) : Memory Mapped File ({1}) のCreate/Openに失敗しました.", capacity, SMem_Name));//OpenもCreateもできなければException

					MMVA = MapViewOfFile(MMF, FILE_MAP_ALL_ACCESS, 0, 0, 0);
					Capacity = capacity;
#else
				MMVA?.Dispose();
					MMF?.Dispose();

					MMF = MemoryMappedFile.CreateOrOpen(SMem_Name, capacity);
					MMVA = MMF.CreateViewAccessor();
#endif
				});
		}


		#region IDisposable Support
		public bool disposing = false;
		private bool disposedValue = false;

		protected virtual void Dispose(bool disposing)
		{
			disposing = true;
			if (!disposedValue)
			{
				if (disposing)
				{
#if NET35
					if (MMVA != IntPtr.Zero)
					{
						UnmapViewOfFile(MMVA);
						CloseHandle(MMVA);
						MMVA = IntPtr.Zero;
					}
					if (MMF != IntPtr.Zero)
					{
						CloseHandle(MMF);
						MMF = IntPtr.Zero;
					}
#else
					MMVA?.Dispose();
					MMF?.Dispose();

					MMVA = null;
					MMF = null;
#endif
					semap.Dispose();
					semap = null;

					disposedValue = true;
				}
			}
		}
#if NET35
		/// <summary>SMemのハンドルを閉じます.</summary>
		~SMemIF() => Dispose(false);
#endif
		public void Dispose() => Dispose(true);
#endregion

	}

	/// <summary>await可能なR/Wロックを提供する</summary>
	public class RWSemap : IDisposable
	{
		private const MethodImplOptions MIOpt = (MethodImplOptions)256;//MethodImplOptions.AggressiveInlining;
		private long WAIT_TICK = 1;//別モード動作中に, モード復帰をチェックする間隔[tick]
		private int Reading = 0;//Read操作中のActionの数 (Interlockedで操作を行う)
		private int Want_to_Write = 0;//Write操作待機中, あるいは実行中のActionの数 (Interlockedで操作を行う)

		private object LockObj = new object();//Writeロックを管理するobject

		/// <summary>リソースを解放します(予定)</summary>
		public void Dispose() { }

		/// <summary>Writeロックを行ったうえで, 指定の読み取り操作を行います</summary>
		/// <param name="act">読み取り操作</param>
		/// <returns>成功したかどうか</returns>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public
#if NET35
			bool
#else
			async Task<bool>
#endif
			Read(Action act)
		{
			while (Want_to_Write > 0)//Writeロック取得待機
			{
#if !NET35
				await
#endif
				Delay(TimeSpan.FromTicks(WAIT_TICK));
			}
			try
			{
				Interlocked.Increment(ref Reading);
				act?.Invoke();
			}
			finally
			{
				Interlocked.Decrement(ref Reading);
			}
			return true;
		}

		/// <summary>Readロックを行ったうえで, 指定の書き込み操作を実行します</summary>
		/// <param name="act">書き込み操作</param>
		/// <returns>成功したかどうか</returns>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public
#if NET35
			bool
#else
			async Task<bool>
#endif
	Write(Action act)
		{
			try
			{
				Interlocked.Increment(ref Want_to_Write);//Write待機
				while (Reading > 0)//Read完了待機
				{
#if !NET35
				await
#endif
					Delay(TimeSpan.FromTicks(WAIT_TICK));

				}
				lock (LockObj)//Writeロック
				{
					act?.Invoke();
				}
			}
			finally
			{
				Interlocked.Decrement(ref Want_to_Write);//Write完了処理
			}
			return true;
		}

#if NET35
		static private void Delay(TimeSpan ts) => Thread.Sleep(ts);
#else
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		static private async Task Delay(TimeSpan ts) => await Task.Delay(ts);
#endif

	}
}
