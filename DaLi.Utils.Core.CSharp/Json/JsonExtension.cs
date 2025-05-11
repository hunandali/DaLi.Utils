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
 * 	JSON 扩展操作
 * 
 * 	name: JsonExtension
 * 	create: 2025-01-21
 * 	memo: JSON 扩展操作
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using DaLi.Utils.Helper;

namespace DaLi.Utils.Json {
	/// <summary>JSON 扩展操作</summary>
	public static class JsonExtension {

		#region === 相关操作 ===

		/// <summary>解析节点</summary>
		/// <param name="input">Json 节点</param>
		/// <param name="emptyNULL">空值是否用 null 替换</param>
		/// <param name="format">对值进行二次格式化的操作，仅针对字符串类型</param>
		/// <remarks>
		/// JsonValueKind 类型：
		/// 0:Undefined		1:Object		2:Array
		/// 3:String		4:Number		5:True
		/// 6:False			7:Null
		/// </remarks>
		public static object Parse(this JsonElement input, bool emptyNULL = false, Func<string, object> format = null) {
			switch (input.ValueKind) {
				// Case JsonValueKind.Undefined, JsonValueKind.Null

				case JsonValueKind.Object: {
						var data = input.EnumerateObject().Select(x => (x.Name, x.Value.Parse(emptyNULL, format))).Where(x => x.Item2 is not null || !emptyNULL).ToDictionary(x => x.Name, x => x.Item2);
						return !emptyNULL || data?.Count > 0 ? data : null;
					}

				case JsonValueKind.Array: {
						var data = input.EnumerateArray().Select(x => x.Parse(emptyNULL, format)).Where(x => x is not null || !emptyNULL).ToList();
						return !emptyNULL || data?.Count > 0 ? data : null;
					}

				case JsonValueKind.String: {
						// 令牌类型是 JSON 字符串。
						if (input.TryGetDateTime(out var d)) {
							return d;
						}

						if (input.TryGetDateTimeOffset(out var o)) {
							return o;
						}

						if (input.TryGetGuid(out var g)) {
							return g;
						}

						var s = input.GetString();
						return format is null ? s : format(s);
					}

				case JsonValueKind.Number: {
						if (input.TryGetByte(out var b)) {
							return b;
						}
						if (input.TryGetInt16(out var s)) {
							return s;
						}
						if (input.TryGetInt32(out var i)) {
							return i;
						}
						if (input.TryGetInt64(out var l)) {
							return l;
						}
						if (input.TryGetSingle(out var f)) {
							return f;
						}
						if (input.TryGetDouble(out var d)) {
							return d;
						}

						return input.GetDecimal();
					}

				case JsonValueKind.True: {
						return true;
					}

				case JsonValueKind.False: {
						return false;
					}
			}

			return null;
		}

		#endregion

		#region === 序列化 ===

		/// <summary>缓存 JSON 序列化选项</summary>
		private static Dictionary<string, JsonSerializerOptions> _JsonOptions;

		/// <summary>获取 JSON 序列化选项</summary>
		/// <param name="indented">是否缩进</param>
		/// <param name="camelCase">是否驼峰</param>
		/// <param name="skipNull">是否跳过空值</param>
		private static JsonSerializerOptions GetOption(bool indented = true, bool camelCase = true, bool skipNull = false) {
			_JsonOptions ??= [];

			var key = $"{(indented ? 1 : 0)}{(camelCase ? 1 : 0)}{(skipNull ? 1 : 0)}";

			if (_JsonOptions.TryGetValue(key, out var options)) {
				return options;
			}

			options = new JsonSerializerOptions() {
				PropertyNamingPolicy = camelCase ? JsonNamingPolicy.CamelCase : null,
				DictionaryKeyPolicy = camelCase ? JsonNamingPolicy.CamelCase : null,
				WriteIndented = indented,
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
				DefaultIgnoreCondition = skipNull ? JsonIgnoreCondition.WhenWritingNull : JsonIgnoreCondition.Never
			};

			_JsonOptions.Lock(() => _JsonOptions.TryAdd(key, options));

			return options;
		}

		/// <summary>序列化 JSON 对象</summary>
		/// <param name="input">需要序列化的对象</param>
		/// <param name="options">序列化选项</param>
		/// <param name="type">序列化类型</param>
		public static string ToJson(this object input, JsonSerializerOptions options, Type @type = null) {
			if (input is null) { return ""; }

			options ??= GetOption(true, true, false);
			type ??= input.GetType();

			return JsonSerializer.Serialize(input, type, options);
		}

		/// <summary>序列化 JSON 对象</summary>
		public static string ToJson(this object input, bool indented = true, bool camelCase = true, bool skipNull = false) => ToJson(input, GetOption(indented, camelCase, skipNull));

		/// <summary>序列化 JSON 对象</summary>
		/// <param name="input">要序列化的对象</param>
		/// <param name="indented">是否缩进</param>
		/// <param name="camelCase">是否驼峰</param>
		/// <param name="skipNull">是否跳过空值</param>
		public static string ToJson<T>(this T input, bool indented = true, bool camelCase = true, bool skipNull = false) => ToJson(input, GetOption(indented, camelCase, skipNull));

