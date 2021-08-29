using System;
using System.Threading;
using System.Runtime.CompilerServices;
#if !(NET35 || NET20)
using System.Threading.Tasks;
#endif

namespace TR
{
	/// <summary>await可能なR/Wロックを提供する</summary>
	public class RWSemap : IDisposable
	{
		private const MethodImplOptions MIOpt = (MethodImplOptions)256;//MethodImplOptions.AggressiveInlining;
		private long WAIT_TICK = 1;//別モード動作中に, モード復帰をチェックする間隔[tick]
		private int Reading = 0;//Read操作中のActionの数 (Interlockedで操作を行う)
		private int Want_to_Write = 0;//Write操作待機中, あるいは実行中のActionの数 (Interlockedで操作を行う)

		private object LockObj = new object();//Writeロックを管理するobject

		/// <summary>リソースを解放します(予定)</summary>
		public void Dispose() { }

		/// <summary>Writeロックを行ったうえで, 指定の読み取り操作を行います</summary>
		/// <param name="act">読み取り操作</param>
		/// <returns>成功したかどうか</returns>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public
#if NET35 || NET20
			bool
#else
			async Task<bool>
#endif
			Read(Action<object> act)//net2.0対応のため, object型引数を指定  処理的には不要
		{
			while (Want_to_Write > 0)//Writeロック取得待機
			{
#if !(NET35 || NET20)
				await
#endif
				Delay(TimeSpan.FromTicks(WAIT_TICK));
			}
			try
			{
				Interlocked.Increment(ref Reading);
				act?.Invoke(null);
			}
			finally
			{
				Interlocked.Decrement(ref Reading);
			}
			return true;
		}

		/// <summary>Readロックを行ったうえで, 指定の書き込み操作を実行します</summary>
		/// <param name="act">書き込み操作</param>
		/// <returns>成功したかどうか</returns>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public
#if NET35 || NET20
			bool
#else
			async Task<bool>
#endif
	Write(Action<object> act)//net2.0対応のため, object型引数を指定  処理的には不要
		{
			try
			{
				Interlocked.Increment(ref Want_to_Write);//Write待機
				while (Reading > 0)//Read完了待機
				{
#if !(NET35 || NET20)
					await
#endif
					Delay(TimeSpan.FromTicks(WAIT_TICK));

				}
				lock (LockObj)//Writeロック
				{
					act?.Invoke(null);
				}
			}
			finally
			{
				Interlocked.Decrement(ref Want_to_Write);//Write完了処理
			}
			return true;
		}

#if NET35 || NET20
		static private void Delay(TimeSpan ts) => Thread.Sleep(ts);
#else
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		static private async Task Delay(TimeSpan ts) => await Task.Delay(ts);
#endif

	}
}
