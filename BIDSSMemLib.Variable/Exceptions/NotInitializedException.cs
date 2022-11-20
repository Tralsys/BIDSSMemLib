using System;

namespace TR.BIDSSMemLib.Variable.Exceptions;

/// <summary>
/// 共有メモリが初期化されていない場合に発火する例外
/// </summary>
public class NotInitializedException : Exception
{
	/// <summary>
	/// インスタンスを初期化する
	/// </summary>
	/// <param name="SMemName">初期化されていない共有メモリの名前</param>
	public NotInitializedException(string SMemName) : base($"The SMem `{SMemName}` is not initialized")
	{
	}
}
