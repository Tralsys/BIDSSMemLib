﻿using System;
using System.Runtime.InteropServices;

namespace TR
{
	/// <summary>セマフォレスに共有メモリ空間へのアクセスを提供する各処理の前処理を実装したクラス</summary>
	public abstract class SemaphorelessSMemIF_Preprocessing : ISmemIF
	{
		/// <summary>リソースの解放中, あるいは解放に完了しているかどうか</summary>
		protected bool disposingValue { get; set; } = false;

		/// <summary>共有メモリの名前</summary>
		public string SMemName { get; }

		/// <summary>共有メモリ空間のキャパシティ</summary>
		/// <remarks>setterにてキャパシティの増減機能を実装する</remarks>
		public abstract long Capacity { get; set; }

		/// <summary>与えられた型が使用する領域のサイズを計算する</summary>
		/// <typeparam name="T">領域を計算する型</typeparam>
		/// <returns>サイズ [bytes]</returns>
		public static long getElemSize<T>() where T : struct => Marshal.SizeOf(default(T));

		/// <summary>必要になるキャパシティを計算する</summary>
		/// <typeparam name="T">領域を計算する型</typeparam>
		/// <param name="pos">読み書きを行う位置 [bytes]</param>
		/// <param name="count">データの数 [個]</param>
		/// <returns>サイズ [bytes]</returns>
		public static long getNeededCapacity<T>(in long pos, in int count = 1) where T : struct => pos + getElemSize<T>() * count;

		/// <summary>インスタンスを初期化する</summary>
		/// <param name="smem_name">共有メモリ空間の名前としてセットする文字列</param>
		/// <param name="capacity">初期化時点で取得する共有メモリ空間のキャパシティ</param>
		public SemaphorelessSMemIF_Preprocessing(in string smem_name, in long capacity)
		{
			if (string.IsNullOrEmpty(smem_name))
				throw new ArgumentOutOfRangeException(nameof(smem_name), "smem_name cannot be null or empty");
			else if (capacity <= 0)
				throw new ArgumentOutOfRangeException(nameof(capacity), "capacity cannot be 0 or less");

			SMemName = smem_name;
		}

		/// <summary>リソースの解放を行う</summary>
		public virtual void Dispose() => disposingValue = true;

		/// <summary>共有メモリ空間の指定の位置から, 指定の型のデータを読み込む</summary>
		/// <typeparam name="T">読み込みたい型</typeparam>
		/// <param name="pos">読み込む位置 [bytes]</param>
		/// <param name="buf">読み込むデータ</param>
		/// <returns>読み込みに成功したかどうか  (例外は捕捉されません)</returns>
		public virtual bool Read<T>(long pos, out T buf) where T : struct
		{
			buf = default;

			if (disposingValue)
				return false; //解放が開始したら実行しない

			CheckReOpen(getNeededCapacity<T>(pos));

			return true;
		}

		/// <summary>SMemから連続的に値を読み取ります</summary>
		/// <typeparam name="T">値の型</typeparam>
		/// <param name="pos">読み込む位置 [bytes]</param>
		/// <param name="buf">読み取り結果を格納する配列</param>
		/// <param name="offset">配列内で書き込みを開始する位置</param>
		/// <param name="count">読み取りを行う数</param>
		/// <returns>読み取りに成功したかどうか</returns>
		public virtual bool ReadArray<T>(long pos, T[] buf, int offset, int count) where T : struct
		{
			if (disposingValue)
				return false; //解放が開始したら実行しない

			CheckReOpen(getNeededCapacity<T>(pos));

			return true;
		}

		/// <summary>共有メモリ空間の指定の位置に指定のデータを書き込む</summary>
		/// <typeparam name="T">データの型</typeparam>
		/// <param name="pos">書き込む位置 [bytes]</param>
		/// <param name="buf">書き込むデータ</param>
		/// <returns>書き込みに成功したかどうか</returns>
		public virtual bool Write<T>(long pos, ref T buf) where T : struct
		{
			if (disposingValue)
				return false; //解放が開始したら実行しない

			CheckReOpen(getNeededCapacity<T>(pos));

			return true;
		}

		/// <summary>SMemに連続した値を書き込みます</summary>
		/// <typeparam name="T">書き込む値の型</typeparam>
		/// <param name="pos">書き込みを開始するSMem内の位置 [bytes]</param>
		/// <param name="buf">SMemに書き込む配列</param>
		/// <param name="offset">配列内で書き込みを開始する位置</param>
		/// <param name="count">書き込む要素数</param>
		/// <returns>書き込みに成功したかどうか</returns>
		public virtual bool WriteArray<T>(long pos, T[] buf, int offset, int count) where T : struct
		{
			if (disposingValue)
				return false; //解放が開始したら実行しない

			CheckReOpen(getNeededCapacity<T>(pos));

			return true;
		}

		/// <summary>共有メモリ空間を再度開く必要があるかどうかを確認する</summary>
		/// <param name="needed_capacity">必要なキャパシティ</param>
		protected abstract void CheckReOpen(long needed_capacity);
	}
}