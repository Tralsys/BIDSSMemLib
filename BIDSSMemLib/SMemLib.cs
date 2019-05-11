using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;


namespace TR.BIDSSMemLib
{
  public partial class SMemLib : IDisposable
  {
    static private readonly uint BSMDsize = (uint)Marshal.SizeOf(typeof(BIDSSharedMemoryData));
    /// <summary>BIDSSharedMemoryのデータ(互換性確保)</summary>
    public BIDSSharedMemoryData BIDSSMemData
    {
      get { return __BIDSSMemData; }
      private set
      {
#if !bve5 || !obve
        BIDSSMemChanged?.Invoke(value, new BSMDChangedEArgs()
        {
          NewData = value,
          OldData = __BIDSSMemData
        });
#endif
        __BIDSSMemData = value;
      }
    }
    private BIDSSharedMemoryData __BIDSSMemData = new BIDSSharedMemoryData();
#if !bve5
    /// <summary>OpenBVEでのみ得られるデータ(open専用)</summary>
    public OpenD OpenData
    {
      get { return __OpenD; }
      private set
      {
#if !bve5 || !obve
        OpenDChanged?.Invoke(value, new OpenDChangedEArgs()
        {
          NewData = value,
          OldData = __OpenD
        });
#endif
        __OpenD = value;
      }
    }
    private OpenD __OpenD = new OpenD();

    /// <summary>駅情報</summary>
    /*
    public StaD Stations
    {
      get { return __StaD; }
      private set
      {
        __StaD = value;
#if !bve5 || !bve5
        StaDChanged?.Invoke(value, new EventArgs());
#endif
      }
    }
    private StaD __StaD = new StaD();*/
#endif
    /// <summary>Panel配列情報</summary>
    public PanelD Panels
    {
      get { return __PanelD; }
      private set
      {
        __PanelD = value;
#if !bve5 || !obve
        PanelDChanged?.Invoke(value, new EventArgs());
#endif
      }
    }
    private PanelD __PanelD = new PanelD();

