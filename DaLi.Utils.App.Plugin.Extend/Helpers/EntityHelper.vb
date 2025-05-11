' ------------------------------------------------------------
'
' 	Copyright © 2023 湖南大沥网络科技有限公司.
'
' 	  author:	木炭(WOODCOAL)
' 	   email:	i@woodcoal.cn
' 	homepage:	http://www.hunandali.com/
'
' 	Dali.App Is licensed under GPLv3
'
' ------------------------------------------------------------
'
' 	实体数据附加操作
'
' 	name: Helper.EntityHelper
' 	create: 2023-10-11
' 	memo: 实体数据附加操作
'
' ------------------------------------------------------------

Imports System.Reflection

Namespace Helper
	''' <summary>实体数据附加操作</summary>
	Public NotInheritable Class EntityHelper

		''' <summary>从对象数组中获取单个对象，并禁止批量操作</summary>
		''' <param name="data">对象数组</param>
		''' <param name="errorMessage">错误消息对象</param>
		Public Shared Function GetEntity(Of T As {IEntity, Class})(data As ObjectArray(Of T), Optional errorMessage As ErrorMessage = Nothing, Optional defaultValue As T = Nothing) As T
			Dim entity = data?.ToOne
			If entity Is Nothing Then
				If errorMessage IsNot Nothing Then errorMessage.Notification = "Error.NoData"
				Return defaultValue
			End If

			' 不允许批量操作
			If data.IsMuti Then
				If errorMessage IsNot Nothing Then errorMessage.Notification = "Error.NoBatch"
				Return defaultValue
			End If

			Return entity
		End Function

		''' <summary>从对象获取原始数据</summary>
		''' <param name="errorMessage">错误消息对象</param>
		Public Shared Function GetSource(Of T As {IEntity, Class})(entity As T, db As IFreeSql, Optional errorMessage As ErrorMessage = Nothing) As T
			If entity.ID = 0 Then
				If errorMessage IsNot Nothing Then errorMessage.Notification = "Error.NoData"
				Return Nothing
			End If

			' 原始对象
			Dim source = db.Select(Of T).WhereID(entity.ID).ToOne
			If source Is Nothing Then
				If errorMessage IsNot Nothing Then errorMessage.Notification = "Error.NotFound"
				Return Nothing
			End If

			Return source
		End Function

		''' <summary>从对象数组中获取单个对象，并禁止批量操作，同时从数据库获取存在的数据</summary>
		''' <param name="data">对象数组</param>
		''' <param name="errorMessage">错误消息对象</param>
		Public Shared Function GetEntitySource(Of T As {IEntity, Class})(data As ObjectArray(Of T), db As IFreeSql, Optional errorMessage As ErrorMessage = Nothing) As (Entity As T, Source As T)
			Dim entity = GetEntity(data, errorMessage)
			If entity Is Nothing Then Return Nothing

			Dim source = GetSource(entity, db, errorMessage)

			Return (entity, source)
		End Function

		''' <summary>操作验证，检查后续的操作是否在当前操作列表中</summary>
		''' <param name="action">当前操作</param>
		''' <returns>是否允许操作</returns>
		Public Shared Function EnActions(action As EntityActionEnum, actions As EntityActionEnum(), Optional errorMessage As ErrorMessage = Nothing) As Boolean
			' 检查是否允许操作
			Dim isPass = actions.Contains(action)

			' 未通过后的操作
			If Not isPass Then
				Dim enMessage = {EntityActionEnum.ADD, EntityActionEnum.EDIT, EntityActionEnum.DELETE}.Contains(action) AndAlso errorMessage IsNot Nothing
				If enMessage Then errorMessage.Notification = "Error.NoAction"
			End If

			Return isPass
		End Function

		''' <summary>操作验证，检查后续的操作是否不在当前操作列表中</summary>
		''' <param name="action">当前操作</param>
		''' <param name="actions">允许的操作列表</param>
		''' <param name="errorMessage">对于加、改、删操作，在不允许的时候是否提示异常，提示则需要设置消息对象</param>
		''' <returns>是否允许操作</returns>
		Public Shared Function DisActions(action As EntityActionEnum, actions As EntityActionEnum(), Optional errorMessage As ErrorMessage = Nothing) As Boolean
			Return Not EnActions(action, actions, errorMessage)
		End Function

		''' <summary>类型值验证</summary>
		''' <param name="field">值字段</param>
		''' <param name="value">原始字符</param>
		''' <param name="type">数据类型</param>
		''' <returns>有效的数据转换后的文本</returns>
		Public Shared Function TypeValueValidate(field As String, value As String, type As FieldTypeEnum, errorMessage As ErrorMessage, Optional enEmpty As Boolean = False) As String
			' 验证值类型
			Dim data = value.EmptyValue().ToValue(type)

			If data Is Nothing Then
				If Not enEmpty Then errorMessage.Add(field, "您提交的值无效")
			Else
				Select Case type
					Case FieldTypeEnum.BOOLEAN, FieldTypeEnum.TRISTATE, FieldTypeEnum.NUMBER, FieldTypeEnum.BYTES, FieldTypeEnum.INTEGER, FieldTypeEnum.LONG, FieldTypeEnum.SINGLE, FieldTypeEnum.DOUBLE, FieldTypeEnum.DATETIME, FieldTypeEnum.JSON
								' 无需检查

					Case FieldTypeEnum.GUID
						If data = Guid.Empty Then errorMessage.Add(field, "无效 GUID 数据")

					Case Else
						' 不能为空
						If data.ToString.IsEmpty And Not enEmpty Then errorMessage.Add(field, "您提交的值无效")
				End Select
			End If

			Return If(errorMessage.IsPass, TypeExtension.ToObjectString(data), "")
		End Function

