using System;

using AtsEx.PluginHost.Plugins;
using AtsEx.PluginHost.Plugins.Extensions;

using BveTypes.ClassWrappers;

namespace TR.BIDSSMemLib;

[PluginType(PluginType.Extension)]
public partial class AtsExInterface : AssemblyPluginBase, IExtension
{
	readonly SMemLib smemLib = new(
		isNoSMemMode: false,
		isNoEventMode: true,
		isNoOptionalEventMode: true
	);

	public AtsExInterface(PluginBuilder builder) : base(builder)
	{
		smemLib.Write(bsmd);
		smemLib.Write(openD);
		smemLib.WritePanel(new int[panelArrayLength]);
		smemLib.WriteSound(new int[soundArrayLength]);

		BveHacker.ScenarioClosed += OnScenarioClosed;
	}

	nuint panelArrayLength = 256;
	nuint soundArrayLength = 256;
	public override void Dispose()
	{
		smemLib.Write(new BIDSSharedMemoryData());
		smemLib.Write(new OpenD());
		smemLib.WritePanel(new int[panelArrayLength]);
		smemLib.WriteSound(new int[soundArrayLength]);
		smemLib.Dispose();

		BveHacker.ScenarioClosed -= OnScenarioClosed;
	}

	BIDSSharedMemoryData bsmd = new()
	{
		IsEnabled = false,
		VersionNum = SMemLib.VersionNumInt,
	};
	OpenD openD = new();
	Hands hands = new();
	readonly bool[] keyStateArray = new bool[CtrlInput.KeyArrSizeMax];
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

			// BVEへの入力処理
			Hands currentHands = CtrlInput.GetHandD();
			if (!isHandsEqual(in hands, in currentHands))
			{
				hands = currentHands;
				bveInstanceManager.handles.ReverserPosition = hands.R switch
				{
					1 => ReverserPosition.F,
					0 => ReverserPosition.N,
					-1 => ReverserPosition.B,
					_ => ReverserPosition.N,
				};
				if (
					hands.B == 0
					&& hands.P == 0
					&& (
						(!double.IsNaN(hands.BPos) && hands.BPos != 0)
						|| (double.IsNaN(hands.PPos) && hands.PPos != 0)
					)
				)
				{
					hands.P = (int)Math.Round(hands.PPos * bsmd.SpecData.P, MidpointRounding.AwayFromZero);
					hands.B = (int)Math.Round(hands.BPos * bsmd.SpecData.B, MidpointRounding.AwayFromZero);
				}
				else
				{
					bveInstanceManager.handles.PowerNotch = hands.P;
					bveInstanceManager.handles.BrakeNotch = hands.B;
					double.IsNaN(hands.BPos);
				}
			}

			bool[] currentKeys = CtrlInput.GetIsKeyPushed();
			// TODO: ここでキー入力をBVEに反映する
			for (int i = 0; i < keyStateArray.Length; i++)
			{
				if (keyStateArray[i] != currentKeys[i])
				{
					keyStateArray[i] = currentKeys[i];
					if (i < 4)
					{
						// Horm / ConstSpeed
					}
					else
					{
						// ATS Keys
					}
				}
			}
		}
		else if (bsmd.IsEnabled)
			OnScenarioClosed();

		return new ExtensionTickResult();
	}
	static bool isHandsEqual(in Hands a, in Hands b)
		=> (
			a.S == b.S &&
			a.B == b.B &&
			a.P == b.P &&
			a.R == b.R
		);

	void OnScenarioClosed(EventArgs? _ = null)
	{
		bsmd = new()
		{
			IsEnabled = false,
			VersionNum = SMemLib.VersionNumInt,
		};
		openD = new();
		smemLib.Write(bsmd);
		smemLib.Write(openD);
		smemLib.WritePanel(new int[panelArrayLength]);
		smemLib.WriteSound(new int[soundArrayLength]);

		bveInstanceManager = null;
	}

	class BveInstanceManager
	{
		readonly PreTrainObjectList preTrainObj;
		readonly CurveList curves;
		readonly CantList cants;
		readonly Vehicle vehicle;
		readonly SideDoorSet leftDoorSet;
		readonly SideDoorSet rightDoorSet;
		readonly CarInfo motorCarInfo;
		readonly CarInfo trailerCarInfo;
		readonly UserVehicleLocationManager locationManager;
		public readonly HandleSet handles;
		readonly TimeManager timeManager;
		readonly VehicleStateStore vehicleStateStore;
		public readonly AtsPlugin atsPlugin;

		public BveInstanceManager(Scenario scenario)
		{
			Route route = scenario.Route;
			preTrainObj = route.PreTrainObjects;

			MyTrack track = route.MyTrack;
			curves = track.Curves;
			cants = track.Cants;

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

		public void setOpenD(
			ref OpenD openD,
			in TimeSpan elapsed
		)
		{
			double preTrainLastLocation = preTrainObj.GetPreTrainLocation(timeManager.TimeMilliseconds - (int)elapsed.TotalMilliseconds);
			double preTrainLocation = preTrainObj.GetPreTrainLocation(timeManager.TimeMilliseconds);
			double preTrainSpeed_mps = (preTrainLocation - preTrainLastLocation) / elapsed.TotalSeconds;
			double preTrainSpeed_kmph = preTrainSpeed_mps * 3.6;

			openD = new()
			{
				IsEnabled = true,
				ElapTime = (int)elapsed.TotalMilliseconds,

				// TODO: 仮でradを入れてる。将来的には別に分離するかも。
				Cant = (cants[cants.CurrentIndex] as Cant)?.RotationZ ?? 0,
				// TODO: GradientがRC4で未実装のため、実装され次第対応する
				Pitch = 0,
				Radius = curves.GetValueAt(locationManager.Location),

				PreTrain = new()
				{
					IsEnabled = false,
					Distance = preTrainLocation - locationManager.Location,
					Location = preTrainLocation,
					Speed = preTrainSpeed_kmph,
				},
			};
		}
	}
}
