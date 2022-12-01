using System;
using System.Collections.Generic;

using TR.BIDSSMemLib;

namespace TR
{
	/// <summary>任意のデータを共有する機能を実現するクラス</summary>
	public class CustomDataSharingManager : IDisposable
	{
		Dictionary<string, VariableSMem> SMemCtrlersDic { get; } = new();
		VariableSMemNameManager NameManager { get; } = new();

		public VariableSMem<T> CreateDataSharing<T>(in string SMemName, in long Capacity = 0x1000) where T : new()
		{
			if (SMemCtrlersDic.TryGetValue(SMemName, out var value))
			{
				if (value is VariableSMem<T> ret)
					return ret;
				else
					throw new ArgumentException($"SMemName({SMemName}) was found in the dictionary, but the Type is mismatch (requested:`{typeof(T)} / found:`{value.GetType()}``)", nameof(SMemName));
			}

			NameManager.AddName(SMemName);
			VariableSMem<T> ctrler = new(SMemName, Capacity);

			SMemCtrlersDic.Add(SMemName, ctrler);

			return ctrler;
		}

		public VariableSMem<T>? GetDataSharing<T>(in string SMemName) where T : new()
		{
			if (SMemCtrlersDic.TryGetValue(SMemName, out var value))
				return value as VariableSMem<T>;
			else
				return null;
		}

		public bool TrySetValue(in string SMemName, in object value)
		{
			if (SMemCtrlersDic.TryGetValue(SMemName, out var dic_value))
			{
				dic_value.WriteToSMem(value);
				return true;
			}
			else
				return false;
		}

		public bool TrySetValue<T>(in string SMemName, in T value) where T : new()
		{
			if (SMemCtrlersDic.TryGetValue(SMemName, out var dic_value) && dic_value is ISMemCtrler<T> ctrler)
				return ctrler.TryWrite(value);
			else
				return false;
		}

		public bool TryGetValue(in string SMemName, ref object value)
		{
			if (SMemCtrlersDic.TryGetValue(SMemName, out var dic_value))
			{
				dic_value.ReadFromSMem(ref value);
				return true;
			}
			else
				return false;
		}

		public bool TryGetValue<T>(in string SMemName, out T value) where T : new()
		{
			if (SMemCtrlersDic.TryGetValue(SMemName, out var dic_value) && dic_value is ISMemCtrler<T> ctrler)
				return ctrler.TryRead(out value);
			else
			{
				value = default(T) ?? new();
				return false;
			}
		}

		#region IDisposable Support
		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				foreach (var s in SMemCtrlersDic.Values)
					s.Dispose();
				SMemCtrlersDic.Clear();
				NameManager.Dispose();
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
