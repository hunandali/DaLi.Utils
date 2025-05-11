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
 * 	按次数循环
 * 
 * 	name: DaLi.Utils.Flow.Rules.WhileTimes
 * 	create: 2025-03-17
 * 	memo: 按次数循环
 * 
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {
	/// <summary>按次数循环</summary>
	public class WhileTimes : WhileRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "按次数循环";

		/// <summary>循环次数</summary>
		public int Count { get; set; }

		#endregion

		#region INFORMATION

		/// <summary>验证规则是否存在异常</summary>
		public override bool Validate(ref string message) {
			if (Count < 1) {
				message = "循环次数必须大于0";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			// 记录数
			Count = Count < 1 ? 1 : Count;

			// 执行循环
			return WhileExecute(1, Count, 1, context, x => [], cancel);
		}

		#endregion
	}
}