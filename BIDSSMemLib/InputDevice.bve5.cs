using System;
using System.Reflection;
using System.Windows.Forms;
using Mackoy.Bvets;

namespace TR.BIDSSMemLib
{
  public class InputDeviceBVE5 : IInputDevice
  {
    public event InputEventHandler LeverMoved;
    public event InputEventHandler KeyDown;
    public event InputEventHandler KeyUp;

    class Axis
    {
      internal const int FuncKey = -1;
      internal const int ATSKey = -2;
      internal const int Reverser = 0;
      internal const int Power = 1;
      internal const int Brake = 2;
      internal const int SHandle = 3;
      internal const int Positive = 1;
      internal const int Negative = 0;
    }

    bool[] k = new bool[CtrlInput.KeyArrSizeMax];
    Hands h = new Hands();

    public void Configure(IWin32Window owner)
    => MessageBox.Show(owner, "BIDS Shared Memory Library\nBVE5 Input Device Plugin File\nVersion : "
    + Assembly.GetExecutingAssembly().GetName().Version.ToString(), Assembly.GetExecutingAssembly().GetName().Name);

    CtrlInput ci = null;
    public void Dispose() => ci?.Dispose();

    public void Load(string settingsPath) => ci = new CtrlInput();

    bool IsOneHandle = false;
    int MaxB = 0;
    int[] MaxP = new int[2] { 0, 0 };
    public void SetAxisRanges(int[][] ranges)
    {
      IsOneHandle = ranges[Axis.SHandle][Axis.Negative] < 0 && 0 < ranges[Axis.SHandle][Axis.Positive];
      MaxB = ranges[Axis.Brake][Axis.Positive];
      MaxP = ranges[Axis.Power];
    }

    public void Tick()
    {
      Hands hd = ci?.GetHandD() ?? new Hands();
      bool[] kd = ci?.GetIsKeyPushed() ?? new bool[CtrlInput.KeyArrSizeMax];
      if (!Equals(h, hd))
      {
        LM(Axis.Reverser, hd.R);
        if (hd.B == 0 && hd.P == 0 && (hd.BPos != 0 || hd.PPos != 0))
        {
          hd.P = (int)Math.Round(hd.PPos * MaxP[hd.PPos < 0 ? Axis.Negative : Axis.Positive], MidpointRounding.AwayFromZero);
          hd.B = (int)Math.Round(hd.BPos * MaxB, MidpointRounding.AwayFromZero);
        }
        if (IsOneHandle)
        {
          int Pos = -hd.B;
          if (Pos == 0) Pos = hd.P;
          LM(Axis.SHandle, Pos);
        }
        else
        {
          LM(Axis.Power, hd.P);
          LM(Axis.Brake, hd.B);
        }
        h = hd;
      }
      for (int i = 0; i < 20; i++)
      {
        if (k[i] != kd[i]) KE(i, kd[i]);
      }

      k = kd;
    }

    private void LM(int axis, int val) => LeverMoved?.Invoke(null, new InputEventArgs(axis, val));
    private void KE(int index, bool NewState)
    {
      bool IsFunc = index < 4;
      var iea = new InputEventArgs(IsFunc ? Axis.FuncKey : Axis.ATSKey, index - (IsFunc ? 0 : 4));

      if (NewState) KeyDown?.Invoke(null, iea);
      else KeyUp?.Invoke(null, iea);
    }
  }
}
