using System;
using System.Runtime.InteropServices;
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

		const int READ_TIMEOUT = 1;
		const int WRITE_TIMEOUT = 10;
		string SMem_Name { get; }
		ReaderWriterLockSlim RWLS = null;//プロセス間の排他は無視する.

		public SMemIF(string SMemName, long capacity)
		{
			if (string.IsNullOrEmpty(SMemName) || capacity <= 0) Dispose();
			SMem_Name = SMemName;
			RWLS = new ReaderWriterLockSlim();

			CheckReOpen(capacity);
		}

		public bool Read<T>(long pos, out T buf) where T : struct
		{
			buf = new T();
			bool LockGot = false;
			if (pos < 0) throw new ArgumentOutOfRangeException("posに負の値は使用できません.");
			if (disposing) return false;
			if (RWLS.IsReadLockHeld) return false;//ロックがかかってたら, Readを行わない.(bufferから読んでね.)
			try
			{
				CheckReOpen(pos + Marshal.SizeOf((T)default));
				if (!RWLS.TryEnterReadLock(READ_TIMEOUT)) return false;
				LockGot = true;
#region SMemへの操作
#if UMNGD
				//throw new NotImplementedException();
#else
				MMVA.Read(pos, out buf);
#endif
#endregion
			}
			catch (Exception e)
			{
#if DEBUG
				Console.WriteLine("SMemIF.Read<T>({0},) => {1}", pos, e);
#endif
				return false;
			}
			finally
			{
				if (LockGot && RWLS.IsReadLockHeld)
					RWLS.ExitReadLock();
			}
			return true;
		}
		public bool ReadArray<T>(long pos, T[] buf, int offset, int count) where T : struct
		{
			bool LockGot = false;
			if (disposing) return false;
			if (RWLS.IsReadLockHeld) return false;
			try
			{
				long neededCap = pos + Marshal.SizeOf((T)default) * (count - offset);
				CheckReOpen(neededCap);
				if (!RWLS.TryEnterReadLock(READ_TIMEOUT)) return false;
				LockGot = true;
#region SMemへの操作
#if UMNGD
				//throw new NotImplementedException();
#else
				MMVA.ReadArray(pos, buf, offset, count);
#endif
#endregion
			}
			catch (Exception e)
			{
#if DEBUG
				Console.WriteLine("SMemIF.ReadArray<T>({0},,{1},{2}) => {3}", pos, offset, count, e);
#endif
				return false;
			}
			finally
			{
				if (LockGot && RWLS.IsReadLockHeld)
					RWLS.ExitReadLock();
			}
			return true;
		}
		public bool Write<T>(long pos, ref T buf) where T : struct
		{
			bool LockGot = false;
			if (pos < 0) throw new ArgumentOutOfRangeException("posに負の値は使用できません.");
			if (disposing) return false;
			try
			{
				CheckReOpen(pos + Marshal.SizeOf((T)default));
				if (!RWLS.TryEnterWriteLock(WRITE_TIMEOUT)) return false;
				LockGot = true;
#region SMemへの操作
#if UMNGD
				//throw new NotImplementedException();
#else
				MMVA.Write(pos, ref buf);
#endif
#endregion
			}
			catch (Exception e)
			{
#if DEBUG
				Console.WriteLine("SMemIF.Write<T>({0},) => {1}", pos, e);
#endif
				return false;
			}
			finally
			{
				if (LockGot && RWLS.IsWriteLockHeld)
					RWLS.ExitWriteLock();
			}
			return true;
		}
		public bool WriteArray<T>(long pos, T[] buf, int offset, int count) where T : struct
		{
			bool LockGot = false;
			if (disposing) return false;
			try
			{
				long neededCap = pos + Marshal.SizeOf((T)default) * (count - offset);
				CheckReOpen(neededCap);
				if (!RWLS.TryEnterWriteLock(WRITE_TIMEOUT)) return false;
				LockGot = true;
#region SMemへの操作
#if UMNGD
				//throw new NotImplementedException();
#else
				MMVA.WriteArray(pos, buf, offset, count);
#endif
#endregion
			}
			catch (Exception e)
			{
#if DEBUG
				Console.WriteLine("SMemIF.WriteArray<T>({0},,{1},{2}) => {3}", pos, offset, count, e);
#endif
				return false;
			}
			finally
			{
				if (LockGot && RWLS.IsWriteLockHeld)
					RWLS.ExitWriteLock();
			}
			return true;
		}

		void CheckReOpen(long capacity)
		{
			bool LockGot = false;
			try
			{
				if (Capacity > capacity) return;//保持キャパが要求キャパより大きい
				if (!RWLS.TryEnterWriteLock(WRITE_TIMEOUT)) return;//ロック取得失敗
				LockGot = true;
#if UMNGD
				//throw new NotImplementedException();
#else
				MMVA?.Dispose();
				MMF?.Dispose();

				MMF = MemoryMappedFile.CreateOrOpen(SMem_Name, capacity);
				MMVA = MMF.CreateViewAccessor();
#endif
			}
			finally
			{
				if (LockGot && RWLS.IsWriteLockHeld)
					RWLS.ExitWriteLock();
			}
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
				try
				{
					RWLS?.Dispose();
				}catch(Exception e)
				{
					Console.WriteLine(e);
				}
				RWLS = null;

				disposedValue = true;
			}
		}

		public void Dispose() => Dispose(true);
		
#endregion

	}
}
