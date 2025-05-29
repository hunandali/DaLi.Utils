' ------------------------------------------------------------
'
' 	Copyright © 2023 湖南大沥网络科技有限公司.
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
' 	视图模型公共基础操作
'
' 	name: base.VMEntityCommonBase
' 	create: 2023-10-08
' 	memo: 视图模型公共基础操作
'
' ------------------------------------------------------------

Imports System.Reflection
Imports DaLi.Utils.Json
Imports FreeSql

Namespace Base
	''' <summary>视图模型公共基础操作</summary>
	Public MustInherit Class VMEntityBase(Of T As {IEntity, Class})
		Inherits VMBase

#Region "插件"

		''' <summary>插件列表</summary>
		Private Shared _Plugins As List(Of IEntityPlugin)

		''' <summary>插件列表</summary>
		Protected Shared ReadOnly Property Plugins As List(Of IEntityPlugin)
			Get
				If _Plugins Is Nothing Then _Plugins = SYS.Plugins.GetInstances(Of IEntityPlugin)(True)

				Return _Plugins
			End Get
		End Property

#End Region

#Region "事件"

		''' <summary>项目查询前预处理</summary>
		''' <param name="action">操作类型：item/list/export...</param>
		''' <param name="query">查询对象</param>
		''' <param name="queryVM">查询视图</param>
		Private Shared Sub ExecuteQuery_Before(action As EntityActionEnum, query As ISelect(Of T), Optional queryVM As QueryBase(Of T) = Nothing)
			Plugins?.ForEach(Sub(plugin) plugin.ExecuteQuery(action, query, queryVM))
		End Sub

		''' <summary>项目操作之前预处理</summary>
		''' <param name="action">操作类型：item/add/edit/delete/list/export...</param>
		''' <param name="entity">当前实体</param>
		''' <param name="source">编辑时更新前的原始值</param>
		Private Sub ExecuteValidate_Before(action As EntityActionEnum, entity As T, Optional source As T = Nothing)
			Plugins?.ForEach(Sub(plugin) If ErrorMessage.IsPass Then plugin.ExecuteValidate(action, entity, AppContext, ErrorMessage, Db, source))
		End Sub

		''' <summary>项目操作之后</summary>
		''' <param name="action">操作类型：item/add/edit/delete/list/export...</param>
		''' <param name="data">单项操作时单个数值，多项时为数组</param>
		Private Sub ExecuteFinish_Before(action As EntityActionEnum, data As ObjectArray(Of T))
			' 对于加、改、删进行入库后检查操作
			Select Case action
				Case EntityActionEnum.ADD, EntityActionEnum.EDIT, EntityActionEnum.DELETE
					' 操作类型
					Dim baseAction As EntityBaseActionEnum
					Dim nameAction As String
					If action = EntityActionEnum.EDIT Then
						baseAction = EntityBaseActionEnum.EDIT
						nameAction = "修改"

					ElseIf action = EntityActionEnum.DELETE Then
						baseAction = EntityBaseActionEnum.DELETE
						nameAction = "删除"

					Else
						baseAction = EntityBaseActionEnum.ADD
						nameAction = "添加"

					End If

					' 项目检查
					If data.IsMuti Then
						' 批量操作
						Dim messages As New List(Of String)

						data.ForEach(Sub(item, index)
										 ErrorMessage.Reset()

										 item.Finish(baseAction, ErrorMessage, Db, AppContext)
										 If Not ErrorMessage.IsPass Then messages.Add($"{index + 1}. 项目 {item.ID} {nameAction}后异常：{ErrorMessage}")
									 End Sub)

						' 存在异常
						ErrorMessage.Reset()
						If messages.NotEmpty Then ErrorMessage.Notification = messages.JoinString($"；{vbCrLf}")
					Else
						' 单个操作
						data.First.Finish(baseAction, ErrorMessage, Db, AppContext)
					End If

					' 存在异常不再后续处理
					If Not ErrorMessage.IsPass Then Return
			End Select

			Plugins?.ForEach(Sub(plugin) If ErrorMessage.IsPass Then plugin.ExecuteFinish(action, data, AppContext, ErrorMessage, Db))
		End Sub

		''' <summary>不允许重复的字段设置</summary>
		Public Overridable ReadOnly Property DuplicatedInfo As DuplicatedFields(Of T) = Nothing

		''' <summary>项目查询前预处理</summary>
		''' <param name="action">操作类型：item/list/export...</param>
		''' <param name="query">查询对象</param>
		''' <param name="queryVM">查询视图</param>
		Public Overridable Sub ExecuteQuery(action As EntityActionEnum, query As ISelect(Of T), Optional queryVM As QueryBase(Of T) = Nothing)
			RaiseEvent OnQuery(action, query, queryVM)
		End Sub

		''' <summary>项目操作之前预处理</summary>
		''' <param name="action">操作类型：item/add/edit/delete/list/export...</param>
		''' <param name="entity">当前实体</param>
		''' <param name="source">编辑时更新前的原始值</param>
		Public Overridable Sub ExecuteValidate(action As EntityActionEnum, entity As T, Optional source As T = Nothing)
			RaiseEvent OnValidate(action, entity, source)
		End Sub

		''' <summary>项目操作之后</summary>
		''' <param name="action">操作类型：item/add/edit/delete/list/export...</param>
		''' <param name="data">单项操作时单个数值，多项时为数组</param>
		Public Overridable Sub ExecuteFinish(action As EntityActionEnum, data As ObjectArray(Of T))
			RaiseEvent OnFinish(action, data)
		End Sub

		''' <summary>项目查询前预处理</summary>
		''' <param name="action">操作类型：item/list/export...</param>
		''' <param name="query">查询对象</param>
		''' <param name="queryVM">查询视图</param>
		Public Event OnQuery(action As EntityActionEnum, query As ISelect(Of T), queryVM As QueryBase(Of T))

		''' <summary>项目操作之前预处理</summary>
		''' <param name="entity">当前实体</param>
		''' <param name="source">编辑时更新前的原始值</param>
		Public Event OnValidate(action As EntityActionEnum, entity As T, source As T)

		''' <summary>项目操作之后</summary>
		''' <param name="action">操作类型：item/add/edit/delete/list/export...</param>
		Public Event OnFinish(action As EntityActionEnum, data As ObjectArray(Of T))

#End Region

#Region "对象基本属性"

		''' <summary>对象类型</summary>
		Private _EntityType As Type

		''' <summary>对象类型</summary>
		Public ReadOnly Property EntityType As Type
			Get
				If _EntityType Is Nothing Then _EntityType = GetType(T)
				Return _EntityType
			End Get
		End Property

		''' <summary>对象属性</summary>
		Private _EntityProperties As PropertyInfo()

		''' <summary>对象属性</summary>
		Public ReadOnly Property EntityProperties As PropertyInfo()
			Get
				If _EntityProperties Is Nothing Then _EntityProperties = EntityType.GetAllProperties
				Return _EntityProperties
			End Get
		End Property

		''' <summary>对象加改删时是否同时操作级联数据</summary>
		Private _EntityCascade As Boolean?

		''' <summary>对象加改删时是否同时操作级联数据</summary>
		Public ReadOnly Property EntityCascade As Boolean
			Get
				If Not _EntityCascade.HasValue Then _EntityCascade = EntityType.GetCustomAttribute(Of DbCascadeAttribute)(True) IsNot Nothing
				Return _EntityCascade.Value
			End Get
		End Property

		''' <summary>标识类型</summary>
		Private _IdPro As PropertyInfo

		''' <summary>标识类型</summary>
		Public ReadOnly Property IdPro As PropertyInfo
			Get
				If _IdPro Is Nothing Then _IdPro = EntityType.GetSingleProperty("ID")
				Return _IdPro
			End Get
		End Property

		''' <summary>标识类型</summary>
		Private _IdType As Type

		''' <summary>标识类型</summary>
		Public ReadOnly Property IdType As Type
			Get
				If _IdType Is Nothing Then _IdType = IdPro.PropertyType
				Return _IdType
			End Get
		End Property

		''' <summary>当前操作用户</summary>
		Public MustOverride ReadOnly Property User As String

