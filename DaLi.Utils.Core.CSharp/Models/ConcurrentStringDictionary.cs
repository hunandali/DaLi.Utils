/* ------------------------------------------------------------
 * 
 * 	Copyright © 2021 湖南大沥网络科技有限公司.
 * 	Dali.Utils Is licensed under Mulan PSL v2.
 * 
 * 		  author:	木炭(WOODCOAL)
 * 		   email:	i@woodcoal.cn
 * 		homepage:	http://www.hunandali.com/
 * 
 * 	请依据 Mulan PSL v2 的条款使用本项目。获取 Mulan PSL v2 请浏览 http://license.coscl.org.cn/MulanPSL2
 * 
 * ------------------------------------------------------------
 * 
 *  线程安全文本键任意值字典集合
 * 
 * 	name: ConcurrentStringDictionary
 * 	create: 2025-03-07
 * 	memo: 线程安全文本键(String)泛类值字典集合(Dictionary)，忽略键名大小写，对于空白键也不允许
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DaLi.Utils.Model {

	/// <summary>线程安全文本键任意值字典集合</summary>
	public class ConcurrentStringDictionary<T> : ConcurrentDictionary<string, T> {

		#region 初始化

		/// <summary>构造</summary>
		public ConcurrentStringDictionary() : base(StringComparer.OrdinalIgnoreCase) { }

		/// <summary>构造</summary>
		public ConcurrentStringDictionary(IEnumerable<KeyValuePair<string, T>> collection) : base(collection, StringComparer.OrdinalIgnoreCase) { }

		/// <summary>构造</summary>
		public ConcurrentStringDictionary(IDictionary<string, T> dictionary) : base(dictionary, StringComparer.OrdinalIgnoreCase) { }

		#endregion

	}
}