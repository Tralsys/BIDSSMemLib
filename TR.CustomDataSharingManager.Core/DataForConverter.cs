using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
		UnmanagedArray Panel { get; }
		UnmanagedArray Sound { get; }
	}

	public record DataForConverter(
		Dictionary<string, object> ObjectHolder,
		CustomDataSharingManager DataSharingManager,
		UnmanagedArray Panel,
		UnmanagedArray Sound,
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

	public record UnmanagedArray(IntPtr IntPtr)
	{
		public int this[int index]
		{
			get => Marshal.ReadInt32(IntPtr, index * sizeof(int));
			set => Marshal.WriteInt32(IntPtr, index * sizeof(int), value);
		}
	}
}

#if NETFRAMEWORK || NETSTANDARD
// ref : https://ufcpp.net/blog/2020/6/cs9vs16_7p3/
// IsExternalInit属性

namespace System.Runtime.CompilerServices
{
	internal class IsExternalInit
	{

	}
}
#endif
