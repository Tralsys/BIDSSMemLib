using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;

using BveEx.PluginHost.Plugins;
using BveEx.PluginHost.Plugins.Extensions;

using BveTypes.ClassWrappers;

using SIPSorcery.Net;

using TR.BidsWebRtc.Api;
using TR.BidsWebRtc.WebRtc;

namespace TR.BIDSSMemLib;

[Plugin(PluginType.Extension)]
public partial class BveExInterface : AssemblyPluginBase, IExtension
{
#if DEBUG
	[DllImport("kernel32.dll")]
	private static extern bool AllocConsole();
	static BveExInterface()
	{
		AllocConsole();
		Console.WriteLine("TR.BIDSSMemLib.BveExInterface loaded");
	}
#endif
	static readonly HttpClient httpClient = new();
	readonly SMemLib smemLib = new(
		isNoSMemMode: false,
		isNoEventMode: true,
		isNoOptionalEventMode: true
	);

	readonly RtcConnectionManager? rtcConnection = null;
	readonly HashSet<RTCDataChannel> rtcBsmdSendList = [];
	readonly HashSet<RTCDataChannel> rtcPanelSendList = [];
	readonly HashSet<RTCDataChannel> rtcSoundSendList = [];

	public BveExInterface(PluginBuilder builder) : base(builder)
	{
		smemLib.Write(bsmd);
		smemLib.Write(openD);
		smemLib.WritePanel(new int[panelArrayLength]);
		smemLib.WriteSound(new int[soundArrayLength]);

		rtcConnection = CreateRtcConnectionManager();
		if (rtcConnection is not null)
		{
			rtcConnection.OnDataChannelClosed += RtcConnection_OnDataChannelClosed;
			rtcConnection.OnDataGot += RtcConnection_OnDataGot;
		}

		BveHacker.ScenarioClosed += OnScenarioClosed;
	}

	private static RtcConnectionManager? CreateRtcConnectionManager()
	{
		Assembly assembly = Assembly.GetExecutingAssembly();
		if (assembly is null)
		{
			return null;
		}

		string assemblyLocation = assembly.Location;
		string asmDirectory = Path.GetDirectoryName(assemblyLocation);
		string asmName = Path.GetFileNameWithoutExtension(assemblyLocation);
		string tokenFilePath = Path.Combine(asmDirectory, $"{asmName}.bids-rtc.token.bin");
		if (!File.Exists(tokenFilePath))
		{
			return null;
		}

		byte[] tokenBytes = File.ReadAllBytes(tokenFilePath);
		TokenManager tokenManager = new(httpClient, tokenBytes);
		SdpExchangeApi sdpExchangeApi = new(httpClient, tokenManager);
		RtcConnectionManager rtcConnection = RtcConnectionManager.Create(RtcConnectionManager.Role.Provider, sdpExchangeApi);

		return rtcConnection;
	}

