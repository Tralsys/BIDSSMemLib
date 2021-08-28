using System;

namespace TR
{
	/// <summary>値が変化した際に発火するイベントの引数</summary>
	/// <typeparam name="T">値の型</typeparam>
	public class ValueChangedEventArgs<T> : EventArgs
	{
		public readonly T OldValue;
		public readonly T NewValue;
		public ValueChangedEventArgs(in T oldValue, in T newValue)
		{
			OldValue = oldValue;
			NewValue = newValue;
		}
	}
}
