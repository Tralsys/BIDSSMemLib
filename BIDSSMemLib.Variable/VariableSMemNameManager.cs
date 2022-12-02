using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TR.BIDSSMemLib;

/// <summary>
/// 共有メモリ名を管理する共有メモリを操作するクラス
/// </summary>
public class VariableSMemNameManager : IDisposable, IEnumerable<VariableSMemNameManager.SMemName>
{
	/// <summary>
	/// 共有メモリ名に関する情報を記録するレコード型
	/// </summary>
	/// <param name="Position">名前共有用共有メモリにてこのデータが記録されている位置</param>
	/// <param name="Capacity">名前共有用共有メモリにてこのデータのために確保している領域のサイズ [Byte]</param>
	/// <param name="Name">共有メモリ名</param>
	public record SMemName(
		long Position,
		ushort Capacity,
		string Name
	);

	/// <summary>
	/// <see cref="VariableSMemNameManager"/>で使用するエンコーディング
	/// </summary>
	public static readonly Encoding DefaultEncoding = Encoding.UTF8;

	/// <summary>
	/// デフォルトのSMemName共有用SMem名
	/// </summary>
	public static readonly string DefaultManagerAreaSMemName = "BIDS.VariableSMemNameManager";

	/// <summary>
	/// デフォルトのSMemName共有用メモリキャパシティ (1,048,576 Bytes)
	/// </summary>
	public static readonly long DefaultCapacity_Bytes = 0x00100000;

	/// <summary>
	/// 共有メモリ名を共有する共有メモリの名前
	/// </summary>
	public string ManagerAreaSMemName { get; }

	/// <summary>
	/// SMemName共有用メモリキャパシティ
	/// </summary>
	/// <remarks>
	/// 他のインスタンスによって既に共有メモリが作成済みだった場合、そちらで設定されたキャパシティが利用されます
	/// </remarks>
	public long Capacity_Bytes { get; }

	/// <summary>
	/// 共有メモリを操作できるインターフェイスクラス
	/// </summary>
	ISMemIF SMemIF { get; }

	#region Constructors
	/// <summary>
	/// デフォルト値を用いてインスタンスを初期化する
	/// </summary>
	public VariableSMemNameManager() : this(null, 0)
	{
	}

	/// <summary>
	/// 指定の共有メモリ名にてインスタンスを初期化する
	/// </summary>
	/// <param name="managerAreaSMemName">使用する共有メモリ名</param>
	public VariableSMemNameManager(string? managerAreaSMemName) : this(managerAreaSMemName, 0)
	{
	}

	/// <summary>
	/// 指定の共有メモリ名とキャパシティにてインスタンスを初期化する
	/// </summary>
	/// <param name="managerAreaSMemName">使用する共有メモリ名</param>
	/// <param name="capacity_Bytes">共有メモリのキャパシティ</param>
	public VariableSMemNameManager(string? managerAreaSMemName, long capacity_Bytes) : this(
			new SMemIF(
				string.IsNullOrWhiteSpace(managerAreaSMemName)
					? DefaultManagerAreaSMemName
					: managerAreaSMemName,
				capacity_Bytes <= 0
					? DefaultCapacity_Bytes
					: capacity_Bytes
			)
		)
	{
	}

	/// <summary>
	/// 指定の共有メモリ操作用インスタンスを用いてインスタンスを初期化する
	/// </summary>
	/// <param name="_SMemIF">共有メモリ操作用インスタンス</param>
	public VariableSMemNameManager(ISMemIF _SMemIF)
	{
		ManagerAreaSMemName = _SMemIF.SMemName;
		Capacity_Bytes = _SMemIF.Capacity;

		SMemIF = _SMemIF;
	}
	#endregion

