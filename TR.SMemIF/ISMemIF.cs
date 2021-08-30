using System;

namespace TR
{
	/// <summary>.net4.0未満かそれ以上かを気にすることなく共有メモリ空間にアクセスする機能を提供するクラスのインターフェイス</summary>
	public interface ISmemIF : ISMemIF_Reader, ISMemIF_Writer
	{

	}

	/// <summary>SMemIFの読み込み機能を提供する際のインターフェイス</summary>
	public interface ISMemIF_Reader : IDisposable
	{
		/// <summary>共有メモリ空間の名前</summary>
		string SMemName { get; }

		/// <summary>共有メモリ空間のキャパシティ</summary>
		/// <remarks>キャパシティの変更には大きなコストが伴うので注意  (メモリ空間を開き直すため)</remarks>
		long Capacity { get; set; }

		/// <summary>共有メモリ空間の指定の位置から, 指定の型のデータを読み込む</summary>
		/// <typeparam name="T">読み込みたい型</typeparam>
		/// <param name="pos">読み込む位置 [bytes]</param>
		/// <param name="buf">読み込むデータ</param>
		/// <returns>読み込みに成功したかどうか  (例外は捕捉されません)</returns>
		bool Read<T>(long pos, out T buf) where T : struct;

		/// <summary>SMemから連続的に値を読み取ります.</summary>
		/// <typeparam name="T">値の型</typeparam>
		/// <param name="pos">読み込む位置 [bytes]</param>
		/// <param name="buf">読み取り結果を格納する配列</param>
		/// <param name="offset">配列内で書き込みを開始する位置</param>
		/// <param name="count">読み取りを行う数</param>
		/// <returns>読み取りに成功したかどうか</returns>
		bool ReadArray<T>(long pos, T[] buf, int offset, int count) where T : struct;
	}

	/// <summary>SMemIFの書き込み機能を提供する際のインターフェイス</summary>
	public interface ISMemIF_Writer : IDisposable
	{
		/// <summary>共有メモリ空間の名前</summary>
		string SMemName { get; }
		/// <summary>共有メモリ空間のキャパシティ</summary>
		/// <remarks>減少方向への操作には大きなコストが伴うので注意  (メモリ空間を開き直すため)</remarks>
		long Capacity { get; set; }

		/// <summary>共有メモリ空間の指定の位置に指定のデータを書き込む</summary>
		/// <typeparam name="T">データの型</typeparam>
		/// <param name="pos">書き込む位置 [bytes]</param>
		/// <param name="buf">書き込むデータ</param>
		/// <returns>書き込みに成功したかどうか</returns>
		public bool Write<T>(long pos, ref T buf) where T : struct;

		/// <summary>SMemに連続した値を書き込みます</summary>
		/// <typeparam name="T">書き込む値の型</typeparam>
		/// <param name="pos">書き込みを開始するSMem内の位置 [bytes]</param>
		/// <param name="buf">SMemに書き込む配列</param>
		/// <param name="offset">配列内で書き込みを開始する位置</param>
		/// <param name="count">書き込む要素数</param>
		/// <returns>書き込みに成功したかどうか</returns>
		public bool WriteArray<T>(long pos, T[] buf, int offset, int count) where T : struct;
	}
}
