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
' 	设备状态
'
' 	name: Model.DeviceStatus
' 	create: 2024-07-02
' 	memo: 设备状态，设备包括当前应用以及客户端
'
' ------------------------------------------------------------

Namespace Model

	''' <summary>设备状态</summary>
	Public Class DeviceStatus
		Implements IStatus

		''' <summary>服务标识</summary>
		Public Property ID As String Implements IStatus.ID

		''' <summary>服务名称</summary>
		Public Property Name As String Implements IStatus.Name

		''' <summary>类型:Server,Client</summary>
		Public Property Type As String

		''' <summary>设备信息</summary>
		Public Property Machine As String

		''' <summary>IP</summary>
		Public Property IP As String

		''' <summary>启动时间</summary>
		Public Property TimeStart As Date Implements IStatus.TimeStart

		''' <summary>最后执行时间</summary>
		Public Property TimeLast As Date Implements IStatus.TimeLast

		''' <summary>是否忙</summary>
		Public Property IsBusy As Boolean Implements IStatus.IsBusy

		''' <summary>运行消息</summary>
		Public Property Messages As List(Of StatusMessage) = New List(Of StatusMessage) Implements IStatus.Messages

		''' <summary>状态消息</summary>
		Public Property Information As KeyValueDictionary = New KeyValueDictionary Implements IStatus.Information

		''' <summary>展示名称</summary>
		Public ReadOnly Property Display As String
			Get
				Dim data = Machine.EmptyValue(Type, IP)
				Return If(data.IsEmpty, Name, $"{Name}({data})")
			End Get
		End Property

	End Class

End Namespace