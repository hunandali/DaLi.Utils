
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
' 	数据列表扩展
'
' 	name: DataListExtension
' 	create: 2024-08-18
' 	memo: 数据列表扩展
'
' ------------------------------------------------------------

Imports System.Runtime.CompilerServices

Namespace Extension
	''' <summary>数据列表扩展</summary>
	Public Module DataListExtension

		''' <summary>获取自定义属性列表</summary>
		<Extension>
		Public Function ToDataTree(Of T)(this As IEnumerable(Of DataList(Of T)), Optional parent As T = Nothing, Optional distinct As Boolean = True) As List(Of DataTree(Of T))
			If this.IsEmpty Then Return Nothing

			' 移除无效数据
			If distinct Then
				this = this.Where(Function(x) x.Value IsNot Nothing)
				this = this.Distinct(Function(x) x.Value)
			End If

			' 分析树形结构
			Dim data As IEnumerable(Of DataList(Of T)) = this.ToList

			If parent Is Nothing Then
				data = data.Where(Function(x) x.Parent Is Nothing)
			Else
				data = data.Where(Function(x) parent.Equals(x.Parent))
			End If

			Return data.Select(Function(x) New DataTree(Of T)(x) With {.Children = this.ToDataTree(x.Value, False)}).ToList
		End Function

		''' <summary>获取自定义属性列表</summary>
		<Extension>
		Public Function ToDataTree(Of T, E)(this As IEnumerable(Of DataList(Of T, E)), Optional parent As T = Nothing, Optional distinct As Boolean = True) As List(Of DataTree(Of T, E))
			If this.IsEmpty Then Return Nothing

			' 移除无效数据
			If distinct Then
				this = this.Where(Function(x) x.Value IsNot Nothing)
				this = this.Distinct(Function(x) x.Value)
			End If

			' 分析树形结构
			Dim data As IEnumerable(Of DataList(Of T, E)) = this.ToList

			If parent Is Nothing Then
				data = data.Where(Function(x) x.Parent Is Nothing)
			Else
				data = data.Where(Function(x) parent.Equals(x.Parent))
			End If

			Return data.Select(Function(x) New DataTree(Of T, E)(x) With {.Children = this.ToDataTree(x.Value, False)}).ToList
		End Function

	End Module
End Namespace
