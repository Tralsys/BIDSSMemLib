namespace TR.BIDSSMemLib
{
	/// <summary>固定長データ群のID一覧</summary>
	public enum FlexLOptD_ID : uint
	{
		None = 0,

		/// <summary>BIDSで規定されていないデータを書き込むときに使用するID群の先頭番号</summary>
		FreeArea_HEAD = 0x8000,
	}
}
