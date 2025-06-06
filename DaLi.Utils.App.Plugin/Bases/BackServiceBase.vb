﻿' ------------------------------------------------------------
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
' 	后台服务基类
'
' 	name: Base.BackServiceBase
' 	create: 2023-02-14
' 	memo: 后台服务基类
'
' ------------------------------------------------------------

Imports System.Threading
Imports DaLi.Utils.Json
Imports Microsoft.Extensions.FileProviders
Imports Microsoft.Extensions.Logging
Imports Microsoft.Extensions.Primitives

Namespace Base

	''' <summary>后台服务基类</summary>
	Public MustInherit Class BackServiceBase
		Implements IDisposable

#Region "事件"

		''' <summary>任务开始</summary>
		Protected Event TaskStart()

		''' <summary>任务停止</summary>
		Protected Event TaskStop()

		''' <summary>任务结束</summary>
		Protected Event TaskFinish()

		''' <summary>任务异常</summary>
		Protected Event TaskError(ex As Exception)

		''' <summary>参数更新</summary>
		Protected Event SettingUpdate()

#End Region

#Region "属性"

		''' <summary>唯一名称</summary>
		Public MustOverride ReadOnly Property Name As String

		''' <summary>安静模式，除非强制，否则不输出日志。(执行错误消息除外)</summary>
		Public Overridable ReadOnly Property Silent As Boolean = False

		''' <summary>是否强制禁止任务</summary>
		Public Overridable ReadOnly Property Disabled As Boolean = False

		''' <summary>Corn 表达式</summary>
		Public Property Interval As String
			Get
				Return Misc.Cron.Service.Item(ID).Exps
			End Get
			Set(value As String)
				Misc.Cron.Service.Register(ID, value)
			End Set
		End Property

		''' <summary>是否自动启动，否则只能手动执行</summary>
		Private _AutoStart As Boolean

		''' <summary>是否自动启动，否则只能手动执行</summary>
		Public Property AutoStart As Boolean
			Get
				Return _AutoStart
			End Get
			Private Set(value As Boolean)
				_AutoStart = value
			End Set
		End Property

		''' <summary>是否存在配置文件</summary>
		Public ReadOnly Property EnSetting As Boolean
			Get
				Return _FilePath.NotEmpty
			End Get
		End Property

		''' <summary>日志组件</summary>
		Protected ReadOnly Log As ILogger(Of BackServiceBase)

		''' <summary>参数</summary>
		Public ReadOnly Settings As KeyValueDictionary

		''' <summary>目录监控对象</summary>
		Private Shared _FileProvider As PhysicalFileProvider

		''' <summary>参数文件路径</summary>
		Private ReadOnly _FilePath As String

		''' <summary>唯一标识</summary>
		Public ReadOnly ID As String

		''' <summary>是否强制取消运行</summary>
		Public Property IsForceStop As Boolean

		''' <summary>运行状态</summary>
		Public ReadOnly Status As BackServiceStatus

		''' <summary>服务驱动</summary>
		Protected ReadOnly StatusProvider As IBackServiceStatusProvider

		''' <summary>构造，初始化</summary>
		''' <param name="interval">默认周期，Corn 表达式</param>
		Public Sub New(log As ILogger(Of BackServiceBase), Optional interval As String = "", Optional localSetting As Boolean = True, Optional id As String = "")
			Me.Log = log

			' 唯一标识
			Me.ID = id.EmptyValue($"{SYS.ID}|{[GetType].Assembly.Name}|{[GetType].FullName}").ToLowerInvariant.MD5

			' 状态对象初始化
			StatusProvider = SYS.GetService(Of IBackServiceStatusProvider)
			Status = StatusProvider.Get(Me.ID, New BackServiceStatus(Me))

			' 每次运行都重新计算
			Status.TotalCount = 0
			Status.TotalTime = 0
			Status.IsBusy = False

			' 初始化参数
			Settings = New KeyValueDictionary From {{"ID", [GetType].FullName}, {"AutoStart", True}, {"Interval", interval}}

			' 监控
			If localSetting Then
				Dim root = PathHelper.Root(".config/.services", True, True)
				Dim file = $"{[GetType].FullName.ToLowerInvariant}.json"

				' 初次如果配置文件不存在，则创建
				_FilePath = IO.Path.Combine(root, file)
				If Not PathHelper.FileExist(_FilePath) Then PathHelper.SaveJson(_FilePath, Settings)

				_FileProvider = If(_FileProvider, New PhysicalFileProvider(root))
				Dim actionFunc = Debounce(Sub() ReloadSetting(), 30000)   ' 防抖，30 秒内不重复操作，以最后操作为准
				ChangeToken.OnChange(Function() _FileProvider.Watch(file), actionFunc)
			End If

			' 加载参数
			Call ReloadSetting()
		End Sub

#End Region

#Region "参数设置"

		''' <summary>参数文件监控对象</summary>
		Protected FileProvider As PhysicalFileProvider

		''' <summary>重新加载参数</summary>
		Private Sub ReloadSetting()
			' 加载本地参数
			If _FilePath.NotEmpty Then
				' 加载参数
				Dim json = PathHelper.FileRead(_FilePath)
				Dim dic = json.ToJsonDictionary
				Settings.UpdateRange(dic)
			End If

			' 更新事件
			RaiseEvent SettingUpdate()

			' 更新默认参数
			LoadSettingDefault()

			SetMessage("参数加载", StatusMessageEnum.IMPORTANT)
		End Sub

		''' <summary>更新并保存参数</summary>
		Public Sub UpdateSetting(value As KeyValueDictionary)
			If value.IsEmpty Then Return

			Settings.UpdateRange(value)

			' 保存本地参数
			If _FilePath.NotEmpty Then PathHelper.SaveJson(_FilePath, Settings)

			' 更新默认参数
			LoadSettingDefault()
			SetMessage("参数更新", StatusMessageEnum.IMPORTANT)
		End Sub

		''' <summary>加载默认参数</summary>
		Private Sub LoadSettingDefault()
			Interval = Settings("Interval", 300)
			AutoStart = Settings("AutoStart", True)
		End Sub

#End Region

#Region "操作"

		''' <summary>设置消息</summary>
		''' <param name="message">消息内容</param>
		''' <param name="type">消息类型</param>
		''' <param name="force">强制消息输出，忽略状态或者安静模式</param>
		Protected Sub SetMessage(message As String, Optional type As StatusMessageEnum = StatusMessageEnum.INFORMATION, Optional force As Boolean = False)
			Dim enabled = force OrElse type = StatusMessageEnum.ERROR OrElse Not Silent
			If Not enabled Then Return

			StatusProvider.SetMessage(ID, type, message)

			Dim level = LogLevel.Information
			Select Case type
				Case StatusMessageEnum.DEBUG
					level = LogLevel.Debug

				Case StatusMessageEnum.ERROR
					level = LogLevel.Error

				Case StatusMessageEnum.IMPORTANT, StatusMessageEnum.START, StatusMessageEnum.FINISH
					level = LogLevel.Warning
			End Select

			If message.NotEmpty Then Log?.Log(level, "后台服务 {Name}: {message}", Name, message)

			Status.TimeLast = DATE_NOW
		End Sub

		'''' <summary>设置消息</summary>
		'''' <param name="message">消息内容</param>
		'''' <param name="logLevel">是否记录到日志，记录级别。默认不记录</param>
		'Protected Sub SetMessage(message As String, Optional logLevel As LogLevel = LogLevel.Trace)
		'	Dim type = StatusMessageEnum.INFORMATION

		'	Select Case logLevel
		'		Case LogLevel.Debug
		'			type = StatusMessageEnum.DEBUG

		'		Case LogLevel.Warning, LogLevel.Error, LogLevel.Critical
		'			type = StatusMessageEnum.ERROR
		'	End Select

		'	StatusProvider.SetMessage(ID, type, message)

		'	If message.NotEmpty Then Log?.Log(logLevel, "后台服务 {Name}: {message}", Name, message)
		'End Sub

		'''' <summary>设置消息</summary>
		'''' <param name="message">消息内容</param>
		'''' <param name="ex">系统异常信息</param>
		'Protected Sub SetMessage(ex As Exception, Optional message As String = "")
		'	If ex Is Nothing Then Return

		'	SetMessage(StatusMessageEnum.ERROR, message.EmptyValue(ex.Message))
		'	'Status.Exception = ex

		'	Log.LogError(ex, "后台服务 {Name}: {message}", Name, message)
		'End Sub

		''' <summary>获取当前服务所有信息</summary>
		Public Function GetStatus() As Object
			' 获取缓存消息
			Status.Messages = StatusProvider.GetMessages(Of StatusMessage)(ID)

			Return New With {.Type = [GetType].FullName, ID, Name, EnSetting, AutoStart, Status, Settings}
		End Function

		Public Sub Dispose() Implements IDisposable.Dispose
			_StopToken.Cancel()
			GC.SuppressFinalize(Me)
		End Sub

