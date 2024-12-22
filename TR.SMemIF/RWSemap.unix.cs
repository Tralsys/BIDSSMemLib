using System;
using System.Threading;

namespace TR;

/// <summary>
/// UNIZ系OS向けのセマフォ機能を提供します。
/// </summary>
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

	/// <inheritdoc/>
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

	/// <inheritdoc/>
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

	/// <inheritdoc/>
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

	/// <inheritdoc/>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
	#endregion IDisposable Support
}

