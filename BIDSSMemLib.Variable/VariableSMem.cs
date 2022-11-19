using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

using BIDS.Parser.Variable;

using TR.BIDSSMemLib.Variable;

namespace TR.BIDSSMemLib;

/// <summary>
/// 可変構造(可変長)な共有メモリを使用するためのクラス
/// </summary>
public class VariableSMem
{
	ISMemIF SMemIF { get; }

	public VariableStructure Structure { get; }

	public long StructureAreaOffset { get; } = 16;
	public long ContentAreaOffset { get; }

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

		Structure = structure ?? new VariableStructure(-1, members);

		Debug.Assert(Structure.Records == members, "`members` and `structure.Records` must be same instance`");

		_Members = members.ToList();

		ContentAreaOffset = InitSMem();
	}

	/// <summary>
	/// 共有メモリを初期化する
	/// </summary>
	/// <returns>ContentAreaOffset</returns>
	long InitSMem()
	{
		byte[] structureBytes = Structure.GetStructureBytes().ToArray();

		long contentAreaOffset = StructureAreaOffset + structureBytes.Length + 256;
		if (SMemIF.IsNewlyCreated)
		{
			if (!SMemIF.Write(0, ref contentAreaOffset)
				|| SMemIF.WriteArray(StructureAreaOffset, structureBytes, 0, structureBytes.Length))
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

			// elementの型チェック
			if (memberType.GetElementType() is not Type elemType)
				throw new Exception("Cannot recognize Array element type");

			VariableDataType elemVDType = elemType.ToVariableDataType();
			if (arrayStructure.ElemType != elemVDType)
				throw new Exception($"Type mismatch (given: {elemVDType} / Initialized with: {arrayStructure.ElemType})");

			// TODO: 正常にキャストできているかどうかチェック (object[]へのキャストでうまくいってる?)
			return arrayStructure with
			{
				ValueArray = value as object[]
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
}
