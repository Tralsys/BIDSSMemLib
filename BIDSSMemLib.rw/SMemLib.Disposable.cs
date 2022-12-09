using System;

namespace TR.BIDSSMemLib;

public partial class SMemLib : IDisposable
{
	private bool disposedValue;

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				SMC_BSMD.Dispose();
				SMC_OpenD.Dispose();
				SMC_PnlD.Dispose();
				SMC_SndD.Dispose();
			}

			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
}

