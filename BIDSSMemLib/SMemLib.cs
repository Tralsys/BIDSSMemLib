using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace TR.BIDSSMemLib
{
  public class SMemLib:IDisposable
  {
    /// <summary>毎フレームごとに取得できるデータ(本家/open共通)</summary>
    public class ElapD
    {
      /// <summary>情報が有効かどうか</summary>
      public bool IsEnabled { get; internal set; } = false;
      /// <summary>クラスバージョン</summary>
      public readonly int Ver = 100;
      /// <summary>列車位置[m]</summary>
      public double Location { get; internal set; }
      /// <summary>列車速度[km/h]</summary>
      public double Speed { get; internal set; }
      /// <summary>BC圧力[kPa]</summary>
      public double BCPres { get; internal set; }
      /// <summary>MR圧力[kPa]</summary>
      public double MRPres { get; internal set; }
      /// <summary>ER圧力[kPa]</summary>
      public double ERPres { get; internal set; }
      /// <summary>BP圧力[kPa]</summary>
      public double BPPres { get; internal set; }
      /// <summary>SAP圧力[kPa]</summary>
      public double SAPPres { get; internal set; }
      /// <summary>電流[A]</summary>
      public double Current { get; internal set; }
      /// <summary>(未実装)架線電圧[V]</summary>
      public double Voltage { get; internal set; }
      /// <summary>0時からの経過時間[ms]</summary>
      public int Time { get; internal set; }
      /// <summary>現在時刻[時]</summary>
      public byte TimeHH { get { return (byte)TimeSpan.FromMilliseconds(Time).Hours; } }
      /// <summary>現在時刻[分]</summary>
      public byte TimeMM { get { return (byte)TimeSpan.FromMilliseconds(Time).Minutes; } }
      /// <summary>現在時刻[秒]</summary>
      public byte TimeSS { get { return (byte)TimeSpan.FromMilliseconds(Time).Seconds; } }
      /// <summary>現在時刻[ミリ秒]</summary>
      public short TimeMS { get { return (short)TimeSpan.FromMilliseconds(Time).Milliseconds; } }
    }

    /// <summary>毎フレームごとに取得できるデータ(open専用)</summary>
    public class OElapD
    {
      /// <summary>情報が有効かどうか</summary>
      public bool IsEnabled { get; internal set; } = false;
      /// <summary>クラスバージョン</summary>
      public readonly int Ver = 100;
      /// <summary>現在のカーブ半径[m]</summary>
      public double Radius { get; internal set; }
      /// <summary>現在のカントの大きさ[mm]</summary>
      public double Cant { get; internal set; }
      /// <summary>現在の軌間[mm]</summary>
      public double Pitch { get; internal set; }
      /// <summary>1フレーム当たりの時間[ms]</summary>
      public ushort ElapTime { get; internal set; }
      /// <summary>先行列車に関する情報</summary>
      public PreTrainD PreTrain { get; internal set; }

      public class PreTrainD
      {
        /// <summary>情報が有効かどうか</summary>
        public bool IsEnabled { get; internal set; } = false;
        /// <summary>先行列車の尻尾の位置[m]</summary>
        public double Location { get; internal set; }
        /// <summary>先行列車の尻尾までの距離[m]</summary>
        public double Distance { get; internal set; }
        /// <summary>先行列車の速度[km/h]</summary>
        public double Speed { get; internal set; }
      }
    }


    /// <summary>ハンドルに関する情報</summary>
    public class HandleD
    {
      /// <summary>クラスバージョン</summary>
      public readonly int Ver = 100;
      /// <summary>情報が有効かどうか</summary>
      public bool IsEnabled { get; internal set; } = false;
      /// <summary>レバーサー位置</summary>
      public sbyte Reverser { get; internal set; }
      /// <summary>Pノッチ位置</summary>
      public short PNotch { get; internal set; }
      /// <summary>Bノッチ位置</summary>
      public ushort BNotch { get; internal set; }
      /// <summary>単弁位置</summary>
      public ushort LocoBNotch { get; internal set; }
      /// <summary>定速制御の有効状態</summary>
      public bool ConstSP { get; internal set; }
    }

    /// <summary>駅に関するデータ</summary>
    public class StaD
    {
      /// <summary>データサイズ</summary>
      public int Size => Marshal.SizeOf(StaList);
      public List<StationData> StaList { get; internal set; }
      /// <summary>駅に関するデータ</summary>
      public class StationData
      {
        /// <summary>駅名</summary>
        public string Name;
        /// <summary>到着予定時刻[ms]</summary>
        public int ArrTime;
        /// <summary>発車予定時刻[ms]</summary>
        public int DepTime;
        /// <summary>停車時間[ms]</summary>
        public int StopTime;
        /// <summary>停止定位駅かどうか</summary>
        public bool IsSigUsuallyRed;
        /// <summary>左のドアが開くかどうか</summary>
        public bool IsLeftOpen;
        /// <summary>右のドアが開くかどうか</summary>
        public bool IsRightOpen;
        /// <summary>予定された発着番線</summary>
        public float DefaultTrack;
        /// <summary>停止位置[m]</summary>
        public double StopLocation;
        /// <summary>通過駅かどうか</summary>
        public bool IsPass;
        /// <summary>駅の種類</summary>
        public StaType Type;

        /// <summary>駅の種類</summary>
        public enum StaType { Normal, ChangeEnd, Termial, Stop }
      }
    }

    /// <summary>操作情報</summary>
    public class ControlD
    {
      /// <summary>クラスバージョン</summary>
      public readonly int Ver = 100;
      /// <summary>情報が有効かどうか</summary>
      public bool IsEnabled { get; internal set; } = false;
      /// <summary>レバーサー位置</summary>
      public sbyte Reverser { get; set; }
      /// <summary>Pノッチ位置</summary>
      public short PNotch { get; set; }
      /// <summary>Bノッチ位置</summary>
      public ushort BNotch { get; set; }
      /// <summary>単弁位置</summary>
      public ushort LocoBNotch { get; set; }
      /// <summary>キー押下状態</summary>
      public IDictionary<KeyCode,bool> Keys { get; set; }
      /// <summary>キー名称一覧</summary>
      public enum KeyCode {
        S=78, A1, A2, B1, B2, C1, C2, D, E, F, G, H, I, J, K, L, M, N, O, P,
        WiperSpeedUp, WiperSpeedDown, FillFuel, Headlights, LiveSteamInjector, ExhaustSteamInjector, IncreaseCutoff, DecreaseCutoff, Blowers,
        EngineStart, EngineStop, GearUp, GearDown, RaisePantograph, LowerPantograph, MainBreaker=113,
        LeftDoors =21, RightDoors, PrimaryHorn, SecondaryHorn, MusicHorn
      }
    }

    /// <summary>ドア状態情報</summary>
    public enum DoorInfo { Open,Close,RightOpen,LeftOpen }

    /// <summary>毎フレームごとのデータ(本家/open共通)</summary>
    public ElapD ElapData { get; private set; }
    /// <summary>毎フレームごとのデータ(open専用)</summary>
    public OElapD OElapData { get; private set; }
    /// <summary>ハンドルに関する情報</summary>
    public HandleD HandleInfo { get; private set; }
    /// <summary>ドア状態情報</summary>
    public DoorInfo Door { get; private set; }

    public StaD Stations { get; private set; }
    private bool IsMother { get; }
    MemoryMappedFile MMFE;
    MemoryMappedFile MMFO;
    MemoryMappedFile MMFS;
    MemoryMappedFile MMFH;
    MemoryMappedFile MMFD;
    /// <summary>
    /// SharedMemoryを初期化する。
    /// </summary>
    /// <param name="IsThisMother">書き込む側かどうか</param>
    public SMemLib(bool IsThisMother = false)
    {
      IsMother = IsThisMother;
      try
      {
        MMFE = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryE", Marshal.SizeOf(ElapData));
        MMFO = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryO", Marshal.SizeOf(OElapData));
        if (!IsMother) MMFS = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryS", sizeof(int));
        MMFH = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryH", Marshal.SizeOf(HandleInfo));
        MMFD = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryD", Marshal.SizeOf(Door));
      }
      catch (Exception) { throw; }
      if (!IsMother)
      {
        try
        {
          var sz = MMFS?.CreateViewAccessor(0, sizeof(int));
          var size = sz?.ReadInt32(0);
          sz?.Dispose();
          MMFS?.Dispose();
          if (size != null && size > 0) MMFS = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryS", (int)size);
        }
        catch (Exception) { throw; }
      }
    }
    /// <summary>
    /// SharedMemoryを解放する
    /// </summary>
    public void Dispose()
    {

      MMFE?.Dispose();
      MMFO?.Dispose();
      MMFS?.Dispose();
      MMFH?.Dispose();
      MMFD?.Dispose();
    }

    public void Read()
    {

    }
    public void BeginRead(int RefleshTime = 100)
    {

    }
    public void EndRead()
    {

    }
    public void Write(object Obj)
    {
      if (IsMother)
      {
        if (Obj.GetType() == ElapData.GetType()) return;
        if (Obj.GetType() == OElapData.GetType()) return;
        if (Obj.GetType() == HandleInfo.GetType()) return;
        if (Obj.GetType() == Door.GetType()) return;
        if (Obj.GetType() == Stations.GetType()) return;
      }
      else
      {
        if (Obj.GetType() == HandleInfo.GetType()) return;
      }
    }
  }
}
