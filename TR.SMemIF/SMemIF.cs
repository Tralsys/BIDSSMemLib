using System;
using System.Runtime.InteropServices;

namespace TR
{
	/// <summary>TargetFramework別にSharedMemoryを提供します.</summary>
	public class SMemIF : ISMemIF
	{
		private IRWSemaphore Semap { get; }

		private ISMemIF BaseSMemIF { get; }

		/// <inheritdoc/>
		public string SMemName => BaseSMemIF.SMemName;

		/// <inheritdoc/>
		public long Capacity => BaseSMemIF.Capacity;

		/// <inheritdoc/>
		public bool IsNewlyCreated => BaseSMemIF.IsNewlyCreated;

		/// <summary>インスタンスを初期化する</summary>
		/// <param name="smem_name">共有メモリ空間の名前</param>
		/// <param name="capacity">共有メモリ空間のキャパシティ</param>
		public SMemIF(string smem_name, long capacity)
		{
			bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
			Semap = isWindows ? new RWSemap() : new RWSemap_UNIX(smem_name);
			if (capacity <= 0)
				BaseSMemIF = new SMemIFMock(smem_name, capacity);
			else if (isWindows)
				BaseSMemIF = new SemaphorelessSMemIF(smem_name, capacity);
			else
				BaseSMemIF = new SemaphorelessSMemIF_UNIX(smem_name, capacity);
		}

		/// <summary>共有メモリ空間の指定の位置から, 指定の型のデータを読み込む</summary>
		/// <typeparam name="T">読み込みたい型</typeparam>
		/// <param name="pos">読み込む位置 [bytes]</param>
		/// <param name="buf">読み込むデータ</param>
		/// <returns>読み込みに成功したかどうか  (例外は捕捉されません)</returns>
		public bool Read<T>(long pos, out T buf) where T : struct
		{
			T retT = default;

			Semap.Read(() => BaseSMemIF.Read(pos, out retT));

			buf = retT;
			return true;
		}

		/// <summary>SMemから連続的に値を読み取ります.</summary>
		/// <typeparam name="T">値の型(NET35 || NET20モードではint型/bool型のみ使用可能)</typeparam>
		/// <param name="pos">SMem内でのデータ開始位置</param>
		/// <param name="buf">読み取り結果を格納する配列</param>
		/// <param name="offset">配列内で書き込みを開始する位置</param>
		/// <param name="count">読み取りを行う数</param>
		/// <returns>読み取りに成功したかどうか</returns>
		public bool ReadArray<T>(long pos, T[] buf, int offset, int count) where T : struct
		{
			Semap.Read(() => BaseSMemIF.ReadArray(pos, buf, offset, count));

			return true;
		}

		/// <summary>共有メモリ空間の指定の位置に指定のデータを書き込む</summary>
		/// <typeparam name="T">データの型</typeparam>
		/// <param name="pos">書き込む位置 [bytes]</param>
		/// <param name="buf">書き込むデータ</param>
		/// <returns>書き込みに成功したかどうか</returns>
		public bool Write<T>(long pos, ref T buf) where T : struct
		{
			T retT = buf;

			Semap.Write(() => BaseSMemIF.Write(pos, ref retT));

			return true;
		}

		/// <summary>SMemに連続した値を書き込みます.</summary>
		/// <typeparam name="T">書き込む値の型 (NET35 || NET20モードではint/bool型のみ使用可能)</typeparam>
		/// <param name="pos">書き込みを開始するSMem内の位置</param>
		/// <param name="buf">SMemに書き込む配列</param>
		/// <param name="offset">配列内で書き込みを開始する位置</param>
		/// <param name="count">書き込む要素数</param>
		/// <returns>書き込みに成功したかどうか</returns>
		public bool WriteArray<T>(long pos, T[] buf, int offset, int count) where T : struct
		{
			Semap.Write(() => BaseSMemIF.WriteArray(pos, buf, offset, count));

			return true;
		}


		#region IDisposable Support
		/// <summary>リソースの解放が完了したかどうか</summary>
		protected bool disposedValue = false;

		/// <summary>保持しているリソースを解放する</summary>
		/// <param name="disposing">Managedリソースも解放するかどうか</param>
		protected void Dispose(bool disposing)
		{
			disposing = true;
			if (!disposedValue)
			{
				if (disposing)
				{
					BaseSMemIF.Dispose();
					Semap?.Dispose();

					disposedValue = true;
				}
			}
		}

		/// <inheritdoc/>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		#endregion

	}
}
