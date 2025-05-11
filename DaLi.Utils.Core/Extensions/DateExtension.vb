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
' 	日期扩展操作
'
' 	name: Extension.DateExtension
' 	create: 2020-10-23
' 	memo: 日期扩展操作
' 	
' ------------------------------------------------------------

Imports System.Runtime.CompilerServices

Namespace Extension

	''' <summary>日期扩展操作</summary>
	Public Module DateExtension

#Region "1. 格式转换"

		''' <summary>将当前时间转换成整数，仅转换1970-2050 年，精确到秒</summary>
		<Extension>
		Public Function ToInteger(this As Date) As Integer
			If this <= DATE_1970 OrElse this > DATE_2050 Then Return 0
			Return ((this.Ticks - TICKS_19700101) / 10000000) + Integer.MinValue
		End Function

		''' <summary>获取 Unix 时间戳，注意 2038 年问题，最小单位为秒</summary>
		<Extension>
		Public Function UnixTicks(this As Date) As Integer
			If this <= DATE_1970 OrElse this > DATE_2038 Then Return 0
			Return (this.ToUniversalTime.Ticks - TICKS_19700101) / 10000000
		End Function

		'''' <summary>扩展获取 Unix 时间戳，最小单位为秒，UINT 可以支持到 2106 年</summary>
		'<Extension>
		'Public Function UnixTicks2(this As Date) As UInteger
		'	this = this.Range(Date.UnixEpoch, New Date(2106, 1, 1))
		'	Return (this.ToUniversalTime.Ticks - TICKS_19700101) / 10000000
		'End Function

		''' <summary>获取 Js 时间戳，最小单位为毫秒</summary>
		<Extension>
		Public Function JsTicks(this As Date) As Long
			If this <= DATE_1970 Then Return 0
			Return (this.ToUniversalTime.Ticks - TICKS_19700101) / 10000
		End Function

		'''' <summary>将无符号整数转成时间，使用 UnixTicks 方式，最小单位：秒</summary>
		'<Extension>
		'Public Function ToDate(this As UInteger) As Date
		'	Return New Date((this * 10000000) + TICKS_19700101)
		'End Function

		''' <summary>将当前时间转换成整数，仅转换1970-2050 年</summary>
		''' <param name="isUnix">当前数值是否 Unix Ticks</param>
		<Extension>
		Public Function ToDate(this As Integer, Optional isUnix As Boolean = True) As Date
			Dim Ticks As Long = this

			If isUnix Then
				If Ticks < 0 Then Ticks = 0
			Else
				Ticks -= Integer.MinValue
			End If

			Return New Date((Ticks * 10000000) + TICKS_19700101)
		End Function

		''' <summary>将当前时间转换成整数，仅转换1970-2050 年</summary>
		''' <param name="isJs">当前数值是否 Js Ticks</param>
		<Extension>
		Public Function ToDate(this As Long, Optional isJs As Boolean = True) As Date
			If this < 0 Then this = 0
			If Not isJs Then Return New Date(this)

			Return New Date((this * 10000) + TICKS_19700101)
		End Function

		''' <summary>转换为 DateTime</summary>
		''' <param name="This">6位：yyMMdd / 8位：yyyyMMdd / 10,13位：Js Timer / 12位：yyMMddHHmmss / 14位：yyyyMMddHHmmss / 其他数字转换成Timespan / 字符则系统自动转换</param>
		''' <remarks>
		''' 支持部分字符表达式：<para />
		''' now: 当前时间<para />
		''' today: 当前日期<para />
		''' tomorrow: 明天日期<para />
		''' yesterday: 昨天日期<para />
		''' year: 今年第一天<para />
		''' month: 本月第一天<para />
		''' week: 本周第一天<para />
		''' year_end: 今年最后一天<para />
		''' month_end: 本月最后一天<para />
		''' week_end: 本周最后一天<para />
		''' </remarks>
		<Extension>
		Public Function ToDate(this As String, Optional defaultDate As Date = Nothing) As Date
			If this.IsEmpty Then Return defaultDate

			this = this.ToDBC.Trim.ToLowerInvariant
			Select Case this
				Case "now"
					Return DATE_NOW
				Case "today"
					Return DATE_NOW.Date
				Case "tomorrow"
					Return DATE_NOW.AddDays(1).Date
				Case "yesterday"
					Return DATE_NOW.AddDays(-1).Date
				Case "year"
					Return DATE_NOW.YearFirst
				Case "month"
					Return DATE_NOW.MonthFirst
				Case "week"
					Return DATE_NOW.WeekFirst
				Case "year_end"
					Return DATE_NOW.YearEnd
				Case "month_end"
					Return DATE_NOW.MonthEnd
				Case "week_end"
					Return DATE_NOW.WeekEnd
			End Select

			' 尝试直接转换
			Dim ret As Date
			If Date.TryParse(this, ret) Then Return ret

			' 非正整数返回默认值
			If Not this.IsUInt Then Return defaultDate

			' 纯数字
			Dim len = this.Length
			Select Case len
				Case 6, 8, 12, 14
					Dim Y As Integer = defaultDate.Year
					Dim M As Integer = defaultDate.Month
					Dim D As Integer = defaultDate.Day
					Dim hh As Integer = defaultDate.Hour
					Dim mm As Integer = defaultDate.Minute
					Dim ss As Integer = defaultDate.Second

					Select Case len
						Case 6, 12
							Y = 2000 + Integer.Parse(this.Substring(0, 2))
							M = Integer.Parse(this.Substring(2, 2))
							D = Integer.Parse(this.Substring(4, 2))
						Case 8, 14
							Y = Integer.Parse(this.Substring(0, 4))
							M = Integer.Parse(this.Substring(4, 2))
							D = Integer.Parse(this.Substring(6, 2))
					End Select

					Select Case len
						Case 12
							hh = Integer.Parse(this.Substring(6, 2))
							mm = Integer.Parse(this.Substring(8, 2))
							ss = Integer.Parse(this.Substring(10, 2))
						Case 14
							hh = Integer.Parse(this.Substring(8, 2))
							mm = Integer.Parse(this.Substring(10, 2))
							ss = Integer.Parse(this.Substring(12, 2))
					End Select

					Y = Math.Max(1, Math.Min(9999, Y))
					M = Math.Max(1, Math.Min(12, M))
					D = Math.Max(1, Math.Min(31, D))
					hh = Math.Max(0, Math.Min(59, hh))
					mm = Math.Max(0, Math.Min(59, mm))
					ss = Math.Max(0, Math.Min(59, ss))

					Return New Date(Y, M, D, hh, mm, ss)

				Case 10, 13
					Dim ticks = Long.Parse($"{this}0000000".Substring(0, 17))
					Return New Date(ticks + TICKS_19700101)

				Case Else
					Return New Date(Long.Parse(this))
			End Select
		End Function

		''' <summary>调整日期字符串组，将重复或者不规范的日期过滤</summary>
		''' <param name="This">日期数据，格式：YYYY-MM-dd 多个用逗号间隔</param>
		''' <remarks>2016-09-25</remarks>
		<Extension>
		Public Function ToDates(this As String, Optional defaultDate As Date = Nothing) As List(Of Date)
			If this.IsEmpty Then Return Nothing

			Dim arr = this.Split({","c, "，", ";"c, "；", vbCr, vbLf, vbCrLf, vbTab}, StringSplitOptions.RemoveEmptyEntries)
			If arr.Length = 0 Then Return Nothing

			Return arr.Select(Function(x) x.ToDate(defaultDate)).Distinct.ToList
		End Function

