using System;
using System.Runtime.CompilerServices;

namespace TR.BIDSSMemLib
{
	internal static partial class UsefulFunc
	{
		[MethodImpl(SMemLib.MIOpt)]//関数のインライン展開を積極的にやってもらう.
		static internal double MStoHH(in int ms) => ((double)ms) / 1000 / 60 / 60;
		[MethodImpl(SMemLib.MIOpt)]//関数のインライン展開を積極的にやってもらう.
		static internal double MtoKM(in double m) => m / 1000;
		[MethodImpl(SMemLib.MIOpt)]//関数のインライン展開を積極的にやってもらう.
		static internal double MtoKM(in float m) => m / 1000;
	}
	public static partial class SMemLib
	{
	//イベントクラスとイベントを列挙


		public static event EventHandler<ValueChangedEventArgs<BIDSSharedMemoryData>> SMC_BSMDChanged
		{
			add => SMC_BSMD.ValueChanged += value;
			remove => SMC_BSMD.ValueChanged -= value;
		}
		public static event EventHandler<ValueChangedEventArgs<OpenD>> SMC_OpenDChanged
		{
			add => SMC_OpenD.ValueChanged += value;
			remove => SMC_OpenD.ValueChanged -= value;
		}
		public static event EventHandler<ValueChangedEventArgs<int[]>> SMC_PanelDChanged
		{
			add => SMC_PnlD.ArrValueChanged += value;
			remove => SMC_PnlD.ArrValueChanged -= value;
		}
		public static event EventHandler<ValueChangedEventArgs<int[]>> SMC_SoundDChanged
		{
			add => SMC_SndD.ArrValueChanged += value;
			remove => SMC_SndD.ArrValueChanged -= value;
		}
	}
}
