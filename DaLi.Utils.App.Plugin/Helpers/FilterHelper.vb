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
' 	过滤器操作帮助类
'
' 	name: FilterHelper
' 	create: 2025-06-05
' 	memo: 过滤器操作
'
' ------------------------------------------------------------

Imports Microsoft.AspNetCore.Http
Imports Microsoft.AspNetCore.Mvc.Filters

Namespace Helper
	''' <summary>过滤器操作</summary>
	Public NotInheritable Class FilterHelper

		''' <summary>用于过滤器中检查是否需要强制中止结果输出</summary>
		Public Const FILTER_STOPED = "FILTER_STOPED"

		''' <summary>设置过滤器是否已经执行完成，完整则后续不能在继续操作</summary>
		Public Shared Property FilterStatus(http As HttpContext) As Boolean
			Get
				Return http?.ContextItem(Of Boolean)(FILTER_STOPED)
			End Get
			Set(value As Boolean)
				http?.ContextItem(FILTER_STOPED, value)
			End Set
		End Property

		''' <summary>设置过滤器已经执行完成状态</summary>
		Public Shared Sub FilterStoped(context As ActionExecutingContext, message As String, Optional statusCode As Integer = 400)
			context.Result = ResponseJson.Err(statusCode, message, context.HttpContext)
			context.HttpContext.ContextItem(FILTER_STOPED, True)
		End Sub

		''' <summary>获取过滤器是否已经执行完成状态</summary>
		Public Shared Function FilterStoped(context As ResultExecutedContext) As Boolean
			Return context.HttpContext.ContextItem(Of Boolean)(FILTER_STOPED)
		End Function

	End Class
End Namespace
