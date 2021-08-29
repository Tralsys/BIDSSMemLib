using System;
using OpenBveApi.Runtime;
using OpenBveApi.Interface;
using OpenBveApi.FileSystem;
using System.Windows.Forms;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;

namespace TR.BIDSSMemLib
{
  static partial class UsefulFunc
  {
    public static float ToFloat(in this double d) => (float)d;
  }

  class InputDeviceOBVE : IInputDevice
  {
    public InputControl[] Controls { get; private set; } = new InputControl[(PMaxIndex * 2) + 300];
    //Power :    0- 999
    //Brake : 1000-1999
    //Revers: 2000(R),1(N),2(F)
    //Keys  : 2100-
    const int PMaxIndex = 1000;
    const int RevBIndex = PMaxIndex * 2;
    const int RevNIndex = PMaxIndex * 2 + 1;
    const int RevFIndex = PMaxIndex * 2 + 2;
    const int EBIndex = PMaxIndex * 2 + 3;

    public event EventHandler<InputEventArgs> KeyDown;
    public event EventHandler<InputEventArgs> KeyUp;

    public void Config(IWin32Window owner)
    {
      MessageBox.Show(owner, "BIDS Shared Memory Library\nOpenBVE Input Device Plugin File\nVersion : " + StaticSMemLib.VersionNum, Assembly.GetExecutingAssembly().GetName().Name);
    }

    public bool Load(FileSystem fileSystem)
    {
      Parallel.For(0, PMaxIndex, (i) =>
      {
        Controls[i] = new InputControl() { Command = Translations.Command.PowerAnyNotch, Option = i };
        Controls[PMaxIndex + i] = new InputControl() { Command = Translations.Command.BrakeAnyNotch, Option = i };
      });

      Controls[RevBIndex] = new InputControl() { Command = Translations.Command.ReverserAnyPostion, Option = -1 };
      Controls[RevNIndex] = new InputControl() { Command = Translations.Command.ReverserAnyPostion, Option = 0 };
      Controls[RevFIndex] = new InputControl() { Command = Translations.Command.ReverserAnyPostion, Option = 1 };

      Controls[EBIndex] = new InputControl() { Command = Translations.Command.BrakeEmergency};

      Parallel.For(0, CtrlInput.KeyArrSizeMax, (i) =>
      {
        Controls[(PMaxIndex * 2) + 100 + i] = new InputControl() { Command = (Translations.Command)i };
      });

      try
      {
        StaticSMemLib.Begin(false, true);
        return true;
      }
      catch (Exception e)
      {
        MessageBox.Show("BIDSSMemLib Loading Failed\n" + e.Message, Assembly.GetExecutingAssembly().FullName);
        return false;
      }
    }

    //bool[] KeyOld = new bool[CtrlInput.KeyArrSizeMax];
    bool EBUpdated = false;
    int? HandRIndex = null;
    int? HandBIndex = null;
    int? HandPIndex = null;
    int LastPPos = 0;
    public void OnUpdateFrame()
    {
      if (EBUpdated) { KU(Controls[EBIndex]); EBUpdated = false; }
      if (HandBIndex != null) { KU(Controls[PMaxIndex + HandBIndex ?? 0]); HandBIndex = null; }
      if (HandPIndex != null) { KU(Controls[HandPIndex ?? 0]); HandPIndex = null; }
      if (HandRIndex != null) { KU(Controls[RevNIndex+HandRIndex??0]); HandRIndex = null; }

      Hands h = CtrlInput.GetHandD();
      if (h.P > hd.P) h.P = hd.P;
      if (h.B == 0 && h.P == 0 && (h.BPos != 0 || h.PPos != 0))
      {
        h.P = (int)Math.Round(h.PPos * hd.P, MidpointRounding.AwayFromZero);
        h.B = (int)Math.Round(h.BPos * hd.B, MidpointRounding.AwayFromZero);
      }

      if (!Equals(h.B, chp.B))
      {
        if (h.B >= (hd.B + 1))
        {
          KD(Controls[EBIndex]);
          EBUpdated = true;
        }
        else
        {
          if (h.B >= PMaxIndex) h.B = PMaxIndex - 1;
          KD(Controls[PMaxIndex + h.B]);
          HandBIndex = h.B;
        }
      }

      if (!Equals(h.P, LastPPos))
      {
        if (h.P >= PMaxIndex) h.P = PMaxIndex - 1;
        KD(Controls[h.P]);
        HandPIndex = h.P;
        LastPPos = h.P;
      }

      if (!Equals(h.R, chp.R)) { KD(Controls[RevNIndex + h.R]); HandRIndex = h.R; }

      //bool[] KeyI = CI.GetIsKeyPushed();
    }

    private void KU(InputControl c) => KeyUp?.Invoke(this, new InputEventArgs(c));
    private void KD(InputControl c) => KeyDown?.Invoke(this, new InputEventArgs(c));

    Hand chp = new Hand();
    public void SetElapseData(ElapseData data)
    {
      chp.R = data.Handles.Reverser;
      chp.P = data.Handles.PowerNotch;
      chp.B = data.Handles.BrakeNotch;
      BIDSSharedMemoryData BSMD = new BIDSSharedMemoryData()
      {
        HandleData = new Hand()
        {
          B = data.Handles.BrakeNotch,
          P = data.Handles.PowerNotch,
          R = data.Handles.Reverser,
          C = data.Handles.ConstSpeed ? 1 : 0
        },
        StateData = new State()
        {
          BC = data.Vehicle.BcPressure.ToFloat(),
          BP = data.Vehicle.BpPressure.ToFloat(),
          ER = data.Vehicle.ErPressure.ToFloat(),
          I = 0,
          MR = data.Vehicle.MrPressure.ToFloat(),
          SAP = data.Vehicle.SapPressure.ToFloat(),
          T = (int)data.TotalTime.Milliseconds,
          V = data.Vehicle.Speed.KilometersPerHour.ToFloat(),
          Z = data.Vehicle.Location
        },
        IsDoorClosed = data.DoorInterlockState == DoorInterlockStates.Locked,
        IsEnabled = true,
        VersionNum = int.Parse(StaticSMemLib.VersionNum),
        SpecData = new Spec() { B = hd.B, P = hd.P }
      };
      OpenD OD = new OpenD()
      {
        Cant = data.Vehicle.Cant,
        ElapTime = data.ElapsedTime.Milliseconds,
        IsEnabled = true,
        Pitch = data.Vehicle.Pitch,
        Radius = data.Vehicle.Radius,
        SelfBPosition = data.Handles.LocoBrakeNotch,
      };
      if (data.PrecedingVehicle != null)
      {
        OD.PreTrain = new OpenD.PreTrainD()
        {
          IsEnabled = true,
          Distance = data.PrecedingVehicle.Distance,
          Location = data.PrecedingVehicle.Location,
          Speed = data.PrecedingVehicle.Speed.KilometersPerHour
        };
      }
      else OD.PreTrain = new OpenD.PreTrainD() { IsEnabled = false };
      StaticSMemLib.Write(in BSMD);
      StaticSMemLib.Write(in OD);
    }

    Hand hd = new Hand();
    public void SetMaxNotch(int powerNotch, int brakeNotch)
    {
      hd.P = powerNotch;
      hd.B = brakeNotch;
    }

    public void Unload()
    {
      BIDSSharedMemoryData BSMD = new BIDSSharedMemoryData();
      OpenD OD = new OpenD();
      StaticSMemLib.Write(in BSMD);
      StaticSMemLib.Write(in OD);
      //StaticSMemLib.Dispose();//static化に伴い不要になる
    }
  }
}
