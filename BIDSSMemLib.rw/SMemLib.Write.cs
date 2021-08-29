using System.Runtime.CompilerServices;

namespace TR.BIDSSMemLib
{
	static public partial class SMemLib
	{
		/// <summary>BIDSSharedMemoryData構造体の情報を共有メモリに書き込む</summary>
		/// <param name="D">書き込む構造体</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static void Write(in BIDSSharedMemoryData D) => SMC_BSMD?.Write(D);
		/// <summary>OpenD構造体の情報を共有メモリに書き込む</summary>
		/// <param name="D">書き込む構造体</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static void Write(in OpenD D) => SMC_OpenD?.Write(D);
		/// <summary>StaD構造体の情報を共有メモリに書き込む</summary>
		/// <param name="D">書き込む構造体</param>
		//public void Write(in StaD D) => Write(D, 4);
		/// <summary>Panel構造体の情報を共有メモリに書き込む</summary>
		/// <param name="D">書き込む構造体</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static void Write(in PanelD D) => SMC_PnlD?.Write(D.Panels);
		/// <summary>Panel構造体の情報を共有メモリに書き込む</summary>
		/// <param name="D">書き込む配列</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static void WritePanel(in int[] D) => SMC_PnlD?.Write(D);
		/// <summary>Sound構造体の情報を共有メモリに書き込む</summary>
		/// <param name="D">書き込む構造体</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static void Write(in SoundD D) => SMC_SndD?.Write(D.Sounds);
		/// <summary>Sound構造体の情報を共有メモリに書き込む</summary>
		/// <param name="D">書き込む配列</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public static void WriteSound(in int[] D) => SMC_SndD?.Write(D);
	}
}
