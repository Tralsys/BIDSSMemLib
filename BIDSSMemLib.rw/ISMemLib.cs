using System;

namespace TR.BIDSSMemLib
{
	public interface ISMemLib : ISMemLib_BSMD, ISMemLib_OpenD, ISMemLib_Panel, ISMemLib_Sound
	{
		void Read();

		void ReadStart(in int ModeNum, in int Interval);
		void ReadStart(in SMemLib.ARNum num, in int Interval);
		void ReadStop(in int ModeNum);
		void ReadStop(in SMemLib.ARNum num);
	}

	public interface ISMemLib_BSMD
	{
		ISMemCtrler<BIDSSharedMemoryData> SMC_BSMD { get; }

		event EventHandler<ValueChangedEventArgs<BIDSSharedMemoryData>> SMC_BSMDChanged;

		BIDSSharedMemoryData BIDSSMemData { get; }

		BIDSSharedMemoryData Read(out BIDSSharedMemoryData D);

		BIDSSharedMemoryData ReadBSMD();

		void Write(in BIDSSharedMemoryData D);
		void Write(in Spec v);
		void Write(in State v);
		void Write(in Hand v);
		void WriteIsDoorClosed(bool isDoorClosed);
		void WriteVersion(int version);
		void WriteIsEnabled(bool isEnabled);
	}

	public interface ISMemLib_OpenD
	{
		ISMemCtrler<OpenD> SMC_OpenD { get; }

		event EventHandler<ValueChangedEventArgs<OpenD>> SMC_OpenDChanged;

		OpenD OpenData { get; }

		OpenD Read(out OpenD D);

		OpenD ReadOpenD();

		void Write(in OpenD D);
	}

	public interface ISMemLib_Panel
	{
		IArrayDataSMemCtrler<int> SMC_PnlD { get; }

		event EventHandler<ValueChangedEventArgs<int[]>> SMC_PanelDChanged;

		int[] PanelA { get; }

		PanelD Read(out PanelD D);

		int[] ReadPanel();

		void Write(in PanelD D);

		void WritePanel(in int[] D);
	}

	public interface ISMemLib_Sound
	{
		IArrayDataSMemCtrler<int> SMC_SndD { get; }

		event EventHandler<ValueChangedEventArgs<int[]>> SMC_SoundDChanged;

		int[] SoundA { get; }

		SoundD Read(out SoundD D);

		int[] ReadSound();

		void Write(in SoundD D);

		void WriteSound(in int[] D);
	}
}
