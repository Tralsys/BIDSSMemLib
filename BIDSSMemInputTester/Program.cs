using System;
using System.Reflection;
using TR.BIDSSMemLib;

namespace TR.BIDSSMemInputTester
{
  class Program
  {
    static CtrlInput ci = null;
    static void Main(string[] args)
    {
      Console.WriteLine(Assembly.GetExecutingAssembly());
      Console.WriteLine("P:Power, B:Brake, R:Reverser, D:KeyDown, U:KeyUp\nEach Command is needed to be splitted by the Space Char.");
      Console.WriteLine("Command Example : \"P6 B7 R-1 D0 U2\" and Press Enter.");
      Console.WriteLine("If you want to exit, please enter the command \"exit\"");
      bool IsLooping = true;
      ci = new CtrlInput();
      while (IsLooping)
      {
        string s = Console.ReadLine();
        if (s != null && s != string.Empty)
        {
          string[] sa = s.Split(' ');
          for(int i = 0; i < sa.Length; i++)
          {
            try
            {
              switch (sa[i].ToCharArray()[0])
              {
                case 'P':
                  ci.SetHandD(CtrlInput.HandType.Power, int.Parse(sa[i].Remove(0, 1)));
                  break;
                case 'B':
                  ci.SetHandD(CtrlInput.HandType.Brake, int.Parse(sa[i].Remove(0, 1)));
                  break;
                case 'R':
                  ci.SetHandD(CtrlInput.HandType.Reverser, int.Parse(sa[i].Remove(0, 1)));
                  break;
                case 'D':
                  ci.SetIsKeyPushed(int.Parse(sa[i].Remove(0, 1)), true);
                  break;
                case 'U':
                  ci.SetIsKeyPushed(int.Parse(sa[i].Remove(0, 1)), false);
                  break;
                case 'p':
                  ci.SetHandD(CtrlInput.HandType.PPos, double.Parse(sa[i].Remove(0, 1)));
                  break;
                case 'b':
                  ci.SetHandD(CtrlInput.HandType.BPos, double.Parse(sa[i].Remove(0, 1)));
                  break;
                case 'e':
                  IsLooping = sa[i] != "exit";
                  break;
              }
            }catch(Exception e) { Console.WriteLine(e); }
          }
        }
      }
      Console.WriteLine("Please enter any key to exit...");
      Console.ReadKey();
    }
  }
}
