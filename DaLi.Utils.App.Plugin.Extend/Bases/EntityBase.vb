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
' 	数据模型基类
'
' 	name: Base.EntityBase
' 	create: 2023-02-18
' 	memo: 数据模型基类
'
' ------------------------------------------------------------

Imports System.ComponentModel.DataAnnotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Reflection
Imports System.Text.Json
Imports System.Text.Json.Serialization
Imports DaLi.Utils.Json
Imports FreeSql
Imports FreeSql.Internal.Model

Namespace Base

	''' <summary>数据模型基类</summary>
	Public MustInherit Class EntityBase
		Implements IEntity

		''' <summary>编号</summary>
		<DbSnowflake>
		<DbColumn(IsPrimary:=True, IsIdentity:=False)>
		Public Overridable Property ID As Long Implements IEntity.ID

		''' <summary>文本标识</summary>
		Public ReadOnly Property _ID_ As String Implements IEntity.ID_
			Get
				Return ID.ToString
			End Get
		End Property

		''' <summary>克隆</summary>
		Public Function Clone() As Object Implements ICloneable.Clone
			Return MemberwiseClone()
		End Function

		''' <summary>获取本模块的标识</summary>
		Public Function GetModuleId() As UInteger
			Return ExtendHelper.GetModuleId([GetType])
		End Function

#Region "附加参数处理"

		''' <summary>扩展属性，仅用于传递数据，不存入数据库</summary>
		<NotMapped>
		<Output(TristateEnum.FALSE)>
		Public Property Ext As Object

		''' <summary>尝试将扩展数据转换对象</summary>
		Public Function TryExtObject() As Object
			If Ext?.GetType = GetType(JsonElement) Then
				Return JsonExtension.Parse(Ext, True)
				'Return JsonElementParse(Ext, True)
			Else
				Return Ext
			End If
		End Function

		''' <summary>尝试将扩展数据转换成数组，如果是字符串则使用都好分割获取</summary>
		Public Function TryExtArray() As String()
			Dim data = TryExtObject()
			If data Is Nothing Then Return Nothing

			If data.GetType.IsString Then
				' 如果未包含引号，直接分解未数组，否则使用 Json 解析
				Dim sExt = data.ToString
				If sExt.NotEmpty Then
					If sExt.Contains(""""c) Then
						Return sExt.FromJson(Of String())
					Else
						Return sExt.SplitEx
					End If
				End If
			ElseIf data.GetType.IsEnumerable Then
				Dim list = TryCast(data, IEnumerable(Of Object))
				If list IsNot Nothing Then Return list.Select(Function(x) x.ToString).Distinct.ToArray
			End If

			Return Nothing
		End Function

		''' <summary>尝试将扩展数据转换成标识列表，如果是字符串则使用都好分割获取</summary>
		Public Function TryExtIds() As Long()
			Return TryExtArray()?.Select(Function(x) x.ToLong).Where(Function(x) x.NotEmpty).Distinct.ToArray
		End Function

#End Region

		''' <inheritdoc />
		Public Sub Validate(action As EntityBaseActionEnum, errorMessage As ErrorMessage, db As IFreeSql, Optional context As IAppContext = Nothing, Optional source As IEntity = Nothing) Implements IEntity.Validate
			' 编辑模式如果原始数据不存在查询数据库
			If action = EntityBaseActionEnum.EDIT Then
				' 无有效的ID则直接返回
				If ID.IsEmpty Then
					errorMessage.Notification = "Error.NoData"
					Return
				End If

				' 原始数据不存在查询数据库
				If source Is Nothing Then source = db.Select([GetType]).WhereID(ID).ToOne

				' 查询后仍然不存在数据则直接返回
				If source Is Nothing Then
					errorMessage.Notification = "Error.NoData"
					Return
				End If
			End If

			context = If(context, SYS.GetService(Of IAppContext))

			' 实体参数验证
			EntityValidate(action, errorMessage, db, context, source)
			If Not errorMessage.IsPass Then Return

			' 添加与编辑是字段不重复验证
			Select Case action
				Case EntityBaseActionEnum.ADD, EntityBaseActionEnum.EDIT
					' 验证字段是否符合要求
					Dim entity = If(EntityBaseActionEnum.ADD, Me, source.ClassAssign(Me, True))
					ValidateFields(entity, errorMessage)
					If Not errorMessage.IsPass Then Return

					' 检查字段内容是否唯一
					DuplicateValidate(Me, errorMessage, db)
			End Select
		End Sub

		''' <summary>实体加改删操作之前的验证</summary>
		''' <param name="action">基础操作类型：add/edit/delete</param>
		''' <param name="errorMessage">错误消息容器</param>
		''' <param name="db">数据库对象</param>
		''' <param name="context">请求上下文</param>
		''' <param name="source">编辑时更新前的原始值</param>
		Protected Overridable Sub EntityValidate(action As EntityBaseActionEnum, errorMessage As ErrorMessage, db As IFreeSql, context As IAppContext, Optional source As IEntity = Nothing)
		End Sub

		''' <inheritdoc />
		Public Sub Finish(action As EntityBaseActionEnum, errorMessage As ErrorMessage, db As IFreeSql, Optional context As IAppContext = Nothing) Implements IEntity.Finish
			context = If(context, SYS.GetService(Of IAppContext))

			' 实体参数验证
			EntityFinish(action, errorMessage, db, context)
		End Sub

		''' <summary>实体加改删成功之后的操作</summary>
		''' <param name="action">基础操作类型：add/edit/delete</param>
		''' <param name="errorMessage">错误消息容器</param>
		''' <param name="db">数据库对象</param>
		''' <param name="context">请求上下文</param>
		Protected Overridable Sub EntityFinish(action As EntityBaseActionEnum, errorMessage As ErrorMessage, db As IFreeSql, context As IAppContext)
		End Sub

		''' <summary>不能重复字段</summary>
		''' <returns>用于定义字段中不能重复的字段组合，也可以通过 DbIndex 来设置唯一索引键来定义</returns>
		<Output(TristateEnum.FALSE)>
		<JsonIgnore>
		Protected Overridable ReadOnly Property DuplicatedFields As List(Of ObjectArray(Of String)) Implements IEntity.DuplicatedFields

		''' <summary>通过唯一索引验证字段实体数据是否存在重复</summary>
		''' <param name="entity">当前实体</param>
		''' <param name="errorMessage">错误消息容器</param>
		''' <param name="db">数据库对象</param>
		Public Shared Sub DuplicateValidate(Of T As {IEntity, Class})(entity As T, errorMessage As ErrorMessage, db As IFreeSql)
			Dim duplicate As New DuplicatedFields(entity, db)

			' 从 DbIndexAttribute 属性中获取不能重复的键
			Dim attrs = entity.GetType.GetCustomAttributes(Of DbIndexAttribute)
			If attrs.NotEmpty Then
				For Each attr In attrs
					' 非唯一索引，跳过
					If Not attr.IsUnique Then Continue For

					' 字段名称可多个组合，多个组合之间用逗号分隔，名称可能包含排序，如：a desc,b asc
					Dim names = attr.Fields.Split(","c)?.Select(Function(x) x.Trim.Split(" ")(0)).Distinct.ToArray
					If names.IsEmpty Then Continue For

					duplicate = duplicate.Insert(names)

				Next
			End If

			' 从属性中获取不能重复的键
			entity.DuplicatedFields?.ForEach(Sub(keys)
												 duplicate = duplicate.Insert(keys.ToArray())
											 End Sub)

			Dim fields = duplicate.Execute
			fields?.ForEach(Sub(keys)
								' 存在重复项目
								For Each key In keys
									errorMessage.Add(key, "Error.Duplicate")
								Next
							End Sub)
		End Sub

		''' <summary>验证来自客户端提交的对象</summary>
		''' <param name="entity">数据对象</param>
		''' <param name="prefix">验证参数前缀</param>
		Public Shared Sub ValidateFields(Of T As {IEntity, Class})(entity As T, errorMessage As ErrorMessage, Optional prefix As String = "")
			' 无内容或者无提交字段数据，不验证
			If entity Is Nothing Then Return

			' 验证主要字段
			Dim valContext = New ValidationContext(entity, SYS.Application.Services, entity.ToDictionary(False)?.ToDictionary(Of Object, Object)(Function(x) x.Key, Function(x) x.Value))
			Dim result As New List(Of ValidationResult)

			Try
				If Not Validator.TryValidateObject(entity, valContext, result, True) Then
					If prefix.NotEmpty Then prefix &= "."

					For Each res In result
						Dim key = res.MemberNames.FirstOrDefault

						' 替换部分默认消息
						Dim msg = res.ErrorMessage

						'The Body field is required.
						If msg.Contains("required") Then msg = "Error.Validate.Required"

						'The field Title must be a string or array type with a maximum length of '100'.。
						If msg.Contains("maximum length") Then msg = "Error.Validate.MaxLength"

						errorMessage.Add($"{prefix}{key}", msg)
					Next
				End If
			Catch ex As Exception
				errorMessage.Exception = ex
			End Try

			If Not errorMessage.IsPass Then Return

			' 验证子项目，来自实体对象，但非上级栏目实体
			Dim pros = entity.GetType.GetAllProperties.Where(Function(x) x.PropertyType.IsComeFrom(Of IEntity) AndAlso Not x.PropertyType.IsComeFrom(Of IEntityParent)).ToList
			If pros.NotEmpty Then
				pros.ForEach(Sub(pro)
								 Dim key = pro.Name
								 Dim value = pro.GetValue(entity)
								 If value Is Nothing Then
									 ' 是否必填
									 Dim required = pro.GetCustomAttribute(Of RequiredAttribute) IsNot Nothing
									 If required Then errorMessage.Add($"{pro.Name}", "Error.Validate.Required")

									 Return
								 End If

								 ' 内容检查
								 Dim data = TryCast(value, IEntity)
								 If data IsNot Nothing Then ValidateFields(data, errorMessage, pro.Name)
							 End Sub)
			End If
		End Sub

		''' <summary>验证自定义唯一标识文本字段是否唯一</summary>
		''' <param name="field">唯一字段，仅处理文本字段</param>
		''' <param name="titleField">标题字段，当标识不存在时，生成唯一标识</param>
		''' <remarks>如验证路径，HASH等。未设置自动创建</remarks>
		Public Shared Sub ValidateUniqueField(Of T As {IEntity, Class})(entity As T, field As String, titleField As String, db As IFreeSql, errorMessage As ErrorMessage)
			' 已经存在错误不处理
			If Not errorMessage.IsPass Then Return

			' 属性
			Dim pro = entity.GetType.GetSingleProperty(field)
			If pro Is Nothing OrElse Not pro.PropertyType.IsString Then Return

			' 获取字段长度，长度小于20，不做处理，未设置，默认为 50
			Dim len = pro.GetStringAttributeLength
			If len = 0 Then len = 50
			If len <= 20 Then Return

			' 路径不存在，则生成路径
			Dim value = pro.GetValue(entity)?.ToString
			If value.IsEmpty Then
				' 从文本字段获取
				Dim proTitle = entity.GetType.GetSingleProperty(titleField)
				If proTitle IsNot Nothing Then
					Dim title = proTitle.GetValue(entity)
					If title IsNot Nothing Then value = Utils.Extension.ToObjectString(title).ToPinYin(False, False, " ").Trim
				End If
			End If

			' 依然分析不到，使用 guid
			If value.IsEmpty Then value = Guid.NewGuid.ToString.MD5

			' 检查是否存在重复路径
			Dim hash = value

			' 初始路径
			value = value.Left(len - 20)

			While True
				Dim where = New DynamicFilterInfo With {.Logic = DynamicFilterLogic.And, .Filters = New List(Of DynamicFilterInfo)}
				where.Filters.Add(New DynamicFilterInfo With {.Field = "ID", .[Operator] = DynamicFilterOperator.NotEqual, .Value = entity.ID})
				where.Filters.Add(New DynamicFilterInfo With {.Field = field, .[Operator] = DynamicFilterOperator.Equal, .Value = hash})

				If db.Select(Of T).WhereDynamicFilter(where).Any Then
					hash = $"{value}_{SnowFlakeHelper.NextID}"
				Else
					Exit While
				End If
			End While

			pro.SetValue(entity, hash)
		End Sub

	End Class

End Namespace