using System;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace TR.BIDSSMemLib
{
  public class CtrlInput : IDisposable
  {
    const string MMFCtrlKName = "BIDSSMemCtrlK";
    const string MMFCtrlHName = "BIDSSMemCtrlK";
    public const int KeyArrSizeMax = 128;
    MemoryMappedFile MMFCtrlK = null;
    MemoryMappedFile MMFCtrlH = null;
    public CtrlInput()
    {
      MMFCtrlK = MemoryMappedFile.CreateOrOpen(MMFCtrlKName, KeyArrSizeMax * sizeof(bool));
      MMFCtrlH = MemoryMappedFile.CreateOrOpen(MMFCtrlHName, Marshal.SizeOf(new Hand()));
    }

    /// <summary>キーの押下状態を取得する。</summary>
    /// <param name="Index">キー番号</param>
    /// <returns>キー押下状態</returns>
    public bool GetIsKeyPushed(int Index)
    {
      bool data = false;
      if (Index >= KeyArrSizeMax || Index < 0) throw new IndexOutOfRangeException("Please set 0 ~ 127.");
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
    }
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
    }
    /// <summary>すべてのキー状態をMemoryMappedFileに記録する。</summary>
    /// <param name="data">キー状態</param>
    public void SetIsKeyPushed(in bool[] data)
    {
      bool[] ia = new bool[KeyArrSizeMax];
      int il = ia.Length;
      if (il > 0) for (int i = 0; i < (il >= KeyArrSizeMax ? KeyArrSizeMax : il); i++) ia[i] = data[i];
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
    }

    /// <summary>ハンドル位置指令状態を取得する</summary>
    /// <returns>ハンドル位置指令状態</returns>
    public Hand GetHandD()
    {
      Hand hd = new Hand();
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
      return hd;
    }
    /// <summary>ハンドル位置指令状態を取得する</summary>
    /// <param name="hd">ハンドル位置指令状態</param>
    public void GetHandD(ref Hand hd) => hd = GetHandD();

    /// <summary>ハンドル位置を設定する</summary>
    /// <param name="hd">指定するハンドル位置</param>
    public void SetHandD(ref Hand hd)
    {
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
    }

    public void Dispose()
    {
      MMFCtrlH?.Dispose();
      MMFCtrlK?.Dispose();
    }
  }
}