#End Region

#Region "2. 输出格式化"

		''' <summary>计算当前时间与过去时间的时间差（当前时间-过去时间)</summary>
		''' <param name="this">过去时间</param>
		''' <param name="target">当前时间</param>
		''' <param name="isEN">是否返回英文字符串</param>
		<Extension>
		Public Function ShowDiff(this As DateTimeOffset, Optional target As DateTimeOffset = Nothing, Optional isEN As Boolean = False) As String
			If target = DateTimeOffset.MinValue Then target = DATE_FULL_NOW

			Dim Ticks = Math.Abs(target.Subtract(this).Ticks)
			Return New TimeSpan(Ticks).Show(isEN)
		End Function

		''' <summary>计算当前时间与过去时间的时间差（当前时间-过去时间)</summary>
		''' <param name="this">过去时间</param>
		''' <param name="target">当前时间</param>
		''' <param name="isEN">是否返回英文字符串</param>
		<Extension>
		Public Function ShowDiff(this As Date, Optional target As Date = Nothing, Optional isEN As Boolean = False) As String
			If target = Date.MinValue Then target = DATE_NOW

			Dim Ticks = Math.Abs(target.Subtract(this).Ticks)
			Return New TimeSpan(Ticks).Show(isEN)
		End Function

		''' <summary>将秒换算成对应的时长</summary>
		<Extension>
		Public Function ToTimeSpan(this As Long) As String
			Return New TimeSpan(this).ToString("c")
		End Function

		''' <summary>将 TimeSpan 格式化</summary>
		''' <param name="isEN">是否返回英文字符串</param>
		<Extension>
		Public Function Show(this As TimeSpan, Optional isEN As Boolean = False) As String
			If this.Ticks < 1 Then Return "-"

			With New Text.StringBuilder
				If this.TotalSeconds > 30 Then
					If this.Days > 0 Then
						.Append(this.Days)
						.Append(If(isEN, "day ", "天 "))
					End If

					If this.Hours > 0 Then
						.Append(this.Hours)
						.Append(If(isEN, "hour ", "小时 "))
					End If

					If this.Minutes > 0 Then
						.Append(this.Minutes)
						.Append(If(isEN, "min ", "分钟 "))
					End If

					If this.Seconds > 0 Then
						.Append(this.Seconds)
						.Append(If(isEN, "sec", "秒"))
					End If
				ElseIf this.TotalSeconds > 1 Then
					.Append(this.TotalSeconds.ToString("0.00"))
					.Append(If(isEN, "sec", "秒"))
				Else
					.Append(this.TotalMilliseconds)
					.Append(If(isEN, "ms ", "毫秒"))
				End If

				Return .ToString
			End With
		End Function

#End Region

#Region "3. 计算操作"

		''' <summary>以指定时间为中心，获取所在频率内的有效时间范围</summary>
		''' <param name="this">目标时间(可空)</param>
		''' <param name="frequency">时间频率单位</param>
		''' <param name="interval">间隔周期数</param>
		''' <remarks>
		''' 实现逻辑：
		''' 1. 根据不同的时间频率单位进行分支处理
		''' 2. 每个分支先计算基本时间单位边界
		''' 3. 根据间隔参数调整时间范围
		''' 4. 返回包含毫秒级精度的开始结束时间
		''' 如 12:00:00 的 三小时，则：9:00:00 ~ 15:00:00 都是区域有效时间；结果根据频率，精确到对应单位，如：日及以上单位则按天计算；最小单位：秒</remarks>
		''' <returns>包含开始时间和结束时间的元组</returns>
		<Extension()>
		Public Function FrequencyCenter(this As Date?, Optional frequency As TimeFrequencyEnum = TimeFrequencyEnum.DAY, Optional interval As Integer = 1) As (DateStart As Date, DateEnd As Date)
			Dim dateQuery = If(this, DATE_NOW)
			Return dateQuery.FrequencyCenter(frequency, interval)
		End Function

		''' <summary>计算以指定时间为中心的有效时间范围</summary>
		''' <param name="this">目标时间</param>
		''' <param name="frequency">时间频率单位</param>
		''' <param name="interval">间隔周期数</param>
		''' <remarks>
		''' 实现逻辑：
		''' 1. 根据不同的时间频率单位进行分支处理
		''' 2. 每个分支先计算基本时间单位边界
		''' 3. 根据间隔参数调整时间范围
		''' 4. 返回包含毫秒级精度的开始结束时间
		''' 如 12:00:00 的 三小时，则：9:00:00 ~ 15:00:00 都是区域有效时间；结果根据频率，精确到对应单位，如：日及以上单位则按天计算；最小单位：秒</remarks>
		''' <returns>包含开始时间和结束时间的元组</returns>
		<Extension()>
		Public Function FrequencyCenter(this As Date, Optional frequency As TimeFrequencyEnum = TimeFrequencyEnum.DAY, Optional interval As Integer = 1) As (DateStart As Date, DateEnd As Date)
			If interval < 1 Then interval = 1

			Select Case frequency
				Case TimeFrequencyEnum.SECOND
					' 秒级精度处理
					Dim baseSecond = New DateTime(this.Year, this.Month, this.Day, this.Hour, this.Minute, this.Second)
					Dim start = baseSecond.AddSeconds(1).AddSeconds(-interval)
					Dim [end] = baseSecond.AddSeconds(interval).AddMilliseconds(-1)
					Return (start, [end])

				Case TimeFrequencyEnum.MINUTE
					' 分钟级精度处理
					Dim baseMinute = New DateTime(this.Year, this.Month, this.Day, this.Hour, this.Minute, 0)
					Dim start = baseMinute.AddMinutes(1).AddMinutes(-interval)
					Dim [end] = baseMinute.AddMinutes(interval).AddMilliseconds(-1)
					Return (start, [end])

				Case TimeFrequencyEnum.HOUR
					' 小时级精度处理
					Dim baseHour = New DateTime(this.Year, this.Month, this.Day, this.Hour, 0, 0)
					Dim start = baseHour.AddHours(1).AddHours(-interval)
					Dim [end] = baseHour.AddHours(interval).AddMilliseconds(-1)
					Return (start, [end])

				Case TimeFrequencyEnum.DAY
					' 天级精度处理
					Dim start = this.Date.AddDays(1).AddDays(-interval)
					Dim [end] = this.Date.AddDays(interval).AddMilliseconds(-1)
					Return (start, [end])

				Case TimeFrequencyEnum.WEEK
					' 周处理逻辑
					Dim dayOffset = If(this.DayOfWeek = Global.System.DayOfWeek.Sunday, 7, this.DayOfWeek)
					Dim weekStart = this.Date.AddDays(1 - dayOffset)
					Dim start = weekStart.AddDays(7 - (interval * 7))
					Dim [end] = weekStart.AddDays(interval * 7).AddMilliseconds(-1)
					Return (start, [end])

				Case TimeFrequencyEnum.MONTH
					' 月级精度处理
					Dim firstDayOfMonth = New DateTime(this.Year, this.Month, 1)
					Dim start = firstDayOfMonth.AddMonths(1 - interval)
					Dim [end] = firstDayOfMonth.AddMonths(interval).AddMilliseconds(-1)
					Return (start, [end])

				Case TimeFrequencyEnum.YEAR
					' 年级精度处理
					Dim firstDayOfYear = New DateTime(this.Year, 1, 1)
					Dim start = firstDayOfYear.AddYears(1 - interval)
					Dim [end] = firstDayOfYear.AddYears(interval).AddMilliseconds(-1)
					Return (start, [end])

				Case Else
					Return (this, this)
			End Select
		End Function

		''' <summary>
		''' 获取基于基准时间的频率有效时间范围。
		''' 获取以指定时间为基准，获取需要查询时间所在频率内的有效时间起始。如：2020年1月1日10点为基准，每3天为周期，那么 2020年1月4日12点 所在区域为 2020年1月4日10点至2020年1月7日9点末
		''' </summary>
		''' <param name="this">要查询的可空日期，如果为空则使用当前时间</param>
		''' <param name="frequency">时间频率类型，默认为天</param>
		''' <param name="interval">频率间隔（如：3表示每3个频率单位）</param>
		''' <param name="reference">基准时间，用于计算间隔的起始点</param>
		''' <returns>包含开始时间和结束时间的元组</returns>
		<Extension()>
		Public Function FrequencyStart(this As Date?, Optional frequency As TimeFrequencyEnum = TimeFrequencyEnum.DAY, Optional interval As Integer = 1, Optional reference As Date? = Nothing) As (DateStart As Date, DateEnd As Date)
			Dim dateQuery = If(this, DATE_NOW)
			Return dateQuery.FrequencyStart(frequency, interval, reference)
		End Function

		''' <summary>
		''' 获取基于基准时间的频率有效时间范围。
		''' 获取以指定时间为基准，获取需要查询时间所在频率内的有效时间起始。如：2020年1月1日10点为基准，每3天为周期，那么 2020年1月4日12点 所在区域为 2020年1月4日10点至2020年1月7日9点末
		''' </summary>
		''' <param name="this">要查询的日期</param>
		''' <param name="frequency">时间频率类型，默认为天</param>
		''' <param name="interval">频率间隔（如：3表示每3个频率单位）</param>
		''' <param name="reference">基准时间，用于计算间隔的起始点</param>
		''' <returns>包含开始时间和结束时间的元组</returns>
		<Extension()>
		Public Function FrequencyStart(this As Date, Optional frequency As TimeFrequencyEnum = TimeFrequencyEnum.DAY, Optional interval As Integer = 1, Optional reference As Date? = Nothing) As (DateStart As Date, DateEnd As Date)
			' 保证最小间隔为1
			Dim validInterval = Math.Max(interval, 1)
			Dim referenceTime = If(reference, Date.MinValue)

			' 根据不同的频率类型计算时间范围
			Select Case frequency
				Case TimeFrequencyEnum.SECOND
					Dim seconds = Math.Floor(this.Subtract(referenceTime).TotalSeconds / validInterval) * validInterval
					Dim start = referenceTime.AddSeconds(seconds)
					Dim [end] = start.AddSeconds(validInterval).AddMilliseconds(-1)
					Return (start, [end])

				Case TimeFrequencyEnum.MINUTE
					referenceTime = referenceTime.Date.AddHours(referenceTime.Hour).AddMinutes(referenceTime.Minute)
					Dim minutes = Math.Floor(this.Subtract(referenceTime).TotalMinutes / validInterval) * validInterval
					Dim start = referenceTime.AddMinutes(minutes)
					Dim [end] = start.AddMinutes(validInterval).AddMilliseconds(-1)
					Return (start, [end])

				Case TimeFrequencyEnum.HOUR
					referenceTime = referenceTime.Date.AddHours(referenceTime.Hour)
					Dim hours = Math.Floor(this.Subtract(referenceTime).TotalHours / validInterval) * validInterval
					Dim start = referenceTime.AddHours(hours)
					Dim [end] = start.AddHours(validInterval).AddMilliseconds(-1)
					Return (start, [end])

				Case TimeFrequencyEnum.DAY
					referenceTime = referenceTime.Date
					Dim days = Math.Floor(this.Subtract(referenceTime).TotalDays / validInterval) * validInterval
					Dim start = referenceTime.AddDays(days)
					Dim [end] = start.AddDays(validInterval).AddMilliseconds(-1)
					Return (start, [end])

				Case TimeFrequencyEnum.WEEK
					' 计算基于周的间隔（7天为1周）
					Dim totalDays = this.Subtract(referenceTime).TotalDays
					Dim weeks = Math.Floor(totalDays / (validInterval * 7)) * validInterval * 7
					Dim baseDate = referenceTime.AddDays(weeks)
					Dim start = baseDate.FrequencyCenter(TimeFrequencyEnum.WEEK, 1).DateStart
					Dim [end] = start.AddDays(validInterval * 7).AddMilliseconds(-1)
					Return (start, [end])

				Case TimeFrequencyEnum.MONTH
					referenceTime = New DateTime(referenceTime.Year, referenceTime.Month, 1)
					Dim monthDiff = ((this.Year - referenceTime.Year) * 12) + this.Month - referenceTime.Month
					Dim months = Math.Floor(monthDiff / validInterval) * validInterval
					Dim start = referenceTime.AddMonths(months)
					Dim [end] = start.AddMonths(validInterval).AddMilliseconds(-1)
					Return (start, [end])

				Case TimeFrequencyEnum.YEAR
					Dim yearDiff = this.Year - referenceTime.Year
					Dim years = CInt(Math.Floor(yearDiff / validInterval) * validInterval)
					Dim start = referenceTime.AddYears(years)
					Dim [end] = start.AddYears(validInterval).AddMilliseconds(-1)
					Return (start, [end])

				Case Else
					Return (this, this)
			End Select
		End Function

		''' <summary>计算指定日期所在月的日数量</summary>
		''' <param name="this">指定日期</param>
		''' <returns>日数量</returns>
		<Extension()>
		Public Function MonthDays(this As Date) As Integer
			Return New Date(this.Year, this.Month, 1).AddMonths(1).AddSeconds(-1).Day
		End Function

		''' <summary>计算指定日期所在年的日数量</summary>
		''' <param name="this">指定日期</param>
		''' <returns>日数量</returns>
		<Extension()>
		Public Function YearDays(this As Date) As Integer
			Dim ds As New Date(this.Year, 1, 1)
			Dim de As New Date(this.Year, 12, 31)
			Return de.Subtract(ds).TotalDays
		End Function

		'----------------------------------------------------------

		'''' <summary>获取以指定时间为基准，获取需要查询时间所在频率内的有效时间起始。如：2020年1月1日10点为基准，每3天为周期，那么 2020年1月4日12点所在区域为 2020年1月3日10点至2020年1月5日9点末</summary>
		'''' <param name="This">要操作的时间</param>
		'''' <param name="frequencyOption">频率</param>
		'''' <param name="delayValue">频率周期，如3分钟，5小时</param>
		'''' <param name="baseDate">基准日期，用于计算间隔的初始日期，未设置则为 1年1月1日，根据周期将自动精确到指定单位</param>
		'''' <returns>DateStart: 开始时间；DateEnd：结束时间</returns>
		'<Extension>
		'Public Function Frequency(this As Date, frequencyOption As TimeFrequencyEnum, delayValue As Integer, baseDate As Date) As (DateStart As Date, DateEnd As Date)
		'	Return this.FrequencyStart(frequencyOption, delayValue, baseDate)
		'End Function

		'''' <summary>获取指定时间的所在频率内的有效时间起始。如:12:00:00 的 三小时，则：9:00:00 ~ 15:00:00 都是区域有效时间；结果根据频率，精确到对应单位，如：日及以上单位则按天计算；最小单位：秒</summary>
		'''' <param name="This">指定时间</param>
		'''' <param name="frequencyOption">频率</param>
		'''' <param name="delayValue">频率周期，如3分钟，5小时</param>
		'''' <remarks>如12:00:00 的 三小时，则：9:00:00 ~ 15:00:00 都是区域有效时间；结果根据频率，精确到对应单位，如：日及以上单位则按天计算；最小单位：秒</remarks>
		'''' <returns>DateStart: 开始时间；DateEnd：结束时间</returns>
		'<Extension>
		'Public Function Frequency(this As Date?, frequencyOption As TimeFrequencyEnum, delayValue As Integer) As (DateStart As Date, DateEnd As Date)
		'	Dim dateQuery As Date = If(this Is Nothing, DATE_NOW, this)
		'	Return dateQuery.Frequency(frequencyOption, delayValue)
		'End Function

		'''' <summary>获取指定时间的所在频率内的有效时间起始。如:12:00:00 的 三小时，则：9:00:00 ~ 15:00:00 都是区域有效时间；结果根据频率，精确到对应单位，如：日及以上单位则按天计算；最小单位：秒</summary>
		'''' <param name="This">要操作的时间，基准时间以 2000-1-1 为准</param>
		'''' <param name="frequencyOption">频率</param>
		'''' <param name="delayValue">频率周期，如3分钟，5小时</param>
		'''' <returns>DateStart: 开始时间；DateEnd：结束时间</returns>
		'<Extension>
		'Public Function Frequency(this As Date, frequencyOption As TimeFrequencyEnum, delayValue As Integer) As (DateStart As Date, DateEnd As Date)
		'	Return this.FrequencyCenter(frequencyOption, delayValue)
		'End Function

		'----------------------------------------------------------