#End Region

#Region "基础验证"

		''' <summary>查询参数检测</summary>
		Public Overridable Function ValidateQuery(action As EntityActionEnum, Optional queryVM As QueryBase(Of T) = Nothing) As ISelect(Of T)
			' 默认查询对象
			Dim query = Db.Select(Of T)

			' 查询前的检查
			Call ExecuteQuery_Before(action, query, queryVM)
			If Not ErrorMessage.IsPass Then Return Nothing

			Call ExecuteQuery(action, query, queryVM)
			If Not ErrorMessage.IsPass Then Return Nothing

			' 检查是否存在字段数据，不存在则直接取消检测
			If queryVM Is Nothing Then queryVM = New QueryBase(Of T)

			' 更新查询条件，参数
			queryVM.QueryExecute(query)

			' 非列表类直接返回 1 条
			Select Case action
				Case EntityActionEnum.LIST, EntityActionEnum.EXPORT
					Dim max = queryVM.Max.Range(0, queryVM.MaxCount)
					If max > 0 Then query.Take(max)
				Case Else
					query.Take(1)
			End Select

			Return query
		End Function

		''' <summary>查询参数检测，并返回数量及分页信息</summary>
		Public Overridable Function ValidateQueryWithPage(action As EntityActionEnum, queryVM As QueryBase(Of T)) As (query As ISelect(Of T), Count As Long, Pages As Integer)
			' 检查是否存在字段数据，不存在则直接取消检测
			If queryVM Is Nothing Then
				ErrorMessage.Notification = "Error.Invalid"
				Return Nothing
			End If

			' 默认查询对象
			Dim query = Db.Select(Of T)

			' 查询前的检查
			Call ExecuteQuery_Before(action, query, queryVM)
			If Not ErrorMessage.IsPass Then Return Nothing

			Call ExecuteQuery(action, query, queryVM)
			If Not ErrorMessage.IsPass Then Return Nothing

			' 更新查询条件，参数
			queryVM.QueryExecute(query)

			' 非列表类直接返回
			Select Case action
				Case EntityActionEnum.LIST, EntityActionEnum.EXPORT
				Case Else
					Return (query, 1, 1)
			End Select

			' 强制返回最大数量
			Dim PageMax = queryVM.Max.Range(0, queryVM.MaxCount)

			' 当前页
			Dim PageNow = queryVM.Page.Range(1, queryVM.MaxPages)

			' 每页记录数
			Dim PageLimt = queryVM.Limit.Range(5, queryVM.MaxLimit)

			' 总记录数
			Dim Count = query.Count
			If Count < 1 Then Return Nothing

			' 分析结果
			Dim Pages = 0

			' 存在强制返回最大数量
			If PageMax > 0 Then
				' 如果 pageMax < Count 则需要限制大小
				If PageMax < Count Then
					' 返回的最大记录数
					Count = PageMax

					query.Take(PageMax)
				End If
			Else
				' 使用分页
				Pages = Math.Ceiling(Count / PageLimt)

				' 最大页数
				If Pages > queryVM.MaxPages Then Pages = queryVM.MaxPages

				' 最大当前页数
				If Pages < PageNow Then PageNow = Pages

				query.Page(PageNow, PageLimt)
			End If

			Return (query, Count, Pages)
		End Function

		'''' <summary>验证来自客户端提交的对象</summary>
		'''' <param name="entity">数据对象</param>
		'''' <param name="validateAll">是否验证所有字段还是仅验证提交的字段，对于编辑项目来说，如果只要保存修改部分，则只需要验证字段部分数据</param>
		'''' <param name="prefix">验证参数前缀</param>
		'''' <param name="fields">提交字段数据</param>
		'Public Sub ValidateFields(Of E As IEntity)(entity As E, fields As IDictionary(Of String, Object), validateAll As Boolean, Optional prefix As String = "")
		'	' 无内容或者无提交字段数据，不验证
		'	If entity Is Nothing OrElse fields.IsEmpty Then Return

		'	' 验证主要字段
		'	Dim valContext = New ValidationContext(entity, SYS.Application.Services, fields.ToDictionary(Of Object, Object)(Function(x) x.Key, Function(x) x.Value))
		'	Dim result As New List(Of ValidationResult)
		'	Dim keys = fields.Keys.ToArray

		'	Try
		'		If Not Validator.TryValidateObject(entity, valContext, result, True) Then
		'			If prefix.NotEmpty Then prefix &= "."

		'			For Each res In result
		'				Dim key = res.MemberNames.FirstOrDefault

		'				If validateAll OrElse keys.Contains(key, StringComparer.OrdinalIgnoreCase) Then
		'					Dim msg = AppContext.Localizer.TranslateWithPrefix(res.ErrorMessage, "Error.")
		'					ErrorMessage.Add($"{prefix}{key}", msg)
		'				End If
		'			Next
		'		End If
		'	Catch ex As Exception
		'		ErrorMessage.Exception = ex
		'	End Try

		'	If Not ErrorMessage.IsPass Then Return

		'	' 验证子项目，来自实体对象，但非上级栏目实体
		'	Dim pros = entity.GetType.GetAllProperties.Where(Function(x) x.PropertyType.IsComeFrom(Of IEntity) AndAlso Not x.PropertyType.IsComeFrom(Of IEntityParent)).ToList
		'	If pros.NotEmpty Then
		'		pros.ForEach(Sub(pro)
		'						 Dim key = pro.Name

		'						 ' 提交字段必须包含此字段，且字段内容为字典数据
		'						 Dim subFields = fields.Where(Function(x) pro.Name.Equals(x.Key, StringComparison.OrdinalIgnoreCase)).Select(Function(x) x.Value).Cast(Of IDictionary(Of String, Object)).FirstOrDefault
		'						 If subFields Is Nothing Then Return

		'						 Dim value = pro.GetValue(entity)
		'						 If value Is Nothing Then
		'							 ' 是否必填
		'							 Dim required = pro.GetCustomAttribute(Of RequiredAttribute) IsNot Nothing
		'							 If required Then ErrorMessage.Add($"{pro.Name}", "Error.Validate.Required")

		'							 Return
		'						 End If

		'						 ' 内容检查
		'						 Dim data = TryCast(value, IEntity)
		'						 If data IsNot Nothing Then ValidateFields(data, subFields, validateAll, pro.Name)
		'					 End Sub)
		'	End If
		'End Sub

		'''' <summary>验证来自客户端提交的对象</summary>
		'''' <param name="entity">数据对象</param>
		'''' <param name="validateAll">是否验证所有字段还是仅验证提交的字段，对于编辑项目来说，如果只要保存修改部分，则只需要验证字段部分数据</param>
		'Public Sub ValidateFields(entity As T, validateAll As Boolean)
		'	' 验证主要字段
		'	ValidateFields(entity, validateAll, String.Empty, AppContext.Fields)

		'	'' 验证主要字段
		'	'Dim valContext = New ValidationContext(entity, SYS.Application.Services, AppContext.Http.Items)
		'	'Dim result As New List(Of ValidationResult)

		'	'Try
		'	'	If Not Validator.TryValidateObject(entity, valContext, result, True) Then
		'	'		Dim fields = AppContext.Fields

		'	'		For Each res In result
		'	'			Dim key = res.MemberNames.FirstOrDefault()
		'	'			If validateAll OrElse fields.ContainsKey(key) Then
		'	'				Dim msg = AppContext.Localizer.TranslateWithPrefix(res.ErrorMessage, "Error.")
		'	'				ErrorMessage.Add(key, msg)
		'	'			End If
		'	'		Next
		'	'	End If
		'	'Catch ex As Exception
		'	'	ErrorMessage.Exception = ex
		'	'End Try

		'	'If Not ErrorMessage.IsPass Then Return

		'	'' 验证子项目
		'	'Dim pros = EntityType.GetAllProperties.Where(Function(x) x.PropertyType.IsComeFrom(Of IEntity)).ToList
		'	'If pros.NotEmpty Then
		'	'	pros.ForEach(Sub(pro)
		'	'					 Dim value = pro.GetValue(entity)

		'	'					 ' 是否必填
		'	'					 Dim required = pro.GetCustomAttribute(Of RequiredAttribute) IsNot Nothing
		'	'					 If value Is Nothing AndAlso required Then
		'	'						 ErrorMessage.Add($"{pro.Name}", "Error.Validate.Required")
		'	'						 Return
		'	'					 End If

		'	'					 ' 内容检查

		'	'				 End Sub)
		'	'End If

		'	'' 验证子表
		'	'Dim pros = EntityType.GetAllProperties.Where(Function(x) x.PropertyType.IsList(Of Entity_Base))
		'	'If pros.NotEmpty Then
		'	'	For Each pro In pros
		'	'		Dim it = TryCast(pro.GetValue(Entity), IEnumerable)
		'	'		If it Is Nothing Then Continue For

		'	'		' 列表字段，验证每项
		'	'		Dim contextset = False

		'	'		For Each e In it
		'	'			If contextset = False Then
		'	'				valContext = New ValidationContext(e)
		'	'				contextset = True
		'	'			End If

		'	'			If Not Validator.TryValidateObject(e, valContext, result, True) Then
		'	'				For Each res In result
		'	'					Dim key As String = res.MemberNames.FirstOrDefault
		'	'					ErrorMessage.Add(key, res.ErrorMessage)
		'	'				Next
		'	'			End If
		'	'		Next

		'	'	Next
		'	'End If

		'	' 验证字段是否重复
		'	If DuplicatedInfo IsNot Nothing Then
		'		Dim dupQuery = DuplicatedInfo.MakeQuerys(Db, entity)
		'		If dupQuery.NotEmpty Then
		'			For Each dup In dupQuery
		'				Dim query = dup.Value
		'				If query.Any Then
		'					' 存在重复项目
		'					For Each key In dup.Key.Split(",")
		'						ErrorMessage.Add(key, "Error.Duplicate")
		'					Next
		'				End If
		'			Next
		'		End If
		'	End If
		'End Sub

