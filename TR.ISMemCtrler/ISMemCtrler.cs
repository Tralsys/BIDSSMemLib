using System;
using System.ComponentModel;

namespace TR;

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
