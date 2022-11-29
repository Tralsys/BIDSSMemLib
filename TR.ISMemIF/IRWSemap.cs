using System;

namespace TR;

public interface IRWSemaphore : IDisposable
{
	public void Read(Action act);
	public void Write(Action act);
}
