using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using BIDS.Parser.Variable;

using TR.BIDSSMemLib.Variable;
using TR.BIDSSMemLib.Variable.Exceptions;

namespace TR.BIDSSMemLib;

/// <summary>
/// 可変構造(可変長)な共有メモリを使用するためのクラス
/// </summary>
public class VariableSMem
{
	public static Encoding DefaultEncoding { get; } = Encoding.UTF8;

	ISMemIF SMemIF { get; }

	public VariableStructure Structure { get; }

	public static long StructureAreaOffset { get; } = 16;
	public long ContentAreaOffset { get; }
	public static long PaddingBetweenStructreAndContent { get; } = 16;

	readonly List<VariableStructure.IDataRecord> _Members;
	public IReadOnlyList<VariableStructure.IDataRecord> Members => _Members;

	public VariableSMem(string Name, long Capacity, VariableStructure structure) : this(
		new SMemIF(Name, Capacity),
		structure.Records,
		structure
	)
	{
	}

	public VariableSMem(Type type, string Name, long Capacity) : this(
		new SMemIF(Name, Capacity),
		type.ToVariableDataRecordList(),
		null
	)
	{
	}

	public VariableSMem(ISMemIF smemIF, VariableStructure structure) : this(
		smemIF,
		structure.Records,
		structure
	)
	{
	}

	public VariableSMem(Type type, ISMemIF smemIF) : this(
		smemIF,
		type.ToVariableDataRecordList(),
		null
	)
	{
	}

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
			Structure = new(-1, _Members);
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
	/// 型情報を共有メモリから読み取ってインスタンスを初期化する
	/// </summary>
	/// <param name="Name">共有メモリの名前</param>
	/// <param name="Capacity">初期化に使用するキャパシティ (<c>null</c>でデフォルト値 = 4096 Bytes)</param>
	/// <returns>作成したインスタンス</returns>
	/// <exception cref="NotInitializedException">共有メモリが作成されていなかった</exception>
	public static VariableSMem CreateWithoutType(string Name, long? Capacity = null)
		=> CreateWithoutType(new SMemIF(Name, Capacity ?? 0x1000));

	/// <summary>
	/// 型情報を共有メモリから読み取ってインスタンスを初期化する
	/// </summary>
	/// <param name="SMemIF">共有メモリを操作するインターフェイスを持つインスタンス</param>
	/// <returns></returns>
	/// <returns>作成したインスタンス</returns>
	/// <exception cref="NotInitializedException">共有メモリが作成されていなかった</exception>
	public static VariableSMem CreateWithoutType(ISMemIF SMemIF)
	{
		if (SMemIF.IsNewlyCreated)
			throw new NotInitializedException(nameof(SMemIF.SMemName));

		if (!SMemIF.Read(0, out long contentAreaOffset))
			throw new AccessViolationException("Read from SMem failed");

		if (contentAreaOffset <= (StructureAreaOffset + PaddingBetweenStructreAndContent))
			throw new FormatException($"Invalid Structure in SMem (too less size = {contentAreaOffset} bytes)");

		byte[] structureBytes = new byte[
			contentAreaOffset
			- StructureAreaOffset
			- PaddingBetweenStructreAndContent
		];

		if (!SMemIF.ReadArray(StructureAreaOffset, structureBytes, 0, structureBytes.Length))
			throw new AccessViolationException("Read from SMem failed");

		// TODO: `VariableDataParser.ParseDataTypeRegisterCommand`をPublicにして、それを利用してParseする
		throw new NotImplementedException("This feature is currently not implemented");
	}

	public VariableStructurePayload ReadFromSMem()
	{
		if (!SMemIF.Read(ContentAreaOffset, out long contentDataLength))
			throw new AccessViolationException("Read from SMem failed");

		byte[] content = new byte[contentDataLength];
		if (!SMemIF.ReadArray(ContentAreaOffset + sizeof(long), content, 0, content.Length))
			throw new AccessViolationException("Read from SMem failed");

		return Structure.With(content);
	}

	public void ReadFromSMem(ref object target)
	{
		VariableStructurePayload payload = ReadFromSMem();
		Type targetType = target.GetType();

		foreach (var data in payload.Values)
		{
			if (targetType.GetProperty(data.Name) is PropertyInfo propInfo)
			{
				propInfo.SetValue(target, GetValueObjectFromDataRecord(data));
			}
			else if (targetType.GetField(data.Name) is FieldInfo fieldInfo)
			{
				fieldInfo.SetValue(target, GetValueObjectFromDataRecord(data));
			}
		}
	}

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

		byte[] bytes = Structure.GetBytes().ToArray();
		long contentLength = bytes.LongLength;
		if (!SMemIF.Write(ContentAreaOffset, ref contentLength)
			|| !SMemIF.WriteArray(ContentAreaOffset + sizeof(long), bytes, 0, bytes.Length))
			throw new AccessViolationException("Write to SMem failed");
	}

	internal static object? GetValueObjectFromDataRecord(VariableStructure.IDataRecord dataRecord)
		=> dataRecord switch
		{
			VariableStructure.DataRecord v => v.Value,
			VariableStructure.ArrayStructure v => v.ValueArray,

			_ => throw new ArgumentException($"The Type {dataRecord?.GetType()} is currently not supported", nameof(dataRecord)),
		};

	internal static VariableStructure.IDataRecord CreateNewRecordWithValue(VariableStructure.IDataRecord memberDataRecord, Type memberType, object? value)
	{
		VariableDataType memberVDType = memberType.ToVariableDataType();

		if (memberDataRecord.Type != memberVDType)
			throw new Exception($"Type mismatch (given: {memberVDType} / Initialized with: {memberDataRecord.Type})");

		if (memberVDType == VariableDataType.Array)
		{
			if (memberDataRecord is not VariableStructure.ArrayStructure arrayStructure)
				throw new Exception("Type mismatch (Given member was array, but dataRecord was not ArrayStructure)");

			if (memberType == typeof(string))
			{
				if (value is not string s)
					throw new Exception($"Given Value is not string or null (GivenType: {memberType})");

				// TODO: Boxingによりパフォーマンス的に好ましくないはず。極力Boxingなしにできるように書き直す
				// Maybe?: Array型が適切...?
				return arrayStructure with
				{
					ValueArray = DefaultEncoding.GetBytes(s)
				};
			}

			// elementの型チェック
			if (memberType.GetElementType() is not Type elemType)
				throw new Exception("Cannot recognize Array element type");

			VariableDataType elemVDType = elemType.ToVariableDataType();
			if (arrayStructure.ElemType != elemVDType)
				throw new Exception($"Type mismatch (given: {elemVDType} / Initialized with: {arrayStructure.ElemType})");

			return arrayStructure with
			{
				ValueArray = value as Array
			};
		}
		else
		{
			if (memberDataRecord is not VariableStructure.DataRecord dataRecord)
				throw new Exception("Type mismatch (Given member was normal data, but dataRecord was not DataRecord)");

			return dataRecord with
			{
				Value = value
			};
		}
	}
}

public class VariableSMem<T> : VariableSMem
{
	public VariableSMem(string Name, long Capacity) : base(typeof(T), Name, Capacity)
	{
	}

	public VariableSMem(ISMemIF SMemIF) : base(typeof(T), SMemIF)
	{
	}

	// TODO: `WriteToSMem`をoverrideして書く
}
