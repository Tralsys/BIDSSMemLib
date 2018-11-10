using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace TR.BIDSSMemLib
{
  public class SMemLib:IDisposable
  {
    /// <summary>毎フレームごとに取得できるデータ(本家/open共通)</summary>
    public struct ElapD
    {
      /// <summary>情報が有効かどうか</summary>
      public bool IsEnabled { get; internal set; }
      /// <summary>クラスバージョン</summary>
      public readonly int Ver;
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
    public struct OElapD
    {
      /// <summary>情報が有効かどうか</summary>
      public bool IsEnabled { get; internal set; }
      /// <summary>クラスバージョン</summary>
      public readonly int Ver;
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

      public struct PreTrainD
      {
        /// <summary>情報が有効かどうか</summary>
        public bool IsEnabled { get; internal set; }
        /// <summary>先行列車の尻尾の位置[m]</summary>
        public double Location { get; internal set; }
        /// <summary>先行列車の尻尾までの距離[m]</summary>
        public double Distance { get; internal set; }
        /// <summary>先行列車の速度[km/h]</summary>
        public double Speed { get; internal set; }
      }
    }

    /// <summary>ハンドルに関する情報</summary>
    public struct HandleD
    {
      /// <summary>クラスバージョン</summary>
      public readonly int Ver;
      /// <summary>情報が有効かどうか</summary>
      public bool IsEnabled { get; internal set; }
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
    public struct StaD
    {
      /// <summary>データサイズ</summary>
      public int Size => Marshal.SizeOf(StaList);
      public List<StationData> StaList { get; internal set; }
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
      public bool IsEnabled { get; internal set; }
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
    private MemoryMappedFile MMFE { get; } = null;
    private MemoryMappedFile MMFO { get; } = null;
    private MemoryMappedFile MMFS { get; } = null;
    private MemoryMappedFile MMFH { get; } = null;
    private MemoryMappedFile MMFD { get; } = null;
    private MemoryMappedFile MMFC { get; } = null;
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
      MMFC?.Dispose();
    }
    private ElapD EpD = new ElapD();
    private OElapD OEpD = new OElapD();
    private HandleD HeD = new HandleD();
    private DoorInfo DD = new DoorInfo();
    private ControlD ClD = new ControlD();
    private StaD SnD = new StaD();

    /// <summary>
    /// 共有メモリからデータを読み込む
    /// </summary>
    public void Read()
    {
      ElapData = Equals(Read(out EpD, false), ElapData) ? ElapData : EpD;
      OElapData = Equals(Read(out OEpD, false), OElapData) ? OElapData : OEpD;
      HandleInfo = Equals(Read(out HeD, false), HandleInfo) ? HandleInfo : HeD;
      Door = Equals(Read(out DD, false), Door) ? Door : DD;
      ControlData = Equals(Read(out ClD, false), ControlData) ? ControlData : ClD;
      Stations = Equals(Read(out SnD, false), Stations) ? Stations : SnD;
    }

    public object Read<T>(bool DoWrite = true) where T : struct
    {
      T v = default;
      if (v is ElapD e) return Read(out e, DoWrite);
      if (v is OElapD o) return Read(out o, DoWrite);
      if (v is HandleD h) return Read(out h, DoWrite);
      if (v is DoorInfo d) return Read(out d, DoWrite);
      if (v is ControlD c) return Read(out c, DoWrite);
      if (v is StaD s) return Read(out s, DoWrite);

      throw new InvalidOperationException("指定された型は使用できません。");
    }

    /// <summary>
    /// 共有メモリからデータを読み込む
    /// </summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public ElapD Read(out ElapD D, bool DoWrite = true) { D = new ElapD(); using (var m = MMFE?.CreateViewAccessor()) { m.Read(0, out D); } if (DoWrite && !Equals(ElapData, D)) ElapData = D; return D; }
    /// <summary>
    /// 共有メモリからデータを読み込む
    /// </summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public OElapD Read(out OElapD D, bool DoWrite = true) { D = new OElapD(); using (var m = MMFO?.CreateViewAccessor()) { m.Read(0, out D); } if (DoWrite && !Equals(OElapData, D)) OElapData = D; return D; }
    /// <summary>
    /// 共有メモリからデータを読み込む
    /// </summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public HandleD Read(out HandleD D, bool DoWrite = true) { D = new HandleD(); using (var m = MMFH?.CreateViewAccessor()) { m.Read(0, out D); } if (DoWrite && !Equals(HandleInfo, D)) HandleInfo = D; return D; }
    /// <summary>
    /// 共有メモリからデータを読み込む
    /// </summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public DoorInfo Read(out DoorInfo D, bool DoWrite = true) { D = new DoorInfo(); using (var m = MMFD?.CreateViewAccessor()) { m.Read(0, out D); } if (DoWrite) Door = D; return D; }
    /// <summary>
    /// 共有メモリからデータを読み込む
    /// </summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public ControlD Read(out ControlD D, bool DoWrite = true) { D = new ControlD(); using (var m = MMFC?.CreateViewAccessor()) { m.Read(0, out D); } if (DoWrite) ControlData = D; return D; }
    /// <summary>
    /// 共有メモリからデータを読み込む
    /// </summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public StaD Read(out StaD D, bool DoWrite = true) { D = new StaD(); using (var m = MMFS?.CreateViewAccessor()) { m.Read(0, out D); } if (DoWrite) Stations = D; return D; }

    /// <summary>
    /// ElapD構造体の情報を共有メモリに書き込む
    /// </summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in ElapD D) => Write(D, 0);
    /// <summary>
    /// OElapD構造体の情報を共有メモリに書き込む
    /// </summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in OElapD D) => Write(D, 1);
    /// <summary>
    /// HandleD構造体の情報を共有メモリに書き込む
    /// </summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in HandleD D) => Write(D, 2);
    /// <summary>
    /// DoorInfoを共有メモリに書き込む
    /// </summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in DoorInfo D) => Write(D, 3);
    /// <summary>
    /// StaD構造体の情報を共有メモリに書き込む
    /// </summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in StaD D) => Write(D, 4);
    /// <summary>
    /// ControlD構造体の情報を共有メモリに書き込む
    /// </summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in ControlD D) => Write(D, 10);

    private void Write(in object D,byte num)
    {

      if (IsMother)
      {
        switch (num)
        {
          case 0://ElapData
            if (true) { ElapD e = (ElapD)D; ElapData = e; using (var m = MMFE?.CreateViewAccessor()) { m.Write(0, ref e); } }
            break;
          case 1://OElapData
            if (true) { OElapD e = (OElapD)D; OElapData = e; using (var m = MMFO?.CreateViewAccessor()) { m.Write(0, ref e); } }
            break;
          case 2://HandleInfo
            if (true) { HandleD e = (HandleD)D; HandleInfo = e; using (var m = MMFH?.CreateViewAccessor()) { m.Write(0, ref e); } }
            break;
          case 3://DoorInfo
            if (true) { DoorInfo e = (DoorInfo)D; Door = e; using (var m = MMFD?.CreateViewAccessor()) { m.Write(0, ref e); } }
            break;
          case 4:
            if (true) { StaD e = (StaD)D; Stations = e; using (var m = MMFS?.CreateViewAccessor()) { m.Write(0, ref e); } }
            break;
          default:
            throw new TypeAccessException("MotherはHandleInfoに書き込みをすることができません。");
        }
      }
      else
      {
        if (num == 10)
        {
          if (true) { ControlD e = (ControlD)D; ControlData = e; using (var m = MMFC?.CreateViewAccessor()) { m.Write(0, ref e); } }
        }
        else
        {
          throw new TypeAccessException("ChildはHandleInfo以外に書き込みをすることはできません。");
        }
      }
    }
  }
}
