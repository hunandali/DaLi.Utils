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

'Imports System.Collections.Concurrent
'Imports System.Threading

'''' <summary>空闲对象容器管理，可实现自动创建、销毁、扩张收缩，解决【实例】长时间占用问题</summary>
'Partial Public Class IdleBus(Of TKey, TValue As {Class, IDisposable})
'	Implements IDisposable

'	''' <summary>实例数据</summary>
'	Protected ReadOnly Instance As ConcurrentDictionary(Of TKey, ItemInfo)

'	''' <summary>需要移除的项目</summary>
'	Protected ReadOnly RemoveInstance As ConcurrentDictionary(Of String, ItemInfo)

'	''' <summary>默认空闲时长</summary>
'	Private ReadOnly _DefaultIdle As TimeSpan

'	''' <summary>已创建【实例】数量</summary>
'	Private _UsageQuantity As Integer

'	''' <summary>已创建【实例】数量</summary>
'	Public ReadOnly Property UsageQuantity As Integer
'		Get
'			Return _UsageQuantity
'		End Get
'	End Property

'	''' <summary>按空闲时间1分钟，创建空闲容器</summary>
'	Public Sub New()
'		Me.New(TimeSpan.FromMinutes(1))
'	End Sub

'	''' <summary>指定空闲时间、创建空闲容器</summary>
'	''' <param name="idle">空闲时间</param>
'	Public Sub New(idle As TimeSpan)
'		Instance = New ConcurrentDictionary(Of TKey, ItemInfo)()
'		RemoveInstance = New ConcurrentDictionary(Of String, ItemInfo)()

'		_UsageQuantity = 0
'		_DefaultIdle = idle
'	End Sub

'	''' <summary>根据 key 获得或创建【实例】（线程安全）；key 未注册时，抛出异常</summary>
'	Public Function [Get](key As TKey) As TValue
'		' 对象已释放则抛出异常
'		If IsDisposed Then Throw New Exception($"{key} 实例获取失败，{NameOf(IdleBus(Of TValue))} 对象已释放")

'		Dim item As ItemInfo = Nothing

'		' 尝试获取对象，失败则抛出异常
'		If Instance.TryGetValue(key, item) = False Then
'			Dim err = New Exception($"{key} 实例获取失败，因为没有注册")
'			OnNotice(NoticeTypeEnum.GET, key, err, err.Message)
'			Throw err
'		End If

'		Dim now = Date.Now

'		' 创建对象，失败则抛出异常
'		Dim ret = item.GetOrCreate()
'		If item.IsRegisterError Then
'			Dim err = New ArgumentException($"{key} 实例获取失败：检测到注册实例的参数 create 返回了相同的实例，应该每次返回新实例，正确：() => new class()，错误：() => 变量")
'			OnNotice(NoticeTypeEnum.GET, key, Nothing, err.Message)
'			Throw err
'		End If

'		' 创建成功
'		Dim times = Date.Now.Subtract(now).TotalMilliseconds
'		OnNotice(NoticeTypeEnum.GET, key, Nothing, $"{key} 实例获取成功 {item.ActiveCounter} 次 {If(times > 5, $"，耗时 {times}ms", "")}")

'		' 这种在后台扫描 Instance，定时要求可能没那么及时
'		ThreadScanWatch(item)

'		Return ret
'	End Function

'	''' <summary>获得或创建所有【实例】（线程安全）</summary>
'	Public Function GetAll() As List(Of TValue)
'		Return Instance.Keys.Select(Function(x) [Get](x)).ToList()
'	End Function

'	''' <summary>获得所有已注册的 key（线程安全）</summary>
'	Public Function GetKeys(Optional filter As Func(Of TValue, Boolean) = Nothing) As TKey()
'		If filter Is Nothing Then Return Instance.Keys.ToArray()

'		Return Instance.Keys.
'			Where(Function(key)
'					  Dim item As ItemInfo = Nothing
'					  Return Instance.TryGetValue(key, item) AndAlso filter(item.Value)
'				  End Function).
'			ToArray()
'	End Function

'	''' <summary>判断 key 是否注册</summary>
'	Public Function Exists(key As TKey) As Boolean
'		If key Is Nothing Then Return False
'		Return Instance.ContainsKey(key)
'	End Function

'	''' <summary>注册【实例】</summary>
'	''' <param name="create">实例创建方法</param>
'	Public Function Register(key As TKey, create As Func(Of TValue)) As IdleBus(Of TKey, TValue)
'		InternalRegister(key, create, Nothing, True)
'		Return Me
'	End Function

'	''' <summary>注册【实例】</summary>
'	''' <param name="create">实例创建方法</param>
'	Public Function Register(key As TKey, create As Func(Of TValue), idle As TimeSpan) As IdleBus(Of TKey, TValue)
'		InternalRegister(key, create, idle, True)
'		Return Me
'	End Function

'	''' <summary>注册【实例】</summary>
'	''' <param name="create">实例创建方法</param>
'	Public Function TryRegister(key As TKey, create As Func(Of TValue)) As Boolean
'		Return InternalRegister(key, create, Nothing, False)
'	End Function

'	''' <summary>注册【实例】</summary>
'	''' <param name="create">实例创建方法</param>
'	Public Function TryRegister(key As TKey, create As Func(Of TValue), idle As TimeSpan) As Boolean
'		Return InternalRegister(key, create, idle, False)
'	End Function

'	''' <summary>注册【实例】</summary>
'	Public Function TryRemove(key As TKey, Optional now As Boolean = False) As Boolean
'		Return InternalRemove(key, now, False)
'	End Function

