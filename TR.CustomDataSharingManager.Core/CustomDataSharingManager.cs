using System;
using System.Collections.Generic;

namespace TR
{
	/// <summary>任意のデータを共有する機能を実現するクラス</summary>
	public class CustomDataSharingManager : IDisposable
	{
		Dictionary<string, IDisposable> SMemCtrlersDic { get; } = new();

		#region Create SMemCtrler
		public SMemCtrler<T> CreateOneDataSharing<T>(in string SMemName) where T : struct
		{
			if (SMemCtrlersDic.TryGetValue(SMemName, out var value))
			{
				if (value is SMemCtrler<T> ret)
					return ret;
				else
					throw new ArgumentException(nameof(SMemName), $"SMemName({SMemName}) was found in the dictionary, but the Type is mismatch");
			}

			SMemCtrler<T> ctrler = new(SMemName, false, false);

			SMemCtrlersDic.Add(SMemName, ctrler);

			return ctrler;
		}

		public ArrayDataSMemCtrler<T> CreateArrayDataSharing<T>(in string SMemName, in int MaxElemCount) where T : struct
		{
			if (SMemCtrlersDic.TryGetValue(SMemName, out var value))
			{
				if (value is ArrayDataSMemCtrler<T> ret)
					return ret;
				else
					throw new ArgumentException(nameof(SMemName), $"SMemName({SMemName}) was found in the dictionary, but the Type is mismatch");
			}

			ArrayDataSMemCtrler<T> ctrler = new(SMemName, false, false, MaxElemCount);

			SMemCtrlersDic.Add(SMemName, ctrler);

			return ctrler;
		}
		#endregion

		#region GetInstance
		public SMemCtrler<T>? GetOneDataSharing<T>(in string SMemName) where T : struct
		{
			if (SMemCtrlersDic.TryGetValue(SMemName, out var value))
				return value as SMemCtrler<T>;
			else
				return null;
		}

		public ArrayDataSMemCtrler<T>? GetArrayDataSharing<T>(in string SMemName) where T : struct
		{
			if (SMemCtrlersDic.TryGetValue(SMemName, out var value))
				return value as ArrayDataSMemCtrler<T>;
			else
				return null;
		}
		#endregion

		#region TrySetValue
		public bool TrySetValue<T>(in string SMemName, in T value) where T : struct
		{
			if (SMemCtrlersDic.TryGetValue(SMemName, out var dic_value) && dic_value is SMemCtrler<T> ctrler)
				return ctrler.TryWrite(value);
			else
				return false;
		}

		public bool TrySetValue<T>(in string SMemName, in T value, in int posInArr) where T : struct
		{
			if (SMemCtrlersDic.TryGetValue(SMemName, out var dic_value) && dic_value is ArrayDataSMemCtrler<T> ctrler)
				return ctrler.TryWrite(posInArr, value);
			else
				return false;
		}

		public bool TrySetListValue<T>(in string SMemName, in List<T> list) where T : struct
		{
			if (SMemCtrlersDic.TryGetValue(SMemName, out var dic_value) && dic_value is ArrayDataSMemCtrler<T> ctrler)
				return ctrler.TryWrite(list);
			else
				return false;
		}
		#endregion

		#region TryGetValue
		public bool TryGetValue<T>(in string SMemName, out T value) where T : struct
		{
			if (SMemCtrlersDic.TryGetValue(SMemName, out var dic_value) && dic_value is SMemCtrler<T> ctrler)
				return ctrler.TryRead(out value);
			else
			{
				value = default;
				return false;
			}
		}

		public bool TryGetValue<T>(in string SMemName, out T value, in int posInArr) where T : struct
		{
			if (SMemCtrlersDic.TryGetValue(SMemName, out var dic_value) && dic_value is ArrayDataSMemCtrler<T> ctrler)
				return ctrler.TryRead(posInArr, out value);
			else
			{
				value = default;
				return false;
			}
		}

		public bool TryGetListValue<T>(in string SMemName, out List<T> list) where T : struct
		{
			if (SMemCtrlersDic.TryGetValue(SMemName, out var dic_value) && dic_value is ArrayDataSMemCtrler<T> ctrler)
				return ctrler.TryRead(out list);
			else
			{
				list = new();
				return false;
			}
		}
		#endregion

		#region IDisposable Support
		private bool disposedValue;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				foreach (var s in SMemCtrlersDic.Values)
					s.Dispose();
				SMemCtrlersDic.Clear();
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