		/// <summary>序列化 JSON 对象，名称不使用引号</summary>
		/// <param name="input">要序列化的对象</param>
		/// <param name="indented">是否缩进</param>
		/// <param name="camelCase">是否驼峰</param>
		/// <param name="skipNull">是否跳过空值</param>
		public static string ToJsonNoQuote<T>(this T input, bool indented = true, bool camelCase = false, bool skipNull = false) {
			var Json = input.ToJson(indented, camelCase, skipNull);
			if (string.IsNullOrEmpty(Json)) { return ""; }

			// 使用正则表达式替换标签名称上的引号
			try {
				var pattern = @"""(\w+)""(\s*:\s*)";
				var replacement = "$1$2";
				var rge = new Regex(pattern);
				return rge.Replace(Json, replacement);
			} catch {
				return Json;
			}
		}

		#endregion

		#region === 反序列化 ===

		/// <summary>反序列化默认选项</summary>
		private static JsonSerializerOptions DeserializeDefaultOptions {
			get {
				var options = new JsonSerializerOptions() {
					PropertyNameCaseInsensitive = true,
					ReadCommentHandling = JsonCommentHandling.Skip,
					AllowTrailingCommas = true,
					NumberHandling = JsonNumberHandling.AllowReadingFromString
				};

				options.Converters.Add(new JsonObjectConverter());
				options.Converters.Add(new JsonBooleanConverter());
				options.Converters.Add(new JsonDateTimeConverter());

				return options;
			}
		}

		/// <summary>反序列化 JSON 对象</summary>
		/// <param name="input">JSON 字符串</param>
		/// <param name="type">类型</param>
		/// <param name="options">选项</param>
		public static object FromJson(this string input, Type @type, JsonSerializerOptions options = null) {
			if (string.IsNullOrWhiteSpace(input) || @type is null) { return null; }

			options ??= DeserializeDefaultOptions;

			try {
				return JsonSerializer.Deserialize(input, type, options);
			} catch {
				return null;
			}
		}

		/// <summary>反序列化 JSON 对象</summary>
		public static T FromJson<T>(this string input, JsonSerializerOptions options = null, T defaultValue = default) {
			if (string.IsNullOrWhiteSpace(input)) { return defaultValue; }

			options ??= DeserializeDefaultOptions;

			try {
				return JsonSerializer.Deserialize<T>(input, options) ?? defaultValue;
			} catch {
				return defaultValue;
			}
		}

		/// <summary>反序列化 JSON 对象</summary>
		/// <param name="input">Json 字符串</param>
		/// <param name="emptyNULL">空值是否用 null 替换</param>
		/// <param name="format">对值进行二次格式化的操作，仅针对字符串类型</param>
		/// <returns>解析为 Json 数据集合，根据 Json 内容可能为字典或者列表</returns>
		public static object FromJson(this string input, bool emptyNULL = false, Func<string, object> format = null) {
			var options = new JsonDocumentOptions() {
				AllowTrailingCommas = true,
				CommentHandling = JsonCommentHandling.Skip,
				MaxDepth = 64
			};

			object Ret = null;
			try {
				using var Doc = JsonDocument.Parse(input, options);
				Ret = Doc.RootElement.Parse(emptyNULL, format);
			} catch (Exception) {
			}

			return Ret;
		}

		#endregion

		#region === 常用格式反序列化 ===

		/// <summary>反序列化 JSON 对象为字典数据</summary>
		/// <param name="input">Json 字符串</param>
		/// <param name="emptyNULL">是否移除无效内容节点</param>
		/// <param name="format">对值进行二次格式化的操作，仅针对字符串类型</param>
		public static Dictionary<string, object> ToJsonDictionary(this string input, bool emptyNULL = false, Func<string, object> format = null) {
			var data = input.FromJson(emptyNULL, format);
			if (data is Dictionary<string, object> dict) {
				return dict;
			}
			return [];
		}

		/// <summary>反序列化 JSON 对象为集合列表数据</summary>
		/// <param name="input">Json 字符串</param>
		/// <param name="emptyNULL">是否移除无效内容节点</param>
		/// <param name="format">对值进行二次格式化的操作，仅针对字符串类型</param>
		public static List<object> ToJsonList(this string input, bool emptyNULL = false, Func<string, object> format = null) {
			var data = input.FromJson(emptyNULL, format);
			if (data is List<object> list) {
				return list;
			}
			return [];
		}

		#endregion

		/// <summary>将 JSON 字符串转换为扁平化字典</summary>
		/// <param name="input"></param>
		/// <param name="emptyNULL">移除空内容</param>
		/// <param name="comparer">字符串比较器(键是否区分大小写)</param>
		public static Dictionary<string, object> ToFlatDictionary(this string input, bool emptyNULL = false, StringComparer comparer = null) {
			var data = input.ToJsonDictionary(emptyNULL);
			return data?.ToFlatDictionary(comparer);
		}

		/// <summary>将 JSON 字符串转换为扁平化字典</summary>
		/// <param name="input"></param>
		/// <param name="emptyNULL">移除空内容</param>
		/// <param name="comparer">字符串比较器(键是否区分大小写)</param>
		public static Dictionary<string, string> ToFlatStringDictionary(this string input, bool emptyNULL = false, StringComparer comparer = null) {
			var data = input.ToJsonDictionary(emptyNULL);
			return data?.ToFlatStringDictionary(comparer);
		}
	}
}