#Region "查询"

		''' <summary>项目获取检查，并附加基础数据</summary>
		Protected Overridable Function ValidateItem(action As EntityActionEnum, entity As T) As T
			' 检查是否存在字段数据，不存在则直接取消检测
			If entity Is Nothing Then
				ErrorMessage.Notification = "Error.NoData"
				Return Nothing
			End If

			' 附加 dictionary 
			Call DictionaryValue(entity)

			' 验证前的检查
			ExecuteValidate_Before(action, entity)
			If Not ErrorMessage.IsPass Then Return Nothing

			' 实体数据基础验证
			Call ExecuteValidate(action, entity)
			If Not ErrorMessage.IsPass Then Return Nothing

			Return entity
		End Function

		''' <summary>项目获取检查，并附加基础数据</summary>
		Protected Overridable Function ValidateList(action As EntityActionEnum, entities As IEnumerable(Of T)) As IEnumerable(Of T)
			' 检查是否存在字段数据，不存在则直接取消检测
			If entities.IsEmpty Then
				ErrorMessage.Notification = "Error.NoData"
				Return Nothing
			End If

			' 错误信息
			Dim messages As New List(Of String)

			entities = entities.
				Select(Function(x, index)
						   ErrorMessage.Reset()
						   Dim item = ValidateItem(action, x)
						   If item Is Nothing Then messages.Add($"项目 {index} / {x.ID} 异常：{ErrorMessage}")

						   Return item
					   End Function).
				Where(Function(x) x IsNot Nothing).
				ToList

			' 存在异常
			ErrorMessage.Reset()
			If messages.NotEmpty Then
				ErrorMessage.Notification = messages.JoinString($"；{vbCrLf}")
				Return Nothing
			End If

			' 返回结果
			Return entities
		End Function

