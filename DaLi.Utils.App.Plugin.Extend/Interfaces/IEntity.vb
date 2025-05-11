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
' 	基础数据模型接口
'
' 	name: Interface.IEntity
' 	create: 2023-02-15
' 	memo: 基础数据模型接口
'
' ------------------------------------------------------------

Namespace [Interface]

	''' <summary>基础数据模型接口</summary>
	Public Interface IEntity
		Inherits ICloneable, IBase

		''' <summary>编号</summary>
		<DbSnowflake>
		<DbColumn(IsPrimary:=True, IsIdentity:=False)>
		Property ID As Long

		''' <summary>文本标识</summary>
		ReadOnly Property ID_ As String

		''' <summary>实体加改删操作之前的验证</summary>
		''' <param name="action">基础操作类型：add/edit/delete</param>
		''' <param name="errorMessage">错误消息容器</param>
		''' <param name="db">数据库对象</param>
		''' <param name="context">请求上下文</param>
		''' <param name="source">编辑时更新前的原始值</param>
		Sub Validate(action As EntityBaseActionEnum, errorMessage As ErrorMessage, db As IFreeSql, Optional context As IAppContext = Nothing, Optional source As IEntity = Nothing)

		''' <summary>不能重复字段</summary>
		''' <returns>用于定义字段中不能重复的字段组合，也可以通过 DbIndex 来设置唯一索引键来定义</returns>
		ReadOnly Property DuplicatedFields As List(Of ObjectArray(Of String))
	End Interface

End Namespace
