'' ------------------------------------------------------------
''
'' 	Copyright © 2021 湖南大沥网络科技有限公司.
'' 	Dali.Utils Is licensed under Mulan PSL v2.
''
'' 	  author:	木炭(WOODCOAL)
'' 	   email:	a@hndl.vip
'' 	homepage:	http://www.hunandali.com/
''
'' 	请依据 Mulan PSL v2 的条款使用本项目。获取 Mulan PSL v2 请浏览 http://license.coscl.org.cn/MulanPSL2
''
'' ------------------------------------------------------------
''
'' 	空闲对象容器管理
''
'' 	name: IdleBus
'' 	create: 2024-08-22
'' 	memo: 空闲对象容器管理，基于 https://github.com/2881099/IdleBus 修改
''
'' ------------------------------------------------------------

'Imports System.Threading

'Partial Public Class IdleBus(Of TKey, TValue As {Class, IDisposable})

'	Public Class ItemInfo
'		Implements IDisposable

'		''' <summary>内部引用了 IdleBus 实例</summary>
'		Private ReadOnly _IdleBus As IdleBus(Of TKey, TValue)

'		''' <summary>键</summary>
'		Public ReadOnly Property Key As TKey

'		''' <summary>创建值的委托</summary>
'		Public ReadOnly Create As Func(Of TValue)

'		''' <summary>空闲时间跨度</summary>
'		Public ReadOnly Idle As TimeSpan

'		''' <summary>上次活跃时间</summary>
'		Public LastActiveTime As Date

'		''' <summary>释放错误计数器</summary>
'		Public ReleaseErrorCounter As Integer

'		Public Sub New(idleBus As IdleBus(Of TKey, TValue), key As TKey, create As Func(Of TValue), idle As TimeSpan)
'			_IdleBus = idleBus

'			Me.Key = key
'			Me.Create = create
'			Me.Idle = idle
'		End Sub

'		''' <summary>创建时间</summary>
'		Private _CreateTime As Date

'		''' <summary>创建时间</summary>
'		Public ReadOnly Property CreateTime As Date
'			Get
'				Return _CreateTime
'			End Get
'		End Property

'		''' <summary>活跃次数计数器</summary>
'		Private _ActiveCounter As Long

'		''' <summary>活跃次数计数器</summary>
'		Public ReadOnly Property ActiveCounter As Long
'			Get
'				Return _ActiveCounter
'			End Get
'		End Property

'		''' <summary>值对象实例</summary>
'		Private _Value As TValue

'		''' <summary>值对象实例</summary>
'		Friend Property Value As TValue
'			Get
'				Return _Value
'			End Get
'			Private Set(value As TValue)
'				_Value = value
'			End Set
'		End Property

'		''' <summary>第一次创建的值</summary>
'		Private _FirstValue As TValue

'		''' <summary>是否注册错误</summary>
'		Private _IsRegisterError As Boolean

'		''' <summary>是否注册错误</summary>
'		Friend Property IsRegisterError As Boolean
'			Get
'				Return _IsRegisterError
'			End Get
'			Private Set(value As Boolean)
'				_IsRegisterError = value
'			End Set
'		End Property

'		''' <summary>锁</summary>
'		Private ReadOnly Locker As New Object()

'		''' <summary>获取或创建值对象实例</summary>
'		Friend Function GetOrCreate() As TValue
'			' 检查是否已处置
'			If _IsDisposed Then Return Nothing

'			' 如果值为空，则尝试创建
'			If Value Is Nothing Then
'				Dim now = Date.Now

'				Try
'					SyncLock Locker
'						' 再次检查是否已处置
'						If _IsDisposed = True Then Return Nothing

'						' 存在则直接返回
'						If Value IsNot Nothing Then Return Value

'						' 如果值仍然为空，则创建
'						Value = Create?.Invoke()

'						_CreateTime = Date.Now
'						Interlocked.Increment(_IdleBus.UsageQuantity)

'						' 创建成功
'						If Value IsNot Nothing Then
'							If _FirstValue Is Nothing Then
'								' 记录首次值
'								_FirstValue = Value
'							ElseIf _FirstValue Is Value Then
'								' 第二次与首次相等，注册姿势错误
'								IsRegisterError = True
'							End If
'						End If
'					End SyncLock

'					' 如果创建成功，则记录相关信息
'					If Value IsNot Nothing Then
'						Dim times = Date.Now.Subtract(now).TotalMilliseconds
'						_IdleBus.OnNotice(NoticeTypeEnum.AUTOCREATE, Key, Nothing, $"{Key} 创建成功，耗时 {times}ms，{_IdleBus.UsageQuantity}/{_IdleBus.Quantity}")
'					End If

'				Catch ex As Exception
'					' 异常处理
'					_IdleBus.OnNotice(NoticeTypeEnum.AUTOCREATE, Key, ex, $"{Key} 创建失败：{ex.Message}")
'					Throw
'				End Try
'			End If

'			' 更新上次活跃时间，增加活跃次数
'			LastActiveTime = Date.Now
'			Interlocked.Increment(_ActiveCounter)

'			Return Value
'		End Function

'		''' <summary>释放</summary>
'		Friend Function Release(lockInIf As Func(Of Boolean)) As Boolean
'			SyncLock Locker
'				' 检查值是否存在且满足锁定条件
'				If Value IsNot Nothing AndAlso lockInIf() Then
'					Value?.Dispose()
'					Value = Nothing

'					Interlocked.Decrement(_IdleBus.UsageQuantity)
'					Interlocked.Exchange(_ActiveCounter, 0)

'					Return True
'				End If
'			End SyncLock

'			Return False
'		End Function

'#Region "Dispose"

'		Protected Overrides Sub Finalize()
'			Dispose()
'		End Sub

'		Private _IsDisposed As Boolean = False

'		Public Sub Dispose() Implements IDisposable.Dispose
'			If _IsDisposed Then Return

'			SyncLock Locker
'				If _IsDisposed Then Return
'				_IsDisposed = True
'			End SyncLock

'			Try
'				Release(Function() True)
'			Catch
'			End Try

'			GC.SuppressFinalize(Me)
'		End Sub

'#End Region

'	End Class

'End Class
