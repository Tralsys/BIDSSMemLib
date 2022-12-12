using System.Text;

namespace TR.BIDSSMemLib.ValueChecker;

internal static class StringBuilderUtils
{
	static public StringBuilder MyAppend(this StringBuilder builder, in State v)
		=> builder
			.Append("State={ ")
			.AppendFormat("Z={0}, ", v.Z)
			.AppendFormat("V={0}, ", v.V)
			.AppendFormat("T={0}, ", v.T)
			.AppendFormat("BC={0}, ", v.BC)
			.AppendFormat("MR={0}, ", v.MR)
			.AppendFormat("ER={0}, ", v.ER)
			.AppendFormat("BP={0}, ", v.BP)
			.AppendFormat("SAP={0}, ", v.SAP)
			.AppendFormat("I={0}", v.I)
			.Append(" } ");

	static public StringBuilder MyAppend(this StringBuilder builder, in Spec v)
		=> builder
			.Append("Spec={ ")
			.AppendFormat("B={0}, ", v.B)
			.AppendFormat("P={0}, ", v.P)
			.AppendFormat("A={0}, ", v.A)
			.AppendFormat("J={0}, ", v.J)
			.AppendFormat("C={0}", v.C)
			.Append(" } ");

	static public StringBuilder MyAppend(this StringBuilder builder, in Hand v)
		=> builder
			.Append("Hand={ ")
			.AppendFormat("B={0}, ", v.B)
			.AppendFormat("P={0}, ", v.P)
			.AppendFormat("R={0}, ", v.R)
			.AppendFormat("C={0}", v.C)
			.Append(" } ");

	static public StringBuilder MyAppend(this StringBuilder builder, in BIDSSharedMemoryData v)
		=> builder
			.AppendLine("BSMD={")
			.AppendFormat("\tIsEnabled={0}", v.IsEnabled).AppendLine()
			.AppendFormat("\tVersion={0}", v.VersionNum).AppendLine()
			.AppendFormat("\tIsDoorClosed={0}", v.IsDoorClosed).AppendLine()
			.Append('\t').MyAppend(v.HandleData).AppendLine()
			.Append('\t').MyAppend(v.SpecData).AppendLine()
			.Append('\t').MyAppend(v.StateData).AppendLine()
			.Append('}');
}
