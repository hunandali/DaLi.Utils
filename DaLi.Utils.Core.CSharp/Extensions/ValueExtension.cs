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
 * 	值相关的扩展操作
 * 
 * 	name: ValueExtension
 * 	create: 2025-03-05
 * 	memo: 值相关的扩展操作
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DaLi.Utils.Extension {

	/// <summary>值相关的扩展操作</summary>
	public static partial class ValueExtension {

		#region STRING

		/// <summary>是否为 null 或空字符串</summary>
		public static bool IsNull(this string input) => string.IsNullOrEmpty(input);

		/// <summary>是否不为 null 或空字符串</summary>
		public static bool NotNull(this string input) => !input.IsNull();

		/// <summary>是否为 null、空字符串或纯空格</summary>
		public static bool IsEmpty(this string input) => string.IsNullOrWhiteSpace(input);

		/// <summary>是否非空且包含非空格内容</summary>
		public static bool NotEmpty(this string input) => !input.IsEmpty();

		/// <summary>字符串等效性验证（支持大小写敏感模式）</summary>
		/// <param name="input"></param>
		/// <param name="target">对比字符串</param>
		/// <param name="ignoreCase">是否检查大小写（默认不检查）</param>
		public static bool IsSame(this string input, string target, bool ignoreCase = true) {
			if (input == null) { return target == null; }
			if (input == string.Empty) { return target == string.Empty; }

			return ignoreCase
				? input.Equals(target, StringComparison.OrdinalIgnoreCase)
				: input.Equals(target, StringComparison.Ordinal);
		}

		/// <summary>字符串等效性验证（支持大小写敏感模式）</summary>
		/// <param name="input"></param>
		/// <param name="values">用于匹配的内容，无数据时返回 false</param>
		/// <param name="all">是否每条都需要匹配，默认值需要其中一条匹配</param>
		/// <param name="ignoreCase">是否区分大小写，默认不区分</param>
		public static bool IsSame(this string input, IEnumerable<string> values, bool all = false, bool ignoreCase = true) {
			if (input == null) { return values.IsEmpty(); }

			var mathchs = values.Where(x => !string.IsNullOrEmpty(x)).Distinct();

			if (all) {
				return mathchs.All(x => input.IsSame(x, ignoreCase));
			} else {
				return mathchs.Any(x => input.IsSame(x, ignoreCase));
			}
		}

		/// <summary>数据是否存在指定内容，仅作简单比较</summary>
		/// <param name="input">原始内容</param>
		/// <param name="value">用于匹配的内容</param>
		/// <param name="ignoreCase">是否区分大小写，默认不区分</param>
		/// <remarks>
		/// 1. 如果原始内容与匹配内容完全相同则返回 true（原始为 null 且匹配为 null；原始为空字符串且匹配为空字符串）；<para />
		/// 2. 括号包含的内容则使用正则表达式匹配；<para />
		/// 3. 使用星号 * 作为通配符，匹配任何内容；<para />
		/// 4. 使用星号 *xxx* 作为通配符，匹配任何包含 xxx 的内容;<para />
		/// 5. 使用星号 *xxx*yyy* 作为通配符，匹配任何包含 xxx 开头，yyy 结尾的内容;<para />
		/// 6. 使用星号 xxx* 作为通配符，匹配任何以 xxx 开头的内容;<para />
		/// 7. 使用星号 *xxx 作为通配符，匹配任何以 xxx 结尾的内容;<para />
		/// 8. 使用星号 xxx*yyy 作为通配符，匹配任何以 xxx 开头，yyy 结尾的内容;<para />
		/// 9. 都不匹配时返回 false。
		/// </remarks>
		public static bool IsLike(this string input, string value, bool ignoreCase = true) {
			if (input == null) { return value == null; }
			if (input == string.Empty) { return value == string.Empty; }

			var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
			var options = ignoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;

			// 检查是否完全相等
			if (input.Equals(value, comparison)) { return true; }

			// * 表示 全匹配
			if (value == "*") { return true; }

			// *xxxx* 表示 数据中存在 xxxx；
			if (value.StartsWith('*') && value.EndsWith('*')) {
				var pattern = value.Trim('*');
				if (string.IsNullOrEmpty(pattern)) { return true; }

				// *xxxx*yyyy* 表示 数据中存在 xxxx开头，yyyy结尾的数据；
				if (pattern.Contains('*')) {
					// 包含区域内容其实
					pattern = Regex.Escape(pattern).Replace(@"\*", "((.|\n)*?)");
					return Regex.IsMatch(input, pattern, options);
				} else {
					return input.Contains(pattern, comparison);
				}
			}

			// *xxxx 表示 以 xxxx 结尾数据；
			if (value.StartsWith('*') && !value.EndsWith('*')) {
				return input.EndsWith(value[1..], comparison);
			}

			// xxxx* 表示 以 xxxx 开头的数据；
			if (!value.StartsWith('*') && value.EndsWith('*')) {
				return input.StartsWith(value[..^1], comparison);
			}

			// 使用圆括号起始则使用正则表达式
			if (value.StartsWith('(') && value.EndsWith(')')) {
				var pattern = value[1..^1];
				if (string.IsNullOrEmpty(pattern)) { return false; }
				return Regex.IsMatch(input, pattern, options);
			}

			// xxxx*yyyy 表示 以 xxxx 开头，yyyy 结尾的数据；
			if (value.Contains('*')) {
				var Vs = value.Split('*');
				if (Vs.Length == 2) {
					return input.StartsWith(Vs[0], comparison) && input.EndsWith(Vs[1], comparison);
				}
			}

			return false;
		}

		/// <summary>数据是否存在指定内容，仅作简单比较</summary>
		/// <param name="input">原始内容</param>
		/// <param name="values">用于匹配的内容，无数据时返回 false</param>
		/// <param name="all">是否每条都需要匹配，默认值需要其中一条匹配</param>
		/// <param name="ignoreCase">是否区分大小写，默认不区分</param>
		/// <remarks>
		/// 1. 如果原始内容与匹配内容完全相同则返回 true（原始为 null 且匹配为 null；原始为空字符串且匹配为空字符串）；<para />
		/// 2. 括号包含的内容则使用正则表达式匹配；<para />
		/// 3. 使用星号 * 作为通配符，匹配任何内容；<para />
		/// 4. 使用星号 *xxx* 作为通配符，匹配任何包含 xxx 的内容;<para />
		/// 5. 使用星号 *xxx*yyy* 作为通配符，匹配任何包含 xxx 开头，yyy 结尾的内容;<para />
		/// 6. 使用星号 xxx* 作为通配符，匹配任何以 xxx 开头的内容;<para />
		/// 7. 使用星号 *xxx 作为通配符，匹配任何以 xxx 结尾的内容;<para />
		/// 8. 使用星号 xxx*yyy 作为通配符，匹配任何以 xxx 开头，yyy 结尾的内容;<para />
		/// 9. 都不匹配时返回 false。
		/// </remarks>
		public static bool IsLike(this string input, IEnumerable<string> values, bool all = false, bool ignoreCase = true) {
			if (string.IsNullOrEmpty(input) || values is null || !values.Any()) {
				return false;
			}

			var mathchs = values.Where(x => !string.IsNullOrEmpty(x)).Distinct();

			if (all) {
				return mathchs.All(x => input.IsLike(x, ignoreCase));
			} else {
				return mathchs.Any(x => input.IsLike(x, ignoreCase));
			}
		}

		/// <summary>是否匹配正则表达式</summary>
		/// <param name="input">原始内容</param>
		/// <param name="pattern">正则表达式</param>
		/// <param name="options">匹配选项，默认忽略大小写</param>
		public static bool IsMatch(this string input, string pattern, RegexOptions options = RegexOptions.IgnoreCase) => !string.IsNullOrEmpty(input) && !string.IsNullOrEmpty(pattern) && Regex.IsMatch(input, pattern, options);

		#endregion

		#region NULLABLE

		/// <summary>是否为 null、空字符串</summary>
		public static bool IsEmpty<T>(this T? input) where T : struct => !input.HasValue;

		/// <summary>是否非空且包含非空格内容</summary>
		public static bool NotEmpty<T>(this T? input) where T : struct => input.HasValue;

		#endregion

		#region COLLECTION

		///// <summary>数组是否为空</summary>
		///// <param name="input">数组</param>
		//public static bool IsEmpty<T>(this T[] input) => input is null || input.Length < 1;

		/// <summary>数组是否为空</summary>
		/// <param name="input">数组</param>
		public static bool IsEmpty(this Array input) => input is null || input.Length < 1;

		/// <summary>集合是否为空</summary>
		/// <param name="input">数组</param>
		public static bool IsEmpty(this ICollection input) => input is null || input.Count < 1;

		/// <summary>集合是否为空</summary>
		/// <param name="input">数组</param>
		public static bool IsEmpty(this IEnumerable input) => input is null || !input.GetEnumerator().MoveNext();

		///// <summary>集合是否为空</summary>
		///// <param name="input">数组</param>
		//public static bool IsEmpty<T>(this IEnumerable<T> input) => input is null || !input.Any();

		///// <summary>列表存在数据</summary>
		///// <param name="input">数组</param>
		//public static bool NotEmpty<T>(this T[] input) => input is not null && input.Length > 0;

		/// <summary>数组存在数据</summary>
		/// <param name="input">数组</param>
		public static bool NotEmpty(this Array input) => input is not null && input.Length > 0;

		/// <summary>集合存在数据</summary>
		/// <param name="input">数组</param>
		public static bool NotEmpty(this ICollection input) => input is not null && input.Count > 0;

		/// <summary>集合存在数据</summary>
		/// <param name="input">数组</param>
		public static bool NotEmpty(this IEnumerable input) => input is not null && input.GetEnumerator().MoveNext();

		///// <summary>集合存在数据</summary>
		///// <param name="input">数组</param>
		//public static bool NotEmpty<T>(this IEnumerable<T> input) => input is not null && input.Any();

		/// <summary>获取集合的数量长度</summary>
		public static int Count(this IEnumerable input) {
			if (input is null) { return 0; }

			var enumerator = input.GetEnumerator();
			var count = 0;
			while (enumerator.MoveNext()) {
				count++;
			}
			return count;
		}

		#endregion

		#region DICTIONARY

		/// <summary>集合是否为空</summary>
		/// <param name="input">数组</param>
		public static bool IsEmpty(this IDictionary input) => input is null || input.Count < 1;

		///// <summary>集合是否为空</summary>
		///// <param name="input">数组</param>
		//public static bool IsEmpty<TKey, TValue>(this IDictionary<TKey, TValue> input) => input is null || input.Count < 1;

		/// <summary>集合存在数据</summary>
		/// <param name="input">数组</param>
		public static bool NotEmpty(this IDictionary input) => input is not null && input.Count > 0;

		///// <summary>集合存在数据</summary>
		///// <param name="input">数组</param>
		//public static bool NotEmpty<TKey, TValue>(this IDictionary<TKey, TValue> input) => input is not null && input.Count > 0;

		#endregion

		#region 公用操作

		/// <summary>判断当前数据是否无内容</summary>
		/// <param name="value">要判断的对象</param>
		/// <returns>如果对象为空或具有默认值则返回true，否则返回false</returns>
		/// <remarks>
		/// 针对不同类型的判断标准如下：
		/// - 文本：空值或空字符串
		/// - 数字：0
		/// - 布尔：False
		/// - 对象、字典：无任何键
		/// - 数组、集合：长度为0
		/// - GUID：空值
		/// - 时间：初始时间
		/// - 其他：空值
		/// </remarks>
		public static bool IsEmptyValue(this object value) {
			if (value is null) {
				return true;
			}

			// 常用数据类型判断
			if (value.Equals(string.Empty) ||
					value.Equals(0) ||
					value.Equals(false) ||
					value.Equals(DateTime.MinValue) ||
					value.Equals(TimeSpan.Zero) ||
					value.Equals(DateTimeOffset.MinValue) ||
					value.Equals(Guid.Empty)) {
				return true;
			}

			// 处理字符串类型
			if (value is string str) {
				return string.IsNullOrEmpty(str);
			}

			// 处理值类型
			var type = value.GetType();
			if (type.IsValueType) {
				if (type.IsEnum) {
					return Convert.ToInt32(value) == 0;
				}

				if (Nullable.GetUnderlyingType(type) is Type underlyingType) {
					return !((dynamic) value).HasValue;
				}

				return value.Equals(Activator.CreateInstance(type));
			}

			// 处理集合类型
			if (value is ICollection coll) {
				return coll.Count == 0;
			}

			if (value is IDictionary dict) {
				return dict.Count == 0;
			}

			if (value is IEnumerable list) {
				return !list.GetEnumerator().MoveNext();
			}

			if (value is Array arr) {
				return arr.Length == 0;
			}

			// 处理元组类型
			var typeName = type.Name;
			if (typeName.StartsWith("Tuple") || typeName.StartsWith("ValueTuple")) {
				return type.GetFields().All(f => IsEmptyValue(f.GetValue(value)));
			}

			// 处理特殊类型
			switch (value) {
				case MemoryStream ms:
					return ms.Length == 0;
				case Regex regex:
					return regex.ToString() == "(?:)";
				case Uri uri:
					return string.IsNullOrEmpty(uri.ToString());
				case Version version:
					return version.Equals(new Version(0, 0));
				case IPAddress ip:
					return ip.Equals(IPAddress.Any);
			}

			// 处理其他类型：检查所有公共属性
			try {
				return type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
						   .All(p => IsEmptyValue(p.GetValue(value)));
			} catch {
				return false;
			}
		}

		/// <summary>
		/// 判断当前数据是否无内容，支持自定义验证逻辑
		/// </summary>
		/// <typeparam name="T">对象类型</typeparam>
		/// <param name="value">要判断的对象</param>
		/// <param name="validate">自定义验证函数，返回true则认为无内容</param>
		/// <returns>如果对象为空或验证函数返回true则返回true，否则返回false</returns>
		public static bool IsEmptyValue<T>(this T value, Func<T, bool> validate = null) => validate?.Invoke(value) ?? IsEmptyValue((object) value);

		/// <summary>
		/// 获取指定类型的默认值
		/// </summary>
		/// <param name="type">类型</param>
		/// <param name="defaultValue">当无法创建默认值时的替代值</param>
		/// <returns>类型的默认值或指定的替代值</returns>
		/// <remarks>
		/// 对于值类型和字符串类型，始终存在默认值，此时defaultValue参数无效
		/// </remarks>
		public static object GetDefaultValue(this Type type, object defaultValue = null) {
			if (type is null) {
				return defaultValue;
			}

			// 处理值类型
			if (type.IsValueType) {
				// 处理可空类型
				var underlyingType = Nullable.GetUnderlyingType(type);
				if (underlyingType is not null) {
					return defaultValue;
				}

				// 处理枚举类型
				if (type.IsEnum) {
					return Enum.ToObject(type, 0);
				}

				// 处理其他值类型
				return Activator.CreateInstance(type);
			}

			// 处理字符串类型
			if (type == typeof(string)) {
				return string.Empty;
			}

			// 处理数组类型
			if (type.IsArray) {
				return Array.CreateInstance(type.GetElementType(), 0);
			}

			// 处理其他引用类型
			try {
				return Activator.CreateInstance(type) ?? defaultValue;
			} catch {
				return defaultValue;
			}
		}

		/// <summary>
		/// 获取指定类型的默认值
		/// </summary>
		/// <typeparam name="T">类型</typeparam>
		/// <param name="defaultValue">当无法创建默认值时的替代值</param>
		/// <returns>类型的默认值或指定的替代值</returns>
		public static T GetDefaultValue<T>(T defaultValue = default) => (T) GetDefaultValue(typeof(T), defaultValue);

		/// <summary>
		/// 获取常用系统数据类型的默认值
		/// </summary>
		/// <param name="typeCode">类型代码</param>
		/// <returns>对应类型的默认值</returns>
		public static object GetDefaultValue(this TypeCode typeCode) => typeCode switch {
			TypeCode.Boolean => false,
			TypeCode.Char => (char) 0,
			TypeCode.SByte => (sbyte) 0,
			TypeCode.Byte => (byte) 0,
			TypeCode.Int16 => (short) 0,
			TypeCode.UInt16 => (ushort) 0,
			TypeCode.Int32 => 0,
			TypeCode.UInt32 => 0U,
			TypeCode.Int64 => 0L,
			TypeCode.UInt64 => 0UL,
			TypeCode.Single => 0F,
			TypeCode.Double => 0D,
			TypeCode.Decimal => 0M,
			TypeCode.DateTime => DateTime.MinValue,
			TypeCode.String => string.Empty,
			_ => null
		};

		#endregion

	}
}
