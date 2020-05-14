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
    private SMemCtrler<int> SMC_PnlD = null;
    private SMemCtrler<int> SMC_SndD = null;
    //private ArrDSMemCtrler<StaD> SMC_StaD = null;

    
    /// <summary>BIDSSharedMemoryのデータ</summary>
    public BIDSSharedMemoryData BIDSSMemData
    {
      get => SMC_BSMD?.Data ?? default;
      private set => SMC_BSMD?.Write(value);
    }
    /// <summary>OpenBVEでのみ得られるデータ(open専用)</summary>
    public OpenD OpenData
    {
      get => SMC_OpenD?.Data ?? default;
      private set => SMC_OpenD?.Write(value);
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
      get => new PanelD() { Panels = SMC_PnlD?.ArrData ?? new int[0] };

      private set => SMC_PnlD.WriteArr(value.Panels);
    }
    public int[] PanelA
    {
      get => SMC_PnlD?.ArrData;
      set => SMC_PnlD.WriteArr(value);
    }

    /// <summary>Sound配列情報</summary>
    public SoundD Sounds
    {
      get => new SoundD() { Sounds = SMC_SndD?.ArrData ?? new int[0] };
      private set => SMC_SndD.WriteArr(value.Sounds);
    }
    public int[] SoundA
    {
      get => SMC_SndD?.ArrData;
      set => SMC_SndD.WriteArr(value);
    }

    //private bool IsMother { get; }

    /// <summary>SharedMemoryを初期化する。</summary>
    /// <param name="IsThisMother">書き込む側かどうか</param>
    /// <param name="ModeNum">モード番号</param>
    public SMemLib(byte ModeNum = 0, bool IsThisMother = false, bool isNoSMemMode = false, bool isNoEventMode = false)
    {
      NO_SMEM_MODE = isNoSMemMode;

      Console.WriteLine(isNoSMemMode ? "NO_SMEM_MODE Enabled" : "SMemLib Started");
      
      //IsMother = IsThisMother;

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

			SMC_BSMD = new SMemCtrler<BIDSSharedMemoryData>(MMFB_Name, false, BSMD_NO_SMem, isNoEventMode);
      SMC_OpenD = new SMemCtrler<OpenD>(MMFO_Name, false, OpenD_NO_SMem, isNoEventMode);
      SMC_PnlD = new SMemCtrler<int>(MMFPn_Name, true, PnlD_NO_SMem, isNoEventMode);
      SMC_SndD = new SMemCtrler<int>(MMFSn_Name, true, SndD_NO_SMem, isNoEventMode);


      SMC_BSMD.ValueChanged += SMC_BSMD_ValueChanged;
      SMC_OpenD.ValueChanged += SMC_OpenD_ValueChanged;
      SMC_PnlD.ArrValueChanged += SMC_PnlD_ValueChanged;
      SMC_SndD.ArrValueChanged += SMC_SndD_ValueChanged;

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
      D = new PanelD() { Panels = SMC_PnlD?.ReadArr(DoWrite) ?? new int[0] };
      return D;
    }
    /// <summary>共有メモリからデータを読み込む</summary>
    /// <param name="D">読み込んだデータを書き込む変数</param>
    /// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
    public SoundD Read(out SoundD D, bool DoWrite = true)
    {
      D = new SoundD() { Sounds = SMC_SndD?.ReadArr(DoWrite) ?? new int[0] };
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
    public void Write(in PanelD D) => SMC_PnlD?.WriteArr(D.Panels);
    /// <summary>Sound構造体の情報を共有メモリに書き込む</summary>
    /// <param name="D">書き込む構造体</param>
    public void Write(in SoundD D) => SMC_SndD?.WriteArr(D.Sounds);

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
