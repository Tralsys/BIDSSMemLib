using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;

namespace TR.BIDSSMemLib
{
  #region Constant Values
  /// <summary>レバーサー位置</summary>
  public static class Reverser
  {
    /// <summary>後進</summary>
    public const int B = -1;
    /// <summary>中立</summary>
    public const int N = 0;
    /// <summary>前進</summary>
    public const int F = 1;
  }
  /// <summary>定速制御の状態</summary>
  public static class ConstSP
  {
    /// <summary>前回の状態を継続する</summary>
    public const int Continue = 0;
    /// <summary>有効化する</summary>
    public const int Enable = 1;
    /// <summary>無効化する</summary>
    public const int Disable = 2;
  }
  /// <summary>警笛の種類</summary>
  public static class Horn
  {
    /// <summary>Primary Horn</summary>
    public const int Primary = 0;
    /// <summary>Secondary Horn</summary>
    public const int Secondary = 1;
    /// <summary>Music Horn</summary>
    public const int Music = 2;
  }
  /// <summary>サウンド再生に関する操作の情報</summary>
  public static class Sound
  {
    /// <summary>繰り返し再生する</summary>
    public const int Loop = 0;
    /// <summary>一度だけ再生する</summary>
    public const int Once = 1;
    /// <summary>前回の状態を継続する</summary>
    public const int Continue = 2;
    /// <summary>再生を停止する</summary>
    public const int Stop = -1000;
  }
  /// <summary>ハンドルの初期位置設定</summary>
  public static class InitialPos
  {
    /// <summary>常用ブレーキ(B67?)</summary>
    public const int Service = 0;
    /// <summary>非常ブレーキ位置</summary>
    public const int Emergency = 1;
    /// <summary>抜き取り位置</summary>
    public const int Removed = 2;
  }
  /// <summary>ATS Keys</summary>
  public static class ATSKeys
  {
    /// <summary>ATSKey_S (Default : Space)</summary>
    public const int S = 0;
    /// <summary>ATSKey_A1 (Default : Insert)</summary>
    public const int A1 = 1;
    /// <summary>ATSKey_A2 (Default : Delete)</summary>
    public const int A2 = 2;
    /// <summary>ATSKey_B1 (Default : Home)</summary>
    public const int B1 = 3;
    /// <summary>ATSKey_B2 (Default : End)</summary>
    public const int B2 = 4;
    /// <summary>ATSKey_C1 (Default : PageUp)</summary>
    public const int C1 = 5;
    /// <summary>ATSKey_C2 (Default : PageDown)</summary>
    public const int C2 = 6;
    /// <summary>ATSKey_D (Default : D2)</summary>
    public const int D = 7;
    /// <summary>ATSKey_E (Default : D3)</summary>
    public const int E = 8;
    /// <summary>ATSKey_F (Default : D4)</summary>
    public const int F = 9;
    /// <summary>ATSKey_G (Default : D5)</summary>
    public const int G = 10;
    /// <summary>ATSKey_H (Default : D6)</summary>
    public const int H = 11;
    /// <summary>ATSKey_I (Default : D7)</summary>
    public const int I = 12;
    /// <summary>ATSKey_J (Default : D8)</summary>
    public const int J = 13;
    /// <summary>ATSKey_K (Default : D9)</summary>
    public const int K = 14;
    /// <summary>ATSKey_L (Default : D0)</summary>
    public const int L = 15;
  }
  #endregion


  /// <summary>処理を実装するクラス</summary>
  static public class Ats
  {
    static XDocument doc { get; }
    static Ats()
    {
#if DEBUG
      if (!Debugger.IsAttached)
        Debugger.Launch();
#endif
      //Load setting
      try
      {
        doc = XDocument.Load(Assembly.GetExecutingAssembly().Location + ".xml");
        Version = int.Parse(doc.Element("AtsPISetting").Element("Version").Value);
      }
      catch (Exception e)
      {
        Debug.WriteLine("[BIDSSMemLib AtsPI IF] Exception has occured at Ats.ctor\n" + e.GetType().ToString() + "\n" + e.Message);
      }
    }
    private static readonly int Version = 0x00020000;
    public const CallingConvention CalCnv = CallingConvention.StdCall;
    const int MaxIndex = 256;
    /// <summary>Is the Door Closed TF</summary>
    public static bool DoorClosed { get; set; } = false;
    /// <summary>Current State of Handles</summary>
    public static Hand Handle = default;
    /// <summary>Current Key State</summary>
    public static bool[] IsKeyDown { get; set; } = new bool[16];

    static BIDSSharedMemoryData BSMD = new BIDSSharedMemoryData();
    static int[] PArr = new int[MaxIndex];
    static int[] SArr = new int[MaxIndex];
    static BVEConductorChecker BVE_CC = null;
    /// <summary>Called when this plugin is loaded</summary>
    [DllExport(CallingConvention = CalCnv)]
    public static void Load()
    {
#if DEBUG
      MessageBox.Show("BIDSSMemLib Debug Build");
#endif
      StaticSMemLib.Begin(false, true);
      BSMD.IsEnabled = true;
      BSMD.VersionNum = StaticSMemLib.VersionNumInt;
      StaticSMemLib.Write(in BSMD);
      if (!Equals(BSMD, StaticSMemLib.ReadBSMD())) MessageBox.Show("BIDSSMemLib DataWriting Failed");

      BVE_CC ??= new BVEConductorChecker();
      BVE_CC.ConductorActioned += BVE_CC_ConductorActioned;
    }

