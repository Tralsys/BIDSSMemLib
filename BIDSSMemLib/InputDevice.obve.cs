using System;
using OpenBveApi.Runtime;
using OpenBveApi.Interface;
using OpenBveApi.FileSystem;
using System.Windows.Forms;
using System.Reflection;

namespace TR.BIDSSMemLib
{
  static partial class UsefulFunc
  {
    public static float ToFloat(in this double d) => (float)d;
  }

  class InputDeviceOBVE : IInputDevice
  {
    public InputControl[] Controls { get; set; } = new InputControl[3];

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
      Controls[0].Command = Translations.Command.PowerAnyNotch;
      Controls[1].Command = Translations.Command.BrakeAnyNotch;
      Controls[2].Command = Translations.Command.ReverserAnyPostion;
      
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
    public void OnUpdateFrame()
    {
      Hand h = CI.GetHandD();
      Controls[0].Option = h.P;
      Controls[1].Option = h.B;
      Controls[2].Option = h.R;
      //bool[] KeyI = CI.GetIsKeyPushed();

    }

    public void SetElapseData(ElapseData data)
    {
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
        SpecData = new Spec() { B = HD.B, P = HD.P }
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
    Hand HD = new Hand();
    public void SetMaxNotch(int powerNotch, int brakeNotch)
    {
      HD.P = powerNotch;
      HD.B = brakeNotch;
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
