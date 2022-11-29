using System;

namespace TR;

/// <summary>データの自動取得機能を提供するクラスのインターフェイス</summary>
/// <typeparam name="T">自動で取得するデータの型</typeparam>
public interface IAutoReadSupporter<T> : IDisposable
{
	/// <summary>自動取得を開始する</summary>
	/// <param name="interval_ms">読取間隔 [ms]</param>
	/// <returns>自動取得開始時点の値</returns>
	T AR_Start(int interval_ms);

	/// <summary>自動取得を開始する</summary>
	/// <param name="interval">読取間隔</param>
	/// <returns>自動取得開始時点の値</returns>
	T AR_Start(TimeSpan interval);

	/// <summary>自動取得を停止する</summary>
	void AR_Stop();
}