    struct ConductorActionLogStruct
    {
      public ConductorActionLogStruct(int time, int action) => (Time, Action) = (time, action);
      public int Time;
      public int Action;
    }
    static readonly int ConductorActionLog_MaxCount = 16;
    static readonly int ConductorActionLog_CapacityCount = ConductorActionLog_MaxCount + 4;
    static ArrayDataSMemCtrler<ConductorActionLogStruct> ConductorActionLog { get; } = new(CustomDataNames.ConductorActionLog, false, true, ConductorActionLog_CapacityCount);
    private static void BVE_CC_ConductorActioned(object sender, ConductorActionedEventArgs e)
    {
      ConductorActionLog.Add(new(BSMD.StateData.T, (int)e.ActionType));

      while (ConductorActionLog.Count > ConductorActionLog_MaxCount)
        ConductorActionLog.RemoveAt(0);
    }


    /// <summary>Called when this plugin is unloaded</summary>
    [DllExport(CallingConvention = CalCnv)]
    public static void Dispose()
    {
      BSMD = new BIDSSharedMemoryData();
      var BlankArr = new int[MaxIndex];
      StaticSMemLib.Write(in BSMD);
      StaticSMemLib.WritePanel(in BlankArr);
      StaticSMemLib.WriteSound(in BlankArr);
      ConductorActionLog.Dispose();
      if(BVE_CC is not null)
      {
        BVE_CC.ConductorActioned -= BVE_CC_ConductorActioned;
        BVE_CC.Dispose();
        BVE_CC = null;
      }
    }

    /// <summary>Called when the version number is needed</summary>
    /// <returns>plugin version number</returns>
    [DllExport(CallingConvention = CalCnv)]
    public static int GetPluginVersion() => Version;
    
    /// <summary>Called when set the Vehicle Spec</summary>
    /// <param name="s">Set Spec</param>
    [DllExport(CallingConvention = CalCnv)]
    public static void SetVehicleSpec(Spec s) => BSMD.SpecData = s;

    /// <summary>Called when car is put</summary>
    /// <param name="s">Default Brake Position (Refer to InitialPos class)</param>
    [DllExport(CallingConvention = CalCnv)]
    public static void Initialize(int s) { }

    /// <summary>Called in every refleshing the display</summary>
    /// <param name="st">State</param>
    /// <param name="Pa">Panel (Pointer of int[256])</param>
    /// <param name="Sa">Sound (Pointer of int[256])</param>
    /// <returns></returns>
    [DllExport(CallingConvention = CalCnv)]
    static public Hand Elapse(State st, IntPtr Pa, IntPtr Sa)
    {
      BSMD.StateData = st;
      BSMD.HandleData = Handle;
      BSMD.IsDoorClosed = DoorClosed;
      StaticSMemLib.Write(in BSMD);
      Marshal.Copy(Pa, PArr, 0, MaxIndex);
      Marshal.Copy(Sa, SArr, 0, MaxIndex);
      StaticSMemLib.WritePanel(in PArr);
      StaticSMemLib.WriteSound(in SArr);
      return Handle;
    }

    /// <summary>Called when Power notch is moved</summary>
    /// <param name="p">Notch Number</param>
    [DllExport(CallingConvention = CalCnv)]
    static public void SetPower(int p) => Handle.P = p;

    /// <summary>Called when Brake Notch is moved</summary>
    /// <param name="b">Brake notch Number</param>
    [DllExport(CallingConvention = CalCnv)]
    static public void SetBrake(int b) => Handle.B = b;

    /// <summary>Called when Reverser is moved</summary>
    /// <param name="r">Reverser Position</param>
    [DllExport(CallingConvention = CalCnv)]
    static public void SetReverser(int r) => Handle.R = r;

    /// <summary>Called when Key is Pushed</summary>
    /// <param name="k">Pushed Key Number</param>
    [DllExport(CallingConvention = CalCnv)]
    static public void KeyDown(int k)
    {
      IsKeyDown[k] = true;
    }

    /// <summary>Called when Key is Released</summary>
    /// <param name="k">Released Key Number</param>
    [DllExport(CallingConvention = CalCnv)]
    static public void KeyUp(int k)
    {
      IsKeyDown[k] = false;
    }

    /// <summary>Called when the Horn is Blown</summary>
    /// <param name="h">Blown Horn Number</param>
    [DllExport(CallingConvention = CalCnv)]
    static public void HornBlow(int h) { }

    /// <summary>Called when Door is opened</summary>
    [DllExport(CallingConvention = CalCnv)]
    static public void DoorOpen() => DoorClosed = false;


    /// <summary>Called when Door is closed</summary>
    [DllExport(CallingConvention = CalCnv)]
    static public void DoorClose() => DoorClosed = true;


    /// <summary>Called when the Signal Showing Number is changed</summary>
    /// <param name="s">Signal Showing Number</param>
    [DllExport(CallingConvention = CalCnv)]
    static public void SetSignal(int s) { }

    /// <summary>Called when passed above the Beacon</summary>
    /// <param name="b">Beacon info</param>
    [DllExport(CallingConvention = CalCnv)]
    static public void SetBeaconData(Beacon b) { }


  }
}