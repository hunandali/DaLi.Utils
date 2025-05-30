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
' 	字典数据
'
' 	name: Entity.DictionaryEntity
' 	create: 2023-02-19
' 	memo: 字典数据
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations
Imports FreeSql.Internal.Model

Namespace Entity

	''' <summary>字典数据</summary>
	<DbTable("App_Dictionary")>
	<DbIndex(NameOf(DictionaryEntity.Key))>
	<DbModule(2, "字典")>
	Public Class DictionaryEntity
		Inherits EntityTreeBase(Of DictionaryEntity)

		''' <summary>标识</summary>
		<Display(Name:="标识")>
		<MaxLength(50)>
		<DbQuery(DynamicFilterOperator.Contains)>
		Public Property Key As String

		''' <summary>值</summary>
		<Display(Name:="值")>
		<Required>
		<MaxLength(200)>
		<DbQuery(DynamicFilterOperator.Contains)>
		Public Property Value As String

		''' <summary>节点类型（-1. 分组，0.值对象，1. 单选，>1 多选数量）</summary>
		''' <remarks>
		''' 分组下仅能存放单/多选；
		''' 单/多选下仅能存放值；
		''' 值下不能存放任何东西
		''' </remarks>
		<Display(Name:="下级多选数量")>
		Public Property Muti As Integer

		''' <summary>是否系统参数，系统参数不允许删除</summary>
		<Display(Name:="是否系统")>
		<DbQuery>
		Public Property IsSystem As Boolean

		''' <summary>是否必选</summary>
		<Display(Name:="是否必选")>
		<DbQuery>
		Public Property Required As Boolean

		''' <summary>是否启用</summary>
		<Display(Name:="是否启用")>
		<DbQuery>
		Public Property Enabled As Boolean

		''' <summary>最后更新时间</summary>
		<Display(Name:="最后更新时间")>
		<DbColumn(ServerTime:=DateTimeKind.Local)>
		Public Property UpdateTime As Date

		''' <summary>是否分组</summary>
		Public ReadOnly Property IsGroup As Boolean
			Get
				Return Muti <> 0
			End Get
		End Property

	End Class
End Namespace