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
' 	应用基础常量定义
'
' 	name: Constants
' 	create: 2023-02-17
' 	memo: 应用基础常量定义
'
' ------------------------------------------------------------

''' <summary>应用基础常量定义</summary>
Public Module ExtendConstants

	''' <summary>控制器应用上下文参数</summary>
	Public Const VAR_CONTROLLER_CONTEXT = "_CONTROLLER_CONTEXT_"

	''' <summary>基础环境 字典数据标识</summary>
	Public Const VAR_DICTIONARY_SYSTEM_ID = 1

	''' <summary>基础环境 字典数据别名</summary>
	Public Const VAR_DICTIONARY_SYSTEM_KEY = "system"

	''' <summary>默认数据库链接字符串</summary>
	Public Const VAR_DATABASE_CONNECTION_DEFAULT = "default"

	''' <summary>日志数据库链接字符串</summary>
	Public Const VAR_DATABASE_CONNECTION_LOG = "log"

	''' <summary>扩展数据库链接字符串</summary>
	Public Const VAR_DATABASE_CONNECTION_EXTEND = "ext"

	''' <summary>模块重载</summary>
	Public Const E_MODULE_RELOAD = "[MODULE_RELOAD]"

	''' <summary>字典重载</summary>
	Public Const E_DICTIONARY_RELOAD = "[DICTIONARY_RELOAD]"

End Module
