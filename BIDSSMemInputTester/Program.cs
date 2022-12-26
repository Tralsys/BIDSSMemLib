using System;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using TR.BIDSSMemLib;

namespace TR.BIDSSMemInputTester
{
	class Program : IDisposable
	{
		static readonly string helpString;

		static Program()
		{
			StringBuilder builder = new();

			builder.AppendLine(Assembly.GetExecutingAssembly().ToString());
			builder.AppendLine("P:Power, B:Brake, R:Reverser, D:KeyDown, U:KeyUp, W:WatcherStart");
			builder.AppendLine("Each Command is needed to be splitted by the Space Char.");
			builder.AppendLine("Command Example : \"P6 B7 R-1 D0 U2\" and Press Enter.");

			builder.Append("If you want to exit, please enter the command \"exit\"");

			helpString = builder.ToString();
		}

		static void Main(string[] args)
		{
			Console.WriteLine(helpString);

			using Program program = new();

			while (true)
			{
				string? s = Console.ReadLine();

				if (string.IsNullOrEmpty(s))
					continue;

				if (!program.ParseAndExecCommand(s.Split(' ')))
					return;
			}
		}

		/// <summary>
		/// 読み書きを同時に行わないようにするためのコマンド
		/// </summary>
		readonly object lockObj = new();

		readonly CancellationTokenSource CancellationTokenSource = new();

		bool ParseAndExecCommand(in string[] cmdArray)
		{
			lock (lockObj)
			{
				foreach (var cmd in cmdArray)
				{
					try
					{
						if (!ParseAndExecCommand(cmd[0], cmd))
							return false;
					}
					catch (Exception e)
					{
						Console.WriteLine(e);
					}
				}
			}

			return true;
		}

		bool ParseAndExecCommand(in char cmdType, in string cmd)
		{
			switch (cmdType)
			{
				case 'P':
					CtrlInput.SetHandD(CtrlInput.HandType.Power, int.Parse(cmd[1..]));
					break;
				case 'B':
					CtrlInput.SetHandD(CtrlInput.HandType.Brake, int.Parse(cmd[1..]));
					break;
				case 'R':
					CtrlInput.SetHandD(CtrlInput.HandType.Reverser, int.Parse(cmd[1..]));
					break;
				case 'D':
					CtrlInput.SetIsKeyPushed(int.Parse(cmd[1..]), true);
					break;
				case 'U':
					CtrlInput.SetIsKeyPushed(int.Parse(cmd[1..]), false);
					break;
				case 'p':
					CtrlInput.SetHandD(CtrlInput.HandType.PPos, double.Parse(cmd[1..]));
					break;
				case 'b':
					CtrlInput.SetHandD(CtrlInput.HandType.BPos, double.Parse(cmd[1..]));
					break;
				case 'e':
					return cmd != "exit";

				case 'W':
					Task.Run(Watcher);
					break;
			}

			return true;
		}

		readonly TimeSpan Interval = new(0, 0, 0, 0, 10);

		bool isWatcherRunning = false;
		async Task Watcher()
		{
			if (isWatcherRunning)
			{
				Console.WriteLine("Watcher Already Running.");
				return;
			}

			Console.WriteLine("Watcher Started");

			isWatcherRunning = true;
			while (!CancellationTokenSource.IsCancellationRequested)
			{
				await Task.Run(CheckAndPrintChangedValue, CancellationTokenSource.Token);
				await Task.Delay(Interval, CancellationTokenSource.Token);
			}
		}

		const int KEY_PRINT_NEWLINE_EACH = 8;
		Hands? lastHand = null;
		bool[]? lastKey = null;
		void CheckAndPrintChangedValue()
		{
			StringBuilder? builder = null;
			DateTime Now = DateTime.Now;

			bool CheckAndAppend(string str, in object? lastValue, in object newValue, bool printComma = true)
			{
				if (Equals(lastValue, newValue))
					return false;

				if (builder is null)
					builder = new($"[{Now:HH:mm:ss.ffff}] Changed!: ");
				else if (printComma)
					builder.Append($", ");

				builder.AppendFormat("{0}({1} -> {2})", str, lastValue, newValue);
				return true;
			}

			lock (lockObj)
			{
				Hands currentHand = CtrlInput.GetHandD();
				bool[] currentKey = CtrlInput.GetIsKeyPushed();

				CheckAndAppend("Brake", lastHand?.B, currentHand.B);
				CheckAndAppend("Power", lastHand?.P, currentHand.P);
				CheckAndAppend("Reverser", lastHand?.R, currentHand.R);
				CheckAndAppend("OneHandle", lastHand?.S, currentHand.S);
				CheckAndAppend("BrakeByPos", lastHand?.BPos, currentHand.BPos);
				CheckAndAppend("PowerByPos", lastHand?.PPos, currentHand.PPos);

				builder?.Append("\n\t");

				int changedValueCounter = 0;
				for (int i = 0; i < currentKey.Length; i++)
				{
					if (CheckAndAppend($"Key[{i}]", lastKey?[i], currentKey[i], (i % KEY_PRINT_NEWLINE_EACH) != 0)
						&& (++changedValueCounter % KEY_PRINT_NEWLINE_EACH) == 0)
						builder?.Append("\n\t");
				}

				lastHand = currentHand;
				lastKey = currentKey;
			}

			if (builder is not null)
				Console.WriteLine(builder.Append("\n~~~~~~~~~~~~~~~~~~~~"));
		}

		public void Dispose()
		{
			CancellationTokenSource.Cancel();
		}
	}
}
