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
' 	当前时间
'
' 	name: Auto.Rule.TimeNow
' 	create: 2024-08-31
' 	memo: 当前时间
' 	
' ------------------------------------------------------------

Imports System.Threading

Namespace Auto.Rule

	''' <summary>当前时间</summary>
	Public Class TimeNow
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "当前时间"
			End Get
		End Property

		''' <summary>是否返回 UTC 时间</summary>
		Public Property UTC As Boolean

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage, cancel As CancellationToken) As Object
			' 忽略错误
			ErrorIgnore = True

			message.SetSuccess()
			Return If(UTC, DATE_NOW.ToUniversalTime, DATE_NOW)
		End Function

#End Region

	End Class
End Namespace
