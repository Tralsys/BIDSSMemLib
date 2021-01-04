using System;
using System.Runtime.CompilerServices;

namespace TR.BIDSSMemLib
{
	public static partial class SMemLib
	{
		public enum ARNum
		{
			All = 0,
			OpenD = 1,
			BSMD = 5,
			PanelD = 6,
			SoundD = 7
		};

		/// <summary>AutoReadを開始します。</summary>
		/// <param name="ModeNum">自動読み取りを開始する情報種類</param>
		/// <param name="Interval">読み取り頻度[ms]</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static void ReadStart(in int ModeNum = 0, in int Interval = 50)
		{
			if (NO_SMEM_MODE) return;
			switch (ModeNum)
			{
				case (int)ARNum.OpenD://OpenD
					if (SMC_OpenD?.No_SMem_Mode == false)
						SMC_OpenD?.AR_Start(Interval);
					else throw new InvalidOperationException("OpenD共有メモリが有効化されていません。");
					break;
				case (int)ARNum.BSMD://BSMD
					if (SMC_BSMD?.No_SMem_Mode == false)
						SMC_BSMD?.AR_Start(Interval);
					else throw new InvalidOperationException("BSMD共有メモリが有効化されていません。");
					break;
				case (int)ARNum.PanelD://PanelD
					if (SMC_PnlD?.No_SMem_Mode == false)
						SMC_PnlD?.AR_Start(Interval);
					else throw new InvalidOperationException("PanelD共有メモリが有効化されていません。");
					break;
				case (int)ARNum.SoundD://Sound D
					if (SMC_SndD?.No_SMem_Mode == false)
						SMC_SndD?.AR_Start(Interval);
					else throw new InvalidOperationException("SoundD共有メモリが有効化されていません。");
					break;
			}
			if (ModeNum <= 0) for (int i = 1; i < 8; i++) ReadStart(i, Interval);
		}

		/// <summary>AutoReadを開始します。</summary>
		/// <param name="ModeNum">自動読み取りを開始する情報種類</param>
		/// <param name="Interval">読み取り頻度[ms]</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static void ReadStart(in ARNum num, in int Interval = 50) => ReadStart((int)num, Interval);


		/// <summary>AutoReadを終了します。実行中でなくともエラーは返しません。TimeOut:1000ms</summary>
		/// <param name="ModeNum">終了させる情報種類</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static void ReadStop(in int ModeNum = 0)
		{
			switch (ModeNum)
			{
				case (int)ARNum.OpenD://OpenD
					SMC_OpenD?.AR_Stop();
					break;
				case (int)ARNum.BSMD://BSMD
					SMC_BSMD?.AR_Stop();
					break;
				case (int)ARNum.PanelD://PanelD
					SMC_PnlD?.AR_Stop();
					break;
				case (int)ARNum.SoundD://Sound D
					SMC_SndD?.AR_Stop();
					break;
			}
			if (ModeNum <= 0) for (int i = 1; i < 8; i++) ReadStop(i);
		}

		/// <summary>AutoReadを終了します。実行中でなくともエラーは返しません。TimeOut:1000ms</summary>
		/// <param name="ModeNum">終了させる情報種類</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static void ReadStop(in ARNum num) => ReadStop((int)num);
	}
}
