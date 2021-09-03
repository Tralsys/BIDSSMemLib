using System;
using System.Threading.Tasks;

namespace TR
{
	public interface IScriptingModule
	{
		Task RunAsync(DataForConverter data);
	}

	public interface IContainsCustomDataSharingManager
	{
		CustomDataSharingManager DataSharingManager { get; }
	}

	public interface IDataForConverter_Panel_Sound_IntPtr
	{
		IntPtr PanelIntPtr { get; }
		IntPtr SoundIntPtr { get; }
	}

	public unsafe interface IDataForConverter_Panel_Sound
	{
		int* Panel { get; }
		int* Sound { get; }
	}

	public record DataForConverter(
		CustomDataSharingManager DataSharingManager,
		IntPtr PanelIntPtr,
		IntPtr SoundIntPtr,
		double Location,
		float Speed,
		TimeSpan Time,
		float BCPressure,
		float MRPressure,
		float ERPressure,
		float BPPressure,
		float SAPPressure,
		float Current,
		bool IsDoorClosed,
		int CarBrakeNotchCount,
		int CarPowerNotchCount,
		int CarATSCheckPos,
		int CarB67Pos,
		int CarCount,
		int CurrentBrakePos,
		int CurrentPowerPos,
		int CurrentReverserPos) :
		IContainsCustomDataSharingManager,
		IDataForConverter_Panel_Sound_IntPtr,
		IDataForConverter_Panel_Sound
	{
		public unsafe int* Panel => (int*)PanelIntPtr;
		public unsafe int* Sound => (int*)SoundIntPtr;
	}
}

#if NETFRAMEWORK
// ref : https://ufcpp.net/blog/2020/6/cs9vs16_7p3/
// IsExternalInit属性

namespace System.Runtime.CompilerServices
{
	internal class IsExternalInit
	{

	}
}
#endif
