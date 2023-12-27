using System;

using AtsEx.PluginHost.Plugins;
using AtsEx.PluginHost.Plugins.Extensions;

using BveTypes.ClassWrappers;

namespace TR.BIDSSMemLib;

[PluginType(PluginType.Extension)]
public partial class AtsExInterface(PluginBuilder builder) : AssemblyPluginBase(builder), IExtension
{
	readonly SMemLib smemLib = new(
		isNoSMemMode: false,
		isNoEventMode: true,
		isNoOptionalEventMode: true
	);

	nuint panelArrayLength = 256;
	nuint soundArrayLength = 256;
	public override void Dispose()
	{
		smemLib.Write(new BIDSSharedMemoryData());
		smemLib.WritePanel(new int[panelArrayLength]);
		smemLib.WriteSound(new int[soundArrayLength]);
		smemLib.Dispose();
	}

	BIDSSharedMemoryData bsmd = new();
	BveInstanceManager? bveInstanceManager = null;
	public override TickResult Tick(TimeSpan elapsed)
	{
		if (BveHacker.IsScenarioCreated)
		{
			bveInstanceManager ??= new(BveHacker.Scenario);

			bveInstanceManager.setBIDSSharedMemoryData(ref bsmd);
			smemLib.Write(bsmd);

			int[] panelArray = bveInstanceManager.atsPlugin.PanelArray;
			panelArrayLength = (nuint)panelArray.Length;
			smemLib.WritePanel(panelArray);

			int[] soundArray = bveInstanceManager.atsPlugin.SoundArray;
			soundArrayLength = (nuint)soundArray.Length;
			smemLib.WriteSound(soundArray);
		}
		else if (bsmd.IsEnabled)
		{
			bsmd = new()
			{
				IsEnabled = false,
				VersionNum = SMemLib.VersionNumInt,
			};
			smemLib.Write(bsmd);
			smemLib.WritePanel(new int[panelArrayLength]);
			smemLib.WriteSound(new int[soundArrayLength]);

			bveInstanceManager = null;
		}

		return new ExtensionTickResult();
	}

	class BveInstanceManager
	{
		readonly Vehicle vehicle;
		readonly SideDoorSet leftDoorSet;
		readonly SideDoorSet rightDoorSet;
		readonly CarInfo motorCarInfo;
		readonly CarInfo trailerCarInfo;
		readonly UserVehicleLocationManager locationManager;
		readonly HandleSet handles;
		readonly TimeManager timeManager;
		readonly VehicleStateStore vehicleStateStore;
		public readonly AtsPlugin atsPlugin;

		public BveInstanceManager(Scenario scenario)
		{
			vehicle = scenario.Vehicle;
			locationManager = scenario.LocationManager;
			timeManager = scenario.TimeManager;

			DoorSet doorSet = vehicle.Doors;
			leftDoorSet = doorSet.GetSide(DoorSide.Left);
			rightDoorSet = doorSet.GetSide(DoorSide.Right);

			VehicleDynamics dynamics = vehicle.Dynamics;
			motorCarInfo = dynamics.MotorCar;
			trailerCarInfo = dynamics.TrailerCar;

			VehicleInstrumentSet instrumentSet = vehicle.Instruments;
			handles = instrumentSet.Cab.Handles;
			atsPlugin = instrumentSet.AtsPlugin;
			vehicleStateStore = atsPlugin.StateStore;
		}

		public void setBIDSSharedMemoryData(
			ref BIDSSharedMemoryData bsmd
		)
		{
			bsmd.VersionNum = SMemLib.VersionNumInt;

			if (!bsmd.IsEnabled)
			{
				bsmd.IsEnabled = true;

				NotchInfo notchInfo = handles.NotchInfo;
				bsmd.SpecData = new()
				{
					A = notchInfo.AtsCancelNotch,
					B = notchInfo.BrakeNotchCount,
					C = (int)Math.Round(motorCarInfo.Count + trailerCarInfo.Count),
					J = notchInfo.B67Notch,
					P = notchInfo.PowerNotchCount,
				};
			}

			bsmd.StateData = new()
			{
				BC = (float)vehicleStateStore.BcPressure[0],
				BP = (float)vehicleStateStore.BpPressure[0],
				MR = (float)vehicleStateStore.MrPressure[0],
				ER = (float)vehicleStateStore.ErPressure[0],
				SAP = (float)vehicleStateStore.SapPressure[0],
				I = (float)vehicleStateStore.Current[0],
				T = (int)timeManager.TimeMilliseconds,
				V = (float)vehicleStateStore.Speed[0],
				Z = locationManager.Location,
			};
			bsmd.HandleData = new()
			{
				B = handles.BrakeNotch,
				P = handles.PowerNotch,
				R = handles.ReverserPosition switch
				{
					ReverserPosition.F => 1,
					ReverserPosition.N => 0,
					ReverserPosition.B => -1,
					_ => (int)handles.ReverserPosition,
				},
				C = (int)handles.ConstantSpeedMode,
			};
			bsmd.IsDoorClosed = !(leftDoorSet.IsOpen || rightDoorSet.IsOpen);
		}
	}
}
