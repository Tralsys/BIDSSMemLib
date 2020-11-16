using System;
using System.Runtime.CompilerServices;

namespace TR.BIDSSMemLib
{
	public static partial class SMemLib
	{
		public static readonly string VersionNum = VersionNumInt.ToString();
		public static readonly int VersionNumInt = 202;
		private const MethodImplOptions MIOpt = (MethodImplOptions)256;//MethodImplOptions.AggressiveInlining;
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

		/// <summary>共有メモリからデータを読み込む</summary>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static void Read()
		{
			if (NO_SMEM_MODE) return;
			SMC_BSMD.OnlyRead();
			SMC_OpenD.OnlyRead();
			//Read<StaD>();
			SMC_PnlD.OnlyRead();
			SMC_SndD.OnlyRead();
		}

		/// <summary>共有メモリからデータを読み込む</summary>
		/// <param name="D">読み込んだデータを書き込む変数</param>
		/// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static BIDSSharedMemoryData Read(out BIDSSharedMemoryData D, in bool DoWrite = true)
		{
			SMC_BSMD.Read(out D, DoWrite);
			return D;
		}
		/// <summary>共有メモリからデータを読み込む</summary>
		/// <param name="D">読み込んだデータを書き込む変数</param>
		/// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static OpenD Read(out OpenD D, in bool DoWrite = true)
		{
			SMC_OpenD.Read(out D, DoWrite);
			return D;
		}
		/*
		/// <summary>共有メモリからデータを読み込む</summary>
		/// <param name="D">読み込んだデータを書き込む変数</param>
		/// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
		public StaD Read(out StaD D, bool DoWrite = true)
		{
			D = new StaD();
			using (var m = MMFS?.CreateViewAccessor())
			{
				m?.Read(0, out D);
			}
			if (DoWrite) Stations = D;
			return D;
		}*/
		/// <summary>共有メモリからデータを読み込む</summary>
		/// <param name="D">読み込んだデータを書き込む変数</param>
		/// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static PanelD Read(out PanelD D, in bool DoWrite = true)
		{
			D = new PanelD() { Panels = SMC_PnlD.ReadArr(DoWrite) ?? new int[0] };
			return D;
		}
		/// <summary>共有メモリからデータを読み込む</summary>
		/// <param name="D">読み込んだデータを書き込む変数</param>
		/// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static SoundD Read(out SoundD D, in bool DoWrite = true)
		{
			D = new SoundD() { Sounds = SMC_SndD.ReadArr(DoWrite) ?? new int[0] };
			return D;
		}
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static BIDSSharedMemoryData ReadBSMD(in bool DoWrite = true) => SMC_BSMD.Read(DoWrite);
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static OpenD ReadOpenD(in bool DoWrite = true) => SMC_OpenD.Read(DoWrite);
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static int[] ReadPanel(in bool DoWrite = true) => SMC_PnlD.ReadArr(DoWrite);
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static int[] ReadSound(in bool DoWrite = true) => SMC_SndD.ReadArr(DoWrite);
		

		/// <summary>BIDSSharedMemoryData構造体の情報を共有メモリに書き込む</summary>
		/// <param name="D">書き込む構造体</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static void Write(in BIDSSharedMemoryData D) => SMC_BSMD?.Write(D);
		/// <summary>OpenD構造体の情報を共有メモリに書き込む</summary>
		/// <param name="D">書き込む構造体</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static void Write(in OpenD D) => SMC_OpenD?.Write(D);
		/// <summary>StaD構造体の情報を共有メモリに書き込む</summary>
		/// <param name="D">書き込む構造体</param>
		//public void Write(in StaD D) => Write(D, 4);
		/// <summary>Panel構造体の情報を共有メモリに書き込む</summary>
		/// <param name="D">書き込む構造体</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static void Write(in PanelD D) => SMC_PnlD?.WriteArr(D.Panels);
		/// <summary>Panel構造体の情報を共有メモリに書き込む</summary>
		/// <param name="D">書き込む配列</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static void WritePanel(in int[] D) => SMC_PnlD?.WriteArr(D);
		/// <summary>Sound構造体の情報を共有メモリに書き込む</summary>
		/// <param name="D">書き込む構造体</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static void Write(in SoundD D) => SMC_SndD?.WriteArr(D.Sounds);
		/// <summary>Sound構造体の情報を共有メモリに書き込む</summary>
		/// <param name="D">書き込む配列</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static void WriteSound(in int[] D) => SMC_SndD?.WriteArr(D);
	}
}
