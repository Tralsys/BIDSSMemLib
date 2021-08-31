using System;
using System.Runtime.CompilerServices;

namespace TR.BIDSSMemLib
{
	public partial class SMemLib : ISMemLib
	{
		public static readonly string VersionNum = VersionNumInt.ToString();
		public static readonly int VersionNumInt = 203;
		internal const MethodImplOptions MIOpt = (MethodImplOptions)256;//MethodImplOptions.AggressiveInlining;

		public const string MMFB_Name = "BIDSSharedMemory";
		public const string MMFO_Name = "BIDSSharedMemoryO";
		public const string MMFPn_Name = "BIDSSharedMemoryPn";
		public const string MMFSn_Name = "BIDSSharedMemorySn";

		public ISMemCtrler<BIDSSharedMemoryData> SMC_BSMD { get; private set; }
		public ISMemCtrler<OpenD> SMC_OpenD { get; private set; }
		public IArrayDataSMemCtrler<int> SMC_PnlD { get; private set; }
		public IArrayDataSMemCtrler<int> SMC_SndD { get; private set; }


		/// <summary>SharedMemoryを初期化する。</summary>
		/// <param name="isNoSMemMode">MmmoryMappedFileを使用したデータ共有を行うかどうか</param>
		/// <param name="isNoEventMode">値の更新確認およびそれによるValueChangedイベントを発火させるかどうか</param>
		/// <param name="isNoOptionalEventMode">追加イベント(TR.ValueChangedEventArgs以外を使用するイベント)を使用するかどうか</param>
		public SMemLib(in bool isNoSMemMode = false, in bool isNoEventMode = false, in bool isNoOptionalEventMode = false)
		{
			Console.WriteLine("BIDS SMemLib Begin(isNoSMemMode:{0}, isNoEventMode:{1})", isNoSMemMode, isNoEventMode);

			SMC_BSMD = new SMemCtrler<BIDSSharedMemoryData>(MMFB_Name, isNoSMemMode, isNoEventMode);
			SMC_OpenD = new SMemCtrler<OpenD>(MMFO_Name, isNoSMemMode, isNoEventMode);
			SMC_PnlD = new ArrayDataSMemCtrler<int>(MMFPn_Name, isNoSMemMode, isNoEventMode, 256);
			SMC_SndD = new ArrayDataSMemCtrler<int>(MMFSn_Name, isNoSMemMode, isNoEventMode, 256);

			//OptionalEventは削除した
		}

	}
}
