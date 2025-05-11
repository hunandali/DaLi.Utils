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
' 	WebApi 操作结果
'
' 	name: Http.ApiResult
' 	create: 2022-12-15
' 	memo: WebApi 操作结果
' 	
' ------------------------------------------------------------

Namespace Http

	''' <summary>基类</summary>
	Public Class ApiResult(Of T)

		''' <summary>是否成功</summary>
		Public Property Data As T

		''' <summary>结果代码，1000 以下与 statusCode 一致</summary>
		Public Property Code As Integer

		''' <summary>消息内容，当消息方式为 Page 跳转时，此内容为跳转地址</summary>
		Public Property Message As String

		''' <summary>请求队列ID</summary>
		Public Property TraceId As String

		''' <summary>当前请求的域名</summary>
		Public Property Host As String
	End Class

	''' <summary>基类</summary>
	Public Class ApiResult
		Inherits ApiResult(Of Object)
	End Class

End Namespace
