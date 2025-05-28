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
' 	树形扩展时间数据模型基类
'
' 	name: Base.EntityTreeDateExtendBase
' 	create: 2024-08-05
' 	memo: 树形扩展时间数据模型基类
'
' ------------------------------------------------------------

Imports FreeSql.DataAnnotations

Namespace Base

	''' <summary>树形扩展时间数据模型基类</summary>
	<DbIndex("ParentId")>
	Public MustInherit Class EntityTreeDateExtendBase(Of P As IEntityTree(Of P), E As {Class, New})
		Inherits EntityParentDateExtendBase(Of E)
		Implements IEntityTree(Of P)

		''' <summary>父级</summary>
		<Navigate(NameOf(ParentId))>
		Public Property Parent As P Implements IEntityTree(Of P).Parent

		''' <summary>子集列表</summary>
		<Navigate(NameOf(ParentId))>
		Public Property Childs As IEnumerable(Of P) Implements IEntityTree(Of P).Childs
	End Class

	''' <summary>树形扩展时间数据模型基类</summary>
	<DbIndex("ParentId")>
	Public MustInherit Class EntityTreeDateExtendBase(Of P As IEntityTree(Of P))
		Inherits EntityParentDateExtendBase
		Implements IEntityTree(Of P)

		''' <summary>父级</summary>
		<Navigate(NameOf(ParentId))>
		Public Property Parent As P Implements IEntityTree(Of P).Parent

		''' <summary>子集列表</summary>
		<Navigate(NameOf(ParentId))>
		Public Property Childs As IEnumerable(Of P) Implements IEntityTree(Of P).Childs
	End Class
End Namespace