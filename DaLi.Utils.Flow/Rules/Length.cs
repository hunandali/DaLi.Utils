/*
 * ------------------------------------------------------------
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
 * 	数据内容长度
 * 
 * 	name: Rule.Length
 * 	create: 2025-03-17
 * 	memo: 数据内容长度，列表、字典返回数量，文本返回字符长度，其他无效
 * 	
 * ------------------------------------------------------------
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>数据内容长度</summary>
	public class Length : FlowRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "数据内容长度";

		/// <summary>来源数据</summary>
		public object Source { get; set; }

		#endregion

		#region INFORMATION

		/// <summary>验证规则是否存在异常</summary>
		public override bool Validate(ref string message) {
			if (Source is null) {
				message = "来源数据未设置";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			if (Source is string str) {
				return str.Length;
			}

			if (Source is Array arr) {
				return arr.Length;
			}

			if (Source is IEnumerable<object> enu) {
				return enu.Count();
			}

			if (Source is ICollection<object> coll) {
				return coll.Count;
			}

			if (Source is IDictionary<string, object> dict) {
				return dict.Count;
			}

			if (Source is IList<object> list) {
				return list.Count;
			}

			FlowException.Throw(ExceptionEnum.EXECUTE_ERROR, "无效数据格式，获取数据长度仅支持：数组，集合，列表，字典与文本数据");
			return null;
		}

		#endregion
	}
}