using System;

namespace TR.BIDSSMemLib
{
	public static partial class SMemLib
	{
		public static bool NO_SMEM_MODE { get; private set; } = false;
		public static bool NO_EVENT_MODE { get; private set; } = false;
		public static bool NO_OPT_EV_MODE { get; private set; } = false;
		static public bool IsEnabled { get; private set; }

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

		public static FixedLenOptData[] FixedLenCrumbDArr
		{
#if !(NET20 || NET35)
			get => SMC_FixedLOptD.ArrData;
			private set => SMC_FixedLOptD.WriteArr(value);
#else
			get => throw new System.NotImplementedException();
			private set => throw new System.NotImplementedException();
#endif
		}
	}
}
