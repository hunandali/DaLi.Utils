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
' 	设备状态操作
'
' 	name: Interface.IDeviceStatusProvider
' 	create: 2024-07-03
' 	memo: 设备状态操作
'
' ------------------------------------------------------------

Namespace [Interface]
	''' <summary>设备状态操作</summary>
	Public Interface IDeviceStatusProvider
		Inherits IStatusProvider(Of DeviceStatus)

		''' <summary>设备注册</summary>
		Sub Register(dev As DeviceStatus)

		''' <summary>设备注册</summary>
		Sub Register(id As Long, Optional name As String = "", Optional type As String = "", Optional machine As String = "", Optional ip As String = "", Optional information As KeyValueDictionary = Nothing)

		''' <summary>注销注册</summary>
		Sub Unregister(dev As DeviceStatus)

		''' <summary>注销注册</summary>
		Sub Unregister(id As Long)

		''' <summary>当前设备列表，含名称、设备、IP、类型</summary>
		ReadOnly Property Devices As Dictionary(Of String, (Name As String, Machine As String, IP As String, Type As String, Display As String))

		''' <summary>当前设备列表</summary>
		ReadOnly Property Devs As Dictionary(Of String, String)

		''' <summary>获取当前指定类型设备列表，使用模糊搜索类型</summary>
		ReadOnly Property Devs(type As String) As Dictionary(Of String, String)

		''' <summary>是被是否在指定列表中</summary>
		Function HasDev(devId As String, ParamArray devIds As String()) As Boolean

		'''' <summary>是否包含指定设备</summary>
		'Function HasDev(ParamArray devIds As String()) As Boolean

		''' <summary>是否包含当前设备</summary>
		Function IsCurrent(ParamArray devIds As String()) As Boolean

		''' <summary>更新字典中应用信息，如果选择不限则其他应用无需选择</summary>
		Function Update(ParamArray devIds As String()) As String()

		''' <summary>清理离线设备</summary>
		''' <param name="day">离线时长(天)</param>
		Function RemoveOffline(day As Integer) As Integer
	End Interface
End Namespace