#Region "父级数据验证"

		''' <summary>分类（栏目）上级节点验证。注意：此函数用于栏目验证上级</summary>
		''' <param name="entity">要验证的节点数据</param>
		''' <param name="parentName">分类（栏目）名称</param>
		''' <remarks>上级可以为空；栏目只能添加在栏目节点上，如果节点上存在信息，则此节点不能用于栏目；上级节点不能在子节点下</remarks>
		Public Shared Sub ValidateParent(Of T As {IEntityParent, Class})(errorMessage As ErrorMessage, db As IFreeSql, entity As T, Optional parentName As String = "栏目")
			ValidateParent(errorMessage, db, entity, Function(x) False, parentName)
		End Sub

		''' <summary>分类（栏目）上级节点验证。注意：此函数用于栏目验证上级</summary>
		''' <param name="entity">要验证的节点数据</param>
		''' <param name="infoExistAction">验证当前上级是否存在信息，如果返回存在，则上级验证失败。</param>
		''' <param name="parentName">分类（栏目）名称</param>
		''' <remarks>上级可以为空；栏目只能添加在栏目节点上，如果节点上存在信息，则此节点不能用于栏目；上级节点不能在子节点下</remarks>
		Public Shared Sub ValidateParent(Of T As {IEntityParent, Class})(errorMessage As ErrorMessage, db As IFreeSql, entity As T, Optional infoExistAction As Func(Of Long, Boolean) = Nothing, Optional parentName As String = "栏目")
			If entity Is Nothing Then
				errorMessage.Notification = "Error.NoData"
				Return
			End If

			' 实体标识不存在，表示添加数据，无需验证
			If entity.ID.IsEmpty Then Return

			' 当前上级不存在表示加入的为顶级，无需操作
			' 否则需要进行相关验证
			If entity.ParentId.IsEmpty Then
				entity.ParentId = Nothing
				Return
			End If

			' 1. 自己的上级不能是自身
			If entity.ID = entity.ParentId Then
				errorMessage.Add("ParentId", $"上级{parentName}不能为自身")
				Return
			End If

			' 2. 验证是否存在
			Dim parentExist = db.Select(Of T).WhereID(entity.ParentId).Any
			If Not parentExist Then
				errorMessage.Add("ParentId", $"上级{parentName}不存在，请选择有效的上级{parentName}")
				Return
			End If

			' 3. 自己的上级不能放在自己的下级
			Dim childIds As List(Of Long)

			' 根据数据类型来选择获取下级的方式
			Dim entityType = GetType(T)
			If entityType.IsEntityTree Then
				'childIds = db.SelectTreeIDs(entity)
				childIds = GetType(QueryExtension).GetMember("SelectTreeIDs").Cast(Of MethodInfo).
							Where(Function(x) x.IsGenericMethodDefinition AndAlso x.GetParameters(1).ParameterType.IsClass).FirstOrDefault.
							MakeGenericMethod(entityType).
							Invoke(Nothing, {db, entity, Nothing})
			Else
				childIds = db.SelectChildIds(entity)
			End If
			If childIds.NotEmpty AndAlso childIds.Contains(entity.ParentId) Then
				errorMessage.Add("ParentId", $"上级{parentName}不能为自身的下级{parentName}")
				Return
			End If

			' 4. 信息接口验证
			If infoExistAction IsNot Nothing Then
				If infoExistAction.Invoke(entity.ParentId) Then
					errorMessage.Add("ParentId", $"不能选择含有信息的{parentName}，请选择有效的上级{parentName}")
					Return
				End If
			End If
		End Sub

		''' <summary>分类（栏目）上级节点验证。注意：此函数用于栏目验证上级</summary>
		''' <param name="entity">要验证的节点数据</param>
		''' <param name="parentName">分类（栏目）名称</param>
		''' <remarks>上级可以为空；栏目只能添加在栏目节点上，如果节点上存在信息，则此节点不能用于栏目；上级节点不能在子节点下</remarks>
		Public Shared Sub ValidateParent(Of TNode As {IEntityParent, Class}, TInfo As {IEntity, Class})(errorMessage As ErrorMessage, db As IFreeSql, entity As TNode, Optional parentName As String = "栏目")
			ValidateParent(errorMessage, db, entity, Function(x) db.Select(Of TInfo).WhereID(x).Any, parentName)
		End Sub

		''' <summary>信息数据上级节点验证。注意：此函数用于信息验证上级</summary>
		''' <param name="parentId">分类（栏目）标识</param>
		''' <param name="parentName">分类（栏目）名称</param>
		''' <param name="any">是否检查上级是否存在子栏目，True：不检查，任何上级节点都可以添加数据，False：仅数据节点才能添加数据</param>
		''' <remarks>上级不能为空，必须选择；信息只能添加在信息节点上，如果节点上存在子栏目，则此节点不能用于信息</remarks>
		Public Shared Sub ValidateParent(Of TParent As {IEntityParent, Class})(errorMessage As ErrorMessage, db As IFreeSql, parentId As Long?, any As Boolean, Optional parentName As String = "栏目")
			If parentId.IsEmpty Then
				errorMessage.Add("ParentId", $"上级{parentName}未设置，请选择有效的上级{parentName}")
				Return
			End If

			' 1. 检查上级是否存在
			Dim parentExist = db.Select(Of TParent).WhereID(parentId).Any
			If Not parentExist Then
				errorMessage.Add("ParentId", $"上级{parentName}不存在，请选择有效的上级{parentName}")
				Return
			End If

			' 2. 检查是否存在子栏目，存在则不能添加内容
			If any Then Return

			If db.Select(Of TParent).WhereEquals(parentId, Function(x) x.ParentId).Any Then
				errorMessage.Add("ParentId", $"不能选择含有子{parentName}的上级，请选择有效的上级{parentName}")
				Return
			End If
		End Sub

		''' <summary>信息数据上级节点验证。注意：此函数用于信息验证上级</summary>
		''' <param name="entity">信息实体，此数据类型必须包含 Parent 字段，且该字段为 IEntityParent 类型</param>
		''' <param name="parentName">分类（栏目）名称</param>
		''' <param name="any">是否检查上级是否存在子栏目，True：不检查，任何上级节点都可以添加数据，False：仅数据节点才能添加数据</param>
		''' <remarks>上级不能为空，必须选择；信息只能添加在信息节点上，如果节点上存在子栏目，则此节点不能用于信息</remarks>
		Public Shared Sub ValidateParent(Of TInfo As {IEntityParent, Class})(errorMessage As ErrorMessage, db As IFreeSql, entity As TInfo, any As Boolean, Optional parentName As String = "栏目")
			If entity Is Nothing Then
				errorMessage.Notification = "Error.NoData"
				Return
			End If

			' 检查是否有效的上级类型
			Dim pro = entity.GetType.GetSingleProperty("Parent")
			If pro Is Nothing OrElse Not pro.PropertyType.IsComeFrom(Of IEntityParent) Then
				errorMessage.Add("ParentId", $"非有效的数据类型，无法验证上级{parentName}")
				Return
			End If

			If entity.ParentId.IsEmpty Then
				errorMessage.Add("ParentId", $"上级{parentName}未设置，请选择有效的上级{parentName}")
				Return
			End If

			' 1. 检查上级是否存在
			Dim parentExist = db.Select(pro.PropertyType).WhereID(entity.ParentId).Any
			If Not parentExist Then
				errorMessage.Add("ParentId", $"上级{parentName}不存在，请选择有效的上级{parentName}")
				Return
			End If

			' 2. 检查是否存在子栏目，存在则不能添加内容
			If any Then Return

			If db.Select(Of IEntityParent).AsType(pro.PropertyType).WhereEquals(entity.ParentId, Function(x) x.ParentId).Any Then
				errorMessage.Add("ParentId", $"不能选择含有子{parentName}的上级，请选择有效的上级{parentName}")
				Return
			End If
		End Sub

		''' <summary>验证栏目节点下是否存在内容</summary>
		''' <param name="entity">要验证的数据，此数据类型必须包含 Parent 字段，且该字段为 IEntityParent 类型</param>
		''' <param name="infoExistAction">验证当前上级是否存在信息的过程</param>
		Public Shared Function ValidateChildExist(Of TNode As {IEntityParent, Class})(db As IFreeSql, entity As TNode, Optional infoExistAction As Func(Of Long, Boolean) = Nothing) As Boolean
			If entity Is Nothing Then Return False

			' 检查当前是否树形节点
			If db.HasChilds(entity) Then Return True

			' 验证子节点
			If infoExistAction IsNot Nothing Then
				Return infoExistAction.Invoke(entity.ID)
			Else
				Return False
			End If
		End Function

		''' <summary>验证栏目节点下是否存在内容</summary>
		''' <param name="entity">要验证的数据，此数据类型必须包含 Parent 字段，且该字段为 IEntityParent 类型</param>
		Public Shared Function ValidateChildExist(Of TNode As {IEntityParent, Class}, TInfo As {IEntityParent, Class})(db As IFreeSql, entity As TNode) As Boolean
			Return ValidateChildExist(db, entity, Function(x) db.Select(Of TInfo).WhereEquals(x, Function(y) y.ParentId).Any)
		End Function

#End Region

	End Class
End Namespace