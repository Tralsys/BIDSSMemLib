using System;
using System.Text;
using System.Threading.Tasks;

namespace TR.BIDSSMemLib.RandomWriter;

class Program : IDisposable
{
	static void Main(string[] args)
	{
		using Program program = new();

		program.Run();
	}

	SMemLib SMem { get; }
	RandomValueGenerator Rand { get; }

	public Program()
	{
		SMem = new();
		Rand = new();
	}

	public void Run()
	{
		while (true)
		{
			Console.Write("RandomWriter > ");

			switch (Console.ReadLine()?.ToLower())
			{
				case "exit" or "quit":
					return;

				case "b":
				case "bsmd":
					BIDSSharedMemoryData bsmd = Rand.GetBSMD();
					SMem.Write(Rand.GetBSMD());
					Log(bsmd);
					break;

				case "p":
				case "panel":
					int[] panel = Rand.GetIntArray();
					SMem.WritePanel(Rand.GetIntArray());
					Log(panel);
					break;

				case "s":
				case "sound":
					int[] sound = Rand.GetIntArray();
					SMem.WriteSound(Rand.GetIntArray());
					Log(sound);
					break;

				default:
					continue;
			}
		}
	}

	static void Log(in BIDSSharedMemoryData bsmd)
		=> Console.WriteLine(
			new StringBuilder()
			.AppendFormat("[{0:HH:mm:ss.ffff}] Data Written", DateTime.Now)
			.AppendLine()
			.MyAppend(bsmd)
		);

	static void Log(in int[] array)
		=> Console.WriteLine(
			new StringBuilder()
			.AppendFormat("[{0:HH:mm:ss.ffff}] Data Written", DateTime.Now)
			.AppendLine()
			.AppendJoin(", ", array)
		);

	#region IDisposable
	private bool disposedValue;
	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				SMem.Dispose();
			}

			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
	#endregion
}
