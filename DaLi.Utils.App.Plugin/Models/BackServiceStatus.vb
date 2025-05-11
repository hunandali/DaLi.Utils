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
' 	后台服务状态
'
' 	name: Model.BackServiceStatus
' 	create: 2023-02-14
' 	memo: 后台服务状态
'
' ------------------------------------------------------------

Namespace Model

	''' <summary>后台服务状态</summary>
	Public Class BackServiceStatus
		Implements IStatus

		'''' <summary>消息最多记录数</summary>
		'Public MaxMessage As Integer = 50

		Public Sub New()
			Device = SYS.ID
		End Sub

		Public Sub New(sev As BackServiceBase)
			Me.New

			If sev IsNot Nothing Then
				Name = sev.Name
				ID = sev.ID
				Type = sev.GetType.FullName
			End If
		End Sub

		Public Sub New(id As String, name As String, type As String)
			Me.New

			Me.Name = name
			Me.ID = id
			Me.Type = type
		End Sub

		''' <summary>服务标识</summary>
		Public Property ID As String Implements IStatus.ID

		''' <summary>服务名称</summary>
		Public Property Name As String Implements IStatus.Name

		''' <summary>设备信息</summary>
		Public Property Machine As String

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

		''' <summary>类型</summary>
		Public Property Type As String

		''' <summary>服务所在设备标识</summary>
		Public Property Device As Long

		''' <summary>服务开启时间</summary>
		Public Property TimeRun As Date

		''' <summary>服务最后结束时间</summary>
		Public Property TimeStop As Date

		''' <summary>累计运行次数</summary>
		Public Property TotalCount As Integer

		''' <summary>累计运行时间</summary>
		Public Property TotalTime As Long

		''' <summary>定时器</summary>
		Public Property Interval As String

		''' <summary>定时器描述</summary>
		Public ReadOnly Property IntervalDesc As String
			Get
				If Interval.IsEmpty Then Return ""
				Return Misc.Cron.Expression.Description(Interval.Split("|"c))
			End Get
		End Property

		'''' <summary>插入消息并记录最新状态</summary>
		'Public Sub SetMessage(message As StatusMessage)
		'	If message Is Nothing Then Return

		'	SyncLock Messages
		'		Messages.Add(message)
		'		If Messages.Count > MaxMessage Then Messages = Messages.TakeLast(MaxMessage).ToList
		'	End SyncLock

		'	' 更新时间
		'	TimeLast = message.TimeAction

		'	' 启动消息，设置状态
		'	If message.Type = StatusMessageEnum.START Then
		'		TimeStart = message.TimeAction
		'		IsBusy = True
		'	End If

		'	' 结束消息，清除状态
		'	If message.Type = StatusMessageEnum.FINISH Then
		'		IsBusy = False
		'	End If
		'End Sub

		'''' <summary>插入消息并记录最新状态</summary>
		'Public Sub SetMessage(type As StatusMessageEnum, message As String)
		'	SetMessage(New StatusMessage With {.TimeAction = DATE_NOW, .Type = type, .Message = message})
		'End Sub

	End Class

End Namespace