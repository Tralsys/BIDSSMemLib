using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using BIDS.Parser.Variable;

namespace TR.BIDSSMemLib;

/// <summary>
/// 可変構造(可変長)な共有メモリを使用するためのクラス
/// </summary>
public partial class VariableSMem : IDisposable
{
	/// <summary>
	/// デフォルトで使用されるエンコーディング
	/// </summary>
	public static Encoding DefaultEncoding { get; } = Encoding.UTF8;

	/// <summary>
	/// 共有メモリの操作に使用するインターフェイスを持つインスタンス
	/// </summary>
	protected ISMemIF SMemIF { get; }

	/// <summary>
	/// 可変構造データの構造情報
	/// </summary>
	public VariableStructure Structure { get; }

	/// <summary>
	/// 構造情報が記録されているメモリ領域のオフセット [bytes]
	/// </summary>
	public static long StructureAreaOffset { get; } = 16;

	/// <summary>
	/// コンテンツデータが記録されているメモリ領域のオフセット [bytes]
	/// </summary>
	public long ContentAreaOffset { get; }

	/// <summary>
	/// 構造情報とコンテンツデータの間のパディング [bytes]
	/// </summary>
	public static long PaddingBetweenStructreAndContent { get; } = 16;

	/// <summary>
	/// 共有メモリに書き込まれているデータを管理するためのリスト
	/// </summary>
	/// <remarks>
	/// メモリのコンテンツ領域の先頭から、このリストの順に書き込まれている
	/// </remarks>
	protected readonly List<VariableStructure.IDataRecord> _Members;

	/// <summary>
	/// 共有メモリに書き込まれているデータを確認できるリスト
	/// </summary>
	public IReadOnlyList<VariableStructure.IDataRecord> Members => _Members;

	/// <summary>
	/// インスタンスを初期化する
	/// </summary>
	/// <param name="Name">共有メモリ名</param>
	/// <param name="Capacity">共有メモリの初期化に使用するキャパシティ [bytes]</param>
	/// <param name="structure">データの構造情報</param>
	public VariableSMem(string Name, long Capacity, VariableStructure structure) : this(
		new SMemIF(Name, Capacity),
		structure.Records,
		structure
	)
	{
	}

	/// <summary>
	/// インスタンスを初期化する
	/// </summary>
	/// <param name="type">構造情報を取得する型</param>
	/// <param name="Name">共有メモリ名</param>
	/// <param name="Capacity">共有メモリの初期化に使用するキャパシティ [bytes]</param>
	public VariableSMem(Type type, string Name, long Capacity) : this(
		new SMemIF(Name, Capacity),
		type.ToVariableDataRecordList(),
		null
	)
	{
	}

	/// <summary>
	/// インスタンスを初期化する
	/// </summary>
	/// <param name="smemIF">共有メモリの操作に使用するインスタンス</param>
	/// <param name="structure">データの構造情報</param>
	public VariableSMem(ISMemIF smemIF, VariableStructure structure) : this(
		smemIF,
		structure.Records,
		structure
	)
	{
	}

	/// <summary>
	/// インスタンスを初期化する
	/// </summary>
	/// <param name="type">構造情報を取得する型</param>
	/// <param name="smemIF">共有メモリの操作に使用するインスタンス</param>
	public VariableSMem(Type type, ISMemIF smemIF) : this(
		smemIF,
		type.ToVariableDataRecordList(),
		null
	)
	{
	}

	/// <summary>
	/// インスタンスを初期化する
	/// </summary>
	/// <param name="smemIF">共有メモリの操作に使用するインスタンス</param>
	/// <param name="members">データの各要素が順に記録されたリスト</param>
	/// <param name="structure">データの構造情報</param>
	protected VariableSMem(
		ISMemIF smemIF,
		IReadOnlyList<VariableStructure.IDataRecord> members,
		VariableStructure? structure
	)
	{
		SMemIF = smemIF;

		_Members = members.ToList();
		if (structure is null)
		{
			// Structure Nameは、共有メモリにおいてはSMemNameで代替できるため、記録しない
			Structure = new(-1, string.Empty, _Members);
		}
		else
		{
			Structure = structure with
			{
				Records = _Members
			};
		}

		ContentAreaOffset = InitSMem();
	}

