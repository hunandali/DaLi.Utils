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
' 	状态消息
'
' 	name: Model.StatusMessage
' 	create: 2024-07-02
' 	memo: 状态消息
'
' ------------------------------------------------------------

Namespace Model

	''' <summary>状态消息</summary>
	Public Class StatusMessage

		''' <summary>操作时间</summary>
		Public Property TimeAction As Date

		''' <summary>消息类型</summary>
		Public Property Type As StatusMessageEnum

		''' <summary>历史消息</summary>
		Public Property Message As String

		''' <summary>获取文本内容</summary>
		Public Overloads Function ToString() As String
			Dim ret As New List(Of String) From {TimeAction.ToString("yyyy-MM-dd HH:mm:ss")}

			Select Case Type
				Case StatusMessageEnum.IMPORTANT
					ret.Add("⚠")
				Case StatusMessageEnum.DEBUG
					ret.Add("🐞")
				Case StatusMessageEnum.ERROR
					ret.Add("❌")
				Case StatusMessageEnum.SUCCESS
					ret.Add("✔")
				Case StatusMessageEnum.START
					ret.Add("🔵")
				Case StatusMessageEnum.FINISH
					ret.Add("⛔")
				Case Else
					ret.Add("ℹ")
			End Select

			ret.Add(Message)

			Return ret.JoinString(" ")
		End Function
	End Class
End Namespace