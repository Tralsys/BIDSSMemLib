using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TR
{
	/// <summary>単一要素を共有メモリを用いて共有します</summary>
	/// <typeparam name="T">共有するデータの型</typeparam>
	public class SMemCtrler<T> : SMemCtrlerBase<T>, ISMemCtrler<T>, IReadWriteInObject where T : struct
	{
		/// <summary>共有する要素のサイズ</summary>
		public override uint Elem_Size { get; } = (uint)Marshal.SizeOf(default(T));

		/// <summary>インスタンスを初期化します</summary>
		/// <param name="name">共有メモリの名前</param>
		/// <param name="no_smem">共有メモリを使用せずに動作させるか</param>
		/// <param name="no_event">イベントを発火させずに動作させるか</param>
		public SMemCtrler(in string name, in bool no_smem, in bool no_event) : base(name, no_smem, no_event, Marshal.SizeOf(default(T)))
		{
		}

		#region ISMemCtrler
		/// <summary>要素を読み取る</summary>
		/// <returns>読み取ったデータ</returns>
		public override T Read()
		{
			if (MMF is not null && !No_SMem_Mode && MMF.Read(0, out T value))
			{
				//共有メモリから読み込むモードであり, かつ読み込みに成功した場合のみ値の更新チェックを行う
				//CheckAndNotifyPropertyChangedメソッド内でValueの更新は行われる
				CheckAndNotifyPropertyChanged(value);
			}

			return Value;
		}

		/// <summary>データの読み取りを試行する</summary>
		/// <param name="value">読み取り結果の記録先</param>
		/// <returns>試行結果</returns>
		public override bool TryRead(out T value)
		{
			//MMFがnull => SMemに読み書き不可
			//No_SMem_Mode => SMemから読み込まなくてOK
			if (MMF is null || No_SMem_Mode)
			{
				value = Value;
				return true;
			}

			try
			{
				bool result = MMF.Read(0, out value);

				if (result)
					CheckAndNotifyPropertyChanged(value); //Read成功時のみ書き込み

				return result;
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				value = Value;
				return false;
			}
		}

		/// <summary>データの書き込みを試行する</summary>
		/// <param name="value">書き込むデータ</param>
		/// <returns>試行結果</returns>
		public override bool TryWrite(in T value)
		{
			try
			{
				Write(value);
				return true;
			}
			catch (Exception ex) //MMFにTry系のIFを用意してないので, 無駄みが強いけどこの実装で
			{
				Debug.WriteLine(ex);
				return false;
			}
		}

		/// <summary>データを書き込む</summary>
		/// <param name="value">書き込むデータ</param>
		public override void Write(in T value)
		{
			_Value = value;

			if (!No_SMem_Mode)
				MMF?.Write(0, ref _Value);
		}
		#endregion

		#region IReadWriteInObject
		/// <summary>データを読み込む</summary>
		/// <returns>読み込んだデータ</returns>
		public object ReadInObject() => Read();

		/// <summary>データの読み取りを試行する</summary>
		/// <param name="obj">読み取ったデータの記録先</param>
		/// <returns>試行結果</returns>
		public bool TryReadInObject(out object obj)
		{
			bool result = TryRead(out T value);
			obj = value;
			return result;
		}

		/// <summary>データを書き込む</summary>
		/// <param name="obj">書き込むデータ</param>
		public void WriteInObject(in object obj) => Write((T)obj);

		/// <summary>データの書き込みを試行する</summary>
		/// <param name="obj">書き込むデータ</param>
		/// <returns>試行結果</returns>
		public bool TryWriteInObject(in object obj) => TryWrite((T)obj);
		#endregion

		/// <inheritdoc/>
		protected override bool IsValueSame(T v1, T v2)
			=> Equals(v1, v2);
	}

}
