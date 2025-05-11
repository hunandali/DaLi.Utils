' ------------------------------------------------------------
'
' 	Copyright © 2021 湖南大沥网络科技有限公司.
' 	Dali.Utils Is licensed under Mulan PSL v2.
'
' 	  author:	木炭(WOODCOAL)
' 	   email:	i@woodcoal.cn
' 	homepage:	http://www.hunandali.com/
'
' 	请依据 Mulan PSL v2 的条款使用本项目。获取 Mulan PSL v2 请浏览 http://license.coscl.org.cn/MulanPSL2
'
' ------------------------------------------------------------
'
' 	公共工具控制器
'
' 	name: Controller.ToolsController
' 	create: 2024-08-20
' 	memo: 公共工具控制器
' 	
' ------------------------------------------------------------

Imports System.ComponentModel
Imports Microsoft.AspNetCore.Mvc

Namespace Controller

	''' <summary>公共工具控制器</summary>
	<NoLog>
	<Route("tools")>
	Partial Public Class ToolsController
		Inherits ApiControllerBase

		''' <summary>分析日期信息</summary>
		<Description("分析日期信息")>
		<HttpGet("date/{value}")>
		<ResponseCache(Duration:=864000)>
		Public Function DateInfo(Optional value As Date = Nothing) As IActionResult
			If Not value.IsValidate Then value = DATE_NOW

			Return Succ(New With {
				.Value = value.ToString("yyyy-MM-dd"),
				value.Year, value.Month, value.Day,
				.Week = value.DayOfWeek, value.DayOfYear,
				value.IsWorkday, value.IsWeekend, value.IsAdjustday, value.IsWeekendHoliday, value.IsSundayHoliday,
				value.IsRestday, value.IsBeforeRestday, value.IsFirstRestday, value.IsLastRestday, value.IsAfterRestday,
				value.IsHoliday, value.IsBeforeHoliday, value.IsFirstHoliday, value.IsLastHoliday, value.IsAfterHoliday,
				value.IsMonthBegin, value.IsMonthEnd,
				value.WeekFirst, value.WeekEnd, value.WeekWorkFirst, value.WeekWorkEnd,
				value.MonthFirst, value.MonthEnd, value.MonthWorkFirst, value.MonthWorkEnd,
				value.YearFirst, value.YearEnd, value.YearWorkFirst, value.YearWorkEnd,
				value.Ticks, value.JsTicks, value.UnixTicks
			})
		End Function

		''' <summary>获取节假日数据</summary>
		<Description("获取节假日数据")>
		<HttpGet("holiday/{year}")>
		<ResponseCache(Duration:=864000, VaryByQueryKeys:={"year"})>
		Public Function Holiday(Optional year As Integer = 0) As IActionResult
			If year < 2000 Then Return Err

			Return Succ(
				New With {
					.Holiday = DATE_HOLIDAY.Where(Function(x) x.Year = year).Select(Function(x) x.ToString("yyyy-MM-dd")),
					.Adjustday = DATE_ADJUST.Where(Function(x) x.Year = year).Select(Function(x) x.ToString("yyyy-MM-dd"))
				}
			)
		End Function

		''' <summary>Cron 校验</summary>
		''' <param name="value">cron 表达式</param>
		''' <param name="time">用于校验的时间</param>
		''' <param name="onlyDay">仅校验日期</param>
		<Description("Cron 校验")>
		<HttpGet("cron")>
		<ResponseCache(Duration:=864000, VaryByQueryKeys:={"value", "time", "onlyDay"})>
		Public Function Cron(value As String, Optional time As Date = Nothing, Optional onlyDay As Boolean = False) As IActionResult
			If value.IsEmpty Then Return Err

			If Not time.IsValidate Then time = DATE_NOW
			Dim sources = value.SplitDistinct(vbCrLf)

			Dim exps = Misc.Cron.Expression.Update(sources, onlyDay)
			Dim desc = Misc.Cron.Expression.Description(exps, onlyDay)
			Dim timeup = Misc.Cron.Expression.Timeup(exps, time, Nothing, onlyDay)

			Return Succ(New With {
						.Source = value,
						.Expression = exps,
						.Description = desc,
						.IsTimeUp = timeup,
						.Date = time,
						onlyDay
					})
		End Function
	End Class

End Namespace
