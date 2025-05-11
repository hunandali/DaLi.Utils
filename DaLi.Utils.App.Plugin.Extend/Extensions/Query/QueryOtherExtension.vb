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
' 	FreeSQL 查询扩展
'
' 	name: Extension.QueryExtension
' 	create: 2023-02-15
' 	memo: FreeSQL 查询扩展
'
' ------------------------------------------------------------

Imports System.Linq.Expressions
Imports System.Runtime.CompilerServices

Namespace Extension
	Partial Public Module QueryExtension

		''' <summary>指定栏目节点是否包含子栏目</summary>
		<Extension>
		Public Function HasChilds(Of T As {IEntityParent, Class})(this As IFreeSql, entity As T) As Boolean
			If entity Is Nothing OrElse entity.ID = 0 Then Return False

			Return this.Select(Of T).WhereEquals(entity.ID, Function(x) x.ParentId).Any
		End Function

		''' <summary>指定上级表示下是否包含信息数据</summary>
		<Extension>
		Public Function HasChilds(this As IFreeSql, infoType As Type, parentId As Long) As Boolean
			If infoType Is Nothing OrElse parentId.IsEmpty Then Return False

			Return this.Select(Of IEntityParent).AsType(infoType).WhereEquals(parentId, Function(x) x.ParentId).Any
		End Function

		''' <summary>动态创建条件 Function(x) x.Field </summary>
		Public Function MakeFunction(Of T As Class)(field As String) As LambdaExpression
			Dim expPar = Expression.Parameter(GetType(T), "x")
			Dim expPro = Expression.Property(expPar, field)

			Return Expression.Lambda(expPro, expPar)
		End Function

		''' <summary>是否来自树形结构数据</summary>
		Public Function IsEntityTree(Of T As IEntityParent)() As Boolean
			Return IsEntityTree(GetType(T))
		End Function

		''' <summary>是否来自树形结构数据</summary>
		<Extension>
		Public Function IsEntityTree(type As Type) As Boolean
			If type Is Nothing Then Return False

			Return type.GetInterfaces.Any(Function(x) x.Name.StartsWith("IEntityTree`"))
		End Function

	End Module

End Namespace