﻿' ------------------------------------------------------------
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
' 	带扩展操作的数据接口
'
' 	name: Interface.IEntityExtend
' 	create: 2023-02-15
' 	memo: 带扩展操作的数据接口
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations
Imports System.Text.Json.Serialization
Imports FreeSql.DataAnnotations
Imports FreeSql.Internal.Model

Namespace [Interface]

	''' <summary>带扩展操作的数据接口</summary>
	Public Interface IEntityExtend(Of T As {Class, New})
		Inherits IEntity

		''' <summary>扩展内容</summary>
		<Display(Name:="Extension")>
		<JsonIgnore>
		<JsonMap>
		<DbColumn(Position:=-5)>
		<Output(TristateEnum.FALSE)>
		<DbQuery(DynamicFilterOperator.Contains)>
		Property Extension As T

	End Interface

	''' <summary>带扩展操作的数据接口</summary>
	Public Interface IEntityExtend
		Inherits IEntityExtend(Of NameValueDictionary)

	End Interface

End Namespace
