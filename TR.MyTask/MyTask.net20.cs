#if (NET20 || NET35)
using System;
using System.Threading;

namespace TR
{
	public class MyTask : IMyTask
	{
		static public IAsyncResult Run(Action<object> act) => act?.BeginInvoke(null, act.EndInvoke, null);
		public bool IsAlive { get => !IsCompleted; }
		public bool IsCompleted { get; private set; } = false;

		Action<object> Act { get; }

		public MyTask(Action<object> act)
		{
			//if (act is null) ;
			Act = act ?? throw new ArgumentNullException("arg \"act\" is null");
		}

		/// <summary>指定時間の間, 処理を停止します.</summary>
		/// <param name="milliseconds">処理を停止する時間</param>
		public static void Delay(int milliseconds)
		{
			if (milliseconds < 0) throw new ArgumentOutOfRangeException("cannot use minus number in the arg \"milliseconds\"");
			
			Thread.Sleep(milliseconds);
		}

		/// <summary>指定時間だけ, スレッドの実行終了を待機します.</summary>
		/// <param name="milliseconds">待機する時間</param>
		public void Wait(int milliseconds) => Act.EndInvoke(null);


		/// <summary>初期化時に指定した処理を実行します.</summary>
		public void Start()
		{
			IsCompleted = false;
			_ = Act.BeginInvoke(null, Callback, null);
		}

		public void Dispose()
		{
			if (!IsCompleted) Act.EndInvoke(null);
		}

		private void Callback(IAsyncResult ar)
		{
			Act.EndInvoke(ar);
			IsCompleted = true;
		}
	}
}
#endif