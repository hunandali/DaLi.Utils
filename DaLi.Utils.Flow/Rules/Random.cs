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
 * 	随机
 * 
 * 	name: Rule.Random
 * 	create: 2025-03-17
 * 	memo: 随机数，大于等于下限，小于上限
 * 	
 * ------------------------------------------------------------
 */

using System.Threading;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>随机数</summary>
	public class Random : RuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "随机";

		/// <summary>最小值</summary>
		public int Min { get; set; }

		/// <summary>最大值</summary>
		public int Max { get; set; }

		#endregion

		#region INFORMATION

		/// <inheritdoc/>
		public override bool Validate(ref string message) {
			if (Max < 1) {
				message = "最大值必须设置且大于 0";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			if (Min > Max) {
				(Max, Min) = (Min, Max);
			}

			return new System.Random().Next(Min, Max);
		}

		#endregion
	}
}