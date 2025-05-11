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
' 	索引状态
'
' 	name: IndexStatus
' 	create: 2024-07-31
' 	memo: 索引状态
'
' ------------------------------------------------------------

Namespace Model

	''' <summary>索引状态</summary>
	Public Class IndexStatus(Of V)

		''' <summary>最后标识</summary>
		Public Property LastId As V

		''' <summary>最后操作时间</summary>
		Public Property LastTime As Date

		''' <summary>累计处理项目</summary>
		Public Property Count As Integer

		''' <summary>是否操作完成</summary>
		Public Property IsFinish As Boolean

		''' <summary>构造</summary>
		Public Sub New(Optional lastId As V = Nothing)
			Me.LastId = lastId
			LastTime = Date.MinValue
			Count = 0
			IsFinish = False
		End Sub

		''' <summary>更新最后标识</summary>
		Public Sub Update(lastId As V)
			Me.LastId = lastId
			LastTime = DATE_NOW
			Count += 1
			IsFinish = False
		End Sub

		''' <summary>标识完成</summary>
		Public Sub Finished()
			IsFinish = True
		End Sub
	End Class

End Namespace