#End Region

#Region "添加"

		''' <summary>添加对象前检查</summary>
		Public Overridable Function ValidateAdd(entity As T) As T
			' 检查是否存在字段数据，不存在则直接取消检测
			If entity Is Nothing Then
				ErrorMessage.Notification = "Error.NoData"
				Return Nothing
			End If

			' 验证前的检查
			ExecuteValidate_Before(EntityActionEnum.ADD, entity)
			If Not ErrorMessage.IsPass Then Return Nothing

			' 实体数据基础验证
			Call ExecuteValidate(EntityActionEnum.ADD, entity)
			If Not ErrorMessage.IsPass Then Return Nothing

			' 实体添加检查
			entity.Validate(EntityBaseActionEnum.ADD, ErrorMessage, Db, AppContext)
			If Not ErrorMessage.IsPass Then Return Nothing

			' 更新时间
			If EntityType.IsComeFrom(Of IEntityDate) Then
				Dim data = TryCast(entity, IEntityDate)
				If data IsNot Nothing Then
					data.CreateTime = DATE_NOW
					If data.CreateBy.IsEmpty Then data.CreateBy = User
				End If
			End If

			' 更新 IP
			If EntityType.IsComeFrom(Of IEntityIP) Then
				Dim data = TryCast(entity, IEntityIP)
				If data IsNot Nothing Then
					data.CreateIP = data.CreateIP.EmptyValue(AppContext.Http.IP)
				End If
			End If

			' 删除标记
			If EntityType.IsComeFrom(Of IEntityFlag) Then
				Dim data = TryCast(entity, IEntityFlag)
				If data IsNot Nothing Then
					If data.Flag = 0 Then data.Flag = 1
				End If
			End If

			Return entity
		End Function

		''' <summary>添加对象前检查</summary>
		Public Overridable Function ValidateAdd(entities As IEnumerable(Of T)) As IEnumerable(Of T)
			' 检查是否存在字段数据，不存在则直接取消检测
			If entities.IsEmpty Then
				ErrorMessage.Notification = "Error.NoData"
				Return Nothing
			End If

			' 错误信息
			Dim messages As New List(Of String)

			entities = entities.
				Select(Function(x, index)
						   ErrorMessage.Reset()
						   Dim item = ValidateAdd(x)
						   If item Is Nothing Then messages.Add($"添加项目 {index} / {x.ID} 异常：{ErrorMessage}")

						   Return item
					   End Function).
				Where(Function(x) x IsNot Nothing).
				ToList

			' 存在异常
			ErrorMessage.Reset()
			If messages.NotEmpty Then
				ErrorMessage.Notification = messages.JoinString($"；{vbCrLf}")
				Return Nothing
			End If

			' 返回结果
			Return entities
		End Function