    /// <summary>Sound配列情報</summary>
    public SoundD Sounds
    {
      get { return __SoundD; }
      private set
      {
        __SoundD = value;
#if !bve5 || obve
        SoundDChanged?.Invoke(value, new EventArgs());
#endif
      }
    }
    private SoundD __SoundD = new SoundD();
#if !reader
    private bool IsMother { get; }
#endif
    private MemoryMappedFile MMFB { get; } = null;
#if !obve
    private MemoryMappedFile MMFO { get; } = null;
    //private MemoryMappedFile MMFS { get; set; } = null;
#endif
    private MemoryMappedFile MMFPn { get; set; } = null;
    private MemoryMappedFile MMFSn { get; set; } = null;


#if !reader
    /// <summary>SharedMemoryを初期化する。</summary>
    /// <param name="IsThisMother">書き込む側かどうか</param>
    /// <param name="ModeNum">モード番号</param>
    public SMemLib(bool IsThisMother = false, byte ModeNum = 0)
    {
      IsMother = IsThisMother;
#else
    /// <summary>SharedMemoryを初期化する。</summary>
    /// <param name="ModeNum">モード番号</param>
    public SMemLib(byte ModeNum = 0)
    {
#endif
#if !bve5 || !obve
      BIDSSMemChanged += Events.OnBSMDChanged;
      //OpenDChanged += Events.OnOpenDChanged;
#endif
      if (ModeNum >= 4) throw new ArgumentOutOfRangeException("ModeNumは3以下である必要があります。本ライブラリでReqモードはサポートされません。");
      try
      {
        //0 : Full Mode (BSMD, Open, Panel, Sound)
        //1 : v200 Mode (BSMD)
        //2 : Lite+ Mode (BSMD, Open, Panel)
        //3 : Lite Mode (BSMD, Open)
        //4 : Req(Not Supported)
        if (ModeNum == 1) { MMFB = MemoryMappedFile.CreateOrOpen("BIDSSharedMemory", BSMDsize); return; }
        if (ModeNum <= 3)
        {
          MMFB = MemoryMappedFile.CreateOrOpen("BIDSSharedMemory", BSMDsize);
#if !bve5
          MMFO = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryO", Marshal.SizeOf(OpenData));
#endif
        }
        if (ModeNum <= 2)
        {
          int size = 0;
          using (var mmf = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryPn", sizeof(int)))
          using (var sz = mmf?.CreateViewAccessor(0, sizeof(int)))
          {
            size = (sz?.ReadInt32(0) ?? 256) * sizeof(int);
          }
          MMFPn = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryPn", size + sizeof(int));
        }
        if (ModeNum == 0)
        {
#if !bve5
          /*using (var sz = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryS", sizeof(int))?.CreateViewAccessor(0, sizeof(int)))
          {
            var size = sz?.ReadInt32(0);
            if (size != null && size > 0) MMFS = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryS", (int)size);
          }*/
          //Stationは未サポート
#endif
          int size = 0;
          using (var mmf = MemoryMappedFile.CreateOrOpen("BIDSSharedMemorySn", sizeof(int)))
          using (var sz = mmf?.CreateViewAccessor(0, sizeof(int)))
          {
            size = (sz?.ReadInt32(0) ?? 256) * sizeof(int);
          }
          MMFSn = MemoryMappedFile.CreateOrOpen("BIDSSharedMemorySn", size + sizeof(int));
        }

      }
      catch (Exception) { throw; }
    }

    /// <summary>SharedMemoryを解放する</summary>
    public void Dispose()
    {
      MMFB?.Dispose();
#if !bve5
      MMFO?.Dispose();
      //MMFS?.Dispose();
#endif
      MMFPn?.Dispose();
      MMFSn?.Dispose();
    }
    /// <summary>共有メモリからデータを読み込む</summary>
    public void Read()
    {
      Read<BIDSSharedMemoryData>();
#if !bve5
      Read<OpenD>();
      //Read<StaD>();
#endif
      Read<PanelD>();
      Read<SoundD>();
    }
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public object Read<T>(bool DoWrite = true) where T : struct
    {
      T v = default;
      if (v is BIDSSharedMemoryData b) return Read(out b, DoWrite);
#if !bve5
      if (v is OpenD o) return Read(out o, DoWrite);
      //if (v is StaD s) return Read(out s, DoWrite);
#endif
      if (v is PanelD pn) return Read(out pn, DoWrite);
      if (v is SoundD sn) return Read(out sn, DoWrite);
      throw new InvalidOperationException("指定された型は使用できません。");
    }

    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public BIDSSharedMemoryData Read(out BIDSSharedMemoryData D,bool DoWrite = true)
    {
      D = new BIDSSharedMemoryData();
      if ((bool)MMFB?.CreateViewAccessor().CanRead)
      {
        using (var m = MMFB?.CreateViewAccessor())
        {
          m?.Read(0, out D);
        }
        if (DoWrite && !Equals(BIDSSMemData, D)) BIDSSMemData = D;
      }
      return D;
    }
#if !bve5
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public OpenD Read(out OpenD D, bool DoWrite = true)
    {
      D = new OpenD();
      using (var m = MMFO?.CreateViewAccessor())
      {
        m?.Read(0, out D);
      }
      if (DoWrite && !Equals(OpenData, D)) OpenData = D;
      return D;
    }
    /*
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public StaD Read(out StaD D, bool DoWrite = true)
    {
      D = new StaD();
      using (var m = MMFS?.CreateViewAccessor())
      {
        m?.Read(0, out D);
      }
      if (DoWrite) Stations = D;
      return D;
    }*/
#endif
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public PanelD Read(out PanelD D, bool DoWrite = true)
    {
      D = new PanelD();
      bool IsReOpenNeeded = false;
      int Length = 0;
      using (var m = MMFPn?.CreateViewAccessor(0, sizeof(int)))
      {
        Length = m.ReadInt32(0);
        IsReOpenNeeded = (Length + 1) * sizeof(int) > m.Capacity;
      }
      var size = (Length + 1) * sizeof(int);
      if (IsReOpenNeeded)
      {
        MMFPn?.Dispose();
        MMFPn = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryPn", size);
      }
      using (var m = MMFPn?.CreateViewAccessor(0, size))
      {
        if (m != null) m.ReadArray(sizeof(int), D.Panels, 0, m.ReadInt32(0));
      }
      if (DoWrite && !Equals(Panels, D)) Panels = D;
      return D;
    }
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public SoundD Read(out SoundD D, bool DoWrite = true)
    {
      D = new SoundD();
      bool IsReOpenNeeded = false;
      int Length = 0;
      using (var m = MMFSn?.CreateViewAccessor())
      {
        if (m != null) {
          Length = m.ReadInt32(0);
          IsReOpenNeeded = (Length + 1) * sizeof(int) > m.Capacity;
          if (!IsReOpenNeeded) m.ReadArray(sizeof(int), D.Sounds, 0, m.ReadInt32(0));
        }
      }
      var size = (Length + 1) * sizeof(int);
      if (IsReOpenNeeded)
      {
        MMFSn?.Dispose();
        MMFSn = MemoryMappedFile.CreateOrOpen("BIDSSharedMemorySn", size);
        using (var m = MMFSn?.CreateViewAccessor(0, size))
        {
          if (m != null) m.ReadArray(sizeof(int), D.Sounds, 0, m.ReadInt32(0));
        }
      }
      if (DoWrite && !Equals(Sounds, D)) Sounds = D;
      return D;
    }

#if !reader
    /// <summary>BIDSSharedMemoryData構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in BIDSSharedMemoryData D) => Write(D, 5);
#if !bve5
    /// <summary>OpenD構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in OpenD D) => Write(D, 1);
    /// <summary>StaD構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    //public void Write(in StaD D) => Write(D, 4);
#endif
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
        if (!IsMother) throw new NotSupportedException("You are using this library as \"READER\", but your operation is WRITING THINGS OPERATION.\nPlease check your program.");
#if !bve5
        case 1://OpenData
          if (!Equals((OpenD)D, OpenData)) { var e = OpenData = (OpenD)D; using (var m = MMFO?.CreateViewAccessor()) { m.Write(0, ref e); } }
          break;
        /*case 4://Stations => Disabled
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
#endif
        case 5://BSMD
          if (!Equals((BIDSSharedMemoryData)D, BIDSSMemData)) { var f = BIDSSMemData = (BIDSSharedMemoryData)D; using (var m = MMFB?.CreateViewAccessor()) { m?.Write(0, ref f); } }
          break;

        case 6://Panel
          if (!Equals((PanelD)D, Panels))
          {
            var e = (PanelD)D;
            if (!Equals(Panels.Length, e.Length))
            {
              MMFPn?.Dispose();
              MMFPn = MemoryMappedFile.CreateOrOpen("BIDSSharedMemoryPn", e.Length * sizeof(int));
            }
            Panels = e;
            using (var m = MMFPn?.CreateViewAccessor())
            {
              if (m != null && m.CanWrite)
              {
                int size = e.Length * sizeof(int);
                m.Write(0, ref size);
                m.WriteArray(sizeof(int), e.Panels, 0, e.Length);
              }
            }
          }
          break;
        case 7://Sound
          if (!Equals((SoundD)D, Sounds))
          {
            var e = (SoundD)D;
            if (!Equals(Sounds.Length, e.Length))
            {
              MMFSn?.Dispose();
              MMFSn = MemoryMappedFile.CreateOrOpen("BIDSSharedMemorySn", e.Length * sizeof(int));
            }
            Sounds = e;
            using (var m = MMFSn?.CreateViewAccessor())
            {
              if (m != null && m.CanWrite)
              {
                int size = e.Length * sizeof(int);
                m.Write(0, ref size);
                m.WriteArray(sizeof(int), e.Sounds, 0, e.Length);
              }
            }
          }
          break;
        default:
          throw new NotImplementedException("Your operation number is Not Implemented.");
      }
    }
#endif
  }
}
