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
 * 	name: StringDictionary
 * 	create: 2025-03-07
 * 	memo: 文本键(String)泛类值字典集合(Dictionary)，忽略键名大小写，对于空白键也不允许
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Linq;
using DaLi.Utils.Json;

namespace DaLi.Utils.Model {

	/// <summary>文本键任意值字典集合</summary>
	public class StringDictionary<T> : Dictionary<string, T>, ICloneable {

		/// <summary>线程锁定对象</summary>
		protected readonly object Locker = new();

		/// <summary>是否无效键</summary>
		protected static bool BadKey(string key) => string.IsNullOrWhiteSpace(key);

		#region 初始化

		/// <summary>构造</summary>
		public StringDictionary() : base(StringComparer.OrdinalIgnoreCase) { }

		/// <summary>构造</summary>
		public StringDictionary(string json) {
			var dict = json.FromJson<Dictionary<string, T>>();
			if (dict is null || dict.Count == 0) { return; }

			Add(dict);
		}

		/// <summary>构造</summary>
		public StringDictionary(IEnumerable<KeyValuePair<string, T>> collection) : base(collection, StringComparer.OrdinalIgnoreCase) { }

		/// <summary>构造</summary>
		public StringDictionary(IDictionary<string, T> dictionary) : base(dictionary, StringComparer.OrdinalIgnoreCase) { }

		#endregion

		#region ADD

		/// <summary>添加键值</summary>
		public new bool Add(string key, T value) {
			if (BadKey(key)) { return false; }

			lock (Locker) {
				return TryAdd(key, value);
			}
		}

		/// <summary>添加一组数据</summary>
		public int Add(IEnumerable<KeyValuePair<string, T>> collection) {
			if (collection is null || !collection.Any()) { return 0; }

			var ret = 0;
			lock (Locker) {
				foreach (var kv in collection) {
					if (!BadKey(kv.Key)) {
						if (TryAdd(kv.Key, kv.Value)) { ret++; }
					}
				}
			}

			return ret;
		}

		/// <summary>添加一组数据</summary>
		public int Add(IDictionary<string, T> dictionary) {
			if (dictionary is null || dictionary.Count == 0) { return 0; }

			var ret = 0;
			lock (Locker) {
				foreach (var kv in dictionary) {
					if (!BadKey(kv.Key)) {
						if (TryAdd(kv.Key, kv.Value)) { ret++; }
					}
				}
			}

			return ret;
		}

		#endregion

		#region UPDATE

		/// <summary>更新数据，默认不存在则不修改</summary>
		/// <param name="key">键</param>
		/// <param name="value">值</param>
		/// <param name="force">不存在则强制添加</param>
		private bool TryUpdate(string key, T value, bool force = false) {
			if (BadKey(key)) { return false; }

			var ret = false;

			if (force) {
				// 不存在时强制添加
				ret = base.TryAdd(key, value);

				// 添加失败表示存在，即需要更新
				if (!ret) {
					base[key] = value;
					ret = true;
				}
			} else if (base.ContainsKey(key)) {
				base[key] = value;
				ret = true;
			}

			return ret;
		}

		/// <summary>更新数据，默认不存在则不修改</summary>
		/// <param name="key">键</param>
		/// <param name="value">值</param>
		/// <param name="force">不存在则强制添加</param>
		public bool Update(string key, T value, bool force = false) {
			lock (Locker) {
				return TryUpdate(key, value, force);
			}
		}

		/// <summary>更新一组数据，默认不存在则不修改</summary>
		/// <param name="collection"></param>
		/// <param name="force">不存在则强制添加</param>
		public int Update(IEnumerable<KeyValuePair<string, T>> collection, bool force = false) {
			var ret = 0;
			if (collection is null || !collection.Any()) { return ret; }

			lock (Locker) {
				foreach (var kv in collection) {
					if (TryUpdate(kv.Key, kv.Value, force)) { ret++; }
				}
			}

			return ret;
		}

