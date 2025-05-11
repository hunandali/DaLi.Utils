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
' 	实体自定义属性
'
' 	name: EntityCustomAttribute
' 	create: 2024-06-29
' 	memo: 对实体进行自定义属性设置，如：AI 总结
'
' ------------------------------------------------------------

Namespace Attribute

	''' <summary>实体自定义属性</summary>
	<AttributeUsage(AttributeTargets.Property, AllowMultiple:=True)>
	Public Class EntityCustomAttribute
		Inherits System.Attribute

		''' <summary>操作源（如：AI）</summary>
		Public Property Provider As String

		''' <summary>自定义操作（如：总结）</summary>
		Public Property Action As String

		''' <summary>关联字段（如：AI 总结时数据来源字段）</summary>
		Public Property Source As String

		''' <summary>附加操作数据，参数（如：AI 总结的提示词）</summary>
		Public Property Data As String

		''' <summary>是否仅数据为空是才操作</summary>
		Public Property EmptyOnly As Boolean

		''' <summary>构造</summary>
		Public Sub New(provider As String, Optional action As String = "", Optional source As String = "", Optional emptyOnly As Boolean = False, Optional data As String = "")
			Me.Source = source
			Me.Action = action
			Me.Provider = provider
			Me.EmptyOnly = emptyOnly
			Me.data = data
		End Sub

	End Class
End Namespace
