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
 *  类型转换
 * 
 * 	name: ConvertHelper
 * 	create: 2025-03-06
 * 	memo: 类型转换
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DaLi.Utils.Extension;
using DaLi.Utils.Json;

namespace DaLi.Utils.Helper {
	/// <summary>类型转换</summary>
	public static class ConvertHelper {

		#region 数据类型转换

		/// <summary>数据类型转换</summary>
		/// <param name="input">要转换的数据</param>
		/// <param name="type">指定类型</param>
		/// <param name="defaultValue">转换失败的默认值</param>
		/// <remarks>尝试直接转换，如果失败则尝试通过 JSON 序列化和反序列化</remarks>
		public static object ChangeObject(object input, Type type, object defaultValue = null) {
			if (input is null || type is null) { return defaultValue; }

			// 如果输入和目标类型相同，直接返回
			if (input.GetType() == type) { return input; }

			// 如果input的类型可以直接赋值给T，直接进行类型转换
			if (type.IsAssignableFrom(input.GetType())) {
				return input;
			}

			try {
				// 尝试直接转换
				return Convert.ChangeType(input, type) ?? defaultValue;
			} catch {
				try {
					// 如果直接转换失败，尝试通过 JSON 序列化和反序列化
					return input.ToJson().FromJson(type);
				} catch (Exception) {
					return defaultValue;
				}
			}
		}

		/// <summary>数据类型转换</summary>
		/// <typeparam name="T">类型</typeparam>
		/// <param name="input">要转换的数据</param>
		/// <param name="defaultValue">转换失败的默认值</param>
		/// <remarks>尝试直接转换，如果失败则尝试通过 JSON 序列化和反序列化</remarks>
		public static T ChangeType<T>(object input, T defaultValue = default) {
			if (input is null) { return defaultValue; }

			// 如果输入已经是目标类型，直接返回
			if (input is T variable) { return variable; }

			var baseType = typeof(T);

			// 如果input的类型可以直接赋值给T，直接进行类型转换
			if (baseType.IsAssignableFrom(input.GetType())) {
				return (T) input;
			}

			try {
				// 尝试直接转换
				return (T) (Convert.ChangeType(input, baseType) ?? defaultValue);
			} catch {
				try {
					// 如果直接转换失败，尝试通过 JSON 序列化和反序列化
					var json = JsonSerializer.Serialize(input);
					return JsonSerializer.Deserialize<T>(json);
				} catch (Exception) {
					return defaultValue;
				}
			}
		}

		#endregion

		#region 转换为扁平的字典

		/// <summary>将对象转换为扁平的文本键值字典</summary>
		/// <param name="obj">要转换的对象</param>
		/// <param name="comparer">字符串比较器(键是否区分大小写)</param>
		public static Dictionary<string, string> ToFlatStringDictionary(this object obj, StringComparer comparer = null) {
			var result = new Dictionary<string, string>(comparer);

			var data = obj.ToFlatDictionary(comparer);
			if (data == null || data.Count == 0) { return result; }

			foreach (var item in data) {
				result[item.Key] = item.Value?.ToString();
			}

			return result;
		}

		/// <summary>将对象转换为扁平的文本键值字典</summary>
		/// <param name="obj">要转换的对象</param>
		public static T ToFlatStringDictionary<T>(this object obj) where T : IDictionary<string, string>, new() {
			var result = new T();

			var data = obj.ToFlatDictionary();
			if (data == null || data.Count == 0) { return result; }

			foreach (var item in data) {
				result[item.Key] = item.Value?.ToString();
			}

			return result;
		}

		/// <summary>将对象转换为扁平的字典</summary>
		/// <param name="obj">要转换的对象</param>
		/// <param name="comparer">字符串比较器(键是否区分大小写)</param>
		public static Dictionary<string, object> ToFlatDictionary(this object obj, StringComparer comparer = null) {
			var dictionary = new Dictionary<string, object>(comparer);
			ToFlatDictionary(obj, string.Empty, dictionary);
			return dictionary;
		}

		/// <summary>将对象转换为扁平的字典</summary>
		/// <param name="obj">要转换的对象</param>
		public static T ToFlatDictionary<T>(this object obj) where T : IDictionary<string, object>, new() {
			var dictionary = new T();
			ToFlatDictionary(obj, string.Empty, dictionary);
			return dictionary;
		}

		/// <summary>将对象转换为扁平的字典</summary>
		/// <typeparam name="T">文本键值对字典</typeparam>
		/// <param name="obj">要转换的对象</param>
		/// <param name="prefix">前缀</param>
		/// <param name="dictionary">转换后的字典</param>
		private static void ToFlatDictionary<T>(object obj, string prefix, T dictionary) where T : IDictionary<string, object> {
			if (obj == null || dictionary == null) {
				return;
			}

			var objType = obj.GetType();

			// 处理基本类型和字符串
			if (objType.IsPrimitive || obj is string || obj is DateTime || obj is decimal) {
				dictionary[prefix] = obj;
				return;
			}

			// 处理字典类型
			if (obj is IDictionary dict) {
				foreach (DictionaryEntry entry in dict) {
					var keyPrefix = string.IsNullOrEmpty(prefix) ? entry.Key.ToString() : $"{prefix}.{entry.Key}";
					ToFlatDictionary(entry.Value, keyPrefix, dictionary);
				}
				return;
			}

			// 处理集合和数组类型
			if (obj is IEnumerable list) {
				var index = 0;
				foreach (var item in list) {
					var itemPrefix = string.IsNullOrEmpty(prefix) ? $"[{index}]" : $"{prefix}[{index}]";
					ToFlatDictionary(item, itemPrefix, dictionary);
					index++;
				}
				return;
			}

			// 处理对象类型
			if (objType.IsExtendClass()) {
				foreach (var prop in objType.GetProperties()) {
					var value = prop.GetValue(obj, null);
					var propPrefix = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
					ToFlatDictionary(value, propPrefix, dictionary);
				}
			}

			// 其他不处理
			dictionary[prefix] = obj;
		}

