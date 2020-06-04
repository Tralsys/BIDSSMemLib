using System;
using System.Runtime.CompilerServices;

namespace TR.BIDSSMemLib
{
	public static partial class SMemLib
	{
		/// <summary>AutoReadを開始します。</summary>
		/// <param name="ModeNum">自動読み取りを開始する情報種類</param>
		/// <param name="Interval">読み取り頻度[ms]</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static void ReadStart(int ModeNum = 0, int Interval = 50)
		{
			if (NO_SMEM_MODE) return;
			switch (ModeNum)
			{
				case 1://OpenD
					if (SMC_OpenD?.No_SMem_Mode == false)
						SMC_OpenD?.AR_Start(Interval);
					else throw new InvalidOperationException("OpenD共有メモリが有効化されていません。");
					break;
				case 5://BSMD
					if (SMC_BSMD?.No_SMem_Mode == false)
						SMC_BSMD?.AR_Start(Interval);
					else throw new InvalidOperationException("BSMD共有メモリが有効化されていません。");
					break;
				case 6://PanelD
					if (SMC_PnlD?.No_SMem_Mode == false)
						SMC_PnlD?.AR_Start(Interval);
					else throw new InvalidOperationException("PanelD共有メモリが有効化されていません。");
					break;
				case 7://Sound D
					if (SMC_SndD?.No_SMem_Mode == false)
						SMC_SndD?.AR_Start(Interval);
					else throw new InvalidOperationException("SoundD共有メモリが有効化されていません。");
					break;
			}
			if (ModeNum <= 0) for (int i = 1; i < 8; i++) ReadStart(i, Interval);
		}



		/// <summary>AutoReadを終了します。実行中でなくともエラーは返しません。TimeOut:1000ms</summary>
		/// <param name="ModeNum">終了させる情報種類</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static void ReadStop(int ModeNum = 0)
		{
			switch (ModeNum)
			{
				case 1://OpenD
					SMC_OpenD?.AR_Stop();
					break;
				case 5://BSMD
					SMC_BSMD?.AR_Stop();
					break;
				case 6://PanelD
					SMC_PnlD?.AR_Stop();
					break;
				case 7://Sound D
					SMC_SndD?.AR_Stop();
					break;
			}
			if (ModeNum <= 0) for (int i = 1; i < 8; i++) ReadStop(i);
		}
	}
}