#End Region

#Region "4. 基本日期判断"

		''' <summary>当前日期是否有效，大于 2000 年才有效</summary>
		<Extension>
		Public Function IsValidate(this As Date) As Boolean
			Return this >= DATE_2000
		End Function

		''' <summary>当前日期是否有效，大于 2000 年才有效</summary>
		<Extension>
		Public Function IsValidate(this As Date?) As Boolean
			Return this IsNot Nothing AndAlso this >= DATE_2000
		End Function

		''' <summary>指定日期是不是在日期组中</summary>
		''' <param name="this">要操作的时间</param>
		''' <param name="days">日期组</param>
		<Extension>
		Public Function IsInDays(this As Date, days As IEnumerable(Of Date)) As Boolean
			If days Is Nothing Then Return False
			Return days.Any(Function(x) x.Date = this.Date)
		End Function

		''' <summary>指定日期是不是在日期组中</summary>
		''' <param name="this">要操作的时间</param>
		''' <param name="days">日期组</param>
		<Extension>
		Public Function IsInDays(this As Date, ParamArray days As Date()) As Boolean
			If days Is Nothing Then Return False
			Return days.Any(Function(x) x.Date = this.Date)
		End Function

		''' <summary>指定日期是不是在日期组中</summary>
		''' <param name="This">要操作的时间</param>
		''' <param name="Days">日期字符串组，逗号间隔</param>
		<Extension>
		Public Function IsInDays(this As Date, days As String) As Boolean
			Return this.IsInDays(days.ToDateList)
		End Function

		''' <summary>是否一个月的第一天</summary>
		<Extension>
		Public Function IsMonthBegin(this As Date) As Boolean
			Return this.Day = 1
		End Function

		''' <summary>是否一个月的第最后一天</summary>
		<Extension>
		Public Function IsMonthEnd(this As Date) As Boolean
			Return this.Day = New Date(this.Year, this.Month, 1).AddMonths(1).AddDays(-1).Day
		End Function

		''' <summary>是否周一</summary>
		<Extension>
		Public Function IsMonday(this As Date) As Boolean
			Return this.DayOfWeek = DayOfWeek.Monday
		End Function

		''' <summary>是否周二</summary>
		<Extension>
		Public Function IsTuesday(this As Date) As Boolean
			Return this.DayOfWeek = DayOfWeek.Tuesday
		End Function

		''' <summary>是否周三</summary>
		<Extension>
		Public Function IsWednesday(this As Date) As Boolean
			Return this.DayOfWeek = DayOfWeek.Wednesday
		End Function

		''' <summary>是否周四</summary>
		<Extension>
		Public Function IsThursday(this As Date) As Boolean
			Return this.DayOfWeek = DayOfWeek.Thursday
		End Function

		''' <summary>是否周五</summary>
		<Extension>
		Public Function IsFriday(this As Date) As Boolean
			Return this.DayOfWeek = DayOfWeek.Friday
		End Function

		''' <summary>是否周六</summary>
		<Extension>
		Public Function IsSaturday(this As Date) As Boolean
			Return this.DayOfWeek = DayOfWeek.Saturday
		End Function

		''' <summary>是否周日</summary>
		<Extension>
		Public Function IsSunday(this As Date) As Boolean
			Return this.DayOfWeek = DayOfWeek.Sunday
		End Function

		''' <summary>是不是周末</summary>
		<Extension>
		Public Function IsWeekend(this As Date) As Boolean
			Return this.IsSunday OrElse this.IsSaturday
		End Function

		''' <summary>是不是国家法定假期</summary>
		<Extension>
		Public Function IsHoliday(this As Date) As Boolean
			Return this.IsInDays(DATE_HOLIDAY)
		End Function

		''' <summary>是不是法定假期前一天</summary>
		''' <remarks>今天工作，明天假日</remarks>
		<Extension>
		Public Function IsBeforeHoliday(this As Date) As Boolean
			Return Not IsHoliday(this) AndAlso IsHoliday(this.AddDays(1))
		End Function

		''' <summary>是不是法定假期第一天</summary>
		''' <remarks>昨天工作，今天假日</remarks>
		<Extension>
		Public Function IsFirstHoliday(this As Date) As Boolean
			Return Not IsHoliday(this.AddDays(-1)) AndAlso IsHoliday(this)
		End Function

		''' <summary>是不是法定假期最后一天</summary>
		''' <remarks>今天假日，明天工作</remarks>
		<Extension>
		Public Function IsLastHoliday(this As Date) As Boolean
			Return IsHoliday(this) AndAlso Not IsHoliday(this.AddDays(1))
		End Function

		''' <summary>是不是法定假期后一天</summary>
		''' <remarks>昨天假日，今天工作</remarks>
		<Extension>
		Public Function IsAfterHoliday(this As Date) As Boolean
			Return IsHoliday(this.AddDays(-1)) AndAlso Not IsHoliday(this)
		End Function

		''' <summary>是不是法定调整工作日</summary>
		<Extension>
		Public Function IsAdjustday(this As Date) As Boolean
			Return this.IsInDays(DATE_ADJUST)
		End Function

		''' <summary>是否工作日</summary>
		<Extension>
		Public Function IsWorkday(this As Date) As Boolean
			Return Not IsRestday(this)
		End Function

		''' <summary>是否月第一个工作日</summary>
		<Extension>
		Public Function IsMonthWorkFirst(this As Date) As Boolean
			Return this.MonthWorkFirst.EqualsDay(this)
		End Function

		''' <summary>是否月最后一个工作日</summary>
		<Extension>
		Public Function IsMonthWorkEnd(this As Date) As Boolean
			Return this.MonthWorkEnd.EqualsDay(this)
		End Function

		''' <summary>是否周第一个工作日</summary>
		<Extension>
		Public Function IsWeekWorkFirst(this As Date) As Boolean
			Return this.WeekWorkFirst.EqualsDay(this)
		End Function

		''' <summary>是否周最后一个工作日</summary>
		<Extension>
		Public Function IsWeekWorkEnd(this As Date) As Boolean
			Return this.WeekWorkEnd.EqualsDay(this)
		End Function

		''' <summary>是不是休息日，含周末和国家法定假期</summary>
		<Extension>
		Public Function IsRestday(this As Date) As Boolean
			' 假期直接为休息日
			' 周末且不是调休日
			Return IsHoliday(this) OrElse (IsWeekend(this) And Not IsAdjustday(this))
		End Function

		''' <summary>是不是休息日前一天</summary>
		''' <remarks>今天工作，明天假日</remarks>
		<Extension>
		Public Function IsBeforeRestday(this As Date) As Boolean
			Return Not IsRestday(this) AndAlso IsRestday(this.AddDays(1))
		End Function

		''' <summary>是不是休息日第一天</summary>
		''' <remarks>昨天工作，今天假日</remarks>
		<Extension>
		Public Function IsFirstRestday(d As Date) As Boolean
			Return Not IsRestday(d.AddDays(-1)) AndAlso IsRestday(d)
		End Function

		''' <summary>是不是休息日最后一天</summary>
		''' <remarks>今天假日，明天工作</remarks>
		<Extension>
		Public Function IsLastRestday(this As Date) As Boolean
			Return IsRestday(this) AndAlso Not IsRestday(this.AddDays(1))
		End Function

		''' <summary>是不是休息日后一天</summary>
		''' <remarks>昨天假日，今天工作</remarks>
		<Extension>
		Public Function IsAfterRestday(this As Date) As Boolean
			Return IsRestday(this.AddDays(-1)) AndAlso Not IsRestday(this)
		End Function

		''' <summary>是不是周末和国家法定假期</summary>
		<Extension>
		Public Function IsWeekendHoliday(this As Date) As Boolean
			Return IsWeekend(this) Or IsHoliday(this)
		End Function

		''' <summary>是不是周末和国家法定假期</summary>
		<Extension>
		Public Function IsSundayHoliday(this As Date) As Boolean
			Return IsSunday(this) Or IsHoliday(this)
		End Function

		''' <summary>是不是工作时段；开始结束相等，一天都工作，开始大于结束，则凌晨到开始，开始到结束休息，结束到凌晨工作</summary>
		''' <param name="this">当前时间</param>
		''' <param name="timeStart">工作开始时间（时分）</param>
		''' <param name="timeStop">工作结束时间（时分）</param>
		''' <remarks>2016-09-25 增加</remarks>
		<Extension>
		Public Function IsTimeWork(this As Date, timeStart As Date, timeStop As Date) As Boolean
			Return this.IsTimeWork(timeStart.Hour, timeStart.Minute, timeStop.Hour, timeStop.Minute)
		End Function

		''' <summary>是不是工作时段；开始结束相等，一天都工作，开始大于结束，则凌晨到开始，开始到结束休息，结束到凌晨工作</summary>
		''' <param name="this">当前时间</param>
		''' <param name="hourStart">开始小时</param>
		''' <param name="minuteStart">开始分钟</param>
		''' <param name="hourStop">结束小时</param>
		''' <param name="minuteStop">结束分钟</param>
		<Extension>
		Public Function IsTimeWork(this As Date, hourStart As Integer, minuteStart As Integer, hourStop As Integer, minuteStop As Integer) As Boolean
			' 如果开始和结束时间都相等表示全天都工作
			' 如果开始时间比结束时间晚则表示工作到第二天了
			Dim ds = this.Date.AddHours(hourStart).AddMinutes(minuteStart)
			Dim de = this.Date.AddHours(hourStop).AddMinutes(minuteStop)

			If ds < de Then
				Return this <= de And this >= ds
			ElseIf ds > de Then
				Return this <= de Or this >= ds
			Else
				Return True
			End If
		End Function

		''' <summary>计算两个时间是否相等，比较日期以及小时、分钟、秒钟是否相等</summary>
		<Extension>
		Public Function EqualsSecond(this As Date, target As Date) As Boolean
			Return this.Second = target.Second AndAlso EqualsMinute(this, target)
		End Function

		''' <summary>计算两个时间是否相等，比较日期以及小时、分钟是否相等</summary>
		<Extension>
		Public Function EqualsMinute(this As Date, target As Date) As Boolean
			Return this.Minute = target.Minute AndAlso EqualsHour(this, target)
		End Function

		''' <summary>计算两个时间是否相等，比较日期以及小时是否相等</summary>
		<Extension>
		Public Function EqualsHour(this As Date, target As Date) As Boolean
			Return this.Hour = target.Hour AndAlso EqualsDay(this, target)
		End Function

		''' <summary>计算两个时间是否相等，比较日期是否相等</summary>
		<Extension>
		Public Function EqualsDay(this As Date, target As Date) As Boolean
			Return this.Date = target.Date
		End Function

		''' <summary>计算两个时间是否相等，比较日期是否相等</summary>
		<Extension>
		Public Function EqualsMonth(this As Date, target As Date) As Boolean
			Return this.Month = target.Month AndAlso this.EqualsYear(target)
		End Function

		''' <summary>计算两个时间是否相等，比较日期是否相等</summary>
		<Extension>
		Public Function EqualsYear(this As Date, target As Date) As Boolean
			Return this.Year = target.Year
		End Function

