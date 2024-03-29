﻿using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Forms;
using Mackoy.Bvets;

namespace TR.BIDSSMemLib
{
  public class InputDeviceBVE5 : IInputDevice
  {
    static InputDeviceBVE5()
		{
#if DEBUG
      if (!Debugger.IsAttached)
        Debugger.Launch();
#endif
    }

    public event InputEventHandler LeverMoved;
    public event InputEventHandler KeyDown;
    public event InputEventHandler KeyUp;

    static class Axis
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
    => MessageBox.Show(owner, "BIDS Shared Memory Library\nBveTs Input Device Plugin File\nVersion : "
    + Assembly.GetExecutingAssembly().GetName().Version.ToString(), Assembly.GetExecutingAssembly().GetName().Name);

    public void Dispose() { }

    public void Load(string settingsPath) { }

    int MaxB = 0;
    int[] MaxP = new int[2] { 0, 0 };
    public void SetAxisRanges(int[][] ranges)
    {
      MaxB = ranges[Axis.Brake][Axis.Positive];
      MaxP = ranges[Axis.Power];
    }

    public void Tick()
    {
      Hands hd = CtrlInput.GetHandD();
      bool[] kd = CtrlInput.GetIsKeyPushed() ?? new bool[CtrlInput.KeyArrSizeMax];

      if (!Equals(h, hd))
      {
        if (hd.R != h.R)
          LM(Axis.Reverser, hd.R); //更新時のみ適用

        if (hd.B == 0 && hd.P == 0 && (hd.BPos != 0 || hd.PPos != 0)) // 無段階入力用
        {
          // B=0, P=0にセットされていた場合のみ, 無段階入力を受け付ける

          hd.P = (int)Math.Round(hd.PPos * MaxP[hd.PPos < 0 ? Axis.Negative : Axis.Positive], MidpointRounding.AwayFromZero);
          hd.B = (int)Math.Round(hd.BPos * MaxB, MidpointRounding.AwayFromZero);
        }

        if (h.P != hd.P)
          LM(Axis.Power, hd.P); //更新時のみ適用

        //先にPを適用してからBを適用 => ワンハンドル車を2ハンドルで運転したとき, Pを投入しながらBを投入された場合にBのみを有効化するため
        if (h.B != hd.B) //更新時のみ適用
        {
          LM(Axis.Brake, hd.B);

          if (hd.B == 0) //Bが0のとき, Pを再適用することでワンハンドルでも正常に動作するようにする (B0でハンドルが0にセットされてしまうため)
            LM(Axis.Power, hd.P);
        }

        //履歴をｾｯﾄ
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
