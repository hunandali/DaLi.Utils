' ------------------------------------------------------------
'
' 	Copyright © 2021 湖南大沥网络科技有限公司.
' 	Dali.Utils Is licensed under Mulan PSL v2.
'
' 	  author:	木炭(WOODCOAL)
' 	   email:	a@hndl.vip
' 	homepage:	http://www.hunandali.com/
'
' 	请依据 Mulan PSL v2 的条款使用本项目。获取 Mulan PSL v2 请浏览 http://license.coscl.org.cn/MulanPSL2
'
' ------------------------------------------------------------
'
' 	模板文本处理
'
' 	name: TemplateExtension
' 	create: 2025-03-10
' 	memo: 模板文本处理。本项目是对 DaLi.Utils.Core.CSharp 模板文本处理的扩展，注意功能上的区分。此处增加了更多模板命令
'
' ------------------------------------------------------------

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports DaLi.Utils.Json
Imports DaLi.Utils.Template

Namespace Extension

	''' <summary>模板文本处理</summary>
	Public Module TemplateExtension

#Region "默认模板处理实例"

		''' <summary>默认模板处理实例</summary>
		Private ReadOnly _Default As New Lazy(Of TemplateAction)(Function()
																	 Dim instance As New TemplateAction(Text.Encoding.UTF8)
																	 TemplateCommands.Register(instance)
																	 ExtensionCommands.Register(instance)
																	 Return instance
																 End Function)

		''' <summary>默认模板处理实例</summary>
		Public ReadOnly Property [Default] As TemplateAction
			Get
				Return _Default.Value
			End Get
		End Property

#End Region

#Region "模板格式化操作"

		''' <summary>使用数据字典格式化模板，默认使用 {} 前后缀</summary>
		''' <param name="template">模板文本</param>
		''' <param name="data">数据字典</param>
		''' <param name="clearTag">对于字典中不存在的标签是否清除</param>
		''' <returns>格式化后的结果</returns>
		<Extension>
		Public Function FormatTemplate(template As String, data As IDictionary(Of String, Object), Optional clearTag As Boolean = False) As Object
			Return FormatTemplate(template, data, "{"c, "}"c, clearTag, True)
		End Function

		''' <summary>使用数据字典格式化模板</summary>
		''' <param name="template">模板文本</param>
		''' <param name="data">数据字典</param>
		''' <param name="prefix">前缀字符</param>
		''' <param name="suffix">后缀字符</param>
		''' <param name="clearTag">对于字典中不存在的标签是否清除</param>
		''' <param name="skipAttribute">是否跳过标签属性的处理</param>
		''' <returns>格式化后的结果</returns>
		<Extension>
		Public Function FormatTemplate(template As String, data As IDictionary(Of String, Object), Optional prefix As Char = "{"c, Optional suffix As Char = "}"c, Optional clearTag As Boolean = False, Optional skipAttribute As Boolean = False) As Object
			Return [Default].FormatTemplate(template, data, prefix, suffix, clearTag, skipAttribute)
		End Function

#End Region

#Region "注册模板事件"

		Public Class ExtensionCommands

