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
 * 	对象属性操作
 * 
 * 	name: ObjectAccessorHelper
 * 	create: 2025-01-20
 * 	memo: 提供了强大的深度访问功能，使用点号表示法路径，路径访问类型：属性访问（user.name）、索引访问（user[0]）、字典键访问（data ['key']）、选择器访问（items [?(@.type == 'important')]）等
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace DaLi.Utils.Helper {
	/// <summary>对象属性操作 - 支持通过路径访问对象的深层属性</summary>
	/// <remarks>支持属性访问（user.name）、索引访问（user[0]）、字典键访问（data ['key']）、选择器访问（items [?(@.type == 'important')]）等；仅针对字典、集合、动态对象、通用类等支持</remarks>
	public partial class ObjectAccessorHelper {

		/// <summary>属性信息缓存</summary>
		/// <remarks>使用元组(类型,属性名,是否忽略大小写)作为键，提高反射性能</remarks>
		private static readonly ConcurrentDictionary<(Type type, string propertyName, bool ignoreCase), PropertyInfo> _PropertyCache = new();

		/// <summary>字段信息缓存</summary>
		/// <remarks>使用元组(类型,字段名,是否忽略大小写)作为键，提高反射性能</remarks>
		private static readonly ConcurrentDictionary<(Type type, string fieldName, bool ignoreCase), FieldInfo> _FieldCache = new();

		/// <summary>路径解析结果缓存</summary>
		/// <remarks>缓存常用路径的解析结果，避免重复的正则表达式匹配操作</remarks>
		private static readonly ConcurrentDictionary<string, IReadOnlyList<PathSegment>> _PathCache = new(StringComparer.Ordinal);

		/// <summary>选择器条件缓存</summary>
		/// <remarks>缓存选择器条件的解析结果，提高选择器解析性能</remarks>
		private static readonly ConcurrentDictionary<string, (string Path, string Operator, string Value)> _SelectorCache = new();

		/// <summary>选择器操作符列表</summary>
		private static readonly string[] _SelectorOperators = ["==", "!=", ">", "<", ">=", "<="];

		/// <summary>路径段类型</summary>
		public enum PathSegmentEnum {
			/// <summary>属性</summary>
			PROPERTY,

			/// <summary>索引</summary>
			INDEX,

			/// <summary>键</summary>
			KEY,

			/// <summary>选择器</summary>
			SELECTOR
		}

		/// <summary>路径段</summary>
		public record PathSegment(PathSegmentEnum Type, string Value);

		/// <summary>路径匹配模式</summary>
		private const string PATH_PATTERN = @"(?<property>[^.\[\]]+)|" +
										  @"\[(?<index>\d+)\]|" +
										  @"\[\'(?<key>[^']+)\'\]|" +
										  @"\[(?<selector>[^]]+)\]";

		/// <summary>路径匹配正则</summary>
		[GeneratedRegex(PATH_PATTERN, RegexOptions.IgnoreCase, "zh-CN")]
		private static partial Regex PathRegex();

		/// <summary>获取指定路径的值</summary>
		/// <typeparam name="T">返回值类型</typeparam>
		/// <param name="source">源对象</param>
		/// <param name="path">访问路径</param>
		/// <param name="defaultValue">默认值</param>
		/// <param name="ignoreCase">忽略大小写</param>
		/// <returns>转换后的指定类型值</returns>
		public static T GetValue<T>(object source, string path, T defaultValue = default, bool ignoreCase = true) {
			var value = GetValue(source, path, ignoreCase);
			return ConvertHelper.ChangeType(value, defaultValue);
		}

		/// <summary>获取指定路径的值</summary>
		/// <param name="source">源对象</param>
		/// <param name="path">访问路径</param>
		/// <param name="ignoreCase">是否忽略大小写</param>
		/// <returns>获取的值</returns>
		/// <remarks>使用迭代代替递归，减少中间对象创建，提高性能</remarks>
		public static object GetValue(object source, string path, bool ignoreCase = true) {
			// 快速路径：空检查
			if (string.IsNullOrEmpty(path) || source is null) {
				return source;
			}

			// 获取解析后的路径段
			var segments = ParsePath(path);
			if (segments.Count == 0) {
				return source;
			}

			// 使用迭代代替递归，减少栈帧创建
			var current = source;
			foreach (var segment in segments) {
				current = GetSegmentValue(current, segment, ignoreCase);
				if (current == null) {
					break; // 提前退出，避免不必要的计算
				}
			}

			return current;
		}

		/// <summary>设置指定路径的值</summary>
		public static bool SetValue(object target, string path, object value, bool ignoreCase = true) {
			try {
				if (string.IsNullOrEmpty(path)) {
					return false;
				}

				var segments = ParsePath(path).ToList();
				if (segments.Count == 0) { return false; }

				var lastSegment = segments.Last();
				segments.RemoveAt(segments.Count - 1);

				var current = segments.Aggregate(target, (target, segment) => EnsureSegmentPath(target, segment, ignoreCase));
				if (current is null) { return false; }

				SetSegmentValue(current, lastSegment, value, ignoreCase);
				return true;
			} catch {
				return false;
			}
		}

		/// <summary>解析路径</summary>
		/// <param name="path">要解析的路径</param>
		/// <returns>解析后的路径段列表</returns>
		/// <remarks>使用缓存提高频繁使用的路径解析性能</remarks>
		public static IReadOnlyList<PathSegment> ParsePath(string path) {
			// 空路径检查
			if (string.IsNullOrEmpty(path)) {
				return [];
			}

			// 尝试从缓存获取
			if (_PathCache.TryGetValue(path, out var cachedSegments)) {
				return cachedSegments;
			}

			// 缓存未命中，执行解析
			var result = new List<PathSegment>();
			var matches = PathRegex().Matches(path);

			foreach (Match match in matches) {
				if (match.Groups["property"].Success) {
					result.Add(new(PathSegmentEnum.PROPERTY, match.Groups["property"].Value));

				} else if (match.Groups["index"].Success) {
					result.Add(new(PathSegmentEnum.INDEX, match.Groups["index"].Value));

				} else if (match.Groups["key"].Success) {
					result.Add(new(PathSegmentEnum.KEY, match.Groups["key"].Value));

				} else if (match.Groups["selector"].Success) {
					result.Add(new(PathSegmentEnum.SELECTOR, match.Groups["selector"].Value));
				}
			}

			// 转换为不可变数组，确保线程安全
			var resultArray = result.ToArray();

			// 添加到缓存，限制缓存大小防止内存泄漏
			if (_PathCache.Count < 1000) {
				_PathCache.TryAdd(path, resultArray);
			}

			return resultArray;
		}

		#region 获取值

		/// <summary>获取指定路径的值</summary>
		/// <param name="source">源对象</param>
		/// <param name="segment">路径段</param>
		/// <param name="ignoreCase">是否忽略大小写</param>
		/// <returns>获取的值</returns>
		private static object GetSegmentValue(object source, PathSegment segment, bool ignoreCase = true) {
			if (source == null || string.IsNullOrEmpty(segment.Value)) {
				return null;
			}

			return segment.Type switch {
				PathSegmentEnum.PROPERTY => GetPropertyValue(source, segment.Value, ignoreCase),
				PathSegmentEnum.INDEX => GetIndexValue(source, int.Parse(segment.Value)),
				PathSegmentEnum.KEY => GetKeyValue(source, segment.Value, ignoreCase),
				PathSegmentEnum.SELECTOR => GetSelectorValue(source, segment.Value, ignoreCase),
				_ => null
			};
		}

		/// <summary>获取属性值</summary>
		/// <param name="source">原始数据</param>
		/// <param name="propertyName">属性名</param>
		/// <param name="ignoreCase">是否忽略大小写</param>
		/// <remarks>字典数据则使用键查询，对象则使用反射</remarks>
		private static object GetPropertyValue(object source, string propertyName, bool ignoreCase = true) => source switch {
			DynamicObject dyn => GetDynamicValue(dyn, propertyName, ignoreCase),
			IDictionary<string, object> dict => GetDictionaryValue(dict, propertyName, ignoreCase),
			IDictionary dict => GetDictionaryValue(dict, propertyName, ignoreCase),
			_ => GetReflectionPropertyValue(source, propertyName, ignoreCase)
		};

		/// <summary>获取动态属性值</summary>
		/// <param name="source">原始数据</param>
		/// <param name="propertyName">属性名</param>
		/// <param name="ignoreCase">是否忽略大小写</param>
		private static object GetDynamicValue(IDynamicMetaObjectProvider source, string propertyName, bool ignoreCase = true) {
			if (source is DynamicObject dyn) {
				if (dyn.TryGetMember(new DynamicGetMemberBinder(propertyName, ignoreCase), out var value)) {
					return value;
				}
			}
			return GetReflectionPropertyValue(source, propertyName);
		}

		/// <summary>获取反射属性值</summary>
		/// <param name="source">原始数据</param>
		/// <param name="propertyName">属性名</param>
		/// <param name="ignoreCase">是否忽略大小写</param>
		/// <returns>获取的属性值</returns>
		/// <remarks>使用缓存提高反射性能，避免重复查找相同的属性或字段</remarks>
		private static object GetReflectionPropertyValue(object source, string propertyName, bool ignoreCase = true) {
			if (source == null || string.IsNullOrEmpty(propertyName)) {
				return null;
			}

			var type = source.GetType();

			// 尝试从缓存获取属性
			var prop = _PropertyCache.GetOrAdd((type, propertyName, ignoreCase), key => {
				var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

				if (key.ignoreCase) {
					bindingFlags |= BindingFlags.IgnoreCase;
				}

				return key.type.GetProperty(key.propertyName, bindingFlags);
			});

			if (prop != null) {
				try {
					return prop.GetValue(source);
				} catch {
					// 属性访问异常时返回null
					return null;
				}
			}

			// 尝试从缓存获取字段
			var field = _FieldCache.GetOrAdd((type, propertyName, ignoreCase), key => {
				var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
				if (key.ignoreCase) {
					bindingFlags |= BindingFlags.IgnoreCase;
				}

				return key.type.GetField(key.fieldName, bindingFlags);
			});

			if (field != null) {
				try {
					return field.GetValue(source);
				} catch {
					// 字段访问异常时返回null
					return null;
				}
			}

			return null;
		}

		/// <summary>获取索引值</summary>
		/// <param name="source">原始数据</param>
		/// <param name="index">索引</param>
		/// <returns>获取的索引值</returns>
		/// <remarks>优化迭代器访问，减少LINQ操作和对象创建</remarks>
		private static object GetIndexValue(object source, int index) {
			// 快速路径：空检查和边界检查
			if (source == null || index < 0) {
				return null;
			}

			// 使用模式匹配代替 switch 表达式，更清晰的逻辑
			if (source is System.Array array) {
				return index < array.Length ? array.GetValue(index) : null;
			}

			if (source is IList list) {
				return index < list.Count ? list[index] : null;
			}

			// 优化：使用迭代器代替LINQ，避免创建中间集合
			if (source is IEnumerable enumerable) {
				var currentIndex = 0;
				foreach (var item in enumerable) {
					if (currentIndex == index) {
						return item;
					}

					currentIndex++;
					// 提前退出，避免不必要的迭代
					if (currentIndex > index) {
						break;
					}
				}
			}

			return null;
		}

		/// <summary>获取键值</summary>
		/// <param name="source">原始字典数据</param>
		/// <param name="key">查询的键</param>
		/// <param name="ignoreCase">是否忽略大小写</param>
		/// <returns>获取的值</returns>
		private static object GetKeyValue(object source, string key, bool ignoreCase = true) => source switch {
			IDictionary<string, object> dict => GetDictionaryValue(dict, key, ignoreCase),
			IDictionary dict => GetDictionaryValue(dict, key, ignoreCase),
			_ => null
		};

		/// <summary>从字典中获取值（支持忽略大小写）</summary>
		/// <param name="dict">字典对象</param>
		/// <param name="key">键</param>
		/// <param name="ignoreCase">是否忽略大小写</param>
		/// <returns>获取的值</returns>
		/// <remarks>优化忽略大小写的查找算法，减少全字典遍历</remarks>
		private static object GetDictionaryValue(IDictionary<string, object> dict, string key, bool ignoreCase = true) {
			// 快速路径：空检查
			if (dict == null || string.IsNullOrEmpty(key)) {
				return null;
			}

			// 如果直接匹配成功，直接返回
			if (dict.TryGetValue(key, out var value)) {
				return value;
			}

			// 默认比较器
			var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

			// 遍历
			foreach (var pair in dict) {
				if (string.Equals(pair.Key, key, comparison)) {
					return pair.Value;
				}
			}

			return null;
		}

		/// <summary>从字典中获取值（支持忽略大小写）</summary>
		/// <param name="dict">字典对象</param>
		/// <param name="key">键</param>
		/// <param name="ignoreCase">是否忽略大小写</param>
		/// <returns>获取的值</returns>
		/// <remarks>优化忽略大小写的查找算法，减少全字典遍历</remarks>
		private static object GetDictionaryValue(IDictionary dict, string key, bool ignoreCase = true) {
			// 快速路径：空检查
			if (dict == null || string.IsNullOrEmpty(key)) {
				return null;
			}

			// 尝试直接获取
			if (dict.Contains(key)) {
				return dict[key];
			}

			// 默认比较器
			var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

			// 遍历
			foreach (var dictKey in dict.Keys) {
				if (dictKey is string strKey && string.Equals(strKey, key, comparison)) {
					return dict[dictKey];
				}
			}

			return null;
		}

		/// <summary>获取选择器值</summary>
		/// <param name="source">原始数据</param>
		/// <param name="selector">选择器</param>
		/// <param name="ignoreCase">是否忽略大小写</param>
		/// <returns>选择器过滤后的结果</returns>
		/// <remarks>优化类型转换和集合创建，减少内存分配</remarks>
		private static object GetSelectorValue(object source, string selector, bool ignoreCase = true) {
			// 快速路径：空检查
			if (source == null || string.IsNullOrEmpty(selector)) {
				return null;
			}

			// 类型检查
			if (source is not IEnumerable enumerable) {
				return null;
			}

			// 优化：避免不必要的 Cast<object>() 操作
			// 如果已经是 IEnumerable<object>，直接使用
			IEnumerable<object> items;
			if (source is IEnumerable<object> objectItems) {
				items = objectItems;
			} else {
				items = enumerable.Cast<object>();
			}

			return selector[0] switch {
				'?' => EvaluateSelector(items, selector[1..], ignoreCase),
				'@' => items.Select(item => GetValue(item, selector[1..], ignoreCase)),
				_ => null
			};
		}

		/// <summary>解析选择器</summary>
		/// <param name="items">集合项</param>
		/// <param name="condition">条件表达式</param>
		/// <param name="ignoreCase">是否忽略大小写</param>
		/// <returns>过滤后的集合</returns>
		/// <remarks>使用缓存提高选择器解析性能，优化字符串操作</remarks>
		private static object EvaluateSelector(IEnumerable<object> items, string condition, bool ignoreCase = true) {
			// 快速路径：空检查
			if (string.IsNullOrEmpty(condition) || items == null) {
				return items;
			}

			// 尝试从缓存获取解析结果
			var (Path, Operator, Value) = _SelectorCache.GetOrAdd(condition, key => {
				string foundOperator = null;
				var operatorIndex = -1;

				// 查找操作符
				foreach (var op in _SelectorOperators) {
					var index = key.IndexOf(op, StringComparison.Ordinal);
					if (index >= 0 && (operatorIndex == -1 || index < operatorIndex)) {
						operatorIndex = index;
						foundOperator = op;
					}
				}

				if (operatorIndex == -1) {
					return (key, null, null);
				}

				var path = key[..operatorIndex].Trim();
				var value = key[(operatorIndex + foundOperator.Length)..].Trim('\'', '"');

				return (path, foundOperator, value);
			});

			// 如果没有找到有效的操作符，返回原始集合
			if (Operator == null) {
				return items;
			}

			var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
			var path = Path;
			var value = Value;
			var op = Operator;

			// 使用延迟执行和提前退出优化过滤操作
			return items.Where(item => {
				var itemValue = GetValue(item, path, ignoreCase)?.ToString();
				if (itemValue == null) {
					return false;
				}

				return op switch {
					"==" => string.Equals(itemValue, value, comparison),
					"!=" => !string.Equals(itemValue, value, comparison),
					">" => string.Compare(itemValue, value, comparison) > 0,
					"<" => string.Compare(itemValue, value, comparison) < 0,
					">=" => string.Compare(itemValue, value, comparison) >= 0,
					"<=" => string.Compare(itemValue, value, comparison) <= 0,
					_ => false
				};
			});
		}

		#endregion

		#region 设置值

		/// <summary>迭代路径</summary>
		/// <param name="target">目标数据</param>
		/// <param name="segment">路径</param>
		/// <param name="ignoreCase">是否忽略大小写</param>
		/// <returns>路径对应的对象，如果不存在则创建</returns>
		/// <remarks>优化路径创建，减少不必要的对象分配</remarks>
		private static object EnsureSegmentPath(object target, PathSegment segment, bool ignoreCase = true) {
			// 快速路径：空检查
			if (target == null || segment == null) {
				return null;
			}

			// 尝试获取现有值
			var current = GetSegmentValue(target, segment, ignoreCase);
			if (current != null) {
				return current;
			}

			// 根据路径类型创建适当的容器对象
			var comparer = ignoreCase ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

			object newValue = segment.Type switch {
				PathSegmentEnum.PROPERTY or PathSegmentEnum.KEY =>
					// 使用初始容量为4的字典，减少初始内存分配
					new Dictionary<string, object>(4, comparer),
				PathSegmentEnum.INDEX =>
					// 使用初始容量为4的列表，减少初始内存分配
					new List<object>(4),
				_ => null
			};

			// 设置新创建的值
			if (newValue != null) {
				SetSegmentValue(target, segment, newValue, ignoreCase);
			}

			return newValue;
		}

		/// <summary>设置指定路径的值</summary>
		/// <param name="target">目标数据</param>
		/// <param name="segment">路径</param>
		/// <param name="value">值</param>
		/// <param name="ignoreCase"></param>
		private static void SetSegmentValue(object target, PathSegment segment, object value, bool ignoreCase = true) {
			if (target is null) { return; }

			switch (segment.Type) {
				case PathSegmentEnum.PROPERTY:
					SetPropertyValue(target, segment.Value, value, ignoreCase);
					break;

				case PathSegmentEnum.INDEX:
					SetIndexValue(target, int.Parse(segment.Value), value);
					break;

				case PathSegmentEnum.KEY:
					SetKeyValue(target, segment.Value, value);
					break;
			}
		}

		/// <summary>设置属性值</summary>
		/// <param name="target">目标数据</param>
		/// <param name="propertyName">属性名</param>
		/// <param name="value">值</param>
		/// <param name="ignoreCase"></param>
		/// <remarks>使用缓存提高反射性能，避免重复查找相同的属性或字段</remarks>
		private static void SetPropertyValue(object target, string propertyName, object value, bool ignoreCase = true) {
			// 快速路径：空检查
			if (target == null || string.IsNullOrEmpty(propertyName)) { return; }

			// 特殊类型处理
			switch (target) {
				case DynamicObject dyn:
					dyn.TrySetMember(new DynamicSetMemberBinder(propertyName, ignoreCase), value);
					return;

				case IDictionary<string, object> dict:
					dict[propertyName] = value;
					return;

				case IDictionary dict:
					dict[propertyName] = value;
					return;
			}

			// 使用缓存获取属性信息
			var type = target.GetType();

			// 尝试从缓存获取属性
			var prop = _PropertyCache.GetOrAdd((type, propertyName, ignoreCase), key => {
				var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
				if (ignoreCase) {
					bindingFlags |= BindingFlags.IgnoreCase;
				}

				return key.type.GetProperty(key.propertyName, bindingFlags);
			});

			if (prop != null) {
				try {
					var val = ConvertHelper.ChangeObject(value, prop.PropertyType, null);
					prop.SetValue(target, val);
				} catch {
				}
				return;
			}

			// 尝试从缓存获取字段
			var field = _FieldCache.GetOrAdd((type, propertyName, ignoreCase), key => {
				var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
				if (ignoreCase) {
					bindingFlags |= BindingFlags.IgnoreCase;
				}
				return key.type.GetField(key.fieldName, bindingFlags);
			});

			if (field != null) {
				try {
					var val = ConvertHelper.ChangeObject(value, field.FieldType, null);
					field.SetValue(target, val);
				} catch {
				}
			}
		}

		/// <summary>设置索引值</summary>
		/// <param name="target">目标数据</param>
		/// <param name="index">索引</param>
		/// <param name="value">值</param>
		private static void SetIndexValue(object target, int index, object value) {
			if (target is null || index < 0) { return; }

			switch (target) {
				case Array array:
					if (index >= array.Length) {
						var newArray = Array.CreateInstance(array.GetType().GetElementType(), index + 1);
						Array.Copy(array, newArray, array.Length);
						array = newArray;
					}
					array.SetValue(value, index);
					break;

				case IList list:
					while (list.Count <= index) {
						list.Add(null);
					}
					list[index] = value;
					break;
			}
		}

		/// <summary>设置键值</summary>
		/// <param name="target">目标数据</param>
		/// <param name="key">键</param>
		/// <param name="value">值</param>
		private static void SetKeyValue(object target, string key, object value) {
			if (target is null || string.IsNullOrEmpty(key)) { return; }

			switch (target) {
				case IDictionary<string, object> dict:
					dict[key] = value;
					break;
				case IDictionary dict:
					dict[key] = value;
					break;
			}
		}

		#endregion

		#region 添加动态成员绑定器

		private class DynamicGetMemberBinder(string name, bool ignoreCase = true) : GetMemberBinder(name, ignoreCase) {
			public override DynamicMetaObject FallbackGetMember(DynamicMetaObject target, DynamicMetaObject errorSuggestion) => null;
		}

		private class DynamicSetMemberBinder(string name, bool ignoreCase = true) : SetMemberBinder(name, ignoreCase) {
			public override DynamicMetaObject FallbackSetMember(DynamicMetaObject target, DynamicMetaObject value, DynamicMetaObject errorSuggestion) => null;
		}
		#endregion

	}
}
