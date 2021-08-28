using System;
using System.Diagnostics;

namespace TR
{
	public class SMemCtrler<T> : SMemCtrlerBase<T>, ISMemCtrler<T> where T : struct
	{
		public SMemCtrler(in string name, in bool no_smem, in bool no_event) : base(name,no_smem,no_event)
		{

		}

		protected override void Initialize_MMF() => MMF = new SMemIF(SMem_Name, Elem_Size);
		

		public override T Read()
		{
			if (MMF is not null && MMF.Read(0, out T value))
				CheckAndNotifyPropertyChanged(value);

			return Value;
		}

		public override bool TryRead(out T value)
		{
			if(MMF is null)
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
			MMF?.Write(0, ref _Value);
		}
	}

}
