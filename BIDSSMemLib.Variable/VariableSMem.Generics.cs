using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using BIDS.Parser.Variable;

namespace TR.BIDSSMemLib;

/// <summary>
/// 可変構造(可変長)な共有メモリを使用するためのクラス
/// </summary>
/// <typeparam name="T">使用する型</typeparam>
public class VariableSMem<T> : VariableSMem
{
	Type TargetType { get; } = typeof(T);
	Dictionary<string, PropertyInfo> Properties { get; } = typeof(T).GetProperties().ToDictionary(v => v.Name);
	Dictionary<string, FieldInfo> Fields { get; } = typeof(T).GetFields().ToDictionary(v => v.Name);

	// TODO: VariableStructure.DataRecord等を継承した専用クラスを用意し、そこに`GetValue` / `SetValue`メソッドを記録できるようにする

	/// <summary>
	/// インスタンスを初期化する
	/// </summary>
	/// <param name="Name">共有メモリ名</param>
	/// <param name="Capacity">共有メモリの初期化に使用するキャパシティ [bytes]</param>
	public VariableSMem(string Name, long Capacity) : base(typeof(T), Name, Capacity)
	{
	}

	/// <summary>
	/// インスタンスを初期化する
	/// </summary>
	/// <param name="SMemIF">共有メモリの操作に使用するインスタンス</param>
	public VariableSMem(ISMemIF SMemIF) : base(typeof(T), SMemIF)
	{
	}

	/// <summary>
	/// 共有メモリからデータを読み取り、指定のインスタンスに値を書き込む
	/// </summary>
	/// <param name="target">値を書き込む先のインスタンス</param>
	/// <exception cref="AccessViolationException">共有メモリの操作に失敗した</exception>
	public void ReadFromSMem(ref T target)
	{
		VariableStructurePayload payload = ReadFromSMem();

		foreach (var data in payload.Values)
		{
			if (Properties.TryGetValue(data.Name, out PropertyInfo? propInfo))
			{
				propInfo.SetValue(target, GetValueObjectFromDataRecord(data, propInfo.PropertyType == typeof(string)));
			}
			else if (Fields.TryGetValue(data.Name, out FieldInfo? fieldInfo))
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
	public void WriteToSMem(in T data)
	{
		for (int i = 0; i < _Members.Count; i++)
		{
			VariableStructure.IDataRecord iDataRecord = _Members[i];

			string propName = iDataRecord.Name;
			if (Properties.TryGetValue(propName, out PropertyInfo? propInfo))
			{
				_Members[i] = CreateNewRecordWithValue(iDataRecord, propInfo.PropertyType, propInfo.GetValue(data));
			}
			else if (Fields.TryGetValue(propName, out FieldInfo? fieldInfo))
			{
				_Members[i] = CreateNewRecordWithValue(iDataRecord, fieldInfo.FieldType, fieldInfo.GetValue(data));
			}
		}

		// DataType IDはStructure側で既に書き込んであるため、Content側には含めない
		byte[] bytes = Structure.GetBytes().Skip(sizeof(int)).ToArray();
		long contentLength = bytes.LongLength;
		if (!SMemIF.Write(ContentAreaOffset, ref contentLength)
			|| !SMemIF.WriteArray(ContentAreaOffset + sizeof(long), bytes, 0, bytes.Length))
			throw new AccessViolationException("Write to SMem failed");
	}
}
