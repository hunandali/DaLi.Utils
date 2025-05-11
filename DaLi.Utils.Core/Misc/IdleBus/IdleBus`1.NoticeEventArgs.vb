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

'Partial Public Class IdleBus(Of TKey, TValue As {Class, IDisposable})
'	''' <summary>事件参数，用于传递闲忙信号量的各种通知信息</summary>
'	Public Class NoticeEventArgs
'		Inherits EventArgs

'		''' <summary>通知类型属性，用于标识通知的类型</summary>
'		Public ReadOnly Property NoticeType As NoticeTypeEnum

'		''' <summary>键</summary>
'		Public ReadOnly Property Key As TKey

'		''' <summary>异常</summary>
'		Public ReadOnly Property Exception As Exception

'		''' <summary>日志</summary>
'		Public ReadOnly Property Log As String

'		Public Sub New(noticeType As NoticeTypeEnum, key As TKey, exception As Exception, log As String)
'			Me.NoticeType = noticeType
'			Me.Key = key
'			Me.Exception = exception
'			Me.Log = log
'		End Sub

'	End Class
'End Class