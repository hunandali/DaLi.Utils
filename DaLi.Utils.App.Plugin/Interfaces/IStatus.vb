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
' 	状态接口
'
' 	name: Interface.IStatus
' 	create: 2024-07-02
' 	memo: 状态接口
'
' ------------------------------------------------------------

Namespace [Interface]
	''' <summary>状态接口</summary>
	Public Interface IStatus

		''' <summary>服务标识</summary>
		Property ID As String

		''' <summary>服务名称</summary>
		Property Name As String

		''' <summary>开始时间</summary>
		Property TimeStart As Date

		''' <summary>最后执行时间</summary>
		Property TimeLast As Date

		''' <summary>是否在忙</summary>
		Property IsBusy As Boolean

		''' <summary>历史消息</summary>
		Property Messages As List(Of StatusMessage)

		''' <summary>状态信息</summary>
		Property Information As KeyValueDictionary

	End Interface
End Namespace
