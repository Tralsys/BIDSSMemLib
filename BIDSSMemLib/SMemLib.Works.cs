using System;
using System.Threading.Tasks;

namespace TR.BIDSSMemLib
{
  public partial class SMemLib
  {
    /// <summary>AutoReadを開始します。実行中である場合、Exceptionをthrowします。</summary>
    /// <param name="ModeNum">自動読み取りを開始する情報種類</param>
    /// <param name="Interval">読み取り頻度[ms]</param>
    public void ReadStart(int ModeNum = 0, int Interval = 50)
    {
      if (NO_SMEM_MODE) return;
      switch (ModeNum)
      {
        case 1://OpenD
          if (SMC_OpenD?.No_SMem_Mode == false)
            ARS_OpenD?.Start(Interval);
          else throw new InvalidOperationException("OpenD共有メモリが有効化されていません。");
          break;
        case 5://BSMD
          if (SMC_BSMD?.No_SMem_Mode == false)
            ARS_BSMD?.Start(Interval);
          else throw new InvalidOperationException("BSMD共有メモリが有効化されていません。");
          break;
        case 6://PanelD
          if (SMC_PnlD?.No_SMem_Mode == false)
            ARS_PnlD?.Start(Interval);
          else throw new InvalidOperationException("PanelD共有メモリが有効化されていません。");
          break;
        case 7://Sound D
          if (SMC_SndD?.No_SMem_Mode == false)
            ARS_SndD?.Start(Interval);
          else throw new InvalidOperationException("SoundD共有メモリが有効化されていません。");
          break;
      }
      if (ModeNum <= 0) Parallel.For(1, 8, (i) => ReadStart(i, Interval));
    }

    

    /// <summary>AutoReadを終了します。実行中でなくともエラーは返しません。TimeOut:1000ms</summary>
    /// <param name="ModeNum">終了させる情報種類</param>
    public void ReadStop(int ModeNum = 0)
    {
      switch (ModeNum)
      {
        case 1://OpenD
          ARS_OpenD?.Stop();
          break;
        case 5://BSMD
          ARS_BSMD?.Stop();
          break;
        case 6://PanelD
          ARS_PnlD?.Stop();
          break;
        case 7://Sound D
          ARS_SndD?.Stop();
          break;
      }
      if (ModeNum <= 0) Parallel.For(1, 8, (int i) => ReadStop(i));
    }
  }
}
