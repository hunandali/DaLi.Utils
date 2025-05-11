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
'	''' <summary>线程是否已启动</summary>
'	Private _ThreadStarted As Boolean = False

'	''' <summary>标记线程是否已启动的锁</summary>
'	Private _ThreadStartedLock As New Object

'	''' <summary>检测和处理空闲对象</summary>
'	''' <param name="item">空闲对象的信息</param>
'	Private Sub ThreadScanWatch(item As ItemInfo)
'		Dim startThread = False

'		' 双重检查锁定，确保只有一个线程被启动
'		If _ThreadStarted = False Then
'			SyncLock _ThreadStartedLock
'				If _ThreadStarted = False Then
'					startThread = _ThreadStarted = True
'				End If
'			End SyncLock
'		End If

'		' 如果需要启动新线程，则执行
'		If startThread Then
'			Task.Run(Sub()
'						 ThreadScanWatchHandler()

'						 SyncLock _ThreadStartedLock
'							 _ThreadStarted = False
'						 End SyncLock
'					 End Sub)
'		End If
'	End Sub

'	''' <summary>扫描过期对象的设置</summary>
'	Public Class TimeoutScanOptions
'		''' <summary>扫描线程间隔（默认值：2秒）</summary>
'		Public Property Interval As TimeSpan = TimeSpan.FromSeconds(2)

'		''' <summary>扫描线程空闲多少秒退出（默认值：10秒）</summary>
'		Public Property QuitWaitSeconds As Integer = 10

'		''' <summary>扫描的每批数量（默认值：512）；可防止注册数量太多时导致 CPU 占用过高</summary>
'		Public Property BatchQuantity As Integer = 512

'		''' <summary>达到扫描的每批数量时，线程等待（默认值：1秒）</summary>
'		Public Property BatchQuantityWait As TimeSpan = TimeSpan.FromSeconds(1)
'	End Class

'	''' <summary>
'	''' 扫描过期对象的设置<para></para>
'	''' 机制：当窗口里有存活对象时，扫描线程才会开启（只开启一个线程）。<para></para>
'	''' 连续多少秒都没存活的对象时，才退出扫描。
'	''' </summary>
'	Public ReadOnly Property ScanOptions As TimeoutScanOptions = New TimeoutScanOptions()

'	''' <summary>线程监控处理器，定期检查并移除过期对象</summary>
'	Private Sub ThreadScanWatchHandler()
'		Dim couter = 0

'		Dim item As ItemInfo = Nothing
'		While IsDisposed = False
'			' 检查间隔时间
'			If ThreadJoin(ScanOptions.Interval) = False Then Return

'			' 移除延迟处理的对象
'			InternalRemoveDelayHandler()

'			' 如果使用量为0，增加计数器
'			If _usageQuantity = 0 Then
'				couter += CInt(ScanOptions.Interval.TotalSeconds)
'				If couter < ScanOptions.QuitWaitSeconds Then Continue While
'				Exit While
'			End If
'			couter = 0

'			' 获取所有键的数组
'			Dim keys = Instance.Keys.ToArray()
'			Dim keysIndex As Long = 0
'			For Each key In keys
'				' 检查是否已处理完毕
'				If IsDisposed Then Return
'				Interlocked.Increment(keysIndex)

'				If ScanOptions.BatchQuantity > 0 AndAlso keysIndex Mod ScanOptions.BatchQuantity = 0 Then
'					If ThreadJoin(ScanOptions.BatchQuantityWait) = False Then Return
'				End If

'				' 尝试获取对象并处理
'				If Instance.TryGetValue(key, item) = False Then Continue For
'				If item.Value Is Nothing Then Continue For
'				If Date.Now.Subtract(item.LastActiveTime) <= item.Idle Then Continue For
'				Try
'					Dim now = Date.Now
'					' 检查是否需要释放对象
'					If item.Release(Function() Date.Now.Subtract(item.LastActiveTime) > item.Idle AndAlso item.LastActiveTime >= item.CreateTime) Then
'						' 防止并发有其他线程创建，最后活动时间 > 创建时间
'						OnNotice(New NoticeEventArgs(NoticeTypeEnum.AUTORELEASE, item.Key, Nothing, $"{key} ---自动释放成功，耗时 {Date.Now.Subtract(now).TotalMilliseconds}ms，{_UsageQuantity}/{Quantity}"))
'					End If
'				Catch ex As Exception
'					OnNotice(New NoticeEventArgs(NoticeTypeEnum.AUTORELEASE, item.Key, ex, $"{key} ---自动释放执行出错：{ex.Message}"))
'				End Try
'			Next
'		End While
'	End Sub

'	''' <summary>等待指定时间间隔，允许线程安全退出</summary>
'	''' <param name="interval">等待的时间间隔</param>
'	''' <returns>如果线程被安全退出，则返回false；否则返回true</returns>
'	Private Function ThreadJoin(interval As TimeSpan) As Boolean
'		If interval <= TimeSpan.Zero Then Return True

'		Dim milliseconds = interval.TotalMilliseconds
'		Dim seconds = Math.Floor(milliseconds / 1000)
'		milliseconds -= seconds * 1000

'		For a = 0 To seconds - 1
'			Thread.CurrentThread.Join(TimeSpan.FromSeconds(1))
'			If IsDisposed Then Return False
'		Next

'		For a = 0 To milliseconds - 1 Step 200
'			Thread.CurrentThread.Join(TimeSpan.FromMilliseconds(200))
'			If IsDisposed Then Return False
'		Next

'		Return True
'	End Function
'End Class
