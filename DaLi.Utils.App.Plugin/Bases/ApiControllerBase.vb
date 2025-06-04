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
' 	控制器基类
'
' 	name: Base.ApiControllerBase
' 	create: 2023-02-14
' 	memo: 控制器基类
'
' ------------------------------------------------------------

Imports Microsoft.AspNetCore.Authorization
Imports Microsoft.AspNetCore.Mvc
Imports Microsoft.Extensions.Caching.Distributed
Imports Microsoft.Extensions.Caching.Memory

Namespace Base

	''' <summary>控制器基类</summary>
	<ApiController>
	<AllowAnonymous>
	Public MustInherit Class ApiControllerBase
		Inherits ControllerBase

#Region "属性"

		''' <summary>内存缓存</summary>
		Private _Memory As IMemoryCache

		''' <summary>内存缓存</summary>
		Protected ReadOnly Property Memory As IMemoryCache
			Get
				If _Memory Is Nothing Then _Memory = GetService(Of IMemoryCache)()
				Return _Memory
			End Get
		End Property

		''' <summary>缓存</summary>
		Private _Cache As IDistributedCache

		''' <summary>缓存</summary>
		Protected ReadOnly Property Cache As IDistributedCache
			Get
				If _Cache Is Nothing Then _Cache = GetService(Of IDistributedCache)()
				Return _Cache
			End Get
		End Property

		''' <summary>错误消息项目</summary>
		Public Overridable ReadOnly Property ErrorMessage As New ErrorMessage

		''' <summary>本地化语言</summary>
		Protected Function Localizer(name As String, Optional args As Object() = Nothing) As String
			If args.IsEmpty Then
				Return SYS.Localizer(name)
			Else
				Return SYS.Localizer(name, args)
			End If
		End Function

		''' <summary>本地化语言</summary>
		Protected Function Localizer(name As String, prefix As String) As String
			Return SYS.Localizer(name, prefix)
		End Function

		''' <summary>获取参数</summary>
		Protected Function GetSetting(Of T As ISetting)() As T
			Return SYS.GetSetting(Of T)
		End Function

		''' <summary>获取注入项目</summary>
		Protected Function GetService(Of T)() As T
			Return SYS.GetService(Of T)
		End Function

#End Region

#Region "结果"

		''' <summary>输出文本结果</summary>
		Public Shared Function Text(data As String, Optional contentType As String = "application/javascript; charset=utf-8") As IActionResult
			Return New ResponseContent(data, contentType).ActionResult
		End Function

		''' <summary>输出 html</summary>
		Public Shared Function Html(data As String) As IActionResult
			Return New ResponseContent(data).ActionResult
		End Function

		''' <summary>返回成功结果</summary>
		Public Function Succ(Optional data As Object = Nothing, Optional message As String = "") As IActionResult
			Return New ResponseJson(If(data, ""), Localizer(message).EmptyValue(ErrorMessage.SuccessMessage), HttpContext).ActionResult
		End Function

		''' <summary>返回失败结果</summary>
		Public Function Err(code As Integer, message As String) As IActionResult
			Return New ResponseJson(code, Localizer(message), HttpContext).ActionResult
		End Function

		''' <summary>返回失败结果</summary>
		Public Function Err(message As String) As IActionResult
			Return Err(400, message.EmptyValue("Error.NotFound"))
		End Function

		''' <summary>返回表单失败结果</summary>
		Public Function Err(data As Object, Optional code As Integer = 400) As IActionResult
			Return ResponseJson.Default(code, data, HttpContext)
		End Function

		''' <summary>返回表单失败结果</summary>
		Public Function Err(Optional statusCode As Integer? = Nothing) As IActionResult
			'' 如果未设置错误提示，则使用无效参数
			'If ErrorMessage.IsPass Then Return Succ
			If ErrorMessage.IsPass AndAlso statusCode Is Nothing Then Return Err(400, "Error.Invalid")
			Return ResponseJson.Err(ErrorMessage, HttpContext, statusCode)
		End Function

#End Region

	End Class

End Namespace