		/// <summary>更新一组数据，默认不存在则不修改</summary>
		/// <param name="dictionary"></param>
		/// <param name="force">不存在则强制添加</param>
		public int Update(IDictionary<string, T> dictionary, bool force = false) {
			var ret = 0;
			if (dictionary is null || dictionary.Count == 0) { return ret; }

			lock (Locker) {
				foreach (var kv in dictionary) {
					if (TryUpdate(kv.Key, kv.Value, force)) { ret++; }
				}
			}

			return ret;
		}

		/// <summary>遍历更新值</summary>
		/// <param name="action">更新函数（键，值，返回值）</param>
		public void Update(Func<string, object, T> action) {
			if (action is null) { return; }

			// 值发生了更新才重新写入
			lock (Locker) {
				foreach (var kv in this) {
					var value = action.Invoke(kv.Key, kv.Value);
					if (object.Equals(value, kv.Value)) { continue; }

					this[kv.Key] = action(kv.Key, kv.Value);
				}
			}
		}

		#endregion

		#region 值操作

		/// <summary>设置 / 获取项目，设置时如果不存在则新建，存在则更新</summary>
		public new T this[string key] {
			get {
				if (BadKey(key)) { return default; }

				base.TryGetValue(key, out var R);
				return R;
			}
			set {
				if (BadKey(key)) { return; }

				lock (Locker) {
					base[key] = value;
				}
			}
		}

		/// <summary>获取项目值，如果不存在则返回默认值</summary>
		public T this[string key, T defaultValue] => this[key] ?? defaultValue;

		/// <summary>获取项目</summary>
		public bool TryGet(string key, out T value) {
			if (BadKey(key)) {
				value = default;
				return false;
			}

			return base.TryGetValue(key, out value);
		}

		#endregion

		#region 常用函数

		/// <summary>移除项目</summary>
		public new void Clear() {
			if (Count == 0) { return; }

			lock (Locker) {
				base.Clear();
			}
		}

		/// <summary>移除项目</summary>
		public new bool Remove(string key) {
			if (BadKey(key)) { return false; }

			lock (Locker) {
				return base.Remove(key);
			}
		}

		/// <summary>移除项目</summary>
		public int Remove(params string[] keys) {
			var ret = 0;
			if (keys is null || keys.Length == 0) { return ret; }

			lock (Locker) {
				foreach (var Key in keys) {
					if (!BadKey(Key) && base.Remove(Key)) { ret++; }
				}
			}

			return ret;
		}

		/// <summary>一次移除两个键</summary>
		/// <remarks>注意与 Dictionary.Remove(Key, Value) 的差异，此函数目的是防止直接执行内置的 Remove(Key, Value)</remarks>
		public void Remove(string key1, string key2) => Remove([key1, key2]);

		/// <summary>移除项目</summary>
		public int Remove(IEnumerable<string> keys) {
			var ret = 0;
			if (keys is null || !keys.Any()) { return ret; }

			lock (Locker) {
				foreach (var Key in keys) {
					if (!BadKey(Key) && base.Remove(Key)) { ret++; }
				}
			}

			return ret;
		}

		/// <summary>仅保留指定键的值</summary>
		public void Keep(params string[] keys) {
			if (keys is null || keys.Length == 0) { return; }

			var keepSet = new HashSet<string>(keys, StringComparer.OrdinalIgnoreCase);
			var removeKeys = new List<string>();

			foreach (var key in Keys) {
				if (!keepSet.Contains(key)) {
					removeKeys.Add(key);
				}
			}

			Remove(removeKeys);
		}

		#endregion

		#region 其他操作

		/// <summary>克隆</summary>
		/// <remarks>注意：如果值为对象，则在克隆的时候可能不会深度克隆。</remarks>
		public object Clone() => new StringDictionary<T>(this);

		/// <summary>遍历项目</summary>
		public void ForEach(Action<string, T> action) {
			if (action is not null && Count > 0) {
				foreach (var KV in this) {
					action.Invoke(KV.Key, KV.Value);
				}
			}
		}

		/// <summary>通过 JSON 创建字典</summary>
		/// <param name="json">JSON 数据</param>
		public static StringDictionary<T> FromJson(string json) => json.FromJson<StringDictionary<T>>() ?? [];

		/// <summary>将字典转换为 JSON 字符串</summary>
		public string ToJson() => JsonExtension.ToJson(this);

		#endregion
	}
}