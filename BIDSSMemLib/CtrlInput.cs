using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
#if !bve5id
using System.IO.MemoryMappedFiles;
#endif

namespace TR.BIDSSMemLib
{
  public class CtrlInput : IDisposable
  {
    const string MMFCtrlKName = "BIDSSMemCtrlK";
    const string MMFCtrlHName = "BIDSSMemCtrlH";
    public const int KeyArrSizeMax = 128;
#if bve5id
    const string CtrlIO_FName = "TR.pp/TR.BIDSSMemLib.CtrlIOpp.dll";

    [DllImport(CtrlIO_FName)]
    static extern void Initialize(string MMFCtrlKName, string MMFCtrlHName);
    [DllImport(CtrlIO_FName, EntryPoint = "GetHandD")]
    static extern Hands ppGetHandD();
    [DllImport(CtrlIO_FName, EntryPoint = "GetKeyD")]
    static extern IntPtr ppGetKeyD();
    [DllImport(CtrlIO_FName, EntryPoint = "Dispose")]
    static extern void ppDispose();

    public CtrlInput() => Initialize(MMFCtrlKName, MMFCtrlHName);
#else
    MemoryMappedFile MMFCtrlK = null;
    MemoryMappedFile MMFCtrlH = null;
    public CtrlInput()
    {
      MMFCtrlK = MemoryMappedFile.CreateOrOpen(MMFCtrlKName, KeyArrSizeMax * sizeof(bool));
      MMFCtrlH = MemoryMappedFile.CreateOrOpen(MMFCtrlHName, Marshal.SizeOf(new Hands()));
    }
#endif
    public enum HandType
    {
      Reverser, Power, Brake, SelfB, PPos, BPos
    }

#if !CONTROLLER
    /// <summary>キーの押下状態を取得する。</summary>
    /// <param name="Index">キー番号</param>
    /// <returns>キー押下状態</returns>
    public bool GetIsKeyPushed(int Index)
    {
      bool data = false;
      if (Index >= KeyArrSizeMax || Index < 0) throw new IndexOutOfRangeException("Please set 0 ~ 127.");
#if bve5id
      data = GetIsKeyPushed()[Index];
#else
      using (var e = MMFCtrlK?.CreateViewAccessor())
      {
        if (e?.CanRead == true)
        {
          try
          {
            data = e.ReadBoolean(Index * sizeof(bool));
          }
          catch (Exception) { throw; }
        }
        else
        {
          throw new InvalidOperationException("Could not Read from Memory Mapped File.");
        }
      }
#endif
      return data;
    }
    /// <summary>キーの押下状態を取得する。</summary>
    /// <param name="Index">キー番号</param>
    /// <param name="data">取得した情報を格納する変数</param>
    public void GetIsKeyPushed(int Index, ref bool data) => data = GetIsKeyPushed(Index);
    /// <summary>キーの押下状態を指定の場所にすべて記録する。</summary>
    /// <param name="data">キー押下状態を格納する配列</param>
    public void GetIsKeyPushed(ref bool[] data)
    {
#if bve5id
      data = GetIsKeyPushed();
#else
      using (var e = MMFCtrlK?.CreateViewAccessor())
      {
        if (e?.CanRead == true)
        {
          try
          {
            e.ReadArray(0, data, 0, KeyArrSizeMax);
          }
          catch (Exception)
          {
            throw;
          }
        }
        else
        {
          throw new InvalidOperationException("Could not Read from Memory Mapped File.");
        }
      }
#endif
    }
    /// <summary>キーの押下状態を指定の場所にすべて記録する。</summary>
    /// <returns>キー押下状態を格納する配列</returns>
    public bool[] GetIsKeyPushed()
    {
      bool[] ra = new bool[KeyArrSizeMax];
#if bve5id
      byte[] ba = new byte[KeyArrSizeMax];//UInt8
      Marshal.Copy(ppGetKeyD(), ba, 0, KeyArrSizeMax);
      for (int i = 0; i < KeyArrSizeMax; i++)
        ra[i] = ba[i] == 1;
#else
      GetIsKeyPushed(ref ra);
#endif
      return ra;
    }
#endif
    /// <summary>指定のキー状態をMemoryMappedFileに記録する。</summary>
    /// <param name="Index">キー番号</param>
    /// <param name="data">キー状態</param>
    public void SetIsKeyPushed(int Index, in bool data)
    {
      if (Index >= KeyArrSizeMax || Index < 0) throw new IndexOutOfRangeException("Please set 0 ~ 127.");
#if bve5id

#else
      using (var e = MMFCtrlK?.CreateViewAccessor())
      {
        if (e?.CanWrite == true)
        {
          try
          {
            e.Write(Index * sizeof(bool), data);
          }
          catch (Exception) { throw; }
        }
        else
        {
          throw new InvalidOperationException("Could not Write to MemoryMappedFile.");
        }
      }
#endif
    }
    /// <summary>すべてのキー状態をMemoryMappedFileに記録する。</summary>
    /// <param name="data">キー状態</param>
    public void SetIsKeyPushed(in bool[] data)
    {
      bool[] ia = new bool[KeyArrSizeMax];
      int il = ia.Length;
      if (il > 0) for (int i = 0; i < (il >= KeyArrSizeMax ? KeyArrSizeMax : il); i++) ia[i] = data[i];
#if bve5id

#else
      using (var e = MMFCtrlK?.CreateViewAccessor())
      {
        if (e?.CanWrite == true)
        {
          e.WriteArray(0, ia, 0, KeyArrSizeMax);
        }
        else
        {
          throw new InvalidOperationException("Could not Write to MemoryMappedFile.");
        }
      }
#endif
    }
    /// <summary>ハンドル位置指令状態を取得する</summary>
    /// <returns>ハンドル位置指令状態</returns>
    public Hands GetHandD()
    {
      //ctr++;
      Hands hd = new Hands();
#if bve5id
      hd = ppGetHandD();
      //if ((ctr %= 100) == 0)
      //  System.Windows.Forms.MessageBox.Show(hd.R.ToString() + "\n" + hd.B.ToString() + "\n" + hd.P.ToString());
#else
      using(var e = MMFCtrlH?.CreateViewAccessor())
      {
        if (e?.CanRead == true)
        {
          try
          {
            e.Read(0, out hd);
          }
          catch (Exception) { throw; }
        }
        else
        {
          throw new InvalidOperationException("Could not Read from Memory Mapped File.");
        }
      }
#endif
      return hd;
    }
    /// <summary>ハンドル位置指令状態を取得する</summary>
    /// <param name="hd">ハンドル位置指令状態</param>
    public void GetHandD(ref Hands hd) => hd = GetHandD();
    /// <summary>ハンドル位置を設定する</summary>
    /// <param name="hd">指定するハンドル位置</param>
    public void SetHandD(ref Hands hd)
    {
#if bve5id

#else
      using(var e = MMFCtrlH?.CreateViewAccessor())
      {
        if (e?.CanWrite == true)
        {
          e.Write(0, ref hd);
        }
        else
        {
          throw new InvalidOperationException("Could not Write to MemoryMappedFile.");
        }
      }
#endif
    }

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
#if bve5id
      ppDispose();
#else
      MMFCtrlH?.Dispose();
      MMFCtrlK?.Dispose();
#endif
    }
  }
}
