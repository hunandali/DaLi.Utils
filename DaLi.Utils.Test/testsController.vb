' ------------------------------------------------------------
'
' 	Copyright © 2021 湖南大沥网络科技有限公司.
' 	Dali.Utils Is licensed under Mulan PSL v2.
'
' 	  author:	木炭(WOODCOAL)
' 	   email:	a@hndl.vip
' 	homepage:	http://www.hunandali.com/
'
' 	请依据 Mulan PSL v2 的条款使用本项目。获取 Mulan PSL v2 请浏览 http://license.coscl.org.cn/MulanPSL2
'
' ------------------------------------------------------------
'
' 	title
'
' 	name: testsController.vb
' 	create: 2024
' 	memo: introduce
'
' ------------------------------------------------------------

Imports DaLi.Utils.App.Attribute
Imports DaLi.Utils.App.Base
Imports DaLi.Utils.App.Model.Enums
Imports Microsoft.AspNetCore.Mvc

<Route("api/[controller]")>
<Env(EnvRunEnum.APIKEY)>
Public Class testsController
	Inherits CtrBase

	<HttpGet>
	Public Function Hi() As IActionResult
		Return Succ("Hello World!")
	End Function
End Class
