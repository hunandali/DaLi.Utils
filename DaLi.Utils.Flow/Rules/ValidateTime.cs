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
 * 	时间验证
 * 
 * 	name: Rule.ValidateTime
 * 	create: 2025-03-17
 * 	memo: 时间验证
 * 	
 * ------------------------------------------------------------
 */

using System;
using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>时间验证</summary>
	public class ValidateTime : ValidateRuleBase {

		#region PROPERTY

		/// <summary>原始数据</summary>
		public DateTime Source { get; set; }

		/// <summary>规则名称</summary>
		public override string Name => "时间验证";

		/// <summary>开始时间 (HH:mm:ss)</summary>
		public string TimeStart { get; set; }

		/// <summary>结束时间 (HH:mm:ss)</summary>
		public string TimeEnd { get; set; }

		#endregion

		#region INFORMATION

		/// <summary>验证规则是否存在异常</summary>
		public override bool Validate(ref string message) {
			if (!Source.IsValidate()) {
				message = "用于验证的日期无效";
				return false;
			}

			if (TimeStart.IsEmpty() && TimeEnd.IsEmpty()) {
				message = "起始时间未设置";
				return false;
			}

			return base.Validate(ref message);
		}

		#endregion

		#region EXECUTE

		/// <summary>判断时间是否在指定范围内</summary>
		/// <param name="current">当前时间</param>
		/// <param name="start">开始时间</param>
		/// <param name="end">结束时间</param>
		private static bool IsTimeInRange(DateTime current, DateTime start, DateTime end) {
			if (end < start) {
				// 跨天的情况，如 23:00 - 01:00
				end = end.AddDays(1);
				if (current < start) {
					current = current.AddDays(1);
				}
			}

			return current >= start && current <= end;
		}

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			var dateNow = Main.DATE_NOW;

			// 分析时间范围
			var timeS = DateTime.Parse($"{dateNow:yyyy-MM-dd} {TimeStart}".Trim());
			var timeE = DateTime.Parse($"{dateNow:yyyy-MM-dd} {TimeEnd}".Trim());

			// 验证时间是否在范围内
			var ret = IsTimeInRange(dateNow, timeS, timeE);
			return Execute(ret, context);
		}

		#endregion
	}
}