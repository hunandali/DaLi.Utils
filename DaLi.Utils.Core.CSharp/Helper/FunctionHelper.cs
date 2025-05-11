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
 * 	C# 函数扩展
 * 
 * 	name: Helper.FunctionHelper
 * 	create: 2024-07-30
 * 	memo: 用于 VB.net 中不兼容函数的变通方式
 * 
 * ------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DaLi.Utils.Helper {
	/// <summary>C# 函数扩展</summary>
	public static class FunctionHelper {

		/// <summary>异步遍历</summary>
		/// <param name="source">异步列表数据</param>
		/// <param name="action">数据操作</param>
		public static async Task ForEachAsync<T>(this IAsyncEnumerable<T> source, Action<T> action) {
			if (source == null || action == null) {
				return;
			}

			await foreach (var item in source) {
				action(item);
			}
		}
	}
}
