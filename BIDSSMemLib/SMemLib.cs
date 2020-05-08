using System;

namespace TR.BIDSSMemLib
{
  public partial class SMemLib : IDisposable
  {
    public const string VersionNum = "202";
    public bool NO_SMEM_MODE { get; } = false;

    public const string MMFB_Name = "BIDSSharedMemory";
    public const string MMFO_Name = "BIDSSharedMemoryO";
    public const string MMFPn_Name = "BIDSSharedMemoryPn";
    public const string MMFSn_Name = "BIDSSharedMemorySn";

    private SMemCtrler<BIDSSharedMemoryData> SMC_BSMD = null;
    private SMemCtrler<OpenD> SMC_OpenD = null;
    private ArrDSMemCtrler<int> SMC_PnlD = null;
    private ArrDSMemCtrler<int> SMC_SndD = null;
    //private ArrDSMemCtrler<StaD> SMC_StaD = null;

    private SMC_ARSupport<BIDSSharedMemoryData> ARS_BSMD = null;
    private SMC_ARSupport<OpenD> ARS_OpenD = null;
    private SMC_ARSupport<int[]> ARS_PnlD = null;
    private SMC_ARSupport<int[]> ARS_SndD = null;

    /// <summary>BIDSSharedMemoryのデータ</summary>
    public BIDSSharedMemoryData BIDSSMemData
    {
      get => SMC_BSMD?.Data ?? default;
      private set => SMC_BSMD.Data = value;
    }
    /// <summary>OpenBVEでのみ得られるデータ(open専用)</summary>
    public OpenD OpenData
    {
      get => SMC_OpenD?.Data ?? default;
      private set => SMC_OpenD.Data = value;
    }

    ///// <summary>駅情報</summary>
    /*
    public StaD Stations
    {
      get => SMC_StaD?.Data ?? default;
      private set => SMC_StaD.Data = value;
    }*/

    /// <summary>Panel配列情報</summary>
    public PanelD Panels
    {
      get => new PanelD() { Panels = SMC_PnlD?.Data ?? new int[0] };

      private set => SMC_PnlD.Data = value.Panels;
    }
    public int[] PanelA
    {
      get => SMC_PnlD?.Data;
      set => SMC_PnlD.Data = value;
    }

    /// <summary>Sound配列情報</summary>
    public SoundD Sounds
    {
      get => new SoundD() { Sounds = SMC_SndD?.Data ?? new int[0] };
      private set => SMC_SndD.Data = value.Sounds;
    }
    public int[] SoundA
    {
      get => SMC_SndD?.Data;
      set => SMC_SndD.Data = value;
    }

    private bool IsMother { get; }

    /// <summary>SharedMemoryを初期化する。</summary>
    /// <param name="IsThisMother">書き込む側かどうか</param>
    /// <param name="ModeNum">モード番号</param>
    public SMemLib(byte ModeNum = 0, bool IsThisMother = false, bool isNoSMemMode = false)
    {
      NO_SMEM_MODE = isNoSMemMode;

      Console.WriteLine(isNoSMemMode ? "NO_SMEM_MODE Enabled" : "SMemLib Started");
      
      IsMother = IsThisMother;

      if (ModeNum >= 4) throw new ArgumentOutOfRangeException("ModeNumは3以下である必要があります。");

			#region SMemに接続するか否かをモードチェックにより設定
			//0 : Full Mode (BSMD, Open, Panel, Sound)
			//1 : v200 Mode (BSMD)
			//2 : Lite+ Mode (BSMD, Open, Panel)
			//3 : Lite Mode (BSMD, Open)
			bool BSMD_NO_SMem = isNoSMemMode;//モードによらない
      bool OpenD_NO_SMem = isNoSMemMode || ModeNum switch
      {
        0 => false,//使用する
        1 => true,//使用しない
        2 => false,
        3 => false,
        _ => true
      };
      bool PnlD_NO_SMem = isNoSMemMode || ModeNum switch
      {
        0 => false,
        1 => true,
        2 => false,
        3 => true,
        _ => true
      };
      bool SndD_NO_SMem = isNoSMemMode || ModeNum switch
      {
        0 => false,
        1 => true,
        2 => true,
        3 => true,
        _ => true
      };
			#endregion

			SMC_BSMD = new SMemCtrler<BIDSSharedMemoryData>(MMFB_Name, BSMD_NO_SMem);
      SMC_OpenD = new SMemCtrler<OpenD>(MMFO_Name, OpenD_NO_SMem);
      SMC_PnlD = new ArrDSMemCtrler<int>(MMFPn_Name, PnlD_NO_SMem);
      SMC_SndD = new ArrDSMemCtrler<int>(MMFSn_Name, SndD_NO_SMem);

      ARS_BSMD = new SMC_ARSupport<BIDSSharedMemoryData>(SMC_BSMD);
      ARS_OpenD = new SMC_ARSupport<OpenD>(SMC_OpenD);
      ARS_PnlD = new SMC_ARSupport<int[]>(SMC_PnlD);
      ARS_SndD = new SMC_ARSupport<int[]>(SMC_SndD);

      SMC_BSMD.ValueChanged += SMC_BSMD_ValueChanged;
      SMC_OpenD.ValueChanged += SMC_OpenD_ValueChanged;
      SMC_PnlD.ValueChanged += SMC_PnlD_ValueChanged;
      SMC_SndD.ValueChanged += SMC_SndD_ValueChanged;
      SMC_BSMD.ValueChanged += Events.OnBSMDChanged;
    }

    /// <summary>共有メモリからデータを読み込む</summary>
    public void Read()
    {
      if (NO_SMEM_MODE) return;
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
    public BIDSSharedMemoryData Read(out BIDSSharedMemoryData D, bool DoWrite = true)
    {
      SMC_BSMD.Read(out D, DoWrite);
      return D;
    }
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public OpenD Read(out OpenD D, bool DoWrite = true)
    {
      SMC_OpenD.Read(out D, DoWrite);
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
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public PanelD Read(out PanelD D, bool DoWrite = true)
    {
      D = new PanelD() { Panels = SMC_PnlD?.Read(DoWrite) ?? new int[0] };
      return D;
    }
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public SoundD Read(out SoundD D, bool DoWrite = true)
    {
      D = new SoundD() { Sounds = SMC_SndD?.Read(DoWrite) ?? new int[0] };
      return D;
    }

    /// <summary>BIDSSharedMemoryData構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in BIDSSharedMemoryData D) => SMC_BSMD?.Write(D);
    /// <summary>OpenD構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in OpenD D) => SMC_OpenD?.Write(D);
    /// <summary>StaD構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    //public void Write(in StaD D) => Write(D, 4);
    /// <summary>Panel構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in PanelD D) => SMC_PnlD?.Write(D.Panels);
    /// <summary>Sound構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in SoundD D) => SMC_SndD?.Write(D.Sounds);

    #region IDisposable Support
    private bool disposedValue = false;

    protected virtual void Dispose(bool disposing)
    {
      if (!disposedValue)
      {
        if (disposing)
        {
        }
        ReadStop();

        SMC_BSMD?.Dispose();
        SMC_OpenD?.Dispose();
        SMC_PnlD?.Dispose();
        SMC_SndD?.Dispose();

        disposedValue = true;
      }
    }
    public void Dispose() => Dispose(true);
    #endregion
  }
}
