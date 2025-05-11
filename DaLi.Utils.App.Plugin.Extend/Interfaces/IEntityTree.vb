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
' 	树形数据模型接口
'
' 	name: Interface.IEntityTree
' 	create: 2024-08-05
' 	memo: 树形数据模型接口
'
' ------------------------------------------------------------

Namespace [Interface]

	''' <summary>树形数据模型接口</summary>
	Public Interface IEntityTree(Of T As IEntityTree(Of T))
		Inherits IEntityParent

		''' <summary>上级</summary>
		Property Parent As T

		''' <summary>下级列表</summary>
		Property Childs As IEnumerable(Of T)

	End Interface

End Namespace
