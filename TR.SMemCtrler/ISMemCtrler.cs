using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace TR
{
	/// <summary>すべての型引数で共通したRW機能を提供するためのインターフェイス</summary>
	public interface IReadWriteInObject : IDisposable
	{
		object ReadInObject();
		bool TryReadInObject(out object obj);
		void WriteInObject(in object obj);
		bool TryWriteInObject(in object obj);
	}

	public interface ISMemCtrler<T> : IDisposable, INotifyPropertyChanged
	{
		IAutoReadSupporter<T> AutoRead { get; }

		/// <summary>共有メモリ領域を使用してデータを共有するかどうか</summary>
		bool No_SMem_Mode { get; set; }

		/// <summary>データ更新時にイベントを発火させるかどうか</summary>
		bool No_Event_Mode { get; set; }

		/// <summary>共有メモリの名前</summary>
		string SMem_Name { get; }

		/// <summary>要素のサイズ</summary>
		uint Elem_Size { get; }

		event EventHandler<ValueChangedEventArgs<T>> ValueChanged;

		T Value { get; set; }

		T Read();
		bool TryRead(out T value);

		void Write(in T value);
		bool TryWrite(in T value);
	}

	public interface IArrayDataSMemCtrler<T> : IList<T>, ISMemCtrler<List<T>>
	{
		event EventHandler<ValueChangedEventArgs<T[]>> ArrValueChanged;

		T Read(in int index);
		bool TryRead(in int index, out T value);

		void Write(in T[] array);
		void Write(in int index, in T value);
		bool TryWrite(in int index, in T value);

		T[] ToArray();
	}
}
