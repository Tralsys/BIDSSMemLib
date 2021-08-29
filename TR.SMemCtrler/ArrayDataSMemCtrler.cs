using System;
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
			set => CheckAndNotifyPropertyChanged(value);
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

		public void Add(T item)
		{
			if (MMF?.Read<int>(0, out var len) == true && len != Value.Count) //長さが違うなら明確にSMemの内容と手元の内容が違う
				UpdateValueFromSMem(); //SMemから手元にコピー

			Value.Add(item);

			_ = (MMF?.Write(sizeof(int) + (Elem_Size * (Value.Count - 1)), ref item)); //SMemに新規要素を追加する
		}

		public void Clear()
		{
			Value.Clear();

			int newLength = 0;
			_ = (MMF?.Write(0, ref newLength)); //長さ情報を0にするだけ
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

			int newLength = Value.Count;
			_ = (MMF?.Write(0, ref newLength)); //長さ情報更新

			_ = (MMF?.WriteArray(sizeof(int) + (Elem_Size * index), Value.ToArray(), index, Value.Count - index));

			return true;
		}

		public void RemoveAt(int index)
		{
			UpdateValueFromSMem();
			if (Count <= index)
				throw new IndexOutOfRangeException($"Array Length in SMem was {Count} (Your order is {index})");

			Value.RemoveAt(index);

			int newLength = Value.Count;
			_ = (MMF?.Write(0, ref newLength)); //長さ情報更新

			_ = (MMF?.WriteArray(sizeof(int) + (Elem_Size * index), Value.ToArray(), index, Value.Count - index));
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator_T();


		#region SMemCtrler
		public T Read(in int index)
		{
			throw new NotImplementedException();
		}

		public override List<T> Read()
		{
			throw new NotImplementedException();
		}

		public bool TryRead(in int index, out T value)
		{
			throw new NotImplementedException();
		}

		public override bool TryRead(out List<T> value)
		{
			throw new NotImplementedException();
		}

		public bool TryWrite(in int index, in T value)
		{
			throw new NotImplementedException();
		}

		public override bool TryWrite(in List<T> value)
		{
			throw new NotImplementedException();
		}

		public void Write(in int index, in T value)
		{
			throw new NotImplementedException();
		}

		public override void Write(in List<T> value)
		{
			throw new NotImplementedException();
		}

		public void Write(in T[] array)
		{
			throw new NotImplementedException();
		}

		protected override void Initialize_MMF() => MMF = new SMemIF(SMem_Name, sizeof(int));
		#endregion
	}
}
