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
 * 	类型相关扩展操作
 * 
 * 	name: TypeExtension
 * 	create: 2025-03-13
 * 	memo: 类型相关扩展操作
 * 
 * ------------------------------------------------------------
 */

using System;

namespace DaLi.Utils.Extension {
	/// <summary>类型相关扩展操作</summary>
	public static class TypeExtensions {
		/// <summary>判断指定类型是否来源于某个基类型</summary>
		/// <param name="type">当前类型</param>
		/// <param name="baseType">要验证的基类型</param>
		/// <param name="enabledEquals">是否允许当前类型与要验证的基类型相同，默认为 true</param>
		/// <param name="enabledAbstract">是否允许当前类型为抽象类，默认为 false</param>
		/// <returns>如果当前类型来源于指定的基类型，则返回 true；否则返回 false</returns>
		public static bool IsComeFrom(this Type type, Type baseType, bool enabledEquals = true, bool enabledAbstract = false) {
			// 参数验证：如果基类型为空、当前类型为空，或者不允许抽象类且当前类型是抽象类，则返回false
			if (baseType is null || type is null || (!enabledAbstract && type.IsAbstract)) {
				return false;
			}

			// 如果两个类型相同，根据 enabledEquals 参数决定返回结果
			if (type == baseType) {
				return enabledEquals;
			}

			// 验证当前类型是否可以从基类型分配（即是否为基类型的派生类）
			return baseType.IsAssignableFrom(type);
		}

		/// <summary>判断指定类型是否来源于泛型参数指定的基类型</summary>
		/// <typeparam name="T">要验证的基类型</typeparam>
		/// <param name="type">当前类型</param>
		/// <param name="enabledEquals">是否允许当前类型与要验证的基类型相同，默认为true</param>
		/// <param name="enabledAbstract">是否允许当前类型为抽象类，默认为false</param>
		/// <returns>如果当前类型来源于指定的基类型，则返回true；否则返回false</returns>
		public static bool IsComeFrom<T>(this Type type, bool enabledEquals = true, bool enabledAbstract = false) =>
			// 调用非泛型版本的IsComeFrom方法，传入typeof(T)作为基类型
			type.IsComeFrom(typeof(T), enabledEquals, enabledAbstract);

		/// <summary>判断是否为自定义类型，非系统内置类型</summary>
		/// <param name="type">要检查的类型</param>
		/// <returns>如果是自定义类型则返回 true，否则返回 false</returns>
		public static bool IsExtendClass(this Type type) {
			/*
			 - IsValueType：
				包含所有值类型，包括结构体(struct)和枚举(enum)
				常见的包括：int、bool、char、DateTime、自定义struct、枚举等
				这些类型直接存储在栈上
			 - IsPrimitive：
				仅包含.NET中的原始类型
				具体包括：Boolean、Byte、SByte、Int16、UInt16、Int32、UInt32、Int64、UInt64、IntPtr、UIntPtr、Char、Double、Single
				不包括：Decimal、DateTime、自定义struct、枚举等

			 - 关键区别总结：
				IsPrimitive 是 IsValueType 的子集
				所有 IsPrimitive 为 True 的类型， 其 IsValueType 一定为 true
				IsValueType 为 True 的类型， 不一定是原始类型（IsPrimitive 可能为 False）
				自定义的结构体和枚举都是 IsValueType， 但不是 IsPrimitive
			*/

			// 排除系统类型，使用短路逻辑优化性能
			if (type.IsPrimitive ||
				type.IsValueType ||
				type == typeof(string) ||
				type == typeof(object) ||
				type.IsArray ||
				type.IsEnum ||
				type.IsInterface ||
				type.IsAbstract) {
				return false;
			}

			// 检查命名空间和程序集
			// 确保类型是类，且不属于系统程序集或命名空间
			return type.IsClass &&
				   !type.Assembly.Equals(typeof(string).Assembly) &&
				   !(type.Namespace?.StartsWith("System") ?? false) &&
				   !(type.Namespace?.StartsWith("Microsoft") ?? false);
		}
	}
}