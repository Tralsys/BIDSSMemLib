using System;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mackoy.Bvets;

namespace TR.BIDSSMemLib
{
  class InputDeviceBVE5 : IInputDevice
  {
    public event InputEventHandler LeverMoved;
    public event InputEventHandler KeyDown;
    public event InputEventHandler KeyUp;

	class Axis
    {
      internal const int FuncKey = -2;
      internal const int ATSKey = -1;
	  internal const int Reverser = 0;
      internal const int Power = 1;
      internal const int Brake = 2;
      internal const int SHandle = 3;
    }

    bool[] k = new bool[CtrlInput.KeyArrSizeMax];
    Hand h = new Hand();

    public void Configure(IWin32Window owner)
		=> MessageBox.Show(owner, "BIDS Shared Memory Library\nBVE5 Input Device Plugin File\nVersion : "
		+ Assembly.GetExecutingAssembly().GetName().Version.ToString(), Assembly.GetExecutingAssembly().GetName().Name);
    
    CtrlInput ci = null;
    public void Dispose() => ci?.Dispose();

    public void Load(string settingsPath) => ci = new CtrlInput();

    public void SetAxisRanges(int[][] ranges) { }

    public void Tick()
    {
      Hand hd = ci?.GetHandD() ?? new Hand();
      bool[] kd = ci?.GetIsKeyPushed() ?? new bool[CtrlInput.KeyArrSizeMax];
      if (!Equals(h, hd))
      {
        if (h.R != hd.R) LM(Axis.Reverser, hd.R);
        if (h.P != hd.P) LM(Axis.Power, hd.P);
        if (h.B != hd.B) LM(Axis.Brake, hd.B);
        h = hd;
      }
	  for(int i = 0; i < 20; i++)
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
