#if NET40_OR_GREATER || NETCOREAPP || NETSTANDARD

using System;
using System.IO.MemoryMappedFiles;

namespace TR
{
	/// <summary>共有メモリにアクセスする際に, セマフォによる排他制御を呼び出し元で行う場合に使用するクラス</summary>
	public class SemaphorelessSMemIF : SemaphorelessSMemIF_Preprocessing
	{
		MemoryMappedFile? MMF = null;
		MemoryMappedViewAccessor? MMVA = null;

		const long Capacity_Step = 4096;

		/// <summary>インスタンスを初期化します</summary>
		/// <param name="smem_name">共有メモリ空間の名前</param>
		/// <param name="capacity">共有メモリ空間のキャパシティ</param>
		public SemaphorelessSMemIF(string smem_name, long capacity) : base(smem_name, capacity)
		{
		}

		/// <summary>共有メモリ空間のキャパシティ</summary>
		/// <remarks>キャパシティ変更には大きなコストが伴うので注意  (メモリ空間を開き直すため)</remarks>
		public override long Capacity
		{
			get => MMVA?.Capacity ?? 0;
			set
			{
				MMVA?.Dispose();
				MMF?.Dispose();

				if (value <= 0)
					throw new ArgumentOutOfRangeException(nameof(value), "Capacity cannot be 0 or less");

				long newCap = (long)Math.Ceiling((float)value / Capacity_Step) * Capacity_Step;

				MMF = MemoryMappedFile.CreateOrOpen(SMemName, newCap);

				//sizeに0を指定すると, 最大サイズで取得することができる
				//…はずだけど, 0にすると正常に取得できなくてpositionがOutOfRangeだよって言われる
				MMVA = MMF.CreateViewAccessor(0, newCap);
			}
		}

		/// <summary>共有メモリ空間の指定の位置から, 指定の型のデータを読み込む</summary>
		/// <typeparam name="T">読み込みたい型</typeparam>
		/// <param name="pos">読み込む位置 [bytes]</param>
		/// <param name="buf">読み込むデータ</param>
		/// <returns>読み込みに成功したかどうか  (例外は捕捉されません)</returns>
		public override bool Read<T>(long pos, out T buf) where T : struct
		{
			if (!base.Read(pos, out buf) || MMVA?.CanRead != true)
				return false;

			MMVA.Read(pos, out buf);

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
			if (!base.ReadArray(pos, buf, offset, count) || MMVA?.CanRead != true)
				return false;

			MMVA.ReadArray(pos, buf, offset, count);
			return true;
		}

		/// <summary>共有メモリ空間の指定の位置に指定のデータを書き込む</summary>
		/// <typeparam name="T">データの型</typeparam>
		/// <param name="pos">書き込む位置 [bytes]</param>
		/// <param name="buf">書き込むデータ</param>
		/// <returns>書き込みに成功したかどうか</returns>
		public override bool Write<T>(long pos, ref T buf) where T : struct
		{
			if (!base.Write(pos, ref buf) || MMVA?.CanWrite != true)
				return false;

			MMVA.Write(pos, ref buf);
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
			if (!base.WriteArray(pos, buf, offset, count) || MMVA?.CanWrite != true)
				return false;

			MMVA.WriteArray(pos, buf, offset, count);
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
					MMVA?.Dispose();
					MMVA = null;
					MMF?.Dispose();
					MMF = null;
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


		/// <summary>共有メモリ空間を再度開く必要があるかどうかを確認する</summary>
		/// <param name="needed_capacity">必要なキャパシティ</param>
		protected override void CheckReOpen(long needed_capacity)
		{
			if (Capacity > needed_capacity)
				return;//保持キャパが要求キャパより大きいなら, 再度開く必要はない

			Capacity = needed_capacity;
		}
	}
}
#endif