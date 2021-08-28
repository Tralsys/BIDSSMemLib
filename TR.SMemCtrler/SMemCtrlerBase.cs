using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace TR
{
	public abstract class SMemCtrlerBase<T> : ISMemCtrler<T> where T : new()
	{
		protected SMemIF? MMF { get; set; } = null;

		public IAutoReadSupporter<T> AutoRead { get; }
		private bool _No_SMem_Mode = false;
		public bool No_SMem_Mode
		{
			get => _No_SMem_Mode;
			set
			{
				_No_SMem_Mode = value;

				if (MMF is null && !value)
					Initialize_MMF(); //一度インスタンスを取得したのであれば, それを解放せずに使いまわす
			}
		}

		public bool No_Event_Mode { get; set; }
		public string SMem_Name { get; }
		public uint Elem_Size { get; }

		protected T _Value = new();
		public T Value
		{
			get => _Value;
			set => CheckAndNotifyPropertyChanged(value);
		}

		public event EventHandler<ValueChangedEventArgs<T>>? ValueChanged;
		public event PropertyChangedEventHandler? PropertyChanged;

		protected void CheckAndNotifyPropertyChanged(in T newValue, in bool doWriteToSMem = true)
		{
			if (Equals(Value, newValue))
				return;

			T oldValue = Value;
			_Value = newValue;

			if (doWriteToSMem)
				Write(newValue);

			if (!No_Event_Mode)
			{
				ValueChanged?.Invoke(this, new(oldValue, newValue));
				PropertyChanged?.Invoke(this, new(nameof(Value)));
			}
		}

		public SMemCtrlerBase(in string name, in bool no_smem, in bool no_event)
		{
			if (name is null)
				throw new ArgumentNullException("name cannot null");
			else if (name.Length <= 0)
				throw new ArgumentOutOfRangeException("name cannot empty");

			Value = new();
			Elem_Size = (uint)Marshal.SizeOf(default(T));
			SMem_Name = name;
			No_SMem_Mode = no_smem; //必要に応じて, setterでMMFの初期化が行われる
			No_Event_Mode = no_event;

			AutoRead = new AutoReadSupporter<T>(this);
		}

		protected abstract void Initialize_MMF();

		public virtual void Dispose()
		{
			AutoRead.Dispose();
			MMF?.Dispose();
		}

		public abstract T Read();

		public abstract bool TryRead(out T value);

		public abstract bool TryWrite(in T value);

		public abstract void Write(in T value);
	}
}