#End Region

#Region "编辑"

		''' <summary>编辑对象前检查</summary>
		''' <param name="updateAll">是否更新全部项目，否则仅更新获取到的参数</param>
		''' <param name="skipAudit">是否忽略数据审计，控制器审核属性有设置，则以设置为准，否则此参数为准</param>
		''' <param name="skipEmpty">当仅更新提交项目时，是否忽略无数据的值，当更新内容时，如果原始内容为空则不处理</param>
		Public Overridable Function ValidateEdit(entity As T, updateAll As Boolean, skipAudit As Boolean, skipEmpty As Boolean) As T
			' 检查是否存在字段数据，不存在则直接取消检测
			If entity Is Nothing Then
				ErrorMessage.Notification = "Error.NoData"
				Return Nothing
			End If

			' 获取真实 ID，防止 基类 id
			' 对于非 Long 为 KEY 字段的，需要获取真实 ＩＤ
			Dim id = IdPro.GetValue(entity)
			If id Is Nothing Then
				ErrorMessage.Notification = "Error.NoData"
				Return Nothing
			End If

			Dim source = Db.Select(Of T).Where(Function(x) x.ID.Equals(id)).ToOne
			If source Is Nothing Then
				ErrorMessage.Notification = "Error.NoData"
				Return Nothing
			End If

			' 重置错误状态
			ErrorMessage.Reset()

			' 验证前的检查
			ExecuteValidate_Before(EntityActionEnum.EDIT, entity, source)
			If Not ErrorMessage.IsPass Then Return Nothing

			' 实体数据基础验证
			Call ExecuteValidate(EntityActionEnum.EDIT, entity, source)
			If Not ErrorMessage.IsPass Then Return Nothing

			' 实体编辑检查
			entity.Validate(EntityBaseActionEnum.EDIT, ErrorMessage, Db, AppContext, source)

			' 如果不是全部更新，则值检查提交参数的异常信息
			If Not ErrorMessage.IsPass AndAlso Not updateAll Then
				If ErrorMessage.Notification.IsEmpty AndAlso ErrorMessage.Exception Is Nothing Then
					' 获取所有需要检查的字段
					Dim keys = AppContext.Fields.ToSingleDictionary.Keys

					'移除非提交字段的错误
					ErrorMessage.Keys.
						Where(Function(x) Not keys.Contains(x)).
						ToList.
						ForEach(Sub(key) ErrorMessage.Remove(key))
				End If
			End If

			If Not ErrorMessage.IsPass Then Return Nothing

			'---------------
			' 审核检查
			'---------------
			Dim fields = Audit(skipAudit, entity, source)
			If fields.NotEmpty Then
				AppContext.Fields.Remove(fields.Keys.ToArray)
				ErrorMessage.SuccessMessage = "当前部分项目需要审核才能更新，请等待审核结果，在审核完成之前无需再次提交更新。"
			End If

			'---------------
			' 字段更新
			'---------------

			' 仅更新提交字段
			If Not updateAll Then
				For Each fld In AppContext.Fields
					Dim pro = EntityType.GetSingleProperty(fld.Key)
					If pro IsNot Nothing AndAlso pro.CanWrite Then
						Dim value = pro.GetValue(entity)
						If Not skipEmpty OrElse value IsNot Nothing Then pro.SetValue(source, value)
					End If
				Next

				entity = source
			End If

			' 更新时间
			If EntityType.IsComeFrom(Of IEntityDate) Then
				Dim baseData = TryCast(entity, IEntityDate)
				If baseData IsNot Nothing Then
					baseData.UpdateTime = DATE_NOW
					If baseData.UpdateBy.IsEmpty Then baseData.UpdateBy = User
				End If
			End If

			' 更新 IP
			If EntityType.IsComeFrom(Of IEntityIP) Then
				Dim data = TryCast(entity, IEntityIP)
				If data IsNot Nothing Then
					data.UpdateIP = data.UpdateIP.EmptyValue(AppContext.Http.IP)
				End If
			End If

			' 扩展数据调整
			' 对于来自扩展数据的结构，需要先获取原扩展数据，然后更新到新数据后再保存
			'If EntityType.IsComeFrom(Of IEntityExtend) Then
			'	' 克隆一份扩展数据，否则仓库不会自动更新数据
			'	Dim pro = EntityType.GetSingleProperty("Extension")
			'	Dim exts = TryCast(pro.GetValue(entity), NameValueDictionary)
			'	pro.SetValue(entity, exts?.Clone)
			'End If

			If EntityType.IsComeFrom(GetType(IEntityExtend(Of))) Then
				' 克隆一份扩展数据，否则仓库不会自动更新数据
				Dim pro = EntityType.GetSingleProperty("Extension")

				Dim value = pro.GetValue(entity)
				Dim valueClone = JsonExtension.ToJson(value, False, False, False).FromJson(value.GetType)
				pro.SetValue(entity, If(valueClone, value))
			End If

			Return entity
		End Function

		''' <summary>编辑对象前检查，将批量数据换成单个数据检查；批量操作将忽略空值，如果提交的值为空则不更新原始数据</summary>
		''' <param name="skipAudit">是否忽略数据审计，控制器审核属性有设置，则以设置为准，否则此参数为准</param>
		Public Overridable Function ValidateEdit(entities As IEnumerable(Of T), updateAll As Boolean, skipAudit As Boolean) As IEnumerable(Of T)
			' 检查是否存在字段数据，不存在则直接取消检测
			If entities.IsEmpty Then
				ErrorMessage.Notification = "Error.NoData"
				Return Nothing
			End If

			' 错误信息
			Dim messages As New List(Of String)

			entities = entities.
				Select(Function(x, index)
						   ErrorMessage.Reset()
						   Dim item = ValidateEdit(x, updateAll, skipAudit, True)
						   If item Is Nothing Then messages.Add($"编辑项目 {index} / {x.ID} 异常：{ErrorMessage}")

						   Return item
					   End Function).
				Where(Function(x) x IsNot Nothing).
				ToList

			' 存在异常
			ErrorMessage.Reset()
			If messages.NotEmpty Then
				ErrorMessage.Notification = messages.JoinString($"；{vbCrLf}")
				Return Nothing
			End If

			' 返回结果
			Return entities
		End Function

