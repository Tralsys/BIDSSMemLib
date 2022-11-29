using System;
using System.ComponentModel;

namespace TR
{
	/// <summary>共有メモリへの簡単なアクセスを提供するクラスの基底となるクラス</summary>
	/// <typeparam name="T">このクラスて使用する型情報</typeparam>
	public abstract class SMemCtrlerBase<T> : ISMemCtrler<T>, IContainsSMemIF where T : new()
	{
		/// <summary>使用する共有メモリ</summary>
		public ISMemIF? MMF { get; private set; } = null;

		/// <summary>自動読み取り機能を提供するクラスのインスタンス</summary>
		public IAutoReadSupporter<T> AutoRead { get; }

		/// <summary>共有メモリを使用せずに動作するかどうか</summary>
		private bool _No_SMem_Mode = false;

		/// <summary>共有メモリを使用せずに動作するかどうか</summary>
		public bool No_SMem_Mode
		{
			get => _No_SMem_Mode;
			set
			{
				_No_SMem_Mode = value;

				if (MMF is null && !value)
				{
					//一度インスタンスを取得したのであれば, それを解放せずに使いまわす
					MMF = new SMemIF(SMem_Name, Capacity);
				}
			}
		}

		/// <summary>イベントを発火させずに動作させるかどうか</summary>
		public bool No_Event_Mode { get; set; }

		/// <summary>共有メモリの名前</summary>
		public string SMem_Name { get; }

		/// <summary>共有メモリに書き込むデータの1要素あたりのサイズ</summary>
		public abstract uint Elem_Size { get; }

		/// <summary>コンストラクタで要求されたキャパシティ</summary>
		private long RequestedCapacity { get; } = 0;

		/// <summary>操作可能な領域のサイズ</summary>
		public long Capacity { get => MMF?.Capacity ?? RequestedCapacity; }

		/// <summary>共有メモリから取得した値のキャッシュ</summary>
		protected T _Value = new();
		private bool disposedValue;

		/// <summary>共有メモリから取得した値のキャッシュ</summary>
		public T Value
		{
			get => _Value;
			set => CheckAndNotifyPropertyChanged(value);
		}

		/// <summary>値の更新があったかどうか</summary>
		public event EventHandler<ValueChangedEventArgs<T>>? ValueChanged;

		/// <summary>INotifyPropertyChangedインターフェイスで提供される, 値の更新があったことを通知するイベント</summary>
		public event PropertyChangedEventHandler? PropertyChanged;

		/// <summary>値の更新があったかどうかを確認し, 更新があれば値を更新したうえでイベントを発火させる</summary>
		/// <param name="newValue">新しい値</param>
		/// <param name="doWriteToSMem">共有メモリに書き込みを行うかどうか (No_SMem_Modeであれば書き込みは行われません)</param>
		protected void CheckAndNotifyPropertyChanged(in T newValue, in bool doWriteToSMem = true)
		{
			if (Equals(Value, newValue))
				return;

			T oldValue = Value;
			_Value = newValue;

			if (!No_SMem_Mode && doWriteToSMem)
				Write(newValue);

			if (!No_Event_Mode)
			{
				ValueChanged?.Invoke(this, new(oldValue, newValue));
				PropertyChanged?.Invoke(this, new(nameof(Value)));
			}
		}

		/// <summary>インスタンスを初期化する</summary>
		/// <param name="name">共有メモリの名前</param>
		/// <param name="no_smem">共有メモリを使用せずに動作させるか</param>
		/// <param name="no_event">イベントを発火させずに使用するか</param>
		/// <param name="capacityRequest">要求キャパシティ [bytes]</param>
		public SMemCtrlerBase(in string name, in bool no_smem, in bool no_event, in long capacityRequest)
		{
			if (name is null)
				throw new ArgumentNullException("name cannot null");
			else if (name.Length <= 0)
				throw new ArgumentOutOfRangeException("name cannot empty");

			Value = new();
			RequestedCapacity = capacityRequest;
			SMem_Name = name;
			No_SMem_Mode = no_smem; //必要に応じて, setterでMMFの初期化が行われる
			No_Event_Mode = no_event;

			AutoRead = new AutoReadSupporter<T>(this);
		}

		#region IDisposable Support
		/// <summary>リソースを解放します</summary>
		/// <param name="disposing">マネージドリソースを解放するかどうか</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					AutoRead.Dispose();
				}

				MMF?.Dispose();
				MMF = null;
				disposedValue = true;
			}
		}

		/// <summary>インスタンスが保持するリソースを解放し, GCを呼びます</summary>
		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
		#endregion

		/// <summary>データを読み取ります</summary>
		/// <returns>読み取った値</returns>
		public abstract T Read();

		/// <summary>データ読み取りを試行します</summary>
		/// <param name="value">読み取った値</param>
		/// <returns>試行結果</returns>
		public abstract bool TryRead(out T value);

		/// <summary>データ書き込みを試行します</summary>
		/// <param name="value">書き込む値</param>
		/// <returns>試行結果</returns>
		public abstract bool TryWrite(in T value);

		/// <summary>データを書き込います</summary>
		/// <param name="value">書き込む値</param>
		public abstract void Write(in T value);
	}
}
