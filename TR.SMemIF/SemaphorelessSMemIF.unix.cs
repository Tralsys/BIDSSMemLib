#if NETSTANDARD || NETCOREAPP
using System;
using System.IO;
using System.IO.MemoryMappedFiles;

namespace TR
{
	/// <summary>共有メモリにアクセスする際に, セマフォによる排他制御を呼び出し元で行う場合に使用するクラス</summary>
	public class SemaphorelessSMemIF_UNIX : SemaphorelessSMemIF_Preprocessing
	{
		const long Capacity_Step = 4096;
		const long CanWriteInOneTime_Bytes = 4096;

		/// <summary>
		/// Shared Memory Fileが配置されたディレクトリ
		/// </summary>
		public static string DefaultSMemDirectory { get; set; } = Path.Combine(Path.GetTempPath(), "TR.SMemIF.SharedMemory");

		/// <summary>
		/// Shared Memory Fileへのパス
		/// </summary>
		public string PathToSMemFile { get; } = "";

		/// <summary>
		/// Shared Memoryの読み書きに使用するキャパシティ
		/// </summary>
		public override long Capacity { get; }

		static long calcNewCapacity(in long inputCapacity)
			=> (long)Math.Ceiling((float)inputCapacity / Capacity_Step) * Capacity_Step;

		/// <summary>インスタンスを初期化します</summary>
		/// <param name="smem_name">共有メモリ空間の名前</param>
		/// <param name="capacity">共有メモリ空間のキャパシティ</param>
		public SemaphorelessSMemIF_UNIX(string smem_name, long capacity) : base(smem_name, capacity)
		{
			Capacity = calcNewCapacity(capacity);

			if (int.MaxValue < Capacity)
				throw new ArgumentOutOfRangeException(nameof(capacity), "cannot use more than int.MaxValue");

			string dirPath = DefaultSMemDirectory;
			if (!Directory.Exists(dirPath))
				Directory.CreateDirectory(dirPath);

			PathToSMemFile = Path.Combine(dirPath, smem_name);

			if (!File.Exists(PathToSMemFile))
			{
				using var _ = File.Create(PathToSMemFile, (int)Capacity);
				IsNewlyCreated = true;
			}
		}

		/// <summary>共有メモリ空間の指定の位置から, 指定の型のデータを読み込む</summary>
		/// <typeparam name="T">読み込みたい型</typeparam>
		/// <param name="pos">読み込む位置 [bytes]</param>
		/// <param name="buf">読み込むデータ</param>
		/// <returns>読み込みに成功したかどうか  (例外は捕捉されません)</returns>
		public override bool Read<T>(long pos, out T buf) where T : struct
		{
			if (!base.Read(pos, out buf))
				return false;

			using MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(PathToSMemFile, FileMode.Open, null, Capacity);
			using MemoryMappedViewAccessor mmva = mmf.CreateViewAccessor(0, Capacity);
			if (!mmva.CanRead)
				return false;

			mmva.Read(pos, out buf);

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
			if (!base.ReadArray(pos, buf, offset, count))
				return false;

			using MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(PathToSMemFile, FileMode.Open, null, Capacity);
			using MemoryMappedViewAccessor mmva = mmf.CreateViewAccessor(0, Capacity);
			if (!mmva.CanRead)
				return false;

			mmva.ReadArray(pos, buf, offset, count);

			return true;
		}

		/// <summary>共有メモリ空間の指定の位置に指定のデータを書き込む</summary>
		/// <typeparam name="T">データの型</typeparam>
		/// <param name="pos">書き込む位置 [bytes]</param>
		/// <param name="buf">書き込むデータ</param>
		/// <returns>書き込みに成功したかどうか</returns>
		public override bool Write<T>(long pos, ref T buf) where T : struct
		{
			if (!base.Write(pos, ref buf))
				return false;

			using MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(PathToSMemFile, FileMode.Open, null, Capacity);
			using MemoryMappedViewAccessor mmva = mmf.CreateViewAccessor(0, Capacity);
			if (!mmva.CanWrite)
				return false;

			mmva.Write(pos, ref buf);

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
			if (!base.WriteArray(pos, buf, offset, count))
				return false;

			long elemBytes = getElemSize<T>();
			int canWriteInOneTime = (int)(CanWriteInOneTime_Bytes / elemBytes);
			long posStep = elemBytes * canWriteInOneTime;

			using MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(PathToSMemFile, FileMode.Open, null, Capacity);
			using MemoryMappedViewAccessor mmva = mmf.CreateViewAccessor(0, Capacity);
			if (!mmva.CanWrite)
				return false;

			while (count > 0)
			{
				int write_count = count > canWriteInOneTime ? canWriteInOneTime : count;

				mmva.WriteArray(pos, buf, offset, write_count);

				offset += canWriteInOneTime;
				count -= canWriteInOneTime;
				pos += posStep;
			}

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
				if (disposing)
				{
				}

				disposedValue = true;
			}
		}

		/// <summary>保持しているリソースを解放する</summary>
		public override void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
#endif
