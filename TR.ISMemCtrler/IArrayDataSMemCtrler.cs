using System;
using System.Collections.Generic;

namespace TR;
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
