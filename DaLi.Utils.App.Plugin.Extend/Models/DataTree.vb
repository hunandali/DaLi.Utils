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
' 	树形类型
'
' 	name: Model.DataTree
' 	create: 2023-02-19
' 	memo: 树形类型
'
' ------------------------------------------------------------

Namespace Model

	''' <summary>树形类型</summary>
	Public Class DataTree
		Inherits DataTree(Of Object, Object)

		Public Sub New()
		End Sub

		Public Sub New(item As DataList)
			MyBase.New(item)
		End Sub

		Public Sub New(item As DataTree)
			MyBase.New(item)
		End Sub

		''' <summary>下级</summary>
		Public Overloads Property Children As List(Of DataTree)
	End Class

	''' <summary>树形类型</summary>
	Public Class DataTree(Of T)
		Inherits DataTree(Of T, Object)

		Public Sub New()
		End Sub

		Public Sub New(item As DataList(Of T))
			MyBase.New(item)
		End Sub

		Public Sub New(item As DataTree(Of T))
			MyBase.New(item)
		End Sub

		''' <summary>下级</summary>
		Public Overloads Property Children As List(Of DataTree(Of T))
	End Class

	''' <summary>树形类型</summary>
	Public Class DataTree(Of T, E)
		Inherits DataList(Of T, E)

		Public Sub New()
		End Sub

		Public Sub New(item As DataList(Of T, E))
			If item Is Nothing Then Return

			Text = item.Text
			Value = item.Value
			Parent = item.Parent
			Disabled = item.Disabled
			Ext = item.Ext
		End Sub

		Public Sub New(item As DataTree(Of T, E))
			If item Is Nothing Then Return

			Text = item.Text
			Value = item.Value
			Parent = item.Parent
			Disabled = item.Disabled
			Ext = item.Ext
			Children = item.Children
		End Sub

		''' <summary>下级</summary>
		Public Property Children As List(Of DataTree(Of T, E))

	End Class
End Namespace
