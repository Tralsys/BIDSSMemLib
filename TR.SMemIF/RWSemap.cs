using System;
using System.Threading;
using System.Runtime.CompilerServices;

namespace TR
{
	/// <summary>await可能なR/Wロックを提供する</summary>
	public class RWSemap : IDisposable
	{
		private const MethodImplOptions MIOpt = (MethodImplOptions)256;//MethodImplOptions.AggressiveInlining;

		/// <summary>別モード動作中に, モード復帰をチェックする間隔[tick]</summary>
		static private long WAIT_TICK { get; } = 1;

		/// <summary>別モード動作中に, モード復帰をチェックする間隔[tick]</summary>
		static private TimeSpan WAIT_TICK_TIMESPAN { get; } = TimeSpan.FromTicks(WAIT_TICK);

		/// <summary>Read操作中のActionの数 (Interlockedで操作を行う)</summary>
		private int Reading = 0;

		/// <summary>Write操作待機中, あるいは実行中のActionの数 (Interlockedで操作を行う)</summary>
		private int Want_to_Write = 0;

		private readonly object LockObj = new object();//Writeロックを管理するobject

		/// <summary>リソースを解放します(予定)</summary>
		public void Dispose() { }

		/// <summary>Writeロックを行ったうえで, 指定の読み取り操作を行います</summary>
		/// <param name="act">読み取り操作</param>
		/// <returns>成功したかどうか</returns>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public void Read(Action<object?> act)//net2.0対応のため, object型引数を指定  処理的には不要
		{
			while (Want_to_Write > 0)//Writeロック取得待機
				Thread.Sleep(WAIT_TICK_TIMESPAN);

			try
			{
				Interlocked.Increment(ref Reading);
				act?.Invoke(null);
			}
			finally
			{
				Interlocked.Decrement(ref Reading);
			}
		}

		/// <summary>Readロックを行ったうえで, 指定の書き込み操作を実行します</summary>
		/// <param name="act">書き込み操作</param>
		/// <returns>成功したかどうか</returns>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public void Write(Action<object?> act)//net2.0対応のため, object型引数を指定  処理的には不要
		{
			try
			{
				Interlocked.Increment(ref Want_to_Write);//Write待機
				while (Reading > 0)//Read完了待機
					Thread.Sleep(WAIT_TICK_TIMESPAN);

				lock (LockObj)//Writeロック
				{
					act?.Invoke(null);
				}
			}
			finally
			{
				Interlocked.Decrement(ref Want_to_Write);//Write完了処理
			}
		}
	}
}
