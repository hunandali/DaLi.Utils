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
' 	后台服务状态操作
'
' 	name: Interface.IBackServiceStatusProvider
' 	create: 2024-07-03
' 	memo: 后台服务状态操作
'
' ------------------------------------------------------------

Namespace [Interface]
	''' <summary>后台服务状态操作</summary>
	Public Interface IBackServiceStatusProvider
		Inherits IStatusProvider(Of BackServiceStatus)

		''' <summary>远程服务列表</summary>
		ReadOnly Property Services As List(Of BackServiceStatus)

		''' <summary>根据类型获取远程服务列表</summary>
		ReadOnly Property Services(type As String) As List(Of BackServiceStatus)

		''' <summary>根据类型获取远程服务</summary>
		ReadOnly Property Service(id As String) As BackServiceStatus

	End Interface
End Namespace