	/// <summary>
	/// 共有メモリを初期化する
	/// </summary>
	/// <returns>ContentAreaOffset</returns>
	long InitSMem()
	{
		byte[] structureBytes = Structure.GetStructureBytes().ToArray();

		long contentAreaOffset
			= StructureAreaOffset
			+ structureBytes.Length
			+ PaddingBetweenStructreAndContent;
		if (SMemIF.IsNewlyCreated)
		{
			if (!SMemIF.Write(0, ref contentAreaOffset)
				|| !SMemIF.WriteArray(StructureAreaOffset, structureBytes, 0, structureBytes.Length))
				throw new AccessViolationException("Write to SMem failed");
		}
		else
		{
			if (!SMemIF.Read(0, out contentAreaOffset))
				throw new AccessViolationException("Read from SMem failed");

			if (contentAreaOffset < (StructureAreaOffset + structureBytes.Length))
				throw new FormatException("Invalid `ContentAreaOffset` (too few memory to save this data)");

			byte[] structureBytesInSMem = new byte[structureBytes.Length];
			if (!SMemIF.ReadArray(StructureAreaOffset, structureBytesInSMem, 0, structureBytesInSMem.Length))
				throw new AccessViolationException("Read from SMem failed");

			if (!structureBytes.SequenceEqual(structureBytesInSMem))
				throw new FormatException("Invalid Structure (Given structure and structure in SMem are not same)");
		}

		return contentAreaOffset;
	}

	/// <summary>
	/// 共有メモリからデータを読み取る
	/// </summary>
	/// <returns>読み取ったデータ</returns>
	/// <exception cref="AccessViolationException">共有メモリの操作に失敗した</exception>
	public VariableStructurePayload ReadFromSMem()
	{
		if (!SMemIF.Read(ContentAreaOffset, out long contentDataLength))
			throw new AccessViolationException("Read from SMem failed");

		byte[] content = new byte[contentDataLength];
		if (!SMemIF.ReadArray(ContentAreaOffset + sizeof(long), content, 0, content.Length))
			throw new AccessViolationException("Read from SMem failed");

		return Structure.With(content);
	}

	/// <summary>
	/// 共有メモリからデータを読み取り、指定のインスタンスに値を書き込む
	/// </summary>
	/// <param name="target">値を書き込む先のインスタンス</param>
	/// <exception cref="AccessViolationException">共有メモリの操作に失敗した</exception>
	public void ReadFromSMem(ref object target)
	{
		VariableStructurePayload payload = ReadFromSMem();
		Type targetType = target.GetType();

		foreach (var data in payload.Values)
		{
			if (targetType.GetProperty(data.Name) is PropertyInfo propInfo)
			{
				propInfo.SetValue(target, GetValueObjectFromDataRecord(data, propInfo.PropertyType == typeof(string)));
			}
			else if (targetType.GetField(data.Name) is FieldInfo fieldInfo)
			{
				fieldInfo.SetValue(target, GetValueObjectFromDataRecord(data, fieldInfo.FieldType == typeof(string)));
			}
		}
	}

	/// <summary>
	/// 共有メモリにデータを書き込む
	/// </summary>
	/// <param name="data">書き込むデータ</param>
	/// <exception cref="AccessViolationException">共有メモリの操作に失敗した</exception>
	public virtual void WriteToSMem(in object data)
	{
		Type type = data.GetType();

		for (int i = 0; i < _Members.Count; i++)
		{
			VariableStructure.IDataRecord iDataRecord = _Members[i];

			string propName = iDataRecord.Name;
			if (type.GetProperty(propName) is PropertyInfo propInfo)
			{
				_Members[i] = CreateNewRecordWithValue(iDataRecord, propInfo.PropertyType, propInfo.GetValue(data));
			}
			else if (type.GetField(propName) is FieldInfo fieldInfo)
			{
				_Members[i] = CreateNewRecordWithValue(iDataRecord, fieldInfo.FieldType, fieldInfo.GetValue(data));
			}
			else
				continue;
		}

		// DataType IDはStructure側で既に書き込んであるため、Content側には含めない
		byte[] bytes = Structure.GetBytes().Skip(sizeof(int)).ToArray();
		long contentLength = bytes.LongLength;
		if (!SMemIF.Write(ContentAreaOffset, ref contentLength)
			|| !SMemIF.WriteArray(ContentAreaOffset + sizeof(long), bytes, 0, bytes.Length))
			throw new AccessViolationException("Write to SMem failed");
	}

	public void WriteToSMemFromPayload(in VariableStructurePayload payload)
	{
		for (int i = 0; i < _Members.Count; i++)
		{
			VariableStructure.IDataRecord iDataRecord = _Members[i];
			if (!payload.TryGetValue(iDataRecord.Name, out VariableStructure.IDataRecord? gotData))
				continue;

			if (iDataRecord.Type != gotData.Type)
				continue;
			if (gotData is VariableStructure.ArrayStructure v1
				&& iDataRecord is VariableStructure.ArrayStructure v2
				&& v1.ElemType != v2.ElemType)
				continue;

			_Members[i] = gotData;
		}


		// DataType IDはStructure側で既に書き込んであるため、Content側には含めない
		byte[] bytes = Structure.GetBytes().Skip(sizeof(int)).ToArray();
		long contentLength = bytes.LongLength;
		if (!SMemIF.Write(ContentAreaOffset, ref contentLength)
			|| !SMemIF.WriteArray(ContentAreaOffset + sizeof(long), bytes, 0, bytes.Length))
			throw new AccessViolationException("Write to SMem failed");
	}

	#region IDisposable
	private bool disposedValue;
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

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}
	#endregion
}
