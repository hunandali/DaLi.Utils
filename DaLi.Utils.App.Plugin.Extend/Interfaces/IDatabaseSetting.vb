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
' 	数据库参数
'
' 	name: IDatabaseSetting
' 	create: 2024-07-12
' 	memo: 数据库参数
'
' ------------------------------------------------------------

Namespace [Interface]

	''' <summary>数据库参数</summary>
	Public Interface IDatabaseSetting
		Inherits ISetting

		''' <summary>数据库名称前缀</summary>
		Property Prefix As String

		''' <summary>系统数据库连接列表</summary>
		Property Connections As NameValueDictionary

		''' <summary>附加其他连接字符串</summary>
		Sub UpdateConnections(conns As NameValueDictionary)

	End Interface
End Namespace