#End Region

#Region "删除"

		''' <summary>删除前检查</summary>
		''' <returns>False： 软删除；True：硬删除</returns>
		Public Overridable Function ValidateDelete(entity As T) As Boolean?
			' 检查是否存在字段数据，不存在则直接取消检测
			If entity Is Nothing Then
				ErrorMessage.Notification = "Error.NoData"
				Return Nothing
			End If

			' 删除前处理
			ExecuteValidate_Before(EntityActionEnum.DELETE, entity)
			If Not ErrorMessage.IsPass Then Return Nothing

			Call ExecuteValidate(EntityActionEnum.DELETE, entity)
			If Not ErrorMessage.IsPass Then Return Nothing

			' 实体删除检查
			entity.Validate(EntityBaseActionEnum.DELETE, ErrorMessage, Db, AppContext)
			If Not ErrorMessage.IsPass Then Return Nothing

			' 验证成功，检查是否真实删除
			If EntityType.IsComeFrom(Of IEntityFlag) Then
				Dim data = TryCast(entity, IEntityFlag)
				If data IsNot Nothing Then data.Flag = 0

				' 是否带时间标记类型
				If EntityType.IsComeFrom(Of IEntityDate) Then
					Dim item = TryCast(entity, IEntityDate)
					If item IsNot Nothing Then
						item.UpdateTime = DATE_NOW
						If item.UpdateBy.IsEmpty Then item.UpdateBy = User
					End If
				End If

				' 子表也需要设置
				' .......................
				' .......................
				' .......................
				' .......................

				Return False
			End If

			Return True
		End Function

		''' <summary>删除前检查</summary>
		Public Overridable Function ValidateDelete(entities As IEnumerable(Of T)) As Dictionary(Of T, Boolean)
			' 检查是否存在字段数据，不存在则直接取消检测
			If entities.IsEmpty Then
				ErrorMessage.Notification = "Error.NoData"
				Return Nothing
			End If

			' 错误信息
			Dim messages As New List(Of String)
			Dim ents As New Dictionary(Of T, Boolean)

			For I = 0 To entities.Count - 1
				ErrorMessage.Reset()
				Dim item = ValidateDelete(entities(I))
				If item.HasValue Then
					ents.Add(entities(I), item.Value)
				Else
					messages.Add($"删除项目 {I + 1} / {entities(I).ID} 异常：{ErrorMessage}")
				End If
			Next

			' 存在异常
			ErrorMessage.Reset()
			If messages.NotEmpty Then
				ErrorMessage.Notification = messages.JoinString($"；{vbCrLf}")
				Return Nothing
			End If

			Return ents
		End Function

#End Region

#End Region

#Region "完成后处理"

		''' <summary>项目完成后检查</summary>
		Public Overridable Function FinishItem(action As EntityActionEnum, entity As T) As T
			Call ExecuteFinish_Before(action, ObjectArray.NewObject(entity))
			Call ExecuteFinish(action, ObjectArray.NewObject(entity))
			Return If(ErrorMessage.IsPass, entity, Nothing)
		End Function

		''' <summary>项目完成后检查</summary>
		Public Overridable Function FinishList(action As EntityActionEnum, entities As IEnumerable(Of T)) As IEnumerable(Of T)
			Call ExecuteFinish_Before(action, ObjectArray.NewObject(entities))
			Call ExecuteFinish(action, ObjectArray.NewObject(entities))
			Return If(ErrorMessage.IsPass, entities, Nothing)
		End Function

		''' <summary>项目完成后检查</summary>
		Public Overridable Function FinishAdd(entity As T) As T
			Call ExecuteFinish_Before(EntityActionEnum.ADD, ObjectArray.NewObject(entity))
			Call ExecuteFinish(EntityActionEnum.ADD, ObjectArray.NewObject(entity))
			Return If(ErrorMessage.IsPass, entity, Nothing)
		End Function

		''' <summary>项目完成后检查</summary>
		Public Overridable Function FinishAdd(entities As IEnumerable(Of T)) As IEnumerable(Of T)
			Call ExecuteFinish_Before(EntityActionEnum.ADD, ObjectArray.NewObject(entities))
			Call ExecuteFinish(EntityActionEnum.ADD, ObjectArray.NewObject(entities))
			Return If(ErrorMessage.IsPass, entities, Nothing)
		End Function

		''' <summary>项目完成后检查</summary>
		Public Overridable Function FinishEdit(entity As T) As T
			Call ExecuteFinish_Before(EntityActionEnum.EDIT, ObjectArray.NewObject(entity))
			Call ExecuteFinish(EntityActionEnum.EDIT, ObjectArray.NewObject(entity))
			Return If(ErrorMessage.IsPass, entity, Nothing)
		End Function

		''' <summary>项目完成后检查</summary>
		Public Overridable Function FinishEdit(entities As IEnumerable(Of T)) As IEnumerable(Of T)
			Call ExecuteFinish_Before(EntityActionEnum.EDIT, ObjectArray.NewObject(entities))
			Call ExecuteFinish(EntityActionEnum.EDIT, ObjectArray.NewObject(entities))
			Return If(ErrorMessage.IsPass, entities, Nothing)
		End Function

		''' <summary>项目完成后检查</summary>
		Public Overridable Function FinishDelete(entity As T) As Boolean
			Call ExecuteFinish_Before(EntityActionEnum.DELETE, ObjectArray.NewObject(entity))
			Call ExecuteFinish(EntityActionEnum.DELETE, ObjectArray.NewObject(entity))
			Return ErrorMessage.IsPass
		End Function

		''' <summary>项目完成后检查</summary>
		Public Overridable Function FinishDelete(entities As IEnumerable(Of T)) As Boolean
			Call ExecuteFinish_Before(EntityActionEnum.DELETE, ObjectArray.NewObject(entities))
			Call ExecuteFinish(EntityActionEnum.DELETE, ObjectArray.NewObject(entities))
			Return ErrorMessage.IsPass
		End Function

#End Region

#Region "字典属性"

		''' <summary>字典字段</summary>
		Private _DictionaryFieldProperty As PropertyInfo

		''' <summary>字典数据</summary>
		Protected ReadOnly Property DictionaryFieldProperty As PropertyInfo
			Get
				If _DictionaryFieldProperty Is Nothing Then
					_DictionaryFieldProperty = ExtendHelper.GetDictionarProperty(EntityType)
				End If
				Return _DictionaryFieldProperty
			End Get
		End Property

		''' <summary>是否开启字典更新功能</summary>
		Private _DictionaryEnabled As Boolean = True

		''' <summary>是否开启字典更新功能，如果不要更新则设置为 False，主键非 Long 将强制关闭，不存在字典字段也强制关闭</summary>
		Protected Property DictionaryEnabled As Boolean
			Get
				Return _DictionaryEnabled AndAlso IdType.IsLong AndAlso DictionaryFieldProperty IsNot Nothing
			End Get
			Set(value As Boolean)
				_DictionaryEnabled = value
			End Set
		End Property

		''' <summary>字典数据</summary>
		Private _DictionaryProvider As IAppDictionaryProvider

		''' <summary>字典数据</summary>
		Protected ReadOnly Property DictionaryProvider As IAppDictionaryProvider
			Get
				If _DictionaryProvider Is Nothing Then _DictionaryProvider = GetService(Of IAppDictionaryProvider)()
				Return _DictionaryProvider
			End Get
		End Property

		''' <summary>赋值到对象</summary>
		Protected Sub DictionaryValue(data As T)
			If DictionaryEnabled Then DictionaryProvider.EntityUpdate(data)
		End Sub

		''' <summary>赋值到对象</summary>
		Protected Sub DictionaryValue(data As IEnumerable(Of T))
			If DictionaryEnabled Then DictionaryProvider.EntitiesUpdate(data)
		End Sub

		''' <summary>缓存当前系统获取到的字段数据</summary>
		''' <param name="mustField">是否必须客户端提交了 dictionay 字段才检查</param>
		Protected Function DictionaryLoad(entity As T, mustField As Boolean) As List(Of Long)
			' 只要存在数据字段且上传存在 dictionary 参数，则不论有无值都返回有效的列表
			' 否则返回空数据，提醒更新是否需要处理此数据
			' 空数据不操作，空对象则清空数据
			If DictionaryEnabled AndAlso (FieldExist("dictionary") OrElse Not mustField) Then
				Return DictionaryFieldProperty.GetValue(entity)
			Else
				Return Nothing
			End If
		End Function

		''' <summary>当前获取的字段数据，仅针对添加修改</summary>
		Protected Sub DictionaryUpdate(entity As T, dicIds As IEnumerable(Of Long))
			If Not DictionaryEnabled OrElse dicIds Is Nothing Then Return

			' 只要存在数据字段且上传存在 dictionary 参数，则不论有无值都返回有效的列表
			' 否则返回空数据，提醒更新是否需要处理此数据
			' 空数据不操作，空对象则清空数据
			DictionaryProvider.ModuleUpdate(entity, dicIds)
		End Sub

		'''' <summary>字典数据移除</summary>
		'Protected Sub DictionaryRemove(entity As T)
		'	If DictionaryEnabled Then DictionaryProvider.ModuleRemove(entity)
		'End Sub

		''' <summary>字典数据移除</summary>
		Protected Sub DictionaryRemove(ParamArray ids() As Long)
			If DictionaryEnabled Then DictionaryProvider.ModuleRemove(EntityType, ids)
		End Sub

#End Region

#Region "数据审核"

		''' <summary>审核组件</summary>
		Private _AuditProvider As IAppAuditProvider

		''' <summary>是否免审</summary>
		Private _AuditSkip As Boolean?

		''' <summary>审计参数初始化</summary>
		Private _AuditInit As Boolean = False

		''' <summary>数据审计</summary>
		''' <param name="skipAudit">是否免审</param>
		''' <param name="entity">对象</param>
		''' <param name="source">原始对象，不存在则从对象中查询</param>
		Protected Function Audit(skipAudit As Boolean, entity As T, Optional source As T = Nothing) As IDictionary(Of String, String)
			If Not _AuditInit Then
				Dim attr = Controller?.GetType.GetCustomAttribute(Of AuditAttribute)
				If attr Is Nothing Then
					_AuditSkip = Nothing
				Else
					_AuditSkip = Not attr.Audit
				End If

				_AuditProvider = GetService(Of IAppAuditProvider)()

				_AuditInit = True
			End If

			' 免审，直接返回
			If _AuditSkip.HasValue Then skipAudit = _AuditSkip.Value

			Return If(skipAudit, Nothing, _AuditProvider.Audit(entity, source, User))
		End Function

#End Region

	End Class
End Namespace
