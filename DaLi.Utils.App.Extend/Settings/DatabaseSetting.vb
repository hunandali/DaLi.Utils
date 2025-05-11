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
' 	name: Setting.Database
' 	create: 2023-02-17
' 	memo: 数据库参数
'
' ------------------------------------------------------------

Namespace Setting

	''' <summary>数据库参数</summary>
	Public Class DatabaseSetting
		Inherits LocalSettingBase(Of DatabaseSetting)
		Implements IDatabaseSetting

		''' <summary>数据库名称前缀</summary>
		<FieldChange(FieldTypeEnum.ASCII)>
		Public Property Prefix As String Implements IDatabaseSetting.Prefix

		''' <summary>系统数据库连接列表</summary>
		Public Property Connections As NameValueDictionary Implements IDatabaseSetting.Connections

		''' <summary>缓存数据库连接，用于记录默认设置</summary>
		Private _Conns As NameValueDictionary

		Protected Overrides Sub Initialize(provider As ISettingProvider)
			_Conns = If(Connections, New NameValueDictionary)

			Dim defConnect = ""
			Dim logConnect = ""

			' 赋值默认连接，防止默认链接未设置时取第一条数据
			If _Conns.NotEmpty Then
				defConnect = Connections(VAR_DATABASE_CONNECTION_DEFAULT).EmptyValue(_Conns.First.Value)
				logConnect = Connections(VAR_DATABASE_CONNECTION_LOG).EmptyValue(defConnect)
			End If

			If defConnect.IsEmpty Then
				Throw New Exception("未设置有效的数据库连接！！！")
			Else
				_Conns(VAR_DATABASE_CONNECTION_DEFAULT) = defConnect
				_Conns(VAR_DATABASE_CONNECTION_LOG) = logConnect
			End If

			' 克隆连接数据
			Connections = _Conns.Clone

			' 赋值前缀
			If Prefix.IsUserName Then DbTableAttribute.Prefix = Prefix
		End Sub

		''' <summary>附加其他连接字符串</summary>
		Public Sub UpdateConnections(conns As NameValueDictionary) Implements IDatabaseSetting.UpdateConnections
			If conns.IsEmpty Then Return

			' 移除与内置重名项目
			conns.Remove(_Conns.Keys.ToArray)

			' 克隆连接历史数据
			Connections = _Conns.Clone
			Connections.UpdateRange(conns, True)
		End Sub
	End Class

End Namespace

