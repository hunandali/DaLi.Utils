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
 *  标签信息
 * 
 * 	name: TagInfo
 * 	create: 2025-03-08
 * 	memo: 标签信息，包含标签名称、属性列表、原始内容和哈希值
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace DaLi.Utils.Template {

	/// <summary>标签信息，包含标签名称、属性列表、原始内容和哈希值</summary>
	public class TagInfo {
		/// <summary>标签名称</summary>
		public string Name { get; set; }

		/// <summary>标签属性列表</summary>
		public List<KeyValuePair<string, string>> Attributes { get; set; }

		/// <summary>标签原始内容</summary>
		public string OriginalContent { get; set; }

		/// <summary>标签内容的哈希值</summary>
		public string Hash { get; set; }

		/// <summary>初始化标签信息</summary>
		public TagInfo() => Attributes = [];

		/// <summary>获取标签的字符串表示</summary>
		public override string ToString() => OriginalContent ?? $"{Name} (属性数: {Attributes.Count})";

		/// <summary>获取指定名称的属性值</summary>
		/// <param name="name">属性名称</param>
		/// <returns>属性值，如果不存在则返回 null</returns>
		public string GetAttributeValue(string name) {
			if (string.IsNullOrEmpty(name)) {
				return null;
			}

			var attr = Attributes.FirstOrDefault(a => string.Equals(a.Key, name, StringComparison.OrdinalIgnoreCase));
			return attr.Key != null ? attr.Value : null;
		}

		/// <summary>检查是否包含指定名称的属性</summary>
		/// <param name="name">属性名称</param>
		/// <returns>是否包含该属性</returns>
		public bool HasAttribute(string name) {
			if (string.IsNullOrEmpty(name)) {
				return false;
			}

			return Attributes.Any(a => string.Equals(a.Key, name, StringComparison.OrdinalIgnoreCase));
		}
	}
}
