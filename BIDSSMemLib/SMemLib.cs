using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;


namespace TR.BIDSSMemLib
{
  public partial class SMemLib : IDisposable
  {
    static private readonly uint BSMDsize = (uint)Marshal.SizeOf(typeof(BIDSSharedMemoryData));
    /// <summary>BIDSSharedMemoryのデータ(互換性確保)</summary>
    public BIDSSharedMemoryData BIDSSMemData { get { return __BIDSSMemData; } private set { BIDSSMemChanged?.Invoke(value, new BSMDChangedEArgs() { NewData = value, OldData = __BIDSSMemData }); __BIDSSMemData = value; } }
    private BIDSSharedMemoryData __BIDSSMemData = new BIDSSharedMemoryData();

    /// <summary>OpenBVEでのみ得られるデータ(open専用)</summary>
    public OpenD OpenData { get { return __OpenD; } private set { OpenDChanged?.Invoke(value, new OpenDChangedEArgs() { NewData = value, OldData = __OpenD }); __OpenD = value; } }
    private OpenD __OpenD = new OpenD();

    /// <summary>駅情報</summary>
    /*public StaD Stations { get { return __StaD; } private set { __StaD = value; StaDChanged?.Invoke(value, new EventArgs()); } }
    private StaD __StaD = new StaD();*/

    /// <summary>Panel配列情報</summary>
    public PanelD Panels { get { return __PanelD; } private set { __PanelD = value; PanelDChanged?.Invoke(value, new EventArgs()); } }
    private PanelD __PanelD = new PanelD();

    /// <summary>Sound配列情報</summary>
    public SoundD Sounds { get { return __SoundD; } private set { __SoundD = value; SoundDChanged?.Invoke(value, new EventArgs()); } }
    private SoundD __SoundD = new SoundD();

    private bool IsMother { get; }
    private MemoryMappedFile MMFB { get; } = null;
    private MemoryMappedFile MMFO { get; } = null;
    //private MemoryMappedFile MMFS { get; set; } = null;
    private MemoryMappedFile MMFPn { get; set; } = null;
    private MemoryMappedFile MMFSn { get; set; } = null;



