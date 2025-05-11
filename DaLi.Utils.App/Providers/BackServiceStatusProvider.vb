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
' 	name: Provider.BackServiceStatusProvider
' 	create: 2024-07-03
' 	memo: 后台服务状态操作
'
' ------------------------------------------------------------

Namespace Provider
	''' <summary>后台服务状态操作</summary>
	Public Class BackServiceStatusProvider
		Inherits StatusProvider(Of BackServiceStatus)
		Implements IBackServiceStatusProvider

		''' <summary>任务离线时长，如果最后时间超过 N 小时，则移除状态</summary>
		Private Const REDIS_OFFLINE_INTERVAL = 24

		Public Sub New()
			MyBase.New(VAR_REDIS_STATUS_BACKSERVICE)
		End Sub

#Region "远程任务"

		''' <summary>远程服务列表</summary>
		Public ReadOnly Property Services As List(Of BackServiceStatus) Implements IBackServiceStatusProvider.Services
			Get
				Dim list = All?.Values
				If list.IsEmpty Then Return Nothing

				' 检测是否超时，超时则移除
				Dim last = Now.AddHours(0 - REDIS_OFFLINE_INTERVAL)
				Dim keys = list.Where(Function(x) x.TimeLast < last).Select(Function(x) x.ID).ToArray
				If keys.IsEmpty Then
					Return list.ToList
				Else
					' 移除超时键
					Remove(keys)

					Return list.Where(Function(x) Not keys.Contains(x.ID)).ToList
				End If
			End Get
		End Property

		''' <summary>根据类型获取远程服务列表</summary>
		Public ReadOnly Property Services(type As String) As List(Of BackServiceStatus) Implements IBackServiceStatusProvider.Services
			Get
				If type.IsEmpty Then Return Nothing

				Return Me.Services?.Where(Function(x) x.Type.IsSame(type)).ToList
			End Get
		End Property

		''' <summary>根据类型获取远程服务</summary>
		Public ReadOnly Property Service(id As String) As BackServiceStatus Implements IBackServiceStatusProvider.Service
			Get
				Dim status = [Get](id)
				If status Is Nothing Then Return Nothing

				' 检测是否超时，超时则移除
				Dim last = Now.AddHours(0 - REDIS_OFFLINE_INTERVAL)
				If status.TimeLast < last Then
					Remove(id)
					Return Nothing
				End If

				Return status

			End Get
		End Property

#End Region

	End Class
End Namespace