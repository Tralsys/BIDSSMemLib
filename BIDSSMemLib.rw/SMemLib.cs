using System;
using System.Runtime.CompilerServices;

namespace TR.BIDSSMemLib
{
	public static partial class SMemLib
	{
		public static readonly string VersionNum = VersionNumInt.ToString();
		public static readonly int VersionNumInt = 203;
		internal const MethodImplOptions MIOpt = (MethodImplOptions)256;//MethodImplOptions.AggressiveInlining;

		public const string MMFB_Name = "BIDSSharedMemory";
		public const string MMFO_Name = "BIDSSharedMemoryO";
		public const string MMFPn_Name = "BIDSSharedMemoryPn";
		public const string MMFSn_Name = "BIDSSharedMemorySn";

		private static ISMemCtrler<BIDSSharedMemoryData> SMC_BSMD = null;
		private static ISMemCtrler<OpenD> SMC_OpenD = null;
		private static IArrayDataSMemCtrler<int> SMC_PnlD = null;
		private static IArrayDataSMemCtrler<int> SMC_SndD = null;

		/// <summary>SharedMemoryを初期化する。</summary>
		/// <param name="isNoSMemMode">MmmoryMappedFileを使用したデータ共有を行うかどうか</param>
		/// <param name="isNoEventMode">値の更新確認およびそれによるValueChangedイベントを発火させるかどうか</param>
		/// <param name="isNoOptionalEventMode">追加イベント(TR.ValueChangedEventArgs以外を使用するイベント)を使用するかどうか</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		static public void Begin(in bool isNoSMemMode = false, in bool isNoEventMode = false, in bool isNoOptionalEventMode = false)
		{
			if (IsEnabled) return;
			Console.WriteLine("BIDS SMemLib Begin(isNoSMemMode:{0}, isNoEventMode:{1})", isNoSMemMode, isNoEventMode);
			NO_SMEM_MODE = isNoSMemMode;
			NO_EVENT_MODE = isNoEventMode;
			NO_OPT_EV_MODE = isNoOptionalEventMode;

			SMC_BSMD = new SMemCtrler<BIDSSharedMemoryData>(MMFB_Name, isNoSMemMode, isNoEventMode);
			SMC_OpenD = new SMemCtrler<OpenD>(MMFO_Name, isNoSMemMode, isNoEventMode);
			SMC_PnlD = new ArrayDataSMemCtrler<int>(MMFPn_Name, isNoSMemMode, isNoEventMode);
			SMC_SndD = new ArrayDataSMemCtrler<int>(MMFSn_Name, isNoSMemMode, isNoEventMode);

			if (!isNoOptionalEventMode)
				SMC_BSMD.ValueChanged += Events.OnBSMDChanged;

			IsEnabled = true;
		}
	}
}
