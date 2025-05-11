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
' 	系统服务自动注入接口
'
' 	name: Interface.IServiceScoped
' 	create: 2024-07-03
' 	memo: 系统服务自动注入接口，基于此接口的插件都将在系统系统时自动注入 service.AddScoped
'
' ------------------------------------------------------------

Namespace [Interface]
	''' <summary>插件接口</summary>
	''' <remarks>
	''' Scoped 类服务。
	''' 对于同一个请求返回同一个实例，不同的请求返回不同的实例，作用域模式：作用域对象在一个客户端请求中是相同的，但在多个客户端请求中是不同的。
	''' 请求开始-请求结束 在这次请求中获取的对象都是同一个。
	''' </remarks>
	Public Interface IServiceScoped
		Inherits IBase
	End Interface
End Namespace
