using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TR.BIDSSMemLib
{
  //structをまとめ
  /// <summary>車両のスペック</summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct Spec
  {
    /// <summary>ブレーキ段数</summary>
    public int B;
    /// <summary>ノッチ段数</summary>
    public int P;
    /// <summary>ATS確認段数</summary>
    public int A;
    /// <summary>常用最大段数</summary>
    public int J;
    /// <summary>編成車両数</summary>
    public int C;
  };
  /// <summary>車両の状態</summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct State
  {
    /// <summary>列車位置[m]</summary>
    public double Z;
    /// <summary>列車速度[km/h]</summary>
    public float V;
    /// <summary>0時からの経過時間[ms]</summary>
    public int T;
    /// <summary>BC圧力[kPa]</summary>
    public float BC;
    /// <summary>MR圧力[kPa]</summary>
    public float MR;
    /// <summary>ER圧力[kPa]</summary>
    public float ER;
    /// <summary>BP圧力[kPa]</summary>
    public float BP;
    /// <summary>SAP圧力[kPa]</summary>
    public float SAP;
    /// <summary>電流[A]</summary>
    public float I;
  };
  /// <summary>車両のハンドル位置</summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct Hand
  {
    /// <summary>ブレーキハンドル位置</summary>
    public int B;
    /// <summary>ノッチハンドル位置</summary>
    public int P;
    /// <summary>レバーサーハンドル位置</summary>
    public int R;
    /// <summary>定速制御状態</summary>
    public int C;
  };
  /// <summary>車両のハンドル位置(InputDevice用)</summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct Hands
  {
    /// <summary>ブレーキハンドル位置</summary>
    public int B;
    /// <summary>ノッチハンドル位置</summary>
    public int P;
    /// <summary>レバーサーハンドル位置</summary>
    public int R;
    /// <summary>自弁ハンドル位置</summary>
    public int S;
    /// <summary>制動ハンドル位置(0~1)</summary>
    public double BPos;
    /// <summary>力行ハンドル位置(0~1)</summary>
    public double PPos;
  };
  /// <summary>BIDSSharedMemoryのデータ構造体</summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct BIDSSharedMemoryData
  {
    /// <summary>SharedMemoryが有効かどうか</summary>
    public bool IsEnabled;
    /// <summary>SharedRAMの構造バージョン</summary>
    public int VersionNum;
    /// <summary>車両スペック情報</summary>
    public Spec SpecData;
    /// <summary>車両状態情報</summary>
    public State StateData;
    /// <summary>ハンドル位置情報</summary>
    public Hand HandleData;
    /// <summary>ドアが閉まっているかどうか</summary>
    public bool IsDoorClosed;
  };

#if !bve5
  /// <summary>OpenBVEのみで取得できるデータ(open専用)</summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct OpenD
  {
    /// <summary>情報が有効かどうか</summary>
    public bool IsEnabled { get; set; }
    /// <summary>クラスバージョン</summary>
    public readonly int Ver;
    /// <summary>現在のカーブ半径[m]</summary>
    public double Radius { get; set; }
    /// <summary>現在のカントの大きさ[mm]</summary>
    public double Cant { get; set; }
    /// <summary>現在の勾配[‰]</summary>
    public double Pitch { get; set; }
    /// <summary>1フレーム当たりの時間[ms]</summary>
    public double ElapTime { get; set; }
    /// <summary>先行列車に関する情報</summary>
    public PreTrainD PreTrain { get; set; }
    /// <summary>自弁可動域段数</summary>
    public int SelfBCount { get; set; }
    /// <summary>自弁ハンドル位置</summary>
    public int SelfBPosition { get; set; }

    [StructLayout(LayoutKind.Sequential)]
    public struct PreTrainD
    {
      /// <summary>情報が有効かどうか</summary>
      public bool IsEnabled { get; set; }
      /// <summary>先行列車の尻尾の位置[m]</summary>
      public double Location { get; set; }
      /// <summary>先行列車の尻尾までの距離[m]</summary>
      public double Distance { get; set; }
      /// <summary>先行列車の速度[km/h]</summary>
      public double Speed { get; set; }
    }
  }

  /// <summary>駅に関するデータ</summary>
  [StructLayout(LayoutKind.Sequential)]
  public struct StaD
  {
    /// <summary>データサイズ</summary>
    public int Size => Marshal.SizeOf(StaList);
    public List<StationData> StaList { get; set; }
    /// <summary>駅に関するデータ</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct StationData
    {
      /// <summary>駅名番号</summary>
      public byte StaName;
      /// <summary>到着予定時刻[s]の1/5</summary>
      public ushort ArrTime;
      /// <summary>発車予定時刻[s]の1/5</summary>
      public ushort DepTime;
      /// <summary>標準停車時間[s]の1/5</summary>
      public ushort StopTime;
      /// <summary>停止定位駅かどうか</summary>
      public bool IsSigUsuallyRed;
      /// <summary>左のドアが開くかどうか</summary>
      public bool IsLeftOpen;
      /// <summary>右のドアが開くかどうか</summary>
      public bool IsRightOpen;
      /// <summary>予定された発着番線</summary>
      public float DefaultTrack;
      /// <summary>停止位置[m]</summary>
      public double StopLocation;
      /// <summary>通過駅かどうか</summary>
      public bool IsPass;
      /// <summary>駅の種類</summary>
      public StaType Type;

      /// <summary>駅の種類</summary>
      public enum StaType { Normal, ChangeEnd, Termial, Stop }
    }
  }

#endif
  [StructLayout(LayoutKind.Sequential)]
  public struct PanelD
  {
    public int Length => Panels?.Length ?? 0;
    public int[] Panels { get; set; }
  }
  [StructLayout(LayoutKind.Sequential)]
  public struct SoundD
  {
    public int Length => Sounds?.Length ?? 0;
    public int[] Sounds { get; set; }
  }
}
