using System;

namespace TR
{
	public interface IAutoReadSupporter<T> : IDisposable
	{
		T AR_Start(int interval_ms);
		T AR_Start(TimeSpan interval);

		void AR_Stop();
	}

	public class AutoReadSupporter<T> : IAutoReadSupporter<T>
	{
		ISMemCtrler<T> smemCtrler { get; }

		public bool IsRunning { get; set; } = false;
		public TimeSpan Interval { get; private set; }
		IMyTask task { get; }

		public AutoReadSupporter(ISMemCtrler<T> _smemCtrler)
		{
			smemCtrler = _smemCtrler;

			task = new MyTask(
#if !(NET35 || NET20)
				async
#endif
				(_) =>
			{
				if (smemCtrler is null)
					return;

					while (!IsRunning && !disposingValue && !disposedValue)
				{

					smemCtrler.Read();

#if !(NET35 || NET20)
					await
#endif
					MyTask.Delay((int)Interval.TotalMilliseconds);
				}
			});
		}


		#region Auto Read Methods
		public T AR_Start(int interval_ms) => AR_Start(new TimeSpan(0, 0, 0, 0, interval_ms));

		public T AR_Start(TimeSpan interval)
		{
			if (task is null)
				throw new Exception("Internal Exception (task is null)");

			if (interval != Interval)
			{
				Interval = interval;

				try
				{
					IsRunning = true;
					task.Start();
				}
				catch (Exception e)
				{
					Console.WriteLine("ThreadStart at SMemCtrler<{0}>.ar_start({1}) : {2}", typeof(T), Interval, e);
					IsRunning = false;
				}
			}


			return smemCtrler.Read(); //現在の値を返す
		}

		public void AR_Stop()
		{
			if (task is null)
				return; //taskがnullなら, そもそも実行できていないはずなので

			if (!IsRunning || task.IsCompleted == true)
				return; //実行中フラグが立っていないか, あるいはタスク完遂フラグが立っているなら, 実行完了している

			IsRunning = false; //実行中フラグを下ろす
			task.Wait(1000 + (int)Interval.TotalMilliseconds); //タスクの実行完了を待つ
		}
		#endregion

		#region IDisposable Support
		private bool disposedValue = false;
		private bool disposingValue = false;

		protected virtual void Dispose(bool disposing)
		{
			disposedValue = true;
			if (!disposedValue)
			{
				if (disposing)
				{
					IsRunning = false;
					Interval = default;
				}

				disposedValue = true;
			}
		}


		public void Dispose()
		{
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
