using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace TR
{
	/// <summary>共有メモリを利用して配列データを共有する</summary>
	/// <typeparam name="T">使用する型 (struct)</typeparam>
	public class ArrayDataSMemCtrler<T> : SMemCtrlerBase<List<T>>, IList<T>, IArrayDataSMemCtrler<T>, IReadWriteInObject where T : struct
	{
		/// <summary>
		/// 4096バイト以上になる配列データを扱うための共有メモリ
		/// 長さ情報は長さ共有専用の共有メモリに保持する
		/// </summary>
		private ISMemIF? MMF4Array { get; set; } = null;
		/// <summary>
		/// 4096バイト以上になる配列データを扱うための共有メモリの要素数を保持する共有メモリ
		/// </summary>
		private ISMemIF? MMF4ArrayLength { get; set; } = null;
		private string getArraySMemName(in int len) => $"{SMem_Name}_{len}";
		const uint PAGE_SIZE = 4096;
		private static readonly uint MMF_ELEM_MAX_COUNT;

		/// <summary>配列に更新があった際に発行されるイベント</summary>
		public event EventHandler<ValueChangedEventArgs<T[]>>? ArrValueChanged;

		/// <summary>任意の位置に要素を読み書きする (読み込みはキャッシュから行われます)</summary>
		/// <param name="index">読み書きする位置 [要素目]</param>
		/// <returns>読み込んだ結果</returns>
		public T this[int index]
		{
			get => Value[index];
			set => Write(index, value);
		}

		/// <summary>キャッシュの要素数 [個]</summary>
		public int Count { get => Value.Count; }

		/// <summary>読み取り専用かどうか (常にfalseを返します)</summary>
		public bool IsReadOnly { get => false; }

		/// <summary>1要素あたりのサイズ [bytes]</summary>
		public override uint Elem_Size { get; } = ELEM_SIZE;
		private static readonly uint ELEM_SIZE;

		private int MMF4ArrayLengthValue => MMF4ArrayLength?.Read(0, out int value) == true ? value : 0;

		/// <summary>インスタンスを初期化します</summary>
		/// <param name="name">共有メモリの名前</param>
		/// <param name="no_smem">共有メモリを使用せず, キャッシュ管理のみを行うモードで起動するかどうか</param>
		/// <param name="no_event">イベントを発火させないモードで使用するかどうか</param>
		/// <param name="maxCount">書き込む最大要素数 (既に他のプロセスによって共有メモリが作成されていた場合, そこで設定されたキャパシティが使用されます)</param>
		public ArrayDataSMemCtrler(in string name, in bool no_smem, in bool no_event, in int maxCount) : base(name, no_smem, no_event, sizeof(int) + (ELEM_SIZE * MMF_ELEM_MAX_COUNT))
		{
			ValueChanged += (_, e) => ArrValueChanged?.Invoke(this, new(e.OldValue.ToArray(), e.NewValue.ToArray()));

			// 念のためlong分だけ要求しておく
			MMF4ArrayLength = new SMemIF(SMem_Name + "_length", sizeof(long));
			MMF4Array = new SMemIF(getArraySMemName(maxCount), maxCount * ELEM_SIZE);
		}

		static ArrayDataSMemCtrler()
		{
			if (typeof(T).IsValueType == false)
				throw new ArgumentException("T must be a value type");
			ELEM_SIZE = (uint)Marshal.SizeOf(default(T));
			MMF_ELEM_MAX_COUNT = (PAGE_SIZE - sizeof(int)) / ELEM_SIZE;
		}

		bool CheckAndUpdateMMF4ArrayInstance(int? newLen = null, bool disposeOldInstance = true)
		{
			if (MMF4Array is null || MMF4ArrayLength is null)
				return false;

			int currentLength = (int)(MMF4Array.Capacity / ELEM_SIZE);
			int expectedLength;
			if (newLen.HasValue)
			{
				expectedLength = newLen.Value;
				_ = MMF4ArrayLength.Write(0, ref expectedLength);
			}
			else
			{
				expectedLength = MMF4ArrayLengthValue;
			}
			if (expectedLength != currentLength)
			{
				if (disposeOldInstance)
					MMF4Array.Dispose();
				MMF4Array = new SMemIF(getArraySMemName(expectedLength), expectedLength * ELEM_SIZE);
				return true;
			}
			return false;
		}

		void UpdateValueFromSMem() => _ = Read();

		#region IList
		/// <summary>リストに要素を追加します</summary>
		/// <param name="item">追加する要素</param>
		public void Add(T item) => Add(in item);

		/// <summary>リストに要素を追加します</summary>
		/// <param name="item">追加する要素</param>
		public void Add(in T item)
		{
			if (TryGetLengthInSMem(out var len) == true && len != Value.Count) //長さが違うなら明確にSMemの内容と手元の内容が違う
				UpdateValueFromSMem(); //SMemから手元にコピー

			Value.Add(item);

			if (No_SMem_Mode)
				return;

			int newLen = Value.Count;
			int newLenCompat = Math.Min((int)MMF_ELEM_MAX_COUNT, newLen);

			//コピーの必要が出てはじめてコピーをする
			T copied_item = item;

			//長さ情報の更新
			_ = (MMF?.Write(0, ref newLenCompat));

			//配列にデータを追加
			if (newLen <= MMF_ELEM_MAX_COUNT)
			{
				_ = (MMF?.Write(sizeof(int) + (ELEM_SIZE * (newLen - 1)), ref copied_item));
			}

			CheckAndUpdateMMF4ArrayInstance(newLen);
			_ = MMF4Array?.WriteArray(0, Value.ToArray(), 0, newLen);
		}

		/// <summary>リストを空にします</summary>
		public void Clear()
		{
			Value.Clear();

			Write(Value);//空のリストを書き込むことで消去扱い
		}

		/// <summary>リスト中に指定のアイテムが含まれているかどうかを確認します</summary>
		/// <param name="item">確認するアイテム</param>
		/// <returns>要素が含まれていたかどうか</returns>
		public bool Contains(T item) => Contains(in item);

		/// <summary>リスト中に指定のアイテムが含まれているかどうかを確認します</summary>
		/// <param name="item">確認するアイテム</param>
		/// <returns>要素が含まれていたかどうか</returns>
		public bool Contains(in T item)
		{
			UpdateValueFromSMem(); //SMemから手元にコピーする

			return Value.Contains(item);
		}

		/// <summary>指定の配列の指定の位置から, Value[0] ~ Value[Count - 1]の要素を順番に書き込みます</summary>
		/// <param name="array">書き込み先の配列</param>
		/// <param name="arrayIndex">書き込みを開始する配列のインデックス</param>
		public void CopyTo(T[] array, int arrayIndex)
		{
			UpdateValueFromSMem();

			for (int i = 0; i < Value.Count; i++)
				array[arrayIndex + i] = Value[i];
		}

		/// <summary>Enumeratorを取得します</summary>
		/// <returns>Enumerator</returns>
		public IEnumerator<T> GetEnumerator()
		{
			UpdateValueFromSMem();

			return Value.GetEnumerator();
		}

		/// <summary>指定の要素のインデックスを取得します</summary>
		/// <param name="item">検索したい要素</param>
		/// <returns>インデックス</returns>
		public int IndexOf(T item) => IndexOf(in item);

		/// <summary>指定の要素のインデックスを取得します</summary>
		/// <param name="item">検索したい要素</param>
		/// <returns>インデックス</returns>
		public int IndexOf(in T item)
		{
			UpdateValueFromSMem();

			return Value.IndexOf(item);
		}

		/// <summary>指定のindexに指定のitemを挿入する.  indexにあった要素は順に後ろにずれる</summary>
		/// <param name="index">挿入する位置</param>
		/// <param name="item">挿入する要素</param>
		public void Insert(int index, T item) => Insert(in index, in item);

		/// <summary>指定のindexに指定のitemを挿入する.  indexにあった要素は順に後ろにずれる</summary>
		/// <param name="index">挿入する位置</param>
		/// <param name="item">挿入する要素</param>
		public void Insert(in int index, in T item)
		{
			UpdateValueFromSMem();

			Value.Insert(index, item);

			if (No_SMem_Mode)
				return;

			int newLen = Value.Count;
			int newLenCompat = Math.Min((int)MMF_ELEM_MAX_COUNT, newLen);
			_ = (MMF?.Write(0, ref newLenCompat)); //長さ情報の更新

			CheckAndUpdateMMF4ArrayInstance(newLen);
			_ = MMF4Array?.WriteArray(0, Value.ToArray(), 0, newLen); //SMemの内容も更新する

			if (index < MMF_ELEM_MAX_COUNT)
			{
				_ = (MMF?.WriteArray(sizeof(int) + (ELEM_SIZE * index), Value.ToArray(), index, newLenCompat - index)); //SMemの内容も更新する
			}
		}

		/// <summary>指定の要素を削除する  要素が存在しなければfalseが返ります</summary>
		/// <param name="item">削除する要素</param>
		/// <returns>実行結果</returns>
		/// <remarks>削除する要素の位置が解っている場合, RemoveAtを呼び出した方が効率的です</remarks>
		public bool Remove(T item) => Remove(in item);

		/// <summary>指定の要素を削除する  要素が存在しなければfalseが返ります</summary>
		/// <param name="item">削除する要素</param>
		/// <returns>実行結果</returns>
		/// <remarks>削除する要素の位置が解っている場合, RemoveAtを呼び出した方が効率的です</remarks>
		public bool Remove(in T item)
		{
			UpdateValueFromSMem();
			if (!Value.Contains(item))
				return false;

			int index = Value.IndexOf(item);

			_RemoveAt(index);

			return true;
		}

		/// <summary>指定の位置にある要素を削除します</summary>
		/// <param name="index">削除する要素の位置</param>
		public void RemoveAt(int index)
		{
			UpdateValueFromSMem();
			if (Count <= index)
				throw new IndexOutOfRangeException($"Array Length in SMem was {Count} (Your order is {index})");

			_RemoveAt(index);
		}

		private void _RemoveAt(in int index)
		{
			Value.RemoveAt(index);
			if (No_SMem_Mode)
				return;

			int newLength = Value.Count;
			int newLengthCompat = Math.Min((int)MMF_ELEM_MAX_COUNT, newLength);
			_ = (MMF?.Write(0, ref newLengthCompat)); //長さ情報更新
			CheckAndUpdateMMF4ArrayInstance(newLength);
			_ = MMF4Array?.WriteArray(0, Value.ToArray(), 0, newLength);

			if (index < MMF_ELEM_MAX_COUNT)
			{
				int moveLenCompat = newLength == newLengthCompat ? newLengthCompat - index : newLengthCompat - index + 1;
				_ = (MMF?.WriteArray(sizeof(int) + (ELEM_SIZE * index), Value.ToArray(), index, moveLenCompat));
			}
		}

		/// <summary>Enumeratorを取得します</summary>
		/// <returns>Enumerator</returns>
		IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
		#endregion


		#region SMemCtrler
		/// <summary>指定の位置にある要素を共有メモリから取得します</summary>
		/// <param name="index">取得する位置</param>
		/// <returns>取得した要素</returns>
		/// <remarks>指定要素のキャッシュ更新のみ行われ, キャッシュ全体の更新は実行されません</remarks>
		public T Read(in int index)
		{
			if (No_SMem_Mode)
				return Value[index];

			if (index < MMF_ELEM_MAX_COUNT)
			{
				if (MMF?.Read(sizeof(int) + (ELEM_SIZE * index), out T buf) == true)
				{
					Value[index] = buf;
					return buf;
				}
			}
			else
			{
				CheckAndUpdateMMF4ArrayInstance();
				if (MMF4Array?.Read(ELEM_SIZE * index, out T buf) == true)
				{
					Value[index] = buf;
					return buf;
				}
			}

			return Value[index];
		}

		/// <summary>共有メモリにある配列データを取得します</summary>
		/// <returns>取得した配列データを記録したリスト</returns>
		/// <remarks>キャッシュ全体の更新が行われます</remarks>
		public override List<T> Read()
		{
			if (No_SMem_Mode || TryGetLengthInSMem(out var len) != true)
				return Value;

			if (len <= 0)
				return Value = new();

			T[] buf = new T[len];

			int lenCompat = Math.Min((int)MMF_ELEM_MAX_COUNT, len);
			bool isSuccessCompat = MMF?.ReadArray(sizeof(int), buf, 0, lenCompat) == true;
			if (isSuccessCompat)
			{
				if (len != lenCompat)
				{
					_ = MMF4Array?.ReadArray(ELEM_SIZE * lenCompat, buf, lenCompat, len - lenCompat);
				}
				Value = new(buf);
			}
			else if (MMF4Array?.ReadArray(0, buf, 0, len) == true)
			{
				Value = new(buf);
			}

			return Value;
		}

		/// <summary>共有メモリにあるデータの取得を試行します</summary>
		/// <param name="index">配列内のインデックス</param>
		/// <param name="value">取得した要素の書き込み先 (読み込みに失敗した場合はdefault値が書き込まれます)</param>
		/// <returns>試行結果</returns>
		public bool TryRead(in int index, out T value)
		{
			if (No_SMem_Mode)
			{
				value = index < Value.Count ? Value[index] : default;
				return true;
			}

			if (index < MMF_ELEM_MAX_COUNT)
			{
				if (MMF?.Read(sizeof(int) + (ELEM_SIZE * index), out value) == true)
					return true;
			}
			else
			{
				if (MMF4Array?.Read(ELEM_SIZE * index, out value) == true)
					return true;
			}

			value = index < Value.Count ? Value[index] : default;
			return false;
		}

		/// <summary>共有メモリにある配列全体の取得を試行します</summary>
		/// <param name="value">取得した要素の書き込み先 (読み込みに失敗した場合は空リストが書き込まれます)</param>
		/// <returns>試行結果</returns>
		public override bool TryRead(out List<T> value)
		{
			value = Value;
			if (No_SMem_Mode)
				return true;

			if (TryGetLengthInSMem(out int len) != true)
				return false;

			if (len <= 0)
			{
				Value = new();
				return true;
			}

			T[] buf = new T[len];

			int lengthCompat = Math.Min((int)MMF_ELEM_MAX_COUNT, len);
			if (MMF?.ReadArray(sizeof(int), buf, 0, lengthCompat) == true)
			{
				if (len != lengthCompat)
				{
					_ = MMF4Array?.ReadArray(ELEM_SIZE * lengthCompat, buf, lengthCompat, len - lengthCompat);
				}

				Value = new(buf);
				return true;
			}
			else
				return false;
		}

		/// <summary>共有メモリの配列内指定の位置に指定の要素の書き込みを試行します</summary>
		/// <param name="index">書き込む配列内位置</param>
		/// <param name="value">書き込む値</param>
		/// <returns>試行結果</returns>
		public bool TryWrite(in int index, in T value)
		{
			if (No_SMem_Mode)
			{
				if (Value.Count <= index)
				{
					T[] arr = new T[(index - Value.Count) + 1];
					arr[arr.Length - 1] = value;
					Value.AddRange(arr);
				}
				else
				{
					Value[index] = value;
				}
				return true;
			}

			UpdateValueFromSMem();

			T copied_value = value;
			if (Value.Count <= index)
			{
				int last_count = Value.Count;
				var arr = new T[(last_count - index) + 1];
				arr[arr.Length - 1] = value;
				Value.AddRange(arr);
				long tailPos = sizeof(int) + (ELEM_SIZE * last_count);

				int newLength = last_count + arr.Length;
				int newLenCompat = Math.Min((int)MMF_ELEM_MAX_COUNT, newLength);
				if (last_count < newLenCompat)
				{
					_ = (MMF?.Write(0, ref newLenCompat)); //長さ情報を更新
				}

				int copyLenCompat = Math.Min((int)MMF_ELEM_MAX_COUNT - last_count, arr.Length);
				if (0 < copyLenCompat)
				{
					_ = (MMF?.WriteArray(tailPos, arr, 0, copyLenCompat));
				}

				int lastMMF4ArrayLength = MMF4ArrayLengthValue;
				if (lastMMF4ArrayLength < newLength)
				{
					// 長さ情報を更新
					ISMemIF? oldMMF4Array = MMF4Array;
					CheckAndUpdateMMF4ArrayInstance(newLength, false);
					if (oldMMF4Array is not null)
					{
						T[] tmp = new T[lastMMF4ArrayLength];
						_ = oldMMF4Array.ReadArray(0, tmp, 0, lastMMF4ArrayLength);
						_ = MMF4Array?.WriteArray(0, tmp, 0, lastMMF4ArrayLength);
						oldMMF4Array.Dispose();
					}
					else
					{
						_ = MMF4Array?.WriteArray(0, Value.ToArray(), 0, Value.Count);
					}
				}

				_ = (MMF4Array?.WriteArray(ELEM_SIZE * last_count, arr, 0, arr.Length));
			}
			else
			{
				Value[index] = value;

				if (index < MMF_ELEM_MAX_COUNT)
				{
					_ = (MMF?.Write(sizeof(int) + (ELEM_SIZE * index), ref copied_value));
				}
				_ = (MMF4Array?.Write(ELEM_SIZE * index, ref copied_value));
			}

			return true;
		}

		/// <summary>指定のリストを共有メモリに書き込む操作を試行します</summary>
		/// <param name="value">書き込むリスト</param>
		/// <returns>実行結果</returns>
		public override bool TryWrite(in List<T> value)
		{
			Value = value;
			if (No_SMem_Mode)
				return true;

			int newLen = value.Count;
			int newLenCompat = Math.Min((int)MMF_ELEM_MAX_COUNT, newLen);
			_ = (MMF?.Write(0, ref newLenCompat));

			CheckAndUpdateMMF4ArrayInstance(newLen);

			if (newLen <= 0)
				return true;

			_ = (MMF?.WriteArray(sizeof(int), value.ToArray(), 0, newLenCompat));
			_ = (MMF4Array?.WriteArray(0, value.ToArray(), 0, newLen));

			return true;
		}

		/// <summary>指定の値を指定の位置に書き込みます</summary>
		/// <param name="index">書き込む配列内位置</param>
		/// <param name="value">書き込む値</param>
		public void Write(in int index, in T value) => _ = TryWrite(index, value);

		/// <summary>指定のリストを共有メモリに書き込みます</summary>
		/// <param name="value">共有メモリの書き込み先</param>
		public override void Write(in List<T> value) => _ = TryWrite(value);

		/// <summary>指定の配列を共有メモリに書き込みます</summary>
		/// <param name="array">書き込む配列</param>
		public void Write(in T[] array) => Write(new List<T>(array));

		/// <summary>共有メモリに書き込まれている配列長情報の取得を試行する</summary>
		/// <param name="len">取得結果の書き込み先</param>
		/// <returns>試行結果</returns>
		private bool TryGetLengthInSMem(out int len)
		{
			len = 0;
			if (No_SMem_Mode)
				return false;

			int lenCompat = 0;
			bool isSuccessCompat = MMF?.Read(0, out lenCompat) ?? false;
			if (!isSuccessCompat)
				return false;
			int lenArray = MMF4ArrayLengthValue;

			// 旧仕様の場合、lenArrayが存在しないため、lenCompatを使用する
			len = lenArray == 0 ? lenCompat : lenArray;
			string mmf4ArrayName = getArraySMemName(len);
			if (MMF4Array?.SMemName != mmf4ArrayName)
			{
				ISMemIF? oldMMF4Array = MMF4Array;
				MMF4Array = new SMemIF(mmf4ArrayName, ELEM_SIZE * len);
				oldMMF4Array?.Dispose();
			}

			return true;
		}

		/// <summary>キャッシュのリストを配列に変換して取得する</summary>
		/// <returns>変換結果</returns>
		public T[] ToArray() => Value.ToArray();
		#endregion

		#region IReadWriteInObject
		/// <summary>リストを共有メモリから読み取り, object型として返す</summary>
		/// <returns>取得したリスト</returns>
		public object ReadInObject() => Read();

		/// <summary>リストを共有メモリから読み取り, object型として指定の場所に書き込む操作を試行します</summary>
		/// <param name="obj">読み取ったリストの書き込み先</param>
		/// <returns>実行結果</returns>
		public bool TryReadInObject(out object obj)
		{
			bool result = TryRead(out List<T>? value);
			obj = value;
			return result;
		}

		/// <summary>指定のデータを共有メモリに書き込みます</summary>
		/// <param name="obj">書き込むデータ</param>
		public void WriteInObject(in object obj) => Write(value: (List<T>)obj ?? new List<T>());

		/// <summary>指定のデータを共有メモリに書き込む操作を試行します</summary>
		/// <param name="obj">書き込むデータ</param>
		/// <returns>試行結果</returns>
		public bool TryWriteInObject(in object obj)
		{
			var value = obj as List<T>;

			if (value is null)
				return false; //型変換に失敗している

			return TryWrite(in value);
		}
		#endregion

		/// <summary>
		/// 2つのリストが同じ値を持っているかどうかを判定する
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns></returns>
		protected override bool IsValueSame(List<T> v1, List<T> v2)
		{
			if (v1.Count != v2.Count)
				return false;

			for (int i = 0; i < v1.Count; i++)
			{
				if (!Equals(v1[i], v2[i]))
					return false;
			}

			return true;
		}

		bool disposedValue = false;
		/// <summary>
		/// 保持しているリソースを解放する
		/// </summary>
		/// <param name="disposing"></param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (!disposedValue)
			{
				if (disposing)
				{
					MMF4Array?.Dispose();
					MMF4Array = null;
					MMF4ArrayLength?.Dispose();
					MMF4ArrayLength = null;
				}

				disposedValue = true;
			}
		}
	}
}
