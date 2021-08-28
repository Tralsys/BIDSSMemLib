namespace TR.BIDSSMemLib
{
	/// <summary>固定長データ群のID一覧</summary>
	public enum FixedLOptD_ID : uint
	{
		None = 0,

		/// <summary>[ushort]書き込み側のバージョン設定</summary>
		WriterVersion,

		/// <summary>[ushort]シミュレータのProcess ID</summary>
		Sim_PID,

		/// <summary>[bool]戸閉車側灯の点灯状態(0:Off, 1:On)</summary>
		DSIL_AnyCar_AnySide,

		/// <summary>[Update]車掌SWが"開"方向に扱われた記録</summary>
		CDS_UnknownSide_ToOpen,

		/// <summary>[Update]車掌SWが"閉"方向に扱われた記録</summary>
		CDS_UnknownSide_ToClose,

		/// <summary>BIDSで規定されていないデータを書き込むときに使用するID群の先頭番号</summary>
		FreeArea_HEAD = 0x8000,
	}
}
