using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
#if !UMNGD
using System.IO.MemoryMappedFiles;
#endif
using System.Threading;

namespace TR
{
	/// <summary>TargetFramework別にSharedMemoryを提供します.</summary>
	public class SMemIF : IDisposable
	{
		//TargetFramework : https://docs.microsoft.com/en-us/dotnet/core/tutorials/libraries
#if UMNGD//Unmanaged
#region 関数のリンク
			const string DLL_NAME="kernel32.dll";
		[DllImport(DLL_NAME, CharSet = CharSet.Unicode)]
		static extern IntPtr CreateFileMappingA(
	IntPtr hFile,
	IntPtr lpFileMappingAttributes,
	uint flProtect,
	uint dwMaximumSizeHigh,
	uint dwMaximumSizeLow,
	string lpName
);
		[DllImport(DLL_NAME, CharSet = CharSet.Unicode)]
		static extern IntPtr OpenFileMappingA(
	uint dwDesiredAccess,
	byte bInheritHandle,
	string lpName
);
		[DllImport(DLL_NAME, CharSet = CharSet.Unicode)]
		static extern IntPtr MapViewOfFile(
	IntPtr hFileMappingObject,
	uint dwDesiredAccess,
	uint dwFileOffsetHigh,
	uint dwFileOffsetLow,
	uint dwNumberOfBytesToMap
);
		[DllImport(DLL_NAME, CharSet = CharSet.Unicode)]
		static extern byte CloseHandle(IntPtr hObject);

		[DllImport(DLL_NAME, CharSet = CharSet.Unicode)]
		static extern byte UnmapViewOfFile(IntPtr lpBaseAddress);

		[DllImport(DLL_NAME, CharSet = CharSet.Unicode)]
		static extern byte FlushViewOfFile(
	IntPtr lpBaseAddress,
	uint dwNumberOfBytesToFlush
);
#endregion

#region FileMappingまわりの定数
		const int ERROR_ALREADY_EXISTS = 183;
#region flProtect
		const uint PAGE_EXECUTE_READ = 0x20;
		const uint PAGE_EXECUTE_READWRITE = 0x40;
		const uint PAGE_EXECUTE_WRITECOPY = 0x80;
		const uint PAGE_READONLY = 0x02;
		const uint PAGE_READWRITE = 0x04;
		const uint PAGE_WRITECOPY = 0x08;

		const uint SEC_COMMIT = 0x8000000;
		const uint SEC_IMAGE = 0x1000000;
		const uint SEC_IMAGE_NO_EXECUTE = 0x11000000;
		const uint SEC_LARGE_PAGES = 0x80000000;
		const uint SEC_NOCACHE = 0x10000000;
		const uint SEC_RESERVE = 0x4000000;
		const uint SEC_WRITECOMBINE = 0x40000000;
#endregion

#region dwDesiredAccess
		const uint FILE_MAP_ALL_ACCESS = 0xf001f;
		const uint FILE_MAP_READ = 0x04;
		const uint FILE_MAP_WRITE = 0x02;
		const uint FILE_MAP_COPY = 0x01;
		//const uint FILE_MAP_EXECUTE = 0;
		//const uint FILE_MAP_LARGE_PAGES = 0;
		//const uint FILE_MAP_TARGETS_INVALID = 0;
#endregion

#endregion
#else
		MemoryMappedFile MMF = null;
		MemoryMappedViewAccessor MMVA = null;
#endif

		long Capacity { get
			{
#if UMNGD
				throw new NotImplementedException();
#else
				return MMVA?.Capacity ?? 0;
#endif
			}
		}

		string SMem_Name { get; }
		RWSemap semap = null;


		public SMemIF(string SMemName, long capacity)
		{
			if (string.IsNullOrEmpty(SMemName) || capacity <= 0) Dispose();
			SMem_Name = SMemName;
			semap = new RWSemap();
			
			CheckReOpen(capacity);
		}

