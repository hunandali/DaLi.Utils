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
 *  对象锁
 * 
 * 	name: LockerHelper
 * 	create: 2025-02-25
 * 	memo: 对象锁，为任意对象提供动态绑定的独立锁
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Runtime.CompilerServices;

namespace DaLi.Utils.Helper {
	/// <summary>对象锁，为任意对象提供动态绑定的独立锁</summary>
	public static class LockerHelper {

		/// <summary>核心存储结构：对象实例 -> 锁对象</summary>
		private static readonly ConditionalWeakTable<object, LockContainer> _LockTable = [];

		/// <summary>获取与特定实例关联的锁对象（通用版）</summary>
		public static object GetLock(object instance) => _LockTable.GetOrCreateValue(instance).SyncRoot;

		/// <summary>获取与特定实例关联的锁对象（泛型版）</summary>
		public static object GetLock<T>(T instance) where T : class => _LockTable.GetOrCreateValue(instance).SyncRoot;

		/// <summary>锁操作</summary>
		public static void Lock(object instance, Action action) {
			lock (GetLock(instance)) {
				action();
			}
		}

		/// <summary>锁操作</summary>
		public static void Lock<T>(this T instance, Action action) where T : class {
			lock (GetLock(instance)) {
				action();
			}
		}

		/// <summary>锁操作</summary>
		public static V Lock<V>(object instance, Func<V> action) {
			lock (GetLock(instance)) {
				return action();
			}
		}

		/// <summary>锁操作</summary>
		public static V Lock<T, V>(this T instance, Func<V> action) where T : class {
			lock (GetLock(instance)) {
				return action();
			}
		}

		/// <summary>锁容器（确保值类型不会被共享）</summary>
		private class LockContainer {
			public object SyncRoot { get; } = new object();
		}
	}
}