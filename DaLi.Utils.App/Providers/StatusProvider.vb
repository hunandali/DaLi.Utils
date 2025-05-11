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
' 	状态缓存操作
'
' 	name: Provider.StatusProvider
' 	create: 2024-07-03
' 	memo: 状态缓存操作，基于 Redis JSON/HASH 数据状态缓存操作
'
' ------------------------------------------------------------

Imports DaLi.Utils.Redis

Namespace Provider
	''' <summary>状态缓存操作</summary>
	Public Class StatusProvider(Of T As IStatus)
		Implements IDisposable, IStatusProvider(Of T)

		''' <summary>缓存接口</summary>
		Private ReadOnly _IO As StatusCache(Of T)

		''' <summary>消息最大数量</summary>
		Protected Overridable ReadOnly Property MaxMessage As Integer = 100

#Region "操作"

		''' <summary>构造</summary>
		Public Sub New(name As String)
			Dim client = SYS.GetSetting(Of RedisSetting).CreateClient
			_IO = New StatusCache(Of T)(client, name, True)
		End Sub

		''' <summary>注销</summary>
		Public Sub Dispose() Implements IDisposable.Dispose
			_IO?.Dispose()
			GC.SuppressFinalize(Me)
		End Sub

		''' <summary>获取所有状态信息</summary>
		Public ReadOnly Property All As IDictionary(Of String, T) Implements IStatusProvider(Of T).All
			Get
				Return _IO.GET
			End Get
		End Property

		''' <summary>获取、设置当前状态</summary>
		Default Public Property Item(id As String) As T Implements IStatusProvider(Of T).Item
			Get
				Return _IO.GET(id)
			End Get
			Set(value As T)
				_IO.SET(id, value)
			End Set
		End Property

		''' <summary>获取、设置是否忙</summary>
		Public Property IsBusy(id As String) As Boolean Implements IStatusProvider(Of T).IsBusy
			Get
				Return _IO.GET(Of Boolean)(id, "IsBusy")
			End Get
			Set(value As Boolean)
				_IO.SET(id, "IsBusy", value)
			End Set
		End Property

		''' <summary>获取状态消息</summary>
		Public ReadOnly Property Information(id As String) As KeyValueDictionary Implements IStatusProvider(Of T).Information
			Get
				Return _IO.GET(Of KeyValueDictionary)(id, "Information")
			End Get
		End Property

		''' <summary>启动时间</summary>
		Public Property TimeStart(id As String) As Date Implements IStatusProvider(Of T).TimeStart
			Get
				Return _IO.GET(Of Date)(id, "TimeStart")
			End Get
			Set(value As Date)
				_IO.SET(id, "TimeStart", value)
			End Set
		End Property

		''' <summary>最后更新时间</summary>
		Public Property TimeLast(id As String) As Date Implements IStatusProvider(Of T).TimeLast
			Get
				Return _IO.GET(Of Date)(id, "TimeLast")
			End Get
			Set(value As Date)
				_IO.SET(id, "TimeLast", value)
			End Set
		End Property

		''' <summary>名称</summary>
		Public Property Name(id As String) As String Implements IStatusProvider(Of T).Name
			Get
				Return _IO.GET(Of String)(id, "Name")
			End Get
			Set(value As String)
				_IO.SET(id, "Name", value)
			End Set
		End Property

		''' <summary>获取状态，如果不存在使用默认值，并将默认值存入缓存</summary>
		Public Function [Get](id As String, Optional defaultValue As T = Nothing) As T Implements IStatusProvider(Of T).Get
			Dim value = _IO.GET(id)

			If value Is Nothing AndAlso defaultValue IsNot Nothing Then
				value = defaultValue
				_IO.SET(id, value)
			End If

			Return value
		End Function

		''' <summary>设置值</summary>
		Public Sub [Set](Of S)(id As String, key As String, value As S) Implements IStatusProvider(Of T).Set
			_IO.SET(id, key, value)
		End Sub

		''' <summary>获取值</summary>
		Public Function [Get](Of S)(id As String, key As String) As S Implements IStatusProvider(Of T).Get
			Return _IO.GET(Of S)(id, key)
		End Function

		''' <summary>设置状态消息</summary>
		Public Sub SetInformation(id As String, information As KeyValueDictionary) Implements IStatusProvider(Of T).SetInformation
			If information Is Nothing Then information = New KeyValueDictionary

			' 附加信息更新时间
			information.Add("_Update", DATE_NOW)

			' 更新
			_IO.SET(id, "Information", information)
		End Sub

		''' <summary>获取所有消息</summary>
		Public Function GetMessages(Of V As StatusMessage)(id As String) As List(Of V) Implements IStatusProvider(Of T).GetMessages
			Return _IO.GET(Of IEnumerable(Of V))(id, "Messages")
		End Function

		''' <summary>插入消息并记录最新状态</summary>
		Public Sub SetMessage(Of V As StatusMessage)(id As String, message As V) Implements IStatusProvider(Of T).SetMessage
			If message Is Nothing Then Return

			' 更新时间
			TimeLast(id) = message.TimeAction

			' 启动消息，设置状态
			If message.Type = StatusMessageEnum.START Then
				TimeStart(id) = message.TimeAction
				IsBusy(id) = True
			End If

			' 结束消息，清除状态
			If message.Type = StatusMessageEnum.FINISH Then
				Dim start = TimeStart(id)
				If start.IsValidate Then
					Dim info = $"{start:MM-dd HH:mm:ss} => {DATE_NOW:MM-dd HH:mm:ss} ({DATE_NOW.ShowDiff(start)})"
					message.Message = $"{message.Message} | {info}"
				End If

				IsBusy(id) = False
			End If

			' 追加信息，空内容不处理
			If message.Message.NotEmpty Then _IO.APPEND(id, "Messages", message, MaxMessage)
		End Sub

		''' <summary>插入消息并记录最新状态</summary>
		Public Sub SetMessage(id As String, type As StatusMessageEnum, message As String) Implements IStatusProvider(Of T).SetMessage
			SetMessage(id, New StatusMessage With {.TimeAction = DATE_NOW, .Type = type, .Message = message})
		End Sub

		''' <summary>设置状态消息</summary>
		Public Sub SetInformation(Of S)(id As String, key As String, value As S) Implements IStatusProvider(Of T).SetInformation
			_IO.SET(id, $"Information.{key}", value)
		End Sub

		''' <summary>获取状态消息</summary>
		Public Function GetInformation(Of S)(id As String, key As String) As S Implements IStatusProvider(Of T).GetInformation
			Return _IO.GET(Of S)(id, $"Information.{key}")
		End Function

		''' <summary>移除项目</summary>
		Public Sub Remove(ParamArray ids() As String) Implements IStatusProvider(Of T).Remove
			_IO.DEL(ids)
		End Sub

		''' <summary>清除所有项目</summary>
		Public Sub Clear() Implements IStatusProvider(Of T).Clear
			_IO.CLEAR()
		End Sub

#End Region

	End Class
End Namespace