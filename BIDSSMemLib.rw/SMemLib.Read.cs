using System.Runtime.CompilerServices;

namespace TR.BIDSSMemLib
{
	static public partial class SMemLib
	{
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

		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static FixedLenOptData[] ReadFixedLOptD(in bool DoWrite = true)
#if !(NET20 || NET35)
			=> SMC_FixedLOptD.ReadArr(DoWrite);
#else
			=> throw new System.NotImplementedException();
#endif

	}
}
