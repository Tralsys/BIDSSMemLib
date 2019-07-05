using System;
using OpenBveApi.Runtime;
using OpenBveApi.Interface;
using OpenBveApi.FileSystem;
using System.Windows.Forms;
using System.Reflection;
using System.Threading.Tasks;

namespace TR.BIDSSMemLib
{
  static partial class UsefulFunc
  {
    public static float ToFloat(in this double d) => (float)d;
  }

  class InputDeviceOBVE : IInputDevice
  {
    public InputControl[] Controls { get; private set; } = new InputControl[8]
    {
      new InputControl(){ Command = Translations.Command.BrakeEmergency },
      new InputControl(){ Command = Translations.Command.ReverserAnyPostion, Option = -1 },
      new InputControl(){ Command = Translations.Command.ReverserAnyPostion, Option = 0 },
      new InputControl(){ Command = Translations.Command.ReverserAnyPostion, Option = 1 },
      new InputControl(){ Command = Translations.Command.BrakeDecrease },
      new InputControl(){ Command = Translations.Command.BrakeIncrease },
      new InputControl(){ Command = Translations.Command.PowerDecrease },
      new InputControl(){ Command = Translations.Command.PowerIncrease }
    };
    enum CtrlsName : int
    {
      EB, RB, RN, RF, BMinus, BPlus, PMinus, PPlus
    }
    public event EventHandler<InputEventArgs> KeyDown;
    public event EventHandler<InputEventArgs> KeyUp;

    public void Config(IWin32Window owner)
    {
      MessageBox.Show(owner, "BIDS Shared Memory Library\nOpenBVE Input Device Plugin File\nVersion : " + SMemLib.VersionNum, Assembly.GetExecutingAssembly().GetName().Name);
    }
    SMemLib SML = null;
    CtrlInput CI = null;
    public bool Load(FileSystem fileSystem)
    {
      try
      {
        SML = new SMemLib(true, 0);
        CI = new CtrlInput();
        return true;
      }
      catch (Exception e)
      {
        MessageBox.Show("BIDSSMemLib Loading Failed\n" + e.Message, Assembly.GetExecutingAssembly().FullName);
        return false;
      }
    }
    //bool[] KeyOld = new bool[CtrlInput.KeyArrSizeMax];
    Hands hds = new Hands();
    bool HandRUpdated = false;
    public void OnUpdateFrame()
    {
      Hands h = CI.GetHandD();
      if (h.B == 0 && h.P == 0 && (h.BPos != 0 || h.PPos != 0))
      {
        h.P = (int)Math.Round(h.PPos * hd.P, MidpointRounding.AwayFromZero);
        h.B = (int)Math.Round(h.BPos * hd.B, MidpointRounding.AwayFromZero);
      }

      if (!Equals(h.B, hds.B))
      {
        if (h.B >= (hd.B + 1))
        {
          KD(CtrlsName.EB);
          KU(CtrlsName.EB);
        }
        else
        {
          CtrlsName cn = 0;
          if (h.B > chp.B) cn = CtrlsName.BPlus;
          if (h.B < chp.B) cn = CtrlsName.BMinus;
          if (cn != 0)
          {
            for (int i = 0; i < (Math.Abs(h.B - chp.B)); i++) KD(cn);
            KU(cn);
          }
        }
      }

      if (!Equals(h.P, hds.P))
      {
        CtrlsName cn = 0;
        if (h.P > chp.P) cn = CtrlsName.PPlus;
        if (h.P < chp.P) cn = CtrlsName.PMinus;
        if (cn != 0)
        {
          for (int i = 0; i < (Math.Abs(h.P - chp.P)); i++) KD(cn);
          KU(cn);
        }
      }

      if (HandRUpdated) { KU(Controls[hds.R + 2]); HandRUpdated = false; }
      if (!Equals(h.R, hds.R)) { KD(Controls[hds.R + 2]); HandRUpdated = true; }

      //bool[] KeyI = CI.GetIsKeyPushed();
      hds = h;
    }

    private void KU(InputControl c) => KeyUp?.Invoke(this, new InputEventArgs(c));
    private void KD(InputControl c) => KeyDown?.Invoke(this, new InputEventArgs(c));
    private void KU(CtrlsName cn) => KU(Controls[(int)cn]);
    private void KD(CtrlsName cn) => KD(Controls[(int)cn]);
    Hand chp = new Hand();
    public void SetElapseData(ElapseData data)
    {
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
        VersionNum = int.Parse(SMemLib.VersionNum),
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
      SML?.Write(in BSMD);
      SML?.Write(in OD);
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
      SML?.Write(in BSMD);
      SML?.Write(in OD);
      SML?.Dispose();
    }
  }
}
