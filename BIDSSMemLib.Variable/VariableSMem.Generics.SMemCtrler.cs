using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace TR.BIDSSMemLib;

/// <summary>
/// 可変構造(可変長)な共有メモリを使用するためのクラス
/// </summary>
/// <typeparam name="T">使用する型</typeparam>
public partial class VariableSMem<T> : ISMemCtrler<T> where T : new()
{
	IAutoReadSupporter<T>? AutoRead = null;
	IAutoReadSupporter<T> ISMemCtrler<T>.AutoRead
	{
		get
		{
			this.AutoRead ??= new AutoReadSupporter<T>(this);
			return this.AutoRead;
		}
	}

	bool ISMemCtrler<T>.No_SMem_Mode { get; set; }

	bool ISMemCtrler<T>.No_Event_Mode { get; set; }

	string ISMemCtrler<T>.SMem_Name => SMemIF.SMemName;

	uint ISMemCtrler<T>.Elem_Size => (uint)Structure.GetBytes().Count();

	long ISMemCtrler<T>.Capacity => SMemIF.Capacity;

	event EventHandler<ValueChangedEventArgs<T>>? ValueChanged;
	event EventHandler<ValueChangedEventArgs<T>> ISMemCtrler<T>.ValueChanged
	{
		add => this.ValueChanged += value;
		remove => this.ValueChanged -= value;
	}

	event PropertyChangedEventHandler? PropertyChanged;
	event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
	{
		add => this.PropertyChanged += value;
		remove => this.PropertyChanged -= value;
	}

	T _Value = default(T) ?? new();

	T ISMemCtrler<T>.Value
	{
		get => _Value;
		set => setValue(value, true);
	}

	void setValue(in T value, bool writeToSMem)
	{
		if (value is null)
			throw new ArgumentNullException(nameof(value), "value cannot be null");

		if (value.Equals(_Value))
			return;

		T oldValue = _Value;
		_Value = value;

		if (writeToSMem)
		{
			(this as ISMemCtrler<T>).Write(value);
		}

		ValueChanged?.Invoke(this, new(oldValue, value));
		PropertyChanged?.Invoke(this, new(nameof(ISMemCtrler<T>.Value)));
	}

	T ISMemCtrler<T>.Read()
	{
		T result = default(T) ?? new();

		this.ReadFromSMem(ref result);

		setValue(result, false);

		return result;
	}

	bool ISMemCtrler<T>.TryRead(out T value)
	{
		try
		{
			value = (this as ISMemCtrler<T>).Read();
		}
		catch (Exception ex)
		{
			Debug.Assert(ex != null);
			value = new();
			return false;
		}

		return true;
	}

	void ISMemCtrler<T>.Write(in T value)
	{
		WriteToSMem(value);
		setValue(value, false);
	}

	bool ISMemCtrler<T>.TryWrite(in T value)
	{
		try
		{
			(this as ISMemCtrler<T>).Write(value);
		}
		catch (Exception ex)
		{
			Debug.Assert(ex != null);
			return false;
		}

		return true;
	}
}
