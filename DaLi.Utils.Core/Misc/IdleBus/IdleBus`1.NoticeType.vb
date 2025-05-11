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

'	''' <summary>通知类型</summary>
'	Public Enum NoticeTypeEnum

'		''' <summary>执行 register 方法的时候</summary>
'		REGISTER

'		''' <summary>执行 REMOVE 方法的时候，注意：实际会延时释放【实例】</summary>
'		REMOVE

'		''' <summary>自动创建【实例】的时候</summary>
'		AUTOCREATE

'		''' <summary>自动释放不活跃【实例】的时候</summary>
'		AUTORELEASE

'		''' <summary>获取【实例】的时候</summary>
'		[GET]

'	End Enum

'End Class
