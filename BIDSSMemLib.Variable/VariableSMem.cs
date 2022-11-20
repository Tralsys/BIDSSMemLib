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
public partial class VariableSMem
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

		// DataType IDはStructure側で既に書き込んであるため、Content側には含めない
		byte[] bytes = Structure.GetBytes().Skip(sizeof(int)).ToArray();
		long contentLength = bytes.LongLength;
		if (!SMemIF.Write(ContentAreaOffset, ref contentLength)
			|| !SMemIF.WriteArray(ContentAreaOffset + sizeof(long), bytes, 0, bytes.Length))
			throw new AccessViolationException("Write to SMem failed");
	}
}