	/// <summary>
	/// 共有メモリの名前情報を追加する
	/// </summary>
	/// <param name="name">追加する共有メモリ名情報</param>
	/// <returns>記録したPosition</returns>
	/// <exception cref="ArgumentException"><c>name</c>に空白等の無効な値が指定された</exception>
	/// <exception cref="AccessViolationException">共有メモリへのアクセスに失敗した</exception>
	public SMemName? AddName(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
			throw new ArgumentException("must not be null, empty, or only whitespace", nameof(name));

		byte[] nameBytes = DefaultEncoding.GetBytes(name);

		if (ushort.MaxValue < nameBytes.Length)
			throw new ArgumentOutOfRangeException(nameof(name), "must be less than 65535 Bytes in UTF-8");

		long pos = 0;
		ushort capacityBuf = 0;

		while (true)
		{
			if (!SMemIF.Read(pos, out capacityBuf))
				throw new AccessViolationException("Read from SMem Failed");

			// 未記入な領域な場合
			if (capacityBuf == 0)
				break;

			// キャパシティ的に再利用できるかチェック
			if (capacityBuf < nameBytes.Length)
			{
				pos += sizeof(ushort) + capacityBuf;
				continue;
			}

			// 最初の文字を取得
			if (!SMemIF.Read(pos + sizeof(ushort), out byte deletedCheck))
				throw new AccessViolationException("Read from SMem Failed");

			// 最初の文字がNULL文字じゃないのなら、何かしら書き込まれているはず。
			if (deletedCheck == 0)
			{
				// Reservedにするため、(NULL文字以外の)適当な数値を入れておく
				deletedCheck = 1;
				if (!SMemIF.Write(pos + sizeof(ushort), ref deletedCheck))
					throw new AccessViolationException("Write to SMem Failed");

				break;
			}

			// 同じ名前かどうかチェック
			byte[] nameInSMem = new byte[capacityBuf];
			if (!SMemIF.ReadArray(pos + sizeof(ushort), nameInSMem, 0, nameInSMem.Length))
				throw new AccessViolationException("Read from SMem Failed");

			bool targetNameWasFoundInSMem = true;
			// Capacityチェック済みのため、nameBytesよりもnameInSMemの方が長さが同じか長いのは確実
			for (int i = 0; i < nameInSMem.Length; i++)
			{
				// nameBytesとnameInSMemが現時点までで全て一致している、かつnameBytesを全て確認済みである、かつnameInSMemがNULL文字 => 文字列が完全に一致
				// 違うバイトデータが出てきたら、文字列が一致しないのは当たり前。
				if ((i == nameBytes.Length && nameInSMem[i] != 0)
					|| (nameBytes[i] != nameInSMem[i]))
				{
					targetNameWasFoundInSMem = false;
					break;
				}
			}

			if (targetNameWasFoundInSMem)
			{
				return new(
					pos,
					capacityBuf,
					name
				);
			}
			else
			{
				pos += sizeof(ushort) + capacityBuf;
			}
		}

		if (capacityBuf == 0)
		{
			capacityBuf = (ushort)nameBytes.Length;

			if (!SMemIF.Write(pos, ref capacityBuf))
				throw new AccessViolationException("Write to SMem Failed");
		}

		if (!SMemIF.WriteArray(pos + sizeof(ushort), nameBytes, 0, nameBytes.Length))
			throw new AccessViolationException("Write to SMem Failed");

		return new(
			pos,
			capacityBuf,
			name
		);
	}

	/// <summary>
	/// 指定の位置に記録された共有メモリ名情報を読み取る
	/// </summary>
	/// <param name="position">位置情報</param>
	/// <returns>共有メモリ名情報</returns>
	/// <exception cref="AccessViolationException">共有メモリへのアクセスに失敗した</exception>
	public SMemName? ReadName(long position)
	{
		ushort capacity = 0;

		if (!SMemIF.Read(position, out capacity))
			return null;

		return ReadName(position, capacity);
	}

	/// <summary>
	/// 指定の位置に記録された共有メモリ名情報を、指定のキャパシティ情報を用いて読み取る
	/// </summary>
	/// <param name="position">位置情報</param>
	/// <param name="capacity">キャパシティ</param>
	/// <returns>共有メモリ名情報</returns>
	/// <exception cref="AccessViolationException">共有メモリへのアクセスに失敗した</exception>
	public SMemName? ReadName(long position, ushort capacity)
	{
		if (capacity <= 0)
			return null;

		byte[] name = new byte[capacity];

		if (!SMemIF.ReadArray(position + sizeof(ushort), name, 0, name.Length))
			throw new AccessViolationException("Read from SMem Failed");

		return new SMemName(
			Position: position,
			Capacity: capacity,
			Name: name[0] == 0 ? string.Empty : DefaultEncoding.GetString(name)
		);
	}

	/// <summary>
	/// 指定の共有メモリ情報を、名前共有用共有メモリから消去する
	/// </summary>
	/// <param name="name">共有メモリ情報</param>
	/// <returns>消去に成功したかどうか</returns>
	public bool DeleteName(SMemName name)
	{
		byte[] zeroFill = new byte[name.Capacity];

		return SMemIF.WriteArray(name.Position + sizeof(ushort), zeroFill, 0, zeroFill.Length);
	}

	/// <summary>
	/// <see cref="IEnumerator{SMemName}"/>インスタンスを取得する
	/// </summary>
	/// <returns><see cref="IEnumerator{SMemName}"/>インスタンス</returns>
	public IEnumerator<SMemName> GetEnumerator()
	{
		long position = 0;

		while (ReadName(position) is SMemName name)
		{
			if (!string.IsNullOrEmpty(name.Name))
				yield return name;

			position += (sizeof(ushort) + name.Capacity);
		}
	}

	/// <summary>
	/// <see cref="IEnumerator{SMemName}"/>インスタンスを取得する
	/// </summary>
	/// <returns><see cref="IEnumerator{SMemName}"/>インスタンス</returns>
	IEnumerator IEnumerable.GetEnumerator()
		=> this.GetEnumerator();

	#region IDisposable
	private bool disposedValue;

	/// <summary>
	/// リソースを解放する
	/// </summary>
	/// <param name="disposing"></param>
	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				SMemIF.Dispose();
			}

			disposedValue = true;
		}
	}

	/// <summary>
	/// リソースを解放する
	/// </summary>
	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
	#endregion
}