#Region "	公共操作"

			''' <summary>注册当前所有的命令，会覆盖原有命令</summary>
			Public Shared Sub Register(action As TemplateAction)
				If action Is Nothing Then Return

				Dim cmds = AllCommands()
				For Each cmd In cmds
					action.Register(cmd.Key, cmd.Value, True)
				Next
			End Sub

			''' <summary>获取当前所有的命令</summary>
			Public Shared Function AllCommands() As Dictionary(Of String, Func(Of Object, SSDictionary, Object))
				Dim currentType = GetType(ExtensionCommands)

				' 获取所有公共静态只读字段
				Dim fields = currentType.GetFields(BindingFlags.Public Or BindingFlags.Static Or BindingFlags.GetField)
				Dim baseType = GetType(Func(Of Object, SSDictionary, Object))

				Dim cmds As New Dictionary(Of String, Func(Of Object, SSDictionary, Object))

				For Each field In fields
					' 检查字段类型是否为 Func<object, SSDictionary, object>
					If field.FieldType Is baseType Then
						' 获取字段的值
						cmds.Add(field.Name, field.GetValue(Nothing))
					End If
				Next

				Return cmds
			End Function

			''' <summary>尝试获取字符串</summary>
			''' <param name="value">原始内容</param>
			''' <param name="enEmpty">是否允许空字符串</param>
			''' <param name="str">返回的字符串</param>
			''' <returns>原始内容为字符串返回 true，否则返回 false。如果不允许空字符串且原始内容为空字符串则返回 false</returns>
			Private Shared Function TryGetString(value As Object, enEmpty As Boolean, ByRef str As String) As Boolean
				If value Is Nothing Then Return False
				If Not value.GetType.IsString Then Return False

				str = value.ToString
				Return enEmpty Or str.NotEmpty
			End Function

			''' <summary>日期计算</summary>
			''' <param name="value">日期字符串，无效则使用当前时间</param>
			''' <param name="exps">计算表达式，多个逗号间隔。如：1m, 2y ... 为避免冲突，秒使用字母 i 代替</param>
			Private Shared Function DateMath(value As Date, exps As String) As Date
				If exps.IsEmpty Then Return value

				For Each exp In exps.ToLowerInvariant.Split(ArrayExtension.SEPARATOR_DEFAULT_BASE)
					If exp.NotEmpty Then
						Dim v = exp.ToDouble(True)

						Select Case exp.Right(1)
							Case "s"
								value = value.AddSeconds(v)
							Case "i"
								value = value.AddMinutes(v)
							Case "h"
								value = value.AddHours(v)
							Case "m"
								value = value.AddMonths(v)
							Case "y"
								value = value.AddYears(v)
							Case "w"
								value = value.AddDays(v * 7)
							Case Else
								value = value.AddDays(v)
						End Select
					End If
				Next
				Return value
			End Function

			''' <summary>返回标识</summary>
			''' <param name="value">原始结果</param>
			''' <param name="flag">状态标识</param>
			''' <param name="attrs">属性</param>
			Private Shared Function ReturnFlag(value As Object, flag As Boolean, attrs As SSDictionary) As Object
				Dim temp = attrs(If(flag, "true", "false"))
				If temp.IsNull Then Return flag

				Return temp.Replace("[*]", value)
			End Function

			''' <summary>返回标识</summary>
			''' <param name="value">原始结果</param>
			''' <param name="flag">状态标识</param>
			''' <param name="attrs">属性</param>
			Private Shared Function ReturnFlag(value As Object, flag As Boolean?, attrs As SSDictionary) As Object
				If flag.HasValue Then Return ReturnFlag(value, flag.Value, attrs)
				Return value
			End Function

#End Region

