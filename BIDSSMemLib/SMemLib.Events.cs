using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TR.BIDSSMemLib
{
  internal static class UsefulFunc
  {
    static internal double MStoHH(this ref int ms) => ((double)ms) / 1000 / 60 / 60;
    static internal double MtoKM(this ref double m) => m / 1000;
    static internal double MtoKM(this ref float m) => m / 1000;
  }

  public partial class SMemLib
  {
	//イベントクラスとイベントを列挙
	/// <summary>細分データ更新イベント</summary>
    public class Events
    {
      /// <summary>車両情報が変化した際のイベントデータを格納</summary>
      public class SpecDataChangedEventArgs : EventArgs
      {
        /// <summary>ATS確認に必要な段数</summary>
        public int ATSCheck = 0;
        /// <summary>常用最大位置</summary>
        public int MaxServiceBrake = 0;
        /// <summary>力行段数</summary>
        public int MaxPower = 0;
        /// <summary>抑速(発電)段数</summary>
        public int MaxSuppression = 0;
        /// <summary>制動段数</summary>
        public int MaxBrake = 0;
        /// <summary>単弁段数</summary>
        public int MaxSelfBrake = 0;
        /// <summary>編成両数</summary>
        public int Cars = 0;
        /// <summary>台車がM台車かどうか(</summary>
        public bool[] IsMotorBogie;
        /// <summary>パンタグラフ種類(台車の位置と連動)</summary>
        public PantographType[] PantographTypes;

        public enum PantographType
        {
          None, OuterTrolley, InnerTrolley, Diamond, Cross, OuterJointSingleArm, InnerJointSingleArm
        }
      }
      /// <summary>速度が変化した際のイベントデータを格納</summary>
      public class SpeedChangedEventArgs : EventArgs
      {
        /// <summary>現在速度[km/h]</summary>
        public double Speed = 0;
        /// <summary>1つ前のイベントの際の速度[km/h]</summary>
        public double OldSpeed = 0;
        /// <summary>加速度[km/h/s]</summary>
        public double Acceleration = 0;
      }
      /// <summary>列車位置が変化した際のイベントデータを格納</summary>
      public class LocationChangedEventArgs : EventArgs
      {
        /// <summary>現在の列車位置[m]</summary>
        public double Location = 0;
        /// <summary>1つ前のイベントの際の列車位置[m]</summary>
        public double OldLocation = 0;
        /// <summary>加速度[km/h/s]</summary>
        public double Acceleration = 0;
      }
      /// <summary>圧力が変化した際のイベントデータを格納</summary>
      public class PressureChangedEventArgs : EventArgs
      {
        /// <summary>BC圧[kPa]</summary>
        public double BC = 0;
        /// <summary>MR圧[kPa]</summary>
        public double MR = 0;
        /// <summary>BP圧[kPa]</summary>
        public double BP = 0;
        /// <summary>ER圧[kPa]</summary>
        public double ER = 0;
        /// <summary>SAP圧[kPa]</summary>
        public double SAP = 0;
      }
      /// <summary>電気状態が変化した際のイベントデータを格納</summary>
      public class ElectrialStateChangedEventArgs : EventArgs
      {
        /// <summary>周波数[Hz]</summary>
        public byte Frequency = 0;
        /// <summary>架線電圧[V]</summary>
        public double OverheadWireVoltage = 0;
        /// <summary>バッテリー電圧[V]</summary>
        public double BatteryVoltage = 0;
        /// <summary>架線電圧[V]</summary>
        public double SIVVoltage = 0;
        /// <summary>電流[A]</summary>
        public double Current = 0;
      }


      /// <summary>車両情報が変化した際に発火</summary>
      public static event EventHandler<SpecDataChangedEventArgs> SpecChanged;
      /// <summary>速度情報が変化した際に発火</summary>
      public static event EventHandler<SpeedChangedEventArgs> SpeedChanged;
      /// <summary>列車位置情報が変化した際に発火</summary>
      public static event EventHandler<LocationChangedEventArgs> LocationChanged;
      /// <summary>圧力情報が変化した際に発火</summary>
      public static event EventHandler<PressureChangedEventArgs> PressChanged;
      /// <summary>電源情報が変化した際に発火</summary>
      public static event EventHandler<ElectrialStateChangedEventArgs> ElectricalStateChanged;

      private static double OldOldT = 0;
      private static double OldOldZ = 0;

      static internal void OnBSMDChanged(object sender, BSMDChangedEArgs e)
      {
        if (!Equals(e.OldData.SpecData, e.NewData.SpecData)) SpecChanged?.Invoke(e.NewData.SpecData, new SpecDataChangedEventArgs()
        {
          ATSCheck = e.NewData.SpecData.A,
          Cars = e.NewData.SpecData.C,
          MaxBrake = e.NewData.SpecData.B,
          MaxPower = e.NewData.SpecData.P,
          MaxServiceBrake = e.NewData.SpecData.J
        });
        if (!Equals(e.NewData.StateData, e.OldData.StateData))
        {
          State n = e.NewData.StateData;
          State o = e.OldData.StateData;
          if (n.BC != o.BC || n.BP != o.BP || n.ER != o.ER || n.MR != o.MR || n.SAP != o.SAP) PressChanged?.Invoke(n, new PressureChangedEventArgs()
          {
            BC = n.BC,
            BP = n.BP,
            ER = n.ER,
            MR = n.MR,
            SAP = n.SAP
          });

          if (n.Z != o.Z)
          {
            double a = 0;
            double odt, ndt, odz, ndz, ot, nt, ov, nv;
            double oldT = o.T.MStoHH();
            double newT = n.T.MStoHH();
            double oldZ = o.Z.MtoKM();
            double newZ = n.Z.MtoKM();
            if (n.T != o.T && OldOldT != o.T)
            {
              odt = oldT - OldOldT;
              ndt = newT - oldT;
              odz = oldZ - OldOldZ;
              ndz = newZ - oldZ;
              ov = odz / odt;
              nv = ndz / ndt;
              ot = OldOldT + (odt / 2);
              nt = oldT + (ndt / 2);
              if (ot != nt) a = (nv - ov) / (nt - ot);
            }
            LocationChanged?.Invoke(n.Z, new LocationChangedEventArgs() { Acceleration = a, Location = n.Z, OldLocation = o.Z });

            OldOldT = oldT;
            OldOldZ = oldZ;
          }
          if (n.V != o.V)
          {
            double a = 0;
            if (n.T != o.T) a = (n.V.MtoKM() - o.V.MtoKM()) / (n.T.MStoHH() - o.T.MStoHH());
            SpeedChanged?.Invoke(n.V, new SpeedChangedEventArgs() { Acceleration = a, OldSpeed = o.V, Speed = n.V });
          }
          if (n.I != o.I) ElectricalStateChanged?.Invoke(null, new ElectrialStateChangedEventArgs() { Current = n.I });
        }
      }
      
      static internal void OnOpenDChanged(object sender, OpenDChangedEArgs e)
      {

      }
    }

    public class BSMDChangedEArgs : EventArgs
    {
      public BIDSSharedMemoryData OldData;
      public BIDSSharedMemoryData NewData;
    }
    public class OpenDChangedEArgs : EventArgs
    {
      public OpenD OldData;
      public OpenD NewData;
    }


    /// <summary> BIDSSMemDataが更新された際に呼ばれるイベント </summary>
    public event EventHandler<BSMDChangedEArgs> BIDSSMemChanged;
    /// <summary> OpenDが更新された際に呼ばれるイベント </summary>
    public event EventHandler<OpenDChangedEArgs> OpenDChanged;
    /// <summary> StaDが更新された際に呼ばれるイベント </summary>
    //public event EventHandler StaDChanged;

    /// <summary> Panelが更新された際に呼ばれるイベント </summary>
    public event EventHandler PanelDChanged;
    /// <summary> Soundが更新された際に呼ばれるイベント </summary>
    public event EventHandler SoundDChanged;


  }
}
