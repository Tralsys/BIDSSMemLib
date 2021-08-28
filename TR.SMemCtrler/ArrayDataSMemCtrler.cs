using System;
using System.Collections;
using System.Collections.Generic;

namespace TR
{
	public class ArrayDataSMemCtrler<T> : SMemCtrlerBase<List<T>>, IList<T>, IArrayDataSMemCtrler<T> where T : struct
	{
		public T this[int index]
		{
			get => Value[index];
			set => Write(index, value);
		}

		public int Count { get; }
		public bool IsReadOnly { get; }

		private static bool ArrayEqual(in T[] a, in T[] b)
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

		public ArrayDataSMemCtrler(in string name, in bool no_smem, in bool no_event) : base(name, no_smem, no_event)
		{
		}

		public void Add(T item)
		{
			throw new NotImplementedException();
		}

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool Contains(T item)
		{
			throw new NotImplementedException();
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			throw new NotImplementedException();
		}

		public override void Dispose()
		{
			throw new NotImplementedException();
		}

		public IEnumerator<T> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		public int IndexOf(T item)
		{
			throw new NotImplementedException();
		}

		public void Insert(int index, T item)
		{
			throw new NotImplementedException();
		}

		public T Read(in int index)
		{
			throw new NotImplementedException();
		}

		public override List<T> Read()
		{
			throw new NotImplementedException();
		}

		public bool Remove(T item)
		{
			throw new NotImplementedException();
		}

		public void RemoveAt(int index)
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

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		protected override void Initialize_MMF() => MMF = new SMemIF(SMem_Name, sizeof(int));
	}
}
