using System;
using System.Runtime.CompilerServices;

namespace TR.BIDSSMemLib
{
	public static partial class SMemLib
	{
		public static readonly string VersionNum = VersionNumInt.ToString();
		public static readonly int VersionNumInt = 202;
		internal const MethodImplOptions MIOpt = (MethodImplOptions)256;//MethodImplOptions.AggressiveInlining;

		public const string MMFB_Name = "BIDSSharedMemory";
		public const string MMFO_Name = "BIDSSharedMemoryO";
		public const string MMFPn_Name = "BIDSSharedMemoryPn";
		public const string MMFSn_Name = "BIDSSharedMemorySn";

		private static SMemCtrler<BIDSSharedMemoryData> SMC_BSMD = null;
		private static SMemCtrler<OpenD> SMC_OpenD = null;
		private static SMemCtrler<int> SMC_PnlD = null;
		private static SMemCtrler<int> SMC_SndD = null;
		//private ArrDSMemCtrler<StaD> SMC_StaD = null;

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
