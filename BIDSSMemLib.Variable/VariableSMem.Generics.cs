namespace TR.BIDSSMemLib;

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
