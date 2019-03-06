using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TR.BIDSSMemLib
{
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
      public event EventHandler<SpecDataChangedEventArgs> SpecChanged;
      /// <summary>速度情報が変化した際に発火</summary>
      public event EventHandler<SpeedChangedEventArgs> SpeedChanged;
      /// <summary>列車位置情報が変化した際に発火</summary>
      public event EventHandler<LocationChangedEventArgs> LocationChanged;
      /// <summary>圧力情報が変化した際に発火</summary>
      public event EventHandler<PressureChangedEventArgs> PressChanged;
      /// <summary>電源情報が変化した際に発火</summary>
      public event EventHandler<ElectrialStateChangedEventArgs> ElectricalStateChanged;
    }
    /// <summary> BIDSSMemDataが更新された際に呼ばれるイベント </summary>
    public event EventHandler BIDSSMemChanged;
    /// <summary> ElapDが更新された際に呼ばれるイベント </summary>
    public event EventHandler ElapDChanged;
    /// <summary> OElapDが更新された際に呼ばれるイベント </summary>
    public event EventHandler OElapDChanged;
    /// <summary> HandleDが更新された際に呼ばれるイベント </summary>
    public event EventHandler HandleDChanged;
    /// <summary> StaDが更新された際に呼ばれるイベント </summary>
    //public event EventHandler StaDChanged;

    /// <summary> Panelが更新された際に呼ばれるイベント </summary>
    public event EventHandler PanelDChanged;
    /// <summary> Soundが更新された際に呼ばれるイベント </summary>
    public event EventHandler SoundDChanged;

    /// <summary> 固定情報が更新された際に呼ばれるイベント </summary>
    public event EventHandler ConstDChanged;
  }
}