		public bool Read<T>(long pos, out T buf) where T : struct
		{
			buf = default;
			if (pos < 0) throw new ArgumentOutOfRangeException("posに負の値は使用できません.");
			if (disposing) return false;

			CheckReOpen(pos + Marshal.SizeOf(default(T)));
			T retT = default;
			_ = semap.Read(() =>
			{
				#region SMemへの操作
#if UMNGD
				//throw new NotImplementedException();
#else
				MMVA.Read(pos, out retT);
#endif
				#endregion
			});
			buf = retT;
			return true;
		}
		public bool ReadArray<T>(long pos, T[] buf, int offset, int count) where T : struct
		{
			if (disposing) return false;

			long neededCap = pos + Marshal.SizeOf((T)default) * (count - offset);
			CheckReOpen(neededCap);
			_ = semap.Read(() =>
				{
				#region SMemへの操作
#if UMNGD
				//throw new NotImplementedException();
#else
				MMVA.ReadArray(pos, buf, offset, count);
#endif
				#endregion
			});
			
			return true;
		}
		public bool Write<T>(long pos, ref T buf) where T : struct
		{
			if (pos < 0) throw new ArgumentOutOfRangeException("posに負の値は使用できません.");
			if (disposing) return false;
			CheckReOpen(pos + Marshal.SizeOf(default(T)));
			T retT = buf;
			_ = semap.Write(() =>
				{
				#region SMemへの操作
#if UMNGD
				//throw new NotImplementedException();
#else
				MMVA.Write(pos, ref retT);
#endif
				#endregion
			});
			return true;
		}
		public bool WriteArray<T>(long pos, T[] buf, int offset, int count) where T : struct
		{
			if (disposing) return false;
			long neededCap = pos + Marshal.SizeOf((T)default) * (count - offset);
			CheckReOpen(neededCap);
			_ = semap.Write(() =>
				{
				#region SMemへの操作
#if UMNGD
				//throw new NotImplementedException();
#else
				MMVA.WriteArray(pos, buf, offset, count);
#endif
				#endregion
			});
			return true;
		}

		void CheckReOpen(long capacity)
		{
			if (Capacity > capacity) return;//保持キャパが要求キャパより大きい
			_ = semap.Write(() =>
				{
#if UMNGD
				//throw new NotImplementedException();
#else
				MMVA?.Dispose();
					MMF?.Dispose();

					MMF = MemoryMappedFile.CreateOrOpen(SMem_Name, capacity);
					MMVA = MMF.CreateViewAccessor();
#endif
			});
		}


#region IDisposable Support
		public bool disposing = false;
		private bool disposedValue = false;

		protected virtual void Dispose(bool disposing)
		{
			disposing = true;
			if (!disposedValue)
			{
				if (disposing)
				{
#if !(UMNGD)
					MMVA?.Dispose();
					MMF?.Dispose();

					MMVA = null;
					MMF = null;
#endif
				}
				semap.Dispose();
				semap = null;

				disposedValue = true;
			}
		}

		public void Dispose() => Dispose(true);
		
#endregion

	}

	public class RWSemap : IDisposable
	{
		private long WAIT_TICK = 1;
		private int Reading = 0;
		private int Want_to_Write = 0;
		private SemaphoreSlim Semap = new SemaphoreSlim(1);

		public void Dispose()
		{
			((IDisposable)Semap).Dispose();
		}

		public async Task<bool> Read(Action act)
		{
			while (Want_to_Write > 0) await Task.Delay(TimeSpan.FromTicks(WAIT_TICK));
			try
			{
				Interlocked.Increment(ref Reading);
				act?.Invoke();
			}
			finally
			{
				Interlocked.Decrement(ref Reading);
			}
			return true;
		}

		public async Task<bool> Write(Action act)
		{
			try
			{
				Interlocked.Increment(ref Want_to_Write);
				while (Reading > 0) await Task.Delay(TimeSpan.FromTicks(WAIT_TICK));
				try
				{
					await Semap.WaitAsync();
					act?.Invoke();
				}
				finally
				{
					Semap.Release();
				}
			}
			finally
			{
				Interlocked.Decrement(ref Want_to_Write);
			}
			return true;
		}
	}
}

#if UMNGD
namespace System.Threading.Tasks
{
	public class Task
	{
		static public void Delay(TimeSpan ts) => Thread.Sleep(ts);
	}
}
#endif
