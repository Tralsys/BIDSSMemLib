using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace TR
{
	public class SMemCtrler<T> : SMemCtrlerBase<T>, ISMemCtrler<T>, IReadWriteInObject where T : struct
	{
		public override uint Elem_Size { get; } = (uint)Marshal.SizeOf(default(T));

		public SMemCtrler(in string name, in bool no_smem, in bool no_event) : base(name, no_smem, no_event, Marshal.SizeOf(default(T)))
		{
		}

		#region ISMemCtrler
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

		public override bool TryRead(out T value)
		{
			if(MMF is null || !No_SMem_Mode)
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

		public override bool TryWrite(in T value)
		{
			try
			{
				Write(value);
				return true;
			}catch(Exception ex) //MMFにTry系のIFを用意してないので, 無駄みが強いけどこの実装で
			{
				Debug.WriteLine(ex);
				return false;
			}
		}

		public override void Write(in T value)
		{
			_Value = value;

			if (!No_SMem_Mode)
				MMF?.Write(0, ref _Value);
		}
		#endregion

		#region IReadWriteInObject
		public object ReadInObject() => Read();

		public bool TryReadInObject(out object obj)
		{
			bool result = TryRead(out T value);
			obj = value;
			return result;
		}

		public void WriteInObject(in object obj) => Write((T)obj);

		public bool TryWriteInObject(in object obj) => TryWrite((T)obj);
		#endregion
	}

}
