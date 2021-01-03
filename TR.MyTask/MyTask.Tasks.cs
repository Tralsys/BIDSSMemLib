#if !(NET11 || NET20 || NET35)
using System;
using System.Threading.Tasks;

namespace TR
{
	public class MyTask : Task, IMyTask
	{
		static public Task Run(Action<object> act)
#if NET40
		{
			Task t = new Task(() => act.Invoke(null));
			t.Start();
			return t;
		}
#else
			=> Task.Run(() => act.Invoke(null));
#endif
		public MyTask(Action<object> act) : base(() => act.Invoke(null))
		{

		}
		public bool IsAlive { get => !IsCompleted; }

		void IMyTask.Wait(int milliseconds) => Wait(milliseconds);

		void IDisposable.Dispose() { }
	}
}
#endif