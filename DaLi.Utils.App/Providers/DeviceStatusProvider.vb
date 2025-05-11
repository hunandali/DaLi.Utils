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
' 	name: Provider.DeviceStatusProvider
' 	create: 2024-07-03
' 	memo: 设备状态操作
'
' ------------------------------------------------------------

Namespace Provider

	''' <summary>设备状态操作</summary>
	Public Class DeviceStatusProvider
		Inherits StatusProvider(Of DeviceStatus)
		Implements IDeviceStatusProvider

		''' <summary>重新获取设备简介时间，单位：分钟</summary>
		Private Const DEVICE_RELOAD_INTERVAL = 5

		''' <summary>缓存设备信息</summary>
		Private _Dev_List As Dictionary(Of String, (Name As String, Machine As String, IP As String, Type As String, Display As String))

		''' <summary>最后操作时间</summary>
		Private _Dev_Last As Date

		''' <summary>安全锁</summary>
		Private ReadOnly _Lock As New Object

		Public Sub New()
			MyBase.New(VAR_REDIS_STATUS_DEVICE)
		End Sub

		''' <summary>设备注册</summary>
		Public Sub Register(dev As DeviceStatus) Implements IDeviceStatusProvider.Register
			If dev Is Nothing OrElse dev.ID < 1 Then Return

			Register(dev.ID, dev.Name, dev.Type, dev.Machine, dev.IP, dev.Information)
		End Sub

		''' <summary>设备注册</summary>
		Public Sub Register(id As Long, Optional name As String = "", Optional type As String = "", Optional machine As String = "", Optional ip As String = "", Optional information As KeyValueDictionary = Nothing) Implements IDeviceStatusProvider.Register
			If id < 1 Then Return

			' 检查状态，不存在自动创建
			Dim status = [Get](id, New DeviceStatus With {.ID = id, .Name = name, .Type = type, .Machine = machine, .IP = ip, .Information = information})

			If name.NotEmpty AndAlso status.Name <> name Then [Set](id, "Name", name)
			If machine.NotEmpty AndAlso status.Machine <> machine Then [Set](id, "Machine", machine)
			If ip.NotEmpty AndAlso status.IP <> ip Then [Set](id, "IP", ip)
			If type.NotEmpty AndAlso status.Type <> type Then [Set](id, "Type", type)

			' 更新信息
			If information IsNot Nothing Then SetInformation(id, information)

			' 开始
			SetMessage(id, StatusMessageEnum.START, "设备注册")
		End Sub

		''' <summary>注销注册</summary>
		Public Sub Unregister(dev As DeviceStatus) Implements IDeviceStatusProvider.Unregister
			Unregister(dev?.ID)
		End Sub

		''' <summary>注销注册</summary>
		Public Sub Unregister(id As Long) Implements IDeviceStatusProvider.Unregister
			If id < 1 Then Return

			' 结束
			SetMessage(id, StatusMessageEnum.FINISH, "设备注销")
		End Sub

		''' <summary>当前设备列表，含名称、设备、IP、类型</summary>
		Public ReadOnly Property Devices As Dictionary(Of String, (Name As String, Machine As String, IP As String, Type As String, Display As String)) Implements IDeviceStatusProvider.Devices
			Get
				' 超时或者未设置则获取一次数据
				If _Dev_Last.AddMinutes(DEVICE_RELOAD_INTERVAL) > Now OrElse _Dev_List Is Nothing Then
					SyncLock _lock
						_Dev_Last = DATE_NOW
						_Dev_List = New Dictionary(Of String, (Name As String, Machine As String, IP As String, Type As String, Display As String)) From {{VAR_DEVICE_UNLIMITED, ("不限", "", "", "", "不限")}}

						Dim list = All?.Values
						If list.NotEmpty Then
							For Each dev In list
								Dim Display = If(dev.ID = SYS.ID, $"{dev.Name}(此服务)", dev.Display)
								_Dev_List.Add(dev.ID, (dev.Name, dev.Machine, dev.IP, dev.Type, Display))
							Next
						End If
					End SyncLock
				End If

				Return _Dev_List
			End Get
		End Property

		''' <summary>当前设备列表</summary>
		Public ReadOnly Property Devs As Dictionary(Of String, String) Implements IDeviceStatusProvider.Devs
			Get
				Return Devices?.ToDictionary(Function(x) x.Key, Function(x) x.Value.Display)
			End Get
		End Property

		''' <summary>获取当前指定类型设备列表，使用模糊搜索类型</summary>
		Public ReadOnly Property Devs(type As String) As Dictionary(Of String, String) Implements IDeviceStatusProvider.Devs
			Get
				Return Devices?.Where(Function(x) x.Key = VAR_DEVICE_UNLIMITED OrElse x.Value.Type.Like(type)).ToDictionary(Function(x) x.Key, Function(x) x.Value.Display)
			End Get
		End Property

		''' <summary>是否包含指定设备</summary>
		Public Function HasDev(devId As String, ParamArray devIds As String()) As Boolean Implements IDeviceStatusProvider.HasDev
			If devId.IsEmpty OrElse devIds.IsEmpty Then Return False

			' 是否包含设备标识，包含才可以运行
			Return devIds.Contains(devId) OrElse devIds.Contains(VAR_DEVICE_UNLIMITED)
		End Function

		'''' <summary>是否包含指定设备</summary>
		'Public Function HasDev(ParamArray devIds As String()) As Boolean Implements IDeviceStatusProvider.HasDev
		'	If devIds.IsEmpty Then Return False

		'	' 包含全部应用，则其他应用无需选择
		'	Dim ds = Devices
		'	If ds.IsEmpty Then Return False

		'	' 是否包含任何设备标识
		'	Return devIds.Any(Function(x) ds.ContainsKey(x))
		'End Function

		''' <summary>是否包含当前设备</summary>
		Public Function IsCurrent(ParamArray devIds As String()) As Boolean Implements IDeviceStatusProvider.IsCurrent
			If devIds.IsEmpty Then Return False

			' 是否包含本机标识，包含才可以运行
			Return devIds.Contains(SYS.ID) OrElse devIds.Contains(VAR_DEVICE_UNLIMITED)
		End Function

		''' <summary>更新字典中应用信息，如果选择不限则其他应用无需选择</summary>
		Public Function Update(ParamArray devIds As String()) As String() Implements IDeviceStatusProvider.Update
			If devIds.IsEmpty Then Return Nothing

			' 包含全部，则直接返回全部内容
			If devIds.Contains(VAR_DEVICE_UNLIMITED) Then Return {VAR_DEVICE_UNLIMITED}

			' 当前设备不存在，则直接返回
			Dim ds = Devices
			If ds.IsEmpty Then Return Nothing

			' 保留有效的项目
			Return devIds.Where(Function(x) ds.ContainsKey(x)).ToArray
		End Function

		''' <summary>清理离线设备</summary>
		''' <param name="day">离线时长(天)</param>
		Public Function RemoveOffline(day As Integer) As Integer Implements IDeviceStatusProvider.RemoveOffline
			day = day.Range(1)

			' 检查所有离线时长
			Dim devs = All.Values?.Where(Function(x) x.TimeLast.AddDays(day) < DATE_NOW).Select(Function(x) x.ID).ToArray
			If devs.IsEmpty Then Return 0

			Remove(devs)

			Return devs.Length
		End Function
	End Class
End Namespace