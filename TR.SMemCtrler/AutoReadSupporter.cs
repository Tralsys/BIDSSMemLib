using System;
using System.Threading.Tasks;

namespace TR
{
	/// <summary>データの自動取得機能を提供する</summary>
	/// <typeparam name="T">自動で取得するデータの型</typeparam>
	public class AutoReadSupporter<T> : IAutoReadSupporter<T>
	{
		/// <summary>共有メモリのデータを管理するクラス</summary>
		ISMemCtrler<T> smemCtrler { get; }

		/// <summary>自動読み取り機能が動作中かどうか</summary>
		public bool IsRunning { get; set; } = false;

		/// <summary>実行間隔</summary>
		public TimeSpan Interval { get; private set; }
		Task task { get; }

		/// <summary>インスタンスを初期化する</summary>
		/// <param name="_smemCtrler">ターゲットとなるSMemCtrler</param>
		public AutoReadSupporter(ISMemCtrler<T> _smemCtrler)
		{
			smemCtrler = _smemCtrler;

			task = new Task(AutoReadTask);
		}

		async void AutoReadTask()
		{
			if (smemCtrler is null)
				return;

			while (!IsRunning && !disposingValue && !disposedValue)
			{
				smemCtrler.Read();

				await Task.Delay(Interval);
			}
		}


		#region Auto Read Methods
		/// <summary>自動取得を開始する</summary>
		/// <param name="interval_ms">読取間隔 [ms]</param>
		/// <returns>自動取得開始時点の値</returns>
		public T AR_Start(int interval_ms) => AR_Start(new TimeSpan(0, 0, 0, 0, interval_ms));

		/// <summary>自動取得を開始する</summary>
		/// <param name="interval">読取間隔</param>
		/// <returns>自動取得開始時点の値</returns>
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

		/// <summary>自動取得を停止する</summary>
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

		/// <summary>インスタンスが保持するリソースを解放する</summary>
		/// <param name="disposing">マネージドリソースを解放するかどうか</param>
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

		/// <summary>インスタンスが保持するリソースを解放する</summary>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
