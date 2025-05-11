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
 * 	属性相关扩展操作
 * 
 * 	name: PropertyExtension
 * 	create: 2025-03-08
 * 	memo: 属性相关扩展操作
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;

namespace DaLi.Utils.Extension {
	/// <summary>属性相关扩展操作</summary>
	public static class PropertyExtension {

		#region 获取属性列表

		/// <summary>属性缓存</summary>
		private static ImmutableDictionary<string, PropertyInfo[]> _PropertyCache = ImmutableDictionary<string, PropertyInfo[]>.Empty;

		/// <summary>获取类型的所有属性（包含实例/静态、公共/非公共属性）</summary>
		/// <param name="type">目标类型</param>
		/// <returns>属性信息数组</returns>
		public static PropertyInfo[] GetAllProperties(this Type type) {
			if (type == null) { return null; }

			if (!_PropertyCache.TryGetValue(type.FullName, out var properties)) {
				properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);

				_PropertyCache = _PropertyCache.Add(type.FullName, properties);
			}

			return properties;
		}

		/// <summary>获取指定名称的属性（不区分大小写）</summary>
		/// <param name="type">目标类型</param>
		/// <param name="name">属性名称</param>
		/// <returns>匹配的第一个属性或null</returns>
		public static PropertyInfo GetSingleProperty(this Type type, string name) {
			if (type == null || string.IsNullOrEmpty(name)) { return null; }

			return GetAllProperties(type)?.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
		}

		/// <summary>通过条件表达式获取属性</summary>
		/// <param name="type">目标类型</param>
		/// <param name="predicate">筛选条件</param>
		/// <returns>匹配的第一个属性或null</returns>
		public static PropertyInfo GetSingleProperty(this Type type, Func<PropertyInfo, bool> predicate) {
			if (type == null || predicate == null) { return null; }

			return GetAllProperties(type)?.FirstOrDefault(predicate);
		}

		#endregion

		/// <summary>判断属性是否为公共可读属性</summary>
		public static bool IsPublic(this PropertyInfo prop) => prop?.GetMethod?.IsPublic == true && prop.GetIndexParameters().Length == 0;

	}
}
