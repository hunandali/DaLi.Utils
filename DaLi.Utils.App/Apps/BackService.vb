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
' 	后台任务服务管理
'
' 	name: Provider.BackService
' 	create: 2023-02-15
' 	memo: 后台任务服务管理
'
' ------------------------------------------------------------

Imports System.Collections.Concurrent
Imports System.Threading
Imports Microsoft.Extensions.Hosting

Namespace App

	''' <summary>后台任务服务</summary>
	Public Class BackService
		Inherits BackgroundService

#Region "后台服务"

		''' <summary>构造，并从系统实例中加载后台任务数据</summary>
		Public Sub New()
			Add(SYS.GetServices(Of BackServiceBase))

			' 注册全局事件，以便注册任务
			SYS.Events.Register(E_BACKSERVICE_ADD, Sub(service As BackServiceBase) Add(service))
			SYS.Events.Register(E_BACKSERVICE_REMOVE, Sub(id As String) Remove(id))
			SYS.Events.Register(E_BACKSERVICE_START, Sub(id As String) ServiceStart(id))
			SYS.Events.Register(E_BACKSERVICE_STOP, Sub(id As String) ServiceStop(id))
		End Sub

		''' <summary>执行操作</summary>
		Protected Overrides Function ExecuteAsync(stoppingToken As CancellationToken) As Task
			Return Task.Run(Sub()
								Dim s As New Stopwatch
								While True
									s.Restart()

									Dim Services = List.Where(Function(x) x.AutoStart AndAlso Not x.Status.IsBusy AndAlso Not x.IsForceStop AndAlso Not x.Disabled).ToList

									For Each sev In Services
										Call sev.StartAsync(True, stoppingToken)
										If stoppingToken.IsCancellationRequested Then Exit For
									Next

									If stoppingToken.IsCancellationRequested Then Exit While

									s.Stop()

									' 5 秒轮询一次
									Dim delay = 4999 - s.ElapsedMilliseconds
									If delay > 0 Then Thread.Sleep(delay)
								End While
							End Sub, stoppingToken)
		End Function

		''' <summary>注销</summary>
		Public Overrides Sub Dispose()
			' 移除所有远程服务状态
			'For Each sev In _Instance
			'	StatusProvier.Remove(sev.Value.ID)
			'Next

			GC.SuppressFinalize(Me)

			MyBase.Dispose()
		End Sub

#End Region

#Region "任务操作"

		''' <summary>任务列表</summary>
		Private Shared ReadOnly _Instance As New ConcurrentDictionary(Of String, BackServiceBase)(StringComparer.OrdinalIgnoreCase)

		''' <summary>任务列表</summary>
		Public Shared ReadOnly Property List As List(Of BackServiceBase)
			Get
				Return _Instance.Values.ToList
			End Get
		End Property

		''' <summary>获取后台任务</summary>
		Public Shared ReadOnly Property Item(id As String) As BackServiceBase
			Get
				If id.NotEmpty Then
					Dim value As BackServiceBase = Nothing
					If _Instance.TryGetValue(id, value) Then Return value
				End If

				Return Nothing
			End Get
		End Property

		''' <summary>添加后台任务</summary>
		Public Shared Function Add(service As BackServiceBase) As Boolean
			If Not _Instance.ContainsKey(service.ID) Then
				Return _Instance.TryAdd(service.ID, service)
			Else
				Return False
			End If
		End Function

		''' <summary>批量添加后台任务</summary>
		Public Shared Sub Add(services As IEnumerable(Of BackServiceBase))
			If services?.Count > 0 Then
				For Each sev In services
					Add(sev)
				Next
			End If
		End Sub

		''' <summary>移除后台任务</summary>
		Public Shared Function Remove(id As String) As Boolean
			Dim Service = Item(id)

			If Service IsNot Nothing Then
				Service.StopAsync(Nothing).Wait()

				Return _Instance.TryRemove(id, Nothing)
			Else
				Return False
			End If
		End Function

		''' <summary>启动任务</summary>
		Public Shared Function ServiceStart(id As String) As BackServiceStatus
			Dim Service = Item(id)
			If Service Is Nothing Then Return Nothing

			Service.StartAsync(False)
			Return Service.Status
		End Function

		''' <summary>结束任务</summary>
		Public Shared Function ServiceStop(id As String) As BackServiceStatus
			Dim Service = Item(id)
			If Service Is Nothing Then Return Nothing

			Service.StopAsync(Nothing)
			Return Service.Status
		End Function

#End Region

	End Class
End Namespace

