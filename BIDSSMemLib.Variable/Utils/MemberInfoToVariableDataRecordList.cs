using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using BIDS.Parser.Variable;

namespace TR.BIDSSMemLib.Variable;

public static partial class Utils
{
	public static List<VariableStructure.IDataRecord> ToVariableDataRecordList(this Type type)
		=> type
			.GetMembers(BindingFlags.Public | BindingFlags.Instance)
			.Where(v => v.MemberType is MemberTypes.Field or MemberTypes.Property)
			.Select(v => v.ToVariableDataRecord())
			.ToList();
}