#End Region

#Region "5. 日期获取"

		''' <summary>计算指定一天的凌晨零点</summary>
		<Extension>
		Public Function DayFirst(this As Date) As Date
			Return this.Date
		End Function

		''' <summary>计算指定一天的最后时刻</summary>
		<Extension>
		Public Function DayEnd(this As Date) As Date
			Return this.Date.AddDays(1).AddMilliseconds(-1)
		End Function

		''' <summary>计算指定日期所在的周一</summary>
		<Extension>
		Public Function WeekFirst(this As Date) As Date
			Dim w As Integer = this.DayOfWeek
			If w = 0 Then w = 7

			Return this.Date.AddDays(1 - w)
		End Function

		''' <summary>计算指定日期所在的周日</summary>
		<Extension>
		Public Function WeekEnd(this As Date) As Date
			Dim w As Integer = this.DayOfWeek
			If w = 0 Then w = 7

			Return this.Date.AddDays(8 - w).AddMilliseconds(-1)
		End Function

		''' <summary>计算指定日期的月初时刻</summary>
		<Extension>
		Public Function MonthFirst(this As Date) As Date
			Return New Date(this.Year, this.Month, 1)
		End Function

		''' <summary>计算指定日期的月末时刻</summary>
		<Extension>
		Public Function MonthEnd(this As Date) As Date
			Dim value = this.AddMonths(1)
			Return New Date(value.Year, value.Month, 1).AddMilliseconds(-1)
		End Function

		''' <summary>计算指定日期的年初时刻</summary>
		<Extension>
		Public Function YearFirst(this As Date) As Date
			Return New Date(this.Year, 1, 1)
		End Function

		''' <summary>计算指定年末时刻</summary>
		<Extension>
		Public Function YearEnd(this As Date) As Date
			Return New Date(this.Year + 1, 1, 1).AddMilliseconds(-1)
		End Function

		'------------------------------------------------------------------------------------------

		''' <summary>一周第一个工作日</summary>
		<Extension>
		Public Function WeekWorkFirst(this As Date) As Date
			' 获取周一
			Dim w As Integer = this.DayOfWeek
			If w = 0 Then w = 7
			Dim day = this.Date.AddDays(1 - w)

			Return day.FindWorkDay(7, True)
		End Function

		''' <summary>计算指定日期所在的周日</summary>
		<Extension>
		Public Function WeekWorkEnd(this As Date) As Date
			' 获取周日
			Dim w As Integer = this.DayOfWeek
			If w = 0 Then w = 7
			Dim day = this.Date.AddDays(7 - w)

			Return day.FindWorkDay(7, False)
		End Function

		''' <summary>计算指定日期的月初时刻</summary>
		<Extension>
		Public Function MonthWorkFirst(this As Date) As Date
			Dim day = New Date(this.Year, this.Month, 1)
			Return day.FindWorkDay(day.MonthDays, True)
		End Function

		''' <summary>计算指定日期的月末时刻</summary>
		<Extension>
		Public Function MonthWorkEnd(this As Date) As Date
			Dim day = New Date(this.Year, this.Month, 1).AddMonths(1).AddSeconds(-1).Date
			Return day.FindWorkDay(day.Day, False)
		End Function

		''' <summary>计算指定日期的年初时刻</summary>
		<Extension>
		Public Function YearWorkFirst(this As Date) As Date
			Dim day = New Date(this.Year, 1, 1)
			Dim len As Integer = day.AddYears(1).Subtract(day).TotalDays
			Return day.FindWorkDay(len, True)
		End Function

		''' <summary>计算指定年末时刻</summary>
		<Extension>
		Public Function YearWorkEnd(this As Date) As Date
			Dim day = New Date(this.Year, 12, 31)
			Dim len As Integer = day.Subtract(New Date(this.Year, 1, 1)).TotalDays
			Return day.FindWorkDay(len, False)
		End Function

		''' <summary>计算初始日期指定长度内的工作日</summary>
		''' <param name="len">长度</param>
		''' <param name="forward">是否向前</param>
		''' <returns>获取到的工作日，无法获取时返回最小日期</returns>
		<Extension()>
		Public Function FindWorkDay(this As Date, len As Integer, Optional forward As Boolean = True) As Date
			If len < 1 Then len = 1
			Dim interval = If(forward, 1, -1)

			For i As Integer = 1 To len
				If this.IsWorkday() Then Return this
				this = this.AddDays(interval)
			Next

			Return Date.MinValue
		End Function

#End Region

	End Module
End Namespace