using System;

namespace TR.BIDSSMemLib.Variable.Exceptions;

public class NotInitializedException : Exception
{
	public NotInitializedException(string SMemName) : base($"The SMem `{SMemName}` is not initialized")
	{
	}
}
