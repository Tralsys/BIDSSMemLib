using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace TR
{
	public class SMemCtrler<T> : IDisposable where T : struct
	{
		private const MethodImplOptions MIOpt = (MethodImplOptions)256;//MethodImplOptions.AggressiveInlining;

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

		IMyTask ARThread = null;

		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public SMemCtrler(string SMemName, bool IsArray = false, bool No_SMem = false, bool No_Event = false)
		{

			//[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.//ローカル関数へのMethodImpl指定はLangVer9.0以上だそうなので, 一旦無効化
#if !(NET35 || NET20)
			async
#endif
			void AR_Action(Action<object> ReadAction)
			{
				ARDoing = true;
				while (ARDoing && !disposing && !disposedValue)
				{
					//_ = ReadAction.BeginInvoke(ReadAction.EndInvoke, null);
					ReadAction.Invoke(null);
#if !(NET35 || NET20)
					await
#endif
						MyTask.Delay((int)ARInterval);
				}
				ARDoing = false;
			}

			if (string.IsNullOrEmpty(SMemName)) throw new ArgumentException("SMemNameに無効な値が指定されています.");
			SMem_Name = SMemName;
			No_SMem_Mode = No_SMem;
			No_Event_Mode = No_Event;

			if (No_SMem_Mode) return;
			MMF = new SMemIF(SMem_Name, Elem_Size);
			if (IsArray) ARThread = new MyTask((_) => AR_Action(OnlyReadArr));
			else ARThread = new MyTask((_) => AR_Action(OnlyRead));
		}

		private T _Data = default;
		public T Data
		{
			get => _Data;
			private set
			{
				if (!No_Event_Mode && !Equals(_Data, value))
				{
					T oldD = _Data;
					_Data = value;
					MyTask.Run((_) => ValueChanged?.Invoke(this, new ValueChangedEventArgs<T>(oldD, value)));
				}
				else _Data = value;
			}
		}

		private T[] _ArrData = new T[0];
		public T[] ArrData
		{
			get => _ArrData;
			private set
			{
				if (!No_Event_Mode && !ArrayEqual(_ArrData, value))
				{
					T[] oldD = _ArrData;
					MyTask.Run((_) => ArrValueChanged?.Invoke(this, new ValueChangedEventArgs<T[]>(oldD, value)));
				}
				_ArrData = value;
			}
		}

		private bool ArrayEqual(in T[] a, in T[] b)
		{
			if (a is null || b is null)//どっちもnullなら不一致とする
				return false;

			if (a.Length != b.Length)
				return false;

			for (int i = 0; i < a.Length; i++)
				if (!a[i].Equals(b[i]))
					return false;

			return true;
		}

		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public void OnlyRead(object o = null) => Read(out _, true);

		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public T Read(in bool DoWrite = true)
		{
			T d = default;

			Read(out d, DoWrite);

			return d;
		}

		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public void Read(out T d, in bool DoWrite = true)
		{
			d = Data;
			if (No_SMem_Mode || MMF == null) return;

			try
			{
				if (MMF.Read(0, out d) && DoWrite) Data = d;
			}
			catch (ObjectDisposedException) { }
		}

		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public bool Write(in T data)
		{
			if (!Equals(data, Data)) Data = data;

			if (No_SMem_Mode) return true;

			return MMF.Write(0, ref _Data);
		}

		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public void OnlyReadArr(object o = null) => ReadArr(out _, true);

		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public T[] ReadArr(in bool DoWrite = true)
		{
			T[] d = default;

			ReadArr(out d, DoWrite);

			return d;
		}
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public void ReadArr(out T[] d, in bool DoWrite = true)
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
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
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
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public void AR_Start(int Interval = 10) => AR_Start((uint)Interval);
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
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

		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
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

}
