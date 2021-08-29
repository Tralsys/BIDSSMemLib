﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace TR
{
	public class ArrayDataSMemCtrler<T> : SMemCtrlerBase<List<T>>, IList<T>, IArrayDataSMemCtrler<T> where T : struct
	{
		/// <summary></summary>
		protected new List<T> Value
		{
			get => _Value;
			set => CheckAndNotifyPropertyChanged(value, false); // 常にWrite系メソッドからsetされることを想定する
		}

		public T this[int index]
		{
			get => Value[index];
			set => Write(index, value);
		}

		public int Count { get => Value.Count; }
		public bool IsReadOnly { get => false; }

		public ArrayDataSMemCtrler(in string name, in bool no_smem, in bool no_event) : base(name, no_smem, no_event)
		{
			
		}

		void UpdateValueFromSMem() => _ = Read();

		public void Add(T item) => Add(in item);
		public void Add(in T item)
		{
			if (TryGetLengthInSMem(out var len) == true && len != Value.Count) //長さが違うなら明確にSMemの内容と手元の内容が違う
				UpdateValueFromSMem(); //SMemから手元にコピー

			Value.Add(item);

			Write(Value.Count - 1, item); //SMemに新規要素を追加する
		}

		public void Clear()
		{
			Value.Clear();

			Write(Value);//空のリストを書き込むことで消去扱い
		}

		public bool Contains(T item)
		{
			UpdateValueFromSMem(); //SMemから手元にコピーする

			return Value.Contains(item);
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			UpdateValueFromSMem();

			for (int i = 0; i < Value.Count; i++)
				array[arrayIndex + i] = Value[i];
		}

		public override void Dispose()
		{
			AutoRead.Dispose();
			MMF?.Dispose();
		}

		protected IEnumerator<T> GetEnumerator_T()
		{
			UpdateValueFromSMem();

			return Value.GetEnumerator();
		}
		public IEnumerator<T> GetEnumerator() => GetEnumerator_T();

		public int IndexOf(T item) => IndexOf(in item);
		public int IndexOf(in T item)
		{
			UpdateValueFromSMem();

			return Value.IndexOf(item);
		}

		public void Insert(int index, T item)
		{
			UpdateValueFromSMem();

			Value.Insert(index, item);

			if (!No_SMem_Mode)
				_ = (MMF?.WriteArray(sizeof(int) + (Elem_Size * index), Value.ToArray(), index, Value.Count - index)); //SMemの内容も更新する
		}

		public bool Remove(T item) => Remove(in item);
		public bool Remove(in T item)
		{
			UpdateValueFromSMem();
			if (!Value.Contains(item))
				return false;

			int index = Value.IndexOf(item);

			Value.RemoveAt(index);

			if (!No_SMem_Mode)
			{
				int newLength = Value.Count;

				_ = (MMF?.Write(0, ref newLength)); //長さ情報更新

				_ = (MMF?.WriteArray(sizeof(int) + (Elem_Size * index), Value.ToArray(), index, Value.Count - index));
			}
			return true;
		}

		public void RemoveAt(int index)
		{
			UpdateValueFromSMem();
			if (Count <= index)
				throw new IndexOutOfRangeException($"Array Length in SMem was {Count} (Your order is {index})");

			Value.RemoveAt(index);

			if (!No_SMem_Mode)
			{
				int newLength = Value.Count;
				_ = (MMF?.Write(0, ref newLength)); //長さ情報更新

				_ = (MMF?.WriteArray(sizeof(int) + (Elem_Size * index), Value.ToArray(), index, Value.Count - index));
			}
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator_T();


		#region SMemCtrler
		public T Read(in int index)
		{
			if (!No_SMem_Mode && MMF?.Read(sizeof(int) + (Elem_Size * index), out T buf) == true)
			{
				_Value[index] = buf; //値を更新
				return buf;
			}

			return Value[index];
		}

		public override List<T> Read()
		{
			if (No_SMem_Mode || TryGetLengthInSMem(out var len) != true)
				return Value;

			if (len <= 0)
				return Value = new();

			T[] buf = new T[len];

			if (MMF?.ReadArray(sizeof(int), buf, 0, len) == true)
				Value = new(buf);

			return Value;
		}

		public bool TryRead(in int index, out T value)
		{
			if (!No_SMem_Mode && MMF?.Read(sizeof(int) + (Elem_Size * index), out value) == true)
				return true;

			value = Value.Count > index ? Value[index] : default;
			return false;
		}

		public override bool TryRead(out List<T> value)
		{
			value = Value;

			if (TryGetLengthInSMem(out int len) != true)
				return false;

			if (len <= 0)
			{
				Value = new();
				return true;
			}

			T[] buf = new T[len];

			if (MMF?.ReadArray(sizeof(int), buf, 0, len) == true)
			{
				Value = new(buf);
				return true;
			}
			else
				return false;
		}

		public bool TryWrite(in int index, in T value) => TryWrite(index, value);
		public bool TryWrite(in int index, T value)
		{
			UpdateValueFromSMem();

			if (Value.Count < index)
			{
				int last_count = Value.Count;
				var arr = new T[last_count - index];
				Value.AddRange(arr);

				if (!No_SMem_Mode)
				{
					_ = (MMF?.WriteArray(sizeof(int) + (Elem_Size * last_count), arr, 0, arr.Length));

					int newLen = Value.Count;
					_ = (MMF?.Write(0, ref newLen)); //長さ情報を更新
				}
			}

			Value[index] = value;

			if (!No_SMem_Mode)
				_ = (MMF?.Write(sizeof(int) + (Elem_Size * index), ref value));

			return true;
		}

		public override bool TryWrite(in List<T> value)
		{
			Value = value;

			int newLen = value.Count;
			if (!No_SMem_Mode)
				_ = (MMF?.Write(0, ref newLen));

			if (newLen <= 0)
				return true;

			if (!No_SMem_Mode)
				_ = (MMF?.WriteArray(sizeof(int), value.ToArray(), 0, newLen));

			return true;
		}

		public void Write(in int index, in T value) => _ = TryWrite(index, value);

		public override void Write(in List<T> value) => _ = TryWrite(value);

		public void Write(in T[] array) => Write(new List<T>(array));

		protected override void Initialize_MMF() => MMF = new SMemIF(SMem_Name, sizeof(int));

		private bool TryGetLengthInSMem(out int len)
		{
			len = 0;
			if (No_SMem_Mode)
				return false;

			return MMF?.Read(0, out len) ?? false;
		}
		#endregion
	}
}