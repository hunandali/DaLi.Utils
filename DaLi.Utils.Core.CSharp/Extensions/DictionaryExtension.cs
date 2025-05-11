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
 *  字典扩展操作
 * 
 * 	name: DictionaryExtension
 * 	create: 2025-02-25
 * 	memo: 字典扩展操作
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DaLi.Utils.Helper;
using DaLi.Utils.Json;

namespace DaLi.Utils.Extension {
	/// <summary>字典扩展操作</summary>
	public static partial class DictionaryExtension {

		#region ADD

		/// <summary>使用线程安全添加数据</summary>
		public static bool SafeAdd<TKey, TValue>(this Dictionary<TKey, TValue> input, TKey key, TValue value) {
			if (input is null || key is null) { return false; }

			return input.Lock(() => input.TryAdd(key, value));
		}

		/// <summary>使用线程安全添加一组数据，如果存在则不添加</summary>
		public static int SafeAdd<TKey, TValue>(this Dictionary<TKey, TValue> input, IEnumerable<KeyValuePair<TKey, TValue>> collection) {
			var ret = 0;
			if (input is null || collection is null || !collection.Any()) { return ret; }

			input.Lock(() => {
				foreach (var kv in collection) {
					if (input.TryAdd(kv.Key, kv.Value)) { ret++; }
				}
			});

			return ret;
		}

		/// <summary>使用线程安全添加一组数据，如果存在则不添加</summary>
		public static int SafeAdd<TKey, TValue>(this Dictionary<TKey, TValue> input, IDictionary<TKey, TValue> dictionary) {
			var ret = 0;
			if (input is null || dictionary is null || dictionary.Count == 0) { return ret; }

			input.Lock(() => {
				foreach (var kv in dictionary) {
					if (input.TryAdd(kv.Key, kv.Value)) { ret++; }
				}
			});

			return ret;
		}

		/// <summary>添加一组数据，如果存在则不添加；需要注意线程安全</summary>
		public static int Add<TKey, TValue>(this Dictionary<TKey, TValue> input, IEnumerable<KeyValuePair<TKey, TValue>> collection) {
			var ret = 0;
			if (input is null || collection is null || !collection.Any()) { return ret; }

			foreach (var kv in collection) {
				if (input.TryAdd(kv.Key, kv.Value)) { ret++; }
			}

			return ret;
		}

		/// <summary>添加一组数据，如果存在则不添加；需要注意线程安全</summary>
		public static int Add<TKey, TValue>(this Dictionary<TKey, TValue> input, IDictionary<TKey, TValue> dictionary) {
			var ret = 0;
			if (input is null || dictionary is null || dictionary.Count == 0) { return ret; }

			foreach (var kv in dictionary) {
				if (input.TryAdd(kv.Key, kv.Value)) { ret++; }
			}

			return ret;
		}

		#endregion

		#region UPDATE

		/// <summary>更新数据；需要注意线程安全</summary>
		/// <param name="input"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		/// <param name="force">数据不存在时是否强制添加，默认不添加</param>
		public static bool Update<TKey, TValue>(this Dictionary<TKey, TValue> input, TKey key, TValue value, bool force) {
			if (key is null) { return false; }

			var ret = false;

			if (force) {
				// 不存在时强制添加
				ret = input.TryAdd(key, value);

				// 添加失败表示存在，即需要更新
				if (!ret) {
					input[key] = value;
					ret = true;
				}
			} else if (input.ContainsKey(key)) {
				input[key] = value;
				ret = true;
			}

			return ret;
		}

		/// <summary>更新一组数据；需要注意线程安全</summary>
		/// <param name="input"></param>
		/// <param name="collection"></param>
		/// <param name="force">数据不存在时是否强制添加，默认不添加</param>
		public static int Update<TKey, TValue>(this Dictionary<TKey, TValue> input, IEnumerable<KeyValuePair<TKey, TValue>> collection, bool force = false) {
			var ret = 0;
			if (input is null || collection is null || !collection.Any()) { return ret; }

			foreach (var kv in collection) {
				if (Update(input, kv.Key, kv.Value, force)) { ret++; }
			}

			return ret;
		}

		/// <summary>更新一组数据；需要注意线程安全</summary>
		/// <param name="input"></param>
		/// <param name="dictionary"></param>
		/// <param name="force">数据不存在时是否强制添加，默认不添加</param>
		public static int Update<TKey, TValue>(this Dictionary<TKey, TValue> input, IDictionary<TKey, TValue> dictionary, bool force = false) {
			var ret = 0;
			if (input is null || dictionary is null || dictionary.Count == 0) { return ret; }

			foreach (var kv in dictionary) {
				if (Update(input, kv.Key, kv.Value, force)) { ret++; }
			}

			return ret;
		}

