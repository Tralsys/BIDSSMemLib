﻿using System.Collections.Generic;
using System.Runtime.InteropServices;
namespace TR.BIDSSMemLib
{
	//structをまとめ
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

	/// <summary>OpenBVEのみで取得できるデータ(open専用)</summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct OpenD
	{
		/// <summary>情報が有効かどうか</summary>
		public bool IsEnabled;
		/// <summary>クラスバージョン</summary>
		public readonly int Ver;
		/// <summary>現在のカーブ半径[m]</summary>
		public double Radius;
		/// <summary>現在のカントの大きさ[mm]</summary>
		public double Cant;
		/// <summary>現在の勾配[‰]</summary>
		public double Pitch;
		/// <summary>1フレーム当たりの時間[ms]</summary>
		public double ElapTime;
		/// <summary>先行列車に関する情報</summary>
		public PreTrainD PreTrain;
		/// <summary>自弁可動域段数</summary>
		public int SelfBCount;
		/// <summary>自弁ハンドル位置</summary>
		public int SelfBPosition;

		[StructLayout(LayoutKind.Sequential)]
		public struct PreTrainD
		{
			/// <summary>情報が有効かどうか</summary>
			public bool IsEnabled;
			/// <summary>先行列車の尻尾の位置[m]</summary>
			public double Location;
			/// <summary>先行列車の尻尾までの距離[m]</summary>
			public double Distance;
			/// <summary>先行列車の速度[km/h]</summary>
			public double Speed;
		}
	}

	/// <summary>駅に関するデータ</summary>
	[StructLayout(LayoutKind.Sequential)]
	public struct StaD
	{
		/// <summary>データサイズ</summary>
		public int Size => Marshal.SizeOf(StaList);
		public List<StationData> StaList;
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

	[StructLayout(LayoutKind.Sequential)]
	public struct PanelD
	{
		public int Length => Panels?.Length ?? 0;
		public int[] Panels;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct SoundD
	{
		public int Length => Sounds?.Length ?? 0;
		public int[] Sounds;
	}
}