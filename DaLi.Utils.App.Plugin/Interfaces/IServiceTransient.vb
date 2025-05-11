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
' 	name: Interface.IServiceTransient
' 	create: 2024-07-03
' 	memo: 系统服务自动注入接口，基于此接口的插件都将在系统系统时自动注入 service.AddTransient
'
' ------------------------------------------------------------

Namespace [Interface]
	''' <summary>插件接口</summary>
	''' <remarks>
	''' Transient 类服务。
	''' 每次 service 请求都是获得不同的实例，暂时性模式：暂时性对象始终不同，无论是不是同一个请求（同一个请求里的不同服务）同一个客户端， 每次都是创建新的实例。
	''' 请求获取-（GC回收 - 主动释放） 每一次获取的对象都不是同一个。
	''' </remarks>
	Public Interface IServiceTransient
		Inherits IBase
	End Interface
End Namespace
