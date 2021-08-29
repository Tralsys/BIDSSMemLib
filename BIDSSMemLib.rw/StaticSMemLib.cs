using System;

namespace TR.BIDSSMemLib
{
	public static class StaticSMemLib
	{
		public static int VersionNumInt => SMemLib.VersionNumInt;
		public static string VersionNum => SMemLib.VersionNum;

		public static ISMemLib SML { get; private set; } = null;
		public static ISMemLib_BSMD SML_BSMD => SML;
		public static ISMemLib_OpenD SML_OpenD => SML;
		public static ISMemLib_Panel SML_Panel => SML;
		public static ISMemLib_Sound SML_Sound => SML;

		public static bool IsEnabled { get; private set; } = false;
		
		public static void Begin(in bool isNoSMemMode, in bool isNoEventMode, in bool isNoOptionalEventMode) => Begin(isNoSMemMode, isNoEventMode);
		public static void Begin(in bool isNoSMemMode = false, in bool isNoEventMode = false) => SML ??= new SMemLib(isNoSMemMode, isNoEventMode);

		public static void Read() => SML?.Read();

		public static void ReadStart(in int ModeNum = 0, in int Interval = 50) => SML?.ReadStart(ModeNum, Interval);
		public static void ReadStart(in SMemLib.ARNum num, in int Interval = 50) => SML?.ReadStart(num, Interval);
		public static void ReadStop(in int ModeNum = 0) => SML.ReadStop(ModeNum);
		public static void ReadStop(in SMemLib.ARNum num) => SML.ReadStop(num);


		#region BSMD
		public static ISMemCtrler<BIDSSharedMemoryData> SMC_BSMD => SML_BSMD?.SMC_BSMD;
		public static event EventHandler<ValueChangedEventArgs<BIDSSharedMemoryData>> SMC_BSMDChanged
		{
			add => SML_BSMD.SMC_BSMDChanged += value;
			remove => SML_BSMD.SMC_BSMDChanged -= value;
		}
		public static BIDSSharedMemoryData BIDSSMemData => SML_BSMD?.BIDSSMemData ?? default;
		public static BIDSSharedMemoryData Read(out BIDSSharedMemoryData D) => SML_BSMD.Read(out D);
		public static BIDSSharedMemoryData ReadBSMD() => SML_BSMD?.ReadBSMD() ?? default;
		public static void Write(in BIDSSharedMemoryData D) => SML_BSMD?.Write(D);
		#endregion

		#region OpenD
		public static ISMemCtrler<OpenD> SMC_OpenD => SML_OpenD?.SMC_OpenD;
		public static event EventHandler<ValueChangedEventArgs<OpenD>> SMC_OpenDChanged
		{
			add => SML_OpenD.SMC_OpenDChanged += value;
			remove => SML_OpenD.SMC_OpenDChanged -= value;
		}
		public static OpenD OpenData => SML_OpenD?.OpenData ?? default;
		public static OpenD Read(out OpenD D) => SML_OpenD.Read(out D);
		public static OpenD ReadOpenD() => SML_OpenD.ReadOpenD();
		public static void Write(in OpenD D) => SML_OpenD?.Write(D);
		#endregion

		#region Panel
		public static IArrayDataSMemCtrler<int> SMC_PnlD => SML_Panel?.SMC_PnlD;
		public static event EventHandler<ValueChangedEventArgs<int[]>> SMC_PanelDChanged
		{
			add => SML_Panel.SMC_PanelDChanged += value;
			remove => SML_Panel.SMC_PanelDChanged -= value;
		}
		public static int[] PanelA => SML_Panel?.PanelA;
		public static PanelD Read(out PanelD D) => SML_Panel.Read(out D);
		public static int[] ReadPanel() => SML_Panel?.ReadPanel();
		public static void Write(in PanelD D) => SML_Panel?.Write(D);
		public static void WritePanel(in int[] D) => SML_Panel?.WritePanel(D);
		#endregion

		#region Sound
		public static IArrayDataSMemCtrler<int> SMC_SndD => SML_Sound?.SMC_SndD;
		public static event EventHandler<ValueChangedEventArgs<int[]>> SMC_SoundDChanged
		{
			add => SML_Sound.SMC_SoundDChanged += value;
			remove => SML_Sound.SMC_SoundDChanged -= value;
		}
		public static int[] SoundA => SML_Sound?.SoundA;
		public static SoundD Read(out SoundD D) => SML_Sound.Read(out D);
		public static int[] ReadSound() => SML_Sound?.ReadSound();
		public static void Write(in SoundD D) => SML_Sound?.Write(D);
		public static void WriteSound(in int[] D) => SML_Sound?.WriteSound(D);
		#endregion
	}
}
