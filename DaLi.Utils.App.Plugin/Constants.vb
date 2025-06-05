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
' 	create: 2023-02-14
' 	memo: 应用基础常量定义
'
' ------------------------------------------------------------

''' <summary>应用基础常量定义</summary>
Public Module Constants

#Region "常量"

	''' <summary>系统信息 Redis 键</summary>
	Public Const VAR_REDIS_SYSTEM_INFO = "SYSTEM_INFO"

	''' <summary>系统后台服务</summary>
	Public Const VAR_REDIS_STATUS_BACKSERVICE = "STATUS:BACKSERVICE"

	''' <summary>设备信息</summary>
	Public Const VAR_REDIS_STATUS_DEVICE = "STATUS:DEVICE"

	''' <summary>不限设备的标识，如果包含此标识，标识设备不限</summary>
	Public Const VAR_DEVICE_UNLIMITED = -1

#End Region

#Region "事件"

	''' <summary>更新设置</summary>
	Public Const E_SETTING_UPDATE = "[SETTING_UPDATE]"

	''' <summary>任务添加</summary>
	Public Const E_BACKSERVICE_ADD = "[BACKSERVICE_ADD]"

	''' <summary>任务删除</summary>
	Public Const E_BACKSERVICE_REMOVE = "[BACKSERVICE_REMOVE]"

	''' <summary>任务启动</summary>
	Public Const E_BACKSERVICE_START = "[BACKSERVICE_START]"

	''' <summary>任务停止</summary>
	Public Const E_BACKSERVICE_STOP = "[BACKSERVICE_STOP]"

	''' <summary>输出到控制台</summary>
	Public Const E_OUT_CONSOLE = "[OUT_CONSOLE]"

	''' <summary>记录到日志</summary>
	Public Const E_OUT_LOG = "[OUT_LOG]"

#End Region
End Module