#End Region

#Region "任务"

		''' <summary>当前执行的任务</summary>
		Private _ExecutingTask As Task

		''' <summary>取消标记</summary>
		Private _StopToken As New CancellationTokenSource

		''' <summary>执行任务</summary>
		Protected MustOverride Async Function ExecuteAsync(stoppingToken As CancellationToken) As Task

		''' <summary>开启任务</summary>
		Public Function StartAsync(auto As Boolean, Optional cancellationToken As CancellationToken = Nothing) As Task
			' 强制禁用
			If Disabled Then Return Task.CompletedTask

			' 强制取消标记
			IsForceStop = False

			' 自动执行则需要校验是否到执行时间，手动则强制操作
			If auto Then
				' 1 秒内的任务不再执行
				If Status.TimeLast.AddSeconds(1) > DATE_NOW Then Return Task.CompletedTask

				' 未到工作时间
				'If Not CornHelper.Timeup(Interval, DATE_NOW, Status.TimeLast) Then Return Task.CompletedTask
				If Not Misc.Cron.Service.TimeUp(ID, Status.TimeLast) Then Return Task.CompletedTask
			End If

			' 运行中的任务直接退出
			If Status.IsBusy Then Return Task.CompletedTask

			' 立即标记运行状态，防止重复运行
			Status.IsBusy = True

			'-----------------------------------------------------------------

			' 重置状态
			If _StopToken.IsCancellationRequested Then _StopToken = New CancellationTokenSource

			' 执行事件
			Dim execute = Async Function()
							  ' 任务开始
							  RaiseEvent TaskStart()

							  ' 本次运行启动时间
							  StatusProvider.TimeStart(ID) = DATE_NOW

							  ' 初次启动时间
							  If Status.TotalCount < 1 Then
								  Status.TimeRun = DATE_NOW
								  StatusProvider.Set(ID, "TiemRun", DATE_NOW)
							  End If

							  ' 运行消息
							  SetMessage("启动任务，运行中...", StatusMessageEnum.START)

							  Dim s As New Stopwatch
							  s.Start()
							  Try
								  ' 启动执行任务
								  Await ExecuteAsync(_StopToken.Token)
							  Catch ex As Exception
								  SetMessage($"任务异常：{ex.Message} ({ex.StackTrace})", StatusMessageEnum.ERROR)

								  ' 任务异常
								  RaiseEvent TaskError(ex)
							  End Try

							  s.Stop()
							  SetMessage($"执行任务，耗时 {s.Elapsed.Show}", StatusMessageEnum.FINISH)

							  ' 本次运行结束时间
							  Status.TimeStop = DATE_NOW
							  StatusProvider.Set(ID, "TiemStop", Status.TimeStop)

							  ' 累计运行时间
							  Status.TotalTime += s.ElapsedTicks

							  ' 运行次数
							  Status.TotalCount += 1

							  StatusProvider.Set(ID, "TotalCount", Status.TotalCount)
							  StatusProvider.Set(ID, "TotalTime", Status.TotalTime)

							  ' 任务结束
							  RaiseEvent TaskFinish()

							  ' 强制再获取一次定时状态，以便更新定时器最后时间，并重置定时状态
							  ' 目的是解决手动启动任务后，自动任务间隔时间过短
							  Misc.Cron.Service.TimeUp(ID, DATE_NOW)

							  ' 解除运行状态
							  Status.IsBusy = False
						  End Function

			' 执行操作
			_ExecutingTask = Task.Run(execute, cancellationToken)

			' 返回
			If _ExecutingTask.IsCompleted Then
				Return _ExecutingTask
			Else
				Return Task.CompletedTask
			End If
		End Function

		''' <summary>结束任务</summary>
		Public Function StopAsync(cancellationToken As CancellationToken) As Task
			' 标记已被强制结束
			IsForceStop = True

			If _ExecutingTask Is Nothing Then Return Task.CompletedTask

			Try
				_StopToken.Cancel()
			Catch ex As Exception
			End Try

			Return Task.WhenAny(_ExecutingTask, Task.Delay(Timeout.Infinite, cancellationToken))
		End Function

#End Region

	End Class

End Namespace