'	''' <summary>注册数量</summary>
'	Public ReadOnly Property Quantity As Integer
'		Get
'			Return Instance.Count
'		End Get
'	End Property

'	''' <summary>通知事件</summary>
'	Public Event Notice As EventHandler(Of NoticeEventArgs)

'	''' <summary>注册</summary>
'	Private Function InternalRegister(key As TKey, create As Func(Of TValue), idle As TimeSpan?, isThrow As Boolean) As Boolean
'		If IsDisposed Then
'			If isThrow Then Throw New Exception($"{key} 注册失败，{NameOf(IdleBus(Of TValue))} 对象已释放")
'			Return False
'		End If

'		Dim err = New Exception($"{key} 注册失败，请勿重复注册")
'		If Instance.ContainsKey(key) Then
'			OnNotice(NoticeTypeEnum.REGISTER, key, err, err.Message)
'			If isThrow Then Throw err
'			Return False
'		End If

'		If idle Is Nothing Then idle = _DefaultIdle

'		Dim added = Instance.TryAdd(key, New ItemInfo(Me, key, create, idle.Value))
'		If added = False Then
'			OnNotice(NoticeTypeEnum.REGISTER, key, err, err.Message)
'			If isThrow Then Throw err
'			Return False
'		End If

'		OnNotice(NoticeTypeEnum.REGISTER, key, Nothing, $"{key} 注册成功，{UsageQuantity}/{Quantity}")

'		Return True
'	End Function

'	''' <summary>移除实例</summary>
'	''' <param name="key">键</param>
'	''' <param name="isNow">是否立即释放</param>
'	''' <param name="isThrow">失败是否抛出异常</param>
'	Private Function InternalRemove(key As TKey, isNow As Boolean, isThrow As Boolean) As Boolean
'		If IsDisposed Then
'			If isThrow Then Throw New Exception($"{key} 删除失败 ，{NameOf(IdleBus(Of TValue))} 对象已释放")
'			Return False
'		End If

'		Dim item As ItemInfo = Nothing

'		If Instance.TryRemove(key, item) = False Then
'			Dim err = New Exception($"{key} 删除失败 ，因为没有注册")
'			OnNotice(NoticeTypeEnum.REMOVE, key, err, err.Message)
'			If isThrow Then Throw err
'			Return False
'		End If

'		Interlocked.Exchange(item.ReleaseErrorCounter, 0)

'		If isNow Then
'			item.Release(Function() True)
'			OnNotice(NoticeTypeEnum.REMOVE, item.Key, Nothing, $"{key} 删除成功，{UsageQuantity}/{Quantity}")
'			Return True
'		End If

'		item.LastActiveTime = Date.Now
'		If item.Value Is Nothing Then item.LastActiveTime = Date.Now.Subtract(item.Idle).AddSeconds(-60) '延时删除

'		RemoveInstance.TryAdd(Guid.NewGuid().ToString(), item)
'		OnNotice(NoticeTypeEnum.REMOVE, item.Key, Nothing, $"{key} 删除成功，并且已标记为延时释放，{UsageQuantity}/{Quantity}")

'		Return True
'	End Function

'	''' <summary>处理延时删除</summary>
'	Private Sub InternalRemoveDelayHandler()
'		Dim removeKeys = RemoveInstance.Keys
'		Dim removeItem As ItemInfo = Nothing, oldItem As ItemInfo = Nothing

'		For Each removeKey In removeKeys
'			If RemoveInstance.TryGetValue(removeKey, removeItem) = False Then Continue For
'			If Date.Now.Subtract(removeItem.LastActiveTime) <= removeItem.Idle Then Continue For

'			Try
'				removeItem.Release(Function() True)
'			Catch ex As Exception
'				Dim tmp1 = Interlocked.Increment(removeItem.ReleaseErrorCounter)
'				OnNotice(NoticeTypeEnum.REMOVE, removeItem.Key, ex, $"{removeKey} ---延时释放执行出错({tmp1}次)：{ex.Message}")
'				If tmp1 < 3 Then Continue For
'			End Try

'			removeItem.Dispose()
'			RemoveInstance.TryRemove(removeKey, oldItem)
'		Next
'	End Sub

'	Private Sub OnNotice(e As NoticeEventArgs)
'		RaiseEvent Notice(Me, e)
'	End Sub

'	Private Sub OnNotice(noticeType As NoticeTypeEnum, key As TKey, exception As Exception, log As String)
'		RaiseEvent Notice(Me, New NoticeEventArgs(noticeType, key, exception, log))
'	End Sub

'#Region "Dispose"

'	Protected Overrides Sub Finalize()
'		Dispose()
'	End Sub

'	Private _IsDisposed As Boolean

'	Public Property IsDisposed As Boolean
'		Get
'			Return _IsDisposed
'		End Get
'		Private Set(value As Boolean)
'			_IsDisposed = value
'		End Set
'	End Property

'	Private ReadOnly _IsDisposedLock As New Object

'	Public Sub Dispose() Implements IDisposable.Dispose
'		If IsDisposed Then Return
'		SyncLock _IsDisposedLock
'			If IsDisposed Then Return
'			IsDisposed = True
'		End SyncLock
'		For Each item In RemoveInstance.Values
'			item.Dispose()
'		Next
'		For Each item In Instance.Values
'			item.Dispose()
'		Next

'		RemoveInstance.Clear()
'		Instance.Clear()

'		_UsageQuantity = 0

'		GC.SuppressFinalize(Me)
'	End Sub
'#End Region

'End Class
