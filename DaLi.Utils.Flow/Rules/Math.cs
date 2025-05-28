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
 * 	算数计算
 * 
 * 	name: Rule.Math
 * 	create: 2025-03-17
 * 	memo: 算数计算
 * 	
 * ------------------------------------------------------------
 */

using System.Data;
using System.Threading;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Extension;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>算数计算</summary>
	public class Math : RuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "算数计算";

		/// <summary>表达式</summary>
		public string Source { get; set; }

		#endregion

		#region INFORMATION

		/// <inheritdoc/>
		public override bool Validate(ref string message) {
			if (Source.IsEmpty()) {
				message = "表达式未设置";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) => new DataTable().Compute(Source, null);

		#endregion
	}
}