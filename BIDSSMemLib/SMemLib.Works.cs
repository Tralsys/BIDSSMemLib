using System;
using System.Threading;
using System.Threading.Tasks;

namespace TR.BIDSSMemLib
{
  public partial class SMemLib
  {
    Thread BSMDRead = null;
    Thread ODRead = null;
    Thread PDRead = null;
    Thread SDRead = null;

    bool BSMDReadDoing = false;
    bool ODReadDoing = false;
    bool PDReadDoing = false;
    bool SDReadDoing = false;

    /// <summary>AutoReadを開始します。実行中である場合、Exceptionをthrowします。</summary>
    /// <param name="ModeNum">自動読み取りを開始する情報種類</param>
    /// <param name="Interval">読み取り頻度[ms]</param>
    public void ReadStart(int ModeNum, int Interval = 50)
    {
      switch (ModeNum)
      {
        case 1://OpenD
          if (MMFO != null)
          {
            if (ODRead != null)
            {
              if (ODRead.ThreadState == ThreadState.Running) throw new InvalidOperationException("既に操作は実行されています。自動読み取りプロセスは動作中です。");
              ODReadDoing = false;
              ODRead.Join(1000);
            }
            ODRead = new Thread(ODReadMethod);
            ODReadDoing = true;
            ODRead.Start(Interval);
          }
          else throw new InvalidOperationException("共有メモリが有効化されていません。MMFOにはNULLが設定されています。");
          break;
        case 5://BSMD
          if (MMFB != null)
          {
            if (BSMDRead != null)
            {
              if (BSMDRead.ThreadState == ThreadState.Running) throw new InvalidOperationException("既に操作は実行されています。自動読み取りプロセスは動作中です。");
              BSMDReadDoing = false;
              BSMDRead.Join(1000);
            }
            BSMDRead = new Thread(BSMDReadMethBSMD);
            BSMDReadDoing = true;
            BSMDRead.Start(Interval);
          }
          else throw new InvalidOperationException("共有メモリが有効化されていません。MMFBにはNULLが設定されています。");
          break;
        case 6://PanelD
          if (MMFPn != null)
          {
            if (PDRead != null)
            {
              if (PDRead.ThreadState == ThreadState.Running) throw new InvalidOperationException("既に操作は実行されています。自動読み取りプロセスは動作中です。");
              PDReadDoing = false;
              PDRead.Join(1000);
            }
            PDRead = new Thread(PDReadMethPD);
            PDReadDoing = true;
            PDRead.Start(Interval);
          }
          else throw new InvalidOperationException("共有メモリが有効化されていません。MMFPnにはNULLが設定されています。");
          break;
        case 7://Sound D
          if (MMFSn != null)
          {
            if (SDRead != null)
            {
              if (SDRead.ThreadState == ThreadState.Running) throw new InvalidOperationException("既に操作は実行されています。自動読み取りプロセスは動作中です。");
              SDReadDoing = false;
              SDRead.Join(1000);
            }
            SDRead = new Thread(SDReadMethSD);
            SDReadDoing = true;
            SDRead.Start(Interval);
          }
          else throw new InvalidOperationException("共有メモリが有効化されていません。MMFSnにはNULLが設定されています。");
          break;
      }
    }

    //AutoRead Methods
    private void ODReadMethod(object o)
    {
      int Interval = (int)o;
      while (ODReadDoing)
      {
        Read<OpenD>();
        Thread.Sleep(Interval);
      }
    }
    private void BSMDReadMethBSMD(object o)
    {
      int Interval = (int)o;
      while (BSMDReadDoing)
      {
        Read<BIDSSharedMemoryData>();
        Thread.Sleep(Interval);
      }
    }
    private void PDReadMethPD(object o)
    {
      int Interval = (int)o;
      while (PDReadDoing)
      {
        Read<PanelD>();
        Thread.Sleep(Interval);
      }
    }
    private void SDReadMethSD(object o)
    {
      int Interval = (int)o;
      while (SDReadDoing)
      {
        Read<SoundD>();
        Thread.Sleep(Interval);
      }
    }

    /// <summary>AutoReadを終了します。実行中でなくともエラーは返しません。TimeOut:1000ms</summary>
    /// <param name="ModeNum">終了させる情報種類</param>
    public void ReadStop(int ModeNum)
    {
      switch (ModeNum)
      {
        case 1://OpenD
          ODReadDoing = false;
          if (ODRead != null)
          {
            try
            {
              ODRead.Join(1000);
            }
            catch { throw; }
          }
          ODRead = null;
          break;
        case 5://BSMD
          BSMDReadDoing = false;
          if (BSMDRead != null)
          {
            try
            {
              BSMDRead.Join(1000);
            }
            catch { throw; }
          }
          BSMDRead = null;
          break;
        case 6://PanelD
          PDReadDoing = false;
          if (PDRead != null)
          {
            try
            {
              PDRead.Join(1000);
            }
            catch { throw; }
          }
          PDRead = null;
          break;
        case 7://Sound D
          SDReadDoing = false;
          if (SDRead != null)
          {
            try
            {
              SDRead.Join(1000);
            }
            catch { throw; }
          }
          SDRead = null;
          break;
      }
      if (ModeNum < 0) Parallel.For(0, 8, (int i) => ReadStop(i));
    }
  }
}
