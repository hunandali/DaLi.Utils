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
' 	name: Interface.IServiceSingleton
' 	create: 2024-07-03
' 	memo: 系统服务自动注入接口，基于此接口的插件都将在系统系统时自动注入 service.AddSingleton
'
' ------------------------------------------------------------

Namespace [Interface]
	''' <summary>插件接口</summary>
	''' <remarks>
	''' Singleton 类服务。
	''' 每次都是获得同一个实例，单一实例模式：单一实例对象对每个对象和每个请求都是相同的，可以说是不同客户端不同请求都是相同。
	''' 项目启动-项目关闭 相当于静态类 只会有一个。
	''' </remarks>
	Public Interface IServiceSingleton
		Inherits IBase
	End Interface
End Namespace