	private void RtcConnection_OnDataGot(object sender, RtcConnectionManager.OnDataGotEventArgs e)
	{
		if (e.DataChannel is null || e.Data.Length <= 2)
		{
			return;
		}
		if (e.Data[0] != (byte)'{' || e.Data[^1] != (byte)'}')
		{
			return;
		}

		try
		{
			string json = System.Text.Encoding.UTF8.GetString(e.Data);
			var jsonData = System.Text.Json.JsonDocument.Parse(json).RootElement;
			if (!jsonData.TryGetProperty("id", out var idField) || !jsonData.TryGetProperty("cmd", out var cmdField))
			{
				return;
			}
			string? id = idField.GetString();
			string? cmd = cmdField.GetString();
			if (id is null || cmd is null)
			{
				return;
			}

			bool isSuccess = false;
			switch (cmd)
			{
				case "BEGIN_BSMD":
					rtcBsmdSendList.Add(e.DataChannel);
					isSuccess = true;
					break;
				case "END_BSMD":
					rtcBsmdSendList.Remove(e.DataChannel);
					isSuccess = true;
					break;
				case "BEGIN_PANEL":
					rtcPanelSendList.Add(e.DataChannel);
					isSuccess = true;
					break;
				case "END_PANEL":
					rtcPanelSendList.Remove(e.DataChannel);
					isSuccess = true;
					break;
				case "BEGIN_SOUND":
					rtcSoundSendList.Add(e.DataChannel);
					isSuccess = true;
					break;
				case "END_SOUND":
					rtcSoundSendList.Remove(e.DataChannel);
					isSuccess = true;
					break;
					// TODO: WATCH / UNWATCH対応
			}

			//string response = $$"""{"id": "{{id}}"}""";
			using var responseBytes = new MemoryStream();
			using var response = new System.Text.Json.Utf8JsonWriter(responseBytes);
			response.WriteStartObject();
			response.WriteString("id", id);
			response.WriteBoolean("success", isSuccess);
			response.WriteEndObject();
			e.DataChannel.send(responseBytes.ToArray());
		}
		catch (Exception ex)
		{
			Console.WriteLine($"error from {e.ClientId}: {ex}");
		}
	}
	private void RtcConnection_OnDataChannelClosed(object sender, RtcConnectionManager.OnDataChannelStateChangedEventArgs e)
	{
		if (e.DataChannel is null)
		{
			return;
		}
		rtcBsmdSendList.Remove(e.DataChannel);
		rtcPanelSendList.Remove(e.DataChannel);
		rtcSoundSendList.Remove(e.DataChannel);
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

		rtcConnection?.Dispose();

		if (bsmdPtr != IntPtr.Zero)
		{
			Marshal.FreeHGlobal(bsmdPtr);
			bsmdPtr = IntPtr.Zero;
		}

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

	public override void Tick(TimeSpan elapsed)
	{
		if (BveHacker.IsScenarioCreated)
		{
			bveInstanceManager ??= new(BveHacker.Scenario);

			bveInstanceManager.setBIDSSharedMemoryData(ref bsmd);
			smemLib.Write(bsmd);
			sendRtcBsmd(in bsmd);

			bveInstanceManager.setOpenD(ref openD, in elapsed);
			smemLib.Write(openD);

			int[] panelArray = bveInstanceManager.atsPlugin.PanelArray;
			panelArrayLength = (nuint)panelArray.Length;
			smemLib.WritePanel(panelArray);
			sendRtcPanel(panelArray);

			int[] soundArray = bveInstanceManager.atsPlugin.SoundArray;
			soundArrayLength = (nuint)soundArray.Length;
			smemLib.WriteSound(soundArray);
			sendRtcSound(soundArray);

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
		sendRtcBsmd(in bsmd);
		smemLib.Write(openD);
		int[] panelArray = new int[panelArrayLength];
		smemLib.WritePanel(panelArray);
		sendRtcPanel(panelArray);
		int[] soundArray = panelArrayLength == soundArrayLength ? panelArray : new int[soundArrayLength];
		smemLib.WriteSound(soundArray);
		sendRtcSound(soundArray);

		bveInstanceManager = null;
	}

	static readonly int specSize = Marshal.SizeOf<Spec>();
	static readonly int stateSize = Marshal.SizeOf<State>();
	static readonly int handSize = Marshal.SizeOf<Hand>();
	static readonly int bsmdSize = 1 + 4 + specSize + stateSize + handSize + 1;
	static readonly int specOffset = 1 + 4;
	static readonly int stateOffset = specOffset + specSize;
	static readonly int handleOffset = stateOffset + stateSize;
	static readonly int doorClosedOffset = handleOffset + handSize;
	readonly byte[] bsmdBytes = new byte[bsmdSize];
	IntPtr bsmdPtr = IntPtr.Zero;
	void sendRtcBsmd(in BIDSSharedMemoryData bsmd)
	{
		if (rtcBsmdSendList.Count == 0)
		{
			return;
		}
		if (bsmdBytes[0] == 0)
		{
			bsmdBytes[0] = 0x74;
			bsmdBytes[1] = 0x72;
			bsmdBytes[2] = 0x57;
			bsmdBytes[3] = 0x42;
		}
		if (bsmdPtr == IntPtr.Zero)
		{
			bsmdPtr = Marshal.AllocHGlobal(bsmdSize);
		}

		bsmdBytes[4] = bsmd.IsEnabled ? (byte)1 : (byte)0;
		BitConverter.GetBytes(bsmd.VersionNum).CopyTo(bsmdBytes, 4 + 1);

		Marshal.StructureToPtr(bsmd.SpecData, bsmdPtr, false);
		Marshal.Copy(bsmdPtr, bsmdBytes, 4 + specOffset, specSize);

		Marshal.StructureToPtr(bsmd.StateData, bsmdPtr, false);
		Marshal.Copy(bsmdPtr, bsmdBytes, 4 + stateOffset, stateSize);

		Marshal.StructureToPtr(bsmd.HandleData, bsmdPtr, false);
		Marshal.Copy(bsmdPtr, bsmdBytes, 4 + handleOffset, handSize);

		bsmdBytes[4 + doorClosedOffset] = bsmd.IsDoorClosed ? (byte)1 : (byte)0;

		foreach (RTCDataChannel rtcDataChannel in rtcBsmdSendList)
		{
			try
			{
				rtcDataChannel.send(bsmdBytes);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"error: {ex}");
			}
		}
	}
	void sendRtcPanel(in int[] panelArray)
		=> sendRtcArray(panelArray, rtcPanelSendList, 0x50);
	void sendRtcSound(in int[] panelArray)
		=> sendRtcArray(panelArray, rtcPanelSendList, 0x53);
	static void sendRtcArray(in int[] srcArray, in HashSet<RTCDataChannel> rtcDataChannelList, byte type)
	{
		if (rtcDataChannelList.Count == 0)
		{
			return;
		}
		byte[] bytes = new byte[4 + 4 + srcArray.Length * sizeof(int)];
		bytes[0] = 0x74;
		bytes[1] = 0x72;
		bytes[2] = 0x57;
		bytes[3] = type;
		BitConverter.GetBytes(srcArray.Length).CopyTo(bytes, 4);
		Buffer.BlockCopy(srcArray, 0, bytes, 4 + 4, srcArray.Length * sizeof(int));
		foreach (RTCDataChannel rtcDataChannel in rtcDataChannelList)
		{
			try
			{
				rtcDataChannel.send(bytes);
			}
			catch (Exception ex)
			{
				Console.WriteLine($"error: {ex}");
			}
		}
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
		readonly VehicleLocation vehicleLocation;
		public readonly HandleSet handles;
		readonly TimeManager timeManager;
		readonly VehicleStateStore vehicleStateStore;
		public readonly AtsPlugin atsPlugin;

		public BveInstanceManager(Scenario scenario)
		{
			Map map = scenario.Map;
			preTrainObj = map.PreTrainObjects;

			MyTrack track = map.MyTrack;
			curves = track.Curves;
			cants = track.Cants;

			vehicle = scenario.Vehicle;
			vehicleLocation = scenario.VehicleLocation;
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
				Z = vehicleLocation.Location,
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
				C = handles.ConstantSpeedMode == ConstantSpeedMode.Continue ? bsmd.HandleData.C : (int)handles.ConstantSpeedMode,
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
			Cant? cant = cants.GoToAndGetCurrent(vehicleLocation.Location) as Cant;
			Curve? curve = curves.GoToAndGetCurrent(vehicleLocation.Location) as Curve;
			double curvature = curve?.Curvature ?? 0;
			double curveRadius = curvature != 0 ? 1 / curvature
				: openD.Radius < 0 ? double.NegativeInfinity : double.PositiveInfinity;

			openD = new()
			{
				IsEnabled = true,
				ElapTime = (int)elapsed.TotalMilliseconds,

				// TODO: 仮でradを入れてる。将来的には別に分離するかも。
				Cant = cant?.RotationZ ?? 0,
				// TODO: GradientがRC4で未実装のため、実装され次第対応する
				Pitch = 0,
				Radius = curveRadius,

				PreTrain = new()
				{
					IsEnabled = false,
					Distance = preTrainLocation - vehicleLocation.Location,
					Location = preTrainLocation,
					Speed = preTrainSpeed_kmph,
				},
			};
		}
	}
}
