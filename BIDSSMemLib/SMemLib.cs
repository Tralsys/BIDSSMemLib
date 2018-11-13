using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace TR.BIDSSMemLib
{
  public class SMemLib:IDisposable
  {
    /// <summary>車両のスペック(互換性確保)</summary>
    public struct Spec
    {
      /// <summary>ブレーキ段数</summary>
      public int B;
      /// <summary>ノッチ段数</summary>
      public int P;
      /// <summary>ATS確認段数</summary>
      public int A;
      /// <summary>常用最大段数</summary>
      public int J;
      /// <summary>編成車両数</summary>
      public int C;
    };
    /// <summary>車両の状態(互換性確保)</summary>
    public struct State
    {
      /// <summary>列車位置[m]</summary>
      public double Z;
      /// <summary>列車速度[km/h]</summary>
      public float V;
      /// <summary>0時からの経過時間[ms]</summary>
      public int T;
      /// <summary>BC圧力[kPa]</summary>
      public float BC;
      /// <summary>MR圧力[kPa]</summary>
      public float MR;
      /// <summary>ER圧力[kPa]</summary>
      public float ER;
      /// <summary>BP圧力[kPa]</summary>
      public float BP;
      /// <summary>SAP圧力[kPa]</summary>
      public float SAP;
      /// <summary>電流[A]</summary>
      public float I;
    };
    /// <summary>車両のハンドル位置(互換性確保)</summary>
    public struct Hand
    {
      /// <summary>ブレーキハンドル位置</summary>
      public int B;
      /// <summary>ノッチハンドル位置</summary>
      public int P;
      /// <summary>レバーサーハンドル位置</summary>
      public int R;
      /// <summary>定速制御状態</summary>
      public int C;
    };
    /// <summary>BIDSSharedMemoryのデータ構造体(互換性確保)</summary>
    public struct BIDSSharedMemoryData
    {
      /// <summary>SharedMemoryが有効かどうか</summary>
      public bool IsEnabled;
      /// <summary>SharedRAMの構造バージョン</summary>
      public int VersionNum;
      /// <summary>車両スペック情報</summary>
      public Spec SpecData;
      /// <summary>車両状態情報</summary>
      public State StateData;
      /// <summary>ハンドル位置情報</summary>
      public Hand HandleData;
      /// <summary>ドアが閉まっているかどうか</summary>
      public bool IsDoorClosed;
      /// <summary>Panelの表示番号配列</summary>
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      public int[] Panel;
      /// <summary>Soundの値配列</summary>
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
      public int[] Sound;
    };

    static private readonly uint BSMDsize = (uint)Marshal.SizeOf(typeof(BIDSSharedMemoryData));

    /// <summary>毎フレームごとに取得できるデータ(本家/open共通)</summary>
    public struct ElapD
    {
      /// <summary>情報が有効かどうか</summary>
      public bool IsEnabled { get; set; }
      /// <summary>クラスバージョン</summary>
      public readonly int Ver;
      /// <summary>列車位置[m]</summary>
      public double Location { get; set; }
      /// <summary>列車速度[km/h]</summary>
      public double Speed { get; set; }
      /// <summary>BC圧力[kPa]</summary>
      public double BCPres { get; set; }
      /// <summary>MR圧力[kPa]</summary>
      public double MRPres { get; set; }
      /// <summary>ER圧力[kPa]</summary>
      public double ERPres { get; set; }
      /// <summary>BP圧力[kPa]</summary>
      public double BPPres { get; set; }
      /// <summary>SAP圧力[kPa]</summary>
      public double SAPPres { get; set; }
      /// <summary>電流[A]</summary>
      public double Current { get; set; }
      /// <summary>(未実装)架線電圧[V]</summary>
      public double Voltage { get; set; }
      /// <summary>0時からの経過時間[ms]</summary>
      public int Time { get; set; }
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
    public struct OElapD
    {
      /// <summary>情報が有効かどうか</summary>
      public bool IsEnabled { get; set; }
      /// <summary>クラスバージョン</summary>
      public readonly int Ver;
      /// <summary>現在のカーブ半径[m]</summary>
      public double Radius { get; set; }
      /// <summary>現在のカントの大きさ[mm]</summary>
      public double Cant { get; set; }
      /// <summary>現在の軌間[mm]</summary>
      public double Pitch { get; set; }
      /// <summary>1フレーム当たりの時間[ms]</summary>
      public ushort ElapTime { get; set; }
      /// <summary>先行列車に関する情報</summary>
      public PreTrainD PreTrain { get; set; }

      public struct PreTrainD
      {
        /// <summary>情報が有効かどうか</summary>
        public bool IsEnabled { get; set; }
        /// <summary>先行列車の尻尾の位置[m]</summary>
        public double Location { get; set; }
        /// <summary>先行列車の尻尾までの距離[m]</summary>
        public double Distance { get; set; }
        /// <summary>先行列車の速度[km/h]</summary>
        public double Speed { get; set; }
      }
    }

    /// <summary>ハンドルに関する情報</summary>
    public struct HandleD
    {
      /// <summary>クラスバージョン</summary>
      public readonly int Ver;
      /// <summary>情報が有効かどうか</summary>
      public bool IsEnabled { get; set; }
      /// <summary>レバーサー位置</summary>
      public sbyte Reverser { get; set; }
      /// <summary>Pノッチ位置</summary>
      public short PNotch { get; set; }
      /// <summary>Bノッチ位置</summary>
      public ushort BNotch { get; set; }
      /// <summary>単弁位置</summary>
      public ushort LocoBNotch { get; set; }
      /// <summary>定速制御の有効状態</summary>
      public bool ConstSP { get; set; }
    }

    /// <summary>駅に関するデータ</summary>
    public struct StaD
    {
      /// <summary>データサイズ</summary>
      public int Size => Marshal.SizeOf(StaList);
      public List<StationData> StaList { get; set; }
      /// <summary>駅に関するデータ</summary>
      public struct StationData
      {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
        /// <summary>駅名(64Byteまで)</summary>
        public char[] Name;
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
    public struct ControlD
    {
      /// <summary>クラスバージョン</summary>
      public readonly int Ver;
      /// <summary>情報が有効かどうか</summary>
      public bool IsEnabled { get; set; }
      /// <summary>レバーサー位置</summary>
      public sbyte Reverser { get; set; }
      /// <summary>Pノッチ位置</summary>
      public short PNotch { get; set; }
      /// <summary>Bノッチ位置</summary>
      public ushort BNotch { get; set; }
      /*
      /// <summary>キー押下状態</summary>
      public IDictionary<KeyCode,bool> Keys { get; set; }
      /// <summary>キー名称一覧</summary>
      public enum KeyCode {
        S = 78, A1, A2, B1, B2, C1, C2, D, E, F, G, H, I, J, K, L, M, N, O, P,
        WiperSpeedUp, WiperSpeedDown, FillFuel, Headlights, LiveSteamInjector, ExhaustSteamInjector, IncreaseCutoff, DecreaseCutoff, Blowers,
        EngineStart, EngineStop, GearUp, GearDown, RaisePantograph, LowerPantograph, MainBreaker=113,
        LeftDoors = 21, RightDoors, PrimaryHorn, SecondaryHorn, MusicHorn,
        LocoBIncrease = 27, LocoBDecrease
      }*/
    }

    /// <summary>ドア状態情報</summary>
    public enum DoorInfo { Open,Close,RightOpen,LeftOpen }


    /// <summary> BIDSSMemDataが更新された際に呼ばれるイベント </summary>
    public event EventHandler BIDSSMemChanged;
    /// <summary>BIDSSharedMemoryのデータ(互換性確保)</summary>
    public BIDSSharedMemoryData BIDSSMemData { get { return __BIDSSMemData; } private set { __BIDSSMemData = value; BIDSSMemChanged?.Invoke(value, new EventArgs()); } }
    private BIDSSharedMemoryData __BIDSSMemData = new BIDSSharedMemoryData();

    /// <summary> ElapDが更新された際に呼ばれるイベント </summary>
    public event EventHandler ElapDChanged;
    /// <summary>毎フレームごとのデータ(本家/open共通)</summary>
    public ElapD ElapData { get { return __ElapD; } private set { __ElapD = value; ElapDChanged?.Invoke(value, new EventArgs()); } }
    private ElapD __ElapD = new ElapD();

    /// <summary> OElapDが更新された際に呼ばれるイベント </summary>
    public event EventHandler OElapDChanged;
    /// <summary>毎フレームごとのデータ(open専用)</summary>
    public OElapD OElapData { get { return __OElapD; } private set { __OElapD = value; OElapDChanged?.Invoke(value, new EventArgs()); } }
    private OElapD __OElapD = new OElapD();

    /// <summary> HandleDが更新された際に呼ばれるイベント </summary>
    public event EventHandler HandleDChanged;
    /// <summary>ハンドルに関する情報</summary>
    public HandleD HandleInfo { get { return __HandleD; } private set { __HandleD = value; HandleDChanged?.Invoke(value, new EventArgs()); } }
    private HandleD __HandleD = new HandleD();

    /// <summary> DoorInfoが更新された際に呼ばれるイベント </summary>
    public event EventHandler DoorInfoChanged;
    /// <summary>ドア状態情報</summary>
    public DoorInfo Door { get { return __DoorInfo; } private set { __DoorInfo = value; DoorInfoChanged?.Invoke(value, new EventArgs()); } }
    private DoorInfo __DoorInfo = new DoorInfo();

    /// <summary> ControlDが更新された際に呼ばれるイベント </summary>
    public event EventHandler ControlDChanged;
    /// <summary>各種操作情報</summary>
    public ControlD ControlData { get { return __ControlD; } set { __ControlD = value; ControlDChanged?.Invoke(value, new EventArgs()); } }
    private ControlD __ControlD = new ControlD();

    /// <summary> StaDが更新された際に呼ばれるイベント </summary>
    public event EventHandler StaDChanged;
    /// <summary>ドア状態情報</summary>
    public StaD Stations { get { return __StaD; } private set { __StaD = value; StaDChanged?.Invoke(value, new EventArgs()); } }
    private StaD __StaD = new StaD();

    private bool IsMother { get; }
    private MemoryMappedFile MMFB { get; } = null;
    private MemoryMappedFile MMFE { get; } = null;
    private MemoryMappedFile MMFO { get; } = null;
    private MemoryMappedFile MMFS { get; set; } = null;
    private MemoryMappedFile MMFH { get; } = null;
    private MemoryMappedFile MMFD { get; } = null;
    private MemoryMappedFile MMFC { get; } = null;

    /// <summary>SharedMemoryを初期化する。</summary>
    /// <param name="IsThisMother">書き込む側かどうか</param>
    public SMemLib(bool IsThisMother = false)
    {
      IsMother = IsThisMother;
      try
      {
        MMFB = MemoryMappedFile.CreateOrOpen("BIDSSharedMemory", BSMDsize);
        MMFE = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryE", Marshal.SizeOf(ElapData));
        MMFO = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryO", Marshal.SizeOf(OElapData));
        if (!IsMother) MMFS = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryS", sizeof(int));
        MMFH = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryH", Marshal.SizeOf(HandleInfo));
        MMFD = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryD", Marshal.SizeOf(Door));
        MMFC = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryC", Marshal.SizeOf(ControlData));
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
    /// <summary>SharedMemoryを解放する</summary>
    public void Dispose()
    {
      MMFB?.Dispose();
      MMFE?.Dispose();
      MMFO?.Dispose();
      MMFS?.Dispose();
      MMFH?.Dispose();
      MMFD?.Dispose();
      MMFC?.Dispose();
    }
    /// <summary>共有メモリからデータを読み込む</summary>
    public void Read()
    {
      Read<BIDSSharedMemoryData>();
      Read<ElapD>();
      Read<OElapD>();
      Read<HandleD>();
      Read<DoorInfo>();
      Read<ControlD>();
      Read<StaD>();
    }
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public object Read<T>(bool DoWrite = true) where T : struct
    {
      T v = default;
      if (v is BIDSSharedMemoryData b) return Read(out b, DoWrite);
      if (v is ElapD e) return Read(out e, DoWrite);
      if (v is OElapD o) return Read(out o, DoWrite);
      if (v is HandleD h) return Read(out h, DoWrite);
      if (v is DoorInfo d) return Read(out d, DoWrite);
      if (v is ControlD c) return Read(out c, DoWrite);
      if (v is StaD s) return Read(out s, DoWrite);

      throw new InvalidOperationException("指定された型は使用できません。");
    }

    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public BIDSSharedMemoryData Read(out BIDSSharedMemoryData D,bool DoWrite = true) { D = new BIDSSharedMemoryData(); using (var m = MMFB?.CreateViewAccessor()) { m.Read(0, out D); } if (DoWrite && !Equals(BIDSSMemData, D)) BIDSSMemData = D; return D; }
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public ElapD Read(out ElapD D, bool DoWrite = true) { D = new ElapD(); using (var m = MMFE?.CreateViewAccessor()) { m.Read(0, out D); } if (DoWrite && !Equals(ElapData, D)) ElapData = D; return D; }
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public OElapD Read(out OElapD D, bool DoWrite = true) { D = new OElapD(); using (var m = MMFO?.CreateViewAccessor()) { m.Read(0, out D); } if (DoWrite && !Equals(OElapData, D)) OElapData = D; return D; }
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public HandleD Read(out HandleD D, bool DoWrite = true) { D = new HandleD(); using (var m = MMFH?.CreateViewAccessor()) { m.Read(0, out D); } if (DoWrite && !Equals(HandleInfo, D)) HandleInfo = D; return D; }
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public DoorInfo Read(out DoorInfo D, bool DoWrite = true) { D = new DoorInfo(); using (var m = MMFD?.CreateViewAccessor()) { m.Read(0, out D); } if (DoWrite) Door = D; return D; }
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public ControlD Read(out ControlD D, bool DoWrite = true) { D = new ControlD(); using (var m = MMFC?.CreateViewAccessor()) { m.Read(0, out D); } if (DoWrite) ControlData = D; return D; }
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public StaD Read(out StaD D, bool DoWrite = true) { D = new StaD(); using (var m = MMFS?.CreateViewAccessor()) { m.Read(0, out D); } if (DoWrite) Stations = D; return D; }

    /// <summary>(互換性確保)BIDSSharedMemoryData構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in BIDSSharedMemoryData D) => Write(D, 5);
    /// <summary>ElapD構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in ElapD D) => Write(D, 0);
    /// <summary>OElapD構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in OElapD D) => Write(D, 1);
    /// <summary>HandleD構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in HandleD D) => Write(D, 2);
    /// <summary>DoorInfoを共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in DoorInfo D) => Write(D, 3);
    /// <summary>StaD構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in StaD D) => Write(D, 4);
    /// <summary>ControlD構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in ControlD D) => Write(D, 10);

    private void Write(in object D,byte num)
    {
      if (IsMother)
      {
        switch (num)
        {
          case 0://ElapData
            if (!Equals((ElapD)D, ElapData)) { var e = (ElapD)D; ElapData = e; using (var m = MMFE?.CreateViewAccessor()) { m.Write(0, ref e); } }
            break;
          case 1://OElapData
            if (!Equals((OElapD)D, OElapData)) { var e = (OElapD)D; OElapData = e; using (var m = MMFO?.CreateViewAccessor()) { m.Write(0, ref e); } }
            break;
          case 2://HandleInfo
            if (!Equals((HandleD)D, HandleInfo)) { var e = (HandleD)D; HandleInfo = e; using (var m = MMFH?.CreateViewAccessor()) { m.Write(0, ref e); } }
            break;
          case 3://DoorInfo
            if (!Equals((DoorInfo)D, Door)) { var e = (DoorInfo)D; Door = e; using (var m = MMFD?.CreateViewAccessor()) { m.Write(0, ref e); } }
            break;
          case 4://Stations
            if (!Equals((StaD)D, Stations)) {
              var e = (StaD)D;
              if (!Equals(Stations.Size, e.Size))
              {
                MMFS?.Dispose();
                MMFS = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryS", e.Size);
              }
              Stations = e;
              using (var m = MMFS?.CreateViewAccessor()) { m.Write(0, ref e); }
            }
            break;
          case 5:
            if (!Equals((BIDSSharedMemoryData)D, BIDSSMemData)) { var e = (BIDSSharedMemoryData)D; using (var m = MMFB?.CreateViewAccessor()) { m.Write(0, ref e); } }
            break;
          default:
            throw new TypeAccessException("MotherはHandleInfoに書き込みをすることができません。");
        }
      }
      else
      {
        if (num == 10)
        {
          if (!Equals((ControlD)D, ControlData)) { var e = (ControlD)D; ControlData = e; using (var m = MMFC?.CreateViewAccessor()) { m.Write(0, ref e); } }
        }
        else
        {
          throw new TypeAccessException("ChildはHandleInfo以外に書き込みをすることはできません。");
        }
      }
    }
  }
}