		#endregion

		#region 转换为嵌套的对象

		/// <summary>简单路径解析，支持 . 和 [ ]</summary>
		/// <param name="path">要解析的路径</param>
		/// <remarks>
		/// 用 . 表示字典键
		/// 用 [ ] 如果内容为数值，则表示数组下标，如果为其他内容则表示字典键
		/// 对于 [] 内的内容，如果为负数则无效
		/// </remarks>
		private static List<(string key, int index)> ParsePath(this string path) {
			if (string.IsNullOrEmpty(path)) { return null; }

			var paths = path.Split(['.', '['], StringSplitOptions.RemoveEmptyEntries);
			var result = new List<(string key, int index)>();

			for (var i = 0; i < paths.Length; i++) {
				var key = paths[i];

				// 空值忽略
				if (string.IsNullOrEmpty(key)) { continue; }

				// 结尾为 ] 内容为整数表示数组下标
				if (key.EndsWith(']')) {
					key = key[..^1];

					// 如果为负数则无效
					if (int.TryParse(key, out var index)) {
						if (index >= 0) { result.Add((null, index)); }
						continue;
					}
				}

				// 其他为键
				result.Add((key, -1));
			}

			return result;
		}

		/// <summary>文本转值</summary>
		private static object StringToObject(this string input) {
			if (string.IsNullOrWhiteSpace(input)) {
				return input;
			}

			if (DateTime.TryParse(input, out var date)) { return date; }
			if (DateTimeOffset.TryParse(input, out var offset)) { return offset; }
			if (Guid.TryParse(input, out var guid)) { return guid; }

			if (byte.TryParse(input, out var b)) { return b; }
			if (short.TryParse(input, out var s)) { return s; }
			if (int.TryParse(input, out var i)) { return i; }
			if (long.TryParse(input, out var l)) { return l; }
			if (float.TryParse(input, out var f)) { return f; }
			if (double.TryParse(input, out var d)) { return d; }
			if (Int128.TryParse(input, out var bl)) { return bl; }
			if (decimal.TryParse(input, out var dec)) { return dec; }

			if (bool.TryParse(input, out var tf)) { return tf; }

			return input;
		}

		/// <summary>将扁平的字典转换为对象</summary>
		/// <param name="input"></param>
		/// <param name="convertValue">是否将值转换成最相近的类型，如将日期字符串转换成日期，数字字符串转成数字</param>
		/// <param name="comparer">字符串比较器(键是否区分大小写)</param>
		public static object FromFlatStringDictionary<T>(this T input, bool convertValue = true, StringComparer comparer = null) where T : IDictionary<string, string> {
			if (input == null || input.Count == 0) { return null; }

			var data = input.ToDictionary(x => x.Key, x => convertValue ? StringToObject(x.Value) : x.Value);
			return data.FromFlatDictionary(comparer);
		}

		/// <summary>将扁平的字典转换为对象</summary>
		/// <param name="input"></param>
		/// <param name="comparer">字符串比较器(键是否区分大小写)</param>
		public static object FromFlatDictionary<T>(this T input, StringComparer comparer = null) where T : IDictionary<string, object> {
			if (input == null || input.Count == 0) { return null; }

			// 分析集合字典数据，并更新值
			// value 要更新的值
			// force 强制更新值，否则只有当原始值为 null 时才更新
			static object update(ref object source, string key, int index, object value = null, bool force = false, StringComparer comparer = null) {
				// 集合
				if (index >= 0) {
					if (source is not List<object> list) { list = []; }
					while (list.Count <= index) { list.Add(null); }

					if (force || (value != null && list[index] == null)) { list[index] = value; }

					source = list;
					return list[index];
				}

				// 字典
				if (!string.IsNullOrEmpty(key)) {
					if (source is not Dictionary<string, object> dict) { dict = new Dictionary<string, object>(comparer); }
					if (!dict.TryGetValue(key, out var val)) {
						val = null;
						dict.Add(key, val);
					}

					if (force || (value != null && dict[key] == null)) { dict[key] = value; }
					source = dict;
					return dict[key];
				}

				return null;
			}

			// 分析所有键值关系
			object result = null;

			foreach (var kv in input) {
				var keys = kv.Key.ParsePath();
				var count = keys.Count;

				var first = true;
				object current = null;

				for (var i = 0; i < count; i++) {
					var (key, index) = keys[i];

					// 非最后一条则不处理结果
					// 判断下一个项目是否集合或者字典，赋值初始值
					object value = null;
					var force = false;

					if (i < count - 1) {
						var next = keys[i + 1];
						if (next.index >= 0) {
							value = new List<object>();
						}
						if (!string.IsNullOrEmpty(next.key)) {
							value = new Dictionary<string, object>(comparer);
						}
					} else {
						value = kv.Value;
						force = true;
					}

					// 根据首次键判断，结果为哪种类型
					if (first) {
						first = false;
						current = update(ref result, key, index, value, force, comparer);
					} else {
						current = update(ref current, key, index, value, force, comparer);
					}
				}
			}

			return result;
		}

		#endregion

	}
}
