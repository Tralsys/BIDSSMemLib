using System;

namespace TR;

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
