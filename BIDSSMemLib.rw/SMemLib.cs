using System;
using System.Runtime.CompilerServices;

namespace TR.BIDSSMemLib
{
	public static partial class SMemLib
	{
		public static readonly string VersionNum = VersionNumInt.ToString();
		public static readonly int VersionNumInt = 202;
		internal const MethodImplOptions MIOpt = (MethodImplOptions)256;//MethodImplOptions.AggressiveInlining;
		public static bool NO_SMEM_MODE { get; private set; } = false;

		public const string MMFB_Name = "BIDSSharedMemory";
		public const string MMFO_Name = "BIDSSharedMemoryO";
		public const string MMFPn_Name = "BIDSSharedMemoryPn";
		public const string MMFSn_Name = "BIDSSharedMemorySn";

		private static SMemCtrler<BIDSSharedMemoryData> SMC_BSMD = null;
		private static SMemCtrler<OpenD> SMC_OpenD = null;
		private static SMemCtrler<int> SMC_PnlD = null;
		private static SMemCtrler<int> SMC_SndD = null;
		//private ArrDSMemCtrler<StaD> SMC_StaD = null;

		
		/// <summary>BIDSSharedMemoryのデータ</summary>
		public static BIDSSharedMemoryData BIDSSMemData
		{
			get => SMC_BSMD?.Data ?? default;
			private set => SMC_BSMD?.Write(value);
		}
		/// <summary>OpenBVEでのみ得られるデータ(open専用)</summary>
		public static OpenD OpenData
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
		[Obsolete("PanelA(int型配列)を使用してください.")]
		public static PanelD Panels
		{
			get => new PanelD() { Panels = SMC_PnlD?.ArrData ?? new int[0] };

			private set => SMC_PnlD.WriteArr(value.Panels);
		}
		public static int[] PanelA
		{
			get => SMC_PnlD?.ArrData;
			private set => SMC_PnlD.WriteArr(value);
		}

		/// <summary>Sound配列情報</summary>
		[Obsolete("SoundA(int型配列)を使用してください.")]
		public static SoundD Sounds
		{
			get => new SoundD() { Sounds = SMC_SndD?.ArrData ?? new int[0] };
			private set => SMC_SndD.WriteArr(value.Sounds);
		}
		public static int[] SoundA
		{
			get => SMC_SndD?.ArrData;
			private set => SMC_SndD.WriteArr(value);
		}

		static public bool IsEnabled { get; private set; }

		/// <summary>SharedMemoryを初期化する。</summary>
		/// <param name="IsThisMother">書き込む側かどうか</param>
		/// <param name="ModeNum">モード番号</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		static public void Begin(bool isNoSMemMode = false, in bool isNoEventMode = false)
		{
			if (IsEnabled) return;
			Console.WriteLine("BIDS SMemLib Begin(isNoSMemMode:{0}, isNoEventMode:{1})", isNoSMemMode, isNoEventMode);
			NO_SMEM_MODE = isNoSMemMode;


			SMC_BSMD = new SMemCtrler<BIDSSharedMemoryData>(MMFB_Name, false, isNoSMemMode, isNoEventMode);
			SMC_OpenD = new SMemCtrler<OpenD>(MMFO_Name, false, isNoSMemMode, isNoEventMode);
			SMC_PnlD = new SMemCtrler<int>(MMFPn_Name, true, isNoSMemMode, isNoEventMode);
			SMC_SndD = new SMemCtrler<int>(MMFSn_Name, true, isNoSMemMode, isNoEventMode);

			SMC_BSMD.ValueChanged += Events.OnBSMDChanged;

			IsEnabled = true;
		}
	}
}
