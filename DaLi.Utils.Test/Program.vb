' ------------------------------------------------------------
'
' 	Copyright © 2023 湖南大沥网络科技有限公司.
' 	Dali.App Is licensed under Mulan PSL v2.
'
' 	  author:	木炭(WOODCOAL)
' 	   email:	i@woodcoal.cn
' 	homepage:	http://www.hunandali.com/
'
' 	请依据 Mulan PSL v2 的条款使用本项目。获取 Mulan PSL v2 请浏览 http://license.coscl.org.cn/MulanPSL2
'
' ------------------------------------------------------------
'
' 	网站入口
'
' 	name: Program
' 	create: 2023-03-02
' 	memo: 网站入口
'
' ------------------------------------------------------------

Imports DaLi.Plugin.Utils

Public Class Program
	Public Shared Sub Main()
		' 含数据库启动模式
		Call App.Extend.Start()

		' 基础应用启动模式
		'Call App.Start()

		' 手动启动模式
		' 直接从此开始启动代码
		If False Then Dim x As New SwashbuckleSetting
	End Sub

End Class
