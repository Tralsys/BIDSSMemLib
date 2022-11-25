using System;
using System.Threading;

namespace TR;

public class RWSemap_UNIX : IRWSemaphore
{
	private Semaphore Semaphore { get; }

	static readonly private TimeSpan TIMEOUT = new(0, 0, 0, 0, 100);

	/// <summary>
	/// インスタンスを初期化する
	/// </summary>
	/// <param name="name">使用するリソースの名前</param>
	public RWSemap_UNIX(string name)
	{
		Semaphore = new(0, 1, name + nameof(Semaphore));
	}

	public void Read(Action act)
	{
		bool actionTried = false;

		try
		{
			Semaphore.WaitOne(TIMEOUT);

			actionTried = true;

			act.Invoke();
		}
		finally
		{
			if (actionTried)
				Semaphore.Release();
		}
	}

	public void Write(Action act)
	{
		bool actionTried = false;

		try
		{
			Semaphore.WaitOne(TIMEOUT);

			actionTried = true;

			act.Invoke();
		}
		finally
		{
			if (actionTried)
				Semaphore.Release();
		}
	}

	#region IDisposable Support
	private bool disposedValue;

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				Semaphore.Dispose();
			}

			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
	#endregion IDisposable Support
}

