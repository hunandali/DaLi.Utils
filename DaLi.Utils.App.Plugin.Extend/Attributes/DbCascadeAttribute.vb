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
' 	数据实体是否允许级联操作
'
' 	name: Attribute.DbCascade
' 	create: 2024-08-24
' 	memo: 数据实体是否允许级联操作，对于级联数据有效
'		  如：实体 A 的内容位于 实体 B，在 A 与 B 建立好关联后，如果添加、修改 A 时包含完整的 B 信息，则将自动添加，修改 B 中数据
'			 删除 A 时也将自动删除对应 B 中的数据
'			 如果不设置此属性， 则仅操作 A 本身， 而不处理 B
'
' ------------------------------------------------------------

Namespace Attribute

	''' <summary>数据实体是否允许级联操作，如级联添加，修改与删除</summary>
	<AttributeUsage(AttributeTargets.Class, AllowMultiple:=False)>
	Public Class DbCascadeAttribute
		Inherits System.Attribute

	End Class
End Namespace

