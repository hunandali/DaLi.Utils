﻿' ------------------------------------------------------------
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
' 	调试
'
' 	name: Auto.Rule.Debug
' 	create: 2023-01-13
' 	memo: 调试
' 	
' ------------------------------------------------------------

Imports System.Threading

Namespace Auto.Rule

	''' <summary>调试</summary>
	Public Class Debug
		Inherits RuleBase

#Region "PROPERTY"

		''' <summary>规则名称</summary>
		Public Overrides ReadOnly Property Name As String
			Get
				Return "调试"
			End Get
		End Property

		''' <summary>原始内容</summary>
		Public Property Content As String

#End Region

#Region "EXECUTE"

		''' <summary>执行操作，并返回当前的变量及相关值</summary>
		Protected Overrides Function ExecuteRule(data As IDictionary(Of String, Object), message As AutoMessage, cancel As CancellationToken) As Object
			' 忽略错误
			ErrorIgnore = True
			Output = Nothing

			Dim source = AutoHelper.GetVarString(Content, data)
			message.SetSuccess(True, $"{Content} => {source}")

			Return Nothing
		End Function

#End Region

	End Class
End Namespace
