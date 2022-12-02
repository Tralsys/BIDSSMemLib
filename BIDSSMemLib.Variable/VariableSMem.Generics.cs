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
public partial class VariableSMem<T> : VariableSMem
{
	static Dictionary<string, MemberInfo> PropertyAndFields { get; } =
		typeof(T)
			.GetMembers(BindingFlags.Instance | BindingFlags.Public)
			.Where(v => v is PropertyInfo or FieldInfo)
			.ToDictionary(v => v.Name);

	static Dictionary<string, MemberInfo> ReadablePropertyAndFields { get; }

	static Dictionary<string, MemberInfo> WritablePropertyAndFields { get; }

	static Dictionary<string, SetterInfo> Setters { get; }
	static Dictionary<string, (Func<object?, object?> Getter, Type Type)> Getters { get; }

	class SetterInfo
	{
		readonly Func<object?, object?[]?, object?>? PropertySetter = null;
		readonly FieldInfo? FieldInfo;
		readonly Type ValueType;
		public readonly bool IsString;

		public SetterInfo(MemberInfo memberInfo)
		{
			if (memberInfo is PropertyInfo propertyInfo)
			{
				ValueType = propertyInfo.PropertyType;

				if (propertyInfo.GetGetMethod() is MethodInfo methodInfo)
					PropertySetter = methodInfo.Invoke;
			}
			else if (memberInfo is FieldInfo fieldInfo)
			{
				ValueType = fieldInfo.FieldType;
				this.FieldInfo = fieldInfo;
			}
			else
				throw new ArgumentException("only Property or Field is supported", nameof(memberInfo));

			IsString = (ValueType == typeof(string));
		}

		public void SetValue(ref T target, in object value)
		{
			if (PropertySetter is not null)
				PropertySetter(target, new object[] { value });
			else if (FieldInfo is not null)
				FieldInfo.SetValueDirect(__makeref(target), value);
		}
	}

	static VariableSMem()
	{
		if (typeof(T).IsPrimitive)
			throw new ArgumentException($"The primitive type `{typeof(T)}` is not supported.");

		ReadablePropertyAndFields = PropertyAndFields.Values
			.Where(v => (v is PropertyInfo p && p.CanRead) || v is FieldInfo f)
			.ToDictionary(v => v.Name);
		WritablePropertyAndFields = PropertyAndFields.Values
			.Where(v => (v is PropertyInfo p && p.CanWrite) || (v is FieldInfo f && !f.IsInitOnly))
			.ToDictionary(v => v.Name);

		Getters = new();
		foreach (MemberInfo member in ReadablePropertyAndFields.Values)
		{
			if (member is PropertyInfo propertyInfo)
				Getters.Add(propertyInfo.Name, (propertyInfo.GetValue, propertyInfo.PropertyType));
			else if (member is FieldInfo fieldInfo)
				Getters.Add(fieldInfo.Name, (fieldInfo.GetValue, fieldInfo.FieldType));
		}

		Setters = new();
		foreach (MemberInfo member in WritablePropertyAndFields.Values)
		{
			Setters.Add(member.Name, new SetterInfo(member));
		}
	}

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
			if (Setters.TryGetValue(data.Name, out var setterInfo))
			{
				if (GetValueObjectFromDataRecord(data, setterInfo.IsString) is object v)
					setterInfo.SetValue(ref target, v);
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

			if (Getters.TryGetValue(propName, out var getterInfo))
				_Members[i] = CreateNewRecordWithValue(iDataRecord, getterInfo.Type, getterInfo.Getter(data));
		}

		// DataType IDはStructure側で既に書き込んであるため、Content側には含めない
		byte[] bytes = Structure.GetBytes().Skip(sizeof(int)).ToArray();
		long contentLength = bytes.LongLength;
		if (!SMemIF.Write(ContentAreaOffset, ref contentLength)
			|| !SMemIF.WriteArray(ContentAreaOffset + sizeof(long), bytes, 0, bytes.Length))
			throw new AccessViolationException("Write to SMem failed");
	}
}
