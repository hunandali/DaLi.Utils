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
 * 	按范围循环
 * 
 * 	name: Rule.WhileInterval
 * 	create: 2025-03-17
 * 	memo: 按范围循环
 * 
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>按范围循环</summary>
	public class WhileInterval : WhileRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "按范围循环";

		/// <summary>最小值</summary>
		public int Min { get; set; }

		/// <summary>最大值</summary>
		public int Max { get; set; }

		/// <summary>进度。大于零：从小到大；小于零：从大到小</summary>
		public int Interval { get; set; }

		#endregion

		#region INFORMATION

		/// <inheritdoc/>
		public override bool Validate(ref string message) {
			if (Interval == 0) {
				message = "进度不能为零";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) =>
			// 执行循环
			WhileExecute(Min, Max, Interval, context, x => [], cancel);

		#endregion
	}
}