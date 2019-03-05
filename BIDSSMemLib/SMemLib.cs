using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace TR.BIDSSMemLib
{
  public partial class SMemLib : IDisposable
  {
    static private readonly uint BSMDsize = (uint)Marshal.SizeOf(typeof(BIDSSharedMemoryData));



    /// <summary>BIDSSharedMemoryのデータ(互換性確保)</summary>
    public BIDSSharedMemoryData BIDSSMemData { get { return __BIDSSMemData; } private set { __BIDSSMemData = value; BIDSSMemChanged?.Invoke(value, new EventArgs()); } }
    private BIDSSharedMemoryData __BIDSSMemData = new BIDSSharedMemoryData();

    /// <summary>毎フレームごとのデータ(本家/open共通)</summary>
    public ElapD ElapData { get { return __ElapD; } private set { __ElapD = value; ElapDChanged?.Invoke(value, new EventArgs()); } }
    private ElapD __ElapD = new ElapD();

    /// <summary>毎フレームごとのデータ(open専用)</summary>
    public OElapD OElapData { get { return __OElapD; } private set { __OElapD = value; OElapDChanged?.Invoke(value, new EventArgs()); } }
    private OElapD __OElapD = new OElapD();

    /// <summary>ハンドルに関する情報</summary>
    public HandleD HandleInfo { get { return __HandleD; } private set { __HandleD = value; HandleDChanged?.Invoke(value, new EventArgs()); } }
    private HandleD __HandleD = new HandleD();

    /// <summary>ドア状態情報</summary>
    public StaD Stations { get { return __StaD; } private set { __StaD = value; StaDChanged?.Invoke(value, new EventArgs()); } }
    private StaD __StaD = new StaD();

    /// <summary>Panel配列情報</summary>
    public PanelD Panels { get { return __PanelD; } private set { __PanelD = value; PanelDChanged?.Invoke(value, new EventArgs()); } }
    private PanelD __PanelD = new PanelD();

    /// <summary>Sound配列情報</summary>
    public SoundD Sounds { get { return __SoundD; } private set { __SoundD = value; SoundDChanged?.Invoke(value, new EventArgs()); } }
    private SoundD __SoundD = new SoundD();

    private bool IsMother { get; }
    private MemoryMappedFile MMFB { get; } = null;
    private MemoryMappedFile MMFE { get; } = null;
    private MemoryMappedFile MMFO { get; } = null;
    private MemoryMappedFile MMFS { get; set; } = null;
    private MemoryMappedFile MMFH { get; } = null;
    private MemoryMappedFile MMFPn { get; set; } = null;
    private MemoryMappedFile MMFSn { get; set; } = null;

    /// <summary>SharedMemoryを初期化する。</summary>
    /// <param name="IsThisMother">書き込む側かどうか</param>
    public SMemLib(bool IsThisMother = false)
    {
      IsMother = IsThisMother;
      try
      {
        if (!IsMother)
        {
          using (var sz = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryS", sizeof(int))?.CreateViewAccessor(0, sizeof(int)))
          {
            var size = sz?.ReadInt32(0);
            if (size != null && size > 0) MMFS = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryS", (int)size);
          }
          using (var sz = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryPn", sizeof(int))?.CreateViewAccessor(0, sizeof(int)))
          {
            var size = sz?.ReadInt32(0);
            if (size != null && size > 0) MMFPn = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryPn", (int)size);
          }
          using (var sz = MemoryMappedFile.CreateOrOpen("BIDSSharedMemorySn", sizeof(int))?.CreateViewAccessor(0, sizeof(int)))
          {
            var size = sz?.ReadInt32(0);
            if (size != null && size > 0) MMFS = MemoryMappedFile.CreateOrOpen("BIDSSharedMemorySn", (int)size);
          }
        }
        MMFB = MemoryMappedFile.CreateOrOpen("BIDSSharedMemory", BSMDsize);
        MMFE = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryE", Marshal.SizeOf(ElapData));
        MMFO = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryO", Marshal.SizeOf(OElapData));
        MMFH = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryH", Marshal.SizeOf(HandleInfo));
      }
      catch (Exception) { throw; }
    }
    /// <summary>SharedMemoryを解放する</summary>
    public void Dispose()
    {
      MMFB?.Dispose();
      MMFE?.Dispose();
      MMFO?.Dispose();
      MMFS?.Dispose();
      MMFH?.Dispose();
      MMFPn?.Dispose();
      MMFSn?.Dispose();
    }
    /// <summary>共有メモリからデータを読み込む</summary>
    public void Read()
    {
      Read<BIDSSharedMemoryData>();
      Read<ElapD>();
      Read<OElapD>();
      Read<HandleD>();
      Read<StaD>();
      Read<PanelD>();
      Read<SoundD>();
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
      if (v is StaD s) return Read(out s, DoWrite);
      if (v is PanelD) return Read(out s, DoWrite);
      if (v is SoundD) return Read(out s, DoWrite);
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
    public StaD Read(out StaD D, bool DoWrite = true) { D = new StaD(); using (var m = MMFS?.CreateViewAccessor()) { m.Read(0, out D); } if (DoWrite) Stations = D; return D; }
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public PanelD Read(out PanelD D, bool DoWrite = true) { D = new PanelD(); using (var m = MMFPn?.CreateViewAccessor()) { m.Read(0, out D); } if (DoWrite && !Equals(Panels, D)) Panels = D; return D; }
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public SoundD Read(out SoundD D, bool DoWrite = true) { D = new SoundD(); using (var m = MMFPn?.CreateViewAccessor()) { m.Read(0, out D); } if (DoWrite && !Equals(Sounds, D)) Sounds = D; return D; }


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
    /// <summary>StaD構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in StaD D) => Write(D, 4);
    /// <summary>Panel構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in PanelD D) => Write(D, 6);
    /// <summary>Sound構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in SoundD D) => Write(D, 6);

    private void Write(in object D,byte num)
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
        case 4://Stations
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
          break;
        case 5:
          if (!Equals((BIDSSharedMemoryData)D, BIDSSMemData)) { var e = (BIDSSharedMemoryData)D; using (var m = MMFB?.CreateViewAccessor()) { m.Write(0, ref e); } }
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
