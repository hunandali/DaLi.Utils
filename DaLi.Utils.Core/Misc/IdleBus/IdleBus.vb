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
'' 	memo: 空闲对象容器管理，基于 https://github.com/2881099/IdleBus (ver:1.5.3)修改
''
'' ------------------------------------------------------------

'''' <summary>空闲对象容器管理，可实现自动创建、销毁、扩张收缩，解决【实例】长时间占用问题</summary>
'Public Class IdleBus
'	Inherits IdleBus(Of String, IDisposable)

'	''' <summary>按空闲时间1分钟，创建空闲容器</summary>
'	Public Sub New()
'		MyBase.New()
'	End Sub

'	''' <summary>指定空闲时间、创建空闲容器</summary>
'	''' <param name="idle">空闲时间</param>
'	Public Sub New(idle As TimeSpan)
'		MyBase.New(idle)
'	End Sub
'End Class

'''' <summary>空闲对象容器管理，可实现自动创建、销毁、扩张收缩，解决【实例】长时间占用问题</summary>
'Public Class IdleBus(Of TValue As {Class, IDisposable})
'	Inherits IdleBus(Of String, TValue)
'	''' <summary>按空闲时间1分钟，创建空闲容器</summary>
'	Public Sub New()
'		MyBase.New()
'	End Sub

'	''' <summary>指定空闲时间、创建空闲容器</summary>
'	''' <param name="idle">空闲时间</param>
'	Public Sub New(idle As TimeSpan)
'		MyBase.New(idle)
'	End Sub
'End Class