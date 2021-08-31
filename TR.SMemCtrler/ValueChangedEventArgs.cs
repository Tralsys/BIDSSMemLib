using System;

namespace TR
{
	/// <summary>値が変化した際に発火するイベントの引数</summary>
	/// <typeparam name="T">値の型</typeparam>
	public class ValueChangedEventArgs<T> : EventArgs
	{
		/// <summary>更新前の値</summary>
		public readonly T OldValue;
		/// <summary>更新後の値</summary>
		public readonly T NewValue;

		/// <summary>インスタンスを初期化する</summary>
		/// <param name="oldValue">更新前の値</param>
		/// <param name="newValue">更新後の値</param>
		public ValueChangedEventArgs(in T oldValue, in T newValue)
		{
			OldValue = oldValue;
			NewValue = newValue;
		}
	}
}
