using System;

using BIDS.Parser.Variable;
using TR.BIDSSMemLib.Variable.Exceptions;

namespace TR.BIDSSMemLib;

/// <summary>
/// 可変構造(可変長)な共有メモリを使用するためのクラス
/// </summary>
public partial class VariableSMem
{
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

		VariableStructure structure = VariableCmdParser.ParseDataTypeRegisterCommand(structureBytes);

		return new VariableSMem(SMemIF, structure);
	}

	internal static object? GetValueObjectFromDataRecord(VariableStructure.IDataRecord dataRecord, bool isString)
		=> dataRecord switch
		{
			VariableStructure.DataRecord v => v.Value,
			VariableStructure.ArrayStructure v =>
				(isString && v.ValueArray is byte[] arr)
				? DefaultEncoding.GetString(arr)
				: v.ValueArray,

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
