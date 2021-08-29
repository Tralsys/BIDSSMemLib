﻿using System.Runtime.CompilerServices;

namespace TR.BIDSSMemLib
{
	static public partial class SMemLib
	{
		/// <summary>共有メモリからデータを読み込む</summary>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static void Read()
		{
			if (NO_SMEM_MODE)
				return;

			_ = SMC_BSMD.Read();
			_ = SMC_OpenD.Read();
			_ = SMC_PnlD.Read();
			_ = SMC_SndD.Read();
		}

		/// <summary>共有メモリからデータを読み込む</summary>
		/// <param name="D">読み込んだデータを書き込む変数</param>
		/// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static BIDSSharedMemoryData Read(out BIDSSharedMemoryData D) => D = SMC_BSMD.Read();

		/// <summary>共有メモリからデータを読み込む</summary>
		/// <param name="D">読み込んだデータを書き込む変数</param>
		/// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static OpenD Read(out OpenD D) => D = SMC_OpenD.Read();

		/// <summary>共有メモリからデータを読み込む</summary>
		/// <param name="D">読み込んだデータを書き込む変数</param>
		/// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static PanelD Read(out PanelD D)
		{
			D = new PanelD() { Panels = SMC_PnlD.Read().ToArray() ?? new int[0] };
			return D;
		}
		/// <summary>共有メモリからデータを読み込む</summary>
		/// <param name="D">読み込んだデータを書き込む変数</param>
		/// <param name="DoWrite">ライブラリのデータを書き換えるかどうか</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static SoundD Read(out SoundD D)
		{
			D = new SoundD() { Sounds = SMC_SndD.Read().ToArray() ?? new int[0] };
			return D;
		}
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static BIDSSharedMemoryData ReadBSMD() => SMC_BSMD.Read();
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static OpenD ReadOpenD() => SMC_OpenD.Read();
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static int[] ReadPanel() => SMC_PnlD.Read().ToArray();
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static int[] ReadSound() => SMC_SndD.Read().ToArray();
	}
}
