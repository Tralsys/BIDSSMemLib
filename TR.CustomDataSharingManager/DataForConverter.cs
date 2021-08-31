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

	public interface IDataForConverter_Panel_Sound
	{
		IntPtr Panel { get; }
		IntPtr Sound { get; }
	}

	public record DataForConverter(
		CustomDataSharingManager DataSharingManager,
		IntPtr Panel,
		IntPtr Sound,
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
		IDataForConverter_Panel_Sound;
}
