using System;
using System.Text;

namespace TR.BIDSSMemLib.ValueChecker;

class Program : IDisposable
{
	static void Main(string[] args)
	{
		using Program program = new();

		program.Run();
	}

	SMemLib SMem { get; }

	public Program()
	{
		SMem = new();

		SMem.SMC_BSMDChanged += SMem_SMC_BSMDChanged;
		SMem.SMC_PanelDChanged += SMem_SMC_PanelDChanged;
		SMem.SMC_SoundDChanged += SMem_SMC_SoundDChanged;
	}

	private static void SMem_SMC_BSMDChanged(object? sender, ValueChangedEventArgs<BIDSSharedMemoryData> e)
		=> Log(new StringBuilder("BSMD Update Detected!").AppendLine().MyAppend(e.NewValue));

	private static void SMem_SMC_PanelDChanged(object? sender, ValueChangedEventArgs<int[]> e)
		=> Log(new StringBuilder("Panel Data Detected!").AppendLine().AppendJoin(", ", e.NewValue));

	private static void SMem_SMC_SoundDChanged(object? sender, ValueChangedEventArgs<int[]> e)
		=> Log(new StringBuilder("Sound Data Update Detected!").AppendLine().AppendJoin(", ", e.NewValue));


	public void Run()
	{
		while (true)
		{
			Console.Write("ValueChecker > ");

			switch (Console.ReadLine()?.ToLower())
			{
				case "exit" or "quit":
					return;

				case "b":
				case "bsmd":
					Log(SMem.ReadBSMD());
					break;

				case "p":
				case "panel":
					Log("Panel", SMem.ReadPanel());
					break;

				case "s":
				case "sound":
					Log("Sound", SMem.ReadSound());
					break;

				case "start":
				case "readstart":
					SMem.ReadStart(SMemLib.ARNum.All, 1);
					Log("AutoRead Started");
					break;

				case "stop":
				case "readstop":
					SMem.ReadStop(SMemLib.ARNum.All);
					Log("AutoRead Stopped");
					break;

				default:
					continue;
			}
		}
	}

	static void Log(in BIDSSharedMemoryData bsmd)
		=> Console.WriteLine(
			new StringBuilder()
			.AppendFormat("[{0:HH:mm:ss.ffff}] BIDS Shared Memory Data from SMem", DateTime.Now)
			.AppendLine()
			.MyAppend(bsmd)
		);

	static void Log(in string name, in int[] array)
		=> Console.WriteLine(
			new StringBuilder()
			.AppendFormat("[{0:HH:mm:ss.ffff}] {1} Data from SMem", DateTime.Now, name)
			.AppendLine()
			.AppendJoin(", ", array)
		);

	static void Log(in object obj)
		=> Console.WriteLine($"[{DateTime.Now:HH:mm:ss.ffff}] {obj}");

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
