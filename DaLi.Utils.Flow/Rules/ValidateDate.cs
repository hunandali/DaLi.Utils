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
 * 	日期验证
 * 
 * 	name: Rule.ValidateDate
 * 	create: 2025-03-17
 * 	memo: 日期验证
 * 	
 * ------------------------------------------------------------
 */

using System;
using System.Threading;
using DaLi.Utils.Extension;
using DaLi.Utils.Flow.Base;
using DaLi.Utils.Model;

namespace DaLi.Utils.Flow.Rule {

	/// <summary>日期验证</summary>
	public class ValidateDate : ValidateRuleBase {

		#region PROPERTY

		/// <summary>规则名称</summary>
		public override string Name => "日期验证";

		/// <summary>原始日期数据，不设置为当前时间</summary>
		public DateTime Source { get; set; }

		/// <summary>需要验证的时间参数</summary>
		public DateNameEnum[] Enums { get; set; }

		/// <summary>是否所有日期都需要匹配</summary>
		public bool ValidateALL { get; set; }

		#endregion

		#region INFORMATION

		/// <summary>验证规则是否存在异常</summary>
		public override bool Validate(ref string message) {
			if (!Source.IsValidate()) {
				message = "用于验证的日期无效";
				return false;
			}

			if (Enums.IsEmpty()) {
				message = "验证日期参数未设置";
				return false;
			}

			return true;
		}

		#endregion

		#region EXECUTE

		private static bool IsValidate(DateTime now, DateNameEnum name) => name switch {
			DateNameEnum.ALL => true,

			DateNameEnum.MONDAY => now.IsMonday(),
			DateNameEnum.TUESDAY => now.IsTuesday(),
			DateNameEnum.WEDNESDAY => now.IsWednesday(),
			DateNameEnum.THURSDAY => now.IsThursday(),
			DateNameEnum.FRIDAY => now.IsFriday(),
			DateNameEnum.SATURDAY => now.IsSaturday(),
			DateNameEnum.SUNDAY => now.IsSunday(),

			DateNameEnum.MONTH_FIRST => now.IsMonthBegin(),
			DateNameEnum.MONTH_LAST => now.IsMonthEnd(),

			DateNameEnum.WORKDAY => now.IsWorkday(),
			DateNameEnum.WORKDAY_MONTH_FIRST => now.IsMonthWorkFirst(),
			DateNameEnum.WORKDAY_MONTH_LAST => now.IsMonthWorkEnd(),
			DateNameEnum.WORKDAY_WEEK_FIRST => now.IsWeekWorkFirst(),
			DateNameEnum.WORKDAY_WEEK_LAST => now.IsWeekWorkEnd(),

			DateNameEnum.ADJUSTDAY => now.IsAdjustday(),

			DateNameEnum.HOLIDAY => now.IsHoliday(),
			DateNameEnum.HOLIDAY_BEFORE => now.IsBeforeHoliday(),
			DateNameEnum.HOLIDAY_FIRST => now.IsFirstHoliday(),
			DateNameEnum.HOLIDAY_LAST => now.IsLastHoliday(),
			DateNameEnum.HOLIDAY_AFTER => now.IsAfterHoliday(),
			DateNameEnum.RESTDAY => now.IsRestday(),
			DateNameEnum.RESTDAY_BEFORE => now.IsBeforeRestday(),
			DateNameEnum.RESTDAY_FIRST => now.IsFirstRestday(),
			DateNameEnum.RESTDAY_LAST => now.IsLastRestday(),
			DateNameEnum.RESTDAY_AFTER => now.IsAfterRestday(),

			DateNameEnum.YESTERDAY => now.EqualsDay(Main.DATE_NOW.AddDays(-1)),
			DateNameEnum.TODAY => now.EqualsDay(Main.DATE_NOW),
			DateNameEnum.TOMORROW => now.EqualsDay(Main.DATE_NOW.AddDays(1)),

			_ => false,
		};

		/// <inheritdoc/>
		protected override object Execute(SODictionary context, CancellationToken cancel) {
			// 获取时间
			var time = Main.DATE_NOW;

			// 验证日期
			var success = false;
			foreach (var name in Enums) {
				success = IsValidate(time, name);
				if (ValidateALL) {
					if (!success) {
						break;
					}
				} else if (success) {
					break;
				}
			}

			return Execute(success, context);
		}

		#endregion
	}
}