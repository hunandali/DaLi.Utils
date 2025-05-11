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
 *  集合扩展操作
 * 
 * 	name: ListExtension
 * 	create: 2025-02-25
 * 	memo: 集合扩展操作
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using DaLi.Utils.Helper;
using DaLi.Utils.Json;

namespace DaLi.Utils.Extension {
	/// <summary>集合扩展操作</summary>
	public static partial class ListExtension {

		/// <summary>字典值合并，存在替换，不存在添加</summary>
		/// <param name="input">原始内容</param>
		/// <param name="other">用于合并的内容</param>
		/// <returns>合并后的字典</returns>
		public static IList<T> Merge<T>(this IList<T> input, IList<T> other) {
			if (other.IsEmpty()) { return [.. input]; }
			if (input.IsEmpty()) { return [.. other]; }

			var result = new List<T>(input);
			var lenInput = input.Count;
			var lenOther = other.Count;

			for (var i = 0; i < lenOther; i++) {
				if (i >= lenInput) {
					result.Add(other[i]);
				} else {
					result[i] = other[i];
				}
			}

			return result;
		}

		/// <summary>字典值合并，存在替换，不存在添加</summary>
		/// <param name="input">原始内容</param>
		/// <param name="other">用于合并的内容</param>
		/// <param name="convertJson">数据是否需要转换成 JSON 字典</param>
		/// <remarks>先尝试转换成 JSON 字典后合并</remarks>
		/// <returns>合并后的字典</returns>
		public static bool TryMerge(ref IList<object> input, IList<object> other, bool convertJson = true) {
			if (other.IsEmpty()) { return false; }
			if (input.IsEmpty()) {
				input = [.. other];
				return true;
			}

			lock (LockerHelper.GetLock(input)) {
				var source = convertJson ? input.ToJson().ToJsonList() : [.. input];
				var target = convertJson ? other.ToJson().ToJsonList() : other;

				var lenS = source.Count;
				var lenT = target.Count;

				for (var i = 0; i < lenT; i++) {
					if (i >= lenS) {
						source.Add(target[i]);
						continue;
					}

					// 新旧内容一致时跳过处理
					if (object.Equals(source[i], target[i])) { continue; }

					// 新旧数据都为字典时合并
					if (source[i] is Dictionary<string, object> dicOld && target[i] is Dictionary<string, object> dicNew) {
						IDictionary<string, object> dict = new Dictionary<string, object>(dicOld, dicOld.TryGetComparer());
						if (!DictionaryExtension.TryMerge(ref dict, dicNew, false)) { dict = dicNew; }
						source[i] = dict;
						continue;
					}

					//  新旧数据都为集合
					if (source[i] is IList<object> listOld && target[i] is IList<object> listNew) {
						IList<object> list = [.. listOld];
						if (!TryMerge(ref list, listNew, false)) { list = listNew; }
						source[i] = list;
						continue;
					}

					// 其他直接替换
					source[i] = target[i];
				}

				input = source;
			}

			return true;
		}

		/// <summary>移除重复数据 - 根据指定的键选择器函数去除集合中的重复元素</summary>
		/// <typeparam name="T">集合元素类型</typeparam>
		/// <typeparam name="V">键选择器返回的键类型</typeparam>
		/// <param name="source">要操作的源集合</param>
		/// <param name="keySelector">用于从元素中提取键的函数</param>
		/// <returns>去除重复元素后的集合，如果源集合或键选择器为 null 则返回原始集合</returns>
		public static IEnumerable<T> Distinct<T, V>(this IEnumerable<T> source, Func<T, V> keySelector) {
			// 参数验证
			if (source == null || keySelector == null) {
				return source;
			}

			// 使用自定义比较器去除重复项
			return source.Distinct(new CommonEqualityComparer<T, V>(keySelector));
		}

		/// <summary>移除重复数据 - 根据指定的键选择器函数和比较器去除集合中的重复元素</summary>
		/// <typeparam name="T">集合元素类型</typeparam>
		/// <typeparam name="V">键选择器返回的键类型</typeparam>
		/// <param name="source">要操作的源集合</param>
		/// <param name="keySelector">用于从元素中提取键的函数</param>
		/// <param name="comparer">用于比较源元素的比较器</param>
		/// <returns>去除重复元素后的集合，如果源集合或键选择器为 null 则返回原始集合</returns>
		public static IEnumerable<T> Distinct<T, V>(this IEnumerable<T> source, Func<T, V> keySelector, IEqualityComparer<V> comparer) {
			// 参数验证
			if (source == null || keySelector == null) {
				return source;
			}

			// 使用自定义比较器和提供的比较器去除重复项
			return source.Distinct(new CommonEqualityComparer<T, V>(keySelector, comparer));
		}

		/// <summary>自定义相等比较器，用于根据指定的键选择器比较对象</summary>
		/// <typeparam name="T">要比较的对象类型</typeparam>
		/// <typeparam name="V">键的类型</typeparam>
		/// <param name="keySelector">用于从对象中提取键的函数</param>
		/// <param name="comparer">用于比较键的比较器</param>
		public class CommonEqualityComparer<T, V>(Func<T, V> keySelector, IEqualityComparer<V> comparer) : IEqualityComparer<T> {
			private readonly Func<T, V> _KeySelector = keySelector;
			private readonly IEqualityComparer<V> _Comparer = comparer;
			private readonly IEqualityComparer<V> _KeyComparer = EqualityComparer<V>.Default;

			/// <summary>初始化比较器，只使用键选择器</summary>
			/// <param name="keySelector">用于从对象中提取键的函数</param>
			public CommonEqualityComparer(Func<T, V> keySelector)
				: this(keySelector, null) {
			}

			/// <summary>确定指定的对象是否相等</summary>
			/// <param name="x">要比较的第一个对象</param>
			/// <param name="y">要比较的第二个对象</param>
			/// <returns>如果指定的对象相等，则为 true；否则为 false</returns>
			public bool Equals(T x, T y) {
				// 如果两个引用相同，则它们相等
				if (ReferenceEquals(x, y)) { return true; }

				// 如果任一引用为null，则它们不相等
				if (x == null || y == null) { return false; }

				// 选择函数无效
				if (_KeySelector == null) { return false; }

				// 比较
				return (_Comparer ?? _KeyComparer).Equals(_KeySelector(x), _KeySelector(y));
			}

			/// <summary>返回指定对象的哈希代码</summary>
			/// <param name="obj">要获取哈希代码的对象</param>
			/// <returns>指定对象的哈希代码</returns>
			public int GetHashCode([DisallowNull] T obj) {
				// 处理null对象
				if (obj == null) { return 0; }

				// 获取键的哈希代码
				var key = _KeySelector(obj);
				return key == null ? 0 : (_Comparer ?? _KeyComparer).GetHashCode(key);
			}
		}
	}
}