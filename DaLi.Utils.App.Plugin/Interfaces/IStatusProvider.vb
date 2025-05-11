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
' 	状态操作
'
' 	name: Interface.IStatusProvider
' 	create: 2024-07-02
' 	memo: 状态操作
'
' ------------------------------------------------------------

Namespace [Interface]

	''' <summary>状态操作</summary>
	Public Interface IStatusProvider(Of T As IStatus)
		Inherits IServiceSingleton

		''' <summary>获取所有状态信息</summary>
		ReadOnly Property All As IDictionary(Of String, T)

		''' <summary>获取、设置当前状态</summary>
		Property Item(id As String) As T

		''' <summary>获取、设置是否忙</summary>
		Property IsBusy(id As String) As Boolean

		''' <summary>获取状态消息</summary>
		ReadOnly Property Information(id As String) As KeyValueDictionary

		''' <summary>启动时间</summary>
		Property TimeStart(id As String) As Date

		''' <summary>最后更新时间</summary>
		Property TimeLast(id As String) As Date

		''' <summary>名称</summary>
		Property Name(id As String) As String

		''' <summary>获取状态，如果不存在使用默认值，并将默认值存入缓存</summary>
		Function [Get](id As String, Optional defaultValue As T = Nothing) As T

		''' <summary>设置值</summary>
		Sub [Set](Of S)(id As String, key As String, value As S)

		''' <summary>获取值</summary>
		Function [Get](Of S)(id As String, key As String) As S

		''' <summary>获取所有消息</summary>
		Function GetMessages(Of V As StatusMessage)(id As String) As List(Of V)

		''' <summary>插入消息并记录最新状态</summary>
		Sub SetMessage(Of V As StatusMessage)(id As String, message As V)

		''' <summary>插入消息并记录最新状态</summary>
		Sub SetMessage(id As String, type As StatusMessageEnum, message As String)

		''' <summary>设置状态消息</summary>
		Sub SetInformation(Of S)(id As String, key As String, value As S)

		''' <summary>设置状态消息</summary>
		Sub SetInformation(id As String, information As KeyValueDictionary)

		''' <summary>获取状态消息</summary>
		Function GetInformation(Of S)(id As String, key As String) As S

		''' <summary>移除项目</summary>
		Sub Remove(ParamArray ids() As String)

		''' <summary>清除所有项目</summary>
		Sub Clear()

	End Interface
End Namespace