#Region "	基础操作"

			''' <summary>清理 Html</summary>
			Public Shared ReadOnly Clear As Func(Of Object, SSDictionary, Object) =
			Function(value, attrs)
				Dim str = ""
				If Not TryGetString(value, False, str) Then Return value

				Dim cmd = attrs("clear").ToLowerInvariant
				Select Case cmd
					Case "trim"
						Return str.Trim

					Case "trimall"
						Return str.TrimFull

					Case "space"
						Return str.ClearSpace

					Case "control"
						Return str.ClearControl

					Case Else
						Return str.ClearHtml(cmd)
				End Select
			End Function

			''' <summary>使用正则表达式替换 replace / replace.to</summary>
			''' <remarks>为防止标签替换产生异常，正在表达式中 {} 由 \{ \} 代替</remarks>
			Public Shared ReadOnly Replace As Func(Of Object, SSDictionary, Object) =
				Function(value, attrs)
					Dim str = ""
					If Not TryGetString(value, False, str) Then Return value

					Dim strReplace = attrs("replace")
					Dim strTo = attrs("to")

					Return str.ReplaceRegex(strReplace, strTo)
				End Function

			''' <summary>截取 cut / cut.inc</summary>
			''' <remarks>
			''' cut(string) = cut(pattern)
			''' cut(number) = cut(length)
			''' cut.inc 是否包含本身
			''' cut.false = 截取不到内容返回的内容
			''' </remarks>
			Public Shared ReadOnly Cut As Func(Of Object, SSDictionary, Object) =
				Function(value, attrs)
					Dim str = ""
					If Not TryGetString(value, False, str) Then Return value

					Dim strCut = attrs("cut")

					If strCut.IsNumber Then
						Return str.Cut(strCut.ToInteger)
					Else
						If attrs("inc").ToBoolean Then
							' 包含本身
							str = str.Cut(strCut, 0, False)
						Else
							' 不包含本身
							str = str.Cut(strCut, 1, False)
						End If

						If str.IsNull Then str = attrs("false")
						Return str
					End If
				End Function

			''' <summary>截取左侧字符串</summary>
			Public Shared ReadOnly Left As Func(Of Object, SSDictionary, Object) =
				Function(value, attrs)
					Dim str = ""
					If Not TryGetString(value, False, str) Then Return value

					Dim intLeft = attrs("left").ToInteger
					Dim strMore = attrs("more")

					Return str.Left(intLeft, strMore)
				End Function

			''' <summary>截取右侧字符串</summary>
			Public Shared ReadOnly Right As Func(Of Object, SSDictionary, Object) =
				Function(value, attrs)
					Dim str = ""
					If Not TryGetString(value, False, str) Then Return value

					Dim intRight = attrs("right").ToInteger
					Dim strMore = attrs("more")

					Return str.Right(intRight, strMore)
				End Function

			''' <summary>截取中间字符串，参数用逗号间隔</summary>
			''' <remarks>
			''' 1. 值包含两部分，用逗号间隔时，左侧为开始位置，右侧为长度
			''' 2. 值仅包含一个部分，则默认为开始位置
			''' 3. 单独参数方式：
			'''		- mid.start 表示开始位置
			'''		- mid.end 表示结束位置
			''' </remarks>
			Public Shared ReadOnly mid As Func(Of Object, SSDictionary, Object) =
				Function(value, attrs)
					Dim str = ""
					If Not TryGetString(value, False, str) Then Return value

					Dim start = 0
					Dim length = 0

					Dim strMid = attrs("mid")
					If strMid.IsEmpty Then
						start = attrs("start").ToInteger

						Dim strEnd = attrs("end")
						If strEnd.IsEmpty Then
							length = attrs("length").ToInteger
						Else
							length = str.Length - strEnd.ToInteger
						End If
					Else
						Dim mids = $"{strMid},0".Split(","c)
						start = mids(0).ToInteger
						length = mids(1).ToInteger
					End If

					If start < 0 Or start > str.Length Then Return ""
					If length < 0 Then
						Return str.Substring(start)
					Else
						If start + length > str.Length Then Return length = str.Length - start
						Return str.Substring(start, length)
					End If
				End Function

			''' <summary>格式化，可以加随机码干扰，对于数字会自动加上这个值，对于文字则直接附加此数据</summary>
			''' <remarks></remarks>
			Public Shared ReadOnly Format As Func(Of Object, SSDictionary, Object) =
				Function(value, attrs)
					If value Is Nothing Then Return Nothing

					' 格式化内容为空直接返回文本
					Dim strFormat = attrs("format")
					If strFormat.IsEmpty Then Return value.ToString

					Try
						Dim type = value.GetType

						' 非文本直接使用 String.Format 处理
						If Not type.IsString Then Return String.Format(strFormat, value)

						' 文本特殊格式处理
						Dim str = value.ToString
						If str.IsEmpty Then Return ""

						Select Case strFormat
							Case "date"
								Return str.ToDate("yyyy-MM-dd")

							Case "time"
								Return str.ToDate("HH:mm:ss")

							Case "datetime"
								Return str.ToDate("yyyy-MM-dd HH:mm:ss")

							Case "ticks"
								Return str.ToDateTime.Ticks

							Case "jsticks"
								Return str.ToDateTime.JsTicks

							Case "unixticks"
								Return str.ToDateTime.UnixTicks

							Case "num", "number", "decimal"
								Return str.ToNumber

							Case "int", "integer"
								Return str.ToInteger

							Case "long"
								Return str.ToLong

							Case "size"
								Return str.ToDouble.ToSize

							Case "ucase", "upper"
								Return str.ToUpperInvariant

							Case "lcase", "lower"
								Return str.ToLowerInvariant

							Case "pinyin"
								Return str.ToPinYin

							Case "ascii"
								Return str.GetAscii

							Case "dbc"
								Return str.ToDBC

							Case "sbc"
								Return str.ToSBC

							Case "car"
								Return str.ToCar

							Case "phone"
								Return str.ToPhone

							Case "mobi", "mobile", "mobilephone"
								Return str.ToMobilePhone

							Case "len"
								' 文字长度
								Return str.Length

							Case Else
								If value.IsDateTime Then
									Return str.ToDate(strFormat)

								ElseIf value.IsNumber Then
									Return str.ToNumber.ToString(strFormat)

								Else
									Return String.Format(strFormat, str)
								End If
						End Select
					Catch ex As Exception
						Return ""
					End Try
				End Function

			''' <summary>保留头尾，中间省略</summary>
			Public Shared ReadOnly Show As Func(Of Object, SSDictionary, Object) =
				Function(value, attrs)
					Dim str = ""
					If Not TryGetString(value, False, str) Then Return value
					Return str.ShortShow(attrs("show").ToInteger, attrs("more"))
				End Function

#End Region

#Region "	编码解码"

			''' <summary>编码</summary>
			Public Shared ReadOnly Encode As Func(Of Object, SSDictionary, Object) =
				Function(value, attrs)
					Dim strCode = attrs("encode").ToLowerInvariant
					If strCode.IsEmpty Then Return value

					Dim str = ""
					If Not TryGetString(value, False, str) Then
						' json 序列化
						If strCode = "json" Then Return Json.JsonExtension.ToJson(value)
						Return value
					End If

					Select Case strCode
						Case "js"
							' JS 脚本转义，可以用于将数据转换成 JSON
							Return str.EncodeJs

						Case "url"
							Return str.EncodeUrl

						Case "html"
							Return str.EncodeHtml

						Case "md5"
							Return str.MD5(True)

						Case "sha1"
							Return str.SHA1

						Case "base64"
							Return str.EncodeBase64

						Case Else
							Return value
					End Select
				End Function

			''' <summary>解码</summary>
			Public Shared ReadOnly DeCode As Func(Of Object, SSDictionary, Object) =
				Function(value, attrs)
					Dim str = ""
					If Not TryGetString(value, False, str) Then Return value

					Dim strCode = attrs("decode").ToLowerInvariant
					If strCode.IsEmpty Then Return value

					Select Case strCode
						Case "js"
							' JS 脚本转义，可以用于将数据转换成 JSON
							Return str.DecodeJs

						Case "url"
							Return str.DecodeUrl

						Case "html"
							Return str.DecodeHtml

						Case "base64"
							Return str.DecodeBase64

						Case "json"
							Return str.FromJson()

						Case Else
							Return value
					End Select
				End Function

			''' <summary>Xor 异或加密</summary>
			Public Shared ReadOnly [XOR] As Func(Of Object, SSDictionary, Object) =
				Function(value, attrs)
					Dim str = ""
					If Not TryGetString(value, False, str) Then Return value

					Dim strCode = attrs("xor")
					If strCode.IsEmpty Then Return value

					Return str.XorCoder(strCode)
				End Function

