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
 *  文本键文本值字典集合
 * 
 * 	name: SSDictionary
 * 	create: 2025-03-07
 * 	memo: 文本键(String)文本值(String)字典集合(Dictionary)，忽略键名大小写，对于空白键也不允许
 * 
 * ------------------------------------------------------------
 */

using System.Collections.Generic;

namespace DaLi.Utils.Model {

	/// <summary>文本键文本值字典集合</summary>
	public class SSDictionary : StringDictionary<string> {

		/// <summary>构造</summary>
		public SSDictionary() : base() { }

		/// <summary>构造</summary>
		public SSDictionary(IEnumerable<KeyValuePair<string, string>> collection) : base(collection) { }

		/// <summary>构造</summary>
		public SSDictionary(IDictionary<string, string> dictionary) : base(dictionary) { }

		/// <summary>构造</summary>
		public SSDictionary(string json) : base(json) { }

		/// <summary>设置 / 获取项目，设置时如果不存在则新建，存在则更新</summary>
		public new string this[string key] {
			get {
				if (BadKey(key)) { return ""; }

				base.TryGetValue(key, out var R);
				return R ?? "";
			}
			set {
				if (BadKey(key)) { return; }

				lock (Locker) {
					base[key] = value;
				}
			}
		}
	}
}