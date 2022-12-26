using System;
using System.Reflection;

using TR.BIDSSMemLib;

namespace TR.BIDSSMemInputTester
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine(Assembly.GetExecutingAssembly());
			Console.WriteLine("P:Power, B:Brake, R:Reverser, D:KeyDown, U:KeyUp\nEach Command is needed to be splitted by the Space Char.");
			Console.WriteLine("Command Example : \"P6 B7 R-1 D0 U2\" and Press Enter.");
			Console.WriteLine("If you want to exit, please enter the command \"exit\"");

			while (true)
			{
				string? s = Console.ReadLine();

				if (string.IsNullOrEmpty(s))
					continue;

				if (!ParseAndExecCommand(s.Split(' ')))
					return;
			}
		}

		static bool ParseAndExecCommand(in string[] cmdArray)
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

			return true;
		}

		static bool ParseAndExecCommand(in char cmdType, in string cmd)
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
			}

			return true;
		}
	}
}
