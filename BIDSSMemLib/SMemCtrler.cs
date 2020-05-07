using System;
using System.IO.MemoryMappedFiles;
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

		private MemoryMappedFile MMF = null;

		public SMemCtrler(string SMemName, bool No_SMem = false, bool No_Event = false)
		{
			if (string.IsNullOrWhiteSpace(SMemName)) throw new ArgumentException("SMemNameに無効な値が指定されています.");
			SMem_Name = SMemName;
			No_SMem_Mode = No_SMem;
			No_Event_Mode = No_Event;

			if (No_SMem_Mode) return;//SMemを使用しないならここで終了

			//SMem初期化
			MMF = MemoryMappedFile.CreateOrOpen(SMem_Name, Elem_Size);

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
				using(var m = MMF?.CreateViewAccessor())
				{
					if (m?.CanRead != true) return;
					m.Read(0, out d);
					if (DoWrite) Data = d;
					return;
				}
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

			if (No_SMem_Mode || isSame) return true;

			using (var m = MMF?.CreateViewAccessor())
			{
				if (m?.CanWrite == true)
				{
					m.Write(0, ref _Data);
					return true;
				}
			}

			return false;//m==null || CanWrite==falseだったから.
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

				// TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
				// TODO: 大きなフィールドを null に設定します。

				MMF?.Dispose();
				MMF = null;

				disposedValue = true;
			}
		}

		// TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
		// ~SMemCtrler()
		// {
		//   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
		//   Dispose(false);
		// }

		// このコードは、破棄可能なパターンを正しく実装できるように追加されました。
		public void Dispose()
		{
			// このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
			Dispose(true);
			// TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
			// GC.SuppressFinalize(this);
		}
		#endregion
	}

	/// <summary>配列型を書き込みます.</summary>
	/// <typeparam name="T"></typeparam>
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
		private MemoryMappedFile MMF = null;
		/// <summary>配列の長さを格納するMMF</summary>

		/// <summary>配列型の値を格納するMMFを初期化します.</summary>
		/// <param name="SMemName">SMemの名前</param>
		/// <param name="No_SMem">SMemを使用するかどうか</param>
		/// <param name="No_Event">Eventを発火させるかどうか</param>
		public ArrDSMemCtrler(string SMemName, bool No_SMem = false, bool No_Event = false)
		{
			if (string.IsNullOrWhiteSpace(SMemName)) throw new ArgumentException("SMemNameに無効な値が指定されています.");
			SMem_Name = SMemName;
			No_SMem_Mode = No_SMem;
			No_Event_Mode = No_Event;

			if (No_SMem_Mode) return;//SMemを使用しないならここで終了

			//SMem初期化
			long neededSize = 0;
			using (var mmf = MemoryMappedFile.CreateOrOpen(SMem_Name, SizeD_Size))
			using (var m = mmf?.CreateViewAccessor())
			{
				if (m?.CanRead != true || m?.CanWrite != true) throw new Exception("SMem Open Failed");

				neededSize = (m.ReadInt32(0) * Elem_Size) + SizeD_Size;
			}

			MMF = MemoryMappedFile.CreateOrOpen(SMemName, neededSize);
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


			//要求されるSMemのキャパ
			long SMemCapNeed = 0;

			try
			{
				using (var m = MMF.CreateViewAccessor())
				{
					if (!(m?.CanRead == true)) return;//読込不可

					int ArrInSMem_Len = m.ReadInt32(0);

					SMemCapNeed = SizeD_Size + (ArrInSMem_Len * Elem_Size);

					if (SMemCapNeed <= m.Capacity)//要求キャパが使用するSMemのキャパより小さければRead続行
					{
						d = new T[ArrInSMem_Len];
						if (d.Length > 0) m.ReadArray(SizeD_Size, d, 0, d.Length);

						if (DoWrite) Data = d;

						return;
					}
				}

				//ReOpen
				MMF?.Dispose();
				MMF = MemoryMappedFile.CreateOrOpen(SMem_Name, SMemCapNeed);
				Read(out d, DoWrite);
				return;
			}
			catch (ObjectDisposedException) { }//無視
			catch (Exception) { throw; }

			return;
		}
		public bool Write(in T[] data)
		{
			if (disposedValue == true) return false;
			if (data == null) return false;
			Data = data;
			if (No_SMem_Mode) return true;
			if (MMF == null) return false;

			long SMemCapNeed = data.Length * Elem_Size + SizeD_Size;

			try
			{
				using (var m = MMF.CreateViewAccessor())
				{
					if (!(m?.CanWrite == true)) return false;
					m.Write(0, data.Length);
					if (SMemCapNeed <= m.Capacity)//要求キャパが使用するSMemのキャパより小さければWrite続行
					{
						m.WriteArray(SizeD_Size, data, 0, data.Length);
						return true;
					}
				}

				//ReOpen
				MMF?.Dispose();
				MMF = MemoryMappedFile.CreateOrOpen(SMem_Name, SMemCapNeed);
				return Write(data);
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

				// TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
				// TODO: 大きなフィールドを null に設定します。

				MMF?.Dispose();

				disposedValue = true;
			}
		}

		// TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
		// ~SMemCtrler()
		// {
		//   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
		//   Dispose(false);
		// }

		// このコードは、破棄可能なパターンを正しく実装できるように追加されました。
		public void Dispose()
		{
			// このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
			Dispose(true);
			// TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
			// GC.SuppressFinalize(this);
		}
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
			ARDoing = false;
			ARThread?.Join(5000);
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

				// TODO: アンマネージ リソース (アンマネージ オブジェクト) を解放し、下のファイナライザーをオーバーライドします。
				// TODO: 大きなフィールドを null に設定します。
				ARDoing = false;
				Stop();
				SMC = null;
				ARThread = null;

				disposedValue = true;
			}
		}

		// TODO: 上の Dispose(bool disposing) にアンマネージ リソースを解放するコードが含まれる場合にのみ、ファイナライザーをオーバーライドします。
		// ~SMC_ASSupport()
		// {
		//   // このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
		//   Dispose(false);
		// }

		// このコードは、破棄可能なパターンを正しく実装できるように追加されました。
		public void Dispose()
		{
			// このコードを変更しないでください。クリーンアップ コードを上の Dispose(bool disposing) に記述します。
			Dispose(true);
			// TODO: 上のファイナライザーがオーバーライドされる場合は、次の行のコメントを解除してください。
			// GC.SuppressFinalize(this);
		}
		#endregion


	}

	/// <summary>値が変化した際に発火するイベントの引数</summary>
	/// <typeparam name="T">値の型</typeparam>
	public class ValueChangedEventArgs<T>
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