#End Region

#Region "	计算"

			''' <summary>计算，保留小数位数</summary>
			''' <remarks>
			''' math = 表达式，原值部分用 [*] 表示<para />
			''' math.fixed = 保留小数位数<para />
			''' 如：计算周长，保留两位小数：math="[*] * 2 * 3.14", math.fixed="2"<para />
			''' 返回数值
			''' </remarks>
			Public Shared ReadOnly Math As Func(Of Object, SSDictionary, Object) =
				Function(value, attrs)
					If value Is Nothing Then Return Nothing

					' 格式化内容为空直接返回文本
					Dim strMath = attrs("math")
					If strMath.IsEmpty Then Return value

					Dim type = value.GetType

					' 非文本或者数值不处理
					If Not type.IsString OrElse Not type.IsNumber Then Return value

					' 数值处理
					Dim fixed = attrs("fixed").ToInteger
					Dim val = strMath.Replace("[*]", value.ToString).Compute

					If fixed < 0 Then
						Return val
					Else
						Return val.ToFixed(fixed)
					End If
				End Function

			''' <summary>相加计算，相减请使用负数即可</summary>
			''' <remarks>如果为日期则使用时间计算</remarks>
			Public Shared ReadOnly Add As Func(Of Object, SSDictionary, Object) =
				Function(value, attrs)
					If value Is Nothing Then Return Nothing

					' 格式化内容为空直接返回文本
					Dim strAdd = attrs("add")
					If strAdd.IsEmpty Then Return value

					Dim type = value.GetType

					Dim mathNumber = Function(num As Decimal) num + strAdd.ToInteger

					' 文本
					If type.IsString Then
						Dim str = value.ToString
						If str.IsNumber Then Return mathNumber(str.ToNumber)
						If str.IsDateWithExp Then Return DateMath(str.ToDate, strAdd)

						Return String.Empty
					End If

					' 数值处理
					If type.IsNumber Then Return mathNumber(value)
					If type.IsDate Then Return DateMath(value, strAdd)

					Return value
				End Function

			''' <summary>与当前日期计算</summary>
			''' <remarks>与 add 操作取值相反，原始内容为用于时间加减的数据，属性值为默认的时间</remarks>
			Public Shared ReadOnly [Date] As Func(Of Object, SSDictionary, Object) =
		Function(value, attrs)
			If value Is Nothing Then Return Nothing

			Dim type = value.GetType
			If Not type.IsNumber AndAlso Not type.IsString Then Return value

			' 格式化内容为空直接返回文本
			Dim strDate = attrs("date")
			Dim vDate = If(strDate.IsDateWithExp, strDate.ToDate, DATE_NOW)

			Return DateMath(vDate, value.ToString)
		End Function

#End Region

#Region "	判断"

			''' <summary>是否为指定类型</summary>
			''' <remarks>is / is.true / is.false</remarks>
			Public Shared ReadOnly [IS] As Func(Of Object, SSDictionary, Object) =
				Function(value, attrs)
					If value Is Nothing Then Return Nothing

					Dim strIs = attrs("is")
					Dim flag As Boolean? = Nothing

					Dim type = value.GetType

					' 对于字符值且为日期，尝试转换成日期格式
					If type.IsString AndAlso value.ToString.IsDateWithExp Then
						value = value.ToString.ToDate
						type = value.GetType
					End If

					Select Case type.GetTypeCode
						Case TypeCode.Boolean
							flag = strIs.IsSame({"boolean", "bool"}) OrElse (True.Equals(value) AndAlso strIs.IsSame("true")) OrElse (False.Equals(value) AndAlso strIs.IsSame("false"))

						Case TypeCode.Char
							flag = strIs.IsSame({"num", "number", "char"})

						Case TypeCode.SByte
							flag = strIs.IsSame({"num", "number", "sbyte"})

						Case TypeCode.Byte
							flag = strIs.IsSame({"num", "number", "byte"})

						Case TypeCode.Int16
							flag = strIs.IsSame({"num", "number", "int16", "short"})

						Case TypeCode.UInt16
							flag = strIs.IsSame({"num", "number", "uint16", "ushort"})

						Case TypeCode.Int32
							flag = strIs.IsSame({"num", "number", "int32", "int", "integer"})

						Case TypeCode.UInt32
							flag = strIs.IsSame({"num", "number", "uint32", "uint", "uinteger"})

						Case TypeCode.Int64
							flag = strIs.IsSame({"num", "number", "int64", "long"})

						Case TypeCode.UInt64
							flag = strIs.IsSame({"num", "number", "uint64", "ulong"})

						Case TypeCode.Single
							flag = strIs.IsSame({"num", "number", "single", "float"})

						Case TypeCode.Double
							flag = strIs.IsSame({"num", "number", "double"})

						Case TypeCode.Decimal
							flag = strIs.IsSame({"num", "number", "decimal"})

						Case TypeCode.DateTime
							strIs = strIs.ToLowerInvariant
							Dim time As Date = value

							Select Case strIs
								Case "date", "time", "datetime"
									flag = True

								Case "monthbegin"
									flag = time.IsMonthBegin

								Case "monthend"
									flag = time.IsMonthEnd

								Case "monday", "mon"
									flag = time.IsMonday

								Case "tuesday", "tue"
									flag = time.IsTuesday

								Case "wednesday", "wed"
									flag = time.IsWednesday

								Case "thursday", "thu"
									flag = time.IsThursday

								Case "friday", "fri"
									flag = time.IsFriday

								Case "saturday", "sat"
									flag = time.IsSaturday

								Case "sunday", "sun"
									flag = time.IsSunday

								Case "weekend"
									flag = time.IsWeekend

								Case "holiday"
									flag = time.IsHoliday

								Case "beforeholiday", "bh"
									flag = time.IsBeforeHoliday

								Case "firstholiday", "fh"
									flag = time.IsFirstHoliday

								Case "lastholiday", "lh"
									flag = time.IsLastHoliday

								Case "afterholiday", "ah"
									flag = time.IsAfterHoliday

								Case "adjustday", "adj"
									flag = time.IsAdjustday

								Case "workday", "work"
									flag = time.IsWorkday

								Case "restday"
									flag = time.IsRestday

								Case "beforerestday", "br"
									flag = time.IsBeforeRestday

								Case "firstrestday", "fr"
									flag = time.IsFirstRestday

								Case "lastrestday", "lr"
									flag = time.IsLastRestday

								Case "afterrestday", "ar"
									flag = time.IsAfterRestday

								Case "weekendholiday", "wh"
									flag = time.IsWeekendHoliday

								Case "sundayholiday", "sh"
									flag = time.IsSundayHoliday

								Case "cron"
									Dim strExp = attrs("exp")
									flag = Misc.Cron.Expression.Timeup(strExp, time, Nothing, False)

								Case "cronday"
									Dim strExp = attrs("exp")
									flag = Misc.Cron.Expression.Timeup(strExp, time, Nothing, True)

								Case Else
									If strIs.StartsWith("cron:") Then
										' 是否匹配 Cron 表达式
										Dim strCron = strIs.Substring(5)
										If strCron.NotEmpty Then flag = Misc.Cron.Expression.Timeup(strCron, time, Nothing, False)

									ElseIf strIs.StartsWith("cronday:") Then
										' 是否匹配 Cron 表达式，日模式
										Dim strCron = strIs.Substring(8)
										If strCron.NotEmpty Then flag = Misc.Cron.Expression.Timeup(strCron, time, Nothing, True)
									End If
							End Select

						Case TypeCode.String
							' 文本处理
							Dim str = value.ToString

							Select Case strIs
								Case "empty"
									flag = str.IsEmpty

								Case "email"
									flag = str.IsEmail

								Case "guid"
									flag = str.IsGUID

								Case "ip"
									flag = str.IsIP

								Case "ipv4"
									flag = str.IsIPv4

								Case "ipv6"
									flag = str.IsIPv6

								Case "phone"
									flag = str.IsPhone

								Case "mobile", "mobilephone"
									flag = str.IsMobilePhone

								Case "cid", "cardid"
									flag = str.IsCardID

								Case "businessid", "business"
									flag = str.IsBusinessID

								Case "passport"
									flag = str.IsPassport

								Case "hkmo"
									flag = str.IsHKMO

								Case "taiwan"
									flag = str.IsTaiWan

								Case "uint"
									flag = str.IsUInt

								Case "integer"
									flag = str.IsInteger

								Case "number"
									flag = str.IsNumber

								Case "letternumber"
									flag = str.IsLetterNumber

								Case "username"
									flag = str.IsUserName(value.Length, True)

								Case "password"
									flag = str.IsPassword

								Case "url"
									flag = str.IsUrl

								Case "md5", "hash"
									flag = str.IsMD5Hash

								Case "chinese"
									flag = str.IsChinese

								Case "ascii"
									flag = str.IsAscii

								Case "car"
									flag = str.IsCar

								Case "datetime"
									flag = str.IsDateTime

								Case "date"
									flag = str.IsDate

								Case "time"
									flag = str.IsTime

								Case "json"
									flag = str.IsJson

								Case "xml"
									flag = str.IsXml

							End Select

					End Select

					' 返回结果
					Return ReturnFlag(value, flag, attrs)
				End Function

#End Region

#Region "	数组"

			''' <summary>分解数组，并重新组合或者返回指定索引值，注意：重复值将被过滤</summary>
			''' <remarks>
			''' split 分隔符号；<para />
			''' split.join 重新组合的字符串，如果为数值则直接返回指定索引值<para />
			''' split.template 模板
			''' split.distinct 是否过滤重复项目<para />
			''' </remarks>
			Public Shared ReadOnly Split As Func(Of Object, SSDictionary, Object) =
				Function(value, attrs)
					Dim str = ""
					If Not TryGetString(value, False, str) Then Return value

					' 检查分隔符
					Dim strSplit = attrs("split")
					If strSplit.IsEmpty Then Return value

					' 处理数组
					Dim arr = str.Split(strSplit, StringSplitOptions.RemoveEmptyEntries)
					If arr.IsEmpty Then Return ""

					Dim strJoin = attrs("join")
					Dim strTemp = attrs("template")
					Dim distinct = attrs("distinct").ToBoolean

					' 数值，直接返回指定索引
					If strJoin.IsInteger Then
						Dim index = strJoin.ToInteger
						If arr.Length > index AndAlso index > -1 Then
							If strTemp.Contains("[*]") Then
								Return strTemp.Replace("[*]", arr(index)).GetDateTime("[", "]")
							Else
								Return arr(index)
							End If
						Else
							Return ""
						End If
					End If

					' 重新组合
					Dim ret = arr.Where(Function(x) x.NotNull).
								Select(Function(x)
										   x = x.TrimFull

										   If strTemp.Contains("[*]") Then
											   Return strTemp.Replace("[*]", x).GetDateTime("[", "]")
										   Else
											   Return x
										   End If
									   End Function)

					If distinct Then ret.Distinct

					Return ret.JoinString(strJoin)
				End Function

#End Region

#Region "	从字典中获取值"

			''' <summary>从字典中获取值</summary>
			''' <remarks>k1:v1|k2:v2|……|kn:Vn</remarks>
			Public Shared ReadOnly From As Func(Of Object, SSDictionary, Object) =
				Function(value, attrs)
					If value Is Nothing Then Return Nothing

					Dim str = value.ToString
					If str.IsEmpty Then Return Nothing

					Dim arr = attrs("from").DecodeHtml.SplitDistinct("|")
					If arr.IsEmpty Then Return ""

					Dim Nvs As New SSDictionary
					For Each line In arr
						If line.IsEmpty Then Continue For

						Dim kv = $"{line}:".Split(":"c)
						If kv(0).IsEmpty Then Continue For

						If kv(1).IsEmpty Then kv(1) = kv(0)
						Nvs(kv(0)) = kv(1).DecodeLine
					Next

					Return Nvs(str)
				End Function

		End Class

#End Region

#End Region

	End Module

End Namespace
