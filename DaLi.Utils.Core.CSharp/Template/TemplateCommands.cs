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
 *  模板常用命令
 * 
 * 	name: TemplateCommands
 * 	create: 2025-03-07
 * 	memo: 模板常用命令
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DaLi.Utils.Extension;
using DaLi.Utils.Json;
using DaLi.Utils.Model;

namespace DaLi.Utils.Template {
	/// <summary>模板常用命令</summary>
	public static class TemplateCommands {

		#region 注册操作

		/// <summary>注册当前所有的命令</summary>
		public static void Register(TemplateAction action) {
			if (action is null) { return; }

			var cmds = AllCommands();
			foreach (var cmd in cmds) {
				action.Register(cmd.Key, cmd.Value);
			}
		}

		/// <summary>获取当前所有的命令</summary>
		public static Dictionary<string, Func<object, SSDictionary, object>> AllCommands() {
			var currentType = typeof(TemplateCommands);

			// 获取所有公共静态只读字段
			var fields = currentType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.GetField);
			var baseType = typeof(Func<object, SSDictionary, object>);

			Dictionary<string, Func<object, SSDictionary, object>> functions = [];
			foreach (var field in fields) {
				// 检查字段类型是否为 Func<object, SSDictionary, object>
				if (field.FieldType == baseType) {
					// 获取字段的值
					var function = (Func<object, SSDictionary, object>) field.GetValue(null);

					functions.Add(field.Name, function);
				}
			}

			return functions;
		}

		#endregion

		/// <summary>内容是否为空</summary>
		/// <remarks>empty / empty.true / empty.false</remarks>
		public static readonly Func<object, SSDictionary, object> Empty = (value, attrs) => {
			var flag = value is null;

			if (!flag) {
				if (value is string s) {
					flag = string.IsNullOrEmpty(s);
				} else if (value is ICollection c) {
					flag = c.IsEmpty();
				} else if (value is IEnumerable e) {
					flag = e.IsEmpty();
				} else if (value is IDictionary d) {
					flag = d.IsEmpty();
				}
			}

			var temp = attrs[flag ? "true" : "false"];
			if (string.IsNullOrEmpty(temp)) { return flag; }

			var def = (value ?? "").ToString();
			return temp.Replace("[*]", def);
		};

		/// <summary>是否与字符串匹配</summary>
		/// <remarks>like / like.true / like.false</remarks>
		public static readonly Func<object, SSDictionary, object> Like = (value, attrs) => {
			if (value is null) { return null; }

			var s = value.ToString();

			var flag = s.IsLike(attrs["like"]);
			var temp = attrs[flag ? "true" : "false"];
			if (string.IsNullOrEmpty(temp)) { return flag; }

			return temp.Replace("[*]", s);
		};

		/// <summary>替换</summary>
		/// <remarks>replace / replace.to</remarks>
		public static readonly Func<object, SSDictionary, object> Replace = (value, attrs) => {
			if (value is null) { return null; }

			var s = value.ToString();

			var source = attrs["replace"];
			var target = attrs["to"];

			return s.Replace(source, target);
		};

		/// <summary>转换成文本，比较是否相等，默认比较大小写</summary>
		/// <remarks>
		/// equals：用于比较的值
		/// equals.case:是否比较大小写 
		/// equals.true：比较结果为 true 时返回的值
		/// equals.false：比较结果为 false 时返回的值
		/// </remarks>
		public static new readonly Func<object, SSDictionary, object> Equals = (value, attrs) => {
			if (value is null) { return null; }

			var s = value.ToString();

			var equals = attrs["equals"];
			var ingnoreCase = attrs["case"].Equals("true", StringComparison.OrdinalIgnoreCase) ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

			var flag = s.Equals(equals, ingnoreCase);
			var temp = attrs[flag ? "true" : "false"];
			if (string.IsNullOrEmpty(temp)) { return flag; }

			return temp.Replace("[*]", s);
		};

		/// <summary>长度</summary>
		/// <remarks>文本长度，数组长度，集合长度，字典长度，类中包含 length 属性 或者 count 属性</remarks>
		/// <returns>存在返回实际长度，数据为 null 返回 -1，不存在相关属性返回 -2</returns>
		public static readonly Func<object, SSDictionary, object> Length = (value, attrs) => {
			if (value is null) { return -1; }

			if (value is string str) { return str.Length; }
			if (value is Array arr) { return arr.Length; }
			if (value is IDictionary dic) { return dic.Count; }
			if (value is ICollection col) { return col.Count; }
			if (value is IList list) { return list.Count; }
			if (value is IEnumerable enus) { return enus.Count(); }

			var type = value.GetType();

			// 数字，注意小数是不正确的
			if (type.IsPrimitive || value is decimal) { return value.ToString().Length; }

			// 从属性分析
			if (type.IsClass) {
				var len = type.GetSingleProperty("length") ?? type.GetSingleProperty("count");
				if (len != null) { return len.GetValue(value); }
			}

			return -2;
		};

		/// <summary>使用 string.Format 将对象按条件格式化</summary>
		public static readonly Func<object, SSDictionary, object> Format = (value, attrs) => {
			if (value is null) { return null; }

			var format = attrs["format"];
			if (string.IsNullOrWhiteSpace(format)) { return value.ToString(); }

			try {
				return string.Format(format, value);
			} catch (Exception) {
				return "";
			}
		};

		/// <summary>使用 Json 序列或者反序列数据</summary>
		/// <remarks>true: 序列化；false 反序列化</remarks>
		public static readonly Func<object, SSDictionary, object> Json = (value, attrs) => {
			if (value is null) { return null; }

			var json = attrs["json"];
			if (bool.TryParse(json, out var val) && val == true) {
				return value.ToJson();
			}

			if (value is string str) {
				return str.FromJson();
			}

			return null;
		};

	}
}
