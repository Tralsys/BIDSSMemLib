using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;

namespace TR.BIDSSMemLib;

/// <summary>
/// 可変構造(可変長)な共有メモリを使用するためのクラス
/// </summary>
/// <typeparam name="T">使用する型</typeparam>
public partial class VariableSMem<T> : ISMemCtrler<T> where T : new()
{
	IAutoReadSupporter<T>? _AutoRead = null;
	public IAutoReadSupporter<T> AutoRead
	{
		get
		{
			this._AutoRead ??= new AutoReadSupporter<T>(this);
			return this._AutoRead;
		}
	}

	public bool No_SMem_Mode { get; set; }

	public bool No_Event_Mode { get; set; }

	public string SMem_Name => SMemIF.SMemName;

	public uint Elem_Size => (uint)Structure.GetBytes().Count();

	public long Capacity => SMemIF.Capacity;

	public event EventHandler<ValueChangedEventArgs<T>> ValueChanged;

	public event PropertyChangedEventHandler? PropertyChanged;

	T _Value = default(T) ?? new();

	public T Value
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
			Write(value);
		}

		ValueChanged?.Invoke(this, new(oldValue, value));
		PropertyChanged?.Invoke(this, new(nameof(Value)));
	}

	public T Read()
	{
		T result = default(T) ?? new();

		ReadFromSMem(ref result);

		setValue(result, false);

		return result;
	}

	public bool TryRead(out T value)
	{
		try
		{
			value = Read();
		}
		catch (Exception ex)
		{
			Debug.Assert(ex != null);
			value = new();
			return false;
		}

		return true;
	}

	public void Write(in T value)
	{
		WriteToSMem(value);
		setValue(value, false);
	}

	public bool TryWrite(in T value)
	{
		try
		{
			Write(value);
		}
		catch (Exception ex)
		{
			Debug.Assert(ex != null);
			return false;
		}

		return true;
	}
}
