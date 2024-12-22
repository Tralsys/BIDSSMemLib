using System;

namespace TR;

/// <summary>
/// プラットフォーム間の差異を吸収したセマフォ機能を提供します。
/// </summary>
public interface IRWSemaphore : IDisposable
{
	/// <summary>
	/// Read操作を行う指定のActionを実行する
	/// </summary>
	/// <param name="act">Read操作を行うAction</param>
	public void Read(Action act);
	/// <summary>
	/// Write操作を行う指定のActionを実行する
	/// </summary>
	/// <param name="act">Write操作を行うAction</param>
	public void Write(Action act);
}
