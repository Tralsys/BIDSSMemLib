using System;
using System.Collections.Generic;

using OpenBveApi.Runtime;

namespace TR.BIDSSMemLib
{

	public class Ats_obve : IRuntime
	{
		public Ats_obve()
		{
			
		}

		private List<IRuntime> PIList = null;
		
		public void DoorChange(DoorStates oldState, DoorStates newState)
		{
			
		}

		public void Elapse(ElapseData data)
		{
			
		}

		public bool Load(LoadProperties properties)
		{
			//SML Load
			//設定ファイル読込
			//PIListに設定ファイルのPIをLoad
			
			return false;
		}
		public void Unload() { }

		#region Unused Methods
		public void PerformAI(AIData data) { }

		public void SetBeacon(BeaconData data) { }

		public void SetBrake(int brakeNotch) { }

		public void SetPower(int powerNotch) { }

		public void SetReverser(int reverser) { }

		public void SetSignal(SignalData[] data) { }

		public void SetVehicleSpecs(VehicleSpecs specs) { }

		public void Initialize(InitializationModes mode) { }

		public void KeyDown(VirtualKeys key) { }

		public void KeyUp(VirtualKeys key) { }

		public void HornBlow(HornTypes type) { }
		#endregion
	}
}