		/// <summary>遍历更新全部值；需要注意线程安全</summary>
		/// <param name="input"></param>
		/// <param name="action">更新函数（键，值，返回值）</param>
		public static void Update<TKey, TValue>(this Dictionary<TKey, TValue> input, Func<TKey, TValue, TValue> action) {
			if (input is null || action is null) { return; }

			foreach (var kv in input) {
				var value = action.Invoke(kv.Key, kv.Value);
				if (object.Equals(value, kv.Value)) { continue; }

				// 值发生了更新才重新写入
				input[kv.Key] = action(kv.Key, kv.Value);
			}
		}

		/// <summary>使用线程安全更新数据</summary>
		/// <param name="input"></param>
		/// <param name="key">键</param>
		/// <param name="value">值</param>
		/// <param name="force">数据不存在时是否强制添加，默认不添加</param>
		public static bool SafeUpdate<TKey, TValue>(this Dictionary<TKey, TValue> input, TKey key, TValue value, bool force = false) {
			if (input is null || key is null) { return false; }

			return input.Lock(() => Update(input, key, value, force));
		}

		/// <summary>使用线程安全更新一组数据</summary>
		/// <param name="input"></param>
		/// <param name="collection"></param>
		/// <param name="force">数据不存在时是否强制添加，默认不添加</param>
		public static int SafeUpdate<TKey, TValue>(this Dictionary<TKey, TValue> input, IEnumerable<KeyValuePair<TKey, TValue>> collection, bool force = false) {
			var ret = 0;
			if (input is null || collection is null || !collection.Any()) { return ret; }

			input.Lock(() => {
				foreach (var kv in collection) {
					if (Update(input, kv.Key, kv.Value, force)) { ret++; }
				}
			});

			return ret;
		}

		/// <summary>使用线程安全更新一组数据</summary>
		/// <param name="input"></param>
		/// <param name="dictionary"></param>
		/// <param name="force">数据不存在时是否强制添加，默认不添加</param>
		public static int SafeUpdate<TKey, TValue>(this Dictionary<TKey, TValue> input, IDictionary<TKey, TValue> dictionary, bool force = false) {
			var ret = 0;
			if (input is null || dictionary is null || dictionary.Count == 0) { return ret; }

			input.Lock(() => {
				foreach (var kv in dictionary) {
					if (Update(input, kv.Key, kv.Value, force)) { ret++; }
				}
			});

			return ret;
		}

		/// <summary>使用线程安全遍历更新全部值</summary>
		/// <param name="input"></param>
		/// <param name="action">更新函数（键，值，返回值）</param>
		public static void SafeUpdate<TKey, TValue>(this Dictionary<TKey, TValue> input, Func<TKey, TValue, TValue> action) {
			if (input is null || action is null) { return; }

			// 值发生了更新才重新写入
			input.Lock(() => {
				foreach (var kv in input) {
					var value = action.Invoke(kv.Key, kv.Value);
					if (object.Equals(value, kv.Value)) { continue; }

					input[kv.Key] = action(kv.Key, kv.Value);
				}
			});
		}

		#endregion

		#region REMOVE

		/// <summary>一次移除多个键</summary>
		public static int Remove<TKey, TValue>(this Dictionary<TKey, TValue> input, params TKey[] keys) {
			var ret = 0;
			if (input == null || keys == null || keys.Length == 0 || input.Count < 1) { return ret; }

			input.Lock(() => {
				foreach (var Key in keys) {
					if (Key is not null && input.Remove(Key)) { ret++; }
				}
			});

			return ret;
		}

		/// <summary>一次移除多个键</summary>
		public static int Remove<TKey, TValue>(this Dictionary<TKey, TValue> input, IEnumerable<TKey> keys) {
			var ret = 0;
			if (input == null || keys == null || !keys.Any() || input.Count < 1) { return ret; }

			input.Lock(() => {
				foreach (var Key in keys) {
					if (Key is not null && input.Remove(Key)) { ret++; }
				}
			});

			return ret;
		}

		/// <summary>一次移除两个键</summary>
		/// <remarks>注意与 Dictionary.Remove(Key, Value) 的差异，此函数目的是防止直接执行内置的 Remove(Key, Value)</remarks>
		public static int Remove<T>(this Dictionary<string, T> input, string key1, string key2) => input.Remove([key1, key2]);

