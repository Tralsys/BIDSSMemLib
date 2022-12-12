using System.Runtime.CompilerServices;
using System.Threading;

namespace TR.BIDSSMemLib
{
	public partial class SMemLib
	{
		/// <summary>BIDSSharedMemoryData構造体の情報を共有メモリに書き込む</summary>
		/// <param name="D">書き込む構造体</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public void Write(in BIDSSharedMemoryData D) => SMC_BSMD?.Write(D);
		/// <summary>OpenD構造体の情報を共有メモリに書き込む</summary>
		/// <param name="D">書き込む構造体</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public void Write(in OpenD D) => SMC_OpenD?.Write(D);
		/// <summary>StaD構造体の情報を共有メモリに書き込む</summary>
		/// <param name="D">書き込む構造体</param>
		//public void Write(in StaD D) => Write(D, 4);
		/// <summary>Panel構造体の情報を共有メモリに書き込む</summary>
		/// <param name="D">書き込む構造体</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public void Write(in PanelD D) => SMC_PnlD?.Write(D.Panels);
		/// <summary>Panel構造体の情報を共有メモリに書き込む</summary>
		/// <param name="D">書き込む配列</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public void WritePanel(in int[] D) => SMC_PnlD?.Write(D);
		/// <summary>Sound構造体の情報を共有メモリに書き込む</summary>
		/// <param name="D">書き込む構造体</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public void Write(in SoundD D) => SMC_SndD?.Write(D.Sounds);
		/// <summary>Sound構造体の情報を共有メモリに書き込む</summary>
		/// <param name="D">書き込む配列</param>
		[MethodImpl(MIOpt)]//関数のインライン展開を積極的にやってもらう.
		public void WriteSound(in int[] D) => SMC_SndD?.Write(D);

		readonly object BSMDLockObj = new();

		public void Write(in State v)
		{
			lock (BSMDLockObj)
			{
				SMC_BSMD.Write(BIDSSMemData with
				{
					StateData = v
				});
			}
		}

		public void Write(in Spec v)
		{
			lock (BSMDLockObj)
			{
				SMC_BSMD.Write(BIDSSMemData with
				{
					SpecData = v
				});
			}
		}

		public void Write(in Hand v)
		{
			lock (BSMDLockObj)
			{
				SMC_BSMD.Write(BIDSSMemData with
				{
					HandleData = v
				});
			}
		}

		public void WriteIsDoorClosed(bool isDoorClosed)
		{
			lock (BSMDLockObj)
			{
				SMC_BSMD.Write(BIDSSMemData with
				{
					IsDoorClosed = isDoorClosed
				});
			}
		}

		public void WriteVersion(int version)
		{
			lock (BSMDLockObj)
			{
				SMC_BSMD.Write(BIDSSMemData with
				{
					VersionNum = version
				});
			}
		}

		public void WriteIsEnabled(bool isEnabled)
		{
			lock (BSMDLockObj)
			{
				SMC_BSMD.Write(BIDSSMemData with
				{
					IsEnabled = isEnabled
				});
			}
		}
	}
}
