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
' 	枚举
'
' 	name: Enums
' 	create: 2024-07-25
' 	memo: 枚举
'
' ------------------------------------------------------------

Namespace Model

	''' <summary>索引查询操作方式枚举</summary>
	Public Enum IndexQueryActionEnum

		''' <summary>等于</summary>
		EQUAL

		''' <summary>不等于</summary>
		NOTEQUAL

		''' <summary>模糊查询</summary>
		[LIKE]

		''' <summary>小于</summary>
		LESS

		''' <summary>小于等于</summary>
		LESSEQUAL

		''' <summary>大于</summary>
		GREATER

		''' <summary>大于等于</summary>
		GREATEREQUAL
	End Enum

End Namespace