using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace TR
{
	public class SMemCtrler<T> : IDisposable where T : struct
	{
		#region Readonly Properties
		/// <summary>SMemを用いてデータを共有するか</summary>
		public bool No_SMem_Mode { get; }
		/// <summary>データ更新時にイベントを発火させるか</summary>
		public bool No_Event_Mode { get; }

		/// <summary>SMemの名前</summary>
		public string SMem_Name { get; }
		/// <summary>SMemのサイズ</summary>
		public uint Elem_Size { get; } = (uint)Marshal.SizeOf(default(T));

		const int SizeD_Size = sizeof(int);
		#endregion

		public event EventHandler<ValueChangedEventArgs<T>> ValueChanged;
		public event EventHandler<ValueChangedEventArgs<T[]>> ArrValueChanged;

		protected SMemIF MMF = null;
		public bool ARDoing { get; set; } = false;

		public uint ARInterval { get; set; } = 10;

		Task ARThread = null;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public SMemCtrler(string SMemName, bool IsArray = false, bool No_SMem = false, bool No_Event = false)
		{

			[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
#if !UMNGD
			async
#endif
			void AR_Action(Action ReadAction)
			{
				ARDoing = true;
				while (ARDoing && !disposing && !disposedValue)
				{
					_ = ReadAction.BeginInvoke(ReadAction.EndInvoke, null);
#if UMNGD
					Thread.Sleep((int)ARInterval);
#else
					await Task.Delay((int)ARInterval);
#endif
				}
				ARDoing = false;
			}

			if (string.IsNullOrEmpty(SMemName)) throw new ArgumentException("SMemNameに無効な値が指定されています.");
			SMem_Name = SMemName;
			No_SMem_Mode = No_SMem;
			No_Event_Mode = No_Event;

			if (No_SMem_Mode) return;
			MMF = new SMemIF(SMem_Name, Elem_Size);
			if (IsArray) ARThread = new Task(() => AR_Action(OnlyReadArr));
			else ARThread = new Task(() => AR_Action(OnlyRead));
		}

		private T _Data = default;
		public T Data
		{
			get => _Data;
			set
			{
				if (!No_Event_Mode && !Equals(_Data, value))
				{
					T oldD = _Data;
					_Data = value;
					Task.Run(() =>
					{
						ValueChanged?.Invoke(this, new ValueChangedEventArgs<T>(oldD, value));
					});
				}
				else _Data = value;
			}
		}

		private T[] _ArrData = new T[0];
		public T[] ArrData
		{
			get => _ArrData;
			set
			{
				if (!No_Event_Mode && !_ArrData.SequenceEqual(value))
				{
					T[] oldD = (T[])_ArrData.Clone();
					Task.Run(() =>
					{
						ArrValueChanged?.Invoke(this, new ValueChangedEventArgs<T[]>(oldD, (T[])value.Clone()));
					});
				}
				_ArrData = value;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public void OnlyRead() => Read(out _, true);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public T Read(bool DoWrite = true)
		{
			T d = default;

			Read(out d, DoWrite);

			return d;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public void Read(out T d, bool DoWrite = true)
		{
			d = Data;
			if (No_SMem_Mode || MMF == null) return;

			try
			{
				if (MMF.Read(0, out d) && DoWrite) Data = d;
			}
			catch (ObjectDisposedException) { }
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public bool Write(in T data)
		{
			if (!Equals(data, Data)) Data = data;

			if (No_SMem_Mode) return true;

			return MMF.Write(0, ref _Data);
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public void OnlyReadArr() => ReadArr(out _, true);

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public T[] ReadArr(bool DoWrite = true)
		{
			T[] d = default;

			ReadArr(out d, DoWrite);

			return d;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public void ReadArr(out T[] d, bool DoWrite = true)
		{
			d = _ArrData;
			if (No_SMem_Mode || MMF == null) return;//SMem使えないなら内部記録だけを返す.
			try
			{
				int ArrInSMem_Len = 0;
				if (!MMF.Read(0, out ArrInSMem_Len)) return;//配列長の取得(失敗したらしゃーなし)
				d = new T[ArrInSMem_Len];
				if (ArrInSMem_Len <= 0) return;//配列長0なら後は読まない.
				if (MMF.ReadArray(SizeD_Size, d, 0, d.Length) && DoWrite) ArrData = d;//読込成功かつ書き込み可=>Data更新(更新確認はsetterの仕事)
			}
			catch (ObjectDisposedException) { }//無視

			return;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public bool WriteArr(in T[] data)
		{
			if (disposedValue == true || !(data?.Length > 0)) return false;
			ArrData = data;//内部データ更新
			if (No_SMem_Mode) return true;
			if (MMF == null) return false;

			try
			{
				int len = data.Length;
				MMF.Write(0, ref len);
				if (len > 0) return MMF.WriteArray(SizeD_Size, data, 0, len);
			}
			catch (ObjectDisposedException) { }

			return false;
		}

		#region Auto Read Methods
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public void AR_Start(int Interval = 10) => AR_Start((uint)Interval);
		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public void AR_Start(uint Interval)
		{
			if (MMF == null || ARThread == null) return;
			ARInterval = Interval;
			if (ARDoing == true) return;
			ARDoing = true;
			try
			{
				ARThread.Start();
			}
			catch (Exception e)
			{
				Console.WriteLine("ThreadStart at SMemCtrler<{0}>.ar_start({1}) : {2}", typeof(T), Interval, e);
				ARDoing = false;
				return;
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]//関数のインライン展開を積極的にやってもらう.
		public void AR_Stop()
		{
			if (MMF == null || ARThread == null) return;
			if (!ARDoing || ARThread?.IsCompleted != false) return;
			ARDoing = false;
			ARThread.Wait(1000 + (int)ARInterval);
		}
#endregion

#region IDisposable Support
		private bool disposedValue = false;
		private bool disposing = false;

		protected virtual void Dispose(bool disposing)
		{
			disposing = true;
			if (!disposedValue)
			{
				if (disposing)
				{
					// TODO: マネージ状態を破棄します (マネージ オブジェクト)。
					AR_Stop();
				}
				ARThread = null;
				MMF?.Dispose();
				MMF = null;

				disposedValue = true;
			}
		}
		public virtual void Dispose() => Dispose(true);
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
#if UMNGD
namespace System.Threading.Tasks
{
	//Taskクラスの代替
	public class Task : IDisposable
	{
		static public void Run(Action act) => act?.BeginInvoke(act.EndInvoke, null);
		public bool IsAlive => !IsCompleted;
		public bool IsCompleted { get; private set; } = false;

		Action Act { get; }

		public Task(Action act)
		{
			if (act == null) throw new ArgumentNullException("arg \"act\" is null");
			Act = act;
		}

		/// <summary>指定時間の間, 処理を停止します.</summary>
		/// <param name="milliseconds">処理を停止する時間</param>
		public static void Delay(int milliseconds)
		{
			if (milliseconds < 0) throw new ArgumentOutOfRangeException("cannot use minus number in the arg \"milliseconds\"");
			Thread.Sleep(milliseconds);
		}

		/// <summary>指定時間だけ, スレッドの実行終了を待機します.</summary>
		/// <param name="milliseconds">待機する時間</param>
		public void Wait(int milliseconds) => Act.EndInvoke(null);
		

		/// <summary>初期化時に指定した処理を実行します.</summary>
		public void Start()
		{
			IsCompleted = false;
			_ = Act.BeginInvoke(Callback, null);
		}

		public void Dispose()
		{
			if (!IsCompleted) Act.EndInvoke(null);
		}

		private void Callback(IAsyncResult ar)
		{
			Act.EndInvoke(ar);
			IsCompleted = true;
		}
	}
}
#endif