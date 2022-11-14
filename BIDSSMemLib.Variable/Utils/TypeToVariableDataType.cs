using System;

using BIDS.Parser.Variable;

namespace TR.BIDSSMemLib.Variable;

public static partial class Utils
{
	// ref: https://www.nekoteam.com/node/25
	/// <summary>
	/// Type型をBIDS可変データで使用する型情報に変換する
	/// </summary>
	/// <param name="type">変換する型情報</param>
	/// <returns>BIDS可変データで使用する型情報</returns>
	/// <exception cref="NotSupportedException">BIDS可変データで使用できないデータ型</exception>
	public static VariableDataType ToVariableDataType(this Type type)
		=> type.IsArray
			? VariableDataType.Array
			: Type.GetTypeCode(type) switch
			{
				TypeCode.Boolean => VariableDataType.Boolean,

				TypeCode.SByte => VariableDataType.Int8,
				TypeCode.Int16 => VariableDataType.Int16,
				TypeCode.Int32 => VariableDataType.Int32,
				TypeCode.Int64 => VariableDataType.Int64,

				TypeCode.Byte => VariableDataType.UInt8,
				TypeCode.UInt16 => VariableDataType.UInt16,
				TypeCode.UInt32 => VariableDataType.UInt32,
				TypeCode.UInt64 => VariableDataType.UInt64,

				TypeCode.Single => VariableDataType.Float32,
				TypeCode.Double => VariableDataType.Float64,

				TypeCode.Object when type == typeof(Half) => VariableDataType.Float16,

				TypeCode.String => VariableDataType.Array,

				_ => throw new NotSupportedException($"The Type `{type.FullName ?? type.Name}` is currently not supported.")
			};
}