		/// <summary>仅保留指定键的值</summary>
		public static int Keep<T>(this Dictionary<string, T> input, params string[] keys) {
			if (keys == null || input == null || keys.Length == 0 || input.Count == 0) { return 0; }

			var keepSet = new HashSet<string>(keys, input.Comparer);
			var removeKeys = new List<string>();

			foreach (var key in input.Keys) {
				if (!keepSet.Contains(key)) {
					removeKeys.Add(key);
				}
			}

			return input.Remove(removeKeys);
		}

		#endregion

		#region OTHER

		/// <summary>字典值合并，存在替换，不存在添加</summary>
		/// <param name="input">原始内容</param>
		/// <param name="other">用于合并的内容</param>
		/// <returns>合并后的字典</returns>
		public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> input, IDictionary<TKey, TValue> other) {

			// 目标空内容，无需处理
			if (other.IsEmpty()) { return new Dictionary<TKey, TValue>(input, input.TryGetComparer()); }

			// 原始字典为空时直接使用目标字典数据
			if (input.IsEmpty()) { return new Dictionary<TKey, TValue>(other, other.TryGetComparer()); }

			// 复制数据
			var result = new Dictionary<TKey, TValue>(input, input.TryGetComparer());

			foreach (var kv in other) {
				result[kv.Key] = kv.Value;
			}

			return result;
		}

		/// <summary>字典合并并修改原始字典</summary>
		/// <param name="input">原始内容</param>
		/// <param name="other">用于合并的内容</param>
		/// <param name="convertJson">数据是否需要转换成 JSON 字典</param>
		/// <remarks>先尝试转换成 JSON 字典后合并</remarks>
		/// <returns>合并后的字典</returns>
		public static bool TryMerge(ref IDictionary<string, object> input, IDictionary<string, object> other, bool convertJson = true) {
			// 目标空内容，无需处理
			if (other.IsEmpty()) { return false; }

			// 原始字典为空时直接使用目标字典数据
			if (input.IsEmpty()) {
				input = new Dictionary<string, object>(other, other.TryGetComparer());
				return true;
			}

			lock (LockerHelper.GetLock(input)) {
				var source = convertJson ? input.ToJson().ToJsonDictionary() : input;
				var target = convertJson ? other.ToJson().ToJsonDictionary() : other;

				foreach (var kv in target) {
					// 如果原始字典中不存在，则添加
					if (!source.TryGetValue(kv.Key, out var value)) { source.TryAdd(kv.Key, kv.Value); }

					// 新旧内容一致时跳过处理
					if (object.Equals(value, kv.Value)) { continue; }

					// 新旧数据都为字典时合并
					if (value is Dictionary<string, object> dicOld && kv.Value is Dictionary<string, object> dicNew) {
						IDictionary<string, object> dict = new Dictionary<string, object>(dicOld, dicOld.TryGetComparer());
						if (!TryMerge(ref dict, dicNew, false)) { dict = dicNew; }
						input[kv.Key] = dict;
						continue;
					}

					//  新旧数据都为集合
					if (value is IList<object> listOld && kv.Value is IList<object> listNew) {
						IList<object> list = [.. listOld];
						if (!ListExtension.TryMerge(ref list, listNew, false)) { list = listNew; }
						input[kv.Key] = list;
						continue;
					}

					// 其他直接替换
					input[kv.Key] = kv.Value;
				}
			}

			return true;
		}

		/// <summary>遍历项目</summary>
		public static void ForEach<TKey, TValue>(this IDictionary<TKey, TValue> input, Action<TKey, TValue> action) {
			if (action is null || input == null || input.Count == 0) { return; }

			foreach (var kv in input) {
				action.Invoke(kv.Key, kv.Value);
			}
		}

		/// <summary>获取字典数据的键排序比较(Comparer)的扩展方法</summary>
		public static IEqualityComparer<TKey> TryGetComparer<TKey, TValue>(this IDictionary<TKey, TValue> input) {
			if (input is Dictionary<TKey, TValue> genericDict) {
				return genericDict.Comparer;
			}

			if (input is ConcurrentDictionary<TKey, TValue> concurrentDict) {
				return concurrentDict.Comparer;
			}

			if (input is ImmutableDictionary<TKey, TValue> immutableDict) {
				return immutableDict.KeyComparer;
			}

			// 反射回退（需缓存 PropertyInfo 以优化性能）
			var prop = input.GetType().GetSingleProperty("Comparer");
			return prop?.GetValue(input) as IEqualityComparer<TKey> ?? EqualityComparer<TKey>.Default;
		}

		#endregion
	}
}