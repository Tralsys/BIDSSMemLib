#if NET20 || NET35
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace TR
{
	/// <summary>共有メモリにアクセスする際に, セマフォによる排他制御を呼び出し元で行う場合に使用するクラス</summary>
	public class SemaphorelessSMemIF : SemaphorelessSMemIF_Preprocessing
	{
		const byte TRUE_VALUE = 1;
		const byte FALSE_VALUE = 0;
		const long Capacity_Step = 4096;

		IntPtr MMF = IntPtr.Zero;//Memory Mapped File
		IntPtr MMVA = IntPtr.Zero;//Memory Mapped View Accessor (object格納場所)

		/// <summary>インスタンスを初期化します</summary>
		/// <param name="smem_name">共有メモリ空間の名前</param>
		/// <param name="capacity">共有メモリ空間のキャパシティ</param>
		public SemaphorelessSMemIF(string smem_name, long capacity) : base(smem_name, capacity)
		{
			if (capacity > uint.MaxValue)
				throw new ArgumentOutOfRangeException("NET35 || NET20モードでは, CapacityはUInt32.MaxValue以下である必要があります.");

			//最初はOpenを試行
			MMF = OpenFileMapping(FILE_MAP_ALL_ACCESS, FALSE_VALUE, SMemName);

			long newCap = (long)Math.Ceiling((float)capacity / Capacity_Step) * Capacity_Step;
			//Openできない => つくる
			if (MMF == IntPtr.Zero)
				MMF = CreateFileMappingA(unchecked((IntPtr)(int)0xFFFFFFFF), IntPtr.Zero, PAGE_READWRITE, 0, (uint)newCap, SMemName);

			//OpenもCreateもできなければException
			if (MMF == IntPtr.Zero)
				throw new FileLoadException($"SMemIF.CheckReOpen({capacity}) : Memory Mapped File ({SMemName}) のCreate/Openに失敗しました.  newCap:{newCap}");

			//Viewをつくる
			MMVA = MapViewOfFile(MMF, FILE_MAP_ALL_ACCESS, 0, 0, 0);

			//キャパシティ情報を更新
			Capacity = newCap;
		}

		/// <summary>共有メモリ空間のキャパシティ</summary>
		public override long Capacity { get; }

		/// <summary>共有メモリ空間の指定の位置から, 指定の型のデータを読み込む</summary>
		/// <typeparam name="T">読み込みたい型</typeparam>
		/// <param name="pos">読み込む位置 [bytes]</param>
		/// <param name="buf">読み込むデータ</param>
		/// <returns>読み込みに成功したかどうか  (例外は捕捉されません)</returns>
		public override bool Read<T>(long pos, out T buf) where T : struct
		{
			if (!base.Read(pos, out buf) || MMVA == IntPtr.Zero)
				return false;

			IntPtr ip_readFrom = new IntPtr(MMVA.ToInt64() + pos);//読み取り開始位置適用済みのポインタ
			buf = (T)Marshal.PtrToStructure(ip_readFrom, typeof(T));
			return true;
		}

		/// <summary>SMemから連続的に値を読み取ります</summary>
		/// <typeparam name="T">値の型</typeparam>
		/// <param name="pos">読み込む位置 [bytes]</param>
		/// <param name="buf">読み取り結果を格納する配列</param>
		/// <param name="offset">配列内で書き込みを開始する位置</param>
		/// <param name="count">読み取りを行う数</param>
		/// <returns>読み取りに成功したかどうか</returns>
		public override bool ReadArray<T>(long pos, T[] buf, int offset, int count) where T : struct
		{
			if (!base.WriteArray(pos, buf, offset, count) || MMVA == IntPtr.Zero)
				return false;

			//読み取り開始位置適用済みのポインタを取得する
			IntPtr ip_toRead = new IntPtr(MMVA.ToInt64() + pos);

			//int型配列にキャスト(iarrへの書き込みはbufにも反映される)
			if (buf is int[] iarr)
			{
				Marshal.Copy(ip_toRead, iarr, offset, count);//iarrにSMemの値を書き込み
			}
			else if(buf is double[] darr)
			{
				Marshal.Copy(ip_toRead, darr, offset, count);//darrにSMemの値を書き込み
			}
			else if(buf is float[] farr)
			{
				Marshal.Copy(ip_toRead, farr, offset, count);//farrにSMemの値を書き込み
			}
			else if (buf is bool[] barr)//bool型配列にキャスト
			{
				//SMemから読んだRAW値のキャッシュ
				//bool型は1byteなので, byte配列でなんとかなる
				byte[] SMem_ba = new byte[sizeof(bool) * count];

				//ip_toReadの初めから, SMem_ba[offset]~ count個コピーする.
				Marshal.Copy(ip_toRead, SMem_ba, 0, SMem_ba.Length);//SMemからキャッシュにコピー

				for (int i = 0; i < SMem_ba.Length; i++)
					barr[i + offset] = SMem_ba[i] == TRUE_VALUE;//読み取り結果の書き込み
			}
			else
			{
				
			}
			return true;
		}

		/// <summary>共有メモリ空間の指定の位置に指定のデータを書き込む</summary>
		/// <typeparam name="T">データの型</typeparam>
		/// <param name="pos">書き込む位置 [bytes]</param>
		/// <param name="buf">書き込むデータ</param>
		/// <returns>書き込みに成功したかどうか</returns>
		public override bool Write<T>(long pos, ref T buf) where T : struct
		{
			if (!base.Write(pos, ref buf) || MMVA == IntPtr.Zero)
				return false;

			IntPtr ip_writeTo = new IntPtr(MMVA.ToInt64() + pos);//読み取り開始位置適用済みのポインタ
			Marshal.StructureToPtr(buf, ip_writeTo, false);//予め確保してた場所にStructureを書き込む
			return true;
		}

		/// <summary>SMemに連続した値を書き込みます</summary>
		/// <typeparam name="T">書き込む値の型</typeparam>
		/// <param name="pos">書き込みを開始するSMem内の位置 [bytes]</param>
		/// <param name="buf">SMemに書き込む配列</param>
		/// <param name="offset">配列内で書き込みを開始する位置</param>
		/// <param name="count">書き込む要素数</param>
		/// <returns>書き込みに成功したかどうか</returns>
		public override bool WriteArray<T>(long pos, T[] buf, int offset, int count) where T : struct
		{
			if (!base.ReadArray(pos, buf, offset, count) || MMVA == IntPtr.Zero)
				return false;

			IntPtr ip_writeTo = new IntPtr(MMVA.ToInt64() + pos);//読み取り開始位置適用済みのポインタ
			if (buf is int[] iarr)//int型配列にキャスト(iarrへの書き込みはbufにも反映される)
			{
				Marshal.Copy(iarr, offset, ip_writeTo, count);
			}
			else if (buf is bool[] barr)
			{
				byte[] ba2w = new byte[count];//SMemに書き込む配列
				for (int i = 0; i < ba2w.Length; i++)
					ba2w[i] = barr[i + offset] ? TRUE_VALUE : FALSE_VALUE;//SMemに実際に書き込む配列に, 入力された値を書き込む

				Marshal.Copy(ba2w, 0, ip_writeTo, ba2w.Length);//SMemに書き込む
			}
			else
				throw new ArrayTypeMismatchException("NET35 || NET20モードでBuildされています.  Array操作はint/bool型のみ受け付けます.");
			return true;
		}

		#region IDisposable Support
		/// <summary>リソースの解放が完了したかどうか</summary>
		protected bool disposedValue = false;

		/// <summary>保持しているリソースを解放する</summary>
		/// <param name="disposing">Managedリソースも解放するかどうか</param>
		protected virtual void Dispose(bool disposing)
		{
			disposedValue = true;

			if (!disposedValue)
			{
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

				disposedValue = true;
			}
		}

		/// <summary>SemaphorelessSMemIFのデストラクタ</summary>
		~SemaphorelessSMemIF() => Dispose(false);

		/// <summary>保持しているリソースを解放する</summary>
		public override void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		#endregion


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
			uint dwNumberOfBytesToMap);
		[DllImport(DLL_NAME, CharSet = CharSet.Unicode)]
		static extern byte CloseHandle(IntPtr hObject);

		[DllImport(DLL_NAME, CharSet = CharSet.Unicode)]
		static extern byte UnmapViewOfFile(IntPtr lpBaseAddress);

		[DllImport(DLL_NAME, CharSet = CharSet.Unicode)]
		static extern byte FlushViewOfFile(
			IntPtr lpBaseAddress,
			uint dwNumberOfBytesToFlush);

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
	}
}

#endif