#if NETSTANDARD || NETCOREAPP
using System;
using System.Threading;

namespace TR;

public class RWSemap_UNIX : IRWSemaphore
{
	static readonly private TimeSpan TIMEOUT = new(0, 0, 0, 0, 100);

	private Mutex NamedMutex { get; }

	/// <summary>
	/// インスタンスを初期化する
	/// </summary>
	/// <param name="name">使用するリソースの名前</param>
	public RWSemap_UNIX(string name)
	{
		NamedMutex = new(false, name + "Mutex");
	}

	public void Read(Action act)
	{
		bool actionTried = false;

		try
		{
			NamedMutex.WaitOne(TIMEOUT);

			actionTried = true;

			act.Invoke();
		}
		finally
		{
			if (actionTried)
				NamedMutex.ReleaseMutex();
		}
	}

	public void Write(Action act)
	{
		bool actionTried = false;

		try
		{
			NamedMutex.WaitOne(TIMEOUT);

			actionTried = true;

			act.Invoke();
		}
		finally
		{
			if (actionTried)
				NamedMutex.ReleaseMutex();
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
				NamedMutex.Close();
				NamedMutex.Dispose();
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
#endif
