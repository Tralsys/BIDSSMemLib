using System;
using System.Runtime.CompilerServices;

namespace TR.BIDSSMemLib
{
	public partial class SMemLib
	{
		public enum ARNum
		{
			All = 0,
			OpenD = 1,
			FixedLOptD = 2,
			FlexLOptD = 3,
			BSMD = 5,
			PanelD = 6,
			SoundD = 7
		};

		[Flags]
		public enum ARFlags
		{
			All = 0b1111,

			OpenD = 0b0001,
			BSMD = 0b0010,
			PanelD = 0b0100,
			SoundD = 0b1000,
		};
		private static readonly int ARNum_Len = Enum.GetValues(typeof(ARNum)).Length;

		/// <summary>AutoReadを開始します。</summary>
		/// <param name="ModeNum">自動読み取りを開始する情報種類</param>
		/// <param name="Interval">読み取り頻度[ms]</param>
		public void ReadStart(in int ModeNum = 0, in int Interval = 50)
		{
			switch (ModeNum)
			{
				case (int)ARNum.OpenD://OpenD
					ReadStart(ARFlags.OpenD, Interval);
					break;
				case (int)ARNum.BSMD://BSMD
					ReadStart(ARFlags.BSMD, Interval);
					break;
				case (int)ARNum.PanelD://PanelD
					ReadStart(ARFlags.PanelD, Interval);
					break;
				case (int)ARNum.SoundD://Sound D
					ReadStart(ARFlags.SoundD, Interval);
					break;

				case <= 0:
					ReadStart(ARFlags.All, Interval);
					break;
			}
		}

		public void ReadStart(in ARFlags flag, in int Interval = 50)
		{
			if (flag.HasFlag(ARFlags.OpenD))
				SMC_OpenD?.AutoRead.AR_Start(Interval);

			if (flag.HasFlag(ARFlags.BSMD))
				SMC_BSMD?.AutoRead.AR_Start(Interval);

			if (flag.HasFlag(ARFlags.PanelD))
				SMC_PnlD?.AutoRead.AR_Start(Interval);

			if (flag.HasFlag(ARFlags.SoundD))
				SMC_SndD?.AutoRead.AR_Start(Interval);
		}

		/// <summary>AutoReadを開始します。</summary>
		/// <param name="ModeNum">自動読み取りを開始する情報種類</param>
		/// <param name="Interval">読み取り頻度[ms]</param>
		public void ReadStart(in ARNum num, in int Interval = 50) => ReadStart((int)num, Interval);


		/// <summary>AutoReadを終了します。実行中でなくともエラーは返しません。TimeOut:1000ms</summary>
		/// <param name="ModeNum">終了させる情報種類</param>
		public void ReadStop(in int ModeNum = 0)
		{
			switch (ModeNum)
			{
				case (int)ARNum.OpenD://OpenD
					ReadStop(ARFlags.OpenD);
					break;
				case (int)ARNum.BSMD://BSMD
					ReadStop(ARFlags.BSMD);
					break;
				case (int)ARNum.PanelD://PanelD
					ReadStop(ARFlags.PanelD);
					break;
				case (int)ARNum.SoundD://Sound D
					ReadStop(ARFlags.SoundD);
					break;

				case <= 0:
					ReadStop(ARFlags.All);
					break;
			}
		}

		public void ReadStop(in ARFlags flag)
		{
			if (flag.HasFlag(ARFlags.OpenD))
				SMC_OpenD?.AutoRead.AR_Stop();

			if (flag.HasFlag(ARFlags.BSMD))
				SMC_BSMD?.AutoRead.AR_Stop();

			if (flag.HasFlag(ARFlags.PanelD))
				SMC_PnlD?.AutoRead.AR_Stop();

			if (flag.HasFlag(ARFlags.SoundD))
				SMC_SndD?.AutoRead.AR_Stop();
		}

		/// <summary>AutoReadを終了します。実行中でなくともエラーは返しません。TimeOut:1000ms</summary>
		/// <param name="ModeNum">終了させる情報種類</param>
		public void ReadStop(in ARNum num) => ReadStop((int)num);
	}
}