    /// <summary>SharedMemoryを初期化する。</summary>
    /// <param name="IsThisMother">書き込む側かどうか</param>
    /// <param name="ModeNum">モード番号</param>
    public SMemLib(bool IsThisMother = false, byte ModeNum = 0)
    {
      IsMother = IsThisMother;
      if (ModeNum >= 4) throw new ArgumentOutOfRangeException("ModeNumは3以下である必要があります。本ライブラリでReqモードはサポートされません。");
      try
      {
        if (ModeNum == 1) { MMFB = MemoryMappedFile.CreateOrOpen("BIDSSharedMemory", BSMDsize); return; }
        if (ModeNum <= 3)
        {
          MMFO = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryO", Marshal.SizeOf(OpenData));
        }
        if (ModeNum <= 2)
        {
          using (var sz = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryPn", sizeof(int))?.CreateViewAccessor(0, sizeof(int)))
          {
            var size = sz?.ReadInt32(0);
            if (size != null && size > 0) MMFPn = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryPn", (int)size);
          }
        }
        if (ModeNum == 0)
        {
          /*using (var sz = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryS", sizeof(int))?.CreateViewAccessor(0, sizeof(int)))
          {
            var size = sz?.ReadInt32(0);
            if (size != null && size > 0) MMFS = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryS", (int)size);
          }*/
          //Stationは未サポート
          using (var sz = MemoryMappedFile.CreateOrOpen("BIDSSharedMemorySn", sizeof(int))?.CreateViewAccessor(0, sizeof(int)))
          {
            var size = sz?.ReadInt32(0);
            if (size != null && size > 0) MMFSn = MemoryMappedFile.CreateOrOpen("BIDSSharedMemorySn", (int)size);
          }
        }

      }
      catch (Exception) { throw; }
      BIDSSMemChanged += Events.OnBSMDChanged;
      OpenDChanged += Events.OnOpenDChanged;
    }

    /// <summary>SharedMemoryを解放する</summary>
    public void Dispose()
    {
      MMFB?.Dispose();
      MMFO?.Dispose();
      //MMFS?.Dispose();
      MMFPn?.Dispose();
      MMFSn?.Dispose();
    }
    /// <summary>共有メモリからデータを読み込む</summary>
    public void Read()
    {
      Read<BIDSSharedMemoryData>();
      Read<OpenD>();
      //Read<StaD>();
      Read<PanelD>();
      Read<SoundD>();
    }
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public object Read<T>(bool DoWrite = true) where T : struct
    {
      T v = default;
      if (v is BIDSSharedMemoryData b) return Read(out b, DoWrite);
      if (v is OpenD o) return Read(out o, DoWrite);
      //if (v is StaD s) return Read(out s, DoWrite);
      if (v is PanelD pn) return Read(out pn, DoWrite);
      if (v is SoundD sn) return Read(out sn, DoWrite);
      throw new InvalidOperationException("指定された型は使用できません。");
    }

    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public BIDSSharedMemoryData Read(out BIDSSharedMemoryData D,bool DoWrite = true) { D = new BIDSSharedMemoryData(); if ((bool)MMFB?.CreateViewAccessor().CanRead) { using (var m = MMFB?.CreateViewAccessor()) { try { m?.Read(0, out D);/*Error*/} catch (Exception e ){ Console.WriteLine(e); } } if (DoWrite && !Equals(BIDSSMemData, D)) BIDSSMemData = D; } return D; }
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public OpenD Read(out OpenD D, bool DoWrite = true) { D = new OpenD(); using (var m = MMFO?.CreateViewAccessor()) { m.Read(0, out D); } if (DoWrite && !Equals(OpenData, D)) OpenData = D; return D; }
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    //public StaD Read(out StaD D, bool DoWrite = true) { D = new StaD(); using (var m = MMFS?.CreateViewAccessor()) { m.Read(0, out D); } if (DoWrite) Stations = D; return D; }
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public PanelD Read(out PanelD D, bool DoWrite = true) { D = new PanelD(); using (var m = MMFPn?.CreateViewAccessor()) { m.Read(0, out D); } if (DoWrite && !Equals(Panels, D)) Panels = D; return D; }
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public SoundD Read(out SoundD D, bool DoWrite = true) { D = new SoundD(); using (var m = MMFPn?.CreateViewAccessor()) { m.Read(0, out D); } if (DoWrite && !Equals(Sounds, D)) Sounds = D; return D; }


    /// <summary>BIDSSharedMemoryData構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in BIDSSharedMemoryData D) => Write(D, 5);
    /// <summary>OpenD構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in OpenD D) => Write(D, 1);
    /// <summary>StaD構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    //public void Write(in StaD D) => Write(D, 4);
    /// <summary>Panel構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in PanelD D) => Write(D, 6);
    /// <summary>Sound構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in SoundD D) => Write(D, 7);


    private void Write(in object D,byte num)
    {
      switch (num)
      {
        case 0://ElapData
          break;
        case 1://OpenData
          if (!Equals((OpenD)D, OpenData)) { var e = OpenData = (OpenD)D; using (var m = MMFO?.CreateViewAccessor()) { m.Write(0, ref e); } }
          break;
        case 2://HandleInfo
          break;
        case 3://ConstantData
          break;
        /*case 4://Stations
          if (!Equals((StaD)D, Stations))
          {
            var e = (StaD)D;
            if (!Equals(Stations.Size, e.Size))
            {
              MMFS?.Dispose();
              MMFS = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryS", e.Size);
            }
            Stations = e;
            using (var m = MMFS?.CreateViewAccessor()) { m.Write(0, ref e); }
          }
          break;*/
        case 5:
          if (!Equals((BIDSSharedMemoryData)D, BIDSSMemData)) { var f = BIDSSMemData = (BIDSSharedMemoryData)D; using (var m = MMFB?.CreateViewAccessor()) { m?.Write(0, ref f); } }
          break;

        case 6://Panel
          if (!Equals((PanelD)D, Panels))
          {
            var e = (PanelD)D;
            if (!Equals(Panels.Size, e.Size))
            {
              MMFPn?.Dispose();
              MMFPn = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryPn", e.Size);
            }
            Panels = e;
            using (var m = MMFPn?.CreateViewAccessor()) { m.Write(0, ref e); }
          }
          break;
        case 7://Sound
          if (!Equals((SoundD)D, Sounds))
          {
            var e = (SoundD)D;
            if (!Equals(Sounds.Size, e.Size))
            {
              MMFSn?.Dispose();
              MMFSn = MemoryMappedFile.CreateOrOpen("BIDSSharedMemorySn", e.Size);
            }
            Sounds = e;
            using (var m = MMFSn?.CreateViewAccessor()) { m.Write(0, ref e); }
          }
          break;
        default:
          throw new NotImplementedException("Your operation number is Not Implemented.  Please use 0 ~ 7");
      }
    }
  }
}
