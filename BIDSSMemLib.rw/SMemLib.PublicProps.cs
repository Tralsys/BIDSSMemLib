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
			get => SMC_BSMD?.Value ?? default;
			private set => SMC_BSMD?.Write(value);
		}

		/// <summary>OpenBVEでのみ得られるデータ(open専用)</summary>
		public static OpenD OpenData
		{
			get => SMC_OpenD?.Value ?? default;
			private set => SMC_OpenD?.Write(value);
		}

		/// <summary>Panel配列情報</summary>
		[Obsolete("PanelA(int型配列)を使用してください.")]
		public static PanelD Panels
		{
			get => new PanelD() { Panels = SMC_PnlD?.ToArray() ?? new int[0] };

			private set => SMC_PnlD.Write(value.Panels);
		}
		public static int[] PanelA
		{
			get => SMC_PnlD?.ToArray();
			private set => SMC_PnlD.Write(value);
		}

		/// <summary>Sound配列情報</summary>
		[Obsolete("SoundA(int型配列)を使用してください.")]
		public static SoundD Sounds
		{
			get => new SoundD() { Sounds = SMC_SndD?.ToArray() ?? new int[0] };
			private set => SMC_SndD.Write(value.Sounds);
		}
		public static int[] SoundA
		{
			get => SMC_SndD?.ToArray();
			private set => SMC_SndD.Write(value);
		}
	}
}
