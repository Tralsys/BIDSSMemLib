using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace TR.BIDSSMemLib
{
	public interface ISMemCtrler<T> : IDisposable
	{
		#region Readonly Properties
		/// <summary>SMemを用いてデータを共有するか</summary>
		bool No_SMem_Mode { get; }
		/// <summary>データ更新時にイベントを発火させるか</summary>
		bool No_Event_Mode { get; }

		/// <summary>SMemの名前</summary>
		string SMem_Name { get; }
		/// <summary>SMemのサイズ</summary>
		uint Elem_Size { get; }
		#endregion

		event EventHandler<ValueChangedEventArgs<T>> ValueChanged;

		T Data { get; set; }

		T Read(bool DoWrite = true);
		void Read(out T d, bool DoWrite = true);
		bool Write(in T data);
	}

	public class SMemCtrler<T> : ISMemCtrler<T> where T : struct
	{
		#region Readonly Properties
		/// <summary>SMemを用いてデータを共有するか</summary>
		public bool No_SMem_Mode { get; } = false;
		/// <summary>データ更新時にイベントを発火させるか</summary>
		public bool No_Event_Mode { get; } = false;

		/// <summary>SMemの名前</summary>
		public string SMem_Name { get; } = string.Empty;
		/// <summary>SMemのサイズ</summary>
		public uint Elem_Size { get; } = (uint)Marshal.SizeOf(typeof(T));
		#endregion

		/// <summary>値が変化した際に発火するイベント</summary>
		public event EventHandler<ValueChangedEventArgs<T>> ValueChanged;

		private T _Data = new T();
		/// <summary>読み取り結果</summary>
		public T Data
		{
			get => _Data;
			set
			{
				if (!No_Event_Mode && !Equals(value, _Data))
					ValueChanged?.Invoke(this, new ValueChangedEventArgs<T>(_Data, value));

				_Data = value;
			}
		}

		private SMemIF MMF = null;

		public SMemCtrler(string SMemName, bool No_SMem = false, bool No_Event = false)
		{
			if (string.IsNullOrEmpty(SMemName)) throw new ArgumentException("SMemNameに無効な値が指定されています.");
			SMem_Name = SMemName;
			No_SMem_Mode = No_SMem;
			No_Event_Mode = No_Event;

			if (No_SMem_Mode) return;//SMemを使用しないならここで終了

			//SMem初期化
			MMF = new SMemIF(SMem_Name, Elem_Size);

		}

		public T Read(bool DoWrite = true)
		{
			T d = new T();

			Read(out d, DoWrite);

			return d;
		}

		public void Read(out T d, bool DoWrite = true)
		{
			d = Data;
			if (No_SMem_Mode || MMF == null) return;

			try
			{
				if (MMF.Read(0, out d) && DoWrite) Data = d;
			}
			catch (ObjectDisposedException) { }
			catch(Exception e)
			{
				Console.WriteLine("SMemCtrler({0}).Read() : {1}", SMem_Name, e);
			}
		}

		public bool Write(in T data)
		{
			bool isSame = Equals(data, Data);

			if (!isSame) Data = data;

			if (No_SMem_Mode) return true;

			return MMF.Write(0, ref _Data);
		}

		#region IDisposable Support
		private bool disposedValue = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: マネージ状態を破棄します (マネージ オブジェクト)。
				}

				MMF?.Dispose();
				MMF = null;

				disposedValue = true;
			}
		}
		public void Dispose() => Dispose(true);
		#endregion
	}

	/// <summary>配列型を書き込みます.</summary>
	/// <typeparam name="T">配列の要素の型</typeparam>
	public class ArrDSMemCtrler<T> : ISMemCtrler<T[]> where T : struct
	{
		#region Readonly Properties
		/// <summary>SMemを用いてデータを共有するか</summary>
		public bool No_SMem_Mode { get; } = false;

		/// <summary>データ更新時にイベントを発火させるか</summary>
		public bool No_Event_Mode { get; } = false;

		/// <summary>SMemの名前</summary>
		public string SMem_Name { get; } = string.Empty;

		/// <summary>配列の各要素のサイズ</summary>
		public uint Elem_Size { get; } = (uint)Marshal.SizeOf(typeof(T));

		public uint SizeD_Size { get; } = sizeof(int);

		#endregion

		/// <summary>値が変化した際に発火するイベント</summary>
		public event EventHandler<ValueChangedEventArgs<T[]>> ValueChanged;

		private T[] _Data = new T[0];
		/// <summary>読み取り結果</summary>
		public T[] Data
		{
			get => _Data;
			set
			{
				if (!No_Event_Mode && !_Data.SequenceEqual(value))
					ValueChanged?.Invoke(this, new ValueChangedEventArgs<T[]>(_Data, value));

				_Data = value;
			}
		}

		/// <summary>配列の各値を格納するMMF</summary>
		private SMemIF MMF = null;
		/// <summary>配列の長さを格納するMMF</summary>

		/// <summary>配列型の値を格納するMMFを初期化します.</summary>
		/// <param name="SMemName">SMemの名前</param>
		/// <param name="No_SMem">SMemを使用するかどうか</param>
		/// <param name="No_Event">Eventを発火させるかどうか</param>
		public ArrDSMemCtrler(string SMemName, bool No_SMem = false, bool No_Event = false)
		{
			if (string.IsNullOrEmpty(SMemName)) throw new ArgumentException("SMemNameに無効な値が指定されています.");
			SMem_Name = SMemName;
			No_SMem_Mode = No_SMem;
			No_Event_Mode = No_Event;

			if (No_SMem_Mode) return;//SMemを使用しないならここで終了

			//SMem初期化
			MMF = new SMemIF(SMemName, SizeD_Size);
		}

		public T[] Read(bool DoWrite = true)
		{
			T[] da = new T[0];

			Read(out da, DoWrite);

			return da;
		}
		public void Read(out T[] d, bool DoWrite = true)
		{
			d = Data;
			if (No_SMem_Mode || MMF == null) return;

			try
			{
				int ArrInSMem_Len = 0;
				if (!MMF.Read(0, out ArrInSMem_Len)) return;//配列長の取得(失敗したらしゃーなし)
				d = new T[ArrInSMem_Len];
				if (ArrInSMem_Len <= 0) return;
				if (MMF.ReadArray(SizeD_Size, d, 0, d.Length) && DoWrite) Data = d;//読込成功かつ書き込み可=>Data更新
			}
			catch (ObjectDisposedException) { }//無視
			catch (Exception) { throw; }

			return;
		}
		public bool Write(in T[] data)
		{
			if (disposedValue == true) return false;
			if (data == null) return false;
			Data = data;//内部データ更新
			if (No_SMem_Mode) return true;
			if (MMF == null) return false;

			long SMemCapNeed = data.Length * Elem_Size + SizeD_Size;
			try
			{
				int len = data.Length;
				MMF.Write(0, ref len);
				if (len > 0) return MMF.WriteArray(SizeD_Size, data, 0, len);
			}
			catch (ObjectDisposedException) { }
			catch (Exception) { throw; }

			return false;
		}

		#region IDisposable Support
		private bool disposedValue = false; // 重複する呼び出しを検出するには

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: マネージ状態を破棄します (マネージ オブジェクト)。
				}

				MMF?.Dispose();
				MMF = null;

				disposedValue = true;
			}
		}
		public void Dispose() => Dispose(true);
		#endregion
	}


	public class SMC_ARSupport<T> : IDisposable
	{
		ISMemCtrler<T> SMC = null;
		public SMC_ARSupport(in ISMemCtrler<T> smc)
		{
			SMC = smc;

			//AutoReadThread初期化
			ARThread = new Thread(() =>
			{
				while (ARDoing && !disposedValue)
				{
					SMC.Read();
					Thread.Sleep((int)ARInterval);
				}
				ARDoing = false;
			});

		}

		Thread ARThread = null;
		public bool ARDoing { get; private set; } = false;
		public uint ARInterval { get; set; } = 10;

		public void Start(int Interval = 10) => Start((uint)Interval);
		public void Start(uint Interval = 10)
		{
			ARInterval = Interval;
			if (ARDoing == true) return;
			ARDoing = true;
			try
			{
				ARThread.Start();
			}
			catch (Exception e)
			{
				Console.WriteLine("ThreadStart at SMC_ARSupport({0}).Start({1}) : {2}", SMC.SMem_Name, Interval, e);
				return;
			}
		}

		public void Stop()
		{
			if (ARThread?.IsAlive != true) return;
			ARDoing = false;
			ARThread?.Join(5000);
		}

		#region IDisposable Support
		private bool disposedValue = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: マネージ状態を破棄します (マネージ オブジェクト)。
				}

				ARDoing = false;
				Stop();
				SMC = null;
				ARThread = null;

				disposedValue = true;
			}
		}

		public void Dispose()=>Dispose(true);
		#endregion


	}

	/// <summary>値が変化した際に発火するイベントの引数</summary>
	/// <typeparam name="T">値の型</typeparam>
	public class ValueChangedEventArgs<T> : EventArgs
	{
		public T OldValue { get; } = default;
		public T NewValue { get; } = default;
		public ValueChangedEventArgs(in T oldValue, in T newValue)
		{
			OldValue = oldValue;
			NewValue = newValue;
		}
	}
}
