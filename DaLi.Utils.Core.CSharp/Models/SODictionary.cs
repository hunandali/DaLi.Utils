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
 *  文本键任意值字典集合
 * 
 * 	name: SODictionary
 * 	create: 2025-03-07
 * 	memo: 文本键(String)任意值(Object)字典集合(Dictionary)，忽略键名大小写，对于空白键也不允许
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Linq;
using DaLi.Utils.Extension;
using DaLi.Utils.Helper;
using DaLi.Utils.Json;

namespace DaLi.Utils.Model {

	/// <summary>文本键文本值字典集合</summary>
	public class SODictionary : StringDictionary<object> {

		/// <summary>构造</summary>
		public SODictionary() : base() { }

		/// <summary>构造</summary>
		public SODictionary(IEnumerable<KeyValuePair<string, object>> collection) : base(collection) { }

		/// <summary>构造</summary>
		public SODictionary(IDictionary<string, object> dictionary) : base(dictionary) { }

		/// <summary>构造</summary>
		public SODictionary(string json) : base(json) { }

		#region 获取单个值

		/// <summary>获取项目值，如果不存在则返回默认值</summary>
		public T GetValue<T>(string key, T defaultValue = default) {
			var value = this[key];
			return ConvertHelper.ChangeType(value, defaultValue);
		}

		/// <summary>通过多个键获取获取项目值，一旦最先的键获取到内容则直接返回</summary>
		public T GetValue<T>(IEnumerable<string> keys, T defaultValue = default) {
			if (keys is null || !keys.Any()) { return defaultValue; }

			var value = defaultValue;

			foreach (var key in keys) {
				value = GetValue<T>(key);
				if (value is not null) {
					break;
				}
			}

			return value ?? defaultValue;
		}

		/// <summary>获取项目值，如果不存在则返回默认值</summary>
		public object GetValue(string key, Type baseType, object defaultValue = null) {
			var value = this[key];
			return ConvertHelper.ChangeObject(value, baseType, defaultValue);
		}

		/// <summary>通过多个键获取获取项目值，一旦最先的键获取到内容则直接返回</summary>
		public object GetValue(IEnumerable<string> keys, Type baseType) {
			if (keys is null || !keys.Any()) { return null; }

			object value = null;
			foreach (var key in keys) {
				value = GetValue(key, baseType);
				if (value is not null) {
					break;
				}
			}

			return value;
		}

		/// <summary>获取项目文本值，如果不存在则返回空字符</summary>
		public string GetValue(string key, string defaultValue = "") {
			var value = this[key];
			if (value is null) { return defaultValue; }
			return value.ToString() ?? defaultValue;
		}

		/// <summary>通过多个键获取获取项目文本值，一旦最先的键获取到内容则直接返回</summary>
		public string GetValue(params string[] keys) {
			var value = "";

			foreach (var key in keys) {
				value = GetValue<string>(key, "");
				if (!string.IsNullOrEmpty(value)) {
					break;
				}
			}

			return value;
		}

		#endregion

		#region 获取列表值

		/// <summary>获取列表值</summary>
		public List<T> GetListValue<T>(string key) {
			var data = ConvertHelper.ChangeType<IEnumerable<T>>(this[key]);
			if (data is null) { return null; }

			return [.. data.Where(x => x is not null)];
		}

		/// <summary>通过多个键获取获取列表值，一旦最先的键获取到内容则直接返回</summary>
		public List<T> GetListValue<T>(IEnumerable<string> keys) {
			if (keys is null || !keys.Any()) { return null; }

			List<T> value = null;

			foreach (var key in keys) {
				value = GetListValue<T>(key);
				if (value.NotEmpty()) { break; }
			}

			return value;
		}

		/// <summary>获取列表值</summary>
		public List<object> GetListValue(string key, Type baseType) {
			var data = ConvertHelper.ChangeType<IEnumerable<object>>(this[key]);
			if (data is null) { return null; }

			return [.. data.Select(x => ConvertHelper.ChangeObject(x, baseType, null)).Where(x => x is not null)];
		}

		/// <summary>通过多个键获取获取列表值，一旦最先的键获取到内容则直接返回</summary>
		public List<object> GetListValue(IEnumerable<string> keys, Type baseType) {
			if (keys is null || !keys.Any()) { return null; }

			List<object> value = null;

			foreach (var key in keys) {
				value = GetListValue(key, baseType);
				if (value.NotEmpty()) { break; }
			}

			return value;
		}

		/// <summary>获取文本列表值</summary>
		public List<string> GetListValue(string key) {
			var data = ConvertHelper.ChangeType<IEnumerable<string>>(this[key]);
			if (data is null) { return null; }

			return [.. data.Where(x => !string.IsNullOrEmpty(x))];
		}

		/// <summary>通过多个键获取获取文本列表值，一旦最先的键获取到内容则直接返回</summary>
		public List<string> GetListValue(params string[] keys) {
			if (keys is null || keys.Length == 0) { return null; }

			List<string> value = null;

			foreach (var key in keys) {
				value = GetListValue(key);
				if (value.NotEmpty()) { break; }
			}

			return value;
		}

		#endregion

		/// <summary>字典合并并修改原始字典</summary>
		/// <param name="data">用于合并的内容</param>
		/// <param name="convertJson">数据是否需要转换成 JSON 字典</param>
		/// <remarks>先尝试转换成 JSON 字典后合并</remarks>
		/// <returns>合并后的字典</returns>
		public bool TryMerge(IDictionary<string, object> data, bool convertJson = false) {
			// 目标空内容，无需处理
			if (data.IsEmpty()) { return false; }

			// 原始字典为空时直接使用目标字典数据
			if (Count == 0) {
				Add(data);
				return true;
			}

			lock (Locker) {
				var target = convertJson ? data.ToJson().ToJsonDictionary() : data;

				foreach (var kv in target) {
					// 如果原始字典中不存在，则添加
					if (!TryGetValue(kv.Key, out var value)) { TryAdd(kv.Key, kv.Value); }

					// 新旧内容一致时跳过处理
					if (object.Equals(value, kv.Value)) { continue; }

					// 新旧数据都为字典时合并
					if (value is IDictionary<string, object> dicOld && kv.Value is IDictionary<string, object> dicNew) {
						var dict = new SODictionary(dicOld);

						if (dict.TryMerge(dicNew, false)) {
							this[kv.Key] = dict;
						} else {
							this[kv.Key] = kv.Value;
						}

						continue;
					}

					//  新旧数据都为集合
					if (value is IList<object> listOld && kv.Value is IList<object> listNew) {
						IList<object> list = [.. listOld];

						if (ListExtension.TryMerge(ref list, listNew, false)) {
							this[kv.Key] = list;
						} else {
							this[kv.Key] = kv.Value;
						}

						continue;
					}

					// 其他直接替换
					this[kv.Key] = kv.Value;
				}
			}

			return true;
		}
	}
}