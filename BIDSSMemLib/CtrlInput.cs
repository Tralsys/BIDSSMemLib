using System;
using System.Runtime.InteropServices;

namespace TR.BIDSSMemLib
{
  public class CtrlInput : IDisposable
  {
    const string MMFCtrlKName = "BIDSSMemCtrlK";
    const string MMFCtrlHName = "BIDSSMemCtrlH";
    public const int KeyArrSizeMax = 128;

    SMemIF SMIF_CtrlH = null;
    SMemIF SMIF_CtrlK = null;

    public CtrlInput()
    {
      SMIF_CtrlK = new SMemIF(MMFCtrlKName, KeyArrSizeMax * sizeof(bool));
      SMIF_CtrlH = new SMemIF(MMFCtrlHName, Marshal.SizeOf(new Hands()));
    }

    public enum HandType
    {
      Reverser, Power, Brake, SelfB, PPos, BPos
    }

    /// <summary>キーの押下状態を取得する。</summary>
    /// <param name="Index">キー番号</param>
    /// <returns>キー押下状態</returns>
    public bool GetIsKeyPushed(int Index)
    {
      if (Index >= KeyArrSizeMax || Index < 0) throw new IndexOutOfRangeException("Please set 0 ~ 127.");
      bool data = false;
      SMIF_CtrlK.Read(Index * sizeof(bool), out data);
      return data;
    }
    /// <summary>キーの押下状態を取得する。</summary>
    /// <param name="Index">キー番号</param>
    /// <param name="data">取得した情報を格納する変数</param>
    public void GetIsKeyPushed(int Index, ref bool data) => data = GetIsKeyPushed(Index);
    /// <summary>キーの押下状態を指定の場所にすべて記録する。</summary>
    /// <param name="data">キー押下状態を格納する配列</param>
    public void GetIsKeyPushed(ref bool[] data) => SMIF_CtrlK.ReadArray(0, data, 0, KeyArrSizeMax);
    
    /// <summary>キーの押下状態を指定の場所にすべて記録する。</summary>
    /// <returns>キー押下状態を格納する配列</returns>
    public bool[] GetIsKeyPushed()
    {
      bool[] ra = new bool[KeyArrSizeMax];
      GetIsKeyPushed(ref ra);
      return ra;
    }
    /// <summary>指定のキー状態をMemoryMappedFileに記録する。</summary>
    /// <param name="Index">キー番号</param>
    /// <param name="data">キー状態</param>
    public void SetIsKeyPushed(int Index, in bool data)
    {
      if (Index >= KeyArrSizeMax || Index < 0) throw new IndexOutOfRangeException("Please set 0 ~ 127.");
      bool d = data;
      SMIF_CtrlK.Write(Index * sizeof(bool), ref d);
    }
    /// <summary>すべてのキー状態をMemoryMappedFileに記録する。</summary>
    /// <param name="data">キー状態</param>
    public void SetIsKeyPushed(in bool[] data)
    {
      if (!(data?.Length > 0)) return;
      SMIF_CtrlK.WriteArray(0, data, 0, data.Length);
    }
    
    /// <summary>ハンドル位置指令状態を取得する</summary>
    /// <returns>ハンドル位置指令状態</returns>
    public Hands GetHandD()
    {
      //ctr++;
      Hands hd = new Hands();
      GetHandD(ref hd);
      return hd;
    }
    /// <summary>ハンドル位置指令状態を取得する</summary>
    /// <param name="hd">ハンドル位置指令状態</param>
    public void GetHandD(ref Hands hd) => SMIF_CtrlK.Read(0, out hd);
    /// <summary>ハンドル位置を設定する</summary>
    /// <param name="hd">指定するハンドル位置</param>
    public void SetHandD(ref Hands hd) => SMIF_CtrlH.Write(0, ref hd);

    public void SetHandD(HandType ht, int value)
    {
      Hands hd = GetHandD();
      switch (ht)
      {
        case HandType.Brake:
          hd.B = value;
          break;
        case HandType.Power:
          hd.P = value;
          break;
        case HandType.Reverser:
          hd.R = value;
          break;
        case HandType.SelfB:
          hd.S = value;
          break;
      }
      SetHandD(ref hd);
    }
    public void SetHandD(HandType ht, double value)
    {
      Hands hd = GetHandD();
      switch (ht)
      {
        case HandType.Brake:
          hd.B = (int)value;
          break;
        case HandType.Power:
          hd.P = (int)value;
          break;
        case HandType.Reverser:
          hd.R = (int)value;
          break;
        case HandType.SelfB:
          hd.S = (int)value;
          break;
        case HandType.BPos:
          hd.BPos = value;
          break;
        case HandType.PPos:
          hd.PPos = value;
          break;
      }
      SetHandD(ref hd);
    }

    public void Dispose()
    {
      SMIF_CtrlH?.Dispose();
      SMIF_CtrlK?.Dispose();
    }
  }
}
