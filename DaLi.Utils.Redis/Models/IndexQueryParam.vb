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
' 	索引查询参数
'
' 	name: IndexQueryParam
' 	create: 2024-07-25
' 	memo: 索引查询参数
'
' ------------------------------------------------------------

Namespace Model

	''' <summary>索引查询参数</summary>
	Public Class IndexQueryParam

		''' <summary>查询的键</summary>
		Public Property Key As String

		''' <summary>查询的值</summary>
		Public Property Value As Object

		''' <summary>查询操作</summary>
		Public Property Action As IndexQueryActionEnum

		Public Sub New(key As String, Optional value As Object = Nothing, Optional action As IndexQueryActionEnum = IndexQueryActionEnum.EQUAL)
			Me.Key = key
			Me.Value = value
			Me.Action = action
		End Sub

		''' <summary>获取查询字符串</summary>
		Public ReadOnly Property QueryString As String
			Get
				' 键无效返回空值
				If Key.IsEmpty Then Return ""

				' 值无效则直接返回键
				If IsEmptyValue(Value) Then Return $"@.{Key}"

				' 值转换（文本加引号，数值直接使用）
				Dim valueStr As String
				Dim type = Value.GetType
				If type.IsNullableNumber OrElse type.IsNullableBoolean Then
					valueStr = Value.ToString
				Else
					valueStr = $"""{TypeExtension.ToObjectString(Value)}"""
				End If

				Dim actionStr = "=="
				Select Case Action
					Case IndexQueryActionEnum.NOTEQUAL
						actionStr = "!="

					Case IndexQueryActionEnum.LIKE
						actionStr = "=~"

					Case IndexQueryActionEnum.LESS
						actionStr = "<"

					Case IndexQueryActionEnum.LESSEQUAL
						actionStr = "<="

					Case IndexQueryActionEnum.GREATER
						actionStr = ">"

					Case IndexQueryActionEnum.GREATEREQUAL
						actionStr = ">="
				End Select

				' 返回查询条件
				Return $"@.{Key}{actionStr}{valueStr}"
			End Get
		End Property
	End Class

End Namespace