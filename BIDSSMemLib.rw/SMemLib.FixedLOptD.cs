using System;

namespace TR.BIDSSMemLib
{
	static public partial class SMemLib
	{
		//取り扱いが特殊なため, 専用ファイルに分離

		public static void AddFixedLOptD(in FixedLOptD_ID id, in uint data)
		{

		}

		public static uint GetFixedLOptD(in FixedLOptD_ID id)
		{
			if (TryGetFixedLOptD(id, out uint dst))
				return dst;
			else
				throw new InvalidOperationException($"ID Not Found : {id}");
		}

		public static bool TryGetFixedLOptD(in FixedLOptD_ID id, out uint dst)
		{
			dst = 0;
			return true;
		}
	}
}
