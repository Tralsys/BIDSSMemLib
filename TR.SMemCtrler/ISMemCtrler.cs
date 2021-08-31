using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TR
{
	/// <summary>すべての型引数で共通したRW機能を提供するためのインターフェイス</summary>
	public interface IReadWriteInObject : IDisposable
	{
		/// <summary>データを読み込む</summary>
		/// <returns>読み込んだデータ</returns>
		object ReadInObject();

		/// <summary>データの読み取りを試行する</summary>
		/// <param name="obj">読み取ったデータの記録先</param>
		/// <returns>試行結果</returns>
		bool TryReadInObject(out object obj);

		/// <summary>データを書き込む</summary>
		/// <param name="obj">書き込むデータ</param>
		void WriteInObject(in object obj);

		/// <summary>データの書き込みを試行する</summary>
		/// <param name="obj">書き込むデータ</param>
		/// <returns>試行結果</returns>
		bool TryWriteInObject(in object obj);
	}

	/// <summary>SMemCtrlerが備える必要があるインターフェイス</summary>
	/// <typeparam name="T">SMemCtrlerで読み書きするデータの型</typeparam>
	public interface ISMemCtrler<T> : IDisposable, INotifyPropertyChanged
	{
		/// <summary>自動読み取り機能を提供するクラスのインスタンス</summary>
		IAutoReadSupporter<T> AutoRead { get; }

		/// <summary>共有メモリ領域を使用してデータを共有するかどうか</summary>
		bool No_SMem_Mode { get; set; }

		/// <summary>データ更新時にイベントを発火させるかどうか</summary>
		bool No_Event_Mode { get; set; }

		/// <summary>共有メモリの名前</summary>
		string SMem_Name { get; }

		/// <summary>1要素あたりのサイズ [bytes]</summary>
		uint Elem_Size { get; }

		/// <summary>共有メモリのキャパシティ [bytes]</summary>
		long Capacity { get; }

		/// <summary>値の更新を通知するイベント</summary>
		event EventHandler<ValueChangedEventArgs<T>> ValueChanged;

		/// <summary>データのキャッシュ</summary>
		T Value { get; set; }

		/// <summary>データを読み取る</summary>
		/// <returns>読み取ったデータ</returns>
		T Read();

		/// <summary>データ読み取りを試行する</summary>
		/// <param name="value">読み取ったデータの記録先</param>
		/// <returns>試行結果</returns>
		bool TryRead(out T value);

		/// <summary>データを書き込む</summary>
		/// <param name="value">書き込むデータ</param>
		void Write(in T value);

		/// <summary>データの書き込みを試行する</summary>
		/// <param name="value">書き込むデータ</param>
		/// <returns>試行結果</returns>
		bool TryWrite(in T value);
	}

	/// <summary>配列データの操作機能をSMemCtrlerにて提供する場合に備える必要があるインターフェイス</summary>
	/// <typeparam name="T"></typeparam>
	public interface IArrayDataSMemCtrler<T> : IList<T>, ISMemCtrler<List<T>>
	{
		/// <summary>配列形式で値の変化を通知するイベント</summary>
		event EventHandler<ValueChangedEventArgs<T[]>> ArrValueChanged;

		/// <summary>指定の位置にある要素を読み取る</summary>
		/// <param name="index">配列内の位置 (インデックス)</param>
		/// <returns>読み取った値</returns>
		T Read(in int index);

		/// <summary>指定の位置の要素の読取を試行する</summary>
		/// <param name="index">位置指定 (インデックス)</param>
		/// <param name="value">読み取った値の記録先</param>
		/// <returns>試行結果</returns>
		bool TryRead(in int index, out T value);

		/// <summary>指定の配列を共有メモリに書き込む</summary>
		/// <param name="array">書き込む配列</param>
		void Write(in T[] array);

		/// <summary>指定の位置に指定の要素を書き込む</summary>
		/// <param name="index">位置指定 (インデックス)</param>
		/// <param name="value">書き込むデータ</param>
		void Write(in int index, in T value);

		/// <summary>指定のデータを指定の位置に書き込む操作を試行する</summary>
		/// <param name="index">書き込む位置 (インデックス)</param>
		/// <param name="value">書き込む値</param>
		/// <returns>試行結果</returns>
		bool TryWrite(in int index, in T value);

		/// <summary>キャッシュのリストを配列として取得する</summary>
		/// <returns>キャッシュのリストを配列に変換したもの</returns>
		T[] ToArray();
	}
}
