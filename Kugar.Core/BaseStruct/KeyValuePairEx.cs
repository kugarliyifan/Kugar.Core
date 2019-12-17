/*
 * 由SharpDevelop创建。
 * 用户： Administrator
 * 日期: 2013-2-25
 * 时间: 14:18
 * 
 * 要改变这种模板请点击 工具|选项|代码编写|编辑标准头文件
 */
using System;
using System.Collections.Generic;

namespace Kugar.Core.BaseStruct
{
	/// <summary>
	/// Description of KeyValuePairEx.
	/// </summary>
	[Serializable]
	public class KeyValuePairEx<TKey,TValue>
	{
		public KeyValuePairEx(TKey key,TValue value)
		{
			Key=key;
			Value=value;
		}
		
		
		public TKey Key{private set;get;}
		public TValue Value{set;get;}
		
		public static implicit operator KeyValuePair<TKey,TValue>(KeyValuePairEx<TKey,TValue> m)
		{
			return new KeyValuePair<TKey,TValue>(m.Key,m.Value);
		}
		
		public static implicit operator KeyValuePairEx<TKey,TValue>(KeyValuePair<TKey,TValue> m)
		{
			return new KeyValuePairEx<TKey,TValue>(m.Key,m.Value);
		}
	}
}
