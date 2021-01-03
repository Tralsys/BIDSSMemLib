using System;

namespace TR
{
	public interface IMyTask : IDisposable
	{
		bool IsAlive { get; }
		bool IsCompleted { get; }

		/// <summary>指定時間だけ, スレッドの実行終了を待機します.</summary>
		/// <param name="milliseconds">待機する時間</param>
		void Wait(int milliseconds);


		/// <summary>初期化時に指定した処理を実行します.</summary>
		void Start();
	}
